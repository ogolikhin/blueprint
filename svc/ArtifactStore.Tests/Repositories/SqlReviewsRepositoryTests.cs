﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.Models;
using ArtifactStore.Models.Review;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Repositories
{
    [TestClass]
    public class SqlReviewsRepositoryTests
    {
        private SqlConnectionWrapperMock _cxn;
        private IReviewsRepository _reviewsRepository;

        private Mock<IArtifactVersionsRepository> _artifactVersionsRepositoryMock;
        private Mock<ISqlItemInfoRepository> _itemInfoRepositoryMock;
        private Mock<IArtifactPermissionsRepository> _artifactPermissionsRepositoryMock;
        private Mock<IApplicationSettingsRepository> _applicationSettingsRepositoryMock;

        [TestInitialize]
        public void Initialize()
        {
            _cxn = new SqlConnectionWrapperMock();
            _artifactVersionsRepositoryMock = new Mock<IArtifactVersionsRepository>(MockBehavior.Strict);
            _itemInfoRepositoryMock = new Mock<ISqlItemInfoRepository>(MockBehavior.Strict);
            _artifactPermissionsRepositoryMock = new Mock<IArtifactPermissionsRepository>(MockBehavior.Strict);
            _applicationSettingsRepositoryMock = new Mock<IApplicationSettingsRepository>(MockBehavior.Strict);
            _reviewsRepository = new SqlReviewsRepository(_cxn.Object, 
                    _artifactVersionsRepositoryMock.Object, 
                    _itemInfoRepositoryMock.Object,
                    _artifactPermissionsRepositoryMock.Object,
                    _applicationSettingsRepositoryMock.Object);
        }

        [TestMethod]
        public async Task GetReviewContainerAsync_Formal_Success()
        {
            //Arange
            int reviewId = 1;
            string reviewName = "My Review";
            string reviewDescription = "My Description";
            int userId = 2;
            int baselineId = 3;
            int totalArtifacts = 8;
            int revisionId = 999;
            var reviewStatus = ReviewStatus.Completed;

            _itemInfoRepositoryMock.Setup(i => i.GetItemDescription(reviewId, userId, true, int.MaxValue)).ReturnsAsync(reviewDescription);
            var reviewDetails = new ReviewSummaryDetails
            {
                BaselineId = baselineId,
                ReviewPackageStatus = ReviewPackageStatus.Active,
                ReviewParticipantRole = ReviewParticipantRole.Approver,
                TotalArtifacts = totalArtifacts,
                ReviewStatus = reviewStatus,
                Approved = 5,
                Disapproved = 3,
                RevisionId = revisionId
            };

            var param = new Dictionary<string, object> { { "reviewId", reviewId }, { "userId", userId } };
            _cxn.SetupQueryAsync("GetReviewDetails", param, Enumerable.Repeat(reviewDetails, 1));

            var reviewInfo = new VersionControlArtifactInfo
            {
                Name = reviewName,
                PredefinedType = ItemTypePredefined.ArtifactReviewPackage
            };
            var baselineInfo = new VersionControlArtifactInfo
            {
                Id = baselineId,
                PredefinedType = ItemTypePredefined.ArtifactBaseline
            };

            _artifactVersionsRepositoryMock.Setup(r => r.GetVersionControlArtifactInfoAsync(reviewId, null, userId)).ReturnsAsync(reviewInfo);
            _artifactVersionsRepositoryMock.Setup(r => r.GetVersionControlArtifactInfoAsync(baselineId, null, userId)).ReturnsAsync(baselineInfo);

            //Act
            var review = await _reviewsRepository.GetReviewSummary(reviewId, userId);

            //Assert
            _cxn.Verify();

            Assert.AreEqual(totalArtifacts, review.TotalArtifacts);
            Assert.AreEqual(baselineId, review.Source.Id);
            Assert.AreEqual(reviewStatus, review.Status);
            Assert.AreEqual(reviewName, review.Name);
            Assert.AreEqual(reviewDescription, review.Description);
            Assert.AreEqual(revisionId, review.RevisionId);
            Assert.AreEqual(ReviewType.Formal, review.ReviewType);
            Assert.AreEqual(5, review.ArtifactsStatus.Approved);
            Assert.AreEqual(3, review.ArtifactsStatus.Disapproved);
        }

        [TestMethod]
        public async Task GetReviewContainerAsync_Formal_Throws_ResourceNotFoundException()
        {
            //Arange
            int reviewId = 1;
            int userId = 2;
            var reviewInfo = new VersionControlArtifactInfo
            {
                PredefinedType = ItemTypePredefined.Actor
            };

            _artifactVersionsRepositoryMock.Setup(r => r.GetVersionControlArtifactInfoAsync(reviewId, null, userId)).ReturnsAsync(reviewInfo);

            bool isExceptionThrown = false;
            //Act
            try
            {
                var review = await _reviewsRepository.GetReviewSummary(reviewId, userId);
            }
            catch (ResourceNotFoundException ex)
            {
                isExceptionThrown = true;
                //Assert
                Assert.AreEqual(ErrorCodes.ResourceNotFound, ex.ErrorCode);
                Assert.AreEqual("Review (Id:1) is not found.", ex.Message);
            }
            finally
            {
                if (!isExceptionThrown)
                {
                    Assert.Fail();
                }
            }            
        }

        [TestMethod]
        public async Task GetReviewContainerAsync_Formal_Throws_AuthorizationException()
        {
            //Arange
            int reviewId = 1;
            int userId = 2;
            var reviewInfo = new VersionControlArtifactInfo
            {
                PredefinedType = ItemTypePredefined.ArtifactReviewPackage
            };

            _artifactVersionsRepositoryMock.Setup(r => r.GetVersionControlArtifactInfoAsync(reviewId, null, userId)).ReturnsAsync(reviewInfo);
            var reviewDetails = new ReviewSummaryDetails
            {
                ReviewPackageStatus = ReviewPackageStatus.Active,
                ReviewParticipantRole = null, // User is not assigned to the review
                TotalReviewers = 2
            };

            var param = new Dictionary<string, object> { { "reviewId", reviewId }, { "userId", userId } };
            _cxn.SetupQueryAsync("GetReviewDetails", param, Enumerable.Repeat(reviewDetails, 1));

            bool isExceptionThrown = false;
            //Act
            try
            {
                var review = await _reviewsRepository.GetReviewSummary(reviewId, userId);
            }
            catch (AuthorizationException ex)
            {
                isExceptionThrown = true;
                //Assert
                Assert.AreEqual(ErrorCodes.UnauthorizedAccess, ex.ErrorCode);
                Assert.AreEqual("User does not have permissions to access the review (Id:1).", ex.Message);
            }
            finally
            {
                if (!isExceptionThrown)
                {
                    Assert.Fail();
                }
            }
        }

        [TestMethod]
        public async Task GetReviewedArtifacts_AuthorizationException()
        {
            //Arrange
            int reviewId = 1;
            int userId = 2;
            int revisionId = 999;
            var pagination = new Pagination
            {
                Offset =0,
                Limit = 50
            };
            _itemInfoRepositoryMock.Setup(i => i.GetRevisionId(reviewId, userId, null, null)).ReturnsAsync(revisionId);
            _applicationSettingsRepositoryMock.Setup(s => s.GetValue("ReviewArtifactHierarchyRebuildIntervalInMinutes", 20)).ReturnsAsync(20);
            var param = new Dictionary<string, object> {
                { "reviewId", reviewId },
                { "userId", userId },
                { "addDrafts", false },
                { "revisionId", revisionId },
                { "offset", pagination.Offset },
                { "limit", pagination.Limit },
                { "refreshInterval", 20 }
            };
            var reviewArtifacts = new List<ReviewedArtifact>();
            var artifact1 = new ReviewedArtifact { Id = 1 };
            reviewArtifacts.Add(artifact1);
            var artifact2 = new ReviewedArtifact { Id = 2 };
            reviewArtifacts.Add(artifact2);

            var result = new Tuple<IEnumerable<ReviewedArtifact>, IEnumerable<int>>(reviewArtifacts, new[] { 2 });
            _cxn.SetupQueryMultipleAsync("GetReviewArtifacts", param, result);

            _artifactPermissionsRepositoryMock
                .Setup(p => p.GetArtifactPermissions(It.IsAny<IEnumerable<int>>(), userId, false, int.MaxValue, true))
                .ReturnsAsync(new Dictionary<int, RolePermissions>());

            //Act
            bool isExceptionThrown = false;
            try
            {
                var review = await _reviewsRepository.GetReviewedArtifacts(reviewId, userId, pagination, revisionId);
            }
            catch (AuthorizationException ex)
            {
                isExceptionThrown = true;
                //Assert
                Assert.AreEqual(ErrorCodes.UnauthorizedAccess, ex.ErrorCode);
                Assert.AreEqual("User does not have permissions to access the review (Id:1).", ex.Message);
            }
            finally
            {
                if (!isExceptionThrown)
                {
                    Assert.Fail();
                }
            }
        }

        [TestMethod]
        public async Task GetReviewedArtifacts_Success()
        {
            //Arrange
            int reviewId = 1;
            int userId = 2;
            int revisionId = 999;
            var pagination = new Pagination
            {
                Offset = 0,
                Limit = 50
            };
            _itemInfoRepositoryMock.Setup(i => i.GetRevisionId(reviewId, userId, null, null)).ReturnsAsync(revisionId);
            _applicationSettingsRepositoryMock.Setup(s => s.GetValue("ReviewArtifactHierarchyRebuildIntervalInMinutes", 20)).ReturnsAsync(20);
            var param = new Dictionary<string, object> {
                { "reviewId", reviewId },
                { "userId", userId },
                { "addDrafts", false },
                { "revisionId", revisionId },
                { "offset", pagination.Offset },
                { "limit", pagination.Limit },
                { "refreshInterval", 20 }
            };
            var reviewArtifacts = new List<ReviewedArtifact>();
            var artifact1 = new ReviewedArtifact { Id = 2 };
            reviewArtifacts.Add(artifact1);
            var artifact2 = new ReviewedArtifact { Id = 3 };
            reviewArtifacts.Add(artifact2);

            var result = new Tuple<IEnumerable<ReviewedArtifact>, IEnumerable<int>>(reviewArtifacts, new[] { 2 });
            _cxn.SetupQueryMultipleAsync("GetReviewArtifacts", param, result);

            var reviewArtifacts2 = new List<ReviewedArtifact>();
            var reviewArtifact1 = new ReviewedArtifact { Id = 2 };
            reviewArtifacts2.Add(artifact1);
            var reviewArtifact2 = new ReviewedArtifact { Id = 3 };
            reviewArtifacts2.Add(artifact2);

            var param2 = new Dictionary<string, object> {
                { "reviewId", reviewId },
                { "userId", userId },
                { "revisionId", revisionId },
                { "itemIds", SqlConnectionWrapper.ToDataTable(new [] { 2, 3 }) },
            };
            _cxn.SetupQueryAsync("GetReviewArtifactsByParticipant", param2, reviewArtifacts2);

            var permisions = new Dictionary<int, RolePermissions>
            {
                { 1, RolePermissions.Read }
            };
            _artifactPermissionsRepositoryMock
                .Setup(p => p.GetArtifactPermissions(It.IsAny<IEnumerable<int>>(), userId, false, int.MaxValue, true))
                .ReturnsAsync(permisions);

            //Act
            var artifacts = await _reviewsRepository.GetReviewedArtifacts(reviewId, userId, pagination, revisionId);

            Assert.AreEqual(2, artifacts.Total);

            Assert.AreEqual(2, artifacts.Items.ElementAt(0).Id);
            Assert.AreEqual(3, artifacts.Items.ElementAt(1).Id);
        }
    }
}
