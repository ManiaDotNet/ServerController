using ManiaNet.DedicatedServer.Controller.Plugins.Extensibility.Clients;

namespace ManiaNet.DedicatedServer.Controller.Plugins.Extensibility.Chat
{
    internal interface IChatInterface
    {

        /// <summary>
        /// Send a message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="sender">A sender to be displayed as author of the message.</param>
        /// <param name="image">Optionally an image to display along if supported.</param>
        public void Send(string message, IClient sender = null, object image = null);

        /// <summary>
        /// Send a message to a certain player.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="recipient">The receiving player's login.</param>
        /// <param name="sender">A sender to be displayed as author of the message.</param>
        /// <param name="image">Optionally an image to display along if supported.</param>
        public void SendTo(string message, string recipient, IClient sender = null, object image = null);

    }
}