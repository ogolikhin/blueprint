using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using ImageRenderService.ImageGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ImageRenderService.Tests.ImageGen
{
    [TestClass]
    public class ImageGenHelperTest
    {
        private Mock<ImageGenHelper> _imageGenHelperMock;
        private Mock<IVirtualBrowser> _browserMock;

        [TestInitialize]
        public void Initialize()
        {
            _browserMock = new Mock<IVirtualBrowser>();
            var browserPoolMock = new Mock<IBrowserPool>();
            browserPoolMock.Setup(pool => pool.Rent())
                .ReturnsAsync(_browserMock.Object);
            _imageGenHelperMock = new Mock<ImageGenHelper>(browserPoolMock.Object);

        }

        [TestMethod]
        public async Task GenerateImageAsync_Success()
        {
            //Arrange
            const string jsonModel = "json";
            const int maxWidth = 6000;
            const int maxHeight = 5000;

            _browserMock.Setup(b => b.Load(It.IsAny<string>()))
               .Raises(b => b.LoadingStateChanged += null,
                _browserMock.Object, 
                new VirtualBrowserLoadingStateChangedEventArgs
                {
                    Browser = _browserMock.Object, IsLoading = false
                });

            var screenshotMock = new Mock<IScreenshot>();
            _browserMock.SetupGet(s => s.Bitmap).Returns(screenshotMock.Object);
            _browserMock.Setup(b => b.ScreenshotAsync(false))
               .Returns(Task.FromResult(screenshotMock.Object));
            _imageGenHelperMock.Setup(s => s.LoadPageAsync(_browserMock.Object, jsonModel, maxWidth, maxHeight))
                .ReturnsAsync(true);

            //Act
            await _imageGenHelperMock.Object.GenerateImageAsync(jsonModel, maxWidth, maxHeight, ImageFormat.Png);

            //Assert
            screenshotMock.Verify(b => b.Save(It.IsAny<Stream>(), It.IsAny<ImageFormat>()));

        }

        [TestMethod]
        public async Task GenerateImageAsync_NoBrowser_ReturnsNull()
        {
            //Arrange
            var browserPoolMock = new Mock<IBrowserPool>();
            browserPoolMock.Setup(pool => pool.Rent())
                .ReturnsAsync((IVirtualBrowser)null);
            var imageGenHelper = new ImageGenHelper(browserPoolMock.Object);
            

            //Act
            var result = await imageGenHelper.GenerateImageAsync("json", 5000, 5000, ImageFormat.Png);

            //Assert
            Assert.IsNull(result);
        }
    }
}
