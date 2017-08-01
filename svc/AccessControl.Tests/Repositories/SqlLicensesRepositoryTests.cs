using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Models;
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
            DateTime now = new DateTime(2000, 1, 1);
            const int licenseLockTimeMinutes = 1440;
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlLicensesRepository(cxn.Object);
            var licenses = new[]
            {
                new LicenseInfo { LicenseLevel = 1, Count = 1 },
                new LicenseInfo { LicenseLevel = 3, Count = 2 }
            };
            cxn.SetupQueryAsync("[AdminStore].GetActiveLicenses", new Dictionary<string, object> { {"Now", now}, {"LicenseLockTimeMinutes", licenseLockTimeMinutes} }, licenses);

            // Act
            IEnumerable<LicenseInfo> result = await repository.GetActiveLicenses(now, 1440);

            // Assert
            cxn.Verify();
            Assert.AreEqual(result, licenses);
        }

        #endregion GetActiveLicenses

        #region GetLockedLicenses

        [TestMethod]
        public async Task GetLockedLicenses_QueryReturnsValue_ReturnsValue()
        {
            // Arrange
            const int userId = 1;
            const int licenseLevel = 3;
            const int lockTime = 1440;
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlLicensesRepository(cxn.Object);
            int result = 2;
            cxn.SetupExecuteScalarAsync(v => true, new Dictionary<string, object> { {"UserId", userId}, {"LicenseLevel", licenseLevel}, {"TimeDiff", -lockTime} }, result);

            // Act
            var count = await repository.GetLockedLicenses(userId, 3, 1440);

            // Assert
            cxn.Verify();
            Assert.AreEqual(result, count);
        }

        #endregion GetLockedLicenses

        #region GetLicenseTransactions

        [TestMethod]
        public async Task GetLicenseTransactions_QueryReturnsValue_ReturnsValue()
        {
            // Arrange
            DateTime startTime = new DateTime(2000, 1, 1);
            const int consumerType = 2;
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlLicensesRepository(cxn.Object);
            var transactions = new[]
            {
                new LicenseTransaction { LicenseActivityId = 1, UserId = 1, LicenseType = 1, TransactionType = 1, ActionType = 1, ConsumerType = 2 },
                new LicenseTransaction { LicenseActivityId = 2, UserId = 2, LicenseType = 3, TransactionType = 1, ActionType = 1, ConsumerType = 2 },
                new LicenseTransaction { LicenseActivityId = 3, UserId = 1, LicenseType = 1, TransactionType = 2, ActionType = 2, ConsumerType = 2 },
            };
            cxn.SetupQueryAsync("[AdminStore].GetLicenseTransactions", new Dictionary<string, object> { { "StartTime", startTime }, { "ConsumerType", consumerType } }, transactions);

            // Act
            IEnumerable<LicenseTransaction> result = await repository.GetLicenseTransactions(startTime, consumerType);

            // Assert
            cxn.Verify();
            Assert.AreEqual(result, transactions);
        }

        #endregion GetLicenseTransactions
    }
}
