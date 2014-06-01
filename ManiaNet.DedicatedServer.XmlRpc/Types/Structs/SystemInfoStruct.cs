using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ManiaNet.DedicatedServer.XmlRpc.Types.Structs
{
    /// <summary>
    /// Represents the struct returned by the GetSystemInfo method call.
    /// </summary>
    public sealed class SystemInfoStruct : BaseStruct<SystemInfoStruct>
    {
        /// <summary>
        /// Backing field for the ConnectionDownloadRate property.
        /// </summary>
        private XmlRpcI4 connectionDownloadRate = new XmlRpcI4();

        /// <summary>
        /// Backing field for the ConnectionUploadRate property.
        /// </summary>
        private XmlRpcI4 connectionUploadRate = new XmlRpcI4();

        /// <summary>
        /// Backing field for the IsDedicated property.
        /// </summary>
        private XmlRpcBoolean isDedicated = new XmlRpcBoolean();

        /// <summary>
        /// Backing field for the IsServer property.
        /// </summary>
        private XmlRpcBoolean isServer = new XmlRpcBoolean();

        /// <summary>
        /// Backing field for the P2PPort property.
        /// </summary>
        private XmlRpcI4 p2pPort = new XmlRpcI4();

        /// <summary>
        /// Backing field for the Port property.
        /// </summary>
        private XmlRpcI4 port = new XmlRpcI4();

        /// <summary>
        /// Backing field for the PublishedIp property.
        /// </summary>
        private XmlRpcString publishedIp = new XmlRpcString();

        /// <summary>
        /// Backing field for the ServerLogin property.
        /// </summary>
        private XmlRpcString serverLogin = new XmlRpcString();

        /// <summary>
        /// Backing field for the ServerPlayerId property.
        /// </summary>
        private XmlRpcI4 serverPlayerId = new XmlRpcI4();

        /// <summary>
        /// Backing field for the TitleId property.
        /// </summary>
        private XmlRpcString titleId = new XmlRpcString();

        /// <summary>
        /// Gets the maximum download rate of the server. In kBits per second.
        /// </summary>
        public int ConnectionDownloadRate
        {
            get { return connectionDownloadRate.Value; }
        }

        /// <summary>
        /// Gets the maximum upload rate of the server. In kbits per second.
        /// </summary>
        public int ConnectionUploadRate
        {
            get { return connectionUploadRate.Value; }
        }

        /// <summary>
        /// Gets whether the server is dedicated or not.
        /// </summary>
        public bool IsDedicated
        {
            get { return isDedicated.Value; }
        }

        /// <summary>
        /// Gets whether the server is a server or not.
        /// </summary>
        public bool IsServer
        {
            get { return isServer.Value; }
        }

        /// <summary>
        /// Gets the port number that the server uses for p2p connections.
        /// </summary>
        public int P2PPort
        {
            get { return p2pPort.Value; }
        }

        /// <summary>
        /// Gets the port number that the server uses for connecting players.
        /// </summary>
        public int Port
        {
            get { return port.Value; }
        }

        /// <summary>
        /// Gets the IP-address that the server published to the master server.
        /// </summary>
        public string PublishedIp
        {
            get { return publishedIp.Value; }
        }

        /// <summary>
        /// Gets the login of the server account.
        /// </summary>
        public string ServerLogin
        {
            get { return serverLogin.Value; }
        }

        /// <summary>
        /// Gets the player Id of the server.
        /// </summary>
        public int ServerPlayerId
        {
            get { return serverPlayerId.Value; }
        }

        /// <summary>
        /// Gets the ID of the Title that's currently being played (TMCanyon, TMValley, etc.) on the server.
        /// </summary>
        public string TitleId
        {
            get { return titleId.Value; }
        }

        /// <summary>
        /// Generates an XElement storing the information in this struct.
        /// </summary>
        /// <returns>The generated XElement.</returns>
        public override XElement GenerateXml()
        {
            return new XElement(XName.Get(ElementName),
                makeMemberElement("PublishedIp", publishedIp.GenerateXml()),
                makeMemberElement("Port", port.GenerateXml()),
                makeMemberElement("P2PPort", p2pPort.GenerateXml()),
                makeMemberElement("TitleId", titleId.GenerateXml()),
                makeMemberElement("ServerLogin", serverLogin.GenerateXml()),
                makeMemberElement("ServerPlayerId", serverPlayerId.GenerateXml()),
                makeMemberElement("ConnectionDownloadRate", connectionDownloadRate.GenerateXml()),
                makeMemberElement("ConnectionUploadRate", connectionUploadRate.GenerateXml()),
                makeMemberElement("IsServer", isServer.GenerateXml()),
                makeMemberElement("IsDedicated", isDedicated.GenerateXml()));
        }

        /// <summary>
        /// Fills the properties of this struct with the information contained in the element.
        /// </summary>
        /// <param name="xElement">The struct element storing the information.</param>
        /// <returns>Itself, for convenience.</returns>
        public override SystemInfoStruct ParseXml(XElement xElement)
        {
            checkName(xElement);

            foreach (XElement member in xElement.Descendants(XName.Get(MemberElement)))
            {
                checkIsValidMemberElement(member);

                XElement value = getMemberValueElement(member);

                switch (getMemberName(member))
                {
                    case "PublishedIp":
                        publishedIp.ParseXml(getNormalizedStringValueContent(value, publishedIp.ElementName));
                        break;

                    case "Port":
                        port.ParseXml(value);
                        break;

                    case "P2PPort":
                        p2pPort.ParseXml(value);
                        break;

                    case "TitleId":
                        titleId.ParseXml(getNormalizedStringValueContent(value, titleId.ElementName));
                        break;

                    case "ServerLogin":
                        serverLogin.ParseXml(getNormalizedStringValueContent(value, serverLogin.ElementName));
                        break;

                    case "ServerPlayerId":
                        serverPlayerId.ParseXml(value);
                        break;

                    case "ConnectionDownloadRate":
                        connectionDownloadRate.ParseXml(value);
                        break;

                    case "ConnectionUploadRate":
                        connectionUploadRate.ParseXml(value);
                        break;

                    case "IsServer":
                        isServer.ParseXml(value);
                        break;

                    case "IsDedicated":
                        isDedicated.ParseXml(value);
                        break;

                    default:
                        throw new FormatException("Unexpected member with name " + getMemberName(member));
                }
            }

            return this;
        }
    }
}