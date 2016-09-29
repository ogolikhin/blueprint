using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.Impl;
using NUnit.Framework;
using TestCommon;
using Utilities;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class MoveArtifactTests : TestBase
    {
        private const string SVC_PATH = RestPaths.Svc.ArtifactStore.Artifacts_id_.TO_id_;

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

        [TestCase(BaseArtifactType.PrimitiveFolder)]
        [TestRail(182346)]
        [Description("Create & publish 3 artifacts. Create chain : grandparent, parent and child. Move parent artifact with a child to be a child of the project.  Verify the moved artifact is returned with the updated Parent ID.")]
        public void MoveArtifact_PublishedArtifactWithDependentChildBecomesChildOfProject_ReturnsArtifactDetails_200OK(BaseArtifactType artifactType)
        {
            // Setup:git 
            IArtifact grandParentArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);            
            IArtifact parentArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, grandParentArtifact);
            Helper.CreateAndPublishArtifact(_project, _user, artifactType, parentArtifact);

            INovaArtifactDetails movedArtifactDetails = null;

            parentArtifact.Lock();

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = ArtifactStore.MoveArtifact(Helper.BlueprintServer.Address, parentArtifact, _project.Id, _user);
            }, "'GET {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            // Verify:
            INovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, parentArtifact.Id);
            NovaArtifactDetails.AssertEquals(artifactDetails, movedArtifactDetails);
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182373)]
        [Description("Create & publish 2 artifacts.  Move one artifact to be a child of the other.  Send correct version of artifact with the message. Verify the moved artifact is returned with the updated Parent ID.")]
        public void MoveArtifact_SendCurrentVersionWithRequest_PublishedArtifactBecomesChildOfPublishedArtifact_ReturnsArtifactDetails_200OK(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            IArtifact newParentArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            artifact.Lock();
            INovaArtifactDetails movedArtifactDetails = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = Helper.ArtifactStore.MoveArtifact(artifact, newParentArtifact, _user);
            }, "'GET {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            // Verify:
            INovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);
            NovaArtifactDetails.AssertEquals(artifactDetails, movedArtifactDetails);
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182381)]
        [Description("Create & publish an artifact.  Move the artifact to the same location.  Verify the moved artifact is returned with the updated Parent ID.")]
        public void MoveArtifact_PublishedArtifact_FromProjectRootToProjectRoot_VerifyParentDidNotChange_200OK(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            artifact.Lock();
            INovaArtifactDetails movedArtifactDetails = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = ArtifactStore.MoveArtifact(Helper.BlueprintServer.Address, artifact, _project.Id, _user);
            }, "'GET {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            // Verify:
            INovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);
            NovaArtifactDetails.AssertEquals(artifactDetails, movedArtifactDetails);
            Assert.Equals(movedArtifactDetails.Id, _project.Id);
        }

        #endregion 200 OK tests

        #region 401 Unauthorized tests

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182380)]
        [Description("Create & publish 2 artifacts.  Move one artifact to be a child of the other with invalid token in a request.  Verify response returns code 401 Unauthorized.")]
        public void MoveArtifact_PublishedArtifactMoveToParentArtifactWithInvalidToken_401Unauthorized(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            IArtifact newParentArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            IUser userWithBadToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            artifact.Lock();

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.ArtifactStore.MoveArtifact(artifact, newParentArtifact, userWithBadToken);
            }, "'GET {0}' should return 401 Unauthorized when called with a invalid token!", SVC_PATH);

            // Verify:
            const string expectedExceptionMessage = "Unauthorized call";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "{0} was not found in returned message of move published artifact(s) which has invalid token.", expectedExceptionMessage);
        }

        #endregion 401 Unauthorized tests

        #region 403 Forbidden tests

        #endregion 403 Forbidden tests

        #region 409 Conflict tests

        [TestCase(BaseArtifactType.Process, int.MaxValue)]
        [TestCase(BaseArtifactType.Process, 0)]
        [TestCase(BaseArtifactType.Process, 1)]
        [TestRail(182378)]
        [Description("Create & publish 2 artifacts.  Move one artifact to be a child of the other.  Send incorrect version of artifact with the message. Verify the moved artifact is returned with the updated Parent ID.")]
        public void MoveArtifact_PublishedArtifactBecomesChildOfPublishedArtifact_SendIncorrectVersion_409Conflict(BaseArtifactType artifactType, int artifactVersion)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            IArtifact newParentArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            artifact.Save();
            artifact.Publish();

            artifact.Lock();
            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => Helper.ArtifactStore.MoveArtifact(artifact, newParentArtifact, _user, artifactVersion),
                "'POST {0}' should return 409 Conflict when called with incorrect version!", SVC_PATH);

            // Verify:
            string expectedExceptionMessage = "Cannot move a historical version of an artifact. Please refresh.";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "{0} did not find version in returned message of move artifact call due to incorrect one sent with the request.", expectedExceptionMessage);
        }
         
        [Ignore(IgnoreReasons.UnderDevelopment)] //Not fixed yet
        [TestCase(BaseArtifactType.Process)]
        [TestRail(182394)]
        [Description("Create & publish 2 artifacts.  Move one artifact to be a child of the other. Move parent to be a child of child. Send correct version of artifact with the message. Verify the moved artifact is returned with the updated Parent ID.")]
        public void MoveArtifact_PublishedArtifactBecomesChildOfPublishedArtifact_MoveParentToBeAChildOfAChild_409Conflict(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact parentArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            IArtifact childArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, parentArtifact);

            parentArtifact.Lock();

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => Helper.ArtifactStore.MoveArtifact(parentArtifact, childArtifact, _user),
                "'POST {0}' should return 409 Conflict when parent moved to its child", SVC_PATH);

            // Verify:
            string expectedExceptionMessage = "Cannot move an ancester artifact to this position.";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "{0} when user tries to move parent artifact to its child", expectedExceptionMessage);
        }

        #endregion 409 Conflict tests
    }
}
