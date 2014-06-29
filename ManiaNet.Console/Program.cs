using ManiaNet.DedicatedServer;
using ManiaNet.DedicatedServer.Controller;
using ManiaNet.DedicatedServer.XmlRpc;
using ManiaNet.DedicatedServer.XmlRpc.Methods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ManiaNet.ConsoleTesting
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            XmlRpcClient xmlRpcConnection = new XmlRpcClient(new XmlRpcClient.Config(port: 5001));
            xmlRpcConnection.MethodResponse += (client, handle, content) => Console.WriteLine("Handle " + handle + " returned:\r\n" + content);
            xmlRpcConnection.ServerCallback += (client, content) => Console.WriteLine("Callback:\r\n" + content);

            ServerController controller = new ServerController(xmlRpcConnection, new ServerController.Config(password: "ManiaNet"));
            controller.Start();

            Thread.Sleep(250);
            xmlRpcConnection.SendRequest(new EnableCallbacks(true).ToString());
            xmlRpcConnection.SendRequest(new SetApiVersion(ApiVersions.Api2013).ToString());

            Action<ManiaPlanetPlayerChat> testAction = playerChatCall => Console.WriteLine(playerChatCall.Text);
            Console.WriteLine("Trying to register test command: " + (controller.RegisterCommand("test", testAction) ? "Success" : "Failed"));

            Thread.Sleep(30000);
            Console.WriteLine("Trying to unregister test command: " + (controller.UnregisterCommand("test", testAction) ? "Success" : "Failed"));

            Console.ReadLine();
        }
    }
}