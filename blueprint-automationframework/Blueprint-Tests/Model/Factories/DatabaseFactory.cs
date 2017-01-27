using Model.Impl;
using System;
using System.Data.SqlClient;
using TestConfig;
using Utilities;

namespace Model.Factories
{
    public static class DatabaseFactory
    {
        /// <summary>
        /// Creates a new database object.
        /// </summary>
        /// <param name="databaseName">(optional) The database name from the TestConfiguration.</param>
        /// <returns>The database object.</returns>
        public static IDatabase CreateDatabase(string databaseName = "Blueprint")
        {
            ThrowIf.ArgumentNull(databaseName, nameof(databaseName));

            var database = new MsSqlDatabase(GetConnectionString(databaseName));
            return database;
        }

        /// <summary>
        /// Gets the database connection string
        /// </summary>
        /// <param name="databaseName">The database name from the TestConfiguration.</param>
        /// <returns>The database connection string.</returns>
        public static string GetConnectionString(string databaseName)
        {
            ThrowIf.ArgumentNull(databaseName, nameof(databaseName));

            var testConfig = TestConfiguration.GetInstance();

            var connectionString = testConfig.Databases[databaseName].ConnectionString;

            try
            {
                var builder = new SqlConnectionStringBuilder();
                builder.ConnectionString = connectionString;
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException("The value does not represent a connection string", e);
            }

            return connectionString;
        }

    }
}
