using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ManiaNet.DedicatedServer.XmlRpc.Types
{
    public class XmlRpcInt : XmlRpcType<int>
    {
        /// <summary>
        /// The name of Elements of this type.
        /// </summary>
        public override string ElementName
        {
            get { return "int"; }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ManiaNet.XmlRpc.Types.XmlRpcInt"/> class with the default int value for the Value property.
        /// </summary>
        public XmlRpcInt()
        {
            Value = default(int);
        }

        /// <summary>
        /// Sets the Value property with the information contained in the XElement. It must have a name fitting with the ElementName property.
        /// </summary>
        /// <param name="xElement">The element containing the information.</param>
        /// <returns>Itself, for convenience.</returns>
        public override XmlRpcType<int> ParseXml(XElement xElement)
        {
            checkName(xElement);

            Value = int.Parse(xElement.Value);

            return this;
        }
    }
}