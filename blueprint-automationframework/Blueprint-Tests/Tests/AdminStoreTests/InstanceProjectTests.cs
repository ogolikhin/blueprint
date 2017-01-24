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
    public class InstanceProjectTests : TestBase
    {
        private const int DEFAULT_FOLDER_ID = 1;
        private const int NON_EXISTING_FOLDER_ID = int.MaxValue;
        private readonly string UNAUTHORIZED_TOKEN = new Guid().ToString();

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
        [TestRail(123258)]
        [Description("Gets an existing project and verify 200 OK is returned.")]
        public void GetProjectById_OK()
        {
            /*Executes get project REST call and returns HTTP code*/
            /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) USED */
            Helper.AdminStore.GetProjectById(DEFAULT_FOLDER_ID, _user);
        }

        [TestCase]
        [TestRail(123269)]
        [Description("Gets a non-existing project and verifies '404 Not Found' is returned.")]
        public void GetNonExistingProjectById_NotFound()
        {
            Assert.Throws<Http404NotFoundException>(() =>
            {
                /*Executes get project REST call and returns HTTP code*/
                Helper.AdminStore.GetProjectById(NON_EXISTING_FOLDER_ID, _user);
            }, "AdminStore should return a 404 Not Found error when trying to call non existing project");
        }

        [TestCase]
        [TestRail(123271)]
        [Description("Gets an existing project but sends an unauthorized token and verifies '401 Unauthorized' is returned.")]
        public void GetProjectById_SendUnauthorizedToken_Unauthorized()
        {
            // Get a valid Access Control token for the user (for the new REST calls).
            _user.SetToken(UNAUTHORIZED_TOKEN);

            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                /*Executes get project REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) IS USED */
                Helper.AdminStore.GetProjectById(DEFAULT_FOLDER_ID, _user);
            }, "AdminStore should return a 401 Unauthorized error when trying to call with expired token");
        }

        [TestCase]
        [TestRail(123272)]
        [Description("Gets an existing project but doesn't send any token header field and verifies '401 Unauthorized' is returned.")]
        public void GetProjectById_NoTokenHeader_401Unauthorized()
        {
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                /*Executes get project REST call and returns HTTP code*/
                /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) IS USED */
                Helper.AdminStore.GetProjectById(DEFAULT_FOLDER_ID);
            }, "AdminStore should return a 401 Unauthorized error when trying to call without session token");
        }
    }
}
