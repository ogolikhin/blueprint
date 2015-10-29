using System;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using ConfigControl.Models;
using System.Collections.Generic;

namespace ConfigControl.Repositories
{
	public class SqlConfigRepository : IConfigRepository
	{
		public virtual async Task<IEnumerable<ConfigSetting>> GetSettings(bool allowRestricted)
		{
			using (var cxn = new SqlConnection(WebApiConfig.AdminStorage))
			{
				cxn.Open();
				var prm = new DynamicParameters();
				prm.Add("@AllowRestricted", allowRestricted);
				return await cxn.QueryAsync<ConfigSetting>("GetSettings", prm, commandType: CommandType.StoredProcedure);
			}
		}
	}
}