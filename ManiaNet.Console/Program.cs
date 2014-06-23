using ManiaNet.DedicatedServer;
using ManiaNet.DedicatedServer.Controller;
using ManiaNet.DedicatedServer.XmlRpc.MethodCalls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using XmlRpc;

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
            xmlRpcConnection.SendRequest(new ChatSendServerMessage("Teeest").ToString());
            string request = XmlRpcConstants.MethodCallAndNameOpening + "GetTimeAttackSyncdhStartPeriod" + XmlRpcConstants.MethodNameClosingAndParamsOpening /* + XmlRpcConstants.ParamOpening + "<value><string>Description</string></value>" + XmlRpcConstants.ParamClosing + XmlRpcConstants.ParamOpening + "<value><string>banane9</string></value>" + XmlRpcConstants.ParamClosing */ + XmlRpcConstants.ParamsAndMethodCallClosing;
            xmlRpcConnection.SendRequest(request);

            //Console.WriteLine("Setting API Version");
            //Console.WriteLine("Handle: " + xmlRpcConnection.Send(setApiVersionRequest));

            //Console.WriteLine("Enabling Callbacks");
            //Console.WriteLine("Handle: " + xmlRpcConnection.Send(allowCallbacksRequest));

            Console.ReadLine();
        }
    }
}