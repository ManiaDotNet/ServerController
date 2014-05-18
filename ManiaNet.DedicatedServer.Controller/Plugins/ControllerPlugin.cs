using System;
using System.Collections.Generic;
using System.Linq;

namespace ManiaNet.DedicatedServer.Controller.Plugins
{
    /// <summary>
    /// Abstract base class for all ControllerPlugins.
    /// </summary>
    public abstract partial class ControllerPlugin
    {
        /// <summary>
        /// Gets called when the plugin is loaded.
        /// Use this to add your methods to the controller's events and load your saved data.
        /// </summary>
        /// <param name="controller">The controller loading the plugin.</param>
        public abstract void Load(ServerController controller);

        /// <summary>
        /// Gets called when the plugin is unloaded.
        /// Use this to save your data.
        /// </summary>
        public abstract void Unload();
    }
}