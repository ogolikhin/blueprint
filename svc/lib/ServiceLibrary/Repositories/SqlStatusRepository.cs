using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories
{
    public class SqlStatusRepository : IStatusRepository
    {
        internal readonly ISqlConnectionWrapper _connectionWrapper;
        private readonly string _cmd;

        public SqlStatusRepository(string cxn, string cmd)
            : this(new SqlConnectionWrapper(cxn), cmd)
        {
        }

        internal SqlStatusRepository(ISqlConnectionWrapper connectionWrapper, string cmd)
        {
            _connectionWrapper = connectionWrapper;
            _cmd = cmd;
        }

        public async Task<bool> GetStatus()
        {
            return (await _connectionWrapper.QueryAsync<int>(_cmd, commandType: CommandType.StoredProcedure)).Single() >= 0;
        }
    }
}
