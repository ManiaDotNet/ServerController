using ManiaNet.DedicatedServer.Controller.Annotations;
using ManiaNet.DedicatedServer.Controller.Plugins.Extensibility.Clients;
using ManiaNet.ManiaPlanet.WebServices;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace ManiaNet.DedicatedServer.Controller.Plugins
{
    /// <summary>
    /// Stores information about a Client.
    /// </summary>
    [UsedImplicitly]
    public sealed class Client : IClient
    {
        /// <summary>
        /// Gets the DateTime at which the information was last fetched from the Web Services.
        /// </summary>
        [UsedImplicitly]
        public DateTime Fetched { get; internal set; }

        /// <summary>
        /// Gets the ManiaPlanet Id of the Client.
        /// </summary>
        [UsedImplicitly]
        public uint Id { get; internal set; }

        /// <summary>
        /// Gets the Login of the Client.
        /// </summary>
        [UsedImplicitly]
        public string Login { get; internal set; }

        /// <summary>
        /// Gets the Nickname, including the $-formats, of the Client.
        /// </summary>
        [UsedImplicitly]
        public string Nickname { get; internal set; }

        /// <summary>
        /// Gets the Zone-Id of the Client.
        /// </summary>
        [UsedImplicitly]
        public uint ZoneId { get; internal set; }

        /// <summary>
        /// Gets the Zone-Id of the Client.
        /// </summary>
        [UsedImplicitly]
        public string ZonePath { get; internal set; }

        /// <summary>
        /// Creates a new instance of the <see cref="Client"/> class with the given <see cref="PlayerInfo"/> and the given fetch time.
        /// </summary>
        /// <param name="playerInfo">The <see cref="PlayerInfo"/> which's values will be used.</param>
        /// <param name="fetched">The time when the info was fetched.</param>
        internal Client([NotNull] PlayerInfo playerInfo, DateTime fetched)
        {
            if (!playerInfo.Id.HasValue || playerInfo.Login == null || playerInfo.Nickname == null || !playerInfo.ZoneId.HasValue || playerInfo.ZonePath == null)
                throw new ArgumentNullException("playerInfo", "All fields of the PlayerInfo object have to have values.");

            Fetched = fetched;
            Id = playerInfo.Id.Value;
            Login = playerInfo.Login;
            Nickname = playerInfo.Nickname;
            ZoneId = playerInfo.ZoneId.Value;
            ZonePath = playerInfo.ZonePath;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Client"/> class with the given NameValueCollection from a database reader.
        /// </summary>
        /// <param name="nameValueCollection">The information from the database.</param>
        internal Client(NameValueCollection nameValueCollection)
        {
            Fetched = long.Parse(nameValueCollection["Fetched"]).FromUnixTimeStampToDateTime();
            Id = uint.Parse(nameValueCollection["Id"]);
            Login = nameValueCollection["Login"];
            Nickname = nameValueCollection["Nickname"];
            ZoneId = uint.Parse(nameValueCollection["ZoneId"]);
            ZonePath = nameValueCollection["ZonePath"];
        }
    }
}