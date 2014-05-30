using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ManiaNet.DedicatedServer.XmlRpc.Types
{
    /// <summary>
    /// Represents an XmlRpcType containing a double.
    /// </summary>
    public class XmlRpcDouble : XmlRpcType<double>
    {
        /// <summary>
        /// The name of Elements of this type.
        /// </summary>
        public override string ElementName
        {
            get { return "double"; }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ManiaNet.XmlRpc.Types.XmlRpcDouble"/> class with the default double value for the Value property.
        /// </summary>
        public XmlRpcDouble()
        {
            Value = default(double);
        }

        /// <summary>
        /// Sets the Value property with the information contained in the XElement. It must have a name fitting with the ElementName property.
        /// </summary>
        /// <param name="xElement">The element containing the information.</param>
        /// <returns>Itself, for convenience.</returns>
        public override XmlRpcType<double> ParseXml(XElement xElement)
        {
            checkName(xElement);

            Value = double.Parse(xElement.Value);

            return this;
        }
    }
}