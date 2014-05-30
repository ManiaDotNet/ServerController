using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ManiaNet.DedicatedServer.XmlRpc.Types.Structs
{
    /// <summary>
    /// Represents the struct returned by the GetVersion method call.
    /// </summary>
    public class XmlRpcApplicationVersionStruct : XmlRpcStructBase<XmlRpcApplicationVersionStruct>
    {
        /// <summary>
        /// Gets or sets the Version of the API used for callbacks.
        /// </summary>
        public XmlRpcString ApiVersion { get; set; }

        /// <summary>
        /// Gets or sets the Build of the server application.
        /// </summary>
        public XmlRpcString Build { get; set; }

        /// <summary>
        /// Gets or sets the Name of the application.
        /// </summary>
        public XmlRpcString Name { get; set; }

        /// <summary>
        /// Gets or sets the ID of the Title that's currently being played (TMCanyon, TMValley, etc.)
        /// </summary>
        public XmlRpcString TitleId { get; set; }

        /// <summary>
        /// Gets or sets the Version of the application.
        /// </summary>
        public XmlRpcString Version { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="ManiaNet.XmlRpc.Types.Structs.XmlRpcApplicationVersionStruct"/> class.
        /// </summary>
        public XmlRpcApplicationVersionStruct()
        {
            Name = new XmlRpcString();
            TitleId = new XmlRpcString();
            Version = new XmlRpcString();
            Build = new XmlRpcString();
            ApiVersion = new XmlRpcString();
        }

        /// <summary>
        /// Generates an XElement storing the information in this struct.
        /// </summary>
        /// <returns>The generated XElement.</returns>
        public override XElement GenerateXml()
        {
            return new XElement(XName.Get(ElementName),
                makeMemberElement("Name", Name.GenerateXml()),
                makeMemberElement("TitleId", TitleId.GenerateXml()),
                makeMemberElement("Version", Version.GenerateXml()),
                makeMemberElement("Build", Build.GenerateXml()),
                makeMemberElement("ApiVersion", ApiVersion.GenerateXml()));
        }

        /// <summary>
        /// Fills the properties of this struct with the information contained in the element.
        /// </summary>
        /// <param name="xElement">The struct element storing the information.</param>
        /// <returns>Itself, for convenience.</returns>
        public override XmlRpcApplicationVersionStruct ParseXml(XElement xElement)
        {
            checkName(xElement);

            foreach (XElement member in xElement.Descendants(XName.Get(MemberElement)))
            {
                checkIsValidMemberElement(member);

                XElement value = getMemberValueElement(member);

                switch (getMemberName(member))
                {
                    case "Name":
                        Name.ParseXml(getNormalizedStringValueContent(value, Name.ElementName));
                        break;

                    case "TitleId":
                        TitleId.ParseXml(getNormalizedStringValueContent(value, TitleId.ElementName));
                        break;

                    case "Version":
                        Version.ParseXml(getNormalizedStringValueContent(value, Version.ElementName));
                        break;

                    case "Build":
                        Build.ParseXml(getNormalizedStringValueContent(value, Build.ElementName));
                        break;

                    case "ApiVersion":
                        ApiVersion.ParseXml(getNormalizedStringValueContent(value, ApiVersion.ElementName));
                        break;

                    default:
                        throw new FormatException("Unexpected member with name " + getMemberName(member));
                }
            }

            return this;
        }
    }
}