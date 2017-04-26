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
using Model.Factories;
using Model.ArtifactModel.Impl;
using Model.StorytellerModel.Impl;
using Model.NovaModel;
using Model.Common.Enums;
using Model.ArtifactModel.Enums;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class AttachmentTests : TestBase
    {
        private IUser _adminUser = null;
        private IUser _authorUser = null;
        private IProject _project = null;
        private uint _fileSize = (uint)RandomGenerator.RandomNumber(4096);
        private string _fileName = null;
        private IFile _attachmentFile = null;

        private const string _fileType = "text/plain";
        private DateTime defaultExpireTime = DateTime.Now.AddDays(2); // Currently Nova set ExpireTime 2 days from today for newly uploaded file

        private const string ITEM_DOES_NOT_EXIST_OR_NO_PERMISSION = "You have attempted to access an item that does not exist or you do not have permission to view.";

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            
            _project = ProjectFactory.GetProject(_adminUser);
            _fileName = I18NHelper.FormatInvariant("{0}.{1}", RandomGenerator.RandomAlphaNumeric(10), "txt");
            _attachmentFile = FileStoreTestHelper.CreateFileWithRandomByteArray(_fileSize, _fileName, "text/plain");
            _authorUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
            _attachmentFile = null;
        }

        /******************************************************************************************
        * NOTE: Some of these tests use OpenAPI to add attachments because we need the Attachment
        * ID to compare with what was returned by the Nova GetAttachments call.
        ******************************************************************************************/

        #region Positive tests

        [TestCase]
        [TestRail(146332)]
        [Description("Create & save an artifact, add attachment, publish artifact, get attachments.  Verify attachment is returned.")]
        public void GetAttachment_PublishedArtifactWithAttachment_AttachmentIsReturned()
        {
            // Setup:
            var artifact = Helper.CreateNovaArtifact(_adminUser, _project, ItemTypePredefined.Actor);
            var openApiAttachment = Helper.OpenApi.AddArtifactAttachment(_adminUser, _project.Id, artifact.Id, _attachmentFile);
            artifact.Publish(_adminUser);

            var viewerUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);

            Attachments attachment = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                attachment = Helper.ArtifactStore.GetAttachments(viewerUser, artifact.Id);
            }, "'GET {0}' shouldn't return any error when passed a published artifact ID.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT);

            // Verify:
            Assert.AreEqual(1, attachment.AttachedFiles.Count, "List of attached files must have 1 item.");
            OpenApiAttachment.AssertAreEqual(openApiAttachment, attachment.AttachedFiles[0]);
            Assert.AreEqual(_attachmentFile.FileName, attachment.AttachedFiles[0].FileName, "The FileName returned is wrong!");

            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_adminUser, artifact.Id);
            Assert.AreEqual(attachment.AttachedFiles[0].UploadedDate, artifactDetails.LastEditedOn,
                "UploadedDate for published artifact's attachment should be equal to LastEditedOn date of artifact");

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _adminUser, artifact.Id, ItemIndicatorFlags.HasAttachmentsOrDocumentRefs);
        }

        [TestCase]
        [TestRail(146334)]
        [Description("Create a Process artifact, add attachment, publish it, add a different attachment to User task & publish, " +
                     "get attachments for User task.  Verify attachment is returned.")]
        public void GetAttachment_ArtifactAndSubArtifactWithAttachment_OnlyArtifactAttachmentIsReturned()
        {
            // Setup:
            var artifact = Helper.CreateNovaArtifact(_adminUser, _project, ItemTypePredefined.Process);
            var openApiAttachment = Helper.OpenApi.AddArtifactAttachment(_adminUser, _project.Id, artifact.Id, _attachmentFile);

            var process = Helper.Storyteller.GetProcess(_adminUser, artifact.Id);
            var userTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            var file2 = FileStoreTestHelper.CreateFileWithRandomByteArray(_fileSize, _fileName, "text/plain");
            var addedSubArtifactAttachment = Helper.OpenApi.AddSubArtifactAttachment(_adminUser, _project.Id, artifact.Id, userTask.Id, file2);
            artifact.Publish(_adminUser);

            Assert.NotNull(addedSubArtifactAttachment, "Failed to add attachment to the sub-artifact!");
            Assert.AreEqual(file2.FileName, addedSubArtifactAttachment.FileName, "The FileName of the attached file doesn't match!");

            Attachments attachment = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                attachment = Helper.ArtifactStore.GetAttachments(_adminUser, artifact.Id);
            }, "'GET {0}' shouldn't return any error.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT);

            // Verify:
            Assert.AreEqual(1, attachment.AttachedFiles.Count, "List of attached files must have 1 item.");
            OpenApiAttachment.AssertAreEqual(openApiAttachment, attachment.AttachedFiles[0]);

            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_adminUser, artifact.Id);
            Assert.AreEqual(attachment.AttachedFiles[0].UploadedDate, artifactDetails.LastEditedOn,
                "UploadedDate for published artifact's attachment should be equal to LastEditedOn date of artifact");

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _adminUser, artifact.Id, ItemIndicatorFlags.HasAttachmentsOrDocumentRefs);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _adminUser, artifact.Id, ItemIndicatorFlags.HasAttachmentsOrDocumentRefs, userTask.Id);
        }
        
        [TestCase(ItemTypePredefined.Process)]
        [TestCase(ItemTypePredefined.UseCase)]
        [TestRail(154648)]
        [Description("Create a Use Case artifact, add attachment, publish it, add attachment to Precondition (subArtifact) & publish, get attachments for Precondition.  " +
                     "Verify only the Precondition's attachment is returned.")]
        public void GetAttachmentWithSubArtifactId_ArtifactAndSubArtifactWithAttachments_OnlySubArtifactAttachmentIsReturned(ItemTypePredefined artifactType)
        {
            // Setup:
            var artifact = Helper.CreateNovaArtifact(_adminUser, _project, artifactType);
            var novaAttachmentFile = FileStoreTestHelper.UploadNovaFileToFileStore(_adminUser, _fileName, _fileType, defaultExpireTime, Helper.FileStore);
            var addedArtifactAttachment = ArtifactStoreHelper.AddArtifactAttachmentAndSave(_adminUser, artifact, novaAttachmentFile, Helper.ArtifactStore, shouldLockArtifact: false);

            Assert.NotNull(addedArtifactAttachment?.AttachedFiles, "Failed to add attachment to the artifact!");
            Assert.AreEqual(1, addedArtifactAttachment.AttachedFiles.Count, "The artifact should have 1 attachment!");
            Assert.AreEqual(novaAttachmentFile.FileName, addedArtifactAttachment.AttachedFiles[0].FileName, "The FileName of the attached file doesn't match!");

            var attachmentFile2 = FileStoreTestHelper.UploadNovaFileToFileStore(_adminUser, _fileName, _fileType, defaultExpireTime, Helper.FileStore);
            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(_adminUser, artifact.Id);
            var subArtifact = Helper.ArtifactStore.GetSubartifact(_adminUser, artifact.Id, subArtifacts[0].Id);

            ArtifactStoreHelper.AddSubArtifactAttachmentAndSave(_adminUser, artifact, subArtifact.Id.Value, attachmentFile2, Helper.ArtifactStore, shouldLockArtifact: false);
            artifact.Publish(_adminUser);

            var viewerUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);

            Attachments attachment = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                attachment = Helper.ArtifactStore.GetAttachments(viewerUser, artifact.Id, subArtifactId: subArtifacts[0].Id);
            }, "'GET {0}?subArtifactId={1}' shouldn't return any error.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT, subArtifacts[0].Id);

            // Verify:
            Assert.AreEqual(1, attachment.AttachedFiles.Count, "List of attached files must have 1 item.");
            Assert.AreEqual(attachmentFile2.FileName, attachment.AttachedFiles[0].FileName, "FileName should have expected value.");

            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_adminUser, artifact.Id);
            Assert.AreEqual(attachment.AttachedFiles[0].UploadedDate, artifactDetails.LastEditedOn,
                "UploadedDate for published artifact's attachment should be equal to LastEditedOn date of artifact");

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _adminUser, artifact.Id, ItemIndicatorFlags.HasAttachmentsOrDocumentRefs);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _adminUser, artifact.Id, ItemIndicatorFlags.HasAttachmentsOrDocumentRefs, (int)subArtifact.Id);
        }

        [TestCase]
        [TestRail(146335)]
        [Description("Create & publish a Process artifact, add attachment to User task & publish, delete attachment, get attachments for User task, check expectations.")]
        public void GetAttachmentWithSubArtifactId_SubArtifactWithDeletedAttachment_NoAttachmentsReturned()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.Process);
            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(_adminUser, artifact.Id);
            Assert.AreEqual(5, subArtifacts.Count, "Process should have 5 subartifacts.");

            // User Task is subArtifacts[2]
            var subArtifact = Helper.ArtifactStore.GetSubartifact(_adminUser, artifact.Id, subArtifacts[2].Id);
            var novaAttachmentFile = FileStoreTestHelper.UploadNovaFileToFileStore(_adminUser, _fileName, _fileType, defaultExpireTime, Helper.FileStore);
            var attachment = ArtifactStoreHelper.AddSubArtifactAttachmentAndSave(_adminUser, artifact, subArtifact.Id.Value, novaAttachmentFile, Helper.ArtifactStore);
            artifact.Publish(_adminUser);

            Assert.AreEqual(1, attachment.AttachedFiles.Count, "SubArtifact should have 1 file attached.");
            ArtifactStoreHelper.DeleteSubArtifactAttachmentAndSave(_adminUser, artifact, subArtifact, attachment.AttachedFiles[0].AttachmentId, Helper.ArtifactStore);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                attachment = Helper.ArtifactStore.GetAttachments(_adminUser, artifact.Id, subArtifactId: subArtifact.Id);
            }, "'GET {0}?subArtifactId={1}' shouldn't return any error.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT, subArtifact.Id);

            // Verify:
            Assert.AreEqual(0, attachment.AttachedFiles.Count, "List of attached files must be empty.");
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _adminUser, artifact.Id, expectedIndicatorFlags: null, subArtifactId: (int)subArtifact.Id);
        }

        [TestCase(null)]
        [TestCase(true)]
        [TestRail(154591)]
        [Description("Create & save an artifact (don't publish), add attachment, get attachments (with or without addDrafts=true). Verify attachment is returned.")]
        public void GetAttachment_UnpublishedArtifactWithAttachment_AttachmentIsReturned(bool? addDrafts)
        {
            // Setup:
            var artifact = Helper.CreateNovaArtifact(_adminUser, _project, ItemTypePredefined.Actor);
            var openApiAttachment = Helper.OpenApi.AddArtifactAttachment(_adminUser, _project.Id, artifact.Id, _attachmentFile);

            Attachments attachment = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                attachment = Helper.ArtifactStore.GetAttachments(_adminUser, artifact.Id, addDrafts);
            }, "'GET {0}{1}' shouldn't return any error.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT,
                addDrafts == null ? string.Empty : "?addDrafts=" + addDrafts);

            // Verify:
            Assert.AreEqual(1, attachment.AttachedFiles.Count, "List of attached files must have 1 item.");
            OpenApiAttachment.AssertAreEqual(openApiAttachment, attachment.AttachedFiles[0]);
            Assert.IsNull(attachment.AttachedFiles[0].UploadedDate, "UploadedDate for draft artifact's attachment should be null.");

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _adminUser, artifact.Id, ItemIndicatorFlags.HasAttachmentsOrDocumentRefs);
        }

        [TestCase]
        [TestRail(154646)]
        [Description("Create & publish an artifact, add attachment, get attachments (with addDrafts=false).  Verify no attachments are returned.")]
        public void GetAttachmentWithAddDraftsFalse_PublishedArtifactWithAttachmentInDraft_NoAttachmentsReturned()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.Actor);
            var novaAttachmentFile = FileStoreTestHelper.UploadNovaFileToFileStore(_adminUser, _fileName, _fileType, defaultExpireTime, Helper.FileStore);
            ArtifactStoreHelper.AddArtifactAttachmentAndSave(_adminUser, artifact, novaAttachmentFile, Helper.ArtifactStore, shouldReturnAttachments: false);

            Attachments attachment = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                attachment = Helper.ArtifactStore.GetAttachments(_adminUser, artifact.Id, addDrafts: false);
            }, "'GET {0}?addDrafts=false' shouldn't return any error.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT);

            // Verify:
            Assert.AreEqual(0, attachment.AttachedFiles.Count, "List of attached files must be empty.");
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _adminUser, artifact.Id, ItemIndicatorFlags.HasAttachmentsOrDocumentRefs);
        }
        
        [TestCase(2)]
        [TestCase(20)]
        [TestRail(154593)]
        [Description("Create & save an artifact, add multiple attachments, publish artifact, get attachments.  Verify all attachments that were added are returned.")]
        public void GetAttachment_PublishedArtifactWithMultipleAttachments_AllAttachmentsAreReturned(int numberOfAttachments)
        {
            // Setup:
            var artifact = Helper.CreateNovaArtifact(_adminUser, _project, ItemTypePredefined.Actor);
            var openApiAttachments = new List<OpenApiAttachment>();

            for (int i = 0; i < numberOfAttachments; ++i)
            {
                var file = FileStoreTestHelper.CreateFileWithRandomByteArray(_fileSize, _fileName, "text/plain");
                var openApiAttachment = Helper.OpenApi.AddArtifactAttachment(_adminUser, _project.Id, artifact.Id, file);
                openApiAttachments.Add(openApiAttachment);
            }

            artifact.Publish(_adminUser);

            Attachments attachment = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                attachment = Helper.ArtifactStore.GetAttachments(_adminUser, artifact.Id);
            }, "'GET {0}' shouldn't return any error when passed a published artifact ID.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT);

            // Verify:
            Assert.AreEqual(openApiAttachments.Count, attachment.AttachedFiles.Count,
                "List of attached files should have {0} files.", openApiAttachments.Count);

            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_adminUser, artifact.Id);

            foreach (var openApiAttachment in openApiAttachments)
            {
                var attachedFile = attachment.AttachedFiles.Find(f => f.AttachmentId == openApiAttachment.Id);
                Assert.NotNull(attachedFile, "Couldn't find file with ID '{0}' in the list of attached files!", openApiAttachment.Id);
                OpenApiAttachment.AssertAreEqual(openApiAttachment, attachedFile);
                Assert.AreEqual(artifactDetails.LastEditedOn, attachedFile.UploadedDate,
                    "UploadedDate for published artifact's attachment should be equal to LastEditedOn date of artifact");
            }

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _adminUser, artifact.Id, ItemIndicatorFlags.HasAttachmentsOrDocumentRefs);
        }

        [TestCase]
        [TestRail(154600)]
        [Description("Create & publish an artifact, get attachments.  Verify no attachments are returned.")]
        public void GetAttachment_PublishedArtifactWithNoAttachments_NoAttachmentsReturned()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.Actor);
            Attachments attachment = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                attachment = Helper.ArtifactStore.GetAttachments(_adminUser, artifact.Id);
            }, "'GET {0}' shouldn't return any error.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT);

            // Verify:
            Assert.AreEqual(0, attachment.AttachedFiles.Count, "List of attached files must be empty.");
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _adminUser, artifact.Id, expectedIndicatorFlags: null);
        }

        [TestCase]
        [TestRail(182497)]
        [Description("Create and publish artifact, add attachment and publish, get attachments for version 1 and 2, attachments should have expected values.")]
        public void GetAttachmentSpecifyVersion_Version1NoAttachmentVersion2Attachment_CorrectAttachmentIsReturned()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.Actor);
            //versionId = 1 - no attachments

            var novaAttachmentFile = FileStoreTestHelper.UploadNovaFileToFileStore(_adminUser, _fileName, _fileType, defaultExpireTime, Helper.FileStore);
            ArtifactStoreHelper.AddArtifactAttachmentAndSave(_adminUser, artifact, novaAttachmentFile, Helper.ArtifactStore, shouldReturnAttachments: false);
            artifact.Publish(_adminUser);
            //versionId = 2 - 1 attachment - _attachmentFile

            Attachments version1attachment = null;
            Attachments version2attachment = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                version1attachment = Helper.ArtifactStore.GetAttachments(_authorUser, artifact.Id, versionId: 1);
                version2attachment = Helper.ArtifactStore.GetAttachments(_authorUser, artifact.Id, versionId: 2);
            }, "'GET {0}?versionId=x' shouldn't return any error when passed a valid versionId.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT);

            // Verify:
            Assert.AreEqual(0, version1attachment.AttachedFiles.Count, "List of attached files must be empty.");
            Assert.AreEqual(1, version2attachment.AttachedFiles.Count, "List of attached files must have 1 item.");

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _adminUser, artifact.Id, ItemIndicatorFlags.HasAttachmentsOrDocumentRefs);
        }

        [TestCase]
        [TestRail(182498)]
        [Description("Create and publish artifact with 2 attachments, delete attachment and publish, get attachments for version 1 and 2, attachments should have expected values.")]
        public void GetAttachmentSpecifyVersion_ArtifactWithDeletedAttachment_NoAttachmentForLastVersion()
        {
            // Setup:
            var artifact = Helper.CreateNovaArtifact(_adminUser, _project, ItemTypePredefined.Glossary);

            var novaAttachmentFile1 = FileStoreTestHelper.UploadNovaFileToFileStore(_adminUser, _fileName, _fileType, defaultExpireTime, Helper.FileStore);
            var novaAttachmentFile2 = FileStoreTestHelper.UploadNovaFileToFileStore(_adminUser, _fileName, _fileType, defaultExpireTime, Helper.FileStore);

            var attachment = ArtifactStoreHelper.AddArtifactAttachmentsAndSave(_adminUser, artifact,
                new List<INovaFile> { novaAttachmentFile1, novaAttachmentFile2 }, Helper.ArtifactStore);

            artifact.Publish(_adminUser);
            //versionId = 1 - 2 attachments - _attachmentFile

            Assert.AreEqual(2, attachment.AttachedFiles.Count, "Artifact should have 2 attached files at this stage.");

            ArtifactStoreHelper.DeleteArtifactAttachmentAndSave(_adminUser, artifact, attachment.AttachedFiles[0].AttachmentId,
                Helper.ArtifactStore);

            artifact.Publish(_adminUser);
            //versionId = 1 - 1 attachment

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                attachment = Helper.ArtifactStore.GetAttachments(_authorUser, artifact.Id, versionId: 2);
            }, "'GET {0}?versionId=2' shouldn't return any error when passed a valid versionId.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT);

            // Verify:
            Assert.AreEqual(1, attachment.AttachedFiles.Count, "Artifact should have 1 attached file at this stage.");

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _adminUser, artifact.Id, ItemIndicatorFlags.HasAttachmentsOrDocumentRefs);
        }

        [TestCase]
        [TestRail(182501)]
        [Description("Create and publish artifact, get attachments for version 1, attachments should be empty.")]
        public void GetAttachmentSpecifyVersion_ArtifactNoAttachment_NoAttachmentForLastVersion()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.BusinessProcess);
            //versionId = 1 - no attachment

            Attachments attachment = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                attachment = Helper.ArtifactStore.GetAttachments(_authorUser, artifact.Id, versionId: 1);
            }, "'GET {0}?versionId=1' shouldn't return any error when passed a valid versionId.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT);

            // Verify:
            Assert.AreEqual(0, attachment.AttachedFiles.Count, "List of attached files must be empty.");
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _adminUser, artifact.Id, expectedIndicatorFlags: null);
        }

        [TestCase]
        [TestRail(182502)]
        [Description("Create and publish artifact with attachment, add attachment and publish, get attachments for version 1 and 2, attachments should have expected values.")]
        public void GetAttachmentSpecifyVersion_Version1AttachmentVersion2TwoAttachment_CorrectAttachmentIsReturned()
        {
            // Setup:
            var artifact = Helper.CreateNovaArtifact(_adminUser, _project, ItemTypePredefined.Actor);
            var novaAttachmentFile = FileStoreTestHelper.UploadNovaFileToFileStore(_adminUser, _fileName, _fileType, defaultExpireTime, Helper.FileStore);
            ArtifactStoreHelper.AddArtifactAttachmentAndSave(_adminUser, artifact, novaAttachmentFile, Helper.ArtifactStore, shouldReturnAttachments: false);
            artifact.Publish(_adminUser);
            //versionId = 1 - 1 attachment

            ArtifactStoreHelper.AddArtifactAttachmentAndSave(_adminUser, artifact, novaAttachmentFile, Helper.ArtifactStore, expectedAttachedFilesCount: 2);
            artifact.Publish(_adminUser);
            //versionId = 2 - 2 attachments

            Attachments version1attachment = null;
            Attachments version2attachment = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                version1attachment = Helper.ArtifactStore.GetAttachments(_authorUser, artifact.Id, versionId: 1);
                version2attachment = Helper.ArtifactStore.GetAttachments(_authorUser, artifact.Id, versionId: 2);
            }, "'GET {0}?versionId=x' shouldn't return any error when passed a valid versionId.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT);

            // Verify:
            Assert.AreEqual(1, version1attachment.AttachedFiles.Count, "List of attached files must have 1 item.");
            Assert.AreEqual(2, version2attachment.AttachedFiles.Count, "List of attached files must have 2 items.");
            Assert.IsFalse(version2attachment.AttachedFiles[0].AttachmentId == version2attachment.AttachedFiles[1].AttachmentId,
                "AttachmentId should be different for different attachments.");
            Assert.IsTrue(version2attachment.AttachedFiles[0].AttachmentId == version1attachment.AttachedFiles[0].AttachmentId,
                "AttachmentId for the file must be the same across all versions.");

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _adminUser, artifact.Id, ItemIndicatorFlags.HasAttachmentsOrDocumentRefs);
        }

        #endregion Positive tests

        #region 400 Bad Request

        [TestCase]
        [TestRail(155622)]
        [Description("Create & publish a Process artifact and an Actor artifact.  Try to get Attachments for the Process User Task but pass the Artifact ID " +
                     "of the Actor instead of the Process.  Verify 400 Bad Request is returned.")]
        public void GetAttachmentWithSubArtifactId_SubArtifactIdFromDifferentArtifact_400BadRequest()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.Process);
            var process = Helper.Storyteller.GetProcess(_adminUser, artifact.Id);
            var userTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            var artifact2 = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.Actor);

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.ArtifactStore.GetAttachments(_adminUser, artifact2.Id, subArtifactId: userTask.Id);
            }, "'GET {0}' should return 400 Bad Request if passed a sub-artifact ID that doesn't belong to the specified artifact ID.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, 400, string.Empty);    // TFS Bug 6552: It should return a descriptive error code & message.
        }

        #endregion 400 Bad request

        #region 403 Forbidden

        [TestCase]
        [TestRail(154597)]
        [Description("Create & publish an artifact.  Try to get attachments with a user that doesn't have permission to access the artifact.  Verify 403 Forbidden is returned.")]
        public void GetAttachment_PublishedArtifactUserHasNoPermissionToArtifact_403Forbidden()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.Actor);

            var userWithoutPermission = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken,
                InstanceAdminRole.BlueprintAnalytics);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                Helper.ArtifactStore.GetAttachments(userWithoutPermission, artifact.Id);
            }, "'GET {0}' should return 403 Forbidden for a user without permission to the artifact.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT);

            // Verify:
            TestHelper.AssertResponseBodyIsEmpty(ex.RestResponse);
        }

        [TestCase]
        [TestRail(182503)]
        [Description("Create and publish artifact (admin), add attachment and publish (author), set artifact's permission to none for author, " +
                     "get attachments for version 1 should return 403 for author.")]
        public void GetAttachmentSpecifyVersion_UserHaveNoPermissionFromVersion2_Returns403()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.Actor);
            //versionId = 1 - no attachments

            try
            {
                var novaAttachmentFile = FileStoreTestHelper.UploadNovaFileToFileStore(_adminUser, _fileName, _fileType, defaultExpireTime, Helper.FileStore);
                ArtifactStoreHelper.AddArtifactAttachmentAndSave(_authorUser, artifact, novaAttachmentFile, Helper.ArtifactStore, shouldReturnAttachments: false);
                artifact.Publish(_authorUser);
                //versionId = 2 - 1 attachment - _novaAttachmentFile

                Helper.AssignProjectRolePermissionsToUser(_authorUser, TestHelper.ProjectRole.None, _project, artifact);
                //now _userAuthorLicense has no access to artifact

                // Execute:
                var ex = Assert.Throws<Http403ForbiddenException>(() =>
                {
                    Helper.ArtifactStore.GetAttachments(_authorUser, artifact.Id, versionId: 1);
                }, "'GET {0}?versionId=1' should throw 403 exception for user with no access.",
                    RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT);

                // Verify:
                TestHelper.AssertResponseBodyIsEmpty(ex.RestResponse);
            }
            finally
            {
                // Clean up.
                artifact.Delete(_adminUser);
                artifact.Publish(_adminUser);
            }
        }

        [TestCase]
        [TestRail(209267)]
        [Description("Create & publish a Use Case artifact.  Add an attachment to the Precondition subArtifact & publish.  Give a user no access to the artifact " +
                     "and get attachments for Precondition with that user.  Verify 403 Forbidden is returned.")]
        public void GetAttachmentWithSubArtifactId_PublishedArtifactUserHasNoPermissionToArtifact_403Forbidden()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.UseCase);

            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(_adminUser, artifact.Id);
            var subArtifact = Helper.ArtifactStore.GetSubartifact(_adminUser, artifact.Id, subArtifacts[0].Id);

            // Add attachment to a sub-artifact.
            var novaAttachmentFile = FileStoreTestHelper.UploadNovaFileToFileStore(_adminUser, _fileName, _fileType, defaultExpireTime, Helper.FileStore);
            ArtifactStoreHelper.AddSubArtifactAttachmentAndSave(_adminUser, artifact, subArtifact.Id.Value, novaAttachmentFile, Helper.ArtifactStore);
            artifact.Publish(_adminUser);

            // Give author user no access to the artifact.
            Helper.AssignProjectRolePermissionsToUser(_authorUser, TestHelper.ProjectRole.None, _project, artifact);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                Helper.ArtifactStore.GetAttachments(_authorUser, artifact.Id, subArtifactId: subArtifacts[0].Id);
            }, "'GET {0}?subArtifactId={1}' should return 403 Forbidden if the user has no access to the artifact.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT, subArtifacts[0].Id);

            // Verify:
            TestHelper.AssertResponseBodyIsEmpty(ex.RestResponse);
        }

        #endregion 403 Forbidden

        #region 404 Not Found

        [TestCase]
        [TestRail(146333)]
        [Description("Create & save an artifact, add attachment, publish artifact, delete artifact, publish artifact, get attachments.  Verify 404 Not Found is returned.")]
        public void GetAttachment_DeletedArtifactWithAttachment_NotFound()
        {
            // Setup:
            var artifact = Helper.CreateNovaArtifact(_adminUser, _project, ItemTypePredefined.Actor);
            var novaAttachmentFile = FileStoreTestHelper.UploadNovaFileToFileStore(_adminUser, _fileName, _fileType, defaultExpireTime, Helper.FileStore);
            ArtifactStoreHelper.AddArtifactAttachmentAndSave(_adminUser, artifact, novaAttachmentFile, Helper.ArtifactStore, shouldLockArtifact: false, shouldReturnAttachments: false);
            artifact.Publish(_adminUser);
            artifact.Delete(_adminUser);
            artifact.Publish(_adminUser);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetAttachments(_adminUser, artifact.Id);
            }, "'GET {0}' should return 404 Not Found when passed a deleted artifact ID.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.ArtifactNotFound, ITEM_DOES_NOT_EXIST_OR_NO_PERMISSION);
        }

        [TestCase]
        [Explicit(IgnoreReasons.ProductBug)]    // BUG #1712  ArtifactStore service: svc/artifactstore/artifacts/{id}/relationships works for subartifactid
        [TestRail(154604)]
        [Description("Create a Process artifact, publish it, add attachment to User task & publish, get attachments but pass the User Task sub-artifact ID instead of the artifact ID.  " +
                     "Verify 404 Not Found is returned.")]
        public void GetAttachment_SubArtifactIdPassedAsArtifactId_404NotFound()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.Process);
            var process = Helper.Storyteller.GetProcess(_adminUser, artifact.Id);
            var userTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            var novaAttachmentFile = FileStoreTestHelper.UploadNovaFileToFileStore(_adminUser, _fileName, _fileType, defaultExpireTime, Helper.FileStore);
            ArtifactStoreHelper.AddSubArtifactAttachmentAndSave(_adminUser, artifact, userTask.Id, novaAttachmentFile, Helper.ArtifactStore);
            artifact.Publish(_adminUser);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetAttachments(_adminUser, userTask.Id);
            }, "'GET {0}' should return 404 Not Found if passed a sub-artifact ID instead of an artifact ID.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT);

            // Verify:
            TestHelper.AssertResponseBodyIsEmpty(ex.RestResponse);
        }

        [TestCase]
        [TestRail(154592)]
        [Description("Create & save an artifact (don't publish), add attachment, get attachments (with addDrafts=false).  Verify 404 Not Found is returned.")]
        public void GetAttachmentWithAddDraftsFalse_UnpublishedArtifactWithAttachment_404NotFound()
        {
            // Setup:
            var artifact = Helper.CreateNovaArtifact(_adminUser, _project, ItemTypePredefined.Actor);
            var novaAttachmentFile = FileStoreTestHelper.UploadNovaFileToFileStore(_adminUser, _fileName, _fileType, defaultExpireTime, Helper.FileStore);
            ArtifactStoreHelper.AddArtifactAttachmentAndSave(_adminUser, artifact, novaAttachmentFile, Helper.ArtifactStore, shouldLockArtifact: false, shouldReturnAttachments: false);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetAttachments(_adminUser, artifact.Id, addDrafts: false);
            }, "'GET {0}?addDrafts=false' should return 404 Not Found.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.ArtifactNotFound, ITEM_DOES_NOT_EXIST_OR_NO_PERMISSION);

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _adminUser, artifact.Id, ItemIndicatorFlags.HasAttachmentsOrDocumentRefs);
        }

        [TestCase(0)]
        [TestCase(int.MaxValue)]
        [TestRail(154594)]
        [Description("Try to get attachments for a non-existent artifact ID.  Verify 404 Not Found is returned.")]
        public void GetAttachment_NonExistentArtifactId_404NotFound(int artifactId)
        {
            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetAttachments(_adminUser, artifactId);
            }, "'GET {0}' should return 404 Not Found.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT);

            // Verify:
            if (artifactId > 0) // NOTE: Returns generic IIS 404 page if artifactId <= 0.
            {
                TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.ArtifactNotFound, ITEM_DOES_NOT_EXIST_OR_NO_PERMISSION);
            }
        }

        [TestCase(0, Explicit = true, Reason = IgnoreReasons.ProductBug)]   // BUG #1722: Returns 400 instead of 404.
        [TestCase(int.MaxValue)]
        [TestRail(154595)]
        [Description("Create & save a Process artifact.  Try to get attachments for a non-existent sub-artifact ID.  Verify 404 Not Found is returned.")]
        public void GetAttachmentWithSubArtifactId_NonExistentSubArtifactId_404NotFound(int subArtifactId)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.Process);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetAttachments(_adminUser, artifact.Id, subArtifactId: subArtifactId);
            }, "'GET {0}?subArtifactId={1}' should return 404 Not Found.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT, subArtifactId);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.SubartifactNotFound, ITEM_DOES_NOT_EXIST_OR_NO_PERMISSION);
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(2)]
        [TestRail(182553)]
        [Description("Create and publish artifact with attachment, get attachments non-existing version, 404 should be returned.")]
        public void GetAttachmentSpecifyVersion_VersionNotExist_Returned404(int versionId)
        {
            // Setup:
            var artifact = Helper.CreateNovaArtifact(_adminUser, _project, ItemTypePredefined.Actor);

            var novaAttachmentFile = FileStoreTestHelper.UploadNovaFileToFileStore(_adminUser, _fileName, _fileType, defaultExpireTime, Helper.FileStore);
            ArtifactStoreHelper.AddArtifactAttachmentAndSave(_adminUser, artifact, novaAttachmentFile, Helper.ArtifactStore, shouldReturnAttachments: false);
            artifact.Publish(_adminUser);
            //versionId = 1 - 1 attachment - _novaAttachmentFile

            // Execute & Verify:
            var ex = Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetAttachments(_authorUser, artifact.Id, versionId: versionId);
            }, "'GET {0}?versionId={1}' should return 404 error when passed a non-existing valid versionId.",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT, versionId);

            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.ResourceNotFound, "Version Index or Baseline Timestamp is not found.");

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _adminUser, artifact.Id, ItemIndicatorFlags.HasAttachmentsOrDocumentRefs);
        }

        #endregion 404 Not Found

        // TODO: Implement GetAttachment_PublishedArtifactWithDocReferenceUserHasNoPermissionToDocReference_403Forbidden  TestRail ID: 154596
    }
}
