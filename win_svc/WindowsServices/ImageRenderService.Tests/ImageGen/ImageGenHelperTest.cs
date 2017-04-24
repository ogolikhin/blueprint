using System;
using System.Drawing.Imaging;
using CefSharp;
using CefSharp.OffScreen;
using ImageRenderService.ImageGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ImageRenderService.Tests.ImageGen
{
    [TestClass]
    public class ImageGenHelperTest
    {
        private ImageGenHelper _imageGenHelper;
        private Mock<ChromiumWebBrowser> _browserMock;

        [TestInitialize]
        public void Initialize()
        {
            _browserMock = new Mock<ChromiumWebBrowser>();
            var browserPoolMock = new Mock<IBrowserPool>();
            browserPoolMock.Setup(pool => pool.Rent())
                .ReturnsAsync(_browserMock.Object);
            _imageGenHelper = new ImageGenHelper(browserPoolMock.Object);

        }
        [TestMethod]
        public async void GenerateImageAsync_Success()
        {
            var browserM = new Mock<IBrowser>();
            _browserMock.Setup(b => b.Load(It.IsAny<string>()))
                .Raises(b => b.LoadingStateChanged += null, this, new LoadingStateChangedEventArgs(browserM.Object, true, true, false));

            await _imageGenHelper.GenerateImageAsync("test", ImageFormat.Png);
        }
    }
}
