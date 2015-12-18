using ServiceLibrary.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConfigControl.Controllers
{
    [TestClass]
    public class LogControllerTests
    {
        [TestMethod]
        public void Log()
        {
            // Arrange
            var controller = new LogController();
            var logEntry = new LogEntry(
                LogLevelEnum.Informational,
                "Controller source",
                "Some message",
                "",
                "",
                0,
                "");

            // Act
            var result = controller.Log(logEntry);

            // Assert
        }

    }
}
