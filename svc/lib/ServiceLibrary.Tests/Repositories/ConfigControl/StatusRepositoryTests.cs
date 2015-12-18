using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories.ConfigControl
{
    [TestClass]
    public class StatusRepositoryTests
    {
        [TestMethod]
        public async Task Status()
        {
            // Arrange
            var servicelog = new StatusRepository();

            // Act
            var status = await servicelog.GetStatus();

            // Assert
            Assert.IsTrue(status);
        }

    }
}
