﻿using ArtifactStore.Models;
using ArtifactStore.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Attributes;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace ArtifactStore.Controllers
{
    [TestClass]
    public class RelationshipsControllerTests
    {
        private Mock<IArtifactPermissionsRepository> _artifactPermissionsRepositoryMock;
        private Mock<IRelationshipsRepository> _relationshipsRepositoryMock;
        private Session _session;

        [TestInitialize]
        public void Initialize()
        {
            // Arrange
            var userId = 1;
            _session = new Session { UserId = userId };

            _artifactPermissionsRepositoryMock = new Mock<IArtifactPermissionsRepository>();
            _relationshipsRepositoryMock = new Mock<IRelationshipsRepository>();
        }

        [TestMethod]
        public void GetRelationships_HasSessionRequiredAttribute_Success()
        {
            var getDiscussionMethod = typeof(RelationshipsController).GetMethod("GetRelationships");
            var hasSessionRequiredAttr = getDiscussionMethod.GetCustomAttributes(false).Any(a => a is SessionRequiredAttribute);
            Assert.IsTrue(hasSessionRequiredAttr);
        }
        [TestMethod]
        public void GetRelationshipDetails_HasSessionRequiredAttribute_Success()
        {
            var getDiscussionMethod = typeof(RelationshipsController).GetMethod("GetRelationshipDetails");
            var hasSessionRequiredAttr = getDiscussionMethod.GetCustomAttributes(false).Any(a => a is SessionRequiredAttribute);
            Assert.IsTrue(hasSessionRequiredAttr);
        }

        [TestMethod]
        public async Task GetRelationships_ArtifactHasPermission_Success()
        {
            //Arrange
            const int artifactId = 1;
            const int projectId = 10;
            const int destId = 123;
            var itemInfo = new ItemInfo { ProjectId = projectId, ArtifactId = artifactId, ItemId = artifactId };
            var permisionDictionary = new Dictionary<int, RolePermissions>();
            var resultSet = new RelationshipResultSet { ManualTraces = new List<Relationship> { new Relationship { ArtifactId = destId, ArtifactName = "test" } }, OtherTraces = new List<Relationship> { new Relationship { ArtifactId = destId, ArtifactName = "test" } } };
            permisionDictionary.Add(artifactId, RolePermissions.Read);
            permisionDictionary.Add(destId, RolePermissions.Read);

            _artifactPermissionsRepositoryMock.Setup(m => m.GetItemInfo(artifactId, _session.UserId, true, int.MaxValue)).ReturnsAsync(itemInfo);
            _artifactPermissionsRepositoryMock.Setup(m => m.GetArtifactPermissionsInChunks(It.IsAny<List<int>>(), _session.UserId, false, int.MaxValue, true)).ReturnsAsync(permisionDictionary);

            _relationshipsRepositoryMock.Setup(m => m.GetRelationships(artifactId, _session.UserId, true)).ReturnsAsync(resultSet);
            var controller = new RelationshipsController(_relationshipsRepositoryMock.Object, _artifactPermissionsRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties[ServiceConstants.SessionProperty] = _session;
            //Act
            var result = await controller.GetRelationships(artifactId);

            //Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ManualTraces[0].HasAccess);
            Assert.IsTrue(result.OtherTraces[0].HasAccess);
        }

        [TestMethod]
        public async Task GetRelationships_ArtifactCanEdit_Success()
        {
            //Arrange
            const int artifactId = 1;
            const int projectId = 10;
            const int destId = 123;
            var itemInfo = new ItemInfo { ProjectId = projectId, ArtifactId = artifactId, ItemId = artifactId };
            var permisionDictionary = new Dictionary<int, RolePermissions>();
            var resultSet = new RelationshipResultSet { ManualTraces = new List<Relationship> { new Relationship { ArtifactId = destId, ArtifactName = "test" } }, OtherTraces = new List<Relationship> { new Relationship { ArtifactId = destId, ArtifactName = "test" } } };
            permisionDictionary.Add(artifactId, RolePermissions.Read | RolePermissions.Trace | RolePermissions.Edit);       
            permisionDictionary.Add(destId, RolePermissions.Read);

            _artifactPermissionsRepositoryMock.Setup(m => m.GetItemInfo(artifactId, _session.UserId, true, int.MaxValue)).ReturnsAsync(itemInfo);
            _artifactPermissionsRepositoryMock.Setup(m => m.GetArtifactPermissionsInChunks(It.IsAny<List<int>>(), _session.UserId, false, int.MaxValue, true)).ReturnsAsync(permisionDictionary);

            _relationshipsRepositoryMock.Setup(m => m.GetRelationships(artifactId, _session.UserId, true)).ReturnsAsync(resultSet);
            var controller = new RelationshipsController(_relationshipsRepositoryMock.Object, _artifactPermissionsRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties[ServiceConstants.SessionProperty] = _session;
            //Act
            var result = await controller.GetRelationships(artifactId);

            //Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.CanEdit);            
        }

        [TestMethod]
        public async Task GetRelationships_ArtifactCannotEdit_Success()
        {
            //Arrange
            const int artifactId = 1;
            const int projectId = 10;
            const int destId = 123;
            var itemInfo = new ItemInfo { ProjectId = projectId, ArtifactId = artifactId, ItemId = artifactId };
            var permisionDictionary = new Dictionary<int, RolePermissions>();
            var resultSet = new RelationshipResultSet { ManualTraces = new List<Relationship> { new Relationship { ArtifactId = destId, ArtifactName = "test" } }, OtherTraces = new List<Relationship> { new Relationship { ArtifactId = destId, ArtifactName = "test" } } };
            permisionDictionary.Add(artifactId, RolePermissions.Read | RolePermissions.Edit);                
            permisionDictionary.Add(destId, RolePermissions.Read);

            _artifactPermissionsRepositoryMock.Setup(m => m.GetItemInfo(artifactId, _session.UserId, true, int.MaxValue)).ReturnsAsync(itemInfo);
            _artifactPermissionsRepositoryMock.Setup(m => m.GetArtifactPermissionsInChunks(It.IsAny<List<int>>(), _session.UserId, false, int.MaxValue, true)).ReturnsAsync(permisionDictionary);

            _relationshipsRepositoryMock.Setup(m => m.GetRelationships(artifactId, _session.UserId, true)).ReturnsAsync(resultSet);
            var controller = new RelationshipsController(_relationshipsRepositoryMock.Object, _artifactPermissionsRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties[ServiceConstants.SessionProperty] = _session;
            //Act
            var result = await controller.GetRelationships(artifactId);

            //Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.CanEdit);
        }

        [TestMethod]
        public async Task GetRelationships_RelationshipIsReadOnly_Success()
        {
            //Arrange
            const int artifactId = 1;
            const int projectId = 10;
            const int destId = 123;
            var itemInfo = new ItemInfo { ProjectId = projectId, ArtifactId = artifactId, ItemId = artifactId };
            var permisionDictionary = new Dictionary<int, RolePermissions>();
            var resultSet = new RelationshipResultSet { ManualTraces = new List<Relationship> { new Relationship { ArtifactId = destId, ArtifactName = "test" } }, OtherTraces = new List<Relationship> { new Relationship { ArtifactId = destId, ArtifactName = "test" } } };
            permisionDictionary.Add(artifactId, RolePermissions.Read | RolePermissions.Trace | RolePermissions.Edit);
            permisionDictionary.Add(destId, RolePermissions.Read | RolePermissions.Edit);

            _artifactPermissionsRepositoryMock.Setup(m => m.GetItemInfo(artifactId, _session.UserId, true, int.MaxValue)).ReturnsAsync(itemInfo);
            _artifactPermissionsRepositoryMock.Setup(m => m.GetArtifactPermissionsInChunks(It.IsAny<List<int>>(), _session.UserId, false, int.MaxValue, true)).ReturnsAsync(permisionDictionary);

            _relationshipsRepositoryMock.Setup(m => m.GetRelationships(artifactId, _session.UserId, true)).ReturnsAsync(resultSet);
            var controller = new RelationshipsController(_relationshipsRepositoryMock.Object, _artifactPermissionsRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties[ServiceConstants.SessionProperty] = _session;
            //Act
            var result = await controller.GetRelationships(artifactId);

            //Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ManualTraces[0].ReadOnly);
            Assert.IsTrue(result.OtherTraces[0].ReadOnly);
        }

        [TestMethod]
        public async Task GetRelationships_RelationshipIsNotReadOnly_Success()
        {
            //Arrange
            const int artifactId = 1;
            const int projectId = 10;
            const int destId = 123;
            var itemInfo = new ItemInfo { ProjectId = projectId, ArtifactId = artifactId, ItemId = artifactId };
            var permisionDictionary = new Dictionary<int, RolePermissions>();
            var resultSet = new RelationshipResultSet { ManualTraces = new List<Relationship> { new Relationship { ArtifactId = destId, ArtifactName = "test" } }, OtherTraces = new List<Relationship> { new Relationship { ArtifactId = destId, ArtifactName = "test" } } };
            permisionDictionary.Add(artifactId, RolePermissions.Read | RolePermissions.Trace | RolePermissions.Edit);
            permisionDictionary.Add(destId, RolePermissions.Read | RolePermissions.Trace | RolePermissions.Edit);

            _artifactPermissionsRepositoryMock.Setup(m => m.GetItemInfo(artifactId, _session.UserId, true, int.MaxValue)).ReturnsAsync(itemInfo);
            _artifactPermissionsRepositoryMock.Setup(m => m.GetArtifactPermissionsInChunks(It.IsAny<List<int>>(), _session.UserId, false, int.MaxValue, true)).ReturnsAsync(permisionDictionary);

            _relationshipsRepositoryMock.Setup(m => m.GetRelationships(artifactId, _session.UserId, true)).ReturnsAsync(resultSet);
            var controller = new RelationshipsController(_relationshipsRepositoryMock.Object, _artifactPermissionsRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties[ServiceConstants.SessionProperty] = _session;
            //Act
            var result = await controller.GetRelationships(artifactId);

            //Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.ManualTraces[0].ReadOnly);
            Assert.IsFalse(result.OtherTraces[0].ReadOnly);
        }

        [ExpectedException(typeof(HttpResponseException))]
        [TestMethod]
        public async Task GetRelationships_ArtifactHasNoPermission_ExcetionThrown()
        {
            //Arrange
            const int artifactId = 1;
            const int projectId = 10;
            const int destId = 123;
            var itemInfo = new ItemInfo { ProjectId = projectId, ArtifactId = artifactId, ItemId = artifactId };
            var permisionDictionary = new Dictionary<int, RolePermissions>();
            var resultSet = new RelationshipResultSet { ManualTraces = new List<Relationship> { new Relationship { ArtifactId = destId, ArtifactName = "test" } }, OtherTraces = new List<Relationship> { new Relationship { ArtifactId = destId, ArtifactName = "test" } } };

            _artifactPermissionsRepositoryMock.Setup(m => m.GetItemInfo(artifactId, _session.UserId, true, int.MaxValue)).ReturnsAsync(itemInfo);
            _artifactPermissionsRepositoryMock.Setup(m => m.GetArtifactPermissionsInChunks(It.IsAny<List<int>>(), _session.UserId, false, int.MaxValue, true)).ReturnsAsync(permisionDictionary);

            _relationshipsRepositoryMock.Setup(m => m.GetRelationships(artifactId, _session.UserId, true)).ReturnsAsync(resultSet);
            var controller = new RelationshipsController(_relationshipsRepositoryMock.Object, _artifactPermissionsRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties[ServiceConstants.SessionProperty] = _session;
            //Act
            var result = await controller.GetRelationships(artifactId);
        }

        [TestMethod]
        public async Task GetRelationships_ArtifactNoPermission_Success()
        {
            //Arrange
            const int artifactId = 1;
            const int projectId = 10;
            const int destId = 123;
            var itemInfo = new ItemInfo { ProjectId = projectId, ArtifactId = artifactId, ItemId = artifactId };
            var permisionDictionary = new Dictionary<int, RolePermissions>();
            var resultSet = new RelationshipResultSet { ManualTraces = new List<Relationship> { new Relationship { ArtifactId = destId, ArtifactName = "test" } }, OtherTraces = new List<Relationship> { new Relationship { ArtifactId = destId, ArtifactName = "test" } } };
            permisionDictionary.Add(artifactId, RolePermissions.Read);

            _artifactPermissionsRepositoryMock.Setup(m => m.GetItemInfo(artifactId, _session.UserId, true, int.MaxValue)).ReturnsAsync(itemInfo);
            _artifactPermissionsRepositoryMock.Setup(m => m.GetArtifactPermissionsInChunks(It.IsAny<List<int>>(), _session.UserId, false, int.MaxValue, true)).ReturnsAsync(permisionDictionary);

            _relationshipsRepositoryMock.Setup(m => m.GetRelationships(artifactId, _session.UserId, true)).ReturnsAsync(resultSet);
            var controller = new RelationshipsController(_relationshipsRepositoryMock.Object, _artifactPermissionsRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties[ServiceConstants.SessionProperty] = _session;
            //Act
            var result = await controller.GetRelationships(artifactId);

            //Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.ManualTraces[0].HasAccess);
            Assert.IsFalse(result.OtherTraces[0].HasAccess);
        }

        [ExpectedException (typeof(HttpResponseException))]
        [TestMethod]
        public async Task GetRelationships_BadSubartifactId_ExceptionThrown()
        {
            //Arrange
            const int artifactId = 1;
            const int subartifactId = 999;
            const int projectId = 10;
            var itemInfo = new ItemInfo { ProjectId = projectId, ArtifactId = artifactId, ItemId = artifactId };
            var permisionDictionary = new Dictionary<int, RolePermissions>();

            var controller = new RelationshipsController(_relationshipsRepositoryMock.Object, _artifactPermissionsRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            controller.Request.Properties[ServiceConstants.SessionProperty] = _session;
            //Act
            var result = await controller.GetRelationships(artifactId, subartifactId);
        }

        [TestMethod]
        public async Task GetRelationships_GoodSubartifactId_Success()
        {
            //Arrange
            const int artifactId = 1;
            const int subartifactId = 999;
            const int projectId = 10;
            const int destId = 123;
            var itemInfo = new ItemInfo { ProjectId = projectId, ArtifactId = artifactId, ItemId = artifactId };
            var permisionDictionary = new Dictionary<int, RolePermissions>();
            var resultSet = new RelationshipResultSet { ManualTraces = new List<Relationship> { new Relationship { ArtifactId = destId, ArtifactName = "test" } }, OtherTraces = new List<Relationship> { new Relationship { ArtifactId = destId, ArtifactName = "test" } } };
            permisionDictionary.Add(artifactId, RolePermissions.Read);
            permisionDictionary.Add(destId, RolePermissions.Read);

            _artifactPermissionsRepositoryMock.Setup(m => m.GetItemInfo(artifactId, _session.UserId, true, int.MaxValue)).ReturnsAsync(itemInfo);
            _artifactPermissionsRepositoryMock.Setup(m => m.GetItemInfo(subartifactId, _session.UserId, true, int.MaxValue)).ReturnsAsync(itemInfo);
            _artifactPermissionsRepositoryMock.Setup(m => m.GetArtifactPermissionsInChunks(It.IsAny<List<int>>(), _session.UserId, false, int.MaxValue, true)).ReturnsAsync(permisionDictionary);

            _relationshipsRepositoryMock.Setup(m => m.GetRelationships(artifactId, _session.UserId, true)).ReturnsAsync(resultSet);
            var controller = new RelationshipsController(_relationshipsRepositoryMock.Object, _artifactPermissionsRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties[ServiceConstants.SessionProperty] = _session;
            //Act
            var result = await controller.GetRelationships(artifactId);

            //Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ManualTraces[0].HasAccess);
            Assert.IsTrue(result.OtherTraces[0].HasAccess);
        }

        [ExpectedException(typeof(HttpResponseException))]
        [TestMethod]
        public async Task GetRelationshipsDetails_BadartifactId_ExceptionThrown()
        {
            //Arrange
            const int artifactId = 0;
            var controller = new RelationshipsController(_relationshipsRepositoryMock.Object, _artifactPermissionsRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties[ServiceConstants.SessionProperty] = _session;
            //Act
            var result = await controller.GetRelationshipDetails(artifactId);
        }

        [ExpectedException(typeof(HttpResponseException))]
        [TestMethod]
        public async Task GetRelationshipDetails_ArtifactNoPermission_ExceptionThrown()
        {
            //Arrange
            const int artifactId = 1;
            const int projectId = 10;
            var itemInfo = new ItemInfo { ProjectId = projectId, ArtifactId = artifactId, ItemId = artifactId };
            var permisionDictionary = new Dictionary<int, RolePermissions>();

            _artifactPermissionsRepositoryMock.Setup(m => m.GetArtifactPermissions(It.IsAny<IEnumerable<int>>(), _session.UserId, false, int.MaxValue, true)).ReturnsAsync(permisionDictionary);

            var controller = new RelationshipsController(_relationshipsRepositoryMock.Object, _artifactPermissionsRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties[ServiceConstants.SessionProperty] = _session;
            //Act
            var result = await controller.GetRelationshipDetails(artifactId);
        }

        [TestMethod]
        public async Task GetRelationshipDetails_ArtifactHasPermission_Success()
        {
            //Arrange
            const int artifactId = 1;
            const int projectId = 10;
            var itemInfo = new ItemInfo { ProjectId = projectId, ArtifactId = artifactId, ItemId = artifactId };
            var permisionDictionary = new Dictionary<int, RolePermissions>();
            permisionDictionary.Add(artifactId, RolePermissions.Read);

            _artifactPermissionsRepositoryMock.Setup(m => m.GetItemInfo(It.IsAny<int>(), _session.UserId, true, int.MaxValue)).ReturnsAsync(new ItemInfo { });
            _artifactPermissionsRepositoryMock.Setup(m => m.GetArtifactPermissions(It.IsAny<IEnumerable<int>>(), _session.UserId, false, int.MaxValue, true)).ReturnsAsync(permisionDictionary);
            _relationshipsRepositoryMock.Setup(m => m.GetRelationshipExtendedInfo(artifactId, _session.UserId, true, int.MaxValue)).ReturnsAsync(new RelationshipExtendedInfo { ArtifactId = 1 });

            var controller = new RelationshipsController(_relationshipsRepositoryMock.Object, _artifactPermissionsRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties[ServiceConstants.SessionProperty] = _session;
            //Act
            var result = await controller.GetRelationshipDetails(artifactId);

            //Assert
            Assert.AreEqual(1, result.ArtifactId);
        }
    }
}
