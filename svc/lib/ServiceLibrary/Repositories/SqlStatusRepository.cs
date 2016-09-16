using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories
{
    public class SqlStatusRepository : IStatusRepository
    {
        public string Name { get; set; }
        public string AccessInfo { get; set; }

        internal readonly ISqlConnectionWrapper _connectionWrapper;

        public SqlStatusRepository(string cxn, string name)
            : this(new SqlConnectionWrapper(cxn), cxn, name)
        {
        }

        internal SqlStatusRepository(ISqlConnectionWrapper connectionWrapper, string accessInfo, string name)
        {
            _connectionWrapper = connectionWrapper;
            Name = name;
            AccessInfo = accessInfo;
        }

        public async Task<string> GetStatus(int timeout)
        {
            return (await _connectionWrapper.QueryAsync<string>("GetStatus", commandType: CommandType.StoredProcedure, commandTimeout: timeout)).Single();
        }
    }
}
