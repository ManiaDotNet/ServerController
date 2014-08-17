using ManiaNet.DedicatedServer.XmlRpc;
using ManiaNet.DedicatedServer.XmlRpc.Methods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ManiaNet.DedicatedServer.Controller.Tests
{
    [TestClass]
    public class Controller
    {
        private ServerController controller;

        public Controller()
        {
            var client = new XmlRpcClient(new XmlRpcClient.Config(port: 5001));
            controller = new ServerController(client);
        }

        [TestMethod]
        public void CantRegisterCommandTwice()
        {
            Action<ManiaPlanetPlayerChat> testAction = playerChatCall => Console.WriteLine(playerChatCall.Text);
            Assert.IsTrue(controller.RegisterCommand("test", testAction));
            Assert.IsFalse(controller.RegisterCommand("test", testAction));
            Assert.IsTrue(controller.UnregisterCommand("test", testAction));
        }

        [TestMethod]
        public void CantUnregisterDifferentCommand()
        {
            Action<ManiaPlanetPlayerChat> testAction = playerChatCall => Console.WriteLine(playerChatCall.Text);
            Assert.IsTrue(controller.RegisterCommand("test", testAction));
            Assert.IsFalse(controller.UnregisterCommand("test", playerChatCall => Console.WriteLine(playerChatCall.Text)));
        }

        [TestMethod]
        public void CanUnregisterSameCommand()
        {
            Action<ManiaPlanetPlayerChat> testAction = playerChatCall => Console.WriteLine(playerChatCall.Text);
            Assert.IsTrue(controller.RegisterCommand("test", testAction));
            Assert.IsTrue(controller.UnregisterCommand("test", testAction));
        }
    }
}