using System;
using System.Collections.Generic;
using System.Linq;

namespace ManiaNet.DedicatedServer.XmlRpc
{
    public abstract class XmlRpcMethodCall
    {
        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the Xml for this method call. Default implementation works for parameterless methods.
        /// </summary>
        /// <returns>The Xml for the method call.</returns>
        public virtual string GetXml()
        {
            return XmlRpcConstants.XmlDeclaration +
                XmlRpcConstants.MethodCallAndNameOpening + Name + XmlRpcConstants.MethodNameClosingAndParamsOpening +
                XmlRpcConstants.ParamsAndMethodCallClosing;
        }
    }
}