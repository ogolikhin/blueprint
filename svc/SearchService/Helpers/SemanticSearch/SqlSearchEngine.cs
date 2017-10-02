using System.Collections.Generic;
using System.Threading.Tasks;
using SearchService.Models;
using SearchService.Repositories;

namespace SearchService.Helpers.SemanticSearch
{
    public class SqlSearchEngine: SearchEngine
    {
        public SqlSearchEngine(ISemanticSearchRepository semanticSearchRepository) : base (semanticSearchRepository)
        {
        }

        public override async Task<IEnumerable<ArtifactSearchResult>> GetSemanticSearchSuggestions(SearchEngineParameters searchEngineParameters)
        {
            var itemIds = new List<int>();
            
            return await GetArtifactSearchResultsFromItemIds(itemIds, searchEngineParameters.UserId, searchEngineParameters.ArtifactId);
        }
    }
}