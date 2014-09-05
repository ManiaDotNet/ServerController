using SilverConfig;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ManiaNet.DedicatedServer.Controller.Plugins.ManialinkDisplayManager
{
    /// <summary>
    /// Represents the configuration for the default Manialink Display Manager.
    /// </summary>
    [SilverConfig]
    public class ManialinkDisplayManagerConfig
    {
        private int manialinkRefreshInterval;

        /// <summary>
        /// Gets whether clients are allowed to hide the display of Manialinks or not.
        /// </summary>
        [SilverConfigElement(Index = 0,
            Comment = @"Enter whether clients are allowed to hide the display of plugins, or not.
Possible values: True, False")]
        public bool AllowManialinkHiding { get; private set; }

        /// <summary>
        /// Gets the minimum number of milliseconds to wait before refreshing the Manialink that is displayed for clients.
        /// </summary>
        [SilverConfigElement(Index = 1, NewLineBefore = true,
            Comment = "Enter the minimum number of milliseconds to wait before refreshing the Manialink that is displayed for clients.")]
        public int ManialinkRefreshInterval
        {
            get { return manialinkRefreshInterval; }
            private set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", "Value has to be greater than, or equal to 0.");

                manialinkRefreshInterval = value;
            }
        }

        public ManialinkDisplayManagerConfig()
        {
            AllowManialinkHiding = true;
            ManialinkRefreshInterval = 3000;
        }
    }
}