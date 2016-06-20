using System;
using Helper;
using Model;
using NUnit.Framework;
using CustomAttributes;
using System.Net;
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
        IProject _project = null;

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

        [TestCase]
        [TestRail(145867)]
        [Description("...")]
        public void GetHistoryForPublishedArtifact_VerifyHistoryHasExpectedValue()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Actor);
            artifact.Save(_user);
            artifact.Publish(_user);

            List<ArtifactHistoryVersion> artifactHistory = null;
            Assert.DoesNotThrow(() =>
            {
                artifactHistory = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user);
            });
            Assert.AreEqual(1, artifactHistory.Count);
            Assert.AreEqual(1, artifactHistory[0].versionId);
            Assert.AreEqual(false, artifactHistory[0].hasUserIcon);
            Assert.AreEqual(_user.DisplayName, artifactHistory[0].displayName);
            Assert.AreEqual(ArtifactState.Publised, artifactHistory[0].artifactState);
        }

        [TestCase]
        [TestRail(145868)]
        [Description("...")]
        public void GetHistoryForArtifactInDraft_VerifyHistoryHasExpectedValue()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Actor);
            artifact.Save(_user);

            List<ArtifactHistoryVersion> artifactHistory = null;
            Assert.DoesNotThrow(() =>
            {
                artifactHistory = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user);
            });
            Assert.AreEqual(1, artifactHistory.Count);
            Assert.AreEqual(Int32.MaxValue, artifactHistory[0].versionId);
            Assert.AreEqual(false, artifactHistory[0].hasUserIcon);
            Assert.AreEqual(_user.DisplayName, artifactHistory[0].displayName);
            Assert.AreEqual(ArtifactState.Draft, artifactHistory[0].artifactState);
        }

        [TestCase]
        [TestRail(145869)]
        [Description("...")]
        public void GetHistoryForDeletedArtifact_VerifyHistoryHasExpectedValue()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Actor);
            artifact.Save(_user);
            artifact.Publish(_user);
            artifact.Delete(_user);
            artifact.Publish(_user);

            List<ArtifactHistoryVersion> artifactHistory = null;
            Assert.DoesNotThrow(() =>
            {
                artifactHistory = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user);
            });
            Assert.AreEqual(2, artifactHistory.Count);
            Assert.AreEqual(Int32.MaxValue, artifactHistory[0].versionId);
            Assert.AreEqual(false, artifactHistory[0].hasUserIcon);
            Assert.AreEqual(_user.DisplayName, artifactHistory[0].displayName);
            Assert.AreEqual(ArtifactState.Deleted, artifactHistory[0].artifactState);
        }

        [TestCase]
        [TestRail(145870)]
        [Description("...")]
        public void GetHistoryForArtifactInDraft_VerifyOtherUserSeeOnlyPublishedVersions()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Actor);
            artifact.Save(_user);
            artifact.Publish(_user);
            artifact.Save(_user);

            List<ArtifactHistoryVersion> artifactHistory = null;
            Assert.DoesNotThrow(() =>
            {
                artifactHistory = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user2);
            });
            Assert.AreEqual(1, artifactHistory.Count);
            artifactHistory = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user);
        }

        [TestCase]
        [TestRail(145871)]
        [Description("...")]
        public void GetHistoryForArtifactInDraft_VerifyOtherAdminUserSeeEmptyHistory()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Actor);
            artifact.Save(_user);

            List<ArtifactHistoryVersion> artifactHistory = null;
            Assert.DoesNotThrow(() =>
            {
                artifactHistory = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user2);
            });
            Assert.AreEqual(0, artifactHistory.Count);
        }
    }
}
