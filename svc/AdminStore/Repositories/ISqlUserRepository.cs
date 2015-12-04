using System.Threading.Tasks;
using AdminStore.Models;

namespace AdminStore.Repositories
{
    public interface ISqlUserRepository
    {
        Task<LoginUser> GetUserByLoginAsync(string login);

        Task<LoginUser> GetLoginUserByIdAsync(int userId);

	    Task<int> GetEffectiveUserLicenseAsync(int userId);

		Task UpdateUserOnInvalidLoginAsync(LoginUser login);
    }
}