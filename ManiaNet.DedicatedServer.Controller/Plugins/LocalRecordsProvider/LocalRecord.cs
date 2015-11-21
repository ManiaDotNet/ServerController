using ManiaNet.DedicatedServer.Controller.Annotations;
using ManiaNet.DedicatedServer.Controller.Plugins.Extensibility.Clients;
using ManiaNet.DedicatedServer.Controller.Plugins.Extensibility.Records;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace ManiaNet.DedicatedServer.Controller.Plugins.LocalRecordsProvider
{
    /// <summary>
    /// Stores information about a Local Record.
    /// </summary>
    public sealed class LocalRecord : IRecord
    {
        private const char checkpointsSeparatorChar = '|';

        /// <summary>
        /// Gets the times in ms or the scores for the checkpoints in order from first to last.
        /// </summary>
        public IEnumerable<long> CheckpointTimesOrScores { get; private set; }

        /// <summary>
        /// Gets the UId of the map that this record is for.
        /// </summary>
        public string Map { get; private set; }

        /// <summary>
        /// Gets the <see cref="IClient"/> for the Player that achieved the Record.
        /// </summary>
        public IClient Player { get; private set; }

        /// <summary>
        /// Gets the time in ms or the score of the Player achieved.
        /// </summary>
        public long TimeOrScore { get; private set; }

        internal LocalRecord([NotNull] IClient player, [NotNull] SQLiteDataReader reader)
        {
            Player = player;

            Map = (string)reader["Map"];
            TimeOrScore = (long)reader["TimeOrScore"];

            var checkpointTimesOrScores = new List<long>();
            foreach (var checkpointTimeOrScore in ((string)reader["CheckpointTimesOrScores"]).Split(checkpointsSeparatorChar))
            {
                long timeOrScore;
                if (long.TryParse(checkpointTimeOrScore, out timeOrScore))
                    checkpointTimesOrScores.Add(timeOrScore);
            }

            CheckpointTimesOrScores = checkpointTimesOrScores.ToArray();
        }

        internal LocalRecord([NotNull] IClient player, [NotNull] string map, long timeOrScore, [NotNull] long[] checkpointTimesOrScores)
        {
            Player = player;
            Map = map;
            TimeOrScore = timeOrScore;
            CheckpointTimesOrScores = checkpointTimesOrScores;
        }

        internal string toValuesSQL()
        {
            return string.Format("('{0}', '{1}', '{2}', '{3}')", Map, Player.Login, TimeOrScore,
                string.Join(checkpointsSeparatorChar.ToString(), CheckpointTimesOrScores.Select(timeOrScore => timeOrScore.ToString()).ToArray()));
        }
    }
}