using System.Data;
using System.Data.SqlClient;
using System.Linq;
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

        public async Task UpdateUserOnInvalidLogin(LoginUser user)
        {
            using (var cxn = CreateDbConnection())
            {
                cxn.Open();
                var prm = new DynamicParameters();
                prm.Add("@Login", user.Login);
                prm.Add("@Enabled", user.IsEnabled);
                prm.Add("@InvalidLogonAttemptsNumber", user.InvalidLogonAttemptsNumber);
                prm.Add("@LastInvalidLogonTimeStamp", user.LastInvalidLogonTimeStamp);
                await cxn.ExecuteAsync("UpdateUserOnInvalidLogin", prm, commandType: CommandType.StoredProcedure);
            }
        }

        private IDbConnection CreateDbConnection()
        {
            return new SqlConnection(WebApiConfig.RaptorMain);
        }
    }
}