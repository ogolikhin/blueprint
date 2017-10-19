using System;
using System.Threading.Tasks;
using ArtifactStore.Models.Review;
using ArtifactStore.Repositories;
using ArtifactStore.Services.Reviews;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Services
{
    [TestClass]
    public class ReviewServiceTests
    {
        private const int ReviewId = 1;
        private const int UserId = 1;

        private IReviewsService _reviewService;
        private Mock<IReviewsRepository> _mockReviewRepository;
        private Mock<IArtifactRepository> _mockArtifactRepository;
        private Mock<IArtifactPermissionsRepository> _mockArtifactPermissionsRepository;

        private ReviewSettings _reviewSettings;
        private ArtifactBasicDetails _artifactDetails;

        [TestInitialize]
        public void Initialize()
        {
            _reviewSettings = new ReviewSettings();

            _artifactDetails = new ArtifactBasicDetails
            {
                ItemId = ReviewId,
                PrimitiveItemTypePredefined = (int)ItemTypePredefined.ArtifactReviewPackage
            };

            _mockReviewRepository = new Mock<IReviewsRepository>();
            _mockReviewRepository
                .Setup(m => m.GetReviewSettingsAsync(ReviewId, UserId, It.IsAny<int>()))
                .ReturnsAsync(_reviewSettings);
            _mockArtifactRepository = new Mock<IArtifactRepository>();
            _mockArtifactRepository
                .Setup(m => m.GetArtifactBasicDetails(ReviewId, UserId))
                .ReturnsAsync(_artifactDetails);
            _mockArtifactPermissionsRepository = new Mock<IArtifactPermissionsRepository>();
            _mockArtifactPermissionsRepository
                .Setup(m => m.HasReadPermissions(ReviewId, UserId, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _reviewService = new ReviewsService(_mockReviewRepository.Object, _mockArtifactRepository.Object, _mockArtifactPermissionsRepository.Object);
        }

        #region GetReviewSettingsAsync

        [TestMethod]
        public async Task GetReviewSettingsAsync_ReviewIdInvalid_ThrowsArgumentOutOfRangeException()
        {
            // Act
            try
            {
                await _reviewService.GetReviewSettingsAsync(-1, UserId);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                // Assert
                Assert.AreEqual("reviewId", ex.ParamName);
                return;
            }

            Assert.Fail("Expected ArgumentOutOfRangeException to have been thrown.");
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_UserIdInvalid_ThrowsArgumentOutOfRangeException()
        {
            // Act
            try
            {
                await _reviewService.GetReviewSettingsAsync(ReviewId, -1);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                // Assert
                Assert.AreEqual("userId", ex.ParamName);
                return;
            }

            Assert.Fail("Expected ArgumentOutOfRangeException to have been thrown.");
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_ReviewNotFound_ThrowsResourceNotFoundException()
        {
            // Arrange
            _mockArtifactRepository
                .Setup(m => m.GetArtifactBasicDetails(It.IsIn(ReviewId), It.IsIn(UserId)))
                .ReturnsAsync((ArtifactBasicDetails) null);

            // Act
            try
            {
                await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);
            }
            catch (ResourceNotFoundException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.ResourceNotFound, ex.ErrorCode);
                return;
            }

            Assert.Fail("Expected ResourceNotFoundException to have been thrown.");
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_IdIsNotForReview_ThrowsBadRequestException()
        {
            // Arrange
            _artifactDetails.PrimitiveItemTypePredefined = (int) ItemTypePredefined.TextualRequirement;

            // Act
            try
            {
                await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);
            }
            catch (BadRequestException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.InvalidParameter, ex.ErrorCode);
                return;
            }

            Assert.Fail("Expected BadRequestException to have been thrown.");
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_ReviewNotAccessibleForUser_ThrowsAuthorizationException()
        {
            // Arrange
            _mockArtifactPermissionsRepository
                .Setup(m => m.HasReadPermissions(ReviewId, UserId, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(false);

            // Act
            try
            {
                await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);
            }
            catch (AuthorizationException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.Forbidden, ex.ErrorCode);
                return;
            }

            Assert.Fail("Expected AuthorizationException to have been thrown.");
        }

        #endregion GetReviewSettingsAsync
    }
}
