using System.Collections.Generic;
using NUnit.Framework;
using CustomAttributes;
using Model.Factories;
using Helper;
using System.Net;
using Model;
using TestCommon;

namespace AdminStoreTests
{
    [TestFixture]
    [Category(Categories.AdminStore)]
    public class InstanceFolderTests : TestBase
    {
        private const int defaultFolderId = 1;
        private const int nonExistingFolder = 99;

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

        [TestCase(false)]
        [TestCase(true)]
        [TestRail(119382)]
        [Description("Gets the folder or folder children and returns 'OK' if successful")]
        public void GetFolderOrChildren_OK(bool hasChildren)
        {
            // Get a valid Access Control token for the user (for the new REST calls).
            using (TestHelper helper = new TestHelper())
            {
                List<HttpStatusCode> expectedCodesList = new List<HttpStatusCode>();
                expectedCodesList.Add(HttpStatusCode.OK);

                /*Executes get folder REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE INSTANCE FOLDERS ONLY ROOT (BLUEPRINT) FOLDER USED WITH ONLY ONE PROJECT IN IT*/
                helper.AdminStore.GetFolderOrItsChildrenById(defaultFolderId, _user, expectedCodesList, hasChildren);
            }
        }

        [TestCase]
        [TestRail(119383)]
        [Description("Gets the folder and returns 'Not Found' if successfull")]
        public void GetFolder_NotFound()
        {
            // Get a valid Access Control token for the user (for the new REST calls).
            using (TestHelper helper = new TestHelper())
            {
                List<HttpStatusCode> expectedCodesList = new List<HttpStatusCode>();
                expectedCodesList.Add(HttpStatusCode.NotFound);

                /*Executes get folder REST call and returns HTTP code*/
                /*FOLDER INSTANCE 99 DOESN'T EXIST*/
                helper.AdminStore.GetFolderOrItsChildrenById(nonExistingFolder, _user, expectedCodesList);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        [TestRail(119384)]
        [Description("Gets the folder or folder children and returns 'Unauthorized' if successful")]
        public void GetFolderOrChildren_Unauthorized(bool hasChildren)
        {
            // Get a valid Access Control token for the user (for the new REST calls).
            using (TestHelper helper = new TestHelper())
            {
                _user.SetToken("CD4351BF-0162-4AB9-BA80-1A932D94CF7F");

                List<HttpStatusCode> expectedCodesList = new List<HttpStatusCode>();
                expectedCodesList.Add(HttpStatusCode.Unauthorized);

                /*Executes get folder REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE INSTANCE FOLDERS ONLY ROOT (BLUEPRINT) FOLDER USED WITH ONLY ONE PROJECT IN IT*/
                helper.AdminStore.GetFolderOrItsChildrenById(defaultFolderId, _user, expectedCodesList, hasChildren);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        [TestRail(119385)]
        [Description("Gets the folder or folder children and returns 'Bad Request' if successful")]
        public static void GetFolderOrChildren_BadRequest(bool hasChildren)
        {
            List<HttpStatusCode> expectedCodesList = new List<HttpStatusCode>();
            expectedCodesList.Add(HttpStatusCode.BadRequest);

            /*Executes get folder REST call and returns HTTP code*/
            /*CURRENTLY, DUE TO INABILITY TO CREATE INSTANCE FOLDERS ONLY ROOT (BLUEPRINT) FOLDER USED WITH ONLY ONE PROJECT IN IT*/
            using (TestHelper helper = new TestHelper())
            {
                helper.AdminStore.GetFolderOrItsChildrenById(defaultFolderId, null, expectedCodesList, hasChildren);
            }
        }

    }
}
