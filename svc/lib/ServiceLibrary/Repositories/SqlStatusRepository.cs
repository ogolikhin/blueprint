using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace ServiceLibrary.Repositories
{
	public class SqlStatusRepository : IStatusRepository
	{
		private readonly string _cxn;
		private readonly string _cmd;

		public SqlStatusRepository(string cxn, string cmd)
		{
			_cxn = cxn;
			_cmd = cmd;
		}

		public async Task<bool> GetStatus()
		{
			using (var cxn = new SqlConnection(_cxn))
			{
				cxn.Open();
				return (await cxn.QueryAsync<int>(_cmd, commandType: CommandType.StoredProcedure)).Single() >= 0;
			}
		}
	}
}
