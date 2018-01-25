using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ServiceLibrary.Repositories;

namespace BlueprintSys.RC.Services.MessageHandlers.UsersGroupsChanged
{
    public interface IUsersGroupsChangedRepository : IBaseRepository
    {
        /// <summary>
        /// Calls the stored procedure [dbo].[GetSearchItemsUsersGroupsChangeArtifactIds]
        /// </summary>
        Task<List<int>> GetAffectedArtifactIds(IEnumerable<int> userIds, IEnumerable<int> groupIds, int revisionId);
    }

    public class UsersGroupsChangedRepository : BaseRepository, IUsersGroupsChangedRepository
    {
        public UsersGroupsChangedRepository(string connectionString) : base(connectionString)
        {
        }

        public async Task<List<int>> GetAffectedArtifactIds(IEnumerable<int> userIds, IEnumerable<int> groupIds, int revisionId)
        {
            var param = new DynamicParameters();
            param.Add("@userIds", SqlConnectionWrapper.ToDataTable(userIds ?? new int[0]));
            param.Add("@groupIds", SqlConnectionWrapper.ToDataTable(groupIds ?? new int[0]));
            param.Add("@revisionId", revisionId);
            return (await ConnectionWrapper.QueryAsync<int>("[dbo].[GetSearchItemsUsersGroupsChangeArtifactIds]", param, commandType: CommandType.StoredProcedure)).ToList();
        }
    }
}
