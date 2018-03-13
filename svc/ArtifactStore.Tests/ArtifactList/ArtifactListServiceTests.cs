using System.Collections.Generic;
using System.Threading.Tasks;
using ArtifactStore.ArtifactList.Models;
using ArtifactStore.ArtifactList.Models.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models.ProjectMeta;

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
            _xmlProfileSettings = new XmlProfileSettings
            {
                Columns = new List<XmlProfileColumn>
                {
                    new XmlProfileColumn
                    {
                        PropertyName = "Name",
                        Predefined = (int)PropertyTypePredefined.Name,
                        PrimitiveType = (int)PropertyPrimitiveType.Text
                    },
                    new XmlProfileColumn
                    {
                        PropertyName = "My Choice",
                        Predefined = (int)PropertyTypePredefined.CustomGroup,
                        PrimitiveType = (int)PropertyPrimitiveType.Choice,
                        PropertyTypeId = 25
                    }
                }
            };

            _repositoryMock = new Mock<IArtifactListSettingsRepository>();
            _repositoryMock
                .Setup(m => m.GetSettingsAsync(_itemId, _userId))
                .ReturnsAsync(_xmlProfileSettings);
            _service = new ArtifactListService(_repositoryMock.Object);
        }
    }
}
