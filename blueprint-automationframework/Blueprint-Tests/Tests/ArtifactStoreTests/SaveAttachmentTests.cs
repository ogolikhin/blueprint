using System.Collections.Generic;
using System.Linq;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.NovaModel;
using Model.Factories;
using NUnit.Framework;
using TestCommon;
using Common;
using Utilities.Factories;
using Utilities;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class SaveAttachmentTests : TestBase
    {
        private IUser _user = null;
        private IProject _project = null;
        private List<IProject> _allProjects = null;
        private string _fileName = null;
        private const string _fileType = "text/plain";
        private INovaFile _attachmentFile = null;
        private System.DateTime defaultExpireTime = System.DateTime.Now.AddDays(2);//Currently Nova set ExpireTime 2 days from today for newly uploaded file

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            _allProjects = ProjectFactory.GetAllProjects(_user);
            _project = _allProjects.First();
            _project.GetAllArtifactTypes(ProjectFactory.Address, _user);
            _fileName = I18NHelper.FormatInvariant("{0}.{1}", RandomGenerator.RandomAlphaNumeric(10), "docx");
            _attachmentFile = FileStoreTestHelper.UploadNovaFileToFileStore(_user, _fileName, _fileType, defaultExpireTime,
                Helper.FileStore);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        [TestCase]
        [TestRail(182360)]
        [Description("Add attachment to the published artifact, check that it throws no error, check that attachement has expected value.")]
        public void AddAttachment_PublishedArtifact_AttachmentHasExpectedValue()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.TextualRequirement);
            var attachmentBeforeTest = Helper.ArtifactStore.GetAttachments(artifact, _user);
            Assert.AreEqual(0, attachmentBeforeTest.AttachedFiles.Count,
                "Artifact shouldn't have attachments at this point.");

            // Execute:
            Assert.DoesNotThrow(() => ArtifactStoreHelper.AddArtifactAttachmentAndSave(_user, artifact,
                new List<INovaFile> { _attachmentFile }, Helper.ArtifactStore),
                "Exception caught while trying to update an artifact!");

            // Verify:
            var attachmentAfterTest = Helper.ArtifactStore.GetAttachments(artifact, _user);
            Assert.AreEqual(1, attachmentAfterTest.AttachedFiles.Count,
                "Artifact should have 1 attachments at this point.");
            Assert.AreEqual(_attachmentFile.FileName, attachmentAfterTest.AttachedFiles[0].FileName, "Filename must have expected value.");
            Assert.AreEqual(0, attachmentAfterTest.DocumentReferences.Count, "List of Document References must be empty.");
        }

        [TestCase]
        [TestRail(182379)]
        [Description("Delete attached file from artifact with attachement, check that it throws no error.")]
        public void DeleteAttachment_ArtifactWithAttachment_AttachmentIsEmpty()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.TextualRequirement);
            ArtifactStoreHelper.AddArtifactAttachmentAndSave(_user, artifact, new List<INovaFile> { _attachmentFile },
                Helper.ArtifactStore);
            artifact.Publish(_user);
            var attachment = Helper.ArtifactStore.GetAttachments(artifact, _user);

            // Execute:
            Assert.DoesNotThrow(() => ArtifactStoreHelper.DeleteArtifactAttachmentAndSave(_user, artifact,
                attachment.AttachedFiles[0].AttachmentId, Helper.ArtifactStore), "Exception caught while trying to update an artifact!");
            attachment = Helper.ArtifactStore.GetAttachments(artifact, _user);

            // Verify:
            Assert.AreEqual(0, attachment.AttachedFiles.Count, "Artifact shouldn't have attachments at this point.");
            Assert.AreEqual(0, attachment.DocumentReferences.Count, "List of Document References must be empty.");
        }

        [TestCase]
        [TestRail(182397)]
        [Description("Add 2 attachments to the published artifact, check that it throws no error.")]
        public void Add2Attachments_Artifact_AttachmentHasExpectedValue()
        {
            // Setup:
            var attachmentFile1 = FileStoreTestHelper.UploadNovaFileToFileStore(_user, _fileName, _fileType, defaultExpireTime,
                Helper.FileStore);
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.TextualRequirement);

            var attachmentBeforeTest = Helper.ArtifactStore.GetAttachments(artifact, _user);
            Assert.AreEqual(0, attachmentBeforeTest.AttachedFiles.Count,
                "Artifact shouldn't have attachments at this point.");

            // Execute:
            Assert.DoesNotThrow(() =>
            ArtifactStoreHelper.AddArtifactAttachmentAndSave(_user, artifact,
            new List<INovaFile> { _attachmentFile, attachmentFile1 }, Helper.ArtifactStore),
            "Exception caught while trying to update an artifact!");

            // Verify:
            var attachmentAfterTest = Helper.ArtifactStore.GetAttachments(artifact, _user);
            Assert.AreEqual(2, attachmentAfterTest.AttachedFiles.Count,
                "Artifact should have 2 attachments at this point.");
            Assert.AreEqual(0, attachmentAfterTest.DocumentReferences.Count, "List of Document References must be empty.");
        }

        [TestCase]
        [TestRail(182404)]
        [Description("Add attachment to the published Document, it should throw 409 exception, file shouldn't be added.")]
        public void AddAttachment_PublishedDocument_Throw409()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Document);
            var attachmentBeforeTest = Helper.ArtifactStore.GetAttachments(artifact, _user);
            Assert.AreEqual(0, attachmentBeforeTest.AttachedFiles.Count,
                "Artifact shouldn't have attachments at this point.");

            // Execute:
            Assert.Throws<Http409ConflictException>(() => ArtifactStoreHelper.AddArtifactAttachmentAndSave(_user, artifact,
                new List<INovaFile> { _attachmentFile }, Helper.ArtifactStore),
                "Unexpected Exception caught while trying to update an artifact!");
            
            // Verify:
            var attachmentAfterTest = Helper.ArtifactStore.GetAttachments(artifact, _user);
            Assert.AreEqual(0, attachmentAfterTest.AttachedFiles.Count,
                "Artifact shouldn't have at this point.");
            Assert.AreEqual(0, attachmentAfterTest.DocumentReferences.Count, "List of Document References must be empty.");
        }
    }
}
