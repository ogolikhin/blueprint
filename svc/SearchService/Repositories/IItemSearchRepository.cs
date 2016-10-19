using System.Threading.Tasks;
using SearchService.Models;

namespace SearchService.Repositories
{
    public interface IItemSearchRepository
    {
        Task<FullTextSearchResult> Search(int userId, SearchCriteria searchCriteria, int page, int pageSize);
        Task<FullTextSearchMetaDataResult> SearchMetaData(int userId, SearchCriteria searchCriteria);
        Task<ItemSearchResult> SearchName(int userId, ItemSearchCriteria searchCriteria, int startOffset, int pageSize);
    }
}
