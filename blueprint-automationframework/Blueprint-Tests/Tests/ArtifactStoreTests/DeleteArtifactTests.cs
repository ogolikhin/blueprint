using System.Collections.Generic;
using System.Linq;
using Common;
using CustomAttributes;
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
    public class DeleteArtifactTests : TestBase
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

        #region 200 OK tests

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(165741)]
        [Description("Create & publish an artifact.  Delete the artifact - it should return 200 OK and the deleted artifact." +
            "Try to get the artifact and verify you get a 404 since it was deleted.")]
        public void DeleteArtifact_PublishedArtifactWithNoChildren_ArtifactIsDeleted(BaseArtifactType artifactType)
        {
            // Setup:
            IUser authorUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, authorUser, artifactType);

            // Execute:
            List<INovaArtifactResponse> deletedArtifacts = null;

            Assert.DoesNotThrow(() => deletedArtifacts = Helper.ArtifactStore.DeleteArtifact(artifact, authorUser),
                "'DELETE {0}' should return 200 OK if a valid artifact ID is sent!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            // Verify:
            Assert.AreEqual(1, deletedArtifacts.Count, "There should only be 1 deleted artifact returned!");
            Assert.AreEqual(artifact.Id, deletedArtifacts[0].Id, "The artifact ID doesn't match the one that we deleted!");

            VerifyArtifactIsDeleted(artifact, authorUser);
        }

        [TestCase(ItemTypePredefined.ArtifactCollection)]
        [TestCase(ItemTypePredefined.CollectionFolder)]
        [TestRail(185237)]
        [Description("Create & publish a collection or collection folder.  Delete the artifact - it should return 200 OK and the deleted artifact." +
            "Try to get the artifact and verify you get a 404 since it was deleted.")]
        public void DeleteArtifact_PublishedCollectionOrCollectionFolderWithNoChildren_ArtifactIsDeleted(ItemTypePredefined artifactType)
        {
            // Setup:
            IUser authorUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, authorUser);

            var collectionFolder = _project.GetDefaultCollectionFolder(Helper.ArtifactStore.Address, authorUser);
            var fakeBaseType = BaseArtifactType.PrimitiveFolder;
            IArtifact artifact = Helper.CreateAndWrapNovaArtifact(_project, authorUser, artifactType, collectionFolder.Id, baseType: fakeBaseType);
            artifact.Publish();

            // Execute:
            List<INovaArtifactResponse> deletedArtifacts = null;

            Assert.DoesNotThrow(() => deletedArtifacts = Helper.ArtifactStore.DeleteArtifact(artifact, authorUser),
                "'DELETE {0}' should return 200 OK if a valid {1} ID is sent!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_, artifactType);

            // Verify:
            artifact.IsDeleted = true;
            Assert.AreEqual(1, deletedArtifacts.Count, "There should only be 1 deleted artifact returned!");
            Assert.AreEqual(artifact.Id, deletedArtifacts[0].Id, "The artifact ID doesn't match the one that we deleted!");

            VerifyArtifactIsDeleted(artifact, authorUser);
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(165747)]
        [Description("Create & save an artifact.  Delete the artifact - it should return 200 OK and the deleted artifact." +
            "Try to get the artifact and verify you get a 404 since it was deleted.")]
        public void DeleteArtifact_SavedArtifactWithNoChildren_ArtifactIsDeleted(BaseArtifactType artifactType)
        {
            // Setup:
            IUser authorUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, authorUser, artifactType);

            // Execute:
            List<INovaArtifactResponse> deletedArtifacts = null;

            Assert.DoesNotThrow(() => deletedArtifacts = Helper.ArtifactStore.DeleteArtifact(artifact, authorUser),
                "'DELETE {0}' should return 200 OK if a valid artifact ID is sent!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            // Verify:
            Assert.AreEqual(1, deletedArtifacts.Count, "There should only be 1 deleted artifact returned!");
            Assert.AreEqual(artifact.Id, deletedArtifacts[0].Id, "The artifact ID doesn't match the one that we deleted!");

            VerifyArtifactIsDeleted(artifact);
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(165748)]
        [Description("Create & publish an artifact, then save a draft.  Delete the artifact - it should return 200 OK and the deleted artifact." +
            "Try to get the artifact and verify you get a 404 since it was deleted.")]
        public void DeleteArtifact_PublishedWithDraftArtifactWithNoChildren_ArtifactIsDeleted(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            artifact.Save();

            // Execute:
            List<INovaArtifactResponse> deletedArtifacts = null;

            Assert.DoesNotThrow(() => deletedArtifacts = Helper.ArtifactStore.DeleteArtifact(artifact, _user),
                "'DELETE {0}' should return 200 OK if a valid artifact ID is sent!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            // Verify:
            Assert.AreEqual(1, deletedArtifacts.Count, "There should only be 1 deleted artifact returned!");
            Assert.AreEqual(artifact.Id, deletedArtifacts[0].Id, "The artifact ID doesn't match the one that we deleted!");

            VerifyArtifactIsDeleted(artifact);
        }

        [TestCase(0, BaseArtifactType.Actor, BaseArtifactType.Glossary, BaseArtifactType.Process)]
        [TestCase(1, BaseArtifactType.Actor, BaseArtifactType.Glossary, BaseArtifactType.Process)]
        [TestCase(2, BaseArtifactType.Actor, BaseArtifactType.Glossary, BaseArtifactType.Process)]
        [TestRail(165798)]
        [Description("Create & publish an artifact with a child & grandchild.  Delete one of the artifacts in the chain - it should return 200 OK and the deleted artifact and its children." +
            "Try to get the artifact & its children and verify you get a 404 since it was deleted.")]
        public void DeleteArtifact_PublishedArtifactWithChildren_ArtifactIsDeleted(int indexToDelete, params BaseArtifactType[] artifactTypeChain)
        {
            ThrowIf.ArgumentNull(artifactTypeChain, nameof(artifactTypeChain));

            // Setup:
            var artifactChain = Helper.CreatePublishedArtifactChain(_project, _user, artifactTypeChain);

            // Execute:
            List<INovaArtifactResponse> deletedArtifacts = null;

            Assert.DoesNotThrow(() => deletedArtifacts = Helper.ArtifactStore.DeleteArtifact(artifactChain[indexToDelete], _user),
                "'DELETE {0}' should return 200 OK if a valid artifact ID is sent!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            // Verify:
            int expectedArtifactCount = artifactChain.Count - indexToDelete;
            Assert.AreEqual(expectedArtifactCount, deletedArtifacts.Count, "There should be {0} deleted artifact returned!", expectedArtifactCount);

            VerifyDeletedArtifactAndChildrenWereReturned(artifactChain, deletedArtifacts, indexToDelete);
            VerifyArtifactsAreDeleted(artifactChain, indexToDelete);
        }

        [TestCase(0, BaseArtifactType.Actor, BaseArtifactType.Glossary, BaseArtifactType.Process)]
        [TestCase(1, BaseArtifactType.Actor, BaseArtifactType.Glossary, BaseArtifactType.Process)]
        [TestCase(2, BaseArtifactType.Actor, BaseArtifactType.Glossary, BaseArtifactType.Process)]
        [TestRail(165799)]
        [Description("Create & save an artifact with a child & grandchild.  Delete one of the artifacts in the chain - it should return 200 OK and the deleted artifact and its children." +
            "Try to get the artifact & its children and verify you get a 404 since it was deleted.")]
        public void DeleteArtifact_SavedArtifactWithChildren_ArtifactIsDeleted(int indexToDelete, params BaseArtifactType[] artifactTypeChain)
        {
            ThrowIf.ArgumentNull(artifactTypeChain, nameof(artifactTypeChain));

            // Setup:
            var artifactChain = Helper.CreateSavedArtifactChain(_project, _user, artifactTypeChain);

            // Execute:
            List<INovaArtifactResponse> deletedArtifacts = null;

            Assert.DoesNotThrow(() => deletedArtifacts = Helper.ArtifactStore.DeleteArtifact(artifactChain[indexToDelete], _user),
                "'DELETE {0}' should return 200 OK if a valid artifact ID is sent!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            // Verify:
            int expectedArtifactCount = artifactChain.Count - indexToDelete;
            Assert.AreEqual(expectedArtifactCount, deletedArtifacts.Count, "There should be {0} deleted artifact returned!", expectedArtifactCount);

            VerifyDeletedArtifactAndChildrenWereReturned(artifactChain, deletedArtifacts, indexToDelete);
            VerifyArtifactsAreDeleted(artifactChain, indexToDelete);
        }

        [TestCase(BaseArtifactType.Actor, BaseArtifactType.Process)]
        [TestRail(165809)]
        [Description("Create & publish a parent artifact and create a child artifact that is only saved but not published.  Delete the parent artifact.  " +
            "Verify it returns 200 OK with the deleted artifact and its children.  Try to get the artifact & its children and verify you get a 404 since they were deleted.")]
        public void DeleteArtifact_PublishedArtifactWithSavedChild_ArtifactIsDeleted(BaseArtifactType parentArtifactType, BaseArtifactType childArtifactType)
        {
            // Setup:
            var artifactChain = new List<IArtifact>();

            artifactChain.Add(Helper.CreateAndPublishArtifact(_project, _user, parentArtifactType));
            artifactChain.Add(Helper.CreateAndSaveArtifact(_project, _user, childArtifactType, artifactChain.First()));
            
            // Execute:
            List<INovaArtifactResponse> deletedArtifacts = null;
            
            Assert.DoesNotThrow(() => deletedArtifacts = Helper.ArtifactStore.DeleteArtifact(artifactChain.First(), _user),
                "'DELETE {0}' should return 200 OK if a valid artifact ID is sent!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);
                
            // Verify:
            int expectedArtifactCount = artifactChain.Count;
            Assert.AreEqual(expectedArtifactCount, deletedArtifacts.Count, "There should be {0} deleted artifact returned!", expectedArtifactCount);

            VerifyDeletedArtifactAndChildrenWereReturned(artifactChain, deletedArtifacts);
            VerifyArtifactsAreDeleted(artifactChain);
        }

        [TestCase(BaseArtifactType.Actor, BaseArtifactType.Process)]
        [TestRail(165810)]
        [Description("Create & publish parent & child artifacts then modify & save the child artifact.  Delete the parent artifact.  " +
            "Verify it returns 200 OK with the deleted artifact and its children.  Try to get the artifact & its children and verify you get a 404 since they were deleted.")]
        public void DeleteArtifact_PublishedArtifactWithPublishedChildWithDraft_ArtifactIsDeleted(BaseArtifactType parentArtifactType, BaseArtifactType childArtifactType)
        {
            // Setup:
            var artifactChain = new List<IArtifact>();

            artifactChain.Add(Helper.CreateAndPublishArtifact(_project, _user, parentArtifactType));
            var savedArtifact = Helper.CreateAndPublishArtifact(_project, _user, childArtifactType, artifactChain.First());
            savedArtifact.Save();
            artifactChain.Add(savedArtifact);

            // Execute:
            List<INovaArtifactResponse> deletedArtifacts = null;

            Assert.DoesNotThrow(() => deletedArtifacts = Helper.ArtifactStore.DeleteArtifact(artifactChain.First(), _user),
                "'DELETE {0}' should return 200 OK if a valid artifact ID is sent!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            // Verify:
            int expectedArtifactCount = artifactChain.Count;
            Assert.AreEqual(expectedArtifactCount, deletedArtifacts.Count, "There should be {0} deleted artifact returned!", expectedArtifactCount);

            VerifyDeletedArtifactAndChildrenWereReturned(artifactChain, deletedArtifacts);
            VerifyArtifactsAreDeleted(artifactChain);
        }

        [TestCase]
        [TestRail(165821)]
        [Description("Create & publish 2 artifacts and add a manual trace between them.  Delete the first artifact - it should return 200 OK and the deleted artifact." +
            "Try to get the artifact and verify you get a 404 since it was deleted.  Get the relationships of the second artifact and verify there are no traces.")]
        public void DeleteArtifact_PublishedArtifactWithManualTrace_ArtifactAndTraceIsDeleted()
        {
            // Setup:
            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase);

            OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact,
                targetArtifact, TraceDirection.From, _user);

            // Make sure trace was created properly.
            VerifyArtifactHasExpectedNumberOfTraces(sourceArtifact, expectedManualTraces: 1, expectedOtherTraces: 0);

            // Execute:
            List<INovaArtifactResponse> deletedArtifacts = null;

            Assert.DoesNotThrow(() => deletedArtifacts = Helper.ArtifactStore.DeleteArtifact(targetArtifact, _user),
                "'DELETE {0}' should return 200 OK if a valid artifact ID is sent!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            // Verify:
            VerifyArtifactIsDeleted(targetArtifact);
            VerifyArtifactHasExpectedNumberOfTraces(sourceArtifact, expectedManualTraces: 0, expectedOtherTraces: 0);
        }

        [TestCase]
        [TestRail(165822)]
        [Description("Create & save 2 artifacts and add a manual trace between them.  Delete the first artifact - it should return 200 OK and the deleted artifact." +
            "Try to get the artifact and verify you get a 404 since it was deleted.  Get the relationships of the second artifact and verify there are no traces.")]
        public void DeleteArtifact_SavedArtifactWithManualTrace_ArtifactAndTraceIsDeleted()
        {
            // Setup:
            IArtifact sourceArtifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Actor);
            IArtifact targetArtifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.UseCase);

            OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact,
                targetArtifact, TraceDirection.From, _user);

            // Make sure trace was created properly.
            VerifyArtifactHasExpectedNumberOfTraces(sourceArtifact, expectedManualTraces: 1, expectedOtherTraces: 0);

            // Execute:
            List<INovaArtifactResponse> deletedArtifacts = null;

            Assert.DoesNotThrow(() => deletedArtifacts = Helper.ArtifactStore.DeleteArtifact(targetArtifact, _user),
                "'DELETE {0}' should return 200 OK if a valid artifact ID is sent!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            // Verify:
            VerifyArtifactIsDeleted(targetArtifact);
            VerifyArtifactHasExpectedNumberOfTraces(sourceArtifact, expectedManualTraces: 0, expectedOtherTraces: 0);
        }

        [Explicit(IgnoreReasons.ProductBug)]    // TFS Bug:  3208  Gets a 500 error when deleting the parent.
        [TestCase(BaseArtifactType.Actor, BaseArtifactType.Process)]
        [TestRail(190725)]
        [Description("Create & publish a grand-parent, parent & child artifact then move the parent to be under the project.  " +
            "Delete the parent artifact and publish all 3 artifacts.  Now delete the parent.  Verify it returns 200 OK with the deleted artifact and child.  " +
            "Try to get the artifact & its child and verify you get a 404 since they were deleted.")]
        public void DeleteArtifact_GrandParentAndParentAndChild_ParentIsMovedUnderProject_GrandParentIsDeletedAndPublished_DeleteParent_ArtifactIsDeleted(
            BaseArtifactType parentArtifactType, BaseArtifactType childArtifactType)
        {
            // Setup:
            var artifactChain = new List<IArtifact>();

            var grandParentArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.PrimitiveFolder);
            artifactChain.Add(grandParentArtifact);
            var parentArtifact = Helper.CreateAndPublishArtifact(_project, _user, parentArtifactType, grandParentArtifact);
            artifactChain.Add(parentArtifact);
            artifactChain.Add(Helper.CreateAndPublishArtifact(_project, _user, childArtifactType, parentArtifact));

            // Move parent under project.
            parentArtifact.Lock();
            ArtifactStore.MoveArtifact(Helper.ArtifactStore.Address, parentArtifact, _project.Id, _user);

            // Execute:
            List <INovaArtifactResponse> deletedArtifacts = null;

            Assert.DoesNotThrow(() => deletedArtifacts = Helper.ArtifactStore.DeleteArtifact(grandParentArtifact, _user),
                "'DELETE {0}' should return 200 OK if a valid artifact ID is sent!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            Helper.ArtifactStore.PublishArtifacts(artifactChain.ConvertAll(a => (IArtifactBase) a), _user);

            Assert.DoesNotThrow(() => deletedArtifacts = Helper.ArtifactStore.DeleteArtifact(parentArtifact, _user),
                "'DELETE {0}' should return 200 OK if a valid artifact ID is sent!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);
            
            // Verify:
            int expectedArtifactCount = artifactChain.Count;
            Assert.AreEqual(expectedArtifactCount, deletedArtifacts.Count, "There should be {0} deleted artifact returned!", expectedArtifactCount);

            var parentAndChild = artifactChain.GetRange(1, 2);
            VerifyDeletedArtifactAndChildrenWereReturned(parentAndChild, deletedArtifacts);
            VerifyArtifactsAreDeleted(parentAndChild);
            
        }

        #endregion 200 OK tests

        #region 401 Unauthorized tests

        [TestRail(165823)]
        [TestCase(BaseArtifactType.Actor)]
        [Description("Create an artifact and publish. Attempt to delete the artifact with a user that does not have authorization " +
                     "to delete. Verify that HTTP 401 Unauthorized exception is thrown.")]
        public void DeleteArtifact_UserDoesNotHaveAuthorizationToDelete_401Unauthorized(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            IUser userWithBadToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            // Execute & Verify:
            Assert.Throws<Http401UnauthorizedException>(() => Helper.ArtifactStore.DeleteArtifact(artifact, userWithBadToken),
                "We should get a 401 Unauthorized when a user trying to delete an artifact does not have authorization to delete!");
        }

        [TestRail(165843)]
        [TestCase(BaseArtifactType.Actor)]
        [Description("Create an artifact and publish. Attempt to delete the artifact with a missing token header. " +
                     "Verify that HTTP 401 Unauthorized exception is thrown.")]
        public void DeleteArtifact_MissingTokenHeader_401Unauthorized(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            // Execute & Verify:
            Assert.Throws<Http401UnauthorizedException>(() => Helper.ArtifactStore.DeleteArtifact(artifact, null),
                "We should get a 401 Unauthorized when the token header is missing when trying to delete!");
        }

        #endregion 401 Unauthorized tests

        #region 403 Forbidden tests

        [TestRail(165817)]
        [TestCase(BaseArtifactType.Actor)]
        [Description("Create and publish an artifact. Attempt to delete the artifact with a user that does not have permission " +
                     "to delete. Verify that HTTP 403 Forbidden exception is thrown.")]
        public void DeleteArtifact_UserDoesNotHavePermissionToDelete_403Forbidden(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            // Create a user without permission to delete the artifact.
            // NOTE: The difference between AuthorFullAccess & Author is that Author doesn't have delete permission.
            IUser userWithoutDeletePermission = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
            Helper.AssignProjectRolePermissionsToUser(userWithoutDeletePermission, TestHelper.ProjectRole.Author, _project, artifact);

            // Execute & Verify:
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.DeleteArtifact(artifact, userWithoutDeletePermission),
                "We should get a 403 Fordbidden when a user trying to delete an artifact does not have permission to delete!");

            // Verify:
            string expectedMessage = I18NHelper.FormatInvariant("You do not have permission to delete the artifact (ID: {0})", artifact.Id);
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, expectedMessage);
        }

        [TestRail(165844)]
        [TestCase(BaseArtifactType.Process, BaseArtifactType.Actor)]
        [Description("User attempts to delete a parent artifact when they do not have permission to delete the child. " +
             "Verify 403 Forbidden is thrown.")]
        public void DeleteArtifact_UserTriesToDeleteParentArtifactWithoutPermissionToDeleteChildArtifact_403Forbidden(
            BaseArtifactType parentArtifactType, BaseArtifactType childArtifactType)
        {
            // Setup:
            var parentArtifact = Helper.CreateAndPublishArtifact(_project, _user, parentArtifactType);
            var childArtifact = Helper.CreateAndPublishArtifact(_project, _user, childArtifactType, parentArtifact);

            // Create a user without permission to delete the child artifact.
            // NOTE: The difference between AuthorFullAccess & Author is that Author doesn't have delete permission.
            IUser userWithoutDeletePermission = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
            Helper.AssignProjectRolePermissionsToUser(userWithoutDeletePermission, TestHelper.ProjectRole.Author, _project, childArtifact);

            // Execute & Verify:
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.DeleteArtifact(parentArtifact, userWithoutDeletePermission),
                "We should get a 403 Forbidden when a user trying to delete a parent artifact does not have permission to delete one of its children!");

            // Verify:
            string expectedMessage = I18NHelper.FormatInvariant("You do not have permission to delete the artifact (ID: {0})", parentArtifact.Id);
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, expectedMessage);
        }

        [TestRail(190967)]
        [TestCase(ItemTypePredefined.ArtifactCollection)]
        [TestCase(ItemTypePredefined.CollectionFolder)]
        [Description("Create and publish a Collection or Collection Folder artifact.  Attempt to delete the artifact with a user that does not have permission to delete. " +
                     "Verify that HTTP 403 Forbidden exception is thrown.")]
        public void DeleteArtifact_UserDoesNotHavePermissionToDeleteCollectionOrCollectionFolder_403Forbidden(ItemTypePredefined artifactType)
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            var collectionFolder = _project.GetDefaultCollectionFolder(Helper.ArtifactStore.Address, _user);
            var fakeBaseType = BaseArtifactType.PrimitiveFolder;
            IArtifact artifact = Helper.CreateAndWrapNovaArtifact(_project, _user, artifactType, collectionFolder.Id, baseType: fakeBaseType);
            artifact.Publish();

            // Create a user without permission to delete the artifact.
            // NOTE: The difference between AuthorFullAccess & Author is that Author doesn't have delete permission.
            IUser userWithoutDeletePermission = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
            Helper.AssignProjectRolePermissionsToUser(userWithoutDeletePermission, TestHelper.ProjectRole.Author, _project, artifact);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.DeleteArtifact(artifact, userWithoutDeletePermission),
                "We should get a 403 Fordbidden when a user trying to delete an artifact does not have permission to delete!");

            // Verify:
            string expectedMessage = I18NHelper.FormatInvariant("You do not have permission to delete the artifact (ID: {0})", artifact.Id);
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, expectedMessage);
        }

        #endregion 403 Forbidden tests

        #region 404 Not Found tests

        [TestRail(165818)]
        [TestCase(BaseArtifactType.Actor)]
        [Description("Create and publish an artifact. Delete the artifact. Attempt to delete the same artifact again. Verify that " +
                     "HTTP 404 Not Found Exception is thrown.")]
        public void DeleteArtifact_DeletedArtifactNotPublished_404NotFound(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            Assert.DoesNotThrow(() => artifact.Delete(_user),
                "Failed to delete a published artifact!");

            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.DeleteArtifact(artifact, _user),
                "We should get a 404 Not Found when trying to delete an artifact that has already been deleted!");
        }

        [TestRail(165819)]
        [TestCase(BaseArtifactType.Actor)]
        [Description("Create and publish an artifact. Delete the artifact and publish.  Attempt to delete the same artifact again. " +
                     "Verify that HTTP 404 Not Found Exception is thrown.")]
        public void DeleteArtifact_DeletedArtifactPublished_404NotFound(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            Assert.DoesNotThrow(() => artifact.Delete(_user),
                "Failed to delete a published artifact!");

            artifact.Publish(_user);

            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.DeleteArtifact(artifact, _user),
                "We should get a 404 Not Found when trying to delete an artifact that has already been deleted and published!");
        }

        [TestRail(165820)]
        [TestCase(BaseArtifactType.Actor, 0)]
        [TestCase(BaseArtifactType.Actor, int.MaxValue)]
        [Description("Create and publish an artifact.  Attempt to delete the artifact after changing the artifact Id to a " +
                     "non existent Id. Verify that HTTP 404 Not Found Exception is thrown.")]
        public void DeleteArtifact_NonExistentArtifact_404NotFound(BaseArtifactType artifactType, int nonExistentArtifactId)
        {
            // Setup:
            IArtifact artifact = ArtifactFactory.CreateArtifact(_project, _user, artifactType);

            // Replace artifact Id with non-existent Id
            artifact.Id = nonExistentArtifactId;

            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.DeleteArtifact(artifact, _user),
                "We should get a 404 Not Found when trying to delete an artifact that does not exist!");
        }

        #endregion 404 Not Found tests

        #region 409 Conflict tests

        [TestRail(165824)]
        [TestCase(BaseArtifactType.Actor)]
        [Description("Create an artifact and publish. Lock artifact. Attempt to delete the artifact with another user that does not have the " +
                     "lock on the artifact. Verify that HTTP 409 Conflict exception is thrown.")]
        public void DeleteArtifact_UserTriesToDeleteArtifactLockedByAnotherUser_409Conflict(BaseArtifactType artifactType)
        {
            // Setup:
            IUser userWithLock = null;
            userWithLock = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, userWithLock, artifactType);

            // Lock artifact to prevent other users from deleting
            artifact.Lock(userWithLock);

            // Execute & Verify:
            Assert.Throws<Http409ConflictException>(() => Helper.ArtifactStore.DeleteArtifact(artifact, _user),
                "We should get a 409 Conflict when a user tries to delete an artifact that another user has locked!");
        }

        [TestRail(165844)]
        [TestCase(BaseArtifactType.Process, BaseArtifactType.Actor)]
        [Description("Create an artifact and child and publish both artifacts. Lock child with another user. Attempt to delete the parent artifact " +
             "with the user that does not have the lock on the child artifact. Verify that HTTP 409 Conflict exception is thrown.")]
        public void DeleteArtifact_UserTriesToDeleteArtifactWithChildLockedByAnotherUser_409Conflict(
            BaseArtifactType parentArtifactType, BaseArtifactType childArtifactType)
        {
            // Setup:
            IUser userWithLock = null;
            userWithLock = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            var artifactChain = new List<IArtifact>();

            artifactChain.Add(Helper.CreateAndPublishArtifact(_project, _user, parentArtifactType));
            artifactChain.Add(Helper.CreateAndPublishArtifact(_project, _user, childArtifactType, artifactChain.First()));

            artifactChain.Last().Lock(userWithLock);

            try
            {
                // Execute & Verify:
                Assert.Throws<Http409ConflictException>(() => Helper.ArtifactStore.DeleteArtifact(artifactChain.First(), _user),
                    "We should get a 409 Conflict when a user tries to delete an artifact when it has a child locked by another user!");
            }
            finally
            {
                // Delete & publish the locked artifact so the TearDown will succeed.
                artifactChain.Last().Delete(userWithLock);
                artifactChain.Last().Publish(userWithLock);
            }
        }

        #endregion 409 Conflict tests

        #region Private functions

        /// <summary>
        /// Try to get each artifact in the list (starting at the specified index) and verify a 404 error is returned.
        /// </summary>
        /// <param name="artifacts">The list of artifacts.</param>
        /// <param name="startIndex">(optional) To skip artifacts at the beginning of the list, enter the index of the first artifact to check.</param>
        private void VerifyArtifactsAreDeleted(List<IArtifact> artifacts, int startIndex = 0)
        {
            ThrowIf.ArgumentNull(artifacts, nameof(artifacts));

            // Try to get each artifact to verify they're deleted.
            for (int i = startIndex; i < artifacts.Count; ++i)
            {
                VerifyArtifactIsDeleted(artifacts[i]);
            }
        }

        /// <summary>
        /// Try to get the artifact and verify a 404 error is returned.
        /// </summary>
        /// <param name="artifact">The artifact whose existence is being verified.</param>
        /// <param name="user">(optional) The user to use to get the artifact details.  Defaults to _user.</param>
        private void VerifyArtifactIsDeleted(IArtifact artifact, IUser user = null)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            if (user == null)
            {
                user = _user;
            }

            // Try to get the artifact to verify it's deleted.
            Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetArtifactDetails(user, artifact.Id),
                "We should get a 404 Not Found when trying to get artifact details of a deleted artifact!");
        }

        /// <summary>
        /// Verifies that the specified artifact has the specified number of manual & other traces.
        /// </summary>
        /// <param name="artifact">The artifact whose traces you want to verify.</param>
        /// <param name="expectedManualTraces">The expected number of manual traces.</param>
        /// <param name="expectedOtherTraces">The expected number of other traces.</param>
        private void VerifyArtifactHasExpectedNumberOfTraces(IArtifact artifact, int expectedManualTraces, int expectedOtherTraces)
        {
            Relationships relationships = null;

            Assert.DoesNotThrow(() => relationships = Helper.ArtifactStore.GetRelationships(_user, artifact),
                "GetArtifactRelationships shouldn't throw any error when given a valid artifact.");

            Assert.AreEqual(expectedManualTraces, relationships.ManualTraces.Count, "The artifact should have {0} manual trace(s).", expectedManualTraces);
            Assert.AreEqual(expectedOtherTraces, relationships.OtherTraces.Count, "The artifact should have {0} other trace(s).", expectedOtherTraces);
        }

        /// <summary>
        /// Verifies that each artifact in the chain (starting at the specified index) was returned in the list of deleted artifacts.
        /// </summary>
        /// <param name="artifactChain">The chain of parent/child artifacts that were created.</param>
        /// <param name="deletedArtifacts">The list of artifacts returned by the delete call.</param>
        /// <param name="startIndex">(optional) To skip artifacts at the beginning of the list, enter the index of the first artifact to check.</param>
        private static void VerifyDeletedArtifactAndChildrenWereReturned(List<IArtifact> artifactChain,
            List<INovaArtifactResponse> deletedArtifacts,
            int startIndex = 0)
        {
            ThrowIf.ArgumentNull(artifactChain, nameof(artifactChain));
            ThrowIf.ArgumentNull(deletedArtifacts, nameof(deletedArtifacts));

            for (int i = startIndex; i < artifactChain.Count; ++i)
            {
                var artifact = artifactChain[i];

                Assert.That(deletedArtifacts.Exists(a => a.Id == artifact.Id),
                    "The list of deleted artifacts returned by 'DELETE {0}' didn't contain an artifact with ID: {1}!",
                    RestPaths.Svc.ArtifactStore.ARTIFACTS_id_, artifact.Id);
            }
        }

        #endregion Private functions
    }
}
