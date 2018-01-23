using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Repositories.ArtifactListSetting;

namespace ServiceLibrary.Repositories
{
    [TestClass]
    public class ArtifactListSettingsRepositoryTests
    {
        private SqlConnectionWrapperMock _cxn;
        private ArtifactListSettingsRepository _repository;
        private int _itemId = 1;
        private int _userId = 1;
        private string _settings = "test";

        [TestInitialize]
        public void TestInitialize()
        {
            _cxn = new SqlConnectionWrapperMock();
            _repository = new ArtifactListSettingsRepository(_cxn.Object);
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
