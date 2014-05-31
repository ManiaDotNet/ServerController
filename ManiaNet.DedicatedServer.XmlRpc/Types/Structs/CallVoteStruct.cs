using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ManiaNet.DedicatedServer.XmlRpc.Types.Structs
{
    /// <summary>
    /// Represents the struct returned by the GetCurrentCallVote method call.
    /// </summary>
    public sealed class CallVoteStruct : BaseStruct<CallVoteStruct>
    {
        /// <summary>
        /// Backing field for the CallerLogin property.
        /// </summary>
        private XmlRpcString callerLogin = new XmlRpcString();

        /// <summary>
        /// Backing field for the CmdName property.
        /// </summary>
        private XmlRpcString cmdName = new XmlRpcString();

        /// <summary>
        /// Backing field for the CmdParam property.
        /// </summary>
        private XmlRpcString cmdParam = new XmlRpcString();

        /// <summary>
        /// Gets the Login of the player who started the call-vote.
        /// </summary>
        public string CallerLogin
        {
            get { return callerLogin.Value; }
            private set { callerLogin.Value = value; }
        }

        /// <summary>
        /// Gets the Name of the command that the call-vote is for.
        /// </summary>
        public string CmdName
        {
            get { return cmdName.Value; }
            private set { cmdName.Value = value; }
        }

        /// <summary>
        /// Gets the parameter of the command that the call-vote is for.
        /// </summary>
        public string CmdParam
        {
            get { return cmdParam.Value; }
            private set { cmdParam.Value = value; }
        }

        /// <summary>
        /// Generates an XElement storing the information in this struct.
        /// </summary>
        /// <returns>The generated XElement.</returns>
        public override XElement GenerateXml()
        {
            return new XElement(XName.Get(ElementName),
                makeMemberElement("CallerLogin", callerLogin.GenerateXml()),
                makeMemberElement("CmdName", cmdName.GenerateXml()),
                makeMemberElement("CmdParam", cmdParam.GenerateXml()));
        }

        /// <summary>
        /// Fills the properties of this struct with the information contained in the element.
        /// </summary>
        /// <param name="xElement">The struct element storing the information.</param>
        /// <returns>Itself, for convenience.</returns>
        public override CallVoteStruct ParseXml(XElement xElement)
        {
            checkName(xElement);

            foreach (XElement member in xElement.Descendants(XName.Get(MemberElement)))
            {
                checkIsValidMemberElement(member);

                XElement value = getMemberValueElement(member);

                switch (getMemberName(member))
                {
                    case "CallerLogin":
                        callerLogin.ParseXml(getNormalizedStringValueContent(value, callerLogin.ElementName));
                        break;

                    case "CmdName":
                        cmdName.ParseXml(getNormalizedStringValueContent(value, cmdName.ElementName));
                        break;

                    case "CmdParam":
                        cmdParam.ParseXml(getNormalizedStringValueContent(value, cmdParam.ElementName));
                        break;

                    default:
                        throw new FormatException("Unexpected member with name " + getMemberName(member));
                }
            }

            return this;
        }
    }
}