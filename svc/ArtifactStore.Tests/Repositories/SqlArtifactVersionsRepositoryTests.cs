﻿using ArtifactStore.Helpers;
using ArtifactStore.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var artifactIdsTable = DapperHelper.GetIntCollectionTableValueParameter(new List<int> { artifactId });
            var prm2 = new Dictionary<string, object> { { "userId", sessionUserId }, { "artifactIds", artifactIdsTable } };
            cxn.SetupQueryAsync("GetArtifactsWithDraft", prm2, new List<int>());
            var userIdsTable = DapperHelper.GetIntCollectionTableValueParameter(new List<int> { sessionUserId });
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
            var artifactIdsTable = DapperHelper.GetIntCollectionTableValueParameter(new List<int> { artifactId });
            var userIdsTable = DapperHelper.GetIntCollectionTableValueParameter(new List<int> { sessionUserId });
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
            var artifactIdsTable = DapperHelper.GetIntCollectionTableValueParameter(new List<int> { artifactId });
            var prm2 = new Dictionary<string, object> { { "userId", sessionUserId }, { "artifactIds", artifactIdsTable } };
            cxn.SetupQueryAsync("GetArtifactsWithDraft", prm2, new int[] { artifactId });

            var userIdsTable = DapperHelper.GetIntCollectionTableValueParameter(new List<int> { sessionUserId });
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
            var artifactIdsTable = DapperHelper.GetIntCollectionTableValueParameter(new List<int> { artifactId });
            var prm2 = new Dictionary<string, object> { { "userId", sessionUserId }, { "artifactIds", artifactIdsTable } };
            cxn.SetupQueryAsync("GetArtifactsWithDraft", prm2, new int[] { artifactId });

            var userIdsTable = DapperHelper.GetIntCollectionTableValueParameter(new List<int> { sessionUserId });
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
    }
}
