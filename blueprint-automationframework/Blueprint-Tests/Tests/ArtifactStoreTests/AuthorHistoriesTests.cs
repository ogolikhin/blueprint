using Helper;
using Model;
using Model.Impl;
using NUnit.Framework;
using CustomAttributes;
using System.Collections.Generic;
using TestCommon;
using Model.Factories;
using Model.ArtifactModel.Impl;
using Model.ArtifactModel.Enums;

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

        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)]
        [TestCase]
        [TestRail(267032)]
        [Description("Create artifact, publish it, get history.  Verify 1 published artifact history is returned with the expected values.")]
        public void GetArtifactAuthorHistory_PublishedArtifact_VerifyHistory()
        {
            // Setup:
            var artifact = Helper.CreateNovaArtifactInSpecificState(_adminUser, _project, 
                TestHelper.TestArtifactState.ScheduledToDelete, ItemTypePredefined.Actor, _project.Id);

            List<AuthorHistoryItem> artifactHistory = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                artifactHistory = ArtifactStore.GetArtifactsAuthorHistory(Helper.ArtifactStore.Address, new List<int>{artifact.Id}, _viewerUser);
            }, "GetArtifactsAuthorHistory shouldn return 200 OK when sent with valid parameters!");

            // Verify:
            Assert.AreEqual(1, artifactHistory.Count, "Artifacts Author History history must have 1 item, but it has {0} items", artifactHistory.Count);
            Assert.AreEqual(artifact.CreatedBy.Id, artifactHistory[0].CreatedByUserId, "Artifacts Author History item should have expected CreatedByUserId");
            Assert.AreEqual(artifact.Id, artifactHistory[0].ItemId, "Artifacts Author History item should have expected Id");
        }
    }
}
