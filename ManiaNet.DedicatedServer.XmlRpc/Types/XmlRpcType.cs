﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ManiaNet.DedicatedServer.XmlRpc.Types
{
    /// <summary>
    /// Abstract base class for all XmlRpcTypes.
    /// </summary>
    /// <typeparam name="TValue">The Type of the Value property.</typeparam>
    public abstract class XmlRpcType<TValue>
    {
        /// <summary>
        /// The name of Elements of this type.
        /// </summary>
        public abstract string ElementName { get; }

        /// <summary>
        /// Gets or sets the Value contained by this XmlRpcType.
        /// </summary>
        public TValue Value { get; set; }

        /// <summary>
        /// Generates an XElement from the Value. Default implementation creates an XElement with the ElementName and the content from Value.
        /// </summary>
        /// <returns>The generated Xml.</returns>
        public virtual XElement GenerateXml()
        {
            return new XElement(XName.Get(ElementName), Value);
        }

        /// <summary>
        /// Sets the Value property with the information contained in the XElement. It must have a name fitting with the ElementName property.
        /// </summary>
        /// <param name="xElement">The element containing the information.</param>
        /// <returns>Itself, for convenience.</returns>
        public abstract XmlRpcType<TValue> ParseXml(XElement xElement);

        /// <summary>
        /// Checks whether the element has a name name fitting with the ElementName property.
        /// </summary>
        /// <param name="xElement">The element to check.</param>
        protected void checkName(XElement xElement)
        {
            if (!xElement.Name.LocalName.Equals(ElementName))
                throw new ArgumentException("Element has to have the name " + ElementName, "xElement");
        }
    }
}