using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.NovaModel.Reviews;
using NUnit.Framework;
using TestCommon;
using Utilities;
using TestConfig;
using System.Collections.Generic;
using Model.Impl;
using System.Linq;
using Model.ArtifactModel.Impl;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class ReviewTests : TestBase
    {
        private IUser _adminUser = null;
        private IUser _user = null;
        private IProject _projectCustomData = null;

        const int REVISION_ID = int.MaxValue;

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

        #region Positive Tests

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
            QueryResult<ReviewArtifact> reviewArtifacts = null;
            Assert.DoesNotThrow(() => reviewArtifacts = Helper.ArtifactStore.GetReviewArtifacts(_user, reviewId),
                "{0} should be successful.", nameof(Helper.ArtifactStore.GetReviewArtifacts));

            // Verify:
            Assert.AreEqual(numberOfArtifacts, reviewArtifacts.Total, "GetReviewArtifacts should return expected number of artifacts.");
            Assert.AreEqual(numberOfArtifacts, reviewArtifacts.Items.Count, "GetReviewArtifacts should return expected number of artifacts.");
        }

        [Category(Categories.GoldenData)]
        [Category(Categories.CannotRunInParallel)]
        [TestCase]
        [TestRail(303353)]
        [Description("Get Review Content by id from Custom Data project, check that artifacts have expected values.")]
        public void GetReviewContainer_ExistingReview_Reviewer_CheckReviewProperties()
        {
            // Setup:
            _projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_adminUser);
            const int REVIEW_ID = 112;

            var testConfig = TestConfiguration.GetInstance();
            string userName = testConfig.Username;
            string password = testConfig.Password;

            var sessionToken = Helper.AdminStore.AddSession(userName, password);
            var admin = UserFactory.CreateUserOnly(userName, password);
            admin.SetToken(sessionToken.SessionId);

            // Execute: 
            ReviewSummary reviewContainer = null;
            Assert.DoesNotThrow(() => reviewContainer = Helper.ArtifactStore.GetReviewContainer(admin, REVIEW_ID),
                "{0} should throw no error.", nameof(Helper.ArtifactStore.GetReviewContainer));

            // Verify:
            Assert.AreEqual(15, reviewContainer.TotalArtifacts, "TotalArtifacts should be equal to the expected number of artifacts in Review.");
        }

        [Category(Categories.GoldenData)]
        [TestCase]
        [TestRail(0)]
        [Description("Get Review Table of Content by rview id and revision id from Custom Data project, check that artifacts have expected values.")]
        public void GetReviewTableOfContent_ExistingReview_Reviewer_CheckReviewProperties()
        {
            // Setup:
            _projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_adminUser);
            const int REVIEW_ID = 112;
            const int REVISION_ID = 239;

            var testConfig = TestConfiguration.GetInstance();
            string userName = testConfig.Username;
            string password = testConfig.Password;

            var sessionToken = Helper.AdminStore.AddSession(userName, password);
            var reviewer = UserFactory.CreateUserOnly(userName, password);
            reviewer.SetToken(sessionToken.SessionId);

            // Execute:
            ReviewTableOfContent reviewContainer = null;
            Assert.DoesNotThrow(() => reviewContainer = Helper.ArtifactStore.GetReviewTableOfContent(reviewer, REVIEW_ID, REVISION_ID),
                "{0} should throw no error.", nameof(Helper.ArtifactStore.GetReviewContainer));

            // Verify:
            Assert.AreEqual(15, reviewContainer.Total, "TotalArtifacts should be equal to the expected number of artifacts in Review.");
        }

        [Category(Categories.GoldenData)]
        [TestCase]
        [TestRail(0)]
        [Description("Get Review Table of Content by rview id and revision id from Custom Data project, check that artifacts have expected values.")]
        public void GetReviewTableOfContent_ExistingReview_Reviewer_CheckLevelProperty()
        {
            // Setup:
            _projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_adminUser);
            const int REVIEW_ID = 160;
            
            var testConfig = TestConfiguration.GetInstance();
            string userName = testConfig.Username;
            string password = testConfig.Password;

            var sessionToken = Helper.AdminStore.AddSession(userName, password);
            var reviewer = UserFactory.CreateUserOnly(userName, password);
            reviewer.SetToken(sessionToken.SessionId);

            // Execute:
            ReviewTableOfContent tableOfContentResponse = null;
            Assert.DoesNotThrow(() => tableOfContentResponse = Helper.ArtifactStore.GetReviewTableOfContent(reviewer, REVIEW_ID, REVISION_ID),
                "{0} should throw no error.", nameof(Helper.ArtifactStore.GetReviewContainer));

            // Verify:
            Assert.AreEqual(3, tableOfContentResponse.Total, "TotalArtifacts should be equal to the expected number of artifacts in Review.");

            ValidateTableOfContentResponce(tableOfContentResponse);
        }




        [Category(Categories.GoldenData)]
        [TestCase]
        [TestRail(303522)]
        [Description("Get Review Participants by Review id from Custom Data project should return expected number of reviewers.")]
        public void GetReviewParticipants_ExistingReview_CheckReviewersCount()
        {
            // Setup:
            _projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_adminUser);
            const int REVIEW_ID = 111;

            // Execute:
            ReviewParticipantsContent reviewParticipants = null;
            Assert.DoesNotThrow(() =>
            reviewParticipants = Helper.ArtifactStore.GetReviewParticipants(_adminUser, REVIEW_ID), "GetReviewParticipants " +
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
            const int artifactId = 22;

            // Execute:
            QueryResult<ReviewArtifactDetails> reviewParticipants = null;
            Assert.DoesNotThrow(() =>
            reviewParticipants = Helper.ArtifactStore.GetArtifactStatusesByParticipant(_adminUser, artifactId, reviewId,
            versionId: 1), "GetArtifactStatusesByParticipant should return 200 success.");

            // Verify:
            Assert.AreEqual(1, reviewParticipants.Items.Count, "Specified artifact should have one reviewer.");
        }

        [Category(Categories.GoldenData)]
        [TestCase]
        [TestRail(304008)]
        [Description("Adding Artifact to the Review by Artifact Id and Review Id from Custom Data project. Call should return expected number of added artifacts.")]
        public void AddArtifactToReview_PublishedArtifact_CheckReturnedObject()
        {
            // Setup:
            const int artifactToAddId = 22;
            const int reviewId = 113; // TODO: when real server-side call will be implemented review should be replaced

            // either with newly created one or with the copy of existing review
            AddArtifactsParameter content = new AddArtifactsParameter();
            content.ArtifactIds = new List<int> { artifactToAddId };
            content.AddChildren = false;

            // Execute:
            AddArtifactsResult addArtifactResult = null;
            Assert.DoesNotThrow(() => addArtifactResult = Helper.ArtifactStore.AddArtifactsToReview(_adminUser, reviewId,
                content), "AddArtifactsToReview should return 200 success.");

            // Verify:
            Assert.AreEqual(1, addArtifactResult.ArtifactCount, "Number of added artifacts should have expected value.");
        }

        [Category(Categories.GoldenData)]
        [Category(Categories.CannotRunInParallel)]
        [TestCase]
        [TestRail(305010)]
        [Description("Get Review Artifacts (Review Experience) by id from Custom Data project, check that number of artifacts has expected value.")]
        public void GetReviewArtifacts_ExistingReview_Reviewer_CheckArtifactsCount()
        {
            // Setup:
            const int reviewId = 112;

            var testConfig = TestConfiguration.GetInstance();
            string userName = testConfig.Username;
            string password = testConfig.Password;

            var sessionToken = Helper.AdminStore.AddSession(userName, password);
            var admin = UserFactory.CreateUserOnly(userName, password);
            admin.SetToken(sessionToken.SessionId);

            // Execute: 
            QueryResult<ReviewedArtifact> reviewedArtifacts = null;
            Assert.DoesNotThrow(() => reviewedArtifacts = Helper.ArtifactStore.GetReviewedArtifacts(_adminUser, reviewId),
                "{0} should throw no error.", nameof(Helper.ArtifactStore.GetReviewedArtifacts));

            // Verify:
            Assert.AreEqual(15, reviewedArtifacts.Items.Count, "TotalArtifacts should be equal to the expected number of artifacts in Review.");
        }

        #region Artifacts Hierarchy Tests

        [Category(Categories.GoldenData)]
        [Category(Categories.CannotRunInParallel)]
        [Explicit(IgnoreReasons.DeploymentNotReady)]
        [TestCase]
        [TestRail(305070)]
        [Description("Get Review Artifacts (Review Experience) by id from Custom Data project, check that artifacts are ordered by OrderIndex.")]
        public void GetReviewArtifacts_ExistingReview_Reviewer_ValidateHierarchy()
        {
            // Setup:
            const int reviewId = 112;

            var testConfig = TestConfiguration.GetInstance();
            string userName = testConfig.Username;
            string password = testConfig.Password;

            var sessionToken = Helper.AdminStore.AddSession(userName, password);
            var testUser = UserFactory.CreateUserOnly(userName, password);
            testUser.SetToken(sessionToken.SessionId);

            var reviewContainer = Helper.ArtifactStore.GetReviewContainer(testUser, reviewId);

            QueryResult<ReviewedArtifact> reviewedArtifacts = null;

            // Execute: 
            Assert.DoesNotThrow(() => reviewedArtifacts = Helper.ArtifactStore.GetReviewedArtifacts(testUser, reviewId,
                revisionId: reviewContainer.RevisionId),
                "{0} should throw no error.", nameof(Helper.ArtifactStore.GetReviewedArtifacts));

            List<NovaArtifactDetails> artifacts = new List<NovaArtifactDetails>();
            foreach (var a in reviewedArtifacts.Items)
            {
                artifacts.Add(Helper.ArtifactStore.GetArtifactDetails(testUser, a.Id));
            }

            // Verify:
            ValidateArtifactsHierarchy(artifacts, reviewedArtifacts.Items);
        }

        #endregion Artifacts Hierarchy Tests

        #endregion Positive Tests

        #region 400 Bad Request

        #endregion 400 Bad Request

        #region 401 Unauthorized

        #endregion 401 Unauthorized

        #region 403 Forbidden

        [Explicit()]
        [Category(Categories.GoldenData)]
        [TestCase()]
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

        #endregion 403 Forbidden

        #region 404 Not Found

        #endregion 404 Not Found

        #region Private functions

        private static void ValidateTableOfContentResponce(ReviewTableOfContent tableOfContentResponse)
        {
            var items = (List<ReviewTableOfContentItem>)tableOfContentResponse.Items;

            Assert.AreEqual(ApprovalType.NotSpecified, items[0].ApprovalStatus, "Approval status is different from NotSpecified!");
            Assert.IsTrue(items[0].HasAccess, "HasAccess property is supposed to be True!");
            Assert.IsTrue(items[0].HasComments.Value, "HasComments property is supposed to be True!");
            Assert.IsTrue(items[0].Included, "Included property is supposed to be True!");
            Assert.IsFalse(items[0].IsApprovalRequired, "IsApprovedRequired property is supposed to be False!");
            Assert.AreEqual(1, items[0].Level, "Level property is not 1!");
            Assert.IsFalse(items[0].Viewed, "Viewed property is supposed to be False!");

            Assert.AreEqual(ApprovalType.Approved, items[0].ApprovalStatus, "Approval status is different from Approved!");
            Assert.IsTrue(items[0].HasAccess, "HasAccess property is supposed to be True!");
            Assert.IsFalse(items[0].HasComments.Value, "HasComments property is supposed to be False!");
            Assert.IsTrue(items[0].Included, "Included property is supposed to be True!");
            Assert.IsTrue(items[0].IsApprovalRequired, "IsApprovedRequired property is supposed to be True!");
            Assert.AreEqual(2, items[0].Level, "Level property is not 2!");
            Assert.IsTrue(items[0].Viewed, "Viewed property is supposed to be True!");

            Assert.AreEqual(ApprovalType.Disapproved, items[0].ApprovalStatus, "Approval status is different from Disapproved!");
            Assert.IsTrue(items[0].HasAccess, "HasAccess property is supposed to be True!");
            Assert.IsFalse(items[0].HasComments.Value, "HasComments property is supposed to be False!");
            Assert.IsTrue(items[0].Included, "Included property is supposed to be True!");
            Assert.IsTrue(items[0].IsApprovalRequired, "IsApprovedRequired property is supposed to be True!");
            Assert.AreEqual(3, items[0].Level, "Level property is not 3!");
            Assert.IsTrue(items[0].Viewed, "Viewed property is supposed to be True!");
        }

        /// <summary>
        /// Checks that artifacts in review ordered by OrderIndex.
        /// </summary>
        /// <param name="artifactsFromProject">List of Project's artifact.</param>
        /// <param name="artifactsFromReview">List of artifacts from Review.</param>
        private static void ValidateArtifactsHierarchy(List<NovaArtifactDetails> artifactsFromProject,
            List<ReviewedArtifact> artifactsFromReview)
        {
            var artifactsSortedByOrderIndex = artifactsFromProject.OrderBy(i => i.OrderIndex).ToList();
            Assert.AreEqual(artifactsFromReview.Count, artifactsSortedByOrderIndex.Count, "Number of artifacts in these two lists should be the same.");
            for (int i = 0; i < artifactsFromReview.Count; i++)
            {
                Assert.AreEqual(artifactsSortedByOrderIndex[i].Id, artifactsFromReview[i].Id,
                    "Ids of artifacts in the same position of the lists should be equal.");
            }
        }

        #endregion Private Functions
    }
}