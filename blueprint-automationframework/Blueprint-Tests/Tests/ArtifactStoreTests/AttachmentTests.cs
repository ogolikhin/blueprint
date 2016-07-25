﻿using System;
using Common;
using Helper;
using Model;
using NUnit.Framework;
using Utilities;
using CustomAttributes;
using System.Text;
using TestCommon;
using Utilities.Factories;
using Model.ArtifactModel;
using Model.Factories;
using Model.ArtifactModel.Impl;
using Model.StorytellerModel.Impl;

namespace ArtifactStoreTests
{
    public class AttachmentTests : TestBase
    {
        private IUser _user = null;
        IProject _project = null;
        uint _fileSize = (uint)(RandomGenerator.RandomNumber(4096));
        string _fileName = I18NHelper.FormatInvariant("{0}.{1}", RandomGenerator.RandomAlphaNumeric(10), "txt");
        IFile _attachmentFile = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_user);
            _attachmentFile = CreateRandomFile(_fileSize, _fileName);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
            _attachmentFile = null;
        }

        private static IFile CreateRandomFile(uint fileLength, string fileName, string fileType = "text/plain")
        {
            string randomChunk = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(fileLength);
            byte[] fileContents = Encoding.ASCII.GetBytes(randomChunk);
            IFile file = FileFactory.CreateFile(fileName, fileType, DateTime.Now, fileContents);
            return file;
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

            Attachment attachment = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                attachment = Helper.ArtifactStore.GetItemsAttachment(artifact.Id, _user);
            }, "GetItemsAttachment shouldn't throw any error.");

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
                Helper.ArtifactStore.GetItemsAttachment(artifact.Id, _user);
            }, "GetItemsAttachment should throw 404 error, but it doesn't.");
        }

        [TestCase]
        [TestRail(146334)]
        [Description("Create a Process artifact, publish it, add attachment to User task & publish, get attachments for User task.  Verify attachment is returned.")]
        public void GetAttachment_SubArtifactWithAttachment_AttachmentIsReturned()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);
            var process = Helper.Storyteller.GetProcess(_user, artifact.Id);
            var userTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            
            var attachedFile = artifact.AddSubArtifactAttachment(userTask.Id, _attachmentFile, _user);
            artifact.Publish();

            Attachment attachment = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                attachment = Helper.ArtifactStore.GetItemsAttachment(userTask.Id, _user);
            }, "GetItemsAttachment shouldn't throw any error.");

            // Verify:
            Assert.AreEqual(1, attachment.AttachedFiles.Count, "List of attached files must have 1 item.");
            Assert.IsTrue(attachedFile.Equals(attachment.AttachedFiles[0]), "File from attachment should have expected values, but it doesn't.");
        }

        [TestCase]
        [TestRail(146335)]
        [Description("Create & publish a Process artifact, add attachment to User task & publish, delete attachment, get attachments for User task, check expectations.")]
        public void GetAttachment_SubArtifactWithDeletedAttachment_NoAttachmentsReturned()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);
            var process = Helper.Storyteller.GetProcess(_user, artifact.Id);
            var userTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            var result = artifact.AddSubArtifactAttachment(userTask.Id, _attachmentFile, _user);
            artifact.Publish();
            result.Delete(_user);

            Attachment attachment = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                attachment = Helper.ArtifactStore.GetItemsAttachment(userTask.Id, _user);
            }, "GetItemsAttachment shouldn't throw an error.");

            // Verify:
            Assert.AreEqual(0, attachment.AttachedFiles.Count, "List of attached files must be empty.");
        }
    }
}
