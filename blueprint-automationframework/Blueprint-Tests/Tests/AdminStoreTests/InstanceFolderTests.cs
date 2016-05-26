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
    public static class InstanceFolderTests
    {
        private const int defaultFolderId = 1;
        private const int nonExistingFolder = 99;

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
                helper.AdminStore.GetFolderOrItsChildrenById(defaultFolderId, session, expectedCodeslist, false, hasChildren);
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
                /*FOLDER INSTANCE 99 DOESN'T EXIST*/
                helper.AdminStore.GetFolderOrItsChildrenById(nonExistingFolder, session, expectedCodeslist, badKey: false);
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
                helper.AdminStore.GetFolderOrItsChildrenById(defaultFolderId, session, expectedCodeslist, hasChildren);
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
        public static void GetFolderOrChildren_BadRequest(bool hasChildren)
        {
            List<HttpStatusCode> expectedCodeslist = new List<HttpStatusCode>();
            expectedCodeslist.Add(HttpStatusCode.BadRequest);

            /*Executes get folder REST call and returns HTTP code*/
            /*CURRENTLY, DUE TO INABILITY TO CREATE INSTANCE FOLDERS ONLY ROOT (BLUEPRINT) FOLDER USED WITH ONLY ONE PROJECT IN IT*/
            using (TestHelper helper = new TestHelper())
            {
                helper.AdminStore.GetFolderOrItsChildrenById(defaultFolderId, null, expectedCodeslist, true, hasChildren);
            }
        }

    }
}
