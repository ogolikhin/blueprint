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
        private Mock<IArtifactRepository> _artifactRepositoryMock;
        private Mock<IArtifactPermissionsRepository> _artifactPermissionsRepository;
        private SqlConnectionWrapperMock cxn;
        private SqlArtifactRepository repository;
        private HashSet<int> artifactIds;
        private Session session;
        private const int userId = 1;

        [TestInitialize]
        public void Initialize()
        {
            _artifactRepositoryMock = new Mock<IArtifactRepository>();
            _artifactPermissionsRepository = new Mock<IArtifactPermissionsRepository>();
            cxn = new SqlConnectionWrapperMock();
            repository = new SqlArtifactRepository(cxn.Object);
            artifactIds = null;
            session = new Session { UserId = userId };
        }

        [TestMethod]
        public async Task GetProcessArtifactInfo_GetProcessInfo_Success()
        {
            // Arrange
            artifactIds = new HashSet<int>() { 1, 2, 3 };
            List<ProcessInfoDto> processInfos = new List<ProcessInfoDto>()
            {
                new ProcessInfoDto() {ItemId = 1, ProcessType = ProcessType.BusinessProcess},
                new ProcessInfoDto() {ItemId = 2, ProcessType = ProcessType.UserToSystemProcess},
                new ProcessInfoDto() {ItemId = 3, ProcessType = ProcessType.SystemToSystemProcess}
            };

            var permissionDict = new Dictionary<int, RolePermissions>() { };
            permissionDict.Add(key: 1, value: RolePermissions.Read);

            _artifactPermissionsRepository.Setup(q => q.GetArtifactPermissions(It.IsAny<IEnumerable<int>>(), session.UserId, false, Int32.MaxValue, true)).ReturnsAsync(permissionDict);

            _artifactRepositoryMock.Setup(r => r.GetProcessInformationAsync(artifactIds, session.UserId)).ReturnsAsync(processInfos);

            repository = new SqlArtifactRepository(cxn.Object, new SqlItemInfoRepository(new SqlConnectionWrapper("")), _artifactPermissionsRepository.Object);

            // Act
            await repository.GetProcessInformationAsync(artifactIds, session.UserId);

            // Assert
            cxn.Verify();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetProcessArtifactInfo_ArtifactIdOutOfRange_ArgumentOutOfBoundsException()
        {

            // Act
            await repository.GetProcessInformationAsync(artifactIds, session.UserId);
        }

        [TestMethod]
        public async Task GetStandardArtifactTypes_AllRequirementsAreSatisfied_SuccessResult()
        {
            // Arrange
            var artifacts = new List<StandardArtifactType> { new StandardArtifactType { Id = 1, Name = "CustomActor" } };
            cxn.SetupQueryAsync("GetStandardArtifactTypes", It.IsAny<Dictionary<string, object>>(), artifacts);

            // Act
            var standardArtifacts = await repository.GetStandardArtifactTypes();

            // Assert
            Assert.IsNotNull(standardArtifacts);
            Assert.AreEqual(artifacts, standardArtifacts);
        }
    }
}
