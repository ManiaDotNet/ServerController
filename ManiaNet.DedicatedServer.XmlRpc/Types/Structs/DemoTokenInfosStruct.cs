using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ManiaNet.DedicatedServer.XmlRpc.Types.Structs
{
    /// <summary>
    /// Represents the struct returned by the GetDemoTokenInfosForPlayer method call.
    /// </summary>
    public sealed class DemoTokenInfosStruct : BaseStruct<DemoTokenInfosStruct>
    {
        /// <summary>
        /// Backing field for the CanPayToken property.
        /// </summary>
        private XmlRpcBoolean canPayToken = new XmlRpcBoolean();

        /// <summary>
        /// Backing field for the TokenCost property.
        /// </summary>
        private XmlRpcI4 tokenCost = new XmlRpcI4();

        /// <summary>
        /// Gets whether the player can pay for the token or not.
        /// </summary>
        public bool CanPayToken
        {
            get { return canPayToken.Value; }
        }

        /// <summary>
        /// Gets the token cost.
        /// </summary>
        public int TokenCost
        {
            get { return tokenCost.Value; }
        }

        /// <summary>
        /// Generates an XElement storing the information in this struct.
        /// </summary>
        /// <returns>The generated XElement.</returns>
        public override XElement GenerateXml()
        {
            return new XElement(XName.Get(ElementName),
                makeMemberElement("TokenCost", tokenCost.GenerateXml()),
                makeMemberElement("CanPayToken", canPayToken.GenerateXml()));
        }

        /// <summary>
        /// Fills the properties of this struct with the information contained in the element.
        /// </summary>
        /// <param name="xElement">The struct element storing the information.</param>
        /// <returns>Itself, for convenience.</returns>
        public override DemoTokenInfosStruct ParseXml(XElement xElement)
        {
            checkName(xElement);

            foreach (XElement member in xElement.Descendants(XName.Get(MemberElement)))
            {
                checkIsValidMemberElement(member);

                XElement value = getMemberValueElement(member);

                switch (getMemberName(member))
                {
                    case "TokenCost":
                        tokenCost.ParseXml(value);
                        break;

                    case "CanPayToken":
                        canPayToken.ParseXml(value);
                        break;

                    default:
                        throw new FormatException("Unexpected member with name " + getMemberName(member));
                }
            }

            return this;
        }
    }
}