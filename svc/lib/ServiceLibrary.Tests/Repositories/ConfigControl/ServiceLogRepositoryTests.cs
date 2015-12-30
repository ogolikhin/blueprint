using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
            var servicelog = new ServiceLogRepository();

            // Act
            await servicelog.LogInformation("ServiceLogRepositoryTests", "Hello World");

            // Assert
            // Throws an error if it fails
        }

        [TestMethod]
        public async Task LogVerbose()
        {
            // Arrange
            var servicelog = new ServiceLogRepository();

            // Act
            await servicelog.LogVerbose("ServiceLogRepositoryTests", "Hello World");

            // Assert
            // Throws an error if it fails
        }

        [TestMethod]
        public async Task LogWarning()
        {
            // Arrange
            var servicelog = new ServiceLogRepository();

            // Act
            await servicelog.LogWarning("ServiceLogRepositoryTests", "Hello World");

            // Assert
            // Throws an error if it fails
        }

        [TestMethod]
        public async Task LogError()
        {
            // Arrange
            var servicelog = new ServiceLogRepository();

            // Act
            await servicelog.LogError("ServiceLogRepositoryTests", "Hello World");

            // Assert
            // Throws an error if it fails
        }

        [TestMethod]
        public async Task LogErrorWithException()
        {
            // Arrange
            var servicelog = new ServiceLogRepository();
            var innerEx = new ApplicationException("Inner application exception");
            var ex = new Exception("Some bad thing", innerEx);

            // Act
            await servicelog.LogError("ServiceLogRepositoryTests", ex);

            // Assert
            // Throws an error if it fails
        }

    }
}
