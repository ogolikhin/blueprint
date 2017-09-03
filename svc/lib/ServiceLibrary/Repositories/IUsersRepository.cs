using ServiceLibrary.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories
{
    public interface IUsersRepository
    {
        Task<IEnumerable<UserInfo>> GetUserInfos(IEnumerable<int> userIds, IDbTransaction transaction = null);

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

        Task<IEnumerable<SqlGroup>> GetExistingGroupsByNamesAsync(IEnumerable<string> groupNames, bool instanceOnly);

        Task<IEnumerable<SqlGroup>> GetExistingGroupsByIds(IEnumerable<int> groupIds, bool instanceOnly, IDbTransaction transaction = null);

        Task<IEnumerable<SqlUser>> GetExistingUsersByNamesAsync(IEnumerable<string> userNames);

        Task<IEnumerable<SqlUser>> GetExistingUsersByIdsAsync(IEnumerable<int> userIds);

        Task<IEnumerable<UserInfo>> GetUserInfoForWorkflowArtifactForAssociatedUserProperty(int artifactId,
            int instancePropertyTypeId, 
            int revisionId, 
            IDbTransaction transaction = null);
    }
}
