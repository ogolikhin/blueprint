using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AdminStore.Models;

namespace AdminStore.Repositories
{
    public interface IGroupRepository
    {
        Task<QueryResult<GroupDto>> GetGroupsAsync(int userId, TabularData tabularData, Func<Sorting, string> sort = null);
    }
}