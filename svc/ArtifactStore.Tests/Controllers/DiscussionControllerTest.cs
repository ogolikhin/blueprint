﻿using ArtifactStore.Models;
using ArtifactStore.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Attributes;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ServiceLibrary.Repositories;
using ServiceLibrary.Exceptions;

namespace ArtifactStore.Controllers
{
    [TestClass]
    public class DiscussionControllerTest
    {
        private Mock<IDiscussionsRepository> _discussionsRepositoryMock;

        private Mock<IArtifactPermissionsRepository> _artifactPermissionsRepositoryMock;

        private Mock<IArtifactVersionsRepository> _artifactVersionsRepositoryMock;

        private Session _session;

        [TestInitialize]
        public void Initialize()
        {
            // Arrange
            var userId = 1;
            _session = new Session { UserId = userId };

            _discussionsRepositoryMock = new Mock<IDiscussionsRepository>();
            _artifactPermissionsRepositoryMock = new Mock<IArtifactPermissionsRepository>();
            _artifactVersionsRepositoryMock = new Mock<IArtifactVersionsRepository>();
        }

        [TestMethod]
        public void GetDiscussionsMethodHasSessionRequiredAttribute()
        {
            var getDiscussionMethod = typeof(DiscussionController).GetMethod("GetDiscussions");
            var hasSessionRequiredAttr = getDiscussionMethod.GetCustomAttributes(false).Any(a => a is SessionRequiredAttribute);
            Assert.IsTrue(hasSessionRequiredAttr);
        }

        [TestMethod]
        public void GetRepliesMethodHasSessionRequiredAttribute()
        {
            var getRepliesMethod = typeof(DiscussionController).GetMethod("GetReplies");
            var hasSessionRequiredAttr = getRepliesMethod.GetCustomAttributes(false).Any(a => a is SessionRequiredAttribute);
            Assert.IsTrue(hasSessionRequiredAttr);
        }

        [TestMethod]
        public async Task GetDiscussions_Success()
        {
            // Arrange
            const int artifactId = 1;
            const int projectId = 10;
            var itemInfo = new ItemInfo { ProjectId = projectId, ArtifactId = artifactId, ItemId = artifactId };
            var permisionDictionary = new Dictionary<int, RolePermissions>();
            var discussion = new Discussion { ItemId = artifactId };
            permisionDictionary.Add(artifactId, RolePermissions.Read);
            _artifactVersionsRepositoryMock.Setup(m => m.IsItemDeleted(artifactId)).ReturnsAsync(false);
            _artifactPermissionsRepositoryMock.Setup(m => m.GetItemInfo(artifactId, _session.UserId, false, int.MaxValue)).ReturnsAsync(itemInfo);
            _artifactPermissionsRepositoryMock.Setup(m => m.GetArtifactPermissions(It.IsAny<IEnumerable<int>>(), _session.UserId, false, int.MaxValue, true, null)).ReturnsAsync(permisionDictionary);
            _discussionsRepositoryMock.Setup(m => m.GetDiscussions(artifactId, projectId)).ReturnsAsync(new[] { discussion });
            var controller = new DiscussionController(_discussionsRepositoryMock.Object, _artifactPermissionsRepositoryMock.Object, _artifactVersionsRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties[ServiceConstants.SessionProperty] = _session;
            // Act
            var result = await controller.GetDiscussions(artifactId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ThreadStatuses);
            Assert.AreEqual(artifactId, result.Discussions.ElementAt(0).ItemId);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetDiscussions_Forbidden()
        {
            // Arrange
            const int artifactId = 1;
            const int projectId = 10;
            var itemInfo = new ItemInfo { ProjectId = projectId, ArtifactId = artifactId, ItemId = artifactId };
            var permisionDictionary = new Dictionary<int, RolePermissions>();
            var discussion = new Discussion { ItemId = artifactId };
            permisionDictionary.Add(artifactId, RolePermissions.None);
            _artifactVersionsRepositoryMock.Setup(m => m.IsItemDeleted(artifactId)).ReturnsAsync(false);
            _artifactPermissionsRepositoryMock.Setup(m => m.GetItemInfo(artifactId, _session.UserId, false, int.MaxValue)).ReturnsAsync(itemInfo);
            _artifactPermissionsRepositoryMock.Setup(m => m.GetArtifactPermissions(It.IsAny<IEnumerable<int>>(), _session.UserId, false, int.MaxValue, true, null)).ReturnsAsync(permisionDictionary);
            _discussionsRepositoryMock.Setup(m => m.GetDiscussions(artifactId, projectId)).ReturnsAsync(new[] { discussion });
            var controller = new DiscussionController(_discussionsRepositoryMock.Object, _artifactPermissionsRepositoryMock.Object, _artifactVersionsRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties[ServiceConstants.SessionProperty] = _session;
            // Act
            await controller.GetDiscussions(artifactId);
        }

        [TestMethod]
        public async Task GetDiscussions_BadRequest_1()
        {
            // Arrange
            const int artifactId = -1;
            var controller = new DiscussionController(_discussionsRepositoryMock.Object, _artifactPermissionsRepositoryMock.Object, _artifactVersionsRepositoryMock.Object);
            // Act
            try
            {
                var result = await controller.GetDiscussions(artifactId);
            }
            catch (BadRequestException e)
            {
                // Assert
                Assert.AreEqual(HttpStatusCode.BadRequest, (HttpStatusCode)e.ErrorCode);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetDiscussions_BadRequest_2()
        {
            // Arrange
            const int artifactId = 1;
            const int subArtifactId = -1;
            var controller = new DiscussionController(_discussionsRepositoryMock.Object, _artifactPermissionsRepositoryMock.Object, _artifactVersionsRepositoryMock.Object);
            // Act
            await controller.GetDiscussions(artifactId, subArtifactId);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetDiscussions_BadRequest_Same_Artifact_And_SubArtifactId()
        {
            // Arrange
            const int artifactId = 1;
            const int subArtifactId = 1;
            var controller = new DiscussionController(_discussionsRepositoryMock.Object, _artifactPermissionsRepositoryMock.Object, _artifactVersionsRepositoryMock.Object);
            // Act
            await controller.GetDiscussions(artifactId, subArtifactId);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetDiscussions_NotFound()
        {
            // Arrange
            const int artifactId = 1;
            const int projectId = 10;
            var itemInfo = new ItemInfo { ProjectId = projectId, ArtifactId = artifactId, ItemId = artifactId };
            var permisionDictionary = new Dictionary<int, RolePermissions>();
            var discussion = new Discussion { ItemId = artifactId };
            permisionDictionary.Add(artifactId, RolePermissions.Read);
            _artifactPermissionsRepositoryMock.Setup(m => m.GetItemInfo(artifactId, _session.UserId, true, int.MaxValue)).ReturnsAsync((ItemInfo)null);
            _artifactVersionsRepositoryMock.Setup(m => m.IsItemDeleted(artifactId)).ReturnsAsync(false);
            _artifactVersionsRepositoryMock.Setup(m => m.GetDeletedItemInfo(artifactId)).ReturnsAsync((DeletedItemInfo)null);
            _artifactPermissionsRepositoryMock.Setup(m => m.GetArtifactPermissions(It.IsAny<IEnumerable<int>>(), _session.UserId, false, int.MaxValue, true, null)).ReturnsAsync(permisionDictionary);
            _discussionsRepositoryMock.Setup(m => m.GetDiscussions(artifactId, projectId)).ReturnsAsync(new[] { discussion });
            var controller = new DiscussionController(_discussionsRepositoryMock.Object, _artifactPermissionsRepositoryMock.Object, _artifactVersionsRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties[ServiceConstants.SessionProperty] = _session;
            // Act
            await controller.GetDiscussions(artifactId);
        }
        [TestMethod]
        public async Task GetReplies_Success()
        {
            // Arrange
            const int artifactId = 1;
            const int projectId = 10;
            const int discussionId = 1;
            var itemInfo = new ItemInfo { ProjectId = projectId, ArtifactId = artifactId, ItemId = artifactId };
            var permisionDictionary = new Dictionary<int, RolePermissions>();
            var reply = new Reply { ItemId = artifactId };
            permisionDictionary.Add(artifactId, RolePermissions.Read);
            _artifactPermissionsRepositoryMock.Setup(m => m.GetItemInfo(artifactId, _session.UserId, false, int.MaxValue)).ReturnsAsync(itemInfo);
            _artifactPermissionsRepositoryMock.Setup(m => m.GetArtifactPermissions(It.IsAny<IEnumerable<int>>(), _session.UserId, false, int.MaxValue, true, null)).ReturnsAsync(permisionDictionary);
            _discussionsRepositoryMock.Setup(m => m.GetReplies(discussionId, projectId)).ReturnsAsync(new[] { reply });
            var controller = new DiscussionController(_discussionsRepositoryMock.Object, _artifactPermissionsRepositoryMock.Object, _artifactVersionsRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties[ServiceConstants.SessionProperty] = _session;
            // Act
            var result = await controller.GetReplies(artifactId, discussionId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(artifactId, result.ElementAt(0).ItemId);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetReplies_BadRequest_1()
        {
            // Arrange
            const int artifactId = -1;
            const int discussionId = 1;
            var controller = new DiscussionController(_discussionsRepositoryMock.Object, _artifactPermissionsRepositoryMock.Object, _artifactVersionsRepositoryMock.Object);
            // Act
            await controller.GetReplies(artifactId, discussionId);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetReplies_BadRequest_2()
        {
            // Arrange
            const int artifactId = 1;
            const int discussionId = -1;
            var controller = new DiscussionController(_discussionsRepositoryMock.Object, _artifactPermissionsRepositoryMock.Object, _artifactVersionsRepositoryMock.Object);
            // Act
            await controller.GetReplies(artifactId, discussionId);
        }


        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetReplies_BadRequest_Same_Artifact_And_SubArtifactId()
        {
            // Arrange
            const int artifactId = 1;
            const int subArtifactId = 1;
            const int discussionId = 2;
            var controller = new DiscussionController(_discussionsRepositoryMock.Object, _artifactPermissionsRepositoryMock.Object, _artifactVersionsRepositoryMock.Object);
            // Act
            await controller.GetReplies(artifactId, discussionId, subArtifactId);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetReplies_BadRequest_3()
        {
            // Arrange
            const int artifactId = 1;
            const int discussionId = 1;
            const int subArtifactId = -1;
            var controller = new DiscussionController(_discussionsRepositoryMock.Object, _artifactPermissionsRepositoryMock.Object, _artifactVersionsRepositoryMock.Object);
            // Act
            await controller.GetReplies(artifactId, discussionId, subArtifactId);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetReplies_Forbidden()
        {
            // Arrange
            const int artifactId = 1;
            const int projectId = 10;
            const int discussionId = 1;
            var itemInfo = new ItemInfo { ProjectId = projectId, ArtifactId = artifactId, ItemId = artifactId };
            var permisionDictionary = new Dictionary<int, RolePermissions>();
            var reply = new Reply { ItemId = artifactId };
            permisionDictionary.Add(artifactId, RolePermissions.None);
            _artifactPermissionsRepositoryMock.Setup(m => m.GetItemInfo(artifactId, _session.UserId, false, int.MaxValue)).ReturnsAsync(itemInfo);
            _artifactPermissionsRepositoryMock.Setup(m => m.GetArtifactPermissions(It.IsAny<IEnumerable<int>>(), _session.UserId, false, int.MaxValue, true, null)).ReturnsAsync(permisionDictionary);
            _discussionsRepositoryMock.Setup(m => m.GetReplies(discussionId, projectId)).ReturnsAsync(new[] { reply });
            var controller = new DiscussionController(_discussionsRepositoryMock.Object, _artifactPermissionsRepositoryMock.Object, _artifactVersionsRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties[ServiceConstants.SessionProperty] = _session;
            // Act
            await controller.GetReplies(artifactId, discussionId);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetReplies_NotFound()
        {
            // Arrange
            const int artifactId = 1;
            const int projectId = 10;
            const int discussionId = 1;
            var itemInfo = new ItemInfo { ProjectId = projectId, ArtifactId = artifactId, ItemId = artifactId };
            var permisionDictionary = new Dictionary<int, RolePermissions>();
            var reply = new Reply { ItemId = artifactId };
            permisionDictionary.Add(artifactId, RolePermissions.Read);
            _artifactPermissionsRepositoryMock.Setup(m => m.GetItemInfo(artifactId, _session.UserId, true, int.MaxValue)).ReturnsAsync((ItemInfo)null);
            _artifactVersionsRepositoryMock.Setup(m => m.IsItemDeleted(artifactId)).ReturnsAsync(false);
            _artifactVersionsRepositoryMock.Setup(m => m.GetDeletedItemInfo(artifactId)).ReturnsAsync((DeletedItemInfo)null);
            _artifactPermissionsRepositoryMock.Setup(m => m.GetArtifactPermissions(It.IsAny<IEnumerable<int>>(), _session.UserId, false, int.MaxValue, true, null)).ReturnsAsync(permisionDictionary);
            _discussionsRepositoryMock.Setup(m => m.GetReplies(discussionId, projectId)).ReturnsAsync(new[] { reply });
            var controller = new DiscussionController(_discussionsRepositoryMock.Object, _artifactPermissionsRepositoryMock.Object, _artifactVersionsRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties[ServiceConstants.SessionProperty] = _session;
            // Act
            await controller.GetReplies(artifactId, discussionId);
        }
    }
}
