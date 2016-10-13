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
    public class LegacyUseCaseArtifactHistoryTests : TestBase
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
        [TestRail(183020)]
        [Description("Create & publish a use case artifact, Get UseCaseArtifact.  Verify that the latest version of valid use case artifact is returned.")]
        public void GetUseCaseArtifact_PublishAndGetUseCaseArtifact_ReturnsLatestVersionOfUseCaseArtifact()
        {
            // Setup: Create and publish a use case artifact
            var publishedUseCaseArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase);
            var retrievedArtifact = Helper.ArtifactStore.GetArtifactDetails(_user, publishedUseCaseArtifact.Id);
            NovaUseCaseArtifact usecaseArtifact = null;

            // Execute: Get the use case artifact using GetUseCaseArtifact
            Assert.DoesNotThrow(() => {
                usecaseArtifact = Helper.ArtifactStore.GetUseCaseArtifact(_user, publishedUseCaseArtifact.Id);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", RestPaths.Svc.ArtifactStore.USECASE_id_);

            // Validation: Verify that the returned from GetUseCaseArtifact is in valid format
            ArtifactStoreHelper.AssertArtifactsEqual(usecaseArtifact, retrievedArtifact);
        }

        [TestCase(4)]
        [TestRail(183354)]
        [Description("Create & publish a use case artifact multiple times to have multiple version of it, Get use case artifact without passing version. Verify that latest version of artifact is returned.")]
        public void GetUseCaseArtifact_PublishAndGetUseCaseArtifactWithoutSpecificVersion_ReturnsLatestVersionOfUseCaseArtifact(int numberOfVersions)
        {
            // Setup: Create and publish a use case artifact multiple times to have multiple versions of it
            var publishedUseCaseArtifact = Helper.CreateAndPublishOpenApiArtifact(_project, _user, BaseArtifactType.UseCase, numberOfVersions: numberOfVersions);
            // getting the latest version of the artifact using open API GetArtifact
            var retrievedArtifact = Helper.ArtifactStore.GetArtifactDetails(_user, publishedUseCaseArtifact.Id);
            NovaUseCaseArtifact usecaseArtifact = null;

            // Execute: Get the use case artifact using GetUseCaseArtifact without passing versionId parameter
            Assert.DoesNotThrow(() => {
                usecaseArtifact = Helper.ArtifactStore.GetUseCaseArtifact(_user, publishedUseCaseArtifact.Id);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", RestPaths.Svc.ArtifactStore.USECASE_id_);

            // Validation: Verify that the returned from GetUseCaseArtifact in valid format
            ArtifactStoreHelper.AssertArtifactsEqual(usecaseArtifact, retrievedArtifact);
        }

        [TestCase]
        [TestRail(183357)]
        [Description("Create & publish a use case artifact, modify & publish it again, GetUseCaseArtifact with versionId=1. Verify that first version of use case artifact is returned.")]
        public void GetUseCaseArtifact_PublishAndGetUseCaseArtifactWithVersion1_ReturnsFirstVersionOfUseCaseArtifact()
        {
            // Setup: Create and publish a use case artifact two times to have two versions of it			
            IArtifact publishedUseCaseArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase, numberOfVersions: 2);
            var retrievedArtifactVersion1 = Helper.ArtifactStore.GetArtifactDetails(_user, publishedUseCaseArtifact.Id, versionId: 1);
            NovaUseCaseArtifact usecaseArtifact = null;

            // Execute: Get the use case artifact using GetUseCaseArtifact with first versionId			
            Assert.DoesNotThrow(() => {
                usecaseArtifact = Helper.ArtifactStore.GetUseCaseArtifact(_user, publishedUseCaseArtifact.Id, versionId: 1);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", RestPaths.Svc.ArtifactStore.USECASE_id_);

            ArtifactStoreHelper.AssertArtifactsEqual(usecaseArtifact, retrievedArtifactVersion1);
        }

        #endregion 200 OK Tests

        #region 401 Unauthorized Tests

        [TestCase("")]
        [TestCase("invalidTokenString")]
        [TestRail(183035)]
        [Description("Create & publish a use case artifact, Get UseCaseArtifact with invalid token header.  Verify 401 Unauthorized.")]
        public void GetUseCaseArtifact_PublishAndGetUseCaseArtifactWithInvalidTokenHeader_401Unauthorized(string token)
        {
            // Setup: Create and publish a use case artifact
            var publishedUseCaseArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase);
            NovaUseCaseArtifact usecaseArtifact = null;

            IUser userWithBadOrMissingToken = UserFactory.CreateUserAndAddToDatabase();
            userWithBadOrMissingToken.Token.SetToken(token);

            // Execute: Get the use case artifact with invalid token header using GetUseCaseArtifact
            Assert.Throws<Http401UnauthorizedException>(() => usecaseArtifact = Helper.ArtifactStore.GetUseCaseArtifact(userWithBadOrMissingToken, publishedUseCaseArtifact.Id, versionId: 1), "Calling GET {0} with invalid token should return 401 Unauthorized!", RestPaths.Svc.ArtifactStore.USECASE_id_);
        }

        #endregion 401 Unauthorized Tests

        #region 403 Forbidden Tests
        #endregion 403 Forbidden Tests

        #region 404 Not Found Tests

        [TestCase(0)]
        [TestCase(-10)]
        [TestCase(999)]
        [TestRail(183029)]
        [Description("Create & publish a use case artifact, Get UseCaseArtifact with invalid versionId.  Verify 404 NotFound.")]
        public void GetUseCaseArtifact_PublishAndGetUseCaseArtifactWithInvalidVersionId_404NotFound(int versionId)
        {
            // Setup: Create and publish a use case artifact
            var publishedUseCaseArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase);
            NovaUseCaseArtifact usecaseArtifact = null;

            // Execute: Get the use case artifact with invalid versionId using GetUseCaseArtifact
            var ex = Assert.Throws<Http404NotFoundException>(() => usecaseArtifact = Helper.ArtifactStore.GetUseCaseArtifact(_user, publishedUseCaseArtifact.Id, versionId: versionId), "GetUseCaseArtifact call with invalid versionId does not exit with 404 NotFoundException!");

            var serviceErrorMessage = Deserialization.DeserializeObject<ServiceErrorMessage>(ex.RestResponse.Content);

            // Validation: Exception should contain proper errorCode in the response content.
            Assert.That(serviceErrorMessage.ErrorCode.Equals(InternalApiErrorCodes.ItemNotFound), "GetUseCaseArtifact with invalid versionId should return {0} errorCode but {1} is returned", ErrorCodes.ResourceNotFound, serviceErrorMessage.ErrorCode);
        }

        #endregion 404 Not Found Tests

        #region Private Functions

        #endregion Private Functions
    }
}
