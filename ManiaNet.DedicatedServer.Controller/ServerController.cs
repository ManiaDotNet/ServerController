using ManiaNet.DedicatedServer.Controller.Configuration;
using ManiaNet.DedicatedServer.Controller.Plugins;
using ManiaNet.DedicatedServer.XmlRpc.Methods;
using RazorEngine;
using SharpPlugins;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
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
    public class ServerController
    {
        private readonly ConcurrentDictionary<uint, string> methodResponses = new ConcurrentDictionary<uint, string>();
        private readonly ConcurrentDictionary<string, Action<ManiaPlanetPlayerChat>> registeredCommands = new ConcurrentDictionary<string, Action<ManiaPlanetPlayerChat>>();
        private readonly IXmlRpcClient xmlRpcClient;
        private List<string> clients = new List<string>();
        private Thread manialinkSendLoopThread;
        private Dictionary<string, ControllerPlugin> plugins = new Dictionary<string, ControllerPlugin>();
        private List<Thread> pluginThreads;

        /// <summary>
        /// Gets the logins of the connected clients.
        /// </summary>
        public IEnumerable<string> Clients
        {
            get
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                // If clients is returned as it is, user can modify the list by casting back to List<string>
                foreach (var client in clients)
                    yield return client;
            }
        }

        /// <summary>
        /// Gets the configuration of the controller.
        /// </summary>
        public ServerControllerConfig Configuration { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="ManiaNet.DedicatedServer.Controller.ServerController"/> class with the given XmlRpc client and config.
        /// </summary>
        /// <param name="xmlRpcClient">The client used to communicate with the server.</param>
        /// <param name="config">The configuration for the controller.</param>
        public ServerController(IXmlRpcClient xmlRpcClient, ServerControllerConfig config)
        {
            this.xmlRpcClient = xmlRpcClient;
            this.xmlRpcClient.MethodResponse += xmlRpcClient_MethodResponse;
            this.xmlRpcClient.ServerCallback += xmlRpcClient_ServerCallback;

            Configuration = config;

            RegisterCommand("plugins", playerChatCall =>
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
                                       });

            if (Configuration.AllowManialinkHiding)
            {
                RegisterCommand("hide", playerChatCall =>
                                        {
                                            var pluginIds = playerChatCall.Text.ToLower().Split(' ').Skip(1).ToArray();

                                            if (pluginIds.Length < 1)
                                                CallMethod(new ChatSendServerMessageToId("Usage: /hide pluginId1 [pluginId2 ...]", playerChatCall.ClientId), 0);

                                            var hidePlugins = pluginIds.Where(pluginId => plugins.ContainsKey(pluginId)).ToArray();
                                            if (hidePlugins.Length > 0)
                                            {
                                                if (!clientsDisabledPluginDisplays.ContainsKey(playerChatCall.ClientLogin))
                                                    clientsDisabledPluginDisplays.Add(playerChatCall.ClientLogin, new List<string>());

                                                var response = new StringBuilder("Hid display of plugins: ");
                                                foreach (var hidePlugin in hidePlugins)
                                                {
                                                    if (!clientsDisabledPluginDisplays[playerChatCall.ClientLogin].Contains(hidePlugin))
                                                        clientsDisabledPluginDisplays[playerChatCall.ClientLogin].Add(hidePlugin);

                                                    response.Append(PluginBase.GetName(plugins[hidePlugin].GetType()));
                                                    response.Append(" (");
                                                    response.Append(hidePlugin);
                                                    response.Append("), ");
                                                }

                                                clientManialinksNeedRefresh = true;

                                                response.Remove(response.Length - 2, 2);
                                                CallMethod(new ChatSendServerMessageToId(response.ToString(), playerChatCall.ClientId), 0);
                                            }

                                            var unknownPlugins = pluginIds.Where(pluginId => !plugins.ContainsKey(pluginId)).ToArray();
                                            if (unknownPlugins.Length > 0)
                                            {
                                                var response = new StringBuilder("Plugins not loaded: ");
                                                foreach (var unknownPlugin in unknownPlugins)
                                                {
                                                    response.Append(unknownPlugin);
                                                    response.Append(", ");
                                                }

                                                response.Remove(response.Length - 2, 2);
                                                CallMethod(new ChatSendServerMessageToId(response.ToString(), playerChatCall.ClientId), 0);
                                            }
                                        });

                RegisterCommand("unhide", playerChatCall =>
                                          {
                                              string[] pluginIds = playerChatCall.Text.ToLower().Split(' ').Skip(1).ToArray();

                                              if (pluginIds.Length < 1)
                                                  CallMethod(new ChatSendServerMessageToId("Usage: /unhide pluginId1 [pluginId2 ...]", playerChatCall.ClientId), 0);

                                              if (!clientsDisabledPluginDisplays.ContainsKey(playerChatCall.ClientLogin)
                                                  || !clientsDisabledPluginDisplays[playerChatCall.ClientLogin].Any())
                                              {
                                                  CallMethod(new ChatSendServerMessageToId("No plugins to unhide.", playerChatCall.ClientId), 0);
                                                  return;
                                              }

                                              var unhidePlugins = pluginIds.Where(pluginId => clientsDisabledPluginDisplays[playerChatCall.ClientLogin].Remove(pluginId)).ToArray();

                                              if (unhidePlugins.Length > 0)
                                              {
                                                  var response = new StringBuilder("Unhid display of plugins: ");
                                                  foreach (var unhidePlugin in unhidePlugins)
                                                  {
                                                      response.Append(PluginBase.GetName(plugins[unhidePlugin].GetType()));
                                                      response.Append(" (");
                                                      response.Append(unhidePlugin);
                                                      response.Append("), ");
                                                  }

                                                  clientManialinksNeedRefresh = true;

                                                  response.Remove(response.Length - 2, 2);
                                                  CallMethod(new ChatSendServerMessageToId(response.ToString(), playerChatCall.ClientId), 0);
                                              }

                                              var unknownPlugins = pluginIds.Where(pluginId => !plugins.ContainsKey(pluginId)).ToArray();
                                              if (unknownPlugins.Length > 0)
                                              {
                                                  var response = new StringBuilder("Plugins not loaded: ");
                                                  foreach (var unknownPlugin in unknownPlugins)
                                                  {
                                                      response.Append(unknownPlugin);
                                                      response.Append(", ");
                                                  }

                                                  response.Remove(response.Length - 2, 2);
                                                  CallMethod(new ChatSendServerMessageToId(response.ToString(), playerChatCall.ClientId), 0);
                                              }
                                          });
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ManiaNet.DedicatedServer.Controller.ServerController"/> class with the given XmlRpc client and the config loaded from disk/default.
        /// </summary>
        /// <param name="xmlRpcClient">The client used to communicate with the server.</param>
        public ServerController(IXmlRpcClient xmlRpcClient)
            : this(xmlRpcClient, new ServerControllerConfig.Builder().Config)
        { }

        /// <summary>
        /// Sends a method call to the server and waits a maximum of the given timeout before returning whether the call returned (regardless of whether with a fault or not).
        /// </summary>
        /// <typeparam name="TReturn">The method call's returned XmlRpcType.</typeparam>
        /// <typeparam name="TReturnBase">The method call's type of the returned value.</typeparam>
        /// <param name="methodCall">The method call to be executed.</param>
        /// <param name="timeout">The maximum time in milliseconds to wait for a response.</param>
        /// <returns>Whether the call was returned (regardless of whether successful or not).</returns>
        public bool CallMethod<TReturn, TReturnBase>(XmlRpcMethodCall<TReturn, TReturnBase> methodCall, int timeout) where TReturn : XmlRpcType<TReturnBase>, new()
        {
            uint methodHandle = xmlRpcClient.SendRequest(methodCall.GenerateCallXml().ToString());

            if (timeout > 0)
            {
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
                catch
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Starts the controller.
        /// </summary>
        public bool Start()
        {
            xmlRpcClient.StartReceive();

            if (!authenticate())
                return false;

            CallMethod(new EnableCallbacks(true), 2000);
            CallMethod(new SetApiVersion(ApiVersions.Api2013), 2000);

            if (!setUpClientsList())
                return false;

            loadPlugins();
            startPlugins();

            startManialinkSendLoop();

            return true;
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

        private bool authenticate()
        {
            var methodCall = new Authenticate(Configuration.Login, Configuration.Password);

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

        private bool setUpClientsList()
        {
            var getPlayerListCall = new GetPlayerList(100, 0);

            if (!CallMethod(getPlayerListCall, 1000))
                return false;

            if (getPlayerListCall.HadFault)
                return false;

            clients = getPlayerListCall.ReturnValue.Select(clientInfo => clientInfo.Value.Login).ToList();

            PlayerConnect += (controller, playerConnectCall) =>
                             {
                                 Console.WriteLine(playerConnectCall.Login + " joined");
                                 if (!clients.Contains(playerConnectCall.Login))
                                 {
                                     clients.Add(playerConnectCall.Login);
                                     clientManialinksNeedRefresh = true;
                                 }
                             };
            PlayerDisconnect += (controller, playerDisconnectCall) => clients.Remove(playerDisconnectCall.Login);

            return true;
        }

        #region Manialink Display

        private readonly Dictionary<string, List<string>> clientsDisabledPluginDisplays = new Dictionary<string, List<string>>();

        private readonly string manialinkTemplate =
            new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("ManiaNet.DedicatedServer.Controller.ClientManialinkTemplate.csxml")).ReadToEnd();

        // Default is false
        private volatile bool clientManialinksNeedRefresh;

        private void manialinkSendLoop()
        {
            var timer = new Stopwatch();
            var clientManialinkElements = new List<string>();

            while (true)
            {
                while (!clientManialinksNeedRefresh)
                    Thread.Sleep(100);

                clientManialinksNeedRefresh = false;
                timer.Restart();

                foreach (var client in Clients)
                {
                    clientManialinkElements.Clear();

                    List<string> clientDisabledPluginDisplays;
                    clientsDisabledPluginDisplays.TryGetValue(client, out clientDisabledPluginDisplays);
                    clientDisabledPluginDisplays = clientDisabledPluginDisplays ?? new List<string>();

                    // Iterate over all the plugins that aren't on the disabled-list of the client.
                    foreach (var plugin in plugins.Where(plugin => !clientDisabledPluginDisplays.Contains(plugin.Key)).Select(pluginKv => pluginKv.Value))
                    {
                        // See if there's a value specifically for the client, or otherwise one for all.
                        if (plugin.ClientManialinks.ContainsKey(client))
                            clientManialinkElements.Add(plugin.ClientManialinks[client]);
                        else if (plugin.ClientManialinks.ContainsKey("*"))
                            clientManialinkElements.Add(plugin.ClientManialinks["*"]);
                    }

                    string clientManialink = WebUtility.HtmlDecode(Razor.Parse(manialinkTemplate, clientManialinkElements, client));

                    CallMethod(new SendDisplayManialinkPageToLogin(client, clientManialink, 0, false), 0);
                }

                timer.Stop();

                int delay = Configuration.ManialinkRefreshInterval - (int)timer.ElapsedMilliseconds;

                if (delay > 0)
                    Thread.Sleep(delay);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private void plugin_ClientManialinksChanged()
        {
            clientManialinksNeedRefresh = true;
        }

        private void startManialinkSendLoop()
        {
            manialinkSendLoopThread = new Thread(manialinkSendLoop)
            {
                Name = xmlRpcClient.Name + " Manialink Send Loop",
                IsBackground = true
            };
            manialinkSendLoopThread.Start();
        }

        #endregion Manialink Display

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
        public bool IsPluginLoaded(string identifier)
        {
            return plugins.ContainsKey(identifier);
        }

        /// <summary>
        /// Performs a stop-unload-load-start cycle on the plugins.
        /// </summary>
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

            plugins = PluginLoader.InstanciatePlugins<ControllerPlugin>(pluginTypes).Select(plugin =>
                                                                                            {
                                                                                                Console.Write(PluginBase.GetName(plugin.GetType()) + " ... ");
                                                                                                plugin.ClientManialinksChanged += plugin_ClientManialinksChanged;

                                                                                                bool success = plugin.Load(this);
                                                                                                Console.WriteLine(success ? "OK" : "Failed");

                                                                                                return new { Plugin = plugin, Success = success };
                                                                                            })
                                  .Where(loadedPlugin => loadedPlugin.Success)
                                  .Select(loadedPlugin => loadedPlugin.Plugin)
                                  .ToDictionary(plugin => PluginBase.GetIdentifier(plugin.GetType()).Replace(' ', '_').Replace('$', '_').ToLower());

            Console.WriteLine("Done");
        }

        private void startPlugins()
        {
            pluginThreads = plugins.Values.Select(plugin =>
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

            // Allow 100ms for plugins to finish the Run method.
            Thread.Sleep(100);
        }

        private void unloadPlugins()
        {
            Console.WriteLine("Unloading Plugins...");

            foreach (var plugin in plugins.Values)
            {
                Console.Write(PluginBase.GetName(plugin.GetType()) + " ... ");
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
    }
}