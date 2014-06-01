using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ManiaNet.DedicatedServer.XmlRpc.Types.Structs
{
    /// <summary>
    /// Represents the base for the structs returned by the GetCallVoteTimeOut, GetMaxPlayers, GetMaxSpectators, GetLadderMode, and GetVehicleNetQuality method calls.
    /// </summary>
    /// <typeparam name="TDerived">The type of the derived class.</typeparam>
    public abstract class I4CurrentAndNextValueStruct<TDerived> : BaseStruct<TDerived> where TDerived : I4CurrentAndNextValueStruct<TDerived>
    {
        /// <summary>
        /// Backing field for the CurrentValue property.
        /// </summary>
        protected XmlRpcI4 currentValue = new XmlRpcI4();

        /// <summary>
        /// Backing field for the NextValue property.
        /// </summary>
        protected XmlRpcI4 nextValue = new XmlRpcI4();

        /// <summary>
        /// Gets the current value.
        /// </summary>
        public abstract int CurrentValue { get; }

        /// <summary>
        /// Gets the next value.
        /// </summary>
        public abstract int NextValue { get; }

        /// <summary>
        /// Generates an XElement storing the information in this struct.
        /// </summary>
        /// <returns>The generated XElement.</returns>
        public override XElement GenerateXml()
        {
            return new XElement(XName.Get(ElementName),
                makeMemberElement("CurrentValue", currentValue.GenerateXml()),
                makeMemberElement("NextValue", nextValue.GenerateXml()));
        }

        /// <summary>
        /// Fills the properties of this struct with the information contained in the element.
        /// </summary>
        /// <param name="xElement">The struct element storing the information.</param>
        /// <returns>Itself, for convenience.</returns>
        public override TDerived ParseXml(XElement xElement)
        {
            checkName(xElement);

            foreach (XElement member in xElement.Descendants(XName.Get(MemberElement)))
            {
                checkIsValidMemberElement(member);

                XElement value = getMemberValueElement(member);

                switch (getMemberName(member))
                {
                    case "CurrentValue":
                        currentValue.ParseXml(value);
                        break;

                    case "NextValue":
                        nextValue.ParseXml(value);
                        break;

                    default:
                        throw new FormatException("Unexpected member with name " + getMemberName(member));
                }
            }

            return (TDerived)this;
        }
    }
}