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
    public class Group : IGroup
    {
        /*TODO reuse/remove objArraytoStringList and dateTimeToString; create private function
        to run SQL query and reuse it*/
        public const string GROUPS_TABLE = "[dbo].[Groups]";
        public const string GROUPUSER_TABLE = "[dbo].[GroupUser]";
        public const string ROLEASSIGNMENTS_TABLE = "[dbo].[RoleAssignments]";

        #region Implements IGroup
        public int GroupId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Email { get; set; }

        public GroupSource Source { get; set; }//for now Database groups only

        public GroupLicenseType LicenseType { get; set; }

        public IProject Scope { get; set; }

        public IGroup Parent { get; set; }

        public bool IsLicenseGroup { get; set; }
        # endregion Implements IGroup

        // These are fields not included by IGroup:

        public int CurrentVersion { get; set; }

        public DateTime StartTimestamp { get; set; }

        public DateTime? EndTimestamp { get; set; }

        #region Methods
        public Group (string name, string description, string email,
            GroupLicenseType licenseType = GroupLicenseType.Author)
        {
            CurrentVersion = 0;//always 0(?)
            Name = name;
            Description = description;
            Email = email;
            Source = GroupSource.Database;
            LicenseType = licenseType;
            EndTimestamp = null;//to delete group it must be set to date of deletion
            Scope = null;//for now we will create Instance level groups only
            Parent = null;//this field allow to include one group into another
        }

        /// <seealso cref="IGroup.AddGroupToDatabase()"/>
        public void AddGroupToDatabase()
        {
            using (IDatabase database = DatabaseFactory.CreateDatabase())
            {
                database.Open();

                var fields = "[CurrentVersion],[Name],[Description],[Email],[Source],[LicenseId]," +
                    "[StartTimestamp],[EndTimestamp],[ProjectId],[Parent_GroupId]";// [GroupId] is the Primary Key, so it gets created by SQL Server.

                object[] valueArray =
                {
                    CurrentVersion, Name, Description, Email, (int)Source, (int)LicenseType, DateTime.Now, EndTimestamp,
                    Scope?.Id, Parent?.GroupId
                };

                string values = string.Join(",", objArraytoStringList(valueArray));

                string query = I18NHelper.FormatInvariant("INSERT INTO {0} ({1}) Output Inserted.GroupId VALUES ({2})", GROUPS_TABLE, fields, values);

                Logger.WriteDebug("Running: {0}", query);

                using (SqlCommand cmd = database.CreateSqlCommand(query))
                using (var sqlDataReader = cmd.ExecuteReader())
                {
                    if (sqlDataReader.HasRows)
                    {
                        while (sqlDataReader.Read())
                        {
                            int userIdOrdinal = sqlDataReader.GetOrdinal("GroupId");
                            GroupId = (int)(sqlDataReader.GetSqlInt32(userIdOrdinal));
                        }
                    }
                    else
                    {
                        throw new SqlQueryFailedException(I18NHelper.FormatInvariant("No rows were inserted when running: {0}", query));
                    }
                }
            }
        }

        /// <seealso cref="IGroup.DeleteGroup()"/>
        public void DeleteGroup()
        {
            //TODO If successful, update user.GroupMembership with this group that it was added to.
            string query = I18NHelper.FormatInvariant("UPDATE {0} SET Deleted='{1}' WHERE GroupId='{2}'", ROLEASSIGNMENTS_TABLE, 1, GroupId);
            RunSQLQuery(query);

            query = I18NHelper.FormatInvariant("DELETE FROM {0} WHERE GroupUser_User_GroupId='{1}'",
                    GROUPUSER_TABLE, GroupId);
            RunSQLQuery(query);

            DateTime? oldEndTimestamp = EndTimestamp;
            EndTimestamp = DateTime.Now;
            query = I18NHelper.FormatInvariant("UPDATE {0} SET EndTimestamp='{1}' WHERE GroupId='{2}'", GROUPS_TABLE, EndTimestamp, GroupId);
            try
            {
                RunSQLQuery(query);
            }
            catch
            {
                EndTimestamp = oldEndTimestamp;
                throw;
            }
        }

        /// <seealso cref="IGroup.AddUser(IUser)"/>
        public void AddUser(IUser user)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            var fields = "[GroupUser_User_GroupId],[Users_UserId]";
            object[] valueArray = {GroupId, user.Id};
            string values = string.Join(",", objArraytoStringList(valueArray));

            string query = I18NHelper.FormatInvariant("INSERT INTO {0} ({1}) VALUES ({2})", GROUPUSER_TABLE, fields, values);
            RunSQLQuery(query);
        }


        /// <seealso cref="IGroup.AssignRoleToProjectOrArtifact(IProject, IArtifact, ProjectRole)"/>
        public void AssignRoleToProjectOrArtifact(IProject project, IProjectRole role,
            IArtifactBase artifact = null)
        {
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(role, nameof(role));
            if (artifact != null)
            {
                Assert.AreEqual(artifact.ProjectId, project.Id, "Artifact doesn't belong to the project provided.");
            }
            using (IDatabase database = DatabaseFactory.CreateDatabase())
            {
                database.Open();

                var fields = "[ProjectId],[RoleId],[ItemId],[UserId],[GroupId],[Deleted]";
                //TODO: add query to get [RoleId] from [dbo].[Roles] by [ProjectId] and [Name] = 'Author'
                //or we can determine RoleId by Permissions and ProjectId
                //also we can use [Permissions] = 4623 - for Author
                // 4623 comes from /blueprint-current/BluePrintSys.RC.Data.AccessAPI/Model/RolePermissions.cs
                object[] valueArray = {
                        project.Id, role.RoleId, (artifact==null ? project.Id : artifact.Id), null, GroupId, 0
                            };
                string values = string.Join(",", objArraytoStringList(valueArray));

                string query = I18NHelper.FormatInvariant("INSERT INTO {0} ({1}) VALUES ({2})", ROLEASSIGNMENTS_TABLE, fields, values);

                Logger.WriteDebug("Running: {0}", query);

                using (SqlCommand cmd = database.CreateSqlCommand(query))
                using (var sqlDataReader = cmd.ExecuteReader())
                {
                    if (sqlDataReader.RecordsAffected <= 0)
                    {
                        throw new SqlQueryFailedException(I18NHelper.FormatInvariant("No rows were inserted when running: {0}", query));
                    }
                }
            }
        }

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
                else if (obj is DateTime) { strList.Add("'" + dateTimeToString((DateTime)obj) + "'"); }
                else if (obj == null) { strList.Add("NULL"); }
                else { strList.Add("'" + obj + "'"); }
            }

            return strList;
        }

        /// <summary>
        /// Gets the date in a string format that MS SQL can use.
        /// </summary>
        /// <param name="date">The date to convert to a string.</param>
        /// <returns>A string version of the date.</returns>
        private static string dateTimeToString(DateTime date)
        {
            string dateString = date.ToStringInvariant("yyyy-MM-dd HH:mm:ss");
            return dateString;
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
        #endregion Methods
    }
}
