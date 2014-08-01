using ManiaNet.DedicatedServer.XmlRpc.Methods;
using ManiaNet.DedicatedServer.XmlRpc.Structs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ManiaNet.DedicatedServer.Controller.Plugins
{
    internal class LocalRecords
    {
        private ServerController controller;
        private Dictionary<string, List<int>> currentSplits;
        private RecordSet records;

        public LocalRecords(ServerController controller)
        {
            this.controller = controller;
            controller.BeginMap += controller_BeginMap;
            controller.PlayerFinish += controller_PlayerFinish;
            controller.PlayerCheckpoint += controller_PlayerCheckpoint;
            controller.EndMap += controller_EndMap;
        }

        private void controller_BeginMap(ServerController sender, ManiaPlanetBeginMap methodCall)
        {
            currentSplits = new Dictionary<string, List<int>>();
            MapInfoStruct map = methodCall.Map;
            this.records = new RecordSet(map.UId);
        }

        private void controller_EndMap(ServerController sender, ManiaPlanetEndMap methodCall)
        {
            this.records.WriteRecords();
        }

        private void controller_PlayerCheckpoint(ServerController sender, TrackManiaPlayerCheckpoint methodCall)
        {
            if (currentSplits.ContainsKey(methodCall.PlayerLogin))
            {
                currentSplits[methodCall.PlayerLogin] = new List<int>();
            }
            currentSplits[methodCall.PlayerLogin].Add(methodCall.TimeOrScore);
        }

        private void controller_PlayerFinish(ServerController sender, TrackManiaPlayerFinish methodCall)
        {
            if (methodCall.TimeOrScore > 0)
            {
                List<int> checkpoints = new List<int>();
                if (currentSplits.ContainsKey(methodCall.PlayerLogin))
                {
                    checkpoints = currentSplits[methodCall.PlayerLogin];
                    currentSplits[methodCall.PlayerLogin] = new List<int>();
                }
                Record newRecord = new Record(methodCall.PlayerLogin, methodCall.TimeOrScore, checkpoints);
                this.records.AddRecord(newRecord);
            }
        }
    }

    internal class Record
    {
        public Record(string player, int time, List<int> checkpoints)
        {
            // check for valid amount of checkpoints?
            this.Player = player;
            this.Time = time;
            this.Checkpoints = checkpoints;
        }

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
    }

    // TODO: Rename class?
    /// <summary>
    /// Bundles the difference between an older and a newer record time, providing the respective data
    /// </summary>
    internal class RecordDiff
    {
        public RecordDiff(Record oldRecord, Record newRecord)
        {
            this.Player = newRecord.Player;
            this.NewTime = newRecord.Time;
            this.NewCheckpoints = newRecord.Checkpoints;
            this.NewRank = newRecord.Rank;
            this.OldTime = oldRecord.Time;
            this.OldCheckpoints = oldRecord.Checkpoints;
            this.OldRank = oldRecord.Rank;
            this.TimeDifference = this.NewTime - this.OldTime;
        }

        public List<int> NewCheckpoints { get; private set; }

        public int NewRank { get; private set; }

        public int NewTime { get; private set; }

        public List<int> OldCheckpoints { get; private set; }

        public int OldRank { get; private set; }

        public int OldTime { get; private set; }

        public string Player { get; private set; }

        public int TimeDifference { get; private set; }
    }

    internal class RecordSet
    {
        private List<Record> records;

        private Dictionary<string, Record> recordsByLogin;

        public RecordSet(string uid)
        {
            this.Map = uid;
            this.setupDB();
            // TODO: read records from DB
            string query = String.Format("SELECT * FROM `records` WHERE `map`='{0}'", this.Map);
        }

        public string Map { get; private set; }

        /// <summary>
        /// Adds a record to the sets, updates all ranks.
        /// </summary>
        /// <param name="record">A Record object containing Player, Finishtime and Checkpoint times.</param>
        /// <returns>True, if record could be saved, false if not.</returns>
        public bool AddRecord(Record record)
        {
            Record oldRecord = GetRecord(record.Player);
            // check for valid amount of checkpoints?
            if (oldRecord == null)
            {
                recordsByLogin.Add(record.Player, record);
            }
            else if (oldRecord.Time > record.Time)
            {
                recordsByLogin[record.Player] = record;
                records.Remove(oldRecord);
            }
            else
            {
                return false;
            }
            records.Add(record);
            List<Record> orderedRecords = records.OrderBy(o => o.Time).ToList();
            int rank = 1;
            foreach (Record r in orderedRecords)
            {
                r.Rank = rank;
                // TODO: does this already change the acutal objects in the list?
                // line below if not
                // records[rank - 1] = r;
                recordsByLogin[r.Player].Rank = rank;
                rank++;
            }
            this.records = orderedRecords;
            RecordDiff rd = new RecordDiff(oldRecord, record);
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
            string queryInsert = "INSERT OR IGNORE INTO `records` (`map`, `account`, `time`, `checkpoints`) VALUES ";
            string queryDelete = "DELETE FROM `records` WHERE `map` = '" + this.Map + "' AND `account` IN (";
            int n = 0;
            foreach (Record r in records)
            {
                queryInsert += String.Format("('{0}', '{1}', {2}, '{3}')", this.Map, r.Player, r.Time.ToString(), String.Join(",", r.Checkpoints));
                queryDelete += String.Format("'{0}'", r.Player);
                if (n++ < records.Count)
                {
                    queryInsert += ", ";
                    queryDelete += ", ";
                }
            }
            queryDelete += ")";
            // TODO: execute queries
            throw new NotImplementedException();
        }

        private void setupDB()
        {
            string query = @"CREATE TABLE IF NOT EXISTS `records` (
            `map` VARCHAR(64) NOT NULL,
            `account` VARCHAR(255) NOT NULL,
            `time` INTEGER(10) NOT NULL,
            `checkpoints` TEXT NOT NULL,
            PRIMARY KEY (`map`, `account`)
            )";
        }
    }
}