using System;
using Helper;
using Model;
using NUnit.Framework;
using Utilities;
using CustomAttributes;
using System.Collections.Generic;
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

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_user);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        private static IFile CreateRandomFile(uint fileLength, string fileName, string fileType = "text/plain")
        {
            string randomChunk = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(fileLength);
            byte[] fileContents = Encoding.ASCII.GetBytes(randomChunk);
            IFile file = FileFactory.CreateFile(fileName, fileType, DateTime.Now, fileContents);
            return file;
        }

        [TestCase]
        [TestRail(01)]
        [Description("Create artifact, publish it, add attachment, get attachments, check expectations.")]
        public void GetAttachmentForPublishedArtifactWithAttachment_CheckExpectation()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Actor);
            artifact.Save(_user);
            artifact.Publish(_user);
            IFile attachmentFile = CreateRandomFile(2048, "attachment_test.txt");
            artifact.AddArtifactAttachment(attachmentFile, _user);

            Attachment attachment = null;
            Assert.DoesNotThrow(() =>
            {
                attachment = Helper.ArtifactStore.GetItemsAttachment(artifact.Id, _user);
            });
            Assert.AreEqual(1, attachment.AttachedFiles.Count);
        }

        [TestCase]
        [TestRail(02)]
        [Description("Create artifact, publish it, add attachment, delete artifact, publish artifact, get attachments, check 404.")]
        public void GetAttachmentForDeletedArtifact_NotFound()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Actor);
            artifact.Save(_user);
            artifact.Publish(_user);
            IFile attachmentFile = CreateRandomFile(2048, "attachment_test.txt");
            artifact.AddArtifactAttachment(attachmentFile, _user);
            artifact.Delete(_user);
            artifact.Publish(_user);

            Attachment attachment = null;
            Assert.Throws<Http404NotFoundException>(() =>
            {
                attachment = Helper.ArtifactStore.GetItemsAttachment(artifact.Id, _user);
            });
            Assert.AreEqual(0, attachment.AttachedFiles.Count);
        }

        [TestCase]
        [TestRail(03)]
        [Description("Create Process, publish it, add attachment to User task, get attachments for User task, check expectations.")]
        public void GetAttachmentForSubArtifact_CheckExpectations()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Process);
            artifact.Save(_user);
            artifact.Publish(_user);
            var process = Helper.Storyteller.GetProcess(_user, artifact.Id);
            var userTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            
            var attachmentFile = CreateRandomFile(3000, "attachment_test.txt");
            var result = artifact.AddSubArtifactAttachment(userTask.Id, attachmentFile, _user);
            Assert.IsNotNull(result);
            Attachment attachment = null;
            Assert.DoesNotThrow(() =>
            {
                attachment = Helper.ArtifactStore.GetItemsAttachment(userTask.Id, _user);
            });
            Assert.AreEqual(1, attachment.AttachedFiles.Count);
        }

        [TestCase]
        [TestRail(04)]
        [Description("Create Process, publish it, add attachment to User task, delete attachment, get attachments for User task, check expectations.")]
        public void GetAttachmentForSubArtifactWithDeletedAttachment_CheckExpectations()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Process);
            artifact.Save(_user);
            artifact.Publish(_user);
            var process = Helper.Storyteller.GetProcess(_user, artifact.Id);
            var userTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            var attachmentFile = CreateRandomFile(3000, "attachment_test.txt");
            var result = artifact.AddSubArtifactAttachment(userTask.Id, attachmentFile, _user);
            result.Delete(_user);
            Attachment attachment = null;
            Assert.DoesNotThrow(() =>
            {
                attachment = Helper.ArtifactStore.GetItemsAttachment(userTask.Id, _user);
            });
            Assert.AreEqual(0, attachment.AttachedFiles.Count);
        }
    }
}
