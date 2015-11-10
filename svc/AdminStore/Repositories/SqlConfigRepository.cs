using System.Data;
using System.Threading.Tasks;
using Dapper;
using AdminStore.Models;
using System.Collections.Generic;
using ServiceLibrary.Repositories;

namespace AdminStore.Repositories
{
    public class SqlConfigRepository : IConfigRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;

        public SqlConfigRepository()
            : this(new SqlConnectionWrapper(WebApiConfig.AdminStorage))
        {
            
        }

        internal SqlConfigRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public virtual async Task<IEnumerable<ApplicationLabel>> GetLabels(string locale)
        {
            var prm = new DynamicParameters();
            prm.Add("@Locale", locale);
            return await _connectionWrapper.QueryAsync<ApplicationLabel>("GetApplicationLabels", prm, commandType: CommandType.StoredProcedure);
        }
    }
}
