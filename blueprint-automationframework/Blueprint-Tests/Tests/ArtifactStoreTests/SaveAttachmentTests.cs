using System.Collections.Generic;
using System.Linq;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.NovaModel;
using Model.Factories;
using NUnit.Framework;
using TestCommon;
using Common;
using Utilities.Factories;

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
        [Description("Add attachment to the published artifact, check that it throws no error.")]
        public void AddAttachment_Artifact_DoesntThrowError()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.TextualRequirement);// it gives 500 error for BaseArtifactType.Document
            artifact.Lock();
            NovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);
            artifactDetails.AttachmentValues.Add(new AttachmentValue(_user, _attachmentFile));

            // Execute & Verify:
            Assert.DoesNotThrow(() => Artifact.UpdateArtifact(artifact, _user, artifactDetails, Helper.BlueprintServer.Address),
                "Exception caught while trying to update an artifact!");
        }
    }
}
