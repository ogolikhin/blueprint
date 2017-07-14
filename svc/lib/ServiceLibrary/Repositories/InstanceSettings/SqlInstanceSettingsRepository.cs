using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace ServiceLibrary.Repositories
{
    public class SqlInstanceSettingsRepository : IInstanceSettingsRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;

        public SqlInstanceSettingsRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        internal SqlInstanceSettingsRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public async Task<EmailSettings> GetEmailSettings()
        {
            var result = (await _connectionWrapper.QueryAsync<dynamic>("GetInstanceEmailSettings", commandType: CommandType.StoredProcedure)).FirstOrDefault();
            return result == null ? null : EmailSettings.CreateFromString(result.EmailSettings);
        }
    }
}
