using System;
using System.Collections.Generic;
using System.Linq;

namespace ManiaNet.DedicatedServer.Controller.Plugins
{
    public abstract partial class ControllerPlugin
    {
        /// <summary>
        /// Takes a <see cref="ManiaNet.ServerController.Plugins.ControllerPlugin"/> deriving type and returns whether it's to be loaded as plugin.
        /// </summary>
        /// <param name="pluginType">The <see cref="ManiaNet.ServerController.Plugins.ControllerPlugin"/> derived type to check.</param>
        /// <returns>Whether the type is to be loaded as plugin.</returns>
        public static bool IsRegisteredPlugin(Type pluginType)
        {
            if (!typeof(ControllerPlugin).IsAssignableFrom(pluginType))
                throw new ArgumentException("Type has to be a derivative of ManiaNet.ServerController.Plugins.ControllerPlugin", "pluginType");

            return pluginType.GetCustomAttributes(typeof(RegisterControllerPluginAttribute), false).Length > 0;
        }

        /// <summary>
        /// Marks a <see cref="ManiaNet.ServerController.Plugins.ControllerPlugin"/> derivative as to be loaded as plugin.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
        protected class RegisterControllerPluginAttribute : Attribute
        {
            /// <summary>
            /// Gets or sets the Author of the plugin.
            /// </summary>
            public string Author { get; set; }

            /// <summary>
            /// Gets the name of the plugin.
            /// </summary>
            public string Name { get; private set; }

            /// <summary>
            /// Gets the version of the plugin.
            /// </summary>
            public string Version { get; set; }

            /// <summary>
            /// Creates a new instance of the <see cref="ManiaNet.DedicatedServer.Controller.Plugins.ControllerPlugin.RegisterControllerPluginAttribute"/>
            /// class with the given plugin name.
            /// </summary>
            /// <param name="name">The name of the plugin.</param>
            public RegisterControllerPluginAttribute(string name)
            {
                Name = name;
            }
        }
    }
}