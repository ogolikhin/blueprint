using NUnit.Framework;
using CustomAttributes;
using Model;
using Helper;
using TestCommon;
using Model.Factories;
using System.Collections.Generic;
using Common;

namespace AdminStoreTests
{
    [TestFixture]
    [Category(Categories.AdminStore)]
    public class SettingsTests : TestBase
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

        [Test]//currently method returns empty dictionary
        public void GetSettings_OK()
        {
            Assert.DoesNotThrow(() =>
            {
                Helper.AdminStore.GetSettings(_user);
            });
        }

        [Test]//currently method is under development
        public void GetConfigJS_OK() // TODO: add check for returned content
        {
            Assert.DoesNotThrow(() =>
            {
                const string locale = "en-US";
                const int TEXT_RECORDS_TO_COMPARE = 6;
                string[] textValues = new string[TEXT_RECORDS_TO_COMPARE];

                string json = _adminStore.GetConfigJs(null);
                Logger.WriteDebug("Running: {0}", json);

                using (var database = DatabaseFactory.CreateDatabase("AdminStore"))
                {
                    string query = "SELECT TOP " + System.Convert.ToString(TEXT_RECORDS_TO_COMPARE, null) + " * FROM [dbo].[ApplicationLabels] WHERE Locale = \'" + locale + "\'";
                    Logger.WriteDebug("Running: {0}", query);
                    using (var cmd = database.CreateSqlCommand(query))
                    {
                        database.Open();

                        try
                        {
                            System.Data.SqlClient.SqlDataReader reader;
                            using (reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    int i = 0;
                                    while (reader.Read())
                                    {
                                        int textIdOrdinal = reader.GetOrdinal("Text");
                                        textValues[i] = "\'" + (string)reader.GetSqlString(textIdOrdinal) + "\'";
                                        Assert.True(json.Contains(textValues[i]));
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

                Helper.AdminStore.GetConfigJs(_user);
            });
        }
    }
}
