using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using NUnit.Framework;
using System.Collections.Generic;
using Common;
using TestCommon;
using Utilities;

namespace AdminStoreTests
{
    [TestFixture]
    [Category(Categories.AdminStore)]
    public class ProjectNavigationPathTests : TestBase
    {
        private IUser _adminUser = null;
        private IUser _viewerUser = null;
        private IProject _firstProject = null;
        private IProject _secondProject = null;
        private List<IProject> _projects = null;

        private const string ROOT_FOLDER_NAME = "Blueprint";

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _projects = ProjectFactory.GetProjects(_adminUser, numberOfProjects: 2, shouldRetrievePropertyTypes: false);

            _firstProject = _projects[0];
            _secondProject = _projects[1];
            _viewerUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _firstProject);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        [TestCase]
        [TestRail(191082)]
        [Description("Get Project Navigation Path for project, user with Viewer access, verify that path has expected value.")]
        public void GetProjectNavigation_ValidProject_ReturnsExpectedPath()
        {
            // Setup:
            List<string> path = null;

            // Execute:
            Assert.DoesNotThrow(() => path = Helper.AdminStore.GetProjectNavigationPath(_firstProject.Id, _viewerUser),
                                "GetNavigationPath shouldn't throw an error");

            // Verify:
            Assert.AreEqual(2, path.Count);
            Assert.AreEqual(ROOT_FOLDER_NAME, path[0]);
            Assert.AreEqual(_firstProject.Name, path[1]);
        }

        [TestCase]
        [TestRail(191083)]
        [Description("Get Project Navigation Path for project, user with Viewer access, IncludeProjectItself is false, verify that path has expected value (doesn't contain project's name).")]
        public void GetProjectNavigation_ValidProjectIncludeProjectItselfFalse_ReturnsExpectedPath()
        {
            // Setup:
            List<string> path = null;
            // Execute:
            Assert.DoesNotThrow(() => path = Helper.AdminStore.GetProjectNavigationPath(_firstProject.Id, _viewerUser, includeProjectItself: false),
                                "GetNavigationPath shouldn't throw an error");

            // Verify:
            Assert.AreEqual(1, path.Count);
            Assert.AreEqual(ROOT_FOLDER_NAME, path[0]);
        }

        [TestCase]
        [TestRail(191084)]
        [Description("Get Project Navigation Path for project, user with Admin access, verify that path has expected value.")]
        public void GetProjectNavigation_AdminUser_ReturnsExpectedPath()
        {
            // Setup:
            List<string> path = null;
            // Execute:
            Assert.DoesNotThrow(() => path = Helper.AdminStore.GetProjectNavigationPath(_secondProject.Id, _adminUser),
                                "GetNavigationPath shouldn't throw an error");

            // Verify:
            Assert.AreEqual(2, path.Count);
            Assert.AreEqual(ROOT_FOLDER_NAME, path[0]);
            Assert.AreEqual(_secondProject.Name, path[1]);
        }

        [TestCase]
        [TestRail(191085)]
        [Description("Get Project Navigation Path for project, user no access, verify that it returns 403 Forbidden.")]
        public void GetProjectNavigation_UserHasNoProjectAccess_403Forbidden()
        {
            // Execute & Verify:
            Assert.Throws<Http403ForbiddenException>(() =>
                Helper.AdminStore.GetProjectNavigationPath(_secondProject.Id, _viewerUser),
                "GetNavigationPath should return 403 Forbidden when user has no access to the project.");
        }

        [TestCase(int.MaxValue)]
        [TestRail(191086)]
        [Description("Get Project Navigation Path for non-existing project id, verify that it return 404 Not Found.")]
        public void GetProjectNavigation_ProjectDoesNotExist_404NotFound(int projectId)
        {
            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() =>
                Helper.AdminStore.GetProjectNavigationPath(projectId, _viewerUser),
                "GetNavigationPath should return 404 Not Found when passed a non-existent project Id.");

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.ResourceNotFound,
                I18NHelper.FormatInvariant("The project (Id:2147483647) can no longer be accessed. It may have been deleted, or is no longer accessible by you.", projectId));
        }
    }
}