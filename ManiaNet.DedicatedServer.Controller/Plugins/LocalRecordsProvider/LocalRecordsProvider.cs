using ManiaNet.DedicatedServer.Controller.Annotations;
using ManiaNet.DedicatedServer.Controller.Plugins.Extensibility.Clients;
using ManiaNet.DedicatedServer.Controller.Plugins.Extensibility.Records;
using ManiaNet.DedicatedServer.XmlRpc.Methods;
using ManiaNet.DedicatedServer.XmlRpc.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ManiaNet.DedicatedServer.Controller.Plugins.LocalRecordsProvider
{
    [RegisterPlugin("controller::LocalRecordsProvider")]
    public sealed class LocalRecordsProvider : ControllerPlugin, IRecordsProvider
    {
        private const string tableDDL =
@"CREATE TABLE IF NOT EXISTS LocalRecords (
    Map                     VARCHAR( 27, 27 )  PRIMARY KEY
                                               NOT NULL
                                               REFERENCES Maps ( Id ),
    Player                  VARCHAR( 250 )     NOT NULL
                                               REFERENCES Clients ( Login ),
    TimeOrScore             INTEGER            NOT NULL,
    CheckpointTimesOrScores TEXT               NOT NULL
);";

        private readonly Dictionary<string, List<int>> currentCheckpoints = new Dictionary<string, List<int>>();
        private ServerController controller;
        private int currentGameMode = -1;
        private MapInfoStruct currentMap;
        private Dictionary<string, LocalRecord> currentRecords = new Dictionary<string, LocalRecord>();

        public override bool RequiresRun
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the <see cref="IRecord"/>s for a map. Default is the current map.
        /// </summary>
        /// <param name="map">The UId of the map to get the <see cref="IRecord"/>s for. Default is the current map.</param>
        /// <param name="offset">The offset from which to start returning <see cref="IRecord"/>s.</param>
        /// <param name="length">The maximum number of <see cref="IRecord"/>s to return. Default is all.</param>
        /// <returns>The <see cref="IRecord"/>s for the map.</returns>
        public IEnumerable<IRecord> GetRecords(string map = "", uint offset = 0, uint length = 0)
        {
            using (var command = controller.Database.CreateCommand())
            {
                command.CommandText = string.Format("SELECT * FROM `LocalRecords` WHERE `Map`='{0}'", string.IsNullOrWhiteSpace(map) ? currentMap.UId : map);

                if (offset > 0)
                    command.CommandText += string.Format(" OFFSET {0}", offset);

                if (length > 0)
                    command.CommandText += string.Format(" LIMIT {0}", length);

                command.CommandText += " ORDER BY `TimeOrScore` ASC;";

                var reader = command.ExecuteReader();
                while (reader.NextResult())
                    yield return new LocalRecord(controller.ClientsManager.GetClientInfo((string)reader["Player"]), reader);
            }
        }

        /// <summary>
        /// Gets the <see cref="IRecord"/>s for a Client.
        /// </summary>
        /// <param name="client">The Client to get the <see cref="IRecord"/>s for.</param>
        /// <param name="offset">The offset from which to start returning <see cref="IRecord"/>s.</param>
        /// <param name="length">The maximum number of <see cref="IRecord"/>s to return. Default is all.</param>
        /// <returns>The <see cref="IRecord"/>s for the Client.</returns>
        public IEnumerable<IRecord> GetRecords(IClient client, uint offset = 0, uint length = 0)
        {
            using (var command = controller.Database.CreateCommand())
            {
                command.CommandText = string.Format("SELECT * FROM `LocalRecords` WHERE `Player`='{0}'", client.Login);

                if (offset > 0)
                    command.CommandText += string.Format(" OFFSET {0}", offset);

                if (length > 0)
                    command.CommandText += string.Format(" LIMIT {0}", length);

                command.CommandText += " ORDER BY `TimeOrScore` ASC;";

                var reader = command.ExecuteReader();
                while (reader.NextResult())
                    yield return new LocalRecord(client, reader);
            }
        }

        /// <summary>
        /// Gets called when the plugin is loaded.
        /// Use this to add your methods to the controller's events and load your saved data.
        /// </summary>
        /// <param name="controller">The controller loading the plugin.</param>
        /// <returns>Whether it loaded successfully or not.</returns>
        public override bool Load(ServerController controller)
        {
            if (!isAssemblyServerController(Assembly.GetCallingAssembly()))
                return false;

            this.controller = controller;

            controller.BeginMap += controller_BeginMap;
            controller.PlayerCheckpoint += controller_PlayerCheckpoint;
            controller.PlayerFinish += controller_PlayerFinish;

            using (var command = controller.Database.CreateCommand())
            {
                command.CommandText = tableDDL;
                command.ExecuteNonQuery();
            }

            if (!getGameMode())
                return false;

            var getCurrentMapInfoCall = new GetCurrentMapInfo();
            controller.CallMethod(getCurrentMapInfoCall, 5000);

            if (getCurrentMapInfoCall.HadFault)
                return false;

            currentMap = getCurrentMapInfoCall.ReturnValue;

            return true;
        }

        /// <summary>
        /// Gets called when the plugin is unloaded.
        /// Use this to save your data.
        /// </summary>
        /// <returns>Whether it unloaded successfully or not.</returns>
        public override bool Unload()
        {
            return isAssemblyServerController(Assembly.GetCallingAssembly());
        }

        private void controller_BeginMap(ServerController sender, ManiaPlanetBeginMap methodCall)
        {
            currentCheckpoints.Clear();
            currentMap = methodCall.Map;

            currentRecords = GetRecords(currentMap.UId).Cast<LocalRecord>().ToDictionary(record => record.Player.Login);
        }

        private void controller_PlayerCheckpoint(ServerController sender, TrackManiaPlayerCheckpoint methodCall)
        {
            if (!currentCheckpoints.ContainsKey(methodCall.PlayerLogin))
                currentCheckpoints.Add(methodCall.PlayerLogin, new List<int>());

            currentCheckpoints[methodCall.PlayerLogin].Add(methodCall.TimeOrScore);
        }

        private void controller_PlayerFinish(ServerController sender, TrackManiaPlayerFinish methodCall)
        {
            if (methodCall.TimeOrScore <= 0)
                return;

            var checkpoints = new int[0];
            if (currentCheckpoints.ContainsKey(methodCall.PlayerLogin))
            {
                checkpoints = currentCheckpoints[methodCall.PlayerLogin].ToArray();
                currentCheckpoints[methodCall.PlayerLogin].Clear();
            }

            // TODO: Add stuff for Script mode.
            if (currentRecords.ContainsKey(methodCall.PlayerLogin) && (
                    (currentGameMode != GameModes.Script && currentGameMode != GameModes.Stunts && methodCall.TimeOrScore > currentRecords[methodCall.PlayerLogin].TimeOrScore)
                 || (currentGameMode == GameModes.Stunts && methodCall.TimeOrScore < currentRecords[methodCall.PlayerLogin].TimeOrScore)))
                return;
            controller.ChatInterfaceManager.Get("chat").Send("$zNew Record by " + controller.ClientsManager.GetClientInfo(methodCall.PlayerLogin).Nickname + "$z: " + (methodCall.TimeOrScore / 1000) + ":" + (methodCall.TimeOrScore % 1000));
            save(new LocalRecord(controller.ClientsManager.GetClientInfo(methodCall.PlayerLogin), currentMap.UId, methodCall.TimeOrScore, checkpoints));
        }

        private bool getGameMode()
        {
            var getGameModeCall = new GetGameMode();
            controller.CallMethod(getGameModeCall, 5000);

            if (getGameModeCall.HadFault)
                return false;

            currentGameMode = getGameModeCall.ReturnValue;

            return true;
        }

        private void onNewRecord(LocalRecord record)
        {
            if (NewRecord != null)
                NewRecord(this, record);
        }

        private void onRecordImproved(LocalRecord oldRecord, LocalRecord record)
        {
            if (RecordImproved != null)
                RecordImproved(this, oldRecord, record);
        }

        private void save([NotNull] LocalRecord record)
        {
            LocalRecord oldRecord = currentRecords.ContainsKey(record.Player.Login) ? currentRecords[record.Player.Login] : null;

            currentRecords[record.Player.Login] = record;

            using (var command = controller.Database.CreateCommand())
            {
                command.CommandText = string.Format("INSERT OR REPLACE INTO `LocalRecords` (`Map`, `Player`, `TimeOrScore`, `CheckpointTimesOrScores`) VALUES {0}", record.toValuesSQL());
                command.ExecuteNonQuery();
            }

            if (oldRecord != null)
                onRecordImproved(oldRecord, record);
            else
                onNewRecord(record);
        }

        public event NewRecordEventHandler NewRecord;

        public event RecordImprovedEventHandler RecordImproved;
    }
}