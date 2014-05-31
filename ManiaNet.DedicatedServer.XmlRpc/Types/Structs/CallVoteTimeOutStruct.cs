using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ManiaNet.DedicatedServer.XmlRpc.Types.Structs
{
    /// <summary>
    /// Represents the struct returned by the GetCallVoteTimeOut method call.
    /// </summary>
    public sealed class CallVoteTimeOutStruct : BaseStruct<CallVoteTimeOutStruct>
    {
        /// <summary>
        /// Backing field for the CurrentValue property.
        /// </summary>
        private XmlRpcI4 currentValue = new XmlRpcI4();

        /// <summary>
        /// Backing field for the NextValue property.
        /// </summary>
        private XmlRpcI4 nextValue = new XmlRpcI4();

        /// <summary>
        /// Gets the current timeout value for call-votes. In milliseconds.
        /// </summary>
        public int CurrentValue
        {
            get { return currentValue.Value; }
        }

        /// <summary>
        /// Gets the next timeout value for call-votes. In milliseconds.
        /// </summary>
        public int NextValue
        {
            get { return nextValue.Value; }
        }

        /// <summary>
        /// Generates an XElement storing the information in this struct.
        /// </summary>
        /// <returns>The generated XElement.</returns>
        public override XElement GenerateXml()
        {
            return new XElement(XName.Get(ElementName),
                makeMemberElement("CurrentValue", currentValue.GenerateXml()),
                makeMemberElement("NextValue", nextValue.GenerateXml()));
        }

        /// <summary>
        /// Fills the properties of this struct with the information contained in the element.
        /// </summary>
        /// <param name="xElement">The struct element storing the information.</param>
        /// <returns>Itself, for convenience.</returns>
        public override CallVoteTimeOutStruct ParseXml(XElement xElement)
        {
            checkName(xElement);

            foreach (XElement member in xElement.Descendants(XName.Get(MemberElement)))
            {
                checkIsValidMemberElement(member);

                XElement value = getMemberValueElement(member);

                switch (getMemberName(member))
                {
                    case "CurrentValue":
                        currentValue.ParseXml(value);
                        break;

                    case "NextValue":
                        nextValue.ParseXml(value);
                        break;

                    default:
                        throw new FormatException("Unexpected member with name " + getMemberName(member));
                }
            }

            return this;
        }
    }
}