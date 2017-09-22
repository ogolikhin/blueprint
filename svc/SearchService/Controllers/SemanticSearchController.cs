using System;
using System.Threading.Tasks;
using System.Web.Http;
using SearchService.Models;
using SearchService.Repositories;
using SearchService.Services;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
using ServiceLibrary.Repositories;

namespace SearchService.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    [RoutePrefix("semanticsearch")]
    public class SemanticSearchController : LoggableApiController
    {
        private readonly ISemanticSearchService _semanticSearchService;
        public override string LogSource => "SearchService.SemanticSearch";

        public SemanticSearchController() : this(
            new SemanticSearchService(
                new SemanticSearchRepository(),
                new SqlArtifactPermissionsRepository(new SqlConnectionWrapper(WebApiConfig.BlueprintConnectionString)),
                new SqlUsersRepository(new SqlConnectionWrapper(WebApiConfig.BlueprintConnectionString)), 
                new SqlArtifactRepository(new SqlConnectionWrapper(WebApiConfig.BlueprintConnectionString))))
        {
            
        }
        internal SemanticSearchController(ISemanticSearchService semanticSearchService)
        {
            _semanticSearchService = semanticSearchService;
        }
        
        [HttpGet, NoCache, SessionRequired]
        [Route("{id}")]
        public async Task<SuggestionsSearchResult> GetSuggestions(int id)
        {
            try
            {
                return await _semanticSearchService.Suggests(id, Session.UserId);
            }
            catch (Exception ex)
            {
                await Log.LogError(LogSource, ex);
                throw;
            }
        }
    }
}