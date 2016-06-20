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
        [TestMethod]
        public async Task GetAttachmentsAndDocumentReferences_NotSubArtifactAddDrafts_ResultsReturned()
        {
            // Arrange
            int artifactId = 1;
            int userId = 1;
            int? subArtifactId = null;
            bool addDrafts = true;

            var cxn = new SqlConnectionWrapperMock();
            cxn.SetupQueryAsync("GetItemAttachments", new Dictionary<string, object> { { "itemId", artifactId }, { "userId", userId }, { "addDrafts", addDrafts } }, new List<Attachment> { new Attachment { Name = "Test File Name", FileGuid = new System.Guid() } });
            cxn.SetupQueryAsync("GetDocumentReferenceArtifacts", new Dictionary<string, object> { { "itemId", artifactId }, { "userId", userId }, { "addDrafts", addDrafts } }, new List<int> { 1 });
            cxn.SetupQueryAsync("GetOnlyDocumentArtifacts", new Dictionary<string, object> { { "artifactIds", DapperHelper.GetIntCollectionTableValueParameter(new List<int> { artifactId })}, { "userId", userId }, { "addDrafts", addDrafts } }, new List<DocumentReference> { new DocumentReference { Name = "Test File Name", VersionArtifactId = 1 } });
            var repository = new SqlAttachmentsRepository(cxn.Object);

            // Act
            var result = await repository.GetAttachmentsAndDocumentReferences(artifactId, userId, subArtifactId, addDrafts);

            // Assert
            cxn.Verify();
            Assert.AreEqual(result.ArtifactId, 1);
            Assert.AreEqual(result.SubartifactId, null);
            Assert.AreEqual(result.Attachments.Count, 1);
            Assert.AreEqual(result.DocumentReferences.Count, 1);
        }
    }
}
