using System;
using System.Collections.Generic;
using Common;
using Helper;
using Model;
using NUnit.Framework;
using Utilities;
using CustomAttributes;
using TestCommon;
using Utilities.Factories;
using Model.ArtifactModel.Enums;
using Model.Factories;
using Model.NovaModel;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class AttachmentDownloadTests : TestBase
    {
        private IUser _adminUser = null;
        private IProject _project = null;
        private string _fileName = null;
        private INovaFile _attachmentFile = null;

        private const string _fileType = "text/plain";
        private DateTime defaultExpireTime = DateTime.Now.AddDays(2);//Currently Nova set ExpireTime 2 days from today for newly uploaded file

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            _project = ProjectFactory.GetProject(_adminUser);
            _fileName = I18NHelper.FormatInvariant("{0}.{1}", RandomGenerator.RandomAlphaNumeric(10), "txt");
            _attachmentFile = FileStoreTestHelper.UploadNovaFileToFileStore(_adminUser, _fileName, _fileType, defaultExpireTime, Helper.FileStore);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
            _attachmentFile = null;
        }

        #region Positive Tests

        [TestCase]
        [TestRail(191143)]
        [Description("Add attachment to the artifact, publish it, download attached file, check that file has expected name and content.")]
        public void GetAttachmentFile_PublishedArtifactWithAttachment_FileIsReturned()
        {
            // Setup:
            var artifact = Helper.CreateNovaArtifact(_adminUser, _project, ItemTypePredefined.Actor);
            ArtifactStoreHelper.AddArtifactAttachmentAndSave(_adminUser, artifact, _attachmentFile, Helper.ArtifactStore, shouldLockArtifact: false);
            artifact.Publish(_adminUser);

            var viewerUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);
            var attachment = Helper.ArtifactStore.GetAttachments(viewerUser, artifact.Id);
            int fileId = attachment.AttachedFiles[0].AttachmentId;
            IFile downloadedFile = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                downloadedFile = Helper.ArtifactStore.GetAttachmentFile(viewerUser, artifact.Id, fileId);
            }, "Getting attached file shouldn't return any error.");

            // Verify:
            FileStoreTestHelper.AssertFilesAreIdentical(_attachmentFile, downloadedFile, compareIds: false);
        }

        [TestCase]
        [TestRail(191145)]
        [Description("Add attachment to the saved never published artifact, save it, download attached file, check that file has expected name and content.")]
        public void GetAttachmentFile_SavedUnpublishedArtifactWithAttachment_FileIsReturned()
        {
            // Setup:
            var artifact = Helper.CreateNovaArtifact(_adminUser, _project, ItemTypePredefined.BusinessProcess);
            ArtifactStoreHelper.AddArtifactAttachmentAndSave(_adminUser, artifact, _attachmentFile, Helper.ArtifactStore, shouldLockArtifact: false);

            var attachment = Helper.ArtifactStore.GetAttachments(_adminUser, artifact.Id);
            int fileId = attachment.AttachedFiles[0].AttachmentId;
            IFile downloadedFile = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                downloadedFile = Helper.ArtifactStore.GetAttachmentFile(_adminUser, artifact.Id, fileId);
            }, "Getting attached file shouldn't return any error.");

            // Verify:
            FileStoreTestHelper.AssertFilesAreIdentical(_attachmentFile, downloadedFile, compareIds: false);
        }

        [TestCase]
        [TestRail(191153)]
        [Description("Publish artifact, attach file and publish (version 2), delete attachment and publish changes, download attached file for version 2, " +
                     "check that file has expected name and content.")]
        public void GetAttachmentFile_ForHistoricalVersionOfArtifact_FileIsReturned()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.DomainDiagram);
            ArtifactStoreHelper.AddArtifactAttachmentAndSave(_adminUser, artifact, _attachmentFile, Helper.ArtifactStore);
            artifact.Publish(_adminUser);

            // now artifact has attachment in version 2
            int versionId = 2;

            var attachment = Helper.ArtifactStore.GetAttachments(_adminUser, artifact.Id);
            int fileId = attachment.AttachedFiles[0].AttachmentId;

            ArtifactStoreHelper.DeleteArtifactAttachmentAndSave(_adminUser, artifact, fileId, Helper.ArtifactStore);
            artifact.Publish(_adminUser);

            IFile downloadedFile = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                downloadedFile = Helper.ArtifactStore.GetAttachmentFile(_adminUser, artifact.Id, fileId, versionId: versionId);
            }, "Getting attached file shouldn't return any error.");

            // Verify:
            FileStoreTestHelper.AssertFilesAreIdentical(_attachmentFile, downloadedFile, compareIds: false);
        }

        [TestCase]
        [TestRail(191157)]
        [Description("Publish artifact, attach file and publish (version 2), delete artifact and publish changes, download attached file for version 2, " +
                     "check that file has expected name and content.")]
        public void GetAttachmentFile_ForHistoricalVersionOfDeletedArtifact_FileIsReturned()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.GenericDiagram);
            ArtifactStoreHelper.AddArtifactAttachmentAndSave(_adminUser, artifact, _attachmentFile, Helper.ArtifactStore);
            artifact.Publish(_adminUser);

            // now artifact has attachment in version 2
            int versionId = 2;

            var attachment = Helper.ArtifactStore.GetAttachments(_adminUser, artifact.Id);
            int fileId = attachment.AttachedFiles[0].AttachmentId;

            artifact.Delete(_adminUser);
            artifact.Publish(_adminUser);

            IFile downloadedFile = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                downloadedFile = Helper.ArtifactStore.GetAttachmentFile(_adminUser, artifact.Id, fileId, versionId: versionId);
            }, "Getting attached file shouldn't return any error.");

            // Verify:
            FileStoreTestHelper.AssertFilesAreIdentical(_attachmentFile, downloadedFile, compareIds: false);
        }

        [TestCase]
        [TestRail(191151)]
        [Description("Add attachment to subartifact, publish it, download attached file, check that file has expected name and content.")]
        public void GetAttachmentFile_SubArtifactWithAttachment_ReturnsExpectedFile()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.UseCase);
            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(_adminUser, artifact.Id);
            Assert.AreEqual(3, subArtifacts.Count, "Use Case should have 3 subartifacts.");

            var subArtifact = Helper.ArtifactStore.GetSubartifact(_adminUser, artifact.Id, subArtifacts[0].Id);

            ArtifactStoreHelper.AddSubArtifactAttachmentsAndSave(_adminUser, artifact, subArtifact, new List<INovaFile> { _attachmentFile },
                Helper.ArtifactStore);
            artifact.Publish(_adminUser);

            var viewerUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);
            var attachment = Helper.ArtifactStore.GetAttachments(viewerUser, artifact.Id, subArtifactId: subArtifact.Id);
            Assert.AreEqual(1, attachment.AttachedFiles.Count, "SubArtifact should have 1 file attached.");

            int attachmentIdToDownload = attachment.AttachedFiles[0].AttachmentId;
            IFile downloadedFile = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                downloadedFile = Helper.ArtifactStore.GetAttachmentFile(viewerUser, artifact.Id, attachmentIdToDownload);
            }, "File download shouldn't return any error.");

            // Verify:
            FileStoreTestHelper.AssertFilesAreIdentical(_attachmentFile, downloadedFile, compareIds: false);
        }

        #endregion Positive Tests

        [TestCase]
        [TestRail(191150)]
        [Description("Publish a Process artifact.  Add an attachment to a sub-artifact and publish.  Then delete the attachment.  " +
                     "Try to get the attachment.  Verify 404 Not Found is returned.")]
        public void GetAttachmentFile_SubArtifactWithDeletedAttachment_Returns404()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.Process);
            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(_adminUser, artifact.Id);
            Assert.AreEqual(5, subArtifacts.Count, "Process should have 5 subartifacts.");

            // User Task is subArtifacts[2] - Add attachment to the sub-artifact.
            var subArtifact = Helper.ArtifactStore.GetSubartifact(_adminUser, artifact.Id, subArtifacts[2].Id);
            ArtifactStoreHelper.AddSubArtifactAttachmentsAndSave(_adminUser, artifact, subArtifact, new List<INovaFile> { _attachmentFile },
                Helper.ArtifactStore);
            artifact.Publish(_adminUser);

            // Verify attachment was added.
            var attachment = Helper.ArtifactStore.GetAttachments(_adminUser, artifact.Id, subArtifactId: subArtifacts[2].Id);
            Assert.AreEqual(1, attachment.AttachedFiles.Count, "SubArtifact should have 1 file attached.");

            // Delete the attachment.
            int attachmentIdToDownload = attachment.AttachedFiles[0].AttachmentId;
            ArtifactStoreHelper.DeleteSubArtifactAttachmentAndSave(_adminUser, artifact, subArtifact, attachmentIdToDownload,
                Helper.ArtifactStore);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetAttachmentFile(_adminUser, artifact.Id, attachmentIdToDownload);
            }, "Should return 404 error.");

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, "Attachment not found");
        }
    }
}
