using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using AdminStore.Models;
using ServiceLibrary.Repositories;
using ServiceLibrary.Helpers;

namespace AdminStore.Repositories
{
    public class ApplicationSettingsRepository : IApplicationSettingsRepository
    {
        internal readonly ISqlConnectionWrapper _connectionWrapper;

        public ApplicationSettingsRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        internal ApplicationSettingsRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public virtual async Task<IEnumerable<ApplicationSetting>> GetSettings()
        {
            try
            {
                return await _connectionWrapper.QueryAsync<ApplicationSetting>("GetApplicationSettings", null, commandType: CommandType.StoredProcedure);

            }
            catch (Exception ex)
            {
                throw ex;
                
                
            }
        }
    }
}