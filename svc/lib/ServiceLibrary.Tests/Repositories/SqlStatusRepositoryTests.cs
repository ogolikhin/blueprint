using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServiceLibrary.Repositories
{
    [TestClass]
    public class SqlStatusRepositoryTests
    {
        #region GetStatus

        [TestMethod]
        public async Task GetStatus_QueryReturnsNonNegative_ReturnsTrue()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var cmd = "Test";
            var repository = new SqlStatusRepository(cxn.Object, cmd);
            IEnumerable<int> result = new[] { 0 };
            cxn.SetupQueryAsync(cmd, null, result);

            // Act
            bool status = await repository.GetStatus();

            // Assert
            cxn.Verify();
            Assert.IsTrue(status);
        }

        [TestMethod]
        public async Task GetStatus_QueryReturnsNegative_ReturnsFalse()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var cmd = "cmd";
            var repository = new SqlStatusRepository(cxn.Object, cmd);
            IEnumerable<int> result = new[] { -1 };
            cxn.SetupQueryAsync(cmd, null, result);

            // Act
            bool status = await repository.GetStatus();

            // Assert
            cxn.Verify();
            Assert.IsFalse(status);
        }

        #endregion GetStatus
    }
}
