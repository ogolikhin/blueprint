using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ArtifactStore.Helpers;
using ArtifactStore.Models.Review;
using ArtifactStore.Repositories;
using ArtifactStore.Services.Reviews;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using ServiceLibrary.Services;

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

        private ArtifactBasicDetails _artifactDetails;
        private Review _review;

        private int _revisionId;
        private bool _hasReadPermissions;
        private bool _hasEditPermissions;
        private bool _isLockSuccessful;
        private ReviewType _reviewType;
        private ProjectPermissions _projectPermissions;
        private Dictionary<int, List<ParticipantMeaningOfSignatureResult>> _possibleMeaningOfSignatures;
        private DateTime _currentUtcDateTime;

        [TestInitialize]
        public void Initialize()
        {
            _artifactDetails = new ArtifactBasicDetails
            {
                ItemId = ReviewId,
                ProjectId = ProjectId,
                PrimitiveItemTypePredefined = (int)ItemTypePredefined.ArtifactReviewPackage
            };

            _review = new Review(ReviewId);

            _mockReviewRepository = new Mock<IReviewsRepository>();

            _mockReviewRepository
                .Setup(m => m.GetPossibleMeaningOfSignaturesForParticipantsAsync(ReviewId, UserId, It.IsAny<IEnumerable<int>>(), It.IsAny<bool>()))
                .ReturnsAsync(() => _possibleMeaningOfSignatures);

            _mockReviewRepository
                .Setup(m => m.GetReviewTypeAsync(ReviewId, UserId, It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(() => _reviewType);

            _mockReviewRepository
                .Setup(m => m.GetReviewAsync(ReviewId, UserId, It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(() => _review);

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
                .Setup(m => m.GetRevisionId(ReviewId, UserId, null, It.IsAny<int?>()))
                .ReturnsAsync(int.MaxValue);
            _mockItemInfoRepository
                .Setup(m => m.GetRevisionId(ReviewId, UserId, It.IsNotNull<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(_revisionId);

            var mockCurrentDateTimeService = new Mock<ICurrentDateTimeService>();

            _currentUtcDateTime = new DateTime();

            mockCurrentDateTimeService.Setup(m => m.GetUtcNow()).Returns(() => _currentUtcDateTime);

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
                _mockItemInfoRepository.Object,
                mockCurrentDateTimeService.Object);
        }

        #region GetReviewSettingsAsync

        [TestMethod]
        public async Task GetReviewSettingsAsync_DeletedReviewNotFound_ThrowsResourceNotFoundException()
        {
            // Arrange
            _artifactDetails = new ArtifactBasicDetails
            {
                ItemId = ReviewId,
                ProjectId = ProjectId,
                PrimitiveItemTypePredefined = (int)ItemTypePredefined.ArtifactReviewPackage,
                LatestDeleted = true
            };

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
        public async Task GetReviewSettingsAsync_LatestVersion_ReviewDeletedInDraft_ThrowsResourceNotFoundException()
        {
            // Arrange
            _artifactDetails = new ArtifactBasicDetails
            {
                ItemId = ReviewId,
                PrimitiveItemTypePredefined = (int)ItemTypePredefined.ArtifactReviewPackage,
                DraftDeleted = true,
                LatestDeleted = false
            };

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
        public async Task GetReviewSettingsAsync_LatestVersion_ReviewDeleted_ThrowsResourceNotFoundException()
        {
            // Arrange
            _artifactDetails = new ArtifactBasicDetails
            {
                ItemId = ReviewId,
                PrimitiveItemTypePredefined = (int)ItemTypePredefined.ArtifactReviewPackage,
                DraftDeleted = false,
                LatestDeleted = true
            };

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
        public async Task GetReviewSettingsAsync_HistoricalVersion_ReviewDeletedInDraft_ReturnsHistoricalReviewSettings()
        {
            // Arrange
            _revisionId = 1;
            _artifactDetails = new ArtifactBasicDetails
            {
                ItemId = ReviewId,
                PrimitiveItemTypePredefined = (int)ItemTypePredefined.ArtifactReviewPackage,
                DraftDeleted = true,
                LatestDeleted = false
            };
            _review.ReviewPackageRawData.IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed = true;
            _review.ReviewPackageRawData.ShowOnlyDescription = true;

            // Act
            var settings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId, _revisionId);

            // Assert
            Assert.IsNotNull(settings);
            Assert.IsNotNull(settings);
            Assert.AreEqual(_review.ReviewPackageRawData.IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed, settings.CanMarkAsComplete);
            Assert.AreEqual(_review.ReviewPackageRawData.ShowOnlyDescription, settings.ShowOnlyDescription);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_HistoricalVersion_ReviewDeleted_ReturnsHistoricalReviewSettings()
        {
            // Arrange
            _revisionId = 1;
            _artifactDetails = new ArtifactBasicDetails
            {
                ItemId = ReviewId,
                PrimitiveItemTypePredefined = (int)ItemTypePredefined.ArtifactReviewPackage,
                DraftDeleted = false,
                LatestDeleted = true
            };
            _review.ReviewPackageRawData.IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed = true;
            _review.ReviewPackageRawData.ShowOnlyDescription = true;

            // Act
            var settings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId, _revisionId);

            // Assert
            Assert.IsNotNull(settings);
            Assert.AreEqual(_review.ReviewPackageRawData.IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed, settings.CanMarkAsComplete);
            Assert.AreEqual(_review.ReviewPackageRawData.ShowOnlyDescription, settings.ShowOnlyDescription);
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
            _review.ReviewPackageRawData.ShowOnlyDescription = false;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(_review.ReviewPackageRawData.ShowOnlyDescription, reviewSettings.ShowOnlyDescription);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_ReviewPackageRawDataShowOnlyDescriptionIsTrue_ReviewSettingsShowOnlyDescriptionIsTrue()
        {
            // Arrange
            _review.ReviewPackageRawData.ShowOnlyDescription = true;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(_review.ReviewPackageRawData.ShowOnlyDescription, reviewSettings.ShowOnlyDescription);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_ReviewPackageRawDataIsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewedIsFalse_ReviewSettingsCanMarkAsCompleteIsFalse()
        {
            // Arrange
            _review.ReviewPackageRawData.IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed = false;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(_review.ReviewPackageRawData.IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed, reviewSettings.CanMarkAsComplete);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_ReviewPackageRawDataIsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewedIsTrue_ReviewSettingsCanMarkAsCompleteIsTrue()
        {
            // Arrange
            _review.ReviewPackageRawData.IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed = true;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(_review.ReviewPackageRawData.IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed, reviewSettings.CanMarkAsComplete);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_ReviewPackageRawDataIsESignatureEnabledIsFalse_ReviewSettingsRequireESignatureIsFalse()
        {
            // Arrange
            _review.ReviewPackageRawData.IsESignatureEnabled = false;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(_review.ReviewPackageRawData.IsESignatureEnabled, reviewSettings.RequireESignature);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_ReviewPackageRawDataIsESignatureEnabledIsTrue_ReviewSettingsRequireESignatureIsTrue()
        {
            // Arrange
            _review.ReviewPackageRawData.IsESignatureEnabled = true;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(_review.ReviewPackageRawData.IsESignatureEnabled, reviewSettings.RequireESignature);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_ReviewPackageRawDataIsMoSEnabledIsFalse_ReviewSettingsRequireMeaningOfSignatureIsFalse()
        {
            // Arrange
            _review.ReviewPackageRawData.IsMoSEnabled = false;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(_review.ReviewPackageRawData.IsMoSEnabled, reviewSettings.RequireMeaningOfSignature);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_ReviewPackageRawDataIsMoSEnabledIsTrue_ReviewSettingsRequireMeaningOfSignatureIsTrue()
        {
            // Arrange
            _review.ReviewPackageRawData.IsMoSEnabled = true;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(_review.ReviewPackageRawData.IsMoSEnabled, reviewSettings.RequireMeaningOfSignature);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_PublicDraftReview_CanEditRequireESignatureIsTrue()
        {
            // Arrange
            SetReviewType(ReviewType.Public);
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Draft;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(true, reviewSettings.CanEditRequireESignature);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_PublicActiveReview_CanEditRequireESignatureIsTrue()
        {
            // Arrange
            SetReviewType(ReviewType.Public);
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Active;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(true, reviewSettings.CanEditRequireESignature);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_InformalDraftReview_CanEditRequireESignatureIsTrue()
        {
            // Arrange
            SetReviewType(ReviewType.Informal);
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Draft;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(true, reviewSettings.CanEditRequireESignature);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_InformalActiveReview_CanEditRequireESignatureIsTrue()
        {
            // Arrange
            SetReviewType(ReviewType.Informal);
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Active;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(true, reviewSettings.CanEditRequireESignature);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_FormalDraftReview_CanEditRequireESignatureIsTrue()
        {
            // Arrange
            SetReviewType(ReviewType.Formal);
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Draft;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(true, reviewSettings.CanEditRequireESignature);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_FormalActiveReview_CanEditRequireESignatureIsFalse()
        {
            // Arrange
            // Make formal review
            _review.Contents.Artifacts = new List<RDArtifact>
            {
                new RDArtifact
                {
                    Id = 1,
                    ApprovalNotRequested = null
                }
            };
            _review.ReviewPackageRawData.Reviewers = _review.ReviewPackageRawData.Reviewers = new List<ReviewerRawData>
            {
                new ReviewerRawData
                {
                    UserId = 2,
                    Permission = ReviewParticipantRole.Approver
                }
            };

            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Active;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(false, reviewSettings.CanEditRequireESignature);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_PublicClosedReview_CanEditRequireESignatureIsFalse()
        {
            // Arrange
            SetReviewType(ReviewType.Public);
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Closed;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(false, reviewSettings.CanEditRequireESignature);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_InformalClosedReview_CanEditRequireESignatureIsFalse()
        {
            // Arrange
            SetReviewType(ReviewType.Informal);
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Closed;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(false, reviewSettings.CanEditRequireESignature);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_FormalClosedReview_CanEditRequireESignatureIsFalse()
        {
            // Arrange
            SetReviewType(ReviewType.Formal);
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Closed;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(false, reviewSettings.CanEditRequireESignature);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_ClosedFormalReview_CanEditRequireMeaningOfSignatureIsFalse()
        {
            // Arrange
            SetReviewType(ReviewType.Formal);
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Closed;
            _projectPermissions = ProjectPermissions.IsMeaningOfSignatureEnabled;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(false, reviewSettings.CanEditRequireMeaningOfSignature);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_ClosedPublicReview_CanEditRequireMeaningOfSignatureIsFalse()
        {
            // Arrange
            SetReviewType(ReviewType.Public);
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Closed;
            _projectPermissions = ProjectPermissions.IsMeaningOfSignatureEnabled;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(false, reviewSettings.CanEditRequireMeaningOfSignature);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_ClosedInformalReview_CanEditRequireMeaningOfSignatureIsFalse()
        {
            // Arrange
            SetReviewType(ReviewType.Informal);
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Closed;
            _projectPermissions = ProjectPermissions.IsMeaningOfSignatureEnabled;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(false, reviewSettings.CanEditRequireMeaningOfSignature);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_ActiveFormalReview_CanEditRequireMeaningOfSignatureIsFalse()
        {
            // Arrange
            // Make formal review
            _review.Contents.Artifacts = new List<RDArtifact>
            {
                new RDArtifact
                {
                    Id = 1,
                    ApprovalNotRequested = null
                }
            };
            _review.ReviewPackageRawData.Reviewers = _review.ReviewPackageRawData.Reviewers = new List<ReviewerRawData>
            {
                new ReviewerRawData
                {
                    UserId = 2,
                    Permission = ReviewParticipantRole.Approver
                }
            };
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Active;
            _projectPermissions = ProjectPermissions.IsMeaningOfSignatureEnabled;
            SetReviewType(ReviewType.Formal);

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(false, reviewSettings.CanEditRequireMeaningOfSignature);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_MeaningOfSignatureDisabledInProject_CanEditRequireMeaningOfSignatureIsFalse()
        {
            // Arrange
            SetReviewType(ReviewType.Formal);
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Draft;
            _projectPermissions = ProjectPermissions.None;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(false, reviewSettings.CanEditRequireMeaningOfSignature);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_MeaningOfSignatureEnabledInProject_CanEditRequireMeaningOfSignatureIsTrue()
        {
            // Arrange
            SetReviewType(ReviewType.Formal);
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Draft;
            _projectPermissions = ProjectPermissions.IsMeaningOfSignatureEnabled;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(true, reviewSettings.CanEditRequireMeaningOfSignature);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_MeaningOfSignatureDisabledInProject_IsMeaningOfSignatureEnabledInProjectIsFalse()
        {
            // Arrange
            SetReviewType(ReviewType.Formal);
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Draft;
            _projectPermissions = ProjectPermissions.None;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(false, reviewSettings.IsMeaningOfSignatureEnabledInProject);
        }

        [TestMethod]
        public async Task GetReviewSettingsAsync_MeaningOfSignatureEnabledInProject_IsMeaningOfSignatureEnabledInProjectIsTrue()
        {
            // Arrange
            SetReviewType(ReviewType.Formal);
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Draft;
            _projectPermissions = ProjectPermissions.IsMeaningOfSignatureEnabled;

            // Act
            var reviewSettings = await _reviewService.GetReviewSettingsAsync(ReviewId, UserId);

            // Assert
            Assert.AreEqual(true, reviewSettings.IsMeaningOfSignatureEnabledInProject);
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
                await _reviewService.UpdateReviewSettingsAsync(ReviewId, new ReviewSettings(), false, UserId);
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
                await _reviewService.UpdateReviewSettingsAsync(ReviewId, new ReviewSettings(), false, UserId);
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
                await _reviewService.UpdateReviewSettingsAsync(ReviewId, new ReviewSettings(), false, UserId);
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
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Closed;

            // Act
            try
            {
                await _reviewService.UpdateReviewSettingsAsync(ReviewId, new ReviewSettings(), false, UserId);
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
                await _reviewService.UpdateReviewSettingsAsync(ReviewId, new ReviewSettings(), false, UserId);
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
        public async Task UpdateReviewSettingsAsync_ReviewLockedBySameUser_UpdatesSettings()
        {
            // Arrange
            _artifactDetails.LockedByUserId = UserId;
            _review.ReviewPackageRawData.IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed = false;
            var updatedSettings = new ReviewSettings { CanMarkAsComplete = true };

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedSettings, false, UserId);

            // Assert
            _mockLockArtifactsRepository.Verify(m => m.LockArtifactAsync(ReviewId, UserId), Times.Never);
            Assert.AreEqual(true, _review.ReviewPackageRawData.IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_ReviewLockedByAnotherUserWhenLocking_ThrowsConflictException()
        {
            // Arrange
            _isLockSuccessful = false;

            // Act
            try
            {
                await _reviewService.UpdateReviewSettingsAsync(ReviewId, new ReviewSettings(), false, UserId);
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
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, new ReviewSettings(), false, UserId);

            // Assert
            _mockLockArtifactsRepository.Verify(m => m.LockArtifactAsync(ReviewId, UserId));
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_EndDateChanged_ReviewIsDraft_UpdatesSetting()
        {
            // Arrange
            var updatedEndDate = new DateTime(2020, 11, 15);
            var updatedReviewSettings = new ReviewSettings { EndDate = updatedEndDate };
            _review.ReviewPackageRawData.EndDate = null;
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Draft;

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, false, UserId);

            // Assert
            Assert.AreEqual(updatedEndDate, _review.ReviewPackageRawData.EndDate);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_EndDateChanged_ReviewIsActive_UpdatesSetting()
        {
            // Arrange
            var updatedEndDate = new DateTime(2019, 10, 11, 5, 24, 0);
            var updatedReviewSettings = new ReviewSettings { EndDate = updatedEndDate };
            _review.ReviewPackageRawData.EndDate = null;
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Active;

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, false, UserId);

            // Assert
            Assert.AreEqual(updatedEndDate, _review.ReviewPackageRawData.EndDate);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_EndDateNotChanged_DoesNotUpdateSetting()
        {
            // Arrange
            var updatedEndDate = new DateTime(2017, 12, 24, 7, 0, 0);
            var updatedReviewSettings = new ReviewSettings { EndDate = updatedEndDate };
            _review.ReviewPackageRawData.EndDate = updatedEndDate;
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Active;

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, false, UserId);

            // Assert
            Assert.AreEqual(_review.ReviewPackageRawData.EndDate, updatedReviewSettings.EndDate);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_ShowOnlyDescriptionChanged_ReviewIsDraft_UpdatesSetting()
        {
            // Arrange
            var updatedReviewSettings = new ReviewSettings { ShowOnlyDescription = true };
            _review.ReviewPackageRawData.ShowOnlyDescription = false;
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Draft;

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, false, UserId);

            // Assert
            Assert.AreEqual(true, _review.ReviewPackageRawData.ShowOnlyDescription);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_ShowOnlyDescriptionChanged_ReviewIsActive_UpdatesSetting()
        {
            // Arrange
            var updatedReviewSettings = new ReviewSettings { ShowOnlyDescription = true };
            _review.ReviewPackageRawData.ShowOnlyDescription = false;
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Active;

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, false, UserId);

            // Assert
            Assert.AreEqual(true, _review.ReviewPackageRawData.ShowOnlyDescription);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_ShowOnlyDescriptionNotChanged_DoesNotUpdateSetting()
        {
            // Arrange
            var updatedReviewSettings = new ReviewSettings { ShowOnlyDescription = true };
            _review.ReviewPackageRawData.ShowOnlyDescription = true;
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Active;

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, false, UserId);

            // Assert
            Assert.AreEqual(_review.ReviewPackageRawData.ShowOnlyDescription, updatedReviewSettings.ShowOnlyDescription);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_CanMarkAsCompleteChanged_ReviewIsDraft_UpdatesSetting()
        {
            // Arrange
            _review.ReviewPackageRawData.IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed = false;
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Draft;
            var updatedReviewSettings = new ReviewSettings { CanMarkAsComplete = true };

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, false, UserId);

            // Assert
            Assert.AreEqual(true, _review.ReviewPackageRawData.IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_CanMarkAsCompleteNotChanged_ReviewIsActive_DoesNotUpdateSetting()
        {
            // Arrange
            _review.ReviewPackageRawData.IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed = true;
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Active;
            var updatedReviewSettings = new ReviewSettings { CanMarkAsComplete = true };

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, false, UserId);

            // Assert
            Assert.AreEqual(_review.ReviewPackageRawData.IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed, updatedReviewSettings.CanMarkAsComplete);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_CanMarkAsCompleteChanged_ReviewIsActive_ThrowsConflictException()
        {
            // Arrange
            _review.ReviewPackageRawData.IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed = false;
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Active;
            var updatedReviewSettings = new ReviewSettings { CanMarkAsComplete = true };

            // Act
            try
            {
                await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, false, UserId);
            }
            catch (ConflictException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.Conflict, ex.ErrorCode);
                Assert.AreEqual(I18NHelper.FormatInvariant(ErrorMessages.ReviewIsNotDraft, ReviewId, UserId), ex.Message);
                Assert.AreEqual(false, _review.ReviewPackageRawData.IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed);
                return;
            }

            Assert.Fail("Expected ConflictException to have been thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_RequireESignatureIsFalse_OriginalValueIsNull_DoesNotUpdateSetting()
        {
            // Arrange
            _review.ReviewPackageRawData.IsESignatureEnabled = null;
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Draft;
            var updatedReviewSettings = new ReviewSettings { RequireESignature = false };

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, false, UserId);

            // Assert
            Assert.IsNull(_review.ReviewPackageRawData.IsESignatureEnabled);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_RequireESignatureIsTrue_OriginalValueIsNull_UpdatesSetting()
        {
            // Arrange
            _review.ReviewPackageRawData.IsESignatureEnabled = null;
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Draft;
            var updatedReviewSettings = new ReviewSettings { RequireESignature = true };

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, false, UserId);

            // Assert
            Assert.AreEqual(true, _review.ReviewPackageRawData.IsESignatureEnabled);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_RequireESignatureIsFalse_OriginalValueIsFalse_DoesNotUpdateSetting()
        {
            // Arrange
            _review.ReviewPackageRawData.IsESignatureEnabled = false;
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Draft;
            var updatedReviewSettings = new ReviewSettings { RequireESignature = false };

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, false, UserId);

            // Assert
            Assert.AreEqual(false, _review.ReviewPackageRawData.IsESignatureEnabled);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_RequireESignatureIsTrue_OriginalValueIsFalse_UpdatesSetting()
        {
            // Arrange
            _review.ReviewPackageRawData.IsESignatureEnabled = false;
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Draft;
            var updatedReviewSettings = new ReviewSettings { RequireESignature = true };

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, false, UserId);

            // Assert
            Assert.AreEqual(true, _review.ReviewPackageRawData.IsESignatureEnabled);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_RequireESignatureIsFalse_OriginalValueIsTrue_UpdateSetting()
        {
            // Arrange
            _review.ReviewPackageRawData.IsESignatureEnabled = true;
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Draft;
            var updatedReviewSettings = new ReviewSettings { RequireESignature = false };

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, false, UserId);

            // Assert
            Assert.AreEqual(false, _review.ReviewPackageRawData.IsESignatureEnabled);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_RequireESignatureIsTrue_OriginalValueIsTrue_DoesNotUpdateSetting()
        {
            // Arrange
            _review.ReviewPackageRawData.IsESignatureEnabled = true;
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Draft;
            var updatedReviewSettings = new ReviewSettings { RequireESignature = true };

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, false, UserId);

            // Assert
            Assert.AreEqual(true, _review.ReviewPackageRawData.IsESignatureEnabled);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_RequireESignatureChanged_PublicDraftReview_UpdatesSetting()
        {
            // Arrange
            _review.ReviewPackageRawData.IsESignatureEnabled = false;
            SetReviewType(ReviewType.Public);
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Draft;
            var updatedReviewSettings = new ReviewSettings { RequireESignature = true };

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, false, UserId);

            // Assert
            Assert.AreEqual(true, _review.ReviewPackageRawData.IsESignatureEnabled);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_RequireESignatureChanged_PublicActiveReview_UpdatesSetting()
        {
            // Arrange
            _review.ReviewPackageRawData.IsESignatureEnabled = false;
            SetReviewType(ReviewType.Public);
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Active;
            var updatedReviewSettings = new ReviewSettings { RequireESignature = true };

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, false, UserId);

            // Assert
            Assert.AreEqual(true, _review.ReviewPackageRawData.IsESignatureEnabled);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_RequireESignatureChanged_InformalDraftReview_UpdatesSetting()
        {
            // Arrange
            _review.ReviewPackageRawData.IsESignatureEnabled = false;
            SetReviewType(ReviewType.Informal);
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Draft;
            var updatedReviewSettings = new ReviewSettings { RequireESignature = true };

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, false, UserId);

            // Assert
            Assert.AreEqual(true, _review.ReviewPackageRawData.IsESignatureEnabled);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_RequireESignatureChanged_InformalActiveReview_UpdatesSetting()
        {
            // Arrange
            _review.ReviewPackageRawData.IsESignatureEnabled = false;
            SetReviewType(ReviewType.Informal);
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Active;
            var updatedReviewSettings = new ReviewSettings { RequireESignature = true };

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, false, UserId);

            // Assert
            Assert.AreEqual(true, _review.ReviewPackageRawData.IsESignatureEnabled);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_RequireESignatureChanged_FormalDraftReview_UpdatesSetting()
        {
            // Arrange
            _review.ReviewPackageRawData.IsESignatureEnabled = false;
            SetReviewType(ReviewType.Formal);
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Draft;
            var updatedReviewSettings = new ReviewSettings { RequireESignature = true };

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, false, UserId);

            // Assert
            Assert.AreEqual(true, _review.ReviewPackageRawData.IsESignatureEnabled);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_RequireESignatureChanged_FormalActiveReview_ThrowsConflictException()
        {
            // Arrange
            _review.ReviewPackageRawData.IsESignatureEnabled = false;
            SetReviewType(ReviewType.Formal);
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Active;
            var updatedReviewSettings = new ReviewSettings { RequireESignature = true };

            // Act
            try
            {
                await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, false, UserId);
            }
            catch (ConflictException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.ReviewActive, ex.ErrorCode);
                Assert.AreEqual(ErrorMessages.ReviewActiveFormal, ex.Message);
                return;
            }

            Assert.Fail("Expected ConflictException to have been thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_RequireMeaningOfSignatureChanged_FormalActiveReview_ThrowsConflictException()
        {
            // Arrange
            _review.ReviewPackageRawData.IsESignatureEnabled = true;
            _review.ReviewPackageRawData.IsMoSEnabled = false;
            SetReviewType(ReviewType.Formal);
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Active;
            var updatedReviewSettings = new ReviewSettings { RequireESignature = true, RequireMeaningOfSignature = true };

            // Act
            try
            {
                await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, false, UserId);
            }
            catch (ConflictException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.ReviewActive, ex.ErrorCode);
                Assert.AreEqual(ErrorMessages.ReviewActiveFormal, ex.Message);
                return;
            }

            Assert.Fail("Expected ConflictException to have been thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_RequireMeaningOfSignatureNotChanged_DoesNotUpdateSetting()
        {
            // Arrange
            _review.ReviewPackageRawData.IsMoSEnabled = false;
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Draft;
            var updatedReviewSettings = new ReviewSettings { RequireMeaningOfSignature = false };

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, false, UserId);

            // Assert
            Assert.AreEqual(false, _review.ReviewPackageRawData.IsMoSEnabled);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_RequireMeaningOfSignatureSetToTrue_RequireESignatureIsNull_ThrowsConflictException()
        {
            // Arrange
            SetReviewType(ReviewType.Formal);
            _review.ReviewPackageRawData.IsMoSEnabled = false;
            _review.ReviewPackageRawData.IsESignatureEnabled = null;
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Draft;
            var updatedReviewSettings = new ReviewSettings { RequireMeaningOfSignature = true };

            // Act
            try
            {
                await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, false, UserId);
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
        public async Task UpdateReviewSettingsAsync_RequireMeaningOfSignatureSetToTrue_RequireESignatureIsFalse_ThrowsConflictException()
        {
            // Arrange
            SetReviewType(ReviewType.Formal);
            _review.ReviewPackageRawData.IsMoSEnabled = false;
            _review.ReviewPackageRawData.IsESignatureEnabled = false;
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Draft;
            var updatedReviewSettings = new ReviewSettings { RequireMeaningOfSignature = true };

            // Act
            try
            {
                await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, false, UserId);
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
        public async Task UpdateReviewSettingsAsync_RequireMeaningOfSignatureSetToFalse_RequireESignatureIsNull_DoesNotThrowException()
        {
            // Arrange
            SetReviewType(ReviewType.Formal);
            _review.ReviewPackageRawData.IsMoSEnabled = true;
            _review.ReviewPackageRawData.IsESignatureEnabled = null;
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Draft;
            var updatedReviewSettings = new ReviewSettings { RequireMeaningOfSignature = false };

            _mockArtifactPermissionsRepository
                .Setup(m => m.GetProjectPermissions(_artifactDetails.ProjectId))
                .ReturnsAsync(ProjectPermissions.IsMeaningOfSignatureEnabled);

            // Act
            try
            {
                await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, false, UserId);
            }
            catch (Exception ex)
            {
                // Assert
                Assert.Fail("Unexpected exception thrown: {0}!", ex);
            }
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_RequireMeaningOfSignatureSetToFalse_RequireESignatureIsFalse_DoesNotThrowException()
        {
            // Arrange
            SetReviewType(ReviewType.Formal);
            _review.ReviewPackageRawData.IsMoSEnabled = true;
            _review.ReviewPackageRawData.IsESignatureEnabled = null;
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Draft;
            var updatedReviewSettings = new ReviewSettings { RequireMeaningOfSignature = false };

            _mockArtifactPermissionsRepository
                .Setup(m => m.GetProjectPermissions(_artifactDetails.ProjectId))
                .ReturnsAsync(ProjectPermissions.IsMeaningOfSignatureEnabled);

            // Act
            try
            {
                await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, false, UserId);
            }
            catch (Exception ex)
            {
                // Assert
                Assert.Fail("Unexpected exception thrown: {0}!", ex);
            }
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_RequireMeaningOfSignatureSetToTrue_RequireESignatureIsTrue_DoesNotThrowException()
        {
            // Arrange
            SetReviewType(ReviewType.Informal);
            _review.ReviewPackageRawData.IsMoSEnabled = false;
            _review.ReviewPackageRawData.IsESignatureEnabled = true;
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Active;
            _review.ReviewPackageRawData.Reviewers = new List<ReviewerRawData>();

            var updatedReviewSettings = new ReviewSettings { RequireESignature = true, RequireMeaningOfSignature = true };

            _mockArtifactPermissionsRepository
                .Setup(m => m.GetProjectPermissions(_artifactDetails.ProjectId))
                .ReturnsAsync(ProjectPermissions.IsMeaningOfSignatureEnabled);

            // Act
            try
            {
                await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, false, UserId);
            }
            catch (Exception ex)
            {
                // Assert
                Assert.Fail("Unexpected exception thrown: {0}!", ex);
            }
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_RequireMeaningOfSignatureSetToFalse_RequireESignatureIsTrue_DoesNotThrowException()
        {
            // Arrange
            SetReviewType(ReviewType.Formal);
            _review.ReviewPackageRawData.IsMoSEnabled = true;
            _review.ReviewPackageRawData.IsESignatureEnabled = true;
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Draft;
            var updatedReviewSettings = new ReviewSettings { RequireESignature = true, RequireMeaningOfSignature = false };

            _mockArtifactPermissionsRepository
                .Setup(m => m.GetProjectPermissions(_artifactDetails.ProjectId))
                .ReturnsAsync(ProjectPermissions.IsMeaningOfSignatureEnabled);

            // Act
            try
            {
                await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, false, UserId);
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
            SetReviewType(ReviewType.Formal);
            _review.ReviewPackageRawData.IsMoSEnabled = false;
            _review.ReviewPackageRawData.IsESignatureEnabled = true;
            _projectPermissions = ProjectPermissions.None;
            var updatedReviewSettings = new ReviewSettings { RequireESignature = true, RequireMeaningOfSignature = true };

            // Act
            try
            {
                await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, false, UserId);
            }
            catch (ConflictException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.Conflict, ex.ErrorCode);
                Assert.AreEqual(I18NHelper.FormatInvariant(ErrorMessages.MeaningOfSignatureDisabledInProject, ReviewId), ex.Message);
                return;
            }

            // Assert
            Assert.Fail("Expected ConflictException to have been thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_RequireMeaningOfSignatureChanged_AllConditionsSatisfied_UpdatesSetting()
        {
            // Arrange
            SetReviewType(ReviewType.Formal);
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Draft;
            _review.ReviewPackageRawData.IsMoSEnabled = false;
            _review.ReviewPackageRawData.IsESignatureEnabled = true;
            _review.ReviewPackageRawData.Reviewers = new List<ReviewerRawData>();
            _projectPermissions = ProjectPermissions.IsMeaningOfSignatureEnabled;

            var updatedReviewSettings = new ReviewSettings
            {
                RequireESignature = true,
                RequireMeaningOfSignature = true
            };

            // Act
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, false, UserId);

            // Assert
            Assert.AreEqual(true, _review.ReviewPackageRawData.IsMoSEnabled);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_RequireMeaningOfSignatureSetToFalse_Should_Not_Set_Default_Meaning_Of_Signature_For_All_Approvers()
        {
            // Arrange
            SetReviewType(ReviewType.Formal);
            _review.ReviewPackageRawData.IsMoSEnabled = true;
            _review.ReviewPackageRawData.IsESignatureEnabled = true;
            _review.ReviewPackageRawData.Reviewers = new List<ReviewerRawData>
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
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, false, UserId);

            // Assert
            var approver = _review.ReviewPackageRawData.Reviewers.First(r => r.Permission == ReviewParticipantRole.Approver);
            var reviewer = _review.ReviewPackageRawData.Reviewers.First(r => r.Permission == ReviewParticipantRole.Reviewer);

            Assert.IsNull(reviewer.SelectedRoleMoSAssignments);
            Assert.IsNull(approver.SelectedRoleMoSAssignments);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_RequireMeaningOfSignatureChanged_Should_Set_Default_Meaning_Of_Signature_For_All_Approvers()
        {
            // Arrange
            SetReviewType(ReviewType.Formal);
            _review.ReviewPackageRawData.IsMoSEnabled = false;
            _review.ReviewPackageRawData.IsESignatureEnabled = true;
            _review.ReviewPackageRawData.Reviewers = new List<ReviewerRawData>
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
                        new ParticipantMeaningOfSignatureResult { RoleId = 5 },
                        new ParticipantMeaningOfSignatureResult { RoleId = 6 }
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
            await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, false, UserId);

            // Assert
            var approver = _review.ReviewPackageRawData.Reviewers.First(r => r.Permission == ReviewParticipantRole.Approver);
            var reviewer = _review.ReviewPackageRawData.Reviewers.First(r => r.Permission == ReviewParticipantRole.Reviewer);

            Assert.IsNull(reviewer.SelectedRoleMoSAssignments);

            Assert.AreEqual(2, approver.SelectedRoleMoSAssignments.Count);
            Assert.IsTrue(approver.SelectedRoleMoSAssignments.All(mos => mos.RoleId == 5 || mos.RoleId == 6));
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_Should_Throw_When_Expiry_Date_Is_In_The_Past()
        {
            // Arrange
            _currentUtcDateTime = new DateTime(2017, 12, 11, 10, 56, 07);

            var updatedReviewSettings = new ReviewSettings
            {
                EndDate = new DateTime(2017, 12, 11, 10, 56, 07)
            };

            _mockArtifactPermissionsRepository
                .Setup(m => m.GetProjectPermissions(_artifactDetails.ProjectId))
                .ReturnsAsync(ProjectPermissions.IsMeaningOfSignatureEnabled);

            // Act
            try
            {
                await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, false, UserId);
            }
            catch (ConflictException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.ReviewExpired, ex.ErrorCode);

                return;
            }

            Assert.Fail("A ConflictException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_Should_Not_Update_ExpiryDate_When_Autosave_Is_True_And_ExpiryDate_Is_Invalid()
        {
            // Arrange
            _currentUtcDateTime = new DateTime(2017, 12, 11, 10, 56, 07);

            var updatedReviewSettings = new ReviewSettings
            {
                EndDate = new DateTime(2017, 12, 11, 10, 56, 07)
            };

            _mockArtifactPermissionsRepository
                .Setup(m => m.GetProjectPermissions(_artifactDetails.ProjectId))
                .ReturnsAsync(ProjectPermissions.IsMeaningOfSignatureEnabled);

            // Act
            var result = await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, true, UserId);

            // Assert
            Assert.AreEqual(null, result.EndDate);
        }

        [TestMethod]
        public async Task UpdateReviewSettingsAsync_Should_Return_Updated_Settings()
        {
            // Arrange
            _currentUtcDateTime = new DateTime(2017, 12, 11, 10, 56, 07);

            var updatedReviewSettings = new ReviewSettings
            {
                EndDate = new DateTime(2017, 12, 11, 10, 56, 08),
                CanMarkAsComplete = true,
                IgnoreFolders = false,
                RequireESignature = true,
                RequireMeaningOfSignature = false,
                ShowOnlyDescription = true
            };

            _mockArtifactPermissionsRepository
                .Setup(m => m.GetProjectPermissions(_artifactDetails.ProjectId))
                .ReturnsAsync(ProjectPermissions.IsMeaningOfSignatureEnabled);

            // Act
            var result = await _reviewService.UpdateReviewSettingsAsync(ReviewId, updatedReviewSettings, true, UserId);

            // Assert
            Assert.AreEqual(updatedReviewSettings.EndDate, result.EndDate);
            Assert.AreEqual(updatedReviewSettings.CanMarkAsComplete, result.CanMarkAsComplete);
            Assert.AreEqual(updatedReviewSettings.IgnoreFolders, result.IgnoreFolders);
            Assert.AreEqual(updatedReviewSettings.RequireESignature, result.RequireESignature);
            Assert.AreEqual(updatedReviewSettings.RequireMeaningOfSignature, result.RequireMeaningOfSignature);
            Assert.AreEqual(updatedReviewSettings.ShowOnlyDescription, result.ShowOnlyDescription);
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
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Closed;

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
            _review.ReviewPackageRawData.IsMoSEnabled = false;

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
            _review.ReviewPackageRawData.IsMoSEnabled = true;
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
            _review.ReviewPackageRawData.IsMoSEnabled = true;
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
        public async Task UpdateMeaningOfSignaturesAsync_Should_Throw_When_Participant_Is_Not_In_Review_Case_Reviewers_Is_Null()
        {
            // Arrange
            _review.ReviewPackageRawData.IsMoSEnabled = true;
            _review.ReviewPackageRawData.Reviewers = null;

            var meaningOfSignatureParameter = new MeaningOfSignatureParameter
            {
                Adding = true,
                RoleId = 3,
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
        public async Task UpdateMeaningOfSignaturesAsync_Should_Throw_When_Participant_Is_Not_In_Review()
        {
            // Arrange
            _review.ReviewPackageRawData.IsMoSEnabled = true;
            _review.ReviewPackageRawData.Reviewers = new List<ReviewerRawData>();

            var meaningOfSignatureParameter = new MeaningOfSignatureParameter
            {
                Adding = true,
                RoleId = 3,
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
            _review.ReviewPackageRawData.IsMoSEnabled = true;
            _review.ReviewPackageRawData.Reviewers = new List<ReviewerRawData>
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
                RoleId = 3,
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
            _review.ReviewPackageRawData.IsMoSEnabled = true;
            _review.ReviewPackageRawData.Reviewers = new List<ReviewerRawData>
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
                RoleId = 3,
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
            _review.ReviewPackageRawData.IsMoSEnabled = true;
            _review.ReviewPackageRawData.Reviewers = new List<ReviewerRawData>
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
                RoleId = 3,
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
            _review.ReviewPackageRawData.IsMoSEnabled = true;
            _review.ReviewPackageRawData.Reviewers = new List<ReviewerRawData>
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
                RoleId = 8,
                ParticipantId = 4
            };

            // Act
            await _reviewService.UpdateMeaningOfSignaturesAsync(ReviewId, new[] { meaningOfSignatureParameter }, UserId);

            // Assert
            var result = _review.ReviewPackageRawData.Reviewers.First().SelectedRoleMoSAssignments.FirstOrDefault();

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
            _review.ReviewPackageRawData.IsMoSEnabled = true;
            _review.ReviewPackageRawData.Reviewers = new List<ReviewerRawData>
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
                RoleId = 8,
                ParticipantId = 4
            };

            // Act
            await _reviewService.UpdateMeaningOfSignaturesAsync(ReviewId, new[] { meaningOfSignatureParameter }, UserId);

            // Assert
            var result = _review.ReviewPackageRawData.Reviewers.First().SelectedRoleMoSAssignments.FirstOrDefault();

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
            _review.ReviewPackageRawData.IsMoSEnabled = true;
            _review.ReviewPackageRawData.Reviewers = new List<ReviewerRawData>
            {
                new ReviewerRawData
                {
                    UserId = 4,
                    Permission = ReviewParticipantRole.Approver,
                    SelectedRoleMoSAssignments = new List<ParticipantMeaningOfSignature>
                    {
                        new ParticipantMeaningOfSignature
                        {
                            RoleId = 8
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
                RoleId = 8,
                ParticipantId = 4
            };

            // Act
            await _reviewService.UpdateMeaningOfSignaturesAsync(ReviewId, new[] { meaningOfSignatureParameter }, UserId);

            // Assert
            var selectedMos = _review.ReviewPackageRawData.Reviewers.First().SelectedRoleMoSAssignments;
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
            _review.ReviewPackageRawData.IsMoSEnabled = true;
            _review.ReviewPackageRawData.Reviewers = new List<ReviewerRawData>
            {
                new ReviewerRawData
                {
                    UserId = 4,
                    Permission = ReviewParticipantRole.Approver,
                    SelectedRoleMoSAssignments = new List<ParticipantMeaningOfSignature>
                    {
                        new ParticipantMeaningOfSignature
                        {
                            RoleId = 8
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
                RoleId = 8,
                ParticipantId = 4
            };

            // Act
            await _reviewService.UpdateMeaningOfSignaturesAsync(ReviewId, new[] { meaningOfSignatureParameter }, UserId);

            // Assert
            var selectedMos = _review.ReviewPackageRawData.Reviewers.First().SelectedRoleMoSAssignments;

            Assert.AreEqual(0, selectedMos.Count, "There should be no meaning of signature");
        }

        [TestMethod]
        public async Task UpdateMeaningOfSignaturesAsync_Should_Not_Remove_Meaning_Of_Signature_If_It_Doesnt_Already_Exist()
        {
            // Arrange
            _review.ReviewPackageRawData.IsMoSEnabled = true;
            _review.ReviewPackageRawData.Reviewers = new List<ReviewerRawData>
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
                RoleId = 8,
                ParticipantId = 4
            };

            // Act
            await _reviewService.UpdateMeaningOfSignaturesAsync(ReviewId, new[] { meaningOfSignatureParameter }, UserId);

            // Assert
            var selectedMos = _review.ReviewPackageRawData.Reviewers.First().SelectedRoleMoSAssignments;

            Assert.AreEqual(1, selectedMos.Count, "There should be one meaning of signature");
        }

        [TestMethod]
        public async Task UpdateMeaningOfSignaturesAsync_Should_Call_UpdateReviewPackage()
        {
            // Arrange
            _review.ReviewPackageRawData.IsMoSEnabled = true;
            _review.ReviewPackageRawData.Reviewers = new List<ReviewerRawData>
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
                RoleId = 8,
                ParticipantId = 4
            };

            // Act
            await _reviewService.UpdateMeaningOfSignaturesAsync(ReviewId, new[] { meaningOfSignatureParameter }, UserId);

            // Assert
            _mockReviewRepository.Verify(repo => repo.UpdateReviewPackageRawDataAsync(ReviewId, _review.ReviewPackageRawData, 2));
        }

        #endregion

        #region AssignRoleToParticipantAsync

        [TestMethod]
        public void AssignApprovalRequiredToArtifacts_Should_Not_Serialize_ApprovalNotRequested_When_Value_Null()
        {
            var artifacts = new List<int> { 1 };
            var expectedXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewContents xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Artifacts><CA><Id>1</Id></CA></Artifacts></RDReviewContents>";

            // Arrange
            _artifactDetails.LockedByUserId = UserId;

            _review.Contents.Artifacts = new List<RDArtifact>
            {
                new RDArtifact
                {
                    Id = artifacts.First(),
                    ApprovalNotRequested = null
                }
            };

            _mockArtifactPermissionsRepository
                .Setup(m => m.GetArtifactPermissions(artifacts, UserId, false, int.MaxValue, true))
                .ReturnsAsync(new Dictionary<int, RolePermissions>
                    {
                        { artifacts[0], RolePermissions.Read }
                    });

            // Act

                var resultArtifactsXml = ReviewRawDataHelper.GetStoreData(_review.Contents);

           // Assert
            Assert.AreEqual(resultArtifactsXml, expectedXml);
        }

        [TestMethod]
        public void AssignApprovalRequiredToArtifacts_Should_Serialize_ApprovalNotRequested_When_Value_Is_Not_Null()
        {
            var artifacts = new List<int> { 1 };
            var expectedXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewContents xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Artifacts><CA><ANR>true</ANR><Id>1</Id></CA></Artifacts></RDReviewContents>";

            // Arrange
            _artifactDetails.LockedByUserId = UserId;

            _review.Contents.Artifacts = new List<RDArtifact>
            {
                new RDArtifact
                {
                    Id = artifacts.First(),
                    ApprovalNotRequested = true
                }
            };

            _mockArtifactPermissionsRepository
                .Setup(m => m.GetArtifactPermissions(artifacts, UserId, false, int.MaxValue, true))
                .ReturnsAsync(new Dictionary<int, RolePermissions>
                    {
                        { artifacts[0], RolePermissions.Read }
                    });

            // Act

            var resultArtifactsXml = ReviewRawDataHelper.GetStoreData(_review.Contents);

            // Assert
            Assert.AreEqual(resultArtifactsXml, expectedXml);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task AssignRoleToParticipantAsync_Should_Throw_When_Review_Does_Not_Exist()
        {
            _mockArtifactRepository
                .Setup(m => m.GetArtifactBasicDetails(ReviewId, UserId))
                .ReturnsAsync(() => null);

            var content = new AssignParticipantRoleParameter
            {
                Role = ReviewParticipantRole.Approver,
                ItemIds = new List<int> { 1 },
                SelectionType = SelectionType.Selected,
            };

            // Act
            await _reviewService.AssignRoleToParticipantsAsync(ReviewId, content, UserId);
        }

        [TestMethod]
        public async Task AssignRoleToParticipantAsync_Should_Throw_When_Review_Is_ReadOnly()
        {
            // Arrange
            var content = new AssignParticipantRoleParameter
            {
                ItemIds = new List<int> { 1 },
                Role = ReviewParticipantRole.Approver
            };
            _artifactDetails.LockedByUserId = UserId;
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Closed;

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
            var content = new AssignParticipantRoleParameter
            {
                ItemIds = new List<int> { 1 },
                Role = ReviewParticipantRole.Approver
            };

            // Act
            await _reviewService.AssignRoleToParticipantsAsync(ReviewId, content, UserId);
        }

        [TestMethod]
        public async Task AssignRoleToParticipantAsync_Should_Throw_When_Review_Needs_To_Be_Deactivated()
        {
            // Arrange
            var content = new AssignParticipantRoleParameter
            {
                ItemIds = new List<int> { UserId },
                Role = ReviewParticipantRole.Approver
            };
            _artifactDetails.LockedByUserId = UserId;

            _review.Contents.Artifacts = new List<RDArtifact>
            {
                new RDArtifact
                {
                    Id = 5,
                    ApprovalNotRequested = false
                }
            };

            _review.ReviewPackageRawData.Reviewers = new List<ReviewerRawData>
            {
                new ReviewerRawData
                {
                    UserId = UserId,
                    Permission = ReviewParticipantRole.Reviewer
                }
            };
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Active;

            // Act
            try
            {
                await _reviewService.AssignRoleToParticipantsAsync(ReviewId, content, UserId);
            }
            catch (ConflictException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.ReviewNeedsToMoveBackToDraftState, ex.ErrorCode);

                return;
            }

            Assert.Fail("A Conflict Exception was not  thrown.");
        }

        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        [Ignore]
        public async Task AssignRoleToParticipantAsync_Should_Throw_When_User_Is_Disabled()
        {
            // Arrange
            var content = new AssignParticipantRoleParameter
            {
                ItemIds = new List<int> { 1 },
                Role = ReviewParticipantRole.Approver
            };

            // Act
            await _reviewService.AssignRoleToParticipantsAsync(ReviewId, content, UserId);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task AssignRoleToParticipantAsync_Should_Throw_When_NoReviewers()
        {
            // Arrange
            var content = new AssignParticipantRoleParameter
            {
                ItemIds = new List<int> { 1 },
                Role = ReviewParticipantRole.Approver
            };
            _artifactDetails.LockedByUserId = UserId;

            // Act
            await _reviewService.AssignRoleToParticipantsAsync(ReviewId, content, UserId);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task AssignRoleToParticipantAsync_Should_Throw_BadRequestException_With_Wrong_Parameters()
        {
            // Arrange
            var content = new AssignParticipantRoleParameter
            {
                ItemIds = new List<int>(),
                Role = ReviewParticipantRole.Approver,
                SelectionType = SelectionType.Selected
            };

            // Act
            await _reviewService.AssignRoleToParticipantsAsync(ReviewId, content, UserId);

            // Assert
            Expression<Func<ReviewPackageRawData, bool>> reviewRawDataCheck = reviewRawData => reviewRawData.Reviewers.All(r => r.Permission == ReviewParticipantRole.Approver);

            _mockReviewRepository.Verify(repo => repo.UpdateReviewPackageRawDataAsync(ReviewId, It.Is(reviewRawDataCheck), UserId));
        }

        [TestMethod]
        public async Task AssignRoleToParticipantAsync_Should_Return_DropdownItems_Null_When_Meaning_Of_Signature_Is_Disabled()
        {
            // Arrange
            _artifactDetails.LockedByUserId = UserId;

            var reviewerId = 3;
            _review.ReviewPackageRawData.Reviewers = new List<ReviewerRawData> { new ReviewerRawData { UserId = reviewerId } };

            var content = new AssignParticipantRoleParameter
            {
                ItemIds = new List<int> { 1 },
                Role = ReviewParticipantRole.Approver
            };

            // Act
            var result = await _reviewService.AssignRoleToParticipantsAsync(ReviewId, content, UserId);

            // Assert
            Assert.IsNull(result.DropdownItems);
        }

        [TestMethod]
        public async Task AssignRoleToParticipantAsync_Should_Add_All_Possible_Meaning_Of_Signatures_When_Meaning_Of_Signature_Is_Enabled()
        {
            // Arrange
            _artifactDetails.LockedByUserId = UserId;

            var reviewerId = 3;
            _review.ReviewPackageRawData.IsMoSEnabled = true;
            _review.ReviewPackageRawData.Reviewers = new List<ReviewerRawData> { new ReviewerRawData { UserId = reviewerId } };

            var content = new AssignParticipantRoleParameter
            {
                ItemIds = new List<int> { reviewerId },
                Role = ReviewParticipantRole.Approver
            };

            _possibleMeaningOfSignatures = new Dictionary<int, List<ParticipantMeaningOfSignatureResult>>
            {
                {
                    reviewerId,
                    new List<ParticipantMeaningOfSignatureResult>
                    {
                        new ParticipantMeaningOfSignatureResult { RoleId = 2 },
                        new ParticipantMeaningOfSignatureResult { RoleId = 3 }
                    }
                }
            };

            // Act
            await _reviewService.AssignRoleToParticipantsAsync(ReviewId, content, UserId);

            // Assert
            Expression<Func<ReviewPackageRawData, bool>> reviewRawDataCheck = reviewRawData =>
                reviewRawData.Reviewers.First().SelectedRoleMoSAssignments.Count == 2
                && reviewRawData.Reviewers.First().SelectedRoleMoSAssignments.All(mos => mos.RoleId == 2 || mos.RoleId == 3);

            _mockReviewRepository.Verify(repo => repo.UpdateReviewPackageRawDataAsync(ReviewId, It.Is(reviewRawDataCheck), UserId));
        }

        [TestMethod]
        public async Task AssignRoleToParticipantAsync_Should_Return_All_Assigned_Meaning_Of_Signatures_When_Meaning_Of_Signature_Is_Enabled()
        {
            // Arrange
            _artifactDetails.LockedByUserId = UserId;
            var reviewerId = 3;
            _review.ReviewPackageRawData.IsMoSEnabled = true;
            _review.ReviewPackageRawData.Reviewers = new List<ReviewerRawData> { new ReviewerRawData { UserId = reviewerId } };

            var content = new AssignParticipantRoleParameter
            {
                ItemIds = new List<int> { reviewerId },
                Role = ReviewParticipantRole.Approver
            };

            _possibleMeaningOfSignatures = new Dictionary<int, List<ParticipantMeaningOfSignatureResult>>
            {
                {
                    reviewerId,
                    new List<ParticipantMeaningOfSignatureResult>
                    {
                        new ParticipantMeaningOfSignatureResult
                        {
                            MeaningOfSignatureValue = "foo1",
                            RoleId = 2,
                            RoleName = "bar1"
                        },
                        new ParticipantMeaningOfSignatureResult
                        {
                            MeaningOfSignatureValue = "foo2",
                            RoleId = 3,
                            RoleName = "bar2"
                        }
                    }
                }
            };

            // Act
            var result = await _reviewService.AssignRoleToParticipantsAsync(ReviewId, content, UserId);
            var dropDowns = result.DropdownItems.ToArray();
            // Assert
            Assert.AreEqual(2, result.DropdownItems.Count());

            Assert.AreEqual("foo1 (bar1)", dropDowns[0].Label);
            Assert.AreEqual(2, dropDowns[0].Value);

            Assert.AreEqual("foo2 (bar2)", dropDowns[1].Label);
            Assert.AreEqual(3, dropDowns[1].Value);
        }

        [TestMethod]
        public async Task AssignRoleToParticipantAsync_NoApprover_ArtifactsRequiringApproval_RequireESignatureIsNull_ESignatureIsEnabledInProject_DoesNotUpdateRequireESignature()
        {
            // Arrange
            _reviewType = ReviewType.Formal;
            _review.ReviewPackageRawData.IsESignatureEnabled = null;
            _projectPermissions = ProjectPermissions.IsReviewESignatureEnabled;
            _artifactDetails.LockedByUserId = UserId;

            var reviewerId = 3;
            _review.ReviewPackageRawData.Reviewers = new List<ReviewerRawData> { new ReviewerRawData { UserId = reviewerId } };

            var content = new AssignParticipantRoleParameter
            {
                ItemIds = new List<int> { reviewerId },
                Role = ReviewParticipantRole.Reviewer
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
                        }
                    }
                }
            };

            // Act
            await _reviewService.AssignRoleToParticipantsAsync(ReviewId, content, UserId);

            // Assert
            Assert.AreEqual(null, _review.ReviewPackageRawData.IsESignatureEnabled);
        }

        [TestMethod]
        public async Task AssignRoleToParticipantAsync_Approver_NoArtifactsRequiringApproval_RequireESignatureIsNull_ESignatureIsEnabledInProject_DoesNotUpdateRequireESignature()
        {
            // Arrange
            _reviewType = ReviewType.Informal;
            _review.ReviewPackageRawData.IsESignatureEnabled = null;
            _projectPermissions = ProjectPermissions.IsReviewESignatureEnabled;
            _artifactDetails.LockedByUserId = UserId;

            const int reviewerId = 3;
            _review.ReviewPackageRawData.Reviewers = new List<ReviewerRawData>
            {
                new ReviewerRawData
                {
                    UserId = reviewerId
                }
            };

            var content = new AssignParticipantRoleParameter
            {
                ItemIds = new List<int> { reviewerId },
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
                        }
                    }
                }
            };

            // Act
            await _reviewService.AssignRoleToParticipantsAsync(ReviewId, content, UserId);

            // Assert
            Assert.AreEqual(null, _review.ReviewPackageRawData.IsESignatureEnabled);
        }

        [TestMethod]
        public async Task AssignRoleToParticipantAsync_Approver_ArtifactsRequiringApproval_RequireESignatureIsFalse_ESignatureIsEnabledInProject_DoesNotUpdateRequireESignature()
        {
            // Arrange
            _reviewType = ReviewType.Formal;
            _review.ReviewPackageRawData.IsESignatureEnabled = false;
            _projectPermissions = ProjectPermissions.IsReviewESignatureEnabled;
            _artifactDetails.LockedByUserId = UserId;

            var reviewerId = 3;
            _review.ReviewPackageRawData.Reviewers = new List<ReviewerRawData> { new ReviewerRawData { UserId = reviewerId } };

            var content = new AssignParticipantRoleParameter
            {
                ItemIds = new List<int> { 3 },
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
                        }
                    }
                }
            };

            // Act
            await _reviewService.AssignRoleToParticipantsAsync(ReviewId, content, UserId);

            // Assert
            Assert.AreEqual(false, _review.ReviewPackageRawData.IsESignatureEnabled);
        }

        [TestMethod]
        public async Task AssignRoleToParticipantAsync_Approver_ArtifactsRequiringApproval_RequireESignatureIsTrue_ESignatureIsEnabledInProject_DoesNotUpdateRequireESignature()
        {
            // Arrange
            _reviewType = ReviewType.Formal;
            _review.ReviewPackageRawData.IsESignatureEnabled = true;
            _projectPermissions = ProjectPermissions.IsReviewESignatureEnabled;
            _artifactDetails.LockedByUserId = UserId;

            var reviewerId = 3;
            _review.ReviewPackageRawData.Reviewers = new List<ReviewerRawData> { new ReviewerRawData { UserId = reviewerId } };

            var content = new AssignParticipantRoleParameter
            {
                ItemIds = new List<int> { reviewerId },
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
                        }
                    }
                }
            };

            // Act
            await _reviewService.AssignRoleToParticipantsAsync(ReviewId, content, UserId);

            // Assert
            Assert.AreEqual(true, _review.ReviewPackageRawData.IsESignatureEnabled);
        }

        [TestMethod]
        public async Task AssignRoleToParticipantAsync_Approver_ArtifactsRequiringApproval_RequireESignatureIsNull_ESignatureIsDisabledInProject_DoesNotUpdateRequireESignature()
        {
            // Arrange
            _reviewType = ReviewType.Formal;
            _review.ReviewPackageRawData.IsESignatureEnabled = null;
            _projectPermissions = ProjectPermissions.None;
            _artifactDetails.LockedByUserId = UserId;

            var reviewerId = 3;
            _review.ReviewPackageRawData.Reviewers = new List<ReviewerRawData> { new ReviewerRawData { UserId = reviewerId } };

            var content = new AssignParticipantRoleParameter
            {
                ItemIds = new List<int> { reviewerId },
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
                        }
                    }
                }
            };


            // Act
            await _reviewService.AssignRoleToParticipantsAsync(ReviewId, content, UserId);

            // Assert
            Assert.AreEqual(null, _review.ReviewPackageRawData.IsESignatureEnabled);
        }

        [TestMethod]
        public async Task AssignRoleToParticipantAsync_Approver_ArtifactsRequiringApproval_RequireESignatureIsNull_ESignatureIsEnabledInProject_UpdatesRequireESignature()
        {
            // Arrange
            _reviewType = ReviewType.Formal;
            _review.ReviewPackageRawData.IsESignatureEnabled = null;
            _projectPermissions = ProjectPermissions.IsReviewESignatureEnabled;
            _artifactDetails.LockedByUserId = UserId;
            const int reviewerId = 3;
            _review.ReviewPackageRawData.Reviewers = new List<ReviewerRawData> { new ReviewerRawData { UserId = reviewerId } };

            var content = new AssignParticipantRoleParameter
            {
                ItemIds = new List<int> { reviewerId },
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
                        }
                    }
                }
            };

            // Act
            await _reviewService.AssignRoleToParticipantsAsync(ReviewId, content, UserId);

            // Assert
            Assert.AreEqual(true, _review.ReviewPackageRawData.IsESignatureEnabled);
        }

        private void SetReviewType(ReviewType reviewType)
        {
            switch (reviewType)
            {
                case ReviewType.Formal:
                    _review.ReviewPackageRawData.Reviewers = new List<ReviewerRawData>()
                    {
                        new ReviewerRawData() { Permission = ReviewParticipantRole.Approver }
                    };

                    _review.Contents.Artifacts = new List<RDArtifact>()
                    {
                        new RDArtifact() { ApprovalNotRequested = false }
                    };
                    break;
                case ReviewType.Informal:
                    _review.ReviewPackageRawData.Reviewers = new List<ReviewerRawData>()
                    {
                        new ReviewerRawData() { Permission = ReviewParticipantRole.Reviewer }
                    };
                    break;
                case ReviewType.Public:
                    _review.ReviewPackageRawData.Reviewers = null;
                    break;
            }
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
        public async Task AssignApprovalRequiredToArtifacts_ReviewIsDeleted_Should_Throw_ResourceNotFoundException()
        {
            _mockArtifactRepository
                .Setup(m => m.GetArtifactBasicDetails(ReviewId, UserId))
                .ReturnsAsync((ArtifactBasicDetails)null);

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
        public async Task AssignApprovalRequiredToArtifacts_Review_ReadOnly_Should_Throw_ConflictException()
        {
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
        public async Task AssignApprovalRequiredToArtifacts_Review_NotLocked_Should_Throw_ConflictException()
        {
            var content = new AssignArtifactsApprovalParameter
            {
                ItemIds = new List<int> { 1, 2, 3 },
                ApprovalRequired = true
            };

            // Act
            await _reviewService.AssignApprovalRequiredToArtifactsAsync(ReviewId, content, UserId);
        }

        [TestMethod]
        public async Task AssignApprovalRequiredToArtifacts_Review_Needs_To_Be_Deactivated_Should_Throw_ConflictException()
        {
            var artifacts = new List<int> { 1 };
            // Arrange
            var content = new AssignArtifactsApprovalParameter
            {
                ItemIds = artifacts,
                ApprovalRequired = true
            };
            _artifactDetails.LockedByUserId = UserId;

            _review.Contents.Artifacts = new List<RDArtifact>
            {
                new RDArtifact
                {
                    Id = artifacts.First(),
                    ApprovalNotRequested = true
                }
            };

            _review.ReviewPackageRawData.Reviewers = new List<ReviewerRawData>
            {
                new ReviewerRawData
                {
                    UserId = UserId,
                    Permission = ReviewParticipantRole.Approver
                }
            };
            _review.ReviewPackageRawData.Status = ReviewPackageStatus.Active;

            _mockArtifactPermissionsRepository
                .Setup(m => m.GetArtifactPermissions(artifacts, UserId, false, int.MaxValue, true))
                .ReturnsAsync(new Dictionary<int, RolePermissions>
                    {
                        { artifacts[0], RolePermissions.Read }
                    });

            // Act
            try
            {
                await _reviewService.AssignApprovalRequiredToArtifactsAsync(ReviewId, content, UserId);
            }
            catch (ConflictException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.ReviewNeedsToMoveBackToDraftState, ex.ErrorCode);

                return;
            }

            Assert.Fail("A Conflict Exception was not  thrown.");
        }

        [TestMethod]
        public async Task AssignApprovalRequiredToArtifacts_Review_Success()
        {
            _review.Contents.Artifacts = new List<RDArtifact>
            {
                new RDArtifact { Id = 1 },
                new RDArtifact { Id = 2, ApprovalNotRequested = true },
                new RDArtifact { Id = 3 }
            };
            _artifactDetails.LockedByUserId = UserId;

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
            _review.Contents.Artifacts = new List<RDArtifact>
            {
                new RDArtifact { Id = 1 },
                new RDArtifact { Id = 2 },
                new RDArtifact { Id = 3 }
            };
            var artifactIds = new List<int> { 1, 2, 3 };
            var content = new AssignArtifactsApprovalParameter
            {
                ItemIds = artifactIds,
                ApprovalRequired = true
            };
            _artifactDetails.LockedByUserId = UserId;

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
            _review.Contents.Artifacts = new List<RDArtifact>
            {
                new RDArtifact { Id = 1 },
                new RDArtifact { Id = 2 },
                new RDArtifact { Id = 3 }
            };
            var artifactIds = new List<int> { 1, 2, 3, 4, 5 };
            var content = new AssignArtifactsApprovalParameter
            {
                ItemIds = artifactIds,
                ApprovalRequired = false
            };
            _artifactDetails.LockedByUserId = UserId;

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
            _review.Contents.Artifacts = new List<RDArtifact>
            {
                new RDArtifact { Id = 1 },
                new RDArtifact { Id = 2 },
                new RDArtifact { Id = 3 }
            };
            _artifactDetails.LockedByUserId = UserId;
            var artifactIds = new List<int> { 1, 2, 3 };
            var content = new AssignArtifactsApprovalParameter
            {
                ItemIds = artifactIds,
                ApprovalRequired = true
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

        [TestMethod]
        public async Task AssignApprovalRequiredToArtifacts_ArtifactsWithApproval_Approvers_RequireESignatureIsNull_ESignatureEnabledInProject_UpdatesRequireESignature()
        {
            _review.ReviewPackageRawData.IsESignatureEnabled = null;
            _reviewType = ReviewType.Formal;
            _projectPermissions = ProjectPermissions.IsReviewESignatureEnabled;
            _artifactDetails.LockedByUserId = UserId;

            _review.Contents.Artifacts = new List<RDArtifact>
            {
                new RDArtifact { Id = 1, ApprovalNotRequested = true },
                new RDArtifact { Id = 2, ApprovalNotRequested = true },
                new RDArtifact { Id = 3, ApprovalNotRequested = true }
            };

            var artifactIds = new List<int> { 1, 2, 3 };
            var content = new AssignArtifactsApprovalParameter
            {
                ItemIds = artifactIds,
                ApprovalRequired = true
            };

            _mockArtifactVersionsRepository
                .Setup(m => m.GetDeletedAndNotInProjectItems(artifactIds, ProjectId))
                .ReturnsAsync(new List<int>());

            _mockArtifactPermissionsRepository
                .Setup(m => m.GetArtifactPermissions(artifactIds, UserId, false, int.MaxValue, true))
                .ReturnsAsync(() => new Dictionary<int, RolePermissions>
                    {
                        { 1, RolePermissions.Read },
                        { 2, RolePermissions.Read },
                        { 3, RolePermissions.Read }
                    });

            // Act
            await _reviewService.AssignApprovalRequiredToArtifactsAsync(ReviewId, content, UserId);

            // Assert
            Assert.AreEqual(true, _review.ReviewPackageRawData.IsESignatureEnabled);
        }

        [TestMethod]
        public async Task AssignApprovalRequiredToArtifacts_ArtifactsWithoutApproval_Approvers_RequireESignatureIsNull_ESignatureEnabledInProject_DoesNotUpdateRequireESignature()
        {
            _review.ReviewPackageRawData.IsESignatureEnabled = null;
            _reviewType = ReviewType.Formal;
            _projectPermissions = ProjectPermissions.IsReviewESignatureEnabled;
            _artifactDetails.LockedByUserId = UserId;

            _review.Contents.Artifacts = new List<RDArtifact>
            {
                new RDArtifact { Id = 1, ApprovalNotRequested = true },
                new RDArtifact { Id = 2, ApprovalNotRequested = true },
                new RDArtifact { Id = 3, ApprovalNotRequested = true }
            };

            var artifactIds = new List<int> { 1, 2, 3 };
            var content = new AssignArtifactsApprovalParameter
            {
                ItemIds = artifactIds,
                ApprovalRequired = false
            };

            _mockArtifactVersionsRepository
                .Setup(m => m.GetDeletedAndNotInProjectItems(artifactIds, ProjectId))
                .ReturnsAsync(new List<int>());

            _mockArtifactPermissionsRepository
                .Setup(m => m.GetArtifactPermissions(artifactIds, UserId, false, int.MaxValue, true))
                .ReturnsAsync(() => new Dictionary<int, RolePermissions>
                    {
                        { 1, RolePermissions.Read },
                        { 2, RolePermissions.Read },
                        { 3, RolePermissions.Read }
                    });

            // Act
            await _reviewService.AssignApprovalRequiredToArtifactsAsync(ReviewId, content, UserId);

            // Assert
            Assert.AreEqual(null, _review.ReviewPackageRawData.IsESignatureEnabled);
        }

        [TestMethod]
        public async Task AssignApprovalRequiredToArtifacts_ArtifactsWithApproval_NoApprovers_RequireESignatureIsNull_ESignatureEnabledInProject_DoesNotUpdateRequireESignature()
        {
            _review.ReviewPackageRawData.IsESignatureEnabled = null;
            _reviewType = ReviewType.Informal;
            _projectPermissions = ProjectPermissions.IsReviewESignatureEnabled;
            _artifactDetails.LockedByUserId = UserId;

            _review.Contents.Artifacts = new List<RDArtifact>
            {
                new RDArtifact { Id = 1, ApprovalNotRequested = true },
                new RDArtifact { Id = 2, ApprovalNotRequested = true },
                new RDArtifact { Id = 3, ApprovalNotRequested = true }
            };

            var artifactIds = new List<int> { 1, 2, 3 };
            var content = new AssignArtifactsApprovalParameter
            {
                ItemIds = artifactIds,
                ApprovalRequired = true
            };

            _mockArtifactVersionsRepository
                .Setup(m => m.GetDeletedAndNotInProjectItems(artifactIds, ProjectId))
                .ReturnsAsync(new List<int>());

            _mockArtifactPermissionsRepository
                .Setup(m => m.GetArtifactPermissions(artifactIds, UserId, false, int.MaxValue, true))
                .ReturnsAsync(() => new Dictionary<int, RolePermissions>
                    {
                        { 1, RolePermissions.Read },
                        { 2, RolePermissions.Read },
                        { 3, RolePermissions.Read }
                    });

            // Act
            await _reviewService.AssignApprovalRequiredToArtifactsAsync(ReviewId, content, UserId);

            // Assert
            Assert.AreEqual(null, _review.ReviewPackageRawData.IsESignatureEnabled);
        }

        [TestMethod]
        public async Task AssignApprovalRequiredToArtifacts_ArtifactsWithApproval_Approvers_RequireESignatureIsFalse_ESignatureEnabledInProject_DoesNotUpdateRequireESignature()
        {
            _review.ReviewPackageRawData.IsESignatureEnabled = false;
            _reviewType = ReviewType.Formal;
            _projectPermissions = ProjectPermissions.IsReviewESignatureEnabled;
            _artifactDetails.LockedByUserId = UserId;

            _review.Contents.Artifacts = new List<RDArtifact>
            {
                new RDArtifact { Id = 1, ApprovalNotRequested = true },
                new RDArtifact { Id = 2, ApprovalNotRequested = true },
                new RDArtifact { Id = 3, ApprovalNotRequested = true }
            };

            var artifactIds = new List<int> { 1, 2, 3 };
            var content = new AssignArtifactsApprovalParameter
            {
                ItemIds = artifactIds,
                ApprovalRequired = true
            };

            _mockArtifactVersionsRepository
                .Setup(m => m.GetDeletedAndNotInProjectItems(artifactIds, ProjectId))
                .ReturnsAsync(new List<int>());

            _mockArtifactPermissionsRepository
                .Setup(m => m.GetArtifactPermissions(artifactIds, UserId, false, int.MaxValue, true))
                .ReturnsAsync(() => new Dictionary<int, RolePermissions>
                    {
                        { 1, RolePermissions.Read },
                        { 2, RolePermissions.Read },
                        { 3, RolePermissions.Read }
                    });

            // Act
            await _reviewService.AssignApprovalRequiredToArtifactsAsync(ReviewId, content, UserId);

            // Assert
            Assert.AreEqual(false, _review.ReviewPackageRawData.IsESignatureEnabled);
        }

        [TestMethod]
        public async Task AssignApprovalRequiredToArtifacts_ArtifactsWithApproval_Approvers_RequireESignatureIsTrue_ESignatureEnabledInProject_DoesNotUpdateRequireESignature()
        {
            _review.ReviewPackageRawData.IsESignatureEnabled = true;
            _reviewType = ReviewType.Formal;
            _projectPermissions = ProjectPermissions.IsReviewESignatureEnabled;
            _artifactDetails.LockedByUserId = UserId;

            _review.Contents.Artifacts = new List<RDArtifact>
            {
                new RDArtifact { Id = 1, ApprovalNotRequested = true },
                new RDArtifact { Id = 2, ApprovalNotRequested = true },
                new RDArtifact { Id = 3, ApprovalNotRequested = true }
            };

            var artifactIds = new List<int> { 1, 2, 3 };
            var content = new AssignArtifactsApprovalParameter
            {
                ItemIds = artifactIds,
                ApprovalRequired = true
            };

            _mockArtifactVersionsRepository
                .Setup(m => m.GetDeletedAndNotInProjectItems(artifactIds, ProjectId))
                .ReturnsAsync(new List<int>());

            _mockArtifactPermissionsRepository
                .Setup(m => m.GetArtifactPermissions(artifactIds, UserId, false, int.MaxValue, true))
                .ReturnsAsync(() => new Dictionary<int, RolePermissions>
                    {
                        { 1, RolePermissions.Read },
                        { 2, RolePermissions.Read },
                        { 3, RolePermissions.Read }
                    });

            // Act
            await _reviewService.AssignApprovalRequiredToArtifactsAsync(ReviewId, content, UserId);

            // Assert
            Assert.AreEqual(true, _review.ReviewPackageRawData.IsESignatureEnabled);
        }

        [TestMethod]
        public async Task AssignApprovalRequiredToArtifacts_ArtifactsWithApproval_Approvers_RequireESignatureIsNull_ESignatureDisabledInProject_DoesNotUpdateRequireESignature()
        {
            _review.ReviewPackageRawData.IsESignatureEnabled = null;
            _reviewType = ReviewType.Formal;
            _projectPermissions = ProjectPermissions.None;
            _artifactDetails.LockedByUserId = UserId;

            _review.Contents.Artifacts = new List<RDArtifact>
            {
                new RDArtifact { Id = 1, ApprovalNotRequested = true },
                new RDArtifact { Id = 2, ApprovalNotRequested = true },
                new RDArtifact { Id = 3, ApprovalNotRequested = true }
            };

            var artifactIds = new List<int> { 1, 2, 3 };
            var content = new AssignArtifactsApprovalParameter
            {
                ItemIds = artifactIds,
                ApprovalRequired = true
            };

            _mockArtifactVersionsRepository
                .Setup(m => m.GetDeletedAndNotInProjectItems(artifactIds, ProjectId))
                .ReturnsAsync(new List<int>());

            _mockArtifactPermissionsRepository
                .Setup(m => m.GetArtifactPermissions(artifactIds, UserId, false, int.MaxValue, true))
                .ReturnsAsync(() => new Dictionary<int, RolePermissions>
                    {
                        { 1, RolePermissions.Read },
                        { 2, RolePermissions.Read },
                        { 3, RolePermissions.Read }
                    });

            // Act
            await _reviewService.AssignApprovalRequiredToArtifactsAsync(ReviewId, content, UserId);

            // Assert
            Assert.AreEqual(null, _review.ReviewPackageRawData.IsESignatureEnabled);
        }

        #endregion
    }
}
