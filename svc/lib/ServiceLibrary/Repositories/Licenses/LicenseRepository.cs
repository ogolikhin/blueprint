using System.Collections.Generic;
using System.Data;
using System.Linq;
using ServiceLibrary.Helpers;
using Dapper;
using ServiceLibrary.Models;

namespace ServiceLibrary.Repositories
{
    public class LicenseRepository : ILicenseRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;

        public LicenseRepository() : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        public LicenseRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public List<ApplicationSetting> GetAllApplicationSettings()
        {
            var prm = new DynamicParameters();
            prm.Add("@returnNonRestrictedOnly", false);
            return _connectionWrapper.Query<ApplicationSetting>("GetApplicationSettings", prm, commandType: CommandType.StoredProcedure).ToList();
        }

        public List<ApplicationSetting> GetLicenseInfo()
        {
            return _connectionWrapper.Query<ApplicationSetting>("GetLicenseInfo", commandType: CommandType.StoredProcedure).ToList();
        }

        public void UpdateLicenseInfo(string licenseInfoValue)
        {
            var prm = new DynamicParameters();
            prm.Add("@licenseInfoValue", licenseInfoValue);
            _connectionWrapper.ExecuteAsync("UpdateLicenseInfo", prm, commandType: CommandType.StoredProcedure);
        }
    }
}
