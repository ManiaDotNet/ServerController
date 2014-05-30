using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace ManiaNet.DedicatedServer.XmlRpc.Types
{
    /// <summary>
    /// Represents an XmlRpcType containing a string.
    /// </summary>
    public class XmlRpcString : XmlRpcType<string>
    {
        /// <summary>
        /// The name of Elements of this type.
        /// </summary>
        public override string ElementName
        {
            get { return "string"; }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ManiaNet.XmlRpc.Types.XmlRpcString"/> class with an empty string for the Value property.
        /// </summary>
        public XmlRpcString()
        {
            Value = string.Empty;
        }

        /// <summary>
        /// Generates an XElement from the Value. Default implementation creates an XElement with the ElementName and the content from Value.
        /// </summary>
        /// <returns>The generated Xml.</returns>
        public override XElement GenerateXml()
        {
            return new XElement(XName.Get(ElementName), HttpUtility.HtmlEncode(Value));
        }

        /// <summary>
        /// Sets the Value property with the information contained in the XElement. It must have a name fitting with the ElementName property.
        /// </summary>
        /// <param name="xElement">The element containing the information.</param>
        /// <returns>Itself, for convenience.</returns>
        public override XmlRpcType<string> ParseXml(XElement xElement)
        {
            checkName(xElement);

            Value = HttpUtility.HtmlDecode(xElement.Value);

            return this;
        }
    }
}