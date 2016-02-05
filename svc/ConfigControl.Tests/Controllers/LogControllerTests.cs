using ServiceLibrary.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.LocalLog;
using Moq;
using ServiceLibrary.Helpers;
using System;
using System.Web.Http.Results;

namespace ConfigControl.Controllers
{
    [TestClass]
    public class LogControllerTests
    {
        #region Constructor

        [TestMethod]
        public void Constructor_Always_CreatesDefaultDependencies()
        {
            // Arrange

            // Act
            var controller = new LogController();

            // Assert
            Assert.IsInstanceOfType(controller._httpClientProvider, typeof(HttpClientProvider));
        }

        #endregion

        #region Log

        [TestMethod]
        public void Log_Error_ReturnsOk()
        {
            // Arrange
            var httpClientProvider = new Mock<IHttpClientProvider>().Object;
            var controller = new LogController(httpClientProvider);
            var logEntry = new ServiceLogModel()
            {
                LogLevel = LogLevelEnum.Error,
                Source = "Controller source",
                Message = "Hello",
                OccurredAt = DateTime.Now
            };

            // Act
            var result = controller.Log(logEntry);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public void Log_Warning_ReturnsOk()
        {
            // Arrange
            var httpClientProvider = new Mock<IHttpClientProvider>().Object;
            var controller = new LogController(httpClientProvider);
            var logEntry = new ServiceLogModel()
            {
                LogLevel = LogLevelEnum.Warning,
                Source = "Controller source",
                Message = "Hello",
                OccurredAt = DateTime.Now
            };

            // Act
            var result = controller.Log(logEntry);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public void Log_Verbose_ReturnsOk()
        {
            // Arrange
            var httpClientProvider = new Mock<IHttpClientProvider>().Object;
            var controller = new LogController(httpClientProvider);
            var logEntry = new ServiceLogModel()
            {
                LogLevel = LogLevelEnum.Verbose,
                Source = "Controller source",
                Message = "Hello",
                OccurredAt = DateTime.Now
            };

            // Act
            var result = controller.Log(logEntry);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public void Log_Informational_ReturnsOk()
        {
            // Arrange
            var httpClientProvider = new Mock<IHttpClientProvider>().Object;
            var controller = new LogController(httpClientProvider);
            var logEntry = new ServiceLogModel()
            {
                LogLevel = LogLevelEnum.Informational,
                Source = "Controller source",
                Message = "Hello",
                OccurredAt = DateTime.Now
            };

            // Act
            var result = controller.Log(logEntry);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        #endregion

        #region CLog

        [TestMethod]
        public void CLog_Error_ReturnsOk()
        {
            // Arrange
            var httpClientProvider = new Mock<IHttpClientProvider>().Object;
            var controller = new LogController(httpClientProvider);
            var logEntry = new CLogModel()
            {
                LogLevel = LogLevelEnum.Error,
                Source = "Controller source",
                Message = "Hello",
                OccurredAt = DateTime.Now
            };

            // Act
            var result = controller.Log(logEntry);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public void CLog_Warning_ReturnsOk()
        {
            // Arrange
            var httpClientProvider = new Mock<IHttpClientProvider>().Object;
            var controller = new LogController(httpClientProvider);
            var logEntry = new CLogModel()
            {
                LogLevel = LogLevelEnum.Warning,
                Source = "Controller source",
                Message = "Hello",
                OccurredAt = DateTime.Now
            };

            // Act
            var result = controller.Log(logEntry);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public void CLog_Verbose_ReturnsOk()
        {
            // Arrange
            var httpClientProvider = new Mock<IHttpClientProvider>().Object;
            var controller = new LogController(httpClientProvider);
            var logEntry = new CLogModel()
            {
                LogLevel = LogLevelEnum.Verbose,
                Source = "Controller source",
                Message = "Hello",
                OccurredAt = DateTime.Now
            };

            // Act
            var result = controller.Log(logEntry);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public void CLog_Informational_ReturnsOk()
        {
            // Arrange
            var httpClientProvider = new Mock<IHttpClientProvider>().Object;
            var controller = new LogController(httpClientProvider);
            var logEntry = new CLogModel()
            {
                LogLevel = LogLevelEnum.Informational,
                Source = "Controller source",
                Message = "Hello",
                OccurredAt = DateTime.Now
            };

            // Act
            var result = controller.Log(logEntry);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public void CLog_Critical_ReturnsOk()
        {
            // Arrange
            var httpClientProvider = new Mock<IHttpClientProvider>().Object;
            var controller = new LogController(httpClientProvider);
            var logEntry = new CLogModel()
            {
                LogLevel = LogLevelEnum.Critical,
                Source = "Controller source",
                Message = "Hello",
                OccurredAt = DateTime.Now
            };

            // Act
            var result = controller.Log(logEntry);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        #endregion

        #region CLog WithAction

        [TestMethod]
        public void CLog_WithAction_Error_ReturnsOk()
        {
            // Arrange
            var httpClientProvider = new Mock<IHttpClientProvider>().Object;
            var controller = new LogController(httpClientProvider);
            var logEntry = new CLogModel
            {
                LogLevel = LogLevelEnum.Error,
                Source = "Controller source",
                Message = "Hello",
                OccurredAt = DateTime.Now,
                ActionName = "TestAction",
                Duration = 60
            };

            // Act
            var result = controller.Log(logEntry);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public void CLog_WithAction_Warning_ReturnsOk()
        {
            // Arrange
            var httpClientProvider = new Mock<IHttpClientProvider>().Object;
            var controller = new LogController(httpClientProvider);
            var logEntry = new CLogModel()
            {
                LogLevel = LogLevelEnum.Warning,
                Source = "Controller source",
                Message = "Hello",
                OccurredAt = DateTime.Now,
                ActionName = "TestAction"
            };

            // Act
            var result = controller.Log(logEntry);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public void CLog_WithAction_Verbose_ReturnsOk()
        {
            // Arrange
            var httpClientProvider = new Mock<IHttpClientProvider>().Object;
            var controller = new LogController(httpClientProvider);
            var logEntry = new CLogModel()
            {
                LogLevel = LogLevelEnum.Verbose,
                Source = "Controller source",
                Message = "Hello",
                OccurredAt = DateTime.Now,
                ActionName = "TestAction"
            };

            // Act
            var result = controller.Log(logEntry);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public void CLog_WithAction_Informational_ReturnsOk()
        {
            // Arrange
            var httpClientProvider = new Mock<IHttpClientProvider>().Object;
            var controller = new LogController(httpClientProvider);
            var logEntry = new CLogModel()
            {
                LogLevel = LogLevelEnum.Informational,
                Source = "Controller source",
                Message = "Hello",
                OccurredAt = DateTime.Now,
                ActionName = "TestAction"
            };

            // Act
            var result = controller.Log(logEntry);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public void CLog_WithAction_Critical_ReturnsOk()
        {
            // Arrange
            var httpClientProvider = new Mock<IHttpClientProvider>().Object;
            var controller = new LogController(httpClientProvider);
            var logEntry = new CLogModel()
            {
                LogLevel = LogLevelEnum.Critical,
                Source = "Controller source",
                Message = "Hello",
                OccurredAt = DateTime.Now,
                ActionName = "TestAction"
            };

            // Act
            var result = controller.Log(logEntry);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        #endregion

        #region StandardLog

        [TestMethod]
        public void StandardLog_Error_ReturnsOk()
        {
            // Arrange
            var httpClientProvider = new Mock<IHttpClientProvider>().Object;
            var controller = new LogController(httpClientProvider);
            var logEntry = new StandardLogModel()
            {
                LogLevel = LogLevelEnum.Error,
                Source = "Controller source",
                Message = "Hello",
                OccurredAt = DateTime.Now
            };

            // Act
            var result = controller.Log(logEntry);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public void StandardLog_Warning_ReturnsOk()
        {
            // Arrange
            var httpClientProvider = new Mock<IHttpClientProvider>().Object;
            var controller = new LogController(httpClientProvider);
            var logEntry = new StandardLogModel()
            {
                LogLevel = LogLevelEnum.Warning,
                Source = "Controller source",
                Message = "Hello",
                OccurredAt = DateTime.Now
            };

            // Act
            var result = controller.Log(logEntry);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public void StandardLog_Verbose_ReturnsOk()
        {
            // Arrange
            var httpClientProvider = new Mock<IHttpClientProvider>().Object;
            var controller = new LogController(httpClientProvider);
            var logEntry = new StandardLogModel()
            {
                LogLevel = LogLevelEnum.Verbose,
                Source = "Controller source",
                Message = "Hello",
                OccurredAt = DateTime.Now
            };

            // Act
            var result = controller.Log(logEntry);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public void StandardLog_Informational_ReturnsOk()
        {
            // Arrange
            var httpClientProvider = new Mock<IHttpClientProvider>().Object;
            var controller = new LogController(httpClientProvider);
            var logEntry = new StandardLogModel()
            {
                LogLevel = LogLevelEnum.Informational,
                Source = "Controller source",
                Message = "Hello",
                OccurredAt = DateTime.Now
            };

            // Act
            var result = controller.Log(logEntry);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public void StandardLog_Critical_ReturnsOk()
        {
            // Arrange
            var httpClientProvider = new Mock<IHttpClientProvider>().Object;
            var controller = new LogController(httpClientProvider);
            var logEntry = new StandardLogModel()
            {
                LogLevel = LogLevelEnum.Critical,
                Source = "Controller source",
                Message = "Hello",
                OccurredAt = DateTime.Now
            };

            // Act
            var result = controller.Log(logEntry);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        #endregion

        #region PerformanceLog

        [TestMethod]
        public void PerformanceLog_Verbose_ReturnsOk()
        {
            // Arrange
            var httpClientProvider = new Mock<IHttpClientProvider>().Object;
            var controller = new LogController(httpClientProvider);
            var logEntry = new PerformanceLogModel()
            {
                LogLevel = LogLevelEnum.Verbose,
                Source = "Controller source",
                Message = "Hello",
                OccurredAt = DateTime.Now
            };

            // Act
            var result = controller.Log(logEntry);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        #endregion

        #region SqlTraceLog

        [TestMethod]
        public void SqlTraceLog_Verbose_ReturnsOk()
        {
            // Arrange
            var httpClientProvider = new Mock<IHttpClientProvider>().Object;
            var controller = new LogController(httpClientProvider);
            var logEntry = new SQLTraceLogModel()
            {
                LogLevel = LogLevelEnum.Verbose,
                Source = "Controller source",
                Message = "Hello",
                OccurredAt = DateTime.Now
            };

            // Act
            var result = controller.Log(logEntry);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        #endregion

    }
}
