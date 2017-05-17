using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.Factories;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Model.ModelHelpers;
using TestCommon;
using Utilities;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]    // TODO: create separate class for tests with Baselines & Collections.
    public class MoveArtifactTests : TestBase
    {
        private const string SVC_PATH = RestPaths.Svc.ArtifactStore.Artifacts_id_.MOVE_TO_id_;

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

        #region 200 OK tests

        [TestCase(ItemTypePredefined.PrimitiveFolder)]
        [TestRail(182346)]
        [Description("Create & publish three artifacts.  Create chain: grandparent, parent and child.  Move parent artifact with a child to be a child of the project.  " +
                     "Verify the moved artifact is returned with the updated Parent ID.")]
        public void MoveArtifact_PublishedArtifactWithDependentChildBecomesChildOfProject_ReturnsArtifactDetails_200OK(ItemTypePredefined artifactType)
        {
            // Setup: 
            var grandParentArtifact = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, artifactType);
            var parentArtifact = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, artifactType, grandParentArtifact.Id);
            Helper.CreateAndPublishNovaArtifact(_authorUser, _project, artifactType, parentArtifact.Id);

            INovaArtifactDetails movedArtifactDetails = null;

            parentArtifact.Lock(_authorUser);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = MoveArtifact(_authorUser, parentArtifact, newParentId: _project.Id);
            }, "'POST {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, parentArtifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, movedArtifactDetails);

            Assert.AreEqual(_project.Id, movedArtifactDetails.ParentId, "Parent Id of moved artifact is not the same as project Id");
        }

        [TestCase(ItemTypePredefined.PrimitiveFolder)]
        [TestRail(182458)]
        [Description("Create & save three artifacts.  Create chain: grandparent, parent and child.  Move parent artifact with a child to be a child of the project.  " +
                     "Verify the moved artifact is returned with the updated Parent ID.")]
        public void MoveArtifact_SavedArtifactWithDependentChildBecomesChildOfProject_ReturnsArtifactDetails_200OK(ItemTypePredefined artifactType)
        {
            // Setup: 
            var grandParentArtifact = Helper.CreateNovaArtifact(_authorUser, _project, artifactType);
            var parentArtifact = Helper.CreateNovaArtifact(_authorUser, _project, artifactType, grandParentArtifact.Id);
            Helper.CreateNovaArtifact(_authorUser, _project, artifactType, parentArtifact.Id);

            INovaArtifactDetails movedArtifactDetails = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = MoveArtifact(_authorUser, parentArtifact, newParentId: _project.Id);
            }, "'POST {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, parentArtifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, movedArtifactDetails);
            Assert.AreEqual(_project.Id, movedArtifactDetails.ParentId, "Parent Id of moved artifact is not the same as project Id");
        }

        [TestCase(ItemTypePredefined.Process)]
        [TestRail(182373)]
        [Description("Create & publish two artifacts.  Move one artifact to be a child of the other.  Verify the moved artifact is returned with the updated Parent ID.")]
        public void MoveArtifact_PublishedArtifactBecomesChildOfPublishedArtifact_ReturnsArtifactDetails_200OK(ItemTypePredefined artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, artifactType);
            var newParentArtifact = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, artifactType);

            artifact.Lock(_authorUser);
            INovaArtifactDetails movedArtifactDetails = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = MoveArtifact(_authorUser, artifact, newParentId: newParentArtifact.Id);
            }, "'POST {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, artifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, movedArtifactDetails);
            Assert.AreEqual(newParentArtifact.Id, movedArtifactDetails.ParentId, "Parent Id of moved artifact is not the same as project Id");
        }

        [TestCase(ItemTypePredefined.Process)]
        [TestRail(182459)]
        [Description("Create & save an artifact.  Move this artifact to be a child of published artifact.  Verify the moved artifact is returned with the updated Parent ID.")]
        public void MoveArtifact_SavedArtifactBecomesChildOfPublishedArtifact_ReturnsArtifactDetails_200OK(ItemTypePredefined artifactType)
        {
            // Setup:
            var artifact = Helper.CreateNovaArtifact(_authorUser, _project, artifactType);
            var newParentArtifact = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, artifactType);

            INovaArtifactDetails movedArtifactDetails = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = MoveArtifact(_authorUser, artifact, newParentId: newParentArtifact.Id);
            }, "'POST {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, artifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, movedArtifactDetails);
            Assert.AreEqual(newParentArtifact.Id, movedArtifactDetails.ParentId, "Parent Id of moved artifact is not the same as parent artifact Id");
        }

        [TestCase(ItemTypePredefined.Process)]
        [TestRail(190743)]
        [Description("Create & save an artifact.  Move this artifact to be a child of saved artifact.  Verify the moved artifact is returned with the updated Parent ID.")]
        public void MoveArtifact_SavedArtifactBecomesChildOfSavedArtifact_ReturnsArtifactDetails_200OK(ItemTypePredefined artifactType)
        {
            // Setup:
            var artifact = Helper.CreateNovaArtifact(_authorUser, _project, artifactType);
            var newParentArtifact = Helper.CreateNovaArtifact(_authorUser, _project, artifactType);

            INovaArtifactDetails movedArtifactDetails = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = MoveArtifact(_authorUser, artifact, newParentId: newParentArtifact.Id);
            }, "'POST {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, artifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, movedArtifactDetails);
            Assert.AreEqual(newParentArtifact.Id, movedArtifactDetails.ParentId, "Parent Id of moved artifact is not the same as parent artifact Id");
        }

        [TestCase(ItemTypePredefined.Process)]
        [TestRail(182381)]
        [Description("Create & publish an artifact.  Move the artifact to the same location.  Verify the moved artifact is returned with the updated Parent ID.")]
        public void MoveArtifact_PublishedArtifact_FromProjectRootToProjectRoot_VerifyParentDidNotChange_200OK(ItemTypePredefined artifactType)
        {
            INovaArtifactDetails movedArtifactDetails = null;

            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, artifactType);

            artifact.Lock(_authorUser);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = MoveArtifact(_authorUser, artifact, newParentId: _project.Id);
            }, "'POST {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, artifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, movedArtifactDetails);
            Assert.AreEqual(_project.Id, movedArtifactDetails.ParentId, "Parent Id of moved artifact is not the same as project Id");
        }

        [TestCase]
        [TestRail(182480)]
        [Description("Create & publish a folder.  Move the created artifact to be a parent of a folder of its descendents. Verify returned code 200 OK.")]
        public void MoveArtifact_PublishFolderAndMoveToBeAChildOfAnotherFolder_200OK()
        {
            // Setup:
            var folder1 = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, ItemTypePredefined.PrimitiveFolder);
            var folder2 = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, ItemTypePredefined.PrimitiveFolder);

            folder1.Lock(_authorUser);

            INovaArtifactDetails movedArtifactDetails = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = MoveArtifact(_authorUser, folder1, newParentId: folder2.Id);
            }, "'POST {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, folder1.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, movedArtifactDetails);
            Assert.AreEqual(folder2.Id, movedArtifactDetails.ParentId, "Parent Id of moved folder is not the same as parent folder Id");
        }

        [TestCase]
        [TestRail(182486)]
        [Description("Create & save a folder.  Move the created artifact to be a parent of a folder of its descendents. Verify returned code 200 OK.")]
        public void MoveArtifact_SaveFolderAndMoveToBeAChildOfAnotherFolder_200OK()
        {
            INovaArtifactDetails movedArtifactDetails = null;

            // Setup:
            var folder1 = Helper.CreateNovaArtifact(_authorUser, _project, ItemTypePredefined.PrimitiveFolder);
            var folder2 = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, ItemTypePredefined.PrimitiveFolder);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = MoveArtifact(_authorUser, folder1, newParentId: folder2.Id);
            }, "'POST {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, folder1.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, movedArtifactDetails);
            Assert.AreEqual(folder2.Id, movedArtifactDetails.ParentId, "Parent Id of moved folder is not the same as parent folder Id");
        }

        [TestCase(ItemTypePredefined.Process)]
        [TestRail(182460)]
        [Description("Create & save an artifact.  Move the artifact to the same location.  Verify the moved artifact is returned with the updated Parent ID.")]
        public void MoveArtifact_SavedArtifact_FromProjectRootToProjectRoot_VerifyParentDidNotChange_200OK(ItemTypePredefined artifactType)
        {
            // Setup:
            var artifact = Helper.CreateNovaArtifact(_authorUser, _project, artifactType);

            INovaArtifactDetails movedArtifactDetails = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = MoveArtifact(_authorUser, artifact, newParentId: _project.Id);
            }, "'POST {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, artifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, movedArtifactDetails);
            Assert.AreEqual(_project.Id, movedArtifactDetails.ParentId, "Parent Id of moved artifact is not the same as project Id");
        }

        [TestCase(BaselineAndCollectionTypePredefined.ArtifactCollection)]
        [TestCase(BaselineAndCollectionTypePredefined.CollectionFolder)]
        [TestRail(190011)]
        [Description("Create an artifact of collection artifact type & collection folder.  Move this artifact to be a child of the collection folder.  " +
                     "Verify the moved artifact is returned with the updated Parent ID.")]
        public void MoveArtifact_CollectionOrCollectionFolder_MovedToCollectionFolder_ReturnsMovedArtifact(BaselineAndCollectionTypePredefined artifactType)
        {
            // Setup:
            var collectionFolder = Helper.CreateAndPublishCollectionFolder(_project, _authorUser);
            var childArtifact = Helper.CreateCollectionOrCollectionFolder(_project, _authorUser, artifactType);

            childArtifact.Publish(_authorUser);
            childArtifact.Lock(_authorUser);

            INovaArtifactDetails movedArtifactDetails = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = MoveArtifact(_authorUser, childArtifact, newParentId: collectionFolder.Id);
            }, "'POST {0}' should return 200 OK when called with valid parameters!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, childArtifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, movedArtifactDetails);
            Assert.AreEqual(collectionFolder.Id, movedArtifactDetails.ParentId, "Parent Id of moved artifact is not the same as parent artifact Id");
        }

        [TestCase(BaselineAndCollectionTypePredefined.ArtifactCollection)]
        [TestCase(BaselineAndCollectionTypePredefined.CollectionFolder)]
        [TestRail(191030)]
        [Description("Create an artifact of collection artifact type or collection folder. Move this artifact to be a child of the root Collections folder. " + 
                     "Verify the moved artifact is returned with the updated Parent ID.")]
        public void MoveArtifact_CollectionOrCollectionFolder_MovedToDefaultCollectionsFolder_ReturnsMovedArtifact(ItemTypePredefined artifactType)
        {
            // Setup:
            var defaultCollectionFolder = _project.GetDefaultCollectionFolder(_authorUser);
            var collectionFolder = Helper.CreateAndPublishCollectionFolder(_project, _authorUser);
            var childArtifact = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, artifactType, collectionFolder.Id);

            childArtifact.Lock(_authorUser);

            INovaArtifactDetails movedArtifactDetails = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = MoveArtifact(_authorUser, childArtifact, newParentId: defaultCollectionFolder.Id);
            }, "'POST {0}' should return 200 OK when called with valid parameters!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, childArtifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, movedArtifactDetails);
            Assert.AreEqual(defaultCollectionFolder.Id, movedArtifactDetails.ParentId, "Parent Id of moved artifact is not the same as parent artifact Id");
        }

        [TestCase(ItemTypePredefined.Actor, 5, 0, 2.5)]           // Move 1st artifact between 2nd & 3rd artifacts.
        [TestCase(ItemTypePredefined.Document, 5, 2, 0.5)]        // Move 3rd artifact before first artifact.
        [TestCase(ItemTypePredefined.Glossary, 5, 2, 5.0)]        // Move 3rd artifact after last artifact.
        [TestCase(ItemTypePredefined.PrimitiveFolder, 5, 4, 2.5)] // Move last artifact between 2nd & 3rd artifacts.
        [TestCase(ItemTypePredefined.Process, 5, 4, 1.0)]         // Move last artifact to same OrderIndex as first artifact.
        [TestCase(ItemTypePredefined.UseCase, 5, 0, 1.0)]         // Move first artifact to same location.
        [TestRail(191038)]
        [Description("Create & publish several artifacts.  Move an artifact to the same location but specify an OrderIndex.  " +
                     "Verify the OrderIndex of the artifact was updated.")]
        public void MoveArtifactWithOrderIndex_PublishedArtifact_InsideFolder_VerifyOrderIndexUpdated(
            ItemTypePredefined artifactType, int numberOfArtifacts, int whichArtifact, double orderIndex)
        {
            // Setup:
            INovaArtifactDetails movedArtifactDetails = null;

            var parentFolder = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, artifactType);
            var artifacts = Helper.CreateAndPublishMultipleArtifacts(_project, _authorUser, artifactType, numberOfArtifacts, parentFolder.Id);

            artifacts[whichArtifact].Lock(_authorUser);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = MoveArtifact(_authorUser, artifacts[whichArtifact], newParentId: parentFolder.Id, orderIndex: orderIndex);
            }, "'POST {0}?orderIndex={1}' should return 200 OK when called with a valid token!", SVC_PATH, orderIndex);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, artifacts[whichArtifact].Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, movedArtifactDetails);
            Assert.AreEqual(parentFolder.Id, movedArtifactDetails.ParentId, "Parent Id of moved artifact should not be changed!");
            Assert.AreEqual(orderIndex, artifactDetails.OrderIndex, "The OrderIndex of the moved artifact is not the correct value!");
        }

        [TestCase(ItemTypePredefined.ArtifactCollection, 5, 0, 2.5)]    // Move 1st artifact between 2nd & 3rd artifacts.
        [TestCase(ItemTypePredefined.ArtifactCollection, 5, 2, 0.5)]    // Move 3rd artifact before first artifact.
        [TestCase(ItemTypePredefined.ArtifactCollection, 5, 2, 5.0)]    // Move 3rd artifact after last artifact.
        [TestCase(ItemTypePredefined.CollectionFolder, 5, 4, 2.5)]      // Move last artifact between 2nd & 3rd artifacts.
        [TestCase(ItemTypePredefined.CollectionFolder, 5, 4, 1.0)]      // Move last artifact to same OrderIndex as first artifact.
        [TestCase(ItemTypePredefined.CollectionFolder, 5, 0, 1.0)]      // Move first artifact to same location.
        [TestRail(191039)]
        [Description("Create & publish several Collection or Collection Folder artifacts.  Move an artifact to the same location but specify an OrderIndex.  " +
                     "Verify the OrderIndex of the artifact was updated.")]
        public void MoveArtifactWithOrderIndex_PublishedCollectionOrCollectionFolder_InsideCollectionFolder_VerifyOrderIndexUpdated(
            ItemTypePredefined artifactType, int numberOfArtifacts, int whichArtifact, double orderIndex)
        {
            // Setup:
            INovaArtifactDetails movedArtifactDetails = null;
            var artifacts = new List<ArtifactWrapper>();

            var parentFolder = Helper.CreateAndPublishCollectionFolder(_project, _authorUser);

            for (int i = 0; i < numberOfArtifacts; ++i)
            {
                artifacts.Add(Helper.CreateAndPublishNovaArtifact(_authorUser, _project, artifactType, parentFolder.Id));
            }

            artifacts[whichArtifact].Lock(_authorUser);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = MoveArtifact(_authorUser, artifacts[whichArtifact], newParentId: parentFolder.Id, orderIndex: orderIndex);
            }, "'POST {0}?orderIndex={1}' should return 200 OK when called with a valid token!", SVC_PATH, orderIndex);

            // Verify:
            ArtifactStoreHelper.AssertArtifactsEqual(artifacts[whichArtifact], movedArtifactDetails, skipOrderIndex: true, skipPublishedProperties: true);
            Assert.AreEqual(parentFolder.Id, movedArtifactDetails.ParentId, "Parent Id of moved artifact should not be changed!");
            Assert.AreEqual(orderIndex, movedArtifactDetails.OrderIndex, "The OrderIndex of the moved artifact is not the correct value!");
        }

        [TestCase(ItemTypePredefined.Process)]
        [TestRail(195994)]
        [Description("Create & publish an artifact and 2 folders.  Copy the artifact into the first folder, then move the copied artifact into the second folder.  " +
                     "Verify the moved artifact is returned with the updated Parent ID.")]
        public void MoveArtifact_CopiedArtifact_ToNewFolder_ReturnsArtifactDetails(ItemTypePredefined artifactType)
        {
            // Setup:
            var folder1 = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, ItemTypePredefined.PrimitiveFolder);
            var folder2 = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, ItemTypePredefined.PrimitiveFolder);
            var artifact = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, artifactType);

            var copyResult = artifact.CopyTo(_authorUser, _project, folder1.Id);

            INovaArtifactDetails movedArtifactDetails = null;

            artifact.Lock(_authorUser);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = MoveArtifact(_authorUser, artifact, newParentId: folder2.Id);
            }, "'POST {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, artifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, movedArtifactDetails);
            ArtifactStoreHelper.AssertArtifactsEqual(copyResult.Item1.Artifact, movedArtifactDetails, skipIdAndVersion: true, skipParentId: true, skipPublishedProperties: true);

            Assert.AreEqual(folder2.Id, movedArtifactDetails.ParentId, "Parent Id of moved artifact is not the same as folder2 Id!");
        }

        [TestCase(BaselineAndCollectionTypePredefined.BaselineFolder)]
        [TestCase(BaselineAndCollectionTypePredefined.ArtifactBaseline)]
        [TestRail(266500)]
        [Description("Create artifact of baseline/baseline folder type. Move this artifact to be a child of the baseline folder. " +
                     "Verify the moved artifact is returned with the updated Parent ID.")]
        public void MoveArtifact_BaselineOrBaselineFolder_MovedToBaselineFolder_ReturnsMovedArtifact(BaselineAndCollectionTypePredefined artifactType)
        {
            // Setup:
            var newBaselineFolder = Helper.CreateBaselineFolder(_authorUser, _project);
            var childArtifact = Helper.CreateBaselineOrBaselineFolderOrReview(_authorUser, _project, artifactType);

            INovaArtifactDetails movedArtifactDetails = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = MoveArtifact(_authorUser, childArtifact, newParentId: newBaselineFolder.Id);
            }, "'POST {0}' should return 200 OK when called with valid parameters!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, childArtifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, movedArtifactDetails);
            Assert.AreEqual(newBaselineFolder.Id, movedArtifactDetails.ParentId, "Parent Id of moved artifact is not the same as parent artifact Id");
        }

        #endregion 200 OK tests

        #region 400 Bad Request

        [TestCase(ItemTypePredefined.Process)]
        [TestRail(190962)]
        [Description("Create & publish an artifact.  Move the artifact to be a child of itself. Verify returned code 400 Bad Request.")]
        public void MoveArtifact_PublishArtifactsAndMoveToItself_400BadRequest(ItemTypePredefined artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, artifactType);

            artifact.Lock(_authorUser);

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => MoveArtifact(_authorUser, artifact, newParentId: artifact.Id),
                "'POST {0}' should return 400 Bad Request when artifact moved to itself", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.IncorrectInputParameters,
                "This move will result in a circular relationship between the artifact and its new parent.");
       }

        [TestCase(ItemTypePredefined.Process)]
        [TestRail(190963)]
        [Description("Create & save an artifact.  Move the artifact to be a child of itself. Verify returned code 400 Bad Request.")]
        public void MoveArtifact_SaveArtifactsAndMoveToItself_400BadRequest(ItemTypePredefined artifactType)
        {
            // Setup:
            var artifact = Helper.CreateNovaArtifact(_authorUser, _project, artifactType);

            artifact.Lock(_authorUser);

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => MoveArtifact(_authorUser, artifact, newParentId: artifact.Id),
                "'POST {0}' should return 400 Bad Request when artifact moved to itself", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.IncorrectInputParameters,
                "This move will result in a circular relationship between the artifact and its new parent.");
        }

        [TestCase(-1.1)]
        [TestCase(0)]
        [TestRail(191035)]
        [Description("Create & save an artifact.  Move the artifact and specify an OrderIndex <= 0.  Verify 400 Bad Request is returned.")]
        public void MoveArtifactWithOrderIndex_SavedArtifact_NotPositiveOrderIndex_400BadRequest(double orderIndex)
        {
            // Setup:
            var artifact = Helper.CreateNovaArtifact(_authorUser, _project, ItemTypePredefined.Process);

            artifact.Lock(_authorUser);

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
                MoveArtifact(_authorUser, artifact, newParentId: _project.Id, orderIndex: orderIndex),
                "'POST {0}?orderIndex={1}' should return 400 Bad Request for non-positive OrderIndex values", SVC_PATH, orderIndex);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.IncorrectInputParameters,
                "Parameter orderIndex cannot be equal to or less than 0.");
        }

        [TestCase(BaselineAndCollectionTypePredefined.ArtifactCollection, -0.0001)]
        [TestCase(BaselineAndCollectionTypePredefined.CollectionFolder, 0)]
        [TestRail(191036)]
        [Description("Create & save a Collection or Collection Folder artifact.  Move the artifact and specify an OrderIndex <= 0.  " +
                     "Verify 400 Bad Request is returned.")]
        public void MoveArtifactWithOrderIndex_SavedCollectionOrCollectionFolder_NotPositiveOrderIndex_400BadRequest(
            BaselineAndCollectionTypePredefined artifactType, double orderIndex)
        {
            // Setup:
            var collectionFolder = _project.GetDefaultCollectionFolder(_authorUser);
            var artifact = Helper.CreateCollectionOrCollectionFolder(_project, _authorUser, artifactType, collectionFolder.Id);

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
                MoveArtifact(_authorUser, artifact, collectionFolder.Id, orderIndex),
                "'POST {0}?orderIndex={1}' should return 400 Bad Request for non-positive OrderIndex values", SVC_PATH, orderIndex);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.IncorrectInputParameters,
                "Parameter orderIndex cannot be equal to or less than 0.");
        }

        #endregion 400 Bad Request

        #region 401 Unauthorized tests

        [TestCase(ItemTypePredefined.Process)]
        [TestRail(182380)]
        [Description("Create & publish two artifacts.  Move one artifact to be a child of the other with invalid token in a request.  Verify response returns code 401 Unauthorized.")]
        public void MoveArtifact_PublishedArtifactMoveToParentArtifactWithInvalidToken_401Unauthorized(ItemTypePredefined artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, artifactType);
            var newParentArtifact = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, artifactType);

            var userWithBadToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            artifact.Lock(_authorUser);

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                MoveArtifact(userWithBadToken, artifact, newParentId: newParentArtifact.Id);
            }, "'POST {0}' should return 401 Unauthorized when called with a invalid token!", SVC_PATH);

            // Verify:
            const string expectedExceptionMessage = "Unauthorized call";
            TestHelper.ValidateBodyContents(ex.RestResponse, expectedExceptionMessage);
        }

        #endregion 401 Unauthorized tests

        #region 403 Forbidden tests

        [TestCase(ItemTypePredefined.Process)]
        [TestRail(182405)]
        [Description("Create & publish two artifacts.  Each one in different project.  Move the artifact to be a child of the other in different project.  " +
                     "Verify returned code 403 Forbidden.")]
        public void MoveArtifact_PublishedArtifactMoveToBeAChildOfAnotherArtifactInDifferentProject_403Forbidden(ItemTypePredefined artifactType)
        {
            // Setup:
            var projects = ProjectFactory.GetProjects(_adminUser, numberOfProjects: 2);

            var firstProject = projects[0];
            var secondProject = projects[1];

            var artifact1 = Helper.CreateAndPublishNovaArtifact(_adminUser, firstProject, artifactType);
            var artifact2 = Helper.CreateAndPublishNovaArtifact(_adminUser, secondProject, artifactType);

            artifact1.Lock(_adminUser);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => MoveArtifact(_adminUser, artifact1, newParentId: artifact2.Id),
                "'POST {0}' should return 403 Forbidden when user tries to move artifact to different project", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot move artifact to a different project.");
        }

        [TestCase(ItemTypePredefined.Process)]
        [TestRail(182462)]
        [Description("Create & publish two artifacts.  Each one in different project.  Move the artifact to be a child of the other in different project.  " +
                     "Verify returned code 403 Forbidden.")]
        public void MoveArtifact_SavedArtifactMoveToBeAChildOfAnotherArtifactInDifferentProject_403Forbidden(ItemTypePredefined artifactType)
        {
            // Setup:
            var projects = ProjectFactory.GetProjects(_adminUser, numberOfProjects: 2);

            var firstProject = projects[0];
            var secondProject = projects[1];

            var artifact1 = Helper.CreateNovaArtifact(_adminUser, firstProject, artifactType);
            var artifact2 = Helper.CreateAndPublishNovaArtifact(_adminUser, secondProject, artifactType);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => MoveArtifact(_adminUser, artifact1, newParentId: artifact2.Id),
                "'POST {0}' should return 403 Forbidden when user tries to move artifact to different project", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot move artifact to a different project.");
        }

        [TestCase(ItemTypePredefined.Process)]
        [TestRail(182394)]
        [Description("Create & publish an artifact.  Move the created artifact to be a parent of a folder of its descendents. Verify returned code 403 Forbidden.")]
        public void MoveArtifact_PublishFolderAndMoveToBeAChildOfArtifact_403Forbidden(ItemTypePredefined artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, artifactType);
            var folder = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, ItemTypePredefined.PrimitiveFolder);

            folder.Lock(_authorUser);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => MoveArtifact(_authorUser, folder, newParentId: artifact.Id),
                "'POST {0}' should return 403 Forbidden when folder moved to regular artifact", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot move a folder artifact to non folder/project parent.");
        }

        [TestCase(ItemTypePredefined.Process)]
        [TestRail(182464)]
        [Description("Create & save an artifact.  Move the created artifact to be a parent of a folder of its descendents. Verify returned code 403 Forbidden.")]
        public void MoveArtifact_SaveFolderAndMoveToBeAChildOfArtifact_403Forbidden(ItemTypePredefined artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, artifactType);
            var folder = Helper.CreateNovaArtifact(_authorUser, _project, ItemTypePredefined.PrimitiveFolder);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => MoveArtifact(_authorUser, folder, newParentId: artifact.Id),
                "'POST {0}' should return 403 Forbidden when folder moved to regular artifact", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot move a folder artifact to non folder/project parent.");
        }

        [TestCase(BaselineAndCollectionTypePredefined.ArtifactCollection)]
        [TestCase(BaselineAndCollectionTypePredefined.CollectionFolder)]
        [TestRail(182408)]
        [Description("Create collection or collection folder. Move regular artifact to be a child of the collection or collection folder. Verify returned code 403 Forbidden.")]
        public void MoveArtifact_PublishedArtifact_MoveToCollectionOrCollectionFolder_403Forbidden(BaselineAndCollectionTypePredefined artifactType)
        {
            // Setup:
            var collectionArtifact = Helper.CreateCollectionOrCollectionFolder(_project, _authorUser, artifactType);
            var artifact = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, ItemTypePredefined.Process);

            collectionArtifact.Publish(_authorUser);
            artifact.Lock(_authorUser);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => MoveArtifact(_authorUser, artifact, newParentId: collectionArtifact.Id),
               "'POST {0}' should return 403 Forbidden when user tries to move regular artifact to a {1} artifact type", SVC_PATH, artifactType);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot move an artifact to non project section.");
        }

        [TestCase(BaselineAndCollectionTypePredefined.ArtifactCollection)]
        [TestCase(BaselineAndCollectionTypePredefined.CollectionFolder)]
        [TestRail(190010)]
        [Description("Create a collection or collection folder. Move collection or collection folder to be a child of the regular artifact.  " +
                     "Verify returned code 403 Forbidden.")]
        public void MoveArtifact_CollectionOrCollectionFolder_MoveToRegularArtifact_403Forbidden(BaselineAndCollectionTypePredefined artifactType)
        {
            // Setup:
            var collectionArtifact = Helper.CreateCollectionOrCollectionFolder(_project, _authorUser, artifactType);
            var parentArtifact = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, ItemTypePredefined.Process);

            collectionArtifact.Publish(_authorUser);
            collectionArtifact.Lock(_authorUser);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => MoveArtifact(_authorUser, collectionArtifact, newParentId: parentArtifact.Id),
                   "'POST {0}' should return 403 Forbidden when user tries to move collection or collection folder to be a child of a regular artifact", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot move a collection artifact to non collection section.");
        }

        [TestCase(BaselineAndCollectionTypePredefined.ArtifactCollection)]
        [TestCase(BaselineAndCollectionTypePredefined.CollectionFolder)]
        [TestRail(191040)]
        [Description("Create a collection or collection folder. Move collection or collection folder to be a child of the collection artifact.  " +
                     "Verify returned code 403 Forbidden.")]
        public void MoveArtifact_CollectionOrCollectionFolder_MoveToCollectionArtifact_403Forbidden(BaselineAndCollectionTypePredefined artifactType)
        {
            // Setup:
            var collectionOrCollectionFolder = Helper.CreateCollectionOrCollectionFolder(_project, _authorUser, artifactType);
            var collectionArtifact = Helper.CreateAndPublishCollection(_project, _authorUser);

            collectionOrCollectionFolder.Publish(_authorUser);
            collectionOrCollectionFolder.Lock(_authorUser);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => MoveArtifact(_authorUser, collectionOrCollectionFolder, newParentId: collectionArtifact.Id),
                   "'POST {0}' should return 403 Forbidden when user tries to move collection or collection folder to collection artifact", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot move a collection artifact to non folder parent.");
        }

        [TestCase(BaselineAndCollectionTypePredefined.BaselineFolder)]
        [TestCase(BaselineAndCollectionTypePredefined.ArtifactBaseline)]
        [TestRail(266503)]
        [Description("Create artifact of baseline/baseline folder type. Try to move this artifact to the project root. " +
                     "Verify 403 and error message.")]
        public void MoveArtifact_BaselineOrBaselineFolder_MovedToProjectRoot_403Forbidden(BaselineAndCollectionTypePredefined artifactType)
        {
            // Setup:
            var baselineArtifact = Helper.CreateBaselineOrBaselineFolderOrReview(_authorUser, _project, artifactType);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                MoveArtifact(_authorUser, baselineArtifact, _project.Id);
            }, "Attempt to move Baseline or Baseline folder to Project root should return 403 Forbidden.");

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden,
                "Cannot move a baseline artifact to non baseline section.");
        }

        [TestCase]
        [TestRail(266504)]
        [Description("Create and publish artifact. Try to move this artifact to the default Baseline folder. " +
                     "Verify 403 and error message.")]
        public void MoveArtifact_PublishedArtifact_MovedToDefaultBaselineFolder_403Forbidden()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, ItemTypePredefined.Actor);
            var defaultBaselineFolder = _project.GetDefaultBaselineFolder(_authorUser);

            artifact.Lock(_authorUser);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                MoveArtifact(_authorUser, artifact, newParentId: defaultBaselineFolder.Id);
            }, "Attempt to move Baseline or Baseline folder to Project root should return 403 Forbidden.");

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden,
                "Cannot move an artifact to non project section.");
        }

        [TestCase(BaselineAndCollectionTypePredefined.BaselineFolder)]
        [TestCase(BaselineAndCollectionTypePredefined.ArtifactBaseline)]
        [TestRail(266505)]
        [Description("Create and publish artifact. Try to move this artifact to the Baseline. Verify 403 and error message.")]
        public void MoveArtifact_PublishedArtifact_MovedToBaseline_403Forbidden(BaselineAndCollectionTypePredefined artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, ItemTypePredefined.Actor);
            var baselineArtifact = Helper.CreateBaselineOrBaselineFolderOrReview(_authorUser, _project, artifactType);

            artifact.Lock(_authorUser);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                MoveArtifact(_authorUser, artifact, newParentId: baselineArtifact.Id);
            }, "Attempt to move artifact to Baseline or Baseline folder should return 403 Forbidden.");

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden,
                "Cannot move an artifact to non project section.");
        }

        [TestCase(BaselineAndCollectionTypePredefined.BaselineFolder)]
        [TestCase(BaselineAndCollectionTypePredefined.ArtifactBaseline)]
        [TestRail(266507)]
        [Description("Create artifact of baseline/baseline folder type. Move this artifact to the default Collection folder. " +
                     "Verify 403 and error message.")]
        public void MoveArtifact_BaselineOrBaselineFolder_MovedToDefaultCollectionFolder_403Forbidden(BaselineAndCollectionTypePredefined artifactType)
        {
            // Setup:
            var defaultCollectionFolder = _project.GetDefaultCollectionFolder(_authorUser);
            var baselineArtifact = Helper.CreateBaselineOrBaselineFolderOrReview(_authorUser, _project, artifactType);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                MoveArtifact(_authorUser, baselineArtifact, newParentId: defaultCollectionFolder.Id);
            }, "Attempt to move Baseline or Baseline folder to the Default Collection folder should return 403 Forbidden.");

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden,
                "Cannot move a baseline artifact to non baseline section.");
        }

        [TestCase(BaselineAndCollectionTypePredefined.CollectionFolder)]
        [TestCase(BaselineAndCollectionTypePredefined.ArtifactCollection)]
        [TestRail(266508)]
        [Description("Create artifact of baseline/baseline folder type. Move this artifact to the default Collection folder. " +
                     "Verify 403 and error message.")]
        public void MoveArtifact_CollectionOrCollectionFolder_MovedToDefaultBaselineFolder_403Forbidden(BaselineAndCollectionTypePredefined artifactType)
        {
            // Setup:
            var defaultBaselineFolder = _project.GetDefaultBaselineFolder(_authorUser);
            var collectionArtifact = Helper.CreateCollectionOrCollectionFolder(_project, _authorUser, artifactType);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                MoveArtifact(_authorUser, collectionArtifact, newParentId: defaultBaselineFolder.Id);
            }, "Attempt to move Baseline or Baseline folder to the Default Collection folder should return 403 Forbidden.");

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden,
                "Cannot move a collection artifact to non collection section.");
        }

        [TestCase]
        [TestRail(266597)]
        [Description("Try to move default Baseline folder to the default Collection folder. Verify 403 and error message.")]
        public void MoveArtifact_DefaultBaselineFolder_MovedToDefaultCollectionFolder_403Forbidden()
        {
            // Setup:
            var defaultBaselineFolder = _project.GetDefaultBaselineFolder(_authorUser);
            var defaultCollectionFolder = _project.GetDefaultCollectionFolder(_authorUser);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                Helper.ArtifactStore.MoveArtifact(_authorUser, defaultBaselineFolder.Id, newParentId: defaultCollectionFolder.Id);
            }, "Attempt to move Default Baseline folder to the Default Collection folder should return 403 Forbidden.");

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden,
                "Cannot move baselines, reviews or root folders for collections, baselines, reviews.");
        }

        #endregion 403 Forbidden tests

        #region 404 Not Found tests

        [TestCase(ItemTypePredefined.Process)]
        [TestRail(182482)]
        [Description("Create & publish an artifact. Move an artifact to be a child of the artifact with Id 0.  Verify returned code 404 Not Found.")]
        public void MoveArtifact_PublishedArtifactCannotBeMovedToArtifactWithId0_404NotFound(ItemTypePredefined artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, artifactType);

            artifact.Lock(_authorUser);

            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.MoveArtifact(_authorUser, artifact.Id, newParentId: 0),
                "'POST {0}' should return 404 Not Found when user tries to move artifact to one that has Id 0", SVC_PATH);

            // NOTE: No ServiceErrorMessage JSON is returned, so this just returns the generic 404 page from IIS.
        }

        [TestCase(ItemTypePredefined.Process)]
        [TestRail(182403)]
        [Description("Create & save an artifact. Move an artifact to be a child of the artifact with Id 0.  Verify returned code 404 Not Found.")]
        public void MoveArtifact_SavedArtifactCannotBeMovedToArtifactWithId0_404NotFound(ItemTypePredefined artifactType)
        {
            // Setup:
            var artifact = Helper.CreateNovaArtifact(_authorUser, _project, artifactType);

            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.MoveArtifact(_authorUser, artifact.Id, newParentId: 0),
                "'POST {0}' should return 404 Not Found when user tries to move artifact to one that has Id 0", SVC_PATH);

            // NOTE: No ServiceErrorMessage JSON is returned, so this just returns the generic 404 page from IIS.
        }

        [TestCase(ItemTypePredefined.Process, int.MaxValue)]
        [TestRail(182429)]
        [Description("Create & publish an artifact. Move an artifact to be a child of the non existing artifact.  Verify returned code 404 Not Found.")]
        public void MoveArtifact_PublishedArtifactCannotBeMovedToNonExistingArtifact_404NotFound(ItemTypePredefined artifactType, int artifactId)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, artifactType);

            artifact.Lock(_authorUser);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => MoveArtifact(_authorUser, artifact, newParentId: artifactId),
                "'POST {0}' should return 404 Not Found when user tries to move artifact to one that does not exist", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound,
                "You have attempted to access an artifact that does not exist or has been deleted.");
        }

        [TestCase(ItemTypePredefined.Process, int.MaxValue)]
        [TestRail(182470)]
        [Description("Create & save an artifact. Move an artifact to be a child of the non existing artifact.  Verify returned code 404 Not Found.")]
        public void MoveArtifact_SavedArtifactCannotBeMovedToNonExistingArtifact_404NotFound(ItemTypePredefined artifactType, int artifactId)
        {
            // Setup:
            var artifact = Helper.CreateNovaArtifact(_authorUser, _project, artifactType);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => MoveArtifact(_authorUser, artifact, newParentId: artifactId),
                "'POST {0}' should return 404 Not Found when user tries to move artifact to one that does not exist", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound,
                "You have attempted to access an artifact that does not exist or has been deleted.");
        }

        [TestCase(ItemTypePredefined.Process)]
        [TestRail(182432)]
        [Description("Create & publish two artifacts.  Delete 2nd one.  Move first artifact to be a child of second artifact.  Verify returned code 404 Not Found.")]
        public void MoveArtifact_PublishedArtifactCannotBeMovedToDeletedArtifact_404NotFound(ItemTypePredefined artifactType)
        {
            // Setup:
            var artifact1 = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, artifactType);
            var artifact2 = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, artifactType);

            artifact2.Delete(_authorUser);
            artifact2.Publish(_authorUser);
            artifact1.Lock(_authorUser);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => MoveArtifact(_authorUser, artifact1, newParentId: artifact2.Id),
                "'POST {0}' should return 404 Not Found when user tries to move artifact to one that does not exist", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound,
                "You have attempted to access an artifact that does not exist or has been deleted.");
        }

        [TestCase(ItemTypePredefined.Process)]
        [TestRail(182479)]
        [Description("Create & save an artifact.  Delete second one.  Move first artifact to be a child of second artifact.  Verify returned code 404 Not Found.")]
        public void MoveArtifact_SavedArtifactCannotBeMovedToDeletedArtifact_404NotFound(ItemTypePredefined artifactType)
        {
            // Setup:
            var artifact1 = Helper.CreateNovaArtifact(_authorUser, _project, artifactType);
            var artifact2 = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, artifactType);

            artifact2.Delete(_authorUser);
            artifact2.Publish(_authorUser);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => MoveArtifact(_authorUser, artifact1, newParentId: artifact2.Id),
                "'POST {0}' should return 404 Not Found when user tries to move artifact to one that does not exist", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound,
                "You have attempted to access an artifact that does not exist or has been deleted.");
        }

        [TestCase(ItemTypePredefined.Process)]
        [TestRail(182480)]
        [Description("Create & publish two artifacts.  Delete first one.  Move first artifact to be a child of second artifact.  Verify returned code 404 Not Found.")]
        public void MoveArtifact_DeletedArtifactCannotBeMovedToAnotherArtifact_404NotFound(ItemTypePredefined artifactType)
        {
            // Setup:
            var artifact1 = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, artifactType);
            var artifact2 = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, artifactType);

            artifact1.Delete(_authorUser);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => MoveArtifact(_authorUser, artifact1, newParentId: artifact2.Id),
                "'POST {0}' should return 404 Not Found when user tries to move artifact to one that does not exist", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound,
                "You have attempted to access an artifact that does not exist or has been deleted.");
        }

        [TestCase(ItemTypePredefined.Process)]
        [TestRail(182416)]
        [Description("Create & publish two artifacts.  Move an artifact to be a child of the other one with user that does not have proper permissions " +
                     "to future child artifact.  Verify returned code 404 Not Found.")]
        public void MoveArtifact_PublishedArtifactCannotBeMovedForUserWithoutProperPermissions_404NotFound(ItemTypePredefined artifactType)
        {
            // Setup:
            var artifact1 = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, artifactType);
            var artifact2 = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, artifactType);

            var userWithoutPermissions = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            artifact1.Lock(userWithoutPermissions);

            Helper.AssignProjectRolePermissionsToUser(userWithoutPermissions, TestHelper.ProjectRole.None, _project, artifact1);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => MoveArtifact(userWithoutPermissions, artifact1, newParentId: artifact2.Id),
                "'POST {0}' should return 404 Not Found when user tries to move artifact without proper permissions", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound,
                "You have attempted to access an artifact that does not exist or has been deleted.");
        }

        [TestCase(ItemTypePredefined.Process)]
        [TestRail(182463)]
        [Description("Create & save an artifact.  Move an artifact to be a child of the other one with user that does not have proper permissions to future parent artifact.  " +
                     "Verify returned code 404 Not Found.")]
        public void MoveArtifact_SavedArtifactCannotBeMovedForUserWithoutProperPermissions_404NotFound(ItemTypePredefined artifactType)
        {
            // Setup:
            var userWithoutPermissions = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var artifact1 = Helper.CreateNovaArtifact(userWithoutPermissions, _project, artifactType);
            var artifact2 = Helper.CreateNovaArtifact(userWithoutPermissions, _project, artifactType);

            Helper.AssignProjectRolePermissionsToUser(userWithoutPermissions, TestHelper.ProjectRole.None, _project, artifact2);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => MoveArtifact(userWithoutPermissions, artifact1, newParentId: artifact2.Id),
                "'POST {0}' should return 404 Not found when user tries to move artifact without proper permissions", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound,
                "You have attempted to access an artifact that does not exist or has been deleted.");
        }

        [TestCase(ItemTypePredefined.Process)]
        [TestRail(182424)]
        [Description("Create & publish two artifacts.  Move an artifact to be a child of the other one to which user does not have proper permissions.  " +
                     "Verify returned code 404 Not Found.")]
        public void MoveArtifact_PublishedArtifactCannotBeMovedToArtifactWhichUserDoesNotHaveProperPermissions_404NotFound(ItemTypePredefined artifactType)
        {
            // Setup:
            var artifact1 = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, artifactType);
            var artifact2 = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, artifactType);

            // Create a user without permission to the artifact.
            var userWithoutPermissions = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            artifact1.Lock(userWithoutPermissions);

            Helper.AssignProjectRolePermissionsToUser(userWithoutPermissions, TestHelper.ProjectRole.None, _project, artifact2);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => MoveArtifact(userWithoutPermissions, artifact1, newParentId: artifact2.Id),
                "'POST {0}' should return 404 Not Found when user tries to move artifact to an artifact to which user has no permissions", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound,
                "You have attempted to access an artifact that does not exist or has been deleted.");
        }

        #endregion 404 Not Found tests

        #region 409 Conflict tests

        [TestCase(ItemTypePredefined.Process)]
        [TestRail(182401)]
        [Description("Create & publish two artifacts.  Do not put lock on artifact that would be moved.  Move the artifact to be a child of the other.  " +
                     "Verify returned code 409 Conflict.")]
        public void MoveArtifact_PublishArtifactAndMoveToBeAChildOfAnotherArtifact_DoNotSetLock_409Conflict(ItemTypePredefined artifactType)
        {
            // Setup:
            var parentArtifact = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, artifactType);
            var childArtifact = Helper.CreateAndPublishNovaArtifact(_authorUser, _project, artifactType);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => MoveArtifact(_authorUser, childArtifact, newParentId: parentArtifact.Id),
                "'POST {0}' should return 409 Conflict when user moves an unlocked artifact to be a child of another artifact.", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.LockedByOtherUser,
                "Cannot move an artifact that has not been locked.");
        }

        [TestCase(ItemTypePredefined.Process, 2)]
        [TestCase(ItemTypePredefined.Process, 3)]
        [TestRail(182406)]
        [Description("Create & publish number of artifacts.  Move the first created artifact to be a child of one of its descendents.  " +
                     "If one created it will be circular to itself.  Verify returned code 409 Conflict.")]
        public void MoveArtifact_PublishArtifactsAndCreateCircularDependency_409Conflict(ItemTypePredefined artifactType, int numberOfArtifacts)
        {
            // Setup:
            var artifactTypes = CreateListOfArtifactTypes(numberOfArtifacts, artifactType);
            var artifacts = Helper.CreatePublishedArtifactChain(_project, _authorUser, artifactTypes.ToArray());

            Assert.IsNotNull(artifacts, "Artifact List is not created");

            var firstArtifact = artifacts.First();
            var lastArtifact = artifacts.Last();

            firstArtifact.Lock(_authorUser);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => MoveArtifact(_authorUser, firstArtifact, newParentId: lastArtifact.Id),
                "'POST {0}' should return 409 Conflict when artifact moved to one of its descendents", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CycleRelationship,
                "This move will result in a circular relationship between the artifact and its new parent.");
        }

        [TestCase(ItemTypePredefined.Process, 2)]
        [TestCase(ItemTypePredefined.Process, 3)]
        [TestRail(182483)]
        [Description("Publish artifact chain.  Save and move the first created artifact to be a child of one of its descendents.  " +
                     "If one created it will be circular to itself.  Verify returned code 409 Conflict.")]
        public void MoveArtifact_SaveArtifactAndCreateCircularDependency_409Conflict(ItemTypePredefined artifactType, int numberOfArtifacts)
        {
            // Setup:
            var artifactTypes = CreateListOfArtifactTypes(numberOfArtifacts, artifactType);
            var artifacts = Helper.CreatePublishedArtifactChain(_project, _authorUser, artifactTypes.ToArray());

            Assert.IsNotNull(artifacts, "Artifact List is not created");

            var firstArtifact = artifacts.First();
            var lastArtifact = artifacts.Last();

            firstArtifact.Lock(_authorUser);
            firstArtifact.SaveWithNewDescription(_authorUser);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => MoveArtifact(_authorUser, firstArtifact, newParentId: lastArtifact.Id),
                "'POST {0}' should return 409 Conflict when artifact moved to one of its descendents", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CycleRelationship,
                "This move will result in a circular relationship between the artifact and its new parent.");
        }

        #endregion 409 Conflict tests

        #region private call

        /// <summary>
        /// Creates a list of artifact types.
        /// </summary>
        /// <param name="numberOfArtifacts">The number of artifact types to add to the list.</param>
        /// <param name="artifactType">The artifact type.</param>
        /// <returns>A list of artifact types.</returns>
        private static List<ItemTypePredefined> CreateListOfArtifactTypes(int numberOfArtifacts, ItemTypePredefined artifactType)
        {
            var artifactTypes = new List<ItemTypePredefined>();

            for (int i = 0; i < numberOfArtifacts; i++)
            {
                artifactTypes.Add(artifactType);
            }

            return artifactTypes;
        }

        /// <summary>
        /// Moves the specified artifact to the new parent with an optional order index and updates the internal state of the ArtifactWrapper.
        /// </summary>
        /// <param name="user">The user to perform the move.</param>
        /// <param name="artifact">The artifact to move.</param>
        /// <param name="newParentId">The ID of the new parent where the artifact is being moved to.</param>
        /// <param name="orderIndex">(optional) The order index to assign to the artifact.  By default the artifact is added after the last
        ///     artifact under the new parent.</param>
        /// <returns>The artifact response from the ArtifactStore Move call.</returns>
        private INovaArtifactDetails MoveArtifact(
            IUser user,
            ArtifactWrapper artifact,
            int newParentId,
            double? orderIndex = null)
        {
            var response = Helper.ArtifactStore.MoveArtifact(user, artifactId: artifact.Id, newParentId: newParentId, orderIndex: orderIndex);

            artifact.UpdateArtifactState(ArtifactWrapper.ArtifactOperation.Move);

            return response;
        }

        #endregion private call
    }
}
