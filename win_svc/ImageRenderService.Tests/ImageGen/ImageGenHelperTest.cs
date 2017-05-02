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
        private ImageGenHelper _imageGenHelper;
        private Mock<IVirtualBrowser> _browserMock;

        [TestInitialize]
        public void Initialize()
        {
            _browserMock = new Mock<IVirtualBrowser>();
            var browserPoolMock = new Mock<IBrowserPool>();
            browserPoolMock.Setup(pool => pool.Rent())
                .ReturnsAsync(_browserMock.Object);
            _imageGenHelper = new ImageGenHelper(browserPoolMock.Object);

        }

        [TestMethod]
        public async Task GenerateImageAsync_Success()
        {
            //Arange
            const int size = 20;
            const string url = "testUrl";

            _browserMock.Setup(b => b.Load(url))
               .Raises(b => b.LoadingStateChanged += null,
                _browserMock.Object, 
                new VirtualBrowserLoadingStateChangedEventArgs
                {
                    Browser = _browserMock.Object, IsLoading = false
                });
            _browserMock.Setup(b => b.EvaluateScriptAsync(It.IsAny<string>(), null))
                .Returns(Task.FromResult(new VirtualBrowserJavascriptResponse
                {
                    Result = size
                }));
           
            var screenshotMock = new Mock<IScreenshot>();
            screenshotMock.SetupGet(s => s.Width).Returns(size+10);
            screenshotMock.SetupGet(s => s.Height).Returns(size);
            _browserMock.SetupGet(s => s.Bitmap).Returns(screenshotMock.Object);
            _browserMock.Setup(b => b.ScreenshotAsync(false))
               .Returns(Task.FromResult(screenshotMock.Object));

            //Act
            await _imageGenHelper.GenerateImageAsync(url, ImageFormat.Png);

            //Assert
            screenshotMock.Verify(b => b.Save(It.IsAny<Stream>(), It.IsAny<ImageFormat>()));
        }

        [TestMethod]
        public async Task GenerateImageAsync_NoBrowser_ReturnsNull()
        {
            //Arange
            const string url = "testUrl";

            var browserPoolMock = new Mock<IBrowserPool>();
            browserPoolMock.Setup(pool => pool.Rent())
                .ReturnsAsync((IVirtualBrowser)null);
            _imageGenHelper = new ImageGenHelper(browserPoolMock.Object);
            

            //Act
            var result = await _imageGenHelper.GenerateImageAsync(url, ImageFormat.Png);

            //Assert
            Assert.IsNull(result);
        }
    }
}
