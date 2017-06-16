using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.Models;
using ArtifactStore.Models.Review;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Repositories;
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

        #region GetReviewTableOfContent Tests
        [TestMethod]
        [ExpectedException(typeof (ResourceNotFoundException))]
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
         
            var pagination = new Pagination {
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
                {"@reviewId", reviewId},
                {"@revisionId", revisionId},
                {"@userId", userId},
                {"offset", pagination.Offset},
                {"@limit", pagination.Limit},
                {"@refreshInterval", refreshInterval}
            };

            var outPrm = new Dictionary<string, object>
            {
                {"@total", 0},
                {"@retResult", retResult}
            };

            var testResult = new ReviewTableOfContentItem[] { };
            cxn.SetupQueryAsync("GetReviewTableOfContent", prm, testResult, outPrm);

            var repository = new SqlReviewsRepository(cxn.Object, null, null, null, appSettingsRepoMock.Object);

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
        #endregion

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

            var outputParams = new Dictionary<string, object>() {
                { "@numResult", 2 }
            };
            _cxn.SetupQueryAsync("GetReviewArtifacts", param, reviewArtifacts, outputParams);

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
            var outputParams = new Dictionary<string, object>() {
                { "@numResult", 2 }
            };

            var reviewArtifacts = new List<ReviewedArtifact>();
            var artifact1 = new ReviewedArtifact { Id = 2 };
            reviewArtifacts.Add(artifact1);
            var artifact2 = new ReviewedArtifact { Id = 3 };
            reviewArtifacts.Add(artifact2);

          
            _cxn.SetupQueryAsync("GetReviewArtifacts", param, reviewArtifacts, outputParams);

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

        #region Add Review Participants tests

        [TestMethod]
        public async Task AddParticipants_UsersAndGroups_Success()
        {
            //Arrange
            int reviewId = 1;
            int userId = 2;
            var content = new AddParticipantsParameter() {
                UserIds = new []{1,2},
                GroupIds = new []{ 2, 4}
            };

            var param = new Dictionary<string, object> {
                { "reviewId", reviewId },
                { "userId", userId },
                { "xmlString", "" }
            };
            _cxn.SetupExecuteAsync("UpdateReviewParticipants", param, 0);

            //Act
            var result = await _reviewsRepository.AddParticipantsToReviewAsync(reviewId, userId, content);

            //Assert
            _cxn.Verify();

            Assert.AreEqual(0, result.ParticipantCount);
            Assert.AreEqual(0, result.AlreadyIncludedCount);


        }

        [TestMethod]
        public async Task AddParticipants_UsersExist_Success()
        {
            //Arrange
            int reviewId = 1;
            int userId = 2;
            var content = new AddParticipantsParameter()
            {
                UserIds = new[] { 1, 2 },
                GroupIds = new[] { 2, 4 }
            };

            var param = new Dictionary<string, object> {
                { "reviewId", reviewId },
                { "userId", userId },
                { "xmlString", "" }
            };
            _cxn.SetupExecuteAsync("UpdateReviewParticipants", param, 0);

            //Act
            var result = await _reviewsRepository.AddParticipantsToReviewAsync(reviewId, userId, content);

            //Assert
            _cxn.Verify();

            Assert.AreEqual(0, result.ParticipantCount);
            Assert.AreEqual(0, result.AlreadyIncludedCount);

        }

        [TestMethod]
        [Ignore]
        public async Task AddParticipants_UsersNotFound_Failed()
        {
            //Arrange
            int reviewId = 1;
            int userId = 2;
            var content = new AddParticipantsParameter()
            {
                UserIds = new[] { 1, 2 },
                GroupIds = new[] { 2, 4 }
            };
            var isExceptionThrown = false;

            var param = new Dictionary<string, object> {
                { "reviewId", reviewId },
                { "userId", userId },
                { "xmlString", "" }
            };
            _cxn.SetupExecuteAsync("UpdateReviewParticipants", param, 0);

            //Act
            try
            {
                var result = await _reviewsRepository.AddParticipantsToReviewAsync(reviewId, userId, content);

            }
            catch (ResourceNotFoundException ex)
            {
                isExceptionThrown = true;
                //Assert
                Assert.AreEqual(ErrorCodes.ResourceNotFound, ex.ErrorCode);
                Assert.AreEqual("", ex.Message);
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
        public async Task AddParticipants_GroupsNotFound_Failed()
        {
            //Arrange
            int reviewId = 1;
            int userId = 2;
            var content = new AddParticipantsParameter()
            {
                UserIds = new[] { 1, 2 },
                GroupIds = new[] { 2, 4 }
            };
            var isExceptionThrown = false;

            var param = new Dictionary<string, object> {
                { "reviewId", reviewId },
                { "userId", userId },
                { "xmlString", "" }
            };
            _cxn.SetupExecuteAsync("UpdateReviewParticipants", param, 0);

            //Act
            try
            {
                var result = await _reviewsRepository.AddParticipantsToReviewAsync(reviewId, userId, content);

            }
            catch (ResourceNotFoundException ex)
            {
                isExceptionThrown = true;
                //Assert
                Assert.AreEqual(ErrorCodes.ResourceNotFound, ex.ErrorCode);
                Assert.AreEqual("", ex.Message);
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


    }
}
