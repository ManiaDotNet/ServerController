using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ManiaNet.DedicatedServer.Controller.Database
{
    /// <summary>
    /// Represents an entry in the Clients table.
    /// </summary>
    [Table("Clients")]
    public sealed class Client
    {
        [Column("Data"), Unique, NotNull]
        public string Data { get; set; }

        [Column("Fetched"), NotNull]
        public long Fetched { get; set; }

        [Column("Login"), PrimaryKey, Unique, NotNull]
        public string Login { get; set; }

        [Column("Nickname"), NotNull]
        public string Nickname { get; set; }

        [Column("Path"), NotNull]
        public string Path { get; set; }
    }
}