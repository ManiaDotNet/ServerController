using ManiaNet.DedicatedServer.XmlRpc.Methods;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ManiaNet.DedicatedServer.Controller.Plugins
{
    internal class LocalRecords : ControllerPlugin
    {
        private Dictionary<string, List<int>> currentSplits;
        private RecordSet records;

        public override bool RequiresRun
        {
            get { return false; }
        }

        /// <summary>
        /// Gets called when the plugin is loaded.
        /// Use this to add your methods to the controller's events and load your saved data.
        /// </summary>
        /// <param name="controller">The controller loading the plugin.</param>
        /// <returns>Whether it loaded successfully or not.</returns>
        public override bool Load(ServerController controller)
        {
            controller.BeginMap += controller_BeginMap;
            controller.PlayerFinish += controller_PlayerFinish;
            controller.PlayerCheckpoint += controller_PlayerCheckpoint;
            controller.EndMap += controller_EndMap;

            return true;
        }

        /// <summary>
        /// The main method of the plugin.
        /// Gets run in its own thread by the controller and should stop gracefully on a <see cref="System.Threading.ThreadAbortException"/>.
        /// </summary>
        public override void Run()
        { }

        /// <summary>
        /// Gets called when the plugin is unloaded.
        /// Use this to save your data.
        /// </summary>
        /// <returns>Whether it unloaded successfully or not.</returns>
        public override bool Unload()
        {
            return true;
        }

        private void controller_BeginMap(ServerController sender, ManiaPlanetBeginMap methodCall)
        {
            currentSplits = new Dictionary<string, List<int>>();
            records = new RecordSet(methodCall.Map.UId);
        }

        private void controller_EndMap(ServerController sender, ManiaPlanetEndMap methodCall)
        {
            records.WriteRecords();
        }

        private void controller_PlayerCheckpoint(ServerController sender, TrackManiaPlayerCheckpoint methodCall)
        {
            if (!currentSplits.ContainsKey(methodCall.PlayerLogin))
                currentSplits.Add(methodCall.PlayerLogin, new List<int>());

            currentSplits[methodCall.PlayerLogin].Add(methodCall.TimeOrScore);
        }

        private void controller_PlayerFinish(ServerController sender, TrackManiaPlayerFinish methodCall)
        {
            if (methodCall.TimeOrScore <= 0)
                return;

            var checkpoints = new List<int>();

            if (currentSplits.ContainsKey(methodCall.PlayerLogin))
            {
                checkpoints = currentSplits[methodCall.PlayerLogin];
                currentSplits[methodCall.PlayerLogin] = new List<int>();
            }

            var newRecord = new Record(methodCall.PlayerLogin, methodCall.TimeOrScore, checkpoints);
            records.AddRecord(newRecord);
        }
    }

    internal class Record
    {
        /// <summary>
        /// A list of the checkpoint times during the run.
        /// </summary>
        public List<int> Checkpoints { get; private set; }

        /// <summary>
        /// The player's account who drove the record.
        /// </summary>
        public string Player { get; private set; }

        /// <summary>
        /// The rank of the record compared to other records for the map.
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// The time the player achieved.
        /// </summary>
        public int Time { get; private set; }

        public Record(string player, int time, List<int> checkpoints)
        {
            // check for valid amount of checkpoints?
            Player = player;
            Time = time;
            Checkpoints = checkpoints;
        }
    }

    // TODO: Rename class?
    /// <summary>
    /// Bundles the difference between an older and a newer record time, providing the respective data
    /// </summary>
    internal class RecordDiff
    {
        public List<int> NewCheckpoints { get; private set; }

        public int NewRank { get; private set; }

        public int NewTime { get; private set; }

        public List<int> OldCheckpoints { get; private set; }

        public int OldRank { get; private set; }

        public int OldTime { get; private set; }

        public string Player { get; private set; }

        public int TimeDifference { get; private set; }

        public RecordDiff(Record oldRecord, Record newRecord)
        {
            Player = newRecord.Player;
            NewTime = newRecord.Time;
            NewCheckpoints = newRecord.Checkpoints;
            NewRank = newRecord.Rank;
            OldTime = oldRecord.Time;
            OldCheckpoints = oldRecord.Checkpoints;
            OldRank = oldRecord.Rank;
            TimeDifference = NewTime - OldTime;
        }
    }

    internal class RecordSet
    {
        private List<Record> records;
        private Dictionary<string, Record> recordsByLogin = new Dictionary<string, Record>();

        public string Map { get; private set; }

        public RecordSet(string uid)
        {
            Map = uid;
            setupDB();
            // TODO: read records from DB
            var query = String.Format("SELECT * FROM `records` WHERE `map`='{0}' AND `valid`=1 ORDER BY `time` ASC", Map);
        }

        /// <summary>
        /// Adds a record to the sets, updates all ranks.
        /// </summary>
        /// <param name="record">A Record object containing Player, Finishtime and Checkpoint times.</param>
        /// <returns>True, if record could be saved, false if not.</returns>
        public bool AddRecord(Record record)
        {
            Record oldRecord = GetRecord(record.Player);
            // TODO: check for valid amount of checkpoints?
            if (oldRecord == null)
                recordsByLogin.Add(record.Player, record);
            else if (oldRecord.Time > record.Time)
            {
                recordsByLogin[record.Player] = record;
                records.Remove(oldRecord);
            }
            else
                return false;
            records.Add(record);
            records = records.OrderBy(o => o.Time).ToList();
            for (int rank = 0; rank < records.Count; rank++)
                records[rank].Rank = rank + 1;
            recordsByLogin = records.ToDictionary(r => r.Player);
            var rd = new RecordDiff(oldRecord, record);
            // raise NewLocalRecord event returning rd
            return true;
        }

        /// <summary>
        /// Returns the local best time for a player if available.
        /// </summary>
        /// <param name="player">The account of the player to look for.</param>
        /// <returns>A Record object if the player has a local best, null if not.</returns>
        public Record GetRecord(string player)
        {
            if (recordsByLogin.ContainsKey(player))
                return recordsByLogin[player];
            return null;
        }

        /// <summary>
        /// Writes records in set back to database
        /// </summary>
        internal void WriteRecords()
        {
            string queryInvalidate = "UPDATE `records` SET `valid`=0 WHERE `map` = '" + Map + "' AND `account` IN (";
            string queryInsert = "INSERT OR IGNORE INTO `records` (`map`, `account`, `time`, `checkpoints`) VALUES ";
            int n = 0;
            foreach (Record r in records)
            {
                queryInsert += String.Format("('{0}', '{1}', {2}, '{3}')", Map, r.Player, r.Time, String.Join(",", r.Checkpoints));
                queryInvalidate += String.Format("'{0}'", r.Player);
                if (n++ < records.Count)
                {
                    queryInsert += ", ";
                    queryInvalidate += ", ";
                }
            }
            queryInvalidate += ")";
            // TODO: execute queries, queryInvalidate first
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates the databae schema if it does not exist
        /// </summary>
        private void setupDB()
        {
            string query = @"CREATE TABLE IF NOT EXISTS `records` (
            `id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
            `map` VARCHAR(64) NOT NULL,
            `account` VARCHAR(255) NOT NULL,
            `time` INTEGER(10) NOT NULL,
            `checkpoints` TEXT NOT NULL,
            `valid` INTEGER(1) DEFAULT 1
            )";
        }
    }
}