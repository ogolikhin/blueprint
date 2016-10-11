using ArtifactStore.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtifactStore.Repositories
{
    [TestClass]
    public class SqlAttachmentsRepositoryTests
    {
        private IUsersRepository mockUserRepository;
        private SqlConnectionWrapperMock cxn;
        [TestInitialize]
        public void initialize()
        {
            mockUserRepository = new SqlUserRepositoryMock();
            cxn = new SqlConnectionWrapperMock();
        }

        [TestMethod]
        public async Task GetAttachmentsAndDocumentReferences_NotSubArtifactAddDrafts_ResultsReturned()
        {
            // Arrange
            int artifactId = 1;
            int userId = 1;
            int? subArtifactId = null;
            bool addDrafts = true;

            cxn.SetupQueryAsync("GetItemAttachments", new Dictionary<string, object> { { "itemId", artifactId }, { "userId", userId }, { "addDrafts", addDrafts } }, new List<Attachment> { new Attachment { FileName = "Test File Name", FileGuid = new System.Guid() } });
            cxn.SetupQueryAsync("GetDocumentReferenceArtifacts", new Dictionary<string, object> { { "itemId", artifactId }, { "userId", userId }, { "addDrafts", addDrafts } }, new List<DocumentReference> { new DocumentReference { UserId = userId, ArtifactId = artifactId } });
            cxn.SetupQueryAsync("GetDocumentArtifactInfos", new Dictionary<string, object> { { "artifactIds", SqlConnectionWrapper.ToDataTable(new List<int> { artifactId }, "Int32Collection", "Int32Value") }, { "addDrafts", addDrafts }}, new List<LinkedArtifactInfo> { new LinkedArtifactInfo { ArtifactId = artifactId, ArtifactName = "Test Document Name" } });
            var repository = new SqlAttachmentsRepository(cxn.Object, mockUserRepository);
            // Act
            var result = await repository.GetAttachmentsAndDocumentReferences(artifactId, userId, null, subArtifactId, addDrafts);

            // Assert
            cxn.Verify();
            Assert.AreEqual(result.ArtifactId, 1);
            Assert.AreEqual(result.SubartifactId, null);
            Assert.AreEqual(result.Attachments.Count, 1);
            Assert.AreEqual(result.DocumentReferences.Count, 1);
        }

        [TestMethod]
        public async Task GetAttachmentsAndDocumentReferences_SubArtifactDontAddDrafts_ResultsReturned()
        {
            // Arrange
            int artifactId = 1;
            int userId = 1;
            int? subArtifactId = 2;
            bool addDrafts = false;

            var cxn = new SqlConnectionWrapperMock();
            cxn.SetupQueryAsync("GetItemAttachments", new Dictionary<string, object> { { "itemId", subArtifactId }, { "userId", userId }, { "addDrafts", addDrafts } }, new List<Attachment> { new Attachment { FileName = "Test File Name", FileGuid = new System.Guid() } });
            cxn.SetupQueryAsync("GetDocumentReferenceArtifacts", new Dictionary<string, object> { { "itemId", subArtifactId }, { "userId", userId }, { "addDrafts", addDrafts } }, new List<DocumentReference> { new DocumentReference { UserId = userId, ArtifactId = artifactId } });
            cxn.SetupQueryAsync("GetDocumentArtifactInfos", new Dictionary<string, object> { { "artifactIds", SqlConnectionWrapper.ToDataTable(new List<int> { artifactId }, "Int32Collection", "Int32Value") }, { "userId", userId }, { "addDrafts", addDrafts } }, new List<LinkedArtifactInfo> { new LinkedArtifactInfo { ArtifactId = artifactId, ArtifactName = "Test Document Name" } });
            var repository = new SqlAttachmentsRepository(cxn.Object, mockUserRepository);

            // Act
            var result = await repository.GetAttachmentsAndDocumentReferences(artifactId, userId, null, subArtifactId, addDrafts);

            // Assert
            cxn.Verify();
            Assert.AreEqual(result.ArtifactId, 1);
            Assert.AreEqual(result.SubartifactId, subArtifactId);
            Assert.AreEqual(result.Attachments.Count, 1);
            Assert.AreEqual(result.DocumentReferences.Count, 1);
        }

    }
}
