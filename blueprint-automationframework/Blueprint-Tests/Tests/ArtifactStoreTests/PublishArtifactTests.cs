using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.StorytellerModel.Impl;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Model.ModelHelpers;
using TestCommon;
using Utilities;

namespace ArtifactStoreTests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class PublishArtifactTests : TestBase
    {
        private const string PUBLISH_PATH = RestPaths.Svc.ArtifactStore.Artifacts.PUBLISH;
        private const string UPDATE_ARTIFACT_ID_PATH = RestPaths.Svc.ArtifactStore.ARTIFACTS_id_;

        private IUser _user = null;
        private IUser _authorUser = null;
        private IProject _project = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_user);
            _authorUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #region 200 OK Tests

        #region Publish Collection Artifact Tests

        [TestCase(1, ItemTypePredefined.Actor, true)]
        [TestCase(2, ItemTypePredefined.Glossary, false)]
        [TestCase(3, ItemTypePredefined.Document, true)]
        [TestCase(4, ItemTypePredefined.Process, false)]
        [TestRail(230691)]
        [Description("Publish a collection that contains either saved or published artifact(s). Verify contents of the published collection.")]
        public void PublishArtifact_PublishCollectionContainingSavedOrPublishedArtifacts_VerifyCollectionContents(
            int numberOfArtifacts,
            ItemTypePredefined includingArtifactType,
            bool shouldPublishIncludingArtifacts)
        {
            // Setup: Create a collection and add published artifacts
            var collectionArtifact = Helper.CreateUnpublishedCollection(_project, _authorUser);

            var savedOrPublishedArtifacts = Helper.CreateAndSaveMultipleArtifacts(_project, _authorUser, includingArtifactType, numberOfArtifacts);

            if (shouldPublishIncludingArtifacts)
            {
                ArtifactWrapper.PublishArtifacts(_authorUser, savedOrPublishedArtifacts);
            }

            savedOrPublishedArtifacts.ForEach(artifact => Helper.ArtifactStore.AddArtifactToCollection(_authorUser, artifact.Id, collectionArtifact.Id));

            // Execution: Publish the collection that contains published artifacts
            INovaArtifactsAndProjectsResponse publishedResponse = null;

            Assert.DoesNotThrow(() => publishedResponse = Helper.ArtifactStore.PublishArtifact(collectionArtifact.Id, _authorUser),
                "POST {0} call failed when using it with collection (Id: {1}) contains published artifacts!", PUBLISH_PATH, collectionArtifact.Id);

            // Validation: Verify that published collection contains valid data added
            collectionArtifact.RefreshArtifactFromServer(_authorUser);
            var collectionArtifactList = new List<INovaArtifactDetails> { collectionArtifact };

            ArtifactStoreHelper.AssertArtifactsAndProjectsResponseContainsAllArtifactsInList(publishedResponse, collectionArtifactList);

            var collection = Helper.ArtifactStore.GetCollection(_authorUser, collectionArtifact.Id);

            ArtifactStoreHelper.ValidateCollection(collection, savedOrPublishedArtifacts.ConvertAll(a => (INovaArtifactDetails)a));
        }

        [TestCase]
        [TestRail(230693)]
        [Description("Publish a collection with a user doesn't have permission to its publshed artifact.  Verify the content of published collection with " +
                     "different permission based users.")]
        public void PublishArtifact_PublishCollectionWithUserWithNoPermissionToItsPublishedArtifact_VerifyCollectionWithDifferentPermissionBasedUsers()
        {
            // Setup: Create a collection which contains inaccessible published artifact to the user
            var authorWithoutPermission = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var collectionArtifact = Helper.CreateUnpublishedCollection(_project, authorWithoutPermission);
            var publishedArtifact = Helper.CreateAndPublishNovaArtifact(authorWithoutPermission, _project, ItemTypePredefined.Actor);

            Helper.ArtifactStore.AddArtifactToCollection(authorWithoutPermission, publishedArtifact.Id, collectionArtifact.Id);

            Helper.AssignProjectRolePermissionsToUser(authorWithoutPermission, TestHelper.ProjectRole.None, _project, publishedArtifact);
            
            // Execution: Publish the collection which contains inaccessible artifact to the user
            INovaArtifactsAndProjectsResponse publishedResponse = null;

            Assert.DoesNotThrow(() => publishedResponse = Helper.ArtifactStore.PublishArtifact(collectionArtifact.Id, authorWithoutPermission),
                "POST {0} call failed when using it with collection (Id: {1}) containing an inaccessible artifact to a user!",
                PUBLISH_PATH, collectionArtifact.Id);

            // Validation: Verify that collection published with users with different permission
            collectionArtifact.RefreshArtifactFromServer(authorWithoutPermission);
            var collectionArtifactList = new List<INovaArtifactDetails> { collectionArtifact };

            ArtifactStoreHelper.AssertArtifactsAndProjectsResponseContainsAllArtifactsInList(publishedResponse, collectionArtifactList);

            var collectionRetrievedByUserWithoutPermission = Helper.ArtifactStore.GetCollection(authorWithoutPermission, collectionArtifact.Id);

            ArtifactStoreHelper.ValidateCollection(collectionRetrievedByUserWithoutPermission, new List<INovaArtifactDetails>());

            var collectionRetrivedByUserWithPermission = Helper.ArtifactStore.GetCollection(_user, collectionArtifact.Id);
            var publishedArtifactList = new List<INovaArtifactDetails> { publishedArtifact };

            ArtifactStoreHelper.ValidateCollection(collectionRetrivedByUserWithPermission, publishedArtifactList);
        }

        [TestCase(1)]
        [TestCase(3)]
        [TestRail(230694)]
        [Description("Publish a collection that contains deleted artifacts. Verify the published collection doesn't contain deleted artifacts.")]
        public void PublishArtifact_PublishCollectionContainingDeletedArtifacts_VerifyCollection(int numberOfArtifacts)
        {
            // Setup: Create a collection and add artifacts in it
            var collectionArtifact = Helper.CreateUnpublishedCollection(_project, _authorUser);
            var publishedArtifacts = Helper.CreateAndPublishMultipleArtifacts(_project, _authorUser, ItemTypePredefined.Actor, numberOfArtifacts);
            
            publishedArtifacts.ForEach(artifact => Helper.ArtifactStore.AddArtifactToCollection(_authorUser, artifact.Id, collectionArtifact.Id));

            // Execution: Publish the collection with deleted artifacts
            var deletedArtifacts = new List<INovaArtifactResponse>();

            publishedArtifacts.ForEach(artifact => deletedArtifacts.AddRange(artifact.Delete(_authorUser)));

            INovaArtifactsAndProjectsResponse publishedResponse = null;

            Assert.DoesNotThrow(() => publishedResponse = Helper.ArtifactStore.PublishArtifact(collectionArtifact.Id, _authorUser),
                "POST {0} call failed when using it with collection (Id: {1}) contains deleted artifacts!", PUBLISH_PATH, collectionArtifact.Id);

            // Validation: Verify that collection response
            collectionArtifact.RefreshArtifactFromServer(_authorUser);
            var collectionArtifactList = new List<ArtifactWrapper> { collectionArtifact };

            ArtifactStoreHelper.AssertArtifactsAndProjectsResponseContainsAllArtifactsInList(
                publishedResponse, collectionArtifactList.ConvertAll(a => (INovaArtifactDetails)a));

            var collection = Helper.ArtifactStore.GetCollection(_authorUser, collectionArtifact.Id);

            ArtifactStoreHelper.ValidateCollection(collection, new List<INovaArtifactDetails>());
        }

        #endregion Publish Collection Artifact Tests

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForNovaRestMethods))]
        [TestRail(165856)]
        [Description("Create & save a single artifact.  Publish the artifact.  Verify publish is successful and that artifact version is now 1.")]
        public void PublishArtifact_SingleSavedArtifact_ArtifactHasVersion1(ItemTypePredefined artifactType)
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);

            var artifact = Helper.CreateNovaArtifact(author, _project, artifactType);
            var artifactHistoryBefore = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, author);

            Assert.AreEqual(int.MaxValue, artifactHistoryBefore[0].VersionId, "Version ID before publish should be {0}!", int.MaxValue);

            // Execute:
            INovaArtifactsAndProjectsResponse publishResponse = null;

            Assert.DoesNotThrow(() => publishResponse = Helper.ArtifactStore.PublishArtifact(artifact.Id, author),
                "'POST {0}' should return 200 OK if a valid artifact ID is sent!", PUBLISH_PATH);

            // Verify:
            ArtifactStoreHelper.AssertOnlyExpectedProjectWasReturned(publishResponse.Projects, _project);
            Assert.AreEqual(1, publishResponse.Artifacts.Count, "There should only be 1 published artifact returned!");
            ArtifactStoreHelper.AssertNovaArtifactResponsePropertiesMatchWithArtifact(publishResponse.Artifacts.First(), artifact, expectedVersion: 1);

            var artifactHistoryAfter = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, author);
            Assert.AreEqual(1, artifactHistoryAfter[0].VersionId, "Version ID after publish should be 1!");
        }

        [TestCase(ItemTypePredefined.Actor, 2)]
        [TestCase(ItemTypePredefined.Process, 3)]
        [TestRail(165968)]
        [Description("Create & publish a single artifact several times, then save to create a draft.  Publish the artifact.  " +
                     "Verify publish is successful and that artifact has the expected version.")]
        public void PublishArtifact_SinglePublishedArtifactWithMultipleVersionsWithDraft_ArtifactHasExpectedVersion(ItemTypePredefined artifactType, int numberOfVersions)
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);

            var artifactWithMultipleVersions = Helper.CreateAndPublishNovaArtifactWithMultipleVersions(author, _project, artifactType, numberOfVersions);

            var artifactHistoryBefore = Helper.ArtifactStore.GetArtifactHistory(artifactWithMultipleVersions.Id, author);
            Assert.AreEqual(numberOfVersions, artifactHistoryBefore[0].VersionId, "Version ID before Nova publish should be {0}!", numberOfVersions);

            artifactWithMultipleVersions.Lock(author);
            artifactWithMultipleVersions.SaveWithNewDescription(author);    // Now save to make a draft.

            // Execute:
            INovaArtifactsAndProjectsResponse publishResponse = null;

            Assert.DoesNotThrow(() => publishResponse = Helper.ArtifactStore.PublishArtifact(artifactWithMultipleVersions.Id, author),
                "'POST {0}' should return 200 OK if a valid artifact ID is sent!", PUBLISH_PATH);

            // Verify:
            int expectedVersion;

            checked  // This checked section is needed to fix warning:  CA2233: Operations should not overflow
            {
                expectedVersion = numberOfVersions + 1;
            }

            ArtifactStoreHelper.AssertOnlyExpectedProjectWasReturned(publishResponse.Projects, _project);
            Assert.AreEqual(1, publishResponse.Artifacts.Count, "There should only be 1 published artifact returned!");
            ArtifactStoreHelper.AssertNovaArtifactResponsePropertiesMatchWithArtifact(publishResponse.Artifacts.First(), artifactWithMultipleVersions.Artifact, expectedVersion);

            var artifactHistoryAfter = Helper.ArtifactStore.GetArtifactHistory(artifactWithMultipleVersions.Id, _user);

            Assert.AreEqual(expectedVersion, artifactHistoryAfter[0].VersionId, "Version ID after publish should be {0}!", expectedVersion);
        }

        [TestCase(ItemTypePredefined.Process, 3)]
        [TestRail(165956)]
        [Description("Create & save multiple artifacts.  Publish all the artifacts.  Verify publish is successful and that the version of the artifacts is now 1.")]
        public void PublishArtifact_MultipleSavedArtifacts_ArtifactsHaveVersion1(ItemTypePredefined artifactType, int numberOfArtifacts)
        {
            // Setup:
            var artifacts = Helper.CreateAndSaveMultipleArtifacts(_project, _user, artifactType, numberOfArtifacts);
            var artifactHistoryBefore = Helper.ArtifactStore.GetArtifactHistory(artifacts[0].Id, _user);

            Assert.AreEqual(int.MaxValue, artifactHistoryBefore[0].VersionId, "Version ID before publish should be {0}!", int.MaxValue);

            // Execute:
            INovaArtifactsAndProjectsResponse publishResponse = null;

            Assert.DoesNotThrow(() => publishResponse = Helper.ArtifactStore.PublishArtifacts(artifacts.Select(a => a.Id), _user),
                "'POST {0}' should return 200 OK if a valid list of artifact IDs is sent!", PUBLISH_PATH);

            artifacts.ForEach(a => a.UpdateArtifactState(ArtifactWrapper.ArtifactOperation.Publish));

            // Verify:
            ArtifactStoreHelper.AssertOnlyExpectedProjectWasReturned(publishResponse.Projects, _project);
            Assert.AreEqual(numberOfArtifacts, publishResponse.Artifacts.Count, "There should only be {0} published artifact returned!", numberOfArtifacts);

            AssertPublishedArtifactResponseContainsAllArtifactsInListAndHasExpectedVersion(publishResponse, artifacts, expectedVersion: 1);
        }

        [TestCase(2, ItemTypePredefined.Actor, ItemTypePredefined.Document, ItemTypePredefined.Glossary)]
        [TestCase(3, ItemTypePredefined.Process, ItemTypePredefined.TextualRequirement, ItemTypePredefined.UseCase)]
        [TestRail(165969)]
        [Description("Create & publish a multiple artifacts several times and save to create drafts.  Publish the artifacts.  " +
                     "Verify publish is successful and that the artifacts have the expected versions.")]
        public void PublishArtifact_MultiplePublishedArtifactsWithMultipleVersionsWithDraft_ArtifactHasExpectedVersion(int numberOfVersions, params ItemTypePredefined[] artifactTypes)
        {
            ThrowIf.ArgumentNull(artifactTypes, nameof(artifactTypes));

            // Setup:
            var artifactsWithMultipleVersions = new List<ArtifactWrapper>();

            foreach (var artifactType in artifactTypes)
            {
                var artifactWithMultipleVersions = Helper.CreateAndPublishNovaArtifactWithMultipleVersions(_user, _project, artifactType, numberOfVersions);
                artifactsWithMultipleVersions.Add(artifactWithMultipleVersions);

                var artifactHistoryBefore = Helper.ArtifactStore.GetArtifactHistory(artifactWithMultipleVersions.Id, _user);
                Assert.AreEqual(numberOfVersions, artifactHistoryBefore[0].VersionId, "Version ID before Nova publish should be {0}!", numberOfVersions);

                artifactWithMultipleVersions.Lock(_user);
                artifactWithMultipleVersions.SaveWithNewDescription(_user);    // Now save to make a draft.
            }

            // Execute:
            INovaArtifactsAndProjectsResponse publishResponse = null;

            Assert.DoesNotThrow(() => publishResponse = Helper.ArtifactStore.PublishArtifacts(artifactsWithMultipleVersions.Select(a => a.Id), _user),
                "'POST {0}' should return 200 OK if a valid list of artifact IDs is sent!", PUBLISH_PATH);

            artifactsWithMultipleVersions.ForEach(a => a.UpdateArtifactState(ArtifactWrapper.ArtifactOperation.Publish));

            // Verify:
            ArtifactStoreHelper.AssertOnlyExpectedProjectWasReturned(publishResponse.Projects, _project);
            Assert.AreEqual(artifactsWithMultipleVersions.Count, publishResponse.Artifacts.Count,
                "There should only be {0} published artifact returned!", artifactsWithMultipleVersions.Count);

            int expectedVersion;

            checked  // This checked section is needed to fix warning:  CA2233: Operations should not overflow
            {
                expectedVersion = numberOfVersions + 1;
            }

            AssertPublishedArtifactResponseContainsAllArtifactsInListAndHasExpectedVersion(publishResponse, artifactsWithMultipleVersions, expectedVersion);
        }

        [TestCase(ItemTypePredefined.Actor, 2, ItemTypePredefined.Process, 3, ItemTypePredefined.UseCase, 2)]
        [TestRail(165970)]
        [Description("Create multiple artifacts (some saved, some published & some published with drafts).  Publish all the artifacts with unpublished changes.  " +
                     "Verify publish is successful and that the version of the artifacts have the expected versions.")]
        public void PublishArtifact_MultipleSavedAndPublishedArtifactsSomeWithDrafts_PublishArtifactsWithUnpublishedChanges_ArtifactsHaveExpectedVersions(
            ItemTypePredefined savedArtifactType, int numberOfSavedArtifacts,
            ItemTypePredefined publishedWithDraftArtifactType, int numberOfPublishedWithDraftArtifacts,
            ItemTypePredefined publishedArtifactType, int numberOfPublishedArtifacts)
        {
            // Setup:
            var savedArtifacts = Helper.CreateAndSaveMultipleArtifacts(_project, _user, savedArtifactType, numberOfSavedArtifacts);
            var publishedArtifacts = Helper.CreateAndPublishMultipleArtifacts(_project, _user, publishedArtifactType, numberOfPublishedArtifacts);
            var publishedWithDraftArtifacts = Helper.CreateAndPublishMultipleArtifacts(_project, _user, publishedWithDraftArtifactType, numberOfPublishedWithDraftArtifacts);

            ArtifactWrapper.LockArtifacts(_user, publishedWithDraftArtifacts);
            publishedWithDraftArtifacts.ForEach(a => a.SaveWithNewDescription(_user));

            var artifactsToPublish = new List<ArtifactWrapper>();
            artifactsToPublish.AddRange(savedArtifacts);
            artifactsToPublish.AddRange(publishedWithDraftArtifacts);

            // Execute:
            INovaArtifactsAndProjectsResponse publishResponse = null;

            Assert.DoesNotThrow(() => publishResponse = Helper.ArtifactStore.PublishArtifacts(artifactsToPublish.Select(a => a.Id), _user),
                "'POST {0}' should return 200 OK if a valid list of artifact IDs is sent!", PUBLISH_PATH);

            artifactsToPublish.ForEach(a => a.UpdateArtifactState(ArtifactWrapper.ArtifactOperation.Publish));

            // Verify:
            ArtifactStoreHelper.AssertOnlyExpectedProjectWasReturned(publishResponse.Projects, _project);
            Assert.AreEqual(artifactsToPublish.Count, publishResponse.Artifacts.Count,
                "There should only be {0} published artifact returned!", artifactsToPublish.Count);

            AssertPublishedArtifactResponseContainsAllArtifactsInListAndHasExpectedVersion(publishResponse, savedArtifacts, expectedVersion: 1);
            AssertPublishedArtifactResponseContainsAllArtifactsInListAndHasExpectedVersion(publishResponse, publishedWithDraftArtifacts, expectedVersion: 2);

            // Verify the published artifacts without drafts didn't get published again.
            AssertArtifactsVersionEquals(publishedArtifacts, expectedVersion: 1);
        }

        [TestCase(ItemTypePredefined.Process, 3, 2, null)]
        [TestCase(ItemTypePredefined.UseCase, 2, 3, false)]
        [TestRail(165977)]
        [Description("Create & save multiple artifacts.  Publish some of the artifacts (pass all=false or don't pass the all parameter).  " +
                     "Verify publish is successful and that only the artifacts we wanted to publish got published.")]
        public void PublishArtifactWithAllNullOrFalse_MultipleSavedArtifacts_OnlyPublishSome_ArtifactsHaveExpectedVersion(
            ItemTypePredefined artifactType, int numberOfArtifactsToPublish, int numberOfArtifactsToNotPublish, bool? all)
        {
            // Setup:
            var artifactsToPublish = Helper.CreateAndSaveMultipleArtifacts(_project, _user, artifactType, numberOfArtifactsToPublish);
            var artifactsNotToPublish = Helper.CreateAndSaveMultipleArtifacts(_project, _user, artifactType, numberOfArtifactsToNotPublish);

            // Execute:
            INovaArtifactsAndProjectsResponse publishResponse = null;

            Assert.DoesNotThrow(() => publishResponse = Helper.ArtifactStore.PublishArtifacts(artifactsToPublish.Select(a => a.Id), _user, all),
                "'POST {0}{1}' should return 200 OK if a valid list of artifact IDs is sent!",
                PUBLISH_PATH, (all == null) ? string.Empty : I18NHelper.FormatInvariant("?all={0}", all.Value.ToString()));

            artifactsToPublish.ForEach(a => a.UpdateArtifactState(ArtifactWrapper.ArtifactOperation.Publish));

            // Verify:
            ArtifactStoreHelper.AssertOnlyExpectedProjectWasReturned(publishResponse.Projects, _project);
            Assert.AreEqual(numberOfArtifactsToPublish, publishResponse.Artifacts.Count,
                "There should only be {0} published artifact returned!", numberOfArtifactsToPublish);

            AssertPublishedArtifactResponseContainsAllArtifactsInListAndHasExpectedVersion(publishResponse, artifactsToPublish, expectedVersion: 1);

            AssertArtifactsWereNotPublished(artifactsNotToPublish);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForNovaRestMethods))]
        [TestRail(165978)]
        [Description("Create a single published artifact.  Delete the artifact, then publish it.  Verify the artifact is deleted by trying to get it with another user.")]
        public void PublishArtifact_SingleDeletedArtifact_ArtifactIsDeleted(ItemTypePredefined artifactType)
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var artifact = Helper.CreateAndPublishNovaArtifact(author, _project, artifactType);
            artifact.Delete(author);

            // Execute:
            INovaArtifactsAndProjectsResponse publishResponse = null;

            Assert.DoesNotThrow(() => publishResponse = Helper.ArtifactStore.PublishArtifact(artifact.Id, author),
                "'POST {0}' should return 200 OK if a valid artifact ID is sent!", PUBLISH_PATH);

            artifact.UpdateArtifactState(ArtifactWrapper.ArtifactOperation.Publish);

            // Verify:
            ArtifactStoreHelper.AssertOnlyExpectedProjectWasReturned(publishResponse.Projects, _project);
            Assert.AreEqual(1, publishResponse.Artifacts.Count, "There should only be 1 published artifact returned!");

            var viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);

            Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetArtifactDetails(viewer, artifact.Id),
                "After publishing a deleted artifact, other users should not be able to get the deleted artifact!");
        }

        [TestCase(ItemTypePredefined.UseCase, 2, 3)]
        [TestRail(165979)]
        [Description("Create & save multiple artifacts.  Publish with all=true but only pass some of the artifacts.  " +
                     "Verify publish is successful and that all the artifacts we created were published.")]
        public void PublishArtifactWithAllTrue_MultipleSavedArtifacts_OnlyPublishSome_ArtifactsHaveExpectedVersion(
            ItemTypePredefined artifactType, int numberOfArtifactsToPublish, int numberOfArtifactsToNotPublish)
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var artifactsPassedToPublish = Helper.CreateAndSaveMultipleArtifacts(_project, author, artifactType, numberOfArtifactsToPublish);
            var artifactsNotPassedToPublish = Helper.CreateAndSaveMultipleArtifacts(_project, author, artifactType, numberOfArtifactsToNotPublish);

            // Execute:
            INovaArtifactsAndProjectsResponse publishResponse = null;

            Assert.DoesNotThrow(() => publishResponse = Helper.ArtifactStore.PublishArtifacts(artifactsPassedToPublish.Select(a => a.Id), author, publishAll: true),
                "'POST {0}?all=true' should return 200 OK if a valid list of artifact IDs is sent!", PUBLISH_PATH);

            // Verify:
            var allArtifacts = new List<ArtifactWrapper>();
            allArtifacts.AddRange(artifactsPassedToPublish);
            allArtifacts.AddRange(artifactsNotPassedToPublish);

            allArtifacts.ForEach(a => a.UpdateArtifactState(ArtifactWrapper.ArtifactOperation.Publish));

            ArtifactStoreHelper.AssertOnlyExpectedProjectWasReturned(publishResponse.Projects, _project);
            Assert.AreEqual(allArtifacts.Count, publishResponse.Artifacts.Count, "There should only be {0} published artifact returned!", allArtifacts.Count);

            AssertPublishedArtifactResponseContainsAllArtifactsInListAndHasExpectedVersion(publishResponse, allArtifacts, expectedVersion: 1);
        }

        [TestCase(ItemTypePredefined.Process, 3)]
        [TestRail(165980)]
        [Description("Create & save multiple artifacts.  Publish with all=true but don't pass any of the artifacts.  " +
                     "Verify publish is successful and that all the artifacts we created were published.")]
        public void PublishArtifactWithAllTrue_MultipleSavedArtifacts_SendEmptyListToPublish_ArtifactsHaveExpectedVersion(
            ItemTypePredefined artifactType, int numberOfArtifacts)
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var allArtifacts = Helper.CreateAndSaveMultipleArtifacts(_project, author, artifactType, numberOfArtifacts);

            // Execute:
            INovaArtifactsAndProjectsResponse publishResponse = null;

            Assert.DoesNotThrow(() => publishResponse = Helper.ArtifactStore.PublishAllArtifacts(author),
                "'POST {0}?all=true' should return 200 OK if an empty list of artifact IDs is sent!", PUBLISH_PATH);

            allArtifacts.ForEach(a => a.UpdateArtifactState(ArtifactWrapper.ArtifactOperation.Publish));

            // Verify:
            ArtifactStoreHelper.AssertOnlyExpectedProjectWasReturned(publishResponse.Projects, _project);
            Assert.AreEqual(allArtifacts.Count, publishResponse.Artifacts.Count,
                "There should only be {0} published artifact returned!", allArtifacts.Count);

            AssertPublishedArtifactResponseContainsAllArtifactsInListAndHasExpectedVersion(publishResponse, allArtifacts, expectedVersion: 1);
        }

        [TestCase(ItemTypePredefined.Process, 3, ItemTypePredefined.UseCase, 2)]
        [TestRail(166018)]
        [Description("Create multiple artifacts (some saved and others published).  Publish with all=true but don't pass any of the artifacts.  " +
                     "Verify publish is successful and that only the unpublished artifacts we created were published.")]
        public void PublishArtifactWithAllTrue_MultipleSavedArtifactsAndPublishedArtifacts_SendEmptyListToPublish_ArtifactsHaveExpectedVersion(
            ItemTypePredefined savedArtifactType, int numberOfSavedArtifacts, ItemTypePredefined publishedArtifactType, int numberOfPublishedArtifacts)
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
                
            var savedArtifacts = Helper.CreateAndSaveMultipleArtifacts(_project, author, savedArtifactType, numberOfSavedArtifacts);
            var publishedArtifacts = Helper.CreateAndPublishMultipleArtifacts(_project, author, publishedArtifactType, numberOfPublishedArtifacts);

            // Execute:
            INovaArtifactsAndProjectsResponse publishResponse = null;

            Assert.DoesNotThrow(() => publishResponse = Helper.ArtifactStore.PublishAllArtifacts(author),
                "'POST {0}?all=true' should return 200 OK if an empty list of artifact IDs is sent!", PUBLISH_PATH);

            savedArtifacts.ForEach(a => a.UpdateArtifactState(ArtifactWrapper.ArtifactOperation.Publish));

            // Verify:
            ArtifactStoreHelper.AssertOnlyExpectedProjectWasReturned(publishResponse.Projects, _project);
            Assert.AreEqual(savedArtifacts.Count, publishResponse.Artifacts.Count,
                "There should only be {0} published artifact returned!", savedArtifacts.Count);

            AssertPublishedArtifactResponseContainsAllArtifactsInListAndHasExpectedVersion(publishResponse, savedArtifacts, expectedVersion: 1);

            AssertArtifactsVersionEquals(publishedArtifacts, expectedVersion: 1);
        }

        [TestCase(ItemTypePredefined.Process, 3)]
        [TestRail(166019)]
        [Description("Create & save artifacts in multiple projects.  Publish all the artifacts.  Verify publish is successful and that the version of the artifacts is now 1.")]
        public void PublishArtifact_ArtifactsSavedInMultipleProjects_ArtifactsHaveVersion1(ItemTypePredefined artifactType, int numberOfArtifacts)
        {
            // Setup:
            var projects = ProjectFactory.GetProjects(_user, numberOfProjects: 2);

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, projects);

            var firstProject = projects[0];
            var secondProject = projects[1];

            var artifactsInFirstProject = Helper.CreateAndSaveMultipleArtifacts(firstProject, author, artifactType, numberOfArtifacts);
            var artifactsInSecondProject = Helper.CreateAndSaveMultipleArtifacts(secondProject, author, artifactType, numberOfArtifacts);

            var allArtifacts = new List<ArtifactWrapper>();
            allArtifacts.AddRange(artifactsInFirstProject);
            allArtifacts.AddRange(artifactsInSecondProject);

            // Execute:
            INovaArtifactsAndProjectsResponse publishResponse = null;

            Assert.DoesNotThrow(() => publishResponse = Helper.ArtifactStore.PublishArtifacts(allArtifacts.Select(a => a.Id), author),
                "'POST {0}' should return 200 OK if a valid list of artifact IDs is sent!", PUBLISH_PATH);

            allArtifacts.ForEach(a => a.UpdateArtifactState(ArtifactWrapper.ArtifactOperation.Publish));

            // Verify:
            ArtifactStoreHelper.AssertAllExpectedProjectsWereReturned(publishResponse.Projects, projects);
            Assert.AreEqual(allArtifacts.Count, publishResponse.Artifacts.Count, "There should be {0} published artifacts returned!", allArtifacts.Count);

            AssertPublishedArtifactResponseContainsAllArtifactsInListAndHasExpectedVersion(publishResponse, artifactsInFirstProject, expectedVersion: 1);
        }

        [TestCase(ItemTypePredefined.Process, 3)]
        [TestRail(166020)]
        [Description("Create & save artifacts in multiple projects.  Publish all the artifacts.  Verify publish is successful and that the version of the artifacts is now 1.")]
        public void PublishArtifactWithAllTrue_ArtifactsSavedInMultipleProjects_SendEmptyListToPublish_ArtifactsHaveVersion1(ItemTypePredefined artifactType, int numberOfArtifacts)
        {
            // Setup:
            var projects = ProjectFactory.GetProjects(_user, numberOfProjects: 2);

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, projects);

            var firstProject = projects[0];
            var secondProject = projects[1];

            var artifactsInFirstProject = Helper.CreateAndSaveMultipleArtifacts(firstProject, author, artifactType, numberOfArtifacts);
            var artifactsInSecondProject = Helper.CreateAndSaveMultipleArtifacts(secondProject, author, artifactType, numberOfArtifacts);

            var allArtifacts = new List<ArtifactWrapper>();
            allArtifacts.AddRange(artifactsInFirstProject);
            allArtifacts.AddRange(artifactsInSecondProject);

            // Execute:
            INovaArtifactsAndProjectsResponse publishResponse = null;

            Assert.DoesNotThrow(() => publishResponse = Helper.ArtifactStore.PublishAllArtifacts(author),
                "'POST {0}?all=true' should return 200 OK if an empty list of artifact IDs is sent!", PUBLISH_PATH);

            allArtifacts.ForEach(a => a.UpdateArtifactState(ArtifactWrapper.ArtifactOperation.Publish));

            // Verify:
            ArtifactStoreHelper.AssertAllExpectedProjectsWereReturned(publishResponse.Projects, projects);
            Assert.AreEqual(allArtifacts.Count, publishResponse.Artifacts.Count, "There should be {0} published artifacts returned!", allArtifacts.Count);

            AssertPublishedArtifactResponseContainsAllArtifactsInListAndHasExpectedVersion(
                publishResponse, artifactsInFirstProject, expectedVersion: 1);
        }

        [TestCase(ItemTypePredefined.Process)]
        [TestRail(191079)]
        [Description("Create, publish & save artifacts in a couple of projects.  Publish all the artifacts.  User has permissions only for one artifact in 2nd project.  " +
                     "Verify publish is successful.")]
        public void PublishArtifactWithAllTrue_ArtifactsSavedInCoupleOfProjects_UserHasPermissionsOnlyToOneArtifactInSecondProjects(ItemTypePredefined artifactType)
        {
            // Setup:
            var projects = ProjectFactory.GetProjects(_user, numberOfProjects: 2);
            var firstProject = projects[0];
            var secondProject = projects[1];

            // User has author rights for first project but none permissions for the second project
            var user = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, firstProject);
            Helper.AssignProjectRolePermissionsToUser(user, TestHelper.ProjectRole.None, secondProject);

            var artifactInProject1 = Helper.CreateAndPublishNovaArtifact(user, firstProject, artifactType);
            artifactInProject1.Lock(user);
            artifactInProject1.SaveWithNewDescription(user);

            // Create & publish artifact with user that has permissions to the project
            var artifactInProject2 = Helper.CreateAndPublishNovaArtifact(_user, secondProject, artifactType);

            // Allow editing for previously created & published artifact with user that does not have permissions for that project
            Helper.AssignProjectRolePermissionsToUser(user, TestHelper.ProjectRole.Author, secondProject, artifactInProject2);

            artifactInProject2.Lock(user);
            artifactInProject2.SaveWithNewDescription(user);

            var allArtifacts = new List<ArtifactWrapper>();
            allArtifacts.Add(artifactInProject1);
            allArtifacts.Add(artifactInProject2);

            INovaArtifactsAndProjectsResponse publishResponse = null;

            // Execute:
            Assert.DoesNotThrow(() => publishResponse = Helper.ArtifactStore.PublishAllArtifacts(user),
                "'POST {0}?all=true' should return 200 OK if an empty list of artifact IDs is sent!", PUBLISH_PATH);

            allArtifacts.ForEach(a => a.UpdateArtifactState(ArtifactWrapper.ArtifactOperation.Publish));

            // Verify:
            ArtifactStoreHelper.AssertAllExpectedProjectsWereReturned(publishResponse.Projects, projects);
            Assert.AreEqual(allArtifacts.Count, publishResponse.Artifacts.Count, "There should be {0} published artifacts returned!", allArtifacts.Count);

            AssertPublishedArtifactResponseContainsAllArtifactsInListAndHasExpectedVersion(publishResponse, allArtifacts, expectedVersion: 2);
        }

        [TestCase(BaselineAndCollectionTypePredefined.ArtifactCollection)]
        [TestCase(BaselineAndCollectionTypePredefined.CollectionFolder)]
        [TestRail(191154)]
        [Description("Create collection artifact or collection folder.  Publish it.  Verify the published collection artifact or collection folder is returned with proper content.")]
        public void PublishArtifact_CollectionOrCollectionFolder_ReturnsPublishedArtifact(BaselineAndCollectionTypePredefined artifactType)
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var artifact = Helper.CreateCollectionOrCollectionFolder(_project, author, artifactType);

            INovaArtifactsAndProjectsResponse publishResponse = null;

            // Execute:
            Assert.DoesNotThrow(() => publishResponse = Helper.ArtifactStore.PublishArtifact(artifact.Id, author),
                    "'POST {0} should return 200 OK if a valid artifact ID is sent!", PUBLISH_PATH);

            artifact.UpdateArtifactState(ArtifactWrapper.ArtifactOperation.Publish);

            // Verify:
            var expectedProjects = new List<IProject> { _project };

            ArtifactStoreHelper.AssertAllExpectedProjectsWereReturned(publishResponse.Projects, expectedProjects);
            artifact.RefreshArtifactFromServer(author);
            NovaArtifactDetails.AssertArtifactsEqual(publishResponse.Artifacts.Find(a => a.Id == artifact.Id), artifact);
        }

        [TestCase()]
        [TestRail(191155)]
        [Description("Create & save collection artifact and collection folder.  Publish them with all=true.  " + 
                     "Verify the published collection artifact and collection folder have been returned with proper content.")]
        public void PublishAllArtifacts_CollectionAndCollectionFolder_ReturnsPublishedArtifact()
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var collectionFolder = Helper.CreateUnpublishedCollectionFolder(_project, author);
            var collectionArtifact = Helper.CreateUnpublishedCollection(_project, author, collectionFolder.Id);

            INovaArtifactsAndProjectsResponse publishResponse = null;

            // Execute:
            Assert.DoesNotThrow(() => publishResponse = Helper.ArtifactStore.PublishAllArtifacts(author),
                    "'POST {0}?all=true' should return 200 OK if an empty list of artifact IDs is sent!", PUBLISH_PATH);

            collectionArtifact.UpdateArtifactState(ArtifactWrapper.ArtifactOperation.Publish);
            collectionFolder.UpdateArtifactState(ArtifactWrapper.ArtifactOperation.Publish);

            // Verify:
            var expectedProjects = new List<IProject> { _project };

            ArtifactStoreHelper.AssertAllExpectedProjectsWereReturned(publishResponse.Projects, expectedProjects);

            collectionFolder.RefreshArtifactFromServer(author);
            NovaArtifactDetails.AssertArtifactsEqual(publishResponse.Artifacts.Find(a => a.Id == collectionFolder.Id), collectionFolder);

            collectionArtifact.RefreshArtifactFromServer(author);
            NovaArtifactDetails.AssertArtifactsEqual(publishResponse.Artifacts.Find(a => a.Id == collectionArtifact.Id), collectionArtifact);
        }

        [TestCase(BaselineAndCollectionTypePredefined.ArtifactCollection)]
        [TestCase(BaselineAndCollectionTypePredefined.CollectionFolder)]
        [TestRail(191156)]
        [Description("Create & Save collection artifact or collection folder.  Publish it.  Verify the published collection artifact or collection folder is returned with proper content.")]
        public void PublishArtifact_UpdateCollectionOrCollectionFolder_ReturnsPublishedArtifact(BaselineAndCollectionTypePredefined artifactType)
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var artifact = Helper.CreateCollectionOrCollectionFolder(_project, author, artifactType);
            artifact.SaveWithNewDescription(author);

            INovaArtifactsAndProjectsResponse publishResponse = null;

            // Execute:
            Assert.DoesNotThrow(() => publishResponse = Helper.ArtifactStore.PublishArtifact(artifact.Id, author),
                    "'POST {0} should return 200 OK if an empty list of artifact IDs is sent!", PUBLISH_PATH);

            artifact.UpdateArtifactState(ArtifactWrapper.ArtifactOperation.Publish);

            // Verify:
            var expectedProjects = new List<IProject> { _project };

            ArtifactStoreHelper.AssertAllExpectedProjectsWereReturned(publishResponse.Projects, expectedProjects);
            artifact.RefreshArtifactFromServer(author);
            NovaArtifactDetails.AssertArtifactsEqual(publishResponse.Artifacts[0], artifact);
        }

        [Explicit(IgnoreReasons.UnderDevelopmentDev)] // User Story: 3578  [Nova] [Collection] Edit a Collection Folder / Collection Container
        [Category(Categories.CannotRunInParallel)]
        [TestCase]
        [TestRail(191158)]
        [Description("Change the description of the default collection folder.  Publish it.  Verify the published collection artifact or collection folder is returned with proper content.")]
        public void PublishArtifact_UpdateDefaultCollectionFolder_ReturnsPublishedArtifact()
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var defaultCollectionFolder = _project.GetDefaultCollectionFolder(author);

            var novaArtifact = Helper.ArtifactStore.GetArtifactDetails(author, defaultCollectionFolder.Id);
            var artifact = Helper.WrapArtifact(novaArtifact, _project, author);

            artifact.Lock(author);
            artifact.SaveWithNewDescription(author);

            INovaArtifactsAndProjectsResponse publishResponse = null;

            // Execute:
            Assert.DoesNotThrow(() => publishResponse = Helper.ArtifactStore.PublishArtifact(artifact.Id, author),
                    "'POST {0} should return 200 OK if an empty list of artifact IDs is sent!", PUBLISH_PATH);

            // Verify:
            var expectedProjects = new List<IProject> { _project };

            ArtifactStoreHelper.AssertAllExpectedProjectsWereReturned(publishResponse.Projects, expectedProjects);
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, artifact.Id);
            NovaArtifactDetails.AssertArtifactsEqual(publishResponse.Artifacts[0], artifactDetails);
        }

        #endregion 200 OK Tests

        #region 400 Bad Request tests

        [TestCase]
        [TestRail(165971)]
        [Description("Send empty list of artifacts, checks returned result is 400 Bad Request.")]
        public void PublishArtifact_EmptyArtifactList_BadRequest()
        {
            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => Helper.ArtifactStore.PublishArtifacts(new List<int>(), _user),
            "'POST {0}' should return 400 Bad Request if body of the request does not have any artifact ids!", PUBLISH_PATH);

            // Verify:
            string expectedExceptionMessage = "The list of artifact Ids is empty.";
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.IncorrectInputParameters, expectedExceptionMessage);
        }

        #endregion 400 Bad Request tests

        #region 401 Unauthorized tests

        [TestCase(ItemTypePredefined.Actor)]
        [TestRail(165975)]
        [Description("Create & save a single artifact.  Publish the artifact with wrong token.  Verify publish returns code 401 Unauthorized.")]
        public void PublishArtifact_InvalidToken_Unauthorized(ItemTypePredefined artifactType)
        {
            // Setup:
            var artifact = Helper.CreateNovaArtifact(_user, _project, artifactType);

            var userWithBadToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() => Helper.ArtifactStore.PublishArtifact(artifact.Id, userWithBadToken),
                "'POST {0}' should return 401 Unauthorized if a token is invalid!", PUBLISH_PATH);
            
            // Verify:
            const string expectedMessage = "Unauthorized call";
            TestHelper.ValidateBodyContents(ex.RestResponse, expectedMessage);
        }

        #endregion 401 Unauthorized tests

        #region 404 Not Found tests

        [TestCase(ItemTypePredefined.Process)]
        [TestRail(165973)]
        [Description("Create, publish, delete & publish an artifact by another user, checks returned result is 404 Not Found.")]
        public void PublishArtifact_PublishedArtifactDeletedByAnotherUser_NotFound(ItemTypePredefined artifactType)
        {
            // Setup:
            var anotherUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);
            var artifact = Helper.CreateAndPublishNovaArtifact(anotherUser, _project, artifactType);

            artifact.Delete(anotherUser);
            artifact.Publish(anotherUser);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.PublishArtifact(artifact.Id, _user),
                "'POST {0}' should return 404 Not Found if the Artifact ID doesn't exist!", PUBLISH_PATH);

            // Verify:
            string expectedExceptionMessage = I18NHelper.FormatInvariant("Artifact with ID {0} is deleted.", artifact.Id);
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound, expectedExceptionMessage);
        }

        [TestCase(int.MaxValue)]
        [TestRail(165976)]
        [Description("Try to publish an artifact with a non-existent Artifact ID.  Verify 404 Not Found is returned.")]
        public void PublishArtifact_NonExistentArtifactId_NotFound(int nonExistentArtifactId)
        {
            // Setup:
            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.PublishArtifact(nonExistentArtifactId, _user),
                "'POST {0}' should return 404 Not Found if the Artifact ID doesn't exist!", PUBLISH_PATH);

            // Verify:
            string expectedExceptionMessage = I18NHelper.FormatInvariant("Item with ID {0} is not found.", nonExistentArtifactId);
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound, expectedExceptionMessage);
        }

        #endregion 404 Not Found tests

        #region 409 Conflict tests

        [TestCase(ItemTypePredefined.Actor)]
        [TestRail(165972)]
        [Description("Create & publish an artifact.  Try to publish it again.  Verify 409 Conflict is returned for artifact that is already published.")]
        public void PublishArtifact_SinglePublishedArtifact_409Conflict(ItemTypePredefined artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, _project, artifactType);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => Helper.ArtifactStore.PublishArtifact(artifact.Id, _user),
                "'POST {0}' should return 409 Conflict if an artifact already published!", PUBLISH_PATH);

            // Verify:
            string expectedExceptionMessage = I18NHelper.FormatInvariant("Artifact with ID {0} has nothing to publish. The artifact will now be refreshed.", artifact.Id);
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotPublish, expectedExceptionMessage);
        }

        [TestCase(ItemTypePredefined.Process, 1)]
        [TestCase(ItemTypePredefined.Process, 2)]
        [TestRail(165974)]
        [Description("Create, save, parent artifact with two children, publish child artifact, checks returned result is 409 Conflict.")]
        public void PublishArtifact_ParentAndChildArtifacts_OnlyPublishChild_409Conflict(ItemTypePredefined artifactType, int index)
        {
            // Setup:
            var artifactList = CreateParentAndTwoChildrenArtifactsAndGetAllArtifacts(artifactType);
            var childArtifact = artifactList[index];

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => Helper.ArtifactStore.PublishArtifact(childArtifact.Id, _user),
                "'POST {0}' should return 409 Conflict if the Artifact has parent artifact which is not published!", PUBLISH_PATH);

            // Verify:
            // TODO: Also verify the 'errorContent' property that contains the dependent artifacts.
            string expectedExceptionMessage = "Specified artifacts have dependent artifacts to publish.";
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotPublishOverDependencies, expectedExceptionMessage);
        }

        #region Custom data tests

        [Category(Categories.CustomData)]
        [TestCase("value\":10.0", "value\":999.0")] //Insert value into Numeric field which is out of range
        [TestCase("value\":\"20", "value\":\"21")] //Insert value into Date field which is out of range
        [TestRail(166007)]
        [Description("Try to publish an artifact with a value of property that out of its permitted range. Verify 409 Conflict is returned.")]
        public void PublishArtifact_PropertyOutOfRange_409Conflict(string toChange, string changeTo)
        {
            // Setup:
            var projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, projectCustomData, ItemTypePredefined.Actor);
            artifact.Lock(_user);

            // This is needed to suppress 501 error.
            artifact.ItemTypeId = null;

            string requestBody = JsonConvert.SerializeObject(artifact.Artifact);

            requestBody = requestBody.Replace(toChange, changeTo);

            Assert.DoesNotThrow(() => ArtifactStoreHelper.UpdateInvalidArtifact(Helper.BlueprintServer.Address, requestBody, artifact.Id, _user),
                "'PATCH {0}' should return 200 OK if properties are out of range!", UPDATE_ARTIFACT_ID_PATH);

            artifact.UpdateArtifactState(ArtifactWrapper.ArtifactOperation.Update);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => Helper.ArtifactStore.PublishArtifact(artifact.Id, _user),
                "'POST {0}' should return 409 Conflict if an artifact already published!", PUBLISH_PATH);

            // Verify:
            string expectedExceptionMessage = I18NHelper.FormatInvariant("Artifact with ID {0} has validation errors.", artifact.Id);
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotPublishOverValidationErrors, expectedExceptionMessage);
        }

        [TestCase("value\":10.0", "value\":999.0", ItemTypePredefined.Actor, 0)] //Insert value into Numeric field which is out of range in grandparent artifact
        [TestCase("value\":10.0", "value\":999.0", ItemTypePredefined.Actor, 1)] //Insert value into Numeric field which is out of range in parent artifact
        [TestCase("value\":10.0", "value\":999.0", ItemTypePredefined.Actor, 2)] //Insert value into Numeric field which is out of range in child artifact
        [TestCase("value\":\"20", "value\":\"21", ItemTypePredefined.Actor, 0)] //Insert value into Date field which is out of range in grandparent artifact
        [TestCase("value\":\"20", "value\":\"21", ItemTypePredefined.Actor, 1)] //Insert value into Date field which is out of range in parent artifact
        [TestCase("value\":\"20", "value\":\"21", ItemTypePredefined.Actor, 2)] //Insert value into Date field which is out of range in child artifact
        [Category(Categories.CustomData)]
        [TestRail(166129)]
        [Description("Try to publish an artifact with a value of property that out of its permitted range. Verify 409 Conflict is returned.")]
        public void PublishAllArtifacts_PropertyOutOfRange_409Conflict(string toChange, string changeTo, ItemTypePredefined artifactType, int index)
        {
            // Setup:
            var projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);

            var artifactTypes = new ItemTypePredefined[] { artifactType, artifactType, artifactType };
            var artifactList = Helper.CreatePublishedArtifactChain(projectCustomData, _user, artifactTypes);
            artifactList[index].Lock(_user);

            // This is needed to suppress 501 error.
            artifactList[index].ItemTypeId = null;

            string requestBody = JsonConvert.SerializeObject(artifactList[index].Artifact);

            requestBody = requestBody.Replace(toChange, changeTo);

            Assert.DoesNotThrow(() => ArtifactStoreHelper.UpdateInvalidArtifact(Helper.BlueprintServer.Address, requestBody, artifactList[index].Id, _user),
                "'PATCH {0}' should return 200 OK if properties are out of range!", UPDATE_ARTIFACT_ID_PATH);

            artifactList[index].UpdateArtifactState(ArtifactWrapper.ArtifactOperation.Update);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => Helper.ArtifactStore.PublishAllArtifacts(_user),
                "'POST {0}' should return 409 Conflict if an artifact already published!", PUBLISH_PATH);

            // Verify:
            string expectedExceptionMessage = I18NHelper.FormatInvariant("Artifact with ID {0} has validation errors.", artifactList[index].Id);
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotPublishOverValidationErrors, expectedExceptionMessage);
        }

        [Category(Categories.CustomData)]
        [TestCase(ItemTypePredefined.Actor, "Actor - Required Date", "Std-Date-Required")]
        [TestCase(ItemTypePredefined.Process, "Process - Required Number", "Std-Number-Required")]
        [TestCase(ItemTypePredefined.PrimitiveFolder, "Folder - Required Text", "Std-Text-Required")]
        [TestCase(ItemTypePredefined.Document, "Document - Required Choice", "Std-Choice-Required-AllowCustom")]
        [TestCase(ItemTypePredefined.TextualRequirement, "Requirement - Required User", "Std-User-Required")]
        [TestRail(230671)]
        [Description("Create an artifact that has custom properties. Remove required value from custom property. Save and publish artifact. " +
                     "Verify 409 Conflict is returned due to validation errors.")]
        public void PublishArtifact_RemoveRequiredPropertyValueAndPublish_409Conflict(ItemTypePredefined itemType,
            string artifactTypeName, string propertyName)
        {
            // Setup:
            var project = Helper.GetProject(TestHelper.GoldenDataProject.Default, _user);
            var artifact = Helper.CreateNovaArtifact(_user, project, itemType, artifactTypeName: artifactTypeName);

            // Update custom property in artifact.
            var property = ArtifactStoreHelper.SetCustomPropertyToNull(artifact.CustomPropertyValues, propertyName);
            var artifactDetailsChangeset = TestHelper.CreateArtifactChangeSet(artifact, customProperty: property);

            // Save artifact with empty required property (No validation on save)
            artifact.Lock(_user);
            artifact.Update(_user, artifactDetailsChangeset);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => Helper.ArtifactStore.PublishArtifact(artifact.Id, _user),
                "'POST {0}' should return 409 Conflict if the Artifact has required properties that have no values!", PUBLISH_PATH);

            // Verify:
            string expectedExceptionMessage = I18NHelper.FormatInvariant("Artifact with ID {0} has validation errors.", artifact.Id);
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotPublishOverValidationErrors, expectedExceptionMessage);
        }

        [Explicit (IgnoreReasons.UnderDevelopmentQaDev)]  // User Story 4657:[Validation] Validate Process Sub-Artifacts
        [Category(Categories.CustomData)]
        [TestCase("Std-Date-Required-HasDefault")]
        [TestCase("Std-Number-Required-HasDefault")]
        [TestCase("Std-Text-Required-HasDefault")]
        [TestCase("Std-Choice-Required-HasDefault")]
        [TestCase("Std-User-Required-HasDefault")]
        [TestRail(234306)]
        [Description("Create a Process artifact that has subartifact custom properties. Remove required value from subartifact custom property. " +
                     "Save and publish artifact. Verify 409 Conflict is returned due to validation errors.")]
        public void PublishArtifact_RemoveSubArtifactRequiredPropertyValueAndPublish_409Conflict(string propertyName)
        {
            // Setup:
            var project = Helper.GetProject(TestHelper.GoldenDataProject.EmptyProjectWithSubArtifactRequiredProperties, _user);
            var artifact = Helper.CreateNovaProcessArtifact(_user, project, artifactTypeName: "Process");

            // Get nova subartifact
            var processShape = artifact.NovaProcess.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            var novaSubArtifact = Helper.ArtifactStore.GetSubartifact(_user, artifact.Id, processShape.Id);

            // Update custom property in subartifact.
            var property = ArtifactStoreHelper.SetCustomPropertyToNull(novaSubArtifact.CustomPropertyValues, propertyName);

            // Add subartifact changeset to NovaProcess
            var subArtifactChangeSet = TestHelper.CreateSubArtifactChangeSet(novaSubArtifact, customProperty: property);
            artifact.NovaProcess.SubArtifacts = new List<NovaSubArtifact> { subArtifactChangeSet };

            // Save artifact with empty required property (No validation on save)
            artifact.Lock(_user);

            // Update(save) Nova process
            artifact.Update(_user, artifact);   // TODO: This test is getting a 409 here instead of on the Publish.  Is this correct?

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => Helper.ArtifactStore.PublishArtifact(artifact.Id, _user),
                "'POST {0}' should return 409 Conflict if the Artifact has required properties that have no values!", PUBLISH_PATH);

            // Verify:
            string expectedExceptionMessage = I18NHelper.FormatInvariant("Artifact with ID {0} has validation errors.", artifact.Id);
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotPublishOverValidationErrors, expectedExceptionMessage);
        }

        #endregion Custom data tests

        #endregion 409 Conflict tests

        #region Private functions

        /// <summary>
        /// Asserts that the version of all the artifacts in the list still have the expected version.
        /// </summary>
        /// <param name="artifacts">The list of artifacts whose version you want to check.</param>
        /// <param name="expectedVersion">The expected version of all the artifacts.</param>
        private void AssertArtifactsVersionEquals(List<ArtifactWrapper> artifacts, int expectedVersion)
        {
            foreach (var artifact in artifacts)
            {
                var artifactHistoryAfter = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user);
                Assert.AreEqual(expectedVersion, artifactHistoryAfter[0].VersionId,
                    "Version ID after publish should be {0}!", expectedVersion);
            }
        }

        /// <summary>
        /// Asserts that the specified list of artifacts were NOT published by getting the artifacts and comparing against
        /// the list that was passed in.
        /// </summary>
        /// <param name="artifacts">The list of artifacts to verify.</param>
        private void AssertArtifactsWereNotPublished(List<ArtifactWrapper> artifacts)
        {
            foreach (var artifact in artifacts)
            {
                var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);
                ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, artifact);
            }
        }

        /// <summary>
        /// Asserts that the response from the publish call contains all the specified artifacts and that they now have the correct version.
        /// </summary>
        /// <param name="publishResponse">The response from the publish call.</param>
        /// <param name="artifactsToPublish">The Nova artifacts that we sent to the publish call.</param>
        /// <param name="expectedVersion">The version expected in the publishedArtifact.</param>
        private void AssertPublishedArtifactResponseContainsAllArtifactsInListAndHasExpectedVersion(
            INovaArtifactsAndProjectsResponse publishResponse,
            List<ArtifactWrapper> artifactsToPublish,
            int expectedVersion)
        {
            ArtifactStoreHelper.AssertArtifactsAndProjectsResponseContainsAllArtifactsInListAndHasExpectedVersion(
                publishResponse, artifactsToPublish, expectedVersion);

            foreach (var artifact in artifactsToPublish)
            {
                var artifactHistoryAfter = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user);
                Assert.AreEqual(expectedVersion, artifactHistoryAfter[0].VersionId, "Version ID after publish should be {0}!", expectedVersion);
            }
        }

        /// <summary>
        /// Creates chain of tree artifacts. Grandparent, parent and child
        /// </summary>
        /// <param name="artifactType">The type of created artifacts</param>
        /// <returns>List of created artifacts.</returns>
        private List<ArtifactWrapper> CreateParentAndTwoChildrenArtifactsAndGetAllArtifacts(ItemTypePredefined artifactType)
        {
            var artifactTypes = new ItemTypePredefined[] { artifactType, artifactType, artifactType };
            var artifactChain = Helper.CreateSavedArtifactChain(_project, _user, artifactTypes);
            return artifactChain;
        }

        #endregion Private functions
    }
}
