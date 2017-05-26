using System;
using System.Threading.Tasks;
using AdminStore.Models;
using ServiceLibrary.Models;

namespace AdminStore.Repositories
{
    public interface IGroupRepository
    {
        Task<QueryResult<GroupDto>> GetGroupsAsync(int userId, TabularData tabularData, Func<Sorting, string> sort = null);
        Task<int> DeleteGroupsAsync(OperationScope body, string search);
        Task<int> AddGroupAsync(GroupDto group);
        Task<Group> GetGroupDetailsAsync(int groupId);
        Task UpdateGroupAsync(int groupId, GroupDto group);
        Task<QueryResult<GroupUser>> GetGroupUsersAsync(int groupId, TabularData tabularData, Func<Sorting, string> sort = null);
    }
}