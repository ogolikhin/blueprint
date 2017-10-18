﻿using ArtifactStore.Models;
using ArtifactStore.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Attributes;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ArtifactStore.Controllers
{
    [TestClass]
    public class AttachmentsControllerTests
    {
        private Mock<IAttachmentsRepository> _attachmentsRepositoryMock;
        private Mock<IArtifactPermissionsRepository> _artifactPermissionsRepositoryMock;
        private Mock<IArtifactVersionsRepository> _artifactVersionsMock;
        private Session _session;

        [TestInitialize]
        public void init()
        {            
            // Arrange
            var userId = 1;
            _session = new Session { UserId = userId };
            _artifactPermissionsRepositoryMock = new Mock<IArtifactPermissionsRepository>();
            _attachmentsRepositoryMock = new Mock<IAttachmentsRepository>();
            _artifactVersionsMock = new Mock<IArtifactVersionsRepository>();
        }

        [TestMethod]
        public void GetAttachmentsAndDocumentReferencesMethod_HasSessionRequiredAttribute()
        {
            var getAttachmentsAndDocumentReferencesMethod = typeof(AttachmentsController).GetMethod("GetAttachmentsAndDocumentReferences");
            var hasSessionRequiredAttr = getAttachmentsAndDocumentReferencesMethod.GetCustomAttributes(false).Any(a => a is SessionRequiredAttribute);
            Assert.IsTrue(hasSessionRequiredAttr);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetAttachmentsAndDocumentReferences_BadParameters_ExceptionThrown()
        {
            int artifactId = -1;
            int? versionId = null;
            int? subArtifactId = null;
            bool addDrafts = true;
            var controller = new AttachmentsController(_attachmentsRepositoryMock.Object, _artifactPermissionsRepositoryMock.Object, _artifactVersionsMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties[ServiceConstants.SessionProperty] = _session;
            // Act
            await controller.GetAttachmentsAndDocumentReferences(artifactId, versionId, subArtifactId, addDrafts);
        }

        [TestMethod]
        public async Task GetAttachmentsAndDocumentReferences_ItemNotFound_ExceptionThrown()
        {
            // arrange
            int artifactId = 1;
            int? versionId = null;
            int? subArtifactId = null;
            bool addDrafts = true;

            _artifactPermissionsRepositoryMock.Setup(a => a.GetItemInfo(1, 1, true, int.MaxValue)).ReturnsAsync((ItemInfo)null);
            var controller = new AttachmentsController(_attachmentsRepositoryMock.Object, _artifactPermissionsRepositoryMock.Object, _artifactVersionsMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties[ServiceConstants.SessionProperty] = _session;
            // Act
            try
            {
                var result = await controller.GetAttachmentsAndDocumentReferences(artifactId, versionId, subArtifactId, addDrafts);
            }
            catch (ResourceNotFoundException e)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.ArtifactNotFound, e.ErrorCode);
            }
        }

        [TestMethod]
        public async Task GetAttachmentsAndDocumentReferences_ItemNotValid_ExceptionThrown()
        {
            // arrange
            int artifactId = 1;
            int? versionId = null;
            int? subArtifactId = 2;
            bool addDrafts = true;

            _artifactPermissionsRepositoryMock.Setup(a => a.GetItemInfo(2, 1, true, int.MaxValue)).ReturnsAsync(new ItemInfo { ArtifactId = 999 });
            var controller = new AttachmentsController(_attachmentsRepositoryMock.Object, _artifactPermissionsRepositoryMock.Object, _artifactVersionsMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties[ServiceConstants.SessionProperty] = _session;
            // Act
            try
            {
                var result = await controller.GetAttachmentsAndDocumentReferences(artifactId, versionId, subArtifactId, addDrafts);
            }
            catch (BadRequestException e)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.BadRequest, e.ErrorCode);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetAttachmentsAndDocumentReferences_NoItemPermission_ExceptionThrown()
        {
            // arrange
            int artifactId = 1;
            int? versionId = null;
            int? subArtifactId = null;
            bool addDrafts = true;

            _artifactPermissionsRepositoryMock.Setup(a => a.GetItemInfo(1, 1, true, int.MaxValue)).ReturnsAsync(new ItemInfo { ArtifactId = 1 });
            _attachmentsRepositoryMock.Setup(a => a.GetAttachmentsAndDocumentReferences(artifactId, 1, versionId, subArtifactId, true, null))
                .ReturnsAsync(new FilesInfo(new List<Attachment>(), new List<DocumentReference>()));
            _artifactPermissionsRepositoryMock.Setup(a => a.GetArtifactPermissions(new List<int> { 1 }, 1, false, int.MaxValue, true))
                .ReturnsAsync(new Dictionary<int, RolePermissions> { { 1, RolePermissions.None } });

            var controller = new AttachmentsController(_attachmentsRepositoryMock.Object, _artifactPermissionsRepositoryMock.Object, _artifactVersionsMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties[ServiceConstants.SessionProperty] = _session;
            // Act
            await controller.GetAttachmentsAndDocumentReferences(artifactId, versionId, subArtifactId, addDrafts);
        }

        [TestMethod]
        public async Task GetAttachmentsAndDocumentReferences_Success_ResultsReturned()
        {
            // arrange
            int artifactId = 1;
            int? versionId = null;
            int? subArtifactId = null;
            bool addDrafts = true;

            _artifactPermissionsRepositoryMock.Setup(a => a.GetItemInfo(1, 1, true, int.MaxValue)).ReturnsAsync(new ItemInfo { ArtifactId = 1 });
            _attachmentsRepositoryMock.Setup(a => a.GetAttachmentsAndDocumentReferences(artifactId, 1, versionId, subArtifactId, true, null))
                .ReturnsAsync(
                new FilesInfo(
                    new List<Attachment> { new Attachment { AttachmentId = 123 } }, 
                    new List<DocumentReference> { new DocumentReference { ArtifactId = 123 } }));
            _artifactPermissionsRepositoryMock.Setup(a => a.GetArtifactPermissions(new List<int> { 1, 123 }, 1, false, int.MaxValue, true))
                .ReturnsAsync(new Dictionary<int, RolePermissions> { { 1, RolePermissions.Read }, { 123, RolePermissions.Read } });

            var controller = new AttachmentsController(_attachmentsRepositoryMock.Object, _artifactPermissionsRepositoryMock.Object, _artifactVersionsMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties[ServiceConstants.SessionProperty] = _session;
            // Act
            var result = await controller.GetAttachmentsAndDocumentReferences(artifactId, versionId, subArtifactId, addDrafts);
            Assert.AreEqual(123, result.Attachments[0].AttachmentId);
            Assert.AreEqual(123, result.DocumentReferences[0].ArtifactId);
        }

    }
}
