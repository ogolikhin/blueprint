using System.Collections.Generic;
using System.Threading.Tasks;

using SearchEngineLibrary.Repository;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;

namespace SearchEngineLibrary.Service
{
    public class SearchEngineService : ISearchEngineService
    {
        private readonly ISearchEngineRepository _searchEngineRepository;

        public SearchEngineService() : this(new SearchEngineRepository())
        {
        }

        internal SearchEngineService(ISearchEngineRepository searchEngineRepository)
        {
            _searchEngineRepository = searchEngineRepository;
        }

        public async Task<IEnumerable<int>> GetChildrenArtifactIdsByCollectionId(int scopeId, Pagination pagination, ScopeType scopeType, bool includeDraft, int userId)
        {
            return await _searchEngineRepository.GetChildrenArtifactIdsByCollectionId(scopeId, pagination, scopeType, includeDraft, userId);
        }
    }
}
