using System.Collections.Generic;
using System.Data;
using System.Linq;
using ServiceLibrary.Helpers;
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

        public List<ApplicationSetting> GetLicenseInfo()
        {
            return _connectionWrapper.Query<ApplicationSetting>("GetLicenseInfo", commandType: CommandType.StoredProcedure).ToList();
        }
    }
}
