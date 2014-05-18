using ManiaNet.DedicatedServer.XmlRpc;
using ManiaNet.DedicatedServer.XmlRpc.MethodCalls;
using ManiaNet.DedicatedServer.XmlRpc.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace ManiaNet.DedicatedServer.Controller
{
    public class ServerController
    {
        private Dictionary<uint, string> methodResponses = new Dictionary<uint, string>();
        private IXmlRpcClient xmlRpcClient;

        public Config Configuration { get; private set; }

        public ServerController(IXmlRpcClient xmlRpcClient, Config config)
        {
            this.xmlRpcClient = xmlRpcClient;
            this.xmlRpcClient.MethodResponse += xmlRpcClient_MethodResponse;
            this.xmlRpcClient.ServerCallback += xmlRpcClient_ServerCallback;
            Configuration = config;
        }

        public void Start()
        {
            xmlRpcClient.StartReceive();
            authenticate();
        }

        public void Stop()
        {
            xmlRpcClient.EndReceive();
        }

        private bool authenticate()
        {
            string response = awaitResponse(xmlRpcClient.SendRequest(new XmlRpcAuthenticate(Configuration.Login, Configuration.Password).GetXml()), 2000);
            var boolResponse = new XmlRpcBoolean().ParseXml(XDocument.Parse(response).Root.Descendants(XName.Get("boolean")).First());
            return boolResponse.Value;
        }

        /// <summary>
        /// Waits a maximum of timeout ms for the response to come in. If it hasn't come in by then, it returns an empty string.
        /// </summary>
        /// <param name="handle">The handle of the method call for which the response is wanted.</param>
        /// <param name="timeout">The maximum number of ms that it waits before returning an empty string.</param>
        /// <returns>The xml formatted method response or an empty string if it tookmore than timeout ms.</returns>
        private string awaitResponse(uint handle, int timeout)
        {
            int timeSpent = 0;
            while (!methodResponses.ContainsKey(handle) && timeSpent < timeout)
            {
                Thread.Sleep(10);
                timeSpent += 10;
            }

            if (methodResponses.ContainsKey(handle))
            {
                string response = methodResponses[handle];
                methodResponses.Remove(handle);
                return response;
            }

            return string.Empty;
        }

        private void xmlRpcClient_MethodResponse(IXmlRpcClient sender, uint requestHandle, string methodResponse)
        {
            methodResponses.Add(requestHandle, methodResponse);
        }

        private void xmlRpcClient_ServerCallback(IXmlRpcClient sender, string serverCallback)
        {
            XElement methodCall = XDocument.Parse(serverCallback, LoadOptions.None).Root;
            string methodName = methodCall.Element(XName.Get("name")).Value;
            switch (methodName)
            {
                //Swith to the right method name and call the event.
            }
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
            /// Creates a new instance of the <see cref="ManiaNet.DedicatedServer.ServerController.Config"/> class with the given Login and Password.
            /// </summary>
            /// <param name="login">The Login that the controller authenticates with; SuperAdmin by default.</param>
            /// <param name="password">The Password that the controller authenticates with; SuperAdmin by default.</param>
            public Config(string login = "SuperAdmin", string password = "SuperAdmin")
            {
                Login = login;
                Password = password;
            }
        }
    }
}