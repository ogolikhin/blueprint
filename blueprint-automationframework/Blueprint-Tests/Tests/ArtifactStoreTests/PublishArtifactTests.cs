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
    public class PublishArtifactTests : TestBase
    {
        const string PUBLISH_PATH = RestPaths.Svc.ArtifactStore.Artifacts.PUBLISH;

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

        #region 200 OK Tests

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(165856)]
        [Description("Create & save a single artifact.  Publish the artifact.  Verify publish is successful and that artifact version is now 1.")]
        public void PublishArtifact_SingleSavedArtifact_ArtifactHasVersion1(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _user, artifactType);
            var artifactHistoryBefore = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user);
            Assert.AreEqual(int.MaxValue, artifactHistoryBefore[0].VersionId, "Version ID before publish should be {0}!", int.MaxValue);

            // Execute:
            INovaPublishResponse publishResponse = null;

            Assert.DoesNotThrow(() => publishResponse = Helper.ArtifactStore.PublishArtifact(artifact, _user),
                "'POST {0}' should return 200 OK if a valid artifact ID is sent!", PUBLISH_PATH);

            // Verify:
            Assert.AreEqual(1, publishResponse.Projects.Count, "There should only be 1 project returned for the published artifact!");
            Assert.AreEqual(1, publishResponse.Artifacts.Count, "There should only be 1 published artifact returned!");
            Assert.AreEqual(artifact.Id, publishResponse.Artifacts[0].Id, "The artifact ID doesn't match the one that we published!");

            var artifactHistoryAfter = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user);
            Assert.AreEqual(1, artifactHistoryAfter[0].VersionId, "Version ID after publish should be 1!");
        }

        #endregion 200 OK Tests

        #region 400 Bad Request tests
        //public void PublishArtifact_xxxx_400BadRequest()
        #endregion 400 Bad Request tests

        #region 401 Unauthorized tests
        //public void PublishArtifact_xxxx_401Unauthorized()
        #endregion 401 Unauthorized tests

        #region 403 Forbidden tests
        //public void PublishArtifact_xxxx_403Forbidden()
        #endregion 403 Forbidden tests

        #region 404 Not Found tests
        //public void PublishArtifact_xxxx_404NotFound()
        #endregion 404 Not Found tests

        #region 409 Conflict tests
        //public void PublishArtifact_xxxx_409Conflict()
        #endregion 409 Conflict tests
    }
}
