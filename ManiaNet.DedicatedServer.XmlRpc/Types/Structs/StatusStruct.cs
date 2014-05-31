using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ManiaNet.DedicatedServer.XmlRpc.Types.Structs
{
    /// <summary>
    /// Represents the struct returned by the GetStatus method call.
    /// </summary>
    public sealed class StatusStruct : BaseStruct<StatusStruct>
    {
        /// <summary>
        /// Backing field for the Code property.
        /// </summary>
        private XmlRpcI4 code = new XmlRpcI4();

        /// <summary>
        /// Backing field for the Name property.
        /// </summary>
        private XmlRpcString name = new XmlRpcString();

        /// <summary>
        /// Gets the Code for the current status of the server application.
        /// </summary>
        public int Code
        {
            get { return code.Value; }
            private set { code.Value = value; }
        }

        /// <summary>
        /// Gets the Name for the current status of the server application.
        /// </summary>
        public string Name
        {
            get { return name.Value; }
            private set { name.Value = value; }
        }

        /// <summary>
        /// Generates an XElement storing the information in this struct.
        /// </summary>
        /// <returns>The generated XElement.</returns>
        public override XElement GenerateXml()
        {
            return new XElement(XName.Get(MemberElement),
                makeMemberElement("Code", code.GenerateXml()),
                makeMemberElement("Name", name.GenerateXml()));
        }

        /// <summary>
        /// Fills the properties of this struct with the information contained in the element.
        /// </summary>
        /// <param name="xElement">The struct element storing the information.</param>
        /// <returns>Itself, for convenience.</returns>
        public override StatusStruct ParseXml(XElement xElement)
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

                    case "Code":
                        code.ParseXml(value);
                        break;

                    default:
                        throw new FormatException("Unexpected member with name " + getMemberName(member));
                }
            }

            return this;
        }
    }
}