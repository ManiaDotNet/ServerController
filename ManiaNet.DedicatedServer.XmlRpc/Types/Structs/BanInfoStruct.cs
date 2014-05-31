using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ManiaNet.DedicatedServer.XmlRpc.Types.Structs
{
    /// <summary>
    /// Represents the structs returned by the GetBanList method call.
    /// </summary>
    public sealed class BanInfoStruct : BaseStruct<BanInfoStruct>
    {
        /// <summary>
        /// Backing field for the ClientName property.
        /// </summary>
        private XmlRpcString clientName = new XmlRpcString();

        /// <summary>
        /// Backing field for the IPAddress property.
        /// </summary>
        private XmlRpcString ipAddress = new XmlRpcString();

        /// <summary>
        /// Backing field for the Login property.
        /// </summary>
        private XmlRpcString login = new XmlRpcString();

        /// <summary>
        /// Gets the ClientName of the banned player.
        /// </summary>
        public string ClientName
        {
            get { return clientName.Value; }
        }

        /// <summary>
        /// Gets the IP-address of the banned player.
        /// </summary>
        public string IPAddress
        {
            get { return ipAddress.Value; }
        }

        /// <summary>
        /// Gets the Login of the banned player.
        /// </summary>
        public string Login
        {
            get { return login.Value; }
        }

        /// <summary>
        /// Generates an XElement storing the information in this struct.
        /// </summary>
        /// <returns>The generated XElement.</returns>
        public override XElement GenerateXml()
        {
            return new XElement(XName.Get(ElementName),
                makeMemberElement("Login", login.GenerateXml()),
                makeMemberElement("ClientName", clientName.GenerateXml()),
                makeMemberElement("IPAddress", ipAddress.GenerateXml()));
        }

        /// <summary>
        /// Fills the properties of this struct with the information contained in the element.
        /// </summary>
        /// <param name="xElement">The struct element storing the information.</param>
        /// <returns>Itself, for convenience.</returns>
        public override BanInfoStruct ParseXml(XElement xElement)
        {
            checkName(xElement);

            foreach (XElement member in xElement.Descendants(XName.Get(MemberElement)))
            {
                checkIsValidMemberElement(member);

                XElement value = getMemberValueElement(member);

                switch (getMemberName(member))
                {
                    case "Login":
                        login.ParseXml(getNormalizedStringValueContent(value, login.ElementName));
                        break;

                    case "ClientName":
                        clientName.ParseXml(getNormalizedStringValueContent(value, clientName.ElementName));
                        break;

                    case "IPAddress":
                        ipAddress.ParseXml(getNormalizedStringValueContent(value, ipAddress.ElementName));
                        break;

                    default:
                        throw new FormatException("Unexpected member with name " + getMemberName(member));
                }
            }

            return this;
        }
    }
}