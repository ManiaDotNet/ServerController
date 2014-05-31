using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ManiaNet.DedicatedServer.XmlRpc.Types.Structs
{
    /// <summary>
    /// Represents the structs passed to the ChatSendServerMessageToLanguage and ChatSendToLanguage method calls.
    /// </summary>
    public sealed class LanguageMessageStruct : BaseStruct<LanguageMessageStruct>
    {
        /// <summary>
        /// Backing field for the Lang property.
        /// </summary>
        private XmlRpcString lang = new XmlRpcString();

        /// <summary>
        /// Backing field for the Text property.
        /// </summary>
        private XmlRpcString text = new XmlRpcString();

        /// <summary>
        /// Gets or sets the two letter language code for this message.
        /// </summary>
        public string Lang
        {
            get { return lang.Value; }
            set
            {
                if (value.Length != 2)
                    throw new FormatException("Language has to be a two letter code.");

                lang.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the Text for this message.
        /// </summary>
        public string Text
        {
            get { return text.Value; }
            set { text.Value = value; }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ManiaNet.DedicatedServer.XmlRpc.Types.Structs.LanguageMessageStruct"/> class without content (for parsing from Xml).
        /// </summary>
        public LanguageMessageStruct()
        { }

        /// <summary>
        /// Creates a new instance of the <see cref="ManiaNet.DedicatedServer.XmlRpc.Types.Structs.LanguageMessageStruct"/> class with the given text for the given language.
        /// </summary>
        /// <param name="lang">The two letter language code that this message is for.</param>
        /// <param name="text">The content of this message.</param>
        public LanguageMessageStruct(string lang, string text)
        {
            Lang = lang;
            Text = text;
        }

        /// <summary>
        /// Generates an XElement storing the information in this struct.
        /// </summary>
        /// <returns>The generated XElement.</returns>
        public override XElement GenerateXml()
        {
            return new XElement(XName.Get(ElementName),
                makeMemberElement("Lang", lang.GenerateXml()),
                makeMemberElement("Text", text.GenerateXml()));
        }

        /// <summary>
        /// Fills the properties of this struct with the information contained in the element.
        /// </summary>
        /// <param name="xElement">The struct element storing the information.</param>
        /// <returns>Itself, for convenience.</returns>
        public override LanguageMessageStruct ParseXml(XElement xElement)
        {
            checkName(xElement);

            foreach (XElement member in xElement.Descendants(XName.Get(MemberElement)))
            {
                checkIsValidMemberElement(member);

                XElement value = getMemberValueElement(member);

                switch (getMemberName(member))
                {
                    case "Lang":
                        lang.ParseXml(getNormalizedStringValueContent(value, lang.ElementName));
                        break;

                    case "Text":
                        text.ParseXml(getNormalizedStringValueContent(value, text.ElementName));
                        break;

                    default:
                        throw new FormatException("Unexpected member with name " + getMemberName(member));
                }
            }

            return this;
        }
    }
}