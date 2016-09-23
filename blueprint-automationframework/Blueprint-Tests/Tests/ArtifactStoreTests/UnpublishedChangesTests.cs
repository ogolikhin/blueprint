using System.Linq;
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
            // Setup:
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _user, artifactType);
            INovaArtifactsAndProjectsResponse unpublishedChanges = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                unpublishedChanges = Helper.ArtifactStore.GetUnpublishedChanges(_user);
            }, "'GET {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.AssertExpectedProjectWasReturned(unpublishedChanges.Projects, _project);
            Assert.AreEqual(1, unpublishedChanges.Artifacts.Count, "There should be 1 artifact in the unpublished changes!");
            ArtifactStoreHelper.AssertNovaArtifactResponsePropertiesMatchWithArtifactSkipVersion(unpublishedChanges.Artifacts.First(), artifact);
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182260)]
        [Description("Create & publish an artifact, then change & save it.  GetUnpublishedChanges.  Verify the draft artifact is returned.")]
        public void GetArtifactDetails_PublishedArtifact_ReturnsArtifactDetails(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            artifact.Save();
            INovaArtifactsAndProjectsResponse unpublishedChanges = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                unpublishedChanges = Helper.ArtifactStore.GetUnpublishedChanges(_user);
            }, "'GET {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.AssertExpectedProjectWasReturned(unpublishedChanges.Projects, _project);
            Assert.AreEqual(1, unpublishedChanges.Artifacts.Count, "There should be 1 artifact in the unpublished changes!");
            ArtifactStoreHelper.AssertNovaArtifactResponsePropertiesMatchWithArtifact(unpublishedChanges.Artifacts.First(), artifact, expectedVersion: 1);
        }
    }
}
