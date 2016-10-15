﻿using System.Collections.Generic;
using System.Net;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.Impl;
using Newtonsoft.Json;
using NUnit.Framework;
using TestCommon;
using Utilities;
using Utilities.Facades;
using Utilities.Factories;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class CreateArtifactTests : TestBase
    {
        private const string SVC_PATH = RestPaths.Svc.ArtifactStore.Artifacts.CREATE;

        private IUser _user = null;
        private IProject _project = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_user);
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #region 200 OK tests

        [TestCase(ArtifactTypePredefined.Actor)]
        [TestCase(ArtifactTypePredefined.BusinessProcess)]
        [TestCase(ArtifactTypePredefined.Document)]
        [TestCase(ArtifactTypePredefined.DomainDiagram)]
        [TestCase(ArtifactTypePredefined.GenericDiagram)]
        [TestCase(ArtifactTypePredefined.Glossary)]
        [TestCase(ArtifactTypePredefined.PrimitiveFolder)]
        [TestCase(ArtifactTypePredefined.Process)]
        [TestCase(ArtifactTypePredefined.Storyboard)]
        [TestCase(ArtifactTypePredefined.TextualRequirement)]
        [TestCase(ArtifactTypePredefined.UIMockup)]
        [TestCase(ArtifactTypePredefined.UseCase)]
        [TestCase(ArtifactTypePredefined.UseCaseDiagram)]
        [TestRail(154745)]
        [Description("Create an artifact of a supported type in the project root.  Get the artifact.  " +
            "Verify the artifact returned has the same properties as the artifact we created.")]
        public void CreateArtifact_ValidArtifactTypeUnderProject_CanGetArtifact(ItemTypePredefined artifactType)
        {
            // Execute:
            INovaArtifactDetails newArtifact = null;

            Assert.DoesNotThrow(() =>
                newArtifact = CreateArtifactWithRandomName(artifactType, _user, _project),
                "'POST {0}' should return 200 OK when trying to create an artifact of type: '{1}'!",
                SVC_PATH, artifactType);

            // Verify:
            Assert.NotNull(newArtifact, "'POST {0}' returned null for an artifact of type: {1}!", SVC_PATH, artifactType);
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, newArtifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, artifactDetails);
        }

        [TestCase(ArtifactTypePredefined.Actor)]
        [TestCase(ArtifactTypePredefined.BusinessProcess)]
        [TestCase(ArtifactTypePredefined.Document)]
        [TestCase(ArtifactTypePredefined.DomainDiagram)]
        [TestCase(ArtifactTypePredefined.GenericDiagram)]
        [TestCase(ArtifactTypePredefined.Glossary)]
        [TestCase(ArtifactTypePredefined.PrimitiveFolder)]
        [TestCase(ArtifactTypePredefined.Process)]
        [TestCase(ArtifactTypePredefined.Storyboard)]
        [TestCase(ArtifactTypePredefined.TextualRequirement)]
        [TestCase(ArtifactTypePredefined.UIMockup)]
        [TestCase(ArtifactTypePredefined.UseCase)]
        [TestCase(ArtifactTypePredefined.UseCaseDiagram)]
        [TestRail(182496)]
        [Description("Create an artifact of a supported type under a folder.  Get the artifact.  " +
            "Verify the artifact returned has the same properties as the artifact we created.")]
        public void CreateArtifact_ValidArtifactTypeUnderFolder_CanGetArtifact(ItemTypePredefined artifactType)
        {
            // Setup:
            INovaArtifactDetails parentFolder = CreateArtifactWithRandomName(ItemTypePredefined.PrimitiveFolder, _user, _project);

            // Execute:
            INovaArtifactDetails newArtifact = null;

            Assert.DoesNotThrow(() =>
                newArtifact = CreateArtifactWithRandomName(artifactType, _user, _project, parentFolder),
                "'POST {0}' should return 200 OK when trying to create an artifact of type: '{1}'!",
                SVC_PATH, artifactType);

            // Verify:
            Assert.NotNull(newArtifact, "'POST {0}' returned null for an artifact of type: {1}!", SVC_PATH, artifactType);
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, newArtifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, newArtifact);
        }

        [TestCase(BaselineAndCollectionTypePredefined.ArtifactCollection)]
        [TestCase(BaselineAndCollectionTypePredefined.CollectionFolder)]
        [TestRail(182594)]
        [Description("Create an artifact of a supported type under a folder.  Get the artifact.  " +
            "Verify the artifact returned has the same properties as the artifact we created.")]
        public void CreateArtifact_ValidCollectionOrCollectionFolder_CanGetArtifact(ItemTypePredefined artifactType)
        {
            // Setup:
            var collectionFolder = GetDefaultCollectionFolder(_project, _user);

            // Execute:
            INovaArtifactDetails newArtifact = null;

            Assert.DoesNotThrow(() =>
                newArtifact = CreateArtifactWithRandomName(artifactType, _user, _project, collectionFolder, baseType: BaseArtifactType.PrimitiveFolder),
                "'POST {0}' should return 200 OK when trying to create an artifact of type: '{1}'!",
                SVC_PATH, artifactType);

            // Verify:
            Assert.NotNull(newArtifact, "'POST {0}' returned null for an artifact of type: {1}!", SVC_PATH, artifactType);
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, newArtifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, newArtifact);
        }

        [TestCase(BaselineAndCollectionTypePredefined.ArtifactCollection)]
        [TestCase(BaselineAndCollectionTypePredefined.CollectionFolder)]
        [TestRail(182595)]
        [Description("Create an artifact of a supported type under a folder.  Get the artifact.  " +
            "Verify the artifact returned has the same properties as the artifact we created.")]
        public void CreateArtifact_ValidCollectionOrCollectionFolderUnderCollectionFolder_CanGetArtifact(ItemTypePredefined artifactType)
        {
            // Setup:
            BaseArtifactType dummyType = BaseArtifactType.PrimitiveFolder;  // Need to pass something that OpenApi recognizes for the WrapNovaArtifact() call.
            var collectionFolder = GetDefaultCollectionFolder(_project, _user);
            var parentCollectionsFolder = CreateArtifactWithRandomName(ItemTypePredefined.CollectionFolder, _user, _project, collectionFolder, baseType: dummyType);

            // Execute:
            INovaArtifactDetails newArtifact = null;

            Assert.DoesNotThrow(() =>
                newArtifact = CreateArtifactWithRandomName(artifactType, _user, _project, parentCollectionsFolder, baseType: dummyType),
                "'POST {0}' should return 200 OK when trying to create an artifact of type: '{1}'!",
                SVC_PATH, artifactType);

            // Verify:
            Assert.NotNull(newArtifact, "'POST {0}' returned null for an artifact of type: {1}!", SVC_PATH, artifactType);
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, newArtifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, newArtifact);
        }

        [TestCase(ArtifactTypePredefined.Actor, -1)]
        [TestCase(ArtifactTypePredefined.Glossary, 0)]
        [TestCase(ArtifactTypePredefined.Process, 1)]
        [TestCase(ArtifactTypePredefined.UseCase, 1.5)]
        [TestRail(183502)]
        [Description("Create an artifact of a supported type in the project root.  Then create another artifact with OrderIndex before, equal or after the first artifact.  " +
            "Get the artifact.  Verify the artifact returned has the same properties as the artifact we created.")]
        public void CreateArtifact_ValidArtifactTypeUnderProjectWithOrderIndex_VerifyOrderIndexIsCorrect(ItemTypePredefined artifactType, double orderIndexOffset)
        {
            // Setup:
            var firstArtifact = CreateArtifactWithRandomName(artifactType, _user, _project);

            Assert.NotNull(firstArtifact.OrderIndex, "OrderIndex of newly created artifact must not be null!");
            Assert.Greater(firstArtifact.OrderIndex, 0, "OrderIndex of newly created artifact must be > 0!");

            double orderIndexToSet = firstArtifact.OrderIndex.Value + orderIndexOffset;

            // Execute:
            INovaArtifactDetails newArtifact = null;

            Assert.DoesNotThrow(() =>
                newArtifact = CreateArtifactWithRandomName(artifactType, _user, _project, orderIndex: orderIndexToSet),
                "'POST {0}' should return 200 OK when trying to create an artifact of type: '{1}'!",
                SVC_PATH, artifactType);

            // Verify:
            Assert.NotNull(newArtifact, "'POST {0}' returned null for an artifact of type: {1}!", SVC_PATH, artifactType);
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, newArtifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, artifactDetails);

            Assert.NotNull(newArtifact.OrderIndex, "OrderIndex of newly created artifact must not be null!");
            Assert.AreEqual(orderIndexToSet, newArtifact.OrderIndex.Value, "The OrderIndex of the new artifact is not correct!");
        }

        #endregion 200 OK tests

        #region Negative tests

        [TestCase(-1)]
        [TestCase(0)]
        [TestRail(183013)]
        [Description("Create an artifact with an invalid Project ID.  Verify 400 Bad Request is returned.")]
        public void CreateArtifact_InvalidProjectId_400BadRequest(int projectId)
        {
            // Setup:
            // Create a Project with a fake ID that shouldn't exist.
            IProject fakeProject = ProjectFactory.CreateProject(id: projectId);
            string artifactName = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);

            // Execute & Verify:
            Assert.Throws<Http400BadRequestException>(() => CreateArtifact(_user, fakeProject, (int)ItemTypePredefined.Process, artifactName),
                "'POST {0}' should return 400 Bad Request if an invalid Project ID was passed!", SVC_PATH);
        }

        [TestCase(true, true, true, false)]
        [TestCase(true, true, false, true)]
        [TestCase(true, false, true, true)]
        [TestCase(false, true, true, true)]
        [TestRail(183012)]
        [Description("Create an artifact with a missing required property.  Verify 400 Bad Request is returned.")]
        public void CreateArtifact_MissingRequiredProperty_400BadRequest(bool sendItemTypeId, bool sendName, bool sendProjectId, bool sendParentId)
        {
            // Setup:
            // Create a request with a missing required property.
            NovaArtifactDetails artifact = new NovaArtifactDetails
            {
                ItemTypeId = sendItemTypeId ? (int?)ItemTypePredefined.Process : null,
                Name = sendName ? RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10) : null,
                ProjectId = sendProjectId ? (int?)_project.Id : null,
                ParentId = sendParentId ? (int?)_project.Id : null,
            };

            string jsonBody = JsonConvert.SerializeObject(artifact);

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => CreateArtifactFromJson(_user, jsonBody),
                "'POST {0}' should return 400 Bad Request if a required property is missing!", SVC_PATH);

            // Verify:
            var expectedError = ServiceErrorMessageFactory.CreateServiceErrorMessage(103, "Invalid request.");
            var returnedError = JsonConvert.DeserializeObject<ServiceErrorMessage>(ex.RestResponse.Content);
            returnedError.AssertEquals(expectedError);
        }

        [Explicit(IgnoreReasons.ProductBug)]    // Trello bug: https://trello.com/c/oUNtprrI  Now returns 404 "Artifact type not found"
        [TestCase(ArtifactTypePredefined.Baseline)]
        [TestCase(ArtifactTypePredefined.DataElement)]
        [TestCase(ArtifactTypePredefined.Project)]
        [TestCase(BaselineAndCollectionTypePredefined.ArtifactBaseline)]
        [TestCase(BaselineAndCollectionTypePredefined.ArtifactReviewPackage)]
        [TestCase(BaselineAndCollectionTypePredefined.BaselineFolder)]
        [TestRail(182485)]
        [Description("Create an artifact of an unsupported type.  Verify 400 Bad Request is returned.")]
        public void CreateArtifact_UnsupportedArtifactType_400BadRequest(ItemTypePredefined artifactType)
        {
            // Setup:
            NovaArtifactDetails artifact = new NovaArtifactDetails
            {
                ItemTypeId = (int?)artifactType,
                Name = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10),
                ProjectId = _project.Id,
                ParentId = _project.Id,
            };

            string jsonBody = JsonConvert.SerializeObject(artifact);
            
            // Execute & Verify:
            var ex = Assert.Throws<Http400BadRequestException>(() => CreateArtifactFromJson(_user, jsonBody),
                "'POST {0}' should return 400 Bad Request when trying to create an unsupported artifact type of: '{1}'!",
                SVC_PATH, artifactType);

            const string expectedError = "TODO: fill this in when bug is fixed.";
            AssertRestResponseMessageIsCorrect(ex.RestResponse, expectedError);
        }

        [TestCase]
        [TestRail(154746)]
        [Description("Create an artifact but don't send a 'Session-Token' header in the request.  Verify 401 Unauthorized is returned.")]
        public void CreateArtifact_NoTokenHeader_401Unauthorized()
        {
            // Setup:
            IUser userWithNoToken = Helper.CreateUserAndAddToDatabase();

            // Execute & Verify:
            Assert.Throws<Http401UnauthorizedException>(() => CreateArtifactWithRandomName(ItemTypePredefined.Process, userWithNoToken, _project),
                "'POST {0}' should return 401 Unauthorized if no Session-Token header is passed!", SVC_PATH);
        }

        [TestCase]
        [TestRail(154747)]
        [Description("Create an artifact but pass an unauthorized token.  Verify 401 Unauthorized is returned.")]
        public void CreateArtifact_UnauthorizedToken_401Unauthorized()
        {
            // Setup:
            IUser userWithBadToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            // Execute & Verify:
            Assert.Throws<Http401UnauthorizedException>(() => CreateArtifactWithRandomName(ItemTypePredefined.Process, userWithBadToken, _project),
                "'POST {0}' should return 401 Unauthorized if an invalid token is passed!", SVC_PATH);
        }

        [Explicit(IgnoreReasons.ProductBug)]    // Trello bug: https://trello.com/c/xuw4vq9s  Now fails with 404: "Project not found."
        [TestCase]
        [TestRail(154748)]
        [Description("Create an artifact as a user that doesn't have permission to add artifacts to the project.  Verify 403 Forbidden is returned.")]
        public void CreateArtifact_UserWithoutPermissions_403Forbidden()
        {
            // Setup:
            IUser userWithoutPermission = Helper.CreateUserAndAuthenticate(
                TestHelper.AuthenticationTokenTypes.AccessControlToken,
                InstanceAdminRole.BlueprintAnalytics);

            string artifactName = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);

            // Execute & Verify:
            Assert.Throws<Http403ForbiddenException>(() => CreateArtifact(userWithoutPermission,
                _project, (int)ItemTypePredefined.Process, artifactName),
                "'POST {0}' should return 403 Forbidden if the user doesn't have permission to add artifacts!", SVC_PATH);
        }

        [TestCase(int.MaxValue)]
        [TestRail(154749)]
        [Description("Create an artifact with a non-existent Project ID.  Verify 404 Not Found is returned.")]
        public void CreateArtifact_NonExistentProjectId_404NotFound(int projectId)
        {
            // Setup:
            // Create a Project with a fake ID that shouldn't exist.
            IProject fakeProject = ProjectFactory.CreateProject(id: projectId);
            string artifactName = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);

            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() => CreateArtifact(_user, fakeProject, (int)ItemTypePredefined.Process, artifactName),
                "'POST {0}' should return 404 Not Found if the Project ID doesn't exist!", SVC_PATH);
        }

        [Explicit(IgnoreReasons.ProductBug)]    // Trello bug:  https://trello.com/c/zqgZbPQW
        [Category(Categories.CustomData)]       // NOTE: This won't work on Silver02 until we make a required property without a default value.
        [TestCase(ArtifactTypePredefined.Actor)]
        [TestRail(183536)]
        [Description("Create an artifact in the 'Custom Data' project for a type that has a required Custom Property with no default value.  " +
            "Verify the create fails with a 409 Conflict error.")]
        public void CreateArtifact_ArtifactWithMissingRequiredCustomProperty_409Conflict(ItemTypePredefined artifactType)
        {
            // Setup:
            IProject customDataProject = ProjectFactory.GetProject(_user, "Custom Data", shouldRetrievePropertyTypes: true);
            customDataProject.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => CreateArtifactWithRandomName(artifactType, _user, customDataProject),
                "'POST {0}' should return 409 Conflict when trying to create an artifact that has a required property without a default value!");

            // Verify:
            IServiceErrorMessage serviceError = JsonConvert.DeserializeObject<ServiceErrorMessage>(ex.RestResponse.Content);
            IServiceErrorMessage expectedError = ServiceErrorMessageFactory.CreateServiceErrorMessage(0,
                "TODO: Fill in when https://trello.com/c/zqgZbPQW is fixed.");

            serviceError.AssertEquals(expectedError);
        }

        // TODO: Create Collections in project.  Verify 400 error.
        // TODO: Send a corrupt JSON body.  Verify 400 Bad Request.
        // TODO: Create artifact with parent that user has no access to.  Verify 403.
        // TODO: Pass non-existent ItemTypeId.  Verify 404 Not Found.
        // TODO: Create artifact with non-existent parent.  Verify 404.
        // TODO: Create folder under non-folder artifact.  Verify 409.
        // TODO: Create an artifact with ProjectID x with a Parent that exists in project y.  Verify 409 Error.
        // TODO: Create non-Collections under default Collection folder.  Verify 409 error.
        // TODO: Create Collection under a Collection.  Verify 409 error.

        #endregion Negative tests

        #region Private functions

        /// <summary>
        /// Asserts that the specified RestResponse contains the expected error message.
        /// </summary>
        /// <param name="restReponse">The RestResponse that contains the message.</param>
        /// <param name="expectedMessage">The expected error message.</param>
        private static void AssertRestResponseMessageIsCorrect(RestResponse restReponse, string expectedMessage)
        {
            SaveArtifactResult result = JsonConvert.DeserializeObject<SaveArtifactResult>(restReponse.Content);

            Assert.AreEqual(expectedMessage, result.Message, "The wrong message was returned by 'POST {0}'.", SVC_PATH);
        }

        /// <summary>
        /// Creates a new artifact with a random name.
        /// </summary>
        /// <param name="artifactType">The type of artifact to create.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="project">The project where the artifact will be created.</param>
        /// <param name="parent">(optional) The parent of the artifact to be created.</param>
        /// <param name="orderIndex">(optional) The Order Index to assign to the new artifact.</param>
        /// <param name="baseType">(optional) You can select a different BaseArtifactType here other than what's in the novaArtifact.
        ///     Use this for artifact types that don't exist in the BaseArtifactType enum.</param>
        /// <returns>The artifact that was created.</returns>
        private INovaArtifactDetails CreateArtifactWithRandomName(ItemTypePredefined artifactType,
            IUser user,
            IProject project,
            INovaArtifactBase parent = null,
            double? orderIndex = null,
            BaseArtifactType? baseType = null)
        {
            string artifactName = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);
            var artifact = Helper.ArtifactStore.CreateArtifact(user, artifactType, artifactName, project, parent, orderIndex);

            WrapNovaArtifact(artifact, project, user, baseType);

            return artifact;
        }

        /// <summary>
        /// Tries to create an artifact (which could be invalid).
        /// </summary>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="project">The project where the artifact will be created.</param>
        /// <param name="itemTypeId">The ItemType ID of the artifact.</param>
        /// <param name="artifactName">The name of the artifact.</param>
        /// <param name="parentId">The parent ID of the artifact.</param>
        /// <param name="orderIndex">The order index of the artifact.</param>
        /// <param name="baseType">(optional) You can select a different BaseArtifactType here other than what's in the novaArtifact.
        ///     Use this for artifact types that don't exist in the BaseArtifactType enum.</param>
        /// <returns>The artifact that was created.</returns>
        private INovaArtifactDetails CreateArtifact(IUser user,
            IProject project,
            int itemTypeId,
            string artifactName = null,
            int? parentId = null,
            double? orderIndex = null,
            BaseArtifactType? baseType = null)
        {
            artifactName = artifactName ?? RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);

            NovaArtifactDetails artifactDetails = new NovaArtifactDetails
            {
                Name = artifactName,
                ProjectId = project.Id,
                ItemTypeId = itemTypeId,
                ParentId = parentId ?? project.Id,
                OrderIndex = orderIndex
            };
            
            string jsonBody = JsonConvert.SerializeObject(artifactDetails);

            RestResponse response = CreateArtifactFromJson(user, jsonBody);
            INovaArtifactDetails createdArtifact = JsonConvert.DeserializeObject<NovaArtifactDetails>(response.Content);

            WrapNovaArtifact(createdArtifact, project, user, baseType);

            return createdArtifact;
        }

        /// <summary>
        /// Tries to create an artifact based on the supplied JSON body.
        /// </summary>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="jsonBody">The JSON body of the request.</param>
        /// <returns>The REST response.</returns>
        private RestResponse CreateArtifactFromJson(IUser user, string jsonBody)
        {
            RestApiFacade restApi = new RestApiFacade(Helper.BlueprintServer.Address, user?.Token?.AccessControlToken);

            // Set expectedStatusCodes to 201 Created.
            List<HttpStatusCode> expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Created };
            const string contentType = "application/json";

            var response = restApi.SendRequestBodyAndGetResponse(
                SVC_PATH,
                RestRequestMethod.POST,
                jsonBody,
                contentType,
                expectedStatusCodes: expectedStatusCodes);

            return response;
        }

        /// <summary>
        /// Gets the default Collections folder for the project and returns only the Id, PredefinedType, ProjectId and ItemTypeId.
        /// </summary>
        /// <param name="project">The project whose collections folder you want to get.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <returns>The default Collections folder for the project.</returns>
        private INovaArtifactBase GetDefaultCollectionFolder(IProject project, IUser user)
        {
            INovaArtifact collectionFolder = project.GetDefaultCollectionFolder(Helper.ArtifactStore.Address, user);

            return new NovaArtifactDetails
            {
                Id = collectionFolder.Id,
                PredefinedType = collectionFolder.PredefinedType,
                ProjectId = project.Id,
                ItemTypeId = collectionFolder.ItemTypeId
            };
        }

        /// <summary>
        /// Wraps an INovaArtifactDetails in an IArtifactBase and adds it Helper.Artifacts so it gets disposed properly.
        /// </summary>
        /// <param name="novaArtifact">The INovaArtifactDetails that was created by ArtifactStore.</param>
        /// <param name="project">The project where this artifact exists.</param>
        /// <param name="user">The user that created this artifact.</param>
        /// <param name="baseType">(optional) You can select a different BaseArtifactType here other than what's in the novaArtifact.
        ///     Use this for artifact types that don't exist in the BaseArtifactType enum.</param>
        /// <returns>The IArtifactBase wrapper for the novaArtifact.</returns>
        private IArtifactBase WrapNovaArtifact(INovaArtifactDetails novaArtifact,
            IProject project,
            IUser user,
            BaseArtifactType? baseType = null)
        {
            ThrowIf.ArgumentNull(novaArtifact, nameof(novaArtifact));

            Assert.NotNull(novaArtifact.PredefinedType, "PredefinedType is null in the Nova Artifact!");

            if (baseType == null)
            {
                baseType = ((ItemTypePredefined) novaArtifact.PredefinedType.Value).ToBaseArtifactType();
            }

            IArtifactBase artifact = ArtifactFactory.CreateArtifact(project,
                user,
                baseType.Value,
                novaArtifact.Id);

            artifact.IsSaved = true;
            Helper.Artifacts.Add(artifact);

            return artifact;
        }
        
        #endregion Private functions
    }
}
