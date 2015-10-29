using System;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using AdminStore.Models;
using System.Collections.Generic;

namespace AdminStore.Repositories
{
	public class SqlConfigRepository : IConfigRepository
	{
		public virtual async Task<IEnumerable<ApplicationLabel>> GetLabels(string locale)
		{
			using (var cxn = new SqlConnection(WebApiConfig.AdminStorage))
			{
				cxn.Open();
				var prm = new DynamicParameters();
				prm.Add("@Locale", locale);
				return await cxn.QueryAsync<ApplicationLabel>("GetLabels", prm, commandType: CommandType.StoredProcedure);
			}
		}
	}
}