using ServiceLibrary.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories
{
    public interface IUsersRepository
    {
        Task<IEnumerable<UserInfo>> GetUserInfos(IEnumerable<int> userIds);
    }
}
