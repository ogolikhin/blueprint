﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdminStore.Models;
using ServiceLibrary.Models;

namespace AdminStore.Repositories
{
    public interface IGroupRepository
    {
        Task<QueryResult<GroupDto>> GetGroupsAsync(int userId, TabularData tabularData, Func<Sorting, string> sort = null);
        Task<List<int>> DeleteGroupsAsync(OperationScope body, string search);
        Task<int> AddGroupAsync(GroupDto group);
        Task<GroupDto> GetGroupDetailsAsync(int groupId);
        Task UpdateGroupAsync(int groupId, GroupDto group);
        Task<QueryResult<GroupUser>> GetGroupUsersAsync(int groupId, TabularData tabularData, Func<Sorting, string> sort = null);
        Task<QueryResult<GroupUser>> GetGroupMembersAsync(int groupId, TabularData tabularData, Func<Sorting, string> sort = null);
        Task<int> DeleteMembersFromGroupAsync(int groupId, AssignScope body);
        Task<int> AssignMembers(int groupId, AssignScope scope, string search = null);
        Task<QueryResult<GroupDto>> GetProjectGroupsAsync(int projectId, TabularData tabularData,
             Func<Sorting, string> sort = null);

    }
}