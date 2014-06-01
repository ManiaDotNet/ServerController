using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ManiaNet.DedicatedServer.XmlRpc.Types.Structs
{
    /// <summary>
    /// Represents the struct returned by the GetBillState method call.
    /// </summary>
    public sealed class BillStateStruct : BaseStruct<BillStateStruct>
    {
        /// <summary>
        /// Backing field for the State property.
        /// </summary>
        private XmlRpcI4 state = new XmlRpcI4();

        /// <summary>
        /// Backing field for the StateName property.
        /// </summary>
        private XmlRpcString stateName = new XmlRpcString();

        /// <summary>
        /// Backing field for the TransactionId property.
        /// </summary>
        private XmlRpcI4 transactionId = new XmlRpcI4();

        /// <summary>
        /// Gets the code for the current status of the transaction.
        /// </summary>
        public int State
        {
            get { return state.Value; }
        }

        /// <summary>
        /// Gets the name of the current status of the transaction.
        /// <para/>
        /// Possible values: CreatingTransaction, Issued, ValidatingPayement [sic], Payed, Refused, Error.
        /// </summary>
        public string StateName
        {
            get { return stateName.Value; }
        }

        /// <summary>
        /// Gets the Id of the transaction.
        /// </summary>
        public int TransactionId
        {
            get { return transactionId.Value; }
        }

        /// <summary>
        /// Generates an XElement storing the information in this struct.
        /// </summary>
        /// <returns>The generated XElement.</returns>
        public override XElement GenerateXml()
        {
            return new XElement(XName.Get(ElementName),
                makeMemberElement("State", state.GenerateXml()),
                makeMemberElement("StateName", stateName.GenerateXml()),
                makeMemberElement("TransactionId", transactionId.GenerateXml()));
        }

        /// <summary>
        /// Fills the properties of this struct with the information contained in the element.
        /// </summary>
        /// <param name="xElement">The struct element storing the information.</param>
        /// <returns>Itself, for convenience.</returns>
        public override BillStateStruct ParseXml(XElement xElement)
        {
            checkName(xElement);

            foreach (XElement member in xElement.Descendants(XName.Get(MemberElement)))
            {
                checkIsValidMemberElement(member);

                XElement value = getMemberValueElement(member);

                switch (getMemberName(member))
                {
                    case "State":
                        state.ParseXml(value);
                        break;

                    case "StateName":
                        stateName.ParseXml(getNormalizedStringValueContent(value, stateName.ElementName));
                        break;

                    case "TransactionId":
                        transactionId.ParseXml(value);
                        break;

                    default:
                        throw new FormatException("Unexpected member with name " + getMemberName(member));
                }
            }

            return this;
        }
    }
}