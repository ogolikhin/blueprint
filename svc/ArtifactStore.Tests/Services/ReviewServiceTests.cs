using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        private const int UserId = 2;
        private const int ProjectId = 1;

        private IReviewsService _reviewService;
        private Mock<IReviewsRepository> _mockReviewRepository;
        private Mock<IArtifactRepository> _mockArtifactRepository;
        private Mock<IArtifactPermissionsRepository> _mockArtifactPermissionsRepository;
        private Mock<IArtifactVersionsRepository> _mockArtifactVersionsRepository;
        private Mock<ILockArtifactsRepository> _mockLockArtifactsRepository;
        private Mock<IItemInfoRepository> _mockItemInfoRepository;

        private ReviewPackageRawData _reviewPackageRawData;
        private ArtifactBasicDetails _artifactDetails;
        private PropertyValueString _propertyValueString;

        private int _revisionId;
        private bool _hasReadPermissions;
        private bool _hasEditPermissions;
        private bool _isLockSuccessful;
        private ReviewType _reviewType;
        private ProjectPermissions _projectPermissions;
        private Dictionary<int, List<ParticipantMeaningOfSignatureResult>> _possibleMeaningOfSignatures;

        [TestInitialize]
        public void Initialize()
        {
            _reviewPackageRawData = new ReviewPackageRawData();

            _artifactDetails = new ArtifactBasicDetails
            {
                ItemId = ReviewId,
                ProjectId = ProjectId,
                PrimitiveItemTypePredefined = (int)ItemTypePredefined.ArtifactReviewPackage
            };

            _propertyValueString = new PropertyValueString();

            _mockReviewRepository = new Mock<IReviewsRepository>();

            _mockReviewRepository
                .Setup(m => m.GetReviewPackageRawDataAsync(ReviewId, UserId, It.IsAny<int>()))
                .ReturnsAsync(_reviewPackageRawData);

            _mockReviewRepository
                .Setup(m => m.GetPossibleMeaningOfSignaturesForParticipantsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(() => _possibleMeaningOfSignatures);

            _mockReviewRepository
                .Setup(m => m.GetReviewTypeAsync(ReviewId, UserId, It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(() => _reviewType);

            _mockReviewRepository
                .Setup(m => m.GetReviewPropertyStringAsync(ReviewId, UserId))
                .ReturnsAsync(() => _propertyValueString);

            _mockArtifactRepository = new Mock<IArtifactRepository>();
            _mockArtifactRepository
                .Setup(m => m.GetArtifactBasicDetails(ReviewId, UserId))
                .ReturnsAsync(() => _artifactDetails);

            _mockArtifactPermissionsRepository = new Mock<IArtifactPermissionsRepository>();

            _mockArtifactPermissionsRepository
                .Setup(m => m.HasReadPermissions(ReviewId, UserId, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(() => _hasReadPermissions);

            _mockArtifactPermissionsRepository
                .Setup(m => m.HasEditPermissions(ReviewId, UserId, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(() => _hasEditPermissions);

            _mockArtifactPermissionsRepository
                .Setup(m => m.GetProjectPermissions(ProjectId))
                .ReturnsAsync(() => _projectPermissions);

            _mockArtifactVersionsRepository = new Mock<IArtifactVersionsRepository>();

            _mockLockArtifactsRepository = new Mock<ILockArtifactsRepository>();

            _mockLockArtifactsRepository
                .Setup(m => m.LockArtifactAsync(ReviewId, UserId))
                .ReturnsAsync(() => _isLockSuccessful);

            _mockItemInfoRepository = new Mock<IItemInfoRepository>();
            _mockItemInfoRepository
                .Setup(m => m.GetRevisionId(ReviewId, UserId, It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(_revisionId);

            _revisionId = int.MaxValue;
            _hasReadPermissions = true;
            _hasEditPermissions = true;
            _isLockSuccessful = true;
            _reviewType = ReviewType.Public;
            _projectPermissions = ProjectPermissions.None;

            _reviewService = new ReviewsService(
                _mockReviewRepository.Object,
                _mockArtifactRepository.Object,
                _mockArtifactPermissionsRepository.Object,
                _mockArtifactVersionsRepository.Object,
                _mockLockArtifactsRepository.Object,
                _mockItemInfoRepository.Object);
        }

        #region GetReviewSettingsAsync

        [TestMethod]
        public async Task GetReviewSettingsAsync_ReviewNotFound_ThrowsResourceNotFoundException()
        {
            // Arrange
            _artifactDetails = null;

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
            _hasReadPermissions = false;
            _hasEditPermissions = false;

            // Act
            try
            {
                await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);
            }
            catch (AuthorizationException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.UnauthorizedAccess, ex.ErrorCode);
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

        [TestMethod]
        public async Task GetReviewSettingsAsync_PublicReview_IsESignatureEnabledInProjectIsFalse()
        {
            // Arrange
            _reviewType = ReviewType.Public;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(false, reviewSettings.IsESignatureEnabled);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_PublicReview_IsMeaningOfSignatureEnabledIsFalse()
        {
            // Arrange
            _reviewType = ReviewType.Public;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(false, reviewSettings.IsMeaningOfSignatureEnabled);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_InformalReview_IsESignatureEnabledIsFalse()
        {
            // Arrange
            _reviewType = ReviewType.Informal;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(false, reviewSettings.IsESignatureEnabled);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_InformalReview_IsMeaningOfSignatureEnabledIsFalse()
        {
            // Arrange
            _reviewType = ReviewType.Informal;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(false, reviewSettings.IsMeaningOfSignatureEnabled);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_FormalDraftReview_IsESignatureEnabledInProjectIsTrue()
        {
            // Arrange
            _reviewType = ReviewType.Formal;
            _reviewPackageRawData.Status = ReviewPackageStatus.Draft;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(true, reviewSettings.IsESignatureEnabled);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_FormalActiveReview_IsESignatureEnabledIsFalse()
        {
            // Arrange
            _reviewType = ReviewType.Formal;
            _reviewPackageRawData.Status = ReviewPackageStatus.Active;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(false, reviewSettings.IsESignatureEnabled);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_FormalClosedReview_IsESignatureEnabledIsFalse()
        {
            // Arrange
            _reviewType = ReviewType.Formal;
            _reviewPackageRawData.Status = ReviewPackageStatus.Closed;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(false, reviewSettings.IsESignatureEnabled);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_FormalReview_ESignatureIsDisabledInProject_IsMeaningOfSignatureEnabledIsFalse()
        {
            // Arrange
            _reviewType = ReviewType.Formal;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(false, reviewSettings.IsMeaningOfSignatureEnabled);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_FormalReview_ESignatureIsEnabledInProject_IsMeaningOfSignatureEnabledInProjectIsTrue()
        {
            // Arrange
            _reviewType = ReviewType.Formal;
            _projectPermissions = ProjectPermissions.IsMeaningOfSignatureEnabled;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(true, reviewSettings.IsMeaningOfSignatureEnabled);
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
            _hasEditPermissions = false;

            // Act
            try
            {
                await _reviewService.UpdateReviewSettingsAsync(ReviewId, new ReviewSettings(), UserId);
            }
            catch (AuthorizationException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.UnauthorizedAccess, ex.ErrorCode);
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
        public async Task UpdateReviewSettingsAsync_RequireMeaningOfSignatureSetToTrue_ESignatureNotEnabled_ThrowsConflictException()
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
        public async Task UpdateReviewSettingsAsync_RequireMeaningOfSignatureSetToFalse_ESignatureNotEnabled_DoesNotThrowException()
        {
            // Arrange
            _reviewPackageRawData.IsMoSEnabled = true;
            _reviewPackageRawData.IsESignatureEnabled = false;
            _reviewPackageRawData.Status = ReviewPackageStatus.Draft;
            var updatedReviewSettings = new ReviewSettings { RequireMeaningOfSignature = false };

            _mockArtifactPermissionsRepository
                .Setup(m => m.GetProjectPermissions(_artifactDetails.ProjectId))
                .ReturnsAsync(ProjectPermissions.IsMeaningOfSignatureEnabled);

            // Act
            try
            {
                await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, UserId);
            }
            catch (Exception ex)
            {
                // Assert
                Assert.Fail("Unexpected exception thrown: {0}!", ex);
            }
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_RequireMeaningOfSignatureSetToTrue_ESignatureEnabled_DoesNotThrowException()
        {
            // Arrange
            _reviewPackageRawData.IsMoSEnabled = false;
            _reviewPackageRawData.IsESignatureEnabled = true;
            _reviewPackageRawData.Status = ReviewPackageStatus.Draft;
            _reviewPackageRawData.Reviewers = new List<ReviewerRawData>();

            var updatedReviewSettings = new ReviewSettings { RequireESignature = true, RequireMeaningOfSignature = true };

            _mockArtifactPermissionsRepository
                .Setup(m => m.GetProjectPermissions(_artifactDetails.ProjectId))
                .ReturnsAsync(ProjectPermissions.IsMeaningOfSignatureEnabled);

            // Act
            try
            {
                await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, UserId);
            }
            catch (Exception ex)
            {
                // Assert
                Assert.Fail("Unexpected exception thrown: {0}!", ex);
            }
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_RequireMeaningOfSignatureSetToFalse_ESignatureEnabled_DoesNotThrowException()
        {
            // Arrange
            _reviewPackageRawData.IsMoSEnabled = true;
            _reviewPackageRawData.IsESignatureEnabled = true;
            _reviewPackageRawData.Status = ReviewPackageStatus.Draft;
            var updatedReviewSettings = new ReviewSettings { RequireESignature = true, RequireMeaningOfSignature = false };

            _mockArtifactPermissionsRepository
                .Setup(m => m.GetProjectPermissions(_artifactDetails.ProjectId))
                .ReturnsAsync(ProjectPermissions.IsMeaningOfSignatureEnabled);

            // Act
            try
            {
                await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, UserId);
            }
            catch (Exception ex)
            {
                // Assert
                Assert.Fail("Unexpected exception thrown: {0}!", ex);
            }
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
            _reviewPackageRawData.Reviewers = new List<ReviewerRawData>();

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

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_RequireMeaningOfSignatureSetToFalse_Should_Not_Set_Default_Meaning_Of_Signature_For_All_Approvers()
        {
            // Arrange
            _reviewPackageRawData.IsMoSEnabled = true;
            _reviewPackageRawData.IsESignatureEnabled = true;
            _reviewPackageRawData.Reviewers = new List<ReviewerRawData>
            {
                new ReviewerRawData
                {
                    UserId = 2,
                    Permission = ReviewParticipantRole.Approver
                },
                new ReviewerRawData
                {
                    UserId = 3,
                    Permission = ReviewParticipantRole.Reviewer
                }
            };

            _possibleMeaningOfSignatures = new Dictionary<int, List<ParticipantMeaningOfSignatureResult>>
            {
                {
                    2,
                    new List<ParticipantMeaningOfSignatureResult>
                    {
                        new ParticipantMeaningOfSignatureResult { RoleAssignmentId = 5 },
                        new ParticipantMeaningOfSignatureResult { RoleAssignmentId = 6 }
                    }
                }
            };

            var updatedReviewSettings = new ReviewSettings
            {
                RequireESignature = true,
                RequireMeaningOfSignature = false
            };

            _mockArtifactPermissionsRepository
                .Setup(m => m.GetProjectPermissions(_artifactDetails.ProjectId))
                .ReturnsAsync(ProjectPermissions.IsMeaningOfSignatureEnabled);

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, UserId);

            // Assert
            var approver = _reviewPackageRawData.Reviewers.First(r => r.Permission == ReviewParticipantRole.Approver);
            var reviewer = _reviewPackageRawData.Reviewers.First(r => r.Permission == ReviewParticipantRole.Reviewer);

            Assert.IsNull(reviewer.SelectedRoleMoSAssignments);
            Assert.IsNull(approver.SelectedRoleMoSAssignments);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_RequireMeaningOfSignatureChanged_Should_Set_Default_Meaning_Of_Signature_For_All_Approvers()
        {
            // Arrange
            _reviewPackageRawData.IsMoSEnabled = false;
            _reviewPackageRawData.IsESignatureEnabled = true;
            _reviewPackageRawData.Reviewers = new List<ReviewerRawData>
            {
                new ReviewerRawData
                {
                    UserId = 2,
                    Permission = ReviewParticipantRole.Approver
                },
                new ReviewerRawData
                {
                    UserId = 3,
                    Permission = ReviewParticipantRole.Reviewer
                }
            };

            _possibleMeaningOfSignatures = new Dictionary<int, List<ParticipantMeaningOfSignatureResult>>
            {
                {
                    2,
                    new List<ParticipantMeaningOfSignatureResult>
                    {
                        new ParticipantMeaningOfSignatureResult { RoleAssignmentId = 5 },
                        new ParticipantMeaningOfSignatureResult { RoleAssignmentId = 6 }
                    }
                }
            };

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
            var approver = _reviewPackageRawData.Reviewers.First(r => r.Permission == ReviewParticipantRole.Approver);
            var reviewer = _reviewPackageRawData.Reviewers.First(r => r.Permission == ReviewParticipantRole.Reviewer);

            Assert.IsNull(reviewer.SelectedRoleMoSAssignments);

            Assert.AreEqual(2, approver.SelectedRoleMoSAssignments.Count);
            Assert.IsTrue(approver.SelectedRoleMoSAssignments.All(mos => mos.RoleAssignmentId == 5 || mos.RoleAssignmentId == 6));
        }

        #endregion UpdateReviewSettingsAsync

        #region UpdateMeaningOfSignaturesAsync

        [TestMethod]
        public async Task UpdateMeaningOfSignaturesAsync_Should_Throw_When_Review_Doesnt_Exist()
        {
            // Arrange
            _artifactDetails = null;

            // Act
            try
            {
                await _reviewService.UpdateMeaningOfSignaturesAsync(ReviewId, new MeaningOfSignatureParameter[0], UserId);
            }
            catch (ResourceNotFoundException ex)
            {
                Assert.AreEqual(ErrorCodes.ResourceNotFound, ex.ErrorCode);

                return;
            }

            Assert.Fail("A ResourceNotFoundException was not thrown");
        }

        [TestMethod]
        public async Task UpdateMeaningOfSignaturesAsync_Should_Throw_When_Artifact_Is_Not_A_Review()
        {
            // Arrange
            _artifactDetails.PrimitiveItemTypePredefined = (int)ItemTypePredefined.Actor;

            // Act
            try
            {
                await _reviewService.UpdateMeaningOfSignaturesAsync(ReviewId, new MeaningOfSignatureParameter[0], UserId);
            }
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ErrorCodes.BadRequest, ex.ErrorCode);

                return;
            }

            Assert.Fail("A BadRequestException was not thrown");
        }

        [TestMethod]
        public async Task UpdateMeaningOfSignaturesAsync_Should_Throw_When_User_Does_Not_Have_Edit_Permissions_For_Review()
        {
            // Arrange
            _hasEditPermissions = false;

            // Act
            try
            {
                await _reviewService.UpdateMeaningOfSignaturesAsync(ReviewId, new MeaningOfSignatureParameter[0], UserId);
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
                await _reviewService.UpdateMeaningOfSignaturesAsync(ReviewId, new MeaningOfSignatureParameter[0], UserId);
            }
            catch (ConflictException ex)
            {
                Assert.AreEqual(ErrorCodes.ReviewClosed, ex.ErrorCode);

                return;
            }

            Assert.Fail("A ConflictException was not thrown");
        }

        [TestMethod]
        public async Task UpdateMeaningOfSignaturesAsync_Should_Throw_When_Meaning_Of_Signature_Is_Not_Enabled_Case_Empty_ReviewPackage()
        {
            // Arrange
            _reviewPackageRawData = null;

            // Act
            try
            {
                await _reviewService.UpdateMeaningOfSignaturesAsync(ReviewId, new MeaningOfSignatureParameter[0], UserId);
            }
            catch (ConflictException ex)
            {
                Assert.AreEqual(ErrorCodes.MeaningOfSignatureNotEnabled, ex.ErrorCode);

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
                await _reviewService.UpdateMeaningOfSignaturesAsync(ReviewId, new MeaningOfSignatureParameter[0], UserId);
            }
            catch (ConflictException ex)
            {
                Assert.AreEqual(ErrorCodes.MeaningOfSignatureNotEnabled, ex.ErrorCode);

                return;
            }

            Assert.Fail("A ConflictException was not thrown");
        }

        [TestMethod]
        public async Task UpdateMeaningOfSignaturesAsync_Should_Throw_When_Review_Is_Locked_By_Other_User()
        {
            // Arrange
            _reviewPackageRawData.IsMoSEnabled = true;
            _artifactDetails.LockedByUserId = 50;

            // Act
            try
            {
                await _reviewService.UpdateMeaningOfSignaturesAsync(ReviewId, new MeaningOfSignatureParameter[0], UserId);
            }
            catch (ConflictException ex)
            {
                Assert.AreEqual(ErrorCodes.LockedByOtherUser, ex.ErrorCode);

                return;
            }

            Assert.Fail("A ConflictException was not thrown");
        }

        [TestMethod]
        public async Task UpdateMeaningOfSignaturesAsync_Should_Throw_When_Locking_Review_Fails()
        {
            // Arrange
            _reviewPackageRawData.IsMoSEnabled = true;
            _isLockSuccessful = false;

            // Act
            try
            {
                await _reviewService.UpdateMeaningOfSignaturesAsync(ReviewId, new MeaningOfSignatureParameter[0], UserId);
            }
            catch (ConflictException ex)
            {
                Assert.AreEqual(ErrorCodes.LockedByOtherUser, ex.ErrorCode);

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
            var meaningOfSignatureParameter = new MeaningOfSignatureParameter
            {
                Adding = true,
                RoleAssignmentId = 3,
                ParticipantId = 4
            };
            // Act
            try
            {
                await _reviewService.UpdateMeaningOfSignaturesAsync(ReviewId, new[] { meaningOfSignatureParameter }, UserId);
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
            _reviewPackageRawData.Reviewers = new List<ReviewerRawData>
            {
                new ReviewerRawData
                {
                    UserId = 4,
                    Permission = ReviewParticipantRole.Reviewer
                }
            };
            var meaningOfSignatureParameter = new MeaningOfSignatureParameter
            {
                Adding = true,
                RoleAssignmentId = 3,
                ParticipantId = 4
            };

            // Act
            try
            {
                await _reviewService.UpdateMeaningOfSignaturesAsync(ReviewId, new[] { meaningOfSignatureParameter }, UserId);
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
            _reviewPackageRawData.Reviewers = new List<ReviewerRawData>
            {
                new ReviewerRawData
                {
                    UserId = 4,
                    Permission = ReviewParticipantRole.Approver
                }
            };

            _possibleMeaningOfSignatures = new Dictionary<int, List<ParticipantMeaningOfSignatureResult>>();
            var meaningOfSignatureParameter = new MeaningOfSignatureParameter
            {
                Adding = true,
                RoleAssignmentId = 3,
                ParticipantId = 4
            };

            // Act
            try
            {
                await _reviewService.UpdateMeaningOfSignaturesAsync(ReviewId, new[] { meaningOfSignatureParameter }, UserId);
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
            _reviewPackageRawData.Reviewers = new List<ReviewerRawData>
            {
                new ReviewerRawData
                {
                    UserId = 4,
                    Permission = ReviewParticipantRole.Approver
                }
            };

            _possibleMeaningOfSignatures = new Dictionary<int, List<ParticipantMeaningOfSignatureResult>>
            {
                { 4, new List<ParticipantMeaningOfSignatureResult>() }
            };
            var meaningOfSignatureParameter = new MeaningOfSignatureParameter
            {
                Adding = true,
                RoleAssignmentId = 3,
                ParticipantId = 4
            };

            // Act
            try
            {
                await _reviewService.UpdateMeaningOfSignaturesAsync(ReviewId, new[] { meaningOfSignatureParameter }, UserId);
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
            _reviewPackageRawData.Reviewers = new List<ReviewerRawData>
            {
                new ReviewerRawData
                {
                    UserId = 4,
                    Permission = ReviewParticipantRole.Approver
                }
            };

            var meaningOfSignature = new ParticipantMeaningOfSignatureResult
            {
                GroupId = 6,
                MeaningOfSignatureId = 3,
                MeaningOfSignatureValue = "foo",
                ParticipantId = 4,
                RoleAssignmentId = 7,
                RoleId = 8,
                RoleName = "bar"
            };

            _possibleMeaningOfSignatures = new Dictionary<int, List<ParticipantMeaningOfSignatureResult>>
            {
                { 4, new List<ParticipantMeaningOfSignatureResult> { meaningOfSignature } }
            };

            var meaningOfSignatureParameter = new MeaningOfSignatureParameter
            {
                Adding = true,
                RoleAssignmentId = 7,
                ParticipantId = 4
            };

            // Act
            await _reviewService.UpdateMeaningOfSignaturesAsync(ReviewId, new[] { meaningOfSignatureParameter }, UserId);

            // Assert
            var result = _reviewPackageRawData.Reviewers.First().SelectedRoleMoSAssignments.FirstOrDefault();

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
            _reviewPackageRawData.Reviewers = new List<ReviewerRawData>
            {
                new ReviewerRawData
                {
                    UserId = 4,
                    Permission = ReviewParticipantRole.Approver,
                    SelectedRoleMoSAssignments = new List<ParticipantMeaningOfSignature>()
                }
            };

            var meaningOfSignature = new ParticipantMeaningOfSignatureResult
            {
                GroupId = 6,
                MeaningOfSignatureId = 3,
                MeaningOfSignatureValue = "foo",
                ParticipantId = 4,
                RoleAssignmentId = 7,
                RoleId = 8,
                RoleName = "bar"
            };

            _possibleMeaningOfSignatures = new Dictionary<int, List<ParticipantMeaningOfSignatureResult>>
            {
                { 4, new List<ParticipantMeaningOfSignatureResult> { meaningOfSignature } }
            };

            var meaningOfSignatureParameter = new MeaningOfSignatureParameter
            {
                Adding = true,
                RoleAssignmentId = 7,
                ParticipantId = 4
            };

            // Act
            await _reviewService.UpdateMeaningOfSignaturesAsync(ReviewId, new[] { meaningOfSignatureParameter }, UserId);

            // Assert
            var result = _reviewPackageRawData.Reviewers.First().SelectedRoleMoSAssignments.FirstOrDefault();

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
            _reviewPackageRawData.Reviewers = new List<ReviewerRawData>
            {
                new ReviewerRawData
                {
                    UserId = 4,
                    Permission = ReviewParticipantRole.Approver,
                    SelectedRoleMoSAssignments = new List<ParticipantMeaningOfSignature>
                    {
                        new ParticipantMeaningOfSignature
                        {
                            RoleAssignmentId = 7
                        }
                    }
                }
            };

            var meaningOfSignature = new ParticipantMeaningOfSignatureResult
            {
                GroupId = 6,
                MeaningOfSignatureId = 3,
                MeaningOfSignatureValue = "foo",
                ParticipantId = 4,
                RoleAssignmentId = 7,
                RoleId = 8,
                RoleName = "bar"
            };

            _possibleMeaningOfSignatures = new Dictionary<int, List<ParticipantMeaningOfSignatureResult>>
            {
                { 4, new List<ParticipantMeaningOfSignatureResult> { meaningOfSignature } }
            };
            var meaningOfSignatureParameter = new MeaningOfSignatureParameter
            {
                Adding = true,
                RoleAssignmentId = 7,
                ParticipantId = 4
            };

            // Act
            await _reviewService.UpdateMeaningOfSignaturesAsync(ReviewId, new[] { meaningOfSignatureParameter }, UserId);

            // Assert
            var selectedMos = _reviewPackageRawData.Reviewers.First().SelectedRoleMoSAssignments;
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
        public async Task UpdateMeaningOfSignaturesAsync_Should_Remove_Meaning_Of_Signature_When_Remove_Is_True()
        {
            // Arrange
            _reviewPackageRawData.IsMoSEnabled = true;
            _reviewPackageRawData.Reviewers = new List<ReviewerRawData>
            {
                new ReviewerRawData
                {
                    UserId = 4,
                    Permission = ReviewParticipantRole.Approver,
                    SelectedRoleMoSAssignments = new List<ParticipantMeaningOfSignature>
                    {
                        new ParticipantMeaningOfSignature
                        {
                            RoleAssignmentId = 7
                        }
                    }
                }
            };

            var meaningOfSignature = new ParticipantMeaningOfSignatureResult
            {
                GroupId = 6,
                MeaningOfSignatureId = 3,
                MeaningOfSignatureValue = "foo",
                ParticipantId = 4,
                RoleAssignmentId = 7,
                RoleId = 8,
                RoleName = "bar"
            };

            _possibleMeaningOfSignatures = new Dictionary<int, List<ParticipantMeaningOfSignatureResult>>
            {
                { 4, new List<ParticipantMeaningOfSignatureResult> { meaningOfSignature } }
            };
            var meaningOfSignatureParameter = new MeaningOfSignatureParameter
            {
                Adding = false,
                RoleAssignmentId = 7,
                ParticipantId = 4
            };

            // Act
            await _reviewService.UpdateMeaningOfSignaturesAsync(ReviewId, new[] { meaningOfSignatureParameter }, UserId);

            // Assert
            var selectedMos = _reviewPackageRawData.Reviewers.First().SelectedRoleMoSAssignments;

            Assert.AreEqual(0, selectedMos.Count, "There should be one meaning of signature");
        }

        [TestMethod]
        public async Task UpdateMeaningOfSignaturesAsync_Should_Not_Remove_Meaning_Of_Signature_If_It_Doesnt_Already_Exist()
        {
            // Arrange
            _reviewPackageRawData.IsMoSEnabled = true;
            _reviewPackageRawData.Reviewers = new List<ReviewerRawData>
            {
                new ReviewerRawData
                {
                    UserId = 4,
                    Permission = ReviewParticipantRole.Approver,
                    SelectedRoleMoSAssignments = new List<ParticipantMeaningOfSignature>
                    {
                        new ParticipantMeaningOfSignature
                        {
                            RoleAssignmentId = 6
                        }
                    }
                }
            };

            var meaningOfSignature = new ParticipantMeaningOfSignatureResult
            {
                GroupId = 6,
                MeaningOfSignatureId = 3,
                MeaningOfSignatureValue = "foo",
                ParticipantId = 4,
                RoleAssignmentId = 7,
                RoleId = 8,
                RoleName = "bar"
            };

            _possibleMeaningOfSignatures = new Dictionary<int, List<ParticipantMeaningOfSignatureResult>>
            {
                { 4, new List<ParticipantMeaningOfSignatureResult> { meaningOfSignature } }
            };

            var meaningOfSignatureParameter = new MeaningOfSignatureParameter
            {
                Adding = false,
                RoleAssignmentId = 7,
                ParticipantId = 4
            };

            // Act
            await _reviewService.UpdateMeaningOfSignaturesAsync(ReviewId, new[] { meaningOfSignatureParameter }, UserId);

            // Assert
            var selectedMos = _reviewPackageRawData.Reviewers.First().SelectedRoleMoSAssignments;

            Assert.AreEqual(1, selectedMos.Count, "There should be one meaning of signature");
        }

        [TestMethod]
        public async Task UpdateMeaningOfSignaturesAsync_Should_Call_UpdateReviewPackage()
        {
            // Arrange
            _reviewPackageRawData.IsMoSEnabled = true;
            _reviewPackageRawData.Reviewers = new List<ReviewerRawData>
            {
                new ReviewerRawData
                {
                    UserId = 4,
                    Permission = ReviewParticipantRole.Approver
                }
            };

            var meaningOfSignature = new ParticipantMeaningOfSignatureResult
            {
                GroupId = 6,
                MeaningOfSignatureId = 3,
                MeaningOfSignatureValue = "foo",
                ParticipantId = 4,
                RoleAssignmentId = 7,
                RoleId = 8,
                RoleName = "bar"
            };

            _possibleMeaningOfSignatures = new Dictionary<int, List<ParticipantMeaningOfSignatureResult>>
            {
                { 4, new List<ParticipantMeaningOfSignatureResult> { meaningOfSignature } }
            };
            var meaningOfSignatureParameter = new MeaningOfSignatureParameter
            {
                Adding = true,
                RoleAssignmentId = 7,
                ParticipantId = 4
            };

            // Act
            await _reviewService.UpdateMeaningOfSignaturesAsync(ReviewId, new[] { meaningOfSignatureParameter }, UserId);

            // Assert
            _mockReviewRepository.Verify(repo => repo.UpdateReviewPackageRawDataAsync(ReviewId, _reviewPackageRawData, 2));
        }

        #endregion

        #region AssignRoleToParticipantAsync

        [TestMethod]
        public async Task AssignRoleToParticipantAsync_Should_Throw_When_Review_Does_Not_Exist()
        {
            _mockReviewRepository.Setup(repo => repo.GetReviewApprovalRolesInfoAsync(ReviewId, UserId)).ReturnsAsync((PropertyValueString)null);

            var content = new AssignParticipantRoleParameter
            {
                Role = ReviewParticipantRole.Approver,
                ItemIds = new List<int>() { 1 },
                SelectionType = SelectionType.Selected,
            };

            // Act
            try
            {
                await _reviewService.AssignRoleToParticipantsAsync(ReviewId, content, UserId);
            }
            catch (BadRequestException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.ResourceNotFound, ex.ErrorCode);

                return;
            }

            Assert.Fail("A BadRequestException was not thrown.");
        }

        [TestMethod]
        public async Task AssignRoleToParticipantAsync_Should_Throw_When_Review_Is_Deleted()
        {
            // Arrange
            var propertyValue = new PropertyValueString
            {
                IsDraftRevisionExists = true,
                ArtifactXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewContents xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"/>",
                RevewSubartifactId = 3,
                ProjectId = 1,
                LockedByUserId = UserId,
                IsReviewReadOnly = true,
                BaselineId = 2,
                IsReviewDeleted = true,
                IsUserDisabled = false
            };

            _mockReviewRepository.Setup(repo => repo.GetReviewApprovalRolesInfoAsync(ReviewId, UserId)).ReturnsAsync(propertyValue);

            var content = new AssignParticipantRoleParameter
            {
                ItemIds = new List<int>() { 1 },
                Role = ReviewParticipantRole.Approver
            };

            // Act
            try
            {
                await _reviewService.AssignRoleToParticipantsAsync(ReviewId, content, UserId);
            }
            catch (ResourceNotFoundException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.ResourceNotFound, ex.ErrorCode);

                return;
            }

            Assert.Fail("A ResourceNotFoundException was not thrown.");
        }

        [TestMethod]
        public async Task AssignRoleToParticipantAsync_Should_Throw_When_Review_Is_ReadOnly()
        {
            // Arrange

            var propertyValue = new PropertyValueString
            {
                IsDraftRevisionExists = true,
                ArtifactXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewContents xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"/>",
                RevewSubartifactId = 3,
                ProjectId = 1,
                LockedByUserId = UserId,
                IsReviewReadOnly = true,
                BaselineId = 2,
                IsReviewDeleted = false,
                IsUserDisabled = false
            };

            _mockReviewRepository.Setup(repo => repo.GetReviewApprovalRolesInfoAsync(ReviewId, UserId)).ReturnsAsync(propertyValue);

            var content = new AssignParticipantRoleParameter
            {
                ItemIds = new List<int>() { 1 },
                Role = ReviewParticipantRole.Approver
            };
            // Act

            try
            {
                await _reviewService.AssignRoleToParticipantsAsync(ReviewId, content, UserId);
            }
            catch (ConflictException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.ApprovalRequiredIsReadonlyForReview, ex.ErrorCode);

                return;
            }

            Assert.Fail("A Conflict Exception was not  thrown.");
        }

        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task AssignRoleToParticipantAsync_Should_Throw_When_Review_Is_Not_Locked()
        {
            // Arrange
            var propertyValue = new PropertyValueString
            {
                IsDraftRevisionExists = true,
                ArtifactXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewContents xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"/>",
                RevewSubartifactId = 3,
                ProjectId = 1,
                LockedByUserId = null,
                IsReviewReadOnly = false,
                BaselineId = 2,
                IsReviewDeleted = false,
                IsUserDisabled = false
            };

            _mockReviewRepository.Setup(repo => repo.GetReviewApprovalRolesInfoAsync(ReviewId, UserId)).ReturnsAsync(propertyValue);

            var content = new AssignParticipantRoleParameter
            {
                ItemIds = new List<int>() { 1 },
                Role = ReviewParticipantRole.Approver
            };

            // Act
            await _reviewService.AssignRoleToParticipantsAsync(ReviewId, content, UserId);
        }

        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task AssignRoleToParticipantAsync_Should_Throw_When_User_Is_Disabled()
        {
            // Arrange
            var propertyValue = new PropertyValueString
            {
                IsDraftRevisionExists = true,
                ArtifactXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewContents xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"/>",
                RevewSubartifactId = 3,
                ProjectId = 1,
                LockedByUserId = UserId,
                IsReviewReadOnly = false,
                BaselineId = 2,
                IsReviewDeleted = false,
                IsUserDisabled = true
            };

            _mockReviewRepository.Setup(repo => repo.GetReviewApprovalRolesInfoAsync(ReviewId, UserId)).ReturnsAsync(propertyValue);

            var content = new AssignParticipantRoleParameter
            {
                ItemIds = new List<int>() { 1 },
                Role = ReviewParticipantRole.Approver
            };

            // Act
            await _reviewService.AssignRoleToParticipantsAsync(ReviewId, content, UserId);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task AssignRoleToParticipantAsync_Should_Throw_When_ReviewPackageXml_Is_Empty()
        {
            // Arrange
            var propertyValue = new PropertyValueString
            {
                IsDraftRevisionExists = true,
                ArtifactXml = string.Empty,
                RevewSubartifactId = 3,
                ProjectId = 1,
                LockedByUserId = UserId,
                IsReviewReadOnly = false,
                BaselineId = 2,
                IsReviewDeleted = false,
                IsUserDisabled = false
            };

            _mockReviewRepository.Setup(repo => repo.GetReviewApprovalRolesInfoAsync(ReviewId, UserId)).ReturnsAsync(propertyValue);

            var content = new AssignParticipantRoleParameter
            {
                ItemIds = new List<int>() { 1 },
                Role = ReviewParticipantRole.Approver
            };

            // Act
            await _reviewService.AssignRoleToParticipantsAsync(ReviewId, content, UserId);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task AssignRoleToParticipantAsync_Should_Throw_BadRequestException_With_Wrong_Parameters()
        {
            // Arrange
            var propertyValue = new PropertyValueString
            {
                IsDraftRevisionExists = true,
                ArtifactXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Reviwers><ReviewerRawData><Permission>Reviewer</Permission><UserId>3</UserId></ReviewerRawData></Reviwers></ReviewPackageRawData>",
                RevewSubartifactId = 3,
                ProjectId = 1,
                LockedByUserId = UserId,
                IsReviewReadOnly = false,
                BaselineId = 2,
                IsReviewDeleted = false,
                IsUserDisabled = false
            };

            _mockReviewRepository.Setup(repo => repo.GetReviewApprovalRolesInfoAsync(ReviewId, UserId)).ReturnsAsync(propertyValue);

            var content = new AssignParticipantRoleParameter
            {
                ItemIds = new List<int>(),
                Role = ReviewParticipantRole.Approver,
                SelectionType = SelectionType.Selected

            };

            // Act
            await _reviewService.AssignRoleToParticipantsAsync(ReviewId, content, UserId);

            // Assert
            Expression<Func<ReviewPackageRawData, bool>> reviewPackageCheck = reviewPackage => reviewPackage.Reviewers.All(r => r.Permission == ReviewParticipantRole.Approver);

            _mockReviewRepository.Verify(repo => repo.UpdateReviewPackageRawDataAsync(ReviewId, It.Is(reviewPackageCheck), UserId));
        }

        [TestMethod]
        public async Task AssignRoleToParticipantAsync_Should_Return_DropdownItems_Null_When_Meaning_Of_Signature_Is_Disabled()
        {
            // Arrange
            var propertyValue = new PropertyValueString
            {
                IsDraftRevisionExists = true,
                ArtifactXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Reviwers><ReviewerRawData><Permission>Reviewer</Permission><UserId>3</UserId></ReviewerRawData></Reviwers></ReviewPackageRawData>",
                RevewSubartifactId = 3,
                ProjectId = 1,
                LockedByUserId = UserId,
                IsReviewReadOnly = false,
                BaselineId = 2,
                IsReviewDeleted = false,
                IsUserDisabled = false
            };

            _mockReviewRepository.Setup(repo => repo.GetReviewApprovalRolesInfoAsync(ReviewId, UserId)).ReturnsAsync(propertyValue);

            var content = new AssignParticipantRoleParameter
            {
                ItemIds = new List<int>() { 1 },
                Role = ReviewParticipantRole.Approver
            };

            // Act
            var result = await _reviewService.AssignRoleToParticipantsAsync(ReviewId, content, UserId);

            // Assert
            Assert.IsNull(result.DropdownItems);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task AssignRoleToParticipantAsync_Should_Add_All_Possible_Meaning_Of_Signatures_When_Meaning_Of_Signature_Is_Enabled()
        {
            // Arrange
            var propertyValue = new PropertyValueString
            {
                IsDraftRevisionExists = true,
                ArtifactXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><IsMoSEnabled>true</IsMoSEnabled><Reviwers><ReviewerRawData><Permission>Reviewer</Permission><UserId>3</UserId></ReviewerRawData></Reviwers></ReviewPackageRawData>",
                RevewSubartifactId = 3,
                ProjectId = 1,
                LockedByUserId = UserId,
                IsReviewReadOnly = false,
                BaselineId = 2,
                IsReviewDeleted = false,
                IsUserDisabled = false
            };

            _mockReviewRepository.Setup(repo => repo.GetReviewApprovalRolesInfoAsync(ReviewId, UserId)).ReturnsAsync(propertyValue);

            var content = new AssignParticipantRoleParameter
            {
                ItemIds = new List<int>() { 1 },
                Role = ReviewParticipantRole.Approver
            };

            _possibleMeaningOfSignatures = new Dictionary<int, List<ParticipantMeaningOfSignatureResult>>
            {
                {
                    1,
                    new List<ParticipantMeaningOfSignatureResult>
                    {
                        new ParticipantMeaningOfSignatureResult { RoleAssignmentId = 2 },
                        new ParticipantMeaningOfSignatureResult { RoleAssignmentId = 3 }
                    }
                }
            };

            // Act
            await _reviewService.AssignRoleToParticipantsAsync(ReviewId, content, UserId);

            // Assert
            Expression<Func<ReviewPackageRawData, bool>> reviewPackageCheck = reviewPackage => reviewPackage.Reviewers.First().SelectedRoleMoSAssignments.Count == 2
                                                                                               && reviewPackage.Reviewers.First().SelectedRoleMoSAssignments.All(mos => mos.RoleAssignmentId == 2
                                                                                                                                                                 || mos.RoleAssignmentId == 3);

            _mockReviewRepository.Verify(repo => repo.UpdateReviewPackageRawDataAsync(ReviewId, It.Is(reviewPackageCheck), UserId));
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task AssignRoleToParticipantAsync_Should_Return_All_Assigned_Meaning_Of_Signatures_When_Meaning_Of_Signature_Is_Enabled()
        {
            // Arrange
            var propertyValue = new PropertyValueString
            {
                IsDraftRevisionExists = true,
                ArtifactXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><IsMoSEnabled>true</IsMoSEnabled><Reviwers><ReviewerRawData><Permission>Reviewer</Permission><UserId>3</UserId></ReviewerRawData></Reviwers></ReviewPackageRawData>",
                RevewSubartifactId = 3,
                ProjectId = 1,
                LockedByUserId = UserId,
                IsReviewReadOnly = false,
                BaselineId = 2,
                IsReviewDeleted = false,
                IsUserDisabled = false
            };

            _mockReviewRepository.Setup(repo => repo.GetReviewApprovalRolesInfoAsync(ReviewId, UserId)).ReturnsAsync(propertyValue);

            var content = new AssignParticipantRoleParameter
            {
                ItemIds = new List<int>() { 1 },
                Role = ReviewParticipantRole.Approver
            };

            _possibleMeaningOfSignatures = new Dictionary<int, List<ParticipantMeaningOfSignatureResult>>
            {
                {
                    1,
                    new List<ParticipantMeaningOfSignatureResult>
                    {
                        new ParticipantMeaningOfSignatureResult
                        {
                            MeaningOfSignatureValue = "foo1",
                            RoleAssignmentId = 2,
                            RoleName = "bar1"
                        },
                        new ParticipantMeaningOfSignatureResult
                        {
                            MeaningOfSignatureValue = "foo2",
                            RoleAssignmentId = 3,
                            RoleName = "bar2"
                        }
                    }
                }
            };

            // Act
            var result = (await _reviewService.AssignRoleToParticipantsAsync(ReviewId, content, UserId));
            var dropDowns = result.DropdownItems.ToArray();
            // Assert
            Assert.AreEqual(2, result.DropdownItems.Count());

            Assert.AreEqual("foo1 (bar1)", dropDowns[0].Label);
            Assert.AreEqual(2, dropDowns[0].Value);

            Assert.AreEqual("foo2 (bar2)", dropDowns[1].Label);
            Assert.AreEqual(3, dropDowns[10].Value);
        }

        #endregion

        #region AssignApprovalRequiredToArtifactsAsync

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task AssignApprovalRequiredToArtifacts_Should_Throw_BadRequestException()
        {
            // Arrange
            var content = new AssignArtifactsApprovalParameter
            {
                ItemIds = null,
                ApprovalRequired = true
            };

            // Act
            await _reviewService.AssignApprovalRequiredToArtifactsAsync(1, content, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task AssignApprovalRequiredToArtifacts_Should_Throw_ResourceNotFoundException()
        {
            _propertyValueString = new PropertyValueString
            {
                IsDraftRevisionExists = true,
                ArtifactXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewContents xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"/>",
                RevewSubartifactId = 3,
                ProjectId = ProjectId,
                LockedByUserId = UserId,
                IsReviewReadOnly = false,
                BaselineId = 2,
                IsReviewDeleted = true,
                IsUserDisabled = false
            };
            var content = new AssignArtifactsApprovalParameter
            {
                ItemIds = new List<int> { 1, 2, 3 },
                ApprovalRequired = true
            };

            // Act
            await _reviewService.AssignApprovalRequiredToArtifactsAsync(ReviewId, content, UserId);
        }

        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task AssignApprovalRequiredToArtifacts_Review_ReadOnly_Should_Throw_BadRequestException()
        {
            _propertyValueString = new PropertyValueString
            {
                IsDraftRevisionExists = true,
                ArtifactXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewContents xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"/>",
                RevewSubartifactId = 3,
                ProjectId = 1,
                LockedByUserId = UserId,
                ReviewStatus = ReviewPackageStatus.Closed,
                BaselineId = 2,
                IsReviewDeleted = false,
                IsUserDisabled = false
            };
            var content = new AssignArtifactsApprovalParameter
            {
                ItemIds = new List<int> { 1, 2, 3 },
                ApprovalRequired = true
            };

            // Act
            await _reviewService.AssignApprovalRequiredToArtifactsAsync(ReviewId, content, UserId);
        }

        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task AssignApprovalRequiredToArtifacts_Review_ActiveFormal_Throw_ConflictException()
        {
            _propertyValueString = new PropertyValueString
            {
                IsDraftRevisionExists = true,
                ArtifactXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewContents xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"/>",
                RevewSubartifactId = 3,
                ProjectId = 1,
                LockedByUserId = UserId,
                ReviewStatus = ReviewPackageStatus.Active,
                ReviewType = ReviewType.Formal,
                BaselineId = 2,
                IsReviewDeleted = false,
                IsUserDisabled = false
            };
            var content = new AssignArtifactsApprovalParameter
            {
                ItemIds = new List<int> { 1, 2, 3 },
                ApprovalRequired = true
            };

            // Act
            await _reviewService.AssignApprovalRequiredToArtifactsAsync(ReviewId, content, UserId);
        }

        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task AssignApprovalRequiredToArtifacts_Review_NotLocked_Should_Throw_BadRequestException()
        {
            _propertyValueString = new PropertyValueString
            {
                IsDraftRevisionExists = true,
                ArtifactXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewContents xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"/>",
                RevewSubartifactId = 3,
                ProjectId = 1,
                LockedByUserId = null,
                IsReviewReadOnly = false,
                BaselineId = 2,
                IsReviewDeleted = false,
                IsUserDisabled = false
            };
            var content = new AssignArtifactsApprovalParameter
            {
                ItemIds = new List<int> { 1, 2, 3 },
                ApprovalRequired = true
            };

            // Act
            await _reviewService.AssignApprovalRequiredToArtifactsAsync(ReviewId, content, UserId);
        }

        [TestMethod]
        public async Task AssignApprovalRequiredToArtifacts_Review_Success()
        {
            _propertyValueString = new PropertyValueString
            {
                IsDraftRevisionExists = true,
                ArtifactXml = "<?xml version=\"1.0\" encoding=\"utf - 16\"?><RDReviewContents xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Artifacts><CA><Id>1</Id></CA><CA><ANR>true</ANR><Id>2</Id></CA><CA><Id>3</Id></CA></Artifacts></RDReviewContents>",
                RevewSubartifactId = 3,
                ProjectId = ProjectId,
                LockedByUserId = UserId,
                ReviewStatus = ReviewPackageStatus.Draft,
                BaselineId = null,
                IsReviewDeleted = false,
                IsUserDisabled = false,
                ReviewType = ReviewType.Informal
            };

            var artifactIds = new List<int> { 1, 2, 3 };
            var content = new AssignArtifactsApprovalParameter
            {
                ItemIds = artifactIds,
                ApprovalRequired = false
            };

            var requestedArtifactIds = new List<int> { 1, 3 };

            _mockArtifactVersionsRepository
                .Setup(m => m.GetDeletedAndNotInProjectItems(requestedArtifactIds, ProjectId))
                .ReturnsAsync(new List<int>());

            _mockArtifactPermissionsRepository
                .Setup(m => m.GetArtifactPermissions(requestedArtifactIds, UserId, false, int.MaxValue, true))
                .ReturnsAsync(new Dictionary<int, RolePermissions>
                    {
                        { requestedArtifactIds[0], RolePermissions.Read },
                        { requestedArtifactIds[1], RolePermissions.Read }
                    });

            // Act
            var result = await _reviewService.AssignApprovalRequiredToArtifactsAsync(ReviewId, content, UserId);

            Assert.IsNotNull(result);
            Assert.IsNull(result.ReviewChangeItemErrors);
        }

        [TestMethod]
        public async Task AssignApprovalRequiredToArtifacts_SomeArtifactsDeletedFromReview()
        {
            _propertyValueString = new PropertyValueString
            {
                IsDraftRevisionExists = true,
                ArtifactXml = "<?xml version=\"1.0\" encoding=\"utf - 16\"?><RDReviewContents xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Artifacts><CA><Id>1</Id></CA><CA><Id>2</Id></CA><CA><Id>3</Id></CA></Artifacts></RDReviewContents>",
                RevewSubartifactId = 3,
                ProjectId = ProjectId,
                LockedByUserId = UserId,
                ReviewStatus = ReviewPackageStatus.Draft,
                BaselineId = null,
                IsReviewDeleted = false,
                IsUserDisabled = false,
                ReviewType = ReviewType.Informal
            };
            var artifactIds = new List<int> { 1, 2, 3 };
            var content = new AssignArtifactsApprovalParameter
            {
                ItemIds = artifactIds,
                ApprovalRequired = false
            };

            var requestedArtifactIds = new List<int> { 1, 2, 3 };

            _mockArtifactVersionsRepository
                .Setup(m => m.GetDeletedAndNotInProjectItems(requestedArtifactIds, ProjectId))
                .ReturnsAsync(new List<int> { 1, 2 });

            var permissionsArtifactIds = new List<int> { 3 };

            _mockArtifactPermissionsRepository
                .Setup(m => m.GetArtifactPermissions(permissionsArtifactIds, UserId, false, int.MaxValue, true))
                .ReturnsAsync(() => new Dictionary<int, RolePermissions>
                    {
                        { permissionsArtifactIds[0], RolePermissions.Read }
                    });

            // Act
            var result = await _reviewService.AssignApprovalRequiredToArtifactsAsync(ReviewId, content, UserId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ReviewChangeItemErrors);
            Assert.IsTrue(result.ReviewChangeItemErrors.Count() == 1);

            var firstError = result.ReviewChangeItemErrors.First();
            Assert.IsTrue(firstError.ErrorCode == ErrorCodes.ArtifactNotFound);
            Assert.IsTrue(firstError.ItemsCount == 2);
        }

        [TestMethod]
        public async Task AssignApprovalRequiredToArtifacts_SomeArtifactsAreNotInTheReview()
        {
            _propertyValueString = new PropertyValueString
            {
                IsDraftRevisionExists = true,
                ArtifactXml = "<?xml version=\"1.0\" encoding=\"utf - 16\"?><RDReviewContents xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Artifacts><CA><Id>1</Id></CA><CA><Id>2</Id></CA><CA><Id>3</Id></CA></Artifacts></RDReviewContents>",
                RevewSubartifactId = 3,
                ProjectId = ProjectId,
                LockedByUserId = UserId,
                ReviewStatus = ReviewPackageStatus.Draft,
                BaselineId = null,
                IsReviewDeleted = false,
                IsUserDisabled = false,
                ReviewType = ReviewType.Informal
            };
            var artifactIds = new List<int> { 1, 2, 3, 4, 5 };
            var content = new AssignArtifactsApprovalParameter
            {
                ItemIds = artifactIds,
                ApprovalRequired = false
            };

            var requestedArtifactIds = new List<int> { 1, 2, 3 };

            _mockArtifactVersionsRepository
                .Setup(m => m.GetDeletedAndNotInProjectItems(requestedArtifactIds, ProjectId))
                .ReturnsAsync(new List<int>());

            _mockArtifactPermissionsRepository
                .Setup(m => m.GetArtifactPermissions(requestedArtifactIds, UserId, false, int.MaxValue, true))
                .ReturnsAsync(() => new Dictionary<int, RolePermissions>
                    {
                        { requestedArtifactIds[0], RolePermissions.Read },
                        { requestedArtifactIds[1], RolePermissions.Read },
                        { requestedArtifactIds[2], RolePermissions.Read }
                    });

            // Act
            var result = await _reviewService.AssignApprovalRequiredToArtifactsAsync(ReviewId, content, UserId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ReviewChangeItemErrors);
            Assert.IsTrue(result.ReviewChangeItemErrors.Count() == 1);

            var firstError = result.ReviewChangeItemErrors.First();
            Assert.IsTrue(firstError.ErrorCode == ErrorCodes.ApprovalRequiredArtifactNotInReview);
            Assert.IsTrue(firstError.ItemsCount == 2);
        }

        [TestMethod]
        public async Task AssignApprovalRequiredToArtifacts_DeletedAndNoPermissions()
        {
            _propertyValueString = new PropertyValueString
            {
                IsDraftRevisionExists = true,
                ArtifactXml = "<?xml version=\"1.0\" encoding=\"utf - 16\"?><RDReviewContents xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Artifacts><CA><Id>1</Id></CA><CA><Id>2</Id></CA><CA><Id>3</Id></CA></Artifacts></RDReviewContents>",
                RevewSubartifactId = 3,
                ProjectId = ProjectId,
                LockedByUserId = UserId,
                ReviewStatus = ReviewPackageStatus.Draft,
                BaselineId = null,
                IsReviewDeleted = false,
                IsUserDisabled = false,
                ReviewType = ReviewType.Informal
            };

            var artifactIds = new List<int> { 1, 2, 3 };
            var content = new AssignArtifactsApprovalParameter
            {
                ItemIds = artifactIds,
                ApprovalRequired = false
            };

            var requestedArtifactIds = new List<int> { 1, 2, 3 };

            _mockArtifactVersionsRepository
                .Setup(m => m.GetDeletedAndNotInProjectItems(requestedArtifactIds, ProjectId))
                .ReturnsAsync(new List<int> { 1 });

            var permissionsArtifactIds = new List<int> { 2, 3 };

            _mockArtifactPermissionsRepository
                .Setup(m => m.GetArtifactPermissions(permissionsArtifactIds, UserId, false, int.MaxValue, true))
                .ReturnsAsync(() => new Dictionary<int, RolePermissions>
                    {
                        { permissionsArtifactIds[0], RolePermissions.Read }
                    });

            // Act
            var result = await _reviewService.AssignApprovalRequiredToArtifactsAsync(ReviewId, content, UserId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ReviewChangeItemErrors);
            Assert.IsTrue(result.ReviewChangeItemErrors.Count() == 2);

            var firstError = result.ReviewChangeItemErrors.First();
            Assert.IsTrue(firstError.ErrorCode == ErrorCodes.ArtifactNotFound);
            Assert.IsTrue(firstError.ItemsCount == 1);

            var secondError = result.ReviewChangeItemErrors.Last();
            Assert.IsTrue(secondError.ErrorCode == ErrorCodes.UnauthorizedAccess);
            Assert.IsTrue(secondError.ItemsCount == 1);
        }

        #endregion
    }
}
