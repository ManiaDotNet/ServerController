using ManiaNet.DedicatedServer.Controller.Plugins;
using ManiaNet.DedicatedServer.XmlRpc;
using ManiaNet.DedicatedServer.XmlRpc.MethodCalls;
using ManiaNet.DedicatedServer.XmlRpc.Types;
using SharpPlugins;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ManiaNet.DedicatedServer.Controller
{
    public class ServerController : IDisposable
    {
        private ConcurrentDictionary<uint, string> methodResponses = new ConcurrentDictionary<uint, string>();
        private IEnumerable<Thread> pluginThreads;
        private IXmlRpcClient xmlRpcClient;

        public Config Configuration { get; private set; }

        public IEnumerable<ControllerPlugin> Plugins { get; private set; }

        public ServerController(IXmlRpcClient xmlRpcClient, Config config)
        {
            this.xmlRpcClient = xmlRpcClient;
            this.xmlRpcClient.MethodResponse += xmlRpcClient_MethodResponse;
            this.xmlRpcClient.ServerCallback += xmlRpcClient_ServerCallback;

            Configuration = config;
        }

        /// <summary>
        /// Sends a method call to the server and waits a maximum of the given timeout before returning whether the call returned (regardless of whether successful or not).
        /// </summary>
        /// <typeparam name="TReturn">The method call's returned XmlRpcType.</typeparam>
        /// <typeparam name="TReturnBase">The method call's type of the returned value.</typeparam>
        /// <param name="methodCall">The method call to be executed.</param>
        /// <param name="timeout">The maximum time in milliseconds to wait for a response.</param>
        /// <returns>Whether the call was returned (regardless of whether successful or not).</returns>
        public bool CallMethod<TReturn, TReturnBase>(MethodCall<TReturn, TReturnBase> methodCall, int timeout) where TReturn : XmlRpcType<TReturnBase>, new()
        {
            uint methodHandle = xmlRpcClient.SendRequest(methodCall.GenerateXml().ToString());

            if (!methodResponses.TryAdd(methodHandle, null))
                return false;

            string response = awaitResponse(methodHandle, timeout);

            if (string.IsNullOrEmpty(response))
                return false;

            try
            {
                methodCall.ParseXml(XDocument.Parse(response, LoadOptions.None).Root);
            }
            catch { return false; }

            return methodCall.IsCompleted;
        }

        public void Dispose()
        {
            xmlRpcClient.Dispose();
        }

        /// <summary>
        /// Starts the controller.
        /// </summary>
        public void Start()
        {
            xmlRpcClient.StartReceive();
            authenticate();
            loadPlugins();
            pluginThreads = Plugins.Select(plugin =>
                {
                    Thread thread = new Thread(plugin.Run);
                    thread.Name = xmlRpcClient.Name + " " + ControllerPlugin.GetName(plugin.GetType());
                    thread.IsBackground = true;
                    thread.Start();
                    return thread;
                });
        }

        /// <summary>
        /// Stops the controller.
        /// </summary>
        public void Stop()
        {
            unloadPlugins();
            xmlRpcClient.EndReceive();
        }

        private bool authenticate()
        {
            var methodCall = new Authenticate(Configuration.Login, Configuration.Password);

            if (!CallMethod(methodCall, 2000))
                return false;

            return !methodCall.IsCompleted ? false : methodCall.HadFault ? false : methodCall.ReturnValue;
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

        private void loadPlugins()
        {
            var pluginTypes = PluginLoader.LoadPluginsFromFolders<ControllerPlugin>(Configuration.PluginFolders);
            Plugins = PluginLoader.InstanciatePlugins<ControllerPlugin>(pluginTypes);

            Console.WriteLine("Loading Plugins...");
            foreach (var plugin in Plugins)
            {
                Console.Write(ControllerPlugin.GetName(plugin.GetType()) + " ... ");
                Console.WriteLine(plugin.Load(this) ? "OK" : "Failed");
            }
            Console.WriteLine("Done");
        }

        private void unloadPlugins()
        {
            Console.WriteLine("Unloading Plugins...");
            foreach (var plugin in Plugins)
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
            Task.Factory.StartNew(() =>
                {
                    XElement methodCall = XDocument.Parse(serverCallback, LoadOptions.None).Root;
                    string methodName = methodCall.Element(XName.Get("methodName")).Value;
                    switch (methodName)
                    {
                        //Swith to the right method name and call the event.
                    }
                });
        }

        /// <summary>
        /// Represents a configuration for the server controller.
        /// </summary>
        public class Config
        {
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
        }
    }
}