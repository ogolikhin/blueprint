using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;
using ServiceLibrary.Models;

namespace ArtifactStore.Repositories
{
    [TestClass]
    public class SqlArtifactRepositoryTests
    {
        #region GetProjectOrGetChildrenAsync

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetProjectOrGetChildrenAsync_InvalidProjectId()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactRepository(cxn.Object);

            // Act
            await repository.GetProjectOrArtifactChildrenAsync(0, 1, 2);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetProjectOrGetChildrenAsync_InvalidArtifactId()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactRepository(cxn.Object);

            // Act
            await repository.GetProjectOrArtifactChildrenAsync(1, 0, 2);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetProjectOrGetChildrenAsync_InvalidUserId()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactRepository(cxn.Object);

            // Act
            await repository.GetProjectOrArtifactChildrenAsync(1, 2, 0);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetProjectOrGetChildrenAsync_ArtifactIdIsEqualToProjectId_NotFound()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactRepository(cxn.Object);

            // Act
            await repository.GetProjectOrArtifactChildrenAsync(1, 1, 2);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetProjectOrGetChildrenAsync_ProjectNotFound()
        {
            // Arrange
            var projectId = 1;
            var artifactId = 1;
            var userId = 1;

            var input = new List<ArtifactVersion>();

            // Act and Assert
            await GetProjectOrGetChildrenBaseTest(projectId, artifactId, userId, input, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetProjectOrGetChildrenAsync_ArtifactInProjectNotFound()
        {
            // Arrange
            var projectId = 1;
            var artifactId = 10;
            var userId = 1;

            var input = new List<ArtifactVersion>();

            // Act and Assert
            await GetProjectOrGetChildrenBaseTest(projectId, artifactId, userId, input, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetProjectOrGetChildrenAsync_ArtifactInProjectNotFoundWhenItsVersionMissing()
        {
            // Arrange
            var projectId = 1;
            var artifactId = 11;
            var userId = 1;

            var input = CreateChildrenArtifactVersions();
            input.RemoveAt(2);
            input[1].HasDraft = false;

            // Act and Assert
            await GetProjectOrGetChildrenBaseTest(projectId, artifactId, userId, input, null);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetProjectOrGetChildrenAsync_Forbidden_NullDirectPermissions()
        {
            // Arrange
            var projectId = 1;
            var artifactId = 10;
            var userId = 1;

            var input = CreateChildrenArtifactVersions();
            input.RemoveAt(2);
            input[1].HasDraft = false;
            input[0].DirectPermissions = null;

            // Act and Assert
            await GetProjectOrGetChildrenBaseTest(projectId, artifactId, userId, input, null);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetProjectOrGetChildrenAsync_Forbidden_NoneDirectPermissions()
        {
            // Arrange
            var projectId = 1;
            var artifactId = 10;
            var userId = 1;

            var input = CreateChildrenArtifactVersions();
            input.RemoveAt(2);
            input[1].HasDraft = false;
            input[0].DirectPermissions = RolePermissions.None;

            // Act and Assert
            await GetProjectOrGetChildrenBaseTest(projectId, artifactId, userId, input, null);
        }

        [TestMethod]
        public async Task GetProjectOrGetChildrenAsync_ChildPublished_DirectPermissons()
        {
            // Arrange
            var projectId = 1;
            var artifactId = 10;
            var userId = 1;

            var input = CreateChildrenArtifactVersions();
            input.RemoveAt(2);
            input[1].HasDraft = false;

            var expected = new List<Artifact>
            {
                new Artifact
                {
                    PredefinedType = input[1].ItemTypePredefined.GetValueOrDefault(),
                    OrderIndex = input[1].OrderIndex,
                    ParentId = input[1].ParentId,
                    Id = input[1].ItemId,
                    HasChildren = true,
                    Name = input[1].Name,
                    Permissions = input[1].DirectPermissions,
                    LockedDateTime = input[1].LockedByUserTime,
                    ItemTypeId = input[1].ItemTypeId,
                    LockedByUser = new UserGroup { Id = input[1].LockedByUserId },
                    Version = input[1].VersionsCount,
                    ProjectId = input[1].VersionProjectId,
                    Prefix = input[1].Prefix
                }
            };

            // Act and Assert
            await GetProjectOrGetChildrenBaseTest(projectId, artifactId, userId, input, expected);
        }

        [TestMethod]
        public async Task GetProjectOrGetChildrenAsync_ChildPublished_ProjectPermissons()
        {
            // Arrange
            var projectId = 1;
            var artifactId = 10;
            var userId = 1;

            var input = CreateChildrenArtifactVersions();
            input.RemoveAt(2);
            input[0].DirectPermissions = null;
            input[0].ParentId = 9;
            input[1].HasDraft = false;
            var project = CreateArtifactVersion(1, 1, null, 33, ServiceConstants.VersionHead, RolePermissions.Read, false);
            input.Add(project);

            var expected = new List<Artifact>
            {
                new Artifact
                {
                    PredefinedType = input[1].ItemTypePredefined.GetValueOrDefault(),
                    OrderIndex = input[1].OrderIndex,
                    ParentId = input[1].ParentId,
                    Id = input[1].ItemId,
                    HasChildren = true,
                    Name = input[1].Name,
                    Permissions = project.DirectPermissions,
                    LockedDateTime = input[1].LockedByUserTime,
                    ItemTypeId = input[1].ItemTypeId,
                    LockedByUser = new UserGroup { Id = input[1].LockedByUserId },
                    Version = input[1].VersionsCount,
                    ProjectId = input[1].VersionProjectId,
                    Prefix = input[1].Prefix
                }
            };

            // Act and Assert
            await GetProjectOrGetChildrenBaseTest(projectId, artifactId, userId, input, expected);
        }

        [TestMethod]
        public async Task GetProjectOrGetChildrenAsync_ChildPublished_AncestorPermissons()
        {
            // Arrange
            var projectId = 1;
            var artifactId = 10;
            var userId = 1;

            var input = CreateChildrenArtifactVersions();
            input.RemoveAt(2);
            input[0].DirectPermissions = null;
            input[0].ParentId = 9;
            input[1].HasDraft = false;

            var ancestor1 = CreateArtifactVersion(9, 1, 8, 33, ServiceConstants.VersionHead, null, false);
            input.Add(ancestor1);

            var ancestor2 = CreateArtifactVersion(8, 1, 7, 33, ServiceConstants.VersionHead, null, false);
            input.Add(ancestor2);

            var ancestor3 = CreateArtifactVersion(7, 1, 6, 33, ServiceConstants.VersionHead, RolePermissions.Read, false);
            input.Add(ancestor3);

            var expected = new List<Artifact>
            {
                new Artifact
                {
                    PredefinedType = input[1].ItemTypePredefined.GetValueOrDefault(),
                    OrderIndex = input[1].OrderIndex,
                    ParentId = input[1].ParentId,
                    Id = input[1].ItemId,
                    HasChildren = true,
                    Name = input[1].Name,
                    Permissions = ancestor3.DirectPermissions,
                    LockedDateTime = input[1].LockedByUserTime,
                    ItemTypeId = input[1].ItemTypeId,
                    LockedByUser = new UserGroup { Id = input[1].LockedByUserId },
                    Version = input[1].VersionsCount,
                    ProjectId = input[1].VersionProjectId,
                    Prefix = input[1].Prefix
                }
            };

            // Act and Assert
            await GetProjectOrGetChildrenBaseTest(projectId, artifactId, userId, input, expected);
        }

        [TestMethod]
        public async Task GetProjectOrGetChildrenAsync_ChildDraft()
        {
            // Arrange
            var projectId = 1;
            var artifactId = 10;
            var userId = 1;

            var input = CreateChildrenArtifactVersions();
            input[1].LockedByUserId = input[2].LockedByUserId;
            input[1].LockedByUserTime = input[2].LockedByUserTime;

            var expected = new List<Artifact>
            {
                new Artifact
                {
                    PredefinedType = input[2].ItemTypePredefined.GetValueOrDefault(),
                    OrderIndex = input[2].OrderIndex,
                    ParentId = input[2].ParentId,
                    Id = input[2].ItemId,
                    HasChildren = true,
                    Name = input[2].Name,
                    Permissions = input[2].DirectPermissions,
                    LockedDateTime = input[2].LockedByUserTime,
                    ItemTypeId = input[2].ItemTypeId,
                    LockedByUser = new UserGroup { Id = input[1].LockedByUserId },
                    Version = input[2].VersionsCount / 2,
                    ProjectId = input[2].VersionProjectId,
                    Prefix = input[2].Prefix
                }
            };

            // Act and Assert
            await GetProjectOrGetChildrenBaseTest(projectId, artifactId, userId, input, expected);
        }

        [TestMethod]
        public async Task GetProjectOrGetChildrenAsync_ChildDraftMovedWithinProject()
        {
            // Arrange
            var projectId = 1;
            var artifactId = 10;
            var userId = 1;

            var input = CreateChildrenArtifactVersions();
            input[1].LockedByUserId = input[2].LockedByUserId;
            input[1].LockedByUserTime = input[2].LockedByUserTime;
            input[2].ParentId = 40;

            var expected = new List<Artifact>();

            // Act and Assert
            await GetProjectOrGetChildrenBaseTest(projectId, artifactId, userId, input, expected);
        }

        [TestMethod]
        public async Task GetProjectOrGetChildrenAsync_ChildDraftMovedToAnotherProject()
        {
            // Arrange
            var projectId = 1;
            var artifactId = 10;
            var userId = 1;

            var input = CreateChildrenArtifactVersions();
            input[1].LockedByUserId = input[2].LockedByUserId;
            input[1].LockedByUserTime = input[2].LockedByUserTime;
            input.RemoveAt(2);

            var expected = new List<Artifact>();

            // Act and Assert
            await GetProjectOrGetChildrenBaseTest(projectId, artifactId, userId, input, expected);
        }

        [TestMethod]
        public async Task GetProjectOrGetChildrenAsync_ChildDraftByAnotherUser()
        {
            // Arrange
            var projectId = 1;
            var artifactId = 10;
            var userId = 2;

            var input = CreateChildrenArtifactVersions();
            input[1].HasDraft = false;
            input[1].LockedByUserId = input[2].LockedByUserId;
            input[1].LockedByUserTime = input[2].LockedByUserTime;
            input.RemoveAt(2);

            var expected = new List<Artifact>
            {
                new Artifact
                {
                    PredefinedType = input[1].ItemTypePredefined.GetValueOrDefault(),
                    OrderIndex = input[1].OrderIndex,
                    ParentId = input[1].ParentId,
                    Id = input[1].ItemId,
                    HasChildren = true,
                    Name = input[1].Name,
                    Permissions = input[1].DirectPermissions,
                    LockedDateTime = input[1].LockedByUserTime,
                    ItemTypeId = input[1].ItemTypeId,
                    LockedByUser = new UserGroup { Id = input[1].LockedByUserId },
                    Version = input[1].VersionsCount,
                    ProjectId = input[1].VersionProjectId,
                    Prefix = input[1].Prefix
                }
            };

            // Act and Assert
            await GetProjectOrGetChildrenBaseTest(projectId, artifactId, userId, input, expected);
        }

        [TestMethod]
        public async Task GetProjectOrGetChildrenAsync_Order_CollectionsBaselinesAndReviews()
        {
            // Arrange
            var projectId = 1;
            var userId = 1;

            var input = CreateChildrenArtifactVersions();
            input.RemoveAt(2);
            input[1].HasDraft = false;
            input[0].ItemId = projectId;
            input[0].ParentId = null;
            input[1].ParentId = projectId;

            //NOTE:: Temporary filter Review and BaseLines ou from the list
            // See US#809: http://svmtfs2015:8080/tfs/svmtfs2015/Blueprint/_workitems?_a=edit&id=809
            //var baselinesAndReviews = CreateArtifactVersion(2, 1, 1, 99, ServiceConstants.VersionHead, RolePermissions.Read, false,
            //    name: "BaselinesAndReviews",
            //    orderIndex: -1,
            //    itemTypePredefined: ItemTypePredefined.BaselineFolder,
            //    itemTypeId: 22,
            //    prefix: "BRF",
            //    lockedByUserId: null,
            //    lockedByUserTime: null,
            //    versionsCount: 1);
            //input.Add(baselinesAndReviews);

            var collections = CreateArtifactVersion(3, 1, 1, 99, ServiceConstants.VersionHead, RolePermissions.Read, false,
                name: "Collections",
                orderIndex: -1,
                itemTypePredefined: ItemTypePredefined.CollectionFolder,
                itemTypeId: ServiceConstants.StubCollectionsItemTypeId,
                prefix: "CF",
                lockedByUserId: null,
                lockedByUserTime: null,
                versionsCount: 1);
            input.Add(collections);

            var inputOrphans = new List<ArtifactVersion>();

            var expected = new List<Artifact>
            {
                new Artifact
                {
                    PredefinedType = input[1].ItemTypePredefined.GetValueOrDefault(),
                    OrderIndex = input[1].OrderIndex,
                    ParentId = input[1].ParentId,
                    Id = input[1].ItemId,
                    HasChildren = true,
                    Name = input[1].Name,
                    Permissions = input[1].DirectPermissions,
                    LockedDateTime = input[1].LockedByUserTime,
                    ItemTypeId = input[1].ItemTypeId,
                    LockedByUser = new UserGroup { Id = input[1].LockedByUserId },
                    Version = input[1].VersionsCount,
                    ProjectId = input[1].VersionProjectId,
                    Prefix = input[1].Prefix
                },
                new Artifact
                {
                    PredefinedType = collections.ItemTypePredefined.GetValueOrDefault(),
                    OrderIndex = collections.OrderIndex,
                    ParentId = collections.ParentId,
                    Id = collections.ItemId,
                    HasChildren = false,
                    Name = collections.Name,
                    Permissions = collections.DirectPermissions,
                    LockedDateTime = collections.LockedByUserTime,
                    ItemTypeId = collections.ItemTypeId,
                    LockedByUser = new UserGroup { Id = collections.LockedByUserId },
                    Version = collections.VersionsCount,
                    ProjectId = collections.VersionProjectId,
                    Prefix = collections.Prefix
                },
                //NOTE:: Temporary filter Review and BaseLines ou from the list
                // See US#809: http://svmtfs2015:8080/tfs/svmtfs2015/Blueprint/_workitems?_a=edit&id=809
                //new Artifact
                //{
                //    PredefinedType = baselinesAndReviews.ItemTypePredefined.GetValueOrDefault(),
                //    OrderIndex = baselinesAndReviews.OrderIndex,
                //    ParentId = baselinesAndReviews.ParentId,
                //    Id = baselinesAndReviews.ItemId,
                //    HasChildren = false,
                //    Name = baselinesAndReviews.Name,
                //    Permissions = baselinesAndReviews.DirectPermissions,
                //    LockedDateTime = baselinesAndReviews.LockedByUserTime,
                //    ItemTypeId = baselinesAndReviews.ItemTypeId,
                //    LockedByUserId = baselinesAndReviews.LockedByUserId,
                //    Version = baselinesAndReviews.VersionsCount,
                //    ProjectId = baselinesAndReviews.VersionProjectId,
                //    Prefix = baselinesAndReviews.Prefix
                //}
            };

            // Act and Assert
            await GetProjectOrGetChildrenBaseTest(projectId, null, userId, input, expected, inputOrphans);
        }

        [TestMethod]
        public async Task GetProjectOrGetChildrenAsync_NoOrphans()
        {
            // Arrange
            var projectId = 1;
            var userId = 1;

            var input = CreateChildrenArtifactVersions();
            input.RemoveAt(2);
            input[1].HasDraft = false;
            input[0].ItemId = projectId;
            input[0].ParentId = null;
            input[1].ParentId = projectId;

            var inputOrphans = new List<ArtifactVersion>();

            var expected = new List<Artifact>
            {
                new Artifact
                {
                    PredefinedType = input[1].ItemTypePredefined.GetValueOrDefault(),
                    OrderIndex = input[1].OrderIndex,
                    ParentId = input[1].ParentId,
                    Id = input[1].ItemId,
                    HasChildren = true,
                    Name = input[1].Name,
                    Permissions = input[1].DirectPermissions,
                    LockedDateTime = input[1].LockedByUserTime,
                    ItemTypeId = input[1].ItemTypeId,
                    LockedByUser = new UserGroup { Id = input[1].LockedByUserId },
                    Version = input[1].VersionsCount,
                    ProjectId = input[1].VersionProjectId,
                    Prefix = input[1].Prefix
                }
            };

            // Act and Assert
            await GetProjectOrGetChildrenBaseTest(projectId, null, userId, input, expected, inputOrphans);
        }

        [TestMethod]
        public async Task GetProjectOrGetChildrenAsync_OrphanPublished()
        {
            // Arrange
            var projectId = 1;
            var userId = 1;

            var input = CreateChildrenArtifactVersions();
            input.RemoveAt(3);
            input.RemoveAt(2);
            input.RemoveAt(1);
            input[0].ItemId = projectId;
            input[0].ParentId = null;

            var inputOrphans = CreateChildrenArtifactVersions();
            inputOrphans.RemoveAt(2);
            inputOrphans.RemoveAt(0);
            inputOrphans[0].ParentId = projectId;
            inputOrphans[0].HasDraft = false;
            inputOrphans[0].Name = "orphan";

            var expected = new List<Artifact>
            {
                new Artifact
                {
                    PredefinedType = inputOrphans[0].ItemTypePredefined.GetValueOrDefault(),
                    OrderIndex = inputOrphans[0].OrderIndex,
                    ParentId = inputOrphans[0].ParentId,
                    Id = inputOrphans[0].ItemId,
                    HasChildren = true,
                    Name = inputOrphans[0].Name,
                    Permissions = inputOrphans[0].DirectPermissions,
                    LockedDateTime = inputOrphans[0].LockedByUserTime,
                    ItemTypeId = inputOrphans[0].ItemTypeId,
                    LockedByUser = new UserGroup { Id = inputOrphans[0].LockedByUserId },
                    Version = inputOrphans[0].VersionsCount,
                    ProjectId = inputOrphans[0].VersionProjectId,
                    Prefix = inputOrphans[0].Prefix
                }
            };

            // Act and Assert
            await GetProjectOrGetChildrenBaseTest(projectId, null, userId, input, expected, inputOrphans);
        }

        [TestMethod]
        public async Task GetProjectOrGetChildrenAsync_OrphanDraft()
        {
            // Arrange
            var projectId = 1;
            var userId = 1;

            var input = CreateChildrenArtifactVersions();
            input.RemoveAt(3);
            input.RemoveAt(2);
            input.RemoveAt(1);
            input[0].ItemId = projectId;
            input[0].ParentId = null;

            var inputOrphans = CreateChildrenArtifactVersions();
            inputOrphans.RemoveAt(0);
            inputOrphans[0].ParentId = 90;
            inputOrphans[0].Name = "orphan";
            inputOrphans[1].ParentId = projectId;
            inputOrphans[1].Name = "orphanDraft";

            var expected = new List<Artifact>
            {
                new Artifact
                {
                    PredefinedType = inputOrphans[1].ItemTypePredefined.GetValueOrDefault(),
                    OrderIndex = inputOrphans[1].OrderIndex,
                    ParentId = inputOrphans[1].ParentId,
                    Id = inputOrphans[1].ItemId,
                    HasChildren = true,
                    Name = inputOrphans[1].Name,
                    Permissions = inputOrphans[1].DirectPermissions,
                    LockedDateTime = inputOrphans[1].LockedByUserTime,
                    ItemTypeId = inputOrphans[1].ItemTypeId,
                    LockedByUser = new UserGroup { Id = inputOrphans[1].LockedByUserId },
                    Version = inputOrphans[1].VersionsCount / 2,
                    ProjectId = inputOrphans[1].VersionProjectId,
                    Prefix = inputOrphans[1].Prefix
                }
            };

            // Act and Assert
            await GetProjectOrGetChildrenBaseTest(projectId, null, userId, input, expected, inputOrphans);
        }

        [TestMethod]
        public async Task GetProjectOrGetChildrenAsync_OrphanDraftByAnotherUser()
        {
            // Arrange
            var projectId = 1;
            var userId = 2;

            var input = CreateChildrenArtifactVersions();
            input.RemoveAt(3);
            input[0].ItemId = projectId;
            input[0].ParentId = null;
            input[1].LockedByUserId = input[2].LockedByUserId;
            input[1].LockedByUserTime = input[2].LockedByUserTime;
            input[1].ParentId = 90;
            input[1].ParentId = projectId;

            var inputOrphans = CreateChildrenArtifactVersions();
            inputOrphans.RemoveAt(2);
            inputOrphans.RemoveAt(0);
            inputOrphans[0].ParentId = projectId;
            inputOrphans[0].Name = "orphan";
            inputOrphans[0].HasDraft = false;

            var expected = new List<Artifact>
            {
                new Artifact
                {
                    PredefinedType = inputOrphans[0].ItemTypePredefined.GetValueOrDefault(),
                    OrderIndex = inputOrphans[0].OrderIndex,
                    ParentId = inputOrphans[0].ParentId,
                    Id = inputOrphans[0].ItemId,
                    HasChildren = true,
                    Name = inputOrphans[0].Name,
                    Permissions = inputOrphans[0].DirectPermissions,
                    LockedDateTime = inputOrphans[0].LockedByUserTime,
                    ItemTypeId = inputOrphans[0].ItemTypeId,
                    LockedByUser = new UserGroup { Id = inputOrphans[0].LockedByUserId },
                    Version = inputOrphans[0].VersionsCount,
                    ProjectId = inputOrphans[0].VersionProjectId,
                    Prefix = inputOrphans[0].Prefix
                }
            };

            // Act and Assert
            await GetProjectOrGetChildrenBaseTest(projectId, null, userId, input, expected, inputOrphans);
        }

        private async Task GetProjectOrGetChildrenBaseTest(int projectId, int? artifactId, int userId,
                                    List<ArtifactVersion> input, List<Artifact> expected,
                                    List<ArtifactVersion> inputOrphans = null)
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactRepository(cxn.Object);
            cxn.SetupQueryAsync("GetArtifactChildren", new Dictionary<string, object> { { "projectId", projectId }, { "artifactId", artifactId ?? projectId }, { "userId", userId } }, input);
            if (artifactId == null)
                cxn.SetupQueryAsync("GetProjectOrphans", new Dictionary<string, object> { { "projectId", projectId }, { "userId", userId } }, inputOrphans);
            // Act
            var actual = await repository.GetProjectOrArtifactChildrenAsync(projectId, artifactId, userId);

            // Assert
            cxn.Verify();
            string errorMessage;
            Assert.IsTrue(CompareArtifactChildren(expected, actual, out errorMessage), errorMessage);
        }

        private bool CompareArtifactChildren(List<Artifact> expected, List<Artifact> actual, out string errorMessage)
        {
            if (expected.Count != actual.Count)
            {
                errorMessage = I18NHelper.FormatInvariant("Expected Count: {0} does not match Actual Count: {1}.", expected.Count, actual.Count);
                return false;
            }


            for (int i = 0; i < expected.Count; i++)
            {
                const string template =
                    "At index {0} {1} of Expected element : '{2}' does not match {1} of Actual element: {3}.";

                var e = expected.ElementAt(i);
                var a = actual.ElementAt(i);
                if (e.PredefinedType != a.PredefinedType)
                {
                    errorMessage = I18NHelper.FormatInvariant(template, i, "PredefinedType", e.PredefinedType, a.PredefinedType);
                    return false;
                }

                if (e.HasChildren != a.HasChildren)
                {
                    errorMessage = I18NHelper.FormatInvariant(template, i, "HasChildren", e.HasChildren, a.HasChildren);
                    return false;
                }
                if (e.Id != a.Id)
                {
                    errorMessage = I18NHelper.FormatInvariant(template, i, "Id", e.Id, a.Id);
                    return false;
                }
                if (e.LockedByUser?.Id != a.LockedByUser?.Id)
                {
                    errorMessage = I18NHelper.FormatInvariant(template, i, "LockedByUserId", e.LockedByUser?.Id, a.LockedByUser?.Id);
                    return false;
                }
                if (e.LockedDateTime != a.LockedDateTime)
                {
                    errorMessage = I18NHelper.FormatInvariant(template, i, "LockedDateTime", e.LockedDateTime, a.LockedDateTime);
                    return false;
                }
                if (e.Name != a.Name)
                {
                    errorMessage = I18NHelper.FormatInvariant(template, i, "Name", e.Name, a.Name);
                    return false;
                }
                if (e.OrderIndex != a.OrderIndex)
                {
                    errorMessage = I18NHelper.FormatInvariant(template, i, "OrderIndex", e.OrderIndex, a.OrderIndex);
                    return false;
                }
                if (e.ParentId != a.ParentId)
                {
                    errorMessage = I18NHelper.FormatInvariant(template, i, "ParentId", e.ParentId, a.ParentId);
                    return false;
                }
                if (e.Permissions != a.Permissions)
                {
                    errorMessage = I18NHelper.FormatInvariant(template, i, "Permissions", e.Permissions, a.Permissions);
                    return false;
                }
                if (e.Prefix != a.Prefix)
                {
                    errorMessage = I18NHelper.FormatInvariant(template, i, "Prefix", e.Prefix, a.Prefix);
                    return false;
                }
                if (e.ProjectId != a.ProjectId)
                {
                    errorMessage = I18NHelper.FormatInvariant(template, i, "ProjectId", e.ProjectId, a.ProjectId);
                    return false;
                }
                if (e.ItemTypeId != a.ItemTypeId)
                {
                    errorMessage = I18NHelper.FormatInvariant(template, i, "TypeId", e.ItemTypeId, a.ItemTypeId);
                    return false;
                }
                if (e.Version != a.Version)
                {
                    errorMessage = I18NHelper.FormatInvariant(template, i, "Version", e.Version, a.Version);
                    return false;
                }
            }

            errorMessage = null;
            return true;
        }

        private List<ArtifactVersion> CreateChildrenArtifactVersions()
        {
            return new List<ArtifactVersion>
            {
                CreateArtifactVersion(10, 1, 1, 99, ServiceConstants.VersionHead, RolePermissions.Read, false),
                CreateArtifactVersion(20, 1, 10, 99, ServiceConstants.VersionHead, RolePermissions.Read, true,
                                        name: "parent",
                                        orderIndex: 10,
                                        itemTypePredefined: ItemTypePredefined.PrimitiveFolder,
                                        itemTypeId: 88,
                                        prefix: "PF",
                                        lockedByUserId: null,
                                        lockedByUserTime: null,
                                        versionsCount: 22),
                CreateArtifactVersion(20, 1, 10, 1, 1, RolePermissions.Read, true,
                                        name: "parent (draft)",
                                        orderIndex: 10,
                                        itemTypePredefined: ItemTypePredefined.PrimitiveFolder,
                                        itemTypeId: 88,
                                        prefix: "PF",
                                        lockedByUserId: 1,
                                        lockedByUserTime: DateTime.Now,
                                        versionsCount: 22),
                CreateArtifactVersion(30, 1, 20, 99, ServiceConstants.VersionHead, RolePermissions.Read, false)
            };
        }

        private ArtifactVersion CreateArtifactVersion(int itemId,
                                                        int versionProjectId,
                                                        int? parentId,
                                                        int startRevision,
                                                        int endRevision,
                                                        RolePermissions? directPermissions,
                                                        bool hasDraft,
                                                        string name = null,
                                                        double? orderIndex = null,
                                                        ItemTypePredefined? itemTypePredefined = null,
                                                        int? itemTypeId = null,
                                                        string prefix = null,
                                                        int? lockedByUserId = null,
                                                        DateTime? lockedByUserTime = null,
                                                        int? versionsCount = null)
        {
            return new ArtifactVersion
            {
                ItemId = itemId,
                VersionProjectId = versionProjectId,
                ParentId = parentId,
                Name = name,
                OrderIndex = orderIndex,
                StartRevision = startRevision,
                EndRevision = endRevision,
                ItemTypePredefined = itemTypePredefined,
                ItemTypeId = itemTypeId,
                Prefix = prefix,
                LockedByUserId = lockedByUserId,
                LockedByUserTime = lockedByUserTime,
                DirectPermissions = directPermissions,
                VersionsCount = versionsCount,
                HasDraft = hasDraft
            };
        }

        #endregion

        #region GetExpandedTreeToArtifactAsync

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetExpandedTreeToArtifactAsync_InvalidProjectId()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactRepository(cxn.Object);

            // Act
            await repository.GetExpandedTreeToArtifactAsync(0, 1, true, 2);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetExpandedTreeToArtifactAsync_InvalidArtifactId()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactRepository(cxn.Object);

            // Act
            await repository.GetExpandedTreeToArtifactAsync(1, 0, true, 2);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetExpandedTreeToArtifactAsync_InvalidUserId()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactRepository(cxn.Object);

            // Act
            await repository.GetExpandedTreeToArtifactAsync(1, 2, true, 0);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetExpandedTreeToArtifactAsync_ArtifactIdIsEqualToProjectId_NotFound()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactRepository(cxn.Object);

            // Act
            await repository.GetExpandedTreeToArtifactAsync(1, 1, true, 2);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetExpandedTreeToArtifactAsync_ArtifactInProjectNotFound()
        {
            // Arrange
            const int projectId = 1;
            const int artifactId = 2;
            const int userId = 3;

            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactRepository(cxn.Object);
            cxn.SetupQueryAsync("GetArtifactAncestorsAndSelf", new Dictionary<string, object> { { "projectId", projectId }, { "artifactId", artifactId }, { "userId", userId } }, new List<ArtifactVersion>());
           
            // Act
            await repository.GetExpandedTreeToArtifactAsync(projectId, artifactId, true, userId);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetExpandedTreeToArtifactAsync_NoProjectPermissions_Forbidden()
        {
            // Arrange
            const int projectId = 1;
            const int artifactId = 3;
            const int userId = 99;
            var ancestorsAndSelf = new List<ArtifactVersion>
            {
                new ArtifactVersion { ItemId = 1 },
                new ArtifactVersion { ItemId = 2 },
                new ArtifactVersion { ItemId = 3 },
            };

            var cxn = new SqlConnectionWrapperMock();
            cxn.SetupQueryAsync("GetArtifactAncestorsAndSelf", new Dictionary<string, object> { { "projectId", projectId }, { "artifactId", artifactId }, { "userId", userId } }, ancestorsAndSelf);

            var mockRepository = new Mock<SqlArtifactRepository>(cxn.Object) {CallBase = true};

            mockRepository.Setup(r => r.GetProjectOrArtifactChildrenAsync(projectId, null, userId))
                .Throws(new AuthorizationException());

            // Act
            await mockRepository.Object.GetExpandedTreeToArtifactAsync(projectId, artifactId, true, userId);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetExpandedTreeToArtifactAsync_NoArtifactPermissions_Forbidden()
        {
            // Arrange
            const int projectId = 1;
            const int artifactId = 3;
            const int userId = 99;
            var ancestorsAndSelf = new List<ArtifactVersion>
            {
                new ArtifactVersion { ItemId = 1 },
                new ArtifactVersion { ItemId = 2 },
                new ArtifactVersion { ItemId = 3 },
            };

            var children1 = new List<Artifact>
            {
                new Artifact { Id = 2 }
            };
            var children2 = new List<Artifact>();

            var cxn = new SqlConnectionWrapperMock();
            cxn.SetupQueryAsync("GetArtifactAncestorsAndSelf", new Dictionary<string, object> { { "projectId", projectId }, { "artifactId", artifactId }, { "userId", userId } }, ancestorsAndSelf);

            var mockRepository = new Mock<SqlArtifactRepository>(cxn.Object) { CallBase = true };

            mockRepository.Setup(r => r.GetProjectOrArtifactChildrenAsync(projectId, null, userId))
                .Returns(Task.FromResult(children1));
            mockRepository.Setup(r => r.GetProjectOrArtifactChildrenAsync(projectId, ancestorsAndSelf[1].ItemId, userId))
                .Returns(Task.FromResult(children2));

            // Act
            await mockRepository.Object.GetExpandedTreeToArtifactAsync(projectId, artifactId, true, userId);

            // Assert
        }

        [TestMethod]
        public async Task GetExpandedTreeToArtifactAsync_SuccessWithoutChildren()
        {
            // Arrange
            const int projectId = 1;
            const int artifactId = 3;
            const int userId = 99;
            var ancestorsAndSelf = new List<ArtifactVersion>
            {
                new ArtifactVersion { ItemId = 1 },
                new ArtifactVersion { ItemId = 2 },
                new ArtifactVersion { ItemId = 3 },
            };

            var children1 = new List<Artifact>
            {
                new Artifact { Id = 2 },
                new Artifact { Id = 22 }
            };
            var children2 = new List<Artifact>
            {
                new Artifact { Id = 3 },
                new Artifact { Id = 33 }
            };

            var cxn = new SqlConnectionWrapperMock();
            cxn.SetupQueryAsync("GetArtifactAncestorsAndSelf", new Dictionary<string, object> { { "projectId", projectId }, { "artifactId", artifactId }, { "userId", userId } }, ancestorsAndSelf);

            var mockRepository = new Mock<SqlArtifactRepository>(cxn.Object) { CallBase = true };

            mockRepository.Setup(r => r.GetProjectOrArtifactChildrenAsync(projectId, null, userId))
                .Returns(Task.FromResult(children1));
            mockRepository.Setup(r => r.GetProjectOrArtifactChildrenAsync(projectId, ancestorsAndSelf[1].ItemId, userId))
                .Returns(Task.FromResult(children2));

            // Act
            var result = await mockRepository.Object.GetExpandedTreeToArtifactAsync(projectId, artifactId, false, userId);

            // Assert
            Assert.AreEqual(children1.Count, result.Count);
            Assert.AreEqual(children1[0].Id, result[0].Id);
            Assert.AreEqual(children1[1].Id, result[1].Id);

            Assert.AreEqual(children2.Count, result[0].Children.Count);
            Assert.AreEqual(children2[0].Id, result[0].Children[0].Id);
            Assert.AreEqual(children2[1].Id, result[0].Children[1].Id);

            Assert.IsNull(result[0].Children[0].Children);
        }

        [TestMethod]
        public async Task GetExpandedTreeToArtifactAsync_SuccessWithChildren()
        {
            // Arrange
            const int projectId = 1;
            const int artifactId = 3;
            const int userId = 99;
            var ancestorsAndSelf = new List<ArtifactVersion>
            {
                new ArtifactVersion { ItemId = 1 },
                new ArtifactVersion { ItemId = 2 },
                new ArtifactVersion { ItemId = 3 },
            };

            var children1 = new List<Artifact>
            {
                new Artifact { Id = 2 },
                new Artifact { Id = 22 }
            };
            var children2 = new List<Artifact>
            {
                new Artifact { Id = 3 },
                new Artifact { Id = 33 }
            };
            var children3 = new List<Artifact>
            {
                new Artifact { Id = 4 },
                new Artifact { Id = 44 }
            };

            var cxn = new SqlConnectionWrapperMock();
            cxn.SetupQueryAsync("GetArtifactAncestorsAndSelf", new Dictionary<string, object> { { "projectId", projectId }, { "artifactId", artifactId }, { "userId", userId } }, ancestorsAndSelf);

            var mockRepository = new Mock<SqlArtifactRepository>(cxn.Object) { CallBase = true };

            mockRepository.Setup(r => r.GetProjectOrArtifactChildrenAsync(projectId, null, userId))
                .Returns(Task.FromResult(children1));
            mockRepository.Setup(r => r.GetProjectOrArtifactChildrenAsync(projectId, ancestorsAndSelf[1].ItemId, userId))
                .Returns(Task.FromResult(children2));
            mockRepository.Setup(r => r.GetProjectOrArtifactChildrenAsync(projectId, ancestorsAndSelf[2].ItemId, userId))
                .Returns(Task.FromResult(children3));

            // Act
            var result = await mockRepository.Object.GetExpandedTreeToArtifactAsync(projectId, artifactId, true, userId);

            // Assert
            Assert.AreEqual(children1.Count, result.Count);
            Assert.AreEqual(children1[0].Id, result[0].Id);
            Assert.AreEqual(children1[1].Id, result[1].Id);

            Assert.AreEqual(children2.Count, result[0].Children.Count);
            Assert.AreEqual(children2[0].Id, result[0].Children[0].Id);
            Assert.AreEqual(children2[1].Id, result[0].Children[1].Id);

            Assert.AreEqual(children3.Count, result[0].Children[0].Children.Count);
            Assert.AreEqual(children3[0].Id, result[0].Children[0].Children[0].Id);
            Assert.AreEqual(children3[1].Id, result[0].Children[0].Children[1].Id);
        }

        #endregion

        [TestMethod]
        public async Task GetSubArtifactTreeAsync_SingleSubArtifact_Success()
        {
            // Arrange
            const int artifactId = 3;
            const int userId = 99;
            var subArtifacts = new List<SubArtifact> { new SubArtifact { Id = 1111, ParentId = artifactId } };
            var cxn = new SqlConnectionWrapperMock();
            cxn.SetupQueryAsync("GetSubArtifacts", 
                new Dictionary<string, object> { { "artifactId", artifactId }, { "userId", userId }, { "revisionId", int.MaxValue }, { "includeDrafts", true } }, subArtifacts);
            var mockRepository = new Mock<SqlArtifactRepository>(cxn.Object) { CallBase = true };
            // Act
            var result = (await mockRepository.Object.GetSubArtifactTreeAsync(artifactId, userId, int.MaxValue, true)).ToList();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(1111, result[0].Id);
        }

        [TestMethod]
        public async Task GetSubArtifactTreeAsync_MultipleArtifactHierarchy_Success()
        {
            // Arrange
            const int artifactId = 3;
            const int userId = 99;
            var subArtifacts = new List<SubArtifact>();
            subArtifacts.Add( new SubArtifact { Id = 1111, ParentId = artifactId });
            subArtifacts.Add(new SubArtifact { Id = 2222, ParentId = 1111 });
           var cxn = new SqlConnectionWrapperMock();
            cxn.SetupQueryAsync("GetSubArtifacts",
                new Dictionary<string, object> { { "artifactId", artifactId }, { "userId", userId }, { "revisionId", int.MaxValue }, { "includeDrafts", true } }, subArtifacts);
            var mockRepository = new Mock<SqlArtifactRepository>(cxn.Object) { CallBase = true };
            // Act
            var result = (await mockRepository.Object.GetSubArtifactTreeAsync(artifactId, userId, int.MaxValue, true)).ToList();
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(1111, result[0].Id);
            Assert.AreEqual(1, result[0].Children.Count());
            Assert.AreEqual(2222, result[0].Children.ToList()[0].Id);
        }


        [TestMethod]
        public async Task GetSubArtifactTreeAsync_MultipleArtifactHierarchyUseCases_Success()
        {
            // Arrange
            const int artifactId = 3;
            const int userId = 99;
            var subArtifacts = new List<SubArtifact>();
            subArtifacts.Add(new SubArtifact { Id = 1111, ParentId = artifactId, PredefinedType = ItemTypePredefined.PreCondition });
            subArtifacts.Add(new SubArtifact { Id = 2222, ParentId = artifactId, PredefinedType = ItemTypePredefined.PostCondition });

            var itemLabels = new List<ItemLabel> { new ItemLabel { ItemId = 1111, Label = "Precondition" }, new ItemLabel { ItemId = 2222, Label = "Postcondition" }};

            var cxn = new SqlConnectionWrapperMock();
            cxn.SetupQueryAsync("GetSubArtifacts",
                new Dictionary<string, object> { { "artifactId", artifactId }, { "userId", userId }, { "revisionId", int.MaxValue }, { "includeDrafts", true } }, subArtifacts);

            var itemIds = new[] { 1111, 2222 };

            cxn.SetupQueryAsync("GetItemsLabels",
                new Dictionary<string, object> { { "itemIds", SqlConnectionWrapper.ToDataTable(itemIds, "Int32Collection", "Int32Value") }, { "userId", userId }, { "addDrafts", true }, { "revisionId", int.MaxValue } }, itemLabels);

            var mockRepository = new Mock<SqlArtifactRepository>(cxn.Object) { CallBase = true };
            // Act
            var result = (await mockRepository.Object.GetSubArtifactTreeAsync(artifactId, userId, int.MaxValue, true)).ToList();
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(1111, result[0].Id);
            Assert.AreEqual("Precondition", result[0].DisplayName);
            Assert.AreEqual(2222, result[1].Id);
            Assert.AreEqual("Postcondition", result[1].DisplayName);
        }

        #region GetArtifactNavigatioPathAsync

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetArtifactNavigatioPathAsync_InvalidArtifactId()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactRepository(cxn.Object);

            // Act
            await repository.GetArtifactNavigatioPathAsync(0, 1);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetArtifactNavigatioPathAsync_InvalidUserId()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactRepository(cxn.Object);

            // Act
            await repository.GetArtifactNavigatioPathAsync(1, 0);

            // Assert
        }

        [ExpectedException(typeof(ResourceNotFoundException))]
        [TestMethod]
        public async Task GetArtifactNavigatioPathAsync_ArtifactNotFound_ThrowException()
        {
            // Arrange
            const int artifactId = 3;
            const int userId = 99;

            var cxn = new SqlConnectionWrapperMock();
            cxn.SetupQueryAsync<List<ArtifactBasicDetails>>("GetArtifactBasicDetails", new Dictionary<string, object> { { "itemId", artifactId }, { "userId", userId } }, null);

            var repository = new SqlArtifactRepository(cxn.Object, null, null);

            // Act
            try
            {
                await repository.GetArtifactNavigatioPathAsync(artifactId, userId);
            }
            catch (ResourceNotFoundException e)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.ResourceNotFound, e.ErrorCode);
                throw;
            }
        }

        [ExpectedException(typeof(ResourceNotFoundException))]
        [TestMethod]
        public async Task GetArtifactNavigatioPathAsync_ArtifactIsDeletedAndPublished_ThrowException()
        {
            // Arrange
            const int artifactId = 3;
            const int userId = 99;
            var arifactBasicDetails = new List<ArtifactBasicDetails> { new ArtifactBasicDetails { LatestDeleted = true } };

            var cxn = new SqlConnectionWrapperMock();
            cxn.SetupQueryAsync("GetArtifactBasicDetails", new Dictionary<string, object> { { "itemId", artifactId }, { "userId", userId } }, arifactBasicDetails);

            var repository = new SqlArtifactRepository(cxn.Object, null, null);

            // Act
            try
            {
                await repository.GetArtifactNavigatioPathAsync(artifactId, userId);
            }
            catch (ResourceNotFoundException e)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.ResourceNotFound, e.ErrorCode);
                throw;
            }
        }

        [ExpectedException(typeof(AuthorizationException))]
        [TestMethod]
        public async Task GetArtifactNavigatioPathAsync_NoPermissionsForArtifact_ThrowException()
        {
            // Arrange
            const int artifactId = 3;
            const int userId = 99;
            var arifactBasicDetails = new List<ArtifactBasicDetails> { new ArtifactBasicDetails { LatestDeleted = false } };

            var permissions = new Dictionary<int, RolePermissions>();

            var cxn = new SqlConnectionWrapperMock();
            cxn.SetupQueryAsync("GetArtifactBasicDetails", new Dictionary<string, object> { { "itemId", artifactId }, { "userId", userId } }, arifactBasicDetails);

            var mockArtifactPermissionsRepository = new Mock<IArtifactPermissionsRepository>();
            mockArtifactPermissionsRepository.Setup(m => m.GetArtifactPermissions(It.IsAny<IEnumerable<int>>(), userId, false, int.MaxValue, true))
                .ReturnsAsync(permissions);

            var repository = new SqlArtifactRepository(cxn.Object, null, mockArtifactPermissionsRepository.Object);

            // Act
            try
            {
                await repository.GetArtifactNavigatioPathAsync(artifactId, userId);
            }
            catch (AuthorizationException e)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.UnauthorizedAccess, e.ErrorCode);
                throw;
            }
        }

        [TestMethod]
        public async Task GetArtifactNavigatioPathAsync_Success()
        {
            // Arrange
            const int projectId = 1;

            const int artifactId = 3;
            const int userId = 99;
            var arifactBasicDetails = new List<ArtifactBasicDetails> { new ArtifactBasicDetails { LatestDeleted = false } };

            var ancestorsAndSelf = new List<ArtifactVersion>
            {
                new ArtifactVersion { ItemId = 3, ParentId = 2, VersionProjectId = projectId, Name = "artifact", ItemTypeId = 88 },
                new ArtifactVersion { ItemId = 1, ParentId = null, VersionProjectId = projectId, Name = "project", ItemTypeId = 66 },
                new ArtifactVersion { ItemId = 2, ParentId = 1, VersionProjectId = projectId, Name = "folder", ItemTypeId = 77 }
            };

            var permissions = new Dictionary<int, RolePermissions> {{artifactId, RolePermissions.Read}};

            var cxn = new SqlConnectionWrapperMock();
            cxn.SetupQueryAsync("GetArtifactBasicDetails", new Dictionary<string, object> { { "itemId", artifactId }, { "userId", userId } }, arifactBasicDetails);
            cxn.SetupQueryAsync("GetArtifactNavigationPath", new Dictionary<string, object> { { "artifactId", artifactId }, { "userId", userId } }, ancestorsAndSelf);

            var mockArtifactPermissionsRepository = new Mock<IArtifactPermissionsRepository>();
            mockArtifactPermissionsRepository.Setup(m => m.GetArtifactPermissions(It.IsAny<IEnumerable<int>>(), userId, false, int.MaxValue, true))
                .ReturnsAsync(permissions);

            var repository = new SqlArtifactRepository(cxn.Object, null, mockArtifactPermissionsRepository.Object);

            var expected = new List<Artifact>
            {
                new Artifact { Id = ancestorsAndSelf[1].ItemId, Name = ancestorsAndSelf[1].Name, ProjectId = ancestorsAndSelf[1].VersionProjectId, ItemTypeId = ancestorsAndSelf[1].ItemTypeId },
                new Artifact { Id = ancestorsAndSelf[2].ItemId, Name = ancestorsAndSelf[2].Name, ProjectId = ancestorsAndSelf[2].VersionProjectId, ItemTypeId = ancestorsAndSelf[2].ItemTypeId }
            };

            // Act
            var actual = await repository.GetArtifactNavigatioPathAsync(artifactId, userId);

            // Assert
            Assert.AreEqual(expected.Count, actual.Count);

            Assert.AreEqual(expected[0].Id, actual[0].Id);
            Assert.AreEqual(expected[0].Name, actual[0].Name);
            Assert.AreEqual(expected[0].ProjectId, actual[0].ProjectId);
            Assert.AreEqual(expected[0].ItemTypeId, actual[0].ItemTypeId);

            Assert.AreEqual(expected[1].Id, actual[1].Id);
            Assert.AreEqual(expected[1].Name, actual[1].Name);
            Assert.AreEqual(expected[1].ProjectId, actual[1].ProjectId);
            Assert.AreEqual(expected[1].ItemTypeId, actual[1].ItemTypeId);
        }

        #endregion

    }
}

