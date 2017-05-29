using System;
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
        /// Executes a SQL DELETE command and returns the number of rows deleted.
        /// </summary>
        /// <param name="tableName">The name of the table to delete from.</param>
        /// <param name="whereColumnName">The name of the column to use to filter which rows to delete (used in the WHERE clause).</param>
        /// <param name="whereValue">The value to use to filter which rows to delete (used in the WHERE clause).</param>
        /// <param name="databaseName">(optional) The database to run the delete against.</param>
        /// <returns>The number of rows deleted.</returns>
        public static int ExecuteDeleteSqlQuery(
            string tableName,
            string whereColumnName,
            string whereValue,
            string databaseName = "Blueprint")
        {
            var query = I18NHelper.FormatInvariant("DELETE FROM {0} WHERE {1}='{2}'",
                    tableName, whereColumnName, whereValue);

            using (var database = DatabaseFactory.CreateDatabase(databaseName))
            {
                database.Open();

                Logger.WriteDebug("Running: {0}", query);

                using (var cmd = database.CreateSqlCommand(query))
                {
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected <= 0)
                    {
                        string msg = I18NHelper.FormatInvariant("No rows were affected when running: {0}", query);
                        Logger.WriteError(msg);
                    }

                    return rowsAffected;
                }
            }
        }

        /// <summary>
        /// Executes a SQL INSERT command and returns the Id of the inserted row.
        /// </summary>
        /// <param name="tableName">The name of the table to insert into.</param>
        /// <param name="columnNames">The column names that match with the values being inserted.</param>
        /// <param name="valueArray">The list of values to insert.</param>
        /// <param name="idColumnName">(optional) The name of the column where the Id will exist.</param>
        /// <param name="databaseName">(optional) The database to run the insert against.</param>
        /// <returns>The ID of the inserted row.</returns>
        public static int ExecuteInsertSqlQueryAndGetId(
            string tableName,
            string[] columnNames,
            object[] valueArray,
            string idColumnName = "Id",
            string databaseName = "Blueprint")
        {
            string fields = string.Join(",", columnNames);

            return ExecuteInsertSqlQueryAndGetId(tableName, fields, valueArray, idColumnName, databaseName);
        }

        /// <summary>
        /// Executes a SQL INSERT command and returns the Id of the inserted row.
        /// </summary>
        /// <param name="tableName">The name of the table to insert into.</param>
        /// <param name="columnNames">The (comma delimited) column names that match with the values being inserted.</param>
        /// <param name="valueArray">The list of values to insert.</param>
        /// <param name="idColumnName">(optional) The name of the column where the Id will exist.</param>
        /// <param name="databaseName">(optional) The database to run the insert against.</param>
        /// <returns>The ID of the inserted row.</returns>
        public static int ExecuteInsertSqlQueryAndGetId(
            string tableName,
            string columnNames,
            object[] valueArray,
            string idColumnName = "Id",
            string databaseName = "Blueprint")
        {
            using (var database = DatabaseFactory.CreateDatabase(databaseName))
            {
                database.Open();
                
                string values = string.Join(",", ObjArraytoStringList(valueArray));
                string query = I18NHelper.FormatInvariant("INSERT INTO {0} ({1}) Output Inserted.{2} VALUES ({3})",
                    tableName, columnNames, idColumnName, values);

                Logger.WriteDebug("Running: {0}", query);

                using (var cmd = database.CreateSqlCommand(query))
                using (var sqlDataReader = cmd.ExecuteReader())
                {
                    if (sqlDataReader.HasRows)
                    {
                        while (sqlDataReader.Read())
                        {
                            int idOrdinal = sqlDataReader.GetOrdinal(idColumnName);
                            return (int)sqlDataReader.GetSqlInt32(idOrdinal);
                        }
                    }
                    else
                    {
                        throw new SqlQueryFailedException(I18NHelper.FormatInvariant("No rows were inserted when running: {0}", query));
                    }
                }

                throw new SqlQueryFailedException(I18NHelper.FormatInvariant("An error occurred when running: {0}", query));
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

        /// <summary>
        /// Converts the array of objects into a list of strings that are properly formatted and quoted for MS SQL to use.
        /// </summary>
        /// <param name="objArray">The array of objects to convert.</param>
        /// <returns>A list of strings that MS SQL can use.</returns>
        private static List<string> ObjArraytoStringList(object[] objArray)
        {
            var strList = new List<string>();

            foreach (var obj in objArray)
            {
                if (obj is bool)
                { strList.Add((bool)obj ? "1" : "0"); }
                else if (obj is int)
                { strList.Add(obj.ToString()); }
                else if (obj is DateTime)
                { strList.Add("'" + DateTimeToString((DateTime)obj) + "'"); }
                else if (obj == null)
                { strList.Add("NULL"); }
                else
                { strList.Add("'" + obj + "'"); }
            }

            return strList;
        }

        /// <summary>
        /// Gets the date in a string format that MS SQL can use.
        /// </summary>
        /// <param name="date">The date to convert to a string.</param>
        /// <returns>A string version of the date.</returns>
        private static string DateTimeToString(DateTime date)
        {
            string dateString = date.ToStringInvariant("yyyy-MM-dd HH:mm:ss");
            return dateString;
        }
    }
}
