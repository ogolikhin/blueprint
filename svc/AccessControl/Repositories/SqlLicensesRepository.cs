using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using System.Collections.Generic;
using ServiceLibrary.Repositories;
using ServiceLibrary.Models;

namespace AccessControl.Repositories
{
    public class SqlLicensesRepository : ILicensesRepository
    {
        internal readonly ISqlConnectionWrapper _connectionWrapper;

        public SqlLicensesRepository(string cxn)
            : this(new SqlConnectionWrapper(cxn))
        {
        }

        internal SqlLicensesRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

		public Task<IEnumerable<LicenseInfo>> GetLicensesStatus(int licenseLockTimeMinutes)
        {
            var prm = new DynamicParameters();
			prm.Add("@TimeUtc", DateTime.UtcNow);
			prm.Add("@TimeDiff", -licenseLockTimeMinutes);
            return _connectionWrapper.QueryAsync<LicenseInfo>("GetLicensesStatus", prm, commandType: CommandType.StoredProcedure);
        }

		public Task<int> GetActiveLicenses(int excludeUserId, int licenseLevel, int licenseLockTimeMinutes)
		{
			var prm = new DynamicParameters();
			prm.Add("@TimeUtc", DateTime.UtcNow);
			prm.Add("@UserId", excludeUserId);
			prm.Add("@LicenseLevel", licenseLevel);
			prm.Add("@TimeDiff", -licenseLockTimeMinutes);

			return _connectionWrapper.ExecuteScalarAsync<int>(
				@"SELECT COUNT(*) FROM [dbo].[Sessions] 
				WHERE LicenseLevel = @LicenseLevel AND UserId <> @UserId AND 
				(EndTime IS NULL OR EndTime > DATEADD(MINUTE, @TimeDiff, @TimeUtc) )",
				prm);
		}
	}
}
