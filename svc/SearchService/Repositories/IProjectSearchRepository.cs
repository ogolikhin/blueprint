using SearchService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SearchService.Repositories
{
    public interface IProjectSearchRepository
    {
        Task<IEnumerable<ProjectSearchResult>> GetProjectsByName(int userId, string searchText, int resultCount);        
    }
}
