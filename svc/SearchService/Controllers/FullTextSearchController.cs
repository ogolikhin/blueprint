using System;
using System.Threading.Tasks;
using System.Web.Http;
using SearchService.Models;
using SearchService.Repositories;
using ServiceLibrary.Helpers;
using ServiceLibrary.Attributes;

namespace SearchService.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    [RoutePrefix("FullTextSearch")]
    public class FullTextSearchController : LoggableApiController
    {
        public override string LogSource => "SearchService.FullTextSearch";

        public FullTextSearchController() : this(new SqlFullTextSearchRepository())
        {
        }

        private readonly IFullTextSearchRepository _fullTextSearchRepository;
        public FullTextSearchController(IFullTextSearchRepository fullTextSearchRepository)
        {
            _fullTextSearchRepository = fullTextSearchRepository;
        }

        [HttpPost, NoCache, NoSessionRequired]
        [Route("")]
        public async Task<IHttpActionResult> Post([FromBody] SearchCriteria searchCriteria, int page = 1, int pageSize = -1)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (pageSize == -1) pageSize = WebApiConfig.PageSize;

            var results = await _fullTextSearchRepository.Search(searchCriteria, page, pageSize);

            var totalPages = Math.Ceiling((double)results.TotalCount / pageSize);

            return Ok(new
            {
                Page = page,
                TotalCount = results.TotalCount,
                TotalPages = totalPages,
                PageSize = pageSize,
                SearchResults = results.FullTextSearchItems,
                TypeResults = results.FullTextSearchTypeItems
            });
        }

        [HttpGet, NoCache, NoSessionRequired]
        [Route("UpCheck")]
        public IHttpActionResult GetStatusUpCheck()
        {
            return Ok();
        }
    }
}
