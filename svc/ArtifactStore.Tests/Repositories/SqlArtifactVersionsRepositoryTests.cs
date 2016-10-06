using ArtifactStore.Helpers;
using ArtifactStore.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtifactStore.Repositories
{
    [TestClass]
    public class SqlArtifactVersionsRepositoryTests
    {

        [TestMethod]
        [ExpectedException (typeof(ArgumentOutOfRangeException))]
        public async Task GetArtifactVersions_ArtifactIdOutOfRange_ArgumentOutOfBoundsException()
        {
            // Arrange
            int artifactId = -1;
            int limit = 1;
            int offset = 1;
            int? userId = 1;
            bool asc = false;
            int sessionUserId = 1;
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactVersionsRepository(cxn.Object);
            // Act
            await repository.GetArtifactVersions(artifactId, limit, offset, userId, asc, sessionUserId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetArtifactVersions_LimitOutOfRange_ArgumentOutOfBoundsException()
        {
            // Arrange
            int artifactId = 1;
            int limit = -1;
            int offset = 1;
            int? userId = 1;
            bool asc = false;
            int sessionUserId = 1;
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactVersionsRepository(cxn.Object);
            // Act
            await repository.GetArtifactVersions(artifactId, limit, offset, userId, asc, sessionUserId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetArtifactVersions_OffSetOutOfRange_ArgumentOutOfBoundsException()
        {
            // Arrange
            int artifactId = 1;
            int limit = 1;
            int offset = -1;
            int? userId = 1;
            bool asc = false;
            int sessionUserId = 1;
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactVersionsRepository(cxn.Object);
            // Act
            await repository.GetArtifactVersions(artifactId, limit, offset, userId, asc, sessionUserId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetArtifactVersions_UserIdOutOfRange_ArgumentOutOfBoundsException()
        {
            // Arrange
            int artifactId = 1;
            int limit = 1;
            int offset = 0;
            int? userId = -1;
            bool asc = false;
            int sessionUserId = 1;
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactVersionsRepository(cxn.Object);
            // Act
            await repository.GetArtifactVersions(artifactId, limit, offset, userId, asc, sessionUserId);
        }

        [TestMethod]
        public async Task GetArtifactVersions_ArtifactNoDraftsOrPublishedVersion_Success()
        {
            // Arrange
            int artifactId = 1;
            int limit = 1;
            int offset = 1;
            int? userId = 1;
            bool asc = false;
            int sessionUserId = 1;
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactVersionsRepository(cxn.Object);
            cxn.SetupQueryAsync("DoesArtifactHavePublishedOrDraftVersion", new Dictionary<string, object> { { "artifactId", artifactId } }, new List<bool> { true });
            // Act
            var actual = await repository.GetArtifactVersions(artifactId, limit, offset, userId, asc, sessionUserId);
            // Assert
            cxn.Verify();
            Assert.AreEqual(actual.ArtifactId, 1);
            Assert.AreEqual(actual.ArtifactHistoryVersions.ToList().Count(), 0);
        }

        [TestMethod]
        public async Task GetArtifactVersions_NoDraftsNotDeleted_Success()
        {
            // Arrange
            int artifactId = 1;
            int limit = 1;
            int offset = 1;
            int? userId = 1;
            bool asc = false;
            int sessionUserId = 1;
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactVersionsRepository(cxn.Object);
            var prm = new Dictionary<string, object>
                { { "artifactId", artifactId },
                  { "lim", limit },
                  { "offset", offset },
                  { "userId", userId.Value },
                  { "ascd", asc } };
            var testResult = new ArtifactHistoryVersion[] { new ArtifactHistoryVersion { VersionId = 1, UserId = 1, Timestamp = new DateTime() } };
            cxn.SetupQueryAsync("DoesArtifactHavePublishedOrDraftVersion", new Dictionary<string, object> { { "artifactId", artifactId } }, new List<bool> { true });
            cxn.SetupQueryAsync("IsArtifactDeleted", new Dictionary<string, object> { { "artifactId", artifactId } }, new List<bool> { false });
            cxn.SetupQueryAsync("GetArtifactVersions", prm, testResult);
            var artifactIdsTable = SqlConnectionWrapper.ToDataTable(new List<int> { artifactId }, "Int32Collection", "Int32Value");
            var prm2 = new Dictionary<string, object> { { "userId", sessionUserId }, { "artifactIds", artifactIdsTable } };
            cxn.SetupQueryAsync("GetArtifactsWithDraft", prm2, new List<int>());
            var userIdsTable = SqlConnectionWrapper.ToDataTable(new List<int> { sessionUserId }, "Int32Collection", "Int32Value");
            cxn.SetupQueryAsync("GetUserInfos", new Dictionary<string, object> { { "userIds", userIdsTable } }, new List<UserInfo> { new UserInfo { UserId = 1, DisplayName = "David", ImageId = 1 } });

            // Act
            var actual = await repository.GetArtifactVersions(artifactId, limit, offset, userId, asc, sessionUserId);
            // Assert
            cxn.Verify();
            Assert.AreEqual(actual.ArtifactId, 1);
            Assert.AreEqual(actual.ArtifactHistoryVersions.ToList().Count(), 1);
            Assert.AreEqual(actual.ArtifactHistoryVersions.ToList()[0].VersionId, 1);
        }

        [TestMethod]
        public async Task GetArtifactVersions_NoDraftsArtifactDeleted_Success()
        {
            // Arrange
            int artifactId = 1;
            int limit = 1;
            int offset = 0;
            int? userId = 1;
            bool asc = false;
            int sessionUserId = 1;
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactVersionsRepository(cxn.Object);
            var prm = new Dictionary<string, object>
                { { "artifactId", artifactId },
                  { "lim", limit },
                  { "offset", offset },
                  { "userId", userId.Value },
                  { "ascd", asc } };
            var testResult = new ArtifactHistoryVersion[] { new ArtifactHistoryVersion { VersionId = 1, UserId = 1, Timestamp = new DateTime() } };
            cxn.SetupQueryAsync("DoesArtifactHavePublishedOrDraftVersion", new Dictionary<string, object> { { "artifactId", artifactId } }, new List<bool> { true });
            cxn.SetupQueryAsync("IsArtifactDeleted", new Dictionary<string, object> { { "artifactId", artifactId } }, new List<bool> { true });
            cxn.SetupQueryAsync("GetDeletedVersionInfo", new Dictionary<string, object> { { "artifactId", artifactId } }, new List<ArtifactHistoryVersion> { new ArtifactHistoryVersion { VersionId = int.MaxValue, UserId = 1, Timestamp = null, ArtifactState = ArtifactState.Deleted } });
            cxn.SetupQueryAsync("GetArtifactVersions", prm, testResult);
            var artifactIdsTable = SqlConnectionWrapper.ToDataTable(new List<int> { artifactId }, "Int32Collection", "Int32Value");
            var userIdsTable = SqlConnectionWrapper.ToDataTable(new List<int> { sessionUserId }, "Int32Collection", "Int32Value");
            cxn.SetupQueryAsync("GetUserInfos", new Dictionary<string, object> { { "userIds", userIdsTable } }, new List<UserInfo> { new UserInfo { UserId = 1, DisplayName = "David", ImageId = 1 } });
            // Act
            var actual = await repository.GetArtifactVersions(artifactId, limit, offset, userId, asc, sessionUserId);
            // Assert
            cxn.Verify();
            Assert.AreEqual(actual.ArtifactId, 1);
            Assert.AreEqual(actual.ArtifactHistoryVersions.ToList().Count(), 2);
            Assert.AreEqual(actual.ArtifactHistoryVersions.ToList()[1].VersionId, 1);
        }

        [TestMethod]
        public async Task GetArtifactVersions_WithDrafts_Success()
        {
            // Arrange
            int artifactId = 1;
            int limit = 1;
            int offset = 0;
            int? userId = 1;
            bool asc = false;
            int sessionUserId = 1;
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactVersionsRepository(cxn.Object);
            var prm = new Dictionary<string, object>
                { { "artifactId", artifactId },
                  { "lim", limit },
                  { "offset", offset },
                  { "userId", userId.Value },
                  { "ascd", asc } };
            cxn.SetupQueryAsync("DoesArtifactHavePublishedOrDraftVersion", new Dictionary<string, object> { { "artifactId", artifactId } }, new List<bool> { true });
            cxn.SetupQueryAsync("IsArtifactDeleted", new Dictionary<string, object> { { "artifactId", artifactId } }, new List<bool> { false });
            var testResult = new ArtifactHistoryVersion[] { new ArtifactHistoryVersion { VersionId = 1, UserId = 1, Timestamp = new DateTime() } };
            cxn.SetupQueryAsync("GetArtifactVersions", prm, testResult);
            var artifactIdsTable = SqlConnectionWrapper.ToDataTable(new List<int> { artifactId }, "Int32Collection", "Int32Value");
            var prm2 = new Dictionary<string, object> { { "userId", sessionUserId }, { "artifactIds", artifactIdsTable } };
            cxn.SetupQueryAsync("GetArtifactsWithDraft", prm2, new int[] { artifactId });

            var userIdsTable = SqlConnectionWrapper.ToDataTable(new List<int> { sessionUserId }, "Int32Collection", "Int32Value");
            cxn.SetupQueryAsync("GetUserInfos", new Dictionary<string, object> { { "userIds", userIdsTable } }, new List<UserInfo> { new UserInfo { UserId = 1, DisplayName = "David", ImageId = 1 } });

            // Act
            var actual = await repository.GetArtifactVersions(artifactId, limit, offset, userId, asc, sessionUserId);
            // Assert
            cxn.Verify();
            Assert.AreEqual(actual.ArtifactId, 1);
            Assert.AreEqual(actual.ArtifactHistoryVersions.ToList().Count(), 2);
            Assert.AreEqual(actual.ArtifactHistoryVersions.ToList()[0].VersionId, int.MaxValue);
            Assert.AreEqual(actual.ArtifactHistoryVersions.ToList()[1].VersionId, 1);
        }

        [TestMethod]
        public async Task GetArtifactVersions_WithDraftsAscending_Success()
        {
            // Arrange
            int artifactId = 1;
            int limit = 2;
            int offset = 0;
            int? userId = 1;
            bool asc = true;
            int sessionUserId = 1;
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactVersionsRepository(cxn.Object);
            var prm = new Dictionary<string, object>
                { { "artifactId", artifactId },
                  { "lim", limit },
                  { "offset", offset },
                  { "userId", userId.Value },
                  { "ascd", asc } };
            cxn.SetupQueryAsync("DoesArtifactHavePublishedOrDraftVersion", new Dictionary<string, object> { { "artifactId", artifactId } }, new List<bool> { true });
            cxn.SetupQueryAsync("IsArtifactDeleted", new Dictionary<string, object> { { "artifactId", artifactId } }, new List<bool> { false });
            var testResult = new ArtifactHistoryVersion[] { new ArtifactHistoryVersion { VersionId = 1, UserId = 1, Timestamp = new DateTime() } };
            cxn.SetupQueryAsync("GetArtifactVersions", prm, testResult);
            var artifactIdsTable = SqlConnectionWrapper.ToDataTable(new List<int> { artifactId }, "Int32Collection", "Int32Value");
            var prm2 = new Dictionary<string, object> { { "userId", sessionUserId }, { "artifactIds", artifactIdsTable } };
            cxn.SetupQueryAsync("GetArtifactsWithDraft", prm2, new int[] { artifactId });

            var userIdsTable = SqlConnectionWrapper.ToDataTable(new List<int> { sessionUserId }, "Int32Collection", "Int32Value");
            cxn.SetupQueryAsync("GetUserInfos", new Dictionary<string, object> { { "userIds", userIdsTable } }, new List<UserInfo> { new UserInfo { UserId = 1, DisplayName = "David", ImageId = 1 } });

            // Act
            var actual = await repository.GetArtifactVersions(artifactId, limit, offset, userId, asc, sessionUserId);
            // Assert
            cxn.Verify();
            Assert.AreEqual(actual.ArtifactId, 1);
            Assert.AreEqual(actual.ArtifactHistoryVersions.ToList().Count(), 2);
            Assert.AreEqual(actual.ArtifactHistoryVersions.ToList()[1].VersionId, int.MaxValue);
            Assert.AreEqual(actual.ArtifactHistoryVersions.ToList()[0].VersionId, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetVersionControlArtifactInfoAsync_ResourceNotFoundException()
        {
            // Arrange
            int userId = 1, itemId = 11;

            SqlConnectionWrapperMock connectionWrapperMock = new SqlConnectionWrapperMock();
            connectionWrapperMock.SetupQueryAsync("GetArtifactBasicDetails",
                new Dictionary<string, object> { { "@userId", userId }, { "@itemId", itemId } },
                new List<ArtifactBasicDetails> { });

            Mock<IArtifactPermissionsRepository> artifactPermissionsRepositoryMock = new Mock<IArtifactPermissionsRepository>();
            artifactPermissionsRepositoryMock.Setup(apr => apr.GetArtifactPermissions(
                It.IsAny<IEnumerable<int>>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(null);

            SqlArtifactVersionsRepository artifactVersionsRepository = new SqlArtifactVersionsRepository(
                connectionWrapperMock.Object, artifactPermissionsRepositoryMock.Object);

            // Act
            await artifactVersionsRepository.GetVersionControlArtifactInfoAsync(itemId, userId);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetVersionControlArtifactInfoAsync_AuthorizationException_EmptyPermissions()
        {
            // Arrange
            int userId = 1, itemId = 11, artifactId = 10;

            SqlConnectionWrapperMock connectionWrapperMock = new SqlConnectionWrapperMock();
            connectionWrapperMock.SetupQueryAsync("GetArtifactBasicDetails",
                new Dictionary<string, object> { { "@userId", userId }, { "@itemId", itemId } },
                new List<ArtifactBasicDetails> { new ArtifactBasicDetails
                {
                     ItemId = itemId,
                     ArtifactId = artifactId
                }});

            Mock<IArtifactPermissionsRepository> artifactPermissionsRepositoryMock = new Mock<IArtifactPermissionsRepository>();
            artifactPermissionsRepositoryMock.Setup(apr => apr.GetArtifactPermissions(
                It.IsAny<IEnumerable<int>>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(new Dictionary<int, RolePermissions> { });

            SqlArtifactVersionsRepository artifactVersionsRepository = new SqlArtifactVersionsRepository(
                connectionWrapperMock.Object, artifactPermissionsRepositoryMock.Object);

            // Act
            await artifactVersionsRepository.GetVersionControlArtifactInfoAsync(itemId, userId);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetVersionControlArtifactInfoAsync_AuthorizationException_SubArtifactRead()
        {
            // Arrange
            int userId = 1, itemId = 11, artifactId = 10;

            SqlConnectionWrapperMock connectionWrapperMock = new SqlConnectionWrapperMock();
            connectionWrapperMock.SetupQueryAsync("GetArtifactBasicDetails",
                new Dictionary<string, object> { { "@userId", userId }, { "@itemId", itemId } },
                new List<ArtifactBasicDetails> { new ArtifactBasicDetails
                {
                     ItemId = itemId,
                     ArtifactId = artifactId
                }});

            Mock<IArtifactPermissionsRepository> artifactPermissionsRepositoryMock = new Mock<IArtifactPermissionsRepository>();
            artifactPermissionsRepositoryMock.Setup(apr => apr.GetArtifactPermissions(
                It.IsAny<IEnumerable<int>>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(new Dictionary<int, RolePermissions> { { itemId, RolePermissions.Read } });

            SqlArtifactVersionsRepository artifactVersionsRepository = new SqlArtifactVersionsRepository(
                connectionWrapperMock.Object, artifactPermissionsRepositoryMock.Object);

            // Act
            await artifactVersionsRepository.GetVersionControlArtifactInfoAsync(itemId, userId);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetVersionControlArtifactInfoAsync_AuthorizationException_SubArtifactReadArtifactNone()
        {
            // Arrange
            int userId = 1, itemId = 11, artifactId = 10;

            SqlConnectionWrapperMock connectionWrapperMock = new SqlConnectionWrapperMock();
            connectionWrapperMock.SetupQueryAsync("GetArtifactBasicDetails",
                new Dictionary<string, object> { { "@userId", userId }, { "@itemId", itemId } },
                new List<ArtifactBasicDetails> { new ArtifactBasicDetails
                {
                     ItemId = itemId,
                     ArtifactId = artifactId
                }});

            Mock<IArtifactPermissionsRepository> artifactPermissionsRepositoryMock = new Mock<IArtifactPermissionsRepository>();
            artifactPermissionsRepositoryMock.Setup(apr => apr.GetArtifactPermissions(
                It.IsAny<IEnumerable<int>>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(new Dictionary<int, RolePermissions> { { itemId, RolePermissions.Read }, { artifactId, RolePermissions.None } });

            SqlArtifactVersionsRepository artifactVersionsRepository = new SqlArtifactVersionsRepository(
                connectionWrapperMock.Object, artifactPermissionsRepositoryMock.Object);

            // Act
            await artifactVersionsRepository.GetVersionControlArtifactInfoAsync(itemId, userId);
        }

        [TestMethod]
        public async Task GetVersionControlArtifactInfoAsync_SubArtifactIdNull()
        {
            // Arrange
            int userId = 1, itemId = 11, artifactId = 10;

            SqlConnectionWrapperMock connectionWrapperMock = new SqlConnectionWrapperMock();
            connectionWrapperMock.SetupQueryAsync("GetArtifactBasicDetails",
                new Dictionary<string, object> { { "@userId", userId }, { "@itemId", itemId } },
                new List<ArtifactBasicDetails> { new ArtifactBasicDetails
                {
                     ItemId = artifactId,
                     ArtifactId = artifactId
                }});

            Mock<IArtifactPermissionsRepository> artifactPermissionsRepositoryMock = new Mock<IArtifactPermissionsRepository>();
            artifactPermissionsRepositoryMock.Setup(apr => apr.GetArtifactPermissions(
                It.IsAny<IEnumerable<int>>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(new Dictionary<int, RolePermissions> { { artifactId, RolePermissions.Read } });

            SqlArtifactVersionsRepository artifactVersionsRepository = new SqlArtifactVersionsRepository(
                connectionWrapperMock.Object, artifactPermissionsRepositoryMock.Object);

            // Act
            VersionControlArtifactInfo artifactInfo = (await artifactVersionsRepository.GetVersionControlArtifactInfoAsync(itemId, userId));

            // Assert
            connectionWrapperMock.Verify();
            Assert.IsNull(artifactInfo.SubArtifactId);
        }

        [TestMethod]
        public async Task GetVersionControlArtifactInfoAsync_SubArtifactIdNotNull()
        {
            // Arrange
            int userId = 1, itemId = 11, artifactId = 10;

            SqlConnectionWrapperMock connectionWrapperMock = new SqlConnectionWrapperMock();
            connectionWrapperMock.SetupQueryAsync("GetArtifactBasicDetails",
                new Dictionary<string, object> { { "@userId", userId }, { "@itemId", itemId } },
                new List<ArtifactBasicDetails> { new ArtifactBasicDetails
                {
                     ItemId = itemId,
                     ArtifactId = artifactId
                }});

            Mock<IArtifactPermissionsRepository> artifactPermissionsRepositoryMock = new Mock<IArtifactPermissionsRepository>();
            artifactPermissionsRepositoryMock.Setup(apr => apr.GetArtifactPermissions(
                It.IsAny<IEnumerable<int>>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(new Dictionary<int, RolePermissions> { { artifactId, RolePermissions.Read } });

            SqlArtifactVersionsRepository artifactVersionsRepository = new SqlArtifactVersionsRepository(
                connectionWrapperMock.Object, artifactPermissionsRepositoryMock.Object);

            // Act
            VersionControlArtifactInfo artifactInfo = (await artifactVersionsRepository.GetVersionControlArtifactInfoAsync(itemId, userId));

            // Assert
            connectionWrapperMock.Verify();
            Assert.IsNotNull(artifactInfo.SubArtifactId);
        }

        [TestMethod]
        public async Task GetVersionControlArtifactInfoAsync_NotDeleted()
        {
            // Arrange
            int userId = 1, itemId = 11, artifactId = 10, lockedByUserId = 2, latestDeletedByUserId = 3;

            SqlConnectionWrapperMock connectionWrapperMock = new SqlConnectionWrapperMock();
            connectionWrapperMock.SetupQueryAsync("GetArtifactBasicDetails",
                new Dictionary<string, object> { { "@userId", userId }, { "@itemId", itemId } },
                new List<ArtifactBasicDetails> { new ArtifactBasicDetails
                {
                     ItemId = itemId,
                     ArtifactId = artifactId,
                     DraftDeleted = false,
                     LatestDeleted = false,
                     UserId = userId,
                     LockedByUserId = lockedByUserId,
                     LatestDeletedByUserId = latestDeletedByUserId
                }});

            Mock<IArtifactPermissionsRepository> artifactPermissionsRepositoryMock = new Mock<IArtifactPermissionsRepository>();
            artifactPermissionsRepositoryMock.Setup(apr => apr.GetArtifactPermissions(
                It.IsAny<IEnumerable<int>>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(new Dictionary<int, RolePermissions> { { artifactId, RolePermissions.Read } });

            SqlArtifactVersionsRepository artifactVersionsRepository = new SqlArtifactVersionsRepository(
                connectionWrapperMock.Object, artifactPermissionsRepositoryMock.Object);

            // Act
            VersionControlArtifactInfo artifactInfo = (await artifactVersionsRepository.GetVersionControlArtifactInfoAsync(itemId, userId));

            // Assert
            connectionWrapperMock.Verify();
            Assert.IsNull(artifactInfo.DeletedByUser);
        }

        [TestMethod]
        public async Task GetVersionControlArtifactInfoAsync_DraftDeleted()
        {
            // Arrange
            int userId = 1, itemId = 11, artifactId = 10, lockedByUserId = 2, latestDeletedByUserId = 3;

            SqlConnectionWrapperMock connectionWrapperMock = new SqlConnectionWrapperMock();
            connectionWrapperMock.SetupQueryAsync("GetArtifactBasicDetails",
                new Dictionary<string, object> { { "@userId", userId }, { "@itemId", itemId } },
                new List<ArtifactBasicDetails> { new ArtifactBasicDetails
                {
                     ItemId = itemId,
                     ArtifactId = artifactId,
                     DraftDeleted = true,
                     LatestDeleted = false,
                     UserId = userId,
                     LockedByUserId = lockedByUserId,
                     LatestDeletedByUserId = latestDeletedByUserId
                }});

            Mock<IArtifactPermissionsRepository> artifactPermissionsRepositoryMock = new Mock<IArtifactPermissionsRepository>();
            artifactPermissionsRepositoryMock.Setup(apr => apr.GetArtifactPermissions(
                It.IsAny<IEnumerable<int>>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(new Dictionary<int, RolePermissions> { { artifactId, RolePermissions.Read } });

            SqlArtifactVersionsRepository artifactVersionsRepository = new SqlArtifactVersionsRepository(
                connectionWrapperMock.Object, artifactPermissionsRepositoryMock.Object);

            // Act
            VersionControlArtifactInfo artifactInfo = (await artifactVersionsRepository.GetVersionControlArtifactInfoAsync(itemId, userId));

            // Assert
            connectionWrapperMock.Verify();
            Assert.IsNotNull(artifactInfo.DeletedByUser);
            Assert.IsNotNull(artifactInfo.DeletedByUser.Id);
            Assert.IsTrue(artifactInfo.DeletedByUser.Id.Value == userId);
        }

        [TestMethod]
        public async Task GetVersionControlArtifactInfoAsync_LatestDeleted()
        {
            // Arrange
            int userId = 1, itemId = 11, artifactId = 10, lockedByUserId = 2, latestDeletedByUserId = 3;

            SqlConnectionWrapperMock connectionWrapperMock = new SqlConnectionWrapperMock();
            connectionWrapperMock.SetupQueryAsync("GetArtifactBasicDetails",
                new Dictionary<string, object> { { "@userId", userId }, { "@itemId", itemId } },
                new List<ArtifactBasicDetails> { new ArtifactBasicDetails
                {
                     ItemId = itemId,
                     ArtifactId = artifactId,
                     DraftDeleted = false,
                     LatestDeleted = true,
                     UserId = userId,
                     LockedByUserId = lockedByUserId,
                     LatestDeletedByUserId = latestDeletedByUserId
                }});

            Mock<IArtifactPermissionsRepository> artifactPermissionsRepositoryMock = new Mock<IArtifactPermissionsRepository>();
            artifactPermissionsRepositoryMock.Setup(apr => apr.GetArtifactPermissions(
                It.IsAny<IEnumerable<int>>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(new Dictionary<int, RolePermissions> { { artifactId, RolePermissions.Read } });

            SqlArtifactVersionsRepository artifactVersionsRepository = new SqlArtifactVersionsRepository(
                connectionWrapperMock.Object, artifactPermissionsRepositoryMock.Object);

            // Act
            VersionControlArtifactInfo artifactInfo = (await artifactVersionsRepository.GetVersionControlArtifactInfoAsync(itemId, userId));

            // Assert
            connectionWrapperMock.Verify();
            Assert.IsNotNull(artifactInfo.DeletedByUser);
            Assert.IsNotNull(artifactInfo.DeletedByUser.Id);
            Assert.IsTrue(artifactInfo.DeletedByUser.Id.Value == latestDeletedByUserId);
        }

        [TestMethod]
        public async Task GetVersionControlArtifactInfoAsync_NoChangesLockedBySomeoneElse()
        {
            // Arrange
            int userId = 1, itemId = 11, artifactId = 10, lockedByUserId = 2;

            SqlConnectionWrapperMock connectionWrapperMock = new SqlConnectionWrapperMock();
            connectionWrapperMock.SetupQueryAsync("GetArtifactBasicDetails",
                new Dictionary<string, object> { { "@userId", userId }, { "@itemId", itemId } },
                new List<ArtifactBasicDetails> { new ArtifactBasicDetails
                {
                     ItemId = itemId,
                     ArtifactId = artifactId,
                     HasDraftRelationships = false,
                     UserId = userId,
                     LockedByUserId = lockedByUserId,
                }});

            Mock<IArtifactPermissionsRepository> artifactPermissionsRepositoryMock = new Mock<IArtifactPermissionsRepository>();
            artifactPermissionsRepositoryMock.Setup(apr => apr.GetArtifactPermissions(
                It.IsAny<IEnumerable<int>>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(new Dictionary<int, RolePermissions> { { artifactId, RolePermissions.Read } });

            SqlArtifactVersionsRepository artifactVersionsRepository = new SqlArtifactVersionsRepository(
                connectionWrapperMock.Object, artifactPermissionsRepositoryMock.Object);

            // Act
            VersionControlArtifactInfo artifactInfo = (await artifactVersionsRepository.GetVersionControlArtifactInfoAsync(itemId, userId));

            // Assert
            connectionWrapperMock.Verify();
            Assert.IsFalse(artifactInfo.HasChanges);
        }

        [TestMethod]
        public async Task GetVersionControlArtifactInfoAsync_NoChangesDraftRelationships()
        {
            // Arrange
            int userId = 1, itemId = 11, artifactId = 10, lockedByUserId = 2;

            SqlConnectionWrapperMock connectionWrapperMock = new SqlConnectionWrapperMock();
            connectionWrapperMock.SetupQueryAsync("GetArtifactBasicDetails",
                new Dictionary<string, object> { { "@userId", userId }, { "@itemId", itemId } },
                new List<ArtifactBasicDetails> { new ArtifactBasicDetails
                {
                     ItemId = itemId,
                     ArtifactId = artifactId,
                     HasDraftRelationships = true,
                     UserId = userId,
                     LockedByUserId = lockedByUserId,
                }});

            Mock<IArtifactPermissionsRepository> artifactPermissionsRepositoryMock = new Mock<IArtifactPermissionsRepository>();
            artifactPermissionsRepositoryMock.Setup(apr => apr.GetArtifactPermissions(
                It.IsAny<IEnumerable<int>>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(new Dictionary<int, RolePermissions> { { artifactId, RolePermissions.Read } });

            SqlArtifactVersionsRepository artifactVersionsRepository = new SqlArtifactVersionsRepository(
                connectionWrapperMock.Object, artifactPermissionsRepositoryMock.Object);

            // Act
            VersionControlArtifactInfo artifactInfo = (await artifactVersionsRepository.GetVersionControlArtifactInfoAsync(itemId, userId));

            // Assert
            connectionWrapperMock.Verify();
            Assert.IsTrue(artifactInfo.HasChanges);
        }

        [TestMethod]
        public async Task GetVersionControlArtifactInfoAsync_HasChangesLockedByMe()
        {
            // Arrange
            int userId = 1, itemId = 11, artifactId = 10;

            SqlConnectionWrapperMock connectionWrapperMock = new SqlConnectionWrapperMock();
            connectionWrapperMock.SetupQueryAsync("GetArtifactBasicDetails",
                new Dictionary<string, object> { { "@userId", userId }, { "@itemId", itemId } },
                new List<ArtifactBasicDetails> { new ArtifactBasicDetails
                {
                     ItemId = itemId,
                     ArtifactId = artifactId,
                     HasDraftRelationships = false,
                     UserId = userId,
                     LockedByUserId = userId,
                }});

            Mock<IArtifactPermissionsRepository> artifactPermissionsRepositoryMock = new Mock<IArtifactPermissionsRepository>();
            artifactPermissionsRepositoryMock.Setup(apr => apr.GetArtifactPermissions(
                It.IsAny<IEnumerable<int>>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(new Dictionary<int, RolePermissions> { { artifactId, RolePermissions.Read } });

            SqlArtifactVersionsRepository artifactVersionsRepository = new SqlArtifactVersionsRepository(
                connectionWrapperMock.Object, artifactPermissionsRepositoryMock.Object);

            // Act
            VersionControlArtifactInfo artifactInfo = (await artifactVersionsRepository.GetVersionControlArtifactInfoAsync(itemId, userId));

            // Assert
            connectionWrapperMock.Verify();
            Assert.IsTrue(artifactInfo.HasChanges);
        }
    }
}
