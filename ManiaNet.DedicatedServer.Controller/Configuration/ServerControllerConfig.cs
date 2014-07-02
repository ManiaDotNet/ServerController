using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace ManiaNet.DedicatedServer.Controller.Configuration
{
    /// <summary>
    /// Represents a configuration for the server controller.
    /// </summary>
    public sealed class ServerControllerConfig : Config
    {
        private const string AssemblyResourceName = "ManiaNet.DedicatedServer.Controller.Configuration.ServerControllerConfig.xml";
        private const string ConfigFileName = "ServerControllerConfig.xml";

        /// <summary>
        /// Gets whether clients are allowed to disable the display of manialinks from certain plugins.
        /// </summary>
        public bool AllowManialinkHiding { get; private set; }

        /// <summary>
        /// Gets the Login that the controller will use to authenticate with the xml rpc server.
        /// </summary>
        public string Login { get; private set; }

        /// <summary>
        /// Gets the minimum number of milliseconds to wait before refreshing the Manialink that is displayed for clients.
        /// </summary>
        public ushort ManialinkRefreshInterval { get; private set; }

        /// <summary>
        /// Gets the Password that the controller will use to authenticate with the xml rpc server.
        /// </summary>
        public string Password { get; private set; }

        /// <summary>
        /// Gets the path(s) to the folders used to load plugins from.
        /// </summary>
        public string[] PluginFolders { get; private set; }

        private ServerControllerConfig()
        { }

        /// <summary>
        /// For building an instance of the <see cref="ManiaNet.DedicatedServer.Controller.Configuration.ServerControllerConfig"/> class.
        /// </summary>
        [XmlRoot("ServerControllerConfig")]
        public sealed class Builder : BuilderBase<ServerControllerConfig, Builder>
        {
            /// <summary>
            /// The internal state of the builder.
            /// </summary>
            private ServerControllerConfig internalConfig = new ServerControllerConfig();

            /// <summary>
            /// Gets or sets whether clients are allowed to disable the display of manialinks from certain plugins.
            /// </summary>
            public bool AllowManialinkHiding
            {
                get { return config.AllowManialinkHiding; }
                set { config.AllowManialinkHiding = value; }
            }

            /// <summary>
            /// Gets or sets the Login that the controller will use to authenticate with the xml rpc server.
            /// </summary>
            public string Login
            {
                get { return config.Login; }
                set { config.Login = value; }
            }

            /// <summary>
            /// Gets or sets the minimum number of milliseconds to wait before refreshing the Manialink that is displayed for clients.
            /// </summary>
            public ushort ManialinkRefreshInterval
            {
                get { return config.ManialinkRefreshInterval; }
                set { config.ManialinkRefreshInterval = value; }
            }

            /// <summary>
            /// Gets or sets the Password that the controller will use to authenticate with the xml rpc server.
            /// </summary>
            public string Password
            {
                get { return config.Password; }
                set { config.Password = value; }
            }

            /// <summary>
            /// Gets or sets the path(s) to the folders used to load plugins from.
            /// </summary>
            [XmlArrayItem("PluginFolder")]
            public List<string> PluginFolders
            {
                get { return config.PluginFolders.ToList(); }
                set { config.PluginFolders = value.ToArray(); }
            }

            /// <summary>
            /// The internal state of the builder.
            /// </summary>
            protected override ServerControllerConfig config
            {
                get { return internalConfig; }
            }

            /// <summary>
            /// Creates a new instance of the <see cref="ManiaNet.DedicatedServer.Controller.Configuration.ServerControllerConfig"/> class with the given configuration values.
            /// </summary>
            /// <param name="allowManialinkHiding">Whether clients are allowed to disable the display of manialinks from certain plugins.</param>
            /// <param name="manialinkRefreshInterval">The number of milliseconds to wait before refreshing the Manialink that is displayed for clients.</param>
            /// <param name="login">The Login that the controller authenticates with; SuperAdmin by default.</param>
            /// <param name="password">The Password that the controller authenticates with; SuperAdmin by default.</param>
            /// <param name="pluginFolders">The path(s) to the folders used to load plugins from; { "plugins" } by default.</param>
            public Builder(bool allowManialinkHiding = true, ushort manialinkRefreshInterval = 1000, string login = "SuperAdmin", string password = "SuperAdmin", IEnumerable<string> pluginFolders = null)
            {
                AllowManialinkHiding = allowManialinkHiding;
                ManialinkRefreshInterval = manialinkRefreshInterval;
                Login = login;
                Password = password;
                PluginFolders = pluginFolders != null ? pluginFolders.ToList() : new List<string> { "plugins" };
            }

            /// <summary>
            /// Creates a new instance of the <see cref="ManiaNet.DedicatedServer.Controller.Configuration.ServerControllerConfig"/> class with default content.
            /// </summary>
            public Builder()
                : this(allowManialinkHiding: true)
            { }

            /// <summary>
            /// Loads the content of a builder instance from the config on disk, or, if that fails, loads it from the Assembly resources and also saves it to disk.
            /// </summary>
            /// <returns>The builder instance containing the loaded content.</returns>
            public static Builder Load()
            {
                return Builder.loadConfig(AssemblyResourceName, ConfigFileName);
            }

            /// <summary>
            /// Saves the current internal state of the builder to disk.
            /// </summary>
            /// <returns>Whether it was successful or not.</returns>
            public bool Save()
            {
                return saveConfig(ConfigFileName);
            }
        }
    }
}