using System.Threading.Tasks;
using SearchService.Models;
using SearchService.Repositories;
using ServiceLibrary.Exceptions;

namespace SearchService.Helpers.SemanticSearch
{
    public class SearchEngineFactory
    {
        public static ISearchEngine CreateSearchEngine(ISemanticSearchRepository semanticSearchRepository)
        {
            var settings = Task.Factory.StartNew(async () => await semanticSearchRepository.GetSemanticSearchSetting()).Unwrap().Result;

            switch (settings?.SemanticSearchEngineType)
            {
                case SemanticSearchEngine.ElasticSearch:
                {
                    return new ElasticSearchEngine(settings.ElasticsearchConnectionString, settings.TenantId, semanticSearchRepository);
                }
                case SemanticSearchEngine.Sql:
                {
                    return new SqlSearchEngine(semanticSearchRepository);
                }
                default:
                {
                    throw new SearchEngineNotFoundException(
                        $"Search enging type {settings?.SemanticSearchEngineType} is unrecognized");
                }
            }
        }
    }
}