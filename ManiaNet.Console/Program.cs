using ManiaNet.DedicatedServer.Controller;
using ManiaNet.DedicatedServer.XmlRpc;
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

            ServerController controller = new ServerController(xmlRpcConnection, new ServerController.Config(password: "ManiaNet"));
            controller.Start();

            Thread.Sleep(250);

            xmlRpcConnection.SendRequest(XmlRpcConstants.XmlDeclaration + XmlRpcConstants.MethodCallAndNameOpening + "GetCurrentCallVote" + XmlRpcConstants.MethodNameClosingAndParamsOpening + XmlRpcConstants.ParamsAndMethodCallClosing);

            //Console.WriteLine("Setting API Version");
            //Console.WriteLine("Handle: " + xmlRpcConnection.Send(setApiVersionRequest));

            //Console.WriteLine("Enabling Callbacks");
            //Console.WriteLine("Handle: " + xmlRpcConnection.Send(allowCallbacksRequest));

            Console.ReadLine();
        }
    }
}