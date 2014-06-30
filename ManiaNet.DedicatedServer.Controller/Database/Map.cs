using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ManiaNet.DedicatedServer.Controller.Database
{
    /// <summary>
    /// Represents an entry in the Maps table.
    /// </summary>
    public sealed class Map
    {
        [NotNull]
        public string Author { get; set; }

        [NotNull]
        public int AuthorTime { get; set; }

        [NotNull]
        public int BronzeTime { get; set; }

        [NotNull]
        public int CopperPrice { get; set; }

        [NotNull]
        public string Environment { get; set; }

        [NotNull]
        public string Filename { get; set; }

        [NotNull]
        public int GoldTime { get; set; }

        [PrimaryKey, Unique, NotNull]
        public string Id { get; set; }

        [NotNull]
        public bool LapRace { get; set; }

        [NotNull]
        public string MapStyle { get; set; }

        [NotNull]
        public string Mood { get; set; }

        [NotNull]
        public string Name { get; set; }

        [NotNull]
        public int NbCheckpoints { get; set; }

        [NotNull]
        public int NbLaps { get; set; }

        [NotNull]
        public int SilverTime { get; set; }
    }
}