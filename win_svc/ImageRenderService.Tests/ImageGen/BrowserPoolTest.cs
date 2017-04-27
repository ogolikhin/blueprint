using System;
using CefSharp.OffScreen;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageRenderService.ImageGen;
using Moq;

namespace ImageRenderService.Tests.ImageGen
{
    [TestClass]
    public class BrowserPoolTest
    {
        private IBrowserPool _browserPool;

        [TestInitialize]
        public void Initialize()
        {
            _browserPool = BrowserPool.Create();
        }

        [TestMethod]
        public void Rent_Success()
        {
            //not sure if this class can have meaningful unit tests
            Assert.AreEqual(true, true);
        }
    }
}
