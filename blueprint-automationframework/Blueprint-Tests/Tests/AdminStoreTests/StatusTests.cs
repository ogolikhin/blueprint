using System.Collections.Generic;
using NUnit.Framework;
using CustomAttributes;
using Helper;
using Utilities;
using System.Net;
using TestCommon;
using Model;
using Model.Factories;

namespace AdminStoreTests
{
    [TestFixture]
    [Category(Categories.AdminStore)]
    public static class StatusTests
    {
        private const int defaultFolderId = 1;
        private const int nonExistingFolder = 99;

        [TestCase]
        [Description("Calls the /status endpoint for AdminStore with a valid preAuthorizedKey and verifies that it returns 200 OK and returns the proper data content.")]
        public static void Status_ValidateReturnedContent()
        {
            using (TestHelper helper = new TestHelper())
            {
            string content = null;

            Assert.DoesNotThrow(() =>
            {
                    content = helper.AdminStore.GetStatus();
            }, "The GET /status endpoint should return 200 OK!");

                var extraExpectedStrings = new List<string> {"AdminStore", "AdminStorage", "RaptorDB"};

            CommonServiceHelper.ValidateStatusResponseContent(content, extraExpectedStrings);
        }
        }

        [TestCase(null)]
        [TestCase("ABCDEFG123456")]
        [Description("Calls the /status endpoint for AdminStore and passes invalid preAuthorizedKey values.  Verifies that it returns a 401 error.")]
        public static void StatusWithBadKeys_Expect401Unauthorized(string preAuthorizedKey)
        {
            using (TestHelper helper = new TestHelper())
        {
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                    helper.AdminStore.GetStatus(preAuthorizedKey);
            }, "The GET /status endpoint should return 401 Unauthorized when we pass an invalid or missing preAuthorizedKey!");
        }
        }

        [TestCase]
        [Description("Calls the /status/upcheck endpoint for AdminStore and verifies that it returns 200 OK")]
        public static void GetStatusUpcheck_OK()
        {
            using (TestHelper helper = new TestHelper())
        {
            Assert.DoesNotThrow(() =>
            {
                    helper.AdminStore.GetStatusUpcheck();
                }, "'GET /status/upcheck' should return 200 OK.");
            }
        }

        // TODO: Move these GetFolder tests to a new file, since they have nothing to do with status.
        [TestCase(false)]
        [TestCase(true)]
        [TestRail(119382)]
        [Description("Gets the folder or folder children and returns 'OK' if successful")]
        public static void GetFolderOrChildren_OK(bool hasChildren)
        {
            IUser _user = UserFactory.CreateUserAndAddToDatabase();

            // Get a valid Access Control token for the user (for the new REST calls).
            using (TestHelper helper = new TestHelper())
            {
                ISession session = helper.AdminStore.AddSession(_user.Username, _user.Password);
            
                List<HttpStatusCode> expectedCodeslist = new List<HttpStatusCode>();
                expectedCodeslist.Add(HttpStatusCode.OK);

                /*Executes get folder REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE INSTANCE FOLDERS ONLY ROOT (BLUEPRINT) FOLDER USED WITH ONLY ONE PROJECT IN IT*/
                helper.AdminStore.GetReturnCodeFromFolderOrItsChildrenRequest(defaultFolderId, session, expectedCodeslist, hasChildren);
            }
            
            if (_user != null)
            {
                _user.DeleteUser();
                _user = null;
            }
        }

        [TestCase]
        [TestRail(119383)]
        [Description("Gets the folder and returns 'Not Found' if successfull")]
        public static void GetFolder_NotFound()
        {
            IUser _user = UserFactory.CreateUserAndAddToDatabase();

            // Get a valid Access Control token for the user (for the new REST calls).
            using (TestHelper helper = new TestHelper())
            {
                ISession session = helper.AdminStore.AddSession(_user.Username, _user.Password);

                List<HttpStatusCode> expectedCodeslist = new List<HttpStatusCode>();
                expectedCodeslist.Add(HttpStatusCode.NotFound);

                /*Executes get folder REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE INSTANCE FOLDERS ONLY ROOT (BLUEPRINT) FOLDER EXISTS. INSTANCE 99 DOESN'T EXIST*/
                helper.AdminStore.GetReturnCodeFromFolderOrItsChildrenRequest(nonExistingFolder, session, expectedCodeslist, false);
            }

            if (_user != null)
            {
                _user.DeleteUser();
                _user = null;
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        [TestRail(119384)]
        [Description("Gets the folder or folder children and returns 'Unauthorized' if successful")]
        public static void GetFolderOrChildren_Unauthorized(bool hasChildren)
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

                /*Executes get folder REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE INSTANCE FOLDERS ONLY ROOT (BLUEPRINT) FOLDER USED WITH ONLY ONE PROJECT IN IT*/
                helper.AdminStore.GetReturnCodeFromFolderOrItsChildrenRequest(defaultFolderId, session, expectedCodeslist, hasChildren);
            }

            if (_user != null)
            {
                _user.DeleteUser();
                _user = null;
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        [TestRail(119385)]
        [Description("Gets the folder or folder children and returns 'Bad Request' if successful")]
        public static void GetFolderOrChildren_BadRequestd(bool hasChildren)
        {
            List<HttpStatusCode> expectedCodeslist = new List<HttpStatusCode>();
            expectedCodeslist.Add(HttpStatusCode.BadRequest);

            /*Executes get folder REST call and returns HTTP code*/
            /*CURRENTLY, DUE TO INABILITY TO CREATE INSTANCE FOLDERS ONLY ROOT (BLUEPRINT) FOLDER USED WITH ONLY ONE PROJECT IN IT*/
            using (TestHelper helper = new TestHelper())
            {
                helper.AdminStore.GetReturnCodeFromFolderOrItsChildrenRequest(defaultFolderId, null, expectedCodeslist, hasChildren, true);
            }
        }
    }
}
