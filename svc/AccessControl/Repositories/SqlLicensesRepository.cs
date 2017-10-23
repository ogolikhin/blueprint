using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;

namespace AccessControl.Repositories
{
    public class SqlLicensesRepository : ILicensesRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;

        public SqlLicensesRepository()
            : this(new SqlConnectionWrapper(WebApiConfig.AdminStorage))
        {
        }

        internal SqlLicensesRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public Task<IEnumerable<LicenseInfo>> GetActiveLicenses(DateTime now, int licenseLockTimeMinutes)
        {
            var prm = new DynamicParameters();
            prm.Add("@Now", now);
            prm.Add("@LicenseLockTimeMinutes", licenseLockTimeMinutes);

            return _connectionWrapper.QueryAsync<LicenseInfo>("[AdminStore].GetActiveLicenses", prm, commandType: CommandType.StoredProcedure);
        }

        public Task<int> GetLockedLicenses(int excludeUserId, int licenseLevel, int licenseLockTimeMinutes)
        {
            var prm = new DynamicParameters();
            prm.Add("@TimeUtc", DateTime.UtcNow);
            prm.Add("@UserId", excludeUserId);
            prm.Add("@LicenseLevel", licenseLevel);
            prm.Add("@TimeDiff", -licenseLockTimeMinutes);

            return _connectionWrapper.ExecuteScalarAsync<int>(
                @"SELECT COUNT(*) FROM [AdminStore].[Sessions] 
                WHERE LicenseLevel = @LicenseLevel AND UserId <> @UserId AND 
                (EndTime IS NULL OR EndTime > DATEADD(MINUTE, @TimeDiff, @TimeUtc) )",
                prm);
        }

        public Task<IEnumerable<LicenseTransaction>> GetLicenseTransactions(DateTime startTime, int consumerType)
        {
            var prm = new DynamicParameters();
            prm.Add("@StartTime", startTime);
            prm.Add("@ConsumerType", consumerType);

            return _connectionWrapper.QueryAsync<LicenseTransaction>("[AdminStore].GetLicenseTransactions", prm, commandType: CommandType.StoredProcedure);
        }

        public async Task<LicenseUsage> GetLicenseUsage(int? month, int? year)
        {

            var prm = new DynamicParameters();
            prm.Add("@month", month);
            prm.Add("@year", year);
            var usage = new LicenseUsage();
            usage.Summary = await _connectionWrapper.QueryAsync<LicenseUsageSummary>("[AdminStore].GetLicenseUsage", prm, commandType: CommandType.StoredProcedure);
            usage.UserActivities = await _connectionWrapper.QueryAsync<LicenseUserActivity>("[AdminStore].GetLicenseUserActivity", prm, commandType: CommandType.StoredProcedure);

            return usage;
        }
    }
}
