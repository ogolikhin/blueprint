using System;
using Helper;
using Model;
using Model.Impl;
using NUnit.Framework;
using CustomAttributes;
using System.Collections.Generic;
using TestCommon;
using Model.ArtifactModel;
using Model.Factories;
using Model.ArtifactModel.Impl;
using Utilities;
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

        [TestCase]
        [TestRail(7)]
        [Description("Create artifact, publish it, get history.  Verify 1 published artifact history is returned with the expected values.")]
        public void GetArtifactAuthorHistory_PublishedWithDraft_VerifyHistoryHasExpectedValue()
        {
            // Setup:
            Helper.CreateNovaArtifactInSpecificState(_adminUser, _project, 
                TestHelper.TestArtifactState.ScheduledToDelete, ItemTypePredefined.Actor, _project.Id);

            List<AuthorHistoryItem> artifactHistory = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                artifactHistory = ArtifactStore.GetArtifactsAuthorHistory(Helper.ArtifactStore.Address, Helper.NovaArtifacts[_adminUser], _viewerUser);
            }, "GetArtifactsAuthorHistory shouldn return 200 OK when sent with valid parameters!");

            // Verify:
            Assert.AreEqual(1, artifactHistory.Count, "Artifacts Author History history must have 1 item, but it has {0} items", artifactHistory.Count);
        }
    }
}
