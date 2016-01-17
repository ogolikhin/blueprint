using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Helpers;
using ServiceLibrary.LocalLog;
using ServiceLibrary.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories.ConfigControl
{
    [TestClass]
    public class ServiceLogRepositoryTests
    {
        [TestMethod]
        public async Task LogInformation()
        {
            // Arrange
            var httpClientProvider = new TestHttpClientProvider(null);
            var localLog = new Mock<ILocalLog>().Object;
            var servicelog = new ServiceLogRepository(httpClientProvider, localLog);

            // Act
            await servicelog.LogInformation("ServiceLogRepositoryTests", "Hello World");

            // Assert
            // Doesn't return anything
        }

        [TestMethod]
        public async Task LogVerbose()
        {
            // Arrange
            var httpClientProvider = new TestHttpClientProvider(null);
            var localLog = new Mock<ILocalLog>().Object;
            var servicelog = new ServiceLogRepository(httpClientProvider, localLog);

            // Act
            await servicelog.LogVerbose("ServiceLogRepositoryTests", "Hello World");

            // Assert
            // Doesn't return anything
        }

        [TestMethod]
        public async Task LogWarning()
        {
            // Arrange
            var httpClientProvider = new TestHttpClientProvider(null);
            var localLog = new Mock<ILocalLog>().Object;
            var servicelog = new ServiceLogRepository(httpClientProvider, localLog);

            // Act
            await servicelog.LogWarning("ServiceLogRepositoryTests", "Hello World");

            // Assert
            // Doesn't return anything
        }

        [TestMethod]
        public async Task LogError()
        {
            // Arrange
            var httpClientProvider = new TestHttpClientProvider(null);
            var localLog = new Mock<ILocalLog>().Object;
            var servicelog = new ServiceLogRepository(httpClientProvider, localLog);

            // Act
            await servicelog.LogError("ServiceLogRepositoryTests", "Hello World");

            // Assert
            // Doesn't return anything
        }

        [TestMethod]
        public async Task LogErrorWithException()
        {
            // Arrange
            var httpClientProvider = new TestHttpClientProvider(null);
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
        public async Task LogCLog()
        {
            // Arrange
            var httpClientProvider = new TestHttpClientProvider(null);
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

    }
}
