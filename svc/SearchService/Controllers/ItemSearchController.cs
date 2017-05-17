using System;
using System.Threading.Tasks;
using System.Web.Http;
using SearchService.Helpers;
using SearchService.Models;
using SearchService.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;

namespace SearchService.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    [RoutePrefix("itemsearch")]
    public class ItemSearchController : LoggableApiController
    {
        public override string LogSource => "SearchService.ItemSearch";
        internal const int MaxResultCount = 101;
        private const string DefaultSeparator = "/";

        internal readonly IItemSearchRepository ItemSearchRepo;
        private readonly ISearchConfigurationProvider _searchConfigurationProvider;
        private readonly CriteriaValidator _criteriaValidator;

        public ItemSearchController() : this(new SqlItemSearchRepository(), new SearchConfiguration(), new ServiceLogRepository())
        {
        }

        internal ItemSearchController(IItemSearchRepository itemSearchRepo, ISearchConfiguration configuration, IServiceLogRepository serviceLogRepository) : base(serviceLogRepository)
        {
            ItemSearchRepo = itemSearchRepo;
            _searchConfigurationProvider = new SearchConfigurationProvider(configuration);
            _criteriaValidator = new CriteriaValidator();
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
        public async Task<FullTextSearchResultSet> SearchFullText([FromBody] FullTextSearchCriteria searchCriteria, int? page = null, int? pageSize = null)
        {
            
                // get the UserId from the session
            var userId = ValidateAndExtractUserId();

            _criteriaValidator.Validate(SearchOption.FullTextSearch, ModelState.IsValid, searchCriteria,
                ServiceConstants.MinSearchQueryCharLimit);

            int searchPageSize = GetPageSize(_searchConfigurationProvider, pageSize);

            int searchPage = GetStartCounter(page, 1, 1);

            try
            {
                return await ItemSearchRepo.SearchFullText(userId, searchCriteria, searchPage, searchPageSize);
            }
            catch (Exception ex)
            {
                await Log.LogError(LogSource, ex);
                throw;
            }
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
        public async Task<MetaDataSearchResultSet> FullTextMetaData([FromBody] FullTextSearchCriteria searchCriteria, int? pageSize = null)
        {
            // get the UserId from the session
            int userId = ValidateAndExtractUserId();

            _criteriaValidator.Validate(SearchOption.FullTextSearch, ModelState.IsValid, searchCriteria, ServiceConstants.MinSearchQueryCharLimit); 

            int searchPageSize = GetPageSize(_searchConfigurationProvider, pageSize);

            try
            {
                var results = await ItemSearchRepo.FullTextMetaData(userId, searchCriteria);

                results.PageSize = searchPageSize;
                results.TotalPages = results.TotalCount >= 0
                    ? (int) Math.Ceiling((double) results.TotalCount/searchPageSize)
                    : -1;

                return results;
            }
            catch (Exception ex)
            {
                await Log.LogError(LogSource, ex);
                throw;
            }
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
        public async Task<ItemNameSearchResultSet> SearchName(
            [FromBody] ItemNameSearchCriteria searchCriteria, 
            int? startOffset = null, 
            int? pageSize = null)
        {
            // get the UserId from the session
            var userId = ValidateAndExtractUserId();

            _criteriaValidator.Validate(SearchOption.ItemName, ModelState.IsValid, searchCriteria, 1);

            int searchPageSize = GetPageSize(_searchConfigurationProvider, pageSize, MaxResultCount);

            int searchStartOffset = GetStartCounter(startOffset, 0, 0);


            try
            {
                var results = await ItemSearchRepo.SearchName(userId, searchCriteria, searchStartOffset, searchPageSize);

                foreach (var searchItem in results.Items)
                {
                    searchItem.LockedByUser = searchItem.LockedByUserId.HasValue
                        ? new UserGroup {Id = searchItem.LockedByUserId}
                        : null;
                }

                return results;
            }
            catch (Exception ex)
            {
                await Log.LogError(LogSource, ex);
                throw;
            }
        }

        #endregion SearchName

        private int ValidateAndExtractUserId()
        {
            // get the UserId from the session
            object sessionValue;
            if (!Request.Properties.TryGetValue(ServiceConstants.SessionProperty, out sessionValue))
            {
                throw new AuthenticationException("Authorization is required", ErrorCodes.UnauthorizedAccess);
            }
            return ((Session)sessionValue).UserId;
        }

        

        private int GetPageSize(ISearchConfigurationProvider searchConfigurationProvider, int? requestedPageSize, int maxPageSize = Int32.MaxValue)
        {
            int searchPageSize = requestedPageSize.GetValueOrDefault(searchConfigurationProvider.PageSize);
            if (searchPageSize <= 0)
            {
                searchPageSize = searchConfigurationProvider.PageSize;
            }

            return searchPageSize > maxPageSize ? maxPageSize : searchPageSize;
        }

        private int GetStartCounter(int? requestedStartCounter, int minStartCounter, int defaultCounterValue)
        {
            int startCounter = requestedStartCounter.GetValueOrDefault(defaultCounterValue);
            return startCounter < minStartCounter ? defaultCounterValue : startCounter;
        }
    }
}
