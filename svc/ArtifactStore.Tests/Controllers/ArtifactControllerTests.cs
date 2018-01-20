﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;
using System.Web.Http;
using System.Net;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Controllers
{
    [TestClass]
    public class ArtifactControllerTests
    {
        private const int userId = 1;
        private Session session;
        private HashSet<int> artifactIds;
        private List<ProcessInfoDto> processInfo;
        private Mock<IArtifactRepository> mockArtifactRepository;
        private Mock<IServiceLogRepository> mockServiceLogRepository;
        private Mock<IArtifactPermissionsRepository> mockArtifactPermissionsRepository;
        private Mock<IPrivilegesRepository> _mockSqlPrivilegesRepository;
        private ArtifactController artifactController;
        private StandardArtifactTypes filter;

        [TestInitialize]
        public void Initialize()
        {
            session = new Session { UserId = userId };
            artifactIds = new HashSet<int>() { 1, 2, 3 };
            processInfo = new List<ProcessInfoDto>()
            {
                new ProcessInfoDto()
                {
                    ItemId = 1,
                    ProcessType = ProcessType.None
                },
                new ProcessInfoDto()
                {
                    ItemId = 2,
                    ProcessType = ProcessType.None
                },
                new ProcessInfoDto()
                {
                    ItemId = 3,
                    ProcessType = ProcessType.None
                }
            };

            filter = StandardArtifactTypes.All;

            mockArtifactRepository = new Mock<IArtifactRepository>();
            mockServiceLogRepository = new Mock<IServiceLogRepository>();
            mockArtifactPermissionsRepository = new Mock<IArtifactPermissionsRepository>();
            _mockSqlPrivilegesRepository = new Mock<IPrivilegesRepository>();
            artifactController = new ArtifactController(mockArtifactRepository.Object,
                mockArtifactPermissionsRepository.Object, _mockSqlPrivilegesRepository.Object, mockServiceLogRepository.Object)
            {
                Request = new HttpRequestMessage()
            };

            artifactController.Request.Properties[ServiceConstants.SessionProperty] = session;
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
            var permissionsDictionary = new Dictionary<int, RolePermissions>();
            permissionsDictionary.Add(artifactId, RolePermissions.None);
            var mockArtifactRepository = new Mock<IArtifactRepository>();
            var mockArtifactPermissionsRepository = new Mock<IArtifactPermissionsRepository>();
            mockArtifactPermissionsRepository.Setup(r => r.GetArtifactPermissions(artifactIds, userId, false, int.MaxValue, true, null)).ReturnsAsync(permissionsDictionary);
            var mockServiceLogRepository = new Mock<IServiceLogRepository>();

            var artifactController = new ArtifactController(mockArtifactRepository.Object, mockArtifactPermissionsRepository.Object, _mockSqlPrivilegesRepository.Object, mockServiceLogRepository.Object) { Request = new HttpRequestMessage() };
            artifactController.Request.Properties[ServiceConstants.SessionProperty] = session;

            HttpResponseException result = null;

            // Act
            try {
                await artifactController.GetSubArtifactTreeAsync(artifactId);
            } catch (HttpResponseException e)
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
            var permissionsDictionary = new Dictionary<int, RolePermissions>();
            permissionsDictionary.Add(artifactId, RolePermissions.Read);
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
            mockArtifactRepository.Setup(r => r.GetProcessInformationAsync(artifactIds, session.UserId))
                                  .ReturnsAsync(processInfo);

            // Act
            var result = await artifactController.GetProcessInformationAsync(artifactIds);

            // Assert
            Assert.AreSame(processInfo, result);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task Artifact_GetProcessInfo_ThrowsBadRequestException()
        {
            // Arrange
            artifactIds = null;
            processInfo = new List<ProcessInfoDto>() { };

            mockArtifactRepository.Setup(r => r.GetProcessInformationAsync(artifactIds, session.UserId))
                                  .ReturnsAsync(processInfo);

            // Act
            var result = await artifactController.GetProcessInformationAsync(artifactIds);

        }

        #region GetStandardProperties

        [TestMethod]
        public async Task GetStandardProperties_AllParametersAreCorrect_SuccessResult()
        {
            // Arrange
            HashSet<int> artifactTypeIds = new HashSet<int>() { 1, 2, 3 };
            List<PropertyType> properties = new List<PropertyType>()
            {
                new PropertyType() { Id = 1, Name = "Property1" },
                new PropertyType() { Id = 2, Name = "Property2" }
            };

            mockArtifactRepository.Setup(r => r.GetStandardProperties(artifactTypeIds))
                                  .ReturnsAsync(properties);

            _mockSqlPrivilegesRepository
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(userId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            // Act
            var result = await artifactController.GetStandardProperties(artifactTypeIds);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result, properties);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetStandardProperties_IncorrectParameters_ThrowsBadRequestException()
        {
            // Arrange
            HashSet<int> artifactTypeIds = null;

            _mockSqlPrivilegesRepository
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(userId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            // Act
            await artifactController.GetStandardProperties(artifactTypeIds);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetStandardProperties_InsufficientPermissions_ThrowsAuthorizationException()
        {
            // Arrange
            HashSet<int> artifactTypeIds = null;

            _mockSqlPrivilegesRepository
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(userId))
                .ReturnsAsync(InstanceAdminPrivileges.ManageGroups);

            // Act
            await artifactController.GetStandardProperties(artifactTypeIds);

            // Assert
        }

        #endregion

        [TestMethod]
        public async Task GetStandardArtifactTypes_AllParametersAreCorrect_SuccessResult()
        {
            // Arrange
            var artifacts = new List<StandardArtifactType> { new StandardArtifactType { Id = 1, Name = "CustomActor" } };
            mockArtifactRepository.Setup(r => r.GetStandardArtifactTypes(filter))
                                  .ReturnsAsync(artifacts);

            _mockSqlPrivilegesRepository
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(userId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            // Act
            var result = await artifactController.GetStandardArtifactTypes(filter);

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
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(userId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            // Act
            await artifactController.GetStandardArtifactTypes((StandardArtifactTypes)100);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetStandardArtifactTypes_IncorrectPermissions_AuthorizationException()
        {
            // Arrange
            _mockSqlPrivilegesRepository
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(userId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewUsers);

            // Act
            await artifactController.GetStandardArtifactTypes(filter);

            // Assert
        }
    }
}
