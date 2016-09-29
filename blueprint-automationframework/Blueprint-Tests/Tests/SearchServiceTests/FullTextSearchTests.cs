using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.Factories;
using Model.FullTextSearchModel.Impl;
using NUnit.Framework;
using System;
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
        private IUser _userSecond = null;
        private List<IProject> _projects = null;
        private List<IArtifactBase> _publishedArtifacts;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _userSecond = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _projects = ProjectFactory.GetAllProjects(_user, shouldRetrievePropertyTypes: true);
            _publishedArtifacts = SearchServiceTestHelper.SetupSearchData(_projects, _user, Helper);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #region 200 OK Tests

        [TestCase(2)]
        [TestRail(181022)]
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

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(182340)]
        [Ignore(IgnoreReasons.UnderDevelopment)]
        [Description("Searching with the search criteria that matches with saved artifacts. Execute Search with the same user - Must return SearchResult with empty list of FullTextSearchItems.")]
        public void FullTextSearch_SearchSavedNotPublishedArtifact_VerifyWithSameUserEmptySearchResult(BaseArtifactType baseArtifactType)
        {
            // Setup
            FullTextSearchResult fullTextSearchResult = null;
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);

            // Setup: Create and save single artifact for each project
            List<IArtifactBase> savedOnlyArtifacts = new List<IArtifactBase>();
            _projects.ForEach(project => savedOnlyArtifacts.Add(Helper.CreateAndSaveArtifact(project, _user, baseArtifactType)));

            // Setup: Create search criteria with search term that maches with saved artifact(s).
            // Search with the name of the artifact created on on of available projects
            var searchCriteria = new FullTextSearchCriteria(savedOnlyArtifacts.First().Name, selectedProjectIds);

            // Execute: Execute FullTextSearch with search term using the same user
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.FullTextSearch.Search(_user, searchCriteria), "Nova FullTextSearch call failed when using following search term: {0} which matches with saved only artifacts!", searchCriteria.Query);

            // Validation: Verify that searchResult contains empty list of FullTextSearchItems 
            FullTextSearchResultValidation(fullTextSearchResult);
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(182369)]
        [Ignore(IgnoreReasons.UnderDevelopment)]
        [Description("Searching with the search criteria that matches with saved artifacts. Execute Search with different user - Must return SearchResult with empty list of FullTextSearchItems.")]
        public void FullTextSearch_SearchSavedNotPublishedArtifact_VerifyWithDifferentUserEmptySearchResult(BaseArtifactType baseArtifactType)
        {
            // Setup
            FullTextSearchResult fullTextSearchResult = null;
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);

            // Setup: Create and save single artifact for each project
            List<IArtifactBase> savedOnlyArtifacts = new List<IArtifactBase>();
            _projects.ForEach(project => savedOnlyArtifacts.Add(Helper.CreateAndSaveArtifact(project, _user, baseArtifactType)));

            // Setup: Create search criteria with search term that maches with saved artifact(s).
            // Search with the name of the artifact created on on of available projects
            var searchCriteria = new FullTextSearchCriteria(savedOnlyArtifacts.First().Name, selectedProjectIds);

            // Execute: Execute FullTextSearch with search term using the different user
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.FullTextSearch.Search(_userSecond, searchCriteria), "Nova FullTextSearch call failed when using following search term: {0} which matches with saved only artifacts!", searchCriteria.Query);

            // Validation: Verify that searchResult contains empty list of FullTextSearchItems 
            FullTextSearchResultValidation(fullTextSearchResult);
        }

        [TestCase]
        [TestRail(182341)]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [Description("Searching with the search criteria that matches with published artifacts. Execute Search - Must return SearchResult with list of FullTextSearchItems.")]
        public void FullTextSearch_SearchPublishedArtifact_VerifySearchResultIncludesItem()
        {
            // Setup: Create search criteria with search term that matches published artifact(s) description
            FullTextSearchResult fullTextSearchResult = null;
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);

            var searchCriteria = new FullTextSearchCriteria(_publishedArtifacts.First().Properties.Find(p => p.Name.Equals("Name")).TextOrChoiceValue, selectedProjectIds);

            // Execute: Execute FullTextSearch with search terms that matches published artifact(s) description
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.FullTextSearch.Search(_userSecond, searchCriteria), "Nova FullTextSearch call failed when using following search term: {0} which matches with published artifacts!", searchCriteria.Query);

            // Validation: Verify that searchResult contains published artifacts
            FullTextSearchResultValidation(fullTextSearchResult, _publishedArtifacts);
        }

        [TestCase]
        [TestRail(182342)]
        [Ignore(IgnoreReasons.UnderDevelopment)]
        [Description("Searching with the search criteria that matches with deleted but not published artifacts. Execute Search with the same user - Must return SearchResult with empty list of FullTextSearchItems.")]
        public void FullTextSearch_SearchDeletedNotPublishedArtifact_VerifyWithSameUserEmptySearchResult()
        {
            // Setup
            FullTextSearchResult fullTextSearchResult = null;
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);

            // Setup: Delete all published artifacts
            foreach (var publishedArtifact in _publishedArtifacts)
            {
                publishedArtifact.Delete(_user);
            }

            // Setup: Create search criteria with search term that maches with deleted but not published artifact(s) description
            var searchCriteria = new FullTextSearchCriteria(_publishedArtifacts.First().Properties.Find(p => p.Name.Equals("Description")).TextOrChoiceValue, selectedProjectIds);

            // Execute: Execute FullTextSearch with search terms that maches deleted but not published artifact(s) description
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.FullTextSearch.Search(_user, searchCriteria), "Nova FullTextSearch call failed when using following search term: {0} which matches with deleted but not published artifacts!", searchCriteria.Query);

            // Validation: Verify that searchResult contains empty list of FullTextSearchItems
            FullTextSearchResultValidation(fullTextSearchResult);
        }

        [TestCase]
        [TestRail(182370)]
        [Ignore(IgnoreReasons.UnderDevelopment)]
        [Description("Searching with the search criteria that matches with deleted but not published artifacts. Execute Search - Must return SearchResult with list of FullTextSearchItems.")]
        public void FullTextSearch_SearchDeletedNotPublishedArtifact_VerifyWithDifferentUserSearchResultIncludesItem()
        {
            // Setup
            FullTextSearchResult fullTextSearchResult = null;
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);

            // Setup: Delete all published artifacts
            foreach (var publishedArtifact in _publishedArtifacts)
            {
                publishedArtifact.Delete(_user);
            }

            // Setup: Create search criteria with search term that maches with deleted but not published artifact(s) description
            var searchCriteria = new FullTextSearchCriteria(_publishedArtifacts.First().Properties.Find(p => p.Name.Equals("Description")).TextOrChoiceValue, selectedProjectIds);

            // Execute: Execute FullTextSearch with search terms that maches deleted but not published artifact(s) description
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.FullTextSearch.Search(_userSecond, searchCriteria), "Nova FullTextSearch call failed when using following search term: {0} which matches with deleted but not published artifacts!", searchCriteria.Query);

            // Validation: Verify that searchResult contains list of FullTextSearchItems
            FullTextSearchResultValidation(fullTextSearchResult, _publishedArtifacts);
        }

        [TestCase]
        [TestRail(182343)]
        [Ignore(IgnoreReasons.UnderDevelopment)]
        [Description("Searching with the search criteria that matches with deleted and published artifacts. Execute Search with the same user - Must return SearchResult with empty list of FullTextSearchItems.")]
        public void FullTextSearch_SearchDeletedAndPublishedArtifact_VerifyWithSameUserEmptySearchResult()
        {
            // Setup
            FullTextSearchResult fullTextSearchResult = null;
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);

            // Setup: Delete all published artifacts
            foreach (var publishedArtifact in _publishedArtifacts)
            {
                publishedArtifact.Delete(_user);
                publishedArtifact.Publish(_user);
            }

            // Setup: Create search criteria with search term that maches with deleted and published artifact(s) description
            var searchCriteria = new FullTextSearchCriteria(_publishedArtifacts.First().Properties.Find(p => p.Name.Equals("Description")).TextOrChoiceValue, selectedProjectIds);

            // Execute: Execute FullTextSearch with search terms that maches deleted and published artifact(s) description
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.FullTextSearch.Search(_user, searchCriteria), "Nova FullTextSearch call failed when using following search term: {0} which matches with deleted and published artifacts!", searchCriteria.Query);

            // Validation: Verify that searchResult contains empty list of FullTextSearchItems
            FullTextSearchResultValidation(fullTextSearchResult);
        }

        [TestCase]
        [TestRail(182371)]
        [Ignore(IgnoreReasons.UnderDevelopment)]
        [Description("Searching with the search criteria that matches with deleted and published artifacts. Execute Search with different user - Must return SearchResult with empty list of FullTextSearchItems.")]
        public void FullTextSearch_SearchDeletedAndPublishedArtifact_VerifyWithDifferentUserEmptySearchResult()
        {
            // Setup
            FullTextSearchResult fullTextSearchResult = null;
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);

            // Setup: Delete all published artifacts
            foreach (var publishedArtifact in _publishedArtifacts)
            {
                publishedArtifact.Delete(_user);
                publishedArtifact.Publish(_user);
            }

            // Setup: Create search criteria with search term that maches with deleted and published artifact(s) description
            var searchCriteria = new FullTextSearchCriteria(_publishedArtifacts.First().Properties.Find(p => p.Name.Equals("Description")).TextOrChoiceValue, selectedProjectIds);

            // Execute: Execute FullTextSearch with search terms that maches deleted and published artifact(s) description
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.FullTextSearch.Search(_userSecond, searchCriteria), "Nova FullTextSearch call failed when using following search term: {0} which matches with deleted and published artifacts!", searchCriteria.Query);

            // Validation: Verify that searchResult contains empty list of FullTextSearchItems
            FullTextSearchResultValidation(fullTextSearchResult);
        }

        [TestCase]
        [TestRail(182258)]
        [Ignore(IgnoreReasons.UnderDevelopment)]
        [Description("Searching with the search criteria that matches with published artifacts using user doesn't have permission to the project. Execute Search - Must return SearchResult with empty list of FullTextSearchItems.")]
        public void FullTextSearch_SearchWithoutPermissionOnProjects_VerifyEmptyFullTextSearchItemsOnSearchResult()
        {
            // Setup: Create search criteria with search term that matches published artifact(s) description
            FullTextSearchResult fullTextSearchResult = null;
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);

            var searchCriteria = new FullTextSearchCriteria(_publishedArtifacts.First().Properties.Find(p => p.Name.Equals("Name")).TextOrChoiceValue, selectedProjectIds);

            // Setup: Create user with no permission on any project
            var userWithNoPermissionOnAnyProject = TestHelper.CreateUserWithProjectRolePermissions(Helper, role: TestHelper.ProjectRole.None, projects: _projects);

            // Execute: Execute FullTextSearch with search terms that maches published artifact(s) name
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.FullTextSearch.Search(userWithNoPermissionOnAnyProject, searchCriteria), "Nova FullTextSearch call failed when using following search term: {0} which matches with published artifacts!", searchCriteria.Query);

            // Validation: Verify that searchResult contains empty list of FullTextSearchItems
            FullTextSearchResultValidation(fullTextSearchResult);
        }

        [TestCase(1, TestHelper.ProjectRole.Viewer)]
        [TestCase(1, TestHelper.ProjectRole.Author)]
        [TestCase(0, TestHelper.ProjectRole.None)]
        [TestRail(182374)]
        [Description("Searching with the search criteria that matches with published artifacts using user have permission to certain project(s). Execute Search - Must return corresponding SearchResult based on user's permission per project")]
        public void FullTextSearch_SearchWithPermissionOnProjects_VerifyCorrespondingFullTextSearchItemsOnSearchResult(
            int permissionAvailableProjectCount, 
            TestHelper.ProjectRole projectRole)
        {
            // Setup: Create search criteria with search term that matches published artifact(s) description
            FullTextSearchResult fullTextSearchResult = null;
            var searchProjectIds = _projects.ConvertAll(project => project.Id);

            List<IProject> selectedProjects = new List<IProject>();
            List<IArtifactBase> selectedPublishedArtifacts = new List<IArtifactBase>();

            selectedProjects.AddRange(_projects.Take(permissionAvailableProjectCount));

            foreach (var selectedProjectId in selectedProjects.ConvertAll(o=>o.Id))
            {
                selectedPublishedArtifacts.AddRange(_publishedArtifacts.FindAll(a => a.ProjectId.Equals(selectedProjectId)));
            }

            var searchCriteria = new FullTextSearchCriteria(_publishedArtifacts.First().Properties.Find(p => p.Name.Equals("Name")).TextOrChoiceValue, projectIds: searchProjectIds);

            // Setup: Create user with the specific permission on project(s)
            var userForPermissionTest = TestHelper.CreateUserWithProjectRolePermissions(Helper, role: projectRole, projects: selectedProjects);

            // Execute: Execute FullTextSearch with search terms that maches published artifact(s) description
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.FullTextSearch.Search(userForPermissionTest, searchCriteria), "Nova FullTextSearch call failed when using following search term: {0} which matches with published artifacts!", searchCriteria.Query);

            // Validation: Verify that searchResult contains list of FullTextSearchItems depending on permission for project(s)
            FullTextSearchResultValidation(fullTextSearchResult, selectedPublishedArtifacts);
        }

        [TestCase]
        [TestRail(182344)]
        [Description("Searching with the search criteria that matches with older version of published artifacts. Execute Search - Must return SearchResult with empty list of FullTextSearchItems.")]
        public void FullTextSearch_SearchOlderVersionArtifact_VerifyEmptySearchResults()
        {
            // Setup
            FullTextSearchResult fullTextSearchResult = null;
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);

            // Setup: Create search criteria with search term that maches with current version of artifact(s) description
            var searchCriteria = new FullTextSearchCriteria(_publishedArtifacts.First().Properties.Find(p => p.Name.Equals("Description")).TextOrChoiceValue, selectedProjectIds);

            // Setup: Create search criteria with search term that maches with new version of artifact(s) description
            var newSearchTerm = "NewDescription";
            var newSearchCriteria = new FullTextSearchCriteria(newSearchTerm, selectedProjectIds);

            // Setup: Update all published artifacts with updated description value that doesn't match old search criteria
            foreach (var publishedArtifact in _publishedArtifacts.ConvertAll(o => (IArtifact)o))
            {
                publishedArtifact.Lock(_user);
                SearchServiceTestHelper.UpdateArtifactProperty(Helper, _user, _projects.Find(p=>p.Id.Equals(publishedArtifact.ProjectId)), publishedArtifact, publishedArtifact.BaseArtifactType, "Description", newSearchTerm);

                publishedArtifact.Publish(_user);
            }

            // Wait until user can see the artifact in the search results or timeout occurs
            SearchServiceTestHelper.WaitForSearchIndexerToUpdate(_user, Helper, newSearchCriteria, _publishedArtifacts.Count());

            // Execute: Execute FullTextSearch with search terms that maches older version of artifact(s) description
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.FullTextSearch.Search(_userSecond, searchCriteria), "Nova FullTextSearch call failed when using following search term: {0} which matches with older version of artifacts!", searchCriteria.Query);

            // Validation: Verify that searchResult contains empty list of FullTextSearchItems
            FullTextSearchResultValidation(fullTextSearchResult);
        }

        #endregion 200 OK Tests

        #region 400 Bad Request Tests

        [TestCase]
        [TestRail(166162)]
        [Description("Searching with invalid search criteria. Execute Search - Must return 400 bad request")]
        public void FullTextSearch_SearchWithInvalidSearchCriteria_400BadRequest()
        {
            // Setup: Create searchable artifact(s) with unique search terms
            FullTextSearchResult fullTextSearchResult = null;
            var invalidSearchCriteria = new FullTextSearchCriteria();

            // Execute: Execute FullTextSearch with invalid Search criteria
            var ex = Assert.Throws<Http400BadRequestException>(() => fullTextSearchResult = Helper.FullTextSearch.Search(_user, invalidSearchCriteria), "Nova FullTextSearch call should exit with 400 BadRequestException when using invalid search criteria!");
            
            // Validation: Exception should contain empty response content.
            Assert.That(ex.RestResponse.Content.Length.Equals(0), "FullTextSearch with invalid searchCriteria should return empty content but {0} is returned", ex.RestResponse.Content.ToString());
        }

        [TestCase]
        [TestRail(166164)]
        [Description("Searching with the search term less than miminum size. Execute Search - Must return 400 bad request")]
        public void FullTextSearch_SearchWithSearchTermLessThanMimimumSize_400BadRequest()
        {
            // Setup: Create searchable artifact(s) with the search term less than minimum size
            FullTextSearchResult fullTextSearchResult = null;
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var lessThanMinimumSearchTermSearchCriteria = new FullTextSearchCriteria("ox", selectedProjectIds);

            // Execute: Execute FullTextSearch with the search term less than minimum size
            var ex = Assert.Throws<Http400BadRequestException>(() => fullTextSearchResult = Helper.FullTextSearch.Search(_user, lessThanMinimumSearchTermSearchCriteria), "Nova FullTextSearch call shuold exit with 400 BadRequestException when using less than minium length search term!");

            // Validation: Exception should contain empty response content.
            Assert.That(ex.RestResponse.Content.Length.Equals(0), "FullTextSearch with invalid searchCriteria should return empty content but {0} is returned", ex.RestResponse.Content.ToString());
        }

        #endregion 400 Bad Request Tests

        #region 401 Unauthorized Tests

        [TestCase]
        [TestRail(166163)]
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

                for (int i = 0; i < Math.Min(artifactsToBeFound.Count, (int)pageSize); i++)
                {
                    Assert.That(ReturnedFullTextSearchItemArtifactIds.Contains(artifactsToBeFound[i].Id), "The expected artifact whose Id is {0} does not exist on the response from the Nova FullTextSearch call.", artifactsToBeFound[i].Id);
                }
            } else
            {
                Assert.That(searchResult.FullTextSearchItems.Count().Equals(0), "The FullTextSearchItems should be null list when expected return result is empty but the response from the Nova FullTextSearch call returns {0} results", searchResult.FullTextSearchItems.Count());
            }

            // Validation: Verify that searchResult uses page value passed as optional parameter and DefaultPageSize
            Assert.That(searchResult.Page.Equals(page), "The expected default page value is {0} but {1} was found from the returned searchResult.", page, searchResult.Page);
            Assert.That(searchResult.PageSize.Equals(pageSize), "The expected default pagesize value is {0} but {1} was found from the returned searchResult.", pageSize, searchResult.PageSize);
        }
        
        #endregion Private Functions

    }
}
