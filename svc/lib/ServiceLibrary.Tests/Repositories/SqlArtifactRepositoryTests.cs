using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace ServiceLibrary.Repositories
{
    [TestClass]
    public class SqlArtifactRepositoryTests
    {
        #region GetProjectOrArtifactChildrenAsync

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetProjectOrArtifactChildrenAsync_InvalidProjectId()
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
        public async Task GetProjectOrArtifactChildrenAsync_InvalidArtifactId()
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
        public async Task GetProjectOrArtifactChildrenAsync_InvalidUserId()
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
        public async Task GetProjectOrArtifactChildrenAsync_ArtifactIdIsEqualToProjectId_NotFound()
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
        public async Task GetProjectOrArtifactChildrenAsync_ProjectNotFound()
        {
            // Arrange
            var projectId = 1;
            var artifactId = 1;
            var userId = 1;

            var input = new List<ArtifactVersion>();

            // Act and Assert
            await GetProjectOrArtifactChildrenBaseTestAsync(projectId, artifactId, userId, input, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetProjectOrArtifactChildrenAsync_ArtifactInProjectNotFound()
        {
            // Arrange
            var projectId = 1;
            var artifactId = 10;
            var userId = 1;

            var input = new List<ArtifactVersion>();

            // Act and Assert
            await GetProjectOrArtifactChildrenBaseTestAsync(projectId, artifactId, userId, input, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetProjectOrArtifactChildrenAsync_ArtifactInProjectNotFoundWhenItsVersionMissing()
        {
            // Arrange
            var projectId = 1;
            var artifactId = 11;
            var userId = 1;

            var input = CreateChildrenArtifactVersions();
            input.RemoveAt(2);
            input[1].HasDraft = false;

            // Act and Assert
            await GetProjectOrArtifactChildrenBaseTestAsync(projectId, artifactId, userId, input, null);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetProjectOrArtifactChildrenAsync_Forbidden_NullDirectPermissions()
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
            await GetProjectOrArtifactChildrenBaseTestAsync(projectId, artifactId, userId, input, null);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetProjectOrArtifactChildrenAsync_Forbidden_NoneDirectPermissions()
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
            await GetProjectOrArtifactChildrenBaseTestAsync(projectId, artifactId, userId, input, null);
        }

        [TestMethod]
        public async Task GetProjectOrArtifactChildrenAsync_ChildPublished_DirectPermissons()
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
            await GetProjectOrArtifactChildrenBaseTestAsync(projectId, artifactId, userId, input, expected);
        }

        [TestMethod]
        public async Task GetProjectOrArtifactChildrenAsync_ChildPublished_ProjectPermissons()
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
            await GetProjectOrArtifactChildrenBaseTestAsync(projectId, artifactId, userId, input, expected);
        }

        [TestMethod]
        public async Task GetProjectOrArtifactChildrenAsync_ChildPublished_AncestorPermissons()
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
            await GetProjectOrArtifactChildrenBaseTestAsync(projectId, artifactId, userId, input, expected);
        }

        [TestMethod]
        public async Task GetOpenArtifactAuthorHistories_Successfully()
        {
            // Arrange
            var artifactId = 10;
            var authorHistory = new SqlAuthorHistory
            {
                ItemId = artifactId,
                CreationTimestamp = DateTime.Today.AddHours(-2),
                CreationUserId = 1,
                ModificationTimestamp = DateTime.Today.AddHours(-1),
                ModificationUserId = 2
            };
            var artifactIds = new[] { 1 };

            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactRepository(cxn.Object);
            cxn.SetupQueryAsync("GetOpenArtifactAuthorHistories", new Dictionary<string, object> { { "artifactIds", SqlConnectionWrapper.ToDataTable(artifactIds) }, { "revisionId", int.MaxValue } }, Enumerable.Repeat(authorHistory, 1));
            // Act
            var actual = await repository.GetAuthorHistories(artifactIds);

            // Assert
            cxn.Verify();

            Assert.AreEqual(artifactId, actual.ElementAt(0).ItemId);
            Assert.AreEqual(authorHistory.CreationUserId, actual.ElementAt(0).CreatedBy);
            Assert.AreEqual(DateTime.SpecifyKind(authorHistory.CreationTimestamp.Value, DateTimeKind.Utc), actual.ElementAt(0).CreatedOn);
            Assert.AreEqual(authorHistory.ModificationUserId, actual.ElementAt(0).LastEditedBy);
            Assert.AreEqual(DateTime.SpecifyKind(authorHistory.ModificationTimestamp.Value, DateTimeKind.Utc), actual.ElementAt(0).LastEditedOn);
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
            await GetProjectOrArtifactChildrenBaseTestAsync(projectId, artifactId, userId, input, expected);
        }

        [TestMethod]
        public async Task GetProjectOrArtifactChildrenAsync_ChildDraftMovedWithinProject()
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
            await GetProjectOrArtifactChildrenBaseTestAsync(projectId, artifactId, userId, input, expected);
        }

        [TestMethod]
        public async Task GetProjectOrArtifactChildrenAsync_ChildDraftMovedToAnotherProject()
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
            await GetProjectOrArtifactChildrenBaseTestAsync(projectId, artifactId, userId, input, expected);
        }

        [TestMethod]
        public async Task GetProjectOrArtifactChildrenAsync_ChildDraftByAnotherUser()
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
            await GetProjectOrArtifactChildrenBaseTestAsync(projectId, artifactId, userId, input, expected);
        }

        [TestMethod]
        public async Task GetProjectOrArtifactChildrenAsync_Order_CollectionsBaselinesAndReviews()
        {
            // Arrange
            var projectId = 1;
            var userId = 1;

            var input = CreateChildrenArtifactVersions();
            input.RemoveAt(2);
            input[1].HasDraft = false;
            input[0].ItemId = projectId;
            input[0].ParentId = null;
            input[0].ItemTypePredefined = ItemTypePredefined.Project;
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
            await GetProjectOrArtifactChildrenBaseTestAsync(projectId, null, userId, input, expected, inputOrphans);
        }

        [TestMethod]
        public async Task GetProjectOrArtifactChildrenAsync_NoOrphans()
        {
            // Arrange
            var projectId = 1;
            var userId = 1;

            var input = CreateChildrenArtifactVersions();
            input.RemoveAt(2);
            input[1].HasDraft = false;
            input[0].ItemId = projectId;
            input[0].ParentId = null;
            input[0].ItemTypePredefined = ItemTypePredefined.Project;
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
            await GetProjectOrArtifactChildrenBaseTestAsync(projectId, null, userId, input, expected, inputOrphans);
        }

        [TestMethod]
        public async Task GetProjectOrArtifactChildrenAsync_OrphanPublished_Project_OrphanArtifact_IncludeOrphan()
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
            input[0].ItemTypePredefined = ItemTypePredefined.Project;

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
            await GetProjectOrArtifactChildrenBaseTestAsync(projectId, null, userId, input, expected, inputOrphans);
        }

        [TestMethod]
        public async Task GetProjectOrArtifactChildrenAsync_OrphanPublished_Project_OrphanCollection_DoNotIncludeOrphan()
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
            input[0].ItemTypePredefined = ItemTypePredefined.Project;

            var inputOrphans = CreateChildrenArtifactVersions();
            inputOrphans.RemoveAt(3);
            inputOrphans.RemoveAt(2);
            inputOrphans.RemoveAt(0);
            inputOrphans[0].ParentId = projectId;
            inputOrphans[0].HasDraft = false;
            inputOrphans[0].Name = "orphan";
            inputOrphans[0].ItemTypePredefined = ItemTypePredefined.ArtifactCollection;

            var expected = new List<Artifact>();

            // Act and Assert
            await GetProjectOrArtifactChildrenBaseTestAsync(projectId, null, userId, input, expected, inputOrphans);
        }

        [TestMethod]
        public async Task GetProjectOrArtifactChildrenAsync_OrphanPublished_Collections_OrphanCollection_IncludeOrphan()
        {
            // Arrange
            var projectId = 1;
            var collectionsId = 2;
            var userId = 1;

            var input = CreateChildrenArtifactVersions();
            input.RemoveAt(3);
            input.RemoveAt(2);
            input.RemoveAt(1);
            input[0].ItemId = collectionsId;
            input[0].ParentId = projectId;
            input[0].ItemTypePredefined = ItemTypePredefined.CollectionFolder;

            var inputOrphans = CreateChildrenArtifactVersions();
            inputOrphans.RemoveAt(3);
            inputOrphans.RemoveAt(2);
            inputOrphans.RemoveAt(0);
            inputOrphans[0].ParentId = projectId;
            inputOrphans[0].HasDraft = false;
            inputOrphans[0].Name = "orphan";
            inputOrphans[0].ItemTypePredefined = ItemTypePredefined.ArtifactCollection;

            var expected = new List<Artifact>
            {
                new Artifact
                {
                    PredefinedType = inputOrphans[0].ItemTypePredefined.GetValueOrDefault(),
                    OrderIndex = inputOrphans[0].OrderIndex,
                    ParentId = collectionsId,
                    Id = inputOrphans[0].ItemId,
                    HasChildren = false,
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
            await GetProjectOrArtifactChildrenBaseTestAsync(projectId, collectionsId, userId, input, expected, inputOrphans);
        }

        [TestMethod]
        public async Task GetProjectOrArtifactChildrenAsync_OrphanPublished_Collections_OrphanArtifact_DoNotIncludeOrphan()
        {
            // Arrange
            var projectId = 1;
            var collectionsId = 2;
            var userId = 1;

            var input = CreateChildrenArtifactVersions();
            input.RemoveAt(3);
            input.RemoveAt(2);
            input.RemoveAt(1);
            input[0].ItemId = collectionsId;
            input[0].ParentId = projectId;
            input[0].ItemTypePredefined = ItemTypePredefined.CollectionFolder;

            var inputOrphans = CreateChildrenArtifactVersions();
            inputOrphans.RemoveAt(3);
            inputOrphans.RemoveAt(2);
            inputOrphans.RemoveAt(0);
            inputOrphans[0].ParentId = projectId;
            inputOrphans[0].HasDraft = false;
            inputOrphans[0].Name = "orphan";
            inputOrphans[0].ItemTypePredefined = ItemTypePredefined.Actor;

            var expected = new List<Artifact>();

            // Act and Assert
            await GetProjectOrArtifactChildrenBaseTestAsync(projectId, collectionsId, userId, input, expected, inputOrphans);
        }

        [TestMethod]
        public async Task GetProjectOrArtifactChildrenAsync_OrphanDraft()
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
            input[0].ItemTypePredefined = ItemTypePredefined.Project;

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
            await GetProjectOrArtifactChildrenBaseTestAsync(projectId, null, userId, input, expected, inputOrphans);
        }

        [TestMethod]
        public async Task GetProjectOrArtifactChildrenAsync_OrphanDraftByAnotherUser()
        {
            // Arrange
            var projectId = 1;
            var userId = 2;

            var input = CreateChildrenArtifactVersions();
            input.RemoveAt(3);
            input[0].ItemId = projectId;
            input[0].ParentId = null;
            input[0].ItemTypePredefined = ItemTypePredefined.Project;
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
            await GetProjectOrArtifactChildrenBaseTestAsync(projectId, null, userId, input, expected, inputOrphans);
        }

        private async Task GetProjectOrArtifactChildrenBaseTestAsync(int projectId, int? artifactId, int userId,
                                    List<ArtifactVersion> input, List<Artifact> expected,
                                    List<ArtifactVersion> inputOrphans = null)
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactRepository(cxn.Object);
            cxn.SetupQueryAsync("GetArtifactChildren", new Dictionary<string, object> { { "projectId", projectId }, { "artifactId", artifactId ?? projectId }, { "userId", userId } }, input);
            if (artifactId == null || inputOrphans != null)
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

        #endregion GetProjectOrArtifactChildrenAsync

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

            var mockRepository = new Mock<SqlArtifactRepository>(cxn.Object) { CallBase = true };

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

        [TestMethod]
        public async Task GetExpandedTreeToArtifactAsync_OrphanedCollectionArtifact_Success()
        {
            // Arrange
            const int projectId = 1;
            const int artifactId = 999;
            const int userId = 99;
            var ancestorsAndSelf = new List<ArtifactVersion>
            {
                new ArtifactVersion { ItemId = 4 } // ancestorsAndSelf does not contain projectID
            };

            var children1 = new List<Artifact>
            {
                new Artifact { Id = 2, PredefinedType = ItemTypePredefined.CollectionFolder },
                new Artifact { Id = 3, PredefinedType = ItemTypePredefined.BaselineFolder }
            };
            var children2 = new List<Artifact>
            {
                new Artifact { Id = 4, ParentId = -1} // Orphaned Collection artifact
            };
            var children3 = new List<Artifact>
            {
                new Artifact { Id = 5 }
            };
            var children4 = new List<Artifact>
            {
                new Artifact { Id = artifactId }
            };

            var cxn = new SqlConnectionWrapperMock();
            cxn.SetupQueryAsync("GetArtifactAncestorsAndSelf", new Dictionary<string, object> { { "projectId", projectId }, { "artifactId", artifactId }, { "userId", userId } }, ancestorsAndSelf);

            var mockRepository = new Mock<SqlArtifactRepository>(cxn.Object) { CallBase = true };

            mockRepository.Setup(r => r.GetProjectOrArtifactChildrenAsync(projectId, null, userId))
                .ReturnsAsync(children1);
            mockRepository.Setup(r => r.GetProjectOrArtifactChildrenAsync(projectId, children1[0].Id, userId))
                .ReturnsAsync(children2);
            mockRepository.Setup(r => r.GetProjectOrArtifactChildrenAsync(projectId, children1[1].Id, userId))
                .ReturnsAsync(children3);
            mockRepository.Setup(r => r.GetProjectOrArtifactChildrenAsync(projectId, children2[0].Id, userId))
                .ReturnsAsync(children4);

            // Act
            var result = await mockRepository.Object.GetExpandedTreeToArtifactAsync(projectId, artifactId, false, userId);

            // Assert
            Assert.AreEqual(children1.Count, result.Count);
            Assert.AreEqual(children1[0].Id, result[0].Id);
            Assert.AreEqual(children1[1].Id, result[1].Id);

            Assert.AreEqual(true, result[0].HasChildren);
            Assert.AreEqual(children2.Count, result[0].Children.Count);
            Assert.AreEqual(children2[0].Id, result[0].Children[0].Id);

            Assert.AreEqual(true, result[0].Children[0].HasChildren);
            Assert.AreEqual(children4.Count, result[0].Children[0].Children.Count);
            Assert.AreEqual(artifactId, result[0].Children[0].Children[0].Id);

            Assert.IsNull(result[1].Children);
        }

        [TestMethod]
        public async Task GetExpandedTreeToArtifactAsync_OrphanedBaselinesAndReviewsArtifact_Success()
        {
            // Arrange
            const int projectId = 1;
            const int artifactId = 999;
            const int userId = 99;
            var ancestorsAndSelf = new List<ArtifactVersion>
            {
                new ArtifactVersion { ItemId = 4 } // ancestorsAndSelf does not contain projectID
            };

            var children1 = new List<Artifact>
            {
                new Artifact { Id = 2, PredefinedType = ItemTypePredefined.CollectionFolder },
                new Artifact { Id = 3, PredefinedType = ItemTypePredefined.BaselineFolder }
            };
            var children2 = new List<Artifact>
            {
                new Artifact { Id = 5 }
            };
            var children3 = new List<Artifact>
            {
                new Artifact { Id = 4, ParentId = -1} // Orphaned Collection artifact
            };
            var children4 = new List<Artifact>
            {
                new Artifact { Id = artifactId }
            };

            var cxn = new SqlConnectionWrapperMock();
            cxn.SetupQueryAsync("GetArtifactAncestorsAndSelf", new Dictionary<string, object> { { "projectId", projectId }, { "artifactId", artifactId }, { "userId", userId } }, ancestorsAndSelf);

            var mockRepository = new Mock<SqlArtifactRepository>(cxn.Object) { CallBase = true };

            mockRepository.Setup(r => r.GetProjectOrArtifactChildrenAsync(projectId, null, userId))
                .ReturnsAsync(children1);
            mockRepository.Setup(r => r.GetProjectOrArtifactChildrenAsync(projectId, children1[0].Id, userId))
                .ReturnsAsync(children2);
            mockRepository.Setup(r => r.GetProjectOrArtifactChildrenAsync(projectId, children1[1].Id, userId))
                .ReturnsAsync(children3);
            mockRepository.Setup(r => r.GetProjectOrArtifactChildrenAsync(projectId, children3[0].Id, userId))
                .ReturnsAsync(children4);

            // Act
            var result = await mockRepository.Object.GetExpandedTreeToArtifactAsync(projectId, artifactId, false, userId);

            // Assert
            Assert.AreEqual(children1.Count, result.Count);
            Assert.AreEqual(children1[0].Id, result[0].Id);
            Assert.AreEqual(children1[1].Id, result[1].Id);

            Assert.IsNull(result[0].Children);

            Assert.AreEqual(true, result[1].HasChildren);
            Assert.AreEqual(children3.Count, result[1].Children.Count);
            Assert.AreEqual(children3[0].Id, result[1].Children[0].Id);

            Assert.AreEqual(true, result[1].Children[0].HasChildren);
            Assert.AreEqual(children4.Count, result[1].Children[0].Children.Count);
            Assert.AreEqual(artifactId, result[1].Children[0].Children[0].Id);
        }

        #endregion GetExpandedTreeToArtifactAsync

        #region GetSubArtifactTreeAsync

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
            subArtifacts.Add(new SubArtifact { Id = 1111, ParentId = artifactId });
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

            var itemLabels = new List<ItemLabel> { new ItemLabel { ItemId = 1111, Label = "Precondition" }, new ItemLabel { ItemId = 2222, Label = "Postcondition" } };

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

        #endregion GetSubArtifactTreeAsync

        #region GetArtifactNavigationPathAsync

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetArtifactNavigationPathAsync_InvalidArtifactId()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactRepository(cxn.Object);

            // Act
            await repository.GetArtifactNavigationPathAsync(0, 1);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetArtifactNavigationPathAsync_InvalidUserId()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactRepository(cxn.Object);

            // Act
            await repository.GetArtifactNavigationPathAsync(1, 0);

            // Assert
        }

        [ExpectedException(typeof(ResourceNotFoundException))]
        [TestMethod]
        public async Task GetArtifactNavigationPathAsync_ArtifactNotFound_ThrowException()
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
                await repository.GetArtifactNavigationPathAsync(artifactId, userId);
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
        public async Task GetArtifactNavigationPathAsync_ArtifactIsDeletedAndPublished_ThrowException()
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
                await repository.GetArtifactNavigationPathAsync(artifactId, userId);
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
        public async Task GetArtifactNavigationPathAsync_NoPermissionsForArtifact_ThrowException()
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
                await repository.GetArtifactNavigationPathAsync(artifactId, userId);
            }
            catch (AuthorizationException e)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.UnauthorizedAccess, e.ErrorCode);
                throw;
            }
        }

        [TestMethod]
        public async Task GetArtifactNavigationPathAsync_Success()
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

            var permissions = new Dictionary<int, RolePermissions> { { artifactId, RolePermissions.Read } };

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
            var actual = await repository.GetArtifactNavigationPathAsync(artifactId, userId);

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

        #endregion GetArtifactNavigationPathAsync

        #region GetArtifactsNavigationPathsAsync

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetArtifactsNavigationPathsAsync_InvalidArtifactIds_ThrowsException()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactRepository(cxn.Object);

            // Act
            await repository.GetArtifactsNavigationPathsAsync(1, null);

            // Assert
        }

        [TestMethod]
        public async Task GetArtifactsNavigationPathsAsync_Success()
        {
            // Arrange
            int[] artifactIds = { 1 };
            const int userId = 1;

            ArtifactsNavigationPath[] queryResult ={
                new ArtifactsNavigationPath()
            };

            var cxn = new SqlConnectionWrapperMock();
            cxn.SetupQueryAsync("GetArtifactsNavigationPaths",
                new Dictionary<string, object>
                {
                    { "artifactIds", SqlConnectionWrapper.ToDataTable(artifactIds, "Int32Collection", "Int32Value")},
                    { "userId", userId },
                    { "revisionId", int.MaxValue },
                    { "addDrafts", true }
                },
                queryResult);

            var repository = new SqlArtifactRepository(cxn.Object, null, null);

            // Act
            var actual = await repository.GetArtifactsNavigationPathsAsync(userId, artifactIds);

            // Assert
            Assert.AreEqual(queryResult.Length, actual.Count);
        }

        /**
         * P (99)
         *       \
         *        A (100) 
         *               \
         *                B (101)         
         */
        [TestMethod]
        public async Task GetArtifactsNavigationPathsAsync_TwoArtifactsNotIncludeItself_Success()
        {
            var artifact1Id = 100;
            var artifact2Id = 101;
            int projectId = 99;
            // Arrange
            int[] artifactIds = { artifact1Id, artifact2Id };            
            const int userId = 1;

            ArtifactsNavigationPath[] queryResult = {
                new ArtifactsNavigationPath {ArtifactId = artifact1Id, Level = 0, Name = "A", ParentId = projectId},
                new ArtifactsNavigationPath {ArtifactId = artifact2Id, Level = 0, Name = "B", ParentId = artifact1Id},
                new ArtifactsNavigationPath {ArtifactId = artifact2Id, Level = 1, Name = "A", ParentId = projectId},
                new ArtifactsNavigationPath {ArtifactId = artifact1Id, Level = 1, Name = "P", ParentId = null},
                new ArtifactsNavigationPath {ArtifactId = artifact2Id, Level = 2, Name = "P", ParentId = null},
            };

            var cxn = new SqlConnectionWrapperMock();
            cxn.SetupQueryAsync("GetArtifactsNavigationPaths",
                new Dictionary<string, object>
                {
                    { "artifactIds", SqlConnectionWrapper.ToDataTable(artifactIds)},
                    { "userId", userId },
                    { "revisionId", int.MaxValue },
                    { "addDrafts", true }
                },
                queryResult);

            var repository = new SqlArtifactRepository(cxn.Object, null, null);

            // Act
            var actual = await repository.GetArtifactsNavigationPathsAsync(userId, artifactIds, false);

            // Assert
            Assert.AreEqual(artifactIds.Length, actual.Count);
            // for Artifact Id = 100
            var firstArtifactResult = actual[artifact1Id];
            Assert.AreEqual(1, firstArtifactResult.Count());
            var firstItem = firstArtifactResult.First();
            Assert.AreEqual(projectId, firstItem.Id);
            Assert.AreEqual("P", firstItem.Name);

            // for Artifact Id = 101
            var secondArtifactResult = actual[artifact2Id];
            Assert.AreEqual(2, secondArtifactResult.Count());
            firstItem = secondArtifactResult.First();
            Assert.AreEqual(projectId, firstItem.Id);
            Assert.AreEqual("P", firstItem.Name);
            var secondItem = secondArtifactResult.Last();
            Assert.AreEqual(artifact1Id, secondItem.Id);
            Assert.AreEqual("A", secondItem.Name);
        }


        /**
         * P (99)
         *       \
         *        A (100) 
         *               \
         *                B (101)         
         */
        [TestMethod]
        public async Task GetArtifactsNavigationPathsAsync_TwoArtifactsIncludeItself_Success()
        {
            var artifact1Id = 100;
            var artifact2Id = 101;
            int projectId = 99;
            // Arrange
            int[] artifactIds = { artifact1Id, artifact2Id };
            const int userId = 1;

            ArtifactsNavigationPath[] queryResult = {
                new ArtifactsNavigationPath {ArtifactId = artifact1Id, Level = 0, Name = "A", ParentId = projectId},
                new ArtifactsNavigationPath {ArtifactId = artifact2Id, Level = 0, Name = "B", ParentId = artifact1Id},
                new ArtifactsNavigationPath {ArtifactId = artifact2Id, Level = 1, Name = "A", ParentId = projectId},
                new ArtifactsNavigationPath {ArtifactId = artifact1Id, Level = 1, Name = "P", ParentId = null},
                new ArtifactsNavigationPath {ArtifactId = artifact2Id, Level = 2, Name = "P", ParentId = null},
            };

            var cxn = new SqlConnectionWrapperMock();
            cxn.SetupQueryAsync("GetArtifactsNavigationPaths",
                new Dictionary<string, object>
                {
                    { "artifactIds", SqlConnectionWrapper.ToDataTable(artifactIds)},
                    { "userId", userId },
                    { "revisionId", int.MaxValue },
                    { "addDrafts", true }
                },
                queryResult);

            var repository = new SqlArtifactRepository(cxn.Object, null, null);

            // Act
            var actual = await repository.GetArtifactsNavigationPathsAsync(userId, artifactIds);

            // Assert
            Assert.AreEqual(artifactIds.Length, actual.Count);
            // for Artifact Id = 100
            var firstArtifactResult = actual[artifact1Id];
            Assert.AreEqual(2, firstArtifactResult.Count());
            var firstItem = firstArtifactResult.First();
            Assert.AreEqual(projectId, firstItem.Id);
            Assert.AreEqual("P", firstItem.Name);
            var secondItem = firstArtifactResult.Last();
            Assert.AreEqual(artifact1Id, secondItem.Id);
            Assert.AreEqual("A", secondItem.Name);

            // for Artifact Id = 101
            var secondArtifactResult = actual[artifact2Id];
            Assert.AreEqual(3, secondArtifactResult.Count());
            firstItem = secondArtifactResult.First();
            Assert.AreEqual(projectId, firstItem.Id);
            Assert.AreEqual("P", firstItem.Name);
            var lastItem = secondArtifactResult.Last();
            Assert.AreEqual(artifact2Id, lastItem.Id);
            Assert.AreEqual("B", lastItem.Name);
        }

        [TestMethod]
        public async Task GetArtifactsNavigationPathsAsync_ReturnsMultipleLevels_Success()
        {
            // Arrange
            int[] artifactIds = { 1 };
            const int userId = 1;

            var queryResult = new List<ArtifactsNavigationPath>
            {
                new ArtifactsNavigationPath {ArtifactId = 1, Level = 0, Name = "ArtifactName"},
                new ArtifactsNavigationPath {ArtifactId = 1, Level = 1, Name = "ArtifactParent"},
                new ArtifactsNavigationPath {ArtifactId = 1, Level = 2, Name = "ArtifactGrandParent"}
            };

            var cxn = new SqlConnectionWrapperMock();
            cxn.SetupQueryAsync("GetArtifactsNavigationPaths",
                new Dictionary<string, object>
                {
                    { "artifactIds", SqlConnectionWrapper.ToDataTable(artifactIds, "Int32Collection", "Int32Value")},
                    { "userId", userId },
                    { "revisionId", int.MaxValue },
                    { "addDrafts", true }
                },
                queryResult);

            var repository = new SqlArtifactRepository(cxn.Object, null, null);

            // Act
            var actual = await repository.GetArtifactsNavigationPathsAsync(userId, artifactIds, false, null);

            // Assert
            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual(2, actual[1].Count());
        }

        #endregion GetArtifactsNavigationPathsAsync

        #region GetAuthorHistoriesWithPermissionsCheck

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetAuthorHistoriesWithPermissionsCheck_ThrowsError()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactRepository(cxn.Object);

            // Act
            await repository.GetAuthorHistoriesWithPermissionsCheck(null, 1);

            // Assert
        }

        private static SqlArtifactRepository CreateSqlRepositoryWithPermissions(int[] artifactIds, int userId,
            IEnumerable<SqlAuthorHistory> authorHistories, RolePermissions rolePermissions)
        {
            var cxn = new SqlConnectionWrapperMock();

            cxn.SetupQueryAsync("GetOpenArtifactAuthorHistories",
                new Dictionary<string, object>
                {
                    {"artifactIds", SqlConnectionWrapper.ToDataTable(artifactIds)},
                    {"revisionId", int.MaxValue}
                },
                authorHistories);

            var permissions = new Dictionary<int, RolePermissions>();
            permissions.Add(artifactIds[0], rolePermissions);
            var mockArtifactPermissionsRepository = new Mock<IArtifactPermissionsRepository>();
            mockArtifactPermissionsRepository.Setup(
                m => m.GetArtifactPermissions(It.IsAny<IEnumerable<int>>(), userId, false, int.MaxValue, true))
                .ReturnsAsync(permissions);

            return new SqlArtifactRepository(cxn.Object, null, mockArtifactPermissionsRepository.Object);
        }

        [TestMethod]
        public async Task GetAuthorHistoriesWithPermissionsCheck_Success()
        {
            // Arrange
            int[] artifactIds = { 1 };
            const int userId = 1;

            var authorHistory = new SqlAuthorHistory
            {
                ItemId = artifactIds.First(),
                CreationTimestamp = DateTime.Today.AddHours(-2),
                CreationUserId = 1,
                ModificationTimestamp = DateTime.Today.AddHours(-1),
                ModificationUserId = 2
            };
            
            var repository = CreateSqlRepositoryWithPermissions(artifactIds, userId,
                Enumerable.Repeat(authorHistory, 1), RolePermissions.Read);

            // Act
            var actual = await repository.GetAuthorHistoriesWithPermissionsCheck(artifactIds, userId);

            // Assert            
            Assert.IsTrue(actual.Count() == 1);
            var firstActualResult = actual.First();
            Assert.AreEqual(firstActualResult.CreatedBy, authorHistory.CreationUserId);
            Assert.AreEqual(firstActualResult.CreatedOn, authorHistory.CreationTimestamp);
            Assert.AreEqual(firstActualResult.LastEditedBy, authorHistory.ModificationUserId);
            Assert.AreEqual(firstActualResult.LastEditedOn, authorHistory.ModificationTimestamp);
        }        


        [TestMethod]
        public async Task GetAuthorHistoriesWithPermissionsCheck_NoPermissions()
        {
            // Arrange
            int[] artifactIds = { 1 };
            const int userId = 1;                      

            var authorHistory = new SqlAuthorHistory
            {
                ItemId = artifactIds.First(),
                CreationTimestamp = DateTime.Today.AddHours(-2),
                CreationUserId = 1,
                ModificationTimestamp = DateTime.Today.AddHours(-1),
                ModificationUserId = 2
            };

            var repository = CreateSqlRepositoryWithPermissions(artifactIds, userId,
                Enumerable.Repeat(authorHistory, 1), RolePermissions.Trace);

            // Act
            var actual = await repository.GetAuthorHistoriesWithPermissionsCheck(artifactIds, userId);

            // Assert            
            Assert.IsFalse(actual.Any());
        }

        [TestMethod]
        public async Task GetAuthorHistoriesWithPermissionsCheck_ArtifactNotExists()
        {
            // Arrange
            int[] artifactIds = { 1 };
            const int userId = 1;

            var repository = CreateSqlRepositoryWithPermissions(artifactIds, userId,
                Enumerable.Empty<SqlAuthorHistory>(), RolePermissions.Read);            

            // Act
            var actual = await repository.GetAuthorHistoriesWithPermissionsCheck(artifactIds, userId);

            // Assert            
            Assert.IsTrue(actual.Count() == 0);            
        }

        #endregion GetAuthorHistoriesWithPermissionsCheck
    }
}
