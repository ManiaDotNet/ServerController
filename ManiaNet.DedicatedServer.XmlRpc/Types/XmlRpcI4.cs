using System;
using System.Collections.Generic;
using System.Linq;

namespace ManiaNet.DedicatedServer.XmlRpc.Types
{
    public class XmlRpcI4 : XmlRpcInt
    {
        /// <summary>
        /// The name of Elements of this type.
        /// </summary>
        public override string ElementName
        {
            get { return "i4"; }
        }
    }
}