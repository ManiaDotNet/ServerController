using ManiaNet.DedicatedServer.XmlRpc.Types.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ManiaNet.DedicatedServer.XmlRpc.Types
{
    /// <summary>
    /// Represents an XmlRpcType containing a xml rpc struct that is derived from <see cref="ManiaNet.DedicatedServer.XmlRpc.Types.Structs.XmlRpcStructBase"/>.
    /// </summary>
    /// <typeparam name="TXmlRpcStruct">The Type of the struct. Also the Type of the Value property.</typeparam>
    public class XmlRpcStruct<TXmlRpcStruct> : XmlRpcType<TXmlRpcStruct> where TXmlRpcStruct : BaseStruct<TXmlRpcStruct>, new()
    {
        /// <summary>
        /// The name of Elements of this type.
        /// </summary>
        public override string ElementName
        {
            get { return "struct"; }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ManiaNet.XmlRpc.Types.XmlRpcStruct"/> class with the parameterless TXmlRpcStruct constructor.
        /// </summary>
        public XmlRpcStruct()
        {
            Value = new TXmlRpcStruct();
        }

        /// <summary>
        /// Generates an XElement from the Value. Default implementation creates an XElement with the ElementName and the content from Value.
        /// </summary>
        /// <returns>The generated Xml.</returns>
        public override XElement GenerateXml()
        {
            return Value.GenerateXml();
        }

        /// <summary>
        /// Sets the Value property with the information contained in the XElement. It must have a name fitting with the ElementName property.
        /// </summary>
        /// <param name="xElement">The element containing the information.</param>
        /// <returns>Itself, for convenience.</returns>
        public override XmlRpcType<TXmlRpcStruct> ParseXml(XElement xElement)
        {
            checkName(xElement);

            Value.ParseXml(xElement);

            return this;
        }
    }
}