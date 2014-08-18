using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ManiaNet.DedicatedServer.Controller.Database
{
    public sealed class ClientsAccess : IDisposable
    {
        private readonly SQLiteConnection connection;

        public ClientsAccess(SQLiteConnection connection)
        {
            this.connection = connection;
            connection.CreateTable<Client>();
        }

        public void Dispose()
        {
            connection.Dispose();
        }
    }
}