using System.Threading.Tasks;
using SearchService.Models;

namespace SearchService.Repositories
{
    public interface IItemSearchRepository
    {
        Task<FullTextSearchResultSet> SearchFullText(int userId, FullTextSearchCriteria searchCriteria, int page, int pageSize);
        Task<MetaDataSearchResultSet> FullTextMetaData(int userId, FullTextSearchCriteria searchCriteria);
        Task<ItemNameSearchResultSet> SearchName(int userId, ItemNameSearchCriteria searchCriteria, int startOffset, int pageSize, string separatorString);
    }
}
