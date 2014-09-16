using ManiaNet.DedicatedServer.Controller.Plugins.Extensibility.Chat;
using ManiaNet.DedicatedServer.Controller.Plugins.Extensibility.Clients;
using ManiaNet.DedicatedServer.XmlRpc.Methods;
using System;

namespace ManiaNet.DedicatedServer.Controller.Plugins.ChatInterfaces
{
    public class StandardChatInterface : IChatInterface
    {
        private ServerController controller;

        public StandardChatInterface(ServerController controller)
        {
            this.controller = controller;
        }

        public void Send(string message, IClient sender = null, object image = null)
        {
            controller.CallMethod(new ChatSendServerMessage(formatMessage(sender, message)), 0);
        }

        public void Send(string message, string sender, object image = null)
        {
            Send(message, controller.ClientsManager.GetClientInfo(sender), image);
        }

        public void SendTo(string message, string recipient, IClient sender = null, object image = null)
        {
            // TODO: var chatmethod = new ChatSendServerMessageToLogin(msg, recipient);
            controller.CallMethod(new ChatSendServerMessageToId(formatMessage(sender, message),
                (int)controller.ClientsManager.GetClientInfo(recipient).Id), 0);
        }

        private static string formatMessage(IClient sender, string message)
        {
            if (sender != null)
                return string.Format("$z$s[$<$fff{0}$>] {1}",
                    sender.Nickname.Contains("$s") ? sender.Nickname.Remove(sender.Nickname.IndexOf("$s"), 2) : sender.Nickname,
                    message);

            return message;
        }
    }
}