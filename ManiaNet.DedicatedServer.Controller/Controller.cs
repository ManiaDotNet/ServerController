using ManiaNet.DedicatedServer.Controller.Plugins;
using ManiaNet.DedicatedServer.XmlRpc.Methods;
using SharpPlugins;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using XmlRpc;
using XmlRpc.Methods;
using XmlRpc.Types;

namespace ManiaNet.DedicatedServer.Controller
{
    /// <summary>
    /// Represents the main component of the controller, that handles all the incomming callbacks, command registrations of plugins, etc.
    /// </summary>
    public class ServerController
    {
        private ConcurrentDictionary<uint, string> methodResponses = new ConcurrentDictionary<uint, string>();
        private Dictionary<string, ControllerPlugin> plugins = new Dictionary<string, ControllerPlugin>();
        private List<Thread> pluginThreads;
        private ConcurrentDictionary<string, Action<ManiaPlanetPlayerChat>> registeredCommands = new ConcurrentDictionary<string, Action<ManiaPlanetPlayerChat>>();
        private IXmlRpcClient xmlRpcClient;

        public ServerController(IXmlRpcClient xmlRpcClient, Config config)
        {
            this.xmlRpcClient = xmlRpcClient;
            this.xmlRpcClient.MethodResponse += xmlRpcClient_MethodResponse;
            this.xmlRpcClient.ServerCallback += xmlRpcClient_ServerCallback;

            Configuration = config;
        }

        public Config Configuration { get; private set; }

        /// <summary>
        /// Gets the registered commands (all in lower case).
        /// </summary>
        public IEnumerable<string> RegisteredCommands
        {
            get { return registeredCommands.Keys; }
        }

        /// <summary>
        /// Sends a method call to the server and waits a maximum of the given timeout before returning whether the call returned (regardless of whether successful or not).
        /// </summary>
        /// <typeparam name="TReturn">The method call's returned XmlRpcType.</typeparam>
        /// <typeparam name="TReturnBase">The method call's type of the returned value.</typeparam>
        /// <param name="methodCall">The method call to be executed.</param>
        /// <param name="timeout">The maximum time in milliseconds to wait for a response.</param>
        /// <returns>Whether the call was returned (regardless of whether successful or not).</returns>
        public bool CallMethod<TReturn, TReturnBase>(XmlRpcMethodCall<TReturn, TReturnBase> methodCall, int timeout) where TReturn : XmlRpcType<TReturnBase>, new()
        {
            uint methodHandle = xmlRpcClient.SendRequest(methodCall.GenerateCallXml().ToString());

            if (!methodResponses.TryAdd(methodHandle, null))
                return false;

            string response = awaitResponse(methodHandle, timeout);

            if (string.IsNullOrEmpty(response))
                return false;

            try
            {
                if (!methodCall.ParseResponseXml(XDocument.Parse(response, LoadOptions.None).Root))
                    return false;
            }
            catch { return false; }

            return true;
        }

        /// <summary>
        /// Gets whether the plugin with the given identifier is loaded or not.
        /// </summary>
        /// <param name="identifier">The identifier to check.</param>
        /// <returns>Whether the plugin is loaded.</returns>
        public bool IsPluginLoaded(string identifier)
        {
            return plugins.ContainsKey(identifier);
        }

        /// <summary>
        /// Finds out whether the given command identifier is already taken.
        /// </summary>
        /// <param name="cmdName">The command identifier to check the availability for.</param>
        /// <returns>Whether it's taken or not.</returns>
        public bool IsRegisteredCommand(string cmdName)
        {
            return registeredCommands.ContainsKey(cmdName.ToLower());
        }

        /// <summary>
        /// Trys to register an action for a given command.
        /// <para/>
        /// If a message is an registered command, it doesn't activate the PlayerChat event.
        /// </summary>
        /// <param name="cmdName">The command identifier. Will be stored in lower case.</param>
        /// <param name="cmdAction">The action to be performed when the command is received. The parameter is the full message parameters.<para/>
        /// If you're planning to unregister the command, you have to store the action.</param>
        /// <returns>Whether it was successfully added or not.</returns>
        public bool RegisterCommand(string cmdName, Action<ManiaPlanetPlayerChat> cmdAction)
        {
            string cmd = cmdName.ToLower();

            if (registeredCommands.ContainsKey(cmd))
                return false;

            return registeredCommands.TryAdd(cmd, cmdAction);
        }

        public void ReloadPlugins()
        {
            stopPlugins();
            unloadPlugins();
            loadPlugins();
            startPlugins();
        }

        /// <summary>
        /// Starts the controller.
        /// </summary>
        public void Start()
        {
            xmlRpcClient.StartReceive();
            authenticate();
            loadPlugins();
            startPlugins();
        }

        /// <summary>
        /// Stops the controller.
        /// </summary>
        public void Stop()
        {
            stopPlugins();
            unloadPlugins();
            xmlRpcClient.EndReceive();
        }

        /// <summary>
        /// Trys to unregister a command, given its identifier and action.
        /// </summary>
        /// <param name="cmdName">The command identifier.</param>
        /// <param name="cmdAction">The that was performed when the command was received. Has to be the same (reference) action as the one registered.</param>
        /// <returns>Whether there's now no command with the given identifier registered.</returns>
        public bool UnregisterCommand(string cmdName, Action<ManiaPlanetPlayerChat> cmdAction)
        {
            string cmd = cmdName.ToLower();

            if (!registeredCommands.ContainsKey(cmd))
                return true;

            bool isSameEntry = false;
            foreach (var registeredCommand in registeredCommands)
            {
                if (registeredCommand.Key.Equals(cmd) && registeredCommand.Value.Equals(cmdAction))
                {
                    isSameEntry = true;
                    break;
                }
            }

            if (!isSameEntry)
                return false;

            return registeredCommands.TryRemove(cmd, out cmdAction);
        }

        private bool authenticate()
        {
            var methodCall = new Authenticate(Configuration.Login, Configuration.Password);

            if (!CallMethod(methodCall, 2000))
                return false;

            return methodCall.HadFault ? false : methodCall.ReturnValue;
        }

        /// <summary>
        /// Waits a maximum of timeout ms for the response to come in. If it hasn't come in by then, it returns an empty string.
        /// </summary>
        /// <param name="handle">The handle of the method call for which the response is wanted.</param>
        /// <param name="timeout">The maximum number of ms that it waits before returning an empty string.</param>
        /// <returns>The xml formatted method response or an empty string if it took more than timeout ms.</returns>
        private string awaitResponse(uint handle, int timeout)
        {
            int timeSpent = 0;
            if (!methodResponses.ContainsKey(handle))
                return string.Empty;

            while (methodResponses[handle] == null && timeSpent < timeout)
            {
                Thread.Sleep(10);
                timeSpent += 10;
            }

            if (methodResponses[handle] != null)
            {
                string response;
                if (methodResponses.TryRemove(handle, out response))
                    return response;
            }

            return string.Empty;
        }

        private void callEvent(object serverCallback)
        {
            XElement methodCall = XDocument.Parse((string)serverCallback, LoadOptions.None).Root;
            string methodName = methodCall.Element(XName.Get("methodName")).Value;

            //Swith to the right method name and call the event.
            switch (methodName)
            {
                case "ManiaPlanet.PlayerConnect":
                    if (PlayerConnect != null)
                    {
                        var playerConnectCall = new ManiaPlanetPlayerConnect();
                        if (playerConnectCall.ParseCallXml(methodCall))
                            PlayerConnect(this, playerConnectCall);
                    }
                    break;

                case "ManiaPlanet.PlayerDisconnect":
                    if (PlayerDisconnect != null)
                    {
                        var playerDisconnectCall = new ManiaPlanetPlayerDisconnect();
                        if (playerDisconnectCall.ParseCallXml(methodCall))
                            PlayerDisconnect(this, playerDisconnectCall);
                    }
                    break;

                case "ManiaPlanet.PlayerChat":
                    var playerChatCall = new ManiaPlanetPlayerChat();
                    if (playerChatCall.ParseCallXml(methodCall))
                    {
                        bool registeredCommand = false;
                        foreach (var cmdName in registeredCommands.Keys)
                        {
                            if (playerChatCall.Text.ToLower().StartsWith("/" + cmdName))
                            {
                                registeredCommands[cmdName](playerChatCall);
                                registeredCommand = true;
                                break;
                            }
                        }

                        if (!registeredCommand && PlayerChat != null)
                        {
                            PlayerChat(this, playerChatCall);
                        }
                    }
                    break;

                case "ManiaPlanet.PlayerManialinkPageAnswer":
                    if (PlayerManialinkPageAnswer != null)
                    {
                        var playerManialinkPageAnswerCall = new ManiaPlanetPlayerManialinkPageAnswer();
                        if (playerManialinkPageAnswerCall.ParseCallXml(methodCall))
                            PlayerManialinkPageAnswer(this, playerManialinkPageAnswerCall);
                    }
                    break;

                case "ManiaPlanet.Echo":
                    if (Echo != null)
                    {
                        var echoCall = new ManiaPlanetEcho();
                        if (echoCall.ParseCallXml(methodCall))
                            Echo(this, echoCall);
                    }
                    break;

                case "ManiaPlanet.ServerStart":
                    if (ServerStart != null)
                    {
                        var serverStartCall = new ManiaPlanetServerStart();
                        if (serverStartCall.ParseCallXml(methodCall))
                            ServerStart(this, serverStartCall);
                    }
                    break;

                case "ManiaPlanet.ServerStop":
                    if (ServerStop != null)
                    {
                        var serverStopCall = new ManiaPlanetServerStop();
                        if (serverStopCall.ParseCallXml(methodCall))
                            ServerStop(this, serverStopCall);
                    }
                    break;

                case "ManiaPlanet.BeginMatch":
                    if (BeginMatch != null)
                    {
                        var beginMatchCall = new ManiaPlanetBeginMatch();
                        if (beginMatchCall.ParseCallXml(methodCall))
                            BeginMatch(this, beginMatchCall);
                    }
                    break;

                case "ManiaPlanet.EndMatch":
                    if (EndMatch != null)
                    {
                        var endMatchCall = new ManiaPlanetEndMatch();
                        if (endMatchCall.ParseCallXml(methodCall))
                            EndMatch(this, endMatchCall);
                    }
                    break;

                case "ManiaPlanet.BeginMap":
                    if (BeginMap != null)
                    {
                        var beginMapCall = new ManiaPlanetBeginMap();
                        if (beginMapCall.ParseCallXml(methodCall))
                            BeginMap(this, beginMapCall);
                    }
                    break;

                case "ManiaPlanet.EndMap":
                    if (EndMap != null)
                    {
                        var endMapCall = new ManiaPlanetEndMap();
                        if (endMapCall.ParseCallXml(methodCall))
                            EndMap(this, endMapCall);
                    }
                    break;

                case "ManiaPlanet.StatusChanged":
                    if (StatusChanged != null)
                    {
                        var statusChangedCall = new ManiaPlanetStatusChanged();
                        if (statusChangedCall.ParseCallXml(methodCall))
                            StatusChanged(this, statusChangedCall);
                    }
                    break;

                case "TrackMania.PlayerCheckpoint":
                    Console.WriteLine("Player drove through checkpoint.");
                    if (PlayerCheckpoint != null)
                    {
                        var playerCheckpointCall = new TrackManiaPlayerCheckpoint();
                        if (playerCheckpointCall.ParseCallXml(methodCall))
                            PlayerCheckpoint(this, playerCheckpointCall);
                    }
                    break;

                case "TrackMania.PlayerFinish":
                    if (PlayerFinish != null)
                    {
                        var playerFinishCall = new TrackManiaPlayerFinish();
                        if (playerFinishCall.ParseCallXml(methodCall))
                            PlayerFinish(this, playerFinishCall);
                    }
                    break;

                case "TrackMania.PlayerIncoherence":
                    if (PlayerIncoherence != null)
                    {
                        var playerIncoherenceCall = new TrackManiaPlayerIncoherence();
                        if (playerIncoherenceCall.ParseCallXml(methodCall))
                            PlayerIncoherence(this, playerIncoherenceCall);
                    }
                    break;

                case "ManiaPlanet.BillUpdated":
                    if (BillUpdated != null)
                    {
                        var billUpdatedCall = new ManiaPlanetBillUpdated();
                        if (billUpdatedCall.ParseCallXml(methodCall))
                            BillUpdated(this, billUpdatedCall);
                    }
                    break;

                case "ManiaPlanet.TunnelDataReceived":
                    if (TunnelDataReceived != null)
                    {
                        var tunnelDataReceivedCall = new ManiaPlanetTunnelDataReceived();
                        if (tunnelDataReceivedCall.ParseCallXml(methodCall))
                            TunnelDataReceived(this, tunnelDataReceivedCall);
                    }
                    break;

                case "ManiaPlanet.MapListModified":
                    if (MapListModified != null)
                    {
                        var mapListModifiedCall = new ManiaPlanetMapListModified();
                        if (mapListModifiedCall.ParseCallXml(methodCall))
                            MapListModified(this, mapListModifiedCall);
                    }
                    break;

                case "ManiaPlanet.PlayerInfoChanged":
                    if (PlayerInfoChanged != null)
                    {
                        var playerInfoChangedCall = new ManiaPlanetPlayerInfoChanged();
                        if (playerInfoChangedCall.ParseCallXml(methodCall))
                            PlayerInfoChanged(this, playerInfoChangedCall);
                    }
                    break;

                case "ManiaPlanet.VoteUpdated":
                    if (VoteUpdated != null)
                    {
                        var voteUpdatedCall = new ManiaPlanetVoteUpdated();
                        if (voteUpdatedCall.ParseCallXml(methodCall))
                            VoteUpdated(this, voteUpdatedCall);
                    }
                    break;

                case "ManiaPlanet.PlayerAlliesChanged":
                    if (PlayerAlliesChanged != null)
                    {
                        var playerAlliesChangedCall = new ManiaPlanetPlayerAlliesChanged();
                        if (playerAlliesChangedCall.ParseCallXml(methodCall))
                            PlayerAlliesChanged(this, playerAlliesChangedCall);
                    }
                    break;
            }
        }

        private void loadPlugins()
        {
            Console.WriteLine("Loading Plugins...");

            var pluginTypes = PluginLoader.LoadPluginsFromFolders<ControllerPlugin>(Configuration.PluginFolders);

            plugins = PluginLoader.InstanciatePlugins<ControllerPlugin>(pluginTypes).Select(plugin =>
            {
                Console.Write(ControllerPlugin.GetName(plugin.GetType()) + " ... ");
                bool success = plugin.Load(this);
                Console.WriteLine(success ? "OK" : "Failed");
                return new { Plugin = plugin, Success = success };
            })
            .Where(loadedPlugin => loadedPlugin.Success)
            .Select(loadedPlugin => loadedPlugin.Plugin)
            .ToDictionary(plugin => ControllerPlugin.GetIdentifier(plugin.GetType()));

            Console.WriteLine("Done");
        }

        private void startPlugins()
        {
            pluginThreads = plugins.Values.Select(plugin =>
                {
                    Thread thread = new Thread(plugin.Run);
                    thread.Name = xmlRpcClient.Name + " " + ControllerPlugin.GetName(plugin.GetType());
                    thread.IsBackground = true;
                    thread.Start();
                    return thread;
                }).ToList();
        }

        private void stopPlugins()
        {
            foreach (var pluginThread in pluginThreads)
                pluginThread.Abort();

            // Allow 100ms for plugins to finish the Run method.
            Thread.Sleep(100);
        }

        private void unloadPlugins()
        {
            Console.WriteLine("Unloading Plugins...");

            foreach (var plugin in plugins.Values)
            {
                Console.Write(ControllerPlugin.GetName(plugin.GetType()) + " ... ");
                Console.WriteLine(plugin.Unload() ? "OK" : "Failed");
            }

            Console.WriteLine("Done");
        }

        private void xmlRpcClient_MethodResponse(IXmlRpcClient sender, uint requestHandle, string methodResponse)
        {
            if (methodResponses.ContainsKey(requestHandle))
                methodResponses[requestHandle] = methodResponse;
        }

        private void xmlRpcClient_ServerCallback(IXmlRpcClient sender, string serverCallback)
        {
            Task.Factory.StartNew(new Action<object>(callEvent), (object)serverCallback);
        }

        #region Callback Events

        /// <summary>
        /// Delegate for the various server callback events.
        /// </summary>
        /// <typeparam name="TMethodCall">The type representing the called method.</typeparam>
        /// <param name="sender">The ServerController that fired the event.</param>
        /// <param name="methodCall">The method call information.</param>
        public delegate void ServerCallbackEventHandler<TMethodCall>(ServerController sender, TMethodCall methodCall)
            where TMethodCall : XmlRpcMethodCall<XmlRpcBoolean, bool>;

        //public delegate void AllServerCallbackEventHandler<>

        /// <summary>
        /// Fires when a map begins.
        /// </summary>
        public event ServerCallbackEventHandler<ManiaPlanetBeginMap> BeginMap;

        /// <summary>
        /// Fires when a match begins.
        /// </summary>
        public event ServerCallbackEventHandler<ManiaPlanetBeginMatch> BeginMatch;

        /// <summary>
        /// Fires when a bill is updated.
        /// </summary>
        public event ServerCallbackEventHandler<ManiaPlanetBillUpdated> BillUpdated;

        /// <summary>
        /// Fires when the <see cref="ManiaNet.DedicatedServer.XmlRpc.Methods.Echo"/> method is called by another controller.
        /// </summary>
        public event ServerCallbackEventHandler<ManiaPlanetEcho> Echo;

        /// <summary>
        /// Fires when a map ended.
        /// </summary>
        public event ServerCallbackEventHandler<ManiaPlanetEndMap> EndMap;

        /// <summary>
        /// Fires when a match ended.
        /// </summary>
        public event ServerCallbackEventHandler<ManiaPlanetEndMatch> EndMatch;

        /// <summary>
        /// Fires when the map playlist is modified.
        /// </summary>
        public event ServerCallbackEventHandler<ManiaPlanetMapListModified> MapListModified;

        /// <summary>
        /// Fires when a player changes allies.
        /// </summary>
        public event ServerCallbackEventHandler<ManiaPlanetPlayerAlliesChanged> PlayerAlliesChanged;

        /// <summary>
        /// Fires when a client sent a chat message.
        /// <para/>
        /// Doesn't get fired when the message is a registered command.
        /// </summary>
        public event ServerCallbackEventHandler<ManiaPlanetPlayerChat> PlayerChat;

        /// <summary>
        /// Fires when a player drives through a checkpoint.
        /// </summary>
        public event ServerCallbackEventHandler<TrackManiaPlayerCheckpoint> PlayerCheckpoint;

        /// <summary>
        /// Fires when a client connected to the server.
        /// </summary>
        public event ServerCallbackEventHandler<ManiaPlanetPlayerConnect> PlayerConnect;

        /// <summary>
        /// Fires when a client disconnected from the server.
        /// </summary>
        public event ServerCallbackEventHandler<ManiaPlanetPlayerDisconnect> PlayerDisconnect;

        /// <summary>
        /// Fires when a player finishes the map.
        /// </summary>
        public event ServerCallbackEventHandler<TrackManiaPlayerFinish> PlayerFinish;

        /// <summary>
        /// Fires when a player sends incoherent data.
        /// </summary>
        public event ServerCallbackEventHandler<TrackManiaPlayerIncoherence> PlayerIncoherence;

        /// <summary>
        /// Fires when a client's info changed.
        /// </summary>
        public event ServerCallbackEventHandler<ManiaPlanetPlayerInfoChanged> PlayerInfoChanged;

        /// <summary>
        /// Fires when a client answered a mnailink page.
        /// </summary>
        public event ServerCallbackEventHandler<ManiaPlanetPlayerManialinkPageAnswer> PlayerManialinkPageAnswer;

        /// <summary>
        /// Fires when the server is started.
        /// </summary>
        public event ServerCallbackEventHandler<ManiaPlanetServerStart> ServerStart;

        /// <summary>
        /// Fires when the server is stopped.
        /// </summary>
        public event ServerCallbackEventHandler<ManiaPlanetServerStop> ServerStop;

        /// <summary>
        /// Fires when the server's status changed.
        /// </summary>
        public event ServerCallbackEventHandler<ManiaPlanetStatusChanged> StatusChanged;

        /// <summary>
        /// Fires when the server receives tunneled data.
        /// </summary>
        public event ServerCallbackEventHandler<ManiaPlanetTunnelDataReceived> TunnelDataReceived;

        /// <summary>
        /// Fires when the current vote's state changes.
        /// </summary>
        public event ServerCallbackEventHandler<ManiaPlanetVoteUpdated> VoteUpdated;

        #endregion Callback Events

        /// <summary>
        /// Represents a configuration for the server controller.
        /// </summary>
        public class Config
        {
            /// <summary>
            /// Creates a new instance of the <see cref="ManiaNet.DedicatedServer.ServerController.Config"/> class with the given Login and Password.
            /// </summary>
            /// <param name="login">The Login that the controller authenticates with; SuperAdmin by default.</param>
            /// <param name="password">The Password that the controller authenticates with; SuperAdmin by default.</param>
            /// <param name="pluginFolders">The path(s) to the folders used to load plugins from; { "plugins" } by default.</param>
            public Config(string login = "SuperAdmin", string password = "SuperAdmin", IEnumerable<string> pluginFolders = null)
            {
                Login = login;
                Password = password;
                PluginFolders = pluginFolders ?? new string[] { "plugins" };
            }

            /// <summary>
            /// Gets the Login that the controller will use to authenticate with the xml rpc server.
            /// </summary>
            public string Login { get; private set; }

            /// <summary>
            /// Gets the Password that the controller will use to authenticate with the xml rpc server.
            /// </summary>
            public string Password { get; private set; }

            /// <summary>
            /// Gets the path(s) to the folders used to load plugins from.
            /// </summary>
            public IEnumerable<string> PluginFolders { get; private set; }
        }
    }
}