using ManiaNet.DedicatedServer.Controller.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ManiaNet.DedicatedServer.Controller.Plugins.Extensibility.Clients
{
    /// <summary>
    /// Defines methods for a Clients Manager.
    /// </summary>
    public interface IClientsManager
    {
        /// <summary>
        /// Gets the <see cref="Client"/>s that are currently connected to the Server.
        /// </summary>
        [NotNull, UsedImplicitly]
        IEnumerable<Client> CurrentClients { get; }

        /// <summary>
        /// Gets the <see cref="Client"/>s from the Database that match the given SQL-Query.
        /// <para/>
        /// Query snippet will be used like: SELECT * FROM `Clients` WHERE sqlQuery
        /// </summary>
        /// <param name="sqlQuery">The WHERE part of the SQL-Query. Query snippet will be used like: SELECT * FROM `Clients` WHERE sqlQuery</param>
        /// <returns>The <see cref="Client"/>s from the Databse that match the given SQL-Query.</returns>
        [NotNull, UsedImplicitly]
        IEnumerable<Client> GetClientsWhere(string sqlQuery);
    }
}