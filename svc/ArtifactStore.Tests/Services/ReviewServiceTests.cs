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
using System.Collections.Generic;
using System.Linq;

namespace ArtifactStore.Services
{
    [TestClass]
    public class ReviewServiceTests
    {
        private const int ReviewId = 1;
        private const int UserId = 2;

        private IReviewsService _reviewService;
        private Mock<IReviewsRepository> _mockReviewRepository;
        private Mock<IArtifactRepository> _mockArtifactRepository;
        private Mock<IArtifactPermissionsRepository> _mockArtifactPermissionsRepository;
        private Mock<ILockArtifactsRepository> _mockLockArtifactsRepository;

        private ReviewPackageRawData _reviewPackageRawData;
        private ArtifactBasicDetails _artifactDetails;

        private bool _hasReadPermissions;
        private Dictionary<int, List<ParticipantMeaningOfSignatureResult>> _possibleMeaningOfSignatures;

        [TestInitialize]
        public void Initialize()
        {
            _reviewPackageRawData = new ReviewPackageRawData();

            _artifactDetails = new ArtifactBasicDetails
            {
                ItemId = ReviewId,
                ProjectId = 1,
                PrimitiveItemTypePredefined = (int)ItemTypePredefined.ArtifactReviewPackage
            };

            _mockReviewRepository = new Mock<IReviewsRepository>();

            _mockReviewRepository
                .Setup(m => m.GetReviewPackageRawDataAsync(ReviewId, UserId, It.IsAny<int>()))
                .ReturnsAsync(_reviewPackageRawData);

            _mockReviewRepository
                .Setup(m => m.GetPossibleMeaningOfSignaturesForParticipantsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(() => _possibleMeaningOfSignatures);

            _mockArtifactRepository = new Mock<IArtifactRepository>();
            _mockArtifactRepository
                .Setup(m => m.GetArtifactBasicDetails(ReviewId, UserId))
                .ReturnsAsync(_artifactDetails);

            _mockArtifactPermissionsRepository = new Mock<IArtifactPermissionsRepository>();

            _mockArtifactPermissionsRepository
                .Setup(m => m.HasReadPermissions(ReviewId, UserId, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(() => _hasReadPermissions);

            _mockLockArtifactsRepository = new Mock<ILockArtifactsRepository>();

            _mockLockArtifactsRepository
                .Setup(m => m.LockArtifactAsync(ReviewId, UserId))
                .ReturnsAsync(true);

            _hasReadPermissions = true;

            _reviewService = new ReviewsService(
                _mockReviewRepository.Object,
                _mockArtifactRepository.Object,
                _mockArtifactPermissionsRepository.Object,
                _mockLockArtifactsRepository.Object);
        }

        #region GetReviewSettingsAsync

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
                Assert.AreEqual(ErrorCodes.BadRequest, ex.ErrorCode);
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

        #region UpdateReviewSettingsAsync

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_ReviewDoesNotExist_ThrowsResourceNotFoundException()
        {
            // Arrange
            _mockArtifactRepository
                .Setup(m => m.GetArtifactBasicDetails(ReviewId, UserId))
                .ReturnsAsync((ArtifactBasicDetails)null);

            // Act
            try
            {
                await _reviewService.UpdateReviewSettingsAsync(ReviewId, new ReviewSettings(), UserId);
            }
            catch (ResourceNotFoundException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.ResourceNotFound, ex.ErrorCode);
                Assert.AreEqual(I18NHelper.FormatInvariant(ErrorMessages.ReviewNotFound, ReviewId), ex.Message);
                return;
            }

            Assert.Fail("Expected ResourceNotFoundException to have been thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_IdIsNotForReview_ThrowsBadRequestException()
        {
            // Arrange
            _artifactDetails.PrimitiveItemTypePredefined = (int)ItemTypePredefined.ArtifactBaseline;

            // Act
            try
            {
                await _reviewService.UpdateReviewSettingsAsync(ReviewId, new ReviewSettings(), UserId);
            }
            catch (BadRequestException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.BadRequest, ex.ErrorCode);
                Assert.AreEqual(I18NHelper.FormatInvariant(ErrorMessages.ArtifactIsNotReview, ReviewId), ex.Message);
                return;
            }

            Assert.Fail("Expected BadRequestException to have been thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_ReviewNotAccessibleForUser_ThrowsAuthorizationException()
        {
            // Arrange
            _hasReadPermissions = false;

            // Act
            try
            {
                await _reviewService.UpdateReviewSettingsAsync(ReviewId, new ReviewSettings(), UserId);
            }
            catch (AuthorizationException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.Forbidden, ex.ErrorCode);
                Assert.AreEqual(I18NHelper.FormatInvariant(ErrorMessages.CannotAccessReview, ReviewId), ex.Message);
                return;
            }

            Assert.Fail("Expected AuthorizationException to have been thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_ReviewIsClosed_ThrowsConflictException()
        {
            // Arrange
            _reviewPackageRawData.Status = ReviewPackageStatus.Closed;

            // Act
            try
            {
                await _reviewService.UpdateReviewSettingsAsync(ReviewId, new ReviewSettings(), UserId);
            }
            catch (ConflictException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.ReviewClosed, ex.ErrorCode);
                Assert.AreEqual(I18NHelper.FormatInvariant(ErrorMessages.ReviewIsClosed, ReviewId), ex.Message);
                return;
            }

            Assert.Fail("Expected ConflictException to have been thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_ReviewLockedByAnotherUser_ThrowsConflictException()
        {
            // Arrange
            _artifactDetails.LockedByUserId = 100;

            // Act
            try
            {
                await _reviewService.UpdateReviewSettingsAsync(ReviewId, new ReviewSettings(), UserId);
            }
            catch (ConflictException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.LockedByOtherUser, ex.ErrorCode);
                Assert.AreEqual(I18NHelper.FormatInvariant(ErrorMessages.ArtifactNotLockedByUser, ReviewId, UserId), ex.Message);
                return;
            }

            Assert.Fail("Expected ConflictException to have been thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_ReviewLockedByAnotherUserWhenLocking_ThrowsConflictException()
        {
            // Arrange
            _mockLockArtifactsRepository
                .Setup(m => m.LockArtifactAsync(ReviewId, UserId))
                .ReturnsAsync(false);

            // Act
            try
            {
                await _reviewService.UpdateReviewSettingsAsync(ReviewId, new ReviewSettings(), UserId);
            }
            catch (ConflictException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.LockedByOtherUser, ex.ErrorCode);
                Assert.AreEqual(I18NHelper.FormatInvariant(ErrorMessages.ArtifactNotLockedByUser, ReviewId, UserId), ex.Message);
                return;
            }

            Assert.Fail("Expected ConflictException to have been thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_ReviewIsNotLocked_LocksReview()
        {
            // Arrange
            _artifactDetails.LockedByUserId = null;

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, new ReviewSettings(), UserId);

            // Assert
            _mockLockArtifactsRepository.Verify(m => m.LockArtifactAsync(ReviewId, UserId));
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_EndDateChanged_ReviewIsDraft_UpdatesSetting()
        {
            // Arrange
            var updatedEndDate = new DateTime(2020, 11, 15);
            var updatedReviewSettings = new ReviewSettings { EndDate = updatedEndDate };
            _reviewPackageRawData.EndDate = null;
            _reviewPackageRawData.Status = ReviewPackageStatus.Draft;

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, UserId);

            // Assert
            Assert.AreEqual(updatedEndDate, _reviewPackageRawData.EndDate);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_EndDateChanged_ReviewIsActive_UpdatesSetting()
        {
            // Arrange
            var updatedEndDate = new DateTime(2019, 10, 11, 5, 24, 0);
            var updatedReviewSettings = new ReviewSettings { EndDate = updatedEndDate };
            _reviewPackageRawData.EndDate = null;
            _reviewPackageRawData.Status = ReviewPackageStatus.Active;

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, UserId);

            // Assert
            Assert.AreEqual(updatedEndDate, _reviewPackageRawData.EndDate);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_EndDateNotChanged_DoesNotUpdateSetting()
        {
            // Arrange
            var updatedEndDate = new DateTime(2017, 12, 24, 7, 0, 0);
            var updatedReviewSettings = new ReviewSettings { EndDate = updatedEndDate };
            _reviewPackageRawData.EndDate = updatedEndDate;
            _reviewPackageRawData.Status = ReviewPackageStatus.Active;

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, UserId);

            // Assert
            Assert.AreEqual(_reviewPackageRawData.EndDate, updatedReviewSettings.EndDate);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_ShowOnlyDescriptionChanged_ReviewIsDraft_UpdatesSetting()
        {
            // Arrange
            var updatedReviewSettings = new ReviewSettings { ShowOnlyDescription = true };
            _reviewPackageRawData.ShowOnlyDescription = false;
            _reviewPackageRawData.Status = ReviewPackageStatus.Draft;

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, UserId);

            // Assert
            Assert.AreEqual(true, _reviewPackageRawData.ShowOnlyDescription);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_ShowOnlyDescriptionChanged_ReviewIsActive_UpdatesSetting()
        {
            // Arrange
            var updatedReviewSettings = new ReviewSettings { ShowOnlyDescription = true };
            _reviewPackageRawData.ShowOnlyDescription = false;
            _reviewPackageRawData.Status = ReviewPackageStatus.Active;

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, UserId);

            // Assert
            Assert.AreEqual(true, _reviewPackageRawData.ShowOnlyDescription);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_ShowOnlyDescriptionNotChanged_DoesNotUpdateSetting()
        {
            // Arrange
            var updatedReviewSettings = new ReviewSettings { ShowOnlyDescription = true };
            _reviewPackageRawData.ShowOnlyDescription = true;
            _reviewPackageRawData.Status = ReviewPackageStatus.Active;

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, UserId);

            // Assert
            Assert.AreEqual(_reviewPackageRawData.ShowOnlyDescription, updatedReviewSettings.ShowOnlyDescription);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_CanMarkAsCompleteChanged_ReviewIsDraft_UpdatesSetting()
        {
            // Arrange
            _reviewPackageRawData.IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed = false;
            _reviewPackageRawData.Status = ReviewPackageStatus.Draft;
            var updatedReviewSettings = new ReviewSettings { CanMarkAsComplete = true };

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, UserId);

            // Assert
            Assert.AreEqual(true, _reviewPackageRawData.IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_CanMarkAsCompleteNotChanged_ReviewIsActive_DoesNotUpdateSetting()
        {
            // Arrange
            _reviewPackageRawData.IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed = true;
            _reviewPackageRawData.Status = ReviewPackageStatus.Active;
            var updatedReviewSettings = new ReviewSettings { CanMarkAsComplete = true };

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, UserId);

            // Assert
            Assert.AreEqual(_reviewPackageRawData.IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed, updatedReviewSettings.CanMarkAsComplete);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_CanMarkAsCompleteChanged_ReviewIsActive_ThrowsConflictException()
        {
            // Arrange
            _reviewPackageRawData.IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed = false;
            _reviewPackageRawData.Status = ReviewPackageStatus.Active;
            var updatedReviewSettings = new ReviewSettings { CanMarkAsComplete = true };

            // Act
            try
            {
                await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, UserId);
            }
            catch (ConflictException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.Conflict, ex.ErrorCode);
                Assert.AreEqual(I18NHelper.FormatInvariant(ErrorMessages.ReviewIsNotDraft, ReviewId, UserId), ex.Message);
                Assert.AreEqual(false, _reviewPackageRawData.IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed);
                return;
            }

            Assert.Fail("Expected ConflictException to have been thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_RequireESignatureChanged_ReviewIsDraft_UpdatesSetting()
        {
            // Arrange
            _reviewPackageRawData.IsESignatureEnabled = false;
            _reviewPackageRawData.Status = ReviewPackageStatus.Draft;
            var updatedReviewSettings = new ReviewSettings { RequireESignature = true };

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, UserId);

            // Assert
            Assert.AreEqual(true, _reviewPackageRawData.IsESignatureEnabled);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_RequireESignatureChanged_ReviewIsActive_UpdatesSetting()
        {
            // Arrange
            _reviewPackageRawData.IsESignatureEnabled = false;
            _reviewPackageRawData.Status = ReviewPackageStatus.Active;
            var updatedReviewSettings = new ReviewSettings { RequireESignature = true };

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, UserId);

            // Assert
            Assert.AreEqual(true, _reviewPackageRawData.IsESignatureEnabled);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_RequireESignatureNotChanged_DoesNotUpdateSetting()
        {
            // Arrange
            _reviewPackageRawData.IsESignatureEnabled = false;
            _reviewPackageRawData.Status = ReviewPackageStatus.Active;
            var updatedReviewSettings = new ReviewSettings { RequireESignature = false };

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, UserId);

            // Assert
            Assert.AreEqual(false, _reviewPackageRawData.IsESignatureEnabled);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_RequireMeaningOfSignatureNotChanged_ReviewIsActive_DoesNotUpdateSetting()
        {
            // Arrange
            _reviewPackageRawData.IsMoSEnabled = false;
            _reviewPackageRawData.Status = ReviewPackageStatus.Active;
            var updatedReviewSettings = new ReviewSettings { RequireMeaningOfSignature = false };

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, UserId);

            // Assert
            Assert.AreEqual(false, _reviewPackageRawData.IsMoSEnabled);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_RequireMeaningOfSignatureChanged_ReviewIsActive_ThrowsConflictException()
        {
            // Arrange
            _reviewPackageRawData.IsMoSEnabled = false;
            _reviewPackageRawData.Status = ReviewPackageStatus.Active;
            var updatedReviewSettings = new ReviewSettings { RequireMeaningOfSignature = true };

            // Act
            try
            {
                await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, UserId);
            }
            catch (ConflictException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.Conflict, ex.ErrorCode);
                Assert.AreEqual(I18NHelper.FormatInvariant(ErrorMessages.ReviewIsNotDraft, ReviewId), ex.Message);
                return;
            }

            // Assert
            Assert.Fail("Expected ConflictException to have been thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_RequireMeaningOfSignatureChanged_ESignatureNotEnabled_ThrowsConflictException()
        {
            // Arrange
            _reviewPackageRawData.IsMoSEnabled = false;
            _reviewPackageRawData.IsESignatureEnabled = false;
            _reviewPackageRawData.Status = ReviewPackageStatus.Draft;
            var updatedReviewSettings = new ReviewSettings { RequireMeaningOfSignature = true };

            // Act
            try
            {
                await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, UserId);
            }
            catch (ConflictException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.Conflict, ex.ErrorCode);
                Assert.AreEqual(I18NHelper.FormatInvariant(ErrorMessages.RequireESignatureDisabled, ReviewId), ex.Message);
                return;
            }

            // Assert
            Assert.Fail("Expected ConflictException to have been thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_RequireMeaningOfSignatureChanged_MeaningOfSignatureDisabledInProject_ThrowsConflictException()
        {
            // Arrange
            _reviewPackageRawData.IsMoSEnabled = false;
            _reviewPackageRawData.IsESignatureEnabled = true;
            var updatedReviewSettings = new ReviewSettings { RequireMeaningOfSignature = true };

            _mockArtifactPermissionsRepository
                .Setup(m => m.GetProjectPermissions(_artifactDetails.ProjectId))
                .ReturnsAsync(ProjectPermissions.None);

            // Act
            try
            {
                await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, UserId);
            }
            catch (ConflictException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.Conflict, ex.ErrorCode);
                Assert.AreEqual(I18NHelper.FormatInvariant(ErrorMessages.RequireESignatureDisabled, ReviewId), ex.Message);
                return;
            }

            // Assert
            Assert.Fail("Expected ConflictException to have been thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_RequireMeaningOfSignatureChanged_AllConditionsSatisfied_UpdatesSetting()
        {
            // Arrange
            _reviewPackageRawData.IsMoSEnabled = false;
            _reviewPackageRawData.IsESignatureEnabled = true;
            var updatedReviewSettings = new ReviewSettings
            {
                RequireESignature = true,
                RequireMeaningOfSignature = true
            };

            _mockArtifactPermissionsRepository
                .Setup(m => m.GetProjectPermissions(_artifactDetails.ProjectId))
                .ReturnsAsync(ProjectPermissions.IsMeaningOfSignatureEnabled);

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, UserId);

            // Assert
            Assert.AreEqual(true, _reviewPackageRawData.IsMoSEnabled);
        }

        #endregion UpdateReviewSettingsAsync

        #region UpdateMeaningOfSignaturesAsync

        [TestMethod]
        public async Task UpdateMeaningOfSignaturesAsync_Should_Throw_When_User_Does_Not_Have_Read_Permissions_For_Review()
        {
            // Arrange
            _hasReadPermissions = false;

            // Act
            try
            {
                await _reviewService.UpdateMeaningOfSignaturesAsync(ReviewId, UserId, new MeaningOfSignatureParameter[0]);
            }
            catch (AuthorizationException ex)
            {
                Assert.AreEqual(ErrorCodes.UnauthorizedAccess, ex.ErrorCode);

                return;
            }

            Assert.Fail("An AuthenticationException was not thrown");
        }

        [TestMethod]
        public async Task UpdateMeaningOfSignaturesAsync_Should_Throw_When_Review_Is_Closed()
        {
            // Arrange
            _reviewPackageRawData.Status = ReviewPackageStatus.Closed;

            // Act
            try
            {
                await _reviewService.UpdateMeaningOfSignaturesAsync(ReviewId, UserId, new MeaningOfSignatureParameter[0]);
            }
            catch (ConflictException ex)
            {
                Assert.AreEqual(ErrorCodes.ReviewClosed, ex.ErrorCode);

                return;
            }

            Assert.Fail("A ConflictException was not thrown");
        }

        [TestMethod]
        public async Task UpdateMeaningOfSignaturesAsync_Should_Throw_When_Meaning_Of_Signature_Is_Not_Enabled()
        {
            // Arrange
            _reviewPackageRawData.IsMoSEnabled = false;

            // Act
            try
            {
                await _reviewService.UpdateMeaningOfSignaturesAsync(ReviewId, UserId, new MeaningOfSignatureParameter[0]);
            }
            catch (ConflictException ex)
            {
                Assert.AreEqual(ErrorCodes.MeaningOfSignatureNotEnabled, ex.ErrorCode);

                return;
            }

            Assert.Fail("A ConflictException was not thrown");
        }

        [TestMethod]
        public async Task UpdateMeaningOfSignaturesAsync_Should_Throw_When_Participant_Is_Not_In_Review()
        {
            // Arrange
            _reviewPackageRawData.IsMoSEnabled = true;
            _reviewPackageRawData.Reviewers = new List<ReviewerRawData>();

            // Act
            try
            {
                await _reviewService.UpdateMeaningOfSignaturesAsync(ReviewId, UserId, new[] {
                    new MeaningOfSignatureParameter() {
                    Adding = true,
                    MeaningOfSignatureId = 3,
                    ParticipantId = 4
                    }
                });
            }
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ErrorCodes.UserNotInReview, ex.ErrorCode);

                return;
            }

            Assert.Fail("A BadRequestException was not thrown");
        }

        [TestMethod]
        public async Task UpdateMeaningOfSignaturesAsync_Should_Throw_When_Participant_Is_Not_An_Approver()
        {
            // Arrange
            _reviewPackageRawData.IsMoSEnabled = true;
            _reviewPackageRawData.Reviewers = new List<ReviewerRawData>()
            {
                new ReviewerRawData()
                {
                    UserId = 4,
                    Permission = ReviewParticipantRole.Reviewer
                }
            };

            // Act
            try
            {
                await _reviewService.UpdateMeaningOfSignaturesAsync(ReviewId, UserId, new[] {
                    new MeaningOfSignatureParameter() {
                    Adding = true,
                    MeaningOfSignatureId = 3,
                    ParticipantId = 4
                    }
                });
            }
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ErrorCodes.ParticipantIsNotAnApprover, ex.ErrorCode);

                return;
            }

            Assert.Fail("A BadRequestException was not thrown");
        }

        [TestMethod]
        public async Task UpdateMeaningOfSignaturesAsync_Should_Throw_When_There_Are_No_Possible_Meaning_Of_Signatures_For_A_Participant()
        {
            // Arrange
            _reviewPackageRawData.IsMoSEnabled = true;
            _reviewPackageRawData.Reviewers = new List<ReviewerRawData>()
            {
                new ReviewerRawData()
                {
                    UserId = 4,
                    Permission = ReviewParticipantRole.Approver
                }
            };

            _possibleMeaningOfSignatures = new Dictionary<int, List<ParticipantMeaningOfSignatureResult>>();

            // Act
            try
            {
                await _reviewService.UpdateMeaningOfSignaturesAsync(ReviewId, UserId, new[] {
                    new MeaningOfSignatureParameter() {
                    Adding = true,
                    MeaningOfSignatureId = 3,
                    ParticipantId = 4
                    }
                });
            }
            catch (ConflictException ex)
            {
                Assert.AreEqual(ErrorCodes.MeaningOfSignatureNotPossible, ex.ErrorCode);

                return;
            }

            Assert.Fail("A ConflictException was not thrown");
        }

        [TestMethod]
        public async Task UpdateMeaningOfSignaturesAsync_Should_Throw_When_There_Are_No_Matching_Meaning_Of_Signatures_For_A_Participant()
        {
            // Arrange
            _reviewPackageRawData.IsMoSEnabled = true;
            _reviewPackageRawData.Reviewers = new List<ReviewerRawData>()
            {
                new ReviewerRawData()
                {
                    UserId = 4,
                    Permission = ReviewParticipantRole.Approver
                }
            };

            _possibleMeaningOfSignatures = new Dictionary<int, List<ParticipantMeaningOfSignatureResult>>()
            {
                { 4, new List<ParticipantMeaningOfSignatureResult>() }
            };

            // Act
            try
            {
                await _reviewService.UpdateMeaningOfSignaturesAsync(ReviewId, UserId, new[] {
                    new MeaningOfSignatureParameter() {
                    Adding = true,
                    MeaningOfSignatureId = 3,
                    ParticipantId = 4
                    }
                });
            }
            catch (ConflictException ex)
            {
                Assert.AreEqual(ErrorCodes.MeaningOfSignatureNotPossible, ex.ErrorCode);

                return;
            }

            Assert.Fail("A ConflictException was not thrown");
        }

        [TestMethod]
        public async Task UpdateMeaningOfSignaturesAsync_Should_Add_New_Meaning_Of_Signature_To_Participant_When_SelectedMoS_Is_Null()
        {
            // Arrange
            _reviewPackageRawData.IsMoSEnabled = true;
            _reviewPackageRawData.Reviewers = new List<ReviewerRawData>()
            {
                new ReviewerRawData()
                {
                    UserId = 4,
                    Permission = ReviewParticipantRole.Approver
                }
            };

            var meaningOfSignature = new ParticipantMeaningOfSignatureResult()
            {
                GroupId = 6,
                MeaningOfSignatureId = 3,
                MeaningOfSignatureValue = "foo",
                ParticipantId = 4,
                RoleAssignmentId = 7,
                RoleId = 8,
                RoleName = "bar"
            };

            _possibleMeaningOfSignatures = new Dictionary<int, List<ParticipantMeaningOfSignatureResult>>()
            {
                { 4, new List<ParticipantMeaningOfSignatureResult>()
                    {
                        meaningOfSignature
                    }
                }
            };

            // Act
            await _reviewService.UpdateMeaningOfSignaturesAsync(ReviewId, UserId, new[] {
                new MeaningOfSignatureParameter() {
                Adding = true,
                MeaningOfSignatureId = 3,
                ParticipantId = 4
                }
            });

            // Assert
            var result = _reviewPackageRawData.Reviewers.FirstOrDefault().SelectedRoleMoSAssignments.FirstOrDefault();

            Assert.IsNotNull(result, "A meaning of signature should have been added");
            Assert.AreEqual(meaningOfSignature.GroupId, result.GroupId);
            Assert.AreEqual(meaningOfSignature.MeaningOfSignatureId, result.MeaningOfSignatureId);
            Assert.AreEqual(meaningOfSignature.MeaningOfSignatureValue, result.MeaningOfSignatureValue);
            Assert.AreEqual(4, result.ParticipantId);
            Assert.AreEqual(ReviewId, result.ReviewId);
            Assert.AreEqual(meaningOfSignature.RoleAssignmentId, result.RoleAssignmentId);
            Assert.AreEqual(meaningOfSignature.RoleId, result.RoleId);
            Assert.AreEqual(meaningOfSignature.RoleName, result.RoleName);
        }

        [TestMethod]
        public async Task UpdateMeaningOfSignaturesAsync_Should_Add_New_Meaning_Of_Signature_To_Participant_When_SelectedMoS_Is_Empty()
        {
            // Arrange
            _reviewPackageRawData.IsMoSEnabled = true;
            _reviewPackageRawData.Reviewers = new List<ReviewerRawData>()
            {
                new ReviewerRawData()
                {
                    UserId = 4,
                    Permission = ReviewParticipantRole.Approver,
                    SelectedRoleMoSAssignments = new List<ParticipantMeaningOfSignature>()
                }
            };

            var meaningOfSignature = new ParticipantMeaningOfSignatureResult()
            {
                GroupId = 6,
                MeaningOfSignatureId = 3,
                MeaningOfSignatureValue = "foo",
                ParticipantId = 4,
                RoleAssignmentId = 7,
                RoleId = 8,
                RoleName = "bar"
            };

            _possibleMeaningOfSignatures = new Dictionary<int, List<ParticipantMeaningOfSignatureResult>>()
            {
                { 4, new List<ParticipantMeaningOfSignatureResult>()
                    {
                        meaningOfSignature
                    }
                }
            };

            // Act
            await _reviewService.UpdateMeaningOfSignaturesAsync(ReviewId, UserId, new[] {
                new MeaningOfSignatureParameter() {
                Adding = true,
                MeaningOfSignatureId = 3,
                ParticipantId = 4
                }
            });

            // Assert
            var result = _reviewPackageRawData.Reviewers.FirstOrDefault().SelectedRoleMoSAssignments.FirstOrDefault();

            Assert.IsNotNull(result, "A meaning of signature should have been added");
            Assert.AreEqual(meaningOfSignature.GroupId, result.GroupId);
            Assert.AreEqual(meaningOfSignature.MeaningOfSignatureId, result.MeaningOfSignatureId);
            Assert.AreEqual(meaningOfSignature.MeaningOfSignatureValue, result.MeaningOfSignatureValue);
            Assert.AreEqual(4, result.ParticipantId);
            Assert.AreEqual(ReviewId, result.ReviewId);
            Assert.AreEqual(meaningOfSignature.RoleAssignmentId, result.RoleAssignmentId);
            Assert.AreEqual(meaningOfSignature.RoleId, result.RoleId);
            Assert.AreEqual(meaningOfSignature.RoleName, result.RoleName);
        }

        [TestMethod]
        public async Task UpdateMeaningOfSignaturesAsync_Should_Update_Existing_Meaning_Of_Signature_To_Participant()
        {
            // Arrange
            _reviewPackageRawData.IsMoSEnabled = true;
            _reviewPackageRawData.Reviewers = new List<ReviewerRawData>()
            {
                new ReviewerRawData()
                {
                    UserId = 4,
                    Permission = ReviewParticipantRole.Approver,
                    SelectedRoleMoSAssignments = new List<ParticipantMeaningOfSignature>()
                    {
                        new ParticipantMeaningOfSignature()
                        {
                            MeaningOfSignatureId = 3
                        }
                    }
                }
            };

            var meaningOfSignature = new ParticipantMeaningOfSignatureResult()
            {
                GroupId = 6,
                MeaningOfSignatureId = 3,
                MeaningOfSignatureValue = "foo",
                ParticipantId = 4,
                RoleAssignmentId = 7,
                RoleId = 8,
                RoleName = "bar"
            };

            _possibleMeaningOfSignatures = new Dictionary<int, List<ParticipantMeaningOfSignatureResult>>()
            {
                { 4, new List<ParticipantMeaningOfSignatureResult>()
                    {
                        meaningOfSignature
                    }
                }
            };

            // Act
            await _reviewService.UpdateMeaningOfSignaturesAsync(ReviewId, UserId, new[] {
                new MeaningOfSignatureParameter() {
                Adding = true,
                MeaningOfSignatureId = 3,
                ParticipantId = 4
                }
            });

            // Assert
            var selectedMos = _reviewPackageRawData.Reviewers.FirstOrDefault().SelectedRoleMoSAssignments;
            var result = selectedMos.FirstOrDefault();

            Assert.AreEqual(1, selectedMos.Count, "There should only be one meaning of signature");
            Assert.IsNotNull(result, "A meaning of signature should have been added");
            Assert.AreEqual(meaningOfSignature.GroupId, result.GroupId);
            Assert.AreEqual(meaningOfSignature.MeaningOfSignatureId, result.MeaningOfSignatureId);
            Assert.AreEqual(meaningOfSignature.MeaningOfSignatureValue, result.MeaningOfSignatureValue);
            Assert.AreEqual(4, result.ParticipantId);
            Assert.AreEqual(ReviewId, result.ReviewId);
            Assert.AreEqual(meaningOfSignature.RoleAssignmentId, result.RoleAssignmentId);
            Assert.AreEqual(meaningOfSignature.RoleId, result.RoleId);
            Assert.AreEqual(meaningOfSignature.RoleName, result.RoleName);
        }

        [TestMethod]
        public async Task UpdateMeaningOfSignaturesAsync_Should_Call_UpdateReviewPackage()
        {
            // Arrange
            _reviewPackageRawData.IsMoSEnabled = true;
            _reviewPackageRawData.Reviewers = new List<ReviewerRawData>()
            {
                new ReviewerRawData()
                {
                    UserId = 4,
                    Permission = ReviewParticipantRole.Approver
                }
            };

            var meaningOfSignature = new ParticipantMeaningOfSignatureResult()
            {
                GroupId = 6,
                MeaningOfSignatureId = 3,
                MeaningOfSignatureValue = "foo",
                ParticipantId = 4,
                RoleAssignmentId = 7,
                RoleId = 8,
                RoleName = "bar"
            };

            _possibleMeaningOfSignatures = new Dictionary<int, List<ParticipantMeaningOfSignatureResult>>()
            {
                { 4, new List<ParticipantMeaningOfSignatureResult>()
                    {
                        meaningOfSignature
                    }
                }
            };

            // Act
            await _reviewService.UpdateMeaningOfSignaturesAsync(ReviewId, UserId, new[] {
                new MeaningOfSignatureParameter() {
                Adding = true,
                MeaningOfSignatureId = 3,
                ParticipantId = 4
                }
            });

            // Assert
            _mockReviewRepository.Verify(repo => repo.UpdateReviewPackageRawDataAsync(ReviewId, _reviewPackageRawData, 2));
        }

        #endregion
    }
}
