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
        /// Gets the most recent <see cref="Client"/> information. Null when there's information missing from the fetched info or it couldn't be found.
        /// </summary>
        /// <param name="login">The login of the Client that the information is wanted for.</param>
        /// <returns>The most recent <see cref="Client"/> information. Null when there's information missing from the fetched info.</returns>
        [CanBeNull, UsedImplicitly]
        Client FetchClientInfo([NotNull] string login);

        /// <summary>
        /// Gets the <see cref="Client"/> information. Null when there's information missing from the fetched info or it couldn't be found.
        /// </summary>
        /// <param name="login">The login of the Client that the information is wanted for.</param>
        /// <returns>The <see cref="Client"/> information.</returns>
        [CanBeNull, UsedImplicitly]
        Client GetClientInfo([NotNull] string login);

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