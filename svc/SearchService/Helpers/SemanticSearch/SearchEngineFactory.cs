using System.Threading.Tasks;
using SearchService.Models;
using SearchService.Repositories;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

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
                    return new ElasticSearchEngine(settings.ConnectionString, semanticSearchRepository);
                }
                case SemanticSearchEngine.Sql:
                {
                    return new SqlSearchEngine(semanticSearchRepository);
                }
                default:
                {
                    throw new SearchEngineNotFoundException(
                        I18NHelper.FormatInvariant("Search enging type {0} is unrecognized", settings?.SemanticSearchEngineType));
                }
            }
        }
    }
}