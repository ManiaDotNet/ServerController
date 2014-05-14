using ManiaNet.DedicatedServer.XmlRpc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ManiaNet.ConsoleTesting
{
    internal class Program
    {
        //System.Web.HttpUtility.HtmlEncode for string values

        private static string authenticationRequest = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><methodCall><methodName>Authenticate</methodName><params>" +
            "<param><value><string>SuperAdmin</string></value></param>\n" +
            "<param><value><string>ManiaNet</string></value></param>\n</params></methodCall>";

        private static string listMethodsRequest = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><methodCall><methodName>system.listMethods</methodName><params></params></methodCall>";

        private static void Main(string[] args)
        {
            XmlRpcConnection xmlRpcConnection = new XmlRpcConnection(new XmlRpcConnectionConfig(port: 5001, username: "SuperAdmin", password: "ManiaNet"));
            Console.WriteLine("Connecting...");
            xmlRpcConnection.Connect();
            Console.WriteLine("Connected. Sending Authentication");
            xmlRpcConnection.Send(authenticationRequest);
            Console.WriteLine("Authentication sent");
            Console.WriteLine("Sending listmethods");
            xmlRpcConnection.Send(listMethodsRequest);
            xmlRpcConnection.Receive();

            Console.ReadLine();
        }
    }
}