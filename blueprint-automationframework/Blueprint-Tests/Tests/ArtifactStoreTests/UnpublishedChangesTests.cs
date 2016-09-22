using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.Factories;
using NUnit.Framework;
using TestCommon;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class UnpublishedChangesTests : TestBase
    {
        private const string SVC_PATH = RestPaths.Svc.ArtifactStore.Artifacts.UNPUBLISHED;

        private IUser _user = null;
        private IProject _project = null;

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

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182259)]
        [Description("Create & save an artifact.  GetUnpublishedChanges.  Verify the saved artifact is returned.")]
        public void GetArtifactDetails_SavedArtifact_ReturnsArtifactDetails(BaseArtifactType artifactType)
        {
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _user, artifactType);
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);

            INovaPublishResponse unpublishedChanges = null;

            Assert.DoesNotThrow(() =>
            {
                unpublishedChanges = Helper.ArtifactStore.GetUnpublishedChanges(_user);
            }, "'GET {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            foreach (var change in unpublishedChanges.Artifacts)
            {
                artifactDetails.AssertEquals(change, skipDatesAndDescription: true);
            }
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182260)]
        [Description("Create & publish an artifact, then change & save it.  GetUnpublishedChanges.  Verify the draft artifact is returned.")]
        public void GetArtifactDetails_PublishedArtifact_ReturnsArtifactDetails(BaseArtifactType artifactType)
        {
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            artifact.Save();

            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);

            INovaPublishResponse unpublishedChanges = null;

            Assert.DoesNotThrow(() =>
            {
                unpublishedChanges = Helper.ArtifactStore.GetUnpublishedChanges(_user);
            }, "'GET {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            foreach (var change in unpublishedChanges.Artifacts)
            {
                artifactDetails.AssertEquals(change, skipDatesAndDescription: true);
            }
        }
    }
}
