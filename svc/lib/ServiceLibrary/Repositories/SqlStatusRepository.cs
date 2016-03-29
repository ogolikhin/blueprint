using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories
{
    public class SqlStatusRepository : IStatusRepository
    {
        public string Name { get; set; }

        internal readonly ISqlConnectionWrapper _connectionWrapper;

        public SqlStatusRepository(string cxn, string name)
            : this(new SqlConnectionWrapper(cxn), name)
        {
        }

        internal SqlStatusRepository(ISqlConnectionWrapper connectionWrapper, string name)
        {
            _connectionWrapper = connectionWrapper;
            Name = name;
        }

        public async Task<string> GetStatus(int timeout)
        {
            return (await _connectionWrapper.QueryAsync<string>("GetStatus", commandType: CommandType.StoredProcedure, commandTimeout: timeout)).Single();
        }
    }
}
