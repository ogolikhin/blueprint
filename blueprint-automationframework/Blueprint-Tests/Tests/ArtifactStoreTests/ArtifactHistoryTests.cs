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
            Assert.AreEqual(_user.UserId, artifactHistory[0].userId);
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

        [TestCase]
        [TestRail(145896)]
        [Description("...")]
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
            });
            Assert.AreEqual(10, artifactHistory.Count);//By default versions returns 10 versions
        }

        [TestCase]
        [TestRail(145897)]
        [Description("...")]
        public void GetHistoryForArtifact_VerifyLimit5ReturnsNoMoreThan5Versions()
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
                //get first 5 versions from artifact history
                artifactHistory = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user, limit: 5);
            });
            Assert.AreEqual(5, artifactHistory.Count);
        }

        [TestCase]
        [TestRail(145899)]
        [Description("...")]
        public void GetHistoryForArtifact_VerifyLimit12ReturnsNoMoreThan12Versions()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Actor);
            //create artifact with 15 versions
            for (int i = 0; i < 15; i++)
            {
                artifact.Save(_user);
                artifact.Publish(_user);
            }

            List<ArtifactHistoryVersion> artifactHistory = null;
            Assert.DoesNotThrow(() =>
            {
                //get first 12 versions from artifact history
                artifactHistory = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user, limit: 12);
            });
            Assert.AreEqual(12, artifactHistory.Count);
        }

        [TestCase]
        [TestRail(145901)]
        [Description("...")]
        public void GetHistoryForArtifact_VerifyDeafaultAscIsFalse()
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
            });
            Assert.AreEqual(5, artifactHistory.Count);
            Assert.AreEqual(5, artifactHistory[0].versionId);//first version in the returned list is 5
            Assert.AreEqual(1, artifactHistory[4].versionId);//last version in the returned list is 1
        }

        [TestCase]
        [TestRail(145902)]
        [Description("...")]
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
            });
            Assert.AreEqual(5, artifactHistory.Count);
            Assert.AreEqual(1, artifactHistory[0].versionId);//first version in the returned list is 1
            Assert.AreEqual(5, artifactHistory[4].versionId);//last version in the returned list is 5
        }

        [TestCase]
        [TestRail(145903)]
        [Description("...")]
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
            });
            Assert.AreEqual(5, artifactHistory.Count);
            Assert.AreEqual(5, artifactHistory[0].versionId);//first version in the returned list is 5
            Assert.AreEqual(1, artifactHistory[4].versionId);//last version in the returned list is 1
        }

        [TestCase]
        [TestRail(145904)]
        [Description("...")]
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
            });
            Assert.AreEqual(3, artifactHistory.Count);
            Assert.AreEqual(3, artifactHistory[0].versionId);//first version in the returned list is 3, versions 4 and 5 are skipped
            Assert.AreEqual(1, artifactHistory[2].versionId);//last version in the returned list is 1
        }

        [TestCase]
        [TestRail(145909)]
        [Description("...")]
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
            });
            Assert.AreEqual(1, artifactHistory.Count);
            Assert.AreEqual(4, artifactHistory[0].versionId);
        }
    }
}
