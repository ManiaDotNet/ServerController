﻿using ManiaNet.DedicatedServer.XmlRpc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ManiaNet.ConsoleTesting
{
    internal class Program
    {
        //System.Web.HttpUtility.HtmlEncode for string values

        private static string allowCallbacksRequest = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><methodCall><methodName>EnableCallbacks</methodName><params><param><value><boolean>1</boolean></value></param></params></methodCall>";

        private static string authenticationRequest = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><methodCall><methodName>Authenticate</methodName><params>" +
            "<param><value><string>SuperAdmin</string></value></param>" +
            "<param><value><string>ManiaNet</string></value></param></params></methodCall>";

        private static string listMethodsRequest = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><methodCall><methodName>system.listMethods</methodName><params></params></methodCall>";

        private static string setApiVersionRequest = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><methodCall><methodName>SetApiVersion</methodName><params><param><value><string>2013-04-16</string></value></param></params></methodCall>";

        private static void Main(string[] args)
        {
            XmlRpcConnection xmlRpcConnection = new XmlRpcConnection(new XmlRpcConnectionConfig(port: 5001, username: "SuperAdmin", password: "ManiaNet"));
            Console.WriteLine("Connecting...");
            xmlRpcConnection.Connect();
            Console.WriteLine("Connected. Sending Authentication...");
            Console.WriteLine("Handle: " + xmlRpcConnection.Send(authenticationRequest));
            Console.WriteLine("Authentication sent");
            //Thread.Sleep(250);

            //Console.WriteLine("Sending listmethods");
            //xmlRpcConnection.Send(listMethodsRequest);
            //Thread.Sleep(250);

            Console.WriteLine("Setting API Version");
            Console.WriteLine("Handle: " + xmlRpcConnection.Send(setApiVersionRequest));
            //Thread.Sleep(250);

            Console.WriteLine("Enabling Callbacks");
            Console.WriteLine("Handle: " + xmlRpcConnection.Send(allowCallbacksRequest));
            //Thread.Sleep(250);

            xmlRpcConnection.Receive();

            Console.ReadLine();
        }
    }
}