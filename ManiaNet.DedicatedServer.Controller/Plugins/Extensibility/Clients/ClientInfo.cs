using ManiaNet.DedicatedServer.Controller.Annotations;
using ManiaNet.ManiaPlanet.WebServices;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace ManiaNet.DedicatedServer.Controller.Plugins.Extensibility.Clients
{
    /// <summary>
    /// Stores information about a Client.
    /// </summary>
    [UsedImplicitly]
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class Client
    {
        /// <summary>
        /// Gets the DateTime at which the information was last fetched from the Web Services.
        /// </summary>
        [UsedImplicitly]
        public DateTime Fetched { get; protected set; }

        /// <summary>
        /// Gets the ManiaPlanet Id of the Client.
        /// </summary>
        [UsedImplicitly]
        public uint Id { get; protected set; }

        /// <summary>
        /// Gets the Login of the Client.
        /// </summary>
        [NotNull, UsedImplicitly]
        public string Login { get; protected set; }

        /// <summary>
        /// Gets the Nickname, including the $-formats, of the Client.
        /// </summary>
        [NotNull, UsedImplicitly]
        public string Nickname { get; protected set; }

        /// <summary>
        /// Gets the Zone-Id of the Client.
        /// </summary>
        [UsedImplicitly]
        public uint ZoneId { get; protected set; }

        /// <summary>
        /// Gets the Zone-Id of the Client.
        /// </summary>
        [NotNull, UsedImplicitly]
        public string ZonePath { get; protected set; }

        /// <summary>
        /// Creates a new instance of the <see cref="Client"/> class with the given <see cref="PlayerInfo"/> and the given fetch time.
        /// </summary>
        /// <param name="playerInfo">The <see cref="PlayerInfo"/> which's values will be used.</param>
        /// <param name="fetched">The time when the info was fetched.</param>
        public Client([NotNull] PlayerInfo playerInfo, DateTime fetched)
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

        public Client(NameValueCollection nameValueCollection)
        {
            Fetched = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(ulong.Parse(nameValueCollection["Fetched"]));
            Id = uint.Parse(nameValueCollection["Id"]);
            Login = nameValueCollection["Login"];
            Nickname = nameValueCollection["Nickname"];
            ZoneId = uint.Parse(nameValueCollection["ZoneId"]);
            ZonePath = nameValueCollection["ZonePath"];
        }
    }
}