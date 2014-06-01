using System;
using System.Collections.Generic;
using System.Linq;

namespace ManiaNet.DedicatedServer.XmlRpc.Types.Structs
{
    /// <summary>
    /// Represents the struct returned by the GetLadderMode method call.
    /// </summary>
    public sealed class LadderModeStruct : I4CurrentAndNextValueStruct<LadderModeStruct>
    {
        /// <summary>
        /// Value for Forced ladder mode.
        /// </summary>
        public const int Forced = 1;

        /// <summary>
        /// Value for Inactive ladder mode.
        /// </summary>
        public const int Inactive = 0;

        /// <summary>
        /// Gets the current ladder mode. Compare to Forced and Inactive constants.
        /// </summary>
        public override int CurrentValue
        {
            get { return currentValue.Value; }
        }

        /// <summary>
        /// Gets the next ladder mode. Compare to Forced and Inactive constants.
        /// </summary>
        public override int NextValue
        {
            get { return nextValue.Value; }
        }
    }
}