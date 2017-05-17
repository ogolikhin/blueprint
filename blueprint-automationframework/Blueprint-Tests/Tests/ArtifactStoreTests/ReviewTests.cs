using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel.Impl.OperationsResults;
using Model.NovaModel.Impl;
using Model.Factories;
using NUnit.Framework;
using TestCommon;
using Utilities;
using TestConfig;

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
            ReviewContent reviewArtifacts = null;
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
            const int reviewId = 112;

            var testConfig = TestConfiguration.GetInstance();
            string userName = testConfig.Username;
            string password = testConfig.Password;

            var sessionToken = Helper.AdminStore.AddSession(userName, password);
            var admin = UserFactory.CreateUserOnly(userName, password);
            admin.SetToken(sessionToken.SessionId);

            ReviewContainer reviewContainer = null;

            // Execute: 
            Assert.DoesNotThrow(() => reviewContainer = Helper.ArtifactStore.GetReviewContainer(admin, reviewId),
                "{0} should throw no error.", nameof(Helper.ArtifactStore.GetReviewContainer));

            // Verify:
            Assert.AreEqual(15, reviewContainer.TotalArtifacts, "TotalArtifacts should be equal to the expected number of artifacts in Review.");
        }

        [Category(Categories.GoldenData)]
        [TestCase]
        [TestRail(303349)]
        [Description("Get Review Content by id from Custom Data project, non-reviewer user, check that server returns 403.")]
        public void GetReviewContainer_ExistingReview_NonReviewer_403Forbidden()
        {
            // Setup:
            _projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_adminUser);
            const int reviewId = 112;

            // Execute & Verify: 
            Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.GetReviewContainer(_adminUser, reviewId),
                "{0} should return 403 for non-reviewer user.", nameof(Helper.ArtifactStore.GetReviewContainer));
        }

        [Category(Categories.GoldenData)]
        [TestCase]
        [TestRail(303522)]
        [Description("Get Review Participants by Review id from Custom Data project should return expected number of reviewers.")]
        public void GetReviewParticipants_ExistingReview_CheckReviewersCount()
        {
            // Setup:
            _projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_adminUser);
            const int reviewId = 111;

            ReviewParticipantsContent reviewParticipants = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            reviewParticipants = Helper.ArtifactStore.GetReviewParticipants(_adminUser, reviewId), "GetReviewParticipants " +
            "should return 200 success.");

            // Verify:
            Assert.AreEqual(1, reviewParticipants.Total, "ReviewParticipantsContent should have expected number of Reviewers.");
            Assert.AreEqual(1, reviewParticipants.Items?.Count, "List of Reviewers should have expected number of items.");
        }

        [Category(Categories.GoldenData)]
        [TestCase]
        [TestRail(303891)]
        [Description("Get Artifact approval statuses by Artifact Id and Review id from Custom Data project should return expected number of reviewers.")]
        public void GetReviewArtifactParticipants_ExistingReview_CheckReviewersCount()
        {
            // Setup:
            _projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_adminUser);
            const int reviewId = 113;
            const int artifactId = 22; //15

            ArtifactReviewContent reviewParticipants = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            reviewParticipants = Helper.ArtifactStore.GetArtifactStatusesByParticipant(_adminUser, artifactId, reviewId,
            versionId: 1), "GetArtifactStatusesByParticipant should return 200 success.");

            // Verify:
            Assert.AreEqual(1, reviewParticipants.Items.Count, "Specified artifact should have one reviewer.");
        }
    }
}