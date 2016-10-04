using SearchService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SearchService.Repositories
{
    public interface IItemSearchRepository
    {
        Task<ItemSearchResult> FindItemByName(int userId, ItemSearchCriteria searchCriteria, int startOffset,
            int pageSize);
    }
}
