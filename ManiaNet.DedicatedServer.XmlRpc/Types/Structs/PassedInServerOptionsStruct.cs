using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ManiaNet.DedicatedServer.XmlRpc.Types.Structs
{
    public sealed class PassedInServerOptionsStruct : BaseStruct<PassedInServerOptionsStruct>
    {
        /// <summary>
        /// Backing field for the AllowChallengeDownload property.
        /// </summary>
        private XmlRpcBoolean allowChallengeDownload = new XmlRpcBoolean();

        /// <summary>
        /// Backing field for the AutoSaveReplays property.
        /// </summary>
        private XmlRpcBoolean autoSaveReplays = new XmlRpcBoolean();

        /// <summary>
        /// Backing field for the CallVoteRatio property.
        /// </summary>
        private XmlRpcDouble callVoteRatio = new XmlRpcDouble();

        /// <summary>
        /// Backing field for the Comment property.
        /// </summary>
        private XmlRpcString comment = new XmlRpcString();

        /// <summary>
        /// Backing field for the IsP2PDownload property.
        /// </summary>
        private XmlRpcBoolean isP2PDownload = new XmlRpcBoolean();

        /// <summary>
        /// Backing field for the IsP2PUpload property.
        /// </summary>
        private XmlRpcBoolean isP2PUpload = new XmlRpcBoolean();

        /// <summary>
        /// Backing field for the Name property.
        /// </summary>
        private XmlRpcString name = new XmlRpcString();

        /// <summary>
        /// Backing field for the NextCallVoteTimeOut property.
        /// </summary>
        private XmlRpcI4 nextCallVoteTimeOut = new XmlRpcI4();

        /// <summary>
        /// Backing field for the NextLadderMode property.
        /// </summary>
        private XmlRpcI4 nextLadderMode = new XmlRpcI4();

        /// <summary>
        /// Backing field for the NextMaxPlayers property.
        /// </summary>
        private XmlRpcI4 nextMaxPlayers = new XmlRpcI4();

        /// <summary>
        /// Backing field for the NextMaxSpectators property.
        /// </summary>
        private XmlRpcI4 nextMaxSpectators = new XmlRpcI4();

        /// <summary>
        /// Backing field for the NextVehicleNetQuality property.
        /// </summary>
        private XmlRpcI4 nextVehicleNetQuality = new XmlRpcI4();

        /// <summary>
        /// Backing field for the Password property.
        /// </summary>
        private XmlRpcString password = new XmlRpcString();

        /// <summary>
        /// Backing field for the PasswordForSpectator property.
        /// </summary>
        private XmlRpcString passwordForSpectator = new XmlRpcString();

        /// <summary>
        /// Gets or sets whether clients are allowed to download the challenges from the server.
        /// </summary>
        public bool AllowChallengeDownload
        {
            get { return allowChallengeDownload.Value; }
            set { allowChallengeDownload.Value = value; }
        }

        /// <summary>
        /// Gets or sets whether the server automatically saves replays of the players.
        /// </summary>
        public bool AutoSaveReplays
        {
            get { return autoSaveReplays.Value; }
            set { autoSaveReplays.Value = value; }
        }

        /// <summary>
        /// Gets or sets the Ratio for call-votes for this command. Range from 0-1, or -1 for disabled.
        /// </summary>
        public double CallVoteRatio
        {
            get { return callVoteRatio.Value; }
            set
            {
                if (value != -1 && (value < 0 || value > 1))
                    throw new ArgumentOutOfRangeException("value", "Ratio has to be between 0 and 1, or -1.");

                callVoteRatio.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the server comment.
        /// </summary>
        public string Comment
        {
            get { return comment.Value; }
            set { comment.Value = value; }
        }

        /// <summary>
        /// Gets or sets whether p2p download is active or not.
        /// </summary>
        public bool IsP2PDownload
        {
            get { return isP2PDownload.Value; }
            set { isP2PDownload.Value = value; }
        }

        /// <summary>
        /// Gets or sets whether p2p upload is active or not.
        /// </summary>
        public bool IsP2PUpload
        {
            get { return isP2PUpload.Value; }
            set { isP2PUpload.Value = value; }
        }

        /// <summary>
        /// Gets or sets the server name.
        /// </summary>
        public string Name
        {
            get { return name.Value; }
            set { name.Value = value; }
        }

        /// <summary>
        /// Gets or sets the next call-vote timeout in milliseconds.
        /// </summary>
        public int NextCallVoteTimeOut
        {
            get { return nextCallVoteTimeOut.Value; }
            set { nextCallVoteTimeOut.Value = value; }
        }

        /// <summary>
        /// Gets or sets the next ladder mode. Compare to Forced and Inactive constants in <see cref="ManiaNet.DedicatedServer.XmlRpc.Types.Structs.LadderModeStruct"/>.
        /// </summary>
        public int NextLadderMode
        {
            get { return nextLadderMode.Value; }
            set
            {
                if (value != LadderModeStruct.Forced && value != LadderModeStruct.Inactive)
                    throw new ArgumentOutOfRangeException("value", "Ladder mode has to be Forced or Inactive");

                nextLadderMode.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the next maximum number of players.
        /// </summary>
        public int NextMaxPlayers
        {
            get { return nextMaxPlayers.Value; }
            set
            {
                nextMaxPlayers.Value = value > 0 ? value : 1;
            }
        }

        /// <summary>
        /// Gets or sets the next maximum number of spectators.
        /// </summary>
        public int NextMaxSpectators
        {
            get { return nextMaxSpectators.Value; }
            set
            {
                nextMaxSpectators.Value = value > -1 ? value : 0;
            }
        }

        /// <summary>
        /// Gets or sets the next vehicle quality.
        /// </summary>
        public int NextVehicleNetQuality
        {
            get { return nextVehicleNetQuality.Value; }
            set
            {
                nextVehicleNetQuality.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the server password.
        /// </summary>
        public string Password
        {
            get { return password.Value; }
            set { password.Value = value; }
        }

        /// <summary>
        /// Gets or sets the spectator password.
        /// </summary>
        public string PasswordForSpectator
        {
            get { return passwordForSpectator.Value; }
            set { passwordForSpectator.Value = value; }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ManiaNet.DedicatedServer.XmlRpc.Types.Structs.PassedInServerOptionsStruct"/> class without content (for parsing from Xml).
        /// </summary>
        public PassedInServerOptionsStruct()
        { }

        /// <summary>
        /// Creates a new instance of the <see cref="ManiaNet.DedicatedServer.XmlRpc.Types.Structs.PassedInServerOptionsStruct"/> class with from the given server options.
        /// </summary>
        /// <param name="serverOptions">The server options to use for the creation.</param>
        public PassedInServerOptionsStruct(ReturnedServerOptionsStruct serverOptions)
        {
            Name = serverOptions.Name;
            Comment = serverOptions.Comment;
            Password = serverOptions.Password;
            PasswordForSpectator = serverOptions.PasswordForSpectator;
            NextMaxPlayers = serverOptions.NextMaxPlayers;
            IsP2PUpload = serverOptions.IsP2PUpload;
            IsP2PDownload = serverOptions.IsP2PDownload;
            NextLadderMode = serverOptions.NextLadderMode;
            NextVehicleNetQuality = serverOptions.NextVehicleNetQuality;
            NextCallVoteTimeOut = serverOptions.NextCallVoteTimeOut;
            AllowChallengeDownload = serverOptions.AllowChallengeDownload;
            AutoSaveReplays = serverOptions.AutoSaveReplays;
        }

        /// <summary>
        /// Generates an XElement storing the information in this struct.
        /// </summary>
        /// <returns>The generated XElement.</returns>
        public override XElement GenerateXml()
        {
            return new XElement(XName.Get(ElementName),
                makeMemberElement("Name", name.GenerateXml()),
                makeMemberElement("Comment", comment.GenerateXml()),
                makeMemberElement("Password", password.GenerateXml()),
                makeMemberElement("PasswordForSpectator", passwordForSpectator.GenerateXml()),
                makeMemberElement("NextMaxPlayers", nextMaxPlayers.GenerateXml()),
                makeMemberElement("IsP2PUpload", isP2PUpload.GenerateXml()),
                makeMemberElement("IsP2PDownload", isP2PDownload.GenerateXml()),
                makeMemberElement("NextLadderMode", nextLadderMode.GenerateXml()),
                makeMemberElement("NextVehicleNetQuality", nextVehicleNetQuality.GenerateXml()),
                makeMemberElement("NextCallVoteTimeOut", nextCallVoteTimeOut.GenerateXml()),
                makeMemberElement("CallVoteRatio", callVoteRatio.GenerateXml()),
                makeMemberElement("AllowChallengeDownload", allowChallengeDownload.GenerateXml()),
                makeMemberElement("AutoSaveReplays", autoSaveReplays.GenerateXml()));
        }

        /// <summary>
        /// Fills the properties of this struct with the information contained in the element.
        /// </summary>
        /// <param name="xElement">The struct element storing the information.</param>
        /// <returns>Itself, for convenience.</returns>
        public override PassedInServerOptionsStruct ParseXml(XElement xElement)
        {
            checkName(xElement);

            foreach (XElement member in xElement.Descendants(XName.Get(MemberElement)))
            {
                checkIsValidMemberElement(member);

                XElement value = getMemberValueElement(member);

                switch (getMemberName(member))
                {
                    case "Name":
                        name.ParseXml(getNormalizedStringValueContent(value, name.ElementName));
                        break;

                    case "Comment":
                        comment.ParseXml(getNormalizedStringValueContent(value, comment.ElementName));
                        break;

                    case "Password":
                        password.ParseXml(getNormalizedStringValueContent(value, password.ElementName));
                        break;

                    case "PasswordForSpectator":
                        passwordForSpectator.ParseXml(getNormalizedStringValueContent(value, passwordForSpectator.ElementName));
                        break;

                    case "NextMaxPlayers":
                        nextMaxPlayers.ParseXml(value);
                        break;

                    case "NextMaxSpectators":
                        nextMaxSpectators.ParseXml(value);
                        break;

                    case "IsP2PUpload":
                        isP2PUpload.ParseXml(value);
                        break;

                    case "IsP2PDownload":
                        isP2PDownload.ParseXml(value);
                        break;

                    case "NextLadderMode":
                        nextLadderMode.ParseXml(value);
                        break;

                    case "NextVehicleNetQuality":
                        nextVehicleNetQuality.ParseXml(value);
                        break;

                    case "NextCallVoteTimeOut":
                        nextCallVoteTimeOut.ParseXml(value);
                        break;

                    case "CallVoteRatio":
                        callVoteRatio.ParseXml(value);
                        break;

                    case "AllowChallengeDownload":
                        allowChallengeDownload.ParseXml(value);
                        break;

                    case "AutoSaveReplays":
                        autoSaveReplays.ParseXml(value);
                        break;

                    default:
                        throw new FormatException("Unexpected member with name " + getMemberName(member));
                }
            }

            return this;
        }
    }
}