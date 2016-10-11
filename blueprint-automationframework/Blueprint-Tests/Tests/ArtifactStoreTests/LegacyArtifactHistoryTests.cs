using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel.Impl;
using Model.Factories;
using NUnit.Framework;
using TestCommon;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class LegacyArtifactHistoryTests : TestBase
    {
        private IUser _user = null;
        private IProject _project = null;

        #region Setup and Cleanup

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

        #endregion Setup and Cleanup

        #region 200 OK Tests

        [TestCase]
        [TestRail(182934)]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [Description("Create & publish a diagram artifact, GetNovaDiagramArtifact.  Verify that the valid diagram artifact is returned.")]
        public void GetDiagramArtifact_PublishDiagamArtifact_ReturnsDiagramArtifact()
        {
            // Setup: Create and publish a diagram artifact
            var publishedDiagramArtifact = Helper.CreateAndPublishArtifact(_project, _user, Model.ArtifactModel.BaseArtifactType.GenericDiagram);
            var retrievedArtifact = OpenApiArtifact.GetArtifact(publishedDiagramArtifact.Address, _project, publishedDiagramArtifact.Id, _user);

            // Execute: Get the diagram artifact using GetNovaDiagramArtifact
            NovaDiagramArtifact diagramArtifact = null;

            Assert.DoesNotThrow(() => {
                diagramArtifact = Helper.ArtifactStore.GetDiagramArtifact(_user, publishedDiagramArtifact.Id, versionId: 1);
            }, "'GET {0}' should return 200 OK when passed a valid diagram artifact ID!", RestPaths.Svc.ArtifactStore.DIAGRAM_id_);

            // Validation: Verify that the returned from GetNovaDiagramArtifact is valid format
            ArtifactStoreHelper.AssertEquals(diagramArtifact, retrievedArtifact);
        }

        #endregion 200 OK Tests

        #region 401 Unauthorized Tests
        #endregion 401 Unauthorized Tests

        #region 403 Forbidden Tests
        #endregion 403 Forbidden Tests

        #region 404 Not Found Tests
        #endregion 404 Not Found Tests

        #region Private Functions
        #endregion Private Functions
    }
}
