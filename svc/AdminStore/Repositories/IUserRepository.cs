using System.Threading.Tasks;
using AdminStore.Models;

namespace AdminStore.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserByLogin(string login);
    }
}