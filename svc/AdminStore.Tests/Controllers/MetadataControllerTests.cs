using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AdminStore.Services.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;
using ServiceLibrary.Services.Image;

namespace AdminStore.Controllers
{
    [TestClass]
    public class MetadataControllerTests
    {
        #region Vars

        private MetadataController _controller;
        private Mock<IPrivilegesRepository> _privilegesRepositoryMock;
        private Mock<IMetadataService> _metadataServiceMock;
        private Mock<IServiceLogRepository> _logMock;
        private Mock<IImageService> _imageServiceMock;
        private const string _type = "actor";
        private const int _typeId = 128;
        private const string _color = "ffffff";
        private const int _sessionUserId = 1;
        private const int ItemTypeIconSize = 32;
        private const int _imageId = 123;
        Icon _icon = new Icon
        {
            Content = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A },
            IsSvg = true
        };

        #endregion

        [TestInitialize]
        public void Initialize()
        {
            _metadataServiceMock = new Mock<IMetadataService>();
            _logMock = new Mock<IServiceLogRepository>();
            _privilegesRepositoryMock = new Mock<IPrivilegesRepository>();
            _imageServiceMock = new Mock<IImageService>();
            _controller = new MetadataController(_metadataServiceMock.Object, _logMock.Object, _imageServiceMock.Object)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
            var session = new Session { UserId = _sessionUserId };

            _controller.Request.Properties[ServiceConstants.SessionProperty] = session;
            _controller.Request.RequestUri = new Uri("http://localhost");
            _imageServiceMock
                .Setup(m => m.CreateByteArrayContent(_icon.Content, _icon.IsSvg))
                .Returns(new ByteArrayContent(_icon.Content))
                .Verifiable();
            _imageServiceMock
                .Setup(m => m.ConvertBitmapImageToPng(_icon.Content, ItemTypeIconSize, ItemTypeIconSize))
                .Returns(_icon.Content)
                .Verifiable();
            _metadataServiceMock.Setup(service => service.GetIconAsync(_type, null, _color, _imageId)).ReturnsAsync(_icon);
        }

        #region GetIcons

        [TestMethod]
        public async Task GetIconsAsync_AllParamsAreCorrect_ReturnCustomIconSvgICon()
        {
            // Arrange
            var icon = new Icon
            {
                Content = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A },
                IsSvg = false
            };
            _metadataServiceMock.Setup(service => service.GetIconAsync(_type, _typeId, _color, _imageId)).ReturnsAsync(icon);

            // Act
            var result = await _controller.GetIconsAsync(_type, _typeId, _color, _imageId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        [TestMethod]
        public async Task GetIconsAsync_AllParamsAreCorrect_ReturnSvgICon()
        {
            // Arrange

            // Act
            var result = await _controller.GetIconsAsync(_type, null, _color, _imageId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        [TestMethod]
        public async Task GetIconsAsync_TypeIdNotCorrect_ThrowResourceNotFoundException()
        {
            // Arrange
            Exception exception = null;
            var icon = new Icon
            {
                Content = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A },
                IsSvg = false
            };
            _metadataServiceMock.Setup(service => service.GetIconAsync(_type, _typeId, _color, _imageId)).ReturnsAsync(icon);

            // Act
            try
            {
                var result = await _controller.GetIconsAsync(_type, _typeId + 1, _color, _imageId);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(ResourceNotFoundException));
        }

        #endregion

    }
}
