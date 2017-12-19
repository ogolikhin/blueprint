using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SearchEngineLibrary.Model;
using SearchEngineLibrary.Repository;

namespace SearchEngineLibrary.Service
{
    public class SearchEngineService : ISearchEngineService
    {
        private readonly ISearchEngineRepository _searchEngineRepository;

        public SearchEngineService() : this(new SearchEngineRepository())
        {
            
        }

        public SearchEngineService(ISearchEngineRepository searchEngineRepository)
        {
            _searchEngineRepository = searchEngineRepository;
        }

        public async Task<int> GetCountArtifactIdsSearchItems()
        {
            return await _searchEngineRepository.GetCountArtifactIdsSearchItems();
        }
    }
}
