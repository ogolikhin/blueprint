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

        [TestCase(4)]
        [TestRail(183353)]
        [Description("Create & publish a glossary artifact multiple times to have multiple version of it, Get glossary artifact without version. Verify that latest version of artifact is returned.")]
        public void GetGlossaryArtifact_PublishAndGetGlossaryArtifactWithoutSpecificVersion_ReturnsLatestVersionOfGlossaryArtifact(int numberOfVersions)
        {
            // Setup: Create and publish a glossary artifact multiple times to have multiple versions of it
            var publishedGlossaryArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Glossary, numberOfVersions: numberOfVersions);
            // getting the latest version of the artifact using open API GetArtifact
            var retrievedArtifact = Helper.ArtifactStore.GetArtifactDetails(_user, publishedGlossaryArtifact.Id);
            var viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);

            // Execute: Get the glossary artifact using GetGlossaryArtifact without versionId parameter
            NovaGlossaryArtifact glossaryArtifact = null;
            Assert.DoesNotThrow(() => { glossaryArtifact = Helper.ArtifactStore.GetGlossaryArtifact(viewer, publishedGlossaryArtifact.Id); }, 
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", RestPaths.Svc.ArtifactStore.GLOSSARY_id_);

            // Validation: Verify that the returned from GetGlossaryArtifact in valid format
            NovaArtifactDetails.AssertArtifactsEqual(glossaryArtifact, retrievedArtifact);
        }

        [TestCase]
        [TestRail(183356)]
        [Description("Create & publish a glossary artifact, modify & publish it again, GetGlossaryArtifact with versionId=1. Verify that first version of glossary artifact is returned.")]
        public void GetGlossaryArtifact_PublishAndGetGlossaryArtifactWithVersion1_ReturnsFirstVersionOfGlossaryArtifact()
        {
            // Setup: Create and publish a glossary artifact two times to have two versions of it			
            var publishedGlossaryArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Glossary, numberOfVersions: 2);
            var retrievedArtifactVersion1 = Helper.ArtifactStore.GetArtifactDetails(_user, publishedGlossaryArtifact.Id, versionId: 1);
            var viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);

            // Execute: Get the glossary artifact using GetGlossaryArtifact with first versionId			
            NovaGlossaryArtifact glossaryArtifact = null;
            Assert.DoesNotThrow(() => { glossaryArtifact = Helper.ArtifactStore.GetGlossaryArtifact(viewer, publishedGlossaryArtifact.Id, versionId: 1); },
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", RestPaths.Svc.ArtifactStore.GLOSSARY_id_);

            NovaArtifactDetails.AssertArtifactsEqual(glossaryArtifact, retrievedArtifactVersion1);
        }

        #endregion 200 OK Tests

        #region 401 Unauthorized Tests

        [TestCase("")]
        [TestCase("invalidTokenString")]
        [TestRail(183034)]
        [Description("Create & publish a glossary artifact, Get GlossaryArtifact with invalid token header. Verify 401 Unauthorized.")]
        public void GetGlossaryArtifact_PublishAndGetGlossaryArtifactWithInvalidTokenHeader_401Unauthorized(string token)
        {
            // Setup: Create and publish a glossary artifact
            var publishedGlossaryArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Glossary);
            var userWithBadOrMissingToken = UserFactory.CreateUserAndAddToDatabase();
            userWithBadOrMissingToken.Token.SetToken(token);

            // Execute: Get the glossary artifact with invalid token header using GetGlossaryArtifact
            Assert.Throws<Http401UnauthorizedException>(() => Helper.ArtifactStore.GetGlossaryArtifact(userWithBadOrMissingToken, publishedGlossaryArtifact.Id, versionId: 1),
                "Calling GET {0} with invalid token should return 401 Unauthorized!", RestPaths.Svc.ArtifactStore.GLOSSARY_id_);
        }

        #endregion 401 Unauthorized Tests

        #region 403 Forbidden Tests

        [TestCase]
        [TestRail(195424)]
        [Description("Create & publish a glossary artifact, Get GlossaryArtifact with the user with no permission to the artifact. Verify that 403 forbidden exception is returned.")]
        public void GetGlossaryArtifact_PublishAndGetGlossaryArtifactWithNoPermissionForTheArtifact_403Forbidden()
        {
            // Setup: Create and publish a glossary artifact
            var publishedGlossaryArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Glossary);
            var userWithNonePermissionForArtifact = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);
            Helper.AssignProjectRolePermissionsToUser(userWithNonePermissionForArtifact, TestHelper.ProjectRole.None, _project, publishedGlossaryArtifact);

            // Execute: Get the glossary artifact with the user with no permission to the artifact
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.GetGlossaryArtifact(userWithNonePermissionForArtifact, publishedGlossaryArtifact.Id),
                "Calling GET {0} with the user with the user which has no permission to the artifact shuold return 403 Forbidden!",
                RestPaths.Svc.ArtifactStore.GLOSSARY_id_);

            // Validation: Exception should contain proper errorCode in the response content
            var serviceErrorMessage = SerializationUtilities.DeserializeObject<ServiceErrorMessage>(ex.RestResponse.Content);
            Assert.AreEqual(InternalApiErrorCodes.Forbidden, serviceErrorMessage.ErrorCode,
                "Error code for GetUseCaseArtifact with the user which has no permission to the artifact should be {0}", InternalApiErrorCodes.Forbidden);
        }

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
            var viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);

            // Execute: Get the glossary artifact with invalid versionId using GetGlossaryArtifact
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetGlossaryArtifact(viewer, publishedGlossaryArtifact.Id, versionId: versionId), "GetGlossaryArtifact call with invalid versionId does not exit with 404 NotFoundException!");

            var serviceErrorMessage = SerializationUtilities.DeserializeObject<ServiceErrorMessage>(ex.RestResponse.Content);

            // Validation: Exception should contain proper errorCode in the response content
            Assert.AreEqual(InternalApiErrorCodes.ItemNotFound, serviceErrorMessage.ErrorCode, "Error code for GetGlossaryArtifact with invalid versionId should be {0}", InternalApiErrorCodes.ItemNotFound);
        }

        #endregion 404 Not Found Tests
    }
}
