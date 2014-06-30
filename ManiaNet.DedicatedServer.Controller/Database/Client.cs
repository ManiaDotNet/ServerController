using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ManiaNet.DedicatedServer.Controller.Database
{
    /// <summary>
    /// Represents an entry in the Clients table.
    /// </summary>
    public sealed class Client
    {
        [NotNull]
        public DateTime LastVisit { get; set; }

        [PrimaryKey, Unique, NotNull]
        public string Login { get; set; }

        [NotNull]
        public string Nickname { get; set; }

        [NotNull]
        public int Visits { get; set; }
    }
}