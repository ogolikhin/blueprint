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
using Model.StorytellerModel.Impl;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class AttachmentTests : TestBase
    {
        private IUser _user = null;
        private IProject _project = null;
        private uint _fileSize = (uint)RandomGenerator.RandomNumber(4096);
        private string _fileName = I18NHelper.FormatInvariant("{0}.{1}", RandomGenerator.RandomAlphaNumeric(10), "txt");
        private IFile _attachmentFile = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_user);
            _attachmentFile = FileStoreTestHelper.CreateFileWithRandomByteArray(_fileSize, _fileName, "text/plain");
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
            _attachmentFile = null;
        }

        [TestCase]
        [TestRail(146332)]
        [Description("Create & save an artifact, add attachment, publish artifact, get attachments.  Verify attachment is returned.")]
        public void GetAttachment_PublishedArtifactWithAttachment_AttachmentIsReturned()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Actor);
            var openApiAttachment = artifact.AddArtifactAttachment(_attachmentFile, _user);
            artifact.Publish();

            Attachments attachment = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                attachment = Helper.ArtifactStore.GetAttachments(artifact, _user);
            }, "'{0}' shouldn't return any error when passed a published artifact ID.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT);

            // Verify:
            Assert.AreEqual(1, attachment.AttachedFiles.Count, "List of attached files must have 1 item.");
            Assert.IsTrue(openApiAttachment.Equals(attachment.AttachedFiles[0]), "The file attachment returned from ArtifactStore doesn't match the file attachment uploaded.");
        }

        [TestCase]
        [TestRail(146333)]
        [Description("Create & save an artifact, add attachment, publish artifact, delete artifact, publish artifact, get attachments.  Verify 404 Not Found is returned.")]
        public void GetAttachment_DeletedArtifactWithAttachment_NotFound()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Actor);
            artifact.AddArtifactAttachment(_attachmentFile, _user);
            artifact.Publish();
            artifact.Delete(_user);
            artifact.Publish(_user);

            // Execute & verify:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetAttachments(artifact, _user);
            }, "'{0}' should return 404 Not Found when passed a deleted artifact ID.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT);
        }

        [TestCase]
        [TestRail(146334)]
        [Description("Create a Process artifact, add attachment, publish it, add a different attachment to User task & publish, get attachments for User task.  Verify attachment is returned.")]
        public void GetAttachment_ArtifactAndSubArtifactWithAttachment_OnlyArtifactAttachmentIsReturned()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Process);
            var addedArtifactAttachment = artifact.AddArtifactAttachment(_attachmentFile, _user);

            var process = Helper.Storyteller.GetProcess(_user, artifact.Id);
            var userTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            IFile file2 = FileStoreTestHelper.CreateFileWithRandomByteArray(_fileSize, _fileName, "text/plain");
            var addedSubArtifactAttachment = artifact.AddSubArtifactAttachment(userTask.Id, file2, _user);
            artifact.Publish();

            Assert.NotNull(addedSubArtifactAttachment, "Failed to add attachment to the sub-artifact!");
            Assert.AreEqual(file2.FileName, addedSubArtifactAttachment.FileName, "The FileName of the attached file doesn't match!");

            Attachments attachment = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                attachment = Helper.ArtifactStore.GetAttachments(artifact, _user);
            }, "'{0}' shouldn't return any error.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT);

            // Verify:
            Assert.AreEqual(1, attachment.AttachedFiles.Count, "List of attached files must have 1 item.");
            Assert.IsTrue(addedArtifactAttachment.Equals(attachment.AttachedFiles[0]), "File from attachment should have expected values, but it doesn't.");
        }

        [TestCase]
        [TestRail(154648)]
        [Description("Create a Process artifact, add attachment, publish it, add attachment to User task & publish, get attachments for User task.  Verify only the User Task's attachment is returned.")]
        public void GetAttachmentWithSubArtifactId_ArtifactAndSubArtifactWithAttachments_OnlySubArtifactAttachmentIsReturned()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Process);
            var addedArtifactAttachment = artifact.AddArtifactAttachment(_attachmentFile, _user);

            Assert.NotNull(addedArtifactAttachment, "Failed to add attachment to the artifact!");
            Assert.AreEqual(_attachmentFile.FileName, addedArtifactAttachment.FileName, "The FileName of the attached file doesn't match!");

            artifact.Publish();

            var process = Helper.Storyteller.GetProcess(_user, artifact.Id);
            var userTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            IFile file2 = FileStoreTestHelper.CreateFileWithRandomByteArray(_fileSize, _fileName, "text/plain");
            var addedSubArtifactAttachment = artifact.AddSubArtifactAttachment(userTask.Id, file2, _user);
            artifact.Publish();

            Attachments attachment = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                attachment = Helper.ArtifactStore.GetAttachments(artifact, _user, subArtifactId: userTask.Id);
            }, "'{0}?subArtifactId={1}' shouldn't return any error.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT, userTask.Id);

            // Verify:
            Assert.AreEqual(1, attachment.AttachedFiles.Count, "List of attached files must have 1 item.");
            Assert.IsTrue(addedSubArtifactAttachment.Equals(attachment.AttachedFiles[0]), "File from attachment should have expected values, but it doesn't.");
        }

        [TestCase]
        [TestRail(146335)]
        [Description("Create & publish a Process artifact, add attachment to User task & publish, delete attachment, get attachments for User task, check expectations.")]
        public void GetAttachmentWithSubArtifactId_SubArtifactWithDeletedAttachment_NoAttachmentsReturned()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);
            var process = Helper.Storyteller.GetProcess(_user, artifact.Id);
            var userTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            var result = artifact.AddSubArtifactAttachment(userTask.Id, _attachmentFile, _user);
            artifact.Publish();
            result.Delete(_user);

            Attachments attachment = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                attachment = Helper.ArtifactStore.GetAttachments(artifact, _user, subArtifactId: userTask.Id);
            }, "'{0}?subArtifactId={1}' shouldn't return any error.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT, userTask.Id);

            // Verify:
            Assert.AreEqual(0, attachment.AttachedFiles.Count, "List of attached files must be empty.");
        }

        [TestCase]
        [Explicit(IgnoreReasons.ProductBug)]    // BUG #1712
        [TestRail(154604)]
        [Description("Create a Process artifact, publish it, add attachment to User task & publish, get attachments but pass the User Task sub-artifact ID instead of the artifact ID.  "
            + "Verify 404 Not Found is returned.")]
        public void GetAttachment_SubArtifactIdPassedAsArtifactId_404NotFound()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);
            var process = Helper.Storyteller.GetProcess(_user, artifact.Id);
            var userTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            artifact.AddSubArtifactAttachment(userTask.Id, _attachmentFile, _user);
            artifact.Publish();

            var fakeArtifact = ArtifactFactory.CreateArtifact(_project,
                _user, BaseArtifactType.Process, artifactId: userTask.Id);  // Don't use Helper because this isn't a real artifact, it's just wrapping the sub-artifact ID.

            // Execute & verify:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetAttachments(fakeArtifact, _user);
            }, "'{0}' should return 404 Not Found if passed a sub-artifact ID instead of an artifact ID.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT);
        }

        [TestCase]
        [TestRail(155622)]
        [Description("Create & publish a Process artifact and an Actor artifact.  Try to get Attachments for the Process User Task but pass the Artifact ID of the Actor instead of the Process.  Verify 400 Bad Request is returned.")]
        public void GetAttachmentWithSubArtifactId_SubArtifactIdFromDifferentArtifact_400BadRequest()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);
            var process = Helper.Storyteller.GetProcess(_user, artifact.Id);
            var userTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            IArtifact artifact2 = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);

            // Execute & verify:
            Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.ArtifactStore.GetAttachments(artifact2, _user, subArtifactId: userTask.Id);
            }, "'{0}' should return 400 Bad Request if passed a sub-artifact ID that doesn't belong to the specified artifact ID.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT);
        }

        [TestCase(null)]
        [TestCase(true)]
        [TestRail(154591)]
        [Description("Create & save an artifact (don't publish), add attachment, get attachments (with or without addDrafts=true). Verify attachment is returned.")]
        public void GetAttachment_UnpublishedArtifactWithAttachment_AttachmentIsReturned(bool? addDrafts)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Actor);
            var openApiAttachment = artifact.AddArtifactAttachment(_attachmentFile, _user);

            Attachments attachment = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                attachment = Helper.ArtifactStore.GetAttachments(artifact, _user, addDrafts);
            }, "'{0}{1}' shouldn't return any error.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT,
                addDrafts == null ? string.Empty : "?addDrafts=" + addDrafts);

            // Verify:
            Assert.AreEqual(1, attachment.AttachedFiles.Count, "List of attached files must have 1 item.");
            Assert.IsTrue(openApiAttachment.Equals(attachment.AttachedFiles[0]), "The file attachment returned from ArtifactStore doesn't match the file attachment uploaded.");
        }

        [TestCase]
        [TestRail(154592)]
        [Description("Create & save an artifact (don't publish), add attachment, get attachments (with addDrafts=false).  Verify 404 Not Found is returned.")]
        public void GetAttachmentWithAddDraftsFalse_UnpublishedArtifactWithAttachment_404NotFound()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Actor);
            artifact.AddArtifactAttachment(_attachmentFile, _user);

            // Execute & verify:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetAttachments(artifact, _user, addDrafts: false);
            }, "'{0}?addDrafts=false' should return 404 Not Found.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT);
        }

        [TestCase]
        [TestRail(154646)]
        [Description("Create & publish an artifact, add attachment, get attachments (with addDrafts=false).  Verify no attachments are returned.")]
        public void GetAttachmentWithAddDraftsFalse_PublishedArtifactWithAttachmentInDraft_NoAttachmentsReturned()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            artifact.AddArtifactAttachment(_attachmentFile, _user);

            Attachments attachment = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                attachment = Helper.ArtifactStore.GetAttachments(artifact, _user, addDrafts: false);
            }, "'{0}?addDrafts=false' shouldn't return any error.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT);

            // Verify:
            Assert.AreEqual(0, attachment.AttachedFiles.Count, "List of attached files must be empty.");
        }

        [TestCase(2)]
        [TestCase(20)]
        [TestRail(154593)]
        [Description("Create & save an artifact, add multiple attachments, publish artifact, get attachments.  Verify all attachments that were added are returned.")]
        public void GetAttachment_PublishedArtifactWithMultipleAttachments_AllAttachmentsAreReturned(int numberOfAttachments)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Actor);
            List<OpenApiAttachment> openApiAttachments = new List<OpenApiAttachment>();

            for (int i = 0; i < numberOfAttachments; ++i)
            {
                IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(_fileSize, _fileName, "text/plain");
                var openApiAttachment = artifact.AddArtifactAttachment(file, _user);
                openApiAttachments.Add(openApiAttachment);
            }

            artifact.Publish();

            Attachments attachment = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                attachment = Helper.ArtifactStore.GetAttachments(artifact, _user);
            }, "'{0}' shouldn't return any error when passed a published artifact ID.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT);

            // Verify:
            Assert.AreEqual(openApiAttachments.Count, attachment.AttachedFiles.Count,
                "List of attached files should have {0} files.", openApiAttachments.Count);

            foreach (var openApiAttachment in openApiAttachments)
            {
                AttachedFile attachedFile = attachment.AttachedFiles.Find(f => f.AttachmentId == openApiAttachment.Id);
                Assert.NotNull(attachedFile, "Couldn't find file with ID '{0}' in the list of attached files!", openApiAttachment.Id);
                Assert.IsTrue(openApiAttachment.Equals(attachedFile),
                    "The file attachment returned from ArtifactStore doesn't match the file attachment uploaded.");
            }
        }

        [TestCase(0)]
        [TestCase(int.MaxValue)]
        [TestRail(154594)]
        [Description("Try to get attachments for a non-existent artifact ID.  Verify 404 Not Found is returned.")]
        public void GetAttachment_NonExistentArtifactId_404NotFound(int artifactId)
        {
            // Setup:
            IArtifact fakeArtifact = ArtifactFactory.CreateArtifact(
                _project, _user, BaseArtifactType.Actor, artifactId);   // Don't use Helper because this isn't a real artifact, it's just wrapping the bad artifact ID.

            // Execute & verify:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetAttachments(fakeArtifact, _user);
            }, "'{0}' should return 404 Not Found.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT);
        }

        [TestCase(0, Explicit = true, Reason = IgnoreReasons.ProductBug)]   // BUG #1722: Returns 400 instead of 404.
        [TestCase(int.MaxValue)]
        [TestRail(154595)]
        [Description("Create & save a Process artifact.  Try to get attachments for a non-existent sub-artifact ID.  Verify 404 Not Found is returned.")]
        public void GetAttachmentWithSubArtifactId_NonExistentSubArtifactId_404NotFound(int subArtifactId)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);

            // Execute & verify:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetAttachments(artifact, _user, subArtifactId: subArtifactId);
            }, "'{0}?subArtifactId={1}' should return 404 Not Found.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT, subArtifactId);
        }

        [TestCase]
        [TestRail(154600)]
        [Description("Create & publish an artifact, get attachments.  Verify no attachments are returned.")]
        public void GetAttachment_PublishedArtifactWithNoAttachments_NoAttachmentsReturned()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            Attachments attachment = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                attachment = Helper.ArtifactStore.GetAttachments(artifact, _user);
            }, "'{0}' shouldn't return any error.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT);

            // Verify:
            Assert.AreEqual(0, attachment.AttachedFiles.Count, "List of attached files must be empty.");
        }

        [TestCase]
        [TestRail(154597)]
        [Description("Create & publish an artifact.  Try to get attachments with a user that doesn't have permission to access the artifact.  Verify 403 Forbidden is returned.")]
        public void GetAttachment_PublishedArtifactUserHasNoPermissionToArtifact_403Forbidden()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);

            IUser userWithoutPermission = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken,
                InstanceAdminRole.BlueprintAnalytics);

            // Execute & verify:
            Assert.Throws<Http403ForbiddenException>(() =>
            {
                Helper.ArtifactStore.GetAttachments(artifact, userWithoutPermission);
            }, "'{0}' should return 403 Forbidden for a user without permission to the artifact.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT);
        }

        [TestCase]
        [TestRail(1)]
        [Description(".")]
        public void GetAttachmentSpecifyVersion_PublishedArtifactWithAttachment_AttachmentIsReturned()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            artifact.AddArtifactAttachment(_attachmentFile, _user);
            artifact.Publish();

            Attachments attachment = Helper.ArtifactStore.GetAttachments(artifact, _user, versionId: 2);
            Assert.AreEqual(1, attachment.AttachedFiles.Count, "List of attached files must have 1 item.");

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                attachment = Helper.ArtifactStore.GetAttachments(artifact, _user, versionId: 1);
            }, "'{0}' shouldn't return any error when passed a published artifact ID.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT);

            // Verify:
            Assert.AreEqual(0, attachment.AttachedFiles.Count, "List of attached files must have 1 item.");
        }

        [TestCase]
        [TestRail(2)]
        [Description(".")]
        public void GetAttachment_DraftArtifactWithAttachment_AttachmentIsReturned()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Glossary);
            artifact.AddArtifactAttachment(_attachmentFile, _user);
            
            Attachments attachment = Helper.ArtifactStore.GetAttachments(artifact, _user, addDrafts: false);
            Assert.AreEqual(0, attachment.AttachedFiles.Count, ".");

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                attachment = Helper.ArtifactStore.GetAttachments(artifact, _user);
            }, "'{0}' shouldn't return any error when passed a published artifact ID.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT);

            // Verify:
            Assert.AreEqual(1, attachment.AttachedFiles.Count, "List of attached files must have 1 item.");
        }

        // TODO: Implement GetAttachment_PublishedArtifactWithDocReferenceUserHasNoPermissionToDocReference_403Forbidden  TestRail ID: 154596
    }
}
