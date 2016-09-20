using System.Threading.Tasks;
using SearchService.Models;

namespace SearchService.Repositories
{
    public interface IFullTextSearchRepository
    {
        Task<FullTextSearchResult> Search(SearchCriteria searchCriteria, int page, int pageSize);
        Task<FullTextSearchMetaDataResult> SearchMetaData(SearchCriteria searchCriteria);
    }
}