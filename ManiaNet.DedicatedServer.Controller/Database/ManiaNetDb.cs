using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ManiaNet.DedicatedServer.Controller.Database
{
    public sealed class ManiaNetDb : SQLiteConnection
    {
        public ManiaNetDb()
            : base("ManiaNet.db3", SQLiteOpenFlags.ReadWrite, true)
        { }
    }
}