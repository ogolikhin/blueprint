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
        private const int defaultFolderId = 1;
        private const int nonExistingFolder = int.MaxValue;
        private const IUser noTokenInRequest = null;

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
        [Description("Gets the folder and returns 'OK' if successful")]
        public void GetFolderById_OK()
        {
            Assert.DoesNotThrow(() =>
            {
                /*Executes get folder REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE INSTANCE FOLDERS ONLY ROOT (BLUEPRINT) FOLDER USED WITH ONLY ONE PROJECT IN IT*/
                Helper.AdminStore.GetFolderById(defaultFolderId, _user);
            }, "AdminStore should return a 200 OK for the folder that exists");
        }


        [TestCase]
        [TestRail(119382)]
        [Description("Gets the folder children and returns 'OK' if successful")]
        public void GetFolderChildrenById_OK()
        {
           Assert.DoesNotThrow(() =>
            {
                /*Executes get folder children REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE INSTANCE FOLDERS ONLY ROOT (BLUEPRINT) FOLDER USED WITH ONLY ONE PROJECT IN IT*/
                Helper.AdminStore.GetFolderChildrenByFolderId(defaultFolderId, _user);
            }, "AdminStore should return a 200 OK for the folder that exists");
        }

        [TestCase]
        [TestRail(119383)]
        [Description("Gets the folder and returns 'Not Found' if successfull")]
        public void GetFolder_NotFound()
        {
            Assert.Throws<Http404NotFoundException>(() =>
            {
                /*Executes get folder REST call and returns HTTP code*/
                /*FOLDER INSTANCE int.MaxValue DOESN'T EXIST*/
                Helper.AdminStore.GetFolderById(nonExistingFolder, _user);
            }, "AdminStore should return a 404 Not Found error when trying to get a non-existing Instance");
        }

        [TestCase]
        [TestRail(119384)]
        [Description("Gets the folder or folder children and returns 'Unauthorized' if successful")]
        public void GetFolderById_Unauthorized()
        {
            _user.SetToken("CD4351BF-0162-4AB9-BA80-1A932D94CF7F");

            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                /*Executes get folder REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE INSTANCE FOLDERS ONLY ROOT (BLUEPRINT) FOLDER USED WITH ONLY ONE PROJECT IN IT*/
                Helper.AdminStore.GetFolderById(defaultFolderId, _user);
            }, "AdminStore should return a 401 Unauthorized error when trying to use expired session token");
        }

        [TestCase]
        [TestRail(119384)]
        [Description("Gets the folder or folder children and returns 'Unauthorized' if successful")]
        public void GetFolderChildrenByFolderId_Unauthorized()
        {
            // Get a valid Access Control token for the user (for the new REST calls).
            _user.SetToken("CD4351BF-0162-4AB9-BA80-1A932D94CF7F");

            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                /*Executes get folder children REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE INSTANCE FOLDERS ONLY ROOT (BLUEPRINT) FOLDER USED WITH ONLY ONE PROJECT IN IT*/
                Helper.AdminStore.GetFolderChildrenByFolderId(defaultFolderId, _user);
            }, "AdminStore should return a 401 Unauthorized error when trying to use expired session token");
        }

        [TestCase]
        [TestRail(119385)]
        [Description("Gets the folder or folder children and returns 'Bad Request' if successful")]
        public void GetFolderById_BadRequest()
        {
            Assert.Throws<Http400BadRequestException>(() =>
            {
                /*Executes get folder REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE INSTANCE FOLDERS ONLY ROOT (BLUEPRINT) FOLDER USED WITH ONLY ONE PROJECT IN IT*/
                Helper.AdminStore.GetFolderById(defaultFolderId, noTokenInRequest);
            }, "AdminStore should return a 400 Bad Request error when trying to send a malformed request");
        }

        [TestCase]
        [TestRail(119385)]
        [Description("Gets the folder or folder children and returns 'Bad Request' if successful")]
        public void GetFolderOrChildren_BadRequest()
        {
            Assert.Throws<Http400BadRequestException>(() =>
            {
                /*Executes get folder children REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE INSTANCE FOLDERS ONLY ROOT (BLUEPRINT) FOLDER USED WITH ONLY ONE PROJECT IN IT*/
                Helper.AdminStore.GetFolderChildrenByFolderId(defaultFolderId, noTokenInRequest);
            }, "AdminStore should return a 400 Bad Request error when trying to send a malformed request");
        }

    }
}
