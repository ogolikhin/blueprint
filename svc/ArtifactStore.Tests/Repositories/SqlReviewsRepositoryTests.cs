using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.Models.Review;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Models.VersionControl;
using ServiceLibrary.Repositories;
using ServiceLibrary.Services;

namespace ArtifactStore.Repositories
{
    [TestClass]
    public class SqlReviewsRepositoryTests
    {
        private SqlConnectionWrapperMock _cxn;
        private IReviewsRepository _reviewsRepository;

        private Mock<IArtifactVersionsRepository> _artifactVersionsRepositoryMock;
        private Mock<IItemInfoRepository> _itemInfoRepositoryMock;
        private Mock<IArtifactPermissionsRepository> _artifactPermissionsRepositoryMock;
        private Mock<IApplicationSettingsRepository> _applicationSettingsRepositoryMock;
        private Mock<IUsersRepository> _usersRepositoryMock;
        private Mock<IArtifactRepository> _artifactRepositoryMock;
        private Mock<ICurrentDateTimeService> _currentDateTimeServiceMock;
        private Mock<ILockArtifactsRepository> _lockArtifactsRepositoryMock;
        private bool _hasEditPermissions = true;
        private ArtifactBasicDetails _artifactDetails;

        public const int ReviewId = 1;
        public const int UserId = 2;
        private const int ProjectId = 3;

        [TestInitialize]
        public void Initialize()
        {
            _cxn = new SqlConnectionWrapperMock();
            _artifactVersionsRepositoryMock = new Mock<IArtifactVersionsRepository>(MockBehavior.Strict);
            _itemInfoRepositoryMock = new Mock<IItemInfoRepository>(MockBehavior.Strict);
            _artifactPermissionsRepositoryMock = new Mock<IArtifactPermissionsRepository>(MockBehavior.Strict);
            _applicationSettingsRepositoryMock = new Mock<IApplicationSettingsRepository>(MockBehavior.Strict);
            _usersRepositoryMock = new Mock<IUsersRepository>();
            _artifactRepositoryMock = new Mock<IArtifactRepository>();
            _currentDateTimeServiceMock = new Mock<ICurrentDateTimeService>();
            _lockArtifactsRepositoryMock = new Mock<ILockArtifactsRepository>();

            _artifactRepositoryMock.SetReturnsDefault(Task.FromResult(true));
            _currentDateTimeServiceMock.Setup(service => service.GetUtcNow()).Returns(new DateTime(2017, 07, 10, 13, 20, 0));
            _artifactPermissionsRepositoryMock.Setup(r => r.HasEditPermissions(It.IsAny<int>(), It.IsAny<int>(), false, int.MaxValue, true)).ReturnsAsync(() => _hasEditPermissions);


            _reviewsRepository = new SqlReviewsRepository
            (
                _cxn.Object,
                _artifactVersionsRepositoryMock.Object,
                _itemInfoRepositoryMock.Object,
                _artifactPermissionsRepositoryMock.Object,
                _applicationSettingsRepositoryMock.Object,
                _usersRepositoryMock.Object,
                _artifactRepositoryMock.Object,
                _currentDateTimeServiceMock.Object,
                _lockArtifactsRepositoryMock.Object,
                new SqlHelperMock());

            _artifactDetails = new ArtifactBasicDetails
            {
                ItemId = ReviewId,
                ProjectId = ProjectId,
                PrimitiveItemTypePredefined = (int)ItemTypePredefined.ArtifactReviewPackage
            };

            _artifactRepositoryMock
                .Setup(m => m.GetArtifactBasicDetails(ReviewId, UserId))
                .ReturnsAsync(() => _artifactDetails);
        }

        #region GetReviewSummary

        [TestMethod]
        public async Task GetReviewSummary_Formal_Success()
        {
            // Arange
            var reviewId = 1;
            var reviewName = "My Review";
            var reviewDescription = "My Description";
            var userId = 2;
            var baselineId = 3;
            var totalArtifacts = 8;
            var revisionId = 999;
            var reviewStatus = ReviewStatus.Completed;

            _itemInfoRepositoryMock.Setup(i => i.GetItemDescription(reviewId, userId, true, int.MaxValue)).ReturnsAsync(reviewDescription);
            var reviewDetails = new ReviewSummaryDetails
            {
                BaselineId = baselineId,
                ReviewPackageStatus = ReviewPackageStatus.Active,
                ReviewParticipantRole = ReviewParticipantRole.Approver,
                TotalArtifacts = totalArtifacts,
                TotalViewable = 7,
                TotalReviewers = 5,
                ReviewStatus = reviewStatus,
                Approved = 5,
                Disapproved = 3,
                Pending = 2,
                RevisionId = revisionId,
                RequireAllArtifactsReviewed = true,
                RequireESignature = true,
                RequireMeaningOfSignature = false,
                ShowOnlyDescription = true
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

            _cxn.SetupExecuteScalarAsync("GetReviewType", param, ReviewType.Formal);

            // Act
            var review = await _reviewsRepository.GetReviewSummary(reviewId, userId);

            // Assert
            _cxn.Verify();

            Assert.AreEqual(totalArtifacts, review.TotalArtifacts);
            Assert.AreEqual(7, review.TotalViewable);
            Assert.AreEqual(baselineId, review.Source.Id);
            Assert.AreEqual(reviewStatus, review.Status);
            Assert.AreEqual(reviewName, review.Name);
            Assert.AreEqual(reviewDescription, review.Description);
            Assert.AreEqual(revisionId, review.RevisionId);
            Assert.AreEqual(ReviewType.Formal, review.ReviewType);
            Assert.AreEqual(true, review.RequireAllArtifactsReviewed);
            Assert.AreEqual(true, review.ShowOnlyDescription);
            Assert.AreEqual(true, review.RequireESignature);
            Assert.AreEqual(false, review.RequireMeaningOfSignature);
            Assert.AreEqual(5, review.ArtifactsStatus.Approved);
            Assert.AreEqual(3, review.ArtifactsStatus.Disapproved);
            Assert.AreEqual(2, review.ArtifactsStatus.Pending);
            Assert.AreEqual(null, review.MeaningOfSignatures);
        }

        [TestMethod]
        public async Task GetReviewSummary_Should_Return_ReviewType_Public_When_No_Reviewers()
        {
            // Arange
            var reviewId = 1;
            var reviewName = "My Review";
            var reviewDescription = "My Description";
            var userId = 2;

            _itemInfoRepositoryMock.Setup(i => i.GetItemDescription(reviewId, userId, true, int.MaxValue)).ReturnsAsync(reviewDescription);
            var reviewDetails = new ReviewSummaryDetails
            {
                ReviewPackageStatus = ReviewPackageStatus.Active,
                TotalReviewers = 0
            };

            var param = new Dictionary<string, object> { { "reviewId", reviewId }, { "userId", userId } };
            _cxn.SetupQueryAsync("GetReviewDetails", param, Enumerable.Repeat(reviewDetails, 1));

            var reviewInfo = new VersionControlArtifactInfo
            {
                Name = reviewName,
                PredefinedType = ItemTypePredefined.ArtifactReviewPackage
            };

            _artifactVersionsRepositoryMock.Setup(r => r.GetVersionControlArtifactInfoAsync(reviewId, null, userId)).ReturnsAsync(reviewInfo);

            // Act
            var review = await _reviewsRepository.GetReviewSummary(reviewId, userId);

            // Assert
            _cxn.Verify();

            Assert.AreEqual(ReviewType.Public, review.ReviewType);
        }

        [TestMethod]
        public async Task GetReviewSummary_Formal_Throws_ResourceNotFoundException()
        {
            // Arange
            var reviewId = 1;
            var userId = 2;
            var reviewInfo = new VersionControlArtifactInfo
            {
                PredefinedType = ItemTypePredefined.Actor
            };

            _artifactVersionsRepositoryMock.Setup(r => r.GetVersionControlArtifactInfoAsync(reviewId, null, userId)).ReturnsAsync(reviewInfo);

            var isExceptionThrown = false;
            // Act
            try
            {
                await _reviewsRepository.GetReviewSummary(reviewId, userId);
            }
            catch (ResourceNotFoundException ex)
            {
                isExceptionThrown = true;
                // Assert
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
        public async Task GetReviewSummary_Formal_Throws_AuthorizationException()
        {
            // Arange
            var reviewId = 1;
            var userId = 2;
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

            var isExceptionThrown = false;

            // Act
            try
            {
                await _reviewsRepository.GetReviewSummary(reviewId, userId);
            }
            catch (AuthorizationException ex)
            {
                isExceptionThrown = true;
                // Assert
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

        public async Task GetReviewSummary_When_Meaning_of_Signature_Is_Enabled_Should_Return_Empty_MoS_When_No_Possible_Mos_For_Participant()
        {
            // Arange
            var reviewName = "My Review";
            var reviewDescription = "My Description";
            var baselineId = 3;
            var totalArtifacts = 8;
            var revisionId = 999;
            var reviewStatus = ReviewStatus.Completed;

            _itemInfoRepositoryMock.Setup(i => i.GetItemDescription(ReviewId, UserId, true, int.MaxValue)).ReturnsAsync(reviewDescription);

            var reviewDetails = new ReviewSummaryDetails
            {
                BaselineId = baselineId,
                ReviewPackageStatus = ReviewPackageStatus.Active,
                ReviewParticipantRole = ReviewParticipantRole.Approver,
                TotalArtifacts = totalArtifacts,
                TotalViewable = 7,
                TotalReviewers = 5,
                ReviewStatus = reviewStatus,
                Approved = 5,
                Disapproved = 3,
                Pending = 2,
                RevisionId = revisionId,
                RequireAllArtifactsReviewed = true,
                RequireESignature = true,
                RequireMeaningOfSignature = true,
                ShowOnlyDescription = true
            };

            var param = new Dictionary<string, object> { { "reviewId", ReviewId }, { "userId", UserId } };
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

            _artifactVersionsRepositoryMock.Setup(r => r.GetVersionControlArtifactInfoAsync(ReviewId, null, UserId)).ReturnsAsync(reviewInfo);
            _artifactVersionsRepositoryMock.Setup(r => r.GetVersionControlArtifactInfoAsync(baselineId, null, UserId)).ReturnsAsync(baselineInfo);

            _cxn.SetupExecuteScalarAsync("GetReviewType", param, ReviewType.Formal);

            SetupPossibleMeaningOfSignaturesQuery(new[] { UserId }, new ParticipantMeaningOfSignatureResult[0]);

            // Act
            var review = await _reviewsRepository.GetReviewSummary(ReviewId, UserId);

            // Assert
            _cxn.Verify();

            Assert.AreEqual(0, review.MeaningOfSignatures.Count());
        }

        public async Task GetReviewSummary_When_Meaning_of_Signature_Is_Enabled_Should_Return_Empty_MoS_When_No_Mos_For_Participant()
        {
            // Arange
            var reviewName = "My Review";
            var reviewDescription = "My Description";
            var baselineId = 3;
            var totalArtifacts = 8;
            var revisionId = 999;
            var reviewStatus = ReviewStatus.Completed;

            _itemInfoRepositoryMock.Setup(i => i.GetItemDescription(ReviewId, UserId, true, int.MaxValue)).ReturnsAsync(reviewDescription);

            var reviewDetails = new ReviewSummaryDetails
            {
                BaselineId = baselineId,
                ReviewPackageStatus = ReviewPackageStatus.Active,
                ReviewParticipantRole = ReviewParticipantRole.Approver,
                TotalArtifacts = totalArtifacts,
                TotalViewable = 7,
                TotalReviewers = 5,
                ReviewStatus = reviewStatus,
                Approved = 5,
                Disapproved = 3,
                Pending = 2,
                RevisionId = revisionId,
                RequireAllArtifactsReviewed = true,
                RequireESignature = true,
                RequireMeaningOfSignature = true,
                ShowOnlyDescription = true
            };

            var param = new Dictionary<string, object> { { "reviewId", ReviewId }, { "userId", UserId } };
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

            _artifactVersionsRepositoryMock.Setup(r => r.GetVersionControlArtifactInfoAsync(ReviewId, null, UserId)).ReturnsAsync(reviewInfo);
            _artifactVersionsRepositoryMock.Setup(r => r.GetVersionControlArtifactInfoAsync(baselineId, null, UserId)).ReturnsAsync(baselineInfo);

            _cxn.SetupExecuteScalarAsync("GetReviewType", param, ReviewType.Formal);

            SetupPossibleMeaningOfSignaturesQuery(new[] { UserId }, new[]
            {
                new ParticipantMeaningOfSignatureResult() { RoleAssignmentId = 3 }
            });

            SetupParticipantMeaningOfSignaturesQuery(new[] { UserId }, new ParticipantMeaningOfSignatureResult[0]);

            // Act
            var review = await _reviewsRepository.GetReviewSummary(ReviewId, UserId);

            // Assert
            _cxn.Verify();

            Assert.AreEqual(0, review.MeaningOfSignatures.Count());
        }

        public async Task GetReviewSummary_When_Meaning_of_Signature_Is_Enabled_Should_Return_MoS_For_Participant()
        {
            // Arange
            var reviewName = "My Review";
            var reviewDescription = "My Description";
            var baselineId = 3;
            var totalArtifacts = 8;
            var revisionId = 999;
            var reviewStatus = ReviewStatus.Completed;

            _itemInfoRepositoryMock.Setup(i => i.GetItemDescription(ReviewId, UserId, true, int.MaxValue)).ReturnsAsync(reviewDescription);

            var reviewDetails = new ReviewSummaryDetails
            {
                BaselineId = baselineId,
                ReviewPackageStatus = ReviewPackageStatus.Active,
                ReviewParticipantRole = ReviewParticipantRole.Approver,
                TotalArtifacts = totalArtifacts,
                TotalViewable = 7,
                TotalReviewers = 5,
                ReviewStatus = reviewStatus,
                Approved = 5,
                Disapproved = 3,
                Pending = 2,
                RevisionId = revisionId,
                RequireAllArtifactsReviewed = true,
                RequireESignature = true,
                RequireMeaningOfSignature = true,
                ShowOnlyDescription = true
            };

            var param = new Dictionary<string, object> { { "reviewId", ReviewId }, { "userId", UserId } };
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

            _artifactVersionsRepositoryMock.Setup(r => r.GetVersionControlArtifactInfoAsync(ReviewId, null, UserId)).ReturnsAsync(reviewInfo);
            _artifactVersionsRepositoryMock.Setup(r => r.GetVersionControlArtifactInfoAsync(baselineId, null, UserId)).ReturnsAsync(baselineInfo);

            _cxn.SetupExecuteScalarAsync("GetReviewType", param, ReviewType.Formal);

            SetupPossibleMeaningOfSignaturesQuery(new[] { UserId }, new[]
            {
                new ParticipantMeaningOfSignatureResult() { ParticipantId = UserId, RoleAssignmentId = 3, RoleId = 4, MeaningOfSignatureId = 5, MeaningOfSignatureValue = "mos", RoleName = "role" }
            });

            SetupParticipantMeaningOfSignaturesQuery(new[] { UserId }, new[]
            {
                new ParticipantMeaningOfSignatureResult() { ParticipantId = UserId, RoleAssignmentId = 3 }
            });

            // Act
            var review = await _reviewsRepository.GetReviewSummary(ReviewId, UserId);

            // Assert
            _cxn.Verify();

            Assert.AreEqual(1, review.MeaningOfSignatures.Count());

            var mos = review.MeaningOfSignatures.First();

            Assert.AreEqual("mos (role)", mos.Label);
            Assert.AreEqual(4, mos.MeaningOfSignatureId);
            Assert.AreEqual(5, mos.MeaningOfSignatureId);
        }

        private void SetupParticipantMeaningOfSignaturesQuery(IEnumerable<int> participantIds, IEnumerable<ParticipantMeaningOfSignatureResult> result)
        {
            var parameters = new Dictionary<string, object>()
            {
                { "reviewId", ReviewId },
                { "userId", UserId },
                { "participantIds", SqlConnectionWrapper.ToDataTable(participantIds) }
            };

            _cxn.SetupQueryAsync("GetParticipantsMeaningOfSignatures", parameters, result);
        }

        private void SetupPossibleMeaningOfSignaturesQuery(IEnumerable<int> participantIds, IEnumerable<ParticipantMeaningOfSignatureResult> result)
        {
            var parameters = new Dictionary<string, object>()
            {
                { "participantIds", SqlConnectionWrapper.ToDataTable(participantIds) }
            };

            _cxn.SetupQueryAsync("GetPossibleMeaningOfSignaturesForParticipants", parameters, result);
        }

        #endregion

        #region GetReviewSummaryMetrics

        [TestMethod]
        public async Task GetReviewSummaryMetrics_Success()
        {
            // Arrange
            #region  Define local variables
            var reviewId = 1;
            var userId = 2;
            var totalArtifacts = 2;
            var revisionId = 999;
            var reviewStatus = ReviewStatus.InProgress;
            var reviewPackageStatus = ReviewPackageStatus.Active;
            #endregion

            #region  Mock the method GetArtifactInfo & RevisionId
            var reviewInfo = new VersionControlArtifactInfo
            {
                PredefinedType = ItemTypePredefined.ArtifactReviewPackage
            };
            _artifactVersionsRepositoryMock.Setup(r => r.GetVersionControlArtifactInfoAsync(reviewId, null, userId)).ReturnsAsync(reviewInfo);
            #endregion

            #region Mock the method GetReviewSummaryMetricsAsync
            var resultSummaryMetrics = new FlatReviewSummaryMetrics
            {
                TotalArtifacts = totalArtifacts,
                ReviewPackageStatus = reviewPackageStatus.ToString(),
                ReviewStatus = reviewStatus.ToString(),
                RevisionId = revisionId,
                ArtifactsApprovedByAll = 1,
                ArtifactsDisapproved = 1,
                ArtifactsPending = 0,
                ArtifactsViewedBySome = 0
            };

            var param = new Dictionary<string, object> { { "reviewId", reviewId }, { "userId", userId } };
            _cxn.SetupQueryAsync("GetReviewSummaryMetrics", param, Enumerable.Repeat(resultSummaryMetrics, 1));
            #endregion

            // Act
            var review = await _reviewsRepository.GetReviewSummaryMetrics(reviewId, userId);

            // Assert
            _cxn.Verify();

            Assert.AreEqual(reviewStatus, review.Status);
            Assert.AreEqual(revisionId, review.RevisionId);
            Assert.AreEqual(2, review.Artifacts.Total);
            Assert.AreEqual(1, review.Artifacts.ArtifactStatus.Approved);
            Assert.AreEqual(1, review.Artifacts.ArtifactStatus.Disapproved);
            Assert.AreEqual(0, review.Artifacts.ArtifactStatus.Pending);
            Assert.AreEqual(0, review.Artifacts.ArtifactStatus.ViewedSome);
        }

        [TestMethod]
        public async Task GetReviewSummaryMetrics_ResourceNotFoundException()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var isExceptionThrown = false;

            var reviewInfo = new VersionControlArtifactInfo
            {
                PredefinedType = ItemTypePredefined.Actor // Invalid Artifact Type
            };
            _artifactVersionsRepositoryMock.Setup(r => r.GetVersionControlArtifactInfoAsync(reviewId, null, userId)).ReturnsAsync(reviewInfo);

            // Act
            try
            {
                await _reviewsRepository.GetReviewSummaryMetrics(reviewId, userId);
            }
            catch (ResourceNotFoundException ex)
            {
                isExceptionThrown = true;
                // Assert
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
        public async Task GetReviewSummaryMetrics_DraftReviewException()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var isExceptionThrown = false;

            var reviewInfo = new VersionControlArtifactInfo
            {
                PredefinedType = ItemTypePredefined.ArtifactReviewPackage
            };
            _artifactVersionsRepositoryMock.Setup(r => r.GetVersionControlArtifactInfoAsync(reviewId, null, userId)).ReturnsAsync(reviewInfo);

            var resultSummaryMetrics = new FlatReviewSummaryMetrics
            {
                ReviewPackageStatus = ReviewPackageStatus.Draft.ToString() // Draft Status
            };

            var param = new Dictionary<string, object> { { "reviewId", reviewId }, { "userId", userId } };
            _cxn.SetupQueryAsync("GetReviewSummaryMetrics", param, Enumerable.Repeat(resultSummaryMetrics, 1));

            // Act
            try
            {
                await _reviewsRepository.GetReviewSummaryMetrics(reviewId, userId);
            }
            catch (ResourceNotFoundException ex)
            {
                isExceptionThrown = true;
                // Assert
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

        #endregion

        #region GetReviewTableOfContentAsync

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetReviewTableOfContentAsync_ReviewNotFound()
        {
            await TestGetReviewTableOfContentErrorsAsync(1, ErrorCodes.ResourceNotFound);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetReviewTableOfContentAsync_ReviewNotActive()
        {
            await TestGetReviewTableOfContentErrorsAsync(2, ErrorCodes.ResourceNotFound);
        }

        private static async Task TestGetReviewTableOfContentErrorsAsync(int retResult, int expectedErrorCode)
        {
            // Arrange
            const int reviewId = 11;
            const int revisionId = 22;
            const int userId = 33;

            var pagination = new Pagination
            {
                Offset = 0,
                Limit = 50
            };
            const int refreshInterval = 66;

            var appSettingsRepoMock = new Mock<IApplicationSettingsRepository>();
            appSettingsRepoMock.Setup(m => m.GetValue(
                SqlReviewsRepository.ReviewArtifactHierarchyRebuildIntervalInMinutesKey,
                SqlReviewsRepository.DefaultReviewArtifactHierarchyRebuildIntervalInMinutes))
                .Returns(Task.FromResult(refreshInterval));

            var cxn = new SqlConnectionWrapperMock();

            var prm = new Dictionary<string, object>
            {
                { "@reviewId", reviewId },
                { "@revisionId", revisionId },
                { "@userId", userId },
                { "offset", pagination.Offset },
                { "@limit", pagination.Limit },
                { "@refreshInterval", refreshInterval }
            };

            var outPrm = new Dictionary<string, object>
            {
                { "@total", 0 },
                { "@retResult", retResult }
            };

            var testResult = new ReviewTableOfContentItem[] { };
            cxn.SetupQueryAsync("GetReviewTableOfContent", prm, testResult, outPrm);

            var repository = new SqlReviewsRepository(cxn.Object, null, null, null, appSettingsRepoMock.Object, null, null, null, null, null);

            try
            {
                // Act
                await repository.GetReviewTableOfContent(reviewId, revisionId, userId, pagination);
            }
            catch (ExceptionWithErrorCode e)
            {
                // Assert
                Assert.AreEqual(expectedErrorCode, e.ErrorCode);
                throw;
            }
        }

        [TestMethod]
        public async Task GetReviewArtifactIndex_Success()
        {
            var reviewId = 1;
            var userId = 1;
            var revisionId = 1;
            var artifactId = 1;
            var refreshInterval = 20;
            var reviewInfo = new VersionControlArtifactInfo
            {
                PredefinedType = ItemTypePredefined.ArtifactReviewPackage,
                VersionCount = 1
            };

            _artifactVersionsRepositoryMock.Setup(r => r.GetVersionControlArtifactInfoAsync(reviewId, null, userId)).ReturnsAsync(reviewInfo);

            _applicationSettingsRepositoryMock.Setup(s => s.GetValue("ReviewArtifactHierarchyRebuildIntervalInMinutes", refreshInterval)).ReturnsAsync(refreshInterval);

            var inpParams = new Dictionary<string, object>
            {
                { "@reviewId", reviewId },
                { "@revisionId", revisionId },
                { "@userId", userId },
                { "@artifactId", artifactId },
                { "@refreshInterval", refreshInterval }
            };

            var testResult = new ReviewArtifactIndex
            {
                Index = 1,
                Total = 10
            };

            var outParams = new Dictionary<string, object>
            {
                { "@result", 0 },
            };

            _cxn.SetupQueryAsync("GetReviewArtifactIndex", inpParams, Enumerable.Repeat(testResult, 1), outParams);

            // Act
            var result = await _reviewsRepository.GetReviewArtifactIndexAsync(reviewId, revisionId, artifactId, userId);

            // Assert
            _cxn.Verify();

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Index, 1);
            Assert.AreEqual(result.Total, 10);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetReviewArtifactIndex_NotFoundReview()
        {
            var reviewId = 1;
            var userId = 1;
            var revisionId = 1;
            var artifactId = 1;

            _artifactVersionsRepositoryMock
                .Setup(r => r.GetVersionControlArtifactInfoAsync(reviewId, null, userId))
                .Throws(new ResourceNotFoundException("Item(Id:1) is not found.", ErrorCodes.ResourceNotFound));

            // Act
            await _reviewsRepository.GetReviewArtifactIndexAsync(reviewId, revisionId, artifactId, userId);

            // Assert
            _cxn.Verify();
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetReviewArtifactIndex_ArtifactNotFound()
        {
            var reviewId = 1;
            var userId = 1;
            var revisionId = 1;
            var artifactId = 1;
            var refreshInterval = 20;
            var reviewInfo = new VersionControlArtifactInfo
            {
                PredefinedType = ItemTypePredefined.ArtifactReviewPackage,
                VersionCount = 1
            };

            _artifactVersionsRepositoryMock.Setup(r => r.GetVersionControlArtifactInfoAsync(reviewId, null, userId)).ReturnsAsync(reviewInfo);

            _applicationSettingsRepositoryMock.Setup(s => s.GetValue("ReviewArtifactHierarchyRebuildIntervalInMinutes", 20)).ReturnsAsync(20);

            var inpParams = new Dictionary<string, object>
            {
                { "@reviewId", reviewId },
                { "@revisionId", revisionId },
                { "@userId", userId },
                { "@artifactId", artifactId },
                { "@refreshInterval", refreshInterval }
            };

            var outParams = new Dictionary<string, object>
            {
                { "@result", 1 },
            };
            var testResult = new ReviewArtifactIndex[] { };

            _cxn.SetupQueryAsync("GetReviewArtifactIndex", inpParams, testResult, outParams);

            // Act
            await _reviewsRepository.GetReviewArtifactIndexAsync(reviewId, revisionId, artifactId, userId);

            // Assert
            _cxn.Verify();
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetReviewArtifactIndex_FailedOnPermissions()
        {
            var reviewId = 1;
            var userId = 1;
            var revisionId = 1;
            var artifactId = 1;
            var refreshInterval = 20;

            var reviewInfo = new VersionControlArtifactInfo
            {
                PredefinedType = ItemTypePredefined.ArtifactReviewPackage,
                VersionCount = 1
            };

            _artifactVersionsRepositoryMock.Setup(r => r.GetVersionControlArtifactInfoAsync(reviewId, null, userId)).ReturnsAsync(reviewInfo);

            _applicationSettingsRepositoryMock.Setup(s => s.GetValue("ReviewArtifactHierarchyRebuildIntervalInMinutes", refreshInterval)).ReturnsAsync(refreshInterval);

            var inpParams = new Dictionary<string, object>
            {
                { "@reviewId", reviewId },
                { "@revisionId", revisionId },
                { "@userId", userId },
                { "@artifactId", artifactId },
                { "@refreshInterval", refreshInterval }
            };

            var outParams = new Dictionary<string, object>
            {
                { "@result", 3 },
            };
            var testResult = new ReviewArtifactIndex[] { };

            _cxn.SetupQueryAsync("GetReviewArtifactIndex", inpParams, testResult, outParams);

            // Act
            await _reviewsRepository.GetReviewArtifactIndexAsync(reviewId, revisionId, artifactId, userId);

            // Assert
            _cxn.Verify();
        }

        [TestMethod]
        public async Task GetReviewTableOfContentArtifactIndex_Success()
        {
            var reviewId = 1;
            var userId = 1;
            var revisionId = 1;
            var artifactId = 1;
            var refreshInterval = 20;
            var reviewInfo = new VersionControlArtifactInfo
            {
                PredefinedType = ItemTypePredefined.ArtifactReviewPackage,
                VersionCount = 1
            };

            _artifactVersionsRepositoryMock.Setup(r => r.GetVersionControlArtifactInfoAsync(reviewId, null, userId)).ReturnsAsync(reviewInfo);

            _applicationSettingsRepositoryMock.Setup(s => s.GetValue("ReviewArtifactHierarchyRebuildIntervalInMinutes", refreshInterval)).ReturnsAsync(refreshInterval);

            var inpParams = new Dictionary<string, object>
            {
                { "@reviewId", reviewId },
                { "@revisionId", revisionId },
                { "@userId", userId },
                { "@artifactId", artifactId },
                { "@refreshInterval", refreshInterval }
            };

            var testResult = new ReviewArtifactIndex
            {
                Index = 1,
                Total = 10
            };

            var outParams = new Dictionary<string, object>
            {
                { "@result", 0 }
            };

            _cxn.SetupQueryAsync("GetReviewTableOfContentArtifactIndex", inpParams, Enumerable.Repeat(testResult, 1), outParams);

            // Act
            var result = await _reviewsRepository.GetReviewTableOfContentArtifactIndexAsync(reviewId, revisionId, artifactId, userId);

            // Assert
            _cxn.Verify();

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Index, 1);
            Assert.AreEqual(result.Total, 10);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetReviewTableOfContentArtifactIndex_NotFoundReview()
        {
            var reviewId = 1;
            var userId = 1;
            var revisionId = 1;
            var artifactId = 1;
            var reviewInfo = new VersionControlArtifactInfo
            {
                PredefinedType = ItemTypePredefined.ArtifactReviewPackage,
                VersionCount = 1
            };

            _artifactVersionsRepositoryMock
                .Setup(r => r.GetVersionControlArtifactInfoAsync(reviewId, null, userId))
                .Throws(new ResourceNotFoundException("Item(Id:1) is not found.", ErrorCodes.ResourceNotFound));

            // Act
            await _reviewsRepository.GetReviewTableOfContentArtifactIndexAsync(reviewId, revisionId, artifactId, userId);

            // Assert
            _cxn.Verify();
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetReviewTableOfContentArtifactIndex_ArtifactNotFound()
        {
            var reviewId = 1;
            var userId = 1;
            var revisionId = 1;
            var artifactId = 1;
            var refreshInterval = 20;
            var reviewInfo = new VersionControlArtifactInfo
            {
                PredefinedType = ItemTypePredefined.ArtifactReviewPackage,
                VersionCount = 1
            };

            _artifactVersionsRepositoryMock.Setup(r => r.GetVersionControlArtifactInfoAsync(reviewId, null, userId)).ReturnsAsync(reviewInfo);

            _applicationSettingsRepositoryMock.Setup(s => s.GetValue("ReviewArtifactHierarchyRebuildIntervalInMinutes", refreshInterval)).ReturnsAsync(refreshInterval);

            var inpParams = new Dictionary<string, object>
            {
                { "@reviewId", reviewId },
                { "@revisionId", revisionId },
                { "@userId", userId },
                { "@artifactId", artifactId },
                { "@refreshInterval", refreshInterval }
            };

            var testResult = new ReviewArtifactIndex[] { };

            var outParams = new Dictionary<string, object>
            {
                { "@result", 1 },
            };
            _cxn.SetupQueryAsync("GetReviewTableOfContentArtifactIndex", inpParams, testResult, outParams);

            // Act
            await _reviewsRepository.GetReviewTableOfContentArtifactIndexAsync(reviewId, revisionId, artifactId, userId);

            // Assert
            _cxn.Verify();
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetReviewTableOfContentArtifactIndex_FailedOnPermissions()
        {
            const int reviewId = 1;
            const int userId = 1;
            const int revisionId = 1;
            const int artifactId = 1;
            const int refreshInterval = 20;

            var reviewInfo = new VersionControlArtifactInfo
            {
                PredefinedType = ItemTypePredefined.ArtifactReviewPackage,
                VersionCount = 1
            };

            _artifactVersionsRepositoryMock.Setup(r => r.GetVersionControlArtifactInfoAsync(reviewId, null, userId)).ReturnsAsync(reviewInfo);

            _applicationSettingsRepositoryMock.Setup(s => s.GetValue("ReviewArtifactHierarchyRebuildIntervalInMinutes", refreshInterval)).ReturnsAsync(refreshInterval);

            var inpParams = new Dictionary<string, object>
            {
                { "@reviewId", reviewId },
                { "@revisionId", revisionId },
                { "@userId", userId },
                { "@artifactId", artifactId },
                { "@refreshInterval", refreshInterval },
            };
            var outParams = new Dictionary<string, object>
            {
                { "@result", 3 },
            };
            var testResult = new ReviewArtifactIndex[] { };

            _cxn.SetupQueryAsync("GetReviewTableOfContentArtifactIndex", inpParams, testResult, outParams);

            // Act
            await _reviewsRepository.GetReviewTableOfContentArtifactIndexAsync(reviewId, revisionId, artifactId, userId);

            // Assert
            _cxn.Verify();
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetReviewTableOfContentArtifactIndex_FailedOnDraft()
        {
            var reviewId = 1;
            var userId = 1;
            var revisionId = 1;
            var artifactId = 1;
            // var refreshInterval = 20;
            var reviewInfo = new VersionControlArtifactInfo
            {
                PredefinedType = ItemTypePredefined.ArtifactReviewPackage,
                VersionCount = 0
            };

            _artifactVersionsRepositoryMock.Setup(r => r.GetVersionControlArtifactInfoAsync(reviewId, null, userId)).ReturnsAsync(reviewInfo);
            _applicationSettingsRepositoryMock.Setup(s => s.GetValue("ReviewArtifactHierarchyRebuildIntervalInMinutes", 20)).ReturnsAsync(20);

            // Act
            await _reviewsRepository.GetReviewTableOfContentArtifactIndexAsync(reviewId, revisionId, artifactId, userId);

            // Assert
            _cxn.Verify();
        }

        #endregion

        #region GetReviewedArtifact

        [TestMethod]
        public async Task GetReviewedArtifacts_AuthorizationException()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var revisionId = 999;
            var pagination = new Pagination
            {
                Offset = 0,
                Limit = 50
            };

            _itemInfoRepositoryMock.Setup(i => i.GetRevisionId(reviewId, userId, null, null)).ReturnsAsync(revisionId);

            _applicationSettingsRepositoryMock.Setup(s => s.GetValue("ReviewArtifactHierarchyRebuildIntervalInMinutes", 20)).ReturnsAsync(20);

            var param = new Dictionary<string, object>
            {
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

            var outputParams = new Dictionary<string, object> {
                { "numResult", 2 },
                { "isFormal", false }
            };
            _cxn.SetupQueryAsync("GetReviewArtifacts", param, reviewArtifacts, outputParams);

            SetupArtifactPermissionsCheck(new[] { artifact1.Id, artifact2.Id, reviewId }, userId, new Dictionary<int, RolePermissions>());

            // Act
            var isExceptionThrown = false;
            try
            {
                await _reviewsRepository.GetReviewedArtifacts(reviewId, userId, pagination, revisionId);
            }
            catch (AuthorizationException ex)
            {
                isExceptionThrown = true;
                // Assert
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
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var revisionId = 999;

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

            var outputParams = new Dictionary<string, object> {
                { "numResult", 2 },
                { "isFormal", false }
            };

            var reviewArtifacts = new List<ReviewedArtifact>();
            var artifact1 = new ReviewedArtifact { Id = 2 };
            reviewArtifacts.Add(artifact1);
            var artifact2 = new ReviewedArtifact { Id = 3 };
            reviewArtifacts.Add(artifact2);

            _cxn.SetupQueryAsync("GetReviewArtifacts", param, reviewArtifacts, outputParams);

            var reviewArtifacts2 = new List<ReviewedArtifact>();
            var reviewArtifact1 = new ReviewedArtifact { Id = 2 };
            reviewArtifacts2.Add(reviewArtifact1);
            var reviewArtifact2 = new ReviewedArtifact { Id = 3 };
            reviewArtifacts2.Add(reviewArtifact2);

            var param2 = new Dictionary<string, object> {
                { "reviewId", reviewId },
                { "userId", userId },
                { "revisionId", revisionId },
                { "itemIds", SqlConnectionWrapper.ToDataTable(new[] { 2, 3 }) },
            };
            _cxn.SetupQueryAsync("GetReviewArtifactsByParticipant", param2, reviewArtifacts2);

            var permisions = new Dictionary<int, RolePermissions>
            {
                { 1, RolePermissions.Read }
            };

            SetupArtifactPermissionsCheck(new[] { reviewArtifact1.Id, reviewArtifact2.Id, reviewId }, userId, permisions);

            // Act
            var artifacts = await _reviewsRepository.GetReviewedArtifacts(reviewId, userId, pagination, revisionId);

            Assert.AreEqual(2, artifacts.Total);

            Assert.AreEqual(2, artifacts.Items.ElementAt(0).Id);
            Assert.AreEqual(3, artifacts.Items.ElementAt(1).Id);
        }

        #endregion

        #region AddParticipantsToReviewAsync

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task AddParticipantsToReviewAsync_Should_Throw_If_No_Users_Or_Groups()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;

            var addParticipantsParameter = new AddParticipantsParameter
            {
                GroupIds = new int[0],
                UserIds = new int[0]
            };

            // Act
            await _reviewsRepository.AddParticipantsToReviewAsync(reviewId, userId, addParticipantsParameter);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task AddParticipantsToReviewAsync_Should_Throw_If_Review_Doesnt_Exist()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;

            _artifactRepositoryMock
                .Setup(m => m.GetArtifactBasicDetails(ReviewId, UserId))
                .ReturnsAsync(() => null);

            var addParticipantsParameter = new AddParticipantsParameter
            {
                GroupIds = new int[0],
                UserIds = new[] { userId }
            };

            // Act
            await _reviewsRepository.AddParticipantsToReviewAsync(reviewId, userId, addParticipantsParameter);
        }

        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task AddParticipantsToReviewAsync_Should_Throw_If_Review_Is_Not_Locked_By_User()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;

            var addParticipantsParameter = new AddParticipantsParameter
            {
                GroupIds = new int[0],
                UserIds = new[] { userId }
            };

            var queryParameters = new Dictionary<string, object>
            {
                { "@reviewId", reviewId },
                { "@userId", userId },
                { "@revisionId", int.MaxValue },
                { "@includeDrafts", true }
            };

            var queryResult = new List<string> { null };

            _cxn.SetupQueryAsync("GetReviewPackageRawData", queryParameters, queryResult);

            _artifactRepositoryMock.Setup(artifactRepository => artifactRepository.IsArtifactLockedByUserAsync(reviewId, userId)).ReturnsAsync(false);

            // Act
            await _reviewsRepository.AddParticipantsToReviewAsync(reviewId, userId, addParticipantsParameter);
        }

        [TestMethod]
        public async Task AddParticipantsToReviewAsync_Should_Succeed_When_Returned_Xml_Is_Null()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            _artifactDetails.LockedByUserId = userId;

            var addParticipantsParameter = new AddParticipantsParameter
            {
                GroupIds = new int[0],
                UserIds = new[] { userId }
            };

            SetupGetReviewDataQuery(reviewId, userId, null, null);

            SetupUpdateReviewXmlQuery(reviewId, userId, 1,
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Reviwers><ReviewerRawData><Permission>Reviewer</Permission><UserId>2</UserId></ReviewerRawData></Reviwers></ReviewPackageRawData>");

            // Act
            var addParticipantResult = await _reviewsRepository.AddParticipantsToReviewAsync(reviewId, userId, addParticipantsParameter);

            // Assert
            _cxn.Verify();

            Assert.AreEqual(1, addParticipantResult.ParticipantCount);
            Assert.AreEqual(0, addParticipantResult.AlreadyIncludedCount);
        }

        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task AddParticipantsToReviewAsync_Should_Throw_If_Review_Is_Closed()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;

            var addParticipantsParameter = new AddParticipantsParameter
            {
                GroupIds = new int[0],
                UserIds = new[] { userId }
            };

            SetupGetReviewDataQuery(reviewId, userId, null,
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Status>Closed</Status></ReviewPackageRawData>");

            // Act
            await _reviewsRepository.AddParticipantsToReviewAsync(reviewId, userId, addParticipantsParameter);
        }

        [TestMethod]
        public async Task AddParticipantsToReviewAsync_Should_Return_Non_Existant_Users_If_Users_Are_Deleted_Or_NonExistant()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            _artifactDetails.LockedByUserId = userId;

            var addParticipantsParameter = new AddParticipantsParameter
            {
                GroupIds = new int[0],
                UserIds = new[] { 2, 3, 4 }
            };

            SetupGetReviewDataQuery(reviewId, userId, null, null);

            _usersRepositoryMock.Setup(repo => repo.FindNonExistentUsersAsync(new[] { 2, 3, 4 })).ReturnsAsync(new[] { 2, 3, 4 });

            // Act
            var addParticipantsResult = await _reviewsRepository.AddParticipantsToReviewAsync(reviewId, userId, addParticipantsParameter);

            // Assert
            Assert.AreEqual(addParticipantsResult.NonExistentUsers, 3);
            Assert.AreEqual(addParticipantsResult.ParticipantCount, 0);
            Assert.AreEqual(addParticipantsResult.AlreadyIncludedCount, 0);
        }

        [TestMethod]
        public async Task AddParticipantsToReviewAsync_Should_Add_Users()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            _artifactDetails.LockedByUserId = userId;

            var addParticipantsParameter = new AddParticipantsParameter
            {
                GroupIds = new int[0],
                UserIds = new[] { 2, 3 }
            };

            SetupGetReviewDataQuery(reviewId, userId, null,
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"/>");

            SetupUpdateReviewXmlQuery(reviewId, userId, 1,
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Reviwers><ReviewerRawData><Permission>Reviewer</Permission><UserId>2</UserId></ReviewerRawData><ReviewerRawData><Permission>Reviewer</Permission><UserId>3</UserId></ReviewerRawData></Reviwers></ReviewPackageRawData>");

            // Act
            var addParticipantResult = await _reviewsRepository.AddParticipantsToReviewAsync(reviewId, userId, addParticipantsParameter);

            // Assert
            Assert.AreEqual(2, addParticipantResult.ParticipantCount);
            Assert.AreEqual(0, addParticipantResult.AlreadyIncludedCount);
        }

        [TestMethod]
        public async Task AddParticipantsToReviewAsync_Should_Add_Users_From_Groups()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            _artifactDetails.LockedByUserId = userId;

            var addParticipantsParameter = new AddParticipantsParameter
            {
                GroupIds = new[] { 6, 7 },
                UserIds = new int[0]
            };

            SetupGetReviewDataQuery(reviewId, userId, null,
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"/>");

            SetupUpdateReviewXmlQuery(reviewId, userId, 1,
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Reviwers><ReviewerRawData><Permission>Reviewer</Permission><UserId>3</UserId></ReviewerRawData><ReviewerRawData><Permission>Reviewer</Permission><UserId>4</UserId></ReviewerRawData><ReviewerRawData><Permission>Reviewer</Permission><UserId>5</UserId></ReviewerRawData></Reviwers></ReviewPackageRawData>");

            _usersRepositoryMock.Setup(repo => repo.GetUserInfosFromGroupsAsync(new[] { 6, 7 })).ReturnsAsync(new List<UserInfo>
            {
                new UserInfo { UserId = 3 },
                new UserInfo { UserId = 4 },
                new UserInfo { UserId = 5 }
            });

            // Act
            var addParticipantResult = await _reviewsRepository.AddParticipantsToReviewAsync(reviewId, userId, addParticipantsParameter);

            // Assert
            Assert.AreEqual(3, addParticipantResult.ParticipantCount);
            Assert.AreEqual(0, addParticipantResult.AlreadyIncludedCount);
        }

        [TestMethod]
        public async Task AddParticipantsToReviewAsync_Should_Not_Add_Duplicates_From_Users_And_Groups()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            _artifactDetails.LockedByUserId = userId;

            var addParticipantsParameter = new AddParticipantsParameter
            {
                GroupIds = new[] { 4 },
                UserIds = new[] { 1, 2 }
            };

            SetupGetReviewDataQuery(reviewId, userId, null,
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"/>");

            SetupUpdateReviewXmlQuery(reviewId, userId, 1,
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Reviwers><ReviewerRawData><Permission>Reviewer</Permission><UserId>1</UserId></ReviewerRawData><ReviewerRawData><Permission>Reviewer</Permission><UserId>2</UserId></ReviewerRawData></Reviwers></ReviewPackageRawData>");

            _usersRepositoryMock.Setup(repo => repo.GetUserInfosFromGroupsAsync(new[] { 4 })).ReturnsAsync(new List<UserInfo>
            {
                new UserInfo { UserId = 1 },
                new UserInfo { UserId = 2 }
            });

            // Act
            var addParticipantResult = await _reviewsRepository.AddParticipantsToReviewAsync(reviewId, userId, addParticipantsParameter);

            // Assert
            Assert.AreEqual(2, addParticipantResult.ParticipantCount);
            Assert.AreEqual(0, addParticipantResult.AlreadyIncludedCount);
        }

        [TestMethod]
        public async Task AddParticipantsToReviewAsync_Should_Not_Add_Already_Existing_Users()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            _artifactDetails.LockedByUserId = userId;

            var addParticipantsParameter = new AddParticipantsParameter
            {
                GroupIds = new int[0],
                UserIds = new[] { 2, 3 }
            };

            SetupGetReviewDataQuery(reviewId, userId, null,
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Reviwers><ReviewerRawData><Permission>Reviewer</Permission><UserId>2</UserId></ReviewerRawData><ReviewerRawData><Permission>Reviewer</Permission><UserId>3</UserId></ReviewerRawData></Reviwers></ReviewPackageRawData>");

            // Act
            var addParticipantResult = await _reviewsRepository.AddParticipantsToReviewAsync(reviewId, userId, addParticipantsParameter);

            // Assert
            Assert.AreEqual(0, addParticipantResult.ParticipantCount);
            Assert.AreEqual(2, addParticipantResult.AlreadyIncludedCount);
        }

        [TestMethod]
        public async Task AddParticipants_UsersAndGroups_Success()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            _artifactDetails.LockedByUserId = userId;
            var content = new AddParticipantsParameter
            {
                UserIds = new[] { 1, 2 },
                GroupIds = new[] { 2, 4 }
            };

            SetupGetReviewDataQuery(reviewId, userId, null,
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"/>");

            SetupUpdateReviewXmlQuery(reviewId, userId, 1,
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Reviwers><ReviewerRawData><Permission>Reviewer</Permission><UserId>1</UserId></ReviewerRawData><ReviewerRawData><Permission>Reviewer</Permission><UserId>2</UserId></ReviewerRawData><ReviewerRawData><Permission>Reviewer</Permission><UserId>4</UserId></ReviewerRawData><ReviewerRawData><Permission>Reviewer</Permission><UserId>5</UserId></ReviewerRawData></Reviwers></ReviewPackageRawData>");

            _usersRepositoryMock.Setup(repo => repo.GetUserInfosFromGroupsAsync(new[] { 2, 4 })).ReturnsAsync(new List<UserInfo>
            {
                new UserInfo() { UserId = 4 },
                new UserInfo() { UserId = 5 }
            });

            // Act
            var result = await _reviewsRepository.AddParticipantsToReviewAsync(reviewId, userId, content);

            // Assert
            _cxn.Verify();

            Assert.AreEqual(4, result.ParticipantCount);
            Assert.AreEqual(0, result.AlreadyIncludedCount);
        }

        [TestMethod]
        public async Task AddParticipants_UsersExist_Success()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            _artifactDetails.LockedByUserId = userId;
            var content = new AddParticipantsParameter
            {
                UserIds = new[] { 1, 2 }
            };

            SetupGetReviewDataQuery(reviewId, userId, null,
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"></ReviewPackageRawData>");

            SetupUpdateReviewXmlQuery(reviewId, userId, 1,
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Reviwers><ReviewerRawData><Permission>Reviewer</Permission><UserId>1</UserId></ReviewerRawData><ReviewerRawData><Permission>Reviewer</Permission><UserId>2</UserId></ReviewerRawData></Reviwers></ReviewPackageRawData>");

            // Act
            var result = await _reviewsRepository.AddParticipantsToReviewAsync(reviewId, userId, content);

            // Assert
            _cxn.Verify();

            Assert.AreEqual(2, result.ParticipantCount);
            Assert.AreEqual(0, result.AlreadyIncludedCount);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task AddParticipantsToReviewAsync_Should_Fail_On_Update()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            _artifactDetails.LockedByUserId = userId;

            var content = new AddParticipantsParameter
            {
                UserIds = new[] { 1, 2 },
                GroupIds = new[] { 2, 4 }
            };

            SetupGetReviewDataQuery(reviewId, userId, null,
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"/>");

            SetupUpdateReviewXmlQuery(reviewId, userId, -1,
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Reviwers><ReviewerRawData><Permission>Reviewer</Permission><UserId>1</UserId></ReviewerRawData><ReviewerRawData><Permission>Reviewer</Permission><UserId>2</UserId></ReviewerRawData></Reviwers></ReviewPackageRawData>");

            await _reviewsRepository.AddParticipantsToReviewAsync(reviewId, userId, content);
        }

        [TestMethod]
        public async Task AddParticipants_ShouldThrowUserCannotModifyReviewException()
        {
            // Arrange
            _hasEditPermissions = false;
            _artifactDetails.LockedByUserId = UserId;

            var content = new AddParticipantsParameter
            {
                UserIds = new[] { 1, 2 }
            };

            SetupGetReviewDataQuery(ReviewId, UserId, null,
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"/>");

            SetupUpdateReviewXmlQuery(ReviewId, UserId, 1,
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Reviwers><ReviewerRawData><Permission>Reviewer</Permission><UserId>1</UserId></ReviewerRawData><ReviewerRawData><Permission>Reviewer</Permission><UserId>2</UserId></ReviewerRawData></Reviwers></ReviewPackageRawData>");

            // Act
            try
            {
                await _reviewsRepository.AddParticipantsToReviewAsync(ReviewId, UserId, content);
            }
            catch (AuthorizationException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.UnauthorizedAccess, ex.ErrorCode);

                return;
            }

            Assert.Fail("An AuthorizationException was not thrown.");
        }

        private void SetupGetReviewDataQuery(int reviewId, int userId, string reviewContentsXml, string reviewPackageRawDataXml, int? baselineId = null)
        {
            var queryParameters = new Dictionary<string, object>
            {
                { "@reviewIds", SqlConnectionWrapper.ToDataTable(new[] { reviewId }) },
                { "@userId", userId },
                { "@revisionId", int.MaxValue },
                { "@addDrafts", true }
            };

            var reviewData = new ReviewData
            {
                Id = reviewId,
                ReviewContentsXml = reviewContentsXml,
                ReviewPackageRawDataXml = reviewPackageRawDataXml,
                BaselineId = baselineId
            };
            var reviewsDataResult = new[] { reviewData };
            _cxn.SetupQueryAsync("GetReviewsData", queryParameters, reviewsDataResult);
        }

        private void SetupUpdateReviewXmlQuery(int reviewId, int userId, int returnValue, string xmlString)
        {
            var updateParameters = new Dictionary<string, object>
            {
                { "@reviewId", reviewId },
                { "@userId", userId },
                { "@xmlString", xmlString }
            };

            var outParameters = new Dictionary<string, object>
            {
                { "returnValue", returnValue }
            };

            _cxn.SetupExecuteAsync("UpdateReviewPackageRawData", updateParameters, -1, outParameters);
        }

        #endregion

        #region AddArtifactsToReviewAsync

        [TestMethod]
        public async Task AddArtifacts_AndCollections_Success()
        {
            // Arrange
            var ids = new[] { 1, 2 };
            var content = new AddArtifactsParameter
            {
                ArtifactIds = ids,
                AddChildren = false
            };

            var param = new Dictionary<string, object>
            {
                { "reviewId", ReviewId },
                { "userId", UserId },
                { "xmlArtifacts", "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewContents xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Artifacts><CA><Id>3</Id></CA><CA><Id>4</Id></CA><CA><ANR>true</ANR><Id>1</Id></CA><CA><ANR>true</ANR><Id>2</Id></CA></Artifacts></RDReviewContents>" }
            };
            _cxn.SetupExecuteAsync("UpdateReviewArtifacts", param, 0);

            var reviewContentsXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewContents xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Artifacts><CA><Id>3</Id></CA><CA><Id>4</Id></CA></Artifacts></RDReviewContents>";
            SetupGetReviewDataQuery(ReviewId, UserId, reviewContentsXml, null);

            var effectiveArtifactIdsQueryParameters = new Dictionary<string, object>
            {
                { "@artifactIds",  SqlConnectionWrapper.ToDataTable(ids) },
                { "@userId", UserId },
                { "@projectId", ProjectId }
            };

            IEnumerable<int> artifactIds = new List<int> { 1, 2 };
            IEnumerable<int> unpublished = new List<int> { 0 };
            IEnumerable<int> nonexistent = new List<int> { 0 };
            IEnumerable<bool> isBaselineAdded = new List<bool> { false };

            var outParameters = new Dictionary<string, object>
            {
               { "ArtifactIds",  ids },
               { "Unpublished", 0 },
               { "Nonexistent", 0 },
               { "IsBaselineAdded", false }
            };

            var mockResult = new Tuple<IEnumerable<int>, IEnumerable<int>, IEnumerable<int>, IEnumerable<bool>>(artifactIds, unpublished, nonexistent, isBaselineAdded);

            _cxn.SetupQueryMultipleAsync("GetEffectiveArtifactIds", effectiveArtifactIdsQueryParameters, mockResult, outParameters);

            // Act
            var result = await _reviewsRepository.AddArtifactsToReviewAsync(ReviewId, UserId, content);

            // Assert
            _cxn.Verify();

            Assert.AreEqual(2, result.ArtifactCount);
            Assert.AreEqual(0, result.AlreadyIncludedArtifactCount);
        }

        [TestMethod]
        public async Task AddArtifactsToReviewAsync_ShouldThrowBadRequestException()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var isExceptionThrown = false;
            var content = new AddArtifactsParameter
            {
                ArtifactIds = null,
                AddChildren = false
            };

            // Act
            try
            {
                var review = await _reviewsRepository.AddArtifactsToReviewAsync(reviewId, userId, content);
            }
            catch (BadRequestException ex)
            {
                isExceptionThrown = true;
                // Assert
                Assert.AreEqual(ErrorCodes.OutOfRangeParameter, ex.ErrorCode);
            }
            finally
            {
                if (!isExceptionThrown)
                {
                    Assert.Fail();
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task AddArtifactsToReviewAsync_ShouldThrowReviewNotFoundException()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;

            _artifactRepositoryMock
                .Setup(m => m.GetArtifactBasicDetails(ReviewId, UserId))
                .ReturnsAsync(() => null);

            var isExceptionThrown = false;
            var content = new AddArtifactsParameter
            {
                ArtifactIds = new[] { 1, 2 },
                AddChildren = false
            };

            // Act
            try
            {
                var review = await _reviewsRepository.AddArtifactsToReviewAsync(reviewId, userId, content);
            }
            catch (ResourceNotFoundException ex)
            {
                isExceptionThrown = true;
                // Assert
                Assert.AreEqual(ErrorCodes.ResourceNotFound, ex.ErrorCode);
            }
            finally
            {
                if (!isExceptionThrown)
                {
                    Assert.Fail();
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task AddArtifactsToReviewAsync_ShouldThrowArtifactNotLockedException()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            _artifactDetails.LockedByUserId = 3;
            var isExceptionThrown = false;
            var content = new AddArtifactsParameter
            {
                ArtifactIds = new[] { 1, 2 },
                AddChildren = false
            };
            // Act
            try
            {
                await _reviewsRepository.AddArtifactsToReviewAsync(reviewId, userId, content);
            }
            catch (ConflictException ex)
            {
                isExceptionThrown = true;

                // Assert
                Assert.AreEqual(ErrorCodes.LockedByOtherUser, ex.ErrorCode);

            }
            finally
            {
                if (!isExceptionThrown)
                {
                    Assert.Fail();
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task AddArtifactsToReviewAsync_ShouldThrowUserCannotModifyReviewException()
        {

            // Arrange
            var reviewId = 1;
            var userId = 2;
            var isExceptionThrown = false;
            _hasEditPermissions = false;
            var content = new AddArtifactsParameter
            {
                ArtifactIds = new[] { 1, 2 },
                AddChildren = false
            };
            _artifactDetails.LockedByUserId = 999;

            var queryParameters = new Dictionary<string, object>
            {
                { "@reviewIds", SqlConnectionWrapper.ToDataTable(new int[] { reviewId }) },
                { "@userId", userId },
                { "@revisionId", int.MaxValue },
                { "@addDrafts", true }
            };

            var reviewsDataResult = new[]
            {
               new ReviewData
               {
                    ReviewContentsXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewContents xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"/>"
                }
            };

            _cxn.SetupQueryAsync("GetReviewsData", queryParameters, reviewsDataResult);

            // Act
            try
            {
                await _reviewsRepository.AddArtifactsToReviewAsync(reviewId, userId, content);
            }
            catch (AuthorizationException ex)
            {
                isExceptionThrown = true;

                // Assert
                Assert.AreEqual(ErrorCodes.UnauthorizedAccess, ex.ErrorCode);

            }
            finally
            {
                if (!isExceptionThrown)
                {
                    Assert.Fail();
                }
            }
        }

        #endregion

        #region UpdateReviewArtifactApprovalAsync

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task UpdateReviewArtifactApprovalAsync_Should_Throw_When_Artifacts_Collection_Is_Null()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            ReviewArtifactApprovalParameter approvalParameter = null;

            // Act
            await _reviewsRepository.UpdateReviewArtifactApprovalAsync(reviewId, approvalParameter, userId);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task UpdateReviewArtifactApprovalAsync_Should_Throw_When_No_Artifacts_Provided()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var approvalParameter = new ReviewArtifactApprovalParameter();

            // Act
            await _reviewsRepository.UpdateReviewArtifactApprovalAsync(reviewId, approvalParameter, userId);
        }

        [TestMethod]
        [Ignore]
        public async Task UpdateReviewArtifactApprovalAsync_Should_Succeed_With_No_Existing_Xml()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;

            var approvalParameter = new ReviewArtifactApprovalParameter
            {
                Approval = "Approved",
                ApprovalFlag = ApprovalType.Approved,
                ArtifactIds = new List<int> { 3 }
            };

            var artifactIds = new[] { 3 };

            SetupArtifactApprovalCheck(reviewId, userId, artifactIds);

            SetupGetVersionNumber(reviewId, artifactIds);

            SetupArtifactPermissionsCheck(new[] { artifactIds[0], reviewId }, userId, new Dictionary<int, RolePermissions>
            {
                { 1, RolePermissions.Read },
                { 3, RolePermissions.Read }
            });

            var getXmlParameters = new Dictionary<string, object>
            {
                { "reviewId", 1 },
                { "userId", 2 }
            };

            _cxn.SetupQueryAsync("GetReviewUserStatsXml", getXmlParameters, new List<string>());

            var updateXmlParameters = new Dictionary<string, object>
            {
                { "reviewId", 1 },
                { "userId", 2 },
                { "updateReviewerStatus", false },
                { "value", "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewedArtifacts xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><ReviewedArtifacts><RA><A>Approved</A><AF>Approved</AF><EO>2017-07-10T13:20:00</EO><Id>3</Id><V>1</V><VS>Viewed</VS></RA></ReviewedArtifacts></RDReviewedArtifacts>" }
            };

            _cxn.SetupExecuteAsync("UpdateReviewUserStats", updateXmlParameters, 1);


            // Act
            var result = await _reviewsRepository.UpdateReviewArtifactApprovalAsync(reviewId, approvalParameter, userId);

            // Assert
            _cxn.Verify();

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.ApprovedArtifacts.FirstOrDefault().ArtifactId);
            Assert.AreEqual(DateTime.Parse("2017-07-10T13:20:00"), result.ApprovedArtifacts.FirstOrDefault().Timestamp);
        }

        [TestMethod]
        [Ignore]
        public async Task UpdateReviewArtifactApprovalAsync_Should_Add_New_Artifact_Approval()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var approvalParameter = new ReviewArtifactApprovalParameter
            {
                Approval = "Custom Approval",
                ApprovalFlag = ApprovalType.Approved,
                ArtifactIds = new List<int> { 3 },
                SelectionType = SelectionType.Selected
            };

            var artifactIds = new[] { 3 };

            SetupArtifactApprovalCheck(reviewId, userId, artifactIds);

            SetupGetVersionNumber(reviewId, artifactIds);

            SetupArtifactPermissionsCheck(new[] { artifactIds[0], reviewId }, userId, new Dictionary<int, RolePermissions>
            {
                { 1, RolePermissions.Read },
                { 3, RolePermissions.Read }
            });

            var getXmlParameters = new Dictionary<string, object>
            {
                { "reviewId", 1 },
                { "userId", 2 }
            };

            _cxn.SetupQueryAsync("GetReviewUserStatsXml", getXmlParameters, new List<string>
            {
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewedArtifacts xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><ReviewedArtifacts><RA><A>Approved</A><AF>Approved</AF><Id>4</Id><V>1</V><VS>Viewed</VS></RA></ReviewedArtifacts></RDReviewedArtifacts>"
            });

            var updateXmlParameters = new Dictionary<string, object>
            {
                { "reviewId", 1 },
                { "userId", 2 },
                { "updateReviewerStatus", false },
                { "value", "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewedArtifacts xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><ReviewedArtifacts><RA><A>Approved</A><AF>Approved</AF><Id>4</Id><V>1</V><VS>Viewed</VS></RA><RA><A>Custom Approval</A><AF>Approved</AF><EO>2017-07-10T13:20:00</EO><Id>3</Id><V>1</V><VS>Viewed</VS></RA></ReviewedArtifacts></RDReviewedArtifacts>" }
            };

            _cxn.SetupExecuteAsync("UpdateReviewUserStats", updateXmlParameters, 1);


            // Act
            await _reviewsRepository.UpdateReviewArtifactApprovalAsync(reviewId, approvalParameter, userId);

            // Assert
            _cxn.Verify();
        }

        [TestMethod]
        public async Task UpdateReviewArtifactApprovalAsync_Should_Update_Existing_Approval()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var approvalParameter = new ReviewArtifactApprovalParameter
            {
                Approval = "Disapproved",
                ApprovalFlag = ApprovalType.Disapproved,
                ArtifactIds = new List<int> { 3 },
                SelectionType = SelectionType.Selected
            };

            var artifactIds = new[] { 3 };

            SetupReviewArtifactsUserApprovalCheck(reviewId, userId, artifactIds);
            SetupGetVersionNumber(reviewId, artifactIds);

            SetupArtifactPermissionsCheck(new[] { artifactIds[0], reviewId }, userId, new Dictionary<int, RolePermissions>
            {
                { 1, RolePermissions.Read },
                { 3, RolePermissions.Read }
            });

            var revApppCheck = new ReviewArtifactApprovalCheck
            {
                ReviewExists = true,
                ReviewStatus = ReviewPackageStatus.Active,
                ReviewDeleted = false,
                AllArtifactsInReview = true,
                AllArtifactsRequireApproval = true,
                UserInReview = true,
                ReviewerRole = ReviewParticipantRole.Approver,
                ReviewType = ReviewType.Formal,
                ReviewerStatus = ReviewStatus.InProgress
            };

            var getXmlParameters = new Dictionary<string, object>
            {
                { "reviewId", 1 },
                { "userId", 2 }
            };

            _cxn.SetupQueryAsync("GetReviewUserStatsXml", getXmlParameters, new List<string>
            {
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewedArtifacts xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><ReviewedArtifacts><RA><A>Approved</A><AF>Approved</AF><Id>3</Id><V>1</V><VS>Viewed</VS></RA></ReviewedArtifacts></RDReviewedArtifacts>"
            });

            var updateXmlParameters = new Dictionary<string, object>
            {
                { "reviewId", 1 },
                { "userId", 2 },
                { "updateReviewerStatus", false },
                { "value", "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewedArtifacts xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><ReviewedArtifacts><RA><A>Disapproved</A><AF>Disapproved</AF><EO>2017-07-10T13:20:00</EO><Id>3</Id><V>1</V><VS>Viewed</VS></RA></ReviewedArtifacts></RDReviewedArtifacts>" }
            };

            _cxn.SetupExecuteAsync("UpdateReviewUserStats", updateXmlParameters, 1);


            // Act
            await _reviewsRepository.UpdateReviewArtifactApprovalAsync(reviewId, approvalParameter, userId);

            // Assert
            _cxn.Verify();
        }

        private void SetupReviewArtifactsUserApprovalCheck(int reviewId, int userId, IEnumerable<int> artifactIds, ReviewArtifactApprovalCheck[] reviewApprovalCheck = null)
        {
            if (reviewApprovalCheck == null)
            {
                reviewApprovalCheck = new[]
                {
                    new ReviewArtifactApprovalCheck {
                        ReviewExists = true,
                        ReviewStatus = ReviewPackageStatus.Active,
                        ReviewDeleted = false,
                        AllArtifactsInReview = true,
                        AllArtifactsRequireApproval = true,
                        UserInReview = true,
                        ReviewerRole = ReviewParticipantRole.Approver,
                        ReviewType = ReviewType.Formal,
                        ReviewerStatus = ReviewStatus.InProgress
                    }
                };
            }

            var mockResult = new Tuple<IEnumerable<ReviewArtifactApprovalCheck>, IEnumerable<int>>(reviewApprovalCheck, artifactIds);

            var outParameters = new Dictionary<string, object>
            {
               { "ArtifactIds",  artifactIds },
               { "Unpublished", 0 },
               { "Nonexistent", 0 },
               { "IsBaselineAdded", false }
            };

            var getCheckParameters = new Dictionary<string, object>
            {
                { "reviewId", reviewId },
                { "userId", userId },
                { "artifactIds", SqlConnectionWrapper.ToDataTable(artifactIds) }
            };
            _cxn.SetupQueryMultipleAsync("CheckReviewArtifactUserApproval", getCheckParameters, mockResult, outParameters);
        }

        [TestMethod]
        public async Task UpdateReviewArtifactApprovalAsync_Should_Set_Artifact_To_Viewed()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var approvalParameter = new ReviewArtifactApprovalParameter
            {
                Approval = "Approved",
                ApprovalFlag = ApprovalType.Approved,
                ArtifactIds = new List<int> { 3 },
                SelectionType = SelectionType.Selected
            };

            var artifactIds = new[] { 3 };

            SetupGetVersionNumber(reviewId, artifactIds);
            SetupReviewArtifactsUserApprovalCheck(reviewId, userId, artifactIds);
            SetupArtifactPermissionsCheck(new[] { artifactIds[0], reviewId }, userId, new Dictionary<int, RolePermissions>
            {
                { 1, RolePermissions.Read },
                { 3, RolePermissions.Read }
            });

            var getXmlParameters = new Dictionary<string, object>
            {
                { "reviewId", 1 },
                { "userId", 2 }
            };

            _cxn.SetupQueryAsync("GetReviewUserStatsXml", getXmlParameters, new List<string>
            {
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewedArtifacts xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><ReviewedArtifacts><RA><A>Not Specified</A><Id>3</Id><V>1</V></RA></ReviewedArtifacts></RDReviewedArtifacts>"
            });

            var updateXmlParameters = new Dictionary<string, object>
            {
                { "reviewId", 1 },
                { "userId", 2 },
                { "updateReviewerStatus", false },
                { "value", "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewedArtifacts xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><ReviewedArtifacts><RA><A>Approved</A><AF>Approved</AF><EO>2017-07-10T13:20:00</EO><Id>3</Id><V>1</V><VS>Viewed</VS></RA></ReviewedArtifacts></RDReviewedArtifacts>" }
            };

            _cxn.SetupExecuteAsync("UpdateReviewUserStats", updateXmlParameters, 1);


            // Act
            await _reviewsRepository.UpdateReviewArtifactApprovalAsync(reviewId, approvalParameter, userId);

            // Assert
            _cxn.Verify();
        }

        [TestMethod]
        public async Task UpdateReviewArtifactApprovalAsync_Should_Not_Update_Approval_Timestamp_If_Approval_Doesnt_Change()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var approvalParameter = new ReviewArtifactApprovalParameter
            {
                Approval = "Approved",
                ApprovalFlag = ApprovalType.Approved,
                ArtifactIds = new List<int> { 3 }
            };

            var artifactIds = new[] { 3 };

            SetupReviewArtifactsUserApprovalCheck(reviewId, userId, artifactIds);

            SetupGetVersionNumber(reviewId, artifactIds);

            SetupArtifactPermissionsCheck(new[] { artifactIds[0], reviewId }, userId, new Dictionary<int, RolePermissions>
            {
                { 1, RolePermissions.Read },
                { 3, RolePermissions.Read }
            });

            var getXmlParameters = new Dictionary<string, object>
            {
                { "reviewId", 1 },
                { "userId", 2 }
            };

            _cxn.SetupQueryAsync("GetReviewUserStatsXml", getXmlParameters, new List<string>
            {
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewedArtifacts xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><ReviewedArtifacts><RA><A>Approved</A><AF>Approved</AF><EO>2017-07-10T15:00:00</EO><Id>3</Id><V>1</V></RA></ReviewedArtifacts></RDReviewedArtifacts>"
            });

            var updateXmlParameters = new Dictionary<string, object>
            {
                { "reviewId", 1 },
                { "userId", 2 },
                { "updateReviewerStatus", false },
                { "value", "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewedArtifacts xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><ReviewedArtifacts><RA><A>Approved</A><AF>Approved</AF><EO>2017-07-10T15:00:00</EO><Id>3</Id><V>1</V><VS>Viewed</VS></RA></ReviewedArtifacts></RDReviewedArtifacts>" }
            };

            _cxn.SetupExecuteAsync("UpdateReviewUserStats", updateXmlParameters, 1);


            // Act
            await _reviewsRepository.UpdateReviewArtifactApprovalAsync(reviewId, approvalParameter, userId);

            // Assert
            _cxn.Verify();
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task UpdateReviewArtifactApprovalAsync_Should_Throw_When_Review_Doesnt_Exist()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var artifactIds = new[] { 3 };
            var approvalParameter = new ReviewArtifactApprovalParameter
            {
                Approval = "Approved",
                ApprovalFlag = ApprovalType.Approved,
                ArtifactIds = new List<int> { 3 }
            };

            var reviewApprovalCheck = new[]
            {
                new ReviewArtifactApprovalCheck
                {
                    ReviewExists = false,
                    ReviewStatus = ReviewPackageStatus.Active,
                    ReviewDeleted = false,
                    AllArtifactsInReview = true,
                    AllArtifactsRequireApproval = true,
                    UserInReview = true,
                    ReviewerRole = ReviewParticipantRole.Approver,
                    ReviewType = ReviewType.Formal,
                    ReviewerStatus = ReviewStatus.InProgress
                }
            };
            SetupReviewArtifactsUserApprovalCheck(reviewId, userId, artifactIds, reviewApprovalCheck);


            // Act
            await _reviewsRepository.UpdateReviewArtifactApprovalAsync(reviewId, approvalParameter, userId);
        }

        [TestMethod]

        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task UpdateReviewArtifactApprovalAsync_Should_Throw_When_Review_Is_Draft()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var approvalParameter = new ReviewArtifactApprovalParameter
            {
                Approval = "Approved",
                ApprovalFlag = ApprovalType.Approved,
                ArtifactIds = new List<int> { 3 }
            };
            var artifactIds = new[] { 3 };
            var reviewApprovalCheck = new[]
            {
                new ReviewArtifactApprovalCheck
                {
                    ReviewExists = true,
                    ReviewStatus = ReviewPackageStatus.Draft,
                    ReviewDeleted = false,
                    AllArtifactsInReview = true,
                    AllArtifactsRequireApproval = true,
                    UserInReview = true,
                    ReviewerRole = ReviewParticipantRole.Approver,
                    ReviewType = ReviewType.Formal,
                    ReviewerStatus = ReviewStatus.NotStarted
                }
            };

            SetupReviewArtifactsUserApprovalCheck(reviewId, userId, artifactIds, reviewApprovalCheck);


            // Act
            await _reviewsRepository.UpdateReviewArtifactApprovalAsync(reviewId, approvalParameter, userId);
        }

        [TestMethod]
        public async Task UpdateReviewArtifactApprovalAsync_Should_Throw_When_Review_Is_Closed()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var artifactIds = new[] { 3 };
            var approvalParameter = new ReviewArtifactApprovalParameter
            {
                Approval = "Approved",
                ApprovalFlag = ApprovalType.Approved,
                ArtifactIds = new List<int> { 3 }
            };

            var reviewApprovalCheck = new[]
            {
                new ReviewArtifactApprovalCheck
                {
                    ReviewExists = true,
                    ReviewStatus = ReviewPackageStatus.Closed,
                    ReviewDeleted = false,
                    AllArtifactsInReview = true,
                    AllArtifactsRequireApproval = true,
                    UserInReview = true,
                    ReviewerRole = ReviewParticipantRole.Approver,
                    ReviewType = ReviewType.Formal,
                    ReviewerStatus = ReviewStatus.InProgress
                }
            };

            SetupReviewArtifactsUserApprovalCheck(reviewId, userId, artifactIds, reviewApprovalCheck);


            // Act
            try
            {
                await _reviewsRepository.UpdateReviewArtifactApprovalAsync(reviewId, approvalParameter, userId);
            }
            catch (ConflictException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.ReviewClosed);
                return;
            }

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task UpdateReviewArtifactApprovalAsync_Should_Throw_When_Review_Is_Deleted()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var approvalParameter = new ReviewArtifactApprovalParameter
            {
                Approval = "Approved",
                ApprovalFlag = ApprovalType.Approved,
                ArtifactIds = new List<int> { 3 }
            };
            var artifactIds = new[] { 3 };

            var reviewApprovalCheck = new[]
            {
                new ReviewArtifactApprovalCheck
                {
                    ReviewExists = true,
                    ReviewStatus = ReviewPackageStatus.Active,
                    ReviewDeleted = true,
                    AllArtifactsInReview = true,
                    AllArtifactsRequireApproval = true,
                    UserInReview = true,
                    ReviewerRole = ReviewParticipantRole.Approver,
                    ReviewType = ReviewType.Formal,
                    ReviewerStatus = ReviewStatus.InProgress
                }
            };

            SetupReviewArtifactsUserApprovalCheck(reviewId, userId, artifactIds, reviewApprovalCheck);


            // Act
            await _reviewsRepository.UpdateReviewArtifactApprovalAsync(reviewId, approvalParameter, userId);
        }

        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task UpdateReviewArtifactApprovalAsync_Should_Throw_When_Reviewer_Status_Is_Completed()
        {

            // Arrange
            var reviewId = 1;
            var userId = 2;
            var approvalParameter = new ReviewArtifactApprovalParameter()
            {
                Approval = "Approved",
                ApprovalFlag = ApprovalType.Approved,
                ArtifactIds = new List<int> { 3 }
            };

            var artifactIds = new[] { 3 };

            var reviewApprovalCheck = new[]
            {
                new ReviewArtifactApprovalCheck
                {
                    ReviewExists = true,
                    ReviewStatus = ReviewPackageStatus.Active,
                    ReviewDeleted = false,
                    AllArtifactsInReview = true,
                    AllArtifactsRequireApproval = true,
                    UserInReview = true,
                    ReviewerRole = ReviewParticipantRole.Approver,
                    ReviewType = ReviewType.Formal,
                    ReviewerStatus = ReviewStatus.Completed
                }
            };
            SetupReviewArtifactsUserApprovalCheck(reviewId, userId, artifactIds, reviewApprovalCheck);


            // Act
            await _reviewsRepository.UpdateReviewArtifactApprovalAsync(reviewId, approvalParameter, userId);
        }

        [TestMethod]
        public async Task UpdateReviewArtifactApprovalAsync_ShouldThrowUserCannotModifyReviewException()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var isExceptionThrown = false;
            _hasEditPermissions = false;
            var approvalParameter = new ReviewArtifactApprovalParameter
            {
                Approval = "Disapproved",
                ApprovalFlag = ApprovalType.Disapproved,
                ArtifactIds = new List<int> { 3 },
                SelectionType = SelectionType.Selected
            };

            var artifactIds = new[] { 3 };

            SetupReviewArtifactsUserApprovalCheck(reviewId, userId, artifactIds);
            SetupGetVersionNumber(reviewId, artifactIds);

            SetupArtifactPermissionsCheck(new[] { artifactIds[0], reviewId }, userId, new Dictionary<int, RolePermissions>
            {
                { 1, RolePermissions.Read },
                { 3, RolePermissions.Read }
            });

            var revApppCheck = new ReviewArtifactApprovalCheck
            {
                ReviewExists = true,
                ReviewStatus = ReviewPackageStatus.Active,
                ReviewDeleted = false,
                AllArtifactsInReview = true,
                AllArtifactsRequireApproval = true,
                UserInReview = true,
                ReviewerRole = ReviewParticipantRole.Approver,
                ReviewType = ReviewType.Formal,
                ReviewerStatus = ReviewStatus.InProgress
            };

            var getXmlParameters = new Dictionary<string, object>
            {
                { "reviewId", 1 },
                { "userId", 2 }
            };

            _cxn.SetupQueryAsync("GetReviewUserStatsXml", getXmlParameters, new List<string>
            {
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewedArtifacts xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><ReviewedArtifacts><RA><A>Approved</A><AF>Approved</AF><Id>3</Id><V>1</V><VS>Viewed</VS></RA></ReviewedArtifacts></RDReviewedArtifacts>"
            });

            var updateXmlParameters = new Dictionary<string, object>
            {
                { "reviewId", 1 },
                { "userId", 2 },
                { "updateReviewerStatus", false },
                { "value", "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewedArtifacts xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><ReviewedArtifacts><RA><A>Disapproved</A><AF>Disapproved</AF><EO>2017-07-10T13:20:00</EO><Id>3</Id><V>1</V><VS>Viewed</VS></RA></ReviewedArtifacts></RDReviewedArtifacts>" }
            };

            _cxn.SetupExecuteAsync("UpdateReviewUserStats", updateXmlParameters, 1);

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewArtifactApprovalAsync(reviewId, approvalParameter, userId);
            }
            catch (AuthorizationException ex)
            {
                isExceptionThrown = true;

                // Assert
                Assert.AreEqual(ErrorCodes.UnauthorizedAccess, ex.ErrorCode);

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
        [Ignore]
        public async Task UpdateReviewArtifactApprovalAsync_Should_Not_Throw_When_User_Isnt_Approver()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;

            var approvalParameter = new ReviewArtifactApprovalParameter
            {
                Approval = "Approved",
                ApprovalFlag = ApprovalType.Approved,
                ArtifactIds = new List<int> { 3 }
            };

            var artifactIds = new[] { 3 };

            var reviewApprovalCheck = new[]
           {
                new ReviewArtifactApprovalCheck
                {
                    ReviewExists = true,
                    ReviewStatus = ReviewPackageStatus.Active,
                    ReviewDeleted = false,
                    AllArtifactsInReview = true,
                    AllArtifactsRequireApproval = true,
                    UserInReview = true,
                    ReviewerRole = ReviewParticipantRole.Reviewer,
                    ReviewType = ReviewType.Formal,
                    ReviewerStatus = ReviewStatus.InProgress
                }
           };

            SetupReviewArtifactsUserApprovalCheck(reviewId, userId, artifactIds, reviewApprovalCheck);

            SetupGetVersionNumber(reviewId, artifactIds);

            SetupArtifactPermissionsCheck(new[] { artifactIds[0], reviewId }, userId, new Dictionary<int, RolePermissions>
            {
                { 1, RolePermissions.Read },
                { 3, RolePermissions.Read }
            });

            var getXmlParameters = new Dictionary<string, object>
            {
                { "reviewId", 1 },
                { "userId", 2 }
            };

            _cxn.SetupQueryAsync("GetReviewUserStatsXml", getXmlParameters, new List<string>
            {
                null
            });

            var updateXmlParameters = new Dictionary<string, object>
            {
                { "reviewId", 1 },
                { "userId", 2 },
                { "xmlString", "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewedArtifacts xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><ReviewedArtifacts><RA><A>Approved</A><AF>Approved</AF><Id>3</Id><V>1</V><VS>Viewed</VS></RA></ReviewedArtifacts></RDReviewedArtifacts>" }
            };

            _cxn.SetupExecuteAsync("UpdateReviewUserStatsXml", updateXmlParameters, 1);

            // Act
            await _reviewsRepository.UpdateReviewArtifactApprovalAsync(reviewId, approvalParameter, userId);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task UpdateReviewArtifactApprovalAsync_Should_Throw_When_Artifact_Given_Is_Not_In_Review()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var approvalParameter = new ReviewArtifactApprovalParameter
            {
                Approval = "Approved",
                ApprovalFlag = ApprovalType.Approved,
                ArtifactIds = new List<int> { 3 }
            };

            var artifactIds = new List<int>();

            var reviewApprovalCheck = new[]
             {
                new ReviewArtifactApprovalCheck {
                ReviewExists = true,
                ReviewStatus = ReviewPackageStatus.Active,
                ReviewDeleted = false,
                AllArtifactsInReview = false,
                AllArtifactsRequireApproval = false,
                UserInReview = true,
                ReviewerRole = ReviewParticipantRole.Approver,
                ReviewType = ReviewType.Formal,
                ReviewerStatus = ReviewStatus.InProgress
                }
             };

            var mockResult = new Tuple<IEnumerable<ReviewArtifactApprovalCheck>, IEnumerable<int>>(reviewApprovalCheck, artifactIds);

            var outParameters = new Dictionary<string, object>
            {
               { "ArtifactIds",  artifactIds },
               { "Unpublished", 0 },
               { "Nonexistent", 0 },
               { "IsBaselineAdded", false }
            };

            var getCheckParameters = new Dictionary<string, object>
            {
                { "reviewId", reviewId },
                { "userId", userId },
                { "artifactIds", SqlConnectionWrapper.ToDataTable(new List<int> { 3 }) }
            };

            _cxn.SetupQueryMultipleAsync("CheckReviewArtifactUserApproval", getCheckParameters, mockResult, outParameters);

            // Act
            await _reviewsRepository.UpdateReviewArtifactApprovalAsync(reviewId, approvalParameter, userId);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task UpdateReviewArtifactApprovalAsync_Should_Throw_When_User_Is_Not_Part_Of_The_Review()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var approvalParameter = new ReviewArtifactApprovalParameter
            {
                Approval = "Approved",
                ApprovalFlag = ApprovalType.Approved,
                ArtifactIds = new List<int> { 3 }
            };

            var artifactIds = new[] { 3 };

            var reviewApprovalCheck = new[]
            {
                new ReviewArtifactApprovalCheck
                {
                    ReviewExists = true,
                    ReviewStatus = ReviewPackageStatus.Active,
                    ReviewDeleted = false,
                    AllArtifactsInReview = true,
                    AllArtifactsRequireApproval = true,
                    UserInReview = false,
                    ReviewerRole = ReviewParticipantRole.Approver,
                    ReviewType = ReviewType.Formal,
                    ReviewerStatus = ReviewStatus.InProgress
                }
            };

            SetupReviewArtifactsUserApprovalCheck(reviewId, userId, artifactIds, reviewApprovalCheck);

            // Act
            await _reviewsRepository.UpdateReviewArtifactApprovalAsync(reviewId, approvalParameter, userId);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task UpdateReviewArtifactApprovalAsync_Should_Throw_When_Artifact_Given_Doesnt_Require_Approval()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var approvalParameter = new ReviewArtifactApprovalParameter
            {
                Approval = "Approved",
                ApprovalFlag = ApprovalType.Approved,
                ArtifactIds = new List<int> { 3 }
            };
            var artifactIds = new List<int>();

            var reviewApprovalCheck = new[]
             {
                new ReviewArtifactApprovalCheck {
                    ReviewExists = true,
                    ReviewStatus = ReviewPackageStatus.Active,
                    ReviewDeleted = false,
                    AllArtifactsInReview = true,
                    AllArtifactsRequireApproval = false,
                    UserInReview = true,
                    ReviewerRole = ReviewParticipantRole.Approver,
                    ReviewType = ReviewType.Formal,
                    ReviewerStatus = ReviewStatus.InProgress
                }
             };

            var mockResult = new Tuple<IEnumerable<ReviewArtifactApprovalCheck>, IEnumerable<int>>(reviewApprovalCheck, artifactIds);

            var outParameters = new Dictionary<string, object>
            {
               { "ArtifactIds",  artifactIds },
               { "Unpublished", 0 },
               { "Nonexistent", 0 },
               { "IsBaselineAdded", false }
            };

            var getCheckParameters = new Dictionary<string, object>
            {
                { "reviewId", reviewId },
                { "userId", userId },
                { "artifactIds", SqlConnectionWrapper.ToDataTable(new List<int> { 3 }) }
            };

            _cxn.SetupQueryMultipleAsync("CheckReviewArtifactUserApproval", getCheckParameters, mockResult, outParameters);

            // Act
            await _reviewsRepository.UpdateReviewArtifactApprovalAsync(reviewId, approvalParameter, userId);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task UpdateReviewArtifactApprovalAsync_Should_Throw_When_User_Doesnt_Have_Access_To_Review()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var approvalParameter = new ReviewArtifactApprovalParameter
            {
                Approval = "Approved",
                ApprovalFlag = ApprovalType.Approved,
                ArtifactIds = new List<int> { 3 }
            };

            var artifactIds = new[] { 3 };

            var reviewApprovalCheck = new[]
            {
                new ReviewArtifactApprovalCheck
                {
                    ReviewExists = true,
                    ReviewStatus = ReviewPackageStatus.Active,
                    ReviewDeleted = false,
                    AllArtifactsInReview = true,
                    AllArtifactsRequireApproval = true,
                    UserInReview = true,
                    ReviewerRole = ReviewParticipantRole.Approver,
                    ReviewType = ReviewType.Formal,
                    ReviewerStatus = ReviewStatus.InProgress
                }
            };

            SetupReviewArtifactsUserApprovalCheck(reviewId, userId, artifactIds, reviewApprovalCheck);

            SetupArtifactPermissionsCheck(new[] { 3, reviewId }, userId, new Dictionary<int, RolePermissions>
            {
                { 3, RolePermissions.Read }
            });

            // Act

            await _reviewsRepository.UpdateReviewArtifactApprovalAsync(reviewId, approvalParameter, userId);
        }

        [TestMethod]
        [Ignore]
        public async Task UpdateReviewArtifactApprovalAsync_Should_Throw_When_User_Doesnt_Have_Access_To_Given_Artifact()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var approvalParameter = new ReviewArtifactApprovalParameter
            {
                Approval = "Approved",
                ApprovalFlag = ApprovalType.Approved,
                ArtifactIds = new List<int> { 3 },
                SelectionType = SelectionType.Selected
            };

            var artifactIds = new[] { 3 };

            // SetupArtifactApprovalCheck(reviewId, userId, artifactIds, check => check.ReviewerRole = ReviewParticipantRole.Reviewer);
            var reviewApprovalCheck = new[]
            {
                new ReviewArtifactApprovalCheck
                {
                    ReviewExists = true,
                    ReviewStatus = ReviewPackageStatus.Active,
                    ReviewDeleted = false,
                    AllArtifactsInReview = true,
                    AllArtifactsRequireApproval = true,
                    UserInReview = true,
                    ReviewerRole = ReviewParticipantRole.Approver,
                    ReviewType = ReviewType.Formal,
                    ReviewerStatus = ReviewStatus.InProgress
                }
            };

            SetupReviewArtifactsUserApprovalCheck(reviewId, userId, artifactIds, reviewApprovalCheck);

            SetupArtifactPermissionsCheck(new[] { 3, reviewId }, userId, new Dictionary<int, RolePermissions>
            {
                { 1, RolePermissions.Read }
            });

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewArtifactApprovalAsync(reviewId, approvalParameter, userId);
            }
            catch (AuthorizationException ex)
            {
                Assert.AreEqual(ErrorCodes.UnauthorizedAccess, ex.ErrorCode);
                return;
            }

            Assert.Fail();
        }

        [TestMethod]
        public async Task UpdateReviewArtifactApprovalAsync_When_MoS_Is_Enabled_Should_Throw_If_MoSIds_Is_Null()
        {
            // Arrange
            var approvalParameter = new ReviewArtifactApprovalParameter
            {
                Approval = "Pending",
                ApprovalFlag = ApprovalType.NotSpecified,
                ArtifactIds = new List<int> { 3 }
            };

            var artifactIds = new[] { 3 };

            SetupReviewArtifactsUserApprovalCheck(ReviewId, UserId, artifactIds);

            SetupGetVersionNumber(ReviewId, artifactIds);

            SetupArtifactPermissionsCheck(new[] { artifactIds[0], ReviewId }, UserId, new Dictionary<int, RolePermissions>
            {
                { 1, RolePermissions.Read },
                { 3, RolePermissions.Read }
            });

            SetupIsMeaningOfSignatureEnabledQuery(false, true);

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewArtifactApprovalAsync(ReviewId, approvalParameter, UserId);
            }
            catch (BadRequestException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.MeaningOfSignatureNotChosen, ex.ErrorCode);

                return;
            }


            Assert.Fail("A BadRequestException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewArtifactApprovalAsync_When_MoS_Is_Enabled_Should_Throw_If_MoSIds_Is_Empty()
        {
            // Arrange
            var approvalParameter = new ReviewArtifactApprovalParameter
            {
                Approval = "Pending",
                ApprovalFlag = ApprovalType.NotSpecified,
                ArtifactIds = new List<int> { 3 },
                MeaningOfSignatures = new SelectedMeaningOfSignatureValue[0]
            };

            var artifactIds = new[] { 3 };

            SetupReviewArtifactsUserApprovalCheck(ReviewId, UserId, artifactIds);

            SetupGetVersionNumber(ReviewId, artifactIds);

            SetupArtifactPermissionsCheck(new[] { artifactIds[0], ReviewId }, UserId, new Dictionary<int, RolePermissions>
            {
                { 1, RolePermissions.Read },
                { 3, RolePermissions.Read }
            });

            SetupIsMeaningOfSignatureEnabledQuery(false, true);

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewArtifactApprovalAsync(ReviewId, approvalParameter, UserId);
            }
            catch (BadRequestException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.MeaningOfSignatureNotChosen, ex.ErrorCode);

                return;
            }


            Assert.Fail("A BadRequestException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewArtifactApprovalAsync_When_MoS_Is_Enabled_Should_Throw_If_No_Possible_MoS_For_Participant()
        {
            // Arrange
            var approvalParameter = new ReviewArtifactApprovalParameter
            {
                Approval = "Pending",
                ApprovalFlag = ApprovalType.NotSpecified,
                ArtifactIds = new List<int> { 3 },
                MeaningOfSignatures = new[]
                {
                    new SelectedMeaningOfSignatureValue()
                    {
                        MeaningOfSignatureId = 4,
                        RoleId = 5
                    }
                }
            };

            var artifactIds = new[] { 3 };

            SetupReviewArtifactsUserApprovalCheck(ReviewId, UserId, artifactIds);

            SetupGetVersionNumber(ReviewId, artifactIds);

            SetupArtifactPermissionsCheck(new[] { artifactIds[0], ReviewId }, UserId, new Dictionary<int, RolePermissions>
            {
                { 1, RolePermissions.Read },
                { 3, RolePermissions.Read }
            });

            SetupIsMeaningOfSignatureEnabledQuery(false, true);

            SetupPossibleMeaningOfSignaturesQuery(new[] { UserId }, new ParticipantMeaningOfSignatureResult[0]);

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewArtifactApprovalAsync(ReviewId, approvalParameter, UserId);
            }
            catch (ConflictException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.MeaningOfSignatureNotPossible, ex.ErrorCode);

                return;
            }


            Assert.Fail("A ConflictException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewArtifactApprovalAsync_When_MoS_Is_Enabled_Should_Throw_If_No_Assigned_MoS_For_Participant()
        {
            // Arrange
            var approvalParameter = new ReviewArtifactApprovalParameter
            {
                Approval = "Pending",
                ApprovalFlag = ApprovalType.NotSpecified,
                ArtifactIds = new List<int> { 3 },
                MeaningOfSignatures = new[]
                {
                    new SelectedMeaningOfSignatureValue()
                    {
                        MeaningOfSignatureId = 4,
                        RoleId = 5
                    }
                }
            };

            var artifactIds = new[] { 3 };

            SetupReviewArtifactsUserApprovalCheck(ReviewId, UserId, artifactIds);

            SetupGetVersionNumber(ReviewId, artifactIds);

            SetupArtifactPermissionsCheck(new[] { artifactIds[0], ReviewId }, UserId, new Dictionary<int, RolePermissions>
            {
                { 1, RolePermissions.Read },
                { 3, RolePermissions.Read }
            });

            SetupIsMeaningOfSignatureEnabledQuery(false, true);

            SetupPossibleMeaningOfSignaturesQuery(new[] { UserId }, new[]
            {
                new ParticipantMeaningOfSignatureResult()
                {
                    RoleAssignmentId = 4
                }
            });

            SetupParticipantMeaningOfSignaturesQuery(new[] { UserId }, new ParticipantMeaningOfSignatureResult[0]);

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewArtifactApprovalAsync(ReviewId, approvalParameter, UserId);
            }
            catch (ConflictException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.MeaningOfSignatureNotPossible, ex.ErrorCode);

                return;
            }


            Assert.Fail("A ConflictException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewArtifactApprovalAsync_When_MoS_Is_Enabled_Should_Throw_If_Assigned_MoS_Doesnt_Match_Possible_MoS()
        {
            // Arrange
            var approvalParameter = new ReviewArtifactApprovalParameter
            {
                Approval = "Pending",
                ApprovalFlag = ApprovalType.NotSpecified,
                ArtifactIds = new List<int> { 3 },
                MeaningOfSignatures = new[]
                {
                    new SelectedMeaningOfSignatureValue()
                    {
                        MeaningOfSignatureId = 4,
                        RoleId = 5
                    }
                }
            };

            var artifactIds = new[] { 3 };

            SetupReviewArtifactsUserApprovalCheck(ReviewId, UserId, artifactIds);

            SetupGetVersionNumber(ReviewId, artifactIds);

            SetupArtifactPermissionsCheck(new[] { artifactIds[0], ReviewId }, UserId, new Dictionary<int, RolePermissions>
            {
                { 1, RolePermissions.Read },
                { 3, RolePermissions.Read }
            });

            SetupIsMeaningOfSignatureEnabledQuery(false, true);

            SetupPossibleMeaningOfSignaturesQuery(new[] { UserId }, new[]
            {
                new ParticipantMeaningOfSignatureResult()
                {
                    RoleAssignmentId = 4
                }
            });

            SetupParticipantMeaningOfSignaturesQuery(new[] { UserId }, new[]
            {
                new ParticipantMeaningOfSignatureResult()
                {
                    RoleAssignmentId = 5
                }
            });

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewArtifactApprovalAsync(ReviewId, approvalParameter, UserId);
            }
            catch (ConflictException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.MeaningOfSignatureNotPossible, ex.ErrorCode);

                return;
            }


            Assert.Fail("A ConflictException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewArtifactApprovalAsync_When_MoS_Is_Enabled_Should_Throw_If_Assigned_MoS_Doesnt_Contain_Input_MoS()
        {
            // Arrange
            var approvalParameter = new ReviewArtifactApprovalParameter
            {
                Approval = "Pending",
                ApprovalFlag = ApprovalType.NotSpecified,
                ArtifactIds = new List<int> { 3 },
                MeaningOfSignatures = new[]
                {
                    new SelectedMeaningOfSignatureValue()
                    {
                        MeaningOfSignatureId = 4,
                        RoleId = 5
                    }
                }
            };

            var artifactIds = new[] { 3 };

            SetupReviewArtifactsUserApprovalCheck(ReviewId, UserId, artifactIds);

            SetupGetVersionNumber(ReviewId, artifactIds);

            SetupArtifactPermissionsCheck(new[] { artifactIds[0], ReviewId }, UserId, new Dictionary<int, RolePermissions>
            {
                { 1, RolePermissions.Read },
                { 3, RolePermissions.Read }
            });

            SetupIsMeaningOfSignatureEnabledQuery(false, true);

            SetupPossibleMeaningOfSignaturesQuery(new[] { UserId }, new[]
            {
                new ParticipantMeaningOfSignatureResult()
                {
                    RoleAssignmentId = 5,
                    RoleId = 6,
                    MeaningOfSignatureId = 7
                }
            });

            SetupParticipantMeaningOfSignaturesQuery(new[] { UserId }, new[]
            {
                new ParticipantMeaningOfSignatureResult()
                {
                    RoleAssignmentId = 5
                }
            });

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewArtifactApprovalAsync(ReviewId, approvalParameter, UserId);
            }
            catch (ConflictException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.MeaningOfSignatureNotPossible, ex.ErrorCode);

                return;
            }


            Assert.Fail("A ConflictException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewArtifactApprovalAsync_When_MoS_Is_Enabled_Should_Set_Selected_MoS_For_Artifacts()
        {
            // Arrange
            var approvalParameter = new ReviewArtifactApprovalParameter
            {
                Approval = "Pending",
                ApprovalFlag = ApprovalType.NotSpecified,
                ArtifactIds = new List<int> { 3 },
                MeaningOfSignatures = new[]
                {
                    new SelectedMeaningOfSignatureValue()
                    {
                        MeaningOfSignatureId = 5,
                        RoleId = 6
                    }
                }
            };

            var artifactIds = new[] { 3 };

            SetupReviewArtifactsUserApprovalCheck(ReviewId, UserId, artifactIds);

            SetupGetVersionNumber(ReviewId, artifactIds);

            SetupArtifactPermissionsCheck(new[] { artifactIds[0], ReviewId }, UserId, new Dictionary<int, RolePermissions>
            {
                { 1, RolePermissions.Read },
                { 3, RolePermissions.Read }
            });

            SetupIsMeaningOfSignatureEnabledQuery(false, true);

            SetupPossibleMeaningOfSignaturesQuery(new[] { UserId }, new[]
            {
                new ParticipantMeaningOfSignatureResult()
                {
                    ParticipantId = UserId,
                    RoleAssignmentId = 4,
                    MeaningOfSignatureId = 5,
                    MeaningOfSignatureValue = "mos",
                    RoleId = 6,
                    RoleName = "role"
                }
            });

            SetupParticipantMeaningOfSignaturesQuery(new[] { UserId }, new[]
            {
                new ParticipantMeaningOfSignatureResult()
                {
                    ParticipantId = UserId,
                    RoleId = 6
                }
            });

            var getXmlParameters = new Dictionary<string, object>
            {
                { "reviewId", ReviewId },
                { "userId", UserId }
            };

            _cxn.SetupQueryAsync("GetReviewUserStatsXml", getXmlParameters, new List<string>
            {
                null
            });

            var updateXmlParameters = new Dictionary<string, object>
            {
                { "reviewId", ReviewId },
                { "userId", UserId },
                { "updateReviewerStatus", false },
                { "value", "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewedArtifacts xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><ReviewedArtifacts><RA><A>Pending</A><EO>2017-07-10T13:20:00</EO><Id>3</Id><SMSFE xmlns:d4p1=\"Blueprint.Reviews\"><d4p1:RESMI><d4p1:MOSEV>mos</d4p1:MOSEV><d4p1:MOSRID>6</d4p1:MOSRID><d4p1:MOSRN>role</d4p1:MOSRN><d4p1:MoSEID>5</d4p1:MoSEID></d4p1:RESMI></SMSFE><V>1</V><VS>Viewed</VS></RA></ReviewedArtifacts></RDReviewedArtifacts>" }
            };

            _cxn.SetupExecuteAsync("UpdateReviewUserStats", updateXmlParameters, 1);

            // Act
            await _reviewsRepository.UpdateReviewArtifactApprovalAsync(ReviewId, approvalParameter, UserId);

            _cxn.Verify();
        }

        private void SetupGetVersionNumber(int reviewId, IEnumerable<int> artifactIds)
        {
            var getCheckParameters = new Dictionary<string, object>()
            {
                { "reviewId", reviewId },
                { "artifactIds", SqlConnectionWrapper.ToDataTable(artifactIds) }
            };

            _cxn.SetupQueryAsync("GetReviewArtifactVersionNumber", getCheckParameters, artifactIds.Select(id => new ReviewArtifactVersionNumber
            {
                ArtifactId = id,
                VersionNumber = 1
            }));
        }

        private void SetupArtifactApprovalCheck(int reviewId, int userId, IEnumerable<int> artifactIds, Action<ReviewArtifactApprovalCheck> setCheckResult = null)
        {
            var getCheckParameters = new Dictionary<string, object>
            {
                { "reviewId", reviewId },
                { "userId", userId },
                { "artifactIds", SqlConnectionWrapper.ToDataTable(artifactIds) }
            };

            var check = new ReviewArtifactApprovalCheck
            {
                AllArtifactsInReview = true,
                AllArtifactsRequireApproval = true,
                ReviewStatus = ReviewPackageStatus.Active,
                ReviewerRole = ReviewParticipantRole.Approver,
                ReviewExists = true,
                UserInReview = true,
                ReviewerStatus = ReviewStatus.InProgress,
                ReviewType = ReviewType.Informal
            };

            setCheckResult?.Invoke(check);

            _cxn.SetupQueryMultipleAsync("CheckReviewArtifactUserApproval", getCheckParameters, Tuple.Create(new[] { check }.AsEnumerable(), new int[0].AsEnumerable()));
        }

        #endregion

        #region UpdateReviewArtifactsViewedAsync

        [TestMethod]
        public async Task UpdateReviewArtifactViewedAsync_Should_Throw_When_Review_Doesnt_Exist()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var viewInput = new ReviewArtifactViewedInput
            {
                ArtifactIds = new[] { 3 },
                Viewed = true
            };

            SetupArtifactApprovalCheck(reviewId, userId, viewInput.ArtifactIds, check => check.ReviewExists = false);

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewArtifactsViewedAsync(reviewId, viewInput, userId);
            }
            catch (ResourceNotFoundException ex)
            {
                Assert.AreEqual(ErrorCodes.ResourceNotFound, ex.ErrorCode);

                return;
            }

            Assert.Fail("A ResourceNotFoundException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewArtifactViewedAsync_Should_Throw_When_Review_Is_Draft()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var viewInput = new ReviewArtifactViewedInput
            {
                ArtifactIds = new[] { 3 },
                Viewed = true
            };

            SetupArtifactApprovalCheck(reviewId, userId, viewInput.ArtifactIds, check => check.ReviewStatus = ReviewPackageStatus.Draft);

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewArtifactsViewedAsync(reviewId, viewInput, userId);
            }
            catch (ResourceNotFoundException ex)
            {
                Assert.AreEqual(ErrorCodes.ResourceNotFound, ex.ErrorCode);

                return;
            }

            Assert.Fail("A ResourceNotFoundException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewArtifactViewedAsync_Should_Throw_When_Review_Is_Deleted()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var viewInput = new ReviewArtifactViewedInput
            {
                ArtifactIds = new[] { 3 },
                Viewed = true
            };

            SetupArtifactApprovalCheck(reviewId, userId, viewInput.ArtifactIds, check => check.ReviewDeleted = true);

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewArtifactsViewedAsync(reviewId, viewInput, userId);
            }
            catch (ResourceNotFoundException ex)
            {
                Assert.AreEqual(ErrorCodes.ResourceNotFound, ex.ErrorCode);

                return;
            }

            Assert.Fail("A ResourceNotFoundException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewArtifactViewedAsync_Should_Throw_When_Review_Is_Closed()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var viewInput = new ReviewArtifactViewedInput
            {
                ArtifactIds = new[] { 3 },
                Viewed = true
            };

            SetupArtifactApprovalCheck(reviewId, userId, viewInput.ArtifactIds, check => check.ReviewStatus = ReviewPackageStatus.Closed);

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewArtifactsViewedAsync(reviewId, viewInput, userId);
            }
            catch (ConflictException ex)
            {
                Assert.AreEqual(ErrorCodes.ReviewClosed, ex.ErrorCode);

                return;
            }

            Assert.Fail("A ConflictException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewArtifactViewedAsync_Should_Throw_When_User_Is_Not_A_Reviewer_And_Review_Is_Informal()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var viewInput = new ReviewArtifactViewedInput
            {
                ArtifactIds = new[] { 3 },
                Viewed = true
            };

            SetupArtifactApprovalCheck(reviewId, userId, viewInput.ArtifactIds, check =>
            {
                check.UserInReview = false;
                check.ReviewType = ReviewType.Informal;
            });

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewArtifactsViewedAsync(reviewId, viewInput, userId);
            }
            catch (AuthorizationException ex)
            {
                Assert.AreEqual(ErrorCodes.UserNotInReview, ex.ErrorCode);

                return;
            }

            Assert.Fail("An AuthorizationException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewArtifactViewedAsync_Should_Throw_When_User_Is_Not_A_Reviewer_And_Review_Is_Formal()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var viewInput = new ReviewArtifactViewedInput
            {
                ArtifactIds = new[] { 3 },
                Viewed = true
            };

            SetupArtifactApprovalCheck(reviewId, userId, viewInput.ArtifactIds, check =>
            {
                check.UserInReview = false;
                check.ReviewType = ReviewType.Formal;
            });

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewArtifactsViewedAsync(reviewId, viewInput, userId);
            }
            catch (AuthorizationException ex)
            {
                Assert.AreEqual(ErrorCodes.UserNotInReview, ex.ErrorCode);

                return;
            }

            Assert.Fail("An AuthorizationException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewArtifactViewedAsync_Should_Throw_When_Reviewer_Status_Is_Completed()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var viewInput = new ReviewArtifactViewedInput
            {
                ArtifactIds = new[] { 3 },
                Viewed = true
            };

            SetupArtifactApprovalCheck(reviewId, userId, viewInput.ArtifactIds, check => check.ReviewerStatus = ReviewStatus.Completed);

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewArtifactsViewedAsync(reviewId, viewInput, userId);
            }
            catch (BadRequestException)
            {
                return;
            }

            Assert.Fail("A BadRequestException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewArtifactViewedAsync_Should_Throw_When_Artifact_Is_Not_Part_Of_The_Review()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var viewInput = new ReviewArtifactViewedInput
            {
                ArtifactIds = new[] { 3 },
                Viewed = true
            };

            SetupArtifactApprovalCheck(reviewId, userId, viewInput.ArtifactIds, check => check.AllArtifactsInReview = false);

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewArtifactsViewedAsync(reviewId, viewInput, userId);
            }
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ErrorCodes.ArtifactNotFound, ex.ErrorCode);

                return;
            }

            Assert.Fail("A BadRequestException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewArtifactViewedAsync_Should_Throw_When_User_Cannot_Access_Review()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var artifactId = 3;
            var viewInput = new ReviewArtifactViewedInput
            {
                ArtifactIds = new[] { artifactId },
                Viewed = true
            };

            SetupArtifactApprovalCheck(reviewId, userId, viewInput.ArtifactIds);

            SetupArtifactPermissionsCheck(new[] { artifactId, reviewId }, userId, new Dictionary<int, RolePermissions>()
            {
                { 3, RolePermissions.Read }
            });

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewArtifactsViewedAsync(reviewId, viewInput, userId);
            }
            catch (AuthorizationException ex)
            {
                Assert.AreEqual(ErrorCodes.UnauthorizedAccess, ex.ErrorCode);

                return;
            }

            Assert.Fail("An AuthorizationException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewArtifactViewedAsync_Should_Throw_When_User_Cannot_Access_Review_Artifact()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var artifactId = 3;
            var viewInput = new ReviewArtifactViewedInput
            {
                ArtifactIds = new[] { artifactId },
                Viewed = true
            };

            SetupArtifactApprovalCheck(reviewId, userId, viewInput.ArtifactIds);

            SetupArtifactPermissionsCheck(new[] { artifactId, reviewId }, userId, new Dictionary<int, RolePermissions>()
            {
                { 3, RolePermissions.None }
            });

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewArtifactsViewedAsync(reviewId, viewInput, userId);
            }
            catch (AuthorizationException ex)
            {
                Assert.AreEqual(ErrorCodes.UnauthorizedAccess, ex.ErrorCode);

                return;
            }

            Assert.Fail("An AuthorizationException was not thrown.");
        }

        #endregion

        #region UpdateReviewerStatusAsync

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task UpdateReviewerStatusAsync_Set_To_NotStarted_Should_Throw()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var revisionId = int.MaxValue;
            var parameter = new ReviewerStatusParameter()
            {
                Status = ReviewStatus.NotStarted
            };

            // Act
            await _reviewsRepository.UpdateReviewerStatusAsync(reviewId, revisionId, parameter, userId);
        }

        [TestMethod]
        public async Task UpdateReviewerStatusAsync_Set_To_InProgress_Should_Throw_When_Review_Is_Not_Found()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var revisionId = int.MaxValue;
            var parameter = new ReviewerStatusParameter()
            {
                Status = ReviewStatus.InProgress
            };

            SetupArtifactApprovalCheck(reviewId, userId, new int[0], check => check.ReviewExists = false);

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewerStatusAsync(reviewId, revisionId, parameter, userId);
            }
            // Assert
            catch (ResourceNotFoundException ex)
            {
                Assert.AreEqual(ErrorCodes.ResourceNotFound, ex.ErrorCode);

                return;
            }

            Assert.Fail("A ResourceNotFoundException was not thrown");
        }

        [TestMethod]
        public async Task UpdateReviewerStatusAsync_Set_To_InProgress_Should_Throw_When_Review_Has_Been_Deleted()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var revisionId = int.MaxValue;
            var parameter = new ReviewerStatusParameter()
            {
                Status = ReviewStatus.InProgress
            };

            SetupArtifactApprovalCheck(reviewId, userId, new int[0], check => check.ReviewDeleted = true);

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewerStatusAsync(reviewId, revisionId, parameter, userId);
            }
            // Assert
            catch (ResourceNotFoundException ex)
            {
                Assert.AreEqual(ErrorCodes.ResourceNotFound, ex.ErrorCode);

                return;
            }

            Assert.Fail("A ResourceNotFoundException was not thrown");
        }

        [TestMethod]
        public async Task UpdateReviewerStatusAsync_Set_To_InProgress_Should_Throw_When_Review_Is_In_Draft()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var revisionId = int.MaxValue;
            var parameter = new ReviewerStatusParameter()
            {
                Status = ReviewStatus.InProgress
            };

            SetupArtifactApprovalCheck(reviewId, userId, new int[0], check => check.ReviewStatus = ReviewPackageStatus.Draft);

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewerStatusAsync(reviewId, revisionId, parameter, userId);
            }
            // Assert
            catch (ResourceNotFoundException ex)
            {
                Assert.AreEqual(ErrorCodes.ResourceNotFound, ex.ErrorCode);

                return;
            }

            Assert.Fail("A ResourceNotFoundException was not thrown");
        }

        [TestMethod]
        public async Task UpdateReviewerStatusAsync_Set_To_InProgress_Should_Throw_When_Review_Is_Closed()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var revisionId = int.MaxValue;
            var parameter = new ReviewerStatusParameter()
            {
                Status = ReviewStatus.InProgress
            };

            SetupArtifactApprovalCheck(reviewId, userId, new int[0], check => check.ReviewStatus = ReviewPackageStatus.Closed);

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewerStatusAsync(reviewId, revisionId, parameter, userId);
            }
            // Assert
            catch (ConflictException ex)
            {
                Assert.AreEqual(ErrorCodes.ReviewClosed, ex.ErrorCode);

                return;
            }

            Assert.Fail("A ConflictException was not thrown");
        }

        [TestMethod]
        public async Task UpdateReviewerStatusAsync_Set_To_InProgress_Should_Throw_When_User_Is_Not_In_Review()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var revisionId = int.MaxValue;
            var parameter = new ReviewerStatusParameter()
            {
                Status = ReviewStatus.InProgress
            };

            SetupArtifactApprovalCheck(reviewId, userId, new int[0], check => check.UserInReview = false);

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewerStatusAsync(reviewId, revisionId, parameter, userId);
            }
            // Assert
            catch (AuthorizationException ex)
            {
                Assert.AreEqual(ErrorCodes.UserNotInReview, ex.ErrorCode);

                return;
            }

            Assert.Fail("An AuthorizationException was not thrown");
        }

        [TestMethod]
        public async Task UpdateReviewerStatusAsync_Set_To_InProgress_Should_Throw_When_User_Does_Not_Have_Access_To_The_Review()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var revisionId = int.MaxValue;
            var parameter = new ReviewerStatusParameter()
            {
                Status = ReviewStatus.InProgress
            };

            SetupArtifactApprovalCheck(reviewId, userId, new int[0]);

            SetupArtifactPermissionsCheck(new[] { reviewId }, userId, new Dictionary<int, RolePermissions>());

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewerStatusAsync(reviewId, revisionId, parameter, userId);
            }
            // Assert
            catch (AuthorizationException ex)
            {
                Assert.AreEqual(ErrorCodes.UnauthorizedAccess, ex.ErrorCode);

                return;
            }

            Assert.Fail("An AuthorizationException was not thrown");
        }

        [TestMethod]
        public async Task UpdateReviewerStatusAsync_Set_To_InProgress_Should_Update_The_Users_Status_To_In_Progress()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var revisionId = int.MaxValue;
            var parameter = new ReviewerStatusParameter()
            {
                Status = ReviewStatus.InProgress
            };

            SetupArtifactApprovalCheck(reviewId, userId, new int[0]);

            SetupArtifactPermissionsCheck(new[] { reviewId }, userId, new Dictionary<int, RolePermissions>()
            {
                { reviewId, RolePermissions.Read }
            });

            var parameters = new Dictionary<string, object>
            {
                { "reviewId", reviewId },
                { "userId", userId },
                { "updateReviewerStatus", true },
                { "value", "InProgress" }
            };

            _cxn.SetupExecuteAsync("UpdateReviewUserStats", parameters, 1);

            // Act
            await _reviewsRepository.UpdateReviewerStatusAsync(reviewId, revisionId, parameter, userId);

            // Assert
            _cxn.Verify();
        }

        [TestMethod]
        public async Task UpdateReviewerStatusAsync_Set_To_Completed_Should_Throw_When_Review_Is_Not_Found()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var revisionId = int.MaxValue;
            var parameter = new ReviewerStatusParameter()
            {
                Status = ReviewStatus.Completed
            };

            SetupArtifactApprovalCheck(reviewId, userId, new int[0], check => check.ReviewExists = false);

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewerStatusAsync(reviewId, revisionId, parameter, userId);
            }
            // Assert
            catch (ResourceNotFoundException ex)
            {
                Assert.AreEqual(ErrorCodes.ResourceNotFound, ex.ErrorCode);

                return;
            }

            Assert.Fail("A ResourceNotFoundException was not thrown");
        }

        [TestMethod]
        public async Task UpdateReviewerStatusAsync_Set_To_Completed_Should_Throw_When_Review_Has_Been_Deleted()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var revisionId = int.MaxValue;
            var parameter = new ReviewerStatusParameter()
            {
                Status = ReviewStatus.Completed
            };

            SetupArtifactApprovalCheck(reviewId, userId, new int[0], check => check.ReviewDeleted = true);

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewerStatusAsync(reviewId, revisionId, parameter, userId);
            }
            // Assert
            catch (ResourceNotFoundException ex)
            {
                Assert.AreEqual(ErrorCodes.ResourceNotFound, ex.ErrorCode);

                return;
            }

            Assert.Fail("A ResourceNotFoundException was not thrown");
        }

        [TestMethod]
        public async Task UpdateReviewerStatusAsync_Set_To_Completed_Should_Throw_When_Review_Is_In_Draft()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var revisionId = int.MaxValue;
            var parameter = new ReviewerStatusParameter()
            {
                Status = ReviewStatus.Completed
            };

            SetupArtifactApprovalCheck(reviewId, userId, new int[0], check => check.ReviewStatus = ReviewPackageStatus.Draft);

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewerStatusAsync(reviewId, revisionId, parameter, userId);
            }
            // Assert
            catch (ResourceNotFoundException ex)
            {
                Assert.AreEqual(ErrorCodes.ResourceNotFound, ex.ErrorCode);

                return;
            }

            Assert.Fail("A ResourceNotFoundException was not thrown");
        }

        [TestMethod]
        public async Task UpdateReviewerStatusAsync_Set_To_Completeds_Should_Throw_When_Review_Is_Closed()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var revisionId = int.MaxValue;
            var parameter = new ReviewerStatusParameter()
            {
                Status = ReviewStatus.Completed
            };

            SetupArtifactApprovalCheck(reviewId, userId, new int[0], check => check.ReviewStatus = ReviewPackageStatus.Closed);

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewerStatusAsync(reviewId, revisionId, parameter, userId);
            }
            // Assert
            catch (ConflictException ex)
            {
                Assert.AreEqual(ErrorCodes.ReviewClosed, ex.ErrorCode);

                return;
            }

            Assert.Fail("A ConflictException was not thrown");
        }

        [TestMethod]
        public async Task UpdateReviewerStatusAsync_Set_To_Completed_Should_Throw_When_User_Is_Not_In_Review()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var revisionId = int.MaxValue;
            var parameter = new ReviewerStatusParameter()
            {
                Status = ReviewStatus.Completed
            };

            SetupArtifactApprovalCheck(reviewId, userId, new int[0], check => check.UserInReview = false);

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewerStatusAsync(reviewId, revisionId, parameter, userId);
            }
            // Assert
            catch (AuthorizationException ex)
            {
                Assert.AreEqual(ErrorCodes.UserNotInReview, ex.ErrorCode);

                return;
            }

            Assert.Fail("An AuthorizationException was not thrown");
        }

        [TestMethod]
        public async Task UpdateReviewerStatusAsync_Set_To_Completed_Should_Throw_When_User_Does_Not_Have_Access_To_The_Review()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var revisionId = int.MaxValue;
            var parameter = new ReviewerStatusParameter()
            {
                Status = ReviewStatus.Completed
            };

            SetupArtifactApprovalCheck(reviewId, userId, new int[0]);

            SetupArtifactPermissionsCheck(new[] { reviewId }, userId, new Dictionary<int, RolePermissions>());

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewerStatusAsync(reviewId, revisionId, parameter, userId);
            }
            // Assert
            catch (AuthorizationException ex)
            {
                Assert.AreEqual(ErrorCodes.UnauthorizedAccess, ex.ErrorCode);

                return;
            }

            Assert.Fail("An AuthorizationException was not thrown");
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task UpdateReviewerStatusAsync_Set_To_Completed_Should_Throw_When_Reviewer_Status_Is_Not_Started()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var revisionId = int.MaxValue;
            var parameter = new ReviewerStatusParameter()
            {
                Status = ReviewStatus.Completed
            };

            SetupArtifactApprovalCheck(reviewId, userId, new int[0], check => check.ReviewerStatus = ReviewStatus.NotStarted);

            SetupArtifactPermissionsCheck(new[] { reviewId }, userId, new Dictionary<int, RolePermissions>
            {
                { reviewId, RolePermissions.Read }
            });

            // Act
            await _reviewsRepository.UpdateReviewerStatusAsync(reviewId, revisionId, parameter, userId);
        }

        [TestMethod]
        public async Task UpdateReviewerStatusAsync_Set_To_Completed_Should_Be_Successful_When_RequireAllApproval_Is_False()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var revisionId = int.MaxValue;
            var parameter = new ReviewerStatusParameter()
            {
                Status = ReviewStatus.Completed
            };

            SetupArtifactApprovalCheck(reviewId, userId, new int[0]);

            SetupArtifactPermissionsCheck(new[] { reviewId }, userId, new Dictionary<int, RolePermissions>
            {
                { reviewId, RolePermissions.Read }
            });

            SetupGetRequireAllArtifactsReviewedQuery(false, false);

            // Act
            await _reviewsRepository.UpdateReviewerStatusAsync(reviewId, revisionId, parameter, userId);

            // Assert
            _cxn.Verify();
        }

        [TestMethod]
        public async Task UpdateReviewerStatusAsync_Set_To_Completed_Should_Be_Successful_When_RequireAllApproval_Is_True_And_Artifact_Is_Approved()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var revisionId = int.MaxValue;
            var parameter = new ReviewerStatusParameter()
            {
                Status = ReviewStatus.Completed
            };

            SetupArtifactApprovalCheck(reviewId, userId, new int[0]);

            SetupArtifactPermissionsCheck(new[] { reviewId }, userId, new Dictionary<int, RolePermissions>
            {
                { reviewId, RolePermissions.Read }
            });

            _applicationSettingsRepositoryMock.Setup(repo => repo.GetValue("ReviewArtifactHierarchyRebuildIntervalInMinutes", 20)).ReturnsAsync(20);

            SetupGetRequireAllArtifactsReviewedQuery(false, true);

            SetupReviewedArtifactsQueries(reviewId, userId, ra => ra.ApprovalFlag = ApprovalType.Approved);

            // Act
            await _reviewsRepository.UpdateReviewerStatusAsync(reviewId, revisionId, parameter, userId);

            // Assert
            _cxn.Verify();
        }

        [TestMethod]
        public async Task UpdateReviewerStatusAsync_Set_To_Completed_Should_Be_Successful_When_RequireAllApproval_Is_True_And_Artifact_Is_Disapproved()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var revisionId = int.MaxValue;
            var parameter = new ReviewerStatusParameter()
            {
                Status = ReviewStatus.Completed
            };

            SetupArtifactApprovalCheck(reviewId, userId, new int[0]);

            SetupArtifactPermissionsCheck(new[] { reviewId }, userId, new Dictionary<int, RolePermissions>
            {
                { reviewId, RolePermissions.Read }
            });

            _applicationSettingsRepositoryMock.Setup(repo => repo.GetValue("ReviewArtifactHierarchyRebuildIntervalInMinutes", 20)).ReturnsAsync(20);

            SetupGetRequireAllArtifactsReviewedQuery(false, true);

            SetupReviewedArtifactsQueries(reviewId, userId, ra => ra.ApprovalFlag = ApprovalType.Disapproved);

            // Act
            await _reviewsRepository.UpdateReviewerStatusAsync(reviewId, revisionId, parameter, userId);

            // Assert
            _cxn.Verify();
        }

        [TestMethod]
        public async Task UpdateReviewerStatusAsync_Set_To_Completed_Should_Be_Successful_When_RequireAllApproval_Is_True_And_Artifact_Is_Not_Accessible()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var revisionId = int.MaxValue;
            var parameter = new ReviewerStatusParameter()
            {
                Status = ReviewStatus.Completed
            };

            SetupArtifactApprovalCheck(reviewId, userId, new int[0]);

            SetupArtifactPermissionsCheck(new[] { reviewId }, userId, new Dictionary<int, RolePermissions>
            {
                { reviewId, RolePermissions.Read }
            });

            _applicationSettingsRepositoryMock.Setup(repo => repo.GetValue("ReviewArtifactHierarchyRebuildIntervalInMinutes", 20)).ReturnsAsync(20);

            SetupGetRequireAllArtifactsReviewedQuery(false, true);

            SetupReviewedArtifactsQueries(reviewId, userId, ra => ra.HasAccess = false);

            // Act
            await _reviewsRepository.UpdateReviewerStatusAsync(reviewId, revisionId, parameter, userId);

            // Assert
            _cxn.Verify();
        }

        [TestMethod]
        public async Task UpdateReviewerStatusAsync_Set_To_Completed_Should_Be_Successful_When_RequireAllApproval_Is_True_And_Artifact_Approval_Is_Not_Required()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var revisionId = int.MaxValue;
            var parameter = new ReviewerStatusParameter()
            {
                Status = ReviewStatus.Completed
            };

            SetupArtifactApprovalCheck(reviewId, userId, new int[0]);

            SetupArtifactPermissionsCheck(new[] { reviewId }, userId, new Dictionary<int, RolePermissions>
            {
                { reviewId, RolePermissions.Read }
            });

            _applicationSettingsRepositoryMock.Setup(repo => repo.GetValue("ReviewArtifactHierarchyRebuildIntervalInMinutes", 20)).ReturnsAsync(20);

            SetupGetRequireAllArtifactsReviewedQuery(false, true);

            SetupReviewedArtifactsQueries(reviewId, userId, ra => ra.IsApprovalRequired = false);

            // Act
            await _reviewsRepository.UpdateReviewerStatusAsync(reviewId, revisionId, parameter, userId);

            // Assert
            _cxn.Verify();
        }

        [TestMethod]
        public async Task UpdateReviewerStatusAsync_Set_To_Completed_Should_Be_Successful_When_RequireAllApproval_Is_True_And_Participant_Is_Reviewer_And_Artifact_Is_Viewed()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var revisionId = int.MaxValue;
            var parameter = new ReviewerStatusParameter()
            {
                Status = ReviewStatus.Completed
            };

            SetupArtifactApprovalCheck(reviewId, userId, new int[0], check => check.ReviewerRole = ReviewParticipantRole.Reviewer);

            SetupArtifactPermissionsCheck(new[] { reviewId }, userId, new Dictionary<int, RolePermissions>
            {
                { reviewId, RolePermissions.Read }
            });

            _applicationSettingsRepositoryMock.Setup(repo => repo.GetValue("ReviewArtifactHierarchyRebuildIntervalInMinutes", 20)).ReturnsAsync(20);

            SetupGetRequireAllArtifactsReviewedQuery(false, true);

            SetupReviewedArtifactsQueries(reviewId, userId, ra =>
            {
                ra.ArtifactVersion = 2;
                ra.ViewedArtifactVersion = 2;
                ra.ViewState = ViewStateType.Viewed;
            });

            // Act
            await _reviewsRepository.UpdateReviewerStatusAsync(reviewId, revisionId, parameter, userId);

            // Assert
            _cxn.Verify();
        }

        [TestMethod]
        public async Task UpdateReviewerStatusAsync_Set_To_Completed_Should_Throw_When_RequireAllApproval_Is_True_And_Participant_Is_Reviewer_And_Artifact_Is_Not_Viewed()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var revisionId = int.MaxValue;
            var parameter = new ReviewerStatusParameter()
            {
                Status = ReviewStatus.Completed
            };

            SetupArtifactApprovalCheck(reviewId, userId, new int[0], check => check.ReviewerRole = ReviewParticipantRole.Reviewer);

            SetupArtifactPermissionsCheck(new[] { reviewId }, userId, new Dictionary<int, RolePermissions>
            {
                { reviewId, RolePermissions.Read }
            });

            _applicationSettingsRepositoryMock.Setup(repo => repo.GetValue("ReviewArtifactHierarchyRebuildIntervalInMinutes", 20)).ReturnsAsync(20);

            SetupGetRequireAllArtifactsReviewedQuery(false, true);

            SetupReviewedArtifactsQueries(reviewId, userId, ra =>
            {
                ra.ViewState = ViewStateType.NotViewed;
            });

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewerStatusAsync(reviewId, revisionId, parameter, userId);
            }
            // Assert
            catch (ConflictException ex)
            {
                Assert.AreEqual(ErrorCodes.NotAllArtifactsReviewed, ex.ErrorCode);

                return;
            }

            Assert.Fail("A Conflict Exception was not thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewerStatusAsync_Set_To_Completed_Should_Throw_When_RequireAllApproval_Is_True_And_Participant_Is_Reviewer_And_Artifact_Is_Changed()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var revisionId = int.MaxValue;
            var parameter = new ReviewerStatusParameter()
            {
                Status = ReviewStatus.Completed
            };

            SetupArtifactApprovalCheck(reviewId, userId, new int[0], check => check.ReviewerRole = ReviewParticipantRole.Reviewer);

            SetupArtifactPermissionsCheck(new[] { reviewId }, userId, new Dictionary<int, RolePermissions>
            {
                { reviewId, RolePermissions.Read }
            });

            _applicationSettingsRepositoryMock.Setup(repo => repo.GetValue("ReviewArtifactHierarchyRebuildIntervalInMinutes", 20)).ReturnsAsync(20);

            SetupGetRequireAllArtifactsReviewedQuery(false, true);

            SetupReviewedArtifactsQueries(reviewId, userId, ra =>
            {
                ra.ViewState = ViewStateType.Viewed;
                ra.ArtifactVersion = 2;
                ra.ViewedArtifactVersion = 1;
            });

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewerStatusAsync(reviewId, revisionId, parameter, userId);
            }
            // Assert
            catch (ConflictException ex)
            {
                Assert.AreEqual(ErrorCodes.NotAllArtifactsReviewed, ex.ErrorCode);

                return;
            }

            Assert.Fail("A Conflict Exception was not thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewerStatusAsync_Set_To_Completed_Should_Throw_When_RequireAllApproval_Is_True_And_Artifact_Is_Not_Approved()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var revisionId = int.MaxValue;
            var parameter = new ReviewerStatusParameter()
            {
                Status = ReviewStatus.Completed
            };

            SetupArtifactApprovalCheck(reviewId, userId, new int[0]);

            SetupArtifactPermissionsCheck(new[] { reviewId }, userId, new Dictionary<int, RolePermissions>
            {
                { reviewId, RolePermissions.Read }
            });

            _applicationSettingsRepositoryMock.Setup(repo => repo.GetValue("ReviewArtifactHierarchyRebuildIntervalInMinutes", 20)).ReturnsAsync(20);

            SetupGetRequireAllArtifactsReviewedQuery(false, true);

            SetupReviewedArtifactsQueries(reviewId, userId, ra =>
            {
                ra.ApprovalFlag = ApprovalType.NotSpecified;
            });

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewerStatusAsync(reviewId, revisionId, parameter, userId);
            }
            // Assert
            catch (ConflictException ex)
            {
                Assert.AreEqual(ErrorCodes.NotAllArtifactsReviewed, ex.ErrorCode);

                return;
            }

            Assert.Fail("A Conflict Exception was not thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewerStatusAsync_Set_To_Completed_Should_Throw_When_RequiresMoS_Is_True_And_Parameter_MoS_Is_Null()
        {
            // Arrange
            var revisionId = int.MaxValue;
            var parameter = new ReviewerStatusParameter()
            {
                Status = ReviewStatus.Completed,
                MeaningOfSignatures = null
            };

            SetupArtifactApprovalCheck(ReviewId, UserId, new int[0], check => check.ReviewerRole = ReviewParticipantRole.Approver);

            SetupArtifactPermissionsCheck(new[] { ReviewId }, UserId, new Dictionary<int, RolePermissions>
            {
                { ReviewId, RolePermissions.Read }
            });

            SetupIsMeaningOfSignatureEnabledQuery(false, true);

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewerStatusAsync(ReviewId, revisionId, parameter, UserId);
            }
            // Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ErrorCodes.MeaningOfSignatureNotChosen, ex.ErrorCode);

                return;
            }

            Assert.Fail("A BadRequestException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewerStatusAsync_Set_To_Completed_Should_Throw_When_RequiresMoS_Is_True_And_Parameter_MoS_Is_Empty()
        {
            // Arrange
            var revisionId = int.MaxValue;
            var parameter = new ReviewerStatusParameter()
            {
                Status = ReviewStatus.Completed,
                MeaningOfSignatures = new SelectedMeaningOfSignatureValue[0]
            };

            SetupArtifactApprovalCheck(ReviewId, UserId, new int[0], check => check.ReviewerRole = ReviewParticipantRole.Approver);

            SetupArtifactPermissionsCheck(new[] { ReviewId }, UserId, new Dictionary<int, RolePermissions>
            {
                { ReviewId, RolePermissions.Read }
            });

            SetupIsMeaningOfSignatureEnabledQuery(false, true);

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewerStatusAsync(ReviewId, revisionId, parameter, UserId);
            }
            // Assert
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ErrorCodes.MeaningOfSignatureNotChosen, ex.ErrorCode);

                return;
            }

            Assert.Fail("A BadRequestException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewerStatusAsync_Set_To_Completed_Should_Throw_When_RequiresMoS_Is_True_And_No_Possible_MoS()
        {
            // Arrange
            var revisionId = int.MaxValue;
            var parameter = new ReviewerStatusParameter()
            {
                Status = ReviewStatus.Completed,
                MeaningOfSignatures = new[]
                {
                    new SelectedMeaningOfSignatureValue()
                    {
                        MeaningOfSignatureId = 3,
                        RoleId = 4
                    }
                }
            };

            SetupArtifactApprovalCheck(ReviewId, UserId, new int[0], check => check.ReviewerRole = ReviewParticipantRole.Approver);

            SetupArtifactPermissionsCheck(new[] { ReviewId }, UserId, new Dictionary<int, RolePermissions>
            {
                { ReviewId, RolePermissions.Read }
            });

            SetupIsMeaningOfSignatureEnabledQuery(false, true);

            SetupPossibleMeaningOfSignaturesQuery(new[] { UserId }, new ParticipantMeaningOfSignatureResult[0]);

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewerStatusAsync(ReviewId, revisionId, parameter, UserId);
            }
            // Assert
            catch (ConflictException ex)
            {
                Assert.AreEqual(ErrorCodes.MeaningOfSignatureNotPossible, ex.ErrorCode);

                return;
            }

            Assert.Fail("A ConflictException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewerStatusAsync_Set_To_Completed_Should_Throw_When_RequiresMoS_Is_True_And_No_Assigned_MoS()
        {
            // Arrange
            var revisionId = int.MaxValue;
            var parameter = new ReviewerStatusParameter()
            {
                Status = ReviewStatus.Completed,
                MeaningOfSignatures = new[]
                {
                    new SelectedMeaningOfSignatureValue()
                    {
                        MeaningOfSignatureId = 3,
                        RoleId = 4
                    }
                }
            };

            SetupArtifactApprovalCheck(ReviewId, UserId, new int[0], check => check.ReviewerRole = ReviewParticipantRole.Approver);

            SetupArtifactPermissionsCheck(new[] { ReviewId }, UserId, new Dictionary<int, RolePermissions>
            {
                { ReviewId, RolePermissions.Read }
            });

            SetupIsMeaningOfSignatureEnabledQuery(false, true);

            SetupPossibleMeaningOfSignaturesQuery(new[] { UserId }, new[]
            {
                new ParticipantMeaningOfSignatureResult()
                {
                    ParticipantId = UserId,
                    RoleAssignmentId = 5,
                    MeaningOfSignatureId = 3,
                    RoleId = 4
                }
            });

            SetupParticipantMeaningOfSignaturesQuery(new[] { UserId }, new ParticipantMeaningOfSignatureResult[0]);

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewerStatusAsync(ReviewId, revisionId, parameter, UserId);
            }
            // Assert
            catch (ConflictException ex)
            {
                Assert.AreEqual(ErrorCodes.MeaningOfSignatureNotPossible, ex.ErrorCode);

                return;
            }

            Assert.Fail("A ConflictException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewerStatusAsync_Set_To_Completed_Should_Throw_When_RequiresMoS_Is_True_And_MoS_Dont_Match()
        {
            // Arrange
            var revisionId = int.MaxValue;
            var parameter = new ReviewerStatusParameter()
            {
                Status = ReviewStatus.Completed,
                MeaningOfSignatures = new[]
                {
                    new SelectedMeaningOfSignatureValue()
                    {
                        MeaningOfSignatureId = 3,
                        RoleId = 4
                    }
                }
            };

            SetupArtifactApprovalCheck(ReviewId, UserId, new int[0], check => check.ReviewerRole = ReviewParticipantRole.Approver);

            SetupArtifactPermissionsCheck(new[] { ReviewId }, UserId, new Dictionary<int, RolePermissions>
            {
                { ReviewId, RolePermissions.Read }
            });

            SetupIsMeaningOfSignatureEnabledQuery(false, true);

            SetupPossibleMeaningOfSignaturesQuery(new[] { UserId }, new[]
            {
                new ParticipantMeaningOfSignatureResult()
                {
                    ParticipantId = UserId,
                    RoleAssignmentId = 5,
                    MeaningOfSignatureId = 6,
                    RoleId = 7
                }
            });

            SetupParticipantMeaningOfSignaturesQuery(new[] { UserId }, new[]
            {
                new ParticipantMeaningOfSignatureResult()
                {
                    ParticipantId = UserId,
                    RoleAssignmentId = 5,
                    MeaningOfSignatureId = 6,
                    RoleId = 7
                }
            });

            // Act
            try
            {
                await _reviewsRepository.UpdateReviewerStatusAsync(ReviewId, revisionId, parameter, UserId);
            }
            // Assert
            catch (ConflictException ex)
            {
                Assert.AreEqual(ErrorCodes.MeaningOfSignatureNotPossible, ex.ErrorCode);

                return;
            }

            Assert.Fail("A ConflictException was not thrown.");
        }

        [TestMethod]
        public async Task UpdateReviewerStatusAsync_Set_To_Completed_Should_Add_Completion_Meaning_Of_Signature_When_MoS_Is_Enabled()
        {
            // Arrange
            var revisionId = int.MaxValue;
            var parameter = new ReviewerStatusParameter()
            {
                Status = ReviewStatus.Completed,
                MeaningOfSignatures = new[]
                {
                    new SelectedMeaningOfSignatureValue()
                    {
                        MeaningOfSignatureId = 3,
                        RoleId = 4
                    }
                }
            };

            SetupArtifactApprovalCheck(ReviewId, UserId, new int[0], check => check.ReviewerRole = ReviewParticipantRole.Approver);

            SetupArtifactPermissionsCheck(new[] { ReviewId }, UserId, new Dictionary<int, RolePermissions>
            {
                { ReviewId, RolePermissions.Read }
            });

            SetupIsMeaningOfSignatureEnabledQuery(false, true);

            SetupPossibleMeaningOfSignaturesQuery(new[] { UserId }, new[]
            {
                new ParticipantMeaningOfSignatureResult()
                {
                    ParticipantId = UserId,
                    RoleAssignmentId = 5,
                    MeaningOfSignatureId = 3,
                    RoleId = 4,
                    RoleName = "foo",
                    MeaningOfSignatureValue = "bar"
                }
            });

            SetupParticipantMeaningOfSignaturesQuery(new[] { UserId }, new[]
            {
                new ParticipantMeaningOfSignatureResult()
                {
                    ParticipantId = UserId,
                    RoleAssignmentId = 5,
                    MeaningOfSignatureId = 3,
                    RoleId = 4
                }
            });

            SetupGetReviewUserStatsXmlQuery(null);

            SetupUpdateReviewUserStatsXmlQuery("<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewedArtifacts xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><ReviewedArtifacts /><SCMOSV xmlns:d2p1=\"Blueprint.Reviews\"><d2p1:RESMI><d2p1:MOSEV>bar</d2p1:MOSEV><d2p1:MOSRID>4</d2p1:MOSRID><d2p1:MOSRN>foo</d2p1:MOSRN><d2p1:MoSEID>3</d2p1:MoSEID></d2p1:RESMI></SCMOSV></RDReviewedArtifacts>");

            // Act
            await _reviewsRepository.UpdateReviewerStatusAsync(ReviewId, revisionId, parameter, UserId);

            // Assert
            _cxn.Verify();
        }

        private void SetupGetRequireAllArtifactsReviewedQuery(bool addDrafts, bool value)
        {
            var requireAllArtifactReviewedParameters = new Dictionary<string, object>
            {
                { "reviewId", ReviewId },
                { "userId", UserId },
                { "addDrafts", addDrafts }
            };

            _cxn.SetupExecuteScalarAsync("GetReviewRequireAllArtifactsReviewed", requireAllArtifactReviewedParameters, value);
        }

        private void SetupGetReviewUserStatsXmlQuery(string userStatsXml)
        {
            var parameters = new Dictionary<string, object>
            {
                { "reviewId", ReviewId },
                { "userId", UserId }
            };

            _cxn.SetupQueryAsync("GetReviewUserStatsXml", parameters, new[] { userStatsXml });
        }

        private void SetupUpdateReviewUserStatsXmlQuery(string userStatsXml)
        {
            var parameters = new Dictionary<string, object>
            {
                { "reviewId", ReviewId },
                { "userId", UserId },
                { "value", userStatsXml }
            };

            _cxn.SetupExecuteAsync("UpdateReviewUserStats", parameters, 1);
        }

        private void SetupReviewedArtifactsQueries(int reviewId, int userId, Action<ReviewedArtifact> setArtifactPropertiesAction = null)
        {
            var artifactId = 3;

            var reviewArtifactsParams = new Dictionary<string, object>
            {
                { "reviewId", reviewId },
                { "offset", 0 },
                { "limit", int.MaxValue },
                { "revisionId", int.MaxValue },
                { "addDrafts", false },
                { "userId", 2 },
                { "refreshInterval", 20 }
            };

            var reviewArtifacts = new[]
            {
                new ReviewedArtifact
                {
                    Id = artifactId,
                    ViewedArtifactVersion = 1,
                    ArtifactVersion = 1,
                    ApprovalFlag = ApprovalType.Approved,
                    IsApprovalRequired = true,
                    HasAccess = true
                }
            };

            var reviewArtifactsReturnParams = new Dictionary<string, object>
            {
                { "numResult", 1 },
                { "isFormal", true }
            };

            setArtifactPropertiesAction?.Invoke(reviewArtifacts[0]);

            _cxn.SetupQueryAsync("GetReviewArtifacts", reviewArtifactsParams, reviewArtifacts, reviewArtifactsReturnParams);

            var permissions = new Dictionary<int, RolePermissions>
            {
                { reviewId, RolePermissions.Read },
                { artifactId, reviewArtifacts[0].HasAccess ? RolePermissions.Read : RolePermissions.None }
            };

            SetupArtifactPermissionsCheck(new[] { artifactId, reviewId }, userId, permissions);

            var participantReviewArtifactParams = new Dictionary<string, object>
            {
                { "itemIds", SqlConnectionWrapper.ToDataTable(new[] { artifactId }) },
                { "userId", userId },
                { "reviewId", reviewId },
                { "revisionId", int.MaxValue }
            };

            _cxn.SetupQueryAsync("GetReviewArtifactsByParticipant", participantReviewArtifactParams, reviewArtifacts);
        }

        #endregion

        #region GetReviewParticipantArtifactStatsAsync

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetReviewParticipantArtifactStatsAsync_Should_Throw_Not_Found_When_Review_Isnt_Published()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;

            _artifactVersionsRepositoryMock.Setup(repo => repo.GetVersionControlArtifactInfoAsync(reviewId, null, userId)).ReturnsAsync(new VersionControlArtifactInfo()
            {
                VersionCount = 0
            });

            // Act
            await _reviewsRepository.GetReviewParticipantArtifactStatsAsync(reviewId, userId, userId, new Pagination());
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetReviewParticipantArtifactStatsAsync_Should_Throw_Not_Found_When_Review_Is_Deleted()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;

            _artifactVersionsRepositoryMock.Setup(repo => repo.GetVersionControlArtifactInfoAsync(reviewId, null, userId)).ReturnsAsync(new VersionControlArtifactInfo()
            {
                IsDeleted = true
            });

            // Act
            await _reviewsRepository.GetReviewParticipantArtifactStatsAsync(reviewId, userId, userId, new Pagination());
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetReviewParticipantArtifactStatsAsync_Should_Throw_Unauthorized_When_User_Does_Not_Have_Read_Permission_For_Review()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var participantId = 3;
            var artifactId = 4;

            _artifactVersionsRepositoryMock.Setup(repo => repo.GetVersionControlArtifactInfoAsync(reviewId, null, userId)).ReturnsAsync(new VersionControlArtifactInfo()
            {
                VersionCount = 1
            });

            SetupReviewArtifactsQuery(reviewId, userId, new ReviewedArtifact()
            {
                Id = artifactId
            });

            _applicationSettingsRepositoryMock.Setup(repo => repo.GetValue("ReviewArtifactHierarchyRebuildIntervalInMinutes", 20)).ReturnsAsync(20);

            SetupArtifactPermissionsCheck(new[] { artifactId, reviewId }, userId, new Dictionary<int, RolePermissions>()
            {
                { reviewId, RolePermissions.None }
            });

            // Act
            await _reviewsRepository.GetReviewParticipantArtifactStatsAsync(reviewId, participantId, userId, new Pagination());
        }

        [TestMethod]
        public async Task GetReviewParticipantArtifactStatsAsync_Should_Return_Artifact_With_HasAccess_As_False_When_No_Read_Access_To_Artifact_And_Review_Is_Informal()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var participantId = 3;
            var artifactId = 4;

            _artifactVersionsRepositoryMock.Setup(repo => repo.GetVersionControlArtifactInfoAsync(reviewId, null, userId)).ReturnsAsync(new VersionControlArtifactInfo()
            {
                VersionCount = 1
            });

            SetupReviewArtifactsQuery(reviewId, userId, new ReviewedArtifact()
            {
                Id = artifactId
            });

            _applicationSettingsRepositoryMock.Setup(repo => repo.GetValue("ReviewArtifactHierarchyRebuildIntervalInMinutes", 20)).ReturnsAsync(20);

            SetupArtifactPermissionsCheck(new[] { artifactId, reviewId }, userId, new Dictionary<int, RolePermissions>
            {
                { reviewId, RolePermissions.Read },
                { artifactId, RolePermissions.None }
            });

            // Act
            var artifactStatsResult = await _reviewsRepository.GetReviewParticipantArtifactStatsAsync(reviewId, participantId, userId, new Pagination());

            // Assert
            _cxn.Verify();
            Assert.AreEqual(artifactStatsResult.Total, 1);
            Assert.AreEqual(artifactStatsResult.Items.First().HasAccess, false);
        }

        [TestMethod]
        public async Task GetReviewParticipantArtifactStatsAsync_Should_Return_Artifact_With_HasAccess_As_False_When_No_Read_Access_To_Artifact_And_Review_Is_Formal()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var participantId = 3;
            var reviewArtifact = new ReviewedArtifact
            {
                Id = 4,
                Name = "Review Artifact",
                Prefix = "REV",
                ItemTypeId = 5,
                ItemTypePredefined = 6,
                IconImageId = 7,
                IsApprovalRequired = true
            };

            var participantReviewArtifact = new ReviewedArtifact
            {
                ArtifactVersion = 1,
                ViewedArtifactVersion = 1,
                Approval = "Approved"
            };

            _artifactVersionsRepositoryMock.Setup(repo => repo.GetVersionControlArtifactInfoAsync(reviewId, null, userId)).ReturnsAsync(new VersionControlArtifactInfo
            {
                VersionCount = 1
            });

            SetupReviewArtifactsQuery(reviewId, userId, reviewArtifact, true);

            _applicationSettingsRepositoryMock.Setup(repo => repo.GetValue("ReviewArtifactHierarchyRebuildIntervalInMinutes", 20)).ReturnsAsync(20);

            SetupArtifactPermissionsCheck(new[] { reviewArtifact.Id, reviewId }, userId, new Dictionary<int, RolePermissions>
            {
                { reviewId, RolePermissions.Read }
            });

            SetupParticipantReviewArtifactsQuery(reviewId, participantId, reviewArtifact.Id, participantReviewArtifact);

            // Act
            var artifactStatsResult = await _reviewsRepository.GetReviewParticipantArtifactStatsAsync(reviewId, participantId, userId, new Pagination());

            // Assert
            _cxn.Verify();
            Assert.AreEqual(artifactStatsResult.Total, 1);
            Assert.AreEqual(artifactStatsResult.Items.First().HasAccess, false);
        }

        [TestMethod]
        public async Task GetReviewParticipantArtifactStatsAsync_Should_Return_Artifact_Successfully()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var participantId = 3;
            var reviewArtifact = new ReviewedArtifact
            {
                Id = 4,
                Name = "Review Artifact",
                Prefix = "REV",
                ItemTypeId = 5,
                ItemTypePredefined = 6,
                IconImageId = 7,
                IsApprovalRequired = true
            };

            var participantReviewArtifact = new ReviewedArtifact
            {
                ArtifactVersion = 1,
                ViewedArtifactVersion = 1,
                ViewState = ViewStateType.Viewed,
                Approval = "Approved"
            };

            _artifactVersionsRepositoryMock.Setup(repo => repo.GetVersionControlArtifactInfoAsync(reviewId, null, userId)).ReturnsAsync(new VersionControlArtifactInfo
            {
                VersionCount = 1
            });

            SetupReviewArtifactsQuery(reviewId, userId, reviewArtifact);

            _applicationSettingsRepositoryMock.Setup(repo => repo.GetValue("ReviewArtifactHierarchyRebuildIntervalInMinutes", 20)).ReturnsAsync(20);

            SetupArtifactPermissionsCheck(new[] { reviewArtifact.Id, reviewId }, userId, new Dictionary<int, RolePermissions>
            {
                { reviewId, RolePermissions.Read },
                { reviewArtifact.Id, RolePermissions.Read }
            });

            SetupParticipantReviewArtifactsQuery(reviewId, participantId, reviewArtifact.Id, participantReviewArtifact);

            // Act
            var artifactStatsResult = await _reviewsRepository.GetReviewParticipantArtifactStatsAsync(reviewId, participantId, userId, new Pagination());

            // Assert
            _cxn.Verify();
            Assert.AreEqual(artifactStatsResult.Total, 1);
            var artifactStats = artifactStatsResult.Items.First();
            Assert.AreEqual(true, artifactStats.HasAccess);
            Assert.AreEqual(4, artifactStats.Id);
            Assert.AreEqual("REV", artifactStats.Prefix);
            Assert.AreEqual("Review Artifact", artifactStats.Name);
            Assert.AreEqual(5, artifactStats.ItemTypeId);
            Assert.AreEqual(6, artifactStats.ItemTypePredefined);
            Assert.AreEqual(7, artifactStats.IconImageId);
            Assert.AreEqual(ViewStateType.Viewed, artifactStats.ViewState);
            Assert.AreEqual("Approved", artifactStats.ApprovalStatus);
            Assert.AreEqual(true, artifactStats.ArtifactRequiresApproval);
        }

        [TestMethod]
        public async Task GetReviewParticipantArtifactStatsAsync_ViewState_Should_Be_Not_Viewed_When_ViewedArtifactVersion_Is_Null()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var participantId = 3;
            var reviewArtifact = new ReviewedArtifact
            {
                Id = 4
            };

            var participantReviewArtifact = new ReviewedArtifact
            {
                ViewState = ViewStateType.Viewed,
                ArtifactVersion = 3,
                ViewedArtifactVersion = null
            };

            _artifactVersionsRepositoryMock.Setup(repo => repo.GetVersionControlArtifactInfoAsync(reviewId, null, userId)).ReturnsAsync(new VersionControlArtifactInfo()
            {
                VersionCount = 1
            });

            SetupReviewArtifactsQuery(reviewId, userId, reviewArtifact);

            _applicationSettingsRepositoryMock.Setup(repo => repo.GetValue("ReviewArtifactHierarchyRebuildIntervalInMinutes", 20)).ReturnsAsync(20);

            SetupArtifactPermissionsCheck(new[] { reviewArtifact.Id, reviewId }, userId, new Dictionary<int, RolePermissions>
            {
                { reviewId, RolePermissions.Read },
                { reviewArtifact.Id, RolePermissions.Read }
            });

            SetupParticipantReviewArtifactsQuery(reviewId, participantId, reviewArtifact.Id, participantReviewArtifact);

            // Act
            var artifactStatsResult = await _reviewsRepository.GetReviewParticipantArtifactStatsAsync(reviewId, participantId, userId, new Pagination());

            // Assert
            _cxn.Verify();
            Assert.AreEqual(artifactStatsResult.Total, 1);
            Assert.AreEqual(ViewStateType.NotViewed, artifactStatsResult.Items.First().ViewState);
        }

        [TestMethod]
        public async Task GetReviewParticipantArtifactStatsAsync_ViewState_Should_Be_Not_Viewed_When_ViewState_Is_Not_Viewed()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var participantId = 3;
            var reviewArtifact = new ReviewedArtifact
            {
                Id = 4
            };

            var participantReviewArtifact = new ReviewedArtifact
            {
                ViewState = ViewStateType.NotViewed
            };

            _artifactVersionsRepositoryMock.Setup(repo => repo.GetVersionControlArtifactInfoAsync(reviewId, null, userId)).ReturnsAsync(new VersionControlArtifactInfo
            {
                VersionCount = 1
            });

            SetupReviewArtifactsQuery(reviewId, userId, reviewArtifact);

            _applicationSettingsRepositoryMock.Setup(repo => repo.GetValue("ReviewArtifactHierarchyRebuildIntervalInMinutes", 20)).ReturnsAsync(20);

            SetupArtifactPermissionsCheck(new[] { reviewArtifact.Id, reviewId }, userId, new Dictionary<int, RolePermissions>
            {
                { reviewId, RolePermissions.Read },
                { reviewArtifact.Id, RolePermissions.Read }
            });

            SetupParticipantReviewArtifactsQuery(reviewId, participantId, reviewArtifact.Id, participantReviewArtifact);

            // Act
            var artifactStatsResult = await _reviewsRepository.GetReviewParticipantArtifactStatsAsync(reviewId, participantId, userId, new Pagination());

            // Assert
            _cxn.Verify();
            Assert.AreEqual(artifactStatsResult.Total, 1);
            Assert.AreEqual(ViewStateType.NotViewed, artifactStatsResult.Items.First().ViewState);
        }

        [TestMethod]
        public async Task GetReviewParticipantArtifactStatsAsync_ViewState_Should_Be_Changed_When_ArtifactVersion_And_ViewedArtifactVersion_Are_Different()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var participantId = 3;
            var reviewArtifact = new ReviewedArtifact
            {
                Id = 4
            };

            var participantReviewArtifact = new ReviewedArtifact
            {
                ViewState = ViewStateType.Viewed,
                ArtifactVersion = 3,
                ViewedArtifactVersion = 1
            };

            _artifactVersionsRepositoryMock.Setup(repo => repo.GetVersionControlArtifactInfoAsync(reviewId, null, userId)).ReturnsAsync(new VersionControlArtifactInfo
            {
                VersionCount = 1
            });

            SetupReviewArtifactsQuery(reviewId, userId, reviewArtifact);

            _applicationSettingsRepositoryMock.Setup(repo => repo.GetValue("ReviewArtifactHierarchyRebuildIntervalInMinutes", 20)).ReturnsAsync(20);

            SetupArtifactPermissionsCheck(new[] { reviewArtifact.Id, reviewId }, userId, new Dictionary<int, RolePermissions>
            {
                { reviewId, RolePermissions.Read },
                { reviewArtifact.Id, RolePermissions.Read }
            });

            SetupParticipantReviewArtifactsQuery(reviewId, participantId, reviewArtifact.Id, participantReviewArtifact);

            // Act
            var artifactStatsResult = await _reviewsRepository.GetReviewParticipantArtifactStatsAsync(reviewId, participantId, userId, new Pagination());

            // Assert
            _cxn.Verify();
            Assert.AreEqual(artifactStatsResult.Total, 1);
            Assert.AreEqual(ViewStateType.Changed, artifactStatsResult.Items.First().ViewState);
        }

        [TestMethod]
        public async Task GetReviewParticipantArtifactStatsAsync_ApprovalStatus_Should_Be_Pending_When_Approval_Status_Is_Not_Specified()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var participantId = 3;
            var reviewArtifact = new ReviewedArtifact
            {
                Id = 4,
                IsApprovalRequired = true
            };

            var participantReviewArtifact = new ReviewedArtifact
            {
                ApprovalFlag = ApprovalType.NotSpecified,
                Approval = "Not Specified"
            };

            _artifactVersionsRepositoryMock.Setup(repo => repo.GetVersionControlArtifactInfoAsync(reviewId, null, userId)).ReturnsAsync(new VersionControlArtifactInfo
            {
                VersionCount = 1
            });

            SetupReviewArtifactsQuery(reviewId, userId, reviewArtifact);

            _applicationSettingsRepositoryMock.Setup(repo => repo.GetValue("ReviewArtifactHierarchyRebuildIntervalInMinutes", 20)).ReturnsAsync(20);

            SetupArtifactPermissionsCheck(new[] { reviewArtifact.Id, reviewId }, userId, new Dictionary<int, RolePermissions>
            {
                { reviewId, RolePermissions.Read },
                { reviewArtifact.Id, RolePermissions.Read }
            });

            SetupParticipantReviewArtifactsQuery(reviewId, participantId, reviewArtifact.Id, participantReviewArtifact);

            // Act
            var artifactStatsResult = await _reviewsRepository.GetReviewParticipantArtifactStatsAsync(reviewId, participantId, userId, new Pagination());

            // Assert
            _cxn.Verify();
            Assert.AreEqual(artifactStatsResult.Total, 1);
            Assert.AreEqual("Pending", artifactStatsResult.Items.First().ApprovalStatus);
        }

        [TestMethod]
        public async Task GetReviewParticipantArtifactStatsAsync_ApprovalStatus_Should_Be_Pending_When_Approval_Is_Required_And_ApprovalStatus_Is_Empty()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var participantId = 3;
            var reviewArtifact = new ReviewedArtifact
            {
                Id = 4,
                IsApprovalRequired = true
            };

            var participantReviewArtifact = new ReviewedArtifact
            {
                ApprovalFlag = ApprovalType.NotSpecified,
                Approval = ""
            };

            _artifactVersionsRepositoryMock.Setup(repo => repo.GetVersionControlArtifactInfoAsync(reviewId, null, userId)).ReturnsAsync(new VersionControlArtifactInfo
            {
                VersionCount = 1
            });

            SetupReviewArtifactsQuery(reviewId, userId, reviewArtifact);

            _applicationSettingsRepositoryMock.Setup(repo => repo.GetValue("ReviewArtifactHierarchyRebuildIntervalInMinutes", 20)).ReturnsAsync(20);

            SetupArtifactPermissionsCheck(new[] { reviewArtifact.Id, reviewId }, userId, new Dictionary<int, RolePermissions>
            {
                { reviewId, RolePermissions.Read },
                { reviewArtifact.Id, RolePermissions.Read }
            });

            SetupParticipantReviewArtifactsQuery(reviewId, participantId, reviewArtifact.Id, participantReviewArtifact);

            // Act
            var artifactStatsResult = await _reviewsRepository.GetReviewParticipantArtifactStatsAsync(reviewId, participantId, userId, new Pagination());

            // Assert
            _cxn.Verify();
            Assert.AreEqual(artifactStatsResult.Total, 1);
            Assert.AreEqual("Pending", artifactStatsResult.Items.First().ApprovalStatus);
        }

        [TestMethod]
        public async Task GetReviewParticipantArtifactStatsAsync_Meaning_Of_Signatures_Should_Be_Empty_When_MoS_Is_Disabled()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var participantId = 3;
            var reviewArtifact = new ReviewedArtifact
            {
                Id = 4,
                IsApprovalRequired = true
            };

            var participantReviewArtifact = new ReviewedArtifact
            {
                ApprovalFlag = ApprovalType.NotSpecified,
                Approval = ""
            };

            _artifactVersionsRepositoryMock.Setup(repo => repo.GetVersionControlArtifactInfoAsync(reviewId, null, userId)).ReturnsAsync(new VersionControlArtifactInfo
            {
                VersionCount = 1
            });

            SetupIsMeaningOfSignatureEnabledQuery(true, false);

            SetupGetParticipantsMeaningOfSignatureValuesQuery(new[] { 4 }, participantId, reviewId, new[]
            {
                new ReviewMeaningOfSignatureValue
                {
                    Id = 4,
                    MeaningOfSignatureValue = "foo",
                    RoleName = "bar"
                }
            });

            SetupReviewArtifactsQuery(reviewId, userId, reviewArtifact);

            _applicationSettingsRepositoryMock.Setup(repo => repo.GetValue("ReviewArtifactHierarchyRebuildIntervalInMinutes", 20)).ReturnsAsync(20);

            SetupArtifactPermissionsCheck(new[] { reviewArtifact.Id, reviewId }, userId, new Dictionary<int, RolePermissions>
            {
                { reviewId, RolePermissions.Read },
                { reviewArtifact.Id, RolePermissions.Read }
            });

            SetupParticipantReviewArtifactsQuery(reviewId, participantId, reviewArtifact.Id, participantReviewArtifact);

            // Act
            var artifactStatsResult = await _reviewsRepository.GetReviewParticipantArtifactStatsAsync(reviewId, participantId, userId, new Pagination());

            // Assert
            Assert.AreEqual(artifactStatsResult.Total, 1);
            Assert.IsTrue(!artifactStatsResult.Items.First().MeaningsOfSignature.Any());
        }

        [TestMethod]
        public async Task GetReviewParticipantArtifactStatsAsync_Meaning_Of_Signatures_Should_Be_Empty_When_No_MoS_Is_Returned()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var participantId = 3;
            var reviewArtifact = new ReviewedArtifact
            {
                Id = 4,
                IsApprovalRequired = true
            };

            var participantReviewArtifact = new ReviewedArtifact
            {
                ApprovalFlag = ApprovalType.NotSpecified,
                Approval = ""
            };

            _artifactVersionsRepositoryMock.Setup(repo => repo.GetVersionControlArtifactInfoAsync(reviewId, null, userId)).ReturnsAsync(new VersionControlArtifactInfo
            {
                VersionCount = 1
            });

            SetupIsMeaningOfSignatureEnabledQuery(true, true);

            SetupGetParticipantsMeaningOfSignatureValuesQuery(new[] { 4 }, participantId, reviewId, new ReviewMeaningOfSignatureValue[0]);

            SetupReviewArtifactsQuery(reviewId, userId, reviewArtifact);

            _applicationSettingsRepositoryMock.Setup(repo => repo.GetValue("ReviewArtifactHierarchyRebuildIntervalInMinutes", 20)).ReturnsAsync(20);

            SetupArtifactPermissionsCheck(new[] { reviewArtifact.Id, reviewId }, userId, new Dictionary<int, RolePermissions>
            {
                { reviewId, RolePermissions.Read },
                { reviewArtifact.Id, RolePermissions.Read }
            });

            SetupParticipantReviewArtifactsQuery(reviewId, participantId, reviewArtifact.Id, participantReviewArtifact);

            // Act
            var artifactStatsResult = await _reviewsRepository.GetReviewParticipantArtifactStatsAsync(reviewId, participantId, userId, new Pagination());

            // Assert
            _cxn.Verify();
            Assert.AreEqual(artifactStatsResult.Total, 1);
            Assert.IsTrue(!artifactStatsResult.Items.First().MeaningsOfSignature.Any());
        }

        [TestMethod]
        public async Task GetReviewParticipantArtifactStatsAsync_Meaning_Of_Signatures_Should_Not_Be_Empty_When_MoS_Are_Returned()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var participantId = 3;
            var reviewArtifact = new ReviewedArtifact
            {
                Id = 4,
                IsApprovalRequired = true
            };

            var participantReviewArtifact = new ReviewedArtifact
            {
                ApprovalFlag = ApprovalType.NotSpecified,
                Approval = ""
            };

            _artifactVersionsRepositoryMock.Setup(repo => repo.GetVersionControlArtifactInfoAsync(reviewId, null, userId)).ReturnsAsync(new VersionControlArtifactInfo
            {
                VersionCount = 1
            });

            SetupIsMeaningOfSignatureEnabledQuery(true, true);

            SetupGetParticipantsMeaningOfSignatureValuesQuery(new[] { 4 }, participantId, reviewId, new[]
            {
                new ReviewMeaningOfSignatureValue
                {
                    Id = 4,
                    MeaningOfSignatureValue = "foo",
                    RoleName = "bar"
                }
            });

            SetupReviewArtifactsQuery(reviewId, userId, reviewArtifact);

            _applicationSettingsRepositoryMock.Setup(repo => repo.GetValue("ReviewArtifactHierarchyRebuildIntervalInMinutes", 20)).ReturnsAsync(20);

            SetupArtifactPermissionsCheck(new[] { reviewArtifact.Id, reviewId }, userId, new Dictionary<int, RolePermissions>
            {
                { reviewId, RolePermissions.Read },
                { reviewArtifact.Id, RolePermissions.Read }
            });

            SetupParticipantReviewArtifactsQuery(reviewId, participantId, reviewArtifact.Id, participantReviewArtifact);

            // Act
            var artifactStatsResult = await _reviewsRepository.GetReviewParticipantArtifactStatsAsync(reviewId, participantId, userId, new Pagination());

            // Assert
            _cxn.Verify();
            Assert.AreEqual(artifactStatsResult.Total, 1);
            Assert.IsTrue(artifactStatsResult.Items.First().MeaningsOfSignature.First() == "foo (bar)");
        }

        private void SetupIsMeaningOfSignatureEnabledQuery(bool addDrafts, bool returnValue)
        {
            var parameters = new Dictionary<string, object>
            {
                { "reviewId", ReviewId },
                { "userId", UserId },
                { "addDrafts", addDrafts }
            };

            _cxn.SetupExecuteScalarAsync("GetReviewMeaningOfSignatureEnabled", parameters, returnValue);
        }

        private void SetupGetParticipantsMeaningOfSignatureValuesQuery(IEnumerable<int> artifactIds, int userId, int reviewId, IEnumerable<ReviewMeaningOfSignatureValue> result)
        {
            var parameters = new Dictionary<string, object>
            {
                { "reviewId", reviewId },
                { "userId", userId },
                { "itemIds", SqlConnectionWrapper.ToDataTable(artifactIds) }
            };

            _cxn.SetupQueryAsync("GetParticipantsMeaningOfSignatureValues", parameters, result);
        }

        private void SetupReviewArtifactsQuery(int reviewId, int userId, ReviewedArtifact reviewArtifact, bool isFormal = false)
        {
            var artifactParam = new Dictionary<string, object>
            {
                { "reviewId", reviewId },
                { "userId", userId },
                { "addDrafts", true },
                { "revisionId", int.MaxValue },
                { "offset", 0 },
                { "limit", 50 },
                { "refreshInterval", 20 }
            };

            var artifactOutputParam = new Dictionary<string, object>
            {
                { "numResult", 1 },
                { "isFormal", isFormal }
            };

            var reviewArtifacts = new List<ReviewedArtifact>
            {
                reviewArtifact
            };

            _cxn.SetupQueryAsync("GetReviewArtifacts", artifactParam, reviewArtifacts, artifactOutputParam);
        }

        private void SetupParticipantReviewArtifactsQuery(int reviewId, int userId, int artifactId, ReviewedArtifact participantArtifact)
        {
            var participantParam = new Dictionary<string, object>
            {
                { "reviewId", reviewId },
                { "revisionId", int.MaxValue },
                { "userId", userId },
                { "itemIds", SqlConnectionWrapper.ToDataTable(new[] { artifactId }) }
            };

            participantArtifact.Id = artifactId;

            var participantArtifacts = new List<ReviewedArtifact>
            {
                participantArtifact
            };

            _cxn.SetupQueryAsync("GetReviewArtifactsByParticipant", participantParam, participantArtifacts);
        }

        #endregion

        #region RemoveArtifactFromReview

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task RemoveArtifactsFromReviewAsync_ShouldThrow_BadRequestException()
        {
            // Arrange
            var prms = new ReviewItemsRemovalParams
            {
                ItemIds = new List<int>(),
                SelectionType = SelectionType.Selected
            };

            // Act
            await _reviewsRepository.RemoveArtifactsFromReviewAsync(1, prms, 2);
        }

        [TestMethod]
        public async Task RemoveArtifactsFromReviewAsync_ShouldThrowUserCannotModifyReviewException()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var isExceptionThrown = false;
            _hasEditPermissions = false;

            _artifactDetails.LockedByUserId = userId;

            var reviewContentsXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewContents xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Artifacts><CA><Id>3</Id></CA><CA><Id>4</Id></CA></Artifacts></RDReviewContents>";
            SetupGetReviewDataQuery(reviewId, userId, reviewContentsXml, null);

            var prms = new ReviewItemsRemovalParams
            {
                ItemIds = new List<int> { 1, 2, 3 },
                SelectionType = SelectionType.Selected
            };

            // Act
            try
            {
                await _reviewsRepository.RemoveArtifactsFromReviewAsync(1, prms, 2);
            }
            catch (AuthorizationException ex)
            {
                isExceptionThrown = true;

                // Assert
                Assert.AreEqual(ErrorCodes.UnauthorizedAccess, ex.ErrorCode);

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
        [ExpectedException(typeof(ConflictException))]
        public async Task RemoveArtifactsFromReviewAsync_ShouldThrow_ConflictException_WhenReviewClosed()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;

            _artifactDetails.LockedByUserId = userId;
            var reviewContentsXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewContents xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Artifacts><CA><Id>3</Id></CA><CA><Id>4</Id></CA></Artifacts></RDReviewContents>";
            var reviewPackageRawDataXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Status>Closed</Status></ReviewPackageRawData>";
            SetupGetReviewDataQuery(reviewId, userId, reviewContentsXml, reviewPackageRawDataXml);

            var prms = new ReviewItemsRemovalParams
            {
                ItemIds = new List<int> { 1, 2, 3 },
                SelectionType = SelectionType.Selected
            };

            // Act
            await _reviewsRepository.RemoveArtifactsFromReviewAsync(reviewId, prms, userId);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task RemoveArtifactsFromReviewAsync_ShouldThrow_ResourceNotFoundException_WhenReviewDoesNotExist()
        {
            // Arrange
            _artifactRepositoryMock
                .Setup(m => m.GetArtifactBasicDetails(ReviewId, UserId))
                .ReturnsAsync(() => null);

            var prms = new ReviewItemsRemovalParams
            {
                ItemIds = new List<int> { 1, 2, 3 },
                SelectionType = SelectionType.Selected
            };

            // Act
            await _reviewsRepository.RemoveArtifactsFromReviewAsync(ReviewId, prms, UserId);
        }

        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task RemoveArtifactsFromReviewAsync_ShouldThrow_ConflictException_WhenReviewNotLocked()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;

            var reviewContentsXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewContents xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Artifacts><CA><Id>3</Id></CA><CA><Id>4</Id></CA></Artifacts></RDReviewContents>";
            SetupGetReviewDataQuery(reviewId, userId, reviewContentsXml, null);
            var prms = new ReviewItemsRemovalParams
            {
                ItemIds = new List<int> { 1, 2, 3 },
                SelectionType = SelectionType.Selected
            };

            // Act
            await _reviewsRepository.RemoveArtifactsFromReviewAsync(reviewId, prms, userId);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task RemoveArtifactsFromReviewAsync_ShouldThrow_ConflictException_WhenReviewBaseLined()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var baselineId = 1;
            _artifactDetails.LockedByUserId = userId;

            var reviewContentsXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewContents xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Artifacts><CA><Id>3</Id></CA><CA><Id>4</Id></CA></Artifacts></RDReviewContents>";
            SetupGetReviewDataQuery(reviewId, userId, reviewContentsXml, null, baselineId);

            var prms = new ReviewItemsRemovalParams
            {
                ItemIds = new List<int> { 1, 2, 3 },
                SelectionType = SelectionType.Selected
            };

            // Act
            await _reviewsRepository.RemoveArtifactsFromReviewAsync(reviewId, prms, userId);
        }

        #endregion RemoveArtifactFromReview

        #region RemoveParticipantFromReview

        [TestMethod]
        public async Task RemoveParticipantsFromReviewAsync_Should_Not_Throw_Exception_When_Parameters_valid()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            _artifactDetails.LockedByUserId = userId;
            var xmlString = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><IsIgnoreFolder>true</IsIgnoreFolder><Reviwers><ReviewerRawData><Permission>Approver</Permission><UserId>1</UserId></ReviewerRawData><ReviewerRawData><Permission>Reviewer</Permission><UserId>2</UserId></ReviewerRawData><ReviewerRawData><Permission>Approver</Permission><UserId>3</UserId></ReviewerRawData><ReviewerRawData><Permission>Approver</Permission><UserId>4</UserId></ReviewerRawData></Reviwers><Status>Active</Status></ReviewPackageRawData>";
            var updatedXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><IsIgnoreFolder>true</IsIgnoreFolder><Reviwers><ReviewerRawData><Permission>Approver</Permission><UserId>4</UserId></ReviewerRawData></Reviwers><Status>Active</Status></ReviewPackageRawData>";

            SetupGetReviewDataQuery(reviewId, userId, null, xmlString);
            SetupUpdateReviewXmlQuery(reviewId, userId, 1, updatedXml);

            var prms = new ReviewItemsRemovalParams
            {
                ItemIds = new List<int> { 1, 2, 3 },
                SelectionType = SelectionType.Selected
            };

            // Act
            await _reviewsRepository.RemoveParticipantsFromReviewAsync(reviewId, prms, userId);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task RemoveParticipantsFromReviewAsync_ShouldThrow_BadRequestException()
        {
            // Arrange
            var prms = new ReviewItemsRemovalParams
            {
                ItemIds = new List<int>(),
                SelectionType = SelectionType.Selected
            };

            // Act
            await _reviewsRepository.RemoveParticipantsFromReviewAsync(1, prms, 2);
        }

        [TestMethod]
        public async Task RemoveParticipantsFromReviewAsync_ShouldThrowUserCannotModifyReviewException()
        {
            // Arrange
            _hasEditPermissions = false;
            _artifactDetails.LockedByUserId = UserId;
            var xmlString = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><IsIgnoreFolder>true</IsIgnoreFolder><Reviwers><ReviewerRawData><Permission>Approver</Permission><UserId>1</UserId></ReviewerRawData><ReviewerRawData><Permission>Reviewer</Permission><UserId>2</UserId></ReviewerRawData><ReviewerRawData><Permission>Approver</Permission><UserId>3</UserId></ReviewerRawData></Reviwers><Status>Active</Status></ReviewPackageRawData>";
            var updatedXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><IsIgnoreFolder>true</IsIgnoreFolder><Reviwers /><Status>Active</Status></ReviewPackageRawData>";

            SetupGetReviewDataQuery(ReviewId, UserId, null, xmlString);
            SetupUpdateReviewXmlQuery(ReviewId, UserId, 1, updatedXml);

            var prms = new ReviewItemsRemovalParams
            {
                ItemIds = new List<int> { 1, 2, 3 },
                SelectionType = SelectionType.Selected
            };

            // Act
            try
            {
                await _reviewsRepository.RemoveParticipantsFromReviewAsync(1, prms, 2);
            }
            catch (AuthorizationException ex)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.UnauthorizedAccess, ex.ErrorCode);

                return;
            }

            Assert.Fail("An AuthorizationException was not thrown");
        }

        [TestMethod]
        public async Task RemoveParticipantsFromReviewAsync_ShouldThrow_ConflictException_WhenReviewClosed()
        {
            // Arrange
            _artifactDetails.LockedByUserId = UserId;
            var xmlString = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><IsIgnoreFolder>true</IsIgnoreFolder><Reviwers><ReviewerRawData><Permission>Approver</Permission><UserId>1</UserId></ReviewerRawData><ReviewerRawData><Permission>Reviewer</Permission><UserId>2</UserId></ReviewerRawData><ReviewerRawData><Permission>Approver</Permission><UserId>3</UserId></ReviewerRawData></Reviwers><Status>Closed</Status></ReviewPackageRawData>";

            SetupGetReviewDataQuery(ReviewId, UserId, null, xmlString);

            var prms = new ReviewItemsRemovalParams
            {
                ItemIds = new List<int> { 1, 2, 3 },
                SelectionType = SelectionType.Selected
            };

            // Act
            try
            {
                await _reviewsRepository.RemoveParticipantsFromReviewAsync(ReviewId, prms, UserId);
            }
            catch (ConflictException ex)
            {
                Assert.AreEqual(ErrorCodes.ReviewClosed, ex.ErrorCode);

                return;
            }

            Assert.Fail("A Conflict Exception was not thrown.");
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task RemoveParticipantsFromReviewAsync_ShouldThrow_BadRequestException_WhenXMLNotExist()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            _artifactDetails.LockedByUserId = userId;

            SetupGetReviewDataQuery(reviewId, userId, null, null);

            var prms = new ReviewItemsRemovalParams
            {
                ItemIds = new List<int> { 1, 2, 3 },
                SelectionType = SelectionType.Selected
            };

            // Act
            await _reviewsRepository.RemoveParticipantsFromReviewAsync(reviewId, prms, userId);
        }

        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task RemoveParticipantsFromReviewAsync_ShouldThrow_ConflictException_WhenReviewNotLocked()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var xmlString = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><IsIgnoreFolder>true</IsIgnoreFolder><Reviwers><ReviewerRawData><Permission>Approver</Permission><UserId>1</UserId></ReviewerRawData><ReviewerRawData><Permission>Reviewer</Permission><UserId>2</UserId></ReviewerRawData><ReviewerRawData><Permission>Approver</Permission><UserId>3</UserId></ReviewerRawData></Reviwers><Status>Active</Status></ReviewPackageRawData>";

            SetupGetReviewDataQuery(reviewId, userId, null, xmlString);

            var prms = new ReviewItemsRemovalParams
            {
                ItemIds = new List<int> { 1, 2, 3 },
                SelectionType = SelectionType.Selected
            };
            _artifactRepositoryMock.Setup(artifactRepository => artifactRepository.IsArtifactLockedByUserAsync(reviewId, userId)).ReturnsAsync(false);

            // Act
            await _reviewsRepository.RemoveParticipantsFromReviewAsync(reviewId, prms, userId);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task RemoveParticipantsFromReviewAsync_ShouldThrow_BadRequestException_WhenReviewNotFound()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var xmlString = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><IsIgnoreFolder>true</IsIgnoreFolder><Reviwers><ReviewerRawData><Permission>Approver</Permission><UserId>1</UserId></ReviewerRawData><ReviewerRawData><Permission>Reviewer</Permission><UserId>2</UserId></ReviewerRawData><ReviewerRawData><Permission>Approver</Permission><UserId>3</UserId></ReviewerRawData><ReviewerRawData><Permission>Approver</Permission><UserId>4</UserId></ReviewerRawData></Reviwers><Status>Active</Status></ReviewPackageRawData>";
            var updatedXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><IsIgnoreFolder>true</IsIgnoreFolder><Reviwers><ReviewerRawData><Permission>Approver</Permission><UserId>4</UserId></ReviewerRawData></Reviwers><Status>Active</Status></ReviewPackageRawData>";
            _artifactDetails.LockedByUserId = userId;

            SetupGetReviewDataQuery(reviewId, userId, null, xmlString);
            SetupUpdateReviewXmlQuery(reviewId, userId, -1, updatedXml);

            var prms = new ReviewItemsRemovalParams
            {
                ItemIds = new List<int> { 1, 2, 3 },
                SelectionType = SelectionType.Selected
            };

            // Act
            await _reviewsRepository.RemoveParticipantsFromReviewAsync(reviewId, prms, userId);
        }

        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task RemoveParticipantsFromReviewAsync_ShouldThrow_ConflictException_WhenRemoveLastApprover()
        {
            // Arrange
            var reviewId = 1;
            var userId = 2;
            var xmlString = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><IsIgnoreFolder>true</IsIgnoreFolder><Reviwers><ReviewerRawData><Permission>Approver</Permission><UserId>1</UserId></ReviewerRawData><ReviewerRawData><Permission>Reviewer</Permission><UserId>2</UserId></ReviewerRawData><ReviewerRawData><Permission>Approver</Permission><UserId>3</UserId></ReviewerRawData></Reviwers><Status>Active</Status></ReviewPackageRawData>";
            _artifactDetails.LockedByUserId = userId;

            SetupGetReviewDataQuery(reviewId, userId, null, xmlString);

            var prms = new ReviewItemsRemovalParams
            {
                ItemIds = new List<int> { 1, 2, 3 },
                SelectionType = SelectionType.Selected
            };

            // Act
            await _reviewsRepository.RemoveParticipantsFromReviewAsync(reviewId, prms, userId);
        }

        #endregion

        private void SetupArtifactPermissionsCheck(IEnumerable<int> artifactIds, int userId, Dictionary<int, RolePermissions> result)
        {
            _artifactPermissionsRepositoryMock.Setup(repo => repo.GetArtifactPermissions(artifactIds, userId, false, int.MaxValue, true)).ReturnsAsync(result);
        }
    }
}
