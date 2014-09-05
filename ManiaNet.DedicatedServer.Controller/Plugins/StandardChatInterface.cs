using ManiaNet.DedicatedServer.Controller.Plugins.Extensibility.Chat;
using ManiaNet.DedicatedServer.Controller.Plugins.Extensibility.Clients;
using ManiaNet.DedicatedServer.XmlRpc.Methods;
using System;

namespace ManiaNet.DedicatedServer.Controller.Plugins
{
    internal class StandardChatInterface : IChatInterface
    {
        private ServerController controller;

        public StandardChatInterface(ServerController controller)
        {
            this.controller = controller;
        }

        public void Send(string message, IClient sender = null, object image = null)
        {
            string msg = message;
            if (sender != null)
                msg = String.Format("$s[$<$s{0}$>] {1}$z", sender.Nickname.Replace("$s", ""), message);
            var chatmethod = new ChatSendServerMessage(msg);
            controller.CallMethod(chatmethod, 1000);
        }

        public void Send(string message, string sender, object image = null)
        {
            Send(message, controller.ClientsManager.GetClientInfo(sender), image);
        }

        public void SendTo(string message, string recipient, IClient sender = null, object image = null)
        {
            string msg = message;
            if (sender != null)
                msg = String.Format("$s[$<$s{0}$>] {1}$z", sender.Nickname.Replace("$s", ""), message);
            // TODO: var chatmethod = new ChatSendServerMessageToLogin(msg, recipient);
            var chatmethod = new ChatSendServerMessageToId(msg, (int)controller.ClientsManager.GetClientInfo(recipient).Id);
            controller.CallMethod(chatmethod, 1000);
        }
    }
}