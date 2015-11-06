//using System.Collections.Generic;
//using System.Data;
//using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;

namespace ServiceLibrary.Repositories
{
    [TestClass]
    public class SqlStatusRepositoryTests
    {
        //[TestMethod]
        //public async Task GetStatus_QueryReturnsNonNegative_ReturnsTrue()
        //{
        //    // Arrange
        //    var cmd = "Test";
        //    IEnumerable<int> result = new[] {0};
        //    var cxn = new Mock<IDbConnectionWrapper>();
        //    cxn.Setup(c => c.QueryAsync<int>(cmd, null, It.IsAny<IDbTransaction>(), It.IsAny<int?>(), CommandType.StoredProcedure)).Returns(Task.FromResult(result));
        //    var repository = new SqlStatusRepository(cxn.Object, cmd);

        //    // Act
        //    bool status = await repository.GetStatus();

        //    // Assert
        //    Assert.IsTrue(status);
        //}

        //[TestMethod]
        //public async Task GetStatus_QueryReturnsNegative_ReturnsFalse()
        //{
        //    // Arrange
        //    var cmd = "Test";
        //    IEnumerable<int> result = new[] {-1};
        //    var cxn = new Mock<IDbConnectionWrapper>();
        //    cxn.Setup(c => c.QueryAsync<int>(cmd, null, It.IsAny<IDbTransaction>(), It.IsAny<int?>(), CommandType.StoredProcedure)).Returns(Task.FromResult(result));
        //    var repository = new SqlStatusRepository(cxn.Object, cmd);

        //    // Act
        //    bool status = await repository.GetStatus();

        //    // Assert
        //    Assert.IsFalse(status);
        //}
    }
}
