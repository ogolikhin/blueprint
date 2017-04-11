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
using Utilities.Facades;
using System;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]    // TODO: Maybe move Baseline and collection test to other file.
    public class DeleteArtifactTests : TestBase
    {
        private const string DELETE_ARTIFACT_ID_PATH = RestPaths.Svc.ArtifactStore.ARTIFACTS_id_;

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
            var authorUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
            var artifact = Helper.CreateAndPublishArtifact(_project, authorUser, artifactType);

            // Execute:
            List<INovaArtifactResponse> deletedArtifacts = null;

            Assert.DoesNotThrow(() => deletedArtifacts = Helper.ArtifactStore.DeleteArtifact(artifact, authorUser),
                "'DELETE {0}' should return 200 OK if a valid artifact ID is sent!", DELETE_ARTIFACT_ID_PATH);

            // Verify:
            Assert.AreEqual(1, deletedArtifacts.Count, "There should only be 1 deleted artifact returned!");
            Assert.AreEqual(artifact.Id, deletedArtifacts[0].Id, "The artifact ID doesn't match the one that we deleted!");

            VerifyArtifactIsDeleted(artifact.Id, authorUser);
        }

        [TestCase(ItemTypePredefined.ArtifactCollection)]
        [TestCase(ItemTypePredefined.CollectionFolder)]
        [TestRail(185237)]
        [Description("Create & publish a collection or collection folder.  Delete the artifact - it should return 200 OK and the deleted artifact." +
            "Try to get the artifact and verify you get a 404 since it was deleted.")]
        public void DeleteArtifact_PublishedCollectionOrCollectionFolderWithNoChildren_ArtifactIsDeleted(ItemTypePredefined artifactType)
        {
            // Setup:
            var authorUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, authorUser);

            var collectionFolder = _project.GetDefaultCollectionFolder(authorUser);
            var fakeBaseType = BaseArtifactType.PrimitiveFolder;
            var artifact = Helper.CreateWrapAndPublishNovaArtifact(_project, authorUser, artifactType, collectionFolder.Id, baseType: fakeBaseType);

            // Execute:
            List<INovaArtifactResponse> deletedArtifacts = null;

            Assert.DoesNotThrow(() => deletedArtifacts = Helper.ArtifactStore.DeleteArtifact(artifact, authorUser),
                "'DELETE {0}' should return 200 OK if a valid {1} ID is sent!", DELETE_ARTIFACT_ID_PATH, artifactType);

            // Verify:
            artifact.IsDeleted = true;
            Assert.AreEqual(1, deletedArtifacts.Count, "There should only be 1 deleted artifact returned!");
            Assert.AreEqual(artifact.Id, deletedArtifacts[0].Id, "The artifact ID doesn't match the one that we deleted!");

            VerifyArtifactIsDeleted(artifact.Id, authorUser);
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(165747)]
        [Description("Create & save an artifact.  Delete the artifact - it should return 200 OK and the deleted artifact." +
            "Try to get the artifact and verify you get a 404 since it was deleted.")]
        public void DeleteArtifact_SavedArtifactWithNoChildren_ArtifactIsDeleted(BaseArtifactType artifactType)
        {
            // Setup:
            var authorUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
            var artifact = Helper.CreateAndSaveArtifact(_project, authorUser, artifactType);

            // Execute:
            List<INovaArtifactResponse> deletedArtifacts = null;

            Assert.DoesNotThrow(() => deletedArtifacts = Helper.ArtifactStore.DeleteArtifact(artifact, authorUser),
                "'DELETE {0}' should return 200 OK if a valid artifact ID is sent!", DELETE_ARTIFACT_ID_PATH);

            // Verify:
            Assert.AreEqual(1, deletedArtifacts.Count, "There should only be 1 deleted artifact returned!");
            Assert.AreEqual(artifact.Id, deletedArtifacts[0].Id, "The artifact ID doesn't match the one that we deleted!");

            VerifyArtifactIsDeleted(artifact.Id, authorUser);
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(165748)]
        [Description("Create & publish an artifact, then save a draft.  Delete the artifact - it should return 200 OK and the deleted artifact." +
            "Try to get the artifact and verify you get a 404 since it was deleted.")]
        public void DeleteArtifact_PublishedWithDraftArtifactWithNoChildren_ArtifactIsDeleted(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            artifact.Save();

            // Execute:
            List<INovaArtifactResponse> deletedArtifacts = null;

            Assert.DoesNotThrow(() => deletedArtifacts = Helper.ArtifactStore.DeleteArtifact(artifact, _user),
                "'DELETE {0}' should return 200 OK if a valid artifact ID is sent!", DELETE_ARTIFACT_ID_PATH);

            // Verify:
            Assert.AreEqual(1, deletedArtifacts.Count, "There should only be 1 deleted artifact returned!");
            Assert.AreEqual(artifact.Id, deletedArtifacts[0].Id, "The artifact ID doesn't match the one that we deleted!");

            VerifyArtifactIsDeleted(artifact.Id, _user);
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
                "'DELETE {0}' should return 200 OK if a valid artifact ID is sent!", DELETE_ARTIFACT_ID_PATH);

            // Verify:
            int expectedArtifactCount = artifactChain.Count - indexToDelete;
            Assert.AreEqual(expectedArtifactCount, deletedArtifacts.Count, "There should be {0} deleted artifact returned!", expectedArtifactCount);

            VerifyDeletedArtifactAndChildrenWereReturned(artifactChain, deletedArtifacts, indexToDelete);
            VerifyArtifactsAreDeleted(artifactChain, _user, indexToDelete);
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
                "'DELETE {0}' should return 200 OK if a valid artifact ID is sent!", DELETE_ARTIFACT_ID_PATH);

            // Verify:
            int expectedArtifactCount = artifactChain.Count - indexToDelete;
            Assert.AreEqual(expectedArtifactCount, deletedArtifacts.Count, "There should be {0} deleted artifact returned!", expectedArtifactCount);

            VerifyDeletedArtifactAndChildrenWereReturned(artifactChain, deletedArtifacts, indexToDelete);
            VerifyArtifactsAreDeleted(artifactChain, _user, indexToDelete);
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
                "'DELETE {0}' should return 200 OK if a valid artifact ID is sent!", DELETE_ARTIFACT_ID_PATH);
                
            // Verify:
            int expectedArtifactCount = artifactChain.Count;
            Assert.AreEqual(expectedArtifactCount, deletedArtifacts.Count, "There should be {0} deleted artifact returned!", expectedArtifactCount);

            VerifyDeletedArtifactAndChildrenWereReturned(artifactChain, deletedArtifacts);
            VerifyArtifactsAreDeleted(artifactChain, _user);
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
                "'DELETE {0}' should return 200 OK if a valid artifact ID is sent!", DELETE_ARTIFACT_ID_PATH);

            // Verify:
            int expectedArtifactCount = artifactChain.Count;
            Assert.AreEqual(expectedArtifactCount, deletedArtifacts.Count, "There should be {0} deleted artifact returned!", expectedArtifactCount);

            VerifyDeletedArtifactAndChildrenWereReturned(artifactChain, deletedArtifacts);
            VerifyArtifactsAreDeleted(artifactChain, _user);
        }

        [TestCase]
        [TestRail(165821)]
        [Description("Create & publish 2 artifacts and add a manual trace between them.  Delete the first artifact - it should return 200 OK and the deleted artifact." +
            "Try to get the artifact and verify you get a 404 since it was deleted.  Get the relationships of the second artifact and verify there are no traces.")]
        public void DeleteArtifact_PublishedArtifactWithManualTrace_ArtifactAndTraceIsDeleted()
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            var targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase);

            OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact, targetArtifact, TraceDirection.From, _user);

            // Make sure trace was created properly.
            VerifyArtifactHasExpectedNumberOfTraces(sourceArtifact, expectedManualTraces: 1, expectedOtherTraces: 0);

            // Execute:
            List<INovaArtifactResponse> deletedArtifacts = null;

            Assert.DoesNotThrow(() => deletedArtifacts = Helper.ArtifactStore.DeleteArtifact(targetArtifact, _user),
                "'DELETE {0}' should return 200 OK if a valid artifact ID is sent!", DELETE_ARTIFACT_ID_PATH);

            // Verify:
            VerifyArtifactIsDeleted(targetArtifact.Id, _user);
            VerifyArtifactHasExpectedNumberOfTraces(sourceArtifact, expectedManualTraces: 0, expectedOtherTraces: 0);
        }

        [TestCase]
        [TestRail(165822)]
        [Description("Create & save 2 artifacts and add a manual trace between them.  Delete the first artifact - it should return 200 OK and the deleted artifact." +
            "Try to get the artifact and verify you get a 404 since it was deleted.  Get the relationships of the second artifact and verify there are no traces.")]
        public void DeleteArtifact_SavedArtifactWithManualTrace_ArtifactAndTraceIsDeleted()
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Actor);
            var targetArtifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.UseCase);

            OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact, targetArtifact, TraceDirection.From, _user);

            // Make sure trace was created properly.
            VerifyArtifactHasExpectedNumberOfTraces(sourceArtifact, expectedManualTraces: 1, expectedOtherTraces: 0);

            // Execute:
            List<INovaArtifactResponse> deletedArtifacts = null;

            Assert.DoesNotThrow(() => deletedArtifacts = Helper.ArtifactStore.DeleteArtifact(targetArtifact, _user),
                "'DELETE {0}' should return 200 OK if a valid artifact ID is sent!", DELETE_ARTIFACT_ID_PATH);

            // Verify:
            VerifyArtifactIsDeleted(targetArtifact.Id, _user);
            VerifyArtifactHasExpectedNumberOfTraces(sourceArtifact, expectedManualTraces: 0, expectedOtherTraces: 0);
        }

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
            List<INovaArtifactResponse> deletedArtifacts = null;

            Assert.DoesNotThrow(() => deletedArtifacts = Helper.ArtifactStore.DeleteArtifact(grandParentArtifact, _user),
                "'DELETE {0}' should return 200 OK if a valid artifact ID is sent!", DELETE_ARTIFACT_ID_PATH);

            Helper.ArtifactStore.PublishArtifacts(artifacts: null, user: _user, publishAll: true);

            Assert.DoesNotThrow(() => deletedArtifacts = Helper.ArtifactStore.DeleteArtifact(parentArtifact, _user),
                "'DELETE {0}' should return 200 OK if a valid artifact ID is sent!", DELETE_ARTIFACT_ID_PATH);

            // Verify:
            var parentAndChild = artifactChain.GetRange(1, 2);
            int expectedArtifactCount = parentAndChild.Count;
            Assert.AreEqual(expectedArtifactCount, deletedArtifacts.Count, "There should be {0} deleted artifact returned!", expectedArtifactCount);

            VerifyDeletedArtifactAndChildrenWereReturned(parentAndChild, deletedArtifacts);
            VerifyArtifactsAreDeleted(parentAndChild, _user);
        }

        [TestCase]
        [TestRail(267246)]
        [Description("Delete published Baseline, verify that Baseline was deleted.")]
        public void DeleteArtifact_PublishedBaseline_BaselineIsDeleted()
        {
            // Setup:
            var artifactToAdd = Helper.CreateNovaArtifactInSpecificState(_user, _project, TestHelper.TestArtifactState.Published, ItemTypePredefined.Actor,
                _project.Id);
            var baselineArtifact = Helper.CreateBaseline(_user, _project, artifactToAddId: artifactToAdd.Id);
            Helper.ArtifactStore.PublishArtifacts(new List<int> { baselineArtifact.Id }, _user);

            List<NovaArtifactResponse> deletedArtifacts = null;

            // Execute:
            Assert.DoesNotThrow(() => deletedArtifacts = Helper.ArtifactStore.DeleteArtifact(baselineArtifact.Id, _user),
                "'DELETE {0}' should return 200 OK if a valid artifact ID is sent!", DELETE_ARTIFACT_ID_PATH);

            // Verify:
            Assert.AreEqual(1, deletedArtifacts.Count, "One artifact(Baseline) should be deleted.");
            Assert.AreEqual(baselineArtifact.Id, deletedArtifacts[0].Id, "Deleted artifact should have expected Id.");
            VerifyArtifactIsDeleted(baselineArtifact.Id, _user);
            var versionInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, artifactToAdd.Id);
            Assert.IsFalse(versionInfo.IsDeleted.Value, "Artifact shouldn't be deleted.");
        }

        [TestCase]
        [TestRail(267250)]
        [Description("Create Baseline folder with non-empty Baseline, publish all, delete Baseline folder - check folder and Baseline were deleted.")]
        public void DeleteArtifact_PublishedBaselineFolderWithBaseline_BaselineFolderIsDeleted()
        {
            // Setup:
            var artifactToAdd = Helper.CreateNovaArtifactInSpecificState(_user, _project, TestHelper.TestArtifactState.Published,
                ItemTypePredefined.Actor,_project.Id);
            var defaultBaselineFolder = _project.GetDefaultBaselineFolder(_user);
            var baselineFolder = Helper.CreateNovaArtifactInSpecificState(_user, _project, TestHelper.TestArtifactState.Created,
                ItemTypePredefined.BaselineFolder, defaultBaselineFolder.Id);
            var baselineArtifact = Helper.CreateBaseline(_user, _project, parentId: baselineFolder.Id,
                artifactToAddId: artifactToAdd.Id);
            Helper.ArtifactStore.PublishArtifacts(new List<int> { baselineArtifact.Id }, _user, publishAll: true);

            List<NovaArtifactResponse> deletedArtifacts = null;

            // Execute:
            Assert.DoesNotThrow(() => deletedArtifacts = Helper.ArtifactStore.DeleteArtifact(baselineFolder.Id, _user),
                "'DELETE {0}' should return 200 OK if a valid artifact ID is sent!", DELETE_ARTIFACT_ID_PATH);

            // Verify:
            Assert.AreEqual(2, deletedArtifacts.Count, "Two artifacts(Baseline and Baseline Folder) should be deleted.");
            var deletedArtifactIds = deletedArtifacts.ConvertAll(item => item.Id);
            Assert.IsTrue(deletedArtifactIds.Contains(baselineArtifact.Id), "Baseline should be among deleted artifacts.");
            Assert.IsTrue(deletedArtifactIds.Contains(baselineFolder.Id), "Baseline Folder should be among deleted artifacts.");
            VerifyArtifactIsDeleted(baselineArtifact.Id, _user);
            VerifyArtifactIsDeleted(baselineFolder.Id, _user);
        }

        #endregion 200 OK tests

        #region 400 Bad Request
        
        [TestRail(246533)]
        [TestCase("9999999999", "The request is invalid.")]
        [TestCase("&amp;", "A potentially dangerous Request.Path value was detected from the client (&).")]
        [Description("Create a rest path that tries to delete an artifact with an invalid artifact Id. " +
                     "Attempt to delete the artifact. Verify that HTTP 400 Bad Request exception is thrown.")]
        public void DeleteArtifact_InvalidArtifactId_400BadRequest(string artifactId, string expectedErrorMessage)
        {
            // Setup:
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.ARTIFACTS_id_, artifactId);

            var restApi = new RestApiFacade(Helper.ArtifactStore.Address, _user?.Token?.AccessControlToken);

            // Execute & Verify:
            var ex = Assert.Throws<Http400BadRequestException>(() => restApi.SendRequestAndDeserializeObject<NovaArtifactResponse, object>(
               path,
               RestRequestMethod.DELETE,
               jsonObject: null),
                "We should get a 400 Bad Request when the artifact Id is invalid!");

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, expectedErrorMessage);
        }

        #endregion 400 Bad Request

        #region 401 Unauthorized tests

        [TestRail(165823)]
        [TestCase(BaseArtifactType.Actor)]
        [Description("Create an artifact and publish. Attempt to delete the artifact with a user that does not have authorization " +
                     "to delete. Verify that HTTP 401 Unauthorized exception is thrown.")]
        public void DeleteArtifact_UserDoesNotHaveAuthorizationToDelete_401Unauthorized(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            var userWithBadToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

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
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

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
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            // Create a user without permission to delete the artifact.
            // NOTE: The difference between AuthorFullAccess & Author is that Author doesn't have delete permission.
            var userWithoutDeletePermission = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
            Helper.AssignProjectRolePermissionsToUser(userWithoutDeletePermission, TestHelper.ProjectRole.Author, _project, artifact);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.DeleteArtifact(artifact, userWithoutDeletePermission),
                "We should get a 403 Forbidden when a user tries to delete an artifact and the user does not have permission to delete it!");

            // Verify:
            string expectedMessage = I18NHelper.FormatInvariant("You do not have permission to delete the artifact (ID: {0})", artifact.Id);
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotDelete, expectedMessage);
        }

        [TestRail(267471)]
        [TestCase(BaseArtifactType.Actor)]
        [Description("Create and publish an artifact. Attempt to delete the artifact with a user that does not have permission " +
             "to read. Verify that HTTP 403 Forbidden exception is thrown.")]
        public void DeleteArtifact_UserDoesNotHaveReadPermission_403Forbidden(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            // Create a user without permission to read the artifact.
            var userWithoutReadPermission = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
            Helper.AssignProjectRolePermissionsToUser(userWithoutReadPermission, TestHelper.ProjectRole.None, _project, artifact);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.DeleteArtifact(artifact, userWithoutReadPermission),
                "We should get a 403 Forbidden when a user tries to delete an artifact and the user does not have permission to read it!");

            // Verify:
            string expectedMessage = I18NHelper.FormatInvariant("You do not have permission to access the artifact (ID: {0})", artifact.Id);
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, expectedMessage);
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
            var userWithoutDeletePermission = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
            Helper.AssignProjectRolePermissionsToUser(userWithoutDeletePermission, TestHelper.ProjectRole.Author, _project, childArtifact);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.DeleteArtifact(parentArtifact, userWithoutDeletePermission),
                "We should get a 403 Forbidden when a user trying to delete a parent artifact does not have permission to delete one of its children!");

            // Verify:
            string expectedMessage = I18NHelper.FormatInvariant("You do not have permission to delete the artifact (ID: {0})", childArtifact.Id);
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotDelete, expectedMessage);
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

            var collectionFolder = _project.GetDefaultCollectionFolder(_user);
            var fakeBaseType = BaseArtifactType.PrimitiveFolder;
            var artifact = Helper.CreateWrapAndPublishNovaArtifact(_project, _user, artifactType, collectionFolder.Id, baseType: fakeBaseType);

            // Create a user without permission to delete the artifact.
            // NOTE: The difference between AuthorFullAccess & Author is that Author doesn't have delete permission.
            var userWithoutDeletePermission = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
            Helper.AssignProjectRolePermissionsToUser(userWithoutDeletePermission, TestHelper.ProjectRole.Author, _project, artifact);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.DeleteArtifact(artifact, userWithoutDeletePermission),
                "We should get a 403 Forbidden when a user trying to delete an artifact does not have permission to delete!");

            // Verify:
            string expectedMessage = I18NHelper.FormatInvariant("You do not have permission to delete the artifact (ID: {0})", artifact.Id);
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotDelete, expectedMessage);
        }

        [TestCase]
        [TestRail(267247)]
        [Description("Try to delete published sealed Baseline, verify 403 error message.")]
        public void DeleteArtifact_SealedBaseline_Check403Error()
        {
            // Setup:
            var artifactToAdd = Helper.CreateNovaArtifactInSpecificState(_user, _project, TestHelper.TestArtifactState.Published, ItemTypePredefined.Actor,
                _project.Id);
            var baselineArtifact = Helper.CreateBaseline(_user, _project, artifactToAddId: artifactToAdd.Id);

            var baseline = Helper.ArtifactStore.GetBaseline(_user, baselineArtifact.Id);
            baseline.SetUtcTimestamp(DateTime.UtcNow.AddMinutes(-1));
            baseline.SetIsSealed(true);
            Helper.ArtifactStore.UpdateArtifact(_user, baseline);
            Helper.ArtifactStore.PublishArtifacts(new List<int> { baselineArtifact.Id }, _user);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.DeleteArtifact(baselineArtifact.Id, _user),
                "DELETE sealed Baseline should return 403 error.");

            // Verify:
            string errorMessage = "The artifact cannot be deleted because one or more of its descendants cannot currently be deleted (for example, a Baseline has been sealed).";
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotDelete, errorMessage);
        }

        [TestCase]
        [TestRail(267253)]
        [Description("Create Baseline folder with non-empty sealed Baseline, publish all, try to delete Baseline folder - check 403 error message.")]
        public void DeleteArtifact_PublishedBaselineFolderWithSealedBaseline_Check403Error()
        {
            // Setup:
            var artifactToAdd = Helper.CreateNovaArtifactInSpecificState(_user, _project, TestHelper.TestArtifactState.Published,
                ItemTypePredefined.Actor, _project.Id);
            var defaultBaselineFolder = _project.GetDefaultBaselineFolder(_user);
            var baselineFolder = Helper.CreateNovaArtifactInSpecificState(_user, _project, TestHelper.TestArtifactState.Published,
                ItemTypePredefined.BaselineFolder, defaultBaselineFolder.Id);
            var baselineArtifact = Helper.CreateBaseline(_user, _project, parentId: baselineFolder.Id,
                artifactToAddId: artifactToAdd.Id);

            var baseline = Helper.ArtifactStore.GetBaseline(_user, baselineArtifact.Id);
            baseline.SetUtcTimestamp(DateTime.UtcNow.AddMinutes(-1));
            baseline.SetIsSealed(true);
            Helper.ArtifactStore.UpdateArtifact(_user, baseline);
            Helper.ArtifactStore.PublishArtifacts(new List<int> { baselineArtifact.Id }, _user, publishAll: true);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.DeleteArtifact(baselineFolder.Id, _user),
                "DELETE sealed Baseline should return 403 error.");

            // Verify:
            string errorMessage = "The artifact cannot be deleted because one or more of its descendants cannot currently be deleted (for example, a Baseline has been sealed).";
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotDelete, errorMessage);
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
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            Assert.DoesNotThrow(() => artifact.Delete(_user), "Failed to delete a published artifact!");

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.DeleteArtifact(artifact, _user),
                "We should get a 404 Not Found when trying to delete an artifact that has already been deleted!");

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound,
                "You have attempted to access an artifact that does not exist or has been deleted.");
        }

        [TestRail(165819)]
        [TestCase(BaseArtifactType.Actor)]
        [Description("Create and publish an artifact. Delete the artifact and publish.  Attempt to delete the same artifact again. " +
                     "Verify that HTTP 404 Not Found Exception is thrown.")]
        public void DeleteArtifact_DeletedArtifactPublished_404NotFound(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            Assert.DoesNotThrow(() => artifact.Delete(_user), "Failed to delete a published artifact!");

            artifact.Publish(_user);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.DeleteArtifact(artifact, _user),
                "We should get a 404 Not Found when trying to delete an artifact that has already been deleted and published!");

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound,
                "You have attempted to access an artifact that does not exist or has been deleted.");
        }

        [TestRail(190971)]
        [TestCase(ItemTypePredefined.ArtifactCollection)]
        [TestCase(ItemTypePredefined.CollectionFolder)]
        [Description("Create and publish a Collection or Collection Folder artifact.  Delete the artifact.  Attempt to delete the same artifact again.  " +
                     "Verify that HTTP 404 Not Found Exception is thrown.")]
        public void DeleteArtifact_DeletedCollectionOrCollectionFolderNotPublished_404NotFound(ItemTypePredefined artifactType)
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            var collectionFolder = _project.GetDefaultCollectionFolder(_user);
            var fakeBaseType = BaseArtifactType.PrimitiveFolder;
            var artifact = Helper.CreateWrapAndPublishNovaArtifact(_project, _user, artifactType, collectionFolder.Id, baseType: fakeBaseType);

            Assert.DoesNotThrow(() => Helper.ArtifactStore.DeleteArtifact(artifact, _user), "Failed to delete a published {0}!", artifactType);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.DeleteArtifact(artifact, _user),
                "We should get a 404 Not Found when trying to delete a {0} that has already been deleted!", artifactType);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound,
                "You have attempted to access an artifact that does not exist or has been deleted.");
        }

        [TestRail(190972)]
        [TestCase(ItemTypePredefined.ArtifactCollection)]
        [TestCase(ItemTypePredefined.CollectionFolder)]
        [Description("Create and publish a Collection or Collection Folder artifact.  Delete the artifact and publish.  Attempt to delete the same artifact again.  " +
                     "Verify that HTTP 404 Not Found Exception is thrown.")]
        public void DeleteArtifact_DeletedCollectionOrCollectionFolderPublished_404NotFound(ItemTypePredefined artifactType)
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            var collectionFolder = _project.GetDefaultCollectionFolder(_user);
            var fakeBaseType = BaseArtifactType.PrimitiveFolder;
            var artifact = Helper.CreateWrapAndPublishNovaArtifact(_project, _user, artifactType, collectionFolder.Id, baseType: fakeBaseType);

            Assert.DoesNotThrow(() => Helper.ArtifactStore.DeleteArtifact(artifact, _user), "Failed to delete a published {0}!", artifactType);

            artifact.Publish(_user);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.DeleteArtifact(artifact, _user),
                "We should get a 404 Not Found when trying to delete a {0} that has already been deleted and published!", artifactType);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound,
                "You have attempted to access an artifact that does not exist or has been deleted.");
        }

        [TestRail(165820)]
        [TestCase(BaseArtifactType.Actor, 0)]
        [TestCase(BaseArtifactType.Actor, int.MaxValue)]
        [Description("Create and publish an artifact.  Attempt to delete the artifact after changing the artifact Id to a " +
                     "non existent Id. Verify that HTTP 404 Not Found Exception is thrown.")]
        public void DeleteArtifact_NonExistentArtifact_404NotFound(BaseArtifactType artifactType, int nonExistentArtifactId)
        {
            // Setup:
            var artifact = ArtifactFactory.CreateArtifact(_project, _user, artifactType);

            // Replace artifact Id with non-existent Id
            artifact.Id = nonExistentArtifactId;

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.DeleteArtifact(artifact, _user),
                "We should get a 404 Not Found when trying to delete an artifact that does not exist!");

            // Verify:
            if (nonExistentArtifactId > 0)  // Id's <= 0 get the generic 404 HTML page from IIS.
            {
                TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound,
                    "You have attempted to access an artifact that does not exist or has been deleted.");
            }
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
            var userWithLock = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            var artifact = Helper.CreateAndPublishArtifact(_project, userWithLock, artifactType);

            // Lock artifact to prevent other users from deleting
            artifact.Lock(userWithLock);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => Helper.ArtifactStore.DeleteArtifact(artifact, _user),
                "We should get a 409 Conflict when a user tries to delete an artifact that another user has locked!");

            // Verify:
            string prefix = _project.ArtifactTypes.Find(a => a.BaseArtifactType == artifactType).Prefix;
            string expectedMessage = I18NHelper.FormatInvariant("Artifact \"{0}: {1}\" is already locked by other user", prefix, artifact.Id);
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.LockedByOtherUser, expectedMessage);
        }

        [TestRail(165844)]
        [TestCase(BaseArtifactType.Process, BaseArtifactType.Actor)]
        [Description("Create an artifact and child and publish both artifacts. Lock child with another user. Attempt to delete the parent artifact " +
             "with the user that does not have the lock on the child artifact. Verify that HTTP 409 Conflict exception is thrown.")]
        public void DeleteArtifact_UserTriesToDeleteArtifactWithChildLockedByAnotherUser_409Conflict(
            BaseArtifactType parentArtifactType, BaseArtifactType childArtifactType)
        {
            // Setup:
            var userWithLock = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            var parentArtifact = Helper.CreateAndPublishArtifact(_project, _user, parentArtifactType);
            var childArtifact = Helper.CreateAndPublishArtifact(_project, _user, childArtifactType, parentArtifact);

            childArtifact.Lock(userWithLock);

            try
            {
                // Execute:
                var ex = Assert.Throws<Http409ConflictException>(() => Helper.ArtifactStore.DeleteArtifact(parentArtifact, _user),
                    "We should get a 409 Conflict when a user tries to delete an artifact when it has a child locked by another user!");

                // Verify:
                string prefix = _project.ArtifactTypes.Find(a => a.BaseArtifactType == childArtifactType).Prefix;
                string expectedMessage = I18NHelper.FormatInvariant("Artifact \"{0}: {1}\" is already locked by other user", prefix, childArtifact.Id);
                TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.LockedByOtherUser, expectedMessage);
            }
            finally
            {
                // Delete & publish the locked artifact so the TearDown will succeed.
                childArtifact.Delete(userWithLock);
                childArtifact.Publish(userWithLock);
            }
        }
        
        [TestRail(190978)]
        [TestCase(ItemTypePredefined.ArtifactCollection)]
        [TestCase(ItemTypePredefined.CollectionFolder)]
        [Description("Create a Collection or Collection Folder artifact and publish.  Lock artifact.  Attempt to delete the artifact with another user " +
                     "that does not have the lock on the artifact.  Verify that HTTP 409 Conflict exception is thrown.")]
        public void DeleteArtifact_UserTriesToDeleteCollectionOrCollectionFolderLockedByAnotherUser_409Conflict(ItemTypePredefined artifactType)
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            var collectionFolder = _project.GetDefaultCollectionFolder(_user);
            var fakeBaseType = BaseArtifactType.PrimitiveFolder;

            var userWithLock = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            var artifact = Helper.CreateWrapAndPublishNovaArtifact(_project, userWithLock, artifactType, collectionFolder.Id, baseType: fakeBaseType);

            // Lock artifact to prevent other users from deleting
            artifact.Lock(userWithLock);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => Helper.ArtifactStore.DeleteArtifact(artifact, _user),
                "We should get a 409 Conflict when a user tries to delete a {0} that another user has locked!", artifactType);

            // Verify:
            string prefix = _project.NovaArtifactTypes.Find(a => a.PredefinedType == artifactType).Prefix;
            string expectedMessage = I18NHelper.FormatInvariant("Artifact \"{0}: {1}\" is already locked by other user", prefix, artifact.Id);
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.LockedByOtherUser, expectedMessage);
        }

        [TestRail(190979)]
        [TestCase]
        [Description("Create a Collection Folder with child Collection and publish both artifacts.  Lock child with another user.  Attempt to delete the " +
             "parent Collection Folder with the user that does not have the lock on the child Collection.  Verify that HTTP 409 Conflict exception is thrown.")]
        public void DeleteArtifact_UserTriesToDeleteCollectionFolderWithChildLockedByAnotherUser_409Conflict()
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            var parentCollectionFolder = Helper.CreateAndPublishCollectionFolder(_project, _user);
            var childCollection = Helper.CreateAndPublishCollection(_project, _user, parentCollectionFolder.Id);

            var userWithLock = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            // Another user locks the child collection.
            childCollection.Lock(userWithLock);

            try
            {
                // Execute:
                var ex = Assert.Throws<Http409ConflictException>(() => Helper.ArtifactStore.DeleteArtifact(parentCollectionFolder, _user),
                    "We should get a 409 Conflict when a user tries to delete a Collection Folder when it has a child locked by another user!");

                // Verify:
                string prefix = _project.NovaArtifactTypes.Find(a => a.PredefinedType == ItemTypePredefined.ArtifactCollection).Prefix;
                string expectedMessage = I18NHelper.FormatInvariant("Artifact \"{0}: {1}\" is already locked by other user", prefix, childCollection.Id);
                TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.LockedByOtherUser, expectedMessage);
            }
            finally
            {
                // Delete & publish the locked artifact so the TearDown will succeed.
                Helper.ArtifactStore.DeleteArtifact(childCollection, userWithLock);
                childCollection.Publish(userWithLock);
            }
        }

        #endregion 409 Conflict tests

        #region Private functions

        /// <summary>
        /// Try to get each artifact in the list (starting at the specified index) and verify a 404 error is returned.
        /// </summary>
        /// <param name="artifacts">The list of artifacts.</param>
        /// <param name="user">The user to perform operation.</param>
        /// <param name="startIndex">(optional) To skip artifacts at the beginning of the list, enter the index of the first artifact to check.</param>
        private void VerifyArtifactsAreDeleted(List<IArtifact> artifacts, IUser user, int startIndex = 0)
        {
            ThrowIf.ArgumentNull(artifacts, nameof(artifacts));
            ThrowIf.ArgumentNull(user, nameof(user));

            // Try to get each artifact to verify they're deleted.
            for (int i = startIndex; i < artifacts.Count; ++i)
            {
                VerifyArtifactIsDeleted(artifacts[i].Id, user);
            }
        }

        /// <summary>
        /// Try to get the artifact and verify a 404 error is returned.
        /// </summary>
        /// <param name="artifactId">The artifact whose existence is being verified.</param>
        /// <param name="user">The user to use to get the artifact details.</param>
        private void VerifyArtifactIsDeleted(int artifactId, IUser user)
        {
            // Try to get the artifact to verify it's deleted.
            Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetArtifactDetails(user, artifactId),
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
        private static void VerifyDeletedArtifactAndChildrenWereReturned(List<IArtifact> artifactChain, List<INovaArtifactResponse> deletedArtifacts,
            int startIndex = 0)
        {
            ThrowIf.ArgumentNull(artifactChain, nameof(artifactChain));
            ThrowIf.ArgumentNull(deletedArtifacts, nameof(deletedArtifacts));

            for (int i = startIndex; i < artifactChain.Count; ++i)
            {
                var artifact = artifactChain[i];

                Assert.That(deletedArtifacts.Exists(a => a.Id == artifact.Id),
                    "The list of deleted artifacts returned by 'DELETE {0}' didn't contain an artifact with ID: {1}!",
                    DELETE_ARTIFACT_ID_PATH, artifact.Id);
            }
        }

        #endregion Private functions
    }
}
