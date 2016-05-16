﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.Helpers;
using ArtifactStore.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;
using ServiceLibrary.Models;

namespace ArtifactStore.Repositories
{
    [TestClass]
    public class SqlArtifactRepositoryTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetProjectOrGetChildrenAsync_InvalidProjectId()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactRepository(cxn.Object);

            // Act
            await repository.GetProjectOrGetChildrenAsync(0, 1, 2);

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
            await repository.GetProjectOrGetChildrenAsync(1, 0, 2);

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
            await repository.GetProjectOrGetChildrenAsync(1, 2, 0);

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
            await repository.GetProjectOrGetChildrenAsync(1, 1, 2);

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
            await BaseTest(projectId, artifactId, userId, input, null);
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
            await BaseTest(projectId, artifactId, userId, input, null);
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
            await BaseTest(projectId, artifactId, userId, input, null);
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
            await BaseTest(projectId, artifactId, userId, input, null);
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
            await BaseTest(projectId, artifactId, userId, input, null);
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
                    PredefinedType = input[1].ItemTypePredefined.GetValueOrDefault().ToPredefinedType(),
                    OrderIndex = input[1].OrderIndex,
                    ParentId = input[1].ParentId,
                    Id = input[1].ItemId,
                    HasChildren = true,
                    Name = input[1].Name,
                    Permissions = input[1].DirectPermissions,
                    LockedDateTime = input[1].LockedByUserTime,
                    TypeId = input[1].ItemTypeId,
                    LockedByUserId = input[1].LockedByUserId,
                    Version = input[1].VersionsCount,
                    ProjectId = input[1].VersionProjectId,
                    Prefix = input[1].Prefix
                }
            };

            // Act and Assert
            await BaseTest(projectId, artifactId, userId, input, expected);
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
            var project = CreateArtifactVersion(1, 1, null, 33, int.MaxValue, RolePermissions.Read, false);
            input.Add(project);

            var expected = new List<Artifact>
            {
                new Artifact
                {
                    PredefinedType = input[1].ItemTypePredefined.GetValueOrDefault().ToPredefinedType(),
                    OrderIndex = input[1].OrderIndex,
                    ParentId = input[1].ParentId,
                    Id = input[1].ItemId,
                    HasChildren = true,
                    Name = input[1].Name,
                    Permissions = project.DirectPermissions,
                    LockedDateTime = input[1].LockedByUserTime,
                    TypeId = input[1].ItemTypeId,
                    LockedByUserId = input[1].LockedByUserId,
                    Version = input[1].VersionsCount,
                    ProjectId = input[1].VersionProjectId,
                    Prefix = input[1].Prefix
                }
            };

            // Act and Assert
            await BaseTest(projectId, artifactId, userId, input, expected);
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

            var ancestor1 = CreateArtifactVersion(9, 1, 8, 33, int.MaxValue, null, false);
            input.Add(ancestor1);

            var ancestor2 = CreateArtifactVersion(8, 1, 7, 33, int.MaxValue, null, false);
            input.Add(ancestor2);

            var ancestor3 = CreateArtifactVersion(7, 1, 6, 33, int.MaxValue, RolePermissions.Read, false);
            input.Add(ancestor3);

            var expected = new List<Artifact>
            {
                new Artifact
                {
                    PredefinedType = input[1].ItemTypePredefined.GetValueOrDefault().ToPredefinedType(),
                    OrderIndex = input[1].OrderIndex,
                    ParentId = input[1].ParentId,
                    Id = input[1].ItemId,
                    HasChildren = true,
                    Name = input[1].Name,
                    Permissions = ancestor3.DirectPermissions,
                    LockedDateTime = input[1].LockedByUserTime,
                    TypeId = input[1].ItemTypeId,
                    LockedByUserId = input[1].LockedByUserId,
                    Version = input[1].VersionsCount,
                    ProjectId = input[1].VersionProjectId,
                    Prefix = input[1].Prefix
                }
            };

            // Act and Assert
            await BaseTest(projectId, artifactId, userId, input, expected);
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
                    PredefinedType = input[2].ItemTypePredefined.GetValueOrDefault().ToPredefinedType(),
                    OrderIndex = input[2].OrderIndex,
                    ParentId = input[2].ParentId,
                    Id = input[2].ItemId,
                    HasChildren = true,
                    Name = input[2].Name,
                    Permissions = input[2].DirectPermissions,
                    LockedDateTime = input[2].LockedByUserTime,
                    TypeId = input[2].ItemTypeId,
                    LockedByUserId = input[2].LockedByUserId,
                    Version = input[2].VersionsCount / 2,
                    ProjectId = input[2].VersionProjectId,
                    Prefix = input[2].Prefix
                }
            };

            // Act and Assert
            await BaseTest(projectId, artifactId, userId, input, expected);
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
            await BaseTest(projectId, artifactId, userId, input, expected);
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
            await BaseTest(projectId, artifactId, userId, input, expected);
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
                    PredefinedType = input[1].ItemTypePredefined.GetValueOrDefault().ToPredefinedType(),
                    OrderIndex = input[1].OrderIndex,
                    ParentId = input[1].ParentId,
                    Id = input[1].ItemId,
                    HasChildren = true,
                    Name = input[1].Name,
                    Permissions = input[1].DirectPermissions,
                    LockedDateTime = input[1].LockedByUserTime,
                    TypeId = input[1].ItemTypeId,
                    LockedByUserId = input[1].LockedByUserId,
                    Version = input[1].VersionsCount,
                    ProjectId = input[1].VersionProjectId,
                    Prefix = input[1].Prefix
                }
            };

            // Act and Assert
            await BaseTest(projectId, artifactId, userId, input, expected);
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
                    PredefinedType = input[1].ItemTypePredefined.GetValueOrDefault().ToPredefinedType(),
                    OrderIndex = input[1].OrderIndex,
                    ParentId = input[1].ParentId,
                    Id = input[1].ItemId,
                    HasChildren = true,
                    Name = input[1].Name,
                    Permissions = input[1].DirectPermissions,
                    LockedDateTime = input[1].LockedByUserTime,
                    TypeId = input[1].ItemTypeId,
                    LockedByUserId = input[1].LockedByUserId,
                    Version = input[1].VersionsCount,
                    ProjectId = input[1].VersionProjectId,
                    Prefix = input[1].Prefix
                }
            };

            // Act and Assert
            await BaseTest(projectId, null, userId, input, expected, inputOrphans);
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
                    PredefinedType = inputOrphans[0].ItemTypePredefined.GetValueOrDefault().ToPredefinedType(),
                    OrderIndex = inputOrphans[0].OrderIndex,
                    ParentId = inputOrphans[0].ParentId,
                    Id = inputOrphans[0].ItemId,
                    HasChildren = true,
                    Name = inputOrphans[0].Name,
                    Permissions = inputOrphans[0].DirectPermissions,
                    LockedDateTime = inputOrphans[0].LockedByUserTime,
                    TypeId = inputOrphans[0].ItemTypeId,
                    LockedByUserId = inputOrphans[0].LockedByUserId,
                    Version = inputOrphans[0].VersionsCount,
                    ProjectId = inputOrphans[0].VersionProjectId,
                    Prefix = inputOrphans[0].Prefix
                }
            };

            // Act and Assert
            await BaseTest(projectId, null, userId, input, expected, inputOrphans);
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
                    PredefinedType = inputOrphans[1].ItemTypePredefined.GetValueOrDefault().ToPredefinedType(),
                    OrderIndex = inputOrphans[1].OrderIndex,
                    ParentId = inputOrphans[1].ParentId,
                    Id = inputOrphans[1].ItemId,
                    HasChildren = true,
                    Name = inputOrphans[1].Name,
                    Permissions = inputOrphans[1].DirectPermissions,
                    LockedDateTime = inputOrphans[1].LockedByUserTime,
                    TypeId = inputOrphans[1].ItemTypeId,
                    LockedByUserId = inputOrphans[1].LockedByUserId,
                    Version = inputOrphans[1].VersionsCount / 2,
                    ProjectId = inputOrphans[1].VersionProjectId,
                    Prefix = inputOrphans[1].Prefix
                }
            };

            // Act and Assert
            await BaseTest(projectId, null, userId, input, expected, inputOrphans);
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
                    PredefinedType = inputOrphans[0].ItemTypePredefined.GetValueOrDefault().ToPredefinedType(),
                    OrderIndex = inputOrphans[0].OrderIndex,
                    ParentId = inputOrphans[0].ParentId,
                    Id = inputOrphans[0].ItemId,
                    HasChildren = true,
                    Name = inputOrphans[0].Name,
                    Permissions = inputOrphans[0].DirectPermissions,
                    LockedDateTime = inputOrphans[0].LockedByUserTime,
                    TypeId = inputOrphans[0].ItemTypeId,
                    LockedByUserId = inputOrphans[0].LockedByUserId,
                    Version = inputOrphans[0].VersionsCount,
                    ProjectId = inputOrphans[0].VersionProjectId,
                    Prefix = inputOrphans[0].Prefix
                }
            };

            // Act and Assert
            await BaseTest(projectId, null, userId, input, expected, inputOrphans);
        }

        private async Task BaseTest(int projectId, int? artifactId, int userId,
                                    List<ArtifactVersion> input, List<Artifact> expected,
                                    List<ArtifactVersion> inputOrphans = null)
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactRepository(cxn.Object);
            cxn.SetupQueryAsync("GetArtifactChildren", new Dictionary<string, object> { { "projectId", projectId }, { "artifactId", artifactId ?? projectId }, { "userId", userId } }, input);
            if(artifactId == null)
                cxn.SetupQueryAsync("GetProjectOrphans", new Dictionary<string, object> { { "projectId", projectId }, { "userId", userId } }, inputOrphans);
            // Act
            var actual = await repository.GetProjectOrGetChildrenAsync(projectId, artifactId, userId);

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


            for(int i = 0; i < expected.Count; i++)
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
                if (e.LockedByUserId != a.LockedByUserId)
                {
                    errorMessage = I18NHelper.FormatInvariant(template, i, "LockedByUserId", e.LockedByUserId, a.LockedByUserId);
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
                if (e.TypeId != a.TypeId)
                {
                    errorMessage = I18NHelper.FormatInvariant(template, i, "TypeId", e.TypeId, a.TypeId);
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
                CreateArtifactVersion(10, 1, 1, 99, int.MaxValue, RolePermissions.Read, false),
                CreateArtifactVersion(20, 1, 10, 99, int.MaxValue, RolePermissions.Read, true,
                                        name: "parent",
                                        orderIndex: 10,
                                        itemTypePredefined: 0x1000 | 6,
                                        itemTypeId: 88,
                                        prefix: "PF",
                                        lockedByUserId: null,
                                        lockedByUserTime: null,
                                        versionsCount: 22),
                CreateArtifactVersion(20, 1, 10, 1, 1, RolePermissions.Read, true,
                                        name: "parent (draft)",
                                        orderIndex: 10,
                                        itemTypePredefined: 0x1000 | 6,
                                        itemTypeId: 88,
                                        prefix: "PF",
                                        lockedByUserId: 1,
                                        lockedByUserTime: DateTime.Now,
                                        versionsCount: 22),
                CreateArtifactVersion(30, 1, 20, 99, int.MaxValue, RolePermissions.Read, false)
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
                                                        int? itemTypePredefined = null,
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
    }
}

