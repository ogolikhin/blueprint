using System.Collections.Generic;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.Factories;
using NUnit.Framework;
using TestCommon;
using Utilities;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class DeleteArtifactTests : TestBase
    {
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

        #region 200 OK tests

        [TestCase(BaseArtifactType.Actor)]
        public void DeleteArtifact_PublishedArtifactWithNoChildren_ArtifactIsDeleted(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            // Execute:
            List<INovaArtifactResponse> deletedArtifacts = null;

            Assert.DoesNotThrow(() => deletedArtifacts = Helper.ArtifactStore.DeleteArtifact(artifact, _user),
                "'DELETE {0}' should return 200 OK if a valid artifact ID is sent!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            // Verify:
            Assert.AreEqual(1, deletedArtifacts.Count, "There should only be 1 deleted artifact ID returned!");
            Assert.AreEqual(artifact.Id, deletedArtifacts[0].Id, "The artifact ID doesn't match the one that we deleted!");

            // Try to get the artifact and verify it's deleted.
            Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id),
                "We should get a 404 Not Found when trying to get artifact details of a deleted artifact!");
        }

        #endregion 200 OK tests

        #region 400 Bad Request tests
        // DeleteArtifact_xxxx_400BadRequest()
        #endregion 400 Bad Request tests

        #region 401 Unauthorized tests
        // DeleteArtifact_xxxx_401Unauthorized()
        #endregion 401 Unauthorized tests

        #region 403 Forbidden tests
        // DeleteArtifact_xxxx_403Forbidden()
        #endregion 403 Forbidden tests

        #region 404 Not Found tests
        // DeleteArtifact_xxxx_404NotFound()
        #endregion 404 Not Found tests

        #region 409 Conflict tests
        // DeleteArtifact_xxxx_409Conflict()
        #endregion 409 Conflict tests
    }
}
