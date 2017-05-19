using System;
using System.Collections.Generic;
using NUnit.Framework;
using CustomAttributes;
using Model;
using Helper;
using Model.Factories;
using Common;
using Model.Impl;
using TestCommon;
using Utilities;

namespace AdminStoreTests
{
    [TestFixture]
    [Category(Categories.AdminStore)]
    public class ConfigTests : TestBase
    {
        private IUser _user;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        [TestCase]
        [TestRail(146293)]
        [Description("Run:  GET /config/settings  and pass a valid token.  Verify it returns a dictionary of config settings.")]
        public void GetSettings_ValidToken_ReturnsConfigSettings()
        {
            ConfigSettings configSettings = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                configSettings = Helper.AdminStore.GetSettings(_user);
            }, "GetSettings() should return 200 OK if a valid token is provided!");

            // Verify:
            Assert.NotNull(configSettings, "GetConfig() returned null!");
            Assert.That(configSettings.Settings.Count > 0, "GetSettings() returned an empty list!");

            const string maintenanceKey = "maintenance"; // This is the only setting currently returned.
            Assert.That(configSettings.Settings.ContainsKey(maintenanceKey),
                "GetSettings() should return a '{0}' setting!", maintenanceKey);

            const string maintenanceSetting = "daysToKeepInLogs";
            Assert.That(configSettings[maintenanceKey].Values.ContainsKey(maintenanceSetting),
                "The Maintenance config setting should contain an entry for '{0}'!", maintenanceSetting);
        }

        [TestCase]
        [TestRail(146294)]
        [Description("Run:  GET /config/settings  but don't include any Session-Token header.  Verify it returns 401 Unauthorized.")]
        public void GetSettings_MissingTokenHeader_401Unauthorized()
        {
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.GetSettings(session: null);
            }, "GetSettings() should return 401 Unauthorized if no Session-Token header is provided!");
        }

        [TestCase]
        [TestRail(146300)]
        [Description("Run:  GET /config/settings  and pass an invalid token.  Verify it returns 401 Unauthorized.")]
        public void GetSettings_InvalidToken_401Unauthorized()
        {
            var user = UserFactory.CreateUserOnly();
            user.Token.AccessControlToken = (new Guid()).ToString();

            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.AdminStore.GetSettings(user);
            }, "GetSettings() should return 401 Unauthorized if an invalid token is provided!");
        }

        [TestCase]
        [TestRail(146298)]
        [Description("Run:  GET /config/config.js  and pass a valid token.  Verify it returns a the config.js file.")]
        public void GetConfigJS_ValidToken_ReturnsConfigJS()
        {
            string configJs = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                configJs = Helper.AdminStore.GetConfigJs(_user);
            }, "GetConfigJs() should return 200 OK if a valid token is passed!");

            // Verify:
            VerifyConfigJs(configJs);
        }

        [TestCase]
        [TestRail(146299)]
        [Description("Run:  GET /config/config.js  but don't pass a Session-Token header.  Verify it returns a the config.js file.")]
        public void GetConfigJS_MissingTokenHeader_ReturnsConfigJS()
        {
            string configJs = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                configJs = Helper.AdminStore.GetConfigJs(session: null);
            }, "GetConfigJs() should return 200 OK if token is missing!");

            // Verify:
            VerifyConfigJs(configJs);
        }

        [TestCase]
        [TestRail(303898)]
        [Description("Run: GET /config but don't pass a Session-Token header. Verify it returns the application settings dictionary.")]
        public void GetApplicationSettings_MissingTokenHeader_ReturnsApplicationSettingsDictionary()
        {
            // Setup:
            Dictionary<string, string> settings = null;
            var user = UserFactory.CreateUserOnly();

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                settings = Helper.AdminStore.GetApplicationSettings(user);
            });

            // Verify:
            Assert.IsNotNull(settings);
            Assert.IsTrue(settings.Keys.Count > 0);
        }

        [TestCase]
        [TestRail(303916)]
        [Description("Run: GET /config and pass an invalid token. Verify it returns the application settings dictionary.")]
        public void GetApplicationSettings_InvalidToken_ReturnsApplicationSettingsDictionary()
        {
            // Setup:
            Dictionary<string, string> settings = null;
            var user = UserFactory.CreateUserOnly();
            user.Token.AccessControlToken = new Guid().ToString();

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                settings = Helper.AdminStore.GetApplicationSettings(user);
            });

            // Verify:
            Assert.IsNotNull(settings);
            Assert.IsTrue(settings.Keys.Count > 0);
        }

        [TestCase]
        [TestRail(303899)]
        [Description("Run: GET /config and pass a valid token. Verify it returns the application settings dictionary.")]
        public void GetApplicationSettings_ValidToken_ReturnsApplicationSettingsDictionary()
        {
            // Setup:
            Dictionary<string, string> settings = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                settings = Helper.AdminStore.GetApplicationSettings(_user);
            });

            // Verify:
            Assert.IsNotNull(settings);
            Assert.IsTrue(settings.Keys.Count > 0);
        }

        /// <summary>
        /// Verifies that the specified config JS string contains the Application Settings from the database.
        /// </summary>
        /// <param name="configJs">The Config JS string returned from the REST call.</param>
        private static void VerifyConfigJs(string configJs)
        {
            Logger.WriteDebug("GetConfigJS returned: {0}", configJs);
            var keysAndValues = ReadApplicationSettingsFromDatabase();

            foreach (var keyValue in keysAndValues)
            {
                string searchTerm = I18NHelper.FormatInvariant("'{0}':'{1}'", keyValue.Key, keyValue.Value);
                Logger.WriteDebug("Checking if Config JS contains {0} ...", searchTerm);

                Assert.That(configJs.Contains(searchTerm),
                    "The expected string {0} wasn't found in the returned Config JS text!", searchTerm);
            }
        }

        /// <summary>
        /// Reads the specified number of records from the [dbo].[ApplicationSettings] table.
        /// Used to compare against what was returned by GetConfigJS().
        /// </summary>
        /// <param name="numberOfRecords">(optional) The max number of records to return.</param>
        /// <returns>A dictionary of key/value strings contained in the [dbo].[ApplicationSettings] table.</returns>
        private static Dictionary<string, string> ReadApplicationSettingsFromDatabase(int numberOfRecords = 200)
        {
            const string LOCALE = "en-US";
            var keysAndValues = new Dictionary<string, string>();

            using (var database = DatabaseFactory.CreateDatabase())
            {
                string query = I18NHelper.FormatInvariant("SELECT TOP {0} * FROM [dbo].[ApplicationSettings]",
                    numberOfRecords, LOCALE);
                Logger.WriteDebug("Running: {0}", query);

                using (var cmd = database.CreateSqlCommand(query))
                {
                    database.Open();

                    try
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    int keyIdOrdinal = reader.GetOrdinal("Key");
                                    string key = (string)reader.GetSqlString(keyIdOrdinal);

                                    int valueIdOrdinal = reader.GetOrdinal("Value");
                                    string value = (string)reader.GetSqlString(valueIdOrdinal);

                                    keysAndValues.Add(key.Replace("\\'", "'"), value.Replace("\\'", "'"));
                                }
                            }
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        Logger.WriteError("SQL query didn't get processed. Exception details = {0}", ex);
                    }
                }
            }

            return keysAndValues;
        }
    }
}
