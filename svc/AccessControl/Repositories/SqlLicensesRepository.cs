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
	}
}
