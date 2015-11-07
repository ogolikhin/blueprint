using System;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using AdminStore.Models;
using Dapper;

namespace AdminStore.Repositories
{
    public class UserRepository : IUserRepository
    {
        public async Task<LoginUser> GetUserByLogin(string login)
        {
            using (var cxn = CreateDbConnection())
            {
                cxn.Open();
                var prm = new DynamicParameters();
                prm.Add("@Login", login);
                return (await cxn.QueryAsync<LoginUser>("GetUserByLogin", prm, commandType: CommandType.StoredProcedure)).FirstOrDefault();
            }
        }

        public async Task UpdateUserOnInvalidLogin(string login, bool isEnabled, int invalidLogonAttempts, DateTime? lastInvalidLogonTimeStamp)
        {
            using (var cxn = CreateDbConnection())
            {
                cxn.Open();
                var prm = new DynamicParameters();
                prm.Add("@Login", login);
                prm.Add("@Enabled", isEnabled);
                prm.Add("@InvalidLogonAttemptsNumber", invalidLogonAttempts);
                prm.Add("@LastInvalidLogonTimeStamp", lastInvalidLogonTimeStamp);
                await cxn.ExecuteAsync("UpdateUserOnInvalidLogin", prm, commandType: CommandType.StoredProcedure);
            }
        }

        private IDbConnection CreateDbConnection()
        {
            return new SqlConnection(WebApiConfig.RaptorMain);
        }
    }
}