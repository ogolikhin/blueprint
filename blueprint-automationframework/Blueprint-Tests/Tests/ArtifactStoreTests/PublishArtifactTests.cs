using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.Factories;
using NUnit.Framework;
using TestCommon;
using Utilities;
using Utilities.Facades;

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
        [TestRail(0)]
        [Description("Create, save, publish Process artifact, checks returned result is 400 Bad Request.")]
        public void PublishArtifact_SingleSavedArtifact_BadRequest()
        {
            // Setup:
            string tokenValue = _user.Token?.AccessControlToken;
            RestApiFacade restApi = new RestApiFacade(Helper.BlueprintServer.Address, tokenValue);
            string requestBody = "[]";

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => restApi.SendRequestBodyAndGetResponse(
                PUBLISH_PATH,
                RestRequestMethod.POST,
                requestBody,
                "application/json"),
                "'POST {0}' should return 400 Bad Request if body of the request does not have any artifact ids!", PUBLISH_PATH);
            
            // Verify:
            const string expectedMessage = "{\"message\":\"The list of artifact Ids is empty.\",\"errorCode\":103}";
            Assert.IsTrue(ex.RestResponse.Content.Contains(expectedMessage));
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(0)]
        [Description("Create, save, publish Actor artifact, checks returned result is 400 Bad Request for artifact that already published")]
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
        [TestRail(165860)]
        [Description("Create & save a single artifact.  Publish the artifact with wrong token.  Verify publish is sreturns code 401 Unauthorized.")]
        public void PublishArtifact_SingleSavedArtifact_Unauthorized(BaseArtifactType artifactType)
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
        [TestRail(0)]
        [Description("Create, save, publish Process artifact, checks returned result is 404 Not Found.")]
        public void PublishArtifact_SinglePublishedArtifact_NotFound(BaseArtifactType artifactType)
        {
            // Setup:
            IUser anotherUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            var artifact = Helper.CreateAndPublishArtifact(_project, anotherUser, artifactType);

            artifact.Delete(anotherUser);
            artifact.Publish(anotherUser);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.PublishArtifact(artifact, _user),
                "'POST {0}' should return 404 Not Found if the Artifact ID doesn't exist!", PUBLISH_PATH);

            string expectedMessage = "{\"message\":\"Artifact with Id " + artifact.Id + " is deleted.\",\"errorCode\":101}";
            Assert.IsTrue(ex.RestResponse.Content.Equals(expectedMessage));
        }
        #endregion 404 Not Found tests

        #region 409 Conflict tests
        [TestCase(BaseArtifactType.Process)]
        [TestRail(0)]
        [Description("Create, save, publish document artifact, checks returned result is 409 Not Found.")]
        public void PublishArtifact_SaveDependendArtifacts_Conflict(BaseArtifactType artifactType)
        {
            // Setup:
            IOpenApiArtifact parentArtifact = CreateParentAndTwoChildrenArtifactsAndGetParentArtifact(artifactType);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => Helper.ArtifactStore.PublishArtifact(parentArtifact, _user),
                "'POST {0}' should return 409 Conflict if the Artifact has dependend artifact which not published!", PUBLISH_PATH);

            string expectedMessage = "{\"message\":\"Artifact with Id " + parentArtifact.Id + " is deleted.\",\"errorCode\":101}";
            Assert.IsTrue(ex.RestResponse.Content.Equals(expectedMessage));
        }
        #endregion 409 Conflict tests

        #region private functions
        private IOpenApiArtifact CreateParentAndTwoChildrenArtifactsAndGetParentArtifact(BaseArtifactType artifactType)
        {
            IOpenApiArtifact parentArtifact, childArtifact;

            //Create parent artifact with ArtifactType and populate all required values without properties
            parentArtifact = Helper.CreateOpenApiArtifact(_project, _user, artifactType);
            //add the created artifact object into BP using OpenAPI call - assertions are inside of AddArtifact
            parentArtifact.Save();

            //Create first child artifact with ArtifactType and populate all required values without properties
            childArtifact = Helper.CreateOpenApiArtifact(_project, _user, artifactType);
            //add the created artifact object into BP using OpenAPI call - assertions are inside of AddArtifact
            childArtifact.ParentId = parentArtifact.Id;
            childArtifact.Save();

            //Create second child artifact with ArtifactType and populate all required values without properties
            childArtifact = Helper.CreateOpenApiArtifact(_project, _user, artifactType);
            //add the created artifact object into BP using OpenAPI call - assertions are inside of AddArtifact
            childArtifact.ParentId = parentArtifact.Id;
            childArtifact.Save();

            return parentArtifact;
        }
        #endregion private functions
    }
}