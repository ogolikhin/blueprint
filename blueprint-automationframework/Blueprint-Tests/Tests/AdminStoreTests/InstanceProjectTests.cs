using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.Impl;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TestCommon;
using Utilities;
using Utilities.Facades;

namespace AdminStoreTests
{
    // TODO: Existing default integration project is used since there is no API call available to create project at this point
    [TestFixture]
    [Category(Categories.AdminStore)]
    public class InstanceProjectTests : TestBase
    {
        private const int NON_EXISTING_PROJECT_ID = int.MaxValue;
        private const string INSTANCEPROJECTBYID_PATH = RestPaths.Svc.AdminStore.Instance.PROJECTS_id_;

        private IUser _adminUser = null;
        private List<IProject> _allProjects = null;
        #region Setup and Cleanup

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _allProjects = ProjectFactory.GetAllProjects(_adminUser);
            foreach(var project in _allProjects)
            {
                Helper.AssignProjectRolePermissionsToUser(
                _adminUser,
                RolePermissions.Read |
                RolePermissions.Edit |
                RolePermissions.Delete |
                RolePermissions.Trace |
                RolePermissions.Comment |
                RolePermissions.StealLock |
                RolePermissions.CanReport |
                RolePermissions.Share |
                RolePermissions.Reuse |
                RolePermissions.ExcelUpdate |
                RolePermissions.DeleteAnyComment |
                RolePermissions.CreateRapidReview,
                project);
            }
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
        [Description("Gets all available projects and verify that 200 OK is returned with expected project.")]
        public void GetProjectById_GetAllAvailableProjects_VerifyGetProjectResult()
        {
            // Setup: Not required.

            // Execute and Verify: GetProject for all existing projects
            foreach(var project in _allProjects)
            {
                // Execute:
                InstanceProject returnedInstanceProject = null;
                Assert.DoesNotThrow(() => returnedInstanceProject = Helper.AdminStore.GetProjectById(project.Id, _adminUser),
                    "GET {0} with project Id {1} failed.", INSTANCEPROJECTBYID_PATH, project.Id);

                // Verify:
                AdminStoreHelper.AssertAreEqual(Helper, project, returnedInstanceProject);
            }
        }

        #endregion 200 OK Tests

        #region 400 Bad Request Tests

        [TestCase("*")]
        [TestCase("&")]
        [TestRail(246556)]
        [Description("Get a project using the invalid URL containing a special charactor. Verify that 400 bad request is returned.")]
        public void GetProjectById_SendInvalidUrl_400BadRequest(string invalidCharactor)
        {
            // Setup:
            string invalidPath = I18NHelper.FormatInvariant(INSTANCEPROJECTBYID_PATH, invalidCharactor + _allProjects.First().Id);

            var restApi = new RestApiFacade(Helper.ArtifactStore.Address, _adminUser?.Token?.AccessControlToken);

            // Execute & Verify:
            var ex = Assert.Throws<Http400BadRequestException>(() => restApi.SendRequestAndDeserializeObject<InstanceProject>(
                invalidPath,
                RestRequestMethod.GET,
                shouldControlJsonChanges: true
                ),
                "GET {0} call should return a 400 Bad Request exception when trying with invalid URL.", INSTANCEPROJECTBYID_PATH);

            // Verify:
            string expectedMessage = I18NHelper.FormatInvariant("A potentially dangerous Request.Path value was detected from the client ({0}).", invalidCharactor);

            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, expectedMessage);
        }

        #endregion 400 Bad Request Tests

        #region 401 Unauthorized Tests

        [TestCase]
        [TestRail(123271)]
        [Description("Gets an existing project but sends an unauthorized token and verifies '401 Unauthorized' is returned.")]
        public void GetProjectById_SendUnauthorizedToken_401Unauthorized()
        {
            // Setup: 
            var userWithInvalidToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() => Helper.AdminStore.GetProjectById(_allProjects.First().Id, userWithInvalidToken),
                "AdminStore should return a 401 Unauthorized error when trying to call with invalid token");

            // Verify:
            const string expectedExceptionMessage = "Token is invalid";

            StringAssert.Contains(expectedExceptionMessage, ex.RestResponse.Content,
                "{0} was not found in returned message of GET ProjectById which has invalid token", expectedExceptionMessage);
        }

        [TestCase]
        [TestRail(123272)]
        [Description("Gets an existing project but doesn't send any token header field and verifies '401 Unauthorized' is returned.")]
        public void GetProjectById_NoTokenHeader_401Unauthorized()
        {
            // Setup:

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>  Helper.AdminStore.GetProjectById(_allProjects.First().Id),
                "AdminStore should return a 401 Unauthorized error when trying to call without session token");

            // Verify:
            const string expectedExceptionMessage = "Token is missing or malformed";

            StringAssert.Contains(expectedExceptionMessage, ex.RestResponse.Content,
                "{0} was not found in returned message of GET ProjectById which has no session token.", expectedExceptionMessage);
        }

        #endregion 401 Unauthorized Tests

        #region 403 Forbidden Tests

        [TestCase]
        [TestRail(234444)]
        [Description("Gets an existing project with the user doesn't have permission to the project and verify that it returns 403 Forbidden response.")]
        public void GetProjectById_UsingUserWithNoPermissionToProject_403Forbidden()
        {
            // Setup: Create a user that doesn't have access to the default project
            var userWithNoPermissionToProject = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.None, _allProjects.First());

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.AdminStore.GetProjectById(_allProjects.First().Id, userWithNoPermissionToProject),
                "GET {0} using the user with no permission to the project with Id {1} should return a 403 Forbidden.", INSTANCEPROJECTBYID_PATH, _allProjects.First().Id);

            // Verify:
            var expectedMessage = I18NHelper.FormatInvariant("The user does not have permissions for Project (Id:{0}).", _allProjects.First().Id) ;

            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.UnauthorizedAccess, expectedMessage);
        }

        #endregion 403 Forbidden Tests

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
