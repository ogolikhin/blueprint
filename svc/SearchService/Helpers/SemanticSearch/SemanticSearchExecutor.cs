using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SearchService.Models;
using SearchService.Repositories;
using ServiceLibrary.Repositories;

namespace SearchService.Helpers.SemanticSearch
{
    public interface ISemanticSearchExecutor
    {
        Task<IEnumerable<ArtifactSearchResult>> GetSemanticSearchSuggestions(SearchEngineParameters searchEngingParameters);
    }

    public class SemanticSearchExecutor : ISemanticSearchExecutor
    {
        private static readonly Lazy<SemanticSearchExecutor> _instance = 
            new Lazy<SemanticSearchExecutor>(
                () => new SemanticSearchExecutor(
                    new SemanticSearchRepository(new SqlConnectionWrapper(WebApiConfig.BlueprintConnectionString))), 
                LazyThreadSafetyMode.PublicationOnly);

        public static ISemanticSearchExecutor Instance => _instance.Value;
        protected ISearchEngine _searchEngine;

        internal SemanticSearchExecutor(ISemanticSearchRepository semanticSearchRepository)
        {
            _searchEngine = SearchEngineFactory.CreateSearchEngine(semanticSearchRepository);
            _searchEngine.PerformHealthCheck();
        }

        public async Task<IEnumerable<ArtifactSearchResult>> GetSemanticSearchSuggestions(SearchEngineParameters searchEngineParameters)
        {
            return await _searchEngine.GetSemanticSearchSuggestions(searchEngineParameters);
        }
    }
}