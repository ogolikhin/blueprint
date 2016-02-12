using Common;
using Model.Impl;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Utilities.Factories;

namespace Model.Factories
{
    public static class ProjectFactory
    {
        /// <summary>
        /// Creates a new project object with the values specified, or with random values for any unspecified parameters.
        /// </summary>
        /// <param name="name">The name of the project.</param>
        /// <param name="description">The description of the project.</param>
        /// <param name="location">The location of the project.</param>
        /// <param name="id">Internal database identifier.  Only set this if you read the project from the database.</param>
        /// <returns>The new project object.</returns>
        public static IProject CreateProject(string name = null, string description = null, string location = null, int id = 0)
        {
            if (name == null) { name = RandomGenerator.RandomAlphaNumeric(10); }
            if (description == null) { description = RandomGenerator.RandomAlphaNumeric(10); }
            if (location == null) { location = RandomGenerator.RandomAlphaNumeric(10); }

            IProject project = new Project { Name = name, Description = description, Location = location, Id = id };
            return project;
        }

        /// <summary>
        /// Get the project object with the name specified, or the first project from BP database.
        /// </summary>
        /// <param name="projectName">The name of the project.</param>
        /// <returns>The first valid project object that retrieved from DB or valid project object with the project name specified </returns>
        public static IProject GetProject(string projectName = null)
        {
            IProject project;
            string query = null;
            SqlDataReader reader;

            int query_projectId = 0;
            string query_projectName = "";
            string query_projectDescription = "";

            using (IDatabase database = DatabaseFactory.CreateDatabase())
            {

                if (projectName == null)
                {
                    query = "SELECT ItemId, Parent_InstanceFolderId, Name, Description FROM dbo.Items_Project WHERE Parent_InstanceFolderId is not null and ItemId != 0 order by ItemId asc";
                }
                else
                {
                    query = "SELECT ItemId, Parent_InstanceFolderId, Name FROM dbo.Items_Project WHERE Parent_InstanceFolderId is not null and Name = @Name";
                }
                Logger.WriteDebug("Running: {0}", query);
                using (SqlCommand cmd = database.CreateSqlCommand(query))
                {
                    database.Open();
                    if (projectName != null)
                    {
                        cmd.Parameters.Add("@Name", SqlDbType.NChar).Value = projectName;
                    }
                    cmd.CommandType = CommandType.Text;
                    using (reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                        }

                        query_projectId = Int32.Parse(reader["ItemId"].ToString(), CultureInfo.InvariantCulture);
                        query_projectName = reader["Name"].ToString();
                        query_projectDescription = reader["Description"].ToString();
                    }
                }
            }
            project = new Project { Name = query_projectName, Description = query_projectDescription, Id = query_projectId };
            return project;
        }
    }
}

