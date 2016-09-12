using System;
using System.Data.SqlClient;
using Common;
using Model.Factories;
using Model.ArtifactModel;
using System.Collections.Generic;
using Utilities;
using NUnit.Framework;

namespace Model.Impl
{
    public class ProjectRole : IProjectRole
    {
        public const string ROLES_TABLE = "[dbo].[Roles]";

        #region Implements IProjectRole
        public int RoleId { get; set; }

        public int ProjectId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public RolePermissions Permissions { get; set; }

        public bool IsDeleted { get; set; }

        public ProjectRole(int projectId, string name, string description, RolePermissions permissions)
        {
            ProjectId = projectId;
            Name = name;
            Description = description;
            Permissions = permissions;
        }

        /// <seealso cref="IProjectRole.AddRoleToDatabase()"/>
        public void AddRoleToDatabase()
        {
            using (IDatabase database = DatabaseFactory.CreateDatabase())
            {
                database.Open();

                var fields = "[CurrentVersion],[ProjectAdminRoleId],[ProjectId],[ESignatureMeaningId]," +
                    "[Name],[Description],[Permissions],[Deleted]";// [RoleId] is the Primary Key, so it gets created by SQL Server.

                object[] valueArray =
                {
                    0, null, ProjectId, null, Name, Description, (int)Permissions, 0
                };

                string values = string.Join(",", objArraytoStringList(valueArray));

                string query = I18NHelper.FormatInvariant("INSERT INTO {0} ({1}) Output Inserted.RoleId VALUES ({2})", ROLES_TABLE, fields, values);

                Logger.WriteDebug("Running: {0}", query);

                using (SqlCommand cmd = database.CreateSqlCommand(query))
                using (var sqlDataReader = cmd.ExecuteReader())
                {
                    if (sqlDataReader.HasRows)
                    {
                        while (sqlDataReader.Read())
                        {
                            int userIdOrdinal = sqlDataReader.GetOrdinal("RoleId");
                            RoleId = (int)(sqlDataReader.GetSqlInt32(userIdOrdinal));
                        }
                    }
                    else
                    {
                        throw new SqlQueryFailedException(I18NHelper.FormatInvariant("No rows were inserted when running: {0}", query));
                    }
                }
            }
        }

        /// <seealso cref="IProjectRole.DeleteRole()"/>
        public void DeleteRole()
        {
            string query = I18NHelper.FormatInvariant("UPDATE {0} SET Deleted='{1}' WHERE RoleId='{2}'", ROLES_TABLE, 1, RoleId);
            RunSQLQuery(query);
        }
        #endregion Implements IProjectRole

        /// <summary>
        /// Converts the array of objects into a list of strings that are properly formatted and quoted for MS SQL to use.
        /// </summary>
        /// <param name="objArray">The array of objects to convert.</param>
        /// <returns>A list of strings that MS SQL can use.</returns>
        private static List<string> objArraytoStringList(object[] objArray)
        {
            List<string> strList = new List<string>();

            foreach (object obj in objArray)
            {
                if (obj is bool) { strList.Add((bool)obj ? "1" : "0"); }
                else if (obj is int) { strList.Add(obj.ToString()); }
                else if (obj == null) { strList.Add("NULL"); }
                else { strList.Add("'" + obj + "'"); }
            }

            return strList;
        }

        private static void RunSQLQuery(string query)
        {
            using (IDatabase database = DatabaseFactory.CreateDatabase())
            {
                database.Open();

                int rowsAffected = 0;

                Logger.WriteDebug("Running: {0}", query);

                try
                {
                    using (SqlCommand cmd = database.CreateSqlCommand(query))
                    {
                        rowsAffected = cmd.ExecuteNonQuery();
                    }

                    if (rowsAffected <= 0)
                    {
                        string msg = I18NHelper.FormatInvariant("No rows were affected when running: {0}", query);
                        Logger.WriteError(msg);
                    }
                }
                catch
                {
                    throw;
                }
            }
        }
    }
}
