using System.Collections.Generic;
using System.Threading.Tasks;
using SearchEngineLibrary.Repository;

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

        public async Task<IEnumerable<int>> GetArtifactIds()
        {
            return await _searchEngineRepository.GetArtifactIds();
        }
    }
}
