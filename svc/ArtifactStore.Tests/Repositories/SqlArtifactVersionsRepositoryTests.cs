using ArtifactStore.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtifactStore.Repositories
{
    [TestClass]
    public class SqlArtifactVersionsRepositoryTests
    {
        public async Task SampleTest(int artifactId, int limit, int offset, int? userId, bool asc, int sessionUserId )
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactVersionsRepository(cxn.Object);
            var prm = new Dictionary<string, object>();
            var testResult = new List<ArtifactHistoryVersion>();
            cxn.SetupQueryAsync("GetArtifactChildren", prm, testResult);
            // Act
            var actual = await repository.GetArtifactVersions(artifactId, limit, offset, userId, asc, sessionUserId);
            // Assert
            cxn.Verify();
            //string errorMessage;
            //Assert.IsTrue(CompareArtifactChildren(expected, actual, out errorMessage), errorMessage);
        }

    }
}
