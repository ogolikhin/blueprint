using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;

namespace ArtifactStore.Controllers
{
    [TestClass]
    public class ArtifactControllerTests
    {
        private const int UserId = 1;
        private Session _session;
        private HashSet<int> _artifactIds;
        private List<ProcessInfoDto> _processInfo;
        private Mock<IArtifactRepository> _mockArtifactRepository;
        private Mock<IServiceLogRepository> _mockServiceLogRepository;
        private Mock<IArtifactPermissionsRepository> _mockArtifactPermissionsRepository;
        private Mock<IPrivilegesRepository> _mockSqlPrivilegesRepository;
        private ArtifactController _artifactController;
        private StandardArtifactTypes _filter;

        [TestInitialize]
        public void Initialize()
        {
            _session = new Session { UserId = UserId };
            _artifactIds = new HashSet<int> { 1, 2, 3 };
            _processInfo = new List<ProcessInfoDto>
            {
                new ProcessInfoDto
                {
                    ItemId = 1,
                    ProcessType = ProcessType.None
                },
                new ProcessInfoDto
                {
                    ItemId = 2,
                    ProcessType = ProcessType.None
                },
                new ProcessInfoDto
                {
                    ItemId = 3,
                    ProcessType = ProcessType.None
                }
            };

            _filter = StandardArtifactTypes.All;

            _mockArtifactRepository = new Mock<IArtifactRepository>();
            _mockServiceLogRepository = new Mock<IServiceLogRepository>();
            _mockArtifactPermissionsRepository = new Mock<IArtifactPermissionsRepository>();
            _mockSqlPrivilegesRepository = new Mock<IPrivilegesRepository>();
            _artifactController = new ArtifactController(_mockArtifactRepository.Object,
                _mockArtifactPermissionsRepository.Object, _mockSqlPrivilegesRepository.Object, _mockServiceLogRepository.Object)
            {
                Request = new HttpRequestMessage()
            };

            _artifactController.Request.Properties[ServiceConstants.SessionProperty] = _session;
        }

        [TestMethod]
        public async Task GetProjectChildrenAsync_Success()
        {
            // Arrange
            var userId = 1;
            var session = new Session { UserId = userId };
            var projectId = 10;
            var children = new List<Artifact>();
            var mockArtifactRepository = new Mock<IArtifactRepository>();
            mockArtifactRepository.Setup(r => r.GetProjectOrArtifactChildrenAsync(projectId, null, userId)).ReturnsAsync(children);
            var mockArtifactPermissionsRepository = new Mock<IArtifactPermissionsRepository>();
            var mockServiceLogRepository = new Mock<IServiceLogRepository>();
            var artifactController = new ArtifactController(mockArtifactRepository.Object, mockArtifactPermissionsRepository.Object, _mockSqlPrivilegesRepository.Object, mockServiceLogRepository.Object)
            {
                Request = new HttpRequestMessage()
            };
            artifactController.Request.Properties[ServiceConstants.SessionProperty] = session;

            // Act
            var result = await artifactController.GetProjectChildrenAsync(projectId);

            // Assert
            Assert.AreSame(children, result);
        }

        [TestMethod]
        public async Task GetArtifactChildrenAsync_Success()
        {
            // Arrange
            var userId = 1;
            var session = new Session { UserId = userId };
            var projectId = 10;
            var artifactId = 20;
            var children = new List<Artifact>();
            var mockArtifactRepository = new Mock<IArtifactRepository>();
            mockArtifactRepository.Setup(r => r.GetProjectOrArtifactChildrenAsync(projectId, artifactId, userId)).ReturnsAsync(children);
            var mockArtifactPermissionsRepository = new Mock<IArtifactPermissionsRepository>();
            var mockServiceLogRepository = new Mock<IServiceLogRepository>();
            var artifactController = new ArtifactController(mockArtifactRepository.Object, mockArtifactPermissionsRepository.Object, _mockSqlPrivilegesRepository.Object, mockServiceLogRepository.Object)
            {
                Request = new HttpRequestMessage()
            };
            artifactController.Request.Properties[ServiceConstants.SessionProperty] = session;

            // Act
            var result = await artifactController.GetArtifactChildrenAsync(projectId, artifactId);

            // Assert
            Assert.AreSame(children, result);
        }

        [TestMethod]
        public async Task GetExpandedTreeToArtifactAsync_Success()
        {
            // Arrange
            const int userId = 1;
            var session = new Session { UserId = userId };
            const int projectId = 10;
            const int artifactId = 20;
            var rootChildren = new List<Artifact>();
            var mockArtifactRepository = new Mock<IArtifactRepository>();
            mockArtifactRepository.Setup(r => r.GetExpandedTreeToArtifactAsync(projectId, artifactId, true, userId)).ReturnsAsync(rootChildren);
            var mockArtifactPermissionsRepository = new Mock<IArtifactPermissionsRepository>();
            var mockServiceLogRepository = new Mock<IServiceLogRepository>();
            var artifactController = new ArtifactController(mockArtifactRepository.Object, mockArtifactPermissionsRepository.Object, _mockSqlPrivilegesRepository.Object, mockServiceLogRepository.Object)
            {
                Request = new HttpRequestMessage()
            };
            artifactController.Request.Properties[ServiceConstants.SessionProperty] = session;

            // Act
            var result = await artifactController.GetExpandedTreeToArtifactAsync(projectId, artifactId, true);

            // Assert
            Assert.AreSame(rootChildren, result);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetExpandedTreeToArtifactAsync_BadParameter_ThrowsBadRequestException()
        {
            // Arrange
            const int userId = 1;
            var session = new Session { UserId = userId };
            const int projectId = 10;
            const int artifactId = 0;
            var mockArtifactRepository = new Mock<IArtifactRepository>();
            var mockArtifactPermissionsRepository = new Mock<IArtifactPermissionsRepository>();
            var mockServiceLogRepository = new Mock<IServiceLogRepository>();
            var artifactController = new ArtifactController(mockArtifactRepository.Object, mockArtifactPermissionsRepository.Object, _mockSqlPrivilegesRepository.Object, mockServiceLogRepository.Object)
            {
                Request = new HttpRequestMessage()
            };
            artifactController.Request.Properties[ServiceConstants.SessionProperty] = session;

            // Act
            await artifactController.GetExpandedTreeToArtifactAsync(projectId, artifactId);

            // Assert
        }

        [TestMethod]
        public async Task GetSubArtifactTreeAsync_NoPermissions_ThrowsNoPermissionsException()
        {
            // Arrange
            const int userId = 1;
            var session = new Session { UserId = userId };
            const int artifactId = 2;
            var artifactIds = new[] { artifactId };
            var permissionsDictionary = new Dictionary<int, RolePermissions> { { artifactId, RolePermissions.None } };
            var mockArtifactRepository = new Mock<IArtifactRepository>();
            var mockArtifactPermissionsRepository = new Mock<IArtifactPermissionsRepository>();
            mockArtifactPermissionsRepository.Setup(r => r.GetArtifactPermissions(artifactIds, userId, false, int.MaxValue, true, null)).ReturnsAsync(permissionsDictionary);
            var mockServiceLogRepository = new Mock<IServiceLogRepository>();

            var artifactController = new ArtifactController(mockArtifactRepository.Object, mockArtifactPermissionsRepository.Object, _mockSqlPrivilegesRepository.Object, mockServiceLogRepository.Object) { Request = new HttpRequestMessage() };
            artifactController.Request.Properties[ServiceConstants.SessionProperty] = session;

            HttpResponseException result = null;

            // Act
            try
            {
                await artifactController.GetSubArtifactTreeAsync(artifactId);
            }
            catch (HttpResponseException e)
            {
                result = e;
            }
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.Forbidden, result.Response.StatusCode);
        }

        [TestMethod]
        public async Task GetSubArtifactTreeAsync_HasPermissions_OK()
        {
            // Arrange
            const int userId = 1;
            var session = new Session { UserId = userId };
            const int artifactId = 2;
            var artifactIds = new[] { artifactId };
            var permissionsDictionary = new Dictionary<int, RolePermissions> { { artifactId, RolePermissions.Read } };
            var subArtifacts = new List<SubArtifact> { new SubArtifact { Id = 1 } };
            var mockArtifactRepository = new Mock<IArtifactRepository>();
            mockArtifactRepository.Setup(r => r.GetSubArtifactTreeAsync(artifactId, userId, int.MaxValue, true)).ReturnsAsync(subArtifacts);
            var mockArtifactPermissionsRepository = new Mock<IArtifactPermissionsRepository>();
            mockArtifactPermissionsRepository.Setup(r => r.GetArtifactPermissions(artifactIds, userId, false, int.MaxValue, true, null)).ReturnsAsync(permissionsDictionary);
            var mockServiceLogRepository = new Mock<IServiceLogRepository>();

            var artifactController = new ArtifactController(mockArtifactRepository.Object, mockArtifactPermissionsRepository.Object, _mockSqlPrivilegesRepository.Object, mockServiceLogRepository.Object) { Request = new HttpRequestMessage() };
            artifactController.Request.Properties[ServiceConstants.SessionProperty] = session;

            // Act
            var result = await artifactController.GetSubArtifactTreeAsync(artifactId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result[0].Id);
        }

        [TestMethod]
        public async Task GetArtifactNavigationPathAsync_Success()
        {
            // Arrange
            const int userId = 1;
            var session = new Session { UserId = userId };
            const int artifactId = 10;
            var navPath = new List<Artifact>();
            var mockArtifactRepository = new Mock<IArtifactRepository>();
            mockArtifactRepository.Setup(r => r.GetArtifactNavigationPathAsync(artifactId, userId)).ReturnsAsync(navPath);
            var mockArtifactPermissionsRepository = new Mock<IArtifactPermissionsRepository>();
            var mockServiceLogRepository = new Mock<IServiceLogRepository>();
            var artifactController = new ArtifactController(mockArtifactRepository.Object, mockArtifactPermissionsRepository.Object, _mockSqlPrivilegesRepository.Object, mockServiceLogRepository.Object)
            {
                Request = new HttpRequestMessage()
            };
            artifactController.Request.Properties[ServiceConstants.SessionProperty] = session;

            // Act
            var result = await artifactController.GetArtifactNavigationPathAsync(artifactId);

            // Assert
            Assert.AreSame(navPath, result);
        }

        [TestMethod]
        public async Task Artifact_GetProcessInfo_Success()
        {
            // Arrange
            _mockArtifactRepository
                .Setup(r => r.GetProcessInformationAsync(_artifactIds, _session.UserId, true))
                .ReturnsAsync(_processInfo);

            // Act
            var result = await _artifactController.GetProcessInformationAsync(_artifactIds);

            // Assert
            Assert.AreSame(_processInfo, result);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task Artifact_GetProcessInfo_ThrowsBadRequestException()
        {
            // Arrange
            _artifactIds = null;
            _processInfo = new List<ProcessInfoDto>();

            _mockArtifactRepository
                .Setup(r => r.GetProcessInformationAsync(_artifactIds, _session.UserId, true))
                .ReturnsAsync(_processInfo);

            // Act
            var result = await _artifactController.GetProcessInformationAsync(_artifactIds);
        }

        #region GetStandardProperties

        [TestMethod]
        public async Task GetStandardProperties_AllParametersAreCorrect_SuccessResult()
        {
            // Arrange
            var artifactTypeIds = new HashSet<int> { 1, 2, 3 };
            var properties = new List<PropertyType>
            {
                new PropertyType { Id = 1, Name = "Property1" },
                new PropertyType { Id = 2, Name = "Property2" }
            };

            _mockArtifactRepository.Setup(r => r.GetStandardProperties(artifactTypeIds))
                                  .ReturnsAsync(properties);

            _mockSqlPrivilegesRepository
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            // Act
            var result = await _artifactController.GetStandardProperties(artifactTypeIds);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result, properties);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetStandardProperties_IncorrectParameters_ThrowsBadRequestException()
        {
            // Arrange
            _mockSqlPrivilegesRepository
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            // Act
            await _artifactController.GetStandardProperties(null);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetStandardProperties_InsufficientPermissions_ThrowsAuthorizationException()
        {
            // Arrange
            _mockSqlPrivilegesRepository
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageGroups);

            // Act
            await _artifactController.GetStandardProperties(null);

            // Assert
        }

        #endregion

        [TestMethod]
        public async Task GetStandardArtifactTypes_AllParametersAreCorrect_SuccessResult()
        {
            // Arrange
            var artifacts = new List<StandardArtifactType> { new StandardArtifactType { Id = 1, Name = "CustomActor" } };
            _mockArtifactRepository.Setup(r => r.GetStandardArtifactTypes(_filter))
                                  .ReturnsAsync(artifacts);

            _mockSqlPrivilegesRepository
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            // Act
            var result = await _artifactController.GetStandardArtifactTypes(_filter);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result, artifacts);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetStandardArtifactTypes_NotExistingFilter_BadRequestException()
        {
            // Arrange
            _mockSqlPrivilegesRepository
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            // Act
            await _artifactController.GetStandardArtifactTypes((StandardArtifactTypes)100);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetStandardArtifactTypes_IncorrectPermissions_AuthorizationException()
        {
            // Arrange
            _mockSqlPrivilegesRepository
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(UserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewUsers);

            // Act
            await _artifactController.GetStandardArtifactTypes(_filter);

            // Assert
        }
    }
}
