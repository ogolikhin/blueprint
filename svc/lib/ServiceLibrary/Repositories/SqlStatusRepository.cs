using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Repositories
{
    public class SqlStatusRepository : IStatusRepository
    {
        private readonly string _dbSchema;
        internal readonly ISqlConnectionWrapper _connectionWrapper;

        public string Name { get; set; }

        public string AccessInfo { get; set; }

        public SqlStatusRepository(string cxn, string name, string dbSchema = ServiceConstants.DefaultDBSchema)
            : this(new SqlConnectionWrapper(cxn), cxn, name)
        {
            _dbSchema = dbSchema;
        }

        internal SqlStatusRepository(ISqlConnectionWrapper connectionWrapper, string accessInfo, string name)
        {
            _connectionWrapper = connectionWrapper;
            Name = name;
            AccessInfo = accessInfo;
        }

        public async Task<string> GetStatus(int timeout)
        {
            return (await _connectionWrapper.QueryAsync<string>($"{_dbSchema}.GetStatus", commandType: CommandType.StoredProcedure, commandTimeout: timeout)).Single();
        }
    }
}
