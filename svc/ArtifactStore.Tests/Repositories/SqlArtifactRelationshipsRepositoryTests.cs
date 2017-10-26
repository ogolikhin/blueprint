using ArtifactStore.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using System;

namespace ArtifactStore.Repositories
{
    [TestClass]
    public class SqlArtifactRelationshipsRepositoryTests
    {
        private SqlConnectionWrapperMock _cxn;
        private IRelationshipsRepository _relationshipsRepository;
        private Mock<ISqlItemInfoRepository> _itemInfoRepositoryMock;
        private Mock<IArtifactPermissionsRepository> _artifactPermissionsRepositoryMock;

        [TestInitialize]
        public void Initialize()
        {
            _cxn = new SqlConnectionWrapperMock();
            _itemInfoRepositoryMock = new Mock<ISqlItemInfoRepository>();
            _artifactPermissionsRepositoryMock = new Mock<IArtifactPermissionsRepository>();
            _relationshipsRepository = new SqlRelationshipsRepository(_cxn.Object, _itemInfoRepositoryMock.Object, _artifactPermissionsRepositoryMock.Object);
        }

        [TestMethod]
        public async Task GetRelationships_ReturnsManualAndOtherTraces_Success()
        {
            // Arrange
            int itemId = 1;
            int userId = 1;
            bool addDrafts = true;
            bool allLinks = false;

            var mockLinkInfoList = new List<LinkInfo>();
            mockLinkInfoList.Add(new LinkInfo { DestinationArtifactId = 2, DestinationItemId = 2, DestinationProjectId = 0, IsSuspect = false, LinkType = LinkType.Manual, SourceArtifactId = 1, SourceItemId = 1, SourceProjectId = 0 });
            mockLinkInfoList.Add(new LinkInfo { DestinationArtifactId = 2, DestinationItemId = 2, DestinationProjectId = 0, IsSuspect = false, LinkType = LinkType.Association, SourceArtifactId = 1, SourceItemId = 1, SourceProjectId = 0 });

            _cxn.SetupQueryAsync("GetRelationshipLinkInfo", new Dictionary<string, object> { { "itemId", itemId }, { "userId", userId }, { "addDrafts", addDrafts } }, mockLinkInfoList);

            // Act
            var result = await _relationshipsRepository.GetRelationships(itemId, userId, It.IsAny<int?>(), addDrafts, allLinks, It.IsAny<int?>());

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.ManualTraces.Count);
            Assert.AreEqual(LinkType.Manual, result.ManualTraces[0].TraceType);
            Assert.AreEqual(1, result.OtherTraces.Count);
            Assert.AreEqual(LinkType.Association, result.OtherTraces[0].TraceType);
        }

        [TestMethod]
        public async Task GetRelationships_ToRelationships_Success()
        {
            // Arrange
            int itemId = 1;
            int userId = 1;
            bool addDrafts = true;
            int destinationProjectId = 3;

            var mockLinkInfoList = new List<LinkInfo>();
            mockLinkInfoList.Add(new LinkInfo { DestinationArtifactId = 2, DestinationItemId = 2, DestinationProjectId = destinationProjectId, IsSuspect = false, LinkType = LinkType.Manual, SourceArtifactId = 1, SourceItemId = 1, SourceProjectId = 0 });

            _cxn.SetupQueryAsync("GetRelationshipLinkInfo", new Dictionary<string, object> { { "itemId", itemId }, { "userId", userId }, { "addDrafts", addDrafts } }, mockLinkInfoList);

            // Act
            var result = await _relationshipsRepository.GetRelationships(itemId, userId, It.IsAny<int?>(), addDrafts, false, It.IsAny<int?>());

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.ManualTraces.Count);
            Assert.AreEqual(LinkType.Manual, result.ManualTraces[0].TraceType);
            Assert.AreEqual(TraceDirection.To, result.ManualTraces[0].TraceDirection);
            Assert.AreEqual(destinationProjectId, result.ManualTraces[0].ProjectId);
        }

        [TestMethod]
        public async Task GetRelationships_FromRelationships_Success()
        {
            // Arrange
            int itemId = 1;
            int userId = 1;
            int sourceProjectId = 4;
            bool addDrafts = true;
            bool allLinks = false;

            var mockLinkInfoList = new List<LinkInfo>();
            mockLinkInfoList.Add(new LinkInfo { DestinationArtifactId = 1, DestinationItemId = 1, DestinationProjectId = 0, IsSuspect = false, LinkType = LinkType.Manual, SourceArtifactId = 2, SourceItemId = 2, SourceProjectId = sourceProjectId });

            _cxn.SetupQueryAsync("GetRelationshipLinkInfo", new Dictionary<string, object> { { "itemId", itemId }, { "userId", userId }, { "addDrafts", addDrafts } }, mockLinkInfoList);

            // Act
            var result = await _relationshipsRepository.GetRelationships(itemId, userId, It.IsAny<int?>(), addDrafts, allLinks, It.IsAny<int?>());

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.ManualTraces.Count);
            Assert.AreEqual(LinkType.Manual, result.ManualTraces[0].TraceType);
            Assert.AreEqual(TraceDirection.From, result.ManualTraces[0].TraceDirection);
            Assert.AreEqual(sourceProjectId, result.ManualTraces[0].ProjectId);
        }

        [TestMethod]
        public async Task GetRelationships_TwoWayRelationships_Success()
        {
            // Arrange
            int itemId = 1;
            int userId = 1;
            bool addDrafts = true;
            bool allLinks = false;

            var mockLinkInfoList = new List<LinkInfo>();
            mockLinkInfoList.Add(new LinkInfo { DestinationArtifactId = 2, DestinationItemId = 2, DestinationProjectId = 0, IsSuspect = false, LinkType = LinkType.Manual, SourceArtifactId = 1, SourceItemId = 1, SourceProjectId = 0 });
            mockLinkInfoList.Add(new LinkInfo { DestinationArtifactId = 1, DestinationItemId = 1, DestinationProjectId = 0, IsSuspect = false, LinkType = LinkType.Manual, SourceArtifactId = 2, SourceItemId = 2, SourceProjectId = 0 });

            _cxn.SetupQueryAsync("GetRelationshipLinkInfo", new Dictionary<string, object> { { "itemId", itemId }, { "userId", userId }, { "addDrafts", addDrafts } }, mockLinkInfoList);

            // Act
            var result = await _relationshipsRepository.GetRelationships(itemId, userId, It.IsAny<int?>(), addDrafts, allLinks, It.IsAny<int?>());

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.ManualTraces.Count);
            Assert.AreEqual(LinkType.Manual, result.ManualTraces[0].TraceType);
            Assert.AreEqual(TraceDirection.TwoWay, result.ManualTraces[0].TraceDirection);
        }
        [TestMethod]
        public async Task GetRelationships_ToOtherRelationshipSubartifact_Success()
        {
            // Arrange
            int itemId = 1;
            int userId = 1;
            bool addDrafts = true;
            bool allLinks = false;

            var mockLinkInfoList = new List<LinkInfo>();
            mockLinkInfoList.Add(new LinkInfo { DestinationArtifactId = 2, DestinationItemId = 3, DestinationProjectId = 0, IsSuspect = false, LinkType = LinkType.Manual, SourceArtifactId = 1, SourceItemId = 1, SourceProjectId = 0 });

            _cxn.SetupQueryAsync("GetRelationshipLinkInfo", new Dictionary<string, object> { { "itemId", itemId }, { "userId", userId }, { "addDrafts", addDrafts } }, mockLinkInfoList);

            // Act
            var result = await _relationshipsRepository.GetRelationships(itemId, userId, It.IsAny<int?>(), addDrafts, allLinks, It.IsAny<int?>());

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.ManualTraces.Count);
            Assert.AreEqual(LinkType.Manual, result.ManualTraces[0].TraceType);
            Assert.AreEqual(TraceDirection.To, result.ManualTraces[0].TraceDirection);
            Assert.AreEqual(3, result.ManualTraces[0].ItemId);
            Assert.AreEqual(2, result.ManualTraces[0].ArtifactId);
        }
        [TestMethod]
        public async Task GetRelationships_ArtifactVersionExists_Success()
        {
            // Arrange
            const int artifactId = 1;
            const int userId = 2;
            const int versionId = 3;
            const int revisionId = 999;
            const bool addDrafts = false;
            bool allLinks = false;

            _itemInfoRepositoryMock.Setup(m => m.GetRevisionId(artifactId, userId, versionId, null)).ReturnsAsync(revisionId);

            var mockLinkInfoList = new List<LinkInfo>
            {
                new LinkInfo
                {
                    DestinationArtifactId = 2,
                    DestinationItemId = 2,
                    DestinationProjectId = 0,
                    IsSuspect = false,
                    LinkType = LinkType.Manual,
                    SourceArtifactId = 1,
                    SourceItemId = 1,
                    SourceProjectId = 0
                }
            };

            _cxn.SetupQueryAsync("GetRelationshipLinkInfo", new Dictionary<string, object> { { "itemId", artifactId }, { "userId", userId }, { "addDrafts", addDrafts }, { "revisionId", revisionId } }, mockLinkInfoList);

            // Act
            var result = await _relationshipsRepository.GetRelationships(artifactId, userId, It.IsAny<int?>(), addDrafts, allLinks, versionId);

            // Assert
            Assert.AreEqual(1, result.ManualTraces.Count);
            Assert.AreEqual(LinkType.Manual, result.ManualTraces[0].TraceType);
        }

        [ExpectedException(typeof(ResourceNotFoundException))]
        [TestMethod]
        public async Task GetRelationshipExtendedInfo_ArtifactDoesNotExistForRevision_ThrowException()
        {
            // Arrange
            const int artifactId = 1;
            const int userId = 2;
            const bool isDeleted = false;

            _cxn.SetupQueryAsync("GetArtifactNavigationPath", new Dictionary<string, object> { { "artifactId", artifactId }, { "userId", userId } },
                new List<ItemIdItemNameParentId>());

            try
            {
                // Act
                await _relationshipsRepository.GetRelationshipExtendedInfo(artifactId, userId, null, isDeleted);
            }
            catch (ResourceNotFoundException e)
            {
                Assert.AreEqual(e.ErrorCode, ErrorCodes.ResourceNotFound);
                throw;
            }

            // Assert
        }

        [TestMethod]
        public async Task GetRelationshipExtendedInfo_Success()
        {
            // Arrange
            const int artifactId = 1;
            const int userId = 2;
            const int versionId = 3;
            const bool isDeleted = false;
            const string description = "artifact description";

            _itemInfoRepositoryMock.Setup(m => m.GetRevisionId(artifactId, userId, versionId, null)).ReturnsAsync(int.MaxValue);

            var pathToRoot = new List<ItemIdItemNameParentId>
            {
                new ItemIdItemNameParentId { ItemId = 1, ParentId = 10, Name = "artifact" },
                new ItemIdItemNameParentId { ItemId = 10, ParentId = 100, Name = "folder" },
                new ItemIdItemNameParentId { ItemId = 100, ParentId = null, Name = "project" }
            };
            _cxn.SetupQueryAsync("GetArtifactNavigationPath", new Dictionary<string, object> { { "artifactId", artifactId }, { "userId", userId } }, pathToRoot);

            _itemInfoRepositoryMock.Setup(m => m.GetItemDescription(artifactId, userId, true, int.MaxValue)).ReturnsAsync(description);

            var descriptionResult = new List<string> { description };
            _cxn.SetupQueryAsync("GetItemDescription", new Dictionary<string, object> { { "itemId", artifactId }, { "userId", userId } }, descriptionResult);

            // Act
            var actual = await _relationshipsRepository.GetRelationshipExtendedInfo(artifactId, userId, null, isDeleted);

            // Assert
            Assert.AreEqual(artifactId, actual.ArtifactId);
            Assert.AreEqual(description, actual.Description);
            Assert.AreEqual(pathToRoot.Count, actual.PathToProject.Count());
            Assert.AreEqual(pathToRoot[0].ItemId, actual.PathToProject.ToList()[2].ItemId);
            Assert.AreEqual(pathToRoot[0].ParentId, actual.PathToProject.ToList()[2].ParentId);
            Assert.AreEqual(pathToRoot[0].Name, actual.PathToProject.ToList()[2].Name);
            Assert.AreEqual(pathToRoot[1].ItemId, actual.PathToProject.ToList()[1].ItemId);
            Assert.AreEqual(pathToRoot[1].ParentId, actual.PathToProject.ToList()[1].ParentId);
            Assert.AreEqual(pathToRoot[1].Name, actual.PathToProject.ToList()[1].Name);
            Assert.AreEqual(pathToRoot[2].ItemId, actual.PathToProject.ToList()[0].ItemId);
            Assert.AreEqual(pathToRoot[2].ParentId, actual.PathToProject.ToList()[0].ParentId);
            Assert.AreEqual(pathToRoot[2].Name, actual.PathToProject.ToList()[0].Name);
        }


        [TestMethod]
        public async Task GetReviewRelationships_SingleReviewLink_Success()
        {
            // Arrange
            int itemId = 1;
            int userId = 1;
            bool addDrafts = true;
            int destinationProjectId = 3;

            var mockLinkInfoList = new List<LinkInfo>();
            mockLinkInfoList.Add(new LinkInfo { DestinationArtifactId = 1,
                                                DestinationItemId = 1,
                                                DestinationProjectId = destinationProjectId,
                                                IsSuspect = false, LinkType = LinkType.ReviewPackageReference,
                                                SourceArtifactId = 2,
                                                SourceItemId = 2,
                                                SourceProjectId = 0 });

            _cxn.SetupQueryAsync("GetRelationshipLinkInfo",
                                new Dictionary<string, object> { { "itemId", itemId }, { "userId", userId }, { "addDrafts", addDrafts } }, mockLinkInfoList);

            _itemInfoRepositoryMock.Setup(a => a.GetItemsRawDataCreatedDate(userId, It.IsAny<IEnumerable<int>>(), It.IsAny<bool>(), It.IsAny<int>()))
                .ReturnsAsync(new List<ItemRawDataCreatedDate> {
                    new ItemRawDataCreatedDate {
                        CreatedDateTime = new DateTime(),
                        ItemId = 2,
                        RawData = ""
                    }
                });

            _itemInfoRepositoryMock.Setup(a => a.GetItemsDetails(userId, It.IsAny<IEnumerable<int>>(), It.IsAny<bool>(), It.IsAny<int>()))
                .ReturnsAsync(new List<ItemDetails> {
                    new ItemDetails {
                        HolderId = 2,
                        Name = "Some Review",
                        Prefix = "ReviewPrefix"
                    }
                });
            var permisionDictionary = new Dictionary<int, RolePermissions>();
            permisionDictionary.Add(2, RolePermissions.Read);
            _artifactPermissionsRepositoryMock.Setup(m => m.GetArtifactPermissions(It.IsAny<List<int>>(), userId, false, int.MaxValue, true)).ReturnsAsync(permisionDictionary);

            // Act
            var result = await _relationshipsRepository.GetReviewRelationships(itemId, userId, addDrafts);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.ReviewArtifacts.Count);
            Assert.AreEqual(2, result.ReviewArtifacts[0].ItemId);
            Assert.AreEqual("Some Review", result.ReviewArtifacts[0].ItemName);
            Assert.AreEqual("ReviewPrefix", result.ReviewArtifacts[0].ItemTypePrefix);
        }

        [TestMethod]
        public async Task GetRelationships_ActorInheritsFrom_LinkToDeletedActor_Empty()
        {
            // Arrange
            const int artifactId = 1;
            const int userId = 1;
            const int versionId = 3;
            const int revisionId = 99999;
            const bool addDrafts = false;
            bool allLinks = false;

            var actorInheritsFromLink = new LinkInfo
            {
                DestinationArtifactId = 2,
                DestinationItemId = 2,
                DestinationProjectId = 0,
                IsSuspect = false,
                LinkType = LinkType.ActorInheritsFrom,
                SourceArtifactId = 1,
                SourceItemId = 1,
                SourceProjectId = 0
            };

            var links = new List<LinkInfo>
            {
                actorInheritsFromLink
            };

            _cxn.SetupQueryAsync("GetRelationshipLinkInfo", new Dictionary<string, object> { { "itemId", artifactId }, { "userId", userId }, { "addDrafts", addDrafts }, { "revisionId", revisionId } }, links);

            _artifactPermissionsRepositoryMock.Setup(p => p.GetItemInfo(actorInheritsFromLink.DestinationArtifactId, userId, addDrafts, revisionId)).ReturnsAsync((ItemInfo)null);

            _itemInfoRepositoryMock.Setup(m => m.GetRevisionId(artifactId, userId, versionId, null)).ReturnsAsync(revisionId);

            // Act
            var relationships = await _relationshipsRepository.GetRelationships(artifactId, userId, It.IsAny<int?>(), addDrafts, allLinks, versionId);
            Assert.AreEqual(0, relationships.OtherTraces.Count);
        }

        [TestMethod]
        public async Task GetRelationships_ActorInheritsFrom_LinkToAliveActor_OneResult()
        {
            // Arrange
            const int artifactId = 1;
            const int userId = 1;
            const int versionId = 3;
            const int revisionId = 99999;
            const bool addDrafts = true;
            bool allLinks = false;

            var actorInheritsFromLink = new LinkInfo
            {
                DestinationArtifactId = 2,
                DestinationItemId = 2,
                DestinationProjectId = 0,
                IsSuspect = false,
                LinkType = LinkType.ActorInheritsFrom,
                SourceArtifactId = 1,
                SourceItemId = 1,
                SourceProjectId = 0
            };

            var baseActor = new ItemInfo
            {
                ArtifactId = actorInheritsFromLink.DestinationArtifactId,
                ItemId = actorInheritsFromLink.DestinationArtifactId,
                ProjectId = 1
            };

            var links = new List<LinkInfo>
            {
                actorInheritsFromLink
            };

            _cxn.SetupQueryAsync("GetRelationshipLinkInfo", new Dictionary<string, object> { { "itemId", artifactId }, { "userId", userId }, { "addDrafts", addDrafts }, { "revisionId", revisionId } }, links);

            _artifactPermissionsRepositoryMock.Setup(p => p.GetItemInfo(actorInheritsFromLink.DestinationArtifactId, userId, addDrafts, revisionId)).ReturnsAsync(baseActor);

            _itemInfoRepositoryMock.Setup(m => m.GetRevisionId(artifactId, userId, versionId, null)).ReturnsAsync(revisionId);

            // Act
            var relationships = await _relationshipsRepository.GetRelationships(artifactId, userId, It.IsAny<int?>(), addDrafts, allLinks, versionId);
            Assert.AreEqual(1, relationships.OtherTraces.Count);
            Assert.AreEqual(actorInheritsFromLink.DestinationArtifactId, relationships.OtherTraces.ElementAt(0).ArtifactId);
        }
    }
}
