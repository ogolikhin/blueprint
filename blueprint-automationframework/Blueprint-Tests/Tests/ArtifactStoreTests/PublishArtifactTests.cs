using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using NUnit.Framework;
using TestCommon;
using Utilities;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class PublishArtifactTests : TestBase
    {
        const string PUBLISH_PATH = RestPaths.Svc.ArtifactStore.Artifacts.PUBLISH;

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

        #region 200 OK Tests

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForOpenApiRestMethods))]
        [TestRail(165856)]
        [Description("Create & save a single artifact.  Publish the artifact.  Verify publish is successful and that artifact version is now 1.")]
        public void PublishArtifact_SingleSavedArtifact_ArtifactHasVersion1(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _user, artifactType);
            var artifactHistoryBefore = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user);
            Assert.AreEqual(int.MaxValue, artifactHistoryBefore[0].VersionId, "Version ID before publish should be {0}!", int.MaxValue);

            // Execute:
            INovaPublishResponse publishResponse = null;

            Assert.DoesNotThrow(() => publishResponse = Helper.ArtifactStore.PublishArtifact(artifact, _user),
                "'POST {0}' should return 200 OK if a valid artifact ID is sent!", PUBLISH_PATH);

            // Verify:
            Assert.AreEqual(1, publishResponse.Projects.Count, "There should only be 1 project returned for the published artifact!");
            Assert.AreEqual(1, publishResponse.Artifacts.Count, "There should only be 1 published artifact returned!");
            AssertPublishedArtifactPropertiesMatchWithArtifact(publishResponse.Artifacts.First(), artifact, expectedVersion: 1);

            var artifactHistoryAfter = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user);
            Assert.AreEqual(1, artifactHistoryAfter[0].VersionId, "Version ID after publish should be 1!");
        }

        [TestCase(BaseArtifactType.Actor, 2)]
        [TestCase(BaseArtifactType.Process, 3)]
        [TestRail(165968)]
        [Description("Create & publish a single artifact several times, then save to create a draft.  Publish the artifact." +
            "Verify publish is successful and that artifact has the expected version.")]
        public void PublishArtifact_SinglePublishedArtifactWithMultipleVersionsWithDraft_ArtifactHasExpectedVersion(BaseArtifactType artifactType, int numberOfVersions)
        {
            // Setup:
            IArtifact artifactWithMultipleVersions = Helper.CreateAndPublishArtifact(_project, _user, artifactType, numberOfVersions: numberOfVersions);

            var artifactHistoryBefore = Helper.ArtifactStore.GetArtifactHistory(artifactWithMultipleVersions.Id, _user);
            Assert.AreEqual(numberOfVersions, artifactHistoryBefore[0].VersionId, "Version ID before Nova publish should be {0}!", numberOfVersions);

            artifactWithMultipleVersions.Save();    // Now save to make a draft.

            // Execute:
            INovaPublishResponse publishResponse = null;

            Assert.DoesNotThrow(() => publishResponse = Helper.ArtifactStore.PublishArtifact(artifactWithMultipleVersions, _user),
                "'POST {0}' should return 200 OK if a valid artifact ID is sent!", PUBLISH_PATH);

            // Verify:
            int expectedVersion;

            checked  // This checked section is needed to fix warning:  CA2233: Operations should not overflow
            {
                expectedVersion = numberOfVersions + 1;
            }

            Assert.AreEqual(1, publishResponse.Projects.Count, "There should only be 1 project returned for the published artifact!");
            Assert.AreEqual(1, publishResponse.Artifacts.Count, "There should only be 1 published artifact returned!");
            AssertPublishedArtifactPropertiesMatchWithArtifact(publishResponse.Artifacts.First(), artifactWithMultipleVersions, expectedVersion);

            var artifactHistoryAfter = Helper.ArtifactStore.GetArtifactHistory(artifactWithMultipleVersions.Id, _user);

            Assert.AreEqual(expectedVersion, artifactHistoryAfter[0].VersionId, "Version ID after publish should be {0}!", expectedVersion);
        }

        [TestCase(BaseArtifactType.Process, 3)]
        [TestRail(165956)]
        [Description("Create & save multiple artifacts.  Publish all the artifacts.  Verify publish is successful and that the version of the artifacts is now 1.")]
        public void PublishArtifact_MultipleSavedArtifacts_ArtifactsHaveVersion1(BaseArtifactType artifactType, int numberOfArtifacts)
        {
            // Setup:
            var artifacts = Helper.CreateAndSaveMultipleArtifacts(_project, _user, artifactType, numberOfArtifacts);
            var artifactHistoryBefore = Helper.ArtifactStore.GetArtifactHistory(artifacts[0].Id, _user);
            Assert.AreEqual(int.MaxValue, artifactHistoryBefore[0].VersionId, "Version ID before publish should be {0}!", int.MaxValue);

            // Execute:
            INovaPublishResponse publishResponse = null;

            Assert.DoesNotThrow(() => publishResponse = Helper.ArtifactStore.PublishArtifacts(artifacts, _user),
                "'POST {0}' should return 200 OK if a valid list of artifact IDs is sent!", PUBLISH_PATH);

            // Verify:
            Assert.AreEqual(1, publishResponse.Projects.Count, "There should only be 1 project returned for the published artifact!");
            Assert.AreEqual(_project.Id, publishResponse.Projects.First().Id,
                "The project ID returned by the publish call doesn't match the project of the artifacts we published!");
            Assert.AreEqual(numberOfArtifacts, publishResponse.Artifacts.Count, "There should only be {0} published artifact returned!", numberOfArtifacts);

            AssertPublishedArtifactResponseContainsAllArtifactsInListAndHasExpectedVersion(
                publishResponse, artifacts, expectedVersion: 1);
        }

        [TestCase(2, BaseArtifactType.Actor, BaseArtifactType.Document, BaseArtifactType.Glossary)]
        [TestCase(3, BaseArtifactType.Process, BaseArtifactType.TextualRequirement, BaseArtifactType.UseCase)]
        [TestRail(165969)]
        [Description("Create & publish a multiple artifacts several times and save to create drafts.  Publish the artifacts." +
            "Verify publish is successful and that the artifacts have the expected versions.")]
        public void PublishArtifact_MultiplePublishedArtifactsWithMultipleVersionsWithDraft_ArtifactHasExpectedVersion(int numberOfVersions, params BaseArtifactType[] artifactTypes)
        {
            ThrowIf.ArgumentNull(artifactTypes, nameof(artifactTypes));

            // Setup:
            var artifactsWithMultipleVersions = new List<IArtifactBase>();

            foreach (var artifactType in artifactTypes)
            {
                IArtifact artifactWithMultipleVersions = Helper.CreateAndPublishArtifact(_project, _user, artifactType, numberOfVersions: numberOfVersions);
                artifactsWithMultipleVersions.Add(artifactWithMultipleVersions);

                var artifactHistoryBefore = Helper.ArtifactStore.GetArtifactHistory(artifactWithMultipleVersions.Id, _user);
                Assert.AreEqual(numberOfVersions, artifactHistoryBefore[0].VersionId, "Version ID before Nova publish should be {0}!", numberOfVersions);

                artifactWithMultipleVersions.Save();    // Now save to make a draft.
            }

            // Execute:
            INovaPublishResponse publishResponse = null;

            Assert.DoesNotThrow(() => publishResponse = Helper.ArtifactStore.PublishArtifacts(artifactsWithMultipleVersions, _user),
                "'POST {0}' should return 200 OK if a valid list of artifact IDs is sent!", PUBLISH_PATH);

            // Verify:
            Assert.AreEqual(1, publishResponse.Projects.Count, "There should only be 1 project returned for the published artifact!");
            Assert.AreEqual(_project.Id, publishResponse.Projects.First().Id,
                "The project ID returned by the publish call doesn't match the project of the artifacts we published!");
            Assert.AreEqual(artifactsWithMultipleVersions.Count, publishResponse.Artifacts.Count,
                "There should only be {0} published artifact returned!", artifactsWithMultipleVersions.Count);

            int expectedVersion;

            checked  // This checked section is needed to fix warning:  CA2233: Operations should not overflow
            {
                expectedVersion = numberOfVersions + 1;
            }

            AssertPublishedArtifactResponseContainsAllArtifactsInListAndHasExpectedVersion(
                publishResponse, artifactsWithMultipleVersions, expectedVersion);
        }

        [TestCase(BaseArtifactType.Actor, 2, BaseArtifactType.Process, 3, BaseArtifactType.UseCase, 2)]
        [TestRail(165970)]
        [Description("Create multiple artifacts (some saved, some published & some published with drafts).  Publish all the artifacts with unpublished changes." +
            "Verify publish is successful and that the version of the artifacts have the expected versions.")]
        public void PublishArtifact_MultipleSavedAndPublishedArtifactsSomeWithDrafts_PublishArtifactsWithUnpublishedChanges_ArtifactsHaveExpectedVersions(
            BaseArtifactType savedArtifactType, int numberOfSavedArtifacts,
            BaseArtifactType publishedWithDraftArtifactType, int numberOfPublishedWithDraftArtifacts,
            BaseArtifactType publishedArtifactType, int numberOfPublishedArtifacts)
        {
            // Setup:
            var savedArtifacts = Helper.CreateAndSaveMultipleArtifacts(_project, _user, savedArtifactType, numberOfSavedArtifacts);
            var publishedArtifacts = Helper.CreateAndSaveMultipleArtifacts(_project, _user, publishedArtifactType, numberOfPublishedArtifacts);
            var publishedWithDraftArtifacts = Helper.CreateAndPublishMultipleArtifacts(_project, _user, publishedWithDraftArtifactType, numberOfPublishedWithDraftArtifacts);

            Artifact.LockArtifacts(publishedWithDraftArtifacts, publishedWithDraftArtifacts.First().Address, _user);

            foreach (var artifact in publishedWithDraftArtifacts)
            {
                Artifact.UpdateArtifact(artifact, _user);
            }

            var artifactsToPublish = new List<IArtifactBase>();
            artifactsToPublish.AddRange(savedArtifacts);
            artifactsToPublish.AddRange(publishedWithDraftArtifacts);

            // Execute:
            INovaPublishResponse publishResponse = null;

            Assert.DoesNotThrow(() => publishResponse = Helper.ArtifactStore.PublishArtifacts(artifactsToPublish, _user),
                "'POST {0}' should return 200 OK if a valid list of artifact IDs is sent!", PUBLISH_PATH);

            // Verify:
            Assert.AreEqual(1, publishResponse.Projects.Count, "There should only be 1 project returned for the published artifact!");
            Assert.AreEqual(_project.Id, publishResponse.Projects.First().Id,
                "The project ID returned by the publish call doesn't match the project of the artifacts we published!");
            Assert.AreEqual(artifactsToPublish.Count, publishResponse.Artifacts.Count,
                "There should only be {0} published artifact returned!", artifactsToPublish.Count);

            AssertPublishedArtifactResponseContainsAllArtifactsInListAndHasExpectedVersion(publishResponse, savedArtifacts, expectedVersion: 1);
            AssertPublishedArtifactResponseContainsAllArtifactsInListAndHasExpectedVersion(publishResponse, publishedWithDraftArtifacts, expectedVersion: 2);

            // Verify the published artifacts without drafts didn't get published again.
            AssertArtifactsWereNotPublished(publishedArtifacts);
        }

        [TestCase(BaseArtifactType.Process, 3, 2, null)]
        [TestCase(BaseArtifactType.UseCase, 2, 3, false)]
        [TestRail(165977)]
        [Description("Create & save multiple artifacts.  Publish some of the artifacts (pass all=false or don't pass the all parameter)." +
            "Verify publish is successful and that only the artifacts we wanted to publish got published.")]
        public void PublishArtifactWithAllNullOrFalse_MultipleSavedArtifacts_OnlyPublishSome_ArtifactsHaveExpectedVersion(
            BaseArtifactType artifactType, int numberOfArtifactsToPublish, int numberOfArtifactsToNotPublish, bool? all)
        {
            // Setup:
            var artifactsToPublish = Helper.CreateAndSaveMultipleArtifacts(_project, _user, artifactType, numberOfArtifactsToPublish);
            var artifactsNotToPublish = Helper.CreateAndSaveMultipleArtifacts(_project, _user, artifactType, numberOfArtifactsToNotPublish);

            // Execute:
            INovaPublishResponse publishResponse = null;

            Assert.DoesNotThrow(() => publishResponse = Helper.ArtifactStore.PublishArtifacts(artifactsToPublish, _user, all),
                "'POST {0}{1}' should return 200 OK if a valid list of artifact IDs is sent!",
                PUBLISH_PATH, (all == null) ? string.Empty : I18NHelper.FormatInvariant("?all={0}", all.Value.ToString()));

            // Verify:
            Assert.AreEqual(1, publishResponse.Projects.Count, "There should only be 1 project returned for the published artifact!");
            Assert.AreEqual(_project.Id, publishResponse.Projects.First().Id,
                "The project ID returned by the publish call doesn't match the project of the artifacts we published!");
            Assert.AreEqual(numberOfArtifactsToPublish, publishResponse.Artifacts.Count,
                "There should only be {0} published artifact returned!", numberOfArtifactsToPublish);

            AssertPublishedArtifactResponseContainsAllArtifactsInListAndHasExpectedVersion(
                publishResponse, artifactsToPublish, expectedVersion: 1);

            AssertArtifactsWereNotPublished(artifactsNotToPublish);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForOpenApiRestMethods))]
        [TestRail(165978)]
        [Description("Create a single published artifact.  Delete the artifact, then publish it.  Verify the artifact is deleted by trying to get it with another user.")]
        public void PublishArtifact_SingleDeletedArtifact_ArtifactIsDeleted(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            artifact.Delete();

            // Execute:
            INovaPublishResponse publishResponse = null;

            Assert.DoesNotThrow(() => publishResponse = Helper.ArtifactStore.PublishArtifact(artifact, _user),
                "'POST {0}' should return 200 OK if a valid artifact ID is sent!", PUBLISH_PATH);

            // Verify:
            Assert.AreEqual(1, publishResponse.Projects.Count, "There should only be 1 project returned for the published artifact!");
            Assert.AreEqual(1, publishResponse.Artifacts.Count, "There should only be 1 published artifact returned!");

            IUser anotherUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetArtifactDetails(anotherUser, artifact.Id),
                "After publishing a deleted artifact, other users should not be able to get the deleted artifact!");
        }

        [TestCase(BaseArtifactType.UseCase, 2, 3)]
        [TestRail(165979)]
        [Description("Create & save multiple artifacts.  Publish with all=true but only pass some of the artifacts." +
            "Verify publish is successful and that all the artifacts we created were published.")]
        public void PublishArtifactWithAllTrue_MultipleSavedArtifacts_OnlyPublishSome_ArtifactsHaveExpectedVersion(
            BaseArtifactType artifactType, int numberOfArtifactsToPublish, int numberOfArtifactsToNotPublish)
        {
            // Setup:
            var artifactsPassedToPublish = Helper.CreateAndSaveMultipleArtifacts(_project, _user, artifactType, numberOfArtifactsToPublish);
            var artifactsNotPassedToPublish = Helper.CreateAndSaveMultipleArtifacts(_project, _user, artifactType, numberOfArtifactsToNotPublish);

            // Execute:
            INovaPublishResponse publishResponse = null;

            Assert.DoesNotThrow(() => publishResponse = Helper.ArtifactStore.PublishArtifacts(artifactsPassedToPublish, _user, all: true),
                "'POST {0}?all=true' should return 200 OK if a valid list of artifact IDs is sent!", PUBLISH_PATH);

            // Verify:
            var allArtifacts = new List<IArtifactBase>();
            allArtifacts.AddRange(artifactsPassedToPublish);
            allArtifacts.AddRange(artifactsNotPassedToPublish);

            Assert.AreEqual(1, publishResponse.Projects.Count, "There should only be 1 project returned for the published artifact!");
            Assert.AreEqual(_project.Id, publishResponse.Projects.First().Id,
                "The project ID returned by the publish call doesn't match the project of the artifacts we published!");
            Assert.AreEqual(allArtifacts.Count, publishResponse.Artifacts.Count,
                "There should only be {0} published artifact returned!", allArtifacts.Count);

            AssertPublishedArtifactResponseContainsAllArtifactsInListAndHasExpectedVersion(
                publishResponse, allArtifacts, expectedVersion: 1);
        }

        // TODO: Test saving some artifacts and passing none of them to publish with all=true.
        [TestCase(BaseArtifactType.Process, 3)]
        [TestRail(000)]
        [Description("Create & save multiple artifacts.  Publish with all=true but don't pass any of the artifacts." +
            "Verify publish is successful and that all the artifacts we created were published.")]
        public void PublishArtifactWithAllTrue_MultipleSavedArtifacts_SendEmptyListToPublish_ArtifactsHaveExpectedVersion(
            BaseArtifactType artifactType, int numberOfArtifacts)
        {
            // Setup:
            var allArtifacts = Helper.CreateAndSaveMultipleArtifacts(_project, _user, artifactType, numberOfArtifacts);

            // Execute:
            INovaPublishResponse publishResponse = null;

            try
            {
                Assert.DoesNotThrow(
                    () => publishResponse = Helper.ArtifactStore.PublishArtifacts(new List<IArtifactBase>(), _user, all: true),
                    "'POST {0}?all=true' should return 200 OK if an empty list of artifact IDs is sent!", PUBLISH_PATH);

                // Verify:
                Assert.AreEqual(1, publishResponse.Projects.Count, "There should only be 1 project returned for the published artifact!");
                Assert.AreEqual(_project.Id, publishResponse.Projects.First().Id,
                    "The project ID returned by the publish call doesn't match the project of the artifacts we published!");
                Assert.AreEqual(allArtifacts.Count, publishResponse.Artifacts.Count,
                    "There should only be {0} published artifact returned!", allArtifacts.Count);

                AssertPublishedArtifactResponseContainsAllArtifactsInListAndHasExpectedVersion(
                    publishResponse, allArtifacts, expectedVersion: 1);
            }
            finally
            {
                // This is needed so the Dispose() in the TearDown doesn't fail.
                if (publishResponse != null)
                {
                    allArtifacts.First().NotifyArtifactPublish(publishResponse.Artifacts);
                }
            }
        }

        // TODO: Test saving some artifacts (some published with no drafts) and passing none of them to publish with all=true.
        // TODO: Test with artifacts saved in different projects.
        // TODO: Test with artifacts saved in different projects with all=true.

        #endregion 200 OK Tests

        #region 400 Bad Request tests
        //public void PublishArtifact_xxxx_400BadRequest()
        #endregion 400 Bad Request tests

        #region 401 Unauthorized tests
        //public void PublishArtifact_xxxx_401Unauthorized()
        #endregion 401 Unauthorized tests

        #region 403 Forbidden tests
        //public void PublishArtifact_xxxx_403Forbidden()
        #endregion 403 Forbidden tests

        #region 404 Not Found tests
        //public void PublishArtifact_xxxx_404NotFound()
        #endregion 404 Not Found tests

        #region 409 Conflict tests
        //public void PublishArtifact_xxxx_409Conflict()
        #endregion 409 Conflict tests

        #region Private functions

        /// <summary>
        /// Asserts that the specified list of artifacts were NOT published by getting the artifacts and comparing against
        /// the list that was passed in.
        /// </summary>
        /// <param name="artifacts">The list of artifacts to verify.</param>
        private void AssertArtifactsWereNotPublished(List<IArtifactBase> artifacts)
        {
            foreach (var artifact in artifacts)
            {
                var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);
                artifactDetails.AssertEquals(artifact);
            }
        }

        /// <summary>
        /// Asserts that the properties of the published artifact match with the artifact we tried to publish.  Some properties are expected to be null.
        /// </summary>
        /// <param name="publishedArtifact">The artifact returned by the Nova publish call.</param>
        /// <param name="artifact">The OpenApi artifact we tried to publish.</param>
        /// <param name="expectedVersion">The version expected in the publishedArtifact.</param>
        private static void AssertPublishedArtifactPropertiesMatchWithArtifact(
            INovaArtifactResponse publishedArtifact,
            IArtifactBase artifact,
            int expectedVersion)
        {
            AssertPublishedArtifactPropertiesMatchWithArtifactSkipVersion(publishedArtifact, artifact);
            Assert.AreEqual(expectedVersion, publishedArtifact.Version, "The Version properties of the artifacts don't match!");
        }

        /// <summary>
        /// Asserts that the properties of the published artifact match with the artifact we tried to publish (but don't check the versions).
        /// Some properties are expected to be null.
        /// </summary>
        /// <param name="publishedArtifact">The artifact returned by the Nova publish call.</param>
        /// <param name="artifact">The OpenApi artifact we tried to publish.</param>
        private static void AssertPublishedArtifactPropertiesMatchWithArtifactSkipVersion(
            INovaArtifactResponse publishedArtifact,
            IArtifactBase artifact)
        {
            Assert.AreEqual(artifact.Id, publishedArtifact.Id, "The Id properties of the artifacts don't match!");
            Assert.AreEqual(artifact.ArtifactTypeId, publishedArtifact.ItemTypeId, "The ItemTypeId properties of the artifacts don't match!");
            Assert.AreEqual(artifact.Name, publishedArtifact.Name, "The Name properties of the artifacts don't match!");
            Assert.AreEqual(artifact.ParentId, publishedArtifact.ParentId, "The ParentId properties of the artifacts don't match!");
            Assert.AreEqual(artifact.ProjectId, publishedArtifact.ProjectId, "The ProjectId properties of the artifacts don't match!");

            // These properties should always be null:
            Assert.IsNull(publishedArtifact.CreatedBy, "The CreatedBy property of the published artifact should always be null!");
            Assert.IsNull(publishedArtifact.CreatedOn, "The CreatedOn property of the published artifact should always be null!");
            Assert.IsNull(publishedArtifact.Description, "The Description property of the published artifact should always be null!");
            Assert.IsNull(publishedArtifact.LastEditedBy, "The LastEditedBy property of the published artifact should always be null!");
            Assert.IsNull(publishedArtifact.LastEditedOn, "The LastEditedOn property of the published artifact should always be null!");

            // OpenAPI doesn't have these properties, so they can't be compared:  OrderIndex, PredefinedType, Prefix
        }

        /// <summary>
        /// Asserts that the response from the publish call contains all the specified artifacts and that they now have the correct version.
        /// </summary>
        /// <param name="publishResponse">The response from the publish call.</param>
        /// <param name="artifactsToPublish">The OpenApi artifacts that we sent to the publish call.</param>
        /// <param name="expectedVersion">The version expected in the publishedArtifact.</param>
        private void AssertPublishedArtifactResponseContainsAllArtifactsInListAndHasExpectedVersion(
            INovaPublishResponse publishResponse,
            List<IArtifactBase> artifactsToPublish,
            int expectedVersion)
        {
            foreach (var artifact in artifactsToPublish)
            {
                var publishedArtifact = publishResponse.Artifacts.Find(a => a.Id == artifact.Id);
                Assert.NotNull(publishedArtifact, "Couldn't find artifact ID {0} in the list of published artifacts!");

                // The artifact doesn't have a version before it's published at least once, so we can't compare version of unpublished artifacts.
                if (artifact.IsPublished)
                {
                    AssertPublishedArtifactPropertiesMatchWithArtifact(publishedArtifact, artifact, expectedVersion);
                }
                else
                {
                    AssertPublishedArtifactPropertiesMatchWithArtifactSkipVersion(publishedArtifact, artifact);
                }

                var artifactHistoryAfter = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user);
                Assert.AreEqual(expectedVersion, artifactHistoryAfter[0].VersionId, "Version ID after publish should be {0}!", expectedVersion);
            }
        }

        #endregion Private functions
    }
}
