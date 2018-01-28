using System.Threading.Tasks;
using ArtifactStore.ArtifactList.Models.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ArtifactStore.ArtifactList
{
    [TestClass]
    public class ArtifactListServiceTests
    {
        private int _userId;
        private int _itemId;
        private XmlProfileSettings _xmlProfileSettings;

        private Mock<IArtifactListSettingsRepository> _repositoryMock;
        private ArtifactListService _service;

        [TestInitialize]
        public void Initialize()
        {
            _userId = 1;
            _itemId = 1;
            _xmlProfileSettings = new XmlProfileSettings();

            _repositoryMock = new Mock<IArtifactListSettingsRepository>();
            _repositoryMock
                .Setup(m => m.GetSettingsAsync(_itemId, _userId))
                .ReturnsAsync(_xmlProfileSettings);
            _service = new ArtifactListService(_repositoryMock.Object);
        }

        [TestMethod]
        public async Task GetColumnSettingsAsync_NoSettingsExist_ReturnsNull()
        {
            // Arrange
            _repositoryMock
                .Setup(m => m.GetSettingsAsync(_itemId, _userId))
                .ReturnsAsync((XmlProfileSettings)null);

            // Act
            var result = await _service.GetColumnSettingsAsync(_itemId, _userId);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetColumnSettingsAsync_SettingsExist_ReturnsSettings()
        {
            // Act
            var result = await _service.GetColumnSettingsAsync(_itemId, _userId);

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
