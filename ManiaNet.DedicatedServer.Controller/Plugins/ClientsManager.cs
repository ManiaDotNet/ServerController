using ManiaNet.DedicatedServer.Controller.Annotations;
using ManiaNet.DedicatedServer.Controller.Plugins.Extensibility.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ManiaNet.DedicatedServer.Controller.Plugins
{
    /// <summary>
    /// The default Clients Manager implementation.
    /// </summary>
    [UsedImplicitly]
    public sealed class ClientsManager : ControllerPlugin, IClientsManager
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

        private ServerController controller;

        /// <summary>
        /// Gets the <see cref="Client"/>s that are currently connected to the Server.
        /// </summary>
        public IEnumerable<Client> CurrentClients { get; private set; }

        /// <summary>
        /// Gets whether the plugin requires its Run method to be called.
        /// </summary>
        public override bool RequiresRun
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the <see cref="Client"/>s from the Database that match the given SQL-Query.
        /// <para/>
        /// Query snippet will be used like: SELECT * FROM `Clients` WHERE sqlQuery
        /// </summary>
        /// <param name="sqlQuery">The WHERE part of the SQL-Query. Query snippet will be used like: SELECT * FROM `Clients` WHERE sqlQuery</param>
        /// <returns>The <see cref="Client"/>s from the Databse that match the given SQL-Query.</returns>
        public IEnumerable<Client> GetClientsWhere(string sqlQuery)
        {
            using (var command = controller.Database.CreateCommand())
            {
                command.CommandText = "SELECT * FROM `Clients` WHERE " + sqlQuery;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        yield return new Client(reader.GetValues());
                }
            }
        }

        /// <summary>
        /// Gets called when the plugin is loaded.
        /// Use this to add your methods to the controller's events and load your saved data.
        /// </summary>
        /// <param name="controller">The controller loading the plugin.</param>
        /// <returns>Whether it loaded successfully or not.</returns>
        // ReSharper disable once ParameterHidesMember
        public override bool Load(ServerController controller)
        {
            if (!isAssemblyServerController(Assembly.GetCallingAssembly()))
                return false;

            using (var command = controller.Database.CreateCommand())
            {
                command.CommandText = tableDDL;
                command.ExecuteNonQuery();
            }

            this.controller = controller;

            return true;
        }

        /// <summary>
        /// The main method of the plugin.
        /// Gets run in its own thread by the controller and should stop gracefully on a <see cref="System.Threading.ThreadAbortException"/>.
        /// </summary>
        public override void Run()
        { }

        /// <summary>
        /// Gets called when the plugin is unloaded.
        /// Use this to save your data.
        /// </summary>
        /// <returns>Whether it unloaded successfully or not.</returns>
        public override bool Unload()
        {
            return isAssemblyServerController(Assembly.GetCallingAssembly());
        }
    }
}