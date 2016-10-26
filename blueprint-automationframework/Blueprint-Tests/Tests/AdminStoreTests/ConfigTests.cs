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
        private IUser _user = null;

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
        [Description("Run:  GET /config/settings  but don't include any Session-Token header.  Verify it returns 400 Bad Request.")]
        public void GetSettings_MissingTokenHeader_400BadRequest()
        {
            Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.AdminStore.GetSettings(session: null);
            }, "GetSettings() should return 400 Bad Request if no Session-Token header is provided!");
        }

        [TestCase]
        [TestRail(146300)]
        [Description("Run:  GET /config/settings  and pass an invalid token.  Verify it returns 401 Unauthorized.")]
        public void GetSettings_InvalidToken_401Unauthorized()
        {
            IUser user = UserFactory.CreateUserOnly();
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
            Logger.WriteDebug("GetConfigJS returned: {0}", configJs);
            var textValues = ReadApplicationLabelsFromDatabase();

            foreach (string value in textValues)
            {
                Logger.WriteDebug("Checking if JSON contains '{0}'...", value);

                // Note: We need to replace ' with \' because single quotes are escaped in JSON.
                Assert.That(configJs.Contains(value.Replace("'", "\\'")),
                    "The expected string '{0}' wasn't found in the returned JSON text!", value);
            }
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
            Logger.WriteDebug("GetConfigJS returned: {0}", configJs);
            var textValues = ReadApplicationLabelsFromDatabase();

            foreach (string value in textValues)
            {
                Logger.WriteDebug("Checking if JSON contains '{0}'...", value);

                // Note: We need to replace ' with \' because single quotes are escaped in JSON.
                Assert.That(configJs.Contains(value.Replace("'", "\\'")),
                    "The expected string '{0}' wasn't found in the returned JSON text!", value);
            }
        }

        /// <summary>
        /// Reads the specified number of records from the [dbo].[ApplicationLabels] table.
        /// Used to compare against what was returned by GetConfigJS().
        /// </summary>
        /// <param name="numberOfRecords">(optional) The max number of records to return.</param>
        /// <returns>A list of strings contained in the [dbo].[ApplicationLabels] table.</returns>
        private static List<string> ReadApplicationLabelsFromDatabase(int numberOfRecords = 200)
        {
            const string LOCALE = "en-US";
            var textValues = new List<string>();

            using (var database = DatabaseFactory.CreateDatabase("AdminStore"))
            {
                string query = I18NHelper.FormatInvariant("SELECT TOP {0} * FROM [dbo].[ApplicationLabels] WHERE Locale = '{1}'",
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
                                    int textIdOrdinal = reader.GetOrdinal("Text");
                                    string rawValue = (string)reader.GetSqlString(textIdOrdinal);
                                    textValues.Add(rawValue.Replace("\\'", "'"));
                                }
                            }
                        }
                    }
                    catch (System.InvalidOperationException ex)
                    {
                        Logger.WriteError("SQL query didn't get processed. Exception details = {0}", ex);
                    }
                }
            }

            return textValues;
        }
    }
}
