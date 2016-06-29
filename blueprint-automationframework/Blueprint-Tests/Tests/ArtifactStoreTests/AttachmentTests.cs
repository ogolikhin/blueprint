using System;
using Helper;
using Model;
using NUnit.Framework;
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
        [Description("...")]
        public void BasicAttachment()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Actor);
            artifact.Save(_user);
            artifact.Publish(_user);
            IFile attachmentFile = CreateRandomFile(2048, "attachment_test.txt");
            OpenApiArtifact.AddAttachment(Helper.BlueprintServer.Address, _project.Id,
                artifact.Id, attachmentFile, _user);

            Attachment attachment = null;
            Assert.DoesNotThrow(() =>
            {
                attachment = Helper.ArtifactStore.GetItemsAttachment(artifact.Id, _user);
            });
            Assert.AreEqual(1, attachment.AttachedFiles.Count);
        }
    }
}
