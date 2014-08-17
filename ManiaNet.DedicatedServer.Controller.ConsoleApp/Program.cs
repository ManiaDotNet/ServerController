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
        private static void main(string[] args)
        {
            var xmlRpcConnection = new XmlRpcClient(new XmlRpcClient.Config(port: 5001));

#if DEBUG
            xmlRpcConnection.MethodResponse += (client, handle, content) => Console.WriteLine("Handle " + handle + " returned:\r\n" + content);
            xmlRpcConnection.ServerCallback += (client, content) => Console.WriteLine("Callback:\r\n" + content);
#endif

            var controller = new ServerController(xmlRpcConnection);
            controller.Start();

            Console.ReadLine();
        }
    }
}