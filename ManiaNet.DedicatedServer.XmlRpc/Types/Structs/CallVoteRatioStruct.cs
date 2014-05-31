using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ManiaNet.DedicatedServer.XmlRpc.Types.Structs
{
    public sealed class CallVoteRatioStruct : BaseStruct<CallVoteRatioStruct>
    {
        /// <summary>
        /// Backing field for the Command property.
        /// </summary>
        private XmlRpcString command = new XmlRpcString();

        /// <summary>
        /// Backing field for the Ratio property.
        /// </summary>
        private XmlRpcDouble ratio = new XmlRpcDouble();

        /// <summary>
        /// Gets or sets the name of the call-vote command that this ratio is for. * for all.
        /// </summary>
        public string Command
        {
            get { return command.Value; }
            set { command.Value = value; }
        }

        /// <summary>
        /// Gets or sets the Ratio for call-votes for this command. Range from 0-1, or -1 for disabled.
        /// </summary>
        public double Ratio
        {
            get { return ratio.Value; }
            set
            {
                if (value != -1 && (value < 0 || value > 1))
                    throw new ArgumentOutOfRangeException("value", "Ratio has to be between 0 and 1, or -1.");

                ratio.Value = value;
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ManiaNet.DedicatedServer.XmlRpc.Types.Structs.CallVoteRatioStruct"/> class with the given ratio for the command.
        /// </summary>
        /// <param name="command">The name of the call-vote command that this ratio is for. * for all.</param>
        /// <param name="ratio">The ratio for call-votes for this command. Range from 0-1, or -1 for disabled.</param>
        public CallVoteRatioStruct(string command, double ratio)
        {
            Command = command;
            Ratio = ratio;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ManiaNet.DedicatedServer.XmlRpc.Types.Structs.CallVoteRatioStruct"/> class without content (for parsing from Xml).
        /// </summary>
        public CallVoteRatioStruct()
        {
        }

        /// <summary>
        /// Generates an XElement storing the information in this struct.
        /// </summary>
        /// <returns>The generated XElement.</returns>
        public override XElement GenerateXml()
        {
            return new XElement(XName.Get(ElementName),
                makeMemberElement("Command", command.GenerateXml()),
                makeMemberElement("Ratio", ratio.GenerateXml()));
        }

        /// <summary>
        /// Fills the properties of this struct with the information contained in the element.
        /// </summary>
        /// <param name="xElement">The struct element storing the information.</param>
        /// <returns>Itself, for convenience.</returns>
        public override CallVoteRatioStruct ParseXml(XElement xElement)
        {
            checkName(xElement);

            foreach (XElement member in xElement.Descendants(XName.Get(MemberElement)))
            {
                checkIsValidMemberElement(member);

                XElement value = getMemberValueElement(member);

                switch (getMemberName(member))
                {
                    case "Command":
                        command.ParseXml(getNormalizedStringValueContent(value, command.ElementName));
                        break;

                    case "Ratio":
                        ratio.ParseXml(value);
                        break;

                    default:
                        throw new FormatException("Unexpected member with name " + getMemberName(member));
                }
            }

            return this;
        }
    }
}