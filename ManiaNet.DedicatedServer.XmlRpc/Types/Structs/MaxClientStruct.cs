using System;
using System.Collections.Generic;
using System.Linq;

namespace ManiaNet.DedicatedServer.XmlRpc.Types.Structs
{
    /// <summary>
    /// Represents the struct returned by the GetMaxPlayers and GetMaxSpectators method calls.
    /// </summary>
    public sealed class MaxClientStruct : I4CurrentAndNextValueStruct<MaxClientStruct>
    {
        /// <summary>
        /// Gets the current maximum number of clients of a type.
        /// </summary>
        public override int CurrentValue
        {
            get { return currentValue.Value; }
        }

        /// <summary>
        /// Gets the next maximum number of clients of a type.
        /// </summary>
        public override int NextValue
        {
            get { return nextValue.Value; }
        }
    }
}