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

        private ReviewPackageRawData _reviewPackageRawData;
        private ArtifactBasicDetails _artifactDetails;

        [TestInitialize]
        public void Initialize()
        {
            _reviewPackageRawData = new ReviewPackageRawData();

            _artifactDetails = new ArtifactBasicDetails
            {
                ItemId = ReviewId,
                PrimitiveItemTypePredefined = (int)ItemTypePredefined.ArtifactReviewPackage
            };

            _mockReviewRepository = new Mock<IReviewsRepository>();
            _mockReviewRepository
                .Setup(m => m.GetReviewPackageRawDataAsync(ReviewId, UserId, It.IsAny<int>()))
                .ReturnsAsync(_reviewPackageRawData);
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
                .ReturnsAsync((ArtifactBasicDetails)null);

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
            _artifactDetails.PrimitiveItemTypePredefined = (int)ItemTypePredefined.TextualRequirement;

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

        [TestMethod]
        public async Task GetReviewSettingsAsync_ReviewPackageRawDataShowOnlyDescriptionIsFalse_ReviewSettingsShowOnlyDescriptionIsFalse()
        {
            // Arrange
            _reviewPackageRawData.ShowOnlyDescription = false;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(_reviewPackageRawData.ShowOnlyDescription, reviewSettings.ShowOnlyDescription);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_ReviewPackageRawDataShowOnlyDescriptionIsTrue_ReviewSettingsShowOnlyDescriptionIsTrue()
        {
            // Arrange
            _reviewPackageRawData.ShowOnlyDescription = true;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(_reviewPackageRawData.ShowOnlyDescription, reviewSettings.ShowOnlyDescription);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_ReviewPackageRawDataIsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewedIsFalse_ReviewSettingsCanMarkAsCompleteIsFalse()
        {
            // Arrange
            _reviewPackageRawData.IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed = false;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(_reviewPackageRawData.IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed, reviewSettings.CanMarkAsComplete);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_ReviewPackageRawDataIsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewedIsTrue_ReviewSettingsCanMarkAsCompleteIsTrue()
        {
            // Arrange
            _reviewPackageRawData.IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed = true;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(_reviewPackageRawData.IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed, reviewSettings.CanMarkAsComplete);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_ReviewPackageRawDataIsESignatureEnabledIsFalse_ReviewSettingsRequireESignatureIsFalse()
        {
            // Arrange
            _reviewPackageRawData.IsESignatureEnabled = false;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(_reviewPackageRawData.IsESignatureEnabled, reviewSettings.RequireESignature);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_ReviewPackageRawDataIsESignatureEnabledIsTrue_ReviewSettingsRequireESignatureIsTrue()
        {
            // Arrange
            _reviewPackageRawData.IsESignatureEnabled = true;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(_reviewPackageRawData.IsESignatureEnabled, reviewSettings.RequireESignature);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_ReviewPackageRawDataIsMoSEnabledIsFalse_ReviewSettingsRequireMeaningOfSignatureIsFalse()
        {
            // Arrange
            _reviewPackageRawData.IsMoSEnabled = false;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(_reviewPackageRawData.IsMoSEnabled, reviewSettings.RequireMeaningOfSignature);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_ReviewPackageRawDataIsMoSEnabledIsTrue_ReviewSettingsRequireMeaningOfSignatureIsTrue()
        {
            // Arrange
            _reviewPackageRawData.IsMoSEnabled = true;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(_reviewPackageRawData.IsMoSEnabled, reviewSettings.RequireMeaningOfSignature);
        }

        #endregion GetReviewSettingsAsync
    }
}
