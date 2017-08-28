using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
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
        public async Task GetProcessArtifactInfo_GetProcessInfo_Success()
        {
            // Arrange
            
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlArtifactRepository(cxn.Object);
            HashSet<int> artifactIds = new HashSet<int>(){1, 2, 3};

            List<ProcessInfoDto> processInfos = new List<ProcessInfoDto>()
            {
                new ProcessInfoDto() {ItemId = 1, ProcessType = ProcessType.BusinessProcess},
                new ProcessInfoDto() {ItemId = 2, ProcessType = ProcessType.UserToSystemProcess},
                new ProcessInfoDto() {ItemId = 3, ProcessType = ProcessType.SystemToSystemProcess}
            };

            _artifactRepositoryMock.Setup(r => r.GetProcessInformationAsync(artifactIds))
                .ReturnsAsync(processInfos);

            // Act
            await repository.GetProcessInformationAsync(artifactIds);

            // Assert
            cxn.Verify();

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
