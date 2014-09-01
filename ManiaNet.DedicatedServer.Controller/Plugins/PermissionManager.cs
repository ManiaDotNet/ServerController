using ManiaNet.DedicatedServer.Controller.Annotations;
using ManiaNet.DedicatedServer.XmlRpc.Methods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ManiaNet.DedicatedServer.Controller.Plugins
{
    /// <summary>
    /// The default Permission Manager implementation.
    /// </summary>
    [UsedImplicitly]
    [RegisterPlugin("controller::PermissionManager", "proni, Banane9", "Default Permission Manager", "1.0",
        "The default implementation for no interface, yet.")]
    public class PermissionManager : ControllerPlugin
    {
        private ServerController controller;

        private Dictionary<string, List<string>> permissionCache;

        /// <summary>
        /// Gets whether the plugin requires its Run method to be called.
        /// </summary>
        public override bool RequiresRun
        {
            get { return false; }
        }

        /// <summary>
        /// Gets called when the plugin is loaded.
        /// Use this to add your methods to the controller's events and load your saved data.
        /// </summary>
        /// <param name="controller">The controller loading the plugin.</param>
        /// <returns>Whether it loaded successfully or not.</returns>
        public override bool Load(ServerController controller)
        {
            if (!isAssemblyServerController(Assembly.GetCallingAssembly()))
                return false;

            this.controller = controller;
            bool setupSuccess = true;
            permissionCache = new Dictionary<string, List<string>>();
            setupSuccess &= controller.RegisterCommand("perm", chatCommand);
            using (var command = controller.Database.CreateCommand())
            {
                command.CommandText =
@"CREATE TABLE IF NOT EXISTS `permissions` (
    `id`			INTEGER PRIMARY KEY AUTOINCREMENT,
    `permission`	VARCHAR(200) NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS `permissions_groups` (
    `id`			INTEGER PRIMARY KEY AUTOINCREMENT,
    `name`			VARCHAR(200) NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS `permissions_inheritance` (
    `id`			INTEGER PRIMARY KEY AUTOINCREMENT,
    `heir`			VARCHAR(200) NOT NULL,
    `holder`		VARCHAR(200) NOT NULL
);

CREATE TABLE IF NOT EXISTS `permissions_targets` (
    `id`			INTEGER PRIMARY KEY AUTOINCREMENT,
    `target`		VARCHAR(200) NOT NULL,
    `permission`	VARCHAR(200) NOT NULL,
    `target_type`	INTEGER(1) DEFAULT 0,   	-- 1=group, 0=player
    `perm_type`		INTEGER(1) DEFAULT 0 	  	-- 1=group, 0=permission
);";
                command.ExecuteNonQuery();
            }

            return setupSuccess;
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

        /// <summary>
        /// Iterates all the ways for how the given permission can be granted.
        /// </summary>
        /// <param name="permission">The permission for which to iterate over the possibilities.</param>
        /// <returns>All the ways that the given permission can be granted.</returns>
        private static IEnumerable<string> getPermissionPossibilities(string permission)
        {
            var currentPermissionPath = "";
            foreach (var permissionNamespace in permission.Split('.'))
            {
                yield return currentPermissionPath + "*";

                currentPermissionPath += permissionNamespace + ".";
            }

            yield return permission;
        }

        /// <summary>
        /// Adds a Client to a group. Creates the group if it doesn't exist.
        /// </summary>
        /// <param name="login">The login of the Client to add.</param>
        /// <param name="group">The Group to add to.</param>
        private void addToGroup(string login, string group)
        {
            if (!this.groupExists(group))
                this.createGroup(group);

            string ddl = String.Format(@"INSERT INTO `permissions_targets` (`target`, `target_type`, `permission`, `perm_type`)
                VALUES ('{0}', 0, (SELECT `id` FROM `permissions_groups` WHERE `name`='{1}'), 1)",
                login,
                group);

            using (var cmd = controller.Database.CreateCommand())
            {
                cmd.CommandText = ddl;
                cmd.ExecuteNonQuery();
            }

            permissionCache = new Dictionary<string, List<string>>();
        }

        private void chatCommand(ManiaPlanetPlayerChat obj)
        {
            string[] cmds = obj.Text.Split(' ');
            if (cmds.Length < 2 || cmds[1].ToLower() == "help")
                writeHelp(obj.ClientLogin);
            else if (cmds.Length >= 2)
            {
                switch (cmds[1])
                {
                }
            }
            throw new NotImplementedException();
        }

        private bool createGroup(string name, params string[] parents)
        {
            if (groupExists(name))
                return false;

            var ddlBuilder = new StringBuilder();
            ddlBuilder.AppendFormat("INSERT INTO `permissions_groups` (`name`) VALUES ('{0}');", name);
            ddlBuilder.AppendLine(Environment.NewLine);

            if (parents != null && parents.Length > 0)
            {
                ddlBuilder.AppendLine("INSERT INTO `permissions_inheritance` (`heir`, `holder`) VALUES");
                ddlBuilder.Append(string.Join("," + Environment.NewLine,
                    parents.Select(parent => string.Format(
@"    (
        (SELECT `id` FROM `permissions_groups` WHERE `name`='{0}'),
        (SELECT `id` FROM `permissions_groups` WHERE `name`='{1}')
    )",
                        name, parent))));

                permissionCache = new Dictionary<string, List<string>>();
            }

            using (var command = controller.Database.CreateCommand())
            {
                command.CommandText = ddlBuilder.ToString();
                command.ExecuteNonQuery();
            }

            return true;
        }

        /// <summary>
        /// Returns all permissions granted to the target. Null when it couldn't be found.
        /// </summary>
        /// <param name="target">A Client login or a Group name.</param>
        /// <param name="type">The type of the Target.</param>
        /// <returns>A list of all permissions granted to the Target. Null when it couldn't be found.</returns>
        private IEnumerable<string> getPermissions(string target, TargetType type)
        {
            if (string.IsNullOrWhiteSpace(target))
                return null;

            string key = type.ToString() + "_" + target;

            if (permissionCache.ContainsKey(key))
                return permissionCache[key].ToArray();
            else
                permissionCache[key] = new List<string>();

            string ddl = "";
            switch (type)
            {
                case TargetType.Player:
                    ddl = String.Format(
@"SELECT `permission` FROM `permissions` WHERE `id` IN (
    SELECT `id` FROM `permissions_targets` WHERE `target_type`=0 AND `perm_type`=0 AND `target`='{0}'
    UNION
    SELECT `permission` FROM `permissions_targets` WHERE `target_type`=1 AND `perm_type`=0 AND `target` IN (
        WITH cte (ID) AS (
            SELECT `a`.`holder`
            FROM `permissions_inheritance` `a`
            WHERE `a`.`heir` IN (SELECT `permission` FROM `permissions_targets` WHERE `target_type`=0 AND `perm_type`=1 AND `target`='{0}')
            UNION ALL
            SELECT `a`.`holder`
            FROM cte
            JOIN `permissions_inheritance` `a` ON cte.ID=`a`.heir)
        SELECT *
        FROM cte
        UNION
        SELECT `permission` FROM `permissions_targets` WHERE `target_type`=0 AND `perm_type`=1 AND `target`='{0}'
    ) GROUP BY `permission`
)", target);
                    break;

                case TargetType.Group:
                    ddl = String.Format(
@"SELECT `permission` FROM `permissions` WHERE `id` IN (
    SELECT `permission` FROM `permissions_targets` WHERE `target_type`=1 AND `perm_type`=0 AND `target` IN (
        WITH cte (ID) AS (
            SELECT `a`.`holder`
            FROM `permissions_inheritance` `a`
            WHERE `a`.`heir` IN (SELECT `id` FROM `permissions_groups` WHERE `name`='{0}')
            UNION ALL
            SELECT `a`.`holder`
            FROM cte
                JOIN `permissions_inheritance` `a` ON cte.ID=`a`.heir)
            SELECT *
            FROM cte
            UNION
                SELECT `id` FROM `permissions_groups` WHERE `name`='{0}'
        ) GROUP BY `permission`
    )", target);
                    break;

                default:
                    return null;
            }

            var result = new List<string>();
            using (var cmd = controller.Database.CreateCommand())
            {
                cmd.CommandText = ddl;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var permission = reader.GetFieldValue<string>(0);
                        result.Add(permission);
                    }
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Grants a permission to a Group or Client login.
        /// </summary>
        /// <param name="target">A Client login or a Group name.</param>
        /// <param name="permission">The permission to grant.</param>
        /// <param name="type">The type of the Target.</param>
        /// <returns>Whether it was successful or not.</returns>
        private bool grantPermission(string target, string permission, TargetType type)
        {
            permissionCache = new Dictionary<string, List<string>>();

            if (type == TargetType.Group && !this.groupExists(target))
                this.createGroup(target);

            string ddl = "";
            switch (type)
            {
                case TargetType.Player:
                    ddl = String.Format(@"INSERT INTO `permissions_targets` (`target`, `target_type`, `permission`, `perm_type`)
                        VALUES ('{0}', 0, (SELECT `id` FROM `permissions` WHERE `permission`='{1}'), 0)", target, permission);
                    break;

                case TargetType.Group:
                    ddl = String.Format(@"INSERT INTO `permissions_targets` (`target`, `target_type`, `permission`, `perm_type`)
                        VALUES ('(SELECT `id` FROM `permissions_groups` WHERE `name`='{0}')', 1, (SELECT `id` FROM `permissions` WHERE `permission`='{1}'), 0)", target, permission);
                    break;

                default:
                    return false;
            }

            using (var cmd = controller.Database.CreateCommand())
            {
                cmd.CommandText = ddl;
                cmd.ExecuteNonQuery();
            }

            return true;
        }

        private bool groupExists(string name)
        {
            string ddl = String.Format(@"SELECT COUNT(*) FROM `permissions_groups` WHERE `name`='{0}';", name);
            using (var command = controller.Database.CreateCommand())
            {
                command.CommandText = ddl;
                var result = command.ExecuteScalar();
                return result != null && (int)result > 0;
            }
        }

        /// <summary>
        /// Returns if a Client or a Group has the given permission.
        /// Note that the permission example.* grants the permission example.domain as well.
        /// </summary>
        /// <param name="target">A Client login or a Group name.</param>
        /// <param name="permission">The permission to check for.</param>
        /// <param name="type">The type of the Target.</param>
        /// <returns>Whether the target has the permission or not.</returns>
        private bool hasPermission(string target, string permission, TargetType type)
        {
            string[] perms = getPermissions(target, type).ToArray();

            return getPermissionPossibilities(permission).Any(possibility => perms.Contains(possibility));
        }

        /// <summary>
        /// Inserts new permissions into the database.
        /// Note that example.domain will also insert example.* for later use.
        /// </summary>
        /// <param name="permissions">The permissions to insert.</param>
        /// <returns>False on error.</returns>
        private bool insertPermission(params string[] permissions)
        {
            bool result = true;

            foreach (var permission in permissions)
                result &= insertPermission(permission);

            return result;
        }

        /// <summary>
        /// Inserts a new permission into the database.
        /// Note that example.domain will also insert example.* for later use.
        /// </summary>
        /// <param name="permission">The permission to insert.</param>
        /// <returns>False on error.</returns>
        private bool insertPermission(string permission)
        {
            if (string.IsNullOrWhiteSpace(permission))
                return false;

            try
            {
                using (var cmd = controller.Database.CreateCommand())
                {
                    cmd.CommandText = "INSERT OR IGNORE INTO `permissions` (`permission`) VALUES ('" + string.Join("', '", getPermissionPossibilities(permission)) + "')";
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Removes a Client from a Group.
        /// </summary>
        /// <param name="login">The player to remove from the group.</param>
        /// <param name="group">the group to remove from.</param>
        private void removeFromGroup(string login, string group)
        {
            var ddl = String.Format(@"DELETE FROM `permissions_targets` WHERE `target`='{0}' AND `target_type`=0 AND `perm_type`=1 AND
                `permission`=(SELECT `id` FROM `permissions_groups` WHERE `name`='{1}')",
                login,
                group);

            using (var cmd = controller.Database.CreateCommand())
            {
                cmd.CommandText = ddl;
                cmd.ExecuteNonQuery();
            }

            permissionCache = new Dictionary<string, List<string>>();
        }

        /// <summary>
        /// Revokes a permission for a Client or Group.
        /// Note that it will revoke all subpermissions as well.
        /// </summary>
        /// <param name="target">A Client login or a Group name.</param>
        /// <param name="permission">The permission to revoke.</param>
        /// <param name="type">The type of the Target.</param>
        /// <returns>Whether it was successful or not.</returns>
        private bool revokePermission(string target, string permission, TargetType type)
        {
            permissionCache = new Dictionary<string, List<string>>();

            if (type == TargetType.Group && !this.groupExists(target))
                return false;

            string ddl = "";
            switch (type)
            {
                case TargetType.Player:
                    ddl = String.Format(@"DELETE FROM `permissions_targets` WHERE `target`='{0}' AND `target_type`=0 AND `perm_type`=0
                    AND `permission` IN (SELECT `id` FROM `permissions` WHERE `permission` LIKE '{1}.%' OR `permission`='{1}')",
                        target,
                        permission);
                    break;

                case TargetType.Group:
                    ddl = String.Format(@"DELETE FROM `permissions_targets` WHERE `target`='(SELECT `id` FROM `permissions_groups` WHERE `name`='{0}')'
                    AND `target_type`=1 AND `perm_type`=0 AND `permission` IN (SELECT `id` FROM `permissions` WHERE `permission` LIKE '{1}.%' OR `permission`='{1}')",
                        target,
                        permission);
                    break;

                default:
                    return false;
            }

            using (var cmd = controller.Database.CreateCommand())
            {
                cmd.CommandText = ddl;
                cmd.ExecuteNonQuery();
            }

            return true;
        }

        private void writeHelp(string sender, string cmd = null)
        {
            // TODO: Write how to use it
        }

        public enum TargetType
        {
            Player,
            Group,
        }
    }
}