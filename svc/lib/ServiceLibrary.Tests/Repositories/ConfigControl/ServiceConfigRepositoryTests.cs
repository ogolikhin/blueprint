using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Repositories.ConfigControl
{
    [TestClass]
    public class ServiceConfigRepositoryTests
    {
        #region Constructor

        [TestMethod]
        public void Constructor_Always_CreatesDefaultDependencies()
        {
            // Arrange

            // Act
            var controller = new ServiceConfigRepository();

            // Assert
            Assert.IsInstanceOfType(controller._httpClientProvider, typeof(HttpClientProvider));
        }

        #endregion

    }
}
