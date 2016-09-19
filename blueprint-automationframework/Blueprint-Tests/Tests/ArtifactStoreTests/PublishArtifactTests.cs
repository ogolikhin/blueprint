using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.Factories;
using NUnit.Framework;
using System.Collections.Generic;
using TestCommon;
using Utilities;

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
        [TestCase]
        [TestRail(165971)]
        [Description("Send empty list of artifacts, checks returned result is 400 Bad Request.")]
        public void PublishArtifact_EmptyArtifactList_BadRequest()
        {
            // Setup:
            List<IArtifactBase> artifacts = new List<IArtifactBase>();

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => Helper.ArtifactStore.PublishArtifacts(artifacts, _user),
            "'POST {0}' should return 400 Bad Request if body of the request does not have any artifact ids!", PUBLISH_PATH);

            // Verify:
            const string expectedMessage = "{\"message\":\"The list of artifact Ids is empty.\",\"errorCode\":103}";
            Assert.IsTrue(ex.RestResponse.Content.Contains(expectedMessage));
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(165972)]
        [Description("Create, save, publish Actor artifact.  Verify 400 Bad Request is returned for an artifact that is already published.")]

        public void PublishArtifact_SinglePublishedArtifact_BadRequest(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => Helper.ArtifactStore.PublishArtifact(artifact, _user),
                "'POST {0}' should return 400 Bad Request if an artifact already published!", PUBLISH_PATH);

            // Verify:{"message":"Artifact with Id 80654 has nothing to publish.","errorCode":114}
            string expectedMessage = "{\"message\":\"Artifact with Id " + artifact.Id + " has nothing to publish.\",\"errorCode\":114}";
            Assert.IsTrue(ex.RestResponse.Content.Contains(expectedMessage));

        }
        #endregion 400 Bad Request tests

        #region 401 Unauthorized tests
        [TestCase(BaseArtifactType.Actor)]
        [TestRail(165975)]
        [Description("Create & save a single artifact.  Publish the artifact with wrong token.  Verify publish returns 401 Unauthorized.")]
        public void PublishArtifact_InvalidToken_Unauthorized(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _user, artifactType);

            IUser userWithBadToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() => Helper.ArtifactStore.PublishArtifact(artifact, userWithBadToken),
                "'POST {0}' should return 401 Unauthorized if a token is invalid!", PUBLISH_PATH);
            
            // Verify:
            const string expectedMessage = "\"Unauthorized call\"";
            Assert.IsTrue(ex.RestResponse.Content.Equals(expectedMessage));
        }

        #endregion 401 Unauthorized tests

        #region 403 Forbidden tests
        //public void PublishArtifact_xxxx_403Forbidden()
        #endregion 403 Forbidden tests

        #region 404 Not Found tests
        [TestCase(BaseArtifactType.Process)]
        [TestRail(165973)]
        [Description("Create, save, publish, delete Process artifact by another user, checks returned result is 404 Not Found.")]
        public void PublishArtifact_PublishedArtifactDeletedByAnotherUser_NotFound(BaseArtifactType artifactType)
        {
            // Setup:
            IUser anotherUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            var artifact = Helper.CreateAndPublishArtifact(_project, anotherUser, artifactType);

            artifact.Delete(anotherUser);
            artifact.Publish(anotherUser);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.PublishArtifact(artifact, _user),
                "'POST {0}' should return 404 Not Found if the Artifact ID doesn't exist!", PUBLISH_PATH);

            // Verify:
            string expectedMessage = "{\"message\":\"Artifact with Id " + artifact.Id + " is deleted.\",\"errorCode\":101}";
            Assert.IsTrue(ex.RestResponse.Content.Equals(expectedMessage));
        }

        [TestCase(int.MaxValue)]
        [TestRail(165976)]
        [Description("Try to publish an artifact with a non-existent Artifact ID.  Verify 404 Not Found is returned.")]
        public void PublishArtifact_NonExistentArtifactId_NotFound(int nonExistentArtifactId)
        {
            // Setup:
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Process);

            // Replace ProjectId with a fake ID that shouldn't exist.
            artifact.Id = nonExistentArtifactId;

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.PublishArtifact(artifact, _user),
                "'POST {0}' should return 404 Not Found if the Artifact ID doesn't exist!", PUBLISH_PATH);

            // Verify:
            string expectedMessage = "{\"message\":\"Item with Id " + artifact.Id + " is not found.\",\"errorCode\":101}";
            Assert.IsTrue(ex.RestResponse.Content.Equals(expectedMessage));
        }

        #endregion 404 Not Found tests

        #region 409 Conflict tests
        [TestCase(BaseArtifactType.Process)]
        [TestRail(165974)]
        [Description("Create, save, parent artifact with two children, publish child artifact, checks returned result is 409 Not Found.")]
        public void PublishArtifact_ParentAndChildArtifacts_OnlyPublishChild_Conflict(BaseArtifactType artifactType)
        {
            // Setup:
            List<IArtifact> artifactList = CreateParentAndTwoChildrenArtifactsAndGetParentArtifact(artifactType);
            IArtifact childArtifact = artifactList[2];

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => Helper.ArtifactStore.PublishArtifact(childArtifact, _user),
                "'POST {0}' should return 409 Conflict if the Artifact has parent artifact which is not published!", PUBLISH_PATH);
            
            // Verify:
            string expectedMessage = "{\"message\":\"Specified artifacts have dependent artifacts to publish.\",\"errorCode\":120,\"errorContent\":{";
            Assert.IsTrue(ex.RestResponse.Content.Contains(expectedMessage));
        }
        #endregion 409 Conflict tests

        #region private functions
        private List<IArtifact> CreateParentAndTwoChildrenArtifactsAndGetParentArtifact(BaseArtifactType artifactType)
        {
            var artifactTypes = new BaseArtifactType[] { artifactType, artifactType, artifactType };
            var artifactChain = Helper.CreateSavedArtifactChain(_project, _user, artifactTypes);
            return artifactChain;
        }
        #endregion private functions
    }
}
