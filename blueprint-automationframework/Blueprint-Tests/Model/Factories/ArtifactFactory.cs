using Common;
using Model.Impl;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using TestConfig;
using Utilities;
using Utilities.Factories;

namespace Model.Factories
{
    public static class ArtifactFactory
    {
        /// <summary>
        /// Create an artifact object and populate required attribute values with ArtifactTypeId, ArtifactTypeName, and ProjectId based the target project
        /// </summary>
        /// <param name="address">address for Blueprint application server</param>
        /// <param name="project">The target project</param>
        /// <param name="artifactType">artifactType</param>
        /// <returns>new artifact object for the target project with selected artifactType</returns>
        /// <exception cref="System.Data.SqlClient.SqlException">The exception that is thrown when SQL Server returns a warning or error.</exception>
        /// <exception cref="System.InvalidOperationException">If no data is present with the requested sql</exception>
        public static IOpenApiArtifact CreateOpenApiArtifact(string address, IProject project, BaseArtifactType artifactType)
        {
            ThrowIf.ArgumentNull(project, nameof(project));

            IOpenApiArtifact artifact = new OpenApiArtifact(address);
            artifact.ArtifactTypeName = artifactType.ToString();
            artifact.Name = "OpenApi_Artifact_" + artifact.ArtifactTypeName + "_" + RandomGenerator.RandomAlphaNumeric(5);

            using (IDatabase database = DatabaseFactory.CreateDatabase())
            {
                string query = "SELECT Project_ItemId, ItemTypeId, Name FROM dbo.TipItemTypesView WHERE Project_ItemId = @Project_ItemId and Name = @Name;";
                using (SqlCommand cmd = database.CreateSqlCommand(query))
                {
                    database.Open();
                    cmd.Parameters.Add("@Project_ItemId", SqlDbType.Int).Value = project.Id;
                    cmd.Parameters.Add("@Name", SqlDbType.NChar).Value = artifact.ArtifactTypeName;
                    cmd.CommandType = CommandType.Text;

                    try
                    {
                        SqlDataReader reader;
                        using (reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();
                            }
                            int queryProjectId = Int32.Parse(reader["Project_ItemId"].ToString(), CultureInfo.InvariantCulture);
                            int queryArtifactTypeId = Int32.Parse(reader["ItemTypeId"].ToString(), CultureInfo.InvariantCulture);
                            string queryArtifactTypeName = reader["Name"].ToString();

                            artifact.ArtifactTypeName = queryArtifactTypeName;
                            artifact.ProjectId = queryProjectId;
                            artifact.ArtifactTypeId = queryArtifactTypeId;
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        Logger.WriteError("No artifact type is available which matches with condition. Exception details - {0}", ex);
                    }
                }
            }
            return artifact;
        }

        /// <summary>
        /// Create an artifact object using the Blueprint application server address from the TestConfiguration file
        /// </summary>
        /// <returns>new artifact object</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]   // Ignore this warning.
        public static IOpenApiArtifact CreateOpenApiArtifact(IProject project, BaseArtifactType artifactType)
        {
            TestConfiguration testConfig = TestConfiguration.GetInstance();
            return CreateOpenApiArtifact(testConfig.BlueprintServerAddress, project, artifactType);
        }
    }
}
