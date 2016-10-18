using System;
using System.Collections.Generic;
using System.Net;
using Common;
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

        #region 201 Created tests

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
                "'POST {0}' should return 201 Created when trying to create an artifact of type: '{1}'!",
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
                newArtifact = CreateArtifactWithRandomName(artifactType, _user, _project, parentFolder.Id),
                "'POST {0}' should return 201 Created when trying to create an artifact of type: '{1}'!",
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
                newArtifact = CreateArtifactWithRandomName(artifactType, _user, _project, collectionFolder.Id, baseType: BaseArtifactType.PrimitiveFolder),
                "'POST {0}' should return 201 Created when trying to create an artifact of type: '{1}'!",
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
            var parentCollectionsFolder = CreateArtifactWithRandomName(
                ItemTypePredefined.CollectionFolder, _user, _project, collectionFolder.Id, baseType: dummyType);

            // Execute:
            INovaArtifactDetails newArtifact = null;

            Assert.DoesNotThrow(() =>
                newArtifact = CreateArtifactWithRandomName(artifactType, _user, _project, parentCollectionsFolder.Id, baseType: dummyType),
                "'POST {0}' should return 201 Created when trying to create an artifact of type: '{1}'!",
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
        [Description("Create an artifact of a supported type in the project root.  Then create another artifact with OrderIndex before, " +
            "equal or after the first artifact.  Get the artifact.  Verify the artifact returned has the same properties as the artifact we created.")]
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
                "'POST {0}' should return 201 Created when trying to create an artifact of type: '{1}'!",
                SVC_PATH, artifactType);

            // Verify:
            Assert.NotNull(newArtifact, "'POST {0}' returned null for an artifact of type: {1}!", SVC_PATH, artifactType);
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, newArtifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, artifactDetails);

            Assert.NotNull(newArtifact.OrderIndex, "OrderIndex of newly created artifact must not be null!");
            Assert.AreEqual(orderIndexToSet, newArtifact.OrderIndex.Value, "The OrderIndex of the new artifact is not correct!");
        }

        #endregion 201 Created tests

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
            fakeProject.NovaArtifactTypes.AddRange(_project.NovaArtifactTypes);
            string artifactName = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => CreateArtifact(_user, fakeProject, ItemTypePredefined.Process, artifactName),
                "'POST {0}' should return 400 Bad Request if an invalid Project ID was passed!", SVC_PATH);

            // Verify:
            ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.IncorrectInputParameters, "Project not found.");
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
            var artifact = CreateNovaArtifactDetails(
                itemTypeId: sendItemTypeId ? (int?)_project.GetItemTypeIdForPredefinedType(ItemTypePredefined.Process) : null,
                artifactName: sendName ? RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10) : null,
                projectId: sendProjectId ? (int?)_project.Id : null,
                parentId: sendParentId ? (int?)_project.Id : null);

            string jsonBody = JsonConvert.SerializeObject(artifact);

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => CreateArtifactFromJson(_user, jsonBody),
                "'POST {0}' should return 400 Bad Request if a required property is missing!", SVC_PATH);

            // Verify:
            ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.IncorrectInputParameters, "Invalid request.");
        }

        [TestCase(ArtifactTypePredefined.Actor)]
        [TestRail(183537)]
        [Description("Send a corrupt JSON body to the Create Artifact call.  Verify the create fails with a 400 Bad Request error.")]
        public void CreateArtifact_SendCorruptJson_400BadRequest(ItemTypePredefined artifactType)
        {
            // Setup:
            var artifact = CreateNovaArtifactDetails(RandomGenerator.RandomAlphaNumeric(10), _project.Id, (int)artifactType, _project.Id);
            string jsonBody = JsonConvert.SerializeObject(artifact);
            string corruptJsonBody = jsonBody.Remove(0, 2);

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => CreateArtifactFromJson(_user, corruptJsonBody),
                "'POST {0}' should return 400 Bad Request when a corrupt JSON body is sent!");

            // Verify:
            ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.IncorrectInputParameters, "An artifact is not defined.");
        }

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
                ItemTypeId = _project.GetItemTypeIdForPredefinedType(artifactType),
                Name = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10),
                ProjectId = _project.Id,
                ParentId = _project.Id,
            };

            string jsonBody = JsonConvert.SerializeObject(artifact);
            
            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => CreateArtifactFromJson(_user, jsonBody),
                "'POST {0}' should return 400 Bad Request when trying to create an unsupported artifact type of: '{1}'!",
                SVC_PATH, artifactType);

            // Verify:
            ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.IncorrectInputParameters, "Cannot create an artifact with the specified Artifact Type.");
        }

        [TestCase]
        [TestRail(154746)]
        [Description("Create an artifact but don't send a 'Session-Token' header in the request.  Verify 401 Unauthorized is returned.")]
        public void CreateArtifact_NoTokenHeader_401Unauthorized()
        {
            // Setup:
            IUser userWithNoToken = Helper.CreateUserAndAddToDatabase();

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() => CreateArtifactWithRandomName(ItemTypePredefined.Process, userWithNoToken, _project),
                "'POST {0}' should return 401 Unauthorized if no Session-Token header is passed!", SVC_PATH);

            // Verify:
            Assert.AreEqual("\"Unauthorized call\"", ex.RestResponse.Content);
        }

        [TestCase]
        [TestRail(154747)]
        [Description("Create an artifact but pass an unauthorized token.  Verify 401 Unauthorized is returned.")]
        public void CreateArtifact_UnauthorizedToken_401Unauthorized()
        {
            // Setup:
            IUser userWithBadToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() => CreateArtifactWithRandomName(ItemTypePredefined.Process, userWithBadToken, _project),
                "'POST {0}' should return 401 Unauthorized if an invalid token is passed!", SVC_PATH);

            // Verify:
            Assert.AreEqual("\"Unauthorized call\"", ex.RestResponse.Content);
        }

        [TestCase]
        [TestRail(154748)]
        [Description("Create an artifact as a user that doesn't have permission to add artifacts to the project.  Verify 403 Forbidden is returned.")]
        public void CreateArtifact_UserWithoutPermissionToProject_403Forbidden()
        {
            // Setup:
            IUser userWithoutPermission = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.None, _project);

            string artifactName = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => CreateArtifact(userWithoutPermission,
                _project, ItemTypePredefined.Process, artifactName),
                "'POST {0}' should return 403 Forbidden if the user doesn't have permission to add artifacts!", SVC_PATH);

            // Verify:
            ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "You do not have permission to perform this action.");
        }

        [Explicit(IgnoreReasons.ProductBug)]    // Trello bug: https://trello.com/c/uypOAMKF  It gets 201 instead of 403.
        [TestCase]
        [TestRail(183538)]
        [Description("Create an artifact as a user that full access to the project, but no access to the parent.  " +
            "Verify 403 Forbidden is returned.")]
        public void CreateArtifact_UserHasNoPermissionToParentArtifact_403Forbidden()
        {
            // Setup:
            var parentArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.PrimitiveFolder);

            // Create a user that has full access to project, but no access to parentArtifact.
            IUser userWithoutPermission = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
            Helper.AssignProjectRolePermissionsToUser(userWithoutPermission, TestHelper.ProjectRole.None, _project, parentArtifact);

            string artifactName = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);

            // Execute:
            var ex= Assert.Throws<Http403ForbiddenException>(() => CreateArtifact(userWithoutPermission,
                _project, ItemTypePredefined.Process, artifactName),
                "'POST {0}' should return 403 Forbidden if the user doesn't have permission to parent artifact!", SVC_PATH);

            // Verify:
            ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "You do not have permission to perform this action.");
        }

        [TestCase(int.MaxValue)]
        [TestRail(183539)]
        [Description("Create an artifact with a non-existent ItemType ID.  Verify 404 Not Found is returned.")]
        public void CreateArtifact_NonExistentItemTypeId_404NotFound(int itemTypeId)
        {
            // Setup:
            // Create a Project with a fake ID that shouldn't exist.
            string artifactName = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);
            var artifactDetails = CreateNovaArtifactDetails(artifactName, _project.Id, itemTypeId, _project.Id);
            string jsonBody = JsonConvert.SerializeObject(artifactDetails);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => CreateArtifactFromJson(_user, jsonBody),
                "'POST {0}' should return 404 Not Found if the ItemType ID doesn't exist!", SVC_PATH);

            // Verify:
            ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.NotFound, "Artifact type not found.");
        }

        [TestCase(int.MaxValue)]
        [TestRail(183540)]
        [Description("Create an artifact with a non-existent Parent ID.  Verify 404 Not Found is returned.")]
        public void CreateArtifact_NonExistentParentId_404NotFound(int parentId)
        {
            // Setup:
            // Create a Project with a fake ID that shouldn't exist.
            string artifactName = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => CreateArtifact(_user, _project, ItemTypePredefined.Process, artifactName, parentId),
                "'POST {0}' should return 404 Not Found if the Parent ID doesn't exist!", SVC_PATH);

            // Verify:
            ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound,
                "You have attempted to access an artifact that does not exist or has been deleted.");
        }

        [TestCase(int.MaxValue)]
        [TestRail(154749)]
        [Description("Create an artifact with a non-existent Project ID.  Verify 404 Not Found is returned.")]
        public void CreateArtifact_NonExistentProjectId_404NotFound(int projectId)
        {
            // Setup:
            // Create a Project with a fake ID that shouldn't exist.
            IProject fakeProject = ProjectFactory.CreateProject(id: projectId);
            fakeProject.NovaArtifactTypes.AddRange(_project.NovaArtifactTypes);
            string artifactName = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => CreateArtifact(_user, fakeProject, ItemTypePredefined.Process, artifactName),
                "'POST {0}' should return 404 Not Found if the Project ID doesn't exist!", SVC_PATH);

            // Verify:
            ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.NotFound, "Project not found.");
        }

        [Explicit(IgnoreReasons.ProductBug)]    // Trello bug: https://trello.com/c/rSl4L0zv  Gets a 404 instead of 409.
        [TestCase(ItemTypePredefined.Actor)]
        [TestRail(183543)]
        [Description("Create a regular artifact under the default Collections folder.  Verify the create fails with a 409 Conflict error.")]
        public void CreateArtifact_AddArtifactUnderCollectionsFolder_409Conflict(ItemTypePredefined artifactType)
        {
            // Setup:
            var collectionFolder = GetDefaultCollectionFolder(_project, _user);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => CreateArtifactWithRandomName(
                artifactType, _user, _project, collectionFolder.Id),
                "'POST {0}' should return 409 Conflict when creating a regular artifact under the Collections folder!");

            // Verify:
            ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotSaveConflictWithParent,
                "Invalid request.");
        }

        [TestCase]
        [TestRail(183544)]
        [Description("Create a Collection under another Collection.  Verify the create fails with a 409 Conflict error.")]
        public void CreateArtifact_AddCollectionUnderAnotherCollection_409Conflict()
        {
            // Setup:
            var collectionFolder = GetDefaultCollectionFolder(_project, _user);
            ItemTypePredefined artifactType = ItemTypePredefined.ArtifactCollection;
            BaseArtifactType dummyType = BaseArtifactType.PrimitiveFolder;  // Need to pass something that OpenApi recognizes for the WrapNovaArtifact() call.
            var parentCollection = CreateArtifactWithRandomName(artifactType, _user, _project, collectionFolder.Id, baseType: dummyType);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => CreateArtifactWithRandomName(
                artifactType, _user, _project, parentCollection.Id),
                "'POST {0}' should return 409 Conflict when creating a Collection under another Collectino!");

            // Verify:
            ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotSaveConflictWithParent,
                "Cannot create an artifact at this location.");
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(183541)]
        [Description("Create a folder under an artifact.  Verify the create fails with a 409 Conflict error.")]
        public void CreateArtifact_AddFolderUnderNonFolder_409Conflict(BaseArtifactType artifactType)
        {
            // Setup:
            var parentArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => CreateArtifactWithRandomName(
                ItemTypePredefined.PrimitiveFolder, _user, _project, parentArtifact.Id),
                "'POST {0}' should return 409 Conflict when trying to create a folder under a regular artifact!");

            // Verify:
            ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotSaveConflictWithParent,
                "Cannot create an artifact at this location.");
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
            ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ValidationFailed, "TODO: Fill in when https://trello.com/c/zqgZbPQW is fixed.");
        }

        [TestCase(ItemTypePredefined.Actor)]
        [TestRail(183542)]
        [Description("Create an artifact and specify a ProjectId that is different than the project of the parent.  " +
            "Verify the create fails with a 409 Conflict error.")]
        public void CreateArtifact_ParentExistsInADifferentProject_409Conflict(ItemTypePredefined artifactType)
        {
            // Setup:
            var projects = ProjectFactory.GetProjects(_user, numberOfProjects: 2);
            var parentArtifact = Helper.CreateAndPublishArtifact(projects[0], _user, BaseArtifactType.PrimitiveFolder);
            projects[1].GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => CreateArtifactWithRandomName(
                artifactType, _user, projects[1], parentArtifact.Id),
                "'POST {0}' should return 409 Conflict when the Project ID is different than the project of the parent!");

            // Verify:
            ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotSaveConflictWithParent,
                "Invalid request.");
        }

        #endregion Negative tests

        #region Manually run performance tests
        
        [Explicit(IgnoreReasons.ManualOnly)]
        [TestCase]
        [TestRail(183565)]
        [Description("Create 100 artifacts in Open API and in Nova and compare how long each took.  Verify that Nova is at least as fast as Open API.")]
        public void CreateArtifact_ComparePerformanceOfOpenApiToNova_VerifyNovaIsNotSlowerThanOpenApi()
        {
            ItemTypePredefined artifactType = ItemTypePredefined.Process;
            const int numberOfIterations = 100;

            // Start timing Nova:
            string baseNovaArtifactName = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);
            var startTime = DateTime.Now;

            for (int i = 0; i < numberOfIterations; ++i)
            {
                string artifactName = I18NHelper.FormatInvariant("{0}-{1}", baseNovaArtifactName, i);

                Assert.DoesNotThrow(() =>
                    CreateArtifact(_user, _project, artifactType, artifactName, _project.Id),
                    "'POST {0}' should return 201 Created when trying to create an artifact of type: '{1}'!",
                    SVC_PATH, artifactType);
            }

            var endTime = DateTime.Now;
            var elapsedTime = new TimeSpan(endTime.Ticks - startTime.Ticks);
            double novaTime = elapsedTime.TotalSeconds;

            // Start timing OpenApi.
            string baseOpenApiArtifactName = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);
            startTime = DateTime.Now;

            for (int i = 0; i < numberOfIterations; ++i)
            {
                string artifactName = I18NHelper.FormatInvariant("{0}-{1}", baseOpenApiArtifactName, i);

                Assert.DoesNotThrow(() =>
                    Helper.CreateAndSaveArtifact(_project, _user, artifactType.ToBaseArtifactType(), name: artifactName),
                    "'POST {0}' should return 200 OK when trying to create an artifact of type: '{1}'!",
                    RestPaths.OpenApi.Projects_id_.ARTIFACTS, artifactType);
            }

            endTime = DateTime.Now;
            elapsedTime = new TimeSpan(endTime.Ticks - startTime.Ticks);
            double openApiTime = elapsedTime.TotalSeconds;

            // Compare the times:
            Assert.LessOrEqual(novaTime, openApiTime,
                "The Nova CreateArtifact should be faster or equal to the OpenApi Save method!  OpenApi is {0}% faster than Nova!",
                (novaTime - openApiTime) / openApiTime * 100);
        }
        
        #endregion Manually run performance tests

        #region Private functions

        /// <summary>
        /// Creates a new artifact with a random name.
        /// </summary>
        /// <param name="artifactType">The type of artifact to create.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="project">The project where the artifact will be created.</param>
        /// <param name="parentId">(optional) The ID of the parent of the artifact to be created.</param>
        /// <param name="orderIndex">(optional) The Order Index to assign to the new artifact.</param>
        /// <param name="baseType">(optional) You can select a different BaseArtifactType here other than what's in the novaArtifact.
        ///     Use this for artifact types that don't exist in the BaseArtifactType enum.</param>
        /// <returns>The artifact that was created.</returns>
        private INovaArtifactDetails CreateArtifactWithRandomName(ItemTypePredefined artifactType,
            IUser user,
            IProject project,
            int? parentId = null,
            double? orderIndex = null,
            BaseArtifactType? baseType = null)
        {
            string artifactName = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);
            var artifact = ArtifactStore.CreateArtifact(Helper.ArtifactStore.Address,
                user, artifactType, artifactName, project, parentId, orderIndex);

            WrapNovaArtifact(artifact, project, user, baseType);

            return artifact;
        }

        /// <summary>
        /// Tries to create an artifact (which could be invalid).
        /// </summary>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="project">The project where the artifact will be created.</param>
        /// <param name="predefinedType">The predefined (base) type of the artifact.</param>
        /// <param name="artifactName">The name of the artifact.</param>
        /// <param name="parentId">The parent ID of the artifact.</param>
        /// <param name="orderIndex">The order index of the artifact.</param>
        /// <param name="baseType">(optional) You can select a different BaseArtifactType here other than what's in the novaArtifact.
        ///     Use this for artifact types that don't exist in the BaseArtifactType enum.</param>
        /// <returns>The artifact that was created.</returns>
        private INovaArtifactDetails CreateArtifact(IUser user,
            IProject project,
            ItemTypePredefined predefinedType,
            string artifactName = null,
            int? parentId = null,
            double? orderIndex = null,
            BaseArtifactType? baseType = null)
        {
            ThrowIf.ArgumentNull(project, nameof(project));

            artifactName = artifactName ?? RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);
            int itemTypeId = project.GetItemTypeIdForPredefinedType(predefinedType);

            var artifactDetails = CreateNovaArtifactDetails(artifactName, project.Id, itemTypeId, parentId ?? project.Id, orderIndex);
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
        /// Create a NovaArtifactDetails object with the minimum required properties set.
        /// </summary>
        /// <param name="artifactName">(optional) The artifact name.</param>
        /// <param name="projectId">(optional) The Project ID.</param>
        /// <param name="itemTypeId">(optional) The ItemType ID.</param>
        /// <param name="parentId">(optional) The Parent ID.</param>
        /// <param name="orderIndex">(optional) The Order Index.</param>
        /// <returns>A NovaArtifactDetails with the specified properties set.</returns>
        private static INovaArtifactDetails CreateNovaArtifactDetails(string artifactName = null,
            int? projectId = null,
            int? itemTypeId = null,
            int? parentId = null,
            double? orderIndex = null)
        {
            INovaArtifactDetails artifactDetails = new NovaArtifactDetails
            {
                Name = artifactName,
                ProjectId = projectId,
                ItemTypeId = itemTypeId,
                ParentId = parentId,
                OrderIndex = orderIndex
            };

            return artifactDetails;
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
        /// Verifies that the content returned in the rest response contains the specified ErrorCode and Message.
        /// </summary>
        /// <param name="restResponse">The RestResponse that was returned.</param>
        /// <param name="expectedErrorCode">The expected error code.</param>
        /// <param name="expectedErrorMessage">The expected error message.</param>
        private static void ValidateServiceError(RestResponse restResponse, int expectedErrorCode, string expectedErrorMessage)
        {
            IServiceErrorMessage serviceError = null;

            Assert.DoesNotThrow(() =>
            {
                serviceError = JsonConvert.DeserializeObject<ServiceErrorMessage>(restResponse.Content);
            }, "Failed to deserialize the content of the REST response into a ServiceErrorMessage object!");

            IServiceErrorMessage expectedError = ServiceErrorMessageFactory.CreateServiceErrorMessage(
                expectedErrorCode,
                expectedErrorMessage);

            serviceError.AssertEquals(expectedError);
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
