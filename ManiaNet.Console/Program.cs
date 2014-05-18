using ManiaNet.DedicatedServer;
using ManiaNet.DedicatedServer.XmlRpc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ManiaNet.ConsoleTesting
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            XmlRpcClient xmlRpcConnection = new XmlRpcClient(new XmlRpcClient.Config(port: 5001));
            xmlRpcConnection.MethodResponse += (client, handle, content) => Console.WriteLine("Handle " + handle + " returned: " + content);

            ServerController controller = new ServerController(xmlRpcConnection, new ServerController.Config(password: "ManiaNet"));
            controller.Start();
            //Thread.Sleep(250);

            //Console.WriteLine("Sending listmethods");
            //xmlRpcConnection.Send(listMethodsRequest);
            //Thread.Sleep(250);

            //Console.WriteLine("Setting API Version");
            //Console.WriteLine("Handle: " + xmlRpcConnection.Send(setApiVersionRequest));
            ////Thread.Sleep(250);

            //Console.WriteLine("Enabling Callbacks");
            //Console.WriteLine("Handle: " + xmlRpcConnection.Send(allowCallbacksRequest));
            //Thread.Sleep(250);

            Console.ReadLine();
        }
    }
}