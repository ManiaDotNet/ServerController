using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ManiaNet.DedicatedServer.XmlRpc.Types.Structs
{
    /// <summary>
    /// Represents the struct returned by the GetLobbyInfo method call.
    /// </summary>
    public sealed class LobbyInfoStruct : BaseStruct<LobbyInfoStruct>
    {
        /// <summary>
        /// Backing field for the IsLobby property.
        /// </summary>
        private XmlRpcBoolean isLobby = new XmlRpcBoolean();

        /// <summary>
        /// Backing field for the LobbyMaxPlayers property.
        /// </summary>
        private XmlRpcI4 lobbyMaxPlayers = new XmlRpcI4();

        /// <summary>
        /// Backing field for the LobbyPlayers property.
        /// </summary>
        private XmlRpcI4 lobbyPlayers = new XmlRpcI4();

        /// <summary>
        /// Backing field for the LobbyPlayersLevel property.
        /// </summary>
        private XmlRpcDouble lobbyPlayersLevel = new XmlRpcDouble();

        /// <summary>
        /// Gets whether this server is a lobby or not.
        /// </summary>
        public bool IsLobby
        {
            get { return isLobby.Value; }
        }

        /// <summary>
        /// Gets the maximum number of players in the lobby.
        /// </summary>
        public int LobbyMaxPlayers
        {
            get { return lobbyMaxPlayers.Value; }
        }

        /// <summary>
        /// Gets the current number of players in the lobby.
        /// </summary>
        public int LobbyPlayers
        {
            get { return lobbyPlayers.Value; }
        }

        /// <summary>
        /// Gets the level of the players in the lobby.
        /// </summary>
        public double LobbyPlayersLevel
        {
            get { return lobbyPlayersLevel.Value; }
        }

        /// <summary>
        /// Generates an XElement storing the information in this struct.
        /// </summary>
        /// <returns>The generated XElement.</returns>
        public override XElement GenerateXml()
        {
            return new XElement(XName.Get(ElementName),
                makeMemberElement("IsLobby", isLobby.GenerateXml()),
                makeMemberElement("LobbyPlayers", lobbyPlayers.GenerateXml()),
                makeMemberElement("LobbyMaxPlayers", lobbyMaxPlayers.GenerateXml()),
                makeMemberElement("LobbyPlayersLevel", lobbyPlayersLevel.GenerateXml()));
        }

        /// <summary>
        /// Fills the properties of this struct with the information contained in the element.
        /// </summary>
        /// <param name="xElement">The struct element storing the information.</param>
        /// <returns>Itself, for convenience.</returns>
        public override LobbyInfoStruct ParseXml(XElement xElement)
        {
            checkName(xElement);

            foreach (XElement member in xElement.Descendants(XName.Get(MemberElement)))
            {
                checkIsValidMemberElement(member);

                XElement value = getMemberValueElement(member);

                switch (getMemberName(member))
                {
                    case "IsLobby":
                        isLobby.ParseXml(value);
                        break;

                    case "LobbyPlayers":
                        lobbyPlayers.ParseXml(value);
                        break;

                    case "LobbyMaxPlayers":
                        lobbyMaxPlayers.ParseXml(value);
                        break;

                    case "LobbyPlayersLevel":
                        lobbyPlayersLevel.ParseXml(value);
                        break;

                    default:
                        throw new FormatException("Unexpected member with name " + getMemberName(member));
                }
            }

            return this;
        }
    }
}