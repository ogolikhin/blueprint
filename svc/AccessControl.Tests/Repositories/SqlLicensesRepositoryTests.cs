using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Repositories;

namespace AccessControl.Repositories
{
    [TestClass]
    public class SqlLicensesRepositoryTests
    {
        #region GetActiveLicenses

        [TestMethod]
        public async Task GetActiveLicenses_QueryReturnsValue_ReturnsValue()
        {
            // Arrange
            const int userId = 1;
            const int licenseLevel = 3;
            const int lockTime = 1440;

            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlLicensesRepository(cxn.Object);
            int result = 2;
            cxn.SetupExecuteScalarAsync(v => true,
                new Dictionary<string, object>
                {
                    {"UserId", userId},
                    {"LicenseLevel", licenseLevel},
                    {"TimeDiff", -lockTime}
                }, result);

            // Act
            var count = await repository.GetActiveLicenses(userId, 3, 1440);

            // Assert
            cxn.Verify();
            Assert.AreEqual(result, count);
        }

        #endregion
    }
}
