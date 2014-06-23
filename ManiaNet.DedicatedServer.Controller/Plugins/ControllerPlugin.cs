using SharpPlugins;
using System;
using System.Collections.Generic;
using System.Linq;
using XmlRpc;

namespace ManiaNet.DedicatedServer.Controller.Plugins
{
    /// <summary>
    /// Abstract base class for all ControllerPlugins.
    /// </summary>
    public abstract partial class ControllerPlugin : PluginBase
    {
        /// <summary>
        /// Gets called when the plugin is loaded.
        /// Use this to add your methods to the controller's events and load your saved data.
        /// </summary>
        /// <param name="controller">The controller loading the plugin.</param>
        /// <returns>Whether it loaded successfully or not.</returns>
        public abstract bool Load(ServerController controller);

        /// <summary>
        /// The main method of the plugin.
        /// Gets run in its own thread by the controller and should stop gracefully on a <see cref="System.Thread.ThreadAbortExceptions"/>.
        /// </summary>
        public abstract void Run();

        /// <summary>
        /// Gets called when the plugin is unloaded.
        /// Use this to save your data.
        /// </summary>
        /// <returns>Whether it unloaded successfully or not.</returns>
        public abstract bool Unload();
    }
}