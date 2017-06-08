﻿using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.Common.Enums;
using Model.Factories;
using Model.Impl;
using Model.NovaModel.Reviews;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TestCommon;
using TestConfig;
using Utilities;
using Utilities.Facades;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class ReviewTableOfContentTests : TestBase
    {
        const int LAST_REVISION_ID = int.MaxValue;
        const int REVIEW_ID_112 = 112;
        const int REVISION_ID_239 = 239;

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
        [TestCase(REVIEW_ID_112)]
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

            // Execute:
            QueryResult<ReviewTableOfContentItem> tableOfContentResponse = null;
            Assert.DoesNotThrow(() => tableOfContentResponse = Helper.ArtifactStore.GetReviewTableOfContent(user, reviewId, LAST_REVISION_ID),
                "{0} should throw no error.", nameof(Helper.ArtifactStore.GetReviewTableOfContent));

            // Verify:
            Assert.AreEqual(15, tableOfContentResponse.Total, "TotalArtifacts should be equal to the expected number of artifacts in Review.");

            Assert.IsNull(tableOfContentResponse.Items.Last().Name, I18NHelper.FormatInvariant(
                "Name property for artifact {0} should be null!", ARTIFACT_ID));
            Assert.IsNull(tableOfContentResponse.Items.Last().Prefix, I18NHelper.FormatInvariant(
                "Prefix property for artifact {0} should be null!", ARTIFACT_ID));
            Assert.IsFalse(tableOfContentResponse.Items.Last().Included, I18NHelper.FormatInvariant(
                "Included property for artifact {0} is supposed to be False!", ARTIFACT_ID));
            Assert.IsFalse(tableOfContentResponse.Items.Last().Viewed, I18NHelper.FormatInvariant(
                "Viewed property for artifact {0} is supposed to be False!", ARTIFACT_ID));
            Assert.IsFalse(tableOfContentResponse.Items.Last().HasAccess, I18NHelper.FormatInvariant(
                "HasAccess property for artifact {0} should be False!", ARTIFACT_ID));
            Assert.IsFalse(tableOfContentResponse.Items.Last().IsApprovalRequired, I18NHelper.FormatInvariant(
                "IsApprovalRequired property for artifact {0} is supposed to be False!", ARTIFACT_ID));

            Assert.IsFalse(tableOfContentResponse.Items.Last().HasComments.Value, I18NHelper.FormatInvariant(
                "HasComments property for artifact {0} is supposed to be False!", ARTIFACT_ID));
            Assert.AreEqual(29, tableOfContentResponse.Items.Last().Id, I18NHelper.FormatInvariant(
                "Id property is supposed to be {0} for last artifact!", ARTIFACT_ID));

            Assert.AreEqual(2, tableOfContentResponse.Items.Last().Level, I18NHelper.FormatInvariant(
                "Level property for artifact {0} is supposed to be 2 for last artifact!", ARTIFACT_ID));

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
            var testConfig = TestConfiguration.GetInstance();
            string userName = testConfig.Username;
            string password = testConfig.Password;

            var user = UserFactory.CreateUserOnly(userName, password);
            Helper.AdminStore.AddSession(user);

            // Execute:
            QueryResult<ReviewTableOfContentItem> tableOfContentResponse = null;
            Assert.DoesNotThrow(() => tableOfContentResponse = Helper.ArtifactStore.GetReviewTableOfContent(user, REVIEW_ID_112, LAST_REVISION_ID, offset,
                maxToReturn), "{0} should throw no error.", nameof(Helper.ArtifactStore.GetReviewTableOfContent));

            // Verify:
            Assert.AreEqual(15, tableOfContentResponse.Total, "TotalArtifacts should be equal to the expected number of artifacts in Review!");
            Assert.AreEqual(expectedNumberReturned, tableOfContentResponse.Items.Count(),
                "Returned artifact number should be equal to the expected number of returned artifacts!");

            if (expectedNumberReturned > 0)
            {
                ValidateReturnedArtifactIdsForReviewId112(tableOfContentResponse, offset, expectedNumberReturned);
            }
        }

        [TestCase("5", "A", 10)]
        [TestCase("A", "5", 5)]
        [TestRail(308914)]
        [Description("Get review table of content by review id and revision id from Custom Data project with revisionId as a character, " +
            "check that server returns 400 Bad Request.")]
        public void GetReviewTableOfContent_BadParameters(string offset, string maxToReturn, int expectedNumberReturned)
        {
            var testConfig = TestConfiguration.GetInstance();
            string userName = testConfig.Username;
            string password = testConfig.Password;

            var user = UserFactory.CreateUserOnly(userName, password);
            Helper.AdminStore.AddSession(user);

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Containers_id_.TOC_id_, REVIEW_ID_112, REVISION_ID_239);
            var restApi = new RestApiFacade(Helper.BlueprintServer.Address, user?.Token?.AccessControlToken);

            var queryParams = new Dictionary<string, string>();

            if (offset != null)
            {
                queryParams.Add("offset", offset);
            }

            if (maxToReturn != null)
            {
                queryParams.Add("limit", maxToReturn);
            }

            // Execute:
            QueryResult<ReviewTableOfContentItem> tableOfContentResponse = null;
            Assert.DoesNotThrow(() => tableOfContentResponse = restApi.SendRequestAndDeserializeObject<QueryResult<ReviewTableOfContentItem>>(path,
                RestRequestMethod.GET, queryParameters: queryParams, shouldControlJsonChanges: true),
                "{0} should return 200 OK for call with bad parameter.", nameof(Helper.ArtifactStore.GetReviewTableOfContent));

            // Verify:
            Assert.AreEqual(15, tableOfContentResponse.Total, "TotalArtifacts should be equal to the expected number of artifacts in Review!");
            Assert.AreEqual(expectedNumberReturned, tableOfContentResponse.Items.Count(),
                "Returned artifact number should be equal to the expected number of returned artifacts!");

            int parsedOffset;
            if (!int.TryParse(offset, out parsedOffset))
            {
                parsedOffset = 0;
            };

            ValidateReturnedArtifactIdsForReviewId112(tableOfContentResponse, parsedOffset, expectedNumberReturned);
        }

        #endregion Positive tests

        #region 401 Unauthorized

        [TestCase()]
        [TestRail(308915)]
        [Description("Get review table of content by review id and revision id from Custom Data project with user that has invalid token, " +
            "check that server returns 401 Unauthorized.")]
        public void GetReviewTableOfContent_InvalidToken_401Unauthorized()
        {
            // Setup:
            var testConfig = TestConfiguration.GetInstance();
            string userName = testConfig.Username;
            string password = testConfig.Password;

            var user = UserFactory.CreateUserOnly(userName, password);
            Helper.AdminStore.AddSession(user);
            user.SetToken(CommonConstants.InvalidToken);

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() => Helper.ArtifactStore.GetReviewTableOfContent(user, REVIEW_ID_112, REVISION_ID_239),
                "{0} should return 401 Unauthorized for user with bad token.", nameof(Helper.ArtifactStore.GetReviewTableOfContent));

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, "Token is invalid.");
        }

        #endregion 401 Unauthorized

        #region 403 Forbidden

        [Category(Categories.GoldenData)]
        [TestCase()]
        [TestRail(308916)]
        [Description("Get review table of content by review id and revision id from Custom Data project with non-reviewer user, " +
            "check that server returns 403 Forbidden.")]
        public void GetReviewTableOfContent_ExistingReview_NonReviewer_403Forbidden()
        {
            // Setup:
            var adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            // Execute: 
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.GetReviewTableOfContent(adminUser, REVIEW_ID_112, REVISION_ID_239),
                "{0} should return 403 for non-reviewer user.", nameof(Helper.ArtifactStore.GetReviewTableOfContent));

            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.UnauthorizedAccess,
                I18NHelper.FormatInvariant("User does not have permissions to access the review (Id:{0}).", REVIEW_ID_112));
        }

        #endregion 403 Forbidden

        #region 404 Not Found

        [Explicit(IgnoreReasons.UnderDevelopmentDev)]  // Trello bug: https://trello.com/c/Cyp0wdhh
        [TestCase(REVIEW_ID_112, 1)]
        [TestCase(1, REVISION_ID_239)]
        [TestRail(308917)]
        [Description("Get review table of content by using non-existing review id or revision id from Custom Data project " +
            "check that server returns 404 Not Found.")]
        public void GetReviewTableOfContent_NonExistingReviewOrRevisionId_404NotFound(int reviewId, int revisionId)
        {
            // Setup:
            var testConfig = TestConfiguration.GetInstance();
            string userName = testConfig.Username;
            string password = testConfig.Password;

            var user = UserFactory.CreateUserOnly(userName, password);
            Helper.AdminStore.AddSession(user);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetReviewTableOfContent(user, reviewId, revisionId),
                "{0} should return 404 Not Found for review or revision Ids that does not belong to review.", nameof(Helper.ArtifactStore.GetReviewTableOfContent));

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.ResourceNotFound,
                I18NHelper.FormatInvariant("Review (Id:{0}) or its revision (#{1}) is not found.", reviewId, revisionId));
        }

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
