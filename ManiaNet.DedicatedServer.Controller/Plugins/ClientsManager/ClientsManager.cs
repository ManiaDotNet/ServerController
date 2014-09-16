using ManiaNet.DedicatedServer.Controller.Annotations;
using ManiaNet.DedicatedServer.Controller.Plugins.Extensibility.Clients;
using ManiaNet.DedicatedServer.XmlRpc.Methods;
using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ManiaNet.DedicatedServer.Controller.Plugins.ClientsManager
{
    /// <summary>
    /// The default Clients Manager implementation.
    /// </summary>
    [UsedImplicitly]
    [RegisterPlugin("controller::clientsmanager", "Banane9", "Default Clients Manager", "1.0",
        "The default implementation for the IClientsManager interface.")]
    public sealed class ClientsManager : ControllerPlugin, IClientsManager
    {
        // 1 day
        private const uint fetchMaxAgeSeconds = 86400;

        private const string tableDDL =
@"CREATE TABLE IF NOT EXISTS Clients (
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

        private readonly List<Client> currentClients = new List<Client>();
        private ServerController controller;

        /// <summary>
        /// Gets the <see cref="Client"/>s that are currently connected to the Server.
        /// </summary>
        public IEnumerable<IClient> CurrentClients
        {
            get { return currentClients.ToArray(); }
        }

        /// <summary>
        /// Gets whether the plugin requires its Run method to be called.
        /// </summary>
        public override bool RequiresRun
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the <see cref="IClient"/> information for the account hosting the server.
        /// </summary>
        public IClient ServerHost { get; private set; }

        /// <summary>
        /// Gets the most recent <see cref="Client"/> information. Null when there's information missing from the fetched info.
        /// </summary>
        /// <param name="login">The login of the Client that the information is wanted for.</param>
        /// <returns>The most recent <see cref="Client"/> information. Null when there's information missing from the fetched info.</returns>
        public IClient FetchClientInfo(string login)
        {
            var wsClient = controller.WebServicesClient.Players.GetInfoAsync(login).Result;

            try
            {
                var client = new Client(wsClient, DateTime.Now);
                insertOrReplaceInDb(client);
                return client;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the <see cref="Client"/> information. Null when there's information missing from the fetched info or it couldn't be found.
        /// </summary>
        /// <param name="login">The login of the Client that the information is wanted for.</param>
        /// <returns>The <see cref="Client"/> information.</returns>
        public IClient GetClientInfo(string login)
        {
            if (string.IsNullOrWhiteSpace(login))
                return null;

            using (var command = controller.Database.CreateCommand())
            {
                command.CommandText = string.Format("SELECT * FROM `Clients` WHERE `Login`='{0}'", login);
                var reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    if (reader.NextResult())
                        return getDbClientOrFetched(reader);
                }
            }

            try
            {
                return FetchClientInfo(login);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the <see cref="Client"/>s from the Database that match the given SQL-Query. Will throw an Exception if there's a problem with the SQL or the reading of the data.
        /// <para/>
        /// Query snippet will be used like: SELECT * FROM `Clients` WHERE sqlQuery
        /// </summary>
        /// <param name="sqlQuery">The WHERE part of the SQL-Query. Query snippet will be used like: SELECT * FROM `Clients` WHERE sqlQuery</param>
        /// <returns>The <see cref="Client"/>s from the Databse that match the given SQL-Query.</returns>
        public IEnumerable<IClient> GetClientsWhere(string sqlQuery)
        {
            using (var command = controller.Database.CreateCommand())
            {
                command.CommandText = "SELECT * FROM `Clients` WHERE " + sqlQuery;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.NextResult())
                        yield return getDbClientOrFetched(reader);
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

            this.controller = controller;

            using (var command = controller.Database.CreateCommand())
            {
                command.CommandText = tableDDL;
                command.ExecuteNonQuery();
            }

            if (!setUpClientsList())
                return false;

            Console.WriteLine("Set-up clients list.");
            Console.WriteLine("{0} client(s) are already on the Server:\r\n{1}", currentClients.Count, string.Join(", ", currentClients.Select(client => client.Login)));

            controller.RegisterCommand("players", listClients);

            controller.PlayerConnect += controller_PlayerConnect;
            controller.PlayerDisconnect += controller_PlayerDisconnect;

            return true;
        }

        /// <summary>
        /// Gets called when the plugin is unloaded.
        /// Use this to save your data.
        /// </summary>
        /// <returns>Whether it unloaded successfully or not.</returns>
        public override bool Unload()
        {
            return isAssemblyServerController(Assembly.GetCallingAssembly());
        }

        private void controller_PlayerConnect(ServerController sender, ManiaPlanetPlayerConnect methodCall)
        {
            if (controller != sender)
                return;

            if (currentClients.Any(cClient => cClient.Login == methodCall.Login))
                return;

            var getPlayerInfoCall = new GetPlayerInfo(methodCall.Login);
            if (!controller.CallMethod(getPlayerInfoCall, 3000) || getPlayerInfoCall.HadFault)
                return;

            var count = 0;
            Client client = null;
            while (client == null && count++ <= 3)
                client = getClientInfo(getPlayerInfoCall.ReturnValue.Login, getPlayerInfoCall.ReturnValue.NickName);

            if (client != null)
                currentClients.Add(client);

            Console.WriteLine("{0} joined the Server!", methodCall.Login);

            controller.ManialinkDisplayManager.Refresh();
        }

        private void controller_PlayerDisconnect(ServerController sender, ManiaPlanetPlayerDisconnect methodCall)
        {
            if (controller != sender)
                return;

            Console.WriteLine("{0} left the Server!", methodCall.Login);

            currentClients.RemoveAll(client => client.Login == methodCall.Login);
        }

        /// <summary>
        /// Gets the <see cref="Client"/> information. Null when there's information missing from the fetched info or it couldn't be found.
        /// </summary>
        /// <param name="login">The login of the player that the info is wanted for.</param>
        /// <param name="nickName">The up-to-date nickname of the player.</param>
        /// <returns>The <see cref="Client"/> information.</returns>
        [CanBeNull, UsedImplicitly]
        private Client getClientInfo([NotNull] string login, [NotNull] string nickName)
        {
            var client = (Client)GetClientInfo(login);

            if (client == null)
                return null;

            client.Nickname = nickName;
            insertOrReplaceInDb(client);

            return client;
        }

        /// <summary>
        /// Creates a <see cref="Client"/> from the nameValueCollection, and checks its age.
        /// If it's too old it tries to fetch new data and returns that if possible,otherwise it returns the db data.
        /// </summary>
        /// <param name="nameValueCollection">The collection of values from the db.</param>
        /// <returns>The <see cref="Client"/> information.</returns>
        private Client getDbClientOrFetched(SqliteDataReader nameValueCollection)
        {
            var dbClient = new Client(nameValueCollection);

            if ((DateTime.Now - dbClient.Fetched).TotalSeconds < fetchMaxAgeSeconds)
                return dbClient;

            var fetchedClient = (Client)FetchClientInfo(dbClient.Login);

            return fetchedClient ?? dbClient;
        }

        private void insertOrReplaceInDb([NotNull] Client client)
        {
            try
            {
                using (var command = controller.Database.CreateCommand())
                {
                    command.CommandText =
                        string.Format(
                            "INSERT OR REPLACE INTO `Clients` (`Login`, `Id`, `Nickname`, `ZoneId`, `ZonePath`, `Fetched`) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}');",
                            client.Login, client.Id, client.Nickname, client.ZoneId, client.ZonePath, client.Fetched.ToUnixTimeStamp());
                    command.ExecuteNonQuery();
                }
            }
            catch
            {
                Console.WriteLine("Couldn't insert Client information into db.");
            }
        }

        private void listClients(ManiaPlanetPlayerChat chat)
        {
            controller.CallMethod(
                new ChatSendServerMessageToId("$s$fff" + string.Join(", $s$fff", CurrentClients.Select(client => client.Nickname + " $z$s(" + client.Login + ")")),
                    chat.ClientId), 0);
        }

        private bool setUpClientsList()
        {
            var getPlayerListCall = new GetPlayerList(1000, 0);

            if (!controller.CallMethod(getPlayerListCall, 5000))
                return false;

            if (getPlayerListCall.HadFault)
                return false;

            // Server Account is always first.
            var serverHost = getPlayerListCall.ReturnValue.First().Value;
            ServerHost = getClientInfo(serverHost.Login, serverHost.NickName);

            currentClients.Clear();
            currentClients.AddRange(getPlayerListCall.ReturnValue.Skip(1).Select(clientInfo => getClientInfo(clientInfo.Value.Login, clientInfo.Value.NickName)));

            return true;
        }
    }
}