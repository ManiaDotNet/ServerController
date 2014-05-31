using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ManiaNet.DedicatedServer.XmlRpc.Types.Structs
{
    /// <summary>
    /// Represents the struct returned by the GetVersion method call.
    /// </summary>
    public sealed class ApplicationVersionStruct : BaseStruct<ApplicationVersionStruct>
    {
        /// <summary>
        /// Backing field for the ApiVersion property.
        /// </summary>
        private XmlRpcString apiVersion = new XmlRpcString();

        /// <summary>
        /// Backing field for the Build property.
        /// </summary>
        private XmlRpcString build = new XmlRpcString();

        /// <summary>
        /// Backing field for the Name property.
        /// </summary>
        private XmlRpcString name = new XmlRpcString();

        /// <summary>
        /// Backing field for the TitleId property.
        /// </summary>
        private XmlRpcString titleId = new XmlRpcString();

        /// <summary>
        /// Backing field for the Version property.
        /// </summary>
        private XmlRpcString version = new XmlRpcString();

        /// <summary>
        /// Gets the Version of the API used for callbacks.
        /// </summary>
        public string ApiVersion
        {
            get { return apiVersion.Value; }
            private set { apiVersion.Value = value; }
        }

        /// <summary>
        /// Gets the Build of the server application.
        /// </summary>
        public string Build
        {
            get { return build.Value; }
            private set { build.Value = value; }
        }

        /// <summary>
        /// Gets the Name of the server application.
        /// </summary>
        public string Name
        {
            get { return name.Value; }
            private set { name.Value = value; }
        }

        /// <summary>
        /// Gets the ID of the Title that's currently being played (TMCanyon, TMValley, etc.) on the server application.
        /// </summary>
        public string TitleI
        {
            get { return titleId.Value; }
            private set { titleId.Value = value; }
        }

        /// <summary>
        /// Gets the Version of the server application.
        /// </summary>
        public string Version
        {
            get { return version.Value; }
            private set { version.Value = value; }
        }

        /// <summary>
        /// Generates an XElement storing the information in this struct.
        /// </summary>
        /// <returns>The generated XElement.</returns>
        public override XElement GenerateXml()
        {
            return new XElement(XName.Get(ElementName),
                makeMemberElement("Name", name.GenerateXml()),
                makeMemberElement("TitleId", titleId.GenerateXml()),
                makeMemberElement("Version", version.GenerateXml()),
                makeMemberElement("Build", build.GenerateXml()),
                makeMemberElement("ApiVersion", apiVersion.GenerateXml()));
        }

        /// <summary>
        /// Fills the properties of this struct with the information contained in the element.
        /// </summary>
        /// <param name="xElement">The struct element storing the information.</param>
        /// <returns>Itself, for convenience.</returns>
        public override ApplicationVersionStruct ParseXml(XElement xElement)
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

                    case "TitleId":
                        titleId.ParseXml(getNormalizedStringValueContent(value, titleId.ElementName));
                        break;

                    case "Version":
                        version.ParseXml(getNormalizedStringValueContent(value, version.ElementName));
                        break;

                    case "Build":
                        build.ParseXml(getNormalizedStringValueContent(value, build.ElementName));
                        break;

                    case "ApiVersion":
                        apiVersion.ParseXml(getNormalizedStringValueContent(value, apiVersion.ElementName));
                        break;

                    default:
                        throw new FormatException("Unexpected member with name " + getMemberName(member));
                }
            }

            return this;
        }
    }
}