using ManiaNet.DedicatedServer.Controller.Annotations;
using ManiaNet.DedicatedServer.Controller.Configuration;
using ManiaNet.DedicatedServer.Controller.Plugins;
using ManiaNet.DedicatedServer.Controller.Plugins.Extensibility.Clients;
using ManiaNet.DedicatedServer.Controller.Plugins.Extensibility.Manialink;
using ManiaNet.DedicatedServer.Controller.Plugins.LocalRecordsProvider;
using ManiaNet.DedicatedServer.Controller.Plugins.ManialinkDisplayManager;
using ManiaNet.DedicatedServer.XmlRpc.Methods;
using ManiaNet.DedicatedServer.XmlRpc.Structs;
using ManiaNet.ManiaPlanet.WebServices;
using Mono.Data.Sqlite;
using SharpPlugins;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
    public sealed class ServerController : IDisposable
    {
        private readonly ConcurrentDictionary<uint, string> methodResponses = new ConcurrentDictionary<uint, string>();
        private readonly ConcurrentDictionary<string, Action<ManiaPlanetPlayerChat>> registeredCommands = new ConcurrentDictionary<string, Action<ManiaPlanetPlayerChat>>();
        private readonly IXmlRpcClient xmlRpcClient;
        private Dictionary<string, ControllerPlugin> plugins = new Dictionary<string, ControllerPlugin>();
        private List<Thread> pluginThreads;

        /// <summary>
        /// The ChatInterfaceManager to register and unregister ChatInterfaces.
        /// </summary>
        [NotNull, UsedImplicitly]
        public ChatInterfaceManager ChatInterfaceManager { get; private set; }

        /// <summary>
        /// Gets the Clients Manager used by the Controller.
        /// </summary>
        [NotNull, UsedImplicitly]
        public IClientsManager ClientsManager { get; private set; }

        /// <summary>
        /// Gets the configuration of the controller.
        /// </summary>
        [NotNull, UsedImplicitly]
        public ServerControllerConfig Configuration { get; private set; }

        /// <summary>
        /// Gets the connection to the SQLite database.
        /// </summary>
        [NotNull, UsedImplicitly]
        public SqliteConnection Database { get; private set; }

        /// <summary>
        /// Gets the Manialink Display Manager used by the Controller.
        /// </summary>
        [NotNull, UsedImplicitly]
        public IManialinkDisplayManager ManialinkDisplayManager { get; private set; }

        /// <summary>
        /// Gets a readonly Dictionary of the loaded Controller Plugins.
        /// </summary>
        [NotNull, UsedImplicitly]
        public ReadOnlyDictionary<string, ControllerPlugin> Plugins
        {
            get { return new ReadOnlyDictionary<string, ControllerPlugin>(plugins); }
        }

        /// <summary>
        /// Gets the Records Provider Manager used by the Controller.
        /// </summary>
        [NotNull, UsedImplicitly]
        public RecordsProviderManager RecordsProviderManager { get; private set; }

        /// <summary>
        /// Gets the Server Options.
        /// </summary>
        [NotNull, UsedImplicitly]
        public ReturnedServerOptionsStruct ServerOptions { get; private set; }

        /// <summary>
        /// Gets the ManiaPlanet WebServices Client used by the Controller.
        /// </summary>
        [NotNull, UsedImplicitly]
        public CombiClient WebServicesClient { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="ManiaNet.DedicatedServer.Controller.ServerController"/> class with the given XmlRpc client and config.
        /// </summary>
        /// <param name="xmlRpcClient">The client used to communicate with the server.</param>
        /// <param name="config">The configuration for the controller.</param>
        public ServerController([NotNull] IXmlRpcClient xmlRpcClient, [NotNull] ServerControllerConfig config)
        {
            this.xmlRpcClient = xmlRpcClient;
            this.xmlRpcClient.MethodResponse += xmlRpcClient_MethodResponse;
            this.xmlRpcClient.ServerCallback += xmlRpcClient_ServerCallback;

            Database = new SqliteConnection("Data Source=" + config.DatabasePath + ";Version=3;");
            Database.Open();

            WebServicesClient = new CombiClient(config.WebServicesLogin, config.WebServicesPassword);

            Configuration = config;

            RegisterCommand("plugins", listPlugins);

            PlayerChat += ServerController_PlayerChat;
            ChatInterfaceManager = new ChatInterfaceManager();
            ChatInterfaceManager.RegisterInterface("chat", new StandardChatInterface(this));
            ChatInterfaceManager.RegisterInterface("console", new ConsoleChatInterface());

            RecordsProviderManager = new RecordsProviderManager();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ManiaNet.DedicatedServer.Controller.ServerController"/> class with the given XmlRpc client and the default config.
        /// </summary>
        /// <param name="xmlRpcClient">The client used to communicate with the server.</param>
        public ServerController([NotNull] IXmlRpcClient xmlRpcClient)
            : this(xmlRpcClient, new ServerControllerConfig())
        { }

        /// <summary>
        /// Sends a method call to the server and waits a maximum of the given timeout before returning whether the call returned (regardless of whether with a fault or not).
        /// </summary>
        /// <typeparam name="TReturn">The method call's returned XmlRpcType.</typeparam>
        /// <typeparam name="TReturnBase">The method call's type of the returned value.</typeparam>
        /// <param name="methodCall">The method call to be executed.</param>
        /// <param name="timeout">The maximum time in milliseconds to wait for a response.
        /// If the timeout is less than or equal to zero, the function will return true, as it won't wait for the response.</param>
        /// <returns>Whether the call was returned (regardless of whether successful or not).</returns>
        /// <remarks>If the timeout is less than or equal to zero, the function will return true, as it won't wait for the response.</remarks>
        [UsedImplicitly]
        public bool CallMethod<TReturn, TReturnBase>([NotNull] XmlRpcMethodCall<TReturn, TReturnBase> methodCall, int timeout) where TReturn : XmlRpcType<TReturnBase>, new()
        {
            var methodHandle = xmlRpcClient.SendRequest(methodCall.GenerateCallXml().ToString());

            if (timeout <= 0)
                return true;

            if (!methodResponses.TryAdd(methodHandle, null))
                return false;

            var response = awaitResponse(methodHandle, timeout);

            if (string.IsNullOrWhiteSpace(response))
                return false;

            try
            {
                if (!methodCall.ParseResponseXml(XDocument.Parse(response, LoadOptions.None).Root))
                    return false;
            }
            catch
            {
                return false;
            }

            return true;
        }

        [UsedImplicitly]
        public void Dispose()
        {
            Database.Dispose();
            if (xmlRpcClient is IDisposable)
                ((IDisposable)xmlRpcClient).Dispose();
        }

        /// <summary>
        /// Starts the controller.
        /// </summary>
        [UsedImplicitly]
        public bool Start()
        {
            xmlRpcClient.StartReceive();

            if (!authenticate())
                return false;

            if (!CallMethod(new EnableCallbacks(true), 2000))
                return false;

            Console.WriteLine("Enabled Callbacks!");

            if (!CallMethod(new SetApiVersion(ApiVersions.Api2013), 2000))
                return false;

            Console.WriteLine("Set API Version to " + ApiVersions.Api2013);

            if (!setupServerOptions())
                return false;

            Console.WriteLine("Got Server Options.");

            if (!CallMethod(new ChatEnableManualRouting(true, false), 2000))
                return false;

            Console.WriteLine("Enabled Manual Chat Routing.");

            loadPlugins();
            startPlugins();

            return true;
        }

        /// <summary>
        /// Stops the controller.
        /// </summary>
        [UsedImplicitly]
        public void Stop()
        {
            stopPlugins();
            unloadPlugins();
            xmlRpcClient.EndReceive();
        }

        private bool authenticate()
        {
            var methodCall = new Authenticate(Configuration.DedicatedLogin, Configuration.DedicatedPassword);

            if (!CallMethod(methodCall, 2000))
                return false;

            return !methodCall.HadFault && methodCall.ReturnValue;
        }

        /// <summary>
        /// Waits a maximum of timeout ms for the response to come in. If it hasn't come in by then, it returns an empty string.
        /// </summary>
        /// <param name="handle">The handle of the method call for which the response is wanted.</param>
        /// <param name="timeout">The maximum number of ms that it waits before returning an empty string.</param>
        /// <returns>The xml formatted method response or an empty string if it took more than timeout ms.</returns>
        private string awaitResponse(uint handle, int timeout)
        {
            var timeSpent = 0;
            if (!methodResponses.ContainsKey(handle))
                return string.Empty;

            while (methodResponses[handle] == null && timeSpent < timeout)
            {
                Thread.Sleep(10);
                timeSpent += 10;
            }

            if (methodResponses[handle] == null)
                return string.Empty;

            string response;
            return methodResponses.TryRemove(handle, out response) ? response : string.Empty;
        }

        private void callEvent(object serverCallback)
        {
            var methodCall = XDocument.Parse((string)serverCallback, LoadOptions.None).Root;

            if (methodCall == null)
                return;

            var methodNameElement = methodCall.Element("methodName");

            if (methodNameElement == null)
                return;

            //Swith to the right method name and call the event.
            switch (methodNameElement.Value)
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
                        var registeredCommandName = registeredCommands.Keys.SingleOrDefault(cmdName => (playerChatCall.Text + " ").ToLower().StartsWith("/" + cmdName + " "));

                        if (registeredCommandName != null)
                            registeredCommands[registeredCommandName](playerChatCall);
                        else if (PlayerChat != null)
                            PlayerChat(this, playerChatCall);
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

        private void listPlugins(ManiaPlanetPlayerChat playerChatCall)
        {
            var response = new StringBuilder("Plugins: ");
            foreach (var plugin in plugins)
            {
                response.Append(PluginBase.GetName(plugin.Value.GetType()));
                response.Append(" (");
                response.Append(plugin.Key);
                response.Append("), ");
            }

            response.Remove(response.Length - 2, 2);

            CallMethod(new ChatSendServerMessageToId(response.ToString(), playerChatCall.ClientId), 0);
        }

        private void ServerController_PlayerChat(ServerController sender, ManiaPlanetPlayerChat methodCall)
        {
            this.ChatInterfaceManager.Get(Configuration.DefaultChat).Send(methodCall.Text, this.ClientsManager.GetClientInfo(methodCall.ClientLogin));
        }

        private bool setupServerOptions()
        {
            var getServerOptionsCall = new GetServerOptions();

            if (!CallMethod(getServerOptionsCall, 5000) || getServerOptionsCall.HadFault)
                return false;

            ServerOptions = getServerOptionsCall.ReturnValue;

            return true;
        }

        #region Command Registration

        /// <summary>
        /// Gets the registered commands (all in lower case).
        /// </summary>
        public IEnumerable<string> RegisteredCommands
        {
            get { return registeredCommands.Keys; }
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

            // ReSharper disable once AccessToModifiedClosure
            // Check if value supposed to be removed is the same as the one associated with the cmdName
            if (!registeredCommands.Any(registeredCommand => registeredCommand.Key.Equals(cmd) && registeredCommand.Value.Equals(cmdAction)))
                return false;

            return registeredCommands.TryRemove(cmd, out cmdAction);
        }

        #endregion Command Registration

        #region Plugin Control

        /// <summary>
        /// Gets whether the plugin with the given identifier is loaded or not.
        /// </summary>
        /// <param name="identifier">The identifier to check.</param>
        /// <returns>Whether the plugin is loaded.</returns>
        [UsedImplicitly]
        public bool IsPluginLoaded(string identifier)
        {
            return plugins.ContainsKey(identifier);
        }

        /// <summary>
        /// Performs a stop-unload-load-start cycle on the plugins.
        /// </summary>
        [UsedImplicitly]
        public void ReloadPlugins()
        {
            stopPlugins();
            unloadPlugins();
            loadPlugins();
            startPlugins();
        }

        private void loadPlugins()
        {
            Console.WriteLine("Loading Plugins...");

            var pluginTypes = PluginLoader.LoadPluginsFromFolders<ControllerPlugin>(Configuration.PluginFolders);

            var pluginInstances = PluginLoader.InstanciatePlugins<ControllerPlugin>(pluginTypes.Append(typeof(LocalRecordsProvider))).ToList();

            Console.WriteLine();
            ControllerPlugin manialinkDisplayManager = null;
            try
            {
                manialinkDisplayManager = pluginInstances.SingleOrDefault(plugin => typeof(IManialinkDisplayManager).IsAssignableFrom(plugin.GetType()));
            }
            catch
            {
                Console.WriteLine("Multiple custom Manialink Display Managers found, using default one.");
            }

            if (manialinkDisplayManager != null)
            {
                Console.WriteLine("Using custom Manialink Display Manager: " + PluginBase.GetName(manialinkDisplayManager.GetType()) + " ("
                                  + PluginBase.GetIdentifier(manialinkDisplayManager.GetType()) + ")");
                ManialinkDisplayManager = (IManialinkDisplayManager)manialinkDisplayManager;
            }
            else
            {
                Console.WriteLine("No custom Manialink Display Manager found, using default one.");

                manialinkDisplayManager = new ManialinkDisplayManager();
                if (manialinkDisplayManager.Load(this))
                {
                    pluginInstances.Add(manialinkDisplayManager);
                    ManialinkDisplayManager = (IManialinkDisplayManager)manialinkDisplayManager;
                }
            }

            Console.WriteLine();
            ControllerPlugin clientsManager = null;
            try
            {
                clientsManager = pluginInstances.SingleOrDefault(plugin => typeof(IClientsManager).IsAssignableFrom(plugin.GetType()));
            }
            catch
            {
                Console.WriteLine("Multiple custom Clients Managers found, using default one.");
            }

            if (clientsManager != null)
            {
                Console.WriteLine("Using custom Clients Manager: " + PluginBase.GetName(clientsManager.GetType()) + " ("
                                  + PluginBase.GetIdentifier(clientsManager.GetType()) + ")");
                ClientsManager = (IClientsManager)clientsManager;
            }
            else
            {
                Console.WriteLine("No custom Clients Manager found, using default one.");

                clientsManager = new ClientsManager();
                if (clientsManager.Load(this))
                {
                    pluginInstances.Add(clientsManager);
                    ClientsManager = (IClientsManager)clientsManager;
                }
            }

            plugins = pluginInstances.Select(plugin =>
            {
                Console.WriteLine(PluginBase.GetName(plugin.GetType()) + " ...");

                var success = plugin.Load(this);
                Console.WriteLine(success ? "OK" : "Failed");

                return new { Plugin = plugin, Success = success };
            })
                                  .Where(loadedPlugin => loadedPlugin.Success)
                                  .Select(loadedPlugin => loadedPlugin.Plugin)
                                  .ToDictionary(plugin => PluginBase.GetIdentifier(plugin.GetType()).Replace(' ', '_').Replace('$', '_').ToLower());

            Console.WriteLine("Completed Loading Plugins.");
        }

        private void startPlugins()
        {
            pluginThreads = plugins.Values.Where(plugin => plugin.RequiresRun).Select(plugin =>
                                                                                      {
                                                                                          var thread = new Thread(plugin.Run)
                                                                                          {
                                                                                              Name = xmlRpcClient.Name + " " + PluginBase.GetName(plugin.GetType()),
                                                                                              IsBackground = true
                                                                                          };
                                                                                          thread.Start();
                                                                                          return thread;
                                                                                      }).ToList();
        }

        private void stopPlugins()
        {
            foreach (var pluginThread in pluginThreads)
                pluginThread.Abort();

            // Allow 1s for plugins to finish the Run method.
            Thread.Sleep(1000);
        }

        private void unloadPlugins()
        {
            Console.WriteLine("Unloading Plugins...");

            foreach (var plugin in plugins.Values)
            {
                Console.WriteLine(PluginBase.GetName(plugin.GetType()) + " ...");
                Console.WriteLine(plugin.Unload() ? "OK" : "Failed");
            }

            Console.WriteLine("Done");
        }

        #endregion Plugin Control

        private void xmlRpcClient_MethodResponse(IXmlRpcClient sender, uint requestHandle, string methodResponse)
        {
            if (methodResponses.ContainsKey(requestHandle))
                methodResponses[requestHandle] = methodResponse;
        }

        private void xmlRpcClient_ServerCallback(IXmlRpcClient sender, string serverCallback)
        {
            Task.Factory.StartNew(callEvent, serverCallback);
        }

        #region Callback Events

        /// <summary>
        /// Fires when a map begins.
        /// </summary>
        [UsedImplicitly]
        public event ServerCallbackEventHandler<ManiaPlanetBeginMap> BeginMap;

        /// <summary>
        /// Fires when a match begins.
        /// </summary>
        [UsedImplicitly]
        public event ServerCallbackEventHandler<ManiaPlanetBeginMatch> BeginMatch;

        /// <summary>
        /// Fires when a bill is updated.
        /// </summary>
        [UsedImplicitly]
        public event ServerCallbackEventHandler<ManiaPlanetBillUpdated> BillUpdated;

        /// <summary>
        /// Fires when the <see cref="ManiaNet.DedicatedServer.XmlRpc.Methods.Echo"/> method is called by another controller.
        /// </summary>
        [UsedImplicitly]
        public event ServerCallbackEventHandler<ManiaPlanetEcho> Echo;

        /// <summary>
        /// Fires when a map ended.
        /// </summary>
        [UsedImplicitly]
        public event ServerCallbackEventHandler<ManiaPlanetEndMap> EndMap;

        /// <summary>
        /// Fires when a match ended.
        /// </summary>
        [UsedImplicitly]
        public event ServerCallbackEventHandler<ManiaPlanetEndMatch> EndMatch;

        /// <summary>
        /// Fires when the map playlist is modified.
        /// </summary>
        [UsedImplicitly]
        public event ServerCallbackEventHandler<ManiaPlanetMapListModified> MapListModified;

        /// <summary>
        /// Fires when a player changes allies.
        /// </summary>
        [UsedImplicitly]
        public event ServerCallbackEventHandler<ManiaPlanetPlayerAlliesChanged> PlayerAlliesChanged;

        /// <summary>
        /// Fires when a client sent a chat message.
        /// <para/>
        /// Doesn't get fired when the message is a registered command.
        /// </summary>
        [UsedImplicitly]
        public event ServerCallbackEventHandler<ManiaPlanetPlayerChat> PlayerChat;

        /// <summary>
        /// Fires when a player drives through a checkpoint.
        /// </summary>
        [UsedImplicitly]
        public event ServerCallbackEventHandler<TrackManiaPlayerCheckpoint> PlayerCheckpoint;

        /// <summary>
        /// Fires when a client connected to the server.
        /// </summary>
        [UsedImplicitly]
        public event ServerCallbackEventHandler<ManiaPlanetPlayerConnect> PlayerConnect;

        /// <summary>
        /// Fires when a client disconnected from the server.
        /// </summary>
        [UsedImplicitly]
        public event ServerCallbackEventHandler<ManiaPlanetPlayerDisconnect> PlayerDisconnect;

        /// <summary>
        /// Fires when a player finishes the map.
        /// </summary>
        [UsedImplicitly]
        public event ServerCallbackEventHandler<TrackManiaPlayerFinish> PlayerFinish;

        /// <summary>
        /// Fires when a player sends incoherent data.
        /// </summary>
        [UsedImplicitly]
        public event ServerCallbackEventHandler<TrackManiaPlayerIncoherence> PlayerIncoherence;

        /// <summary>
        /// Fires when a client's info changed.
        /// </summary>
        [UsedImplicitly]
        public event ServerCallbackEventHandler<ManiaPlanetPlayerInfoChanged> PlayerInfoChanged;

        /// <summary>
        /// Fires when a client answered a mnailink page.
        /// </summary>
        [UsedImplicitly]
        public event ServerCallbackEventHandler<ManiaPlanetPlayerManialinkPageAnswer> PlayerManialinkPageAnswer;

        /// <summary>
        /// Delegate for the various server callback events.
        /// </summary>
        /// <typeparam name="TMethodCall">The type representing the called method.</typeparam>
        /// <param name="sender">The ServerController that fired the event.</param>
        /// <param name="methodCall">The method call information.</param>
        public delegate void ServerCallbackEventHandler<in TMethodCall>(ServerController sender, TMethodCall methodCall)
            where TMethodCall : XmlRpcMethodCall<XmlRpcBoolean, bool>;

        /// <summary>
        /// Fires when the server is started.
        /// </summary>
        [UsedImplicitly]
        public event ServerCallbackEventHandler<ManiaPlanetServerStart> ServerStart;

        /// <summary>
        /// Fires when the server is stopped.
        /// </summary>
        [UsedImplicitly]
        public event ServerCallbackEventHandler<ManiaPlanetServerStop> ServerStop;

        /// <summary>
        /// Fires when the server's status changed.
        /// </summary>
        [UsedImplicitly]
        public event ServerCallbackEventHandler<ManiaPlanetStatusChanged> StatusChanged;

        /// <summary>
        /// Fires when the server receives tunneled data.
        /// </summary>
        [UsedImplicitly]
        public event ServerCallbackEventHandler<ManiaPlanetTunnelDataReceived> TunnelDataReceived;

        /// <summary>
        /// Fires when the current vote's state changes.
        /// </summary>
        [UsedImplicitly]
        public event ServerCallbackEventHandler<ManiaPlanetVoteUpdated> VoteUpdated;

        #endregion Callback Events
    }
}