using System.Collections.Generic;
using System.Threading.Tasks;
using SearchService.Models;
using SearchService.Repositories;

namespace SearchService.Helpers.SemanticSearch
{
    public sealed class ElasticSearchEngine: SearchEngine
    {
        private const string IndexPrefix = "semanticsdb_";
        private string IndexName { get; }
        

        public ElasticSearchEngine(string connectionString, string tenantId, ISemanticSearchRepository semanticSearchRepository) 
            : base(semanticSearchRepository)
        {
            IndexName = IndexPrefix + tenantId;
            // create ElasticClient using conneciton string
        }

        public override async Task<IEnumerable<ArtifactSearchResult>> GetSemanticSearchSuggestions(SearchEngineParameters searchEngineParameters)
        {
            var searchText = await SemanticSearchRepository.GetSemanticSearchText(searchEngineParameters.ArtifactId,
                searchEngineParameters.UserId);
            // use searchText to execute Elasticsearch query
            var itemIds = new List<int>();
            // parse the artifact ids into a artifactsearchresult to return to the caller
            return await GetArtifactSearchResultsFromItemIds(itemIds, searchEngineParameters.UserId, searchEngineParameters.ArtifactId);
        }
    }
}