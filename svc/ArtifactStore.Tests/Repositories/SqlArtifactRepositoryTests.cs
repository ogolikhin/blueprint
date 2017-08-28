using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Repositories
{
    [TestClass]
    public class SqlArtifactRepositoryTests
    {
        private Mock<ISqlArtifactRepository> _artifactRepositoryMock;
        [TestInitialize]
        public void Initialize()
        {
            _artifactRepositoryMock = new Mock<ISqlArtifactRepository>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetProcessArtifactInfo_ArtifactIdOutOfRange_ArgumentOutOfBoundsException()
        {
            // Arrange
            HashSet<int> artifactIds = null;
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactRepository(cxn.Object);
            // Act
            await repository.GetProcessInformationAsync(artifactIds);
        }
    }
}
