using System.Threading.Tasks;
using AdminStore.Helpers;
using AdminStore.Models;

namespace AdminStore.Repositories
{
    public interface IAuthenticationRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<LoginUser> AuthenticateUser(string login, string password);
    }
}
