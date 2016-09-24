using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.Factories;
using Model.FullTextSearchModel.Impl;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TestCommon;
using Utilities;

namespace SearchServiceTests
{
    [TestFixture]
    [Category(Categories.SearchService)]
    public class FullTextSearchTests : TestBase
    {
        const string FULLTEXTSEARCH_PATH = RestPaths.Svc.SearchService.FULLTEXTSEARCH;
        const int DEFAULT_PAGE_VALUE = 1;
        const int DEFAULT_PAGESIZE_VALUE = 10;

        private IUser _user = null;
        private List<IProject> _projects = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _projects = ProjectFactory.GetAllProjects(_user);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #region 200 OK Tests

        [TestCase(2)]
        [TestRail(181022)]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [Description("Searching with optional parameter, page. Execute Search - Must return SearchResult that uses the page value.")]
        public void FullTextSearch_SearchWithPageOnly_VerifySearchResult(int page)
        {
            // Setup: Create searchable artifact(s) with unique search terms
            FullTextSearchResult fullTextSearchResult = null;
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria("NonExistingSearchTerm", selectedProjectIds);

            // Execute: Execute FullTextSearch with search terms with page parameter value
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.FullTextSearch.Search(_user, searchCriteria, page: page), "Nova FullTextSearch call failed when using following search term: {0} with page={1}!", searchCriteria.Query, page);

            // Validation: Verify that searchResult uses poptional page value and and DefaultPageSize
            FullTextSearchResultValidation(fullTextSearchResult, page: page);
        }

        [TestCase(3)]
        [TestRail(181023)]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [Description("Searching with optional parameter, pageSize. Execute Search - Must return SearchResult that uses pageSize value.")]
        public void FullTextSearch_SearchWithPageSizeOnly_VerifySearchResult(int pageSize)
        {
            // Setup: Create searchable artifact(s) with unique search terms
            FullTextSearchResult fullTextSearchResult = null;
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria("NonExistingSearchTerm", selectedProjectIds);

            // Execute: Execute FullTextSearch with search terms with pageSize parameter value
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.FullTextSearch.Search(_user, searchCriteria, pageSize: pageSize), "Nova FullTextSearch call failed when using following search term: {0} with pageSize={1}!", searchCriteria.Query, pageSize);

            // Validation: Verify that searchResult uses FirstPage and optional pageSize value
            FullTextSearchResultValidation(fullTextSearchResult, pageSize: pageSize);
        }

        [TestCase(4,5)]
        [TestRail(166156)]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [Description("Searching with both optional parameters, page and pageSize. Execute Search - Must return SearchResult that uses page and pageSize values.")]
        public void FullTextSearch_SearchWithBothPageAndPageSize_VerifySearchResult(int page, int pageSize)
        {
            // Setup: Create searchable artifact(s) with unique search terms
            FullTextSearchResult fullTextSearchResult = null;
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria("NonExistingSearchTerm", selectedProjectIds);

            // Execute: Execute FullTextSearch with search terms with both page and pageSize parameter values
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.FullTextSearch.Search(_user, searchCriteria, page: page, pageSize: pageSize), "Nova FullTextSearch call failed when using following search term: {0} with page={1} and pageSize={2}!", searchCriteria.Query, page, pageSize);

            // Validation: Verify that searchResult uses FirstPage and optional page and pageSize values
            FullTextSearchResultValidation(fullTextSearchResult, page: page, pageSize: pageSize);
        }

        [TestCase]
        [TestRail(166155)]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [Description("Searching without both optional parameters, page and pagesize. Execute Search - Must return SearchResult that uses FirstPage and DefaultPageSize")]
        public void FullTextSearch_SearchWithoutBothPageAndPageSize_VerifySearchResultUsesFirstPageAndDefaultPageSize()
        {
            // Setup: Create searchable artifact(s) with unique search terms
            FullTextSearchResult fullTextSearchResult = null;
            var projectIds = new List<int>() { 2, 4 };
            var searchCriteria = new FullTextSearchCriteria("NonExistingSearchTerm", projectIds);

            // Execute: Execute FullTextSearch with search terms
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.FullTextSearch.Search(_user, searchCriteria), "Nova FullTextSearch call failed when using following search term: {0}!", searchCriteria.Query);

            // Validation: Verify that searchResult uses FirstPage and DefaultPageSize
            FullTextSearchResultValidation(fullTextSearchResult, page: DEFAULT_PAGE_VALUE);
        }

        [TestCase(-3)]
        [TestRail(181024)]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [Description("Searching with invalid page value. Execute Search - Must return SearchResult that uses First page.")]
        public void FullTextSearch_SearchWithInvalidPage_VerifySearchResultUsesFirstPage(int page)
        {
            // Setup: Create searchable artifact(s) with unique search terms
            FullTextSearchResult fullTextSearchResult = null;
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria("NonExistingSearchTerm", selectedProjectIds);

            // Execute: Execute FullTextSearch with search terms with invalid page parameter value
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.FullTextSearch.Search(_user, searchCriteria, page: page), "Nova FullTextSearch call failed when using following search term: {0} with invalid page={1}!", searchCriteria.Query, page);

            // Validation: Verify that searchResult uses FirstPage
            FullTextSearchResultValidation(fullTextSearchResult, page: DEFAULT_PAGE_VALUE);
        }

        [TestCase(-10)]
        [TestRail(181025)]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [Description("Searching with invalid pageSize value. Execute Search - Must return SearchResult that uses Default pageSize.")]
        public void FullTextSearch_SearchWithInvalidPageSize_VerifySearchResultUsesDefaultPageSize(int pageSize)
        {
            // Setup: Create searchable artifact(s) with unique search terms
            FullTextSearchResult fullTextSearchResult = null;
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria("NonExistingSearchTerm", selectedProjectIds);

            // Execute: Execute FullTextSearch with search terms with invalid pageSize parameter value
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.FullTextSearch.Search(_user, searchCriteria, pageSize: pageSize), "Nova FullTextSearch call failed when using following search term: {0} with invalid pageSize={1}!", searchCriteria.Query, pageSize);

            // Validation: Verify that searchResult uses Default PageSize
            FullTextSearchResultValidation(fullTextSearchResult, pageSize: DEFAULT_PAGESIZE_VALUE);
        }

        [TestCase(-12, -100)]
        [TestRail(181021)]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [Description("Searching with both invalid page and pageSize values. Execute Search - Must return SearchResult that uses First page and Default pageSize.")]
        public void FullTextSearch_SearchWithInvalidPageAndPageSize_VerifySearchResultUsesFirstPageAndDefaultPageSize(int page, int pageSize)
        {
            // Setup: Create searchable artifact(s) with unique search terms
            FullTextSearchResult fullTextSearchResult = null;
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria("NonExistingSearchTerm", selectedProjectIds);

            // Execute: Execute FullTextSearch with search terms with invalid page and invalid pageSize parameter values
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.FullTextSearch.Search(_user, searchCriteria, page: page, pageSize: pageSize), "Nova FullTextSearch call failed when using following search term: {0} with invalid page={1} and invalid pageSize={2}!", searchCriteria.Query, page, pageSize);

            // Validation: Verify that searchResult uses FirstPage and Default PageSize
            FullTextSearchResultValidation(fullTextSearchResult, page: DEFAULT_PAGE_VALUE, pageSize: DEFAULT_PAGESIZE_VALUE);
        }

        #endregion 200 OK Tests

        #region 400 Bad Request Tests

        [TestCase]
        [TestRail(166162)]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [Description("Searching with invalid search criteria. Execute Search - Must return 400 bad request")]
        public void FullTextSearch_SearchWithInvalidSearchCriteria_400BadRequest()
        {
            // Setup: Create searchable artifact(s) with unique search terms
            FullTextSearchResult fullTextSearchResult = null;
            var invalidSearchCriteria = new FullTextSearchCriteria();

            // Execute: Execute FullTextSearch with invalid Search criteria
            var ex = Assert.Throws<Http400BadRequestException>(() => fullTextSearchResult = Helper.FullTextSearch.Search(_user, invalidSearchCriteria), "Nova FullTextSearch call exit with 400 BadRequestException failed when using invalid search criteria!");
            
            // Validation: Exception should contain empty response content.
            Assert.That(ex.RestResponse.Content.Length.Equals(0), "FullTextSearch with invalid searchCriteria should return empty content but {0} is returned", ex.RestResponse.Content.ToString());
        }

        #endregion 400 Bad Request Tests

        #region 401 Unauthorized Tests

        [TestCase]
        [TestRail(166163)]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [Description("Searching with invalid sesson. Execute Search - Must return 401 Unautorized")]
        public void FullTextSearch_SearchWithInvalidSession_401Unauthorized()
        {
            // Setup: Create searchable artifact(s) with unique search terms
            FullTextSearchResult fullTextSearchResult = null;
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria("NonExistingSearchTerm", selectedProjectIds);
            IUser userWithBadToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            // Execute: Execute FullTextSearch with invalid session
            var ex = Assert.Throws<Http401UnauthorizedException>(() => fullTextSearchResult = Helper.FullTextSearch.Search(userWithBadToken, searchCriteria), "Nova FullTextSearch call exit with 401 UnauthorizedException failed when using invalid session!");

            // Validation: Exception should contain expected message.
            const string expectedExceptionMessage = "Token is invalid";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage), "{0} was not found in returned message of Nova FullTextSearch which has invalid token.", expectedExceptionMessage);
        }

        #endregion 401 Unauthorized Tests

        #region 404 Not Found Tests
        #endregion 404 Not Found Tests

        #region 409 Conflict Tests
        #endregion 409 Conflict Tests

        #region Private Functions

        /// <summary>
        /// Asserts that returned searchResult from the FullTextSearch call match with artifacts that are being searched.
        /// </summary>
        /// <param name="searchResult">The searchResult from Nova search call.</param>
        /// <param name="artifactsToBeFound">artifacts that are being searched</param>
        /// <param name="page"> (optional) page value that represents displaying page number of the rearch result</param>
        /// <param name="pageSize"> (optional) pageSize value that indicates number of items that get displayed per page</param>
        private static void FullTextSearchResultValidation( FullTextSearchResult searchResult, List<IArtifactBase> artifactsToBeFound = null, int? page = null, int? pageSize = null)
        {
            ThrowIf.ArgumentNull(searchResult, nameof(searchResult));

            //Setup: Set comparison values
            artifactsToBeFound = artifactsToBeFound ?? new List<IArtifactBase>();
            page = page ?? DEFAULT_PAGE_VALUE;
            pageSize = pageSize ?? DEFAULT_PAGESIZE_VALUE;

            List<int> ReturnedFullTextSearchItemArtifactIds = new List<int>();

            if (artifactsToBeFound.Any())
            {
                searchResult.FullTextSearchItems.Cast<FullTextSearchItem>().ToList().ForEach(a => ReturnedFullTextSearchItemArtifactIds.Add(a.ArtifactId));

                for (int i = 0; i < artifactsToBeFound.Count; i++)
                {
                    Assert.That(ReturnedFullTextSearchItemArtifactIds.Contains(artifactsToBeFound[i].Id), "The expected artifact whose Id is {0} does not exist on the response from the Nova FullTextSearch call.", artifactsToBeFound[i].Id);
                }
            }

            // Validation: Verify that searchResult uses page value passed as optional parameter and DefaultPageSize
            Assert.That(searchResult.Page.Equals(page), "The expected default page value is {0} but {1} was found from the returned searchResult.", page, searchResult.Page);
            Assert.That(searchResult.PageSize.Equals(pageSize), "The expected default pagesize value is {0} but {1} was found from the returned searchResult.", pageSize, searchResult.PageSize);
        }
        
        #endregion Private Functions

    }
}
