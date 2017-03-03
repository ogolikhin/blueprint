using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.Factories;
using Model.Impl;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Model.ArtifactModel.Impl;
using TestCommon;
using Utilities;
using Utilities.Factories;

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

        [TestCase(BaseArtifactType.PrimitiveFolder)]
        [TestRail(182346)]
        [Description("Create & publish three artifacts.  Create chain: grandparent, parent and child.  Move parent artifact with a child to be a child of the project.  " +
            "Verify the moved artifact is returned with the updated Parent ID.")]
        public void MoveArtifact_PublishedArtifactWithDependentChildBecomesChildOfProject_ReturnsArtifactDetails_200OK(BaseArtifactType artifactType)
        {
            // Setup: 
            var grandParentArtifact = Helper.CreateAndPublishArtifact(_project, _authorUser, artifactType);
            var parentArtifact = Helper.CreateAndPublishArtifact(_project, _authorUser, artifactType, grandParentArtifact);
            Helper.CreateAndPublishArtifact(_project, _authorUser, artifactType, parentArtifact);

            INovaArtifactDetails movedArtifactDetails = null;

            parentArtifact.Lock();

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = ArtifactStore.MoveArtifact(Helper.BlueprintServer.Address, parentArtifact, _project.Id, _authorUser);
            }, "'POST {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, parentArtifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, movedArtifactDetails);

            Assert.AreEqual(_project.Id, movedArtifactDetails.ParentId, "Parent Id of moved artifact is not the same as project Id");
        }

        [TestCase(BaseArtifactType.PrimitiveFolder)]
        [TestRail(182458)]
        [Description("Create & save three artifacts.  Create chain: grandparent, parent and child.  Move parent artifact with a child to be a child of the project.  " +
            "Verify the moved artifact is returned with the updated Parent ID.")]
        public void MoveArtifact_SavedArtifactWithDependentChildBecomesChildOfProject_ReturnsArtifactDetails_200OK(BaseArtifactType artifactType)
        {
            // Setup: 
            var grandParentArtifact = Helper.CreateAndSaveArtifact(_project, _authorUser, artifactType);
            var parentArtifact = Helper.CreateAndSaveArtifact(_project, _authorUser, artifactType, grandParentArtifact);
            Helper.CreateAndSaveArtifact(_project, _authorUser, artifactType, parentArtifact);

            INovaArtifactDetails movedArtifactDetails = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = ArtifactStore.MoveArtifact(Helper.BlueprintServer.Address, parentArtifact, _project.Id, _authorUser);
            }, "'POST {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, parentArtifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, movedArtifactDetails);
            Assert.AreEqual(_project.Id, movedArtifactDetails.ParentId, "Parent Id of moved artifact is not the same as project Id");
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182373)]
        [Description("Create & publish two artifacts.  Move one artifact to be a child of the other.  Verify the moved artifact is returned with the updated Parent ID.")]
        public void MoveArtifact_PublishedArtifactBecomesChildOfPublishedArtifact_ReturnsArtifactDetails_200OK(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _authorUser, artifactType);
            var newParentArtifact = Helper.CreateAndPublishArtifact(_project, _authorUser, artifactType);

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            artifact.Lock(author);
            INovaArtifactDetails movedArtifactDetails = null;

            // Execute:

            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = Helper.ArtifactStore.MoveArtifact(artifact, newParentArtifact, author);
            }, "'POST {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, artifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, movedArtifactDetails);
            Assert.AreEqual(newParentArtifact.Id, movedArtifactDetails.ParentId, "Parent Id of moved artifact is not the same as project Id");
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182459)]
        [Description("Create & save an artifact.  Move this artifact to be a child of published artifact.  Verify the moved artifact is returned with the updated Parent ID.")]
        public void MoveArtifact_SavedArtifactBecomesChildOfPublishedArtifact_ReturnsArtifactDetails_200OK(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndSaveArtifact(_project, _authorUser, artifactType);
            var newParentArtifact = Helper.CreateAndPublishArtifact(_project, _authorUser, artifactType);

            INovaArtifactDetails movedArtifactDetails = null;

            // Execute:

            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = Helper.ArtifactStore.MoveArtifact(artifact, newParentArtifact, _authorUser);
            }, "'POST {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, artifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, movedArtifactDetails);
            Assert.AreEqual(newParentArtifact.Id, movedArtifactDetails.ParentId, "Parent Id of moved artifact is not the same as parent artifact Id");
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(190743)]
        [Description("Create & save an artifact.  Move this artifact to be a child of saved artifact.  Verify the moved artifact is returned with the updated Parent ID.")]
        public void MoveArtifact_SavedArtifactBecomesChildOfSavedArtifact_ReturnsArtifactDetails_200OK(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndSaveArtifact(_project, _authorUser, artifactType);
            var newParentArtifact = Helper.CreateAndSaveArtifact(_project, _authorUser, artifactType);

            INovaArtifactDetails movedArtifactDetails = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = Helper.ArtifactStore.MoveArtifact(artifact, newParentArtifact, _authorUser);
            }, "'POST {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, artifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, movedArtifactDetails);
            Assert.AreEqual(newParentArtifact.Id, movedArtifactDetails.ParentId, "Parent Id of moved artifact is not the same as parent artifact Id");
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182381)]
        [Description("Create & publish an artifact.  Move the artifact to the same location.  Verify the moved artifact is returned with the updated Parent ID.")]
        public void MoveArtifact_PublishedArtifact_FromProjectRootToProjectRoot_VerifyParentDidNotChange_200OK(BaseArtifactType artifactType)
        {
            INovaArtifactDetails movedArtifactDetails = null;

            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _authorUser, artifactType);

            artifact.Lock();

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = ArtifactStore.MoveArtifact(Helper.BlueprintServer.Address, artifact, _project.Id, _authorUser);
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
            var folder1 = Helper.CreateAndPublishArtifact(_project, _authorUser, BaseArtifactType.PrimitiveFolder);
            var folder2 = Helper.CreateAndPublishArtifact(_project, _authorUser, BaseArtifactType.PrimitiveFolder);

            folder1.Lock();

            INovaArtifactDetails movedArtifactDetails = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = Helper.ArtifactStore.MoveArtifact(folder1, folder2, _authorUser);
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
            var folder1 = Helper.CreateAndSaveArtifact(_project, _authorUser, BaseArtifactType.PrimitiveFolder);
            var folder2 = Helper.CreateAndPublishArtifact(_project, _authorUser, BaseArtifactType.PrimitiveFolder);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = Helper.ArtifactStore.MoveArtifact(folder1, folder2, _authorUser);
            }, "'POST {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, folder1.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, movedArtifactDetails);
            Assert.AreEqual(folder2.Id, movedArtifactDetails.ParentId, "Parent Id of moved folder is not the same as parent folder Id");
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182460)]
        [Description("Create & save an artifact.  Move the artifact to the same location.  Verify the moved artifact is returned with the updated Parent ID.")]
        public void MoveArtifact_SavedArtifact_FromProjectRootToProjectRoot_VerifyParentDidNotChange_200OK(BaseArtifactType artifactType)
        {
            INovaArtifactDetails movedArtifactDetails = null;

            // Setup:
            var artifact = Helper.CreateAndSaveArtifact(_project, _authorUser, artifactType);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = ArtifactStore.MoveArtifact(Helper.BlueprintServer.Address, artifact, _project.Id, _authorUser);
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
        public void MoveArtifact_CollectionOrCollectionFolder_MovedToCollectionFolder_ReturnsMovedArtifact(ItemTypePredefined artifactType)
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _authorUser);

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var defaultCollectionFolder = _project.GetDefaultCollectionOrBaselineReviewFolder(Helper.ArtifactStore.Address, author,
                BaselineAndCollectionTypePredefined.CollectionFolder);
            var collectionFolder = Helper.CreateAndPublishCollectionFolder(_project, author);

            var fakeBaseType = BaseArtifactType.PrimitiveFolder;

            IArtifact childArtifact = Helper.CreateWrapAndPublishNovaArtifact(_project, author, artifactType, defaultCollectionFolder.Id, baseType: fakeBaseType);

            childArtifact.Lock(author);

            INovaArtifactDetails movedArtifactDetails = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = ArtifactStore.MoveArtifact(Helper.BlueprintServer.Address, childArtifact, collectionFolder.Id, author);
            }, "'POST {0}' should return 200 OK when called with valid parameters!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, childArtifact.Id);
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
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _authorUser);

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
            var defaultCollectionFolder = _project.GetDefaultCollectionOrBaselineReviewFolder(Helper.ArtifactStore.Address, author,
                BaselineAndCollectionTypePredefined.CollectionFolder);
            var collectionFolder = Helper.CreateAndPublishCollectionFolder(_project, author);

            var fakeBaseType = BaseArtifactType.PrimitiveFolder;
            var childArtifact = Helper.CreateWrapAndPublishNovaArtifact(_project, author, artifactType, collectionFolder.Id, baseType: fakeBaseType);

            childArtifact.Lock(author);

            INovaArtifactDetails movedArtifactDetails = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = ArtifactStore.MoveArtifact(Helper.BlueprintServer.Address, childArtifact, defaultCollectionFolder.Id, author);
            }, "'POST {0}' should return 200 OK when called with valid parameters!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, childArtifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, movedArtifactDetails);
            Assert.AreEqual(defaultCollectionFolder.Id, movedArtifactDetails.ParentId, "Parent Id of moved artifact is not the same as parent artifact Id");
        }

        [TestCase(BaseArtifactType.Actor, 5, 0, 2.5)]           // Move 1st artifact between 2nd & 3rd artifacts.
        [TestCase(BaseArtifactType.Document, 5, 2, 0.5)]        // Move 3rd artifact before first artifact.
        [TestCase(BaseArtifactType.Glossary, 5, 2, 5.0)]        // Move 3rd artifact after last artifact.
        [TestCase(BaseArtifactType.PrimitiveFolder, 5, 4, 2.5)] // Move last artifact between 2nd & 3rd artifacts.
        [TestCase(BaseArtifactType.Process, 5, 4, 1.0)]         // Move last artifact to same OrderIndex as first artifact.
        [TestCase(BaseArtifactType.UseCase, 5, 0, 1.0)]         // Move first artifact to same location.
        [TestRail(191038)]
        [Description("Create & publish several artifacts.  Move an artifact to the same location but specify an OrderIndex.  " +
            "Verify the OrderIndex of the artifact was updated.")]
        public void MoveArtifactWithOrderIndex_PublishedArtifact_InsideFolder_VerifyOrderIndexUpdated(
            BaseArtifactType artifactType, int numberOfArtifacts, int whichArtifact, double orderIndex)
        {
            // Setup:
            INovaArtifactDetails movedArtifactDetails = null;

            var parentFolder = Helper.CreateAndPublishArtifact(_project, _authorUser, artifactType);
            var artifacts = Helper.CreateAndPublishMultipleArtifacts(_project, _authorUser, artifactType, numberOfArtifacts, parentFolder);

            Artifact.Lock(artifacts[whichArtifact], Helper.ArtifactStore.Address, _authorUser);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = Helper.ArtifactStore.MoveArtifact(artifacts[whichArtifact], parentFolder, _authorUser, orderIndex);
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
            var artifacts = new List<IArtifactBase>();

            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _authorUser);

            var parentFolder = Helper.CreateAndPublishCollectionFolder(_project, _authorUser);
            var fakeBaseType = BaseArtifactType.PrimitiveFolder;

            for (int i = 0; i < numberOfArtifacts; ++i)
            {
                artifacts.Add(Helper.CreateWrapAndPublishNovaArtifact(_project, _authorUser, artifactType, parentFolder.Id, baseType: fakeBaseType));
            }

            Artifact.Lock(artifacts[whichArtifact], Helper.ArtifactStore.Address, _authorUser);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = Helper.ArtifactStore.MoveArtifact(artifacts[whichArtifact], parentFolder, _authorUser, orderIndex);
            }, "'POST {0}?orderIndex={1}' should return 200 OK when called with a valid token!", SVC_PATH, orderIndex);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, artifacts[whichArtifact].Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, movedArtifactDetails);
            Assert.AreEqual(parentFolder.Id, movedArtifactDetails.ParentId, "Parent Id of moved artifact should not be changed!");
            Assert.AreEqual(orderIndex, artifactDetails.OrderIndex, "The OrderIndex of the moved artifact is not the correct value!");
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(195994)]
        [Description("Create & publish an artifact and 2 folders.  Copy the artifact into the first folder, then move the copied artifact into the second folder.  " +
            "Verify the moved artifact is returned with the updated Parent ID.")]
        public void MoveArtifact_CopiedArtifact_ToNewFolder_ReturnsArtifactDetails(BaseArtifactType artifactType)
        {
            // Setup:
            var folder1 = Helper.CreateAndPublishArtifact(_project, _authorUser, BaseArtifactType.PrimitiveFolder);
            var folder2 = Helper.CreateAndPublishArtifact(_project, _authorUser, BaseArtifactType.PrimitiveFolder);
            var artifact = Helper.CreateAndPublishArtifact(_project, _authorUser, artifactType);

            var copyResult = Helper.ArtifactStore.CopyArtifact(artifact, folder1, _authorUser);
            var copiedArtifact = Helper.WrapNovaArtifact(copyResult.Artifact, _project, _authorUser);

            INovaArtifactDetails movedArtifactDetails = null;

            copiedArtifact.Lock();

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = Helper.ArtifactStore.MoveArtifact(copiedArtifact, folder2, _authorUser);
            }, "'POST {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, copiedArtifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, movedArtifactDetails);
            ArtifactStoreHelper.AssertArtifactsEqual(copyResult.Artifact, movedArtifactDetails, skipParentId: true);

            Assert.AreEqual(folder2.Id, movedArtifactDetails.ParentId, "Parent Id of moved artifact is not the same as folder2 Id!");
        }

        [TestCase(BaselineAndCollectionTypePredefined.BaselineFolder)]
        [TestCase(BaselineAndCollectionTypePredefined.ArtifactBaseline)]
        [TestRail(266500)]
        [Description("Create artifact of baseline/baseline folder type. Move this artifact to be a child of the baseline folder. " +
            "Verify the moved artifact is returned with the updated Parent ID.")]
        public void MoveArtifact_BaselineOrBaselineFolder_MovedToBaselineFolder_ReturnsMovedArtifact(ItemTypePredefined artifactType)
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _authorUser);

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var defaultBaselineFolder = _project.GetDefaultCollectionOrBaselineReviewFolder(Helper.ArtifactStore.Address,
                author, BaselineAndCollectionTypePredefined.BaselineFolder);

            string artifactName = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);
            var newBaselineFolder = ArtifactStore.CreateArtifact(Helper.ArtifactStore.Address, author,
                ItemTypePredefined.BaselineFolder, artifactName, _project, defaultBaselineFolder.Id);

            var childArtifact = ArtifactStore.CreateArtifact(Helper.ArtifactStore.Address, author, artifactType,
                artifactName, _project, defaultBaselineFolder.Id);

            INovaArtifactDetails movedArtifactDetails = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                movedArtifactDetails = ArtifactStore.MoveArtifact(Helper.ArtifactStore.Address, childArtifact.Id,
                    newBaselineFolder.Id, author);
            }, "'POST {0}' should return 200 OK when called with valid parameters!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, childArtifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, movedArtifactDetails);
            Assert.AreEqual(newBaselineFolder.Id, movedArtifactDetails.ParentId, "Parent Id of moved artifact is not the same as parent artifact Id");
        }

        #endregion 200 OK tests

        #region 400 Bad Request

        [TestCase(BaseArtifactType.Process)]
        [TestRail(190962)]
        [Description("Create & publish an artifact.  Move the artifact to be a child of itself. Verify returned code 400 Bad Request.")]
        public void MoveArtifact_PublishArtifactsAndMoveToItself_400BadRequest(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _authorUser, artifactType);

            artifact.Lock();

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => Helper.ArtifactStore.MoveArtifact(artifact, artifact, _authorUser),
                "'POST {0}' should return 400 Bad Request when artifact moved to itself", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.IncorrectInputParameters,
                "This move will result in a circular relationship between the artifact and its new parent.");
       }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(190963)]
        [Description("Create & save an artifact.  Move the artifact to be a child of itself. Verify returned code 400 Bad Request.")]
        public void MoveArtifact_SaveArtifactsAndMoveToItself_400BadRequest(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndSaveArtifact(_project, _authorUser, artifactType);

            artifact.Lock();

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => Helper.ArtifactStore.MoveArtifact(artifact, artifact, _authorUser),
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
            var artifact = Helper.CreateAndSaveArtifact(_project, _authorUser, BaseArtifactType.Process);

            artifact.Lock();

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => 
                ArtifactStore.MoveArtifact(Helper.ArtifactStore.Address, artifact, _project.Id, _authorUser, orderIndex),
                "'POST {0}?orderIndex={1}' should return 400 Bad Request for non-positive OrderIndex values", SVC_PATH, orderIndex);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.IncorrectInputParameters,
                "Parameter orderIndex cannot be equal to or less than 0.");
        }

        [TestCase(ItemTypePredefined.ArtifactCollection, -0.0001)]
        [TestCase(ItemTypePredefined.CollectionFolder, 0)]
        [TestRail(191036)]
        [Description("Create & save a Collection or Collection Folder artifact.  Move the artifact and specify an OrderIndex <= 0.  " +
            "Verify 400 Bad Request is returned.")]
        public void MoveArtifactWithOrderIndex_SavedCollectionOrCollectionFolder_NotPositiveOrderIndex_400BadRequest(
            ItemTypePredefined artifactType, double orderIndex)
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _authorUser);

            var collectionFolder = _project.GetDefaultCollectionOrBaselineReviewFolder(Helper.ArtifactStore.Address, _authorUser,
                BaselineAndCollectionTypePredefined.CollectionFolder);
            var fakeBaseType = BaseArtifactType.PrimitiveFolder;
            var artifact = Helper.CreateWrapAndSaveNovaArtifact(_project, _authorUser, artifactType, collectionFolder.Id, baseType: fakeBaseType);

            artifact.Lock();

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
                ArtifactStore.MoveArtifact(Helper.ArtifactStore.Address, artifact, collectionFolder.Id, _authorUser, orderIndex),
                "'POST {0}?orderIndex={1}' should return 400 Bad Request for non-positive OrderIndex values", SVC_PATH, orderIndex);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.IncorrectInputParameters,
                "Parameter orderIndex cannot be equal to or less than 0.");
        }

        #endregion 400 Bad Request

        #region 401 Unauthorized tests

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182380)]
        [Description("Create & publish two artifacts.  Move one artifact to be a child of the other with invalid token in a request.  Verify response returns code 401 Unauthorized.")]
        public void MoveArtifact_PublishedArtifactMoveToParentArtifactWithInvalidToken_401Unauthorized(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _authorUser, artifactType);
            var newParentArtifact = Helper.CreateAndPublishArtifact(_project, _authorUser, artifactType);

            var userWithBadToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            artifact.Lock();

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.ArtifactStore.MoveArtifact(artifact, newParentArtifact, userWithBadToken);
            }, "'POST {0}' should return 401 Unauthorized when called with a invalid token!", SVC_PATH);

            // Verify:
            const string expectedExceptionMessage = "Unauthorized call";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "{0} was not found in returned message of move published artifact(s) which has invalid token.", expectedExceptionMessage);
        }

        #endregion 401 Unauthorized tests

        #region 403 Forbidden tests

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182405)]
        [Description("Create & publish two artifacts.  Each one in different project.  Move the artifact to be a child of the other in different project.  " +
            "Verify returned code 403 Forbidden.")]
        public void MoveArtifact_PublishedArtifactMoveToBeAChildOfAnotherArtifactInDifferentProject_403Forbidden(BaseArtifactType artifactType)
        {
            // Setup:
            var projects = ProjectFactory.GetProjects(_adminUser, numberOfProjects: 2);
            Assert.GreaterOrEqual(projects.Count, 2, "This test requires at least 2 projects to exist!");

            var firstProject = projects[0];
            var secondProject = projects[1];

            var artifact1 = Helper.CreateAndPublishArtifact(firstProject, _adminUser, artifactType);
            var artifact2 = Helper.CreateAndPublishArtifact(secondProject, _adminUser, artifactType);

            artifact1.Lock();

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.MoveArtifact(artifact1, artifact2, _adminUser),
                "'POST {0}' should return 403 Forbidden when user tries to move artifact to different project", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot move artifact to a different project.");
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182462)]
        [Description("Create & publish two artifacts.  Each one in different project.  Move the artifact to be a child of the other in different project.  " +
            "Verify returned code 403 Forbidden.")]
        public void MoveArtifact_SavedArtifactMoveToBeAChildOfAnotherArtifactInDifferentProject_403Forbidden(BaseArtifactType artifactType)
        {
            // Setup:
            var projects = ProjectFactory.GetProjects(_adminUser, numberOfProjects: 2);
            Assert.GreaterOrEqual(projects.Count, 2, "This test requires at least 2 projects to exist!");

            var firstProject = projects[0];
            var secondProject = projects[1];

            var artifact1 = Helper.CreateAndSaveArtifact(firstProject, _adminUser, artifactType);
            var artifact2 = Helper.CreateAndPublishArtifact(secondProject, _adminUser, artifactType);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.MoveArtifact(artifact1, artifact2, _adminUser),
                "'POST {0}' should return 403 Forbidden when user tries to move artifact to different project", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot move artifact to a different project.");
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182394)]
        [Description("Create & publish an artifact.  Move the created artifact to be a parent of a folder of its descendents. Verify returned code 403 Forbidden.")]
        public void MoveArtifact_PublishFolderAndMoveToBeAChildOfArtifact_403Forbidden(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _authorUser, artifactType);
            var folder = Helper.CreateAndPublishArtifact(_project, _authorUser, BaseArtifactType.PrimitiveFolder);

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            folder.Lock(author);
            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.MoveArtifact(folder, artifact, author),
                "'POST {0}' should return 403 Forbidden when folder moved to regular artifact", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot move a folder artifact to non folder/project parent.");
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182464)]
        [Description("Create & save an artifact.  Move the created artifact to be a parent of a folder of its descendents. Verify returned code 403 Forbidden.")]
        public void MoveArtifact_SaveFolderAndMoveToBeAChildOfArtifact_403Forbidden(BaseArtifactType artifactType)
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var artifact = Helper.CreateAndPublishArtifact(_project, author, artifactType);
            var folder = Helper.CreateAndSaveArtifact(_project, author, BaseArtifactType.PrimitiveFolder);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.MoveArtifact(folder, artifact, author),
                "'POST {0}' should return 403 Forbidden when folder moved to regular artifact", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot move a folder artifact to non folder/project parent.");
        }

        [TestCase(BaselineAndCollectionTypePredefined.ArtifactCollection)]
        [TestCase(BaselineAndCollectionTypePredefined.CollectionFolder)]
        [TestRail(182408)]
        [Description("Create collection or collection folder. Move regular artifact to be a child of the collection or collection folder. Verify returned code 403 Forbidden.")]
        public void MoveArtifact_PublishedArtifact_MoveToCollectionOrCollectionFolder_403Forbidden(ItemTypePredefined artifactType)
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _authorUser);

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var collectionFolder = _project.GetDefaultCollectionOrBaselineReviewFolder(Helper.ArtifactStore.Address, author,
                BaselineAndCollectionTypePredefined.CollectionFolder);

            var fakeBaseType = BaseArtifactType.PrimitiveFolder;
            var parentArtifact = Helper.CreateWrapAndPublishNovaArtifact(_project, _authorUser, artifactType, collectionFolder.Id, baseType: fakeBaseType);

            var artifact = Helper.CreateAndPublishArtifact(_project, _authorUser, BaseArtifactType.Process);

            artifact.Lock(author);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => ArtifactStore.MoveArtifact(Helper.BlueprintServer.Address, artifact, parentArtifact.Id, author),
               "'POST {0}' should return 403 Forbidden when user tries to move regular artifact to a {1} artifact type", SVC_PATH, artifactType);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot move an artifact to non project section.");
        }

        [TestCase(BaselineAndCollectionTypePredefined.ArtifactCollection)]
        [TestCase(BaselineAndCollectionTypePredefined.CollectionFolder)]
        [TestRail(190010)]
        [Description("Create a collection or collection folder. Move collection or collection folder to be a child of the regular artifact. Verify returned code 403 Forbidden.")]
        public void MoveArtifact_CollectionOrCollectionFolder_MoveToRegularArtifact_403Forbidden(ItemTypePredefined artifactType)
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _authorUser);

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var collectionFolder = _project.GetDefaultCollectionOrBaselineReviewFolder(Helper.ArtifactStore.Address, author,
                BaselineAndCollectionTypePredefined.CollectionFolder);

            var fakeBaseType = BaseArtifactType.PrimitiveFolder;
            var childArtifact = Helper.CreateWrapAndPublishNovaArtifact(_project, author, artifactType, collectionFolder.Id, baseType: fakeBaseType);

            var parentArtifact = Helper.CreateAndPublishArtifact(_project, author, BaseArtifactType.Process);

            childArtifact.Lock(author);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => ArtifactStore.MoveArtifact(Helper.BlueprintServer.Address, childArtifact, parentArtifact.Id, author),
                   "'POST {0}' should return 403 Forbidden when user tries to move collection or collection folder to be a child of a regular artifact", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot move a collection artifact to non collection section.");
        }

        [TestCase(BaselineAndCollectionTypePredefined.ArtifactCollection)]
        [TestCase(BaselineAndCollectionTypePredefined.CollectionFolder)]
        [TestRail(191040)]
        [Description("Create a collection or collection folder. Move collection or collection folder to be a child of the collection artifact. Verify returned code 403 Forbidden.")]
        public void MoveArtifact_CollectionOrCollectionFolder_MoveToCollectionArtifact_403Forbidden(ItemTypePredefined artifactType)
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _authorUser);

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var collectionFolder = _project.GetDefaultCollectionOrBaselineReviewFolder(Helper.ArtifactStore.Address, author,
                BaselineAndCollectionTypePredefined.CollectionFolder);

            var fakeBaseType = BaseArtifactType.PrimitiveFolder;
            var collection = Helper.CreateWrapAndPublishNovaArtifact(_project, author, artifactType, collectionFolder.Id, baseType: fakeBaseType);

            var collectionArtifact = Helper.CreateAndPublishCollection(_project, author);

            collection.Lock(author);
             
            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => ArtifactStore.MoveArtifact(Helper.BlueprintServer.Address, collection, collectionArtifact.Id, author),
                   "'POST {0}' should return 403 Forbidden when user tries to move collection or collection folder to collection artifact", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot move a collection artifact to non folder parent.");
        }

        [TestCase(BaselineAndCollectionTypePredefined.BaselineFolder)]
        [TestCase(BaselineAndCollectionTypePredefined.ArtifactBaseline)]
        [TestRail(266503)]
        [Description("Create artifact of baseline/baseline folder type. Move this artifact to the project root. " +
            "Verify 403 and error message.")]
        public void MoveArtifact_BaselineOrBaselineFolder_MovedToProjectRoot_403Conflict(ItemTypePredefined artifactType)
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _authorUser);

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var defaultBaselineFolder = _project.GetDefaultCollectionOrBaselineReviewFolder(Helper.ArtifactStore.Address,
                author, BaselineAndCollectionTypePredefined.BaselineFolder);

            string artifactName = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);

            var childArtifact = ArtifactStore.CreateArtifact(Helper.ArtifactStore.Address, author, artifactType,
                artifactName, _project, defaultBaselineFolder.Id);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                ArtifactStore.MoveArtifact(Helper.ArtifactStore.Address, childArtifact.Id,
                    _project.Id, author);
            }, "Attempt to move Baseline or Baseline folder to Project root should return 409 Conflict.");

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden,
                "Cannot move a baseline artifact to non baseline section.");
        }

        [TestCase]
        [TestRail(266504)]
        [Description("Create and publish artifact. try to move this artifact to the default Baseline folder. " +
            "Verify 403 and error message.")]
        public void MoveArtifact_PublishedArtifact_MovedToDefaultBaselineFolder_403Conflict()
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _authorUser);

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
            var artifact = Helper.CreateAndPublishArtifact(_project, author, BaseArtifactType.Actor);
            artifact.Lock(author);

            var defaultBaselineFolder = _project.GetDefaultCollectionOrBaselineReviewFolder(Helper.ArtifactStore.Address,
                author, BaselineAndCollectionTypePredefined.BaselineFolder);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                ArtifactStore.MoveArtifact(Helper.ArtifactStore.Address, artifact.Id, defaultBaselineFolder.Id, author);
            }, "Attempt to move Baseline or Baseline folder to Project root should return 409 Conflict.");

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden,
                "Cannot move an artifact to non project section.");
        }

        [TestCase(BaselineAndCollectionTypePredefined.BaselineFolder)]
        [TestCase(BaselineAndCollectionTypePredefined.ArtifactBaseline)]
        [TestRail(266505)]
        [Description("Create and publish artifact. Try to move this artifact to the Baseline. " +
            "Verify 403 and error message.")]
        public void MoveArtifact_PublishedArtifact_MovedToBaseline_403Conflict(ItemTypePredefined artifactType)
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _authorUser);

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
            var artifact = Helper.CreateAndPublishArtifact(_project, author, BaseArtifactType.Actor);
            artifact.Lock(author);

            var defaultBaselineFolder = _project.GetDefaultCollectionOrBaselineReviewFolder(Helper.ArtifactStore.Address,
                author, BaselineAndCollectionTypePredefined.BaselineFolder);
            string baselineArtifactName = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);

            var baselineArtifact = ArtifactStore.CreateArtifact(Helper.ArtifactStore.Address, author, artifactType,
                baselineArtifactName, _project, defaultBaselineFolder.Id);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                ArtifactStore.MoveArtifact(Helper.ArtifactStore.Address, artifact.Id, baselineArtifact.Id, author);
            }, "Attempt to move artifact to Baseline or Baseline folder should return 409 Conflict.");

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden,
                "Cannot move an artifact to non project section.");
        }

        [TestCase(BaselineAndCollectionTypePredefined.BaselineFolder)]
        [TestCase(BaselineAndCollectionTypePredefined.ArtifactBaseline)]
        [TestRail(266507)]
        [Description("Create artifact of baseline/baseline folder type. Move this artifact to the default Collection folder. " +
            "Verify 403 and error message.")]
        public void MoveArtifact_BaselineOrBaselineFolder_MovedToDefaultCollectionFolder_403Conflict(ItemTypePredefined artifactType)
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _authorUser);

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var defaultBaselineFolder = _project.GetDefaultCollectionOrBaselineReviewFolder(Helper.ArtifactStore.Address,
                author, BaselineAndCollectionTypePredefined.BaselineFolder);
            var defaultCollectionFolder = _project.GetDefaultCollectionOrBaselineReviewFolder(Helper.ArtifactStore.Address,
                author, BaselineAndCollectionTypePredefined.CollectionFolder);

            string artifactName = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);

            var childArtifact = ArtifactStore.CreateArtifact(Helper.ArtifactStore.Address, author, artifactType,
                artifactName, _project, defaultBaselineFolder.Id);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                ArtifactStore.MoveArtifact(Helper.ArtifactStore.Address, childArtifact.Id,
                    defaultCollectionFolder.Id, author);
            }, "Attempt to move Baseline or Baseline folder to the Default Collection folder should return 409 Conflict.");

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden,
                "Cannot move a baseline artifact to non baseline section.");
        }

        [TestCase(BaselineAndCollectionTypePredefined.CollectionFolder)]
        [TestCase(BaselineAndCollectionTypePredefined.ArtifactCollection)]
        [TestRail(266508)]
        [Description("Create artifact of baseline/baseline folder type. Move this artifact to the default Collection folder. " +
            "Verify 403 and error message.")]
        public void MoveArtifact_CollectionOrCollectionFolder_MovedToDefaultBaselineFolder_403Conflict(ItemTypePredefined artifactType)
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _authorUser);

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var defaultBaselineFolder = _project.GetDefaultCollectionOrBaselineReviewFolder(Helper.ArtifactStore.Address,
                author, BaselineAndCollectionTypePredefined.BaselineFolder);
            var defaultCollectionFolder = _project.GetDefaultCollectionOrBaselineReviewFolder(Helper.ArtifactStore.Address,
                author, BaselineAndCollectionTypePredefined.CollectionFolder);

            string artifactName = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);

            var childArtifact = ArtifactStore.CreateArtifact(Helper.ArtifactStore.Address, author, artifactType,
                artifactName, _project, defaultCollectionFolder.Id);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                ArtifactStore.MoveArtifact(Helper.ArtifactStore.Address, childArtifact.Id,
                    defaultBaselineFolder.Id, author);
            }, "Attempt to move Baseline or Baseline folder to the Default Collection folder should return 409 Conflict.");

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden,
                "Cannot move a collection artifact to non collection section.");
        }

        #endregion 403 Forbidden tests

        #region 404 Not Found tests

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182482)]
        [Description("Create & publish an artifact. Move an artifact to be a child of the artifact with Id 0.  Verify returned code 404 Not Found.")]
        public void MoveArtifact_PublishedArtifactCannotBeMovedToArtifactWithId0_404NotFound(BaseArtifactType artifactType)
        {
            const int ARTIFACT_WITH_ID_0 = 0;
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _authorUser, artifactType);

            artifact.Lock();

            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() => ArtifactStore.MoveArtifact(Helper.BlueprintServer.Address, artifact, ARTIFACT_WITH_ID_0, _authorUser),
                "'POST {0}' should return 404 Not Found when user tries to move artifact to one that has Id 0", SVC_PATH);

            // NOTE: No ServiceErrorMessage JSON is returned, so this just returns the generic 404 page from IIS.
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182403)]
        [Description("Create & save an artifact. Move an artifact to be a child of the artifact with Id 0.  Verify returned code 404 Not Found.")]
        public void MoveArtifact_SavedArtifactCannotBeMovedToArtifactWithId0_404NotFound(BaseArtifactType artifactType)
        {
            const int ARTIFACT_WITH_ID_0 = 0;
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _authorUser, artifactType);

            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() => ArtifactStore.MoveArtifact(Helper.BlueprintServer.Address, artifact, ARTIFACT_WITH_ID_0, _authorUser),
                "'POST {0}' should return 404 Not Found when user tries to move artifact to one that has Id 0", SVC_PATH);

            // NOTE: No ServiceErrorMessage JSON is returned, so this just returns the generic 404 page from IIS.
        }

        [TestCase(BaseArtifactType.Process, int.MaxValue)]
        [TestRail(182429)]
        [Description("Create & publish an artifact. Move an artifact to be a child of the non existing artifact.  Verify returned code 404 Not Found.")]
        public void MoveArtifact_PublishedArtifactCannotBeMovedToNonExistingArtifact_404NotFound(BaseArtifactType artifactType, int artifactId)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _authorUser, artifactType);

            artifact.Lock();

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => ArtifactStore.MoveArtifact(Helper.BlueprintServer.Address, artifact, artifactId, _authorUser),
                "'POST {0}' should return 404 Not Found when user tries to move artifact to one that does not exist", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound,
                "You have attempted to access an artifact that does not exist or has been deleted.");
        }

        [TestCase(BaseArtifactType.Process, int.MaxValue)]
        [TestRail(182470)]
        [Description("Create & save an artifact. Move an artifact to be a child of the non existing artifact.  Verify returned code 404 Not Found.")]
        public void MoveArtifact_SavedArtifactCannotBeMovedToNonExistingArtifact_404NotFound(BaseArtifactType artifactType, int artifactId)
        {
            // Setup:
            var artifact = Helper.CreateAndSaveArtifact(_project, _authorUser, artifactType);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => ArtifactStore.MoveArtifact(Helper.BlueprintServer.Address, artifact, artifactId, _authorUser),
                "'POST {0}' should return 404 Not Found when user tries to move artifact to one that does not exist", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound,
                "You have attempted to access an artifact that does not exist or has been deleted.");
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182432)]
        [Description("Create & publish two artifacts.  Delete 2nd one.  Move first artifact to be a child of second artifact.  Verify returned code 404 Not Found.")]
        public void MoveArtifact_PublishedArtifactCannotBeMovedToDeletedArtifact_404NotFound(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact1 = Helper.CreateAndPublishArtifact(_project, _authorUser, artifactType);
            var artifact2 = Helper.CreateAndPublishArtifact(_project, _authorUser, artifactType);

            artifact2.Delete();
            artifact2.Publish();
            artifact1.Lock();

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.MoveArtifact(artifact1, artifact2, _authorUser),
                "'POST {0}' should return 404 Not Found when user tries to move artifact to one that does not exist", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound,
                "You have attempted to access an artifact that does not exist or has been deleted.");
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182479)]
        [Description("Create & save an artifact.  Delete second one.  Move first artifact to be a child of second artifact.  Verify returned code 404 Not Found.")]
        public void MoveArtifact_SavedArtifactCannotBeMovedToDeletedArtifact_404NotFound(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact1 = Helper.CreateAndSaveArtifact(_project, _authorUser, artifactType);
            var artifact2 = Helper.CreateAndPublishArtifact(_project, _authorUser, artifactType);

            artifact2.Delete();
            artifact2.Publish();

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.MoveArtifact(artifact1, artifact2, _authorUser),
                "'POST {0}' should return 404 Not Found when user tries to move artifact to one that does not exist", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound,
                "You have attempted to access an artifact that does not exist or has been deleted.");
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182480)]
        [Description("Create & publish two artifacts.  Delete first one.  Move first artifact to be a child of second artifact.  Verify returned code 404 Not Found.")]
        public void MoveArtifact_DeletedArtifactCannotBeMovedToAnotherArtifact_404NotFound(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact1 = Helper.CreateAndPublishArtifact(_project, _authorUser, artifactType);
            var artifact2 = Helper.CreateAndPublishArtifact(_project, _authorUser, artifactType);

            artifact1.Delete();

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.MoveArtifact(artifact1, artifact2, _authorUser),
                "'POST {0}' should return 404 Not Found when user tries to move artifact to one that does not exist", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound,
                "You have attempted to access an artifact that does not exist or has been deleted.");
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182416)]
        [Description("Create & publish two artifacts.  Move an artifact to be a child of the other one with user that does not have proper permissions " +
            "to future child artifact.  Verify returned code 404 Not Found.")]
        public void MoveArtifact_PublishedArtifactCannotBeMovedForUserWithoutProperPermissions_404NotFound(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact1 = Helper.CreateAndPublishArtifact(_project, _authorUser, artifactType);
            var artifact2 = Helper.CreateAndPublishArtifact(_project, _authorUser, artifactType);

            var userWithoutPermissions = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            artifact1.Lock(userWithoutPermissions);

            Helper.AssignProjectRolePermissionsToUser(userWithoutPermissions, TestHelper.ProjectRole.None, _project, artifact1);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.MoveArtifact(artifact1, artifact2, userWithoutPermissions),
                "'POST {0}' should return 404 Not Found when user tries to move artifact without proper permissions", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound,
                "You have attempted to access an artifact that does not exist or has been deleted.");
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182463)]
        [Description("Create & save an artifact.  Move an artifact to be a child of the other one with user that does not have proper permissions to future parent artifact.  " +
            "Verify returned code 404 Not Found.")]
        public void MoveArtifact_SavedArtifactCannotBeMovedForUserWithoutProperPermissions_404NotFound(BaseArtifactType artifactType)
        {
            // Setup:
            var userWithoutPermissions = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var artifact1 = Helper.CreateAndSaveArtifact(_project, userWithoutPermissions, artifactType);
            var artifact2 = Helper.CreateAndSaveArtifact(_project, userWithoutPermissions, artifactType);

            Helper.AssignProjectRolePermissionsToUser(userWithoutPermissions, TestHelper.ProjectRole.None, _project, artifact2);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.MoveArtifact(artifact1, artifact2, userWithoutPermissions),
                "'POST {0}' should return 404 Not found when user tries to move artifact without proper permissions", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound,
                "You have attempted to access an artifact that does not exist or has been deleted.");
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182424)]
        [Description("Create & publish two artifacts.  Move an artifact to be a child of the other one to which user does not have proper permissions.  " +
            "Verify returned code 404 Not Found.")]
        public void MoveArtifact_PublishedArtifactCannotBeMovedToArtifactWhichUserDoesNotHaveProperPermissions_404NotFound(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact1 = Helper.CreateAndPublishArtifact(_project, _authorUser, artifactType);
            var artifact2 = Helper.CreateAndPublishArtifact(_project, _authorUser, artifactType);

            // Create a user without permission to the artifact.
            var userWithoutPermissions = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            artifact1.Lock(userWithoutPermissions);

            Helper.AssignProjectRolePermissionsToUser(userWithoutPermissions, TestHelper.ProjectRole.None, _project, artifact2);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.MoveArtifact(artifact1, artifact2, userWithoutPermissions),
                "'POST {0}' should return 404 Not Found when user tries to move artifact to an artifact to which user has no permissions", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound,
                "You have attempted to access an artifact that does not exist or has been deleted.");
        }

        #endregion 404 Not Found tests

        #region 409 Conflict tests

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182401)]
        [Description("Create & publish two artifacts.  Do not put lock on artifact that would be moved.  Move the artifact to be a child of the other.  " +
            "Verify returned code 409 Conflict.")]
        public void MoveArtifact_PublishArtifactAndMoveToBeAChildOfAnotherArtifact_DoNotSetLock_409Conflict(BaseArtifactType artifactType)
        {
            // Setup:
            var parentArtifact = Helper.CreateAndPublishArtifact(_project, _authorUser, artifactType);
            var childArtifact = Helper.CreateAndPublishArtifact(_project, _authorUser, artifactType);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => Helper.ArtifactStore.MoveArtifact(childArtifact, parentArtifact, _authorUser),
                "'POST {0}' should return 409 Conflict when user moves an unlocked artifact to be a child of another artifact.", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.LockedByOtherUser,
                "Cannot move an artifact that has not been locked.");
        }

        [TestCase(BaseArtifactType.Process, 2)]
        [TestCase(BaseArtifactType.Process, 3)]
        [TestRail(182406)]
        [Description("Create & publish number of artifacts.  Move the first created artifact to be a child of one of its descendents.  " +
            "If one created it will be circular to itself.  Verify returned code 409 Conflict.")]
        public void MoveArtifact_PublishArtifactsAndCreateCircularDependency_409Conflict(BaseArtifactType artifactType, int numberOfArtifacts)
        {
            // Setup:
            var artifactTypes = CreateListOfArtifactTypes(numberOfArtifacts, artifactType);

            var artifactList = Helper.CreatePublishedArtifactChain(_project, _authorUser, artifactTypes.ToArray());

            Assert.IsNotNull(artifactList, "Artifact List is not created");

            var firstArtifact = artifactList.First();
            var lastArtifact = artifactList.Last();

            firstArtifact.Lock();

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => Helper.ArtifactStore.MoveArtifact(firstArtifact, lastArtifact, _authorUser),
                "'POST {0}' should return 409 Conflict when artifact moved to one of its descendents", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CycleRelationship,
                "This move will result in a circular relationship between the artifact and its new parent.");
        }

        [TestCase(BaseArtifactType.Process, 2)]
        [TestCase(BaseArtifactType.Process, 3)]
        [TestRail(182483)]
        [Description("Publish artifact chain.  Save and move the first created artifact to be a child of one of its descendents.  " +
            "If one created it will be circular to itself.  Verify returned code 409 Conflict.")]
        public void MoveArtifact_SaveArtifactAndCreateCircularDependency_409Conflict(BaseArtifactType artifactType, int numberOfArtifacts)
        {
            // Setup:
            var artifactTypes = CreateListOfArtifactTypes(numberOfArtifacts, artifactType);

            var artifactList = Helper.CreatePublishedArtifactChain(_project, _authorUser, artifactTypes.ToArray());

            Assert.IsNotNull(artifactList, "Artifact List is not created");

            var firstArtifact = artifactList.First();
            var lastArtifact = artifactList.Last();

            firstArtifact.Save(_authorUser);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => Helper.ArtifactStore.MoveArtifact(firstArtifact, lastArtifact, _authorUser),
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
        private static List<BaseArtifactType> CreateListOfArtifactTypes(int numberOfArtifacts, BaseArtifactType artifactType)
        {
            var artifactTypes = new List<BaseArtifactType>();

            for (int i = 0; i < numberOfArtifacts; i++)
            {
                artifactTypes.Add(artifactType);
            }

            return artifactTypes;
        }

        #endregion private call
    }
}
