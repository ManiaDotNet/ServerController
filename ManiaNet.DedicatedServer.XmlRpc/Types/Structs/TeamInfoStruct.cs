using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ManiaNet.DedicatedServer.XmlRpc.Types.Structs
{
    /// <summary>
    /// Represents the struct returned from the GetTeamInfo method call.
    /// </summary>
    public sealed class TeamInfoStruct : BaseStruct<TeamInfoStruct>
    {
        /// <summary>
        /// Backing field for the City property.
        /// </summary>
        private XmlRpcString city = new XmlRpcString();

        /// <summary>
        /// Backing field for the ClubLinkUrl property.
        /// </summary>
        private XmlRpcString clubLinkUrl = new XmlRpcString();

        /// <summary>
        /// Backing field for the EmblemUrl property.
        /// </summary>
        private XmlRpcString emblemUrl = new XmlRpcString();

        /// <summary>
        /// Backing field for the HuePrimary property.
        /// </summary>
        private XmlRpcDouble huePrimary = new XmlRpcDouble();

        /// <summary>
        /// Backing field for the HueSecondary property.
        /// </summary>
        private XmlRpcDouble hueSecondary = new XmlRpcDouble();

        /// <summary>
        /// Backing field for the Name property.
        /// </summary>
        private XmlRpcString name = new XmlRpcString();

        /// <summary>
        /// Backing field for the Rgb property.
        /// </summary>
        private XmlRpcString rgb = new XmlRpcString();

        /// <summary>
        /// Backing field for the ZonePath property.
        /// </summary>
        private XmlRpcString zonePath = new XmlRpcString();

        /// <summary>
        /// Gets the name of the City that the team is from.
        /// </summary>
        public string City
        {
            get { return city.Value; }
        }

        /// <summary>
        /// Gets the club link url of the team.
        /// </summary>
        public string ClubLinkUrl
        {
            get { return clubLinkUrl.Value; }
        }

        /// <summary>
        /// Gets the URL of the Emblem of the team.
        /// </summary>
        public string EmblemUrl
        {
            get { return emblemUrl.Value; }
        }

        /// <summary>
        /// Gets the primary hue of the team.
        /// </summary>
        public double HuePrimary
        {
            get { return huePrimary.Value; }
        }

        /// <summary>
        /// Gets the secondary hue of the team.
        /// </summary>
        public double HueSecondary
        {
            get { return hueSecondary.Value; }
        }

        /// <summary>
        /// Gets the name of the team.
        /// </summary>
        public string Name
        {
            get { return name.Value; }
        }

        /// <summary>
        /// Gets the three letter RGB of the team's color.
        /// </summary>
        public string Rgb
        {
            get { return rgb.Value; }
        }

        /// <summary>
        /// Gets the ZonePath of the team.
        /// </summary>
        public string ZonePath
        {
            get { return zonePath.Value; }
        }

        /// <summary>
        /// Generates an XElement storing the information in this struct.
        /// </summary>
        /// <returns>The generated XElement.</returns>
        public override XElement GenerateXml()
        {
            return new XElement(XName.Get(ElementName),
                makeMemberElement("Name", name.GenerateXml()),
                makeMemberElement("ZonePath", zonePath.GenerateXml()),
                makeMemberElement("City", city.GenerateXml()),
                makeMemberElement("EmblemUrl", emblemUrl.GenerateXml()),
                makeMemberElement("ClubLinkUrl", clubLinkUrl.GenerateXml()),
                makeMemberElement("HuePrimary", huePrimary.GenerateXml()),
                makeMemberElement("HueSecondary", hueSecondary.GenerateXml()),
                makeMemberElement("RGB", rgb.GenerateXml()));
        }

        public override TeamInfoStruct ParseXml(XElement xElement)
        {
            checkName(xElement);

            foreach (XElement member in xElement.Descendants(XName.Get(MemberElement)))
            {
                checkIsValidMemberElement(member);

                XElement value = getMemberValueElement(member);

                switch (getMemberName(member))
                {
                    case "Name":
                        name.ParseXml(getNormalizedStringValueContent(value, name.ElementName));
                        break;

                    case "ZonePath":
                        zonePath.ParseXml(getNormalizedStringValueContent(value, zonePath.ElementName));
                        break;

                    case "City":
                        city.ParseXml(getNormalizedStringValueContent(value, city.ElementName));
                        break;

                    case "EmblemUrl":
                        emblemUrl.ParseXml(getNormalizedStringValueContent(value, emblemUrl.ElementName));
                        break;

                    case "ClubLinkUrl":
                        clubLinkUrl.ParseXml(getNormalizedStringValueContent(value, clubLinkUrl.ElementName));
                        break;

                    case "HuePrimary":
                        huePrimary.ParseXml(value);
                        break;

                    case "HueSecondary":
                        hueSecondary.ParseXml(value);
                        break;

                    case "RGB":
                        rgb.ParseXml(getNormalizedStringValueContent(value, rgb.ElementName));
                        break;

                    default:
                        throw new FormatException("Unexpected member with name " + getMemberName(member));
                }
            }

            return this;
        }
    }
}