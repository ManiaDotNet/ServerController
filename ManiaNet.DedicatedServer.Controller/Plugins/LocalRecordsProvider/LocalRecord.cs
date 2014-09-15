using ManiaNet.DedicatedServer.Controller.Annotations;
using ManiaNet.DedicatedServer.Controller.Plugins.Extensibility.Clients;
using ManiaNet.DedicatedServer.Controller.Plugins.Extensibility.Records;
using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
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
        public IEnumerable<int> CheckpointTimesOrScores { get; private set; }

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
        public int TimeOrScore { get; private set; }

        internal LocalRecord([NotNull] IClient player, [NotNull] SqliteDataReader reader)
        {
            Player = player;

            Map = (string)reader["Map"];
            TimeOrScore = (int)reader["TimeOrScore"];

            var checkpointTimesOrScores = new List<int>();
            foreach (var checkpointTimeOrScore in ((string)reader["CheckpointTimesOrScores"]).Split(checkpointsSeparatorChar))
            {
                int timeOrScore;
                if (int.TryParse(checkpointTimeOrScore, out timeOrScore))
                    checkpointTimesOrScores.Add(timeOrScore);
            }

            CheckpointTimesOrScores = checkpointTimesOrScores.ToArray();
        }

        internal LocalRecord([NotNull] IClient player, [NotNull] string map, int timeOrScore, [NotNull] int[] checkpointTimesOrScores)
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