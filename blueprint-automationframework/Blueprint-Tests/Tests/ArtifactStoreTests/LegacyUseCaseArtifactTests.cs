﻿using CustomAttributes;
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
    public class LegacyUseCaseArtifactTests : TestBase
    {
        private IUser _adminUser = null;
        private IUser _viewerUser = null;
        private IUser _authorUser = null;
        private IProject _project = null;
        private IProject _projectCustomData = null;

        private static int USECASE_ID_WITHUIMOCKUP = 147;
        private const string REST_PATH = RestPaths.Svc.ArtifactStore.USECASE_id_;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_adminUser);
            _viewerUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);

            _projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_adminUser);
            _projectCustomData.GetAllNovaArtifactTypes(Helper.ArtifactStore, _adminUser);
            _authorUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _projectCustomData);


        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region 200 OK Tests

        [TestCase(4)]
        [TestRail(183354)]
        [Description("Create & publish a use case artifact multiple times to have multiple version of it, Get use case artifact without passing version.  +" +
                     "Verify that latest version of artifact is returned.")]
        public void GetUseCaseArtifact_PublishAndGetUseCaseArtifactWithoutSpecificVersion_ReturnsLatestVersionOfUseCaseArtifact(int numberOfVersions)
        {
            // Setup: Create and publish a use case artifact multiple times to have multiple versions of it
            var publishedUseCaseArtifact = Helper.CreateAndPublishNovaArtifactWithMultipleVersions(_adminUser, _project, ItemTypePredefined.UseCase, numberOfVersions);
            // getting the latest version of the artifact using open API GetArtifact
            var retrievedArtifact = Helper.ArtifactStore.GetArtifactDetails(_adminUser, publishedUseCaseArtifact.Id);

            // Execute: Get the use case artifact using GetUseCaseArtifact without passing versionId parameter
            NovaUseCaseArtifact usecaseArtifact = null;
            Assert.DoesNotThrow(() => { usecaseArtifact = Helper.ArtifactStore.GetUseCaseArtifact(_viewerUser, publishedUseCaseArtifact.Id); },
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", REST_PATH);

            // Validation: Verify that the returned from GetUseCaseArtifact in valid format
            NovaArtifactDetails.AssertArtifactsEqual(usecaseArtifact, retrievedArtifact);
        }

        [TestCase]
        [TestRail(183357)]
        [Description("Create & publish a use case artifact, modify & publish it again, GetUseCaseArtifact with versionId=1.  " +
                     "Verify that first version of use case artifact is returned.")]
        public void GetUseCaseArtifact_PublishAndGetUseCaseArtifactWithVersion1_ReturnsFirstVersionOfUseCaseArtifact()
        {
            // Setup: Create and publish a use case artifact two times to have two versions of it			
            var publishedUseCaseArtifact = Helper.CreateAndPublishNovaArtifactWithMultipleVersions(_adminUser, _project, ItemTypePredefined.UseCase, numberOfVersions: 2);
            var retrievedArtifactVersion1 = Helper.ArtifactStore.GetArtifactDetails(_adminUser, publishedUseCaseArtifact.Id, versionId: 1);

            // Execute: Get the use case artifact using GetUseCaseArtifact with first versionId		
            NovaUseCaseArtifact usecaseArtifact = null;
            Assert.DoesNotThrow(() => { usecaseArtifact = Helper.ArtifactStore.GetUseCaseArtifact(_viewerUser, publishedUseCaseArtifact.Id, versionId: 1); },
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", REST_PATH);

            NovaArtifactDetails.AssertArtifactsEqual(usecaseArtifact, retrievedArtifactVersion1);
        }

        [Category(Categories.CustomData)]
        [Category(Categories.GoldenData)]
        [TestCase]
        [TestRail(267348)]
        [Description("Get the use case artifact which contains a UIMockup association on its postcondition subartifact. Verify that the indicator contains the value representing the UIMockup association.")]
        public void GetUseCaseArtifact_GetUseCaseArtifactWithUIMockupAssociation_VerifyIndicatorFlags()
        {
            // getting the latest version of the artifact using open API GetArtifact
            var retrievedArtifact = Helper.ArtifactStore.GetArtifactDetails(_adminUser, USECASE_ID_WITHUIMOCKUP);

            // Execution: Get the use case artifact with the UIMockup association on its postcondition subartifact
            NovaUseCaseArtifact usecaseArtifact = null;
            Assert.DoesNotThrow(() => { usecaseArtifact = Helper.ArtifactStore.GetUseCaseArtifact(_authorUser, USECASE_ID_WITHUIMOCKUP); },
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", REST_PATH);

            // Verify: Verify that the postcondition subartifact's indicatorflag contains value that represents UIMockup association.
            NovaArtifactDetails.AssertArtifactsEqual(usecaseArtifact, retrievedArtifact);

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, USECASE_ID_WITHUIMOCKUP, ItemIndicatorFlags.HasUIMockup, usecaseArtifact.PostCondition.Id);
        }

        #endregion 200 OK Tests

        #region 401 Unauthorized Tests

        [TestCase("")]
        [TestCase("invalidTokenString")]
        [TestRail(183035)]
        [Description("Create & publish a use case artifact, Get UseCaseArtifact with invalid token header. Verify 401 Unauthorized.")]
        public void GetUseCaseArtifact_PublishAndGetUseCaseArtifactWithInvalidTokenHeader_401Unauthorized(string token)
        {
            // Setup: Create and publish a use case artifact
            var publishedUseCaseArtifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UseCase);
            var userWithBadOrMissingToken = UserFactory.CreateUserAndAddToDatabase();
            userWithBadOrMissingToken.Token.SetToken(token);

            // Execute: Get the use case artifact with invalid token header using GetUseCaseArtifact
            Assert.Throws<Http401UnauthorizedException>(() => Helper.ArtifactStore.GetUseCaseArtifact(userWithBadOrMissingToken, publishedUseCaseArtifact.Id, versionId: 1),
                "Calling GET {0} with invalid token should return 401 Unauthorized!", REST_PATH);
        }

        #endregion 401 Unauthorized Tests

        #region 403 Forbidden Tests

        [TestCase]
        [TestRail(191202)]
        [Description("Create & publish a use case artifact, Get UseCaseArtifact with the user with no permission to the artifact. Verify that 403 forbidden exception is returned.")]
        public void GetUseCaseArtifact_PublishAndGetUseCaseArtifactWithNoPermissionForTheArtifact_403Forbidden()
        {
            // Setup: Create and publish a use case artifact
            var publishedUseCaseArtifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UseCase);
            var userWithNonePermissionForArtifact = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);
            Helper.AssignProjectRolePermissionsToUser(userWithNonePermissionForArtifact, TestHelper.ProjectRole.None, _project, publishedUseCaseArtifact);

            // Execute: Get the use case artifact with the user with no permission to the artifact
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.GetUseCaseArtifact(userWithNonePermissionForArtifact, publishedUseCaseArtifact.Id),
                "Calling GET {0} with the user with the user which has no permission to the artifact should return 403 Forbidden!",
                REST_PATH);

            // Vaidation: Exception should contain proper errorCode in the response content
            var serviceErrorMessage = SerializationUtilities.DeserializeObject<ServiceErrorMessage>(ex.RestResponse.Content);
            Assert.AreEqual(InternalApiErrorCodes.Forbidden, serviceErrorMessage.ErrorCode,
                "Error code for GetUseCaseArtifact with the user which has no permission to the artifact should be {0}",
                InternalApiErrorCodes.Forbidden);
        }

        #endregion 403 Forbidden Tests

        #region 404 Not Found Tests

        [TestCase(0)]
        [TestCase(-10)]
        [TestCase(999)]
        [TestRail(183029)]
        [Description("Create & publish a use case artifact, Get UseCaseArtifact with invalid versionId. Verify 404 NotFound.")]
        public void GetUseCaseArtifact_PublishAndGetUseCaseArtifactWithInvalidVersionId_404NotFound(int versionId)
        {
            // Setup: Create and publish a use case artifact
            var publishedUseCaseArtifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UseCase);

            // Execute: Get the use case artifact with invalid versionId using GetUseCaseArtifact
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetUseCaseArtifact(_viewerUser, publishedUseCaseArtifact.Id, versionId: versionId), "GetUseCaseArtifact call with invalid versionId does not exit with 404 NotFoundException!");

            // Validation: Exception should contain proper errorCode in the response content
            var serviceErrorMessage = SerializationUtilities.DeserializeObject<ServiceErrorMessage>(ex.RestResponse.Content);
            Assert.AreEqual(InternalApiErrorCodes.ItemNotFound, serviceErrorMessage.ErrorCode, "Error code for GetUseCaseArtifact with invalid versionId should be {0}", InternalApiErrorCodes.ItemNotFound);
        }

        #endregion 404 Not Found Tests
    }
}