using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.FullTextSearchModel.Impl;
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
        private IUser _userAuthorLicenseViewerRole = null;
        private IUser _userAuthorLicenseNoAccessRole = null;

        private IProjectRole _viewerRole = null;

        private IGroup _authorsGroup = null;

        private IProject _project = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _userAdmin = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _userAuthorLicenseViewerRole = Helper.CreateUserAndAddToDatabase(instanceAdminRole: null);
            _userAuthorLicenseNoAccessRole = Helper.CreateUserAndAddToDatabase(instanceAdminRole: null);

            _authorsGroup = Helper.CreateGroupAndAddToDatabase();
            _authorsGroup.AddUser(_userAuthorLicenseViewerRole);
            _authorsGroup.AddUser(_userAuthorLicenseNoAccessRole);

            _project = ProjectFactory.GetProject(_userAdmin);

            _viewerRole = ProjectRoleFactory.CreateProjectRole(_project, RolePermissions.Read);

            _authorsGroup.AssignRoleToProjectOrArtifact(_project, role: _viewerRole);

            Helper.AdminStore.AddSession(_userAuthorLicenseViewerRole);
            Helper.AdminStore.AddSession(_userAuthorLicenseNoAccessRole);
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
        public void SearchProject_UserHasProjectAccess_ReturnsCorrectProject()
        {
            // Setup:
            List<ProjectSearchResult> projects = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                projects = Helper.FullTextSearch.SearchProjects(_userAdmin, "es", 10);//project name is 'test' - search using 'es' substring
            }, "GetArtifactDiscussions shouldn't throw any error.");

            // Verify:
            Assert.AreEqual(1, projects.Count, "Search result should have 1 project, but it doesn't.");
        }
    }
}
