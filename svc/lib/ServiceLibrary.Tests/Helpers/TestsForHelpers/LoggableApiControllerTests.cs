using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServiceLibrary.Helpers.TestsForHelpers
{
    class MyLoggableApiController : LoggableApiController
    {
        public override string LogSource { get; }
    }

    [TestClass]
    public class LoggableApiControllerTests
    {
        [TestMethod]
        public void LoggableApiController_DefaultConstructor_LogIsNotNull()
        {
            // Act
            var controller = new MyLoggableApiController();

            // Assert
            Assert.IsNotNull(controller.Log, "The Log property should not be null!");
        }

    }
}
