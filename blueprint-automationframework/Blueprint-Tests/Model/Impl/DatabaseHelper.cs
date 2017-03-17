using System.Collections.Generic;
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
        /// <exception cref="SqlQueryFailedException">If no rows were found during the query.</exception>
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
        /// Executes the specified SQL query and gets the values for the specified column names for the first row returned.
        /// </summary>
        /// <param name="query">The SQL query to run.</param>
        /// <param name="columnNames">The column names whose values you want to retrieve.</param>
        /// <param name="databaseName">(optional) The database name (as specified in TestConfiguration.xml).  Defaults to 'Blueprint' (i.e. Raptor).</param>
        /// <returns>A map of column names and values returned by the query.</returns>
        /// <exception cref="SqlQueryFailedException">If no rows were found during the query.</exception>
        public static Dictionary<string, string> ExecuteMultipleValueSqlQuery(
            string query,
            List<string> columnNames,
            string databaseName = "Blueprint")
        {
            ThrowIf.ArgumentNull(columnNames, nameof(columnNames));

            using (var database = DatabaseFactory.CreateDatabase(databaseName))
            {
                database.Open();

                Logger.WriteDebug("Running: {0}", query);
                var queryValues = new Dictionary<string, string>();

                using (var cmd = database.CreateSqlCommand(query))
                using (var sqlDataReader = cmd.ExecuteReader())
                {
                    if (sqlDataReader.Read())
                    {
                        foreach (string columnName in columnNames)
                        {
                            string columnvalue = DatabaseUtilities.GetValueAsString(sqlDataReader, columnName);
                            Logger.WriteInfo("SQL Query returned '{0}'='{1}'", columnName, columnvalue);
                            queryValues.Add(columnName, columnvalue);
                        }
                    }
                    else
                    {
                        throw new SqlQueryFailedException(I18NHelper.FormatInvariant("No rows were found when running: {0}", query));
                    }
                }

                return queryValues;
            }
        }

        /// <summary>
        /// Executes a SQL update query and returns number of rows affected.
        /// Example: "UPDATE [dbo].[Users] SET [Enabled] = '1' WHERE [UserId] = '1234'"
        /// </summary>
        /// <param name="updateQuery">The SQL update query string.</param>
        /// <param name="databaseName">(optional) The database to run the update against.</param>
        /// <returns>The number of records affected.</returns>
        public static int ExecuteUpdateSqlQuery(string updateQuery, string databaseName = "Blueprint")
        {
            using (var database = DatabaseFactory.CreateDatabase(databaseName))
            {
                database.Open();

                Logger.WriteDebug("Running: {0}", updateQuery);

                using (var cmd = database.CreateSqlCommand(updateQuery))
                {
                    cmd.ExecuteNonQuery();

                    using (var sqlDataReader = cmd.ExecuteReader())
                    {
                        if (sqlDataReader.RecordsAffected <= 0)
                        {
                            throw new SqlQueryFailedException(I18NHelper.FormatInvariant("No rows were inserted when running: {0}", updateQuery));
                        }

                        return sqlDataReader.RecordsAffected;
                    }
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
