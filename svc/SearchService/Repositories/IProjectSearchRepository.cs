using SearchService.Models;
using System.Threading.Tasks;

namespace SearchService.Repositories
{
    public interface IProjectSearchRepository
    {
        Task<ProjectSearchResultSet> SearchName(
            int userId,
            SearchCriteria searchCriteria,
            int resultCount,
            string separatorString);
    }
}
