using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ManiaNet.DedicatedServer.Controller.Configuration
{
    /// <summary>
    /// Represents a configuration for the server controller.
    /// </summary>
    public sealed class ServerControllerConfig : Config
    {
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
        public sealed class Builder : BuilderBase<ServerControllerConfig, Builder>
        {
            /// <summary>
            /// The internal state of the builder.
            /// </summary>
            private readonly ServerControllerConfig internalConfig = new ServerControllerConfig();

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
            /// Gets the name of the config file.
            /// </summary>
            protected override string configFileName
            {
                get { return "ServerControllerConfig.xml"; }
            }

            /// <summary>
            /// Gets the identifier for the dll resource containing the default config.
            /// </summary>
            protected override string configResource
            {
                get { return "ManiaNet.DedicatedServer.Controller.Configuration.ServerControllerConfig.xml"; }
            }

            /// <summary>
            /// Creates a new instance of the <see cref="ManiaNet.DedicatedServer.Controller.Configuration.ServerControllerConfig"/> class with the content from disk or default.
            /// </summary>
            public Builder()
            {
                Load();
            }

            protected override XElement generateXml()
            {
                return new XElement("serverControllerConfig",
                    new XElement("allowManialinkHiding", AllowManialinkHiding),
                    new XElement("manialinkRefreshInterval", ManialinkRefreshInterval),
                    new XElement("login", Login),
                    new XElement("password", Password),
                    new XElement("pluginFolders", PluginFolders.Select(pluginFolder => new XElement("pluginFolder", pluginFolder))));
            }

            protected override bool parseXml(XElement xElement)
            {
                if (!xElement.Name.LocalName.Equals("serverControllerConfig")
                    || !xElement.HasElements || xElement.Elements().Count() != 5)
                    return false;

                XElement allowManialinkHidingElement = xElement.Element("allowManialinkHiding");
                XElement manialinkRefreshIntervalElement = xElement.Element("manialinkRefreshInterval");
                XElement loginElement = xElement.Element("login");
                XElement passwordElement = xElement.Element("password");
                XElement pluginFoldersElement = xElement.Element("pluginFolders");

                if (allowManialinkHidingElement == null || manialinkRefreshIntervalElement == null
                    || loginElement == null || passwordElement == null || pluginFoldersElement == null
                    || !pluginFoldersElement.HasElements || pluginFoldersElement.Elements().Any(element => !element.Name.LocalName.Equals("pluginFolder")))
                    return false;

                bool allowManialinkHiding;
                ushort manialinkRefreshInterval;

                if (!bool.TryParse(allowManialinkHidingElement.Value, out allowManialinkHiding)
                    || !ushort.TryParse(manialinkRefreshIntervalElement.Value, out manialinkRefreshInterval))
                    return false;

                AllowManialinkHiding = allowManialinkHiding;
                ManialinkRefreshInterval = manialinkRefreshInterval;
                Login = loginElement.Value;
                Password = passwordElement.Value;
                PluginFolders = pluginFoldersElement.Elements().Select(element => element.Value).Where(value => !string.IsNullOrWhiteSpace(value)).ToList();

                return true;
            }
        }
    }
}