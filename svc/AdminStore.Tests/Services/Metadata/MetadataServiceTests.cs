using System;
using System.Linq;
using AdminStore.Repositories.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Repositories;
using System.Threading.Tasks;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Models.ItemType;
using ServiceLibrary.Models;
using ServiceLibrary.Services.Image;

namespace AdminStore.Services.Metadata
{
    [TestClass]
    public class MetadataServiceTests
    {
        #region Vars

        private Mock<ISqlItemTypeRepository> _sqlItemTypeRepositoryMock;
        private Mock<IMetadataRepository> _metadataRepositoryMock;
        private Mock<IImageService> _imageServiceMock;
        private MetadataService _service;
        private const int ItemTypeIconSize = 32;
        private const string _type = "actor";
        private const int _typeId = 128;
        Icon _customIcon = new Icon
        {
            Content = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A },
            IsSvg = false
        };
        Icon _svgIcon = new Icon
        {
            Content = new byte[] { 0x3A, 0x4A, 0x4E, 0x51, 0x0D, 0x0A, 0x1A, 0x0A },
            IsSvg = true
        };
        private const string _color = "ffffff";
        private const int _sessionUserId = 1;
        private const ItemTypePredefined _itemTypePredefined = ItemTypePredefined.Actor;

        #endregion

        [TestInitialize]
        public void Initialize()
        {
            _sqlItemTypeRepositoryMock = new Mock<ISqlItemTypeRepository>();
            _metadataRepositoryMock = new Mock<IMetadataRepository>();
            _imageServiceMock = new Mock<IImageService>();
            _service = new MetadataService(_sqlItemTypeRepositoryMock.Object, _metadataRepositoryMock.Object, _imageServiceMock.Object);
            _sqlItemTypeRepositoryMock.Setup(repo => repo.GetItemTypeInfo(_typeId, int.MaxValue, true))
                .ReturnsAsync(new ItemTypeInfo()
                {
                    Id = 1,
                    Predefined = _itemTypePredefined,
                    HasCustomIcon = true,
                    Icon = _customIcon.Content
                });
            _imageServiceMock
                .Setup(m => m.ConvertBitmapImageToPng(_svgIcon.Content.ToArray(), ItemTypeIconSize, ItemTypeIconSize))
                .Returns(_svgIcon.Content.ToArray())
                .Verifiable();
            _metadataRepositoryMock.Setup(repo => repo.GetSvgIconContent(_itemTypePredefined, _color)).Returns(_svgIcon.Content.ToArray());
        }

        #region GetIcon

        [TestMethod]
        public async Task GetIcon_AllParamsAreCorrect_ReturnSvgIcon()
        {
            // Arrange

            // Act
            var result = await _service.GetIcon(_type, null, _color);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Content.SequenceEqual(_svgIcon.Content));
            Assert.AreEqual(result.IsSvg, _svgIcon.IsSvg);
        }

        [TestMethod]
        public async Task GetIcon_AllParamsAreCorrect_ReturnCustomIcon()
        {
            // Arrange
            _imageServiceMock
                .Setup(m => m.ConvertBitmapImageToPng(_customIcon.Content.ToArray(), ItemTypeIconSize, ItemTypeIconSize))
                .Returns(_customIcon.Content.ToArray())
                .Verifiable();

            // Act
            var result = await _service.GetIcon(_type, _typeId, _color);

            // Assert
            Assert.IsNotNull(result);
            // Assert.IsTrue(result.Content.SequenceEqual(_customIcon.Content));
            Assert.AreEqual(result.IsSvg, _customIcon.IsSvg);
        }

        [TestMethod]
        public async Task GetIcon_TypeDoNotMatch_ThrowBadRequest()
        {
            // Arrange
            Exception exception = null;

            // Act
            try
            {
                var result = await _service.GetIcon("Aactor", _typeId, _color);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(BadRequestException));
        }

        [TestMethod]
        public async Task GetIcon_GetNullItemTypeInfo_ThrowResourceNotFound()
        {
            // Arrange
            Exception exception = null;
            int typeId = 64;

            // Act
            try
            {
                var result = await _service.GetIcon(_type, typeId, _color);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(ResourceNotFoundException));
        }

        [TestMethod]
        public async Task GetIcon_HasCustomIconIsFalse_ReturnSvgIcon()
        {
            // Arrange
            _sqlItemTypeRepositoryMock.Setup(repo => repo.GetItemTypeInfo(_typeId, int.MaxValue, true))
                .ReturnsAsync(new ItemTypeInfo()
                {
                    Id = 1,
                    Predefined = _itemTypePredefined,
                    HasCustomIcon = false,
                    Icon = _customIcon.Content
                });

            // Act
            var result = await _service.GetIcon(_type, _typeId, _color);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.IsSvg, true);
        }

        #endregion
    }
}
