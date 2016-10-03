using ArtifactStore.Models;
using SearchService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SearchService.Repositories
{
    public interface IItemSearchRepository
    {        
        Task<IEnumerable<ItemSearchResult>> FindItemByName(int userId, string searchText, int[] projectIds, int[] itemTypes, int resultCount);
    }
}
