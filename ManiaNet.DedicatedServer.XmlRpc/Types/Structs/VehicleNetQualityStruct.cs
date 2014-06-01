using System;
using System.Collections.Generic;
using System.Linq;

namespace ManiaNet.DedicatedServer.XmlRpc.Types.Structs
{
    /// <summary>
    /// Represents the struct returned by the GetVehicleNetQuality method call.
    /// </summary>
    public sealed class VehicleNetQualityStruct : I4CurrentAndNextValueStruct<VehicleNetQualityStruct>
    {
        // No idea what exactly the modes are ... of course that's documented nowhere.

        /// <summary>
        /// Gets the current vehicle quality.
        /// </summary>
        public override int CurrentValue
        {
            get { return currentValue.Value; }
        }

        /// <summary>
        /// Gets the next vehicle quality.
        /// </summary>
        public override int NextValue
        {
            get { return nextValue.Value; }
        }
    }
}