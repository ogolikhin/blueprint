using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AdminStore.Models;
using Dapper;
using ServiceLibrary.Repositories;

namespace AdminStore.Repositories
{
    public class ApplicationSettingsRepository : IApplicationSettingsRepository
    {
        internal readonly ISqlConnectionWrapper _connectionWrapper;

        public ApplicationSettingsRepository()
            : this(new SqlConnectionWrapper(WebApiConfig.RaptorMain))
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