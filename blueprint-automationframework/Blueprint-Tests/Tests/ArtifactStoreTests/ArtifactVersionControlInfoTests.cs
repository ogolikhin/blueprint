using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using NUnit.Framework;
using TestCommon;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class ArtifactVersionControlInfoTests : TestBase
    {
        private const string SVC_PATH = RestPaths.Svc.ArtifactStore.Artifacts.VERSION_CONTROL_INFO_id_;

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

        #region changes

        [TestCase(BaseArtifactType.Actor, 1)]
        [TestCase(BaseArtifactType.Process, 2)]
        [TestCase(BaseArtifactType.UseCase, 3)]
        [TestRail(182452)]
        public void VersionControlInfo_PublishedArtifact_NoChanges_ReturnsArtifactInfo(BaseArtifactType artifactType, int numberOfVersions)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, numberOfVersions: numberOfVersions);

            INovaVersionControlArtifactInfo artifactBaseInfo = null;

            // Execute:
            Assert.DoesNotThrow(() => artifactBaseInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, artifact.Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);

            artifactDetails.AssertEquals(artifactBaseInfo);

            Assert.IsTrue(artifactBaseInfo.HasChanges.HasValue);
            Assert.IsFalse((bool)artifactBaseInfo.HasChanges);
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(182453)]
        public void VersionControlInfo_SavedArtifact_HasChanges_ReturnsArtifactInfo(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndSaveArtifact(_project, _user, artifactType);

            INovaVersionControlArtifactInfo artifactBaseInfo = null;

            // Execute:
            Assert.DoesNotThrow(() => artifactBaseInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, artifact.Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            Assert.IsTrue(artifactBaseInfo.HasChanges.HasValue);
            Assert.IsTrue((bool)artifactBaseInfo.HasChanges);

            Assert.IsNotNull(artifactBaseInfo.LockedDateTime);
            Assert.AreEqual(artifactBaseInfo.LockedByUser.Id, _user.Id);
        }

        [TestCase(BaseArtifactType.Actor, 1)]
        [TestCase(BaseArtifactType.Process, 2)]
        [TestCase(BaseArtifactType.UseCase, 3)]
        [TestRail(182499)]
        public void VersionControlInfo_PublishedArtifact_NoChangesForAnotherUser_ReturnsArtifactInfo(BaseArtifactType artifactType, int numberOfVersions)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, numberOfVersions: numberOfVersions);
            artifact.Save(_user);

            IUser anotherUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            INovaVersionControlArtifactInfo artifactBaseInfo = null;

            // Execute:
            Assert.DoesNotThrow(() => artifactBaseInfo = Helper.ArtifactStore.GetVersionControlInfo(anotherUser, artifact.Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(anotherUser, artifact.Id);

            artifactDetails.AssertEquals(artifactBaseInfo);

            Assert.IsTrue(artifactBaseInfo.HasChanges.HasValue);
            Assert.IsFalse((bool)artifactBaseInfo.HasChanges);
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(182500)]
        public void VersionControlInfo_SavedArtifact_HasChangesForAnotherUser_ReturnsArtifactInfo(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase);

            sourceArtifact.Save();

            IUser anotherUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact,
                targetArtifact, TraceDirection.From, anotherUser);

            INovaVersionControlArtifactInfo artifactBaseInfo = null;

            // Execute:
            Assert.DoesNotThrow(() => artifactBaseInfo = Helper.ArtifactStore.GetVersionControlInfo(anotherUser, sourceArtifact.Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            Assert.IsTrue(artifactBaseInfo.HasChanges.HasValue);
            Assert.IsTrue((bool)artifactBaseInfo.HasChanges);

            Assert.IsNotNull(artifactBaseInfo.LockedDateTime);
            Assert.AreEqual(artifactBaseInfo.LockedByUser.Id, _user.Id);
        }

        #endregion changes

        // TODO: Unpublished Sub-artifact in published artifact.
        // TODO: Unpublished Sub-artifact in unpublished artifact.
        // TODO: Published Sub-artifact.
        // TODO: Published Sub-artifact in locked Artifact without changes.
        // TODO: Published Sub-artifact in locked Artifact with changes.
        // TODO: Published Sub-artifact in locked Artifact without changes for another user.
        // TODO: Published Sub-artifact in locked Artifact with changes for another user.

        // TODO: Published deleted Artifact(- isDeleted= false).
        // TODO: Saved deleted Artifact.
        // TODO: Saved deleted Artifact for another user.
        // TODO: Sub-Artifact in published deleted Artifact(- isDeleted= false).
        // TODO: Sub-Artifact in saved deleted Artifact.
        // TODO: Published deleted Sub-artifact in live Artifact.
        // TODO: Saved deleted Sub-artifact in live Artifact.
        // TODO: Saved deleted Sub-artifact in live Artifact for another user.

        #endregion 200 OK tests

        #region Negative tests

        // TODO: Call GetVersionControlInfo without a token header.  Verify 400 Bad Request.
        // TODO: Call GetVersionControlInfo with a bad token.  Verify 401 Unauthorized.
        // TODO: Call GetVersionControlInfo with an artifact the user doesn't have access to.  Verify 403 Forbiden.
        // TODO: Call GetVersionControlInfo with an artifact in a project the user doesn't have access to.  Verify 403 Forbidden.
        // TODO: Call GetVersionControlInfo with a non-existent artifact ID.  Verify 404 Not Found.
        // TODO: Call GetVersionControlInfo with an unpublished artifact with a different user.  Verify 404 Not Found.
        // TODO: Call GetVersionControlInfo with an unpublished sub-artifact of a published artifact with a different user.  Verify 404 Not Found.
        // TODO: Call GetVersionControlInfo with an unpublished sub-artifact of an unpublished artifact with a different user.  Verify 404 Not Found.

        #endregion Negative tests
    }
}
