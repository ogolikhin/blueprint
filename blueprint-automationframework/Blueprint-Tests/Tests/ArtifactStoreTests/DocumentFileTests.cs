using System.Collections.Generic;
using System.Linq;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.ArtifactModel.Impl.PredefinedProperties;
using Model.NovaModel;
using Model.Factories;
using Newtonsoft.Json;
using NUnit.Framework;
using TestCommon;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class DocumentFileTests : TestBase
    {
        private IUser _user = null;
        private IProject _project = null;
        private List<IProject> _allProjects = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            _allProjects = ProjectFactory.GetAllProjects(_user);
            _project = _allProjects.First();
            _project.GetAllArtifactTypes(ProjectFactory.Address, _user);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        [TestCase]
        [TestRail(166132)]
        [Description("Create and publish document, attach file, check that artifact details has expected values.")]
        public void AddAttachment_PublishedDocument_ArtifactHasExpectedDetails()
        {
            // Setup:
            INovaFile file = FileStoreTestHelper.CreateNovaFileWithRandomByteArray((uint)2048,
                "2KB_File.txt", "text/plain");
            //Currently Nova set ExpireTime 2 days from today for newly uploaded file
            System.DateTime expireTime = System.DateTime.Now.AddDays(2);

            var uploadedFile = Helper.FileStore.AddFile(file, _user, expireTime: expireTime, useMultiPartMime: true);
            Assert.IsNotNull(Helper.FileStore.GetSQLExpiredTime(uploadedFile.Guid), "Uploaded file shouldn't have null ExpiredTime");
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Document);
            artifact.Lock();

            // Execute & Verify:
            UpdateDocumentFile_CanGetAttachment(_user, artifact, uploadedFile);
        }

        [TestCase]
        [TestRail(166151)]
        [Description("Create and publish document, attach file, delete attachment, check that artifact has no attachments.")]
        public void DeleteAttachment_PublishedDocumentWithAttachment_ArtifactHasNoAttachment()
        {
            // Setup:
            INovaFile file = FileStoreTestHelper.CreateNovaFileWithRandomByteArray((uint)2048,
                "2KB_File.txt", "text/plain");
            //Currently Nova set ExpireTime 2 days from today for newly uploaded file
            System.DateTime expireTime = System.DateTime.Now.AddDays(2);

            var uploadedFile = Helper.FileStore.AddFile(file, _user, expireTime: expireTime, useMultiPartMime: true);
            Assert.IsNotNull(Helper.FileStore.GetSQLExpiredTime(uploadedFile.Guid), "Uploaded file shouldn't have null ExpiredTime");
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Document);
            artifact.Lock();
            UpdateDocumentFile_CanGetAttachment(_user, artifact, uploadedFile);
            //
            artifact.NovaPublish(_user);
            artifact.Lock(_user);
            //
            // Execute & Verify:
            DeleteDocumentFile_CheckAttachmentIsEmpty(_user, artifact);
        }

        [TestCase]
        [TestRail(166154)]
        [Description("Create and publish document, attach file, publish, replace attachment, check that artifact details has expected values.")]
        public void ReplaceAttachment_PublishedDocumentWithAttachment_ArtifactHasExpectedDetails()
        {
            // Setup:
            INovaFile file1 = FileStoreTestHelper.CreateNovaFileWithRandomByteArray((uint)2048,
                "2KB_File.txt", "text/plain");
            INovaFile file2 = FileStoreTestHelper.CreateNovaFileWithRandomByteArray((uint)4096,
                "4KB_File.txt", "text/plain");
            //Currently Nova set ExpireTime 2 days from today for newly uploaded file
            System.DateTime expireTime = System.DateTime.Now.AddDays(2);

            var uploadedFile1 = Helper.FileStore.AddFile(file1, _user, expireTime: expireTime, useMultiPartMime: true);
            Assert.IsNotNull(Helper.FileStore.GetSQLExpiredTime(uploadedFile1.Guid), "Uploaded file shouldn't have null ExpiredTime");
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Document);
            artifact.Lock(_user);
            UpdateDocumentFile_CanGetAttachment(_user, artifact, uploadedFile1);
            artifact.NovaPublish(_user);
            artifact.Lock(_user);
            var uploadedFile2 = Helper.FileStore.AddFile(file2, _user, expireTime: expireTime, useMultiPartMime: true);

            // Execute & Verify:
            UpdateDocumentFile_CanGetAttachment(_user, artifact, uploadedFile2);
        }


        /// <summary>
        /// Attaches specified file to the specified artifact and check that artifact has expected details.
        /// </summary>
        /// <param name="user">The user to perform an operation.</param>
        /// <param name="artifact">The artifact to update.</param>
        /// <param name="file">The file to attach to document.</param>
        private void UpdateDocumentFile_CanGetAttachment(IUser user, IArtifact artifact, INovaFile file)
        {
            // Setup:
            NovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetArtifactDetails(user, artifact.Id);
            DocumentFileValue testFileValue = new DocumentFileValue();

            testFileValue.FileExtension = "xls";//TODO - replace with param
            testFileValue.FileGuid = file.Guid;
            testFileValue.FileName = file.FileName;
            testFileValue.FilePath = file.UriToFile.OriginalString;
            artifactDetails.DocumentFile = testFileValue;

            NovaArtifactDetails updateResult = null;

            // Execute:
            Assert.DoesNotThrow(() => updateResult = Artifact.UpdateArtifact(artifact, _user, artifactDetails, Helper.BlueprintServer.Address),
                "Exception caught while trying to update an artifact!");
            var updatedArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(user, artifact.Id);

            // Verify:
            Assert.IsNull(Helper.FileStore.GetSQLExpiredTime(testFileValue.FileGuid), "After saving ExpiredTime for file should be Null.");
            DocumentHasExpectedAttachment(updatedArtifactDetails, testFileValue);
        }

        /// <summary>
        /// Deletes attached file from the specified Document artifact.
        /// </summary>
        /// <param name="user">The user to perform an operation.</param>
        /// <param name="artifact">The artifact to delete attachment.</param>
        private void DeleteDocumentFile_CheckAttachmentIsEmpty(IUser user, IArtifact artifact)
        {
            // Setup:
            NovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetArtifactDetails(user, artifact.Id);
            DocumentFileValue testFileValue = null;
            artifactDetails.DocumentFile = testFileValue;

            NovaArtifactDetails updateResult = null;

            // Execute:
            Assert.DoesNotThrow(() => updateResult = Artifact.UpdateArtifact(artifact, _user, artifactDetails, Helper.BlueprintServer.Address),
                "Exception caught while trying to update an artifact!");
            var updatedArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(user, artifact.Id);

            // Verify:
            Assert.IsNull(updatedArtifactDetails.SpecificPropertyValues[0].CustomPropertyValue);
        }

        /// <summary>
        /// Check that artifactDetails has info about expectedDocumentFile.
        /// </summary>
        /// <param name="artifactDetails">Artifact details to check.</param>
        /// <param name="expectedDocumentFile">Expected file info.</param>
        private static void DocumentHasExpectedAttachment(INovaArtifactDetails artifactDetails, DocumentFileValue expectedDocumentFile)
        {
            var documentFileProperty = artifactDetails.SpecificPropertyValues[0].CustomPropertyValue;
            var actualDocumentFile = JsonConvert.DeserializeObject<DocumentFileValue>(documentFileProperty.ToString());
            Assert.AreEqual(actualDocumentFile.FileExtension, expectedDocumentFile.FileExtension, "FileExtension should have expected value, but it doesn't.");
            Assert.AreEqual(actualDocumentFile.FileName, expectedDocumentFile.FileName, "FileName should have expected value, but it doesn't.");
        }
    }
}