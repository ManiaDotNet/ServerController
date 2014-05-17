using ManiaNet.DedicatedServer.XmlRpc;
using ManiaNet.DedicatedServer.XmlRpc.MethodCalls;
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
            Console.WriteLine("Connecting...");
            xmlRpcConnection.Connect();
            Console.WriteLine("Connected. Sending Authentication...");
            Console.WriteLine("Handle: " + xmlRpcConnection.SendRequest(new XmlRpcAuthenticate("SuperAdmin", "ManiaNet").GetXml()));
            Console.WriteLine("Authentication sent");
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

            xmlRpcConnection.MethodResponse += (handle, content) => Console.WriteLine();

            xmlRpcConnection.Receive();

            Console.ReadLine();
        }
    }
}