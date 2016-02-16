using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Helpers;
using ServiceLibrary.LocalLog;
using ServiceLibrary.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories.ConfigControl
{
    [TestClass]
    public class ServiceLogRepositoryTests
    {
        #region Constructor

        [TestMethod]
        public void Constructor_Always_CreatesDefaultDependencies()
        {
            // Arrange
            ConfigControlHttpClientLocator.Init(new HttpClientProvider());

            // Act
            var controller = new ServiceLogRepository();

            // Assert
            Assert.IsInstanceOfType(controller._httpClientProvider, typeof(HttpClientProvider));
        }

        #endregion

        [TestMethod]
        public async Task LogInformation_Success()
        {
            // Arrange
            var httpClientProvider = new TestHttpClientProvider(request => new HttpResponseMessage(HttpStatusCode.OK));
            var localLog = new Mock<ILocalLog>().Object;
            var servicelog = new ServiceLogRepository(httpClientProvider, localLog);

            // Act
            await servicelog.LogInformation("ServiceLogRepositoryTests", "Hello World");

            // Assert
            // Doesn't return anything
        }

        [TestMethod]
        public async Task LogVerbose_Success()
        {
            // Arrange
            var httpClientProvider = new TestHttpClientProvider(request => new HttpResponseMessage(HttpStatusCode.OK));
            var localLog = new Mock<ILocalLog>().Object;
            var servicelog = new ServiceLogRepository(httpClientProvider, localLog);

            // Act
            await servicelog.LogVerbose("ServiceLogRepositoryTests", "Hello World");

            // Assert
            // Doesn't return anything
        }

        [TestMethod]
        public async Task LogWarning_Success()
        {
            // Arrange
            var httpClientProvider = new TestHttpClientProvider(request => new HttpResponseMessage(HttpStatusCode.OK));
            var localLog = new Mock<ILocalLog>().Object;
            var servicelog = new ServiceLogRepository(httpClientProvider, localLog);

            // Act
            await servicelog.LogWarning("ServiceLogRepositoryTests", "Hello World");

            // Assert
            // Doesn't return anything
        }

        [TestMethod]
        public async Task LogError_Success()
        {
            // Arrange
            var httpClientProvider = new TestHttpClientProvider(request => new HttpResponseMessage(HttpStatusCode.OK));
            var localLog = new Mock<ILocalLog>().Object;
            var servicelog = new ServiceLogRepository(httpClientProvider, localLog);

            // Act
            await servicelog.LogError("ServiceLogRepositoryTests", "Hello World");

            // Assert
            // Doesn't return anything
        }

        [TestMethod]
        public async Task LogErrorWithException_Success()
        {
            // Arrange
            var httpClientProvider = new TestHttpClientProvider(request => new HttpResponseMessage(HttpStatusCode.OK));
            var localLog = new Mock<ILocalLog>().Object;
            var servicelog = new ServiceLogRepository(httpClientProvider, localLog);
            var innerEx = new ApplicationException("Inner application exception");
            var ex = new Exception("Some bad thing", innerEx);

            // Act
            await servicelog.LogError("ServiceLogRepositoryTests", ex);

            // Assert
            // Doesn't return anything
        }

        [TestMethod]
        public async Task LogCLog_Success()
        {
            // Arrange
            var httpClientProvider = new TestHttpClientProvider(request => new HttpResponseMessage(HttpStatusCode.OK));
            var localLog = new Mock<ILocalLog>().Object;
            var servicelog = new ServiceLogRepository(httpClientProvider, localLog);
            var logEntry = new CLogModel()
            {
                Source = "ServiceLogRepositoryTests",
                LogLevel = LogLevelEnum.Informational,
                Message = "Hello World",
                UserName = "Admin"
            };

            // Act
            await servicelog.LogCLog(logEntry);

            // Assert
            // Doesn't return anything
        }

        [TestMethod]
        public async Task LogPerformanceLog_Success()
        {
            // Arrange
            var httpClientProvider = new TestHttpClientProvider(request => new HttpResponseMessage(HttpStatusCode.OK));
            var localLog = new Mock<ILocalLog>().Object;
            var servicelog = new ServiceLogRepository(httpClientProvider, localLog);
            var logEntry = new PerformanceLogModel()
            {
                Source = "ServiceLogRepositoryTests",
                LogLevel = LogLevelEnum.Informational,
                Message = "Hello World",
                UserName = "Admin"
            };

            // Act
            await servicelog.LogPerformanceLog(logEntry);

            // Assert
            // Doesn't return anything
        }

        [TestMethod]
        public async Task LogSQLTraceLog_Success()
        {
            // Arrange
            var httpClientProvider = new TestHttpClientProvider(request => new HttpResponseMessage(HttpStatusCode.OK));
            var localLog = new Mock<ILocalLog>().Object;
            var servicelog = new ServiceLogRepository(httpClientProvider, localLog);
            var logEntry = new SQLTraceLogModel()
            {
                Source = "ServiceLogRepositoryTests",
                LogLevel = LogLevelEnum.Informational,
                Message = "Hello World",
                UserName = "Admin"
            };

            // Act
            await servicelog.LogSQLTraceLog(logEntry);

            // Assert
            // Doesn't return anything
        }

        [TestMethod]
        public async Task LogStandardLog_Success()
        {
            // Arrange
            var httpClientProvider = new TestHttpClientProvider(request => new HttpResponseMessage(HttpStatusCode.OK));
            var localLog = new Mock<ILocalLog>().Object;
            var servicelog = new ServiceLogRepository(httpClientProvider, localLog);
            var logEntry = new StandardLogModel()
            {
                Source = "ServiceLogRepositoryTests",
                LogLevel = LogLevelEnum.Informational,
                Message = "Hello World",
                UserName = "Admin"
            };

            // Act
            await servicelog.LogStandardLog(logEntry);

            // Assert
            // Doesn't return anything
        }

    }
}
