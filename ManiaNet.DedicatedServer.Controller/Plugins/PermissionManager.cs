using System;
using System.Collections.Generic;
using System.Linq;

namespace ManiaNet.DedicatedServer.Controller.Plugins
{
    [RegisterPlugin("PermissionManager")]
    internal class PermissionManager : ControllerPlugin
    {
        private const int GROUP = 1;
        private const int PLAYER = 0;
        private ServerController controller;

        private Dictionary<string, List<string>> permissionCache;

        public override bool RequiresRun
        {
            get { return false; }
        }

        public override bool Load(ServerController controller)
        {
            this.controller = controller;
            bool setupSuccess = true;
            permissionCache = new Dictionary<string, List<string>>();
            setupSuccess &= controller.RegisterCommand("perm", chatCommand);
            using (var command = controller.Database.CreateCommand())
            {
                command.CommandText = @"CREATE TABLE IF NOT EXISTS `permissions` (
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
	                `holder`			VARCHAR(200) NOT NULL
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

        public override void Run()
        {
            throw new NotImplementedException();
        }

        public override bool Unload()
        {
            throw new NotImplementedException();
        }

        private void writeHelp(string sender, string cmd = null)
        {
            // TODO: Write how to use it
        }

        private void chatCommand(XmlRpc.Methods.ManiaPlanetPlayerChat obj)
        {
            string[] cmds = obj.Text.Split(" ".ToCharArray());
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

        /// <summary>
        /// Inserts new permissions into the database.
        /// Note that example.domain will also insert example.* for later use.
        /// </summary>
        /// <param name="permissions">The permissions to insert.</param>
        /// <returns>False on error.</returns>
        private bool insertPermission(List<string> permissions)
        {
            bool result = true;
            foreach (string permission in permissions)
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
            List<string> parts = permission.Split(".".ToCharArray()).ToList<string>();
            string ddlRaw = "INSERT INTO `permissions` (`permission`) VALUES ('{0}');";
            try {
                using (var cmd = controller.Database.CreateCommand()) {
                    cmd.CommandText = String.Format(ddlRaw, permission);
                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                while (parts.Count > 1)
                {
                    parts.Pop();
                    using (var cmd = controller.Database.CreateCommand())
                    {
                        try
                        {
                            cmd.CommandText = String.Format(ddlRaw, String.Join(".", parts) + ".*");
                            cmd.ExecuteNonQuery();
                        }
                        catch { /* probably conflict with unique attribute */ }
                    }
                }
            }
            return true;
        }

        private bool createGroup(string name, string[] parents = null)
        {
            if (groupExists(name))
                return false;
            string ddl = String.Format(@"INSERT INTO `permissions_groups` (`name`) VALUES ('{0}');", name);
            if (parents != null)
            {
                foreach (string parent in parents)
                {
                    ddl += String.Format(@"INSERT INTO `permissions_inheritance` (`heir`, `holder`) VALUES (
                        (SELECT `id` FROM `permissions_groups` WHERE `name`='{0}'),
                        (SELECT `id` FROM `permissions_groups` WHERE `name`='{1}')
                    )", name, parent);
                }
                permissionCache = new Dictionary<string, List<string>>();
            }
            using (var command = controller.Database.CreateCommand())
            {
                command.CommandText = ddl;
                command.ExecuteNonQuery();
            }
            return true;
        }

        /// <summary>
        /// Adds a player to a group. Creates the group if it doesn't exist.
        /// </summary>
        /// <param name="account">The user to add.</param>
        /// <param name="group">The group to add to.</param>
        private void addToGroup(string account, string group)
        {
            if (!this.groupExists(group))
                this.createGroup(group);
            string ddl = String.Format(@"INSERT INTO `permissions_targets` (`target`, `target_type`, `permission`, `perm_type`)
                VALUES ('{0}', 0, (SELECT `id` FROM `permissions_groups` WHERE `name`='{1}'), 1)",
                account,
                group);
            using (var cmd = controller.Database.CreateCommand())
            {
                cmd.CommandText = ddl;
                cmd.ExecuteNonQuery();
            }
            permissionCache = new Dictionary<string, List<string>>();
        }
        /// <summary>
        /// Removes a player from a group.
        /// </summary>
        /// <param name="account">The player to remove from the group.</param>
        /// <param name="group">the group to remove from.</param>
        private void removeFromGroup(string account, string group)
        {
            string ddl = String.Format(@"DELETE FROM `permissions_targets` WHERE `target`='{0}' AND `target_type`=0 AND `perm_type`=1 AND
                `permission`=(SELECT `id` FROM `permissions_groups` WHERE `name`='{1}')",
                account,
                group);
            using (var cmd = controller.Database.CreateCommand())
            {
                cmd.CommandText = ddl;
                cmd.ExecuteNonQuery();
            }
            permissionCache = new Dictionary<string, List<string>>();
        }

        /// <summary>
        /// Returns all permissions granted for a user.
        /// </summary>
        /// <param name="target">A player account or a group name</param>
        /// <param name="type">PermissionManager.PLAYER or PermissionManager.GROUP</param>
        /// <returns>A list of all permissions</returns>
        private List<string> getPermissions(string target, int type)
        {
            string key = type.ToString() + "_" + target;
            if (permissionCache.ContainsKey(key))
                return permissionCache[key];
            List<string> result = new List<string>();
            string ddl = "";
            if (type == PermissionManager.PLAYER)
                ddl = String.Format(@"SELECT `permission` FROM `permissions` WHERE `id` IN (
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
            else if (type == PermissionManager.GROUP)
                ddl = String.Format(@"SELECT `permission` FROM `permissions` WHERE `id` IN (
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
            else
                return result;
            using (var cmd = controller.Database.CreateCommand())
            {
                cmd.CommandText = ddl;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetFieldValue<string>(0));
                    }
                }
            }
            permissionCache[key] = result;
            return result;
        }

        /// <summary>
        /// Grants a permission for a group or player.
        /// </summary>
        /// <param name="target">A player account or a group name</param>
        /// <param name="permission">The permission to grant.</param>
        /// <param name="type">PermissionManager.PLAYER or PermissionManager.GROUP</param>
        /// <returns>False on error</returns>
        private bool grantPermission(string target, string permission, int type)
        {
            permissionCache = new Dictionary<string, List<string>>();
            if (type == PermissionManager.GROUP && !this.groupExists(target))
                this.createGroup(target);
            string ddl = "";
            if (type == PermissionManager.PLAYER)
                ddl = String.Format(@"INSERT INTO `permissions_targets` (`target`, `target_type`, `permission`, `perm_type`)
                    VALUES ('{0}', 0, (SELECT `id` FROM `permissions` WHERE `permission`='{1}'), 0)", target, permission);
            else if (type == PermissionManager.GROUP)
                ddl = String.Format(@"INSERT INTO `permissions_targets` (`target`, `target_type`, `permission`, `perm_type`)
                    VALUES ('(SELECT `id` FROM `pe-rmissions_groups` WHERE `name`='{0}')', 1, (SELECT `id` FROM `permissions` WHERE `permission`='{1}'), 0)", target, permission);
            else
                return false;
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
        /// Returns if a Player or a Group has a given permission.
        /// Note that a player with permission example.* has the permission example.domain
        /// </summary>
        /// <param name="target">A player account or a group name</param>
        /// <param name="permission">The permission to check for.</param>
        /// <param name="type">PermissionManager.PLAYER or PermissionManager.GROUP</param>
        /// <returns>Bool value if the target has the requested permission.</returns>
        private bool hasPermission(string target, string permission, int type)
        {
            bool hasPerm = false;
            List<string> perms = getPermissions(target, type);
            hasPerm = perms.Contains(permission);
            if (!hasPerm)
            {
                List<string> parts = permission.Split(".".ToCharArray()).ToList<string>();
                while (parts.Count > 1 && !hasPerm)
                {
                    parts.Pop();
                    hasPerm = perms.Contains(String.Join(".", parts) + ".*");
                }
            }
            return hasPerm;
        }

        /// <summary>
        /// Revokes a permission for a group or player.
        /// Note that it will revoke all subpermissions as well.
        /// </summary>
        /// <param name="target">A player account or a group name</param>
        /// <param name="permission">The permission to revoke.</param>
        /// <param name="type">PermissionManager.PLAYER or PermissionManager.GROUP</param>
        /// <returns>False on error</returns>
        private bool revokePermission(string target, string permission, int type)
        {
            permissionCache = new Dictionary<string, List<string>>();
            if (type == PermissionManager.GROUP && !this.groupExists(target))
                this.createGroup(target);
            string ddl = "";
            if (type == PermissionManager.PLAYER)
                ddl = String.Format(@"DELETE FROM `permissions_targets` WHERE `target`='{0}' AND `target_type`=0 AND `perm_type`=0
                    AND `permission` IN (SELECT `id` FROM `permissions` WHERE `permission` LIKE '{1}.%' OR `permission`='{1}')",
                    target,
                    permission);
            else if (type == PermissionManager.GROUP)
                ddl = String.Format(@"DELETE FROM `permissions_targets` WHERE `target`='(SELECT `id` FROM `permissions_groups` WHERE `name`='{0}')'
                    AND `target_type`=1 AND `perm_type`=0 AND `permission` IN (SELECT `id` FROM `permissions` WHERE `permission` LIKE '{1}.%' OR `permission`='{1}')", 
                    target,
                    permission);
            else
                return false;
            using (var cmd = controller.Database.CreateCommand())
            {
                cmd.CommandText = ddl;
                cmd.ExecuteNonQuery();
            }
            return true;
        }
    }
}