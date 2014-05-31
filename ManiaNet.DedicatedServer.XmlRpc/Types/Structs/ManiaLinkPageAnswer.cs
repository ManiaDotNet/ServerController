using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ManiaNet.DedicatedServer.XmlRpc.Types.Structs
{
    /// <summary>
    /// Represents the structs returned from the GetManialinkPageAnswers method call.
    /// </summary>
    public sealed class ManialinkPageAnswer : BaseStruct<ManialinkPageAnswer>
    {
        /// <summary>
        /// Backing field for the Login property.
        /// </summary>
        private XmlRpcString login = new XmlRpcString();

        /// <summary>
        /// Backing field for the PlayerId property.
        /// </summary>
        private XmlRpcInt playerId = new XmlRpcInt(); //Probably actually an i4

        /// <summary>
        /// Backing field for the Result property.
        /// </summary>
        private XmlRpcInt result = new XmlRpcInt();

        /// <summary>
        /// Gets the login of the player that this page answer is for.
        /// </summary>
        public string Login
        {
            get { return login.Value; }
        }

        /// <summary>
        /// Gets the id of the player that this page answer is for.
        /// </summary>
        public int PlayerId
        {
            get { return playerId.Value; }
        }

        /// <summary>
        /// Gets the answer of the page. 0 means no answer.
        /// </summary>
        public int Result
        {
            get { return result.Value; }
        }

        /// <summary>
        /// Generates an XElement storing the information in this struct.
        /// </summary>
        /// <returns>The generated XElement.</returns>
        public override XElement GenerateXml()
        {
            return new XElement(XName.Get(ElementName),
                makeMemberElement("Login", login.GenerateXml()),
                makeMemberElement("PlayerId", playerId.GenerateXml()),
                makeMemberElement("Result", result.GenerateXml()));
        }

        /// <summary>
        /// Fills the properties of this struct with the information contained in the element.
        /// </summary>
        /// <param name="xElement">The struct element storing the information.</param>
        /// <returns>Itself, for convenience.</returns>
        public override ManialinkPageAnswer ParseXml(XElement xElement)
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

                    case "PlayerId":
                        playerId.ParseXml(value);
                        break;

                    case "Result":
                        result.ParseXml(value);
                        break;

                    default:
                        throw new FormatException("Unexpected member with name " + getMemberName(member));
                }
            }

            return this;
        }
    }
}