using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Repositories;
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
            bool addDrafts = true;
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlAttachmentsRepository(cxn.Object);
            await repository.GetAttachmentsAndDocumentReferences(artifactId, userId, null, addDrafts);
            cxn.Verify();
        }
    }
}
