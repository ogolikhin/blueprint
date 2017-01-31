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

namespace AdminStoreTests
{
    // TODO: Exsiting default integration project is used since there is no API call available to create project at this point
    [TestFixture]
    [Category(Categories.AdminStore)]
    public class InstanceProjectTests : TestBase
    {
        private const int NON_EXISTING_PROJECT_ID = int.MaxValue;
        private const string PATH_INSTANCEPROJECTBYID = RestPaths.Svc.AdminStore.Instance.PROJECTS_id_;

        private IUser _adminUser = null;
        //private IProject _project = null;
        private List<IProject> _allProjects = null;
        #region Setup and Cleanup

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            //_project = ProjectFactory.GetProject(_adminUser);
            _allProjects = ProjectFactory.GetAllProjects(_adminUser);
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
            // Setup: Not required.

            // Execute and Verify: GetProject for all existing projects
            foreach(var project in _allProjects)
            {
                // Execute:
                InstanceProject returnedInstanceProject = null;
                Assert.DoesNotThrow(() => returnedInstanceProject = Helper.AdminStore.GetProjectById(project.Id, _adminUser),
                    "GET {0} with project Id {1} failed.", PATH_INSTANCEPROJECTBYID, project.Id);

                // Verify:
                AdminStoreHelper.GetProjectByIdValidation(Helper, _adminUser, project, returnedInstanceProject);
            }
        }

        #endregion 200 OK Tests

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
        public void GetProjectById_UsingUserWithNoPermissionToProject_403Frobidden()
        {
            // Setup: Create a user that doesn't have access to the default project
            var userWithNoPermissionToProject = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.None, _allProjects.First());

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.AdminStore.GetProjectById(_allProjects.First().Id, userWithNoPermissionToProject),
                "GET {0} using the user with no permission to the project with Id {1} should return a 403 Forbidden.", PATH_INSTANCEPROJECTBYID, _allProjects.First().Id);

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
