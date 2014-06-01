using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ManiaNet.DedicatedServer.XmlRpc.Types.Structs
{
    /// <summary>
    /// Represents the struct returned by the GetLadderServerLimits method call.
    /// </summary>
    public sealed class LadderServerLimitStruct : BaseStruct<LadderServerLimitStruct>
    {
        /// <summary>
        /// Backing field for the LadderServerLimitMax property.
        /// </summary>
        private XmlRpcDouble ladderServerLimitMax = new XmlRpcDouble();

        /// <summary>
        /// Backing field for the LadderServerLimitMin property.
        /// </summary>
        private XmlRpcDouble ladderServerLimitMin = new XmlRpcDouble();

        /// <summary>
        /// Gets the maximum number of ladder points a player can reach on the server.
        /// </summary>
        public double LadderServerLimitMax
        {
            get { return ladderServerLimitMax.Value; }
        }

        /// <summary>
        /// Gets the minimumm number of ladder points a player has to have to join the server.
        /// </summary>
        public double LadderServerLimitMin
        {
            get { return ladderServerLimitMin.Value; }
        }

        /// <summary>
        /// Generates an XElement storing the information in this struct.
        /// </summary>
        /// <returns>The generated XElement.</returns>
        public override XElement GenerateXml()
        {
            return new XElement(XName.Get(ElementName),
                makeMemberElement("LadderServerLimitMin", ladderServerLimitMin.GenerateXml()),
                makeMemberElement("LadderServerLimitMax", ladderServerLimitMax.GenerateXml()));
        }

        /// <summary>
        /// Fills the properties of this struct with the information contained in the element.
        /// </summary>
        /// <param name="xElement">The struct element storing the information.</param>
        /// <returns>Itself, for convenience.</returns>
        public override LadderServerLimitStruct ParseXml(XElement xElement)
        {
            checkName(xElement);

            foreach (XElement member in xElement.Descendants(XName.Get(MemberElement)))
            {
                checkIsValidMemberElement(member);

                XElement value = getMemberValueElement(member);

                switch (getMemberName(member))
                {
                    case "LadderServerLimitMin":
                        ladderServerLimitMin.ParseXml(value);
                        break;

                    case "LadderServerLimitMax":
                        ladderServerLimitMax.ParseXml(value);
                        break;

                    default:
                        throw new FormatException("Unexpected member with name " + getMemberName(member));
                }
            }

            return this;
        }
    }
}