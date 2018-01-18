using AdminStore.Repositories.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Repositories;
using System.Threading.Tasks;
using ServiceLibrary.Models.ItemType;
using ServiceLibrary.Models;
using ServiceLibrary.Services;
using ServiceLibrary.Services.Image;

namespace AdminStore.Services.Metadata
{
    [TestClass]
    public class MetadataServiceTests
    {
        #region Vars

        private Mock<ISqlItemTypeRepository> _sqlItemTypeRepositoryMock;
        private Mock<IMetadataRepository> _metadataRepositoryMock;
        private Mock<IImageService> _imageHelperMock;
        private MetadataService _service;
        private const int ItemTypeIconSize = 32;
        private const string _type = "actor";
        private const int _typeId = 128;
        byte[] _icon = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        private const string _color = "ffffff";
        private const int _sessionUserId = 1;

        #endregion

        [TestInitialize]
        public void Initialize()
        {
            _sqlItemTypeRepositoryMock = new Mock<ISqlItemTypeRepository>();
            _metadataRepositoryMock = new Mock<IMetadataRepository>();
            _imageHelperMock = new Mock<IImageService>();
            _service = new MetadataService(_sqlItemTypeRepositoryMock.Object, _metadataRepositoryMock.Object, _imageHelperMock.Object);
            _sqlItemTypeRepositoryMock.Setup(repo => repo.GetItemTypeInfo(_typeId, int.MaxValue, true))
                .ReturnsAsync(new ItemTypeInfo()
                {
                    Id = 1,
                    Predefined = ItemTypePredefined.Actor,
                    HasCustomIcon = true,
                    Icon = _icon
                });
            _imageHelperMock
                .Setup(m => m.ConvertBitmapImageToPng(_icon, 32, 32))
                .Returns(_icon)
                .Verifiable();
        }


        #region GetCustomItemTypeIcon

        [TestMethod]
        public async Task GetCustomItemTypeIcon_CustomIconExist_ReturnByteArray()
        {
            // arrange

            // act
            var result = await _service.GetCustomItemTypeIcon(_typeId);

            // assert
            Assert.IsNotNull(result);
        }

        #endregion

    }
}
