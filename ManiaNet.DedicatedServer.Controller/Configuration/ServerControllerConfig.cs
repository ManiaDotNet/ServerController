using SilverConfig;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ManiaNet.DedicatedServer.Controller.Configuration
{
    /// <summary>
    /// Represents the configuration for the server Controller.
    /// </summary>
    [SilverConfig]
    public sealed class ServerControllerConfig
    {
        /// <summary>
        /// Gets the path to the database file that the Controller will use.
        /// </summary>
        [SilverConfigElement(Index = 2, NewLineBefore = true,
            Comment = "Enter the path to the database file that the Controller will use.")]
        public string DatabasePath { get; private set; }

        /// <summary>
        /// Gets the Login that the Controller will use to authenticate with the xml rpc server.
        /// </summary>
        [SilverConfigElement(Index = 0,
            Comment = "Enter the login/password that the Controller will use to authenticate with the Dedicated Server.")]
        public string DedicatedLogin { get; private set; }

        /// <summary>
        /// Gets the Password that the Controller will use to authenticate with the xml rpc server.
        /// </summary>
        [SilverConfigElement(Index = 1)]
        public string DedicatedPassword { get; private set; }

        /// <summary>
        /// Gets the path(s) to the folders used to load plugins from.
        /// </summary>
        [SilverConfigArrayElement(Index = 5, ArrayItemName = "Path", NewLineBefore = true,
            Comment = "Enter the paths to the folders in which the Controller will look for plugins.")]
        public string[] PluginFolders { get; private set; }

        /// <summary>
        /// Gets the Login that the Controller will use to get information from the ManiaPlanet WebServices.
        /// </summary>
        [SilverConfigElement(Index = 3, NewLineBefore = true,
            Comment = "Enter the login/password that the Controller will use to get information from the ManiaPlanet WebServices.")]
        public string WebServicesLogin { get; private set; }

        /// <summary>
        /// Gets the Password that the Controller will use to get information from the ManiaPlanet WebServices.
        /// </summary>
        [SilverConfigElement(Index = 4)]
        public string WebServicesPassword { get; private set; }

        /// <summary>
        /// Gets the default ChatInterface.
        /// </summary>
        [SilverConfigElement(NewLineBefore = true,
            Comment = "Enter your default chat interface. Set it to 'chat' for the default chat.")]
        public string DefaultChat { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="ServerControllerConfig"/> with the default values.
        /// </summary>
        public ServerControllerConfig()
        {
            DedicatedLogin = "SuperAdmin";
            DedicatedPassword = "SuperAdmin";
            WebServicesLogin = "|ManiaNet";
            WebServicesPassword = "ManiaNet";
            DatabasePath = "ManiaNet.db3";
            PluginFolders = new[] { "plugins" };
            DefaultChat = "chat";
        }
    }
}