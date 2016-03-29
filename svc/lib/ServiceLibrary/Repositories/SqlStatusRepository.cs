using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories
{
    public class SqlStatusRepository : IStatusRepository
    {
        public string Name { get; set; }

        internal readonly ISqlConnectionWrapper _connectionWrapper;
        private readonly string _cmd;

        public SqlStatusRepository(string cxn, string cmd, string name)
            : this(new SqlConnectionWrapper(cxn), cmd, name)
        {
        }

        internal SqlStatusRepository(ISqlConnectionWrapper connectionWrapper, string cmd, string name)
        {
            _connectionWrapper = connectionWrapper;
            _cmd = cmd;
            Name = name;
        }

        public async Task<string> GetStatus()
        {
            return (await _connectionWrapper.QueryAsync<string>(_cmd, commandType: CommandType.StoredProcedure)).Single();
        }
    }
}
