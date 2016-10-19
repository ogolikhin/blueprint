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
    public class LegacyDiagramArtifactHistoryTests : TestBase
    {
        private IUser _user = null;
        private IProject _project = null;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_user);
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region 200 OK Tests

        [TestCase(BaseArtifactType.DomainDiagram)]
        [TestCase(BaseArtifactType.GenericDiagram)]
        [TestCase(BaseArtifactType.UseCaseDiagram)]
        [TestRail(182934)]
        [Description("Create & publish a diagram artifact, Get DiagramArtifact.  Verify that the latest version of valid diagram artifact is returned.")]
        public void GetDiagramArtifact_PublishAndGetDiagamArtifact_ReturnsLatestVersionOfDiagramArtifact(BaseArtifactType artifactType)
        {
            // Setup: Create and publish a diagram artifact
            var publishedDiagramArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType: artifactType);
            var retrievedArtifact = Helper.ArtifactStore.GetArtifactDetails(_user, publishedDiagramArtifact.Id);
            NovaDiagramArtifact diagramArtifact = null;

            // Execute: Get the diagram artifact using GetDiagramArtifact
            Assert.DoesNotThrow(() => {
                diagramArtifact = Helper.ArtifactStore.GetDiagramArtifact(_user, publishedDiagramArtifact.Id);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", RestPaths.Svc.ArtifactStore.DIAGRAM_id_);

            // Validation: Verify that the returned from GetDiagramArtifact is in valid format
            ArtifactStoreHelper.AssertArtifactsEqual(diagramArtifact, retrievedArtifact);
        }

        [TestCase(2,BaseArtifactType.DomainDiagram)]
        [TestCase(3,BaseArtifactType.GenericDiagram)]
        [TestCase(4,BaseArtifactType.UseCaseDiagram)]
        [TestRail(183352)]
        [Description("Create & publish a diagram artifact multiple times to have multiple version of it, Get diagram artifact without version. Verify that latest version of artifact is returned.")]
        public void GetDiagramArtifact_PublishAndGetDiagramArtifactWithoutSpecificVersion_ReturnsLatestVersionOfDiagramArtifact(int numberOfVersions, BaseArtifactType artifactType)
        {
            // Setup: Create and publish a diagram artifact multiple times to have multiple versions of it
            var publishedDiagramArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType: artifactType, numberOfVersions: numberOfVersions);
            // getting the latest version of the artifact using open API GetArtifact
            var retrievedArtifact = Helper.ArtifactStore.GetArtifactDetails(_user, publishedDiagramArtifact.Id);
            NovaDiagramArtifact diagramArtifact = null;

            // Execute: Get the diagram artifact using GetDiagramArtifact without versionId parameter
            Assert.DoesNotThrow(() => {
                diagramArtifact = Helper.ArtifactStore.GetDiagramArtifact(_user, publishedDiagramArtifact.Id);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", RestPaths.Svc.ArtifactStore.DIAGRAM_id_);

            // Validation: Verify that the returned from GetDiagramArtifact in valid format
            ArtifactStoreHelper.AssertArtifactsEqual(diagramArtifact, retrievedArtifact);
        }

        [TestCase(BaseArtifactType.DomainDiagram)]
        [TestCase(BaseArtifactType.GenericDiagram)]
        [TestCase(BaseArtifactType.UseCaseDiagram)]
        [TestRail(183355)]
        [Description("Create & publish a diagram artifact, modify & publish it again, GetDiagramArtifact with versionId=1. Verify that first version of diagram artifact is returned.")]
        public void GetDiagramArtifact_PublishAndGetDiagramArtifactWithVersion1_ReturnsFirstVersionOfDiagramArtifact(BaseArtifactType artifactType)
        {
            // Setup: Create and publish a diagram artifact two times to have two versions of it			
            IArtifact publishedDiagramArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType: artifactType, numberOfVersions: 2);
            var retrievedArtifactVersion1 = Helper.ArtifactStore.GetArtifactDetails(_user, publishedDiagramArtifact.Id, versionId: 1);
            NovaDiagramArtifact diagramArtifact = null;

            // Execute: Get the diagram artifact using GetDiagramArtifact with first versionId			
            Assert.DoesNotThrow(() => {
                diagramArtifact = Helper.ArtifactStore.GetDiagramArtifact(_user, publishedDiagramArtifact.Id, versionId: 1);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", RestPaths.Svc.ArtifactStore.DIAGRAM_id_);

            ArtifactStoreHelper.AssertArtifactsEqual(diagramArtifact, retrievedArtifactVersion1);
        }

        #endregion 200 OK Tests

        #region 401 Unauthorized Tests

        [TestCase("", BaseArtifactType.DomainDiagram)]
        [TestCase("invalidTokenString", BaseArtifactType.GenericDiagram)]
        [TestRail(183033)]
        [Description("Create & publish a diagram artifact, Get DiagramArtifact with invalid token header.  Verify 401 Unauthorized.")]
        public void GetDiagramArtifact_PublishAndGetDiagamArtifactWithInvalidTokenHeader_401Unauthorized(string token, BaseArtifactType artifactType)
        {
            // Setup: Create and publish a diagram artifact
            var publishedDiagramArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType: artifactType);
            NovaDiagramArtifact diagramArtifact = null;

            IUser userWithBadOrMissingToken = UserFactory.CreateUserAndAddToDatabase();
            userWithBadOrMissingToken.Token.SetToken(token);

            // Execute: Get the diagram artifact with invalid token header using GetDiagramArtifact
            Assert.Throws<Http401UnauthorizedException>(() => diagramArtifact = Helper.ArtifactStore.GetDiagramArtifact(userWithBadOrMissingToken, publishedDiagramArtifact.Id, versionId: 1), "Calling GET {0} with invalid token should return 401 Unauthorized!", RestPaths.Svc.ArtifactStore.DIAGRAM_id_);
        }

        #endregion 401 Unauthorized Tests

        #region 403 Forbidden Tests
        #endregion 403 Forbidden Tests

        #region 404 Not Found Tests

        [TestCase(0, BaseArtifactType.DomainDiagram)]
        [TestCase(-10, BaseArtifactType.GenericDiagram)]
        [TestCase(999, BaseArtifactType.UseCaseDiagram)]
        [TestRail(183027)]
        [Description("Create & publish a diagram artifact, Get DiagramArtifact with invalid versionId.  Verify 404 NotFound.")]
        public void GetDiagramArtifact_PublishAndGetDiagamArtifactWithInvalidVersionId_404NotFound(int versionId, BaseArtifactType artifactType)
        {
            // Setup: Create and publish a diagram artifact
            var publishedDiagramArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType: artifactType);

            // Execute: Get the diagram artifact with invalid versionId using GetDiagramArtifact
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetDiagramArtifact(_user, publishedDiagramArtifact.Id, versionId: versionId), "GetDiagramArtifact call with invalid versionId does not exit with 404 NotFoundException!");

            var serviceErrorMessage = Deserialization.DeserializeObject<ServiceErrorMessage>(ex.RestResponse.Content);

            // Validation: Exception should contain proper errorCode in the response content.
            Assert.That(serviceErrorMessage.ErrorCode.Equals(InternalApiErrorCodes.ItemNotFound), "GetDiagramArtifact with invalid versionId should return {0} errorCode but {1} is returned", ErrorCodes.ResourceNotFound, serviceErrorMessage.ErrorCode);
        }

        #endregion 404 Not Found Tests

        #region Private Functions
        #endregion Private Functions
    }
}
