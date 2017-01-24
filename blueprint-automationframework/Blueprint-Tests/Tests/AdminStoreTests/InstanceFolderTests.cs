using System;
using NUnit.Framework;
using CustomAttributes;
using Helper;
using Model;
using TestCommon;
using Utilities;

namespace AdminStoreTests
{
    [TestFixture]
    [Category(Categories.AdminStore)]
    public class InstanceFolderTests : TestBase
    {
        private const int DEFAULT_FOLDER_ID = 1;
        private const int NON_EXISTING_FOLDER_ID = int.MaxValue;

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
        [TestRail(134645)]
        [Description("Gets an existing folder and returns 'OK' if successful")]
        public void GetFolderById_OK()
        {
            Assert.DoesNotThrow(() =>
            {
                /*Executes get folder REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE INSTANCE FOLDERS ONLY ROOT (BLUEPRINT) FOLDER USED WITH ONLY ONE PROJECT IN IT*/
                Helper.AdminStore.GetFolderById(DEFAULT_FOLDER_ID, _user);
            }, "AdminStore should return a 200 OK for an Instance Folder that exists.");
        }


        [TestCase]
        [TestRail(119382)]
        [Description("Get children of an existing folder and verify 200 OK is returned.")]
        public void GetFolderChildrenById_OK()
        {
           Assert.DoesNotThrow(() =>
            {
                /*Executes get folder children REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE INSTANCE FOLDERS ONLY ROOT (BLUEPRINT) FOLDER USED WITH ONLY ONE PROJECT IN IT*/
                Helper.AdminStore.GetFolderChildrenByFolderId(DEFAULT_FOLDER_ID, _user);
            }, "AdminStore should return a 200 OK for an Instance Folder that exists.");
        }

        [TestCase]
        [TestRail(119383)]
        [Description("Get a non-existing folder and verify it returns '404 Not Found'.")]
        public void GetNonExistingFolder_NotFound()
        {
            Assert.Throws<Http404NotFoundException>(() =>
            {
                /*Executes get folder REST call and returns HTTP code*/
                /*FOLDER INSTANCE int.MaxValue DOESN'T EXIST*/
                Helper.AdminStore.GetFolderById(NON_EXISTING_FOLDER_ID, _user);
            }, "AdminStore should return a 404 Not Found error when trying to get a non-existing Instance Folder.");
        }

        [TestCase]
        [TestRail(145865)]
        [Explicit(IgnoreReasons.ProductBug)]    // Bug 1130:  Returns 200 with an empty list instead of 404.
        [Description("Get children of a non-existing folder and verify it returns '404 Not Found'.")]
        public void GetFolderChildrenByNonExistingFolderId_NotFound()
        {
            Assert.Throws<Http404NotFoundException>(() =>
            {
                /*Executes get folder REST call and returns HTTP code*/
                /*FOLDER INSTANCE int.MaxValue DOESN'T EXIST*/
                Helper.AdminStore.GetFolderChildrenByFolderId(NON_EXISTING_FOLDER_ID, _user);
            }, "AdminStore should return a 404 Not Found error when trying to get children of a non-existing Instance Folder.");
        }

        [TestCase]
        [TestRail(119384)]
        [Description("Gets the folder using an unauthorized token and verifies '401 Unauthorized' is returned.")]
        public void GetFolderById_SendUnauthorizedToken_Unauthorized()
        {
            _user.SetToken(CommonConstants.InvalidToken);

            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                /*Executes get folder REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE INSTANCE FOLDERS ONLY ROOT (BLUEPRINT) FOLDER USED WITH ONLY ONE PROJECT IN IT*/
                Helper.AdminStore.GetFolderById(DEFAULT_FOLDER_ID, _user);
            }, "AdminStore should return a 401 Unauthorized error when trying to use expired session token");
        }

        [TestCase]
        [TestRail(145862)]
        [Description("Gets the children of the folder using an unauthorized token and verifies '401 Unauthorized' is returned.")]
        public void GetFolderChildrenByFolderId_SendUnauthorizedToken_Unauthorized()
        {
            // Get a valid Access Control token for the user (for the new REST calls).
            _user.SetToken(CommonConstants.InvalidToken);

            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                /*Executes get folder children REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE INSTANCE FOLDERS ONLY ROOT (BLUEPRINT) FOLDER USED WITH ONLY ONE PROJECT IN IT*/
                Helper.AdminStore.GetFolderChildrenByFolderId(DEFAULT_FOLDER_ID, _user);
            }, "AdminStore should return a 401 Unauthorized error when trying to use expired session token");
        }

        [TestCase]
        [TestRail(119385)]
        [Description("Gets the folder without sending any token header and verifies '401 Unauthorized' is returned.")]
        public void GetFolderById_NoTokenHeader_401Unauthorized()
        {
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                /*Executes get folder REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE INSTANCE FOLDERS ONLY ROOT (BLUEPRINT) FOLDER USED WITH ONLY ONE PROJECT IN IT*/
                Helper.AdminStore.GetFolderById(DEFAULT_FOLDER_ID);
            }, "AdminStore should return a 401 Unauthorized error when trying to send a malformed request");
        }

        [TestCase]
        [TestRail(145864)]
        [Description("Gets the children of a folder without sending any token header and verifies '401 Unauthorized' is returned.")]
        public void GetFolderChildrenByFolderId_NoTokenHeader_401Unauthorized()
        {
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                /*Executes get folder children REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE INSTANCE FOLDERS ONLY ROOT (BLUEPRINT) FOLDER USED WITH ONLY ONE PROJECT IN IT*/
                Helper.AdminStore.GetFolderChildrenByFolderId(DEFAULT_FOLDER_ID);
            }, "AdminStore should return a 401 Unauthorized error when trying to send a malformed request");
        }
    }
}
