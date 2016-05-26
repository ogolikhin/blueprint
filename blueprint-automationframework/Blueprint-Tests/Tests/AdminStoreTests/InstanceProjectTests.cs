using System.Collections.Generic;
using NUnit.Framework;
using CustomAttributes;
using Model.Factories;
using Helper;
using System.Net;
using Model;

namespace AdminStoreTests
{
    [TestFixture]
    [Category(Categories.AdminStore)]
    public static class InstanceProjectTests
    {
        private const int defaultProjectId = 1;
        private const int nonExistingProject = 99;

        [TestCase]
        [TestRail(123258)]
        [Description("Gets the project and returns 'OK' if successful")]
        public static void GetProjectById_OK()
        {
            IUser _user = UserFactory.CreateUserAndAddToDatabase();

            // Get a valid Access Control token for the user (for the new REST calls).
            using (TestHelper helper = new TestHelper())
            {
                ISession session = helper.AdminStore.AddSession(_user.Username, _user.Password);

                List<HttpStatusCode> expectedCodeslist = new List<HttpStatusCode>();
                expectedCodeslist.Add(HttpStatusCode.OK);

                /*Executes get project REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) USED */
                helper.AdminStore.GetProjectById(defaultProjectId, session, expectedCodeslist, badKey:false);
            }

            if (_user != null)
            {
                _user.DeleteUser();
                _user = null;
            }
        }

        [TestCase]
        [TestRail(123269)]
        [Description("Gets the project and returns 'Not Found' if successfull")]
        public static void GetProjectById_NotFound()
        {
            IUser _user = UserFactory.CreateUserAndAddToDatabase();

            // Get a valid Access Control token for the user (for the new REST calls).
            using (TestHelper helper = new TestHelper())
            {
                ISession session = helper.AdminStore.AddSession(_user.Username, _user.Password);

                List<HttpStatusCode> expectedCodeslist = new List<HttpStatusCode>();
                expectedCodeslist.Add(HttpStatusCode.NotFound);

                /*Executes get project REST call and returns HTTP code*/
                helper.AdminStore.GetProjectById(nonExistingProject, session, expectedCodeslist, badKey: false);
            }

            if (_user != null)
            {
                _user.DeleteUser();
                _user = null;
            }
        }

        [TestCase]
        [TestRail(123271)]
        [Description("Gets the project and returns 'Unauthorized' if successful")]
        public static void GetProjectById_Unauthorized()
        {
            IUser _user = UserFactory.CreateUserAndAddToDatabase();

            // Get a valid Access Control token for the user (for the new REST calls).
            using (TestHelper helper = new TestHelper())
            {
                ISession session = helper.AdminStore.AddSession(_user.Username, _user.Password);

                using (var database = DatabaseFactory.CreateDatabase("AdminStore"))
                {
                    string query = "UPDATE [dbo].[Sessions] SET SessionId = 'CD4351BF-0162-4AB9-BA80-1A932D94CF7F' WHERE UserName = \'" + _user.Username + "\'";
                    Common.Logger.WriteDebug("Running: {0}", query);

                    using (var cmd = database.CreateSqlCommand(query))
                    {
                        database.Open();
                        try
                        {
                            Assert.IsTrue(cmd.ExecuteNonQuery() == 1);
                        }
                        catch (System.InvalidOperationException ex)
                        {
                            Common.Logger.WriteError("SQL query didn't processed. Exception details = {0}", ex);
                        }
                    }
                }

                List<HttpStatusCode> expectedCodeslist = new List<HttpStatusCode>();
                expectedCodeslist.Add(HttpStatusCode.Unauthorized);

                /*Executes get project REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) IS USED */
                helper.AdminStore.GetProjectById(defaultProjectId, session, expectedCodeslist);
            }

            if (_user != null)
            {
                _user.DeleteUser();
                _user = null;
            }
        }

        [TestCase]
        [TestRail(123272)]
        [Description("Executes Get project call and returns 'Bad Request' if successful")]
        public static void GetProjectById_BadRequest()
        {
            List<HttpStatusCode> expectedCodeslist = new List<HttpStatusCode>();
            expectedCodeslist.Add(HttpStatusCode.BadRequest);

            /*Executes get project REST call and returns HTTP code*/
            /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) IS USED */
            using (TestHelper helper = new TestHelper())
            {
                helper.AdminStore.GetProjectById(defaultProjectId, null, expectedCodeslist, badKey: true);
            }
        }
    }
}
