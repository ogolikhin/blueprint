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
        [Description("Create and publish document, upload file, update artifact with uploaded file info, check that artifact details has expected values.")]
        public void AddFile_PublishedDocument_ArtifactHasExpectedDetails()
        {
            // Setup:
            INovaFile file = FileStoreTestHelper.CreateNovaFileWithRandomByteArray((uint)2048,
                "2KB_File.txt", "text/plain");

            var uploadedFile = Helper.FileStore.AddFile(file, _user, useMultiPartMime: true);
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Document);
            artifact.Lock();

            // Execute & Verify:
            UpdateDocumentFile_CanGetAttachment(_user, artifact, uploadedFile);
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
            NovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);
            DocumentFileValue testFileValue = new DocumentFileValue();
            testFileValue.FileExtension = "xls";//TODO - replace with param
            testFileValue.FileGuid = file.Guid;
            testFileValue.FileName = file.FileName;
            artifactDetails.DocumentFile = testFileValue;

            NovaArtifactDetails updateResult = null;

            // Execute:
            Assert.DoesNotThrow(() => updateResult = Artifact.UpdateArtifact(artifact, _user, artifactDetails, Helper.BlueprintServer.Address),
                "Exception caught while trying to update an artifact!");
            var updatedArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(user, artifact.Id);

            // Verify:
            DocumentHasExpectedAttachment(updatedArtifactDetails, testFileValue);
        }

        /// <summary>
        /// Returns true if artifactDetails has info about expectedDocumentFile and false in the opposite case.
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