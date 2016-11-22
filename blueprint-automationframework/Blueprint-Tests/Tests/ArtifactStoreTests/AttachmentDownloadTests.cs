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
using Model.ArtifactModel;
using Model.Factories;
using Model.ArtifactModel.Impl;
using Model.NovaModel;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class AttachmentDownloadTests : TestBase
    {
        private IUser _adminUser = null;
        private IUser _viewerUser = null;
        private IProject _project = null;
        private uint _fileSize = (uint)RandomGenerator.RandomNumber(4096);
        private string _fileName = null;
        private IFile _attachmentFile = null;

        private const string _fileType = "text/plain";
        private DateTime defaultExpireTime = DateTime.Now.AddDays(2);//Currently Nova set ExpireTime 2 days from today for newly uploaded file

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            _project = ProjectFactory.GetProject(_adminUser);
            _fileName = I18NHelper.FormatInvariant("{0}.{1}", RandomGenerator.RandomAlphaNumeric(10), "txt");
            _attachmentFile = FileStoreTestHelper.CreateFileWithRandomByteArray(_fileSize, _fileName, "text/plain");
            _viewerUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);
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
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _adminUser, BaseArtifactType.Actor);
            artifact.AddArtifactAttachment(_attachmentFile, _adminUser);
            artifact.Publish();

            Attachments attachment = null;
            attachment = Helper.ArtifactStore.GetAttachments(artifact, _viewerUser);
            int fileId = attachment.AttachedFiles[0].AttachmentId;
            IFile downloadedFile = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                downloadedFile = Helper.ArtifactStore.GetAttachmentFile(_viewerUser, artifact.Id, fileId);
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
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _adminUser, BaseArtifactType.BusinessProcess);
            artifact.AddArtifactAttachment(_attachmentFile, _adminUser);

            Attachments attachment = null;
            attachment = Helper.ArtifactStore.GetAttachments(artifact, _adminUser);
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
        [Description("Publish artifact, attach file and publish (version 2), delete attachment and publish changes, download attached file for version 2, check that file has expected name and content.")]
        public void GetAttachmentFile_ForHistoricalVersionOfArtifact_FileIsReturned()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.DomainDiagram);
            artifact.AddArtifactAttachment(_attachmentFile, _adminUser);
            artifact.Publish(_adminUser);
            // now artifact has attachment in version 2
            int versionId = 2;

            Attachments attachment = null;
            attachment = Helper.ArtifactStore.GetAttachments(artifact, _adminUser);
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
        [Description("Publish artifact, attach file and publish (version 2), delete artifact and publish changes, download attached file for version 2, check that file has expected name and content.")]
        public void GetAttachmentFile_ForHistoricalVersionOfDeletedArtifact_FileIsReturned()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.GenericDiagram);
            artifact.AddArtifactAttachment(_attachmentFile, _adminUser);
            artifact.Publish(_adminUser);
            // now artifact has attachment in version 2
            int versionId = 2;

            Attachments attachment = null;
            attachment = Helper.ArtifactStore.GetAttachments(artifact, _adminUser);
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
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UseCase);
            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(_adminUser, artifact.Id);
            Assert.AreEqual(3, subArtifacts.Count, "Use Case should have 3 subartifacts.");
            var subArtifact = Helper.ArtifactStore.GetSubartifact(_adminUser, artifact.Id, subArtifacts[0].Id);
            var attachmentFile2 = FileStoreTestHelper.UploadNovaFileToFileStore(_adminUser, _fileName, _fileType, defaultExpireTime,
                Helper.FileStore);

            ArtifactStoreHelper.AddSubArtifactAttachmentAndSave(_adminUser, artifact, subArtifact, new List<INovaFile> { attachmentFile2 },
                Helper.ArtifactStore);
            artifact.Publish();
            Attachments attachment = Helper.ArtifactStore.GetAttachments(artifact, _adminUser, subArtifactId: subArtifact.Id);
            Assert.AreEqual(1, attachment.AttachedFiles.Count, "SubArtifact should have 1 file attached.");
            int attachmentIdToDownload = attachment.AttachedFiles[0].AttachmentId;
            IFile downloadedFile = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                downloadedFile = Helper.ArtifactStore.GetAttachmentFile(_viewerUser, artifact.Id, attachmentIdToDownload);
            }, "File download shouldn't return any error.");

            // Verify:
            FileStoreTestHelper.AssertFilesAreIdentical(attachmentFile2, downloadedFile, compareIds: false);
        }

        #endregion Positive Tests

        [TestCase]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestRail(191150)]
        [Description("Add attachment to the saved never published artifact, save it, download attached file, check that file has expected name and content.")]
        public void GetAttachmentFile_SubArtifactWithDeletedAttachment_Returns404()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.Process);
            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(_adminUser, artifact.Id);
            Assert.AreEqual(5, subArtifacts.Count, "Process should have 5 subartifacts.");
            var attachmentFile2 = FileStoreTestHelper.UploadNovaFileToFileStore(_adminUser, _fileName, _fileType, defaultExpireTime,
                Helper.FileStore);

            // User Task is subArtifacts[2]
            var subArtifact = Helper.ArtifactStore.GetSubartifact(_adminUser, artifact.Id, subArtifacts[2].Id);
            ArtifactStoreHelper.AddSubArtifactAttachmentAndSave(_adminUser, artifact, subArtifact, new List<INovaFile> { attachmentFile2 },
                Helper.ArtifactStore);
            artifact.Publish();
            Attachments attachment = Helper.ArtifactStore.GetAttachments(artifact, _adminUser, subArtifactId: subArtifacts[2].Id);
            Assert.AreEqual(1, attachment.AttachedFiles.Count, "SubArtifact should have 1 file attached.");
            int attachmentIdToDownload = attachment.AttachedFiles[0].AttachmentId;
            ArtifactStoreHelper.DeleteSubArtifactAttachmentAndSave(_adminUser, artifact, subArtifact, attachmentIdToDownload,
                Helper.ArtifactStore);

            // Execute:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetAttachmentFile(_adminUser, artifact.Id, attachmentIdToDownload);
            }, "Should return 404 error.");
        }
    }
}
