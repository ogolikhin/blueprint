using System.Threading.Tasks;
using SearchService.Models;

namespace SearchService.Repositories
{
    public interface IFullTextSearchRepository
    {
        Task<FullTextSearchResult> Search(int userId, SearchCriteria searchCriteria, int page, int pageSize);
        Task<FullTextSearchMetaDataResult> SearchMetaData(int userId, SearchCriteria searchCriteria);
    }
}