using System;
using System.Threading.Tasks;
using System.Web.Http;
using SearchService.Models;
using SearchService.Services;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;

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
            new SemanticSearchService())
        {
        }

        internal SemanticSearchController(ISemanticSearchService semanticSearchService)
        {
            _semanticSearchService = semanticSearchService;
        }

        [HttpGet, NoCache, SessionRequired]
        [Route("{artifactId}")]
        public async Task<SuggestionsSearchResult> GetSuggestions([FromUri] SuggestionsSearchCriteria searchCriteria)
        {
            var suggestionParameters = new SemanticSearchSuggestionParameters(searchCriteria.ArtifactId, Session.UserId);
            return await _semanticSearchService.GetSemanticSearchSuggestions(suggestionParameters);
        }
    }
}