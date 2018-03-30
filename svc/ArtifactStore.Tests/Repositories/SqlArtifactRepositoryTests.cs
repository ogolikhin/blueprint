using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private const int UserId = 1;

        private Mock<IArtifactRepository> _artifactRepositoryMock;
        private Mock<IArtifactPermissionsRepository> _artifactPermissionsRepository;
        private SqlConnectionWrapperMock _cxn;
        private SqlArtifactRepository _repository;
        private HashSet<int> _artifactIds;
        private Session _session;

        [TestInitialize]
        public void Initialize()
        {
            _artifactRepositoryMock = new Mock<IArtifactRepository>();
            _artifactPermissionsRepository = new Mock<IArtifactPermissionsRepository>();
            _cxn = new SqlConnectionWrapperMock();
            _repository = new SqlArtifactRepository(_cxn.Object);
            _artifactIds = null;
            _session = new Session { UserId = UserId };
        }

        [TestMethod]
        public async Task GetProcessArtifactInfo_GetProcessInfo_Success()
        {
            // Arrange
            _artifactIds = new HashSet<int> { 1, 2, 3 };
            var processInfos = new List<ProcessInfoDto>
            {
                new ProcessInfoDto { ItemId = 1, ProcessType = ProcessType.BusinessProcess },
                new ProcessInfoDto { ItemId = 2, ProcessType = ProcessType.UserToSystemProcess },
                new ProcessInfoDto { ItemId = 3, ProcessType = ProcessType.SystemToSystemProcess }
            };

            var permissionDict = new Dictionary<int, RolePermissions> { { 1, RolePermissions.Read } };

            _artifactPermissionsRepository.Setup(q => q.GetArtifactPermissions(It.IsAny<IEnumerable<int>>(), _session.UserId, false, int.MaxValue, true, null)).ReturnsAsync(permissionDict);

            _artifactRepositoryMock.Setup(r => r.GetProcessInformationAsync(_artifactIds, _session.UserId, true)).ReturnsAsync(processInfos);

            _repository = new SqlArtifactRepository(_cxn.Object, new SqlItemInfoRepository(new SqlConnectionWrapper("")), _artifactPermissionsRepository.Object);

            // Act
            await _repository.GetProcessInformationAsync(_artifactIds, _session.UserId);

            // Assert
            _cxn.Verify();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetProcessArtifactInfo_ArtifactIdOutOfRange_ArgumentOutOfBoundsException()
        {

            // Act
            await _repository.GetProcessInformationAsync(_artifactIds, _session.UserId);
        }

        [TestMethod]
        public async Task GetStandardArtifactTypes_AllRequirementsAreSatisfied_SuccessResult()
        {
            // Arrange
            var artifacts = new List<StandardArtifactType> { new StandardArtifactType { Id = 1, Name = "CustomActor" } };
            _cxn.SetupQueryAsync("GetStandardArtifactTypes", It.IsAny<Dictionary<string, object>>(), artifacts);

            // Act
            var standardArtifacts = await _repository.GetStandardArtifactTypes();

            // Assert
            Assert.IsNotNull(standardArtifacts);
            Assert.AreEqual(standardArtifacts.Count(), 1);
        }

        #region GetStandardProperties

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetStandardProperties_IncorrectParameters_ArgumentOutOfBoundsException()
        {
            // Act
            await _repository.GetStandardProperties(null);
        }

        #endregion
    }
}
