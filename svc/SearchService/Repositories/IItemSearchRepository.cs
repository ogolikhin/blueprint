using System.Threading.Tasks;
using SearchService.Models;

namespace SearchService.Repositories
{
    public interface IItemSearchRepository
    {
        Task<FullTextSearchResultSet> SearchFullText(int userId, ItemSearchCriteria searchCriteria, int page, int pageSize);
        Task<MetaDataSearchResultSet> FullTextMetaData(int userId, ItemSearchCriteria searchCriteria);
        Task<ItemNameSearchResultSet> SearchName(int userId, ItemSearchCriteria searchCriteria, int startOffset, int pageSize);
    }
}
