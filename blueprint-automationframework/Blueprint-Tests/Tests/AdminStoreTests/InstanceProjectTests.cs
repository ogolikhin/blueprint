using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.Impl;
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
        private const int NON_EXISTING_PROJECT_ID = int.MaxValue;
        private const string PATH_INSTANCEPROJECTBYID = RestPaths.Svc.AdminStore.Instance.PROJECTS_id_;
        private readonly string UNAUTHORIZED_TOKEN = new Guid().ToString();

        private IProject _project = null;
        private IUser _adminUser = null;

        #region Setup and Cleanup

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);
            _project = ProjectFactory.GetProject(_adminUser);
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
            /* Exsiting default integration project is used since there is no API call available to create project at this point */

            // Execute:
            InstanceProject returnedProject = null;
            Assert.DoesNotThrow(() => returnedProject = Helper.AdminStore.GetProjectById(_project.Id, _adminUser),
                "GET {0} with project Id {1} failed.", PATH_INSTANCEPROJECTBYID, _project.Id);

            // Verify:
            Assert.AreEqual(_project.Id, returnedProject.Id, "Project Id {0} was expected but {1} was returned from the returned project.",
                _project.Id, returnedProject.Id);

            ValidateaInstanceProject(returnedProject, _project);
        }

        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)]
        [TestCase]
        [TestRail(00)]
        [Description("Gets an existing project and verify that 200 OK is returned with expected project.")]
        public void GetProjectById_UsingUserWithNoPermissionToProject_VerifyGetProjectResult()
        {
            // Setup: Create a user that doesn't have access to the default project
            //var userWithNoPermissionToProject = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
            var adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            /* Exsiting default integration project is used since there is no API call available to create project at this point */

            // Execute:
            InstanceProject returnedProject = null;
            Assert.DoesNotThrow(() => returnedProject = Helper.AdminStore.GetProjectById(_project.Id, adminUser),
                "GET {0} with project Id {1} failed.", PATH_INSTANCEPROJECTBYID, _project.Id);

            // Verify:
            Assert.AreEqual(_project.Id, returnedProject.Id, "Project Id {0} was expected but {1} was returned from the returned project.",
                _project.Id, returnedProject.Id);
        }

        #endregion 200 OK Tests

        #region 401 Unauthorized Tests

        [TestCase]
        [TestRail(123271)]
        [Description("Gets an existing project but sends an unauthorized token and verifies '401 Unauthorized' is returned.")]
        public void GetProjectById_SendUnauthorizedToken_401Unauthorized()
        {
            // Setup: Get a valid Access Control token for the user (for the new REST calls).
            /* Exsiting default integration project is used since there is no API call available to create project at this point */
            _adminUser.SetToken(UNAUTHORIZED_TOKEN);

            // Execute and Verify:
            Assert.Throws<Http401UnauthorizedException>(() => Helper.AdminStore.GetProjectById(_project.Id, _adminUser),
                "AdminStore should return a 401 Unauthorized error when trying to call with expired token");
        }

        [TestCase]
        [TestRail(123272)]
        [Description("Gets an existing project but doesn't send any token header field and verifies '401 Unauthorized' is returned.")]
        public void GetProjectById_NoTokenHeader_401Unauthorized()
        {
            // Setup:
            /* Exsiting default integration project is used since there is no API call available to create project at this point */

            // Execute and Verify:
            Assert.Throws<Http401UnauthorizedException>(() =>  Helper.AdminStore.GetProjectById(_project.Id),
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

        #region private functions
        
        private static void ValidateaInstanceProject(InstanceProject instanceProject, IProject project)
        {
            ThrowIf.ArgumentNull(instanceProject, nameof(InstanceProject));
            ThrowIf.ArgumentNull(project, nameof(project));

            Assert.AreEqual(project.Id, instanceProject.Id, "Project Id {0} was expected but {1} was returned from the returned project.",
                project.Id, instanceProject.Id);

            Assert.IsNotNull(instanceProject.Name, "{0} snould not be null!", nameof(instanceProject.Name));

            Assert.IsNotNull(instanceProject.ParentFolderId, "{0} should not be null!", nameof(InstanceProject.ParentFolderId));

            Assert.AreEqual(InstanceItemTypeEnum.Project, instanceProject.Type, "Type {0} was expected but {1} was returned.", InstanceItemTypeEnum.Project, instanceProject.Type);
        }

        #endregion private functions
    }
}
