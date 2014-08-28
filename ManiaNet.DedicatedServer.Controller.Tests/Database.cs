using ManiaNet.DedicatedServer.Controller.Plugins;
using ManiaNet.DedicatedServer.Controller.Plugins.Extensibility.Clients;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace ManiaNet.DedicatedServer.Controller.Tests
{
    [TestClass]
    public class Database
    {
        private const string tableDDL = @"CREATE TABLE IF NOT EXISTS Clients (
    Login    VARCHAR( 250 )  PRIMARY KEY
                             NOT NULL
                             UNIQUE,
    Id       INTEGER         NOT NULL
                             UNIQUE
                             CHECK ( Id > 0 ),
    Nickname VARCHAR( 250 )  NOT NULL,
    ZoneId   INTEGER         NOT NULL
                             CHECK ( ZoneId > 0 ),
    ZonePath VARCHAR( 250 )  NOT NULL,
    Fetched  INTEGER         NOT NULL
);";

        private SQLiteConnection database;

        public Database()
        {
            database = new SQLiteConnection("Data Source=ManiaNet.db3");
            database.Open();

            using (var command = database.CreateCommand())
            {
                command.CommandText = tableDDL;
                command.ExecuteNonQuery();
            }
        }

        [TestMethod]
        public void ClientCanRead()
        {
            using (var command = database.CreateCommand())
            {
                command.CommandText = "SELECT * FROM `Clients` WHERE `Login` LIKE 'Banane9'";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        Console.WriteLine(new Client(reader.GetValues()).Fetched);
                }
            }
        }
    }
}