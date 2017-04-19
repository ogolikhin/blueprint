using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
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
    public class LegacyDiagramArtifactTests : TestBase
    {
        private IUser _user = null;
        private IProject _project = null;

        private static int USECASEDIAGRAM_WITHACTOR_ID = 144;

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

        [TestCase(2, ItemTypePredefined.DomainDiagram)]
        [TestCase(3, ItemTypePredefined.GenericDiagram)]
        [TestCase(4, ItemTypePredefined.UseCaseDiagram)]
        [TestRail(183352)]
        [Description("Create & publish a diagram artifact multiple times to have multiple version of it, Get diagram artifact without version.  " +
                     "Verify that latest version of artifact is returned.")]
        public void GetDiagramArtifact_PublishAndGetDiagramArtifactWithoutSpecificVersion_ReturnsLatestVersionOfDiagramArtifact(int numberOfVersions, ItemTypePredefined artifactType)
        {
            // Setup: Create and publish a diagram artifact multiple times to have multiple versions of it
            var publishedDiagramArtifact = Helper.CreateAndPublishNovaArtifactWithMultipleVersions(_user, _project, artifactType, numberOfVersions: numberOfVersions);
            // getting the latest version of the artifact using open API GetArtifact
            var retrievedArtifact = Helper.ArtifactStore.GetArtifactDetails(_user, publishedDiagramArtifact.Id);
            var viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);

            // Execute: Get the diagram artifact using GetDiagramArtifact without versionId parameter
            NovaDiagramArtifact diagramArtifact = null;
            Assert.DoesNotThrow(() => {diagramArtifact = Helper.ArtifactStore.GetDiagramArtifact(viewer, publishedDiagramArtifact.Id);},
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", RestPaths.Svc.ArtifactStore.DIAGRAM_id_);

            // Validation: Verify that the returned from GetDiagramArtifact in valid format
            NovaArtifactDetails.AssertArtifactsEqual(diagramArtifact, retrievedArtifact);
        }

        [TestCase(ItemTypePredefined.DomainDiagram)]
        [TestCase(ItemTypePredefined.GenericDiagram)]
        [TestCase(ItemTypePredefined.UseCaseDiagram)]
        [TestRail(183355)]
        [Description("Create & publish a diagram artifact, modify & publish it again, GetDiagramArtifact with versionId=1.  " +
                     "Verify that first version of diagram artifact is returned.")]
        public void GetDiagramArtifact_PublishAndGetDiagramArtifactWithVersion1_ReturnsFirstVersionOfDiagramArtifact(ItemTypePredefined artifactType)
        {
            // Setup: Create and publish a diagram artifact two times to have two versions of it			
            var publishedDiagramArtifact = Helper.CreateAndPublishNovaArtifactWithMultipleVersions(_user, _project, artifactType, numberOfVersions: 2);
            var retrievedArtifactVersion1 = Helper.ArtifactStore.GetArtifactDetails(_user, publishedDiagramArtifact.Id, versionId: 1);
            var viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);

            // Execute: Get the diagram artifact using GetDiagramArtifact with first versionId			
            NovaDiagramArtifact diagramArtifact = null;
            Assert.DoesNotThrow(() => {diagramArtifact = Helper.ArtifactStore.GetDiagramArtifact(viewer, publishedDiagramArtifact.Id, versionId: 1);},
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", RestPaths.Svc.ArtifactStore.DIAGRAM_id_);

            NovaArtifactDetails.AssertArtifactsEqual(diagramArtifact, retrievedArtifactVersion1);
        }

        [Explicit(IgnoreReasons.ProductBug)] // Trello bug: https://trello.com/c/hy2AlWHo
        [Category(Categories.CustomData)]
        [Category(Categories.GoldenData)]
        [TestCase]
        [TestRail(290207)]
        [Description("Get the use case diagram artifact which contains an Actor association with attachment & comment. " +
            "Verify that the indicator flags contains the values for traces, attachements and comments.")]
        public void GetUseCaseDiagramArtifact_WithActor_VerifyIndicatorFlags()
        {
            // getting the latest version of the artifact using open API GetArtifact
            var retrievedArtifact = Helper.ArtifactStore.GetArtifactDetails(_user, USECASEDIAGRAM_WITHACTOR_ID);

            // Execution: Get the use case diagram artifact an actor association
            NovaDiagramArtifact usecaseDiagramArtifact = null;
            Assert.DoesNotThrow(() => { usecaseDiagramArtifact = Helper.ArtifactStore.GetDiagramArtifact(_user, USECASEDIAGRAM_WITHACTOR_ID); },
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", RestPaths.Svc.ArtifactStore.DIAGRAM_id_);

            // Verify: Verify that the shape indicatorflags contains traces, attachments and comments values.
            NovaArtifactDetails.AssertArtifactsEqual(usecaseDiagramArtifact, retrievedArtifact);

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, USECASEDIAGRAM_WITHACTOR_ID,
                ItemIndicatorFlags.HasManualReuseOrOtherTraces | ItemIndicatorFlags.HasAttachmentsOrDocumentRefs | ItemIndicatorFlags.HasComments,
                usecaseDiagramArtifact.Shapes[0].Id);
        }

        #endregion 200 OK Tests

        #region 401 Unauthorized Tests

        [TestCase("", BaseArtifactType.DomainDiagram)]
        [TestCase("invalidTokenString", BaseArtifactType.GenericDiagram)]
        [TestRail(183033)]
        [Description("Create & publish a diagram artifact, Get DiagramArtifact with invalid token header. Verify 401 Unauthorized.")]
        public void GetDiagramArtifact_PublishAndGetDiagamArtifactWithInvalidTokenHeader_401Unauthorized(string token, BaseArtifactType artifactType)
        {
            // Setup: Create and publish a diagram artifact
            var publishedDiagramArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType: artifactType);
            var userWithBadOrMissingToken = UserFactory.CreateUserAndAddToDatabase();
            userWithBadOrMissingToken.Token.SetToken(token);

            // Execute: Get the diagram artifact with invalid token header using GetDiagramArtifact
            Assert.Throws<Http401UnauthorizedException>(() => Helper.ArtifactStore.GetDiagramArtifact(userWithBadOrMissingToken, publishedDiagramArtifact.Id, versionId: 1),
                "Calling GET {0} with invalid token should return 401 Unauthorized!", RestPaths.Svc.ArtifactStore.DIAGRAM_id_);
        }

        #endregion 401 Unauthorized Tests

        #region 403 Forbidden Tests

        [TestCase(BaseArtifactType.DomainDiagram)]
        [TestCase(BaseArtifactType.GenericDiagram)]
        [TestCase(BaseArtifactType.UseCaseDiagram)]
        [TestRail(195409)]
        [Description("Create & publish a diagram artifact, Get DiagramArtifact with the user with no permission to the artifact. Verify 403 Forbidden exception is returned.")]
        public void GetDiagramArtifact_PublishAndGetDiagamArtifactWithNoPermissionForTheArtifact_403Forbidden(BaseArtifactType artifactType)
        {
            // Setup: Create and publish a diagram artifact
            var publishedDiagramArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType: artifactType);
            var userWithNonePermissionForArtifact = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);
            Helper.AssignProjectRolePermissionsToUser(userWithNonePermissionForArtifact, TestHelper.ProjectRole.None, _project, publishedDiagramArtifact);

            // Execute: Get the diagram artifact using GetDiagramArtifact
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.GetDiagramArtifact(userWithNonePermissionForArtifact, publishedDiagramArtifact.Id),
                "Calling GET {0} with the user with the user which has no permission to the artifact should return 403 Forbidden!", RestPaths.Svc.ArtifactStore.DIAGRAM_id_);

            // Validation: Verify that the returned from GetDiagramArtifact is in valid format
            var serviceErrorMessage = SerializationUtilities.DeserializeObject<ServiceErrorMessage>(ex.RestResponse.Content);
            Assert.AreEqual(InternalApiErrorCodes.Forbidden, serviceErrorMessage.ErrorCode,
                "Error code for GetDiagramArtifact with the user which has no permission to the artifact should be {0}",
                InternalApiErrorCodes.Forbidden);
        }

        #endregion 403 Forbidden Tests

        #region 404 Not Found Tests

        [TestCase(0, BaseArtifactType.DomainDiagram)]
        [TestCase(-10, BaseArtifactType.GenericDiagram)]
        [TestCase(999, BaseArtifactType.UseCaseDiagram)]
        [TestRail(183027)]
        [Description("Create & publish a diagram artifact, Get DiagramArtifact with invalid versionId. Verify 404 NotFound.")]
        public void GetDiagramArtifact_PublishAndGetDiagamArtifactWithInvalidVersionId_404NotFound(int versionId, BaseArtifactType artifactType)
        {
            // Setup: Create and publish a diagram artifact
            var publishedDiagramArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType: artifactType);
            var viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);

            // Execute: Get the diagram artifact with invalid versionId using GetDiagramArtifact
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetDiagramArtifact(viewer, publishedDiagramArtifact.Id, versionId: versionId), "GetDiagramArtifact call with invalid versionId does not exit with 404 NotFoundException!");

            var serviceErrorMessage = SerializationUtilities.DeserializeObject<ServiceErrorMessage>(ex.RestResponse.Content);

            // Validation: Exception should contain proper errorCode in the response content.
            Assert.AreEqual(InternalApiErrorCodes.ItemNotFound, serviceErrorMessage.ErrorCode, "Error code for GetDiagramArtifact with invalid versionId should be {0}", InternalApiErrorCodes.ItemNotFound);
        }

        #endregion 404 Not Found Tests
    }
}
