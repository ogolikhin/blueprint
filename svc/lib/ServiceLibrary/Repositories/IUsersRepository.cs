using ServiceLibrary.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories
{
    public interface IUsersRepository
    {
        Task<IEnumerable<UserInfo>> GetUserInfos(IEnumerable<int> userIds);

        Task<IEnumerable<UserInfo>> GetUserInfosFromGroupsAsync(IEnumerable<int> groupIds);

        Task<IEnumerable<int>> FindNonExistentUsersAsync(IEnumerable<int> userIds);

        /// <summary>
        /// Returns list of users by e-mail.
        /// If the parameter set to NULL returns all users (regestered and guest).
        /// If the parameter set to false the method returns regestered users otherwise guest users only
        /// </summary>
        /// <param name="email"></param>
        /// <param name="guestsOnly"></param>
        /// <returns></returns>
        Task<IEnumerable<UserInfo>> GetUsersByEmail(string email, bool? guestsOnly = false);

        Task<bool> IsInstanceAdmin(bool contextUser, int sessionUserId);
    }
}
