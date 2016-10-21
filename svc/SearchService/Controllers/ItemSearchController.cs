using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using SearchService.Helpers;
using SearchService.Models;
using SearchService.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace SearchService.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    [RoutePrefix("itemsearch")]
    public class ItemSearchController : LoggableApiController
    {
        public override string LogSource => "SearchService.ItemSearch";
        internal const int MaxResultCount = 100;
        private const string ArtifactPathStub = "Selected Project > Selected Folder > Selected Artifact";

        private readonly ISearchConfigurationProvider _searchConfigurationProvider;

        public ItemSearchController() : this(new SqlItemSearchRepository(), new SearchConfiguration())
        {
        }

        internal readonly IItemSearchRepository _itemSearchRepository;
        internal ItemSearchController(IItemSearchRepository itemSearchRepository, ISearchConfiguration configuration)
        {
            _itemSearchRepository = itemSearchRepository;

            _searchConfigurationProvider = new SearchConfigurationProvider(configuration);
        }

        #region SearchFullText

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
        [Route("fulltext")]
        [ResponseType(typeof(FullTextSearchResult))]
        public async Task<IHttpActionResult> SearchFullText([FromBody] SearchCriteria searchCriteria, int? page = null, int? pageSize = null)
        {
            // get the UserId from the session
            var userId = ValidateAndExtractUserId();

            ValidateCriteria(searchCriteria, ServiceConstants.MinSearchQueryCharLimit);

            int searchPageSize = GetPageSize(_searchConfigurationProvider, pageSize);

            int searchPage = GetStartCounter(page, 1, 1);

            var results = await _itemSearchRepository.Search(userId, searchCriteria, searchPage, searchPageSize);

            results.Page = searchPage;
            results.PageSize = searchPageSize;
            results.PageItemCount = results.FullTextSearchItems.Count();

            return Ok(results);
        }

        #endregion SearchFullText

        #region FullTextMetaData

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
        [Route("fulltextmetadata")]
        [ResponseType(typeof(FullTextSearchMetaDataResult))]
        public async Task<IHttpActionResult> FullTextMetaData([FromBody] SearchCriteria searchCriteria, int? pageSize = null)
        {
            // get the UserId from the session
            int userId = ValidateAndExtractUserId();

            ValidateCriteria(searchCriteria, ServiceConstants.MinSearchQueryCharLimit);

            int searchPageSize = GetPageSize(_searchConfigurationProvider, pageSize);

            var results = await _itemSearchRepository.SearchMetaData(userId, searchCriteria);

            results.PageSize = searchPageSize;
            results.TotalPages = results.TotalCount >= 0 ? (int)Math.Ceiling((double)results.TotalCount / searchPageSize) : -1;

            return Ok(results);
        }

        #endregion FullTextMetaData

        #region SearchName

        /// <summary>
        /// Perform an Item search by Name
        /// </summary>
        /// <param name="searchCriteria">SearchCriteria object</param>
        /// <param name="startOffset">Search start offset</param>
        /// <param name="pageSize">Page Size</param>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request.</response>
        /// <response code="404">Not Found.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpPost, NoCache, SessionRequired]
        [Route("name")]
        [ResponseType(typeof(ItemSearchResult))]
        public async Task<IHttpActionResult> SearchName([FromBody] ItemSearchCriteria searchCriteria, int? startOffset = null, int? pageSize = null)
        {
            // get the UserId from the session
            var userId = ValidateAndExtractUserId();

            ValidateCriteria(searchCriteria);

            int searchPageSize = GetPageSize(_searchConfigurationProvider, pageSize, MaxResultCount);

            int searchStartOffset = GetStartCounter(startOffset, 0, 0);

            var results = await _itemSearchRepository.SearchName(userId, searchCriteria, searchStartOffset, searchPageSize);

            results.PageItemCount = results.SearchItems.Count();

            if (searchCriteria.IncludeArtifactPath)
            {
                // TODO Get Search Artifact Path
                foreach (var searchItem in results.SearchItems)
                {
                    searchItem.ArtifactPath = ArtifactPathStub;
                }
            }

            return Ok(results);
        }

        #endregion SearchName

        private int ValidateAndExtractUserId()
        {
            // get the UserId from the session
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                throw new AuthenticationException("Authorization is required", ErrorCodes.UnauthorizedAccess);
            }
            return userId.Value;
        }

        private void ValidateCriteria(ISearchCriteria searchCriteria, int minSearchQueryLimit = 1)
        {
            if (!ModelState.IsValid || !ValidateSearchCriteria(searchCriteria, minSearchQueryLimit))
            {
                throw new BadRequestException("Please provide correct search criteria", ErrorCodes.IncorrectSearchCriteria);
            }
        }

        private int GetPageSize(ISearchConfigurationProvider searchConfigurationProvider, int? requestedPageSize, int maxPageSize = Int32.MaxValue)
        {
            int searchPageSize = requestedPageSize.GetValueOrDefault(searchConfigurationProvider.PageSize);
            if (searchPageSize <= 0)
            {
                searchPageSize = searchConfigurationProvider.PageSize;
            }

            if (searchPageSize > maxPageSize)
            {
                searchPageSize = maxPageSize;
            }
            return searchPageSize;
        }

        private int GetStartCounter(int? requestedStartCounter, int minStartCounter, int defaultCounterValue)
        {

            int startCounter = requestedStartCounter.GetValueOrDefault(defaultCounterValue);
            if (startCounter < minStartCounter)
            {
                startCounter = defaultCounterValue;
            }
            return startCounter;
        }

        private int? GetUserId()
        {
            object sessionValue;
            if (!Request.Properties.TryGetValue(ServiceConstants.SessionProperty, out sessionValue))
            {
                return null;
            }
            var session = sessionValue as Session;
            return session?.UserId;
        }

        private bool ValidateSearchCriteria(ISearchCriteria searchCriteria, int minSearchQueryLimit = 1)
        {
            if (string.IsNullOrWhiteSpace(searchCriteria?.Query) ||
                searchCriteria.Query.Trim().Length < minSearchQueryLimit ||
                !searchCriteria.ProjectIds.Any())
            {
                return false;
            }
            return true;
        }
    }
}
