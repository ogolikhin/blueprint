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

        private const string projectPath = "Blueprint";

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
            List<SearchItem> projects = null;
            string searchString = "es";//project name is 'test' - search using 'es' substring

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                projects = Helper.SearchService.SearchProjects(_userAdmin, searchString);
            }, "SearchProjects shouldn't throw any error.");

            // Verify:
            Assert.IsTrue(projects.Count >= 1, "Search result should have at least 1 project, but it doesn't.");
            Assert.IsTrue(projects[0].Name.Contains(searchString), "Name of returned project should contain searchString, but it doesn't");
            Assert.AreEqual(projectPath, projects[0].Path, "Path of the returned project should be 'Blueprint'");
        }

        [TestCase]
        [TestRail(182449)]
        [Description("Search project, user has no access to the project, returns empty list.")]
        public void SearchProject_UserHasNoProjectAccess_ReturnsEmptyList()
        {
            // Setup:
            List<SearchItem> projects = null;
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
            List<SearchItem> projects = null;
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
            Assert.IsTrue(projects[0].Name.Contains(searchString), "Name of returned project should contain searchString, but it doesn't");
            Assert.AreEqual(projectPath, projects[0].Path, "Path of the returned project should be 'Blueprint'");
        }

        [TestCase]
        [TestRail(182451)]
        [Description("Search project, user has viewer access to the project, check that search result contains at least one project.")]
        public void SearchProjectByFullName_UserHasAuthorAccess_ReturnsCorrectProjects()
        {
            // Setup:
            List<SearchItem> projects = null;
            _group.AssignRoleToProjectOrArtifact(_project, _viewerRole);
            Helper.AdminStore.AddSession(_userAuthorLicense);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                projects = Helper.SearchService.SearchProjects(_userAuthorLicense, _project.Name);
            }, "GetArtifactDiscussions shouldn't throw any error.");

            // Verify:
            Assert.IsTrue(projects.Count >= 1, "Search result should have at least 1 project, but it doesn't.");
            Assert.AreEqual(_project.Name, projects[0].Name, "Name of returned project should have expected value, but it doesn't");
            Assert.AreEqual(projectPath, projects[0].Path, "Path of the returned project should be 'Blueprint'");
        }

        [TestCase]
        [Explicit(IgnoreReasons.ProductBug)]// https://trello.com/c/Hq3GimE1
        [TestRail(185205)]
        [Description("Search project by full name, user has admin privilege, check that found project has expected id.")]
        public void SearchProjectByFullName_UserAdminAccess_ReturnsCorrectProjectId()
        {
            // Setup:
            List<SearchItem> projects = null;
            
            // Execute:
            Assert.DoesNotThrow(() =>
            {
                projects = Helper.SearchService.SearchProjects(_userAdmin, _project.Name);
            }, "SearchProjects shouldn't throw any error.");

            // Verify:
            Assert.IsTrue(projects.Count >= 1, "Search result should have at least 1 project, but it doesn't.");
            Assert.AreEqual(_project.Name, projects[0].Name, "Project should have expected name, but it doesn't");
            Assert.AreEqual(projectPath, projects[0].Path, "Path of the returned project should be 'Blueprint'");
            Assert.AreEqual(_project.Id, projects[0].ProjectId, "Project should have expected id, but it doesn't");
        }
    }
}
