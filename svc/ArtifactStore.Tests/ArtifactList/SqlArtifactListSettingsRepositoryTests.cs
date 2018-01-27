using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Repositories;

namespace ArtifactStore.ArtifactList
{
    [TestClass]
    public class SqlArtifactListSettingsRepositoryTests
    {
        private int _itemId;
        private int _userId;
        private string _settings;

        private SqlConnectionWrapperMock _cxn;
        private SqlArtifactListSettingsRepository _repository;

        [TestInitialize]
        public void TestInitialize()
        {
            _userId = 1;
            _itemId = 1;
            _settings = "test";

            _cxn = new SqlConnectionWrapperMock();
            _repository = new SqlArtifactListSettingsRepository(_cxn.Object);
        }

        [TestMethod]
        public async Task GetSettingsAsync_AllParametersAreValid_Success()
        {
            // Arrange
            var expectedResult = "test";
            _cxn.SetupExecuteScalarAsync("GetArtifactListSettings", It.IsAny<Dictionary<string, object>>(), expectedResult);

            // Act
            var result = await _repository.GetSettingsAsync(_itemId, _userId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result, expectedResult);
        }

        [TestMethod]
        public async Task CreateSettingsAsync_AllParametersAreValid_Success()
        {
            // Arrange
            var expectedResult = 0;
            _cxn.SetupExecuteScalarAsync("CreateArtifactListSettings", It.IsAny<Dictionary<string, object>>(), expectedResult);

            // Act
            var result = await _repository.CreateSettingsAsync(_itemId, _userId, _settings);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result, expectedResult);
        }

        [TestMethod]
        public async Task UpdateSettingsAsync_AllParametersAreValid_Success()
        {
            // Arrange
            var expectedResult = 0;
            _cxn.SetupExecuteScalarAsync("UpdateArtifactListSettings", It.IsAny<Dictionary<string, object>>(), expectedResult);

            // Act
            var result = await _repository.UpdateSettingsAsync(_itemId, _userId, _settings);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result, expectedResult);
        }
    }
}
