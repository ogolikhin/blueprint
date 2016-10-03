using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.SearchServiceModel.Impl;
using NUnit.Framework;
using System.Collections.Generic;
using TestCommon;
using Utilities;

namespace SearchServiceTests
{
    [TestFixture]
    [Category(Categories.SearchService)]
    public class ProjectSearchTests : TestBase
    {
        private IUser _userAdmin = null;
        private IUser _userAuthorLicense = null;

        private IProjectRole _viewerRole = null;

        private IGroup _group = null;

        private IProject _project = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _userAdmin = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _userAuthorLicense = Helper.CreateUserAndAddToDatabase(instanceAdminRole: null);

            _group = Helper.CreateGroupAndAddToDatabase();
            _group.AddUser(_userAuthorLicense);

            _project = ProjectFactory.GetProject(_userAdmin);

            _viewerRole = ProjectRoleFactory.CreateProjectRole(_project, RolePermissions.Read);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
            _viewerRole.DeleteRole();
        }

        [TestCase]
        [TestRail(182423)]
        [Description("Search project, user has admin privilege, check that search result contains one project.")]
        public void SearchProject_UserAdminAccess_ReturnsCorrectProjects()
        {
            // Setup:
            List<ProjectSearchResult> projects = null;
            string searchString = "es";//project name is 'test' - search using 'es' substring

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                projects = Helper.SearchService.SearchProjects(_userAdmin, searchString);
            }, "GetArtifactDiscussions shouldn't throw any error.");

            // Verify:
            Assert.IsTrue(projects.Count >= 1, "Search result should have at least 1 project, but it doesn't.");
            Assert.IsTrue(projects[0].ProjectName.Contains(searchString), "Name of returned project should contain searchString, but it doesn't");
        }

        [TestCase]
        [TestRail(182449)]
        [Description("Search project, user has no access to the project, returns empty list.")]
        public void SearchProject_UserHasNoProjectAccess_ReturnsEmptyList()
        {
            // Setup:
            List<ProjectSearchResult> projects = null;
            Helper.AdminStore.AddSession(_userAuthorLicense);
            string searchString = "es";//project name is 'test' - search using 'es' substring

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                projects = Helper.SearchService.SearchProjects(_userAuthorLicense, searchString);
            }, "GetArtifactDiscussions shouldn't throw any error.");

            // Verify:
            Assert.AreEqual(0, projects.Count, "Search result should be empty, but it doesn't.");
        }

        [TestCase]
        [TestRail(182450)]
        [Description("Search project, uuser has viewer access to the project, check that search result contains at least one project.")]
        public void SearchProject_UserHasAuthorAccess_ReturnsCorrectProjects()
        {
            // Setup:
            List<ProjectSearchResult> projects = null;
            _group.AssignRoleToProjectOrArtifact(_project, _viewerRole);
            Helper.AdminStore.AddSession(_userAuthorLicense);
            string searchString = "es";//project name is 'test' - search using 'es' substring

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                projects = Helper.SearchService.SearchProjects(_userAuthorLicense, searchString);
            }, "GetArtifactDiscussions shouldn't throw any error.");

            // Verify:
            Assert.IsTrue(projects.Count >= 1, "Search result should have at least 1 project, but it doesn't.");
            Assert.IsTrue(projects[0].ProjectName.Contains(searchString), "Name of returned project should contain searchString, but it doesn't");
        }

        [TestCase]
        [TestRail(182451)]
        [Description("Search project, user has viewer access to the project, check that search result contains at least one project.")]
        public void SearchProjectByFullName_UserHasAuthorAccess_ReturnsCorrectProjects()
        {
            // Setup:
            List<ProjectSearchResult> projects = null;
            _group.AssignRoleToProjectOrArtifact(_project, _viewerRole);
            Helper.AdminStore.AddSession(_userAuthorLicense);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                projects = Helper.SearchService.SearchProjects(_userAuthorLicense, _project.Name);
            }, "GetArtifactDiscussions shouldn't throw any error.");

            // Verify:
            Assert.IsTrue(projects.Count >= 1, "Search result should have at least 1 project, but it doesn't.");
            Assert.AreEqual(_project.Name, projects[0].ProjectName, "Name of returned project should have expected value, but it doesn't");
        }
    }
}
