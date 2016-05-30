using System.Collections.Generic;
using NUnit.Framework;
using CustomAttributes;
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
        private const int nonExistingFolder = int.MaxValue;
        private const object noTokenInRequest = null;

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

                Assert.DoesNotThrow(() =>
                {
                    /*Executes get folder or its children REST call and returns HTTP code*/
                    /*CURRENTLY, DUE TO INABILITY TO CREATE INSTANCE FOLDERS ONLY ROOT (BLUEPRINT) FOLDER USED WITH ONLY ONE PROJECT IN IT*/
                    helper.AdminStore.GetFolderOrItsChildrenById(defaultFolderId, _user, expectedCodesList, hasChildren);
                }, "AdminStore should return a 200 OK for the project that exists");
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

                Assert.DoesNotThrow(() =>
                {
                    /*Executes get folder or its children REST call and returns HTTP code*/
                    /*FOLDER INSTANCE int.MaxValue DOESN'T EXIST*/
                    helper.AdminStore.GetFolderOrItsChildrenById(nonExistingFolder, _user, expectedCodesList);
                }, "AdminStore should return a 404 Not Found error when trying to get a non-existing Instance");
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

                Assert.DoesNotThrow(() =>
                {
                    /*Executes get folder or its children REST call and returns HTTP code*/
                    /*CURRENTLY, DUE TO INABILITY TO CREATE INSTANCE FOLDERS ONLY ROOT (BLUEPRINT) FOLDER USED WITH ONLY ONE PROJECT IN IT*/
                    helper.AdminStore.GetFolderOrItsChildrenById(defaultFolderId, _user, expectedCodesList, hasChildren);
                }, "AdminStore should return a 401 Unauthorized error when trying to use expired session token");
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

            /*Executes get folder or its children REST call and returns HTTP code*/
            /*CURRENTLY, DUE TO INABILITY TO CREATE INSTANCE FOLDERS ONLY ROOT (BLUEPRINT) FOLDER USED WITH ONLY ONE PROJECT IN IT*/
            using (TestHelper helper = new TestHelper())
            {
                Assert.DoesNotThrow(() =>
                {
                    helper.AdminStore.GetFolderOrItsChildrenById(defaultFolderId, noTokenInRequest, expectedCodesList, hasChildren);
                }, "AdminStore should return a 400 Bad Request error when trying to send a malformed request");
            }
        }

    }
}
