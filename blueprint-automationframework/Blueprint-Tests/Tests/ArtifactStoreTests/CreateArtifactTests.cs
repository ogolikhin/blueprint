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
using Newtonsoft.Json;
using NUnit.Framework;
using TestCommon;
using Utilities;
using Utilities.Facades;
using Utilities.Factories;
using System.Globalization;
using Model.ModelHelpers;

namespace ArtifactStoreTests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]    // Ignore for now.
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class CreateArtifactTests : TestBase
    {
        private const string SVC_PATH = RestPaths.Svc.ArtifactStore.Artifacts.CREATE;

        private IUser _adminUser = null;
        private IUser _authorUser = null;
        private IProject _project = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_adminUser);
            _authorUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #region 201 Created tests

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForNovaRestMethods))]
        [TestRail(154745)]
        [Description("Create an artifact of a supported type in the project root.  Get the artifact.  " +
                     "Verify the artifact returned has the same properties as the artifact we created.")]
        public void CreateArtifact_ValidArtifactTypeUnderProject_CanGetArtifact(ItemTypePredefined artifactType)
        {
            // Setup:
            var authorUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);  // Author without Delete permission.
            string randomName = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);

            // Execute:
            INovaArtifactDetails newArtifact = null;

            Assert.DoesNotThrow(() => newArtifact = Helper.CreateNovaArtifact(authorUser, _project, artifactType, name: randomName),
                "'POST {0}' should return 201 Created when trying to create an artifact of type: '{1}'!", SVC_PATH, artifactType);

            // Verify:
            Assert.NotNull(newArtifact, "'POST {0}' returned null for an artifact of type: {1}!", SVC_PATH, artifactType);
            Assert.AreEqual(randomName, newArtifact.Name, "The artifact Name is incorrect!");
            // TODO: Validate other properties.

            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(authorUser, newArtifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, newArtifact);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForNovaRestMethods))]
        [TestRail(182496)]
        [Description("Create an artifact of a supported type under a folder.  Get the artifact.  " +
                     "Verify the artifact returned has the same properties as the artifact we created.")]
        public void CreateArtifact_ValidArtifactTypeUnderFolder_CanGetArtifact(ItemTypePredefined artifactType)
        {
            // Setup:
            var parentFolder = CreateArtifactWithRandomName(ItemTypePredefined.PrimitiveFolder, _authorUser, _project);

            // Execute:
            INovaArtifactDetails newArtifact = null;

            Assert.DoesNotThrow(() => newArtifact = CreateArtifactWithRandomName(artifactType, _authorUser, _project, parentFolder.Id),
                "'POST {0}' should return 201 Created when trying to create an artifact of type: '{1}'!", SVC_PATH, artifactType);

            // Verify:
            Assert.NotNull(newArtifact, "'POST {0}' returned null for an artifact of type: {1}!", SVC_PATH, artifactType);
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, newArtifact.Id);
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
            var authorUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);  // Author without Delete permission.
            var collectionFolder = _project.GetDefaultCollectionFolder(authorUser);
            string randomName = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);

            // Execute:
            INovaArtifactDetails newArtifact = null;

            Assert.DoesNotThrow(() =>
                newArtifact = Helper.CreateNovaArtifact(authorUser, _project, artifactType, collectionFolder.Id, name: randomName),
                "'POST {0}' should return 201 Created when trying to create an artifact of type: '{1}'!", SVC_PATH, artifactType);

            // Verify:
            Assert.NotNull(newArtifact, "'POST {0}' returned null for an artifact of type: {1}!", SVC_PATH, artifactType);
            Assert.AreEqual(randomName, newArtifact.Name, "The artifact Name is incorrect!");
            // TODO: Validate other properties.

            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(authorUser, newArtifact.Id);
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
            var parentCollectionsFolder = Helper.CreateUnpublishedCollectionFolder(_project, _authorUser);

            // Execute:
            INovaArtifactDetails newArtifact = null;

            Assert.DoesNotThrow(() =>
                newArtifact = CreateArtifactWithRandomName(artifactType, _authorUser, _project, parentCollectionsFolder.Id),
                "'POST {0}' should return 201 Created when trying to create an artifact of type: '{1}'!", SVC_PATH, artifactType);

            // Verify:
            Assert.NotNull(newArtifact, "'POST {0}' returned null for an artifact of type: {1}!", SVC_PATH, artifactType);
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, newArtifact.Id);
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
            var firstArtifact = CreateArtifactWithRandomName(artifactType, _authorUser, _project);

            Assert.NotNull(firstArtifact.OrderIndex, "OrderIndex of newly created artifact must not be null!");
            Assert.Greater(firstArtifact.OrderIndex, 0, "OrderIndex of newly created artifact must be > 0!");

            double orderIndexToSet = firstArtifact.OrderIndex.Value + orderIndexOffset;

            // Execute:
            INovaArtifactDetails newArtifact = null;

            Assert.DoesNotThrow(() =>
                newArtifact = CreateArtifactWithRandomName(artifactType, _authorUser, _project, orderIndex: orderIndexToSet),
                "'POST {0}' should return 201 Created when trying to create an artifact of type: '{1}'!", SVC_PATH, artifactType);

            // Verify:
            Assert.NotNull(newArtifact, "'POST {0}' returned null for an artifact of type: {1}!", SVC_PATH, artifactType);
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, newArtifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, newArtifact);

            Assert.NotNull(newArtifact.OrderIndex, "OrderIndex of newly created artifact must not be null!");
            Assert.AreEqual(orderIndexToSet, newArtifact.OrderIndex.Value, "The OrderIndex of the new artifact is not correct!");
        }

        [TestCase(BaselineAndCollectionTypePredefined.ArtifactBaseline)]
        [TestCase(BaselineAndCollectionTypePredefined.BaselineFolder)]
        [TestRail(261313)]
        [Description("Create baseline folder under default baseline folder. Create the artifact. " +
                     "Verify the artifact returned has the same properties as the artifact we created.")]
        public void CreateArtifact_ValidBaselineOrBaselineFolderUnderBaselineFolder_CanGetArtifact(ItemTypePredefined artifactType)
        {
            // Setup:
            var parentBaselineFolder = Helper.CreateBaselineFolder(_authorUser, _project);

            // Execute:
            INovaArtifactDetails newArtifact = null;

            Assert.DoesNotThrow(() => newArtifact = CreateArtifactWithRandomName(artifactType, _authorUser, _project, parentBaselineFolder.Id),
                "'POST {0}' should return 201 Created when trying to create an artifact of type: '{1}'!", SVC_PATH, artifactType);

            // Verify:
            Assert.NotNull(newArtifact, "'POST {0}' returned null for an artifact of type: {1}!", SVC_PATH, artifactType);

            NovaArtifactDetails artifactDetails = null;

            if (newArtifact.ItemTypeId == _project.GetNovaBaseItemTypeId(ItemTypePredefined.ArtifactBaseline))
            {
                var baseline = Helper.ArtifactStore.GetBaseline(_authorUser, newArtifact.Id);
                artifactDetails = baseline;

                Assert.AreEqual(0, baseline.Artifacts?.Count, "The {0} property should empty!", nameof(baseline.Artifacts));
                Assert.NotNull(baseline.MinimalUtcTimestamp, "The {0} property should have a value!", nameof(baseline.MinimalUtcTimestamp));
                Assert.IsFalse(baseline.NotAllArtifactsAreShown, "The {0} property should be false!", nameof(baseline.NotAllArtifactsAreShown));
            }
            else if (newArtifact.ItemTypeId == _project.GetNovaBaseItemTypeId(ItemTypePredefined.BaselineFolder))
            {
                artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, newArtifact.Id);
            }
            else
            {
                Assert.Fail("Created Artifact has unexpected ItemType.");
            }

            NovaArtifactDetails.AssertArtifactsEqual(artifactDetails, newArtifact);
        }

        [TestCase(BaselineAndCollectionTypePredefined.ArtifactBaseline)]
        [TestRail(266440)]
        [Description("Create baseline under default baseline folder. Get the baseline. " +
                     "Verify the artifact returned has the same properties as the artifact we created.")]
        public void CreateArtifact_ValidBaselineInDefaultBaselineFolder_CanGetArtifact(ItemTypePredefined artifactType)
        {
            // Setup:
            var defaultBaselineFolder = _project.GetDefaultBaselineFolder(_authorUser);
            
            // Execute:
            INovaArtifactDetails newArtifact = null;

            Assert.DoesNotThrow(() =>
                newArtifact = CreateArtifactWithRandomName(artifactType, _authorUser, _project, defaultBaselineFolder.Id),
                "'POST {0}' should return 201 Created when trying to create an artifact of type: '{1}'!", SVC_PATH, artifactType);

            // Verify:
            Assert.NotNull(newArtifact, "'POST {0}' returned null for an artifact of type: {1}!", SVC_PATH, artifactType);

            NovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetBaseline(_authorUser, newArtifact.Id);
            NovaArtifactDetails.AssertArtifactsEqual(artifactDetails, newArtifact);
        }

        [TestCase(BaselineAndCollectionTypePredefined.ArtifactBaseline)]
        [TestRail(266441)]
        [Description("Create baseline under newly created baseline. Get the baseline. " +
                     "Verify the artifact returned has the same properties as the artifact we created.")]
        public void CreateArtifact_ValidBaselineInNewBaseline_CanGetArtifact(ItemTypePredefined artifactType)
        {
            // Setup:
            var parentBaseline = Helper.CreateBaseline(_authorUser, _project);

            // Execute:
            INovaArtifactDetails newArtifact = null;

            Assert.DoesNotThrow(() =>
                newArtifact = CreateArtifactWithRandomName(artifactType, _authorUser, _project, parentBaseline.Id),
                "'POST {0}' should return 201 Created when trying to create an artifact of type: '{1}'!", SVC_PATH, artifactType);

            // Verify:
            Assert.NotNull(newArtifact, "'POST {0}' returned null for an artifact of type: {1}!", SVC_PATH, artifactType);

            NovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetBaseline(_authorUser, newArtifact.Id);
            NovaArtifactDetails.AssertArtifactsEqual(artifactDetails, newArtifact);
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
            var fakeProject = ProjectFactory.CreateProject(id: projectId);
            fakeProject.NovaArtifactTypes.AddRange(_project.NovaArtifactTypes);

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => CreateArtifact(_authorUser, fakeProject, ItemTypePredefined.Process),
                "'POST {0}' should return 400 Bad Request if an invalid Project ID was passed!", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.IncorrectInputParameters, "Project not found.");
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
            var ex = Assert.Throws<Http400BadRequestException>(() => CreateArtifactFromJson(_authorUser, jsonBody),
                "'POST {0}' should return 400 Bad Request if a required property is missing!", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.IncorrectInputParameters, "Invalid request.");
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
            var ex = Assert.Throws<Http400BadRequestException>(() => CreateArtifactFromJson(_authorUser, corruptJsonBody),
                "'POST {0}' should return 400 Bad Request when a corrupt JSON body is sent!");

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.IncorrectInputParameters, "An artifact is not defined.");
        }

        [TestCase]
        [TestRail(154746)]
        [Description("Create an artifact but don't send a 'Session-Token' header in the request.  Verify 401 Unauthorized is returned.")]
        public void CreateArtifact_NoTokenHeader_401Unauthorized()
        {
            // Setup:
            var userWithNoToken = Helper.CreateUserAndAddToDatabase();

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
                CreateArtifactWithRandomName(ItemTypePredefined.Process, userWithNoToken, _project),
                "'POST {0}' should return 401 Unauthorized if no Session-Token header is passed!", SVC_PATH);

            // Verify:
            TestHelper.ValidateBodyContents(ex.RestResponse, "Unauthorized call");
        }

        [TestCase]
        [TestRail(154747)]
        [Description("Create an artifact but pass an unauthorized token.  Verify 401 Unauthorized is returned.")]
        public void CreateArtifact_UnauthorizedToken_401Unauthorized()
        {
            // Setup:
            var userWithBadToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
                CreateArtifactWithRandomName(ItemTypePredefined.Process, userWithBadToken, _project),
                "'POST {0}' should return 401 Unauthorized if an invalid token is passed!", SVC_PATH);

            // Verify:
            TestHelper.ValidateBodyContents(ex.RestResponse, "Unauthorized call");
        }

        [TestCase]
        [TestRail(154748)]
        [Description("Create an artifact as a user that doesn't have permission to add artifacts to the project.  Verify 403 Forbidden is returned.")]
        public void CreateArtifact_UserWithoutPermissionToProject_403Forbidden()
        {
            // Setup:
            var userWithoutPermission = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() =>
                CreateArtifact(userWithoutPermission, _project, ItemTypePredefined.Process),
                "'POST {0}' should return 403 Forbidden if the user doesn't have permission to add artifacts!", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "You do not have permission to edit the artifact (ID: 1)");
        }

        [TestCase]
        [TestRail(183538)]
        [Description("Create an artifact as a user that full access to the project, but no access to the parent.  Verify 403 Forbidden is returned.")]
        public void CreateArtifact_UserHasNoPermissionToParentArtifact_403Forbidden()
        {
            // Setup:
            var parentArtifact = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, ItemTypePredefined.PrimitiveFolder);

            // Create a user that has full access to project, but no access to parentArtifact.
            var userWithoutPermission = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);
            Helper.AssignProjectRolePermissionsToUser(userWithoutPermission, TestHelper.ProjectRole.None, _project, parentArtifact);

            string artifactName = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);

            // Execute:
            var ex= Assert.Throws<Http403ForbiddenException>(() =>
                CreateArtifact(userWithoutPermission, _project, ItemTypePredefined.Process, artifactName, parentArtifact.Id),
                "'POST {0}' should return 403 Forbidden if the user doesn't have write permission to parent artifact!", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, string.Format(CultureInfo.InvariantCulture,
                "You do not have permission to access the artifact (ID: {0})", parentArtifact.Id));
        }

        [TestCase]
        [TestRail(185235)]
        [Description("Create an artifact as a user that full access to the project, but only view access to the parent.  Verify 403 Forbidden is returned.")]
        public void CreateArtifact_UserHasViewPermissionToParentArtifact_403Forbidden()
        {
            // Setup:
            var parentArtifact = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, ItemTypePredefined.PrimitiveFolder);

            // Create a user that has full access to project, but no access to parentArtifact.
            var userWithoutPermission = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);
            Helper.AssignProjectRolePermissionsToUser(userWithoutPermission, TestHelper.ProjectRole.Viewer, _project, parentArtifact);

            string artifactName = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() =>
                CreateArtifact(userWithoutPermission, _project, ItemTypePredefined.Process, artifactName, parentArtifact.Id),
                "'POST {0}' should return 403 Forbidden if the user doesn't have write permission to parent artifact!", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, string.Format(CultureInfo.InvariantCulture,
                "You do not have permission to edit the artifact (ID: {0})", parentArtifact.Id));
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
            var ex = Assert.Throws<Http404NotFoundException>(() => CreateArtifactFromJson(_authorUser, jsonBody),
                "'POST {0}' should return 404 Not Found if the ItemType ID doesn't exist!", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemTypeNotFound, "Artifact type not found.");
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
            var ex = Assert.Throws<Http404NotFoundException>(() =>
                CreateArtifact(_authorUser, _project, ItemTypePredefined.Process, artifactName, parentId),
                "'POST {0}' should return 404 Not Found if the Parent ID doesn't exist!", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound,
                "You have attempted to access an artifact that does not exist or has been deleted.");
        }

        [TestCase(int.MaxValue)]
        [TestRail(154749)]
        [Description("Create an artifact with a non-existent Project ID.  Verify 404 Not Found is returned.")]
        public void CreateArtifact_NonExistentProjectId_404NotFound(int projectId)
        {
            // Setup:
            // Create a Project with a fake ID that shouldn't exist.
            var fakeProject = ProjectFactory.CreateProject(id: projectId);
            fakeProject.NovaArtifactTypes.AddRange(_project.NovaArtifactTypes);
            string artifactName = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() =>
                CreateArtifact(_authorUser, fakeProject, ItemTypePredefined.Process, artifactName),
                "'POST {0}' should return 404 Not Found if the Project ID doesn't exist!", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ProjectNotFound, "Project not found.");
        }

        [TestRail(183543)]
        [Description("Create a regular artifact under the default Collections folder. Verify the create fails with a 409 Conflict error.")]
        public void CreateArtifact_AddArtifactUnderCollectionsFolder_409Conflict()
        {
            // Setup:
            var defaultCollectionFolder = _project.GetDefaultCollectionFolder(_authorUser);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() =>
                CreateArtifactWithRandomName(ItemTypePredefined.Actor, _authorUser, _project, defaultCollectionFolder.Id),
                "'POST {0}' should return 409 Conflict when creating a regular artifact under the Collections folder!");

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotSaveConflictWithParent, "Cannot create an artifact at this location.");
        }

        [TestRail(266593)]
        [Description("Create a regular artifact under the default Baselines folder. Verify the create fails with a 409 Conflict error.")]
        public void CreateArtifact_AddArtifactUnderBaselinesFolder_409Conflict()
        {
            // Setup:
            var defaultBaselineFolder = _project.GetDefaultBaselineFolder(_authorUser);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() =>
                CreateArtifactWithRandomName(ItemTypePredefined.Process, _authorUser, _project, defaultBaselineFolder.Id),
                "'POST {0}' should return 409 Conflict when creating a regular artifact under the Baselines folder!");

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotSaveConflictWithParent, "Cannot create an artifact at this location.");
        }

        [TestCase]
        [TestRail(183544)]
        [Description("Create a Collection under another Collection.  Verify the create fails with a 409 Conflict error.")]
        public void CreateArtifact_AddCollectionUnderAnotherCollection_409Conflict()
        {
            // Setup:
            var parentCollection = Helper.CreateUnpublishedCollection(_project, _authorUser);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() =>
                CreateArtifactWithRandomName(ItemTypePredefined.ArtifactCollection, _authorUser, _project, parentCollection.Id),
                "'POST {0}' should return 409 Conflict when creating a Collection under another Collection!");

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotSaveConflictWithParent, "Cannot create an artifact at this location.");
        }

        [TestCase]
        [TestRail(185175)]
        [Description("Create a Collection under the project.  Verify the create fails with a 409 Conflict error.")]
        public void CreateArtifact_AddCollectionUnderProject_409Conflict()
        {
            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() =>
                Helper.CreateUnpublishedCollection(_project, _authorUser, parentId: _project.Id),
                "'POST {0}' should return 409 Conflict when creating a Collection under the project!");

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotSaveConflictWithParent, "Cannot create an artifact at this location.");
        }

        [TestCase]
        [TestRail(185176)]
        [Description("Create a Collection Folder under the project.  Verify the create fails with a 409 Conflict error.")]
        public void CreateArtifact_AddCollectionFolderUnderProject_409Conflict()
        {
            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() =>
                Helper.CreateUnpublishedCollectionFolder(_project, _authorUser, parentId: _project.Id),
                "'POST {0}' should return 409 Conflict when creating a Collection Folder under the project!");

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotSaveConflictWithParent, "Cannot create an artifact at this location.");
        }

        [TestCase(ItemTypePredefined.Actor)]
        [TestRail(183541)]
        [Description("Create a folder under an artifact.  Verify the create fails with a 409 Conflict error.")]
        public void CreateArtifact_AddFolderUnderNonFolder_409Conflict(ItemTypePredefined artifactType)
        {
            // Setup:
            var parentArtifact = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, artifactType);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() =>
                CreateArtifactWithRandomName(ItemTypePredefined.PrimitiveFolder, _authorUser, _project, parentArtifact.Id),
                "'POST {0}' should return 409 Conflict when trying to create a folder under a regular artifact!");

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotSaveConflictWithParent, "Cannot create an artifact at this location.");
        }

        [Explicit(IgnoreReasons.DeploymentNotReady)]    // NOTE: This won't work on Silver02 until we make a required property without a default value.
        [Category(Categories.CustomData)]
        [TestCase(ArtifactTypePredefined.Actor)]
        [TestRail(183536)]
        [Description("Create an artifact in the 'Custom Data' project for a type that has a required Custom Property with no default value.  " +
                     "Verify the create succeeds.  Try to publish.  Verify the publish fails with a 409 Conflict error.")]
        public void CreateArtifact_ArtifactWithMissingRequiredCustomProperty_ArtifactIsCreated_VerifyPublishReturns409Conflict(ItemTypePredefined artifactType)
        {
            // Setup:
            var customDataProject = ArtifactStoreHelper.GetCustomDataProject(_authorUser);
            customDataProject.GetAllNovaArtifactTypes(Helper.ArtifactStore, _authorUser);

            // Execute:
            ArtifactWrapper createdArtifact = null;

            Assert.DoesNotThrow(() => createdArtifact = CreateArtifactWithRandomName(artifactType, _authorUser, customDataProject),
                "'POST {0}' should return 201 Created when trying to create an artifact that has a required property without a default value!");

            // Verify:
            Assert.NotNull(createdArtifact, "A valid object should be returned after creating an artifact!");

            // Now try to publish and verify that it fails because of validation errors.
            var ex = Assert.Throws<Http409ConflictException>(() => createdArtifact.Publish(_authorUser),
                "You shouldn't be able to publish an artifact with missing required properties!");

            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotPublishOverValidationErrors,
                I18NHelper.FormatInvariant("Artifact with Id {0} has validation errors.", createdArtifact.Id));
        }

        [Explicit(IgnoreReasons.ProductBug)]    // TFS: Bug 3975: Creating artifact with required custom property is now plain text instead of HTML wrapped value.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]    // Ignore for now.
        [Category(Categories.CustomData)]
        [TestCase(ArtifactTypePredefined.Actor)]
        [TestRail(183640)]
        [Description("Create an artifact in the 'Custom Data' project for a type that has a required Custom Property with default values.  " +
                     "Verify the create succeeds and the artifact has the default values populated.")]
        public void CreateArtifact_ArtifactWithRequiredCustomPropertyWithDefaults_VerifyDefaultValuesArePopulated(ItemTypePredefined artifactType)
        {
            // Setup:
            var customDataProject = ArtifactStoreHelper.GetCustomDataProject(_authorUser);
            customDataProject.GetAllNovaArtifactTypes(Helper.ArtifactStore, _authorUser);

            // Execute:
            ArtifactWrapper newArtifact = null;

            Assert.DoesNotThrow(() => newArtifact = CreateArtifactWithRandomName(artifactType, _authorUser, customDataProject),
                "'POST {0}' should return 201 Created when trying to create an artifact that has required properties with default values!");

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, newArtifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, newArtifact);

            // Make sure the default values of the required properties got set.
            /*  Name:                                   Default Value:
                "CU-Text Required"                      "<html><head/><pre>Default Text</pre></html>"
                "CU-Number Required with Min & Max"     10
                "CU-Date Required with Min & Max"       2016-09-01T00:00:00.0000000

            NOTE: These 2 properties are hard to validate properly, so skipping them.
                "CU-Choice Required with Single Choice  "{\r\n  \"validValueIds\": [\r\n    22\r\n  ]\r\n}"
                "CU-User Required"                      "{\r\n  \"usersGroups\": [\r\n    {\r\n      \"id\": 1,\r\n      \"displayName\": \"Default Instance Admin\"\r\n    }\r\n  ]\r\n}"
            */

            var expectedValues = new Dictionary<string, object>
            {
                { "CU-Text Required", "<html><head/><pre>Default Text</pre></html>" },
                { "CU-Number Required with Min & Max", 10 },
                { "CU-Date Required with Min & Max", new DateTime(2016, 9, 1) }
            };

            foreach (var expectedValue in expectedValues)
            {
                var novaPropertyType = customDataProject.NovaPropertyTypes.Find(o => o.Name == expectedValue.Key);
                Assert.NotNull(novaPropertyType, "Couldn't find a Nova Artifact Type named '{0}'!", expectedValue.Key);

                var customProperty = artifactDetails.CustomPropertyValues.Find(p => p.PropertyTypeId == novaPropertyType.Id);

                Assert.NotNull(customProperty, "Couldn't find a custom property with PropertyTypeId {0}!", novaPropertyType.Id);
                Assert.AreEqual(expectedValue.Value, customProperty.CustomPropertyValue,
                    "Custom Property Value for property '{0}' is not set to the default value!", customProperty.Name);
            }
        }

        [TestCase(ItemTypePredefined.Actor)]
        [TestRail(183542)]
        [Description("Create an artifact and specify a ProjectId that is different than the project of the parent.  " +
                     "Verify the create fails with a 409 Conflict error.")]
        public void CreateArtifact_ParentExistsInADifferentProject_409Conflict(ItemTypePredefined artifactType)
        {
            // Setup:
            var projects = ProjectFactory.GetProjects(_adminUser, numberOfProjects: 2);
            var parentArtifact = Helper.CreateAndPublishNovaArtifact(_adminUser, projects[0], ItemTypePredefined.PrimitiveFolder);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() =>
                CreateArtifactWithRandomName(artifactType, _adminUser, projects[1], parentArtifact.Id),
                "'POST {0}' should return 409 Conflict when the Project ID is different than the project of the parent!", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotSaveConflictWithParent, "Invalid request.");
        }

        [TestCase(BaselineAndCollectionTypePredefined.ArtifactBaseline)]
        [TestCase(BaselineAndCollectionTypePredefined.BaselineFolder)]
        [TestCase(BaselineAndCollectionTypePredefined.ArtifactReviewPackage)]
        [TestRail(266442)]
        [Description("Try to create a baseline/review/baseline folder in the project root.  Verify 409 and error message.")]
        public void CreateArtifact_ValidBaselineInProjectRoot_409Conflict(ItemTypePredefined artifactType)
        {
            // Setup & Execute:
            INovaArtifactDetails newArtifact = null;

            var ex = Assert.Throws<Http409ConflictException>(() =>
                newArtifact = CreateArtifactWithRandomName(artifactType, _authorUser, _project, _project.Id),
                "'POST {0}' should return 409 error when we try to create baseline/review/baseline folder in the root of the project.", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotSaveConflictWithParent,
                "Cannot create an artifact at this location.");
        }

        [TestCase(BaselineAndCollectionTypePredefined.ArtifactBaseline)]
        [TestCase(BaselineAndCollectionTypePredefined.BaselineFolder)]
        [TestCase(BaselineAndCollectionTypePredefined.ArtifactReviewPackage)]
        [TestRail(266445)]
        [Description("Try to create baseline/review/baseline folder in the default collection folder.  Verify 409 and error message.")]
        public void CreateArtifact_ValidBaselineInDefaultCollectionFolder_409Conflict(ItemTypePredefined artifactType)
        {
            // Setup:
            var defaultCollectionFolder = _project.GetDefaultCollectionFolder(_authorUser);

            // Execute:
            INovaArtifactDetails newArtifact = null;

            var ex = Assert.Throws<Http409ConflictException>(() =>
                newArtifact = CreateArtifactWithRandomName(artifactType, _authorUser, _project, defaultCollectionFolder.Id),
                "'POST {0}' should return 409 error when we try to create baseline/review/baseline folder in the default collection folder.", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotSaveConflictWithParent,
                "Cannot create an artifact at this location.");
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

                Assert.DoesNotThrow(() => CreateArtifact(_authorUser, _project, artifactType, artifactName, _project.Id),
                    "'POST {0}' should return 201 Created when trying to create an artifact of type: '{1}'!", SVC_PATH, artifactType);
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
                    Helper.CreateAndSaveOpenApiArtifact(_project, _authorUser, artifactType.ToBaseArtifactType(), name: artifactName),
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
        /// <returns>The artifact that was created.</returns>
        private ArtifactWrapper CreateArtifactWithRandomName(ItemTypePredefined artifactType,
            IUser user,
            IProject project,
            int? parentId = null,
            double? orderIndex = null)
        {
            return Helper.CreateNovaArtifact(user, project, artifactType, parentId, orderIndex);
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
        /// <returns>The artifact that was created.</returns>
        private ArtifactWrapper CreateArtifact(
            IUser user,
            IProject project,
            ItemTypePredefined predefinedType,
            string artifactName = null,
            int? parentId = null,
            double? orderIndex = null)
        {
            ThrowIf.ArgumentNull(project, nameof(project));

            artifactName = artifactName ?? RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);
            int itemTypeId = project.GetItemTypeIdForPredefinedType(predefinedType);

            var artifactDetails = CreateNovaArtifactDetails(artifactName, project.Id, itemTypeId, parentId ?? project.Id, orderIndex);
            string jsonBody = JsonConvert.SerializeObject(artifactDetails);

            var response = CreateArtifactFromJson(user, jsonBody);
            var createdArtifact = JsonConvert.DeserializeObject<NovaArtifactDetails>(response.Content);

            return Helper.WrapArtifact(createdArtifact, project, user);
        }

        /// <summary>
        /// Tries to create an artifact based on the supplied JSON body.
        /// </summary>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="jsonBody">The JSON body of the request.</param>
        /// <returns>The REST response.</returns>
        private RestResponse CreateArtifactFromJson(IUser user, string jsonBody)
        {
            var restApi = new RestApiFacade(Helper.BlueprintServer.Address, user?.Token?.AccessControlToken);

            // Set expectedStatusCodes to 201 Created.
            var expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Created };
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
            var artifactDetails = new NovaArtifactDetails
            {
                Name = artifactName,
                ProjectId = projectId,
                ItemTypeId = itemTypeId,
                ParentId = parentId,
                OrderIndex = orderIndex
            };

            return artifactDetails;
        }

        #endregion Private functions
    }
}
