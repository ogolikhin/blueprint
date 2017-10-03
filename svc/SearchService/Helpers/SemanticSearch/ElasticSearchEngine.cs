using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nest;
using SearchService.Models;
using SearchService.Repositories;

namespace SearchService.Helpers.SemanticSearch
{
    public sealed class ElasticSearchEngine: SearchEngine
    {
        private const string IndexPrefix = "semanticsdb_";
        private string IndexName { get; }

        private IElasticClient _elasticClient;

        public ElasticSearchEngine(string connectionString, string tenantId, ISemanticSearchRepository semanticSearchRepository) 
            : base(semanticSearchRepository)
        {
            IndexName = IndexPrefix + tenantId;

            // create ElasticClient using conneciton string
            var connectionSettings = new ConnectionSettings(new Uri(connectionString)).DefaultIndex(IndexName);
            _elasticClient = new ElasticClient(connectionSettings);
            
        }

        public override async Task<IEnumerable<ArtifactSearchResult>> GetSemanticSearchSuggestions(SearchEngineParameters searchEngineParameters)
        {
            var searchText = await SemanticSearchRepository.GetSemanticSearchText(searchEngineParameters.ArtifactId,
                searchEngineParameters.UserId);
            // use searchText to execute Elasticsearch query
            var itemIds = new List<int>();
            
            _elasticClient.Search<object>(
                s => s.From(0).Size(10).Query(
                    q => q.MoreLikeThis(
                        m => m.Fields(
                            f => f.Field("SearchColumn")).Like( l => l.Text(searchText)))));

            // parse the artifact ids into a artifactsearchresult to return to the caller
            return await GetArtifactSearchResultsFromItemIds(itemIds, searchEngineParameters.UserId, searchEngineParameters.ArtifactId);
        }
    }
}