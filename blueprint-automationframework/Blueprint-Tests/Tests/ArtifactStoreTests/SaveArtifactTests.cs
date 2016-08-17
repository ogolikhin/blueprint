﻿using System.Collections.Generic;
using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.Impl;
using Newtonsoft.Json;
using NUnit.Framework;
using TestCommon;
using Utilities;
using Utilities.Facades;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    [Explicit(IgnoreReasons.UnderDevelopment)]
    public class SaveArtifactTests : TestBase
    {
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

        #region SaveArtifact tests

        [Explicit(IgnoreReasons.UnderDevelopment)]  // POST (Save) functionality isn't implemented yet, only PATCH.
        [TestCase(BaseArtifactType.Actor)]
        [TestCase(BaseArtifactType.BusinessProcess)]
        [TestCase(BaseArtifactType.Document)]
        [TestCase(BaseArtifactType.DomainDiagram)]
        [TestCase(BaseArtifactType.GenericDiagram)]
        [TestCase(BaseArtifactType.Glossary)]
        [TestCase(BaseArtifactType.PrimitiveFolder)]
        [TestCase(BaseArtifactType.Process)]
        [TestCase(BaseArtifactType.Storyboard)]
        [TestCase(BaseArtifactType.TextualRequirement)]
        [TestCase(BaseArtifactType.UIMockup)]
        [TestCase(BaseArtifactType.UseCase)]
        [TestCase(BaseArtifactType.UseCaseDiagram)]
        [TestRail(154745)]
        [Description("Create & save an artifact.  Get the artifact.  Verify the artifact returned has the same properties as the artifact we saved.")]
        public void SaveArtifact_ValidArtifact_CanGetArtifact(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateArtifact(_project, _user, artifactType);

            // Execute:
            Assert.DoesNotThrow(() => artifact.Save(),
                "Exception caught while trying to save an artifact of type: '{0}'!", artifactType);

            // Verify:
            IOpenApiArtifact openApiArtifact = OpenApiArtifact.GetArtifact(Helper.BlueprintServer.Address, _project, artifact.Id, _user);
            TestHelper.AssertArtifactsAreEqual(artifact, openApiArtifact);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]  // POST (Save) functionality isn't implemented yet, only PATCH.
        [TestCase]
        [TestRail(154746)]
        [Description("Create & save an artifact but don't send a 'Session-Token' header in the request.  Verify 400 Bad Request is returned.")]
        public void SaveArtifact_NoTokenHeader_400BadRequest()
        {
            // Setup:
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Process);
            IUser userWithNoToken = Helper.CreateUserAndAddToDatabase();

            // Execute & Verify:
            Assert.Throws<Http400BadRequestException>(() => artifact.Save(userWithNoToken),
                "'POST {0}' should return 400 Bad Request if no Session-Token header is passed!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]  // POST (Save) functionality isn't implemented yet, only PATCH.
        [TestCase]
        [TestRail(154747)]
        [Description("Create & save an artifact but pass an unauthorized token.  Verify 401 Unauthorized is returned.")]
        public void SaveArtifact_UnauthorizedToken_401Unauthorized()
        {
            // Setup:
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Process);
            IUser userWithBadToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            // Execute & Verify:
            Assert.Throws<Http401UnauthorizedException>(() => artifact.Save(userWithBadToken),
                "'POST {0}' should return 401 Unauthorized if an invalid token is passed!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]  // POST (Save) functionality isn't implemented yet, only PATCH.
        [TestCase]
        [TestRail(154748)]
        [Description("Create & save an artifact as a user that doesn't have permission to add artifacts to the project.  Verify 403 Forbidden is returned.")]
        public void SaveArtifact_UserWithoutPermissions_403Forbidden()
        {
            // Setup:
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Process);
            IUser userWithoutPermission = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken,
                InstanceAdminRole.BlueprintAnalytics);

            // Execute & Verify:
            Assert.Throws<Http403ForbiddenException>(() => artifact.Save(userWithoutPermission),
                "'POST {0}' should return 403 Forbidden if the user doesn't have permission to add artifacts!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]  // POST (Save) functionality isn't implemented yet, only PATCH.
        [TestCase(0)]
        [TestCase(int.MaxValue)]
        [TestRail(154749)]
        [Description("Create & save an artifact with a non-existent Project ID.  Verify 404 Not Found is returned.")]
        public void SaveArtifact_NonExistentProjectId_404NotFound(int projectId)
        {
            // Setup:
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Process);

            // Replace ProjectId with a fake ID that shouldn't exist.
            artifact.ProjectId = projectId;

            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() => artifact.Save(),
                "'POST {0}' should return 404 Not Found if the Project ID doesn't exist!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);
        }

        #endregion SaveArtifact tests

        #region UpdateArtifact tests

        [TestCase(BaseArtifactType.Actor)]
        [TestCase(BaseArtifactType.BusinessProcess)]
        [TestCase(BaseArtifactType.Document)]
        [TestCase(BaseArtifactType.DomainDiagram)]
        [TestCase(BaseArtifactType.GenericDiagram)]
        [TestCase(BaseArtifactType.Glossary)]
        [TestCase(BaseArtifactType.PrimitiveFolder)]
        [TestCase(BaseArtifactType.Process)]
        [TestCase(BaseArtifactType.Storyboard)]
        [TestCase(BaseArtifactType.TextualRequirement)]
        [TestCase(BaseArtifactType.UIMockup)]
        [TestCase(BaseArtifactType.UseCase)]
        [TestCase(BaseArtifactType.UseCaseDiagram)]
        [TestRail(156656)]
        [Description("Create & save an artifact.  Update the artifact.  Get the artifact.  Verify the artifact returned has the same properties as the artifact we updated.")]
        public void UpdateArtifact_ValidArtifact_CanGetArtifact(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _user, artifactType);

            // Execute:
            Assert.DoesNotThrow(() => artifact.Save(),
                "Exception caught while trying to update an artifact of type: '{0}'!", artifactType);

            // Verify:
            IOpenApiArtifact openApiArtifact = OpenApiArtifact.GetArtifact(Helper.BlueprintServer.Address, _project, artifact.Id, _user);
            TestHelper.AssertArtifactsAreEqual(artifact, openApiArtifact);
        }

        [TestCase]
        [TestRail(156662)]
        [Description("Try to update an artifact, but send an empty request body.  Verify 400 Bad Request is returned.")]
        public void UpdateArtifact_EmptyBody_400BadRequest()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Process);
            const string requestBody = null;

            // Execute & Verify:
            Assert.Throws<Http400BadRequestException>(() =>
            {
                UpdateInvalidArtifact(requestBody, artifact.Id, _user);
            }, "'PATCH {0}' should return 400 Bad Request if an empty body is sent!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);
        }

        [TestCase]
        [TestRail(156663)]
        [Description("Try to update an artifact, but send a corrupt JSON request body.  Verify 400 Bad Request is returned.")]
        public void UpdateArtifact_CorruptBody_400BadRequest()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Process);
            string requestBody = JsonConvert.SerializeObject(artifact);
            requestBody = requestBody.Remove(0, 5);     // Remove first 5 characters to corrupt the JSON string.

            // Execute & Verify:
            Assert.Throws<Http400BadRequestException>(() =>
            {
                UpdateInvalidArtifact(requestBody, artifact.Id, _user);
            }, "'PATCH {0}' should return 400 Bad Request if a corrupt JSON body is sent!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);
        }

        [TestCase]
        [TestRail(156664)]
        [Description("Try to update an artifact, but send a different Artifact ID in the URL vs request body.  Verify 400 Bad Request is returned.")]
        public void UpdateArtifact_DifferentArtifactIdsInUrlAndBody_400BadRequest()
        {
            // Setup:
            IArtifact artifact1 = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Process);
            IArtifact artifact2 = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Process);

            string requestBody = JsonConvert.SerializeObject(artifact1);
            int wrongArtifactId = artifact2.Id;

            // Execute & Verify:
            Assert.Throws<Http400BadRequestException>(() =>
            {
                UpdateInvalidArtifact(requestBody, wrongArtifactId, _user);
            }, "'PATCH {0}' should return 400 Bad Request if the Artifact ID in the URL is different than in the body!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);
        }

        [TestCase]
        [TestRail(156657)]
        [Description("Create & save an artifact.  Try to update the artifact but don't send a 'Session-Token' header in the request.  Verify 400 Bad Request is returned.")]
        public void UpdateArtifact_NoTokenHeader_401Unauthorized()
        {
            // Setup:
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Process);
            IUser userWithNoToken = Helper.CreateUserAndAddToDatabase();

            // Execute & Verify:
            Assert.Throws<Http401UnauthorizedException>(() => artifact.Save(userWithNoToken),
                "'PATCH {0}' should return 401 Unauthorized if no Session-Token header is passed!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);
        }

        [TestCase]
        [TestRail(156658)]
        [Description("Create & save an artifact.  Try to update the artifact but pass an unauthorized token.  Verify 401 Unauthorized is returned.")]
        public void UpdateArtifact_UnauthorizedToken_401Unauthorized()
        {
            // Setup:
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Process);
            IUser userWithBadToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            // Execute & Verify:
            Assert.Throws<Http401UnauthorizedException>(() => artifact.Save(userWithBadToken),
                "'PATCH {0}' should return 401 Unauthorized if an invalid token is passed!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);
        }

        [TestCase]
        [TestRail(156659)]
        [Description("Create & publish an artifact.  Try to update the artifact as a user that doesn't have permission to update artifacts in the project.  Verify 403 Forbidden is returned.")]
        public void UpdateArtifact_UserWithoutPermissions_403Forbidden()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);
            IUser userWithoutPermission = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken,
                InstanceAdminRole.BlueprintAnalytics);

            // Execute & Verify:
            Assert.Throws<Http403ForbiddenException>(() => artifact.Save(userWithoutPermission),
                "'PATCH {0}' should return 403 Forbidden if the user doesn't have permission to update artifacts!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);
        }

        [TestCase(0)]
        [TestCase(int.MaxValue)]
        [TestRail(156660)]
        [Description("Try to update an artifact with a non-existent Artifact ID.  Verify 404 Not Found is returned.")]
        public void UpdateArtifact_NonExistentArtifactId_404NotFound(int nonExistentArtifactId)
        {
            // Setup:
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Process);

            // Replace ProjectId with a fake ID that shouldn't exist.
            artifact.Id = nonExistentArtifactId;

            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() => artifact.Save(),
                "'PATCH {0}' should return 404 Not Found if the Artifact ID doesn't exist!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);
        }

        [TestRail(156661)]
        [Description("Create & publish an artifact, then delete & publish it.  Try to update the deleted artifact.  Verify 404 Not Found is returned.")]
        public void UpdateArtifact_DeletedArtifact_404NotFound()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);
            artifact.Delete();
            artifact.Publish();

            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() => artifact.Save(),
                "'PATCH {0}' should return 404 Not Found if the artifact was deleted!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);
        }

        // TODO: See if we can test any of the following 409 Conflict cases:

        /*
        409 Conflict
        - (This condition will be removed later, see US 1991) a property value does not match constraints, e.g. min and max values, valid values, required etc, or incorrect.
             - Text - required.
             - Number - required, min, max.
             - Date -  required, min, max.
             - Choice - required, against valid values, not allow multiple choices.
             - User - required, user or group exists.
        - A property is read-only over the reuse.
        - The artifact is not locked by the current user.
        - The version of the artifact in the input NovaArtifact version does not match the current version of the artifact.
        */

        #endregion UpdateArtifact tests

        #region Private functions

        /// <summary>
        /// Try to save a single invalid artifact to ArtifactStore.  Use this for testing cases where the save is expected to fail.
        /// </summary>
        /// <param name="requestBody">The request body (i.e. artifact to be saved).</param>
        /// <param name="artifactId">The ID of the artifact to save.</param>
        /// <param name="user">The user saving the artifact.</param>
        /// <returns>The ArtifactResult returned from ArtifactStore.</returns>
        public ArtifactResult SaveInvalidArtifact(string requestBody,
            int artifactId,
            IUser user)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            string tokenValue = user.Token?.AccessControlToken;

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.ARTIFACTS_id_, artifactId);
            RestApiFacade restApi = new RestApiFacade(Helper.BlueprintServer.Address, tokenValue);

            var response = restApi.SendRequestBodyAndGetResponse(
                path,
                RestRequestMethod.POST,
                requestBody);

            var artifactResult = JsonConvert.DeserializeObject<ArtifactResult>(response.Content);

            return artifactResult;
        }

        /// <summary>
        /// Try to update an invalid Artifact with Property Changes.  Use this for testing cases where the save is expected to fail.
        /// </summary>
        /// <param name="requestBody">The request body (i.e. artifact to be updated).</param>
        /// <param name="artifactId">The ID of the artifact to save.</param>
        /// <param name="user">The user updating the artifact.</param>
        /// <returns>The list of ArtifactResults returned from ArtifactStore.</returns>
        public List<ArtifactResult> UpdateInvalidArtifact(string requestBody,
            int artifactId,
            IUser user)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            string tokenValue = user.Token?.AccessControlToken;

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.ARTIFACTS_id_, artifactId);
            RestApiFacade restApi = new RestApiFacade(Helper.BlueprintServer.Address, tokenValue);

            var response = restApi.SendRequestBodyAndGetResponse(
                path,
                RestRequestMethod.PATCH,
                requestBody);

            var updateResultList = JsonConvert.DeserializeObject<List<ArtifactResult>>(response.Content);

            return updateResultList;
        }

        #endregion Private functions
    }
}
