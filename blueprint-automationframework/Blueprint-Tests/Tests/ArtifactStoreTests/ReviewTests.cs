﻿using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel.Impl.OperationsResults;
using Model.NovaModel.Impl;
using Model.Factories;
using NUnit.Framework;
using TestCommon;
using Utilities;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class ReviewTests : TestBase
    {
        private IUser _adminUser = null;
        private IUser _user = null;
        private IProject _projectCustomData = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        [Category(Categories.GoldenData)]
        [TestCase]
        [TestRail(290224)]
        [Description("Get Review Content by id from Custom Data project, check that artifacts have expected values.")]
        public void GetReviewArtifacts_ExistingReview_CheckArtifactsCount()
        {
            // Setup:
            _projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_adminUser);
            _user = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _projectCustomData);
            const int reviewId = 111;
            const int numberOfArtifacts = 15;
            
            // Execute: 
            GetReviewArtifactsResultSet reviewArtifacts = null;
            Assert.DoesNotThrow(() => reviewArtifacts = Helper.ArtifactStore.GetReviewArtifacts(_user, reviewId),
                "{0} should be successful.", nameof(Helper.ArtifactStore.GetReviewArtifacts));

            // Verify:
            Assert.AreEqual(numberOfArtifacts, reviewArtifacts.Total, "GetReviewArtifacts should return expected number of artifacts.");
            Assert.AreEqual(numberOfArtifacts, reviewArtifacts.Items.Count, "GetReviewArtifacts should return expected number of artifacts.");
        }

        [Category(Categories.GoldenData)]
        [TestCase]
        [TestRail(303353)]
        [Description("Get Review Content by id from Custom Data project, check that artifacts have expected values.")]
        public void GetReviewContainer_ExistingReview_Reviewer_CheckReviewProperties()
        {
            // Setup:
            _projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_adminUser);
            const int reviewId = 111;
            var sessionToken = Helper.AdminStore.AddSession("admin", "$Admin93");
            var admin = UserFactory.CreateUserOnly("admin", "$Admin93");
            admin.SetToken(sessionToken.SessionId);

            ReviewContainer reviewContainer = null;

            // Execute: 
            Assert.DoesNotThrow(() => reviewContainer = Helper.ArtifactStore.GetReviewContainer(admin, reviewId),
                "{0} should return 403 for non-reviewer user.", nameof(Helper.ArtifactStore.GetReviewContainer));

            // Verify:
            Assert.AreEqual(15, reviewContainer.TotalArtifacts, "TotalArtifacts should be equal to the expected number of artifacts in Review.");
        }

        [Category(Categories.GoldenData)]
        [TestCase]
        [TestRail(303349)]
        [Description("Get Review Content by id from Custom Data project, non-reviewer user, check that server returns 403.")]
        public void GetReviewContainer_ExistingReview_NonReviewer_Returns403()
        {
            // Setup:
            _projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_adminUser);
            const int reviewId = 111;

            // Execute & Verify: 
            Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.GetReviewContainer(_adminUser, reviewId),
                "{0} should return 403 for non-reviewer user.", nameof(Helper.ArtifactStore.GetReviewContainer));
        }
    }
}