using System.Data;
using System.Threading.Tasks;
using Dapper;
using ConfigControl.Models;
using System.Collections.Generic;
using ServiceLibrary.Repositories;

namespace ConfigControl.Repositories
{
    public class SqlConfigRepository : IConfigRepository
    {
        private readonly ISqlConnectionWrapper _cxn;

        public SqlConfigRepository()
            : this(new SqlConnectionWrapper(WebApiConfig.AdminStorage))
        {
        }

        internal SqlConfigRepository(ISqlConnectionWrapper cxn)
        {
            _cxn = cxn;
        }

        public virtual async Task<IEnumerable<ConfigSetting>> GetSettings(bool allowRestricted)
        {
            var prm = new DynamicParameters();
            prm.Add("@AllowRestricted", allowRestricted);
            return await _cxn.QueryAsync<ConfigSetting>("GetConfigSettings", prm, commandType: CommandType.StoredProcedure);
        }
    }
}
