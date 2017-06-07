using Common;
using CustomAttributes;
using Helper;
using Model.Common.Enums;
using Model.Factories;
using Model.Impl;
using Model.NovaModel.Reviews;
using NUnit.Framework;
using System.Linq;
using TestCommon;
using TestConfig;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class ReviewTableOfContentTests : TestBase
    {
        const int LAST_REVISION_ID = int.MaxValue;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #region Positive tests

        [Explicit(IgnoreReasons.UnderDevelopmentDev)]  // Trello bug: https://trello.com/c/AKDTJgFY
        [Category(Categories.CannotRunInParallel)]
        [Category(Categories.GoldenData)]
        [TestCase(156, LAST_REVISION_ID)]
        [TestCase(160, 358)]
        [TestCase(160, LAST_REVISION_ID)]
        [TestRail(308878)]
        [Description("Get Review Table of Content by review id and revision id from Custom Data project with approver/reviewer user, " +
            "check that artifacts have expected values.")]
        public void GetReviewTableOfContent_ExistingReview_CheckReviewProperties(int reviewId, int revisionId)
        {
            // Setup:     
            var testConfig = TestConfiguration.GetInstance();
            string userName = testConfig.Username;
            string password = testConfig.Password;

            var user = UserFactory.CreateUserOnly(userName, password);
            Helper.AdminStore.AddSession(user);

            // Execute:
            QueryResult<ReviewTableOfContentItem> tableOfContentResponse = null;
            Assert.DoesNotThrow(() => tableOfContentResponse = Helper.ArtifactStore.GetReviewTableOfContent(user, reviewId, revisionId),
                "{0} should throw no error.", nameof(Helper.ArtifactStore.GetReviewTableOfContent));

            // Verify:
            Assert.AreEqual(3, tableOfContentResponse.Total, "TotalArtifacts should be equal to the expected number of artifacts in Review.");

            ValidateTableOfContentResponseForReview(tableOfContentResponse);
        }

        [Explicit(IgnoreReasons.UnderDevelopmentDev)]  // Trello bug: https://trello.com/c/lpZT5WXc
        [Category(Categories.GoldenData)]
        [Category(Categories.CannotRunInParallel)]
        [TestCase(112)]
        [TestRail(308879)]
        [Description("Get Review Table of Content by review id and revision id from Custom Data project with approver/reviewer user, " +
            "check that artifacts have expected values.")]
        public void GetReviewTableOfContent_ExistingReview_CheckHasAccessProperty(int reviewId)
        {
            // Setup:
            const int ARTIFACT_ID = 29;

            var user = UserFactory.CreateUserOnly(GoldenData.GoldenUsers.UserWithNoAccessToArtifact29.Username, 
                GoldenData.GoldenUsers.UserWithNoAccessToArtifact29.Password);
            Helper.AdminStore.AddSession(user);
            user.Id = int.MaxValue;

            // Execute:
            QueryResult<ReviewTableOfContentItem> tableOfContentResponse = null;
            Assert.DoesNotThrow(() => tableOfContentResponse = Helper.ArtifactStore.GetReviewTableOfContent(user, reviewId, LAST_REVISION_ID),
                "{0} should throw no error.", nameof(Helper.ArtifactStore.GetReviewTableOfContent));

            // Verify:
            Assert.AreEqual(15, tableOfContentResponse.Total, "TotalArtifacts should be equal to the expected number of artifacts in Review.");

            Assert.IsFalse(tableOfContentResponse.Items.Last().HasAccess, I18NHelper.FormatInvariant(
                "HasAccess property for artifact {0} should be False!", ARTIFACT_ID));
            Assert.IsNull(tableOfContentResponse.Items.Last().Name, I18NHelper.FormatInvariant(
                "Name property for artifact {0} should be null!", ARTIFACT_ID));
            Assert.IsNull(tableOfContentResponse.Items.Last().Prefix, I18NHelper.FormatInvariant(
                "Prefix property for artifact {0} should be null!", ARTIFACT_ID));
            Assert.IsFalse(tableOfContentResponse.Items.Last().Included, I18NHelper.FormatInvariant(
                "Included property for artifact {0} is supposed to be False!", ARTIFACT_ID));

            Assert.IsFalse(tableOfContentResponse.Items.Last().HasComments.Value, I18NHelper.FormatInvariant(
                "HasComments property for artifact {0} is supposed to be False!", ARTIFACT_ID));
            Assert.AreEqual(29, tableOfContentResponse.Items.Last().Id, I18NHelper.FormatInvariant(
                "Id property is supposed to be {0} for last artifact!", ARTIFACT_ID));
            Assert.IsFalse(tableOfContentResponse.Items.Last().IsApprovalRequired, I18NHelper.FormatInvariant(
                "IsApprovalRequired property for artifact {0} is supposed to be False!", ARTIFACT_ID));
            Assert.AreEqual(2, tableOfContentResponse.Items.Last().Level, I18NHelper.FormatInvariant(
                "Level property for artifact {0} is supposed to be 2 for last artifact!", ARTIFACT_ID));
            Assert.IsFalse(tableOfContentResponse.Items.Last().Viewed, I18NHelper.FormatInvariant(
                "Viewed property for artifact {0} is supposed to be False!", ARTIFACT_ID));
        }

        [Category(Categories.GoldenData)]
        [Category(Categories.CannotRunInParallel)]
        [TestCase(null, null, 15)]
        [TestCase(null, 5, 5)]
        [TestCase(5, null, 10)]
        [TestCase(2, 3, 3)]
        [TestCase(15, 3, 0)]
        [TestCase(0, int.MaxValue, 15)]
        [TestRail(308880)]
        [Description("Get Review Table of Content by review id and revision id from Custom Data project with " +
            "check that expected artifacts are in the response.")]
        public void GetReviewTableOfContent_ExistingReview_Filtered_CheckCorrectArtifactsReturned(int? offset, int? maxToReturn, int expectedNumberReturned)
        {
            // Setup:
            const int REVIEW_ID = 112;

            var testConfig = TestConfiguration.GetInstance();
            string userName = testConfig.Username;
            string password = testConfig.Password;

            var user = UserFactory.CreateUserOnly(userName, password);
            Helper.AdminStore.AddSession(user);

            // Execute:
            QueryResult<ReviewTableOfContentItem> tableOfContentResponse = null;
            Assert.DoesNotThrow(() => tableOfContentResponse = Helper.ArtifactStore.GetReviewTableOfContent(user, REVIEW_ID, LAST_REVISION_ID, offset, maxToReturn),
                "{0} should throw no error.", nameof(Helper.ArtifactStore.GetReviewTableOfContent));

            // Verify:
            Assert.AreEqual(15, tableOfContentResponse.Total, "TotalArtifacts should be equal to the expected number of artifacts in Review!");
            Assert.AreEqual(expectedNumberReturned, tableOfContentResponse.Items.Count(),
                "Returned artifact number should be equal to the expected number of returned artifacts!");

            if (expectedNumberReturned > 0)
            {
                ValidateReturnedArtifactIdsForReviewId112(tableOfContentResponse, offset, expectedNumberReturned);
            }
        }

        #endregion Positive tests

        #region 400 Bad Request

        #endregion 400 Bad Request

        #region 401 Unauthorized

        #endregion 401 Unauthorized

        #region 403 Forbidden

        #endregion 403 Forbidden

        #region 404 Not Found

        #endregion 404 Not Found

        #region Private functions

        /// <summary>
        /// Validates that returned items in table of content response for review 160 have specific properties.
        /// </summary>
        /// <param name="tableOfContentResponse">Actual table of content call response.</param>
        private static void ValidateTableOfContentResponseForReview(QueryResult<ReviewTableOfContentItem> tableOfContentResponse)
        {
            Assert.AreEqual(ApprovalType.NotSpecified, tableOfContentResponse.Items[0].ApprovalStatus, "Approval status is different from NotSpecified!");
            Assert.IsTrue(tableOfContentResponse.Items[0].HasAccess, "HasAccess property is supposed to be True!");
            Assert.IsTrue(tableOfContentResponse.Items[0].HasComments.Value, "HasComments property is supposed to be True!");
            Assert.IsTrue(tableOfContentResponse.Items[0].Included, "Included property is supposed to be True!");
            Assert.IsFalse(tableOfContentResponse.Items[0].IsApprovalRequired, "IsApprovedRequired property is supposed to be False!");
            Assert.AreEqual(1, tableOfContentResponse.Items[0].Level, "Level property is not 1!");
            Assert.IsTrue(tableOfContentResponse.Items[0].Viewed, "Viewed property is supposed to be False!");

            Assert.AreEqual(ApprovalType.Approved, tableOfContentResponse.Items[1].ApprovalStatus, "Approval status is different from Approved!");
            Assert.IsTrue(tableOfContentResponse.Items[1].HasAccess, "HasAccess property is supposed to be True!");
            Assert.IsFalse(tableOfContentResponse.Items[1].HasComments.Value, "HasComments property is supposed to be False!");
            Assert.IsTrue(tableOfContentResponse.Items[1].Included, "Included property is supposed to be True!");
            Assert.IsTrue(tableOfContentResponse.Items[1].IsApprovalRequired, "IsApprovedRequired property is supposed to be True!");
            Assert.AreEqual(2, tableOfContentResponse.Items[1].Level, "Level property is not 2!");
            Assert.IsTrue(tableOfContentResponse.Items[1].Viewed, "Viewed property is supposed to be True!");

            Assert.AreEqual(ApprovalType.Disapproved, tableOfContentResponse.Items[2].ApprovalStatus, "Approval status is different from Disapproved!");
            Assert.IsTrue(tableOfContentResponse.Items[2].HasAccess, "HasAccess property is supposed to be True!");
            Assert.IsFalse(tableOfContentResponse.Items[2].HasComments.Value, "HasComments property is supposed to be False!");
            Assert.IsTrue(tableOfContentResponse.Items[2].Included, "Included property is supposed to be True!");
            Assert.IsTrue(tableOfContentResponse.Items[2].IsApprovalRequired, "IsApprovedRequired property is supposed to be True!");
            Assert.AreEqual(3, tableOfContentResponse.Items[2].Level, "Level property is not 3!");
            Assert.IsTrue(tableOfContentResponse.Items[2].Viewed, "Viewed property is supposed to be True!");
        }

        /// <summary>
        /// Validates that returned items in table of content response for review 112 are the same as expected.
        /// </summary>
        /// <param name="tableOfContentResponse">Actual table of content call response.</param>
        /// <param name="offset">(optional)Offset from the beginning of artifact list. By default starts from the first item.</param>
        /// <param name="numberReturned">Expected number of items to return.</param>
        private static void ValidateReturnedArtifactIdsForReviewId112(QueryResult<ReviewTableOfContentItem> tableOfContentResponse, int? offset, int numberReturned)
        {
            int[] ids = new int[] { 7, 15, 16, 33, 36, 31, 49, 40, 34, 32, 23, 22, 17, 24, 29 };

            offset = offset ?? 0;

            for (int i = 0; i < numberReturned; i++)
            {
                Assert.AreEqual(ids[offset.Value + i], tableOfContentResponse.Items[i].Id, "Expected artifact id is different from actual artifact id!");
            }
        }

        #endregion Private functions
    }
}
