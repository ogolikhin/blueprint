﻿using System;
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
        private Mock<IUsersRepository> _usersRepositoryMock;
        private Mock<ISqlArtifactRepository> _artifactRepositoryMock;

        [TestInitialize]
        public void Initialize()
        {
            _cxn = new SqlConnectionWrapperMock();
            _artifactVersionsRepositoryMock = new Mock<IArtifactVersionsRepository>(MockBehavior.Strict);
            _itemInfoRepositoryMock = new Mock<ISqlItemInfoRepository>(MockBehavior.Strict);
            _artifactPermissionsRepositoryMock = new Mock<IArtifactPermissionsRepository>(MockBehavior.Strict);
            _applicationSettingsRepositoryMock = new Mock<IApplicationSettingsRepository>(MockBehavior.Strict);
            _usersRepositoryMock = new Mock<IUsersRepository>();
            _artifactRepositoryMock = new Mock<ISqlArtifactRepository>();

            _artifactRepositoryMock.SetReturnsDefault(Task.FromResult(true));

            _reviewsRepository = new SqlReviewsRepository(_cxn.Object, 
                    _artifactVersionsRepositoryMock.Object, 
                    _itemInfoRepositoryMock.Object,
                    _artifactPermissionsRepositoryMock.Object,
                    _applicationSettingsRepositoryMock.Object,
                    _usersRepositoryMock.Object,
                    _artifactRepositoryMock.Object);
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

        #region GetReviewTableOfContentAsync
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

            var repository = new SqlReviewsRepository(cxn.Object, null, null, null, appSettingsRepoMock.Object, null, null);

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

        #region AddParticipantsToReviewAsync

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task AddParticipantsToReviewAsync_Should_Throw_If_No_Users_Or_Groups()
        {
            //Arrange
            int reviewId = 1;
            int userId = 2;

            var addParticipantsParameter = new AddParticipantsParameter()
            {
                GroupIds = new int[0],
                UserIds = new int[0]
            };

            //Act
            await _reviewsRepository.AddParticipantsToReviewAsync(reviewId, userId, addParticipantsParameter);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task AddParticipantsToReviewAsync_Should_Throw_If_Review_Doesnt_Exist()
        {
            //Arrange
            int reviewId = 1;
            int userId = 2;

            var addParticipantsParameter = new AddParticipantsParameter()
            {
                GroupIds = new int[0],
                UserIds = new[] { userId }
            };

            var queryParameters = new Dictionary<string, object>()
            {
                { "@reviewId", reviewId },
                { "@userId", userId }
            };

            var queryResult = new List<string>();

            _cxn.SetupQueryAsync("GetReviewParticipantsPropertyString", queryParameters, queryResult);

            //Act
            await _reviewsRepository.AddParticipantsToReviewAsync(reviewId, userId, addParticipantsParameter);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task AddParticipantsToReviewAsync_Should_Throw_If_Review_Is_Not_Locked_By_User()
        {
            //Arrange
            int reviewId = 1;
            int userId = 2;

            var addParticipantsParameter = new AddParticipantsParameter()
            {
                GroupIds = new int[0],
                UserIds = new[] { userId }
            };

            var queryParameters = new Dictionary<string, object>()
            {
                { "@reviewId", reviewId },
                { "@userId", userId }
            };

            var queryResult = new List<string>()
            {
                null
            };

            _cxn.SetupQueryAsync("GetReviewParticipantsPropertyString", queryParameters, queryResult);

            _artifactRepositoryMock.Setup(artifactRepository => artifactRepository.IsArtifactLockedByUserAsync(reviewId, userId)).ReturnsAsync(false);

            //Act
            await _reviewsRepository.AddParticipantsToReviewAsync(reviewId, userId, addParticipantsParameter);
        }

        [TestMethod]
        public async Task AddParticipantsToReviewAsync_Should_Succeed_When_Returned_Xml_Is_Null()
        {
            //Arrange
            int reviewId = 1;
            int userId = 2;

            var addParticipantsParameter = new AddParticipantsParameter()
            {
                GroupIds = new int[0],
                UserIds = new[] { userId }
            };

            SetupGetReviewXmlQuery(reviewId, userId, null);

            SetupUpdateParticipantsXmlQuery(reviewId, userId, 1,
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Reviwers><ReviewerRawData><Permission>Reviewer</Permission><UserId>2</UserId></ReviewerRawData></Reviwers></ReviewPackageRawData>"
            );

            //Act
            var addParticipantResult = await _reviewsRepository.AddParticipantsToReviewAsync(reviewId, userId, addParticipantsParameter);

            //Assert
            _cxn.Verify();

            Assert.AreEqual(1, addParticipantResult.ParticipantCount);
            Assert.AreEqual(0, addParticipantResult.AlreadyIncludedCount);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task AddParticipantsToReviewAsync_Should_Throw_If_Review_Is_Closed()
        {
            //Arrange
            int reviewId = 1;
            int userId = 2;

            var addParticipantsParameter = new AddParticipantsParameter()
            {
                GroupIds = new int[0],
                UserIds = new[] { userId }
            };

            SetupGetReviewXmlQuery(reviewId, userId,
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Status>Closed</Status></ReviewPackageRawData>"
            );

            //Act
            await _reviewsRepository.AddParticipantsToReviewAsync(reviewId, userId, addParticipantsParameter);
        }

        [TestMethod]
        public async Task AddParticipantsToReviewAsync_Should_Return_Non_Existant_Users_If_Users_Are_Deleted_Or_NonExistant()
        {
            //Arrange
            int reviewId = 1;
            int userId = 2;

            var addParticipantsParameter = new AddParticipantsParameter()
            {
                GroupIds = new int[0],
                UserIds = new[] { 2, 3, 4 }
            };

            SetupGetReviewXmlQuery(reviewId, userId, null);

            _usersRepositoryMock.Setup(repo => repo.FindNonExistentUsersAsync(new[] { 2, 3, 4 })).ReturnsAsync(new[] { 2, 3, 4 });

            //Act
            var addParticipantsResult = await _reviewsRepository.AddParticipantsToReviewAsync(reviewId, userId, addParticipantsParameter);

            //Assert
            Assert.AreEqual(addParticipantsResult.NonExistentUsers, 3);
            Assert.AreEqual(addParticipantsResult.ParticipantCount, 0);
            Assert.AreEqual(addParticipantsResult.AlreadyIncludedCount, 0);
        }

        [TestMethod]
        public async Task AddParticipantsToReviewAsync_Should_Add_Users()
        {
            //Arrange
            int reviewId = 1;
            int userId = 2;

            var addParticipantsParameter = new AddParticipantsParameter()
            {
                GroupIds = new int[0],
                UserIds = new[] { 2, 3 }
            };

            SetupGetReviewXmlQuery(reviewId, userId,
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"/>"
            );

            SetupUpdateParticipantsXmlQuery(reviewId, userId, 1,
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Reviwers><ReviewerRawData><Permission>Reviewer</Permission><UserId>2</UserId></ReviewerRawData><ReviewerRawData><Permission>Reviewer</Permission><UserId>3</UserId></ReviewerRawData></Reviwers></ReviewPackageRawData>"
            );

            //Act 
            var addParticipantResult = await _reviewsRepository.AddParticipantsToReviewAsync(reviewId, userId, addParticipantsParameter);

            //Assert
            Assert.AreEqual(2, addParticipantResult.ParticipantCount);
            Assert.AreEqual(0, addParticipantResult.AlreadyIncludedCount);
        }

        [TestMethod]
        public async Task AddParticipantsToReviewAsync_Should_Add_Users_From_Groups()
        {
            //Arrange
            int reviewId = 1;
            int userId = 2;

            var addParticipantsParameter = new AddParticipantsParameter()
            {
                GroupIds = new[] { 6, 7 },
                UserIds = new int[0]
            };

            SetupGetReviewXmlQuery(reviewId, userId,
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"/>"
            );

            SetupUpdateParticipantsXmlQuery(reviewId, userId, 1,
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Reviwers><ReviewerRawData><Permission>Reviewer</Permission><UserId>3</UserId></ReviewerRawData><ReviewerRawData><Permission>Reviewer</Permission><UserId>4</UserId></ReviewerRawData><ReviewerRawData><Permission>Reviewer</Permission><UserId>5</UserId></ReviewerRawData></Reviwers></ReviewPackageRawData>"
            );

            _usersRepositoryMock.Setup(repo => repo.GetUserInfosFromGroupsAsync(new[] { 6, 7 })).ReturnsAsync(new List<UserInfo>()
            {
                new UserInfo() { UserId = 3 },
                new UserInfo() { UserId = 4 },
                new UserInfo() { UserId = 5 }
            });

            //Act 
            var addParticipantResult = await _reviewsRepository.AddParticipantsToReviewAsync(reviewId, userId, addParticipantsParameter);

            //Assert
            Assert.AreEqual(3, addParticipantResult.ParticipantCount);
            Assert.AreEqual(0, addParticipantResult.AlreadyIncludedCount);
        }

        [TestMethod]
        public async Task AddParticipantsToReviewAsync_Should_Not_Add_Duplicates_From_Users_And_Groups()
        {
            //Arrange
            int reviewId = 1;
            int userId = 2;

            var addParticipantsParameter = new AddParticipantsParameter()
            {
                GroupIds = new[] { 4 },
                UserIds = new[] { 1, 2 }
            };

            SetupGetReviewXmlQuery(reviewId, userId,
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"/>"
            );

            SetupUpdateParticipantsXmlQuery(reviewId, userId, 1,
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Reviwers><ReviewerRawData><Permission>Reviewer</Permission><UserId>1</UserId></ReviewerRawData><ReviewerRawData><Permission>Reviewer</Permission><UserId>2</UserId></ReviewerRawData></Reviwers></ReviewPackageRawData>"
            );

            _usersRepositoryMock.Setup(repo => repo.GetUserInfosFromGroupsAsync(new[] { 4 })).ReturnsAsync(new List<UserInfo>()
            {
                new UserInfo() { UserId = 1 },
                new UserInfo() { UserId = 2 }
            });

            //Act 
            var addParticipantResult = await _reviewsRepository.AddParticipantsToReviewAsync(reviewId, userId, addParticipantsParameter);

            //Assert
            Assert.AreEqual(2, addParticipantResult.ParticipantCount);
            Assert.AreEqual(0, addParticipantResult.AlreadyIncludedCount);
        }

        [TestMethod]
        public async Task AddParticipantsToReviewAsync_Should_Not_Add_Already_Existing_Users()
        {
            //Arrange
            int reviewId = 1;
            int userId = 2;

            var addParticipantsParameter = new AddParticipantsParameter()
            {
                GroupIds = new int[0],
                UserIds = new[] { 2, 3 }
            };

            SetupGetReviewXmlQuery(reviewId, userId,
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Reviwers><ReviewerRawData><Permission>Reviewer</Permission><UserId>2</UserId></ReviewerRawData><ReviewerRawData><Permission>Reviewer</Permission><UserId>3</UserId></ReviewerRawData></Reviwers></ReviewPackageRawData>"
            );

            //Act 
            var addParticipantResult = await _reviewsRepository.AddParticipantsToReviewAsync(reviewId, userId, addParticipantsParameter);

            //Assert
            Assert.AreEqual(0, addParticipantResult.ParticipantCount);
            Assert.AreEqual(2, addParticipantResult.AlreadyIncludedCount);
        }

        [TestMethod]
        public async Task AddParticipants_UsersAndGroups_Success()
        {
            //Arrange
            int reviewId = 1;
            int userId = 2;
            var content = new AddParticipantsParameter() {
                UserIds = new[] { 1, 2 },
                GroupIds = new[] { 2, 4 }
            };

            SetupGetReviewXmlQuery(reviewId, userId,
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"/>"
            );

            SetupUpdateParticipantsXmlQuery(reviewId, userId, 1,
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Reviwers><ReviewerRawData><Permission>Reviewer</Permission><UserId>1</UserId></ReviewerRawData><ReviewerRawData><Permission>Reviewer</Permission><UserId>2</UserId></ReviewerRawData><ReviewerRawData><Permission>Reviewer</Permission><UserId>4</UserId></ReviewerRawData><ReviewerRawData><Permission>Reviewer</Permission><UserId>5</UserId></ReviewerRawData></Reviwers></ReviewPackageRawData>"
            );

            _usersRepositoryMock.Setup(repo => repo.GetUserInfosFromGroupsAsync(new[] { 2, 4 })).ReturnsAsync(new List<UserInfo>()
            {
                new UserInfo() { UserId = 4 },
                new UserInfo() { UserId = 5 }
            });

            //Act
            var result = await _reviewsRepository.AddParticipantsToReviewAsync(reviewId, userId, content);

            //Assert
            _cxn.Verify();

            Assert.AreEqual(4, result.ParticipantCount);
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
                UserIds = new[] { 1, 2 }
            };

            SetupGetReviewXmlQuery(reviewId, userId,
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"/>"
            );

            SetupUpdateParticipantsXmlQuery(reviewId, userId, 1,
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Reviwers><ReviewerRawData><Permission>Reviewer</Permission><UserId>1</UserId></ReviewerRawData><ReviewerRawData><Permission>Reviewer</Permission><UserId>2</UserId></ReviewerRawData></Reviwers></ReviewPackageRawData>"
            );

            //Act
            var result = await _reviewsRepository.AddParticipantsToReviewAsync(reviewId, userId, content);

            //Assert
            _cxn.Verify();

            Assert.AreEqual(2, result.ParticipantCount);
            Assert.AreEqual(0, result.AlreadyIncludedCount);

        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task AddParticipantsToReviewAsync_Should_Fail_On_Update()
        {
            //Arrange
            int reviewId = 1;
            int userId = 2;
            var content = new AddParticipantsParameter()
            {
                UserIds = new[] { 1, 2 },
                GroupIds = new[] { 2, 4 }
            };

            SetupGetReviewXmlQuery(reviewId, userId,
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"/>"
            );

            SetupUpdateParticipantsXmlQuery(reviewId, userId, -1,
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><ReviewPackageRawData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Reviwers><ReviewerRawData><Permission>Reviewer</Permission><UserId>1</UserId></ReviewerRawData><ReviewerRawData><Permission>Reviewer</Permission><UserId>2</UserId></ReviewerRawData></Reviwers></ReviewPackageRawData>"
            );

           await _reviewsRepository.AddParticipantsToReviewAsync(reviewId, userId, content);
        }

        private void SetupGetReviewXmlQuery(int reviewId, int userId, string xmlString)
        {
            var queryParameters = new Dictionary<string, object>()
            {
                { "@reviewId", reviewId },
                { "@userId", userId }
            };

            var queryResult = new List<string>()
            {
                xmlString
            };

            _cxn.SetupQueryAsync("GetReviewParticipantsPropertyString", queryParameters, queryResult);
        }

        private void SetupUpdateParticipantsXmlQuery(int reviewId, int userId, int returnValue, string xmlString)
        {
            var updateParameters = new Dictionary<string, object> {
                { "reviewId", reviewId },
                { "userId", userId },
                { "xmlString", xmlString }
            };

            var updateResult = new Dictionary<string, object>
            {
                { "returnValue", returnValue }
            };

            _cxn.SetupExecuteAsync("UpdateReviewParticipants", updateParameters, -1, updateResult);
        }

        #endregion

        #region AddArtifactsToReviewAsync

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task AddArtifacts_AndCollections_Success()
        {
            //Arrange
            int reviewId = 1;
            int userId = 2;
            int projectId = 1;
           var ids = new[] { 1, 2 };
            var content = new AddArtifactsParameter()
            {
                ArtifactIds = ids,
                AddChildren = false
            };

            var queryParameters = new Dictionary<string, object>()
            {
                { "@reviewId", reviewId },
                { "@userId", userId }
            };

            var param = new Dictionary<string, object> {
                { "reviewId", reviewId },
                { "userId", userId },
                { "xmlArtifacts", "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewContents xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Artifacts><CA><Id>3</Id></CA><CA><Id>4</Id></CA><CA><Id>1</Id></CA><CA><Id>2</Id></CA></Artifacts></RDReviewContents>" }
            };
            _cxn.SetupExecuteAsync("UpdateReviewArtifacts", param, 0);

            var PropertyValueStringResult = new[]
            {
               new PropertyValueString
               {
               IsDraftRevisionExists = true,
                ArtifactXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewContents xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><Artifacts><CA><Id>3</Id></CA><CA><Id>4</Id></CA></Artifacts></RDReviewContents>",
                RevewSubartifactId = 3,
                ProjectId = projectId,
                IsReviewLocked = true
                }
            };

            _cxn.SetupQueryAsync("GetReviewPropertyString", queryParameters,  PropertyValueStringResult);

            var effectiveArtifactIdsQueryParameters = new Dictionary<string, object>()
            {
               {"@artifactIds",  SqlConnectionWrapper.ToDataTable(ids)},
                { "@userId", userId },
            {"@projectId", projectId}
        };
          

            IEnumerable<int> ArtifactIds = new List<int> { 1, 2 };
            IEnumerable< int > Unpublished = new List<int> {0 };
            IEnumerable<int> Nonexistent = new List<int> { 0 };

            Dictionary<string, object> outParameters = new Dictionary<string, object>()
            {
               {"ArtifactIds",  ids},
                { "Unpublished", 0},
            {"Nonexistent", 0},
            {"ProjectMoved", 0}
        };

            var mockResult = new Tuple<IEnumerable<int>, IEnumerable<int>,IEnumerable <int>> (ArtifactIds, Unpublished, Nonexistent);
            
            _cxn.SetupQueryMultipleAsync("GetEffectiveArtifactIds", effectiveArtifactIdsQueryParameters, mockResult, outParameters);

            //Act
            var result = await _reviewsRepository.AddArtifactsToReviewAsync(reviewId, userId, content);

            //Assert
            _cxn.Verify();

            Assert.AreEqual(2, result.ArtifactCount);
            Assert.AreEqual(0, result.AlreadyIncludedArtifactCount);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task AddArtifactsToReviewAsync_ShouldThrowBadRequestException()
        {
            //Arrange
            int reviewId = 1;
            int userId = 2;
            bool isExceptionThrown = false;
            var content = new AddArtifactsParameter()
            {
                ArtifactIds = null,
                AddChildren = false
            };

            //Act

            try
            {
                var review = await _reviewsRepository.AddArtifactsToReviewAsync(reviewId, userId, content);
            }
            catch (BadRequestException ex)
            {
                isExceptionThrown = true;
                //Assert
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
            //Arrange
            int reviewId = 1;
            int userId = 2;
            int projectId = 0;
            bool isExceptionThrown = false;
            var content = new AddArtifactsParameter()
            {
                ArtifactIds = new[] { 1, 2 },
                AddChildren = false
            };

            var queryParameters = new Dictionary<string, object>()
            {
                { "@reviewId", reviewId },
                { "@userId", userId }
            };

            var PropertyValueStringResult = new[]
            {
               new PropertyValueString
               {
               IsDraftRevisionExists = true,
                ArtifactXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewContents xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"/>",
                RevewSubartifactId = 3,
                ProjectId = projectId,
                IsReviewLocked = true
                }
            };

            _cxn.SetupQueryAsync("GetReviewPropertyString", queryParameters, PropertyValueStringResult);

            //Act

            try
            {
                var review = await _reviewsRepository.AddArtifactsToReviewAsync(reviewId, userId, content);
            }
            catch (ResourceNotFoundException ex)
            {
                isExceptionThrown = true;
                //Assert
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
            //Arrange
            int reviewId = 1;
            int userId = 2;
            int projectId = 1;
            bool isExceptionThrown = false;
            var content = new AddArtifactsParameter()
            {
                ArtifactIds = new[] { 1, 2 },
                AddChildren = false
            };

            var queryParameters = new Dictionary<string, object>()
            {
                { "@reviewId", reviewId },
                { "@userId", userId }
            };

            var PropertyValueStringResult = new[]
            {
               new PropertyValueString
               {
               IsDraftRevisionExists = true,
                ArtifactXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewContents xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"/>",
                RevewSubartifactId = 3,
                ProjectId = projectId,
                IsReviewLocked = false
                }
            };

            _cxn.SetupQueryAsync("GetReviewPropertyString", queryParameters, PropertyValueStringResult);

            //Act

            try
            {
                var review = await _reviewsRepository.AddArtifactsToReviewAsync(reviewId, userId, content);
            }
            catch (BadRequestException ex)
            {
                isExceptionThrown = true;

                //Assert
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
        public async Task AssignRolesToReviewers_ShouldThrowResourceNotFoundException()
        {
            //Arrange
            //   PropertyValueString result = null;
            bool isExceptionThrown = false;
            var propertyValueStringResult = new List<PropertyValueString>();

            var propertyValue = new PropertyValueString()
            {
                IsDraftRevisionExists = true,
                ArtifactXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewContents xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"/>",
                RevewSubartifactId = 3,
                ProjectId = 1,
                IsReviewLocked = true,
                IsReviewReadOnly = true,
                BaselineId = 2,
                IsReviewDeleted = true,
                IsUserDisabled = false
            };
            propertyValueStringResult.Add(propertyValue);
            int reviewId = 1;
            int userId = 1;
            var queryParameters = new Dictionary<string, object>()
            {
                { "@reviewId", reviewId },
                { "@userId", userId },
                 { "@roleUserId", 1 }
            };
            _cxn.SetupQueryAsync("GetReviewApprovalRolesInfo", queryParameters, propertyValueStringResult);
            AssignReviewerRolesParameter content = new AssignReviewerRolesParameter()
            {
                UserId = 1,
                Role = ReviewParticipantRole.Approver
            };
            //Act

            try
            {
                await _reviewsRepository.AssignRolesToReviewers(reviewId, content, userId);
            }
            catch (ResourceNotFoundException ex)
            {
                //Assert
                isExceptionThrown = true;

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
        public async Task AssignRolesToReviewers_ShouldThrowBadRequestExceptionException()
        {
            //Arrange
            //   PropertyValueString result = null;
            bool isExceptionThrown = false;
            var propertyValueStringResult = new List<PropertyValueString>();

            var propertyValue = new PropertyValueString()
            {
                IsDraftRevisionExists = true,
                ArtifactXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewContents xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"/>",
                RevewSubartifactId = 3,
                ProjectId = 1,
                IsReviewLocked = true,
                IsReviewReadOnly = true,
                BaselineId = 2,
                IsReviewDeleted = false,
                IsUserDisabled = false
            };
            propertyValueStringResult.Add(propertyValue);
            int reviewId = 1;
            int userId = 1;
            var queryParameters = new Dictionary<string, object>()
            {
                { "@reviewId", reviewId },
                { "@userId", userId },
                 { "@roleUserId", 1 }
            };
            _cxn.SetupQueryAsync("GetReviewApprovalRolesInfo", queryParameters, propertyValueStringResult);
            AssignReviewerRolesParameter content = new AssignReviewerRolesParameter()
            {
                UserId = 1,
                Role = ReviewParticipantRole.Approver
            };
            //Act

            try
            {
                await _reviewsRepository.AssignRolesToReviewers(reviewId, content, userId);
            }
            catch (BadRequestException ex)
            {
                //Assert
                isExceptionThrown = true;

                Assert.AreEqual(ErrorCodes.ApprovalRequiredIsReadonlyForReview, ex.ErrorCode);
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
        [ExpectedException(typeof(BadRequestException))]
        public async Task AssignRolesToReviewers_IsNotLocked_ShouldThrowBadRequestExceptionException()
        {
            var propertyValueStringResult = new List<PropertyValueString>();

            var propertyValue = new PropertyValueString()
            {
                IsDraftRevisionExists = true,
                ArtifactXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewContents xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"/>",
                RevewSubartifactId = 3,
                ProjectId = 1,
                IsReviewLocked = false,
                IsReviewReadOnly = false,
                BaselineId = 2,
                IsReviewDeleted = false,
                IsUserDisabled = false
            };
            propertyValueStringResult.Add(propertyValue);
            int reviewId = 1;
            int userId = 1;
            var queryParameters = new Dictionary<string, object>()
            {
                { "@reviewId", reviewId },
                { "@userId", userId },
                 { "@roleUserId", 1 }
            };
            _cxn.SetupQueryAsync("GetReviewApprovalRolesInfo", queryParameters, propertyValueStringResult);
            AssignReviewerRolesParameter content = new AssignReviewerRolesParameter()
            {
                UserId = 1,
                Role = ReviewParticipantRole.Approver
            };
            //Act

            await _reviewsRepository.AssignRolesToReviewers(reviewId, content, userId);
        }

        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task AssignRolesToReviewers_IfUserDisabled_ShouldThrowConflictException()
        {
            var propertyValueStringResult = new List<PropertyValueString>();

            var propertyValue = new PropertyValueString()
            {
                IsDraftRevisionExists = true,
                ArtifactXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewContents xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"/>",
                RevewSubartifactId = 3,
                ProjectId = 1,
                IsReviewLocked = true,
                IsReviewReadOnly = false,
                BaselineId = 2,
                IsReviewDeleted = false,
                IsUserDisabled = true
            };
            propertyValueStringResult.Add(propertyValue);
            int reviewId = 1;
            int userId = 1;
            var queryParameters = new Dictionary<string, object>()
            {
                { "@reviewId", reviewId },
                { "@userId", userId },
                 { "@roleUserId", 1 }
            };
            _cxn.SetupQueryAsync("GetReviewApprovalRolesInfo", queryParameters, propertyValueStringResult);
            AssignReviewerRolesParameter content = new AssignReviewerRolesParameter()
            {
                UserId = 1,
                Role = ReviewParticipantRole.Approver
            };
            //Act

            await _reviewsRepository.AssignRolesToReviewers(reviewId, content, userId);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task AssignRolesToReviewers_IfXMLEmpty_ShouldThrowConflictException()
        {
            var propertyValueStringResult = new List<PropertyValueString>();

            var propertyValue = new PropertyValueString()
            {
                IsDraftRevisionExists = true,
                ArtifactXml = "",
                RevewSubartifactId = 3,
                ProjectId = 1,
                IsReviewLocked = true,
                IsReviewReadOnly = false,
                BaselineId = 2,
                IsReviewDeleted = false,
                IsUserDisabled = false
            };
            propertyValueStringResult.Add(propertyValue);
            int reviewId = 1;
            int userId = 1;
            var queryParameters = new Dictionary<string, object>()
            {
                { "@reviewId", reviewId },
                { "@userId", userId },
                 { "@roleUserId", 1 }
            };
            _cxn.SetupQueryAsync("GetReviewApprovalRolesInfo", queryParameters, propertyValueStringResult);
            AssignReviewerRolesParameter content = new AssignReviewerRolesParameter()
            {
                UserId = 1,
                Role = ReviewParticipantRole.Approver
            };
            //Act

            await _reviewsRepository.AssignRolesToReviewers(reviewId, content, userId);
        }

        [TestMethod]
        public async Task AddArtifactsToReviewAsync_Should_Throw_Review_Closed_ErrorCode_When_Review_Is_Closed()
        {
            //Arrange
            int reviewId = 1;
            int userId = 2;
            int projectId = 1;
            bool isExceptionThrown = false;
            var content = new AddArtifactsParameter()
            {
                ArtifactIds = new[] { 1, 2 },
                AddChildren = false
            };

            var queryParameters = new Dictionary<string, object>()
            {
                { "@reviewId", reviewId },
                { "@userId", userId }
            };

            var PropertyValueStringResult = new[]
            {
                new PropertyValueString
                {
                    IsDraftRevisionExists = true,
                    ArtifactXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewContents xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"/>",
                    RevewSubartifactId = 3,
                    ProjectId = projectId,
                    IsReviewLocked = true,
                    IsReviewReadOnly = true
                }
            };

            _cxn.SetupQueryAsync("GetReviewPropertyString", queryParameters, PropertyValueStringResult);

            //Act
            try
            {
                var review = await _reviewsRepository.AddArtifactsToReviewAsync(reviewId, userId, content);
            }
            catch (BadRequestException ex)
            {
                isExceptionThrown = true;

                //Assert
                Assert.AreEqual(ErrorCodes.ReviewClosed, ex.ErrorCode);

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
        [ExpectedException(typeof(BadRequestException))]
        public async Task AssignApprovalRequiredToArtifacts_Should_Throw_BadRequestException()
        {
            //Arrange
            var content = new AssignArtifactsApprovalParameter()
            {
                ArtifactIds = null,
                ApprovalRequired = true
            };

            //Act

            await _reviewsRepository.AssignApprovalRequiredToArtifacts(1, 1, content);
        }


        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task AssignApprovalRequiredToArtifacts_Should_Throw_ResourceNotFoundException()
        {

            var propertyValueStringResult = new List<PropertyValueString>();

            var propertyValue = new PropertyValueString()
            {
                IsDraftRevisionExists = true,
                ArtifactXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewContents xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"/>",
                RevewSubartifactId = 3,
                ProjectId = 1,
                IsReviewLocked = true,
                IsReviewReadOnly = false,
                BaselineId = 2,
                IsReviewDeleted = true,
                IsUserDisabled = false
            };
            propertyValueStringResult.Add(propertyValue);
            int reviewId = 1;
            int userId = 1;
            var queryParameters = new Dictionary<string, object>()
            {
                { "@reviewId", reviewId },
                { "@userId", userId }
            };
            var content = new AssignArtifactsApprovalParameter()
            {
                ArtifactIds = new List<int>(new int[] { 1, 2, 3 }),
                ApprovalRequired = true
            };
            _cxn.SetupQueryAsync("GetReviewArtifactApprovalRequestedInfo", queryParameters, propertyValueStringResult);
            //Act
            await _reviewsRepository.AssignApprovalRequiredToArtifacts(reviewId, 1, content);

        }
        
        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task AssignApprovalRequiredToArtifacts_Review_ReadOnly_Should_Throw_BadRequestException()
        {

            var propertyValueStringResult = new List<PropertyValueString>();

            var propertyValue = new PropertyValueString()
            {
                IsDraftRevisionExists = true,
                ArtifactXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewContents xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"/>",
                RevewSubartifactId = 3,
                ProjectId = 1,
                IsReviewLocked = true,
                IsReviewReadOnly = true,
                BaselineId = 2,
                IsReviewDeleted = false,
                IsUserDisabled = false
            };
            propertyValueStringResult.Add(propertyValue);
            int reviewId = 1;
            int userId = 1;
            var queryParameters = new Dictionary<string, object>()
            {
                { "@reviewId", reviewId },
                { "@userId", userId }
            };
            var content = new AssignArtifactsApprovalParameter()
            {
                ArtifactIds = new List<int>(new int[] { 1, 2, 3 }),
                ApprovalRequired = true
            };
            _cxn.SetupQueryAsync("GetReviewArtifactApprovalRequestedInfo", queryParameters, propertyValueStringResult);
            //Act
            await _reviewsRepository.AssignApprovalRequiredToArtifacts(reviewId, 1, content);

        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task AssignApprovalRequiredToArtifacts_Review_NotLocked_Should_Throw_BadRequestException()
        {

            var propertyValueStringResult = new List<PropertyValueString>();

            var propertyValue = new PropertyValueString()
            {
                IsDraftRevisionExists = true,
                ArtifactXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewContents xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"/>",
                RevewSubartifactId = 3,
                ProjectId = 1,
                IsReviewLocked = false,
                IsReviewReadOnly = false,
                BaselineId = 2,
                IsReviewDeleted = false,
                IsUserDisabled = false
            };
            propertyValueStringResult.Add(propertyValue);
            int reviewId = 1;
            int userId = 1;
            var queryParameters = new Dictionary<string, object>()
            {
                { "@reviewId", reviewId },
                { "@userId", userId }
            };
            var content = new AssignArtifactsApprovalParameter()
            {
                ArtifactIds = new List<int>(new int[] { 1, 2, 3 }),
                ApprovalRequired = true
            };
            _cxn.SetupQueryAsync("GetReviewArtifactApprovalRequestedInfo", queryParameters, propertyValueStringResult);
            //Act
            await _reviewsRepository.AssignApprovalRequiredToArtifacts(reviewId, 1, content);

        }
        #endregion

        #region UpdateReviewArtifactApprovalAsync

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task UpdateReviewArtifactApprovalAsync_Should_Throw_When_Artifacts_Collection_Is_Null()
        {
            //Arrange
            int reviewId = 1;
            int userId = 2;
            List<ReviewArtifactApprovalParameter> approvalParameter = null;

            //Act
            await _reviewsRepository.UpdateReviewArtifactApprovalAsync(reviewId, approvalParameter, userId);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task UpdateReviewArtifactApprovalAsync_Should_Throw_When_No_Artifacts_Provided()
        {
            //Arrange
            int reviewId = 1;
            int userId = 2;
            var approvalParameter = new List<ReviewArtifactApprovalParameter>();

            //Act
            await _reviewsRepository.UpdateReviewArtifactApprovalAsync(reviewId, approvalParameter, userId);
        }

        [TestMethod]
        public async Task UpdateReviewArtifactApprovalAsync_Should_Succeed_With_No_Existing_Xml()
        {
            //Arrange
            int reviewId = 1;
            int userId = 2;
            var approvalParameter = new List<ReviewArtifactApprovalParameter>()
            {
                new ReviewArtifactApprovalParameter() { Approval = "Approved", ApprovalFlag = ApprovalType.Approved, ArtifactId = 3, VersionId = 1 }
            };

            SetupArtifactApprovalCheck(reviewId, userId, new[] { 3 });

            _artifactPermissionsRepositoryMock.Setup(repo => repo.GetArtifactPermissions(It.IsAny<IEnumerable<int>>(), userId, false, int.MaxValue, true)).ReturnsAsync(new Dictionary<int, RolePermissions>()
            {
                { 1, RolePermissions.Read },
                { 3, RolePermissions.Read }
            });

            var getXmlParameters = new Dictionary<string, object>()
            {
                { "reviewId", 1 },
                { "userId", 2 }
            };

            _cxn.SetupQueryAsync("GetReviewArtifactApprovalForUserXml", getXmlParameters, new List<string>());

            var updateXmlParameters = new Dictionary<string, object>()
            {
                { "reviewId", 1 },
                { "userId", 2 },
                { "xmlString", "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewedArtifacts xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><ReviewedArtifacts><RA><A>Approved</A><AF>Approved</AF><Id>3</Id><V>1</V><VS>Viewed</VS></RA></ReviewedArtifacts></RDReviewedArtifacts>" }
            };

            _cxn.SetupExecuteAsync("UpdateReviewArtifactApprovalForUserXml", updateXmlParameters, 1);

            //Act
            await _reviewsRepository.UpdateReviewArtifactApprovalAsync(reviewId, approvalParameter, userId);

            //Assert
            _cxn.Verify();
        }

        [TestMethod]
        public async Task UpdateReviewArtifactApprovalAsync_Should_Add_New_Artifact_Approval()
        {
            //Arrange
            int reviewId = 1;
            int userId = 2;
            var approvalParameter = new List<ReviewArtifactApprovalParameter>()
            {
                new ReviewArtifactApprovalParameter() { Approval = "Custom Approval", ApprovalFlag = ApprovalType.Approved, ArtifactId = 3, VersionId = 1 }
            };

            SetupArtifactApprovalCheck(reviewId, userId, new[] { 3 });

            _artifactPermissionsRepositoryMock.Setup(repo => repo.GetArtifactPermissions(It.IsAny<IEnumerable<int>>(), userId, false, int.MaxValue, true)).ReturnsAsync(new Dictionary<int, RolePermissions>()
            {
                { 1, RolePermissions.Read },
                { 3, RolePermissions.Read }
            });

            var getXmlParameters = new Dictionary<string, object>()
            {
                { "reviewId", 1 },
                { "userId", 2 }
            };

            _cxn.SetupQueryAsync("GetReviewArtifactApprovalForUserXml", getXmlParameters, new List<string>()
            {
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewedArtifacts xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><ReviewedArtifacts><RA><A>Approved</A><AF>Approved</AF><Id>4</Id><V>1</V><VS>Viewed</VS></RA></ReviewedArtifacts></RDReviewedArtifacts>"
            });

            var updateXmlParameters = new Dictionary<string, object>()
            {
                { "reviewId", 1 },
                { "userId", 2 },
                { "xmlString", "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewedArtifacts xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><ReviewedArtifacts><RA><A>Approved</A><AF>Approved</AF><Id>4</Id><V>1</V><VS>Viewed</VS></RA><RA><A>Custom Approval</A><AF>Approved</AF><Id>3</Id><V>1</V><VS>Viewed</VS></RA></ReviewedArtifacts></RDReviewedArtifacts>" }
            };

            _cxn.SetupExecuteAsync("UpdateReviewArtifactApprovalForUserXml", updateXmlParameters, 1);

            //Act
            await _reviewsRepository.UpdateReviewArtifactApprovalAsync(reviewId, approvalParameter, userId);

            //Assert
            _cxn.Verify();
        }

        [TestMethod]
        public async Task UpdateReviewArtifactApprovalAsync_Should_Update_Existing_Approval()
        {
            //Arrange
            int reviewId = 1;
            int userId = 2;
            var approvalParameter = new List<ReviewArtifactApprovalParameter>()
            {
                new ReviewArtifactApprovalParameter() { Approval = "Disapproved", ApprovalFlag = ApprovalType.Disapproved, ArtifactId = 3, VersionId = 1 }
            };

            SetupArtifactApprovalCheck(reviewId, userId, new[] { 3 });

            _artifactPermissionsRepositoryMock.Setup(repo => repo.GetArtifactPermissions(It.IsAny<IEnumerable<int>>(), userId, false, int.MaxValue, true)).ReturnsAsync(new Dictionary<int, RolePermissions>()
            {
                { 1, RolePermissions.Read },
                { 3, RolePermissions.Read }
            });

            var getXmlParameters = new Dictionary<string, object>()
            {
                { "reviewId", 1 },
                { "userId", 2 }
            };

            _cxn.SetupQueryAsync("GetReviewArtifactApprovalForUserXml", getXmlParameters, new List<string>()
            {
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewedArtifacts xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><ReviewedArtifacts><RA><A>Approved</A><AF>Approved</AF><Id>3</Id><V>1</V><VS>Viewed</VS></RA></ReviewedArtifacts></RDReviewedArtifacts>"
            });

            var updateXmlParameters = new Dictionary<string, object>()
            {
                { "reviewId", 1 },
                { "userId", 2 },
                { "xmlString", "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewedArtifacts xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><ReviewedArtifacts><RA><A>Disapproved</A><AF>Disapproved</AF><Id>3</Id><V>1</V><VS>Viewed</VS></RA></ReviewedArtifacts></RDReviewedArtifacts>" }
            };

            _cxn.SetupExecuteAsync("UpdateReviewArtifactApprovalForUserXml", updateXmlParameters, 1);

            //Act
            await _reviewsRepository.UpdateReviewArtifactApprovalAsync(reviewId, approvalParameter, userId);

            //Assert
            _cxn.Verify();
        }

        [TestMethod]
        public async Task UpdateReviewArtifactApprovalAsync_Should_Set_Artifact_To_Viewed()
        {
            //Arrange
            int reviewId = 1;
            int userId = 2;
            var approvalParameter = new List<ReviewArtifactApprovalParameter>()
            {
                new ReviewArtifactApprovalParameter() { Approval = "Approved", ApprovalFlag = ApprovalType.Approved, ArtifactId = 3, VersionId = 1 }
            };

            SetupArtifactApprovalCheck(reviewId, userId, new[] { 3 });

            _artifactPermissionsRepositoryMock.Setup(repo => repo.GetArtifactPermissions(It.IsAny<IEnumerable<int>>(), userId, false, int.MaxValue, true)).ReturnsAsync(new Dictionary<int, RolePermissions>()
            {
                { 1, RolePermissions.Read },
                { 3, RolePermissions.Read }
            });

            var getXmlParameters = new Dictionary<string, object>()
            {
                { "reviewId", 1 },
                { "userId", 2 }
            };

            _cxn.SetupQueryAsync("GetReviewArtifactApprovalForUserXml", getXmlParameters, new List<string>()
            {
                "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewedArtifacts xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><ReviewedArtifacts><RA><A>Not Specified</A><Id>3</Id><V>1</V></RA></ReviewedArtifacts></RDReviewedArtifacts>"
            });

            var updateXmlParameters = new Dictionary<string, object>()
            {
                { "reviewId", 1 },
                { "userId", 2 },
                { "xmlString", "<?xml version=\"1.0\" encoding=\"utf-16\"?><RDReviewedArtifacts xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.blueprintsys.com/raptor/reviews\"><ReviewedArtifacts><RA><A>Approved</A><AF>Approved</AF><Id>3</Id><V>1</V><VS>Viewed</VS></RA></ReviewedArtifacts></RDReviewedArtifacts>" }
            };

            _cxn.SetupExecuteAsync("UpdateReviewArtifactApprovalForUserXml", updateXmlParameters, 1);

            //Act
            await _reviewsRepository.UpdateReviewArtifactApprovalAsync(reviewId, approvalParameter, userId);

            //Assert
            _cxn.Verify();
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task UpdateReviewArtifactApprovalAsync_Should_Throw_When_Review_Doesnt_Exist()
        {
            //Arrange
            int reviewId = 1;
            int userId = 2;
            var approvalParameter = new List<ReviewArtifactApprovalParameter>()
            {
                new ReviewArtifactApprovalParameter() { Approval = "Approved", ApprovalFlag = ApprovalType.Approved, ArtifactId = 3, VersionId = 1 }
            };

            SetupArtifactApprovalCheck(reviewId, userId, new[] { 3 }, check => check.ReviewExists = false);

            //Act
            await _reviewsRepository.UpdateReviewArtifactApprovalAsync(reviewId, approvalParameter, userId);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task UpdateReviewArtifactApprovalAsync_Should_Throw_When_Review_Is_Draft()
        {
            //Arrange
            int reviewId = 1;
            int userId = 2;
            var approvalParameter = new List<ReviewArtifactApprovalParameter>()
            {
                new ReviewArtifactApprovalParameter() { Approval = "Approved", ApprovalFlag = ApprovalType.Approved, ArtifactId = 3, VersionId = 1 }
            };

            SetupArtifactApprovalCheck(reviewId, userId, new[] { 3 }, check => check.ReviewIsDraft = true);

            //Act
            await _reviewsRepository.UpdateReviewArtifactApprovalAsync(reviewId, approvalParameter, userId);
        }

        [TestMethod]
        public async Task UpdateReviewArtifactApprovalAsync_Should_Throw_When_Review_Is_Closed()
        {
            //Arrange
            int reviewId = 1;
            int userId = 2;
            var approvalParameter = new List<ReviewArtifactApprovalParameter>()
            {
                new ReviewArtifactApprovalParameter() { Approval = "Approved", ApprovalFlag = ApprovalType.Approved, ArtifactId = 3, VersionId = 1 }
            };

            SetupArtifactApprovalCheck(reviewId, userId, new[] { 3 }, check => check.ReviewClosed = true);

            //Act
            try
            {
                await _reviewsRepository.UpdateReviewArtifactApprovalAsync(reviewId, approvalParameter, userId);
            }
            catch(BadRequestException ex)
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
            //Arrange
            int reviewId = 1;
            int userId = 2;
            var approvalParameter = new List<ReviewArtifactApprovalParameter>()
            {
                new ReviewArtifactApprovalParameter() { Approval = "Approved", ApprovalFlag = ApprovalType.Approved, ArtifactId = 3, VersionId = 1 }
            };

            SetupArtifactApprovalCheck(reviewId, userId, new[] { 3 }, check => check.ReviewDeleted = true);

            //Act
            await _reviewsRepository.UpdateReviewArtifactApprovalAsync(reviewId, approvalParameter, userId);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task UpdateReviewArtifactApprovalAsync_Should_Throw_When_User_Isnt_Approver()
        {
            //Arrange
            int reviewId = 1;
            int userId = 2;
            var approvalParameter = new List<ReviewArtifactApprovalParameter>()
            {
                new ReviewArtifactApprovalParameter() { Approval = "Approved", ApprovalFlag = ApprovalType.Approved, ArtifactId = 3, VersionId = 1 }
            };
            
            SetupArtifactApprovalCheck(reviewId, userId, new[] { 3 }, check => check.ReviewerRole = ReviewParticipantRole.Reviewer);

            //Act
            await _reviewsRepository.UpdateReviewArtifactApprovalAsync(reviewId, approvalParameter, userId);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task UpdateReviewArtifactApprovalAsync_Should_Throw_When_Artifact_Given_Is_Not_In_Review()
        {
            //Arrange
            int reviewId = 1;
            int userId = 2;
            var approvalParameter = new List<ReviewArtifactApprovalParameter>()
            {
                new ReviewArtifactApprovalParameter() { Approval = "Approved", ApprovalFlag = ApprovalType.Approved, ArtifactId = 3, VersionId = 1 }
            };
            
            SetupArtifactApprovalCheck(reviewId, userId, new[] { 3 }, check => check.AllArtifactsInReview = false);

            //Act
            await _reviewsRepository.UpdateReviewArtifactApprovalAsync(reviewId, approvalParameter, userId);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task UpdateReviewArtifactApprovalAsync_Should_Throw_When_Artifact_Given_Doesnt_Require_Approval()
        {
            //Arrange
            int reviewId = 1;
            int userId = 2;
            var approvalParameter = new List<ReviewArtifactApprovalParameter>()
            {
                new ReviewArtifactApprovalParameter() { Approval = "Approved", ApprovalFlag = ApprovalType.Approved, ArtifactId = 3, VersionId = 1 }
            };
            
            SetupArtifactApprovalCheck(reviewId, userId, new[] { 3 }, check => check.AllArtifactsRequireApproval = false);

            //Act
            await _reviewsRepository.UpdateReviewArtifactApprovalAsync(reviewId, approvalParameter, userId);
        }

        [TestMethod]
        public async Task UpdateReviewArtifactApprovalAsync_Should_Throw_When_User_Doesnt_Have_Access_To_Review()
        {
            //Arrange
            int reviewId = 1;
            int userId = 2;
            var approvalParameter = new List<ReviewArtifactApprovalParameter>()
            {
                new ReviewArtifactApprovalParameter() { Approval = "Approved", ApprovalFlag = ApprovalType.Approved, ArtifactId = 3, VersionId = 1 }
            };

            SetupArtifactApprovalCheck(reviewId, userId, new[] { 3 });

            _artifactPermissionsRepositoryMock.Setup(repo => repo.GetArtifactPermissions(It.IsAny<IEnumerable<int>>(), userId, false, int.MaxValue, true)).ReturnsAsync(new Dictionary<int, RolePermissions>()
            {
                { 3, RolePermissions.Read }
            });

            //Act
            try
            {
                await _reviewsRepository.UpdateReviewArtifactApprovalAsync(reviewId, approvalParameter, userId);
            }
            catch(AuthorizationException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.UnauthorizedAccess);
                return;
            }

            Assert.Fail();
        }

        [TestMethod]
        public async Task UpdateReviewArtifactApprovalAsync_Should_Throw_When_User_Doesnt_Have_Access_To_Given_Artifact()
        {
            //Arrange
            int reviewId = 1;
            int userId = 2;
            var approvalParameter = new List<ReviewArtifactApprovalParameter>()
            {
                new ReviewArtifactApprovalParameter() { Approval = "Approved", ApprovalFlag = ApprovalType.Approved, ArtifactId = 3, VersionId = 1 }
            };

            SetupArtifactApprovalCheck(reviewId, userId, new[] { 3 });

            _artifactPermissionsRepositoryMock.Setup(repo => repo.GetArtifactPermissions(It.IsAny<IEnumerable<int>>(), userId, false, int.MaxValue, true)).ReturnsAsync(new Dictionary<int, RolePermissions>()
            {
                { 1, RolePermissions.Read }
            });

            //Act
            try
            {
                await _reviewsRepository.UpdateReviewArtifactApprovalAsync(reviewId, approvalParameter, userId);
            }
            catch (ResourceNotFoundException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.ArtifactNotFound);
                return;
            }

            Assert.Fail();
        }

        private void SetupArtifactApprovalCheck(int reviewId, int userId, IEnumerable<int> artifactIds, Action<ReviewArtifactApprovalCheck> setCheckResult = null)
        {
            var getCheckParameters = new Dictionary<string, object>()
            {
                { "reviewId", reviewId },
                { "userId", userId },
                { "artifactIds", SqlConnectionWrapper.ToDataTable(artifactIds) }
            };

            var check = new ReviewArtifactApprovalCheck()
            {
                AllArtifactsInReview = true,
                AllArtifactsRequireApproval = true,
                ReviewClosed = false,
                ReviewDeleted = false,
                ReviewerRole = ReviewParticipantRole.Approver,
                ReviewExists = true,
                ReviewIsDraft = false
            };

            setCheckResult?.Invoke(check);

            _cxn.SetupQueryAsync("CheckReviewArtifactApproval", getCheckParameters, new[] { check });
        }

        #endregion

    }
}
