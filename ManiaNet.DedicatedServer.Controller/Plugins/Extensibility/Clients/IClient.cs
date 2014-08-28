using System;
using ManiaNet.DedicatedServer.Controller.Annotations;

namespace ManiaNet.DedicatedServer.Controller.Plugins.Extensibility.Clients
{
    public interface IClient
    {
        /// <summary>
        /// Gets the DateTime at which the information was last fetched from the Web Services.
        /// </summary>
        [UsedImplicitly]
        DateTime Fetched { get; }

        /// <summary>
        /// Gets the ManiaPlanet Id of the Client.
        /// </summary>
        [UsedImplicitly]
        uint Id { get; }

        /// <summary>
        /// Gets the Login of the Client.
        /// </summary>
        [NotNull, UsedImplicitly]
        string Login { get; }

        /// <summary>
        /// Gets the Nickname, including the $-formats, of the Client.
        /// </summary>
        [NotNull, UsedImplicitly]
        string Nickname { get; }

        /// <summary>
        /// Gets the Zone-Id of the Client.
        /// </summary>
        [UsedImplicitly]
        uint ZoneId { get; }

        /// <summary>
        /// Gets the Zone-Id of the Client.
        /// </summary>
        [NotNull, UsedImplicitly]
        string ZonePath { get; }
    }
}