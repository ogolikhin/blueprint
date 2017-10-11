using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nest;
using SearchService.Models;
using SearchService.Repositories;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace SearchService.Helpers.SemanticSearch
{
    public class SemanticSearchItem
    {
        public int ItemId;

        public int ProjectId;

        public int EndRevision;

        public int LatestChangingRevision;

        public string Name;

        public string SearchText;
    }
    public sealed class ElasticSearchEngine: SearchEngine
    {
        private const string IndexPrefix = "semanticsdb_";
        private string IndexName { get; }
        private IElasticClient _elasticClient;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308: Normalize strings to uppercase", Justification = "Index name for elastic search must be lower cased")]
        public ElasticSearchEngine(string connectionString, string tenantId, ISemanticSearchRepository semanticSearchRepository) 
            : base(semanticSearchRepository)
        {
            IndexName = (IndexPrefix + tenantId).ToLowerInvariant();

            // create ElasticClient using conneciton string
            var connectionSettings = new ConnectionSettings(new Uri(connectionString)).DefaultIndex(IndexName);
            connectionSettings.MapDefaultTypeNames(d => d.Add(typeof (SemanticSearchItem), "semanticsearchitems"));

            _elasticClient = new ElasticClient(connectionSettings);

            PerformElasticEngineHealthCheck();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308: Normalize strings to uppercase", Justification = "Index name for elastic search must be lower cased")]
        internal ElasticSearchEngine(string tenantId, IElasticClient elasticClient, ISemanticSearchRepository semanticSearchRepository)
            : base (semanticSearchRepository)
        {
            IndexName = (IndexPrefix + tenantId).ToLowerInvariant();

            _elasticClient = elasticClient;

            PerformElasticEngineHealthCheck();
        }

        private void PerformElasticEngineHealthCheck()
        {
            var ping = _elasticClient.Ping();
            if (!ping.IsValid)
            {
                throw new ElasticsearchConfigurationException("Could not connect to elasticsearch connection string. Please check your elasticsearch connection.");
            }

            var doesIndexExists = _elasticClient.IndexExists(IndexName);
            if (!doesIndexExists.Exists)
            {
                throw new ElasticsearchConfigurationException("Elasticsearch Index does not exist");
            }

            var doesTypeExists = _elasticClient.TypeExists(IndexName, typeof(SemanticSearchItem));
            if (!doesTypeExists.Exists)
            {
                throw new ElasticsearchConfigurationException("Elasticsearch Type does not exist");
            }
        }
        public override async Task<IEnumerable<ArtifactSearchResult>> GetSemanticSearchSuggestions(SearchEngineParameters searchEngineParameters)
        {
            try
            {
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
                searchDescriptor.Size(searchEngineParameters.PageSize).Query(q => q.Bool(b => boolQueryDescriptor));

                var results = await _elasticClient.SearchAsync<SemanticSearchItem>(searchDescriptor);

                var items = results.Documents;
                var itemIds = items.Select(i => i.ItemId);

                // parse the artifact ids into a artifactsearchresult to return to the caller
                return await GetArtifactSearchResultsFromItemIds(itemIds.ToList(), searchEngineParameters.UserId);
            }
            catch (Exception ex)
            {
                throw new ElasticsearchException(I18NHelper.FormatInvariant("Elastic search failed to process search. Exception:{0}", ex));
            }
        }

        private QueryContainerDescriptor<SemanticSearchItem> GetMoreLikeThisQuery(SemanticSearchText searchText)
        {
            var container = new QueryContainerDescriptor<SemanticSearchItem>();
            container.MoreLikeThis(fs => fs
                .Fields(
                    f => f.Field(a => a.SearchText).Field(b=>b.Name)
                )
                .Like(
                    l => l.Document(
                        d =>d.Document(new SemanticSearchItem() {Name = searchText.Name, SearchText = searchText.SearchText}))
                )
                .MinDocumentFrequency(1)
                .MinTermFrequency(1));

            // Only retrieve live items at time of migration
            container.Terms(t => t.Field(f => f.EndRevision).Terms(ServiceConstants.VersionHead));
            return container;
        }

        private QueryContainerDescriptor<SemanticSearchItem> GetArtifactIdMatchQuery(int artifactId)
        {
            var container = new QueryContainerDescriptor<SemanticSearchItem>();
            container.Terms(t => t.Field(p => p.ItemId).Terms(artifactId));
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