using Common;
using CustomAttributes;
using Helper;
using Model;
using NUnit.Framework;
using System;
using TestCommon;
using Utilities;

namespace AdminStoreTests
{
    [TestFixture]
    [Category(Categories.AdminStore)]
    public class InstanceProjectTests : TestBase
    {
        private const int DEFAULT_PROJECT_ID = 1;
        private const int NON_EXISTING_PROJECT_ID = int.MaxValue;
        private const string PATH_INSTANCEPROJECTBYID = RestPaths.Svc.AdminStore.Instance.PROJECTS_id_;
        private readonly string UNAUTHORIZED_TOKEN = new Guid().ToString();

        private IUser _adminUser = null;

        #region Setup and Cleanup

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region 200 OK Tests

        [TestCase]
        [TestRail(123258)]
        [Description("Gets an existing project and verify that 200 OK is returned with expected project.")]
        public void GetProjectById_VerifyGetProjectResult()
        {
            // Setup:
            /*Executes get project REST call and returns HTTP code*/
            /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) USED */

            // Execute:
            IProject returnedProject = null;
            Assert.DoesNotThrow(() => returnedProject = Helper.AdminStore.GetProjectById(DEFAULT_PROJECT_ID, _adminUser),
                "GET {0} with project Id {1} failed.", PATH_INSTANCEPROJECTBYID, DEFAULT_PROJECT_ID);

            // Verify:
            Assert.AreEqual(DEFAULT_PROJECT_ID, returnedProject.Id, "Project Id {0} was expected but {1} was returned from the returned project.",
                DEFAULT_PROJECT_ID, returnedProject.Id);
        }

        #endregion 200 OK Tests

        #region 401 Unauthorized Tests

        [TestCase]
        [TestRail(123271)]
        [Description("Gets an existing project but sends an unauthorized token and verifies '401 Unauthorized' is returned.")]
        public void GetProjectById_SendUnauthorizedToken_401Unauthorized()
        {
            // Setup: Get a valid Access Control token for the user (for the new REST calls).
            /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) IS USED */
            _adminUser.SetToken(UNAUTHORIZED_TOKEN);

            // Execute and Verify:
            Assert.Throws<Http401UnauthorizedException>(() => Helper.AdminStore.GetProjectById(DEFAULT_PROJECT_ID, _adminUser),
                "AdminStore should return a 401 Unauthorized error when trying to call with expired token");
        }

        [TestCase]
        [TestRail(123272)]
        [Description("Gets an existing project but doesn't send any token header field and verifies '401 Unauthorized' is returned.")]
        public void GetProjectById_NoTokenHeader_401Unauthorized()
        {
            // Setup:
            /*CURRENTLY, DUE TO INABILITY TO CREATE POJECT ONLY, EXISTING PROJECT (id = 1) IS USED */

            // Execute and Verify:
            Assert.Throws<Http401UnauthorizedException>(() =>  Helper.AdminStore.GetProjectById(DEFAULT_PROJECT_ID),
                "AdminStore should return a 401 Unauthorized error when trying to call without session token");
        }

        #endregion 401 Unauthorized Tests

        #region 404 Not Found Tests

        [TestCase]
        [TestRail(123269)]
        [Description("Gets a non-existing project and verifies '404 Not Found' is returned.")]
        public void GetProjectById_NonExistingProjecId_404NotFound()
        {
            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.AdminStore.GetProjectById(NON_EXISTING_PROJECT_ID, _adminUser),
                "AdminStore should return a 404 Not Found error when trying to call non existing project");

            // Verify:
            string expectedMessage = I18NHelper.FormatInvariant("Project (Id:{0}) is not found.", NON_EXISTING_PROJECT_ID);

            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.ResourceNotFound, expectedMessage);
        }

        #endregion 404 Not Found Tests

    }
}
