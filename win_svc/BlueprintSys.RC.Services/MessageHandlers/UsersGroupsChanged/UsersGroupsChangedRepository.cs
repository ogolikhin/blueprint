using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlueprintSys.RC.Services.MessageHandlers.UsersGroupsChanged
{
    public interface IUsersGroupsChangedRepository : IBaseRepository
    {
        /// <summary>
        /// TODO
        /// </summary>
        Task<List<int>> GetAffectedArtifactIds();
    }

    public class UsersGroupsChangedRepository : BaseRepository, IUsersGroupsChangedRepository
    {
        public UsersGroupsChangedRepository(string connectionString) : base(connectionString)
        {
        }

        public async Task<List<int>> GetAffectedArtifactIds()
        {
            //TODO
            return await Task.FromResult(new List<int>());
        }
    }
}
