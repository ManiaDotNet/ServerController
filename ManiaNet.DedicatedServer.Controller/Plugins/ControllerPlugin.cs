using SharpPlugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ManiaNet.DedicatedServer.Controller.Plugins
{
    /// <summary>
    /// Abstract base class for all ControllerPlugins.
    /// </summary>
    public abstract partial class ControllerPlugin : PluginBase
    {
        /// <summary>
        /// Dictionary containing the manialink elements that the plugin wants to display.
        /// <para/>
        /// Key is the login of the client that the elements are for, Value is a string containing the elements.
        /// <para/>
        /// * means any client. This has a lower precedence than the actual login.
        /// </summary>
        protected Dictionary<string, string> clientManialinks = new Dictionary<string, string>();

        /// <summary>
        /// Gets a readonly dictionary containing the manialink elements that the plugin wants to display.
        /// <para/>
        /// Key is the login of the client that the elements are for, Value is a string containing the elements.
        /// <para/>
        /// * means any client. This has a lower precedence than the actual login.
        /// </summary>
        public ReadOnlyDictionary<string, string> ClientManialinks
        {
            get { return new ReadOnlyDictionary<string, string>(clientManialinks); }
        }

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