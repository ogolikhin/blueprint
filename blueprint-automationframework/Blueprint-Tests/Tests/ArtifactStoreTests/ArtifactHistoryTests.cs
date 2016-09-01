using System;
using Helper;
using Model;
using NUnit.Framework;
using CustomAttributes;
using System.Collections.Generic;
using TestCommon;
using Model.ArtifactModel;
using Model.Factories;
using Model.ArtifactModel.Impl;
using Utilities;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class ArtifactHistoryTests : TestBase
    {
        private IUser _user = null;
        private IUser _user2 = null;
        private IProject _project = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_user);
            _user2 = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #region private functions

        /// <summary>
        /// Creates an expected ArtifactHistoryVersion that tests can compare against.
        /// </summary>
        /// <param name="versionId">(optional) Default is 1.</param>
        /// <param name="userId">(optional) Default is _user.UserId.</param>
        /// <param name="displayName">(optional) Default is _user.DisplayName.</param>
        /// <param name="hasUserIcon">(optional) Default is false.</param>
        /// <param name="timestamp">(optional) Default is now.</param>
        /// <param name="artifactState">(optional) Default is Published.</param>
        /// <returns>An ArtifactHistoryVersion with the values specified.</returns>
        private ArtifactHistoryVersion CreateArtifactHistoryVersion(
            long? versionId = null,
            int? userId = null,
            string displayName = null,
            bool? hasUserIcon = null,
            DateTime? timestamp = null,
            ArtifactState? artifactState = null)
        {
            var expectedArtifactHistory = new ArtifactHistoryVersion
            {
                VersionId = versionId ?? 1,
                UserId = userId ?? _user.Id,
                DisplayName = displayName ?? _user.DisplayName,
                HasUserIcon = hasUserIcon ?? false,
                Timestamp = timestamp ?? DateTime.Now.ToUniversalTime(),
                ArtifactState = artifactState ?? ArtifactState.Published
            };

            return expectedArtifactHistory;
        }

        /// <summary>
        /// Asserts that the returned artifact history version is equal to the expected one.
        /// </summary>
        /// <param name="artifactHistoryVersion">The ArtifactHistoryVersion that was returned.</param>
        /// <param name="expectedArtifactHistoryVersion">The expected ArtifactHistoryVersion values.</param>
        /// <param name="plusOrMinusSeconds">(optional) Compare Timestamps leniently with +/- this many seconds.</param>
        private static void AssertArtifactHistory(
            ArtifactHistoryVersion artifactHistoryVersion,
            ArtifactHistoryVersion expectedArtifactHistoryVersion,
            double plusOrMinusSeconds = 60.0)
        {
            Assert.AreEqual(artifactHistoryVersion.VersionId, expectedArtifactHistoryVersion.VersionId,
                "VersionId should be {0}, but it is {1}", artifactHistoryVersion.VersionId, expectedArtifactHistoryVersion.VersionId);
            Assert.AreEqual(artifactHistoryVersion.HasUserIcon, expectedArtifactHistoryVersion.HasUserIcon,
                "HasUserIcon should be {0}, but it is {1}", artifactHistoryVersion.HasUserIcon, expectedArtifactHistoryVersion.HasUserIcon);
            Assert.AreEqual(artifactHistoryVersion.DisplayName, expectedArtifactHistoryVersion.DisplayName,
                "DisplayName should be {0}, but it is {1}", artifactHistoryVersion.DisplayName, expectedArtifactHistoryVersion.DisplayName);
            Assert.AreEqual(artifactHistoryVersion.ArtifactState, expectedArtifactHistoryVersion.ArtifactState,
                "ArtifactState should be {0}, but it is {1}", artifactHistoryVersion.ArtifactState, expectedArtifactHistoryVersion.ArtifactState);
            Assert.AreEqual(artifactHistoryVersion.UserId, expectedArtifactHistoryVersion.UserId,
                "UserId should be {0}, but it is {1}", artifactHistoryVersion.UserId, expectedArtifactHistoryVersion.UserId);

            // Compare the Timestamps +/- plusOrMinusSeconds because we don't know what the exact time of creation was.
            Assert.That(artifactHistoryVersion.Timestamp.CompareTimePlusOrMinus(expectedArtifactHistoryVersion.Timestamp, plusOrMinusSeconds),
                "Timestamp should be approximately {0}, but it is {1}", expectedArtifactHistoryVersion.Timestamp, artifactHistoryVersion.Timestamp);
        }

        #endregion private functions

        [TestCase]
        [TestRail(145867)]
        [Description("Create artifact, publish it, get history.  Verify 1 published artifact history is returned with the expected values.")]
        public void GetArtifactHistory_PublishedArtifact_VerifyHistoryHasExpectedValue()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);

            List<ArtifactHistoryVersion> artifactHistory = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                artifactHistory = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user);
            }, "GetArtifactHistory shouldn't throw any error.");

            // Verify:
            Assert.AreEqual(1, artifactHistory.Count, "Artifact history must have 1 item, but it has {0} items", artifactHistory.Count);

            var expectedArtifactHistoryVersion = CreateArtifactHistoryVersion();
            AssertArtifactHistory(artifactHistory[0], expectedArtifactHistoryVersion);
        }

        [TestCase]
        [TestRail(145868)]
        [Description("Create artifact, save it, get history.  Verify 1 draft artifact history is returned with the expected values.")]
        public void GetArtifactHistory_ArtifactInDraft_VerifyHistoryHasExpectedValue()
        {
            // Setup:
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Actor);
            artifact.Save(_user);

            List<ArtifactHistoryVersion> artifactHistory = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                artifactHistory = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user);
            }, "GetArtifactHistory shouldn't throw any error.");

            // Verify:
            Assert.AreEqual(1, artifactHistory.Count, "Artifact history must have 1 item, but it has {0} items", artifactHistory.Count);

            var expectedArtifactHistoryVersion = CreateArtifactHistoryVersion(versionId: Int32.MaxValue, artifactState: ArtifactState.Draft, timestamp: new DateTime());
            AssertArtifactHistory(artifactHistory[0], expectedArtifactHistoryVersion);
        }

        [TestCase]
        [TestRail(145869)]
        [Description("Create artifact, publish, delete, publish, get history.  Verify 2 artifact histories are returned with the expected values (latest one with ArtifactState = Deleted).")]
        public void GetArtifactHistory_DeletedArtifact_VerifyHistoryHasExpectedValue()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            artifact.Delete(_user);
            artifact.Publish(_user);

            List<ArtifactHistoryVersion> artifactHistory = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                artifactHistory = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user);
            }, "GetArtifactHistory shouldn't throw any error.");

            // Verify:
            Assert.AreEqual(2, artifactHistory.Count, "Artifact history must have 2 items, but it has {0} items", artifactHistory.Count);

            var expectedArtifactHistoryVersion = CreateArtifactHistoryVersion(versionId: Int32.MaxValue, artifactState: ArtifactState.Deleted);
            AssertArtifactHistory(artifactHistory[0], expectedArtifactHistoryVersion);
        }

        [TestCase]
        [TestRail(145870)]
        [Description("Create artifact, publish, save.  Verify other user doesn't see draft version.")]
        public void GetArtifactHistory_PublishedArtifactInDraft_VerifyOtherUserSeeOnlyPublishedVersions()
        {
            // Setup:
            IOpenApiArtifact artifact = Helper.CreateAndPublishOpenApiArtifact(_project, _user, BaseArtifactType.Actor);
            artifact.Save(_user);

            List<ArtifactHistoryVersion> artifactHistory = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                artifactHistory = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user2);
            }, "GetArtifactHistory shouldn't throw any error.");

            // Verify:
            Assert.AreEqual(1, artifactHistory.Count, "Artifact history must have 1 item, but it has {0} items", artifactHistory.Count);

            var expectedArtifactHistoryVersion = CreateArtifactHistoryVersion();
            AssertArtifactHistory(artifactHistory[0], expectedArtifactHistoryVersion);
        }

        [TestCase]
        [TestRail(145871)]
        [Description("Create artifact, save.  Verify other user sees empty history.")]
        public void GetArtifactHistory_UnpublishedArtifactInDraft_VerifyOtherUserGetsEmptyHistory()
        {
            // Setup:
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Actor);
            artifact.Save(_user);

            List<ArtifactHistoryVersion> artifactHistory = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                artifactHistory = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user2);
            }, "GetArtifactHistory shouldn't throw any error.");

            // Verify:
            Assert.AreEqual(0, artifactHistory.Count, "Artifact history must be empty, but it has {0} items", artifactHistory.Count);
        }

        [TestCase]
        [TestRail(145896)]
        [Description("Create artifact with 12 versions.  Verify 'GET /artifacts/{artifactId}/version' returns only the last 10 versions.")]
        public void GetArtifactHistory_ArtifactWith12PublishedVersions_VerifyOnly10LatestVersionsReturned()
        {
            // Setup:
            IOpenApiArtifact artifact = Helper.CreateAndPublishOpenApiArtifact(_project, _user, BaseArtifactType.Actor, numberOfVersions: 12);

            List<ArtifactHistoryVersion> artifactHistory = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                artifactHistory = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user);
            }, "GetArtifactHistory shouldn't throw any error.");

            // Verify:
            Assert.AreEqual(10, artifactHistory.Count, "Artifact history must have 10 items, but it has {0} items", artifactHistory.Count);//By default versions returns 10 versions
            Assert.AreEqual(12, artifactHistory[0].VersionId, "The first version in the list should be 12!");
            Assert.AreEqual(3, artifactHistory[artifactHistory.Count - 1].VersionId, "The last version in the list should be 3!");
        }

        /// <summary>
        /// Helper for tests that get versions with the limit parameter.
        /// </summary>
        /// <param name="numberOfVersions">The number of versions to publish.</param>
        /// <param name="limit">The limit to pass to the GetArtifactHistory call.</param>
        private void GetArtifactHistoryWithLimitHelper(int numberOfVersions, int limit)
        {
            // Setup:
            IOpenApiArtifact artifact = Helper.CreateAndPublishOpenApiArtifact(_project, _user, BaseArtifactType.Actor, numberOfVersions: numberOfVersions);

            List<ArtifactHistoryVersion> artifactHistory = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                artifactHistory = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user, limit: limit);
            }, "GetArtifactHistory shouldn't throw any error.");

            // Verify:
            Assert.AreEqual(limit, artifactHistory.Count, "Artifact history must have {0} items, but it has {1} items!", limit, artifactHistory.Count);
            Assert.AreEqual(numberOfVersions, artifactHistory[0].VersionId, "The first version in the list should be {0}!", numberOfVersions);

            int expectedLastVersion = numberOfVersions - limit + 1;
            Assert.AreEqual(expectedLastVersion, artifactHistory[artifactHistory.Count - 1].VersionId,
                "The last version in the list should be {0}!", expectedLastVersion);
        }

        [TestCase]
        [TestRail(145897)]
        [Description("Create artifact with 11 versions.  Verify 'GET /artifacts/{artifactId}/version?limit=5' returns only the last 5 items.")]
        public void GetArtifactHistoryWithLimit5_ArtifactWith11PublishedVersions_VerifyOnly5LatestVersionsReturned()
        {
            GetArtifactHistoryWithLimitHelper(numberOfVersions: 11, limit: 5);
        }

        [TestCase]
        [TestRail(145899)]
        [Description("Create artifact with 13 versions.  Verify 'GET /artifacts/{artifactId}/version?limit=12' returns only the last 12 items.")]
        public void GetArtifactHistoryWithLimit12_ArtifactWith13PublishedVersions_VerifyOnly12LatestVersionsReturned()
        {
            GetArtifactHistoryWithLimitHelper(numberOfVersions: 13, limit: 12);
        }

        [TestCase]
        [TestRail(145901)]
        [Description("Create artifact with 5 versions.  Verify 'GET /artifacts/{artifactId}/version' returns latest version first (descending order).")]
        public void GetArtifactHistory_ArtifactWith5PublishedVersions_VerifyDefaultAscIsFalse()
        {
            // Setup:
            IOpenApiArtifact artifact = Helper.CreateAndPublishOpenApiArtifact(_project, _user, BaseArtifactType.Actor, numberOfVersions: 5);

            List<ArtifactHistoryVersion> artifactHistory = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                artifactHistory = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user);
            }, "GetArtifactHistory shouldn't throw any error.");

            // Verify:
            Assert.AreEqual(5, artifactHistory.Count, "Artifact history must have 5 items, but it has {0} items", artifactHistory.Count);

            var expectedFirstVersion = CreateArtifactHistoryVersion(versionId: 5);
            var expectedLastVersion = CreateArtifactHistoryVersion();
            AssertArtifactHistory(artifactHistory[0], expectedFirstVersion);
            AssertArtifactHistory(artifactHistory[4], expectedLastVersion);
        }

        [TestCase]
        [TestRail(145902)]
        [Description("Create artifact with 5 versions.  Verify 'GET /artifacts/{artifactId}/version' with asc=true returns latest version last (ascending order).")]
        public void GetArtifactHistoryWithAscTrue_ArtifactWith5PublishedVersions_VerifyVersionsReturnedInAscendingOrder()
        {
            // Setup:
            IOpenApiArtifact artifact = Helper.CreateAndPublishOpenApiArtifact(_project, _user, BaseArtifactType.Actor, numberOfVersions: 5);

            List<ArtifactHistoryVersion> artifactHistory = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                artifactHistory = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user, sortByDateAsc: true);
            }, "GetArtifactHistory shouldn't throw any error.");

            // Verify:
            Assert.AreEqual(5, artifactHistory.Count, "Artifact history must have 5 items, but it has {0} items", artifactHistory.Count);

            var expectedFirstVersion = CreateArtifactHistoryVersion();
            var expectedLastVersion = CreateArtifactHistoryVersion(versionId: 5);
            AssertArtifactHistory(artifactHistory[0], expectedFirstVersion);
            AssertArtifactHistory(artifactHistory[4], expectedLastVersion);
        }

        [TestCase]
        [TestRail(145903)]
        [Description("Create artifact with 5 versions.  Verify 'GET /artifacts/{artifactId}/version' with asc=false returns latest version first (descending order).")]
        public void GetArtifactHistoryWithAscFalse_ArtifactWith5PublishedVersions_VerifyVersionsReturnedInDescendingOrder()
        {
            // Setup:
            IOpenApiArtifact artifact = Helper.CreateAndPublishOpenApiArtifact(_project, _user, BaseArtifactType.Actor, numberOfVersions: 5);

            List<ArtifactHistoryVersion> artifactHistory = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                artifactHistory = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user, sortByDateAsc: false);
            }, "GetArtifactHistory shouldn't throw any error.");

            // Verify:
            Assert.AreEqual(5, artifactHistory.Count, "Artifact history must have 5 items, but it has {0} items", artifactHistory.Count);

            var expectedFirstVersion = CreateArtifactHistoryVersion(versionId: 5);
            var expectedLastVersion = CreateArtifactHistoryVersion();
            AssertArtifactHistory(artifactHistory[0], expectedFirstVersion);
            AssertArtifactHistory(artifactHistory[4], expectedLastVersion);
        }

        [TestCase]
        [TestRail(145904)]
        [Description("Create artifact with 5 versions.  Verify 'GET /artifacts/{artifactId}/version?offset=2' returns only the 3 latest versions.")]
        public void GetArtifactHistoryWithOffset2_ArtifactWith5PublishedVersions_VerifyOnly3LatestVersionsReturned()
        {
            // Setup:
            IOpenApiArtifact artifact = Helper.CreateAndPublishOpenApiArtifact(_project, _user, BaseArtifactType.Actor, numberOfVersions: 5);

            List<ArtifactHistoryVersion> artifactHistory = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
              artifactHistory = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user, offset: 2);
            }, "GetArtifactHistory shouldn't throw any error.");

            // Verify:
            Assert.AreEqual(3, artifactHistory.Count, "Artifact history must have 3 items, but it has {0} items", artifactHistory.Count);

            var expectedFirstVersion = CreateArtifactHistoryVersion(versionId: 3);
            var expectedLastVersion = CreateArtifactHistoryVersion();
            AssertArtifactHistory(artifactHistory[0], expectedFirstVersion);
            AssertArtifactHistory(artifactHistory[2], expectedLastVersion);
        }

        [TestCase]
        [TestRail(145909)]
        [Description("Create artifact with 5 versions.  Verify 'GET /artifacts/{artifactId}/version?asc=true&offset=3&limit=1' returns only version 4.")]
        public void GetArtifactHistoryWithAscTrueAndOffset3AndLimit1_ArtifactWith5PublishedVersions_VerifyOnlyVersion4Returned()
        {
            // Setup:
            IOpenApiArtifact artifact = Helper.CreateAndPublishOpenApiArtifact(_project, _user, BaseArtifactType.Actor, numberOfVersions: 5);

            List<ArtifactHistoryVersion> artifactHistory = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                //sortByDateAsc: true, offset: 3, limit: 1 for history with 5 versions must return version 4
                artifactHistory = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user, sortByDateAsc: true, offset: 3, limit: 1);
            }, "GetArtifactHistory shouldn't throw any error.");

            // Verify:
            Assert.AreEqual(1, artifactHistory.Count, "Artifact history must have 1 item, but it has {0} items", artifactHistory.Count);

            var expectedFirstVersion = CreateArtifactHistoryVersion(versionId: 4);
            AssertArtifactHistory(artifactHistory[0], expectedFirstVersion);
        }
    }
}
