using ArtifactStore.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace ArtifactStore.Repositories
{
    [TestClass]
    public class SqlArtifactRelationshipsRepositoryTests
    {
        private SqlConnectionWrapperMock _cxn;
        private IRelationshipsRepository _relationshipsRepository;
        private Mock<ISqlItemInfoRepository> _itemInfoRepositoryMock;

        [TestInitialize]
        public void Initialize()
        {
            _cxn = new SqlConnectionWrapperMock();
            _itemInfoRepositoryMock = new Mock<ISqlItemInfoRepository>();
            _relationshipsRepository = new SqlRelationshipsRepository(_cxn.Object, _itemInfoRepositoryMock.Object);
        }

        [TestMethod]
        public async Task GetRelationships_ReturnsManualAndOtherTraces_Success()
        {
            // Arrange
            int itemId = 1;
            int userId = 1;
            bool addDrafts = true;

            var mockLinkInfoList = new List<LinkInfo>();
            mockLinkInfoList.Add(new LinkInfo { DestinationArtifactId = 2, DestinationItemId = 2, DestinationProjectId = 0, IsSuspect = false, LinkType = LinkType.Manual, SourceArtifactId = 1, SourceItemId = 1, SourceProjectId = 0 });
            mockLinkInfoList.Add(new LinkInfo { DestinationArtifactId = 2, DestinationItemId = 2, DestinationProjectId = 0, IsSuspect = false, LinkType = LinkType.Association, SourceArtifactId = 1, SourceItemId = 1, SourceProjectId = 0 });

            _cxn.SetupQueryAsync("GetRelationshipLinkInfo", new Dictionary<string, object> { { "itemId", itemId }, { "userId", userId }, { "addDrafts", addDrafts } }, mockLinkInfoList);

            // Act
            var result = await _relationshipsRepository.GetRelationships(itemId, userId, It.IsAny<int?>(), addDrafts, It.IsAny<int?>());

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

            var mockLinkInfoList = new List<LinkInfo>();
            mockLinkInfoList.Add(new LinkInfo { DestinationArtifactId = 2, DestinationItemId = 2, DestinationProjectId = 0, IsSuspect = false, LinkType = LinkType.Manual, SourceArtifactId = 1, SourceItemId = 1, SourceProjectId = 0 });

            _cxn.SetupQueryAsync("GetRelationshipLinkInfo", new Dictionary<string, object> { { "itemId", itemId }, { "userId", userId }, { "addDrafts", addDrafts } }, mockLinkInfoList);

            // Act
            var result = await _relationshipsRepository.GetRelationships(itemId, userId, It.IsAny<int?>(), addDrafts, It.IsAny<int?>());

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.ManualTraces.Count);
            Assert.AreEqual(LinkType.Manual, result.ManualTraces[0].TraceType);
            Assert.AreEqual(TraceDirection.To, result.ManualTraces[0].TraceDirection);
        }

        [TestMethod]
        public async Task GetRelationships_FromRelationships_Success()
        {
            // Arrange
            int itemId = 1;
            int userId = 1;
            bool addDrafts = true;

            var mockLinkInfoList = new List<LinkInfo>();
            mockLinkInfoList.Add(new LinkInfo { DestinationArtifactId = 1, DestinationItemId = 1, DestinationProjectId = 0, IsSuspect = false, LinkType = LinkType.Manual, SourceArtifactId = 2, SourceItemId = 2, SourceProjectId = 0 });

            _cxn.SetupQueryAsync("GetRelationshipLinkInfo", new Dictionary<string, object> { { "itemId", itemId }, { "userId", userId }, { "addDrafts", addDrafts } }, mockLinkInfoList);

            // Act
            var result = await _relationshipsRepository.GetRelationships(itemId, userId, It.IsAny<int?>(), addDrafts, It.IsAny<int?>());

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.ManualTraces.Count);
            Assert.AreEqual(LinkType.Manual, result.ManualTraces[0].TraceType);
            Assert.AreEqual(TraceDirection.From, result.ManualTraces[0].TraceDirection);
        }

        [TestMethod]
        public async Task GetRelationships_TwoWayRelationships_Success()
        {
            // Arrange
            int itemId = 1;
            int userId = 1;
            bool addDrafts = true;

            var mockLinkInfoList = new List<LinkInfo>();
            mockLinkInfoList.Add(new LinkInfo { DestinationArtifactId = 2, DestinationItemId = 2, DestinationProjectId = 0, IsSuspect = false, LinkType = LinkType.Manual, SourceArtifactId = 1, SourceItemId = 1, SourceProjectId = 0 });
            mockLinkInfoList.Add(new LinkInfo { DestinationArtifactId = 1, DestinationItemId = 1, DestinationProjectId = 0, IsSuspect = false, LinkType = LinkType.Manual, SourceArtifactId = 2, SourceItemId = 2, SourceProjectId = 0 });

            _cxn.SetupQueryAsync("GetRelationshipLinkInfo", new Dictionary<string, object> { { "itemId", itemId }, { "userId", userId }, { "addDrafts", addDrafts } }, mockLinkInfoList);

            // Act
            var result = await _relationshipsRepository.GetRelationships(itemId, userId, It.IsAny<int?>(), addDrafts, It.IsAny<int?>());

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

            var mockLinkInfoList = new List<LinkInfo>();
            mockLinkInfoList.Add(new LinkInfo { DestinationArtifactId = 2, DestinationItemId = 3, DestinationProjectId = 0, IsSuspect = false, LinkType = LinkType.Manual, SourceArtifactId = 1, SourceItemId = 1, SourceProjectId = 0 });

            _cxn.SetupQueryAsync("GetRelationshipLinkInfo", new Dictionary<string, object> { { "itemId", itemId }, { "userId", userId }, { "addDrafts", addDrafts } }, mockLinkInfoList);

            // Act
            var result = await _relationshipsRepository.GetRelationships(itemId, userId, It.IsAny<int?>(), addDrafts, It.IsAny<int?>());

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.ManualTraces.Count);
            Assert.AreEqual(LinkType.Manual, result.ManualTraces[0].TraceType);
            Assert.AreEqual(TraceDirection.To, result.ManualTraces[0].TraceDirection);
            Assert.AreEqual(3, result.ManualTraces[0].ItemId);
            Assert.AreEqual(2, result.ManualTraces[0].ArtifactId);
        }

        [ExpectedException(typeof(ResourceNotFoundException))]
        [TestMethod]
        public async Task GetRelationships_ArtifactVersionNotFound_ThrowExceprion()
        {
            //Arrange
            const int artifactId = 1;
            const int userId = 2;
            const int versionId = 3;
            const int revisionId = 0;
            const bool addDrafts = false;

            _itemInfoRepositoryMock.Setup(m => m.GetRevisionIdByVersionIndex(artifactId, versionId)).ReturnsAsync(revisionId);

            try
            {
                //Act
                await _relationshipsRepository.GetRelationships(artifactId, userId, It.IsAny<int?>(), addDrafts, versionId);
            }
            catch (ResourceNotFoundException ex)
            {
                //Assert
                Assert.AreEqual(ErrorCodes.ResourceNotFound, ex.ErrorCode);
                throw;
            }
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

            _itemInfoRepositoryMock.Setup(m => m.GetRevisionIdByVersionIndex(artifactId, versionId)).ReturnsAsync(revisionId);

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
            var result = await _relationshipsRepository.GetRelationships(artifactId, userId, It.IsAny<int?>(), addDrafts, versionId);

            //Assert
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
            const int revisionId = 999;
            const bool addDrafts = false;

            _cxn.SetupQueryAsync("GetPathIdsNamesToProject", new Dictionary<string, object> { { "artifactId", artifactId }, { "userId", userId }, { "addDrafts", addDrafts }, { "revisionId", revisionId } }
                , new List<ItemIdItemNameParentId>());

            try
            {
                // Act
                await _relationshipsRepository.GetRelationshipExtendedInfo(artifactId, userId, addDrafts, revisionId);
            }
            catch (ResourceNotFoundException e)
            {
                Assert.AreEqual(e.ErrorCode, ErrorCodes.ResourceNotFound);
                throw;
            }

            //Assert
        }

        [TestMethod]
        public async Task GetRelationshipExtendedInfo_Success()
        {
            // Arrange
            const int artifactId = 1;
            const int userId = 2;
            const int revisionId = 999;
            const bool addDrafts = false;
            const string description = "artifact description";

            var pathToRoot = new List<ItemIdItemNameParentId>
            {
                new ItemIdItemNameParentId { ItemId = 1, ParentId = 10, ItemName = "artifact"},
                new ItemIdItemNameParentId { ItemId = 10, ParentId = 100, ItemName = "folder"},
                new ItemIdItemNameParentId { ItemId = 100, ParentId = null, ItemName = "project"}
            };
            _cxn.SetupQueryAsync("GetPathIdsNamesToProject", new Dictionary<string, object> { { "artifactId", artifactId }, { "userId", userId }, { "addDrafts", addDrafts }, { "revisionId", revisionId } }, pathToRoot);

            var descriptionResult = new List<string> { description };
            _cxn.SetupQueryAsync("GetItemDescription", new Dictionary<string, object> { { "itemId", artifactId }, { "userId", userId }, { "addDrafts", addDrafts }, { "revisionId", revisionId } }, descriptionResult);
            
            // Act
            var actual = await _relationshipsRepository.GetRelationshipExtendedInfo(artifactId, userId, addDrafts, revisionId);

            //Assert
            Assert.AreEqual(artifactId, actual.ArtifactId);
            Assert.AreEqual(description, actual.Description);
            Assert.AreEqual(pathToRoot.Count, actual.PathToProject.Count());
            Assert.AreEqual(pathToRoot[0].ItemId, actual.PathToProject.ToList()[2].ItemId);
            Assert.AreEqual(pathToRoot[0].ParentId, actual.PathToProject.ToList()[2].ParentId);
            Assert.AreEqual(pathToRoot[0].ItemName, actual.PathToProject.ToList()[2].ItemName);
            Assert.AreEqual(pathToRoot[1].ItemId, actual.PathToProject.ToList()[1].ItemId);
            Assert.AreEqual(pathToRoot[1].ParentId, actual.PathToProject.ToList()[1].ParentId);
            Assert.AreEqual(pathToRoot[1].ItemName, actual.PathToProject.ToList()[1].ItemName);
            Assert.AreEqual(pathToRoot[2].ItemId, actual.PathToProject.ToList()[0].ItemId);
            Assert.AreEqual(pathToRoot[2].ParentId, actual.PathToProject.ToList()[0].ParentId);
            Assert.AreEqual(pathToRoot[2].ItemName, actual.PathToProject.ToList()[0].ItemName);
        }
    }
}
