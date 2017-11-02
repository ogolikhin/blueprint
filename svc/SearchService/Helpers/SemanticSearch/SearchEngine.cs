using System.Collections.Generic;
using System.Threading.Tasks;
using SearchService.Models;
using SearchService.Repositories;

namespace SearchService.Helpers.SemanticSearch
{
    public interface ISearchEngine
    {
        void PerformHealthCheck();
        Task<IEnumerable<ArtifactSearchResult>> GetSemanticSearchSuggestions(SearchEngineParameters searchEngineParameters);
    }
    public abstract class SearchEngine : ISearchEngine
    {
        protected ISemanticSearchRepository SemanticSearchRepository { get; }

        protected SearchEngine(ISemanticSearchRepository semanticSearchRepository)
        {
            SemanticSearchRepository = semanticSearchRepository;
        }

        public abstract void PerformHealthCheck();

        public abstract Task<IEnumerable<ArtifactSearchResult>> GetSemanticSearchSuggestions(SearchEngineParameters searchEngineParameters);

        protected async Task<IEnumerable<ArtifactSearchResult>> GetArtifactSearchResultsFromItemIds(HashSet<int> itemIds, int userId)
        {
            return await SemanticSearchRepository.GetSuggestedArtifactDetails(itemIds, userId);
        }
    }
}