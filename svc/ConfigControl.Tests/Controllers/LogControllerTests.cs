using ServiceLibrary.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.LocalLog;
using Moq;
using ServiceLibrary.Helpers;

namespace ConfigControl.Controllers
{
    [TestClass]
    public class LogControllerTests
    {
        [TestMethod]
        public void Log()
        {
            // Arrange
            var httpClientProvider = new Mock<IHttpClientProvider>().Object;
            var controller = new LogController(httpClientProvider);
            var logEntry = new ServiceLogEntry()
            {
                LogLevel = LogLevelEnum.Informational,
                Source = "Controller source",
                Message = "Hello",
            };

            // Act
            var result = controller.Log(logEntry);

            // Assert
        }

    }
}
