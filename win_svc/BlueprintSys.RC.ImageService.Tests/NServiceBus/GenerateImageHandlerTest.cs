using System;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using BlueprintSys.RC.ImageService.ImageGen;
using BlueprintSys.RC.ImageService.Transport;
using BluePrintSys.Messaging.Models.ProcessImageGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NServiceBus.Testing;

namespace BlueprintSys.RC.ImageService.Tests.NServiceBus
{
    [TestClass]
    public class GenerateImageHandlerTest
    {
        [TestMethod]
        public void Handle_Success_ReturnsImage()
        {
            string inputJson = "{data}";

            var imageGenHelperMock = new Mock<IImageGenHelper>();
            imageGenHelperMock.Setup(
                m => m.GenerateImageAsync(inputJson, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ImageFormat>()))
                .Returns(Task.FromResult(new MemoryStream{Capacity = 100}));
            ImageGenService.Instance.ImageGenerator = imageGenHelperMock.Object;

            Test.Handler<GenerateImageHandler>()
            .ExpectReply<ImageResponseMessage>(
                check: message => message.ErrorMessage == null)
            .OnMessage<GenerateImageMessage>(
                initializeMessage: message =>
                {
                    message.ProcessJsonModel = inputJson;
                });
        }

        [TestMethod]
        public void Handle_NoImageError_ReturnsError()
        {
            string inputJson = "{data}";

            var imageGenHelperMock = new Mock<IImageGenHelper>();
            imageGenHelperMock.Setup(
                m => m.GenerateImageAsync(inputJson, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ImageFormat>()))
                .Returns(Task.FromResult(new MemoryStream()));
            ImageGenService.Instance.ImageGenerator = imageGenHelperMock.Object;

            Test.Handler<GenerateImageHandler>()
            .ExpectReply<ImageResponseMessage>(
                check: message => message.ErrorMessage != null)
            .OnMessage<GenerateImageMessage>(
                initializeMessage: message =>
                {
                    message.ProcessJsonModel = inputJson;
                });
        }

        [TestMethod]
        public void Handle_Exception_ReturnsError()
        {
            string inputJson = "{data}";
            string exceptionMessage = "exception was thrown";

            var imageGenHelperMock = new Mock<IImageGenHelper>();
            imageGenHelperMock.Setup(
                m => m.GenerateImageAsync(inputJson, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ImageFormat>()))
                .Throws(new Exception(exceptionMessage));
            ImageGenService.Instance.ImageGenerator = imageGenHelperMock.Object;

            Test.Handler<GenerateImageHandler>()
            .ExpectReply<ImageResponseMessage>(
                check: message => message.ErrorMessage == exceptionMessage)
            .OnMessage<GenerateImageMessage>(
                initializeMessage: message =>
                {
                    message.ProcessJsonModel = inputJson;
                });
        }
    }
}
