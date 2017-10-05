using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308: Normalize strings to uppercase", Justification = "Index name for elastic search must be lower cased")]
        public ElasticSearchEngine(string connectionString, string tenantId, ISemanticSearchRepository semanticSearchRepository) 
            : base(semanticSearchRepository)
        {
            IndexName = (IndexPrefix + tenantId).ToLowerInvariant();

            // create ElasticClient using conneciton string
            var connectionSettings = new ConnectionSettings(new Uri(connectionString)).DefaultIndex(IndexName);
            _elasticClient = new ElasticClient(connectionSettings);
            
        }

        public override async Task<IEnumerable<ArtifactSearchResult>> GetSemanticSearchSuggestions(SearchEngineParameters searchEngineParameters)
        {
            var searchText = await SemanticSearchRepository.GetSemanticSearchText(searchEngineParameters.ArtifactId,
                searchEngineParameters.UserId);
            
            // Create the bool query descripter that just searchs for the searchText we constructed
            var boolQueryDescriptor = new BoolQueryDescriptor<SemanticSearchItems>();
            boolQueryDescriptor.Must(m => m.MoreLikeThis(
                fs => fs.Fields(
                    f => f.Field(a => a.SearchText)).Like(
                        l => l.Text(searchText))
                        .MinDocumentFrequency(1)
                        .MinTermFrequency(1)));
            
            // Dont return back result for the current artifact id we're searching against
            boolQueryDescriptor.MustNot(
                mn=> mn.Terms(t => t.Field(p => p.ItemId).Terms(searchEngineParameters.ArtifactId)));

            // If not instance admin, use the list of accessible project ids to filter out, otherwise no need to filter
            if (!searchEngineParameters.IsInstanceAdmin)
            {
                boolQueryDescriptor.Filter(
                    f => f.Terms(
                        t => t.Field(
                            p => p.ProjectId).Terms(
                                searchEngineParameters.AccessibleProjectIds)));
            }

            // Creates the search descriptor 
            var searchDescriptor = new SearchDescriptor<SemanticSearchItems>();
            searchDescriptor.Size(searchEngineParameters.PageSize).Query(q => q.Bool(b => boolQueryDescriptor));

            var results = await _elasticClient.SearchAsync<SemanticSearchItems>(searchDescriptor);

            var items = results.Documents;
            var itemIds = items.Select(i => i.ItemId);

            // parse the artifact ids into a artifactsearchresult to return to the caller
            return await GetArtifactSearchResultsFromItemIds(itemIds.ToList(), searchEngineParameters.UserId);
        }
    }

    public class SemanticSearchItems
    {
        public int ItemId;

        public int ProjectId;

        public int EndRevision;

        public int LatestChangingRevision;

        public string SearchText;
    }
}