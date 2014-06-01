using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ManiaNet.DedicatedServer.XmlRpc.Types.Structs
{
    /// <summary>
    /// Represents the struct returned by the GetForcedClubLinks method call.
    /// </summary>
    public sealed class ForcedClubLinksStruct : BaseStruct<ForcedClubLinksStruct>
    {
        /// <summary>
        /// Backing field for the ClubLink1 property.
        /// </summary>
        private XmlRpcString clubLink1 = new XmlRpcString();

        /// <summary>
        /// Backing field for the ClubLink2 property.
        /// </summary>
        private XmlRpcString clubLink2 = new XmlRpcString();

        /// <summary>
        /// Gets the forced club link for team 1.
        /// </summary>
        public string ClubLink1
        {
            get { return clubLink1.Value; }
        }

        /// <summary>
        /// Gets the forced club link for team 2.
        /// </summary>
        public string ClubLink2
        {
            get { return clubLink2.Value; }
        }

        /// <summary>
        /// Generates an XElement storing the information in this struct.
        /// </summary>
        /// <returns>The generated XElement.</returns>
        public override XElement GenerateXml()
        {
            return new XElement(XName.Get(ElementName),
                makeMemberElement("ClubLink1", clubLink1.GenerateXml()),
                makeMemberElement("ClubLink2", clubLink2.GenerateXml()));
        }

        /// <summary>
        /// Fills the properties of this struct with the information contained in the element.
        /// </summary>
        /// <param name="xElement">The struct element storing the information.</param>
        /// <returns>Itself, for convenience.</returns>
        public override ForcedClubLinksStruct ParseXml(XElement xElement)
        {
            checkName(xElement);

            foreach (XElement member in xElement.Descendants(XName.Get(MemberElement)))
            {
                checkIsValidMemberElement(member);

                XElement value = getMemberValueElement(member);

                switch (getMemberName(member))
                {
                    case "ClubLink1":
                        clubLink1.ParseXml(getNormalizedStringValueContent(value, clubLink1.ElementName));
                        break;

                    case "ClubLink2":
                        clubLink2.ParseXml(getNormalizedStringValueContent(value, clubLink2.ElementName));
                        break;

                    default:
                        throw new FormatException("Unexpected member with name " + getMemberName(member));
                }
            }

            return this;
        }
    }
}