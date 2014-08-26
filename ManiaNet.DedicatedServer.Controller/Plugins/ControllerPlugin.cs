using ManiaNet.DedicatedServer.Controller.Annotations;
using SharpPlugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ManiaNet.DedicatedServer.Controller.Plugins
{
    /// <summary>
    /// Abstract base class for all ControllerPlugins.
    /// </summary>
    public abstract class ControllerPlugin : PluginBase
    {
        private static readonly Assembly serverControllerAssembly = Assembly.GetExecutingAssembly();

        /// <summary>
        /// Gets whether the plugin requires its Run method to be called.
        /// </summary>
        public abstract bool RequiresRun { get; }

        /// <summary>
        /// Gets called when the plugin is loaded.
        /// Use this to add your methods to the controller's events and load your saved data.
        /// </summary>
        /// <param name="controller">The controller loading the plugin.</param>
        /// <returns>Whether it loaded successfully or not.</returns>
        public abstract bool Load([NotNull] ServerController controller);

        /// <summary>
        /// The main method of the plugin.
        /// Gets run in its own thread by the controller and should stop gracefully on a <see cref="System.Threading.ThreadAbortException"/>.
        /// </summary>
        public abstract void Run();

        /// <summary>
        /// Gets called when the plugin is unloaded.
        /// Use this to save your data.
        /// </summary>
        /// <returns>Whether it unloaded successfully or not.</returns>
        public abstract bool Unload();

        /// <summary>
        /// Takes an assembly and returns whether it's the Server Controller assembly or not.
        /// <para/>
        /// Allows the plugin to make sure that only the Server Controller can call certain methods.
        /// <para/>
        /// Usage: isAssemblyServerController(Assembly.GetCallingAssembly());
        /// </summary>
        /// <example>isAssemblyServerController(Assembly.GetCallingAssembly());</example>
        /// <param name="assembly">The assembly to check.</param>
        /// <returns>Whether the given assembly is the Server Controller assembly or not.</returns>
        protected static bool isAssemblyServerController([NotNull] Assembly assembly)
        {
            return serverControllerAssembly.Equals(assembly);
        }
    }
}