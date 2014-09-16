using ManiaNet.DedicatedServer.Controller.Annotations;
using ManiaNet.DedicatedServer.Controller.Plugins.Extensibility.Clients;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ManiaNet.DedicatedServer.Controller.Plugins.Extensibility.Records
{
    /// <summary>
    /// Defines properties for a Record.
    /// </summary>
    public interface IRecord
    {
        /// <summary>
        /// Gets the times in ms or the scores for the checkpoints in order from first to last.
        /// </summary>
        [NotNull, UsedImplicitly]
        IEnumerable<long> CheckpointTimesOrScores { get; }

        /// <summary>
        /// Gets the UId of the map that this record is for.
        /// </summary>
        [NotNull, UsedImplicitly]
        string Map { get; }

        /// <summary>
        /// Gets the <see cref="IClient"/> for the Player that achieved the Record.
        /// </summary>
        [NotNull, UsedImplicitly]
        IClient Player { get; }

        /// <summary>
        /// Gets the time in ms or the score of the Player achieved.
        /// </summary>
        [UsedImplicitly]
        long TimeOrScore { get; }
    }
}