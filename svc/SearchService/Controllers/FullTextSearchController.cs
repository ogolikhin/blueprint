using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using SearchService.Models;
using SearchService.Repositories;
using ServiceLibrary.Attributes;
using SearchService.Helpers;
using ServiceLibrary.Helpers;

namespace SearchService.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    [RoutePrefix("FullTextSearch")]
    public class FullTextSearchController : BaseSearchController
    {
        public override string LogSource => "SearchService.FullTextSearch";

        private readonly ISearchConfigurationProvider _searchConfigurationProvider;

        public FullTextSearchController() : this(new SqlFullTextSearchRepository(), new SearchConfiguration())
        {
        }

        private readonly IFullTextSearchRepository _fullTextSearchRepository;
        internal FullTextSearchController(IFullTextSearchRepository fullTextSearchRepository, ISearchConfiguration configuration)
        {
            _fullTextSearchRepository = fullTextSearchRepository;

            _searchConfigurationProvider = new SearchConfigurationProvider(configuration);
        }

        #region Search

        /// <summary>
        /// Perform a Full Text Search
        /// </summary>
        /// <param name="searchCriteria">SearchCriteria object</param>
        /// <param name="page">Page Number</param>
        /// <param name="pageSize">Page Size</param>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request.</response>
        /// <response code="404">Not Found.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        /// /// <response code="500">Service Not Available.</response>
        [HttpPost, NoCache, SessionRequired]
        [Route("")]
        [ResponseType(typeof(FullTextSearchResult))]
        public async Task<IHttpActionResult> Post([FromBody] SearchCriteria searchCriteria, int? page = null, int? pageSize = null)
        {
            // get the UserId from the session
            var userId = ValidateAndExtractUserId();

            ValidateCriteria(searchCriteria, ServiceConstants.MinSearchQueryCharLimit);

            int searchPageSize = GetPageSize(_searchConfigurationProvider, pageSize);
            
            int searchPage = GetStartCounter(page, 1, 1);
            
            var results = await _fullTextSearchRepository.Search(userId, searchCriteria, searchPage, searchPageSize);

            results.Page = searchPage;
            results.PageSize = searchPageSize;
            results.PageItemCount = results.FullTextSearchItems.Count();

            return Ok(results);
        }

        

        #endregion

        #region Metadata

        /// <summary>
        /// Return metadata for a Full Text Search
        /// </summary>
        /// <param name="searchCriteria">SearchCriteria object</param>
        /// <param name="pageSize">Page Size</param>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request.</response>
        /// <response code="404">Not Found.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpPost, NoCache, SessionRequired]
        [Route("metadata")]
        [ResponseType(typeof(FullTextSearchMetaDataResult))]
        public async Task<IHttpActionResult> MetaData([FromBody] SearchCriteria searchCriteria, int? pageSize = null)
        {
            // get the UserId from the session
            int userId = ValidateAndExtractUserId();

            ValidateCriteria(searchCriteria, ServiceConstants.MinSearchQueryCharLimit);

            int searchPageSize = GetPageSize(_searchConfigurationProvider, pageSize);

            var results = await _fullTextSearchRepository.SearchMetaData(userId, searchCriteria);

            results.PageSize = searchPageSize;
            results.TotalPages = results.TotalCount >= 0 ? (int)Math.Ceiling((double)results.TotalCount / searchPageSize) : -1;

            return Ok(results);
        }



        #endregion

        

    }
}
