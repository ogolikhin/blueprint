﻿using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ArtifactStore.Models;
using ArtifactStore.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;

namespace ArtifactStore.Controllers
{
    [TestClass]
    public class ArtifactControllerTests
    {
        [TestMethod]
        public async Task GetProjectChildrenAsync_Success()
        {
            // Arrange
            var userId = 1;
            var session = new Session { UserId = userId };
            var projectId = 10;
            var children = new List<Artifact>();
            var mockArtifactRepository = new Mock<ISqlArtifactRepository>();
            mockArtifactRepository.Setup(r => r.GetProjectOrArtifactChildrenAsync(projectId, null, userId)).ReturnsAsync(children);
            var mockServiceLogRepository = new Mock<IServiceLogRepository>();
            var artifactController = new ArtifactController(mockArtifactRepository.Object, mockServiceLogRepository.Object)
            {
                Request = new HttpRequestMessage()
            };
            artifactController.Request.Properties[ServiceConstants.SessionProperty] = session;

            // Act
            var result = await artifactController.GetProjectChildrenAsync(projectId);

            //Assert
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
            var mockArtifactRepository = new Mock<ISqlArtifactRepository>();
            mockArtifactRepository.Setup(r => r.GetProjectOrArtifactChildrenAsync(projectId, artifactId, userId)).ReturnsAsync(children);
            var mockServiceLogRepository = new Mock<IServiceLogRepository>();
            var artifactController = new ArtifactController(mockArtifactRepository.Object, mockServiceLogRepository.Object)
            {
                Request = new HttpRequestMessage()
            };
            artifactController.Request.Properties[ServiceConstants.SessionProperty] = session;

            // Act
            var result = await artifactController.GetArtifactChildrenAsync(projectId, artifactId);

            //Assert
            Assert.AreSame(children, result);
        }
    }
}
