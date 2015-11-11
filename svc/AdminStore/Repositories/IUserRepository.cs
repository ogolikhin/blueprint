using System.Threading.Tasks;
using AdminStore.Models;

namespace AdminStore.Repositories
{
    public interface IUserRepository
    {
        Task<LoginUser> GetUserByLoginAsync(string login);

        Task UpdateUserOnInvalidLoginAsync(LoginUser login);
    }
}