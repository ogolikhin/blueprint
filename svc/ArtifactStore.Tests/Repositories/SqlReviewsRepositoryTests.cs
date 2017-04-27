using System.Collections.Generic;
using System.Threading.Tasks;
using ArtifactStore.Models.Review;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Repositories;
using System;
using System.Linq;
using ArtifactStore.Models;
using ServiceLibrary.Models;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace ArtifactStore.Repositories
{
    [TestClass]
    public class SqlReviewsRepositoryTests
    {
        private SqlConnectionWrapperMock _cxn;
        private IReviewsRepository _reviewsRepository;

        private Mock<IArtifactVersionsRepository> _artifactVersionsRepositoryMock;
        private Mock<ISqlItemInfoRepository> _itemInfoRepositoryMock;

        [TestInitialize]
        public void Initialize()
        {
            _cxn = new SqlConnectionWrapperMock();
            _artifactVersionsRepositoryMock = new Mock<IArtifactVersionsRepository>(MockBehavior.Strict);
            _itemInfoRepositoryMock = new Mock<ISqlItemInfoRepository>(MockBehavior.Strict);
            _reviewsRepository = new SqlReviewsRepository(_cxn.Object, _artifactVersionsRepositoryMock.Object, _itemInfoRepositoryMock.Object);
        }

        [TestMethod]
        public async Task GetReviewerSuccess()
        {
            //Arange
            int reviewId = 1;
            int userId = 2;
            Reviewer[] result = { new Reviewer { UserId = userId, Role = ReviwerRole.Reviewer } };
            _cxn.SetupQueryAsync("GetReviewer", new Dictionary<string, object> { { "reviewId", reviewId }, { "userId", userId } }, result);

            //Act
            var reviewer = await _reviewsRepository.GetReviewer(reviewId, userId);

            //Assert
            _cxn.Verify();

            Assert.AreEqual(userId, reviewer.UserId);
            Assert.AreEqual(ReviwerRole.Reviewer, reviewer.Role);
        }

        [TestMethod]
        public async Task GetReviewContainerAsync_Formal_Success()
        {
            //Arange
            int reviewId = 1;
            string reviewName = "My Review";
            int userId = 2;
            int revisionId = int.MaxValue;
            int baselineId = 3;
            int totalArtifacts = 8;
            var reviewStatus = ReviewStatus.Completed;
            var artifactsStatus = new ReviewArtifactsStatus {
                Approved = 5,
                Disapproved = 3
            };

            var result = Tuple.Create(Enumerable.Repeat((int?)baselineId, 1), Enumerable.Repeat(totalArtifacts, 1), Enumerable.Repeat(reviewStatus, 1), Enumerable.Repeat(artifactsStatus, 1));
            var param = new Dictionary<string, object> { { "reviewId", reviewId }, { "userId", userId }, { "revisionId", revisionId } };
            _cxn.SetupQueryMultipleAsync("GetReviewDetails", param, result);

            var param2 = new Dictionary<string, object> { { "reviewId", reviewId }, { "userId", userId } };
            var reviewer = new Reviewer
            {
                Role = ReviwerRole.Approver
            };
            _cxn.SetupQueryAsync("GetReviewer", param2, Enumerable.Repeat(reviewer, 1));

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
            var review = await _reviewsRepository.GetReviewContainerAsync(reviewId, userId);

            //Assert
            _cxn.Verify();

            Assert.AreEqual(totalArtifacts, review.TotalArtifacts);
            Assert.AreEqual(baselineId, review.Source.Id);
            Assert.AreEqual(reviewStatus, review.Status);
            Assert.AreEqual(reviewName, review.Name);
            Assert.AreEqual(ReviewSourceType.Baseline, review.SourceType);
            Assert.AreEqual(ReviewType.Formal, review.ReviewType);
            Assert.AreEqual(5, artifactsStatus.Approved);
            Assert.AreEqual(3, artifactsStatus.Disapproved);
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
                var review = await _reviewsRepository.GetReviewContainerAsync(reviewId, userId);
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
            var param = new Dictionary<string, object> { { "reviewId", reviewId }, { "userId", userId } };
            _cxn.SetupQueryAsync("GetReviewer", param, Enumerable.Repeat((Reviewer)null, 1));

            bool isExceptionThrown = false;
            //Act
            try
            {
                var review = await _reviewsRepository.GetReviewContainerAsync(reviewId, userId);
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

    }
}
