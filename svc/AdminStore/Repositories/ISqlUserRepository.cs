using System.Threading.Tasks;
using AdminStore.Models;

namespace AdminStore.Repositories
{
    public interface ISqlUserRepository
    {
        Task<LoginUser> GetUserByLoginAsync(string login);

        Task UpdateUserOnInvalidLoginAsync(LoginUser login);
    }
}