using ServiceLibrary.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.LocalLog;
using Moq;
using ServiceLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http.Results;
using ConfigControl.Repositories;
using ServiceLibrary.Repositories.ConfigControl;

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



        private IEnumerable<string> GetTestLogEntries()
        {
            return new List<string> {
                "id,IpAddress,Source,FormattedMessage,MethodName,FilePath,LineNumber,StackTrace,InstanceName,ProviderId,ProviderName,EventId,EventKeywords,Level,Opcode,Task,Timestamp,Version,Payload,ActivityId,RelatedActivityId,ProcessId,ThreadId",
                ",::1,,,,,0,,BlueprintSys-Blueprint-Blueprint,7395ba7f-7335-5dbf-2878-6ba4dfdbd1a0,BlueprintSys-Blueprint-StandardLog,400,0,4,0,65134,1/1/0001 12:00:00 AM -05:00,0,<Payload><IpAddress>::1</IpAddress><DateTime>1/1/0001 12:00:00 AM</DateTime></Payload>,00000000-0000-0000-0000-000000000000,00000000-0000-0000-0000-000000000000,31240,27144",
                ",::1,,,,,0,,BlueprintSys-Blueprint-Blueprint,7395ba7f-7335-5dbf-2878-6ba4dfdbd1a0,BlueprintSys-Blueprint-StandardLog,400,0,4,0,65134,1/1/0001 12:00:00 AM -05:00,0,<Payload><IpAddress>::1</IpAddress><DateTime>1/1/0001 12:00:00 AM</DateTime></Payload>,00000000-0000-0000-0000-000000000000,00000000-0000-0000-0000-000000000000,31240,29172",
                ",::1,,, [PROFILING], 0, 00000000-0000-0000-0000-000000000000, ,,,0,,BlueprintSys-Blueprint-Blueprint,2530ba0d-e71b-5cc1-54a7-f40c531356b0,BlueprintSys-Blueprint-PerformanceLog,500,0,5,0,65034,1/1/0001 12:00:00 AM -05:00,0,<Payload><IpAddress>::1</IpAddress><DateTime>1/1/0001 12:00:00 AM</DateTime><CorrelationId>00000000-0000-0000-0000-000000000000</CorrelationId><Duration>0</Duration></Payload>,00000000-0000-0000-0000-000000000000,00000000-0000-0000-0000-000000000000,31240,15120"};
        }

        [TestMethod]
        public void GetLog_WithTitle_Succsess()
        {

            // Arrange
            var entries = GetTestLogEntries();
            var length = entries.Sum(it => it.Length + Environment.NewLine.Length);
            var mockLogRepository = new Mock<ILogRepository>();
            var mockServiceLogRepository = new Mock<IServiceLogRepository>();

            mockLogRepository.Setup(o => o.GetLogEntries(It.IsAny<int>(), It.IsAny<bool>())).Returns(entries);

            // Act
            var result = new CsvLogContent(mockLogRepository.Object, mockServiceLogRepository.Object).Generate(It.IsAny<int>(), true);


            // Assert
            Assert.IsInstanceOfType(result, typeof(HttpContent));
            Assert.AreEqual(length, result.Headers.ContentLength);
            Assert.AreEqual("text/csv", result.Headers.ContentType.MediaType);
            Assert.AreEqual("AdminStore.csv", result.Headers.ContentDisposition.FileName);

        }

        [TestMethod]
        public void GetLog_NoTitle_Succsess()
        {

            // Arrange
            var entries = GetTestLogEntries().Skip(1);
            var length = entries.Sum(it => it.Length + Environment.NewLine.Length);
            var mockLogRepository = new Mock<ILogRepository>();
            var mockServiceLogRepository = new Mock<IServiceLogRepository>();

            mockLogRepository.Setup(o => o.GetLogEntries(It.IsAny<int>(), It.IsAny<bool>())).Returns(entries);

            // Act
            var result = new CsvLogContent(mockLogRepository.Object, mockServiceLogRepository.Object).Generate(It.IsAny<int>(), false);


            // Assert
            Assert.IsInstanceOfType(result, typeof(HttpContent));
            Assert.AreEqual(length, result.Headers.ContentLength);
            Assert.AreEqual("text/csv", result.Headers.ContentType.MediaType);
            Assert.AreEqual("AdminStore.csv", result.Headers.ContentDisposition.FileName);

        }

    }
}
