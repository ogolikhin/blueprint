using ArtifactStore.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtifactStore.Repositories
{
    [TestClass]
    public class SqlArtifactRelationshipsRepositoryTests
    {
        private SqlConnectionWrapperMock cxn;
        private IRelationshipsRepository relationshipsRepository;
        [TestInitialize]
        public void initialize()
        {
            cxn = new SqlConnectionWrapperMock();
            relationshipsRepository = new SqlRelationshipsRepository(cxn.Object);
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

            cxn.SetupQueryAsync("GetRelationshipLinkInfo", new Dictionary<string, object> { { "itemId", itemId }, { "userId", userId }, { "addDrafts", addDrafts } }, mockLinkInfoList);

            // Act
            var result = await relationshipsRepository.GetRelationships(itemId, userId, addDrafts);

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

            cxn.SetupQueryAsync("GetRelationshipLinkInfo", new Dictionary<string, object> { { "itemId", itemId }, { "userId", userId }, { "addDrafts", addDrafts } }, mockLinkInfoList);

            // Act
            var result = await relationshipsRepository.GetRelationships(itemId, userId, addDrafts);

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

            cxn.SetupQueryAsync("GetRelationshipLinkInfo", new Dictionary<string, object> { { "itemId", itemId }, { "userId", userId }, { "addDrafts", addDrafts } }, mockLinkInfoList);

            // Act
            var result = await relationshipsRepository.GetRelationships(itemId, userId, addDrafts);

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

            cxn.SetupQueryAsync("GetRelationshipLinkInfo", new Dictionary<string, object> { { "itemId", itemId }, { "userId", userId }, { "addDrafts", addDrafts } }, mockLinkInfoList);

            // Act
            var result = await relationshipsRepository.GetRelationships(itemId, userId, addDrafts);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.ManualTraces.Count);
            Assert.AreEqual(LinkType.Manual, result.ManualTraces[0].TraceType);
            Assert.AreEqual(TraceDirection.TwoWay, result.ManualTraces[0].TraceDirection);
        }

    }
}
