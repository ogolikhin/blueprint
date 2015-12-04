using System;
using Model.Impl;
using TestConfig;

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
            if (databaseName == null) { throw new ArgumentNullException("databaseName"); }

            TestConfiguration testConfig = TestConfiguration.GetInstance();

            IDatabase database = new MsSqlDatabase(testConfig.Databases[databaseName].ConnectionString);
            return database;
        }
    }
}
