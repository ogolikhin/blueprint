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
        [Description("Create artifact, publish it, get history, check fields of returned version")]
        public void GetHistoryForPublishedArtifact_VerifyHistoryHasExpectedValue()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Actor);
            artifact.Save(_user);
            artifact.Publish(_user);

            List<ArtifactHistoryVersion> artifactHistory = null;
            Assert.DoesNotThrow(() =>
            {
                artifactHistory = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user);
            }, "GetArtifactHistory shouldn't throw any error.");
            Assert.AreEqual(1, artifactHistory.Count, "Artifact history must have 1 item, but it has {0} items", artifactHistory.Count);
            Assert.AreEqual(1, artifactHistory[0].VersionId, "VersionId must be 1, but it is {0}", artifactHistory[0].VersionId);
            Assert.AreEqual(false, artifactHistory[0].HasUserIcon, "HasUserIcon must be false, but it is {0}", artifactHistory[0].HasUserIcon);
            Assert.AreEqual(_user.DisplayName, artifactHistory[0].DisplayName, "DisplayName must be {0}, but it is {1}", _user.DisplayName, artifactHistory[0].DisplayName);
            Assert.AreEqual(ArtifactState.Published, artifactHistory[0].ArtifactState, "ArtifactState must be Published, but it is {0}", artifactHistory[0].ArtifactState);
            Assert.AreEqual(_user.UserId, artifactHistory[0].UserId, "UserId must be {0}, but it is {1}", _user.UserId, artifactHistory[0].UserId);
        }

        [TestCase]
        [TestRail(145868)]
        [Description("Create artifact, save it, get history - check that history contains draft version")]
        public void GetHistoryForArtifactInDraft_VerifyHistoryHasExpectedValue()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Actor);
            artifact.Save(_user);

            List<ArtifactHistoryVersion> artifactHistory = null;
            Assert.DoesNotThrow(() =>
            {
                artifactHistory = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user);
            }, "GetArtifactHistory shouldn't throw any error.");
            Assert.AreEqual(1, artifactHistory.Count, "Artifact history must have 1 item, but it has {0} items", artifactHistory.Count);
            Assert.AreEqual(Int32.MaxValue, artifactHistory[0].VersionId, "VersionId must be {0}, but it is {1}", Int32.MaxValue, artifactHistory[0].VersionId);
            Assert.AreEqual(false, artifactHistory[0].HasUserIcon, "HasUserIcon must be false, but it is {0}", artifactHistory[0].HasUserIcon);
            Assert.AreEqual(_user.DisplayName, artifactHistory[0].DisplayName, "DisplayName must be {0}, but it is {1}", _user.DisplayName, artifactHistory[0].DisplayName);
            Assert.AreEqual(ArtifactState.Draft, artifactHistory[0].ArtifactState, "ArtifactState must be Draft, but it is {0}", artifactHistory[0].ArtifactState);
        }

        [TestCase]
        [TestRail(145869)]
        [Description("Create artifact, publish, delete, publish, get history - check that history contains deleted version")]
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
            }, "GetArtifactHistory shouldn't throw any error.");
            Assert.AreEqual(2, artifactHistory.Count, "Artifact history must have 2 items, but it has {0} items", artifactHistory.Count);
            Assert.AreEqual(Int32.MaxValue, artifactHistory[0].VersionId, "VersionId must be {0}, but it is {1}", Int32.MaxValue, artifactHistory[0].VersionId);
            Assert.AreEqual(false, artifactHistory[0].HasUserIcon, "HasUserIcon must be false, but it is {0}", artifactHistory[0].HasUserIcon);
            Assert.AreEqual(_user.DisplayName, artifactHistory[0].DisplayName, "DisplayName must be {0}, but it is {1}", _user.DisplayName, artifactHistory[0].DisplayName);
            Assert.AreEqual(ArtifactState.Deleted, artifactHistory[0].ArtifactState, "ArtifactState must be Deleted, but it is {0}", artifactHistory[0].ArtifactState);
        }

        [TestCase]
        [TestRail(145870)]
        [Description("Create artifact, publish, save, check that other user doesn't see draft version")]
        public void GetHistoryForPublishedArtifactInDraft_VerifyOtherUserSeeOnlyPublishedVersions()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Actor);
            artifact.Save(_user);
            artifact.Publish(_user);
            artifact.Save(_user);

            List<ArtifactHistoryVersion> artifactHistory = null;
            Assert.DoesNotThrow(() =>
            {
                artifactHistory = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user2);
            }, "GetArtifactHistory shouldn't throw any error.");
            Assert.AreEqual(1, artifactHistory.Count, "Artifact history must have 1 item, but it has {0} items", artifactHistory.Count);
        }

        [TestCase]
        [TestRail(145871)]
        [Description("Create artifact, save, check that other user sees empty history")]
        public void GetHistoryForArtifactInDraft_VerifyOtherAdminUserSeeEmptyHistory()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Actor);
            artifact.Save(_user);

            List<ArtifactHistoryVersion> artifactHistory = null;
            Assert.DoesNotThrow(() =>
            {
                artifactHistory = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user2);
            }, "GetArtifactHistory shouldn't throw any error.");
            Assert.AreEqual(0, artifactHistory.Count, "Artifact history must be empty, but it has {0} items", artifactHistory.Count);
        }

        [TestCase]
        [TestRail(145896)]
        [Description("Create artifact with 12 versions, check that gethistory returns 10 items")]
        public void GetHistoryForArtifact_VerifyDefaultLimitIs10()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Actor);
            //create artifact with 12 versions
            for (int i = 0; i < 12; i++)
            {
                artifact.Save(_user);
                artifact.Publish(_user);
            }

            List<ArtifactHistoryVersion> artifactHistory = null;
            Assert.DoesNotThrow(() =>
            {
                artifactHistory = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user);
            }, "GetArtifactHistory shouldn't throw any error.");
            Assert.AreEqual(10, artifactHistory.Count, "Artifact history must have 10 items, but it has {0} items", artifactHistory.Count);//By default versions returns 10 versions
        }

        [TestCase]
        [TestRail(145897)]
        [Description("Create artifact with 11 versions, check that gethistory with limit 5 returns 5 items")]
        public void GetHistoryForArtifact_VerifyLimit5ReturnsNoMoreThan5Versions()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Actor);
            //create artifact with 11 versions
            for (int i = 0; i < 11; i++)
            {
                artifact.Save(_user);
                artifact.Publish(_user);
            }

            List<ArtifactHistoryVersion> artifactHistory = null;
            Assert.DoesNotThrow(() =>
            {
                //get first 5 versions from artifact history
                artifactHistory = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user, limit: 5);
            }, "GetArtifactHistory shouldn't throw any error.");
            Assert.AreEqual(5, artifactHistory.Count, "Artifact history must have 5 items, but it has {0} items", artifactHistory.Count);
        }

        [TestCase]
        [TestRail(145899)]
        [Description("Create artifact with 13 versions, check that gethistory with limit 12 returns 12 items")]
        public void GetHistoryForArtifact_VerifyLimit12ReturnsNoMoreThan12Versions()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Actor);
            //create artifact with 13 versions
            for (int i = 0; i < 13; i++)
            {
                artifact.Save(_user);
                artifact.Publish(_user);
            }

            List<ArtifactHistoryVersion> artifactHistory = null;
            Assert.DoesNotThrow(() =>
            {
                //get first 12 versions from artifact history
                artifactHistory = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user, limit: 12);
            }, "GetArtifactHistory shouldn't throw any error.");
            Assert.AreEqual(12, artifactHistory.Count, "Artifact history must have 12 items, but it has {0} items", artifactHistory.Count);
        }

        [TestCase]
        [TestRail(145901)]
        [Description("Create artifact with 5 versions, check that gethistory returns later versions first")]
        public void GetHistoryForArtifact_VerifyDefaultAscIsFalse()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Actor);
            //create artifact with 5 versions
            for (int i = 0; i < 5; i++)
            {
                artifact.Save(_user);
                artifact.Publish(_user);
            }

            List<ArtifactHistoryVersion> artifactHistory = null;
            Assert.DoesNotThrow(() =>
            {
                artifactHistory = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user);
            }, "GetArtifactHistory shouldn't throw any error.");
            Assert.AreEqual(5, artifactHistory.Count, "Artifact history must have 5 items, but it has {0} items", artifactHistory.Count);
            Assert.AreEqual(5, artifactHistory[0].VersionId, "VersionId must be 5, but it is {0}", artifactHistory[0].VersionId);//first version in the returned list is 5
            Assert.AreEqual(1, artifactHistory[4].VersionId, "VersionId must be 1, but it is {0}", artifactHistory[4].VersionId);//last version in the returned list is 1
        }

        [TestCase]
        [TestRail(145902)]
        [Description("Create artifact with 5 versions, check that gethistory with asc=true returns later versions last")]
        public void GetHistoryForArtifactWithAscIsTrue_VerifyOrderOfVersions()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Actor);
            //create artifact with 5 versions
            for (int i = 0; i < 5; i++)
            {
                artifact.Save(_user);
                artifact.Publish(_user);
            }

            List<ArtifactHistoryVersion> artifactHistory = null;
            Assert.DoesNotThrow(() =>
            {
                artifactHistory = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user, sortByDateAsc: true);
            }, "GetArtifactHistory shouldn't throw any error.");
            Assert.AreEqual(5, artifactHistory.Count, "Artifact history must have 5 items, but it has {0} items", artifactHistory.Count);
            Assert.AreEqual(1, artifactHistory[0].VersionId, "VersionId must be 1, but it is {0}", artifactHistory[0].VersionId);//first version in the returned list is 1
            Assert.AreEqual(5, artifactHistory[4].VersionId, "VersionId must be 5, but it is {0}", artifactHistory[4].VersionId);//last version in the returned list is 5
        }

        [TestCase]
        [TestRail(145903)]
        [Description("Create artifact with 5 versions, check that gethistory with asc=false returns later versions first")]
        public void GetHistoryForArtifactWithAscIsFalse_VerifyOrderOfVersions()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Actor);
            //create artifact with 5 versions
            for (int i = 0; i < 5; i++)
            {
                artifact.Save(_user);
                artifact.Publish(_user);
            }

            List<ArtifactHistoryVersion> artifactHistory = null;
            Assert.DoesNotThrow(() =>
            {
                artifactHistory = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user, sortByDateAsc: false);
            }, "GetArtifactHistory shouldn't throw any error.");
            Assert.AreEqual(5, artifactHistory.Count, "Artifact history must have 5 items, but it has {0} items", artifactHistory.Count);
            Assert.AreEqual(5, artifactHistory[0].VersionId, "VersionId must be 5, but it is {0}", artifactHistory[0].VersionId);//first version in the returned list is 5
            Assert.AreEqual(1, artifactHistory[4].VersionId, "VersionId must be 1, but it is {0}", artifactHistory[4].VersionId);//last version in the returned list is 1
        }

        [TestCase]
        [TestRail(145904)]
        [Description("Create artifact with 5 versions, check that gethistory with offset=2 returns 3 versions")]
        public void GetHistoryForArtifact_VerifyOffset2Skip2Versions()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Actor);
            //create artifact with 5 versions
            for (int i = 0; i < 5; i++)
            {
                artifact.Save(_user);
                artifact.Publish(_user);
            }

            List<ArtifactHistoryVersion> artifactHistory = null;
            Assert.DoesNotThrow(() =>
            {
              artifactHistory = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user, offset: 2);
            }, "GetArtifactHistory shouldn't throw any error.");
            Assert.AreEqual(3, artifactHistory.Count, "Artifact history must have 3 items, but it has {0} items", artifactHistory.Count);
            Assert.AreEqual(3, artifactHistory[0].VersionId, "VersionId must be 3, but it is {0}", artifactHistory[0].VersionId);//first version in the returned list is 3, versions 4 and 5 are skipped
            Assert.AreEqual(1, artifactHistory[2].VersionId, "VersionId must be 1, but it is {0}", artifactHistory[2].VersionId);//last version in the returned list is 1
        }

        [TestCase]
        [TestRail(145909)]
        [Description("Create artifact with 5 versions, check that gethistory with asc=true, offset=3, limit=1 returns version 4")]
        public void GetHistoryForArtifactWithNonDefaultParams_VerifyHistory()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Actor);
            //create artifact with 5 versions
            for (int i = 0; i < 5; i++)
            {
                artifact.Save(_user);
                artifact.Publish(_user);
            }

            List<ArtifactHistoryVersion> artifactHistory = null;
            Assert.DoesNotThrow(() =>
            {
                //sortByDateAsc: true, offset: 3, limit: 1 for history with 5 versions must return version 4
                artifactHistory = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user, sortByDateAsc: true, offset: 3, limit: 1);
            }, "GetArtifactHistory shouldn't throw any error.");
            Assert.AreEqual(1, artifactHistory.Count, "Artifact history must have 1 item, but it has {0} items", artifactHistory.Count);
            Assert.AreEqual(4, artifactHistory[0].VersionId, "VersionId must be 4, but it is {0}", artifactHistory[0].VersionId);
        }
    }
}
