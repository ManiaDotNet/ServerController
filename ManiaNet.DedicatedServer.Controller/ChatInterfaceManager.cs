using ManiaNet.DedicatedServer.Controller.Plugins.Extensibility.Chat;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Reflection;

namespace ManiaNet.DedicatedServer.Controller.Plugins
{
    internal class UnknownChatInterfaceException : Exception, ISerializable
    {
        public UnknownChatInterfaceException(string message)
            : base(message) { }

        public UnknownChatInterfaceException(string message, Exception innerException)
            : base(message, innerException) { }

        protected UnknownChatInterfaceException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }

    class ChatInterfacePermissionException : Exception, ISerializable
    {
        public ChatInterfacePermissionException(string message)
            : base(message) { }

        public ChatInterfacePermissionException(string message, Exception innerException)
            : base(message, innerException) { }

        protected ChatInterfacePermissionException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }

    internal class ChatInterfaceManager
    {
        private Dictionary<string, IChatInterface> interfaces = new Dictionary<string, IChatInterface>();
        private Dictionary<string, string> registrars = new Dictionary<string, string>();
        
        /// <summary>
        /// Returns the IChatInterface registered for the given name.
        /// </summary>
        /// <param name="name"></param>
        /// <exception cref="UnknownChatInterfaceException">Thrown if the given name is not registered.</exception>
        /// <returns></returns>
        public IChatInterface Get(string name)
        {
            if (interfaces.ContainsKey(name))
                return interfaces[name];
            throw new UnknownChatInterfaceException(String.Format("{0} is not a registered ChatInterface.", name));
        }

        /// <summary>
        /// Registers a IChatInterface.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="chatinterface"></param>
        /// <exception cref="ChatInterfacePermissionException">Thrown if the given name is already registered.</exception>
        /// <returns></returns>
        public bool RegisterInterface(string name, IChatInterface chatinterface)
        {
            if (!this.interfaces.ContainsKey(name))
            {
                this.interfaces.Add(name, chatinterface);
                this.registrars.Add(name, Assembly.GetCallingAssembly().FullName);
                return true;
            }
            throw new ChatInterfacePermissionException(String.Format("The ChatInterface {0} is already registered.", name));
        }

        /// <summary>
        /// Unregisters a registered IChatInterface.
        /// </summary>
        /// <param name="name">The ChatInterface to unregister.</param>
        /// <exception cref="UnknownChatInterfaceException">Thrown if the given name is not registered.</exception>
        /// <exception cref="ChatInterfacePermissionException">Thrown if you try to unregister a ChatInterface you didn't register yourself.</exception>
        /// <returns>True on success.</returns>
        public bool UnregisterInterface(string name)
        {
            if (!interfaces.ContainsKey(name))
                throw new UnknownChatInterfaceException(String.Format("{0} is not a registered ChatInterface.", name));
            if (registrars[name] != Assembly.GetCallingAssembly().FullName)
                throw new ChatInterfacePermissionException(String.Format("You cannot unregister a ChatInterface you didn't register yourself."));
            this.interfaces.Remove(name);
            this.registrars.Remove(name);
            return true;
        }
    }
}