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
    public class LegacyGlossaryArtifactHistoryTests : TestBase
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

        [TestCase]
        [TestRail(183019)]
        [Description("Create & publish a glossary artifact, Get GlossaryArtifact.  Verify that the latest version of valid glossary artifact is returned.")]
        public void GetGlossaryArtifact_PublishAndGetGlossaryArtifact_ReturnsLatestVersionOfGlossaryArtifact()
        {
            // Setup: Create and publish a glossary artifact
            var publishedGlossaryArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType: BaseArtifactType.Glossary);
            var retrievedArtifact = Helper.ArtifactStore.GetArtifactDetails(_user, publishedGlossaryArtifact.Id);
            NovaGlossaryArtifact glossaryArtifact = null;

            // Execute: Get the glossary artifact using GetGlossaryArtifact
            Assert.DoesNotThrow(() => {
                glossaryArtifact = Helper.ArtifactStore.GetGlossaryArtifact(_user, publishedGlossaryArtifact.Id);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", RestPaths.Svc.ArtifactStore.GLOSSARY_id_);

            // Validation: Verify that the returned from GetGlossaryArtifact is in valid format
            ArtifactStoreHelper.AssertArtifactsEqual(glossaryArtifact, retrievedArtifact);
        }

        [TestCase(4)]
        [TestRail(183353)]
        [Description("Create & publish a glossary artifact multiple times to have multiple version of it, Get glossary artifact without version. Verify that latest version of artifact is returned.")]
        public void GetGlossaryArtifact_PublishAndGetGlossaryArtifactWithoutSpecificVersion_ReturnsLatestVersionOfGlossaryArtifact(int numberOfVersions)
        {
            // Setup: Create and publish a glossary artifact multiple times to have multiple versions of it
            var publishedGlossaryArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Glossary, numberOfVersions: numberOfVersions);
            // getting the latest version of the artifact using open API GetArtifact
            var retrievedArtifact = Helper.ArtifactStore.GetArtifactDetails(_user, publishedGlossaryArtifact.Id);
            NovaGlossaryArtifact glossaryArtifact = null;

            // Execute: Get the glossary artifact using GetGlossaryArtifact without versionId parameter
            Assert.DoesNotThrow(() => {
                glossaryArtifact = Helper.ArtifactStore.GetGlossaryArtifact(_user, publishedGlossaryArtifact.Id);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", RestPaths.Svc.ArtifactStore.GLOSSARY_id_);

            // Validation: Verify that the returned from GetGlossaryArtifact in valid format
            ArtifactStoreHelper.AssertArtifactsEqual(glossaryArtifact, retrievedArtifact);
        }

        [TestCase]
        [TestRail(183356)]
        [Description("Create & publish a glossary artifact, modify & publish it again, GetGlossaryArtifact with versionId=1. Verify that first version of glossary artifact is returned.")]
        public void GetGlossaryArtifact_PublishAndGetGlossaryArtifactWithVersion1_ReturnsFirstVersionOfGlossaryArtifact()
        {
            // Setup: Create and publish a glossary artifact two times to have two versions of it			
            IArtifact publishedGlossaryArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Glossary, numberOfVersions: 2);
            var retrievedArtifactVersion1 = Helper.ArtifactStore.GetArtifactDetails(_user, publishedGlossaryArtifact.Id, versionId: 1);
            NovaGlossaryArtifact glossaryArtifact = null;

            // Execute: Get the glossary artifact using GetGlossaryArtifact with first versionId			
            Assert.DoesNotThrow(() => {
                glossaryArtifact = Helper.ArtifactStore.GetGlossaryArtifact(_user, publishedGlossaryArtifact.Id, versionId: 1);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", RestPaths.Svc.ArtifactStore.GLOSSARY_id_);

            ArtifactStoreHelper.AssertArtifactsEqual(glossaryArtifact, retrievedArtifactVersion1);
        }

        #endregion 200 OK Tests

        #region 401 Unauthorized Tests

        [TestCase("")]
        [TestCase("invalidTokenString")]
        [TestRail(183034)]
        [Description("Create & publish a glossary artifact, Get GlossaryArtifact with invalid token header.  Verify 401 Unauthorized.")]
        public void GetGlossaryArtifact_PublishAndGetGlossaryArtifactWithInvalidTokenHeader_401Unauthorized(string token)
        {
            // Setup: Create and publish a glossary artifact
            var publishedGlossaryArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Glossary);
            NovaGlossaryArtifact glossaryArtifact = null;

            IUser userWithBadOrMissingToken = UserFactory.CreateUserAndAddToDatabase();
            userWithBadOrMissingToken.Token.SetToken(token);

            // Execute: Get the glossary artifact with invalid token header using GetGlossaryArtifact
            Assert.Throws<Http401UnauthorizedException>(() => glossaryArtifact = Helper.ArtifactStore.GetGlossaryArtifact(userWithBadOrMissingToken, publishedGlossaryArtifact.Id, versionId: 1), "Calling GET {0} with invalid token should return 401 Unauthorized!", RestPaths.Svc.ArtifactStore.GLOSSARY_id_);
        }

        #endregion 401 Unauthorized Tests

        #region 403 Forbidden Tests
        #endregion 403 Forbidden Tests

        #region 404 Not Found Tests

        [TestCase(0)]
        [TestCase(-10)]
        [TestCase(999)]
        [TestRail(183028)]
        [Description("Create & publish a glossary artifact, Get GlossaryArtifact with invalid versionId.  Verify 404 NotFound.")]
        public void GetGlossaryArtifact_PublishAndGetGlossaryArtifactWithInvalidVersionId_404NotFound(int versionId)
        {
            // Setup: Create and publish a glossary artifact
            var publishedGlossaryArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Glossary);
            NovaGlossaryArtifact glossaryArtifact = null;

            // Execute: Get the glossary artifact with invalid versionId using GetGlossaryArtifact
            var ex = Assert.Throws<Http404NotFoundException>(() => glossaryArtifact = Helper.ArtifactStore.GetGlossaryArtifact(_user, publishedGlossaryArtifact.Id, versionId: versionId), "GetGlossaryArtifact call with invalid versionId does not exit with 404 NotFoundException!");

            var serviceErrorMessage = Deserialization.DeserializeObject<ServiceErrorMessage>(ex.RestResponse.Content);

            // Validation: Exception should contain proper errorCode in the response content.
            Assert.That(serviceErrorMessage.ErrorCode.Equals(InternalApiErrorCodes.ItemNotFound), "GetGlossaryArtifact with invalid versionId should return {0} errorCode but {1} is returned", ErrorCodes.ResourceNotFound, serviceErrorMessage.ErrorCode);
        }

        #endregion 404 Not Found Tests

        #region Private Functions

        #endregion Private Functions
    }
}
