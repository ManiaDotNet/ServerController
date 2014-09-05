using ManiaNet.DedicatedServer.Controller.Configuration;
using ManiaNet.DedicatedServer.Controller.ConsoleApp.Annotations;
using ManiaNet.DedicatedServer.XmlRpc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ManiaNet.DedicatedServer.Controller.ConsoleApp
{
    internal class Program
    {
        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static void Main(string[] args)
        {
            var serverControllerConfig = ConfigLoader.LoadConfigFrom<ServerControllerConfig>("ServerControllerConfig.xml");

            var xmlRpcConnection = new XmlRpcClient(new XmlRpcClient.Config(port: 5001));

#if DEBUG
            xmlRpcConnection.MethodResponse += (client, handle, content) => Console.WriteLine("Handle " + handle + " returned:\r\n" + content);
            xmlRpcConnection.ServerCallback += (client, content) => Console.WriteLine("Callback:\r\n" + content);
#endif

            var controller = new ServerController(xmlRpcConnection, serverControllerConfig);

            if (controller.Start())
                Console.WriteLine("Controller started successfully.");
            else
                Console.WriteLine("Controller start failed.");

            Console.ReadLine();
        }
    }
}