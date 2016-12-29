using ServiceLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories
{
    public class SqlUserRepositoryMock: IUsersRepository
    {
        public async Task<IEnumerable<UserInfo>> GetUserInfos(IEnumerable<int> userIds)
        {
            var result = new List<UserInfo>();
            foreach (int userId in userIds)
            {
                result.Add(new UserInfo
                           {
                                UserId = userId,
                                DisplayName = "User"+userId,
                                ImageId = userId,
                                IsEnabled = true,
                                IsGuest = false
                           });
            }
            return await Task.FromResult(result);
        }
        public async Task<IEnumerable<UserInfo>> GetUsersByEmail(string email, bool? guestsOnly = false)
        {
            if (email == "DisabledUser@MyDomain")
            {
                return await Task.FromResult(new List<UserInfo> { new UserInfo
                           {
                                UserId = 1,
                                DisplayName = "User1",
                                ImageId = 1,
                                IsEnabled = false,
                                IsGuest = true
                           }});

            }
            return await Task.FromResult(new List<UserInfo> { new UserInfo
                           {
                                UserId = 1,
                                DisplayName = "User1",
                                ImageId = 1,
                                IsEnabled = true,
                                IsGuest = false
                           }});
        }
        public async Task<bool> IsInstanceAdmin(bool contextUser, int sessionUserId)
        {
            return await Task.FromResult(true);
        }
    }
}
