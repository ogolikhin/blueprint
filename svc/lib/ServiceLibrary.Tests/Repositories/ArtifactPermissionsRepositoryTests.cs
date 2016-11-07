using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Models;

namespace ServiceLibrary.Repositories
{
    [TestClass]
    public class ArtifactPermissionsRepositoryTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetArtifactPermissions_ItemIdsMoreThan50_ArgumentOutOfRangeException()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactPermissionsRepository(cxn.Object);
            var itemIds = Enumerable.Range(1, 60);

            // Act
            await repository.GetArtifactPermissions(itemIds, 0);
        }

        [TestMethod]
        public async Task GetArtifactPermissions_UserIsInstanceAdmin_ReturnsAllPermissions()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactPermissionsRepository(cxn.Object);
            MockToReturnInstanceAdmin(true, cxn);
            var itemIds = new List<int> { 1 };
            var allPermissions = Enum.GetValues(typeof(RolePermissions)).Cast<long>().Aggregate(RolePermissions.None, (current, permission) => current | (RolePermissions)permission);

            // Act
            var result = await repository.GetArtifactPermissions(itemIds, 0);


            //Assert
            Assert.IsTrue(result[1] == allPermissions);
        }

        [TestMethod]
        public async Task GetArtifactPermissions_VersionProjectInfosContainsProjectId_ReturnsEditPermissions()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactPermissionsRepository(cxn.Object);
            MockToReturnInstanceAdmin(false, cxn);
            var itemIds = new List<int> { 1 };
            var mockBoolResult = new List<bool> { true }.AsEnumerable();
            var mockProjectsArtifactsItemsResult = new List<ProjectsArtifactsItem>
            {
                new ProjectsArtifactsItem
                {
                    HolderId = 1,
                    VersionArtifactId = 1,
                    VersionProjectId = 1
                }
            }.AsEnumerable();
            var mockVersionProjectInfoResult = new List<VersionProjectInfo>
            {
                new VersionProjectInfo
                {
                    ProjectId = 1,
                    Permissions = (long)RolePermissions.Edit
                }
            }.AsEnumerable();
            MockQueryMultipleAsync(itemIds, cxn, mockBoolResult, mockProjectsArtifactsItemsResult, mockVersionProjectInfoResult);

            // Act
            var result = await repository.GetArtifactPermissions(itemIds, 0);

            //Assert
            Assert.IsTrue(result[1] == RolePermissions.Edit);
        }

        [TestMethod]
        public async Task GetArtifactPermissions_VersionProjectInfosNotContainProjectId_ReturnsCanReportPermissions()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactPermissionsRepository(cxn.Object);
            MockToReturnInstanceAdmin(false, cxn);
            var itemIds = new List<int> { 1 };
            var mockBoolResult = new List<bool> { true }.AsEnumerable();
            var mockProjectsArtifactsItemsResult = new List<ProjectsArtifactsItem>
            {
                new ProjectsArtifactsItem
                {
                    HolderId = 1,
                    VersionArtifactId = 1,
                    VersionProjectId = 1
                }
            }.AsEnumerable();
            var mockVersionProjectInfoResult = new List<VersionProjectInfo>
            {
                new VersionProjectInfo
                {
                    ProjectId = 2,
                    Permissions = (long)RolePermissions.CanReport
                }
            }.AsEnumerable();
            MockQueryMultipleAsync(itemIds, cxn, mockBoolResult, mockProjectsArtifactsItemsResult, mockVersionProjectInfoResult);
            var mockOpenArtifactPermissionsResult = new List<OpenArtifactPermission>()
            {
                new OpenArtifactPermission
                {
                    HolderId = 1,
                    Permissions = (long) RolePermissions.CanReport
                }
            }.AsEnumerable();
            MockGetOpenArtifactPermissions(mockOpenArtifactPermissionsResult, cxn, new List<int>() { 1 });

            // Act
            var result = await repository.GetArtifactPermissions(itemIds, 0);

            //Assert
            Assert.IsTrue(result[1] == RolePermissions.CanReport);
        }

        [TestMethod]
        public async Task GetArtifactPermissions_WithRevisionId_ReturnsEditPermissions()
        {
            // Arrange
            int revisionId = 1;
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactPermissionsRepository(cxn.Object);
            MockToReturnInstanceAdmin(false, cxn);
            var itemIds = new List<int> { 1 };
            var mockBoolResult = new List<bool> { true }.AsEnumerable();
            var mockProjectsArtifactsItemsResult = new List<ProjectsArtifactsItem>
            {
                new ProjectsArtifactsItem
                {
                    HolderId = 1,
                    VersionArtifactId = 1,
                    VersionProjectId = 1
                }
            }.AsEnumerable();
            var mockVersionProjectInfoResult = new List<VersionProjectInfo>
            {
                new VersionProjectInfo
                {
                    ProjectId = 2,
                    Permissions = (long)RolePermissions.Edit
                }
            }.AsEnumerable();
            MockQueryMultipleAsync(itemIds, cxn, mockBoolResult, mockProjectsArtifactsItemsResult, mockVersionProjectInfoResult, revisionId);
            var mockOpenArtifactPermissionsResult = new List<OpenArtifactPermission>()
            {
                new OpenArtifactPermission
                {
                    HolderId = 1,
                    Permissions = (long) RolePermissions.Edit
                }
            }.AsEnumerable();
            MockGetOpenArtifactPermissions(mockOpenArtifactPermissionsResult, cxn, new List<int>() { 1 }, revisionId);

            // Act
            var result = await repository.GetArtifactPermissions(itemIds, 0, false, revisionId);

            //Assert
            Assert.IsTrue(result[1] == RolePermissions.Edit);
        }

        [TestMethod]
        public async Task GetArtifactPermissions_MultipleProjectsArtifactsItems_ReturnsDeletePermissions()
        {
            // Arrange
            int revisionId = 1;
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactPermissionsRepository(cxn.Object);
            MockToReturnInstanceAdmin(false, cxn);
            var itemIds = new List<int> { 1 };
            var mockBoolResult = new List<bool> { true }.AsEnumerable();
            var mockProjectsArtifactsItemsResult = new List<ProjectsArtifactsItem>
            {
                new ProjectsArtifactsItem
                {
                    HolderId = 1,
                    VersionArtifactId = 1,
                    VersionProjectId = 1
                },
                new ProjectsArtifactsItem
                {
                    HolderId = 3,
                    VersionArtifactId = 3,
                    VersionProjectId = 3
                }
            }.AsEnumerable();
            var mockVersionProjectInfoResult = new List<VersionProjectInfo>
            {
                new VersionProjectInfo
                {
                    ProjectId = 2,
                    Permissions = (long)RolePermissions.Delete
                }
            }.AsEnumerable();
            MockQueryMultipleAsync(itemIds, cxn, mockBoolResult, mockProjectsArtifactsItemsResult, mockVersionProjectInfoResult, revisionId);
            var mockOpenArtifactPermissionsResult = new List<OpenArtifactPermission>()
            {
                new OpenArtifactPermission
                {
                    HolderId = 1,
                    Permissions = (long) RolePermissions.Delete
                }
            }.AsEnumerable();
            MockGetOpenArtifactPermissions(mockOpenArtifactPermissionsResult, cxn, new List<int>() { 1 }, revisionId);

            // Act
            var result = await repository.GetArtifactPermissions(itemIds, 0, false, revisionId);

            //Assert
            Assert.IsTrue(result[1] == RolePermissions.Delete);
        }

        [TestMethod]
        public async Task GetArtifactPermissions_MultipleProjectsWithNullPermission_ReturnsReadPermissions()
        {
            // Arrange
            int revisionId = 1;
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactPermissionsRepository(cxn.Object);
            MockToReturnInstanceAdmin(false, cxn);
            var itemIds = new List<int> { 1 };
            var mockBoolResult = new List<bool> { true }.AsEnumerable();
            var mockProjectsArtifactsItemsResult = new List<ProjectsArtifactsItem>
            {
                new ProjectsArtifactsItem
                {
                    HolderId = 1,
                    VersionArtifactId = 1,
                    VersionProjectId = 1
                },
                new ProjectsArtifactsItem
                {
                    HolderId = 3,
                    VersionArtifactId = 3,
                    VersionProjectId = 3
                }
            }.AsEnumerable();
            var mockVersionProjectInfoResult = new List<VersionProjectInfo>
            {
                new VersionProjectInfo
                {
                    ProjectId = 2,
                    Permissions = (long)RolePermissions.Read
                },
                new VersionProjectInfo
                {
                    ProjectId = 3
                }
            }.AsEnumerable();
            MockQueryMultipleAsync(itemIds, cxn, mockBoolResult, mockProjectsArtifactsItemsResult, mockVersionProjectInfoResult, revisionId);
            var mockOpenArtifactPermissionsResult = new List<OpenArtifactPermission>()
            {
                new OpenArtifactPermission
                {
                    HolderId = 1,
                    Permissions = (long) RolePermissions.Read
                }
            }.AsEnumerable();
            MockGetOpenArtifactPermissions(mockOpenArtifactPermissionsResult, cxn, new List<int>() { 1 }, revisionId);

            // Act
            var result = await repository.GetArtifactPermissions(itemIds, 0, false, revisionId);

            //Assert
            Assert.IsTrue(result[1] == RolePermissions.Read);
        }

        #region Private Members

        private void MockToReturnInstanceAdmin(bool isInstanceAdmin, SqlConnectionWrapperMock cxn)
        {
            var result = new List<bool> { isInstanceAdmin };
            cxn.SetupQueryAsync("NOVAIsInstanceAdmin",
                new Dictionary<string, object> { { "contextUser", false }, { "userId", 0 } }, result);
        }

        private void MockGetOpenArtifactPermissions(IEnumerable<OpenArtifactPermission> mockOpenArtifactPermissionsResult, SqlConnectionWrapperMock cxn, IEnumerable<int> projectArtifactIds, int revisionId = int.MaxValue, bool addDrafts = true)
        {
            var artifactIds = SqlConnectionWrapper.ToDataTable(projectArtifactIds, "Int32Collection", "Int32Value");
            cxn.SetupQueryAsync("GetOpenArtifactPermissions",
                new Dictionary<string, object>
                {
                    {"userId", 0},
                    {"artifactIds", artifactIds}
                }, mockOpenArtifactPermissionsResult);
        }

        private void MockQueryMultipleAsync(
            IEnumerable<int> itemIds,
            SqlConnectionWrapperMock cxn,
            IEnumerable<bool> mockBoolResult,
            IEnumerable<ProjectsArtifactsItem> mockProjectsArtifactsItemsResult,
            IEnumerable<VersionProjectInfo> mockVersionProjectInfoResult,
            int revisionId = int.MaxValue,
            bool addDrafts = true
            )
        {
            var tvp = SqlConnectionWrapper.ToDataTable(itemIds, "Int32Collection", "Int32Value");

            var result = Tuple.Create(mockBoolResult, mockProjectsArtifactsItemsResult, mockVersionProjectInfoResult);
            cxn.SetupQueryMultipleAsync("GetArtifactsProjects",
                new Dictionary<string, object>
                {
                    {"contextUser", false},
                    {"userId", 0},
                    {"itemIds", tvp}
                }, result);
        }

        #endregion
    }
}
