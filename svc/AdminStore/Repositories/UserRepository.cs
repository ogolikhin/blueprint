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
        public async Task<User> GetUserByLogin(string login)
        {
            using (var cxn = new SqlConnection(WebApiConfig.RaptorMain))
            {
                cxn.Open();
                var prm = new DynamicParameters();
                prm.Add("@Login", login);
                return (await cxn.QueryAsync<User>("GetUserByLogin", prm, commandType: CommandType.StoredProcedure)).FirstOrDefault();
            }
        } 
    }
}