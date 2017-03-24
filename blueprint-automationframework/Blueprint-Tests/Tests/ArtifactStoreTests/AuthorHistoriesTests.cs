using Helper;
using Model;
using NUnit.Framework;
using CustomAttributes;
using System.Collections.Generic;
using TestCommon;
using Model.Factories;
using Model.ArtifactModel.Impl;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel;

namespace ArtifactStoreTests
{
    public class AuthorHistoriesTests : TestBase
    {
        private IUser _adminUser = null;
        private IUser _viewerUser = null;
        private IProject _project = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_adminUser);
            _viewerUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        [TestCase(TestHelper.TestArtifactState.Created)]
        [TestCase(TestHelper.TestArtifactState.Published)]
        [TestCase(TestHelper.TestArtifactState.PublishedWithDraft)]
        [TestRail(267032)]
        [Description("Create artifact, publish it, get history. Verify published artifact history is returned with the expected values.")]
        public void GetArtifactAuthorHistory_PublishedArtifact_VerifyHistory(TestHelper.TestArtifactState state)
        {
            // Setup:
            var artifact = Helper.CreateNovaArtifactInSpecificState(_adminUser, _project, state, ItemTypePredefined.Actor, _project.Id);

            List<AuthorHistoryItem> artifactHistory = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                artifactHistory = Helper.ArtifactStore.GetArtifactsAuthorHistory(new List<int>{artifact.Id}, _adminUser);
            }, "GetArtifactsAuthorHistory shouldn return 200 OK when sent with valid parameters!");

            // Verify:
            if ((state == TestHelper.TestArtifactState.Published) || (state == TestHelper.TestArtifactState.PublishedWithDraft))
            {
                ValidateArtifactHistory(artifactHistory, new List<INovaArtifactDetails> { artifact });
            }

            if ((state == TestHelper.TestArtifactState.Created))
            {
                Assert.IsEmpty(artifactHistory, "Author history should be empty for never published artifact.");
            }
        }

        [TestCase]
        [TestRail(267359)]
        [Description("Create and publish two artifacts, get history. Verify artifacts history has expected values.")]
        public void GetArtifactAuthorHistory_TwoPublishedArtifact_VerifyHistory()
        {
            // Setup:
            var artifact1 = Helper.CreateNovaArtifactInSpecificState(_adminUser, _project, TestHelper.TestArtifactState.Published,
                ItemTypePredefined.Actor, _project.Id);
            var artifact2 = Helper.CreateNovaArtifactInSpecificState(_adminUser, _project, TestHelper.TestArtifactState.Published,
                ItemTypePredefined.Document, _project.Id);

            List<AuthorHistoryItem> artifactHistory = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                artifactHistory = Helper.ArtifactStore.GetArtifactsAuthorHistory(new List<int> { artifact1.Id, artifact2.Id }, _viewerUser);
            }, "GetArtifactsAuthorHistory shouldn return 200 OK when sent with valid parameters!");

            // Verify:
            ValidateArtifactHistory(artifactHistory, new List<INovaArtifactDetails> { artifact1, artifact2 });
        }

        private static void ValidateArtifactHistory(List<AuthorHistoryItem> historyItems, List<INovaArtifactDetails> artifacts)
        {
            Assert.AreEqual(artifacts.Count, historyItems.Count, "List of Author history items should have expected number of elements.");
            foreach (var artifact in artifacts)
            {
                var historyItem = historyItems.Find(item => item.ItemId == artifact.Id);
                Assert.AreEqual(artifact.CreatedBy.Id, historyItem.CreatedByUserId, "Artifacts Author History item should have expected CreatedByUserId");
                Assert.AreEqual(artifact.CreatedOn, historyItem.CreatedOn, "Artifacts Author History item should have expected CreatedOn");
                Assert.AreEqual(artifact.LastEditedOn, historyItem.LastEditedOn, "Artifacts Author History item should have expected LastEditedOn");
                Assert.AreEqual(artifact.LastEditedBy.Id, historyItem.LastEditedByUserId, "Artifacts Author History item should have expected LastEditedByUserId");
            }
        }
    }
}
