using Common;
using Model.Factories;
using Utilities;

namespace Model.Impl
{
    public static class DatabaseHelper
    {
        /// <summary>
        /// Executes the specified SQL query and gets the first value found for the specified column.
        /// </summary>
        /// <typeparam name="T">The data type to return.</typeparam>
        /// <param name="query">The SQL query to run.</param>
        /// <param name="columnName">The column name whose value you want to retrieve.</param>
        /// <param name="databaseName">(optional) The database name (as specified in TestConfiguration.xml).  Defaults to 'Blueprint' (i.e. Raptor).</param>
        /// <returns>The value returned by the query.</returns>
        public static T ExecuteSingleValueSqlQuery<T>(string query, string columnName, string databaseName = "Blueprint")
        {
            using (var database = DatabaseFactory.CreateDatabase(databaseName))
            {
                database.Open();

                Logger.WriteDebug("Running: {0}", query);

                using (var cmd = database.CreateSqlCommand(query))
                using (var sqlDataReader = cmd.ExecuteReader())
                {
                    if (sqlDataReader.Read())
                    {
                        T value = DatabaseUtilities.GetValueOrDefault<T>(sqlDataReader, columnName);
                        Logger.WriteInfo("SQL Query returned '{0}'='{1}'", columnName, value);
                        return value;
                    }

                    throw new SqlQueryFailedException(I18NHelper.FormatInvariant("No rows were found when running: {0}", query));
                }
            }
        }

        /// <summary>
        /// Gets the FileStore FileId from the EmbeddedImages table that matches the specified EmbeddedImageId.
        /// </summary>
        /// <param name="embeddedImageid">The GUID of the EmbeddedImage that was added to ArtifactStore.</param>
        /// <returns>The FileStore FileId that corresponds to the EmbeddedImageId.</returns>
        public static string GetFileStoreIdForEmbeddedImage(string embeddedImageid)
        {
            string selectQuery = I18NHelper.FormatInvariant("SELECT FileId FROM [dbo].[EmbeddedImages] WHERE [EmbeddedImageId] ='{0}'", embeddedImageid);

            return ExecuteSingleValueSqlQuery<string>(selectQuery, "FileId");
        }

    }
}
