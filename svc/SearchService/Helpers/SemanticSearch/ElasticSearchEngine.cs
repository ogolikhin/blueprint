using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nest;
using SearchService.Models;
using SearchService.Repositories;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace SearchService.Helpers.SemanticSearch
{
    [ElasticsearchType(Name = "semantic_search_items")]
    public class SemanticSearchItem
    {
        [Number(Name = "project_id")]
        public int ProjectId { get; set; }
        [Number(Name = "end_revision")]
        public int EndRevision { get; set; }
        [Number(Name = "latest_changing_revision")]
        public int LatestChangingRevision { get; set; }
        [Text(Name = "name", Analyzer = "blueprint_analyzer")]
        public string Name { get; set; }
        [Text(Name = "search_text", Analyzer = "blueprint_analyzer")]
        public string SearchText { get; set; }
    }
    public sealed class ElasticSearchEngine : SearchEngine
    {
        private readonly IElasticClient _elasticClient;
        private const string IdFieldKey = "_id";

        public ElasticSearchEngine(string connectionString, ISemanticSearchRepository semanticSearchRepository)
            : base(semanticSearchRepository)
        {
            // create ElasticClient using conneciton string
            var connectionSettings = new ConnectionSettings(new Uri(connectionString));

            _elasticClient = new ElasticClient(connectionSettings);
        }

        internal ElasticSearchEngine(IElasticClient elasticClient, ISemanticSearchRepository semanticSearchRepository)
            : base(semanticSearchRepository)
        {
            _elasticClient = elasticClient;
        }

        public override void PerformHealthCheck()
        {
            var ping = _elasticClient.Ping();
            if (!ping.IsValid)
            {
                throw new ElasticsearchConfigurationException("Could not connect to elasticsearch connection string. Please check your elasticsearch connection.");
            }
        }

        private void PerformIndexHealthCheck(string indexName)
        {
            var doesIndexExists = _elasticClient.IndexExists(indexName);
            if (!doesIndexExists.Exists)
            {
                throw new ElasticsearchConfigurationException("Elasticsearch Index does not exist");
            }

            var doesTypeExists = _elasticClient.TypeExists(indexName, typeof(SemanticSearchItem));
            if (!doesTypeExists.Exists)
            {
                throw new ElasticsearchConfigurationException("Elasticsearch Type does not exist");
            }
        }

        public override async Task<IEnumerable<ArtifactSearchResult>> GetSemanticSearchSuggestions(SearchEngineParameters searchEngineParameters)
        {
            try
            {
                var index = await SemanticSearchRepository.GetSemanticSearchIndex();
                if (String.IsNullOrEmpty(index))
                {
                    // Returning empty results when index has not been created yet.
                    return new List<ArtifactSearchResult>();
                }
                PerformIndexHealthCheck(index);

                // Setting default index name on the connection, otherwise the search request will fail
                _elasticClient.ConnectionSettings.DefaultIndices.Clear();
                _elasticClient.ConnectionSettings.DefaultIndices.Add(typeof(SemanticSearchItem), index);

                var searchText = await SemanticSearchRepository.GetSemanticSearchText(searchEngineParameters.ArtifactId,
                    searchEngineParameters.UserId);

                // Create the bool query descripter that just searchs for the searchText we constructed
                var boolQueryDescriptor = new BoolQueryDescriptor<SemanticSearchItem>();
                boolQueryDescriptor.Must(GetMoreLikeThisQuery(searchText));

                // Dont return back result for the current artifact id we're searching against
                boolQueryDescriptor.MustNot(GetArtifactIdMatchQuery(searchEngineParameters.ArtifactId));

                // If not instance admin, use the list of accessible project ids to filter out, otherwise no need to filter
                if (!searchEngineParameters.IsInstanceAdmin)
                {
                    boolQueryDescriptor.Filter(GetContainsProjectIdsQuery(searchEngineParameters.AccessibleProjectIds));
                }

                // Creates the search descriptor
                var searchDescriptor = new SearchDescriptor<SemanticSearchItem>();
                searchDescriptor.Index(index).Size(searchEngineParameters.PageSize).Query(q => q.Bool(b => boolQueryDescriptor));
                var results = await _elasticClient.SearchAsync<SemanticSearchItem>(searchDescriptor);

                var hits = results.Hits;
                var itemIds = new List<int>();
                hits.ForEach(a =>
                {
                    int output;
                    if (Int32.TryParse(a.Id, out output))
                    {
                       itemIds.Add(output);
                    }
                });

                // parse the artifact ids into a artifactsearchresult to return to the caller
                return await GetArtifactSearchResultsFromItemIds(itemIds, searchEngineParameters.UserId);
            }
            catch (Exception ex)
            {
                if (ex is ElasticsearchConfigurationException)
                {
                    throw;
                }
                throw new ElasticsearchException(I18NHelper.FormatInvariant("Elastic search failed to process search. Exception:{0}", ex));
            }
        }

        private QueryContainerDescriptor<SemanticSearchItem> GetMoreLikeThisQuery(SemanticSearchText searchText)
        {
            var container = new QueryContainerDescriptor<SemanticSearchItem>();
            container.MoreLikeThis(fs => fs
                .Fields(
                    f => f.Field(a => a.SearchText).Field(b => b.Name))
                .Like(
                    l => l.Document(
                        d => d.Document(new SemanticSearchItem() { Name = searchText.Name, SearchText = searchText.SearchText })))
                .MinDocumentFrequency(1)
                .MinTermFrequency(1));

            // Only retrieve live items at time of migration
            container.Terms(t => t.Field(f => f.EndRevision).Terms(ServiceConstants.VersionHead));
            return container;
        }

        private QueryContainerDescriptor<SemanticSearchItem> GetArtifactIdMatchQuery(int artifactId)
        {
            var container = new QueryContainerDescriptor<SemanticSearchItem>();
            container.Terms(t => t.Field(IdFieldKey).Terms(artifactId));
            return container;
        }

        private QueryContainerDescriptor<SemanticSearchItem> GetContainsProjectIdsQuery(IEnumerable<int> projectIds)
        {
            var container = new QueryContainerDescriptor<SemanticSearchItem>();
            container.Terms(
                t => t.Field(
                    p => p.ProjectId).Terms(projectIds));

            return container;
        }
    }
}