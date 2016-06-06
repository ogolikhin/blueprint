using System;
using System.Data;
using System.Data.SqlClient;
using Common;
using Model.StorytellerModel;
using Model.StorytellerModel.Impl;
using TestConfig;

namespace Model.Factories
{
    public static class StorytellerFactory
    {
        /// <summary>
        /// Creates a new IStoryteller.
        /// </summary>
        /// <param name="address">The URI address of the Storyteller REST API.</param>
        /// <returns>An IStoryteller object.</returns>
        public static IStoryteller CreateStoryteller(string address)
        {
            IStoryteller storyteller = new Storyteller(address);
            return storyteller;
        }

        /// <summary>
        /// Creates a Storyteller object with the settings defined in the TestConfiguration.
        /// </summary>
        /// <returns>The Storyteller object.</returns>
        /// <exception cref="DataException">If there was an error reading required information from the TestConfiguration.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]   // Ignore this warning.
        public static IStoryteller GetStorytellerFromTestConfig()
        {
            TestConfiguration testConfig = TestConfig.TestConfiguration.GetInstance();
            return CreateStoryteller(testConfig.BlueprintServerAddress);
        }

        /// <summary>
        /// Retrieves the Storyteller limit from the ApplicationSettings table
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]   // Ignore this warning.
        public static int GetStorytellerShapeLimitFromDb()
        {
            using (IDatabase database = DatabaseFactory.CreateDatabase())
            {
                database.Open();
                //string query = @"SELECT [Value] FROM [dbo].[ApplicationSettings] WHERE [Key] = 'StorytellerShapeLimit'";
                string query = I18NHelper.FormatInvariant("SELECT [Value] FROM {0} WHERE [Key] = '{1}'", Storyteller.APPLICATION_SETTINGS_TABLE, Storyteller.STORYTELLER_LIMIT_KEY);

                Logger.WriteDebug("Running: {0}", query);
                using (SqlCommand cmd = database.CreateSqlCommand(query))
                {
                    var result = cmd.ExecuteScalar();
                    int returnVal;
                    if (result != null && Int32.TryParse(result.ToString(), out returnVal))
                    {
                        return returnVal;
                    }
                    return 100;
                }
            }
        }
    }
}
