using System;
using System.IO;
using System.Linq;
using AdminStore.Repositories.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Repositories;
using System.Threading.Tasks;
using ServiceLibrary.Exceptions;
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
        private Mock<IImageService> _imageServiceMock;
        private MetadataService _service;
        private const int ItemTypeIconSize = 32;
        private const string _type = "actor";
        private const int _typeId = 128;
        byte[] _icon = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
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
                    Predefined = ItemTypePredefined.Actor,
                    HasCustomIcon = true,
                    Icon = _icon
                });
            _imageServiceMock
                .Setup(m => m.ConvertBitmapImageToPng(_icon, ItemTypeIconSize, ItemTypeIconSize))
                .Returns(_icon)
                .Verifiable();
            _metadataRepositoryMock.Setup(repo => repo.GetSvgIcon(_itemTypePredefined, _color))
                .Returns(new MemoryStream());
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
            Assert.IsTrue(_icon.SequenceEqual(result));
        }

        [TestMethod]
        public async Task GetCustomItemTypeIcon_CustomIconNotExist_ThrowResourceNotFound()
        {
            // arrange
            int typeId = 8;
            Exception exception = null;

            // act
            try
            {
                var result = await _service.GetCustomItemTypeIcon(typeId);

            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(ResourceNotFoundException));
        }

        #endregion

        #region GetItemTypeIcon

        [TestMethod]
        public void GetItemTypeIcon_CustomIconExist_ReturnStream()
        {
            // arrange

            // act
            var result = _service.GetItemTypeIcon(_itemTypePredefined, _color);

            // assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetItemTypeIcon_CustomIconNotExist_ReturnNull()
        {
            // arrange
            ItemTypePredefined itemTypePredefined = ItemTypePredefined.None;

            // act
            var result = _service.GetItemTypeIcon(itemTypePredefined, _color);

            // assert
            Assert.IsNull(result);
        }

        #endregion

    }
}
