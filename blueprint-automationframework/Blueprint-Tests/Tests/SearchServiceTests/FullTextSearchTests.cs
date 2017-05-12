using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ModelHelpers;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.Impl;
using Model.SearchServiceModel.Impl;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TestCommon;
using Utilities;
using Utilities.Facades;
using Utilities.Factories;

namespace SearchServiceTests
{
    /// <summary>
    /// Base class for FullTextSearchTestsSharedFixture and FullTextSearchTests.
    /// </summary>
    public class FullTextSearchTestsBase : TestBase
    {
        protected const string FULLTEXTSEARCH_PATH = RestPaths.Svc.SearchService.FULLTEXTSEARCH;
        protected const int DEFAULT_PAGE_VALUE = 1;
        protected const int DEFAULT_PAGESIZE_VALUE = 10;

        protected IUser _user { get; set; } = null;
        protected IUser _userSecond { get; set; } = null;
        protected List<IProject> _projects { get; private set; } = null;
        protected List<ArtifactWrapper> _publishedArtifacts { get; private set; } = null;

        /// <summary>
        /// This code should be called in the Setup() or ClassSetup() method to set up the test fixture.
        /// </summary>
        protected void SetupData()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _userSecond = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _projects = ProjectFactory.GetAllProjects(_user, shouldRetrieveArtifactTypes: true);
            _publishedArtifacts = SearchServiceTestHelper.SetupFullTextSearchData(_projects, _user, Helper);
        }

        /// <summary>
        /// This code should be called in the TearDown() or ClassTearDown() to destroy the test fixture.
        /// </summary>
        protected void TearDownData()
        {
            Helper?.Dispose();
        }

        /// <summary>
        /// Asserts that returned searchResult from the FullTextSearch call match with artifacts that are being searched.
        /// </summary>
        /// <param name="searchResult">The searchResult from Nova search call.</param>
        /// <param name="artifactsToBeFound"> (optional) artifacts that are being searched, if this parameter is empty, searched items validation step gets skipped.</param>
        /// <param name="page"> (optional) page value that represents displaying page number of the rearch result</param>
        /// <param name="pageSize"> (optional) pageSize value that indicates number of items that get displayed per page</param>
        protected static void FullTextSearchResultValidation(FullTextSearchResult searchResult,
            List<ArtifactWrapper> artifactsToBeFound = null,
            int? page = null,
            int? pageSize = null)
        {
            ThrowIf.ArgumentNull(searchResult, nameof(searchResult));

            // Setup: Set comparison values
            artifactsToBeFound = artifactsToBeFound ?? new List<ArtifactWrapper>();
            page = page ?? DEFAULT_PAGE_VALUE;
            pageSize = pageSize ?? DEFAULT_PAGESIZE_VALUE;

            page = page.Equals(0) ? DEFAULT_PAGE_VALUE : page;
            pageSize = pageSize.Equals(0) ? DEFAULT_PAGESIZE_VALUE : pageSize;

            var returnedFullTextSearchItemArtifactIds = new List<int>();

            if (artifactsToBeFound.Any())
            {
                searchResult.Items.ToList().ForEach(a => returnedFullTextSearchItemArtifactIds.Add(a.ArtifactId));

                for (int i = 0; i < Math.Min(artifactsToBeFound.Count, (int)pageSize); i++)
                {
                    Assert.That(returnedFullTextSearchItemArtifactIds.Contains(artifactsToBeFound[i].Id),
                        "The expected artifact whose Id is {0} does not exist on the response from the Nova FullTextSearch call.",
                        artifactsToBeFound[i].Id);
                }
            }
            else
            {
                Assert.AreEqual(0, searchResult.Items.Count(),
                    "The FullTextSearchItems should be null list when expected return result is empty but the response from the Nova FullTextSearch call returns {0} results",
                    searchResult.Items.Count());
            }

            // Validation: Verify that searchResult uses page value passed as optional parameter and DefaultPageSize
            Assert.That(searchResult.Page.Equals(page),
                "The expected default page value is {0} but {1} was found from the returned searchResult.",
                page, searchResult.Page);
            Assert.That(searchResult.PageSize.Equals(pageSize),
                "The expected default pagesize value is {0} but {1} was found from the returned searchResult.",
                pageSize, searchResult.PageSize);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    [TestFixture]
    [Category(Categories.SearchService)]
    public class FullTextSearchTestsSharedFixture : FullTextSearchTestsBase
    {
        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            SetupData();
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            TearDownData();
        }

        #region 200 OK Tests

        [TestCase(2)]
        [TestRail(181022)]
        [Description("Searching with optional page parameter. Execute Search - Must return SearchResult that uses the requested page value.")]
        public void FullTextSearch_SearchWithPageOnly_VerifySearchResult(int page)
        {
            // Setup: Create searchable artifact(s) with unique search term
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria("NonExistingSearchTerm", selectedProjectIds);

            // Execute: Execute FullTextSearch with search term with page parameter value
            FullTextSearchResult fullTextSearchResult = null;
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_user, searchCriteria, page),
                "POST {0} call failed when using search term {1} with page parameter = {2}!", FULLTEXTSEARCH_PATH, searchCriteria.Query, page);

            // Validation: Verify that searchResult uses poptional page value and and DefaultPageSize
            FullTextSearchResultValidation(searchResult: fullTextSearchResult, page: page);
        }

        [TestCase(3)]
        [TestCase(72)]
        [TestRail(181023)]
        [Description("Searching with optional pageSize parameter.  Execute Search - Must return SearchResult that uses the " +
            "default page value (FirstPage) and the requested pageSize value.")]
        public void FullTextSearch_SearchWithPageSizeOnly_VerifySearchResult(int pageSize)
        {
            // Setup: Create searchable artifact(s) with unique search term
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria("NonExistingSearchTerm", selectedProjectIds);

            // Execute: Execute FullTextSearch with search term with pageSize parameter value
            FullTextSearchResult fullTextSearchResult = null;
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_user, searchCriteria, pageSize: pageSize),
                "POST {0} call failed when using search term {1} with pageSize parameter = {2}!",
                FULLTEXTSEARCH_PATH, searchCriteria.Query, pageSize);

            // Validation: Verify that searchResult uses FirstPage and optional pageSize value
            FullTextSearchResultValidation(fullTextSearchResult, pageSize: pageSize);
        }

        [TestCase(4, 5)]
        [TestCase(12, 38)]
        [TestRail(166156)]
        [Description("Searching with both optional page and pageSize parameters.  Execute Search - " +
            "Must return SearchResult that uses requested page and pageSize values.")]
        public void FullTextSearch_SearchWithBothPageAndPageSize_VerifySearchResult(int page, int pageSize)
        {
            // Setup: Create searchable artifact(s) with unique search term
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria("NonExistingSearchTerm", selectedProjectIds);

            // Execute: Execute FullTextSearch with search term with both page and pageSize parameter values
            FullTextSearchResult fullTextSearchResult = null;
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_user, searchCriteria, page, pageSize),
                "POST {0} call failed when using search term {1} with page paramter = {2} and pageSize parameter = {3}!",
                FULLTEXTSEARCH_PATH, searchCriteria.Query, page, pageSize);

            // Validation: Verify that searchResult uses optional page and pageSize values
            FullTextSearchResultValidation(fullTextSearchResult, page: page, pageSize: pageSize);
        }

        [TestCase]
        [TestRail(166155)]
        [Description("Searching without both optional parameters, (page and pagesize).  Execute Search - " +
            "Must return SearchResult that uses default page value (FirstPage) and default pageSize values.")]
        public void FullTextSearch_SearchWithoutBothPageAndPageSize_VerifySearchResultUsesFirstPageAndDefaultPageSize()
        {
            // Setup: Create searchable artifact(s) with unique search term
            var projectIds = new List<int>() { 2, 4 };
            var searchCriteria = new FullTextSearchCriteria("NonExistingSearchTerm", projectIds);

            // Execute: Execute FullTextSearch with search term
            FullTextSearchResult fullTextSearchResult = null;
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_user, searchCriteria),
                "POST {0} call failed when using search term {1}!", FULLTEXTSEARCH_PATH, searchCriteria.Query);

            // Validation: Verify that searchResult uses default page value (FirstPage) and the Default pageSize value
            FullTextSearchResultValidation(fullTextSearchResult, page: DEFAULT_PAGE_VALUE);
        }

        [TestCase(-3)]
        [TestCase(int.MinValue)]
        [TestRail(181024)]
        [Description("Searching with invalid page value. Execute Search - Must return SearchResult that uses default page value (FirstPage).")]
        public void FullTextSearch_SearchWithInvalidPage_VerifySearchResultUsesFirstPage(int page)
        {
            // Setup: Create searchable artifact(s) with unique search term
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria("NonExistingSearchTerm", selectedProjectIds);

            // Execute: Execute FullTextSearch with search term with invalid page parameter value
            FullTextSearchResult fullTextSearchResult = null;
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_user, searchCriteria, page),
                "POST {0} call failed when using search term {1} with invalid page paramter = {2}!", FULLTEXTSEARCH_PATH, searchCriteria.Query, page);

            // Validation: Verify that searchResult uses default page value (FirstPage)
            FullTextSearchResultValidation(fullTextSearchResult, page: DEFAULT_PAGE_VALUE);
        }

        [TestCase(-10)]
        [TestCase(int.MinValue)]
        [TestRail(181025)]
        [Description("Searching with invalid pageSize value. Execute Search - Must return SearchResult that uses default pageSize.")]
        public void FullTextSearch_SearchWithInvalidPageSize_VerifySearchResultUsesDefaultPageSize(int pageSize)
        {
            // Setup: Create searchable artifact(s) with unique search term
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria("NonExistingSearchTerm", selectedProjectIds);

            // Execute: Execute FullTextSearch with search term with invalid pageSize parameter value
            FullTextSearchResult fullTextSearchResult = null;
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_user, searchCriteria, pageSize: pageSize),
                "POST {0} call failed when using search term {1} with invalid pageSize parameter = {2}!", FULLTEXTSEARCH_PATH, searchCriteria.Query, pageSize);

            // Validation: Verify that searchResult uses default PageSize value
            FullTextSearchResultValidation(fullTextSearchResult, pageSize: DEFAULT_PAGESIZE_VALUE);
        }

        [TestCase(-12, -100)]
        [TestCase(int.MinValue, int.MinValue)]
        [TestRail(181021)]
        [Description("Searching with both invalid page and pageSize values.  Execute Search - Must return SearchResult that uses default " +
            "page value (FirstPage) and default pageSize.")]
        public void FullTextSearch_SearchWithInvalidPageAndPageSize_VerifySearchResultUsesFirstPageAndDefaultPageSize(int page, int pageSize)
        {
            // Setup: Create searchable artifact(s) with unique search term
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria("NonExistingSearchTerm", selectedProjectIds);

            // Execute: Execute FullTextSearch with search term with invalid page and invalid pageSize parameter values
            FullTextSearchResult fullTextSearchResult = null;
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_user, searchCriteria, page, pageSize),
                "POST {0} call failed when using search term {1} with invalid page parameter = {2} and invalid pageSize parameter = {3}!",
                FULLTEXTSEARCH_PATH, searchCriteria.Query, page, pageSize);

            // Validation: Verify that searchResult uses default page value (FirstPage) and default PageSize value
            FullTextSearchResultValidation(fullTextSearchResult, page: DEFAULT_PAGE_VALUE, pageSize: DEFAULT_PAGESIZE_VALUE);
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(182340)]
        [Description("Searching with the search criteria that matches with saved artifacts.  Execute Search with the same user - " +
            "Must return SearchResult with empty list of FullTextSearchItems.")]
        public void FullTextSearch_SearchSavedNotPublishedArtifact_VerifyWithSameUserEmptySearchResult(BaseArtifactType baseArtifactType)
        {
            // Setup: Create 2 artifacts in each project (1 saved and 1 published).
            var savedOnlyArtifacts = new List<IArtifactBase>();
            var publishedArtifacts = new List<IArtifactBase>();

            _projects.ForEach(project => savedOnlyArtifacts.Add(Helper.CreateAndSaveArtifact(project, _user, baseArtifactType)));
            _projects.ForEach(project => publishedArtifacts.Add(Helper.CreateAndPublishArtifact(project, _user, baseArtifactType)));

            // Create search criteria with search term that matches with one of the published artifacts across all projects.
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria(publishedArtifacts.First().Name, selectedProjectIds);

            // Wait until user can see the published artifact in the search results or timeout occurs.
            SearchServiceTestHelper.WaitForFullTextSearchIndexerToUpdate(_user, Helper, searchCriteria, 1);

            // Execute: Execute FullTextSearch with the saved artifact search term using the same user.
            searchCriteria.Query = savedOnlyArtifacts.First().Name;
            FullTextSearchResult fullTextSearchResult = null;

            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_user, searchCriteria),
                "POST {0} call failed when using search term {1} which matches with saved-only artifacts!",
                FULLTEXTSEARCH_PATH, searchCriteria.Query);

            // Validation: Verify that searchResult contains empty list of FullTextSearchItems.
            FullTextSearchResultValidation(fullTextSearchResult);
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(182369)]
        [Description("Searching with the search criteria that matches with saved artifacts.  Execute Search with different user - " +
            "Must return SearchResult with empty list of FullTextSearchItems.")]
        public void FullTextSearch_SearchSavedNotPublishedArtifact_VerifyWithDifferentUserEmptySearchResult(BaseArtifactType baseArtifactType)
        {
            // Setup: Create 2 artifacts in each project (1 saved and 1 published).
            var savedOnlyArtifacts = new List<IArtifactBase>();
            var publishedArtifacts = new List<IArtifactBase>();

            _projects.ForEach(project => savedOnlyArtifacts.Add(Helper.CreateAndSaveArtifact(project, _user, baseArtifactType)));
            _projects.ForEach(project => publishedArtifacts.Add(Helper.CreateAndPublishArtifact(project, _user, baseArtifactType)));

            // Create search criteria with search term that matches with one of the published artifacts across all projects.
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria(publishedArtifacts.First().Name, selectedProjectIds);

            // Wait until first user can see the published artifact in the search results or timeout occurs.
            SearchServiceTestHelper.WaitForFullTextSearchIndexerToUpdate(_user, Helper, searchCriteria, 1);

            // Execute: Execute FullTextSearch with saved artifact search term using the different user.
            searchCriteria.Query = savedOnlyArtifacts.First().Name;
            FullTextSearchResult fullTextSearchResult = null;

            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_userSecond, searchCriteria),
                "POST {0} call failed when using search term {1} which matches with saved-only artifacts!",
                FULLTEXTSEARCH_PATH, searchCriteria.Query);

            // Validation: Verify that searchResult contains empty list of FullTextSearchItems.
            FullTextSearchResultValidation(fullTextSearchResult);
        }

        [TestCase]
        [TestRail(182341)]
        [Description("Searching with the search criteria that matches with published artifacts.  Execute Search - " +
            "Must return SearchResult with list of FullTextSearchItems.")]
        public void FullTextSearch_SearchPublishedArtifact_VerifySearchResultIncludesItem()
        {
            // Setup: Create search criteria with search term that matches with description value of all published artifacts
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchTerm = StringUtilities.ConvertHtmlToText(_publishedArtifacts[0].Description);
            var searchCriteria = new FullTextSearchCriteria(searchTerm, selectedProjectIds);

            // Set the pageSize that can accomodate all expecting search results for the user
            var customSearchPageSize = _publishedArtifacts.Count();

            // Execute: Execute FullTextSearch with search term that matches published artifact(s) description
            FullTextSearchResult fullTextSearchResult = null;
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_user, searchCriteria, pageSize: customSearchPageSize),
                "POST {0} call failed when using search term {1} which matches with published artifacts!", FULLTEXTSEARCH_PATH, searchCriteria.Query);

            // Validation: Verify that searchResult contains all published artifacts
            FullTextSearchResultValidation(searchResult: fullTextSearchResult, artifactsToBeFound: _publishedArtifacts, pageSize: customSearchPageSize);
        }

        [TestCase]
        [TestRail(182342)]
        [Ignore(IgnoreReasons.ProductBug)]  // TFS Bug: 4191  The GET svc/searchservice/itemsearch/fulltextmetadata call doesn't find saved (unpublished) changes
        [Description("Searching with the search criteria that matches with deleted but not published artifacts. Execute Search with the same user - " +
            "Must return SearchResult with empty list of FullTextSearchItems.")]
        public void FullTextSearch_SearchDeletedNotPublishedArtifact_VerifyWithSameUserEmptySearchResult()
        {
            // Setup: Delete all published artifacts
            foreach (var publishedArtifact in _publishedArtifacts)
            {
                publishedArtifact.Delete(_user);
            }

            // Create search criteria with search term that matches with deleted but not published artifact(s) description
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchTerm = StringUtilities.ConvertHtmlToText(_publishedArtifacts[0].Description);
            var searchCriteria = new FullTextSearchCriteria(searchTerm, selectedProjectIds);

            // Wait until first user no longer sees the artifact in search results or timeout occurs
            SearchServiceTestHelper.WaitForFullTextSearchIndexerToUpdate(_user, Helper, searchCriteria, 0, waitForArtifactsToDisappear: true);

            // Execute: Execute FullTextSearch with the search term using the same user
            FullTextSearchResult fullTextSearchResult = null;
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_user, searchCriteria: searchCriteria),
                "POST {0} call failed when using search term {1} which matches with deleted but not published artifacts!", FULLTEXTSEARCH_PATH, searchCriteria.Query);

            // Validation: Verify that searchResult contains empty list of FullTextSearchItems
            FullTextSearchResultValidation(searchResult: fullTextSearchResult);
        }

        [TestCase]
        [TestRail(182370)]// TODO: delete or rewrite test and generate new TestRail ID
        [Ignore(IgnoreReasons.ProductBug)]  // TFS Bug: 4191  The GET svc/searchservice/itemsearch/fulltextmetadata call doesn't find saved (unpublished) changes
        [Description("Searching with the search criteria that matches with deleted but not published artifacts. Execute Search - Must return SearchResult with list of FullTextSearchItems.")]
        public void FullTextSearch_SearchDeletedNotPublishedArtifact_VerifyWithDifferentUserSearchResultIncludesItem()
        {
            // Setup: Delete all published artifacts
            foreach (var publishedArtifact in _publishedArtifacts)
            {
                publishedArtifact.Delete(_user);
            }

            // Create search criteria with search term that matches with deleted but not published artifact(s) description
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchTerm = StringUtilities.ConvertHtmlToText(_publishedArtifacts[0].Description);
            var searchCriteria = new FullTextSearchCriteria(searchTerm, selectedProjectIds);

            // Set the pageSize that can accomodate all expecting search results for the user
            var customSearchPageSize = _publishedArtifacts.Count();

            // Wait until first user no longer sees the artifact in search results or timeout occurs
            SearchServiceTestHelper.WaitForFullTextSearchIndexerToUpdate(_user, Helper, searchCriteria, 0, waitForArtifactsToDisappear: true);

            // Execute: Execute FullTextSearch with the search term using the different user
            FullTextSearchResult fullTextSearchResult = null;
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_userSecond, searchCriteria: searchCriteria),
                "POST {0} call failed when using search term {1} which matches with deleted but not published artifacts!", FULLTEXTSEARCH_PATH, searchCriteria.Query);

            // Validation: Verify that searchResult contains list of FullTextSearchItems
            FullTextSearchResultValidation(searchResult: fullTextSearchResult, artifactsToBeFound: _publishedArtifacts, pageSize: customSearchPageSize);
        }

        [Explicit(IgnoreReasons.FlakyTest)] // This test deletes the artifacts in the FixtureSetup which then causes all tests after it to fail.
        [TestCase]
        [TestRail(182343)]
        [Description("Searching with the search criteria that matches with deleted and published artifacts. Execute Search with the same user - " +
            "Must return SearchResult with empty list of FullTextSearchItems.")]
        public void FullTextSearch_SearchDeletedAndPublishedArtifact_VerifyWithSameUserEmptySearchResult()
        {
            // Setup: Delete all published artifacts
            foreach (var publishedArtifact in _publishedArtifacts)
            {
                publishedArtifact.Delete(_user);
                publishedArtifact.Publish(_user);
            }

            // Create search criteria with search term that matches with deleted and published artifact(s) description
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchTerm = StringUtilities.ConvertHtmlToText(_publishedArtifacts[0].Description);
            var searchCriteria = new FullTextSearchCriteria(searchTerm, selectedProjectIds);

            // Execute: Execute FullTextSearch with the search term using the same user
            FullTextSearchResult fullTextSearchResult = null;
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_user, searchCriteria: searchCriteria),
                "POST {0} call failed when using search term {1} which matches with deleted and published artifacts!", FULLTEXTSEARCH_PATH, searchCriteria.Query);

            // Validation: Verify that searchResult contains empty list of FullTextSearchItems
            FullTextSearchResultValidation(searchResult: fullTextSearchResult);
        }

        [Explicit(IgnoreReasons.FlakyTest)] // This test deletes the artifacts in the FixtureSetup which then causes all tests after it to fail.
        [TestCase]
        [TestRail(182371)]
        [Description("Searching with the search criteria that matches with deleted and published artifacts. Execute Search with different user - " +
            "Must return SearchResult with empty list of FullTextSearchItems.")]
        public void FullTextSearch_SearchDeletedAndPublishedArtifact_VerifyWithDifferentUserEmptySearchResult()
        {
            // Setup: Delete all published artifacts
            foreach (var publishedArtifact in _publishedArtifacts)
            {
                publishedArtifact.Delete(_user);
                publishedArtifact.Publish(_user);
            }

            // Create search criteria with search term that matches with deleted and published artifact(s) description
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchTerm = StringUtilities.ConvertHtmlToText(_publishedArtifacts[0].Description);
            var searchCriteria = new FullTextSearchCriteria(searchTerm, selectedProjectIds);

            // Execute: Execute FullTextSearch with the search term using the different user
            FullTextSearchResult fullTextSearchResult = null;
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_userSecond, searchCriteria: searchCriteria),
                "POST {0} call failed when using search term {1} which matches with deleted and published artifacts!", FULLTEXTSEARCH_PATH, searchCriteria.Query);

            // Validation: Verify that searchResult contains empty list of FullTextSearchItems
            FullTextSearchResultValidation(searchResult: fullTextSearchResult);
        }

        [TestCase]
        [TestRail(182258)]
        [Description("Searching with the search criteria that matches with published artifacts using user doesn't have permission to the project.  " +
            "Execute Search - Must return SearchResult with empty list of FullTextSearchItems.")]
        public void FullTextSearch_SearchWithoutPermissionOnProjects_VerifyEmptyFullTextSearchItemsOnSearchResult()
        {
            // Setup: Create search criteria with search term that matches published artifact(s) name
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria(_publishedArtifacts[0].Name, selectedProjectIds);

            // Create user with no permission on any project
            var userWithNoPermissionOnAnyProject = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.None, _projects);

            // Execute: Execute FullTextSearch with search term that matches published artifact(s) name
            FullTextSearchResult fullTextSearchResult = null;
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(userWithNoPermissionOnAnyProject, searchCriteria),
                "POST {0} call failed when using search term {1} which matches with published artifacts!",
                FULLTEXTSEARCH_PATH, searchCriteria.Query);

            // Validation: Verify that searchResult contains empty list of FullTextSearchItems
            FullTextSearchResultValidation(fullTextSearchResult);
        }

        [TestCase(1, TestHelper.ProjectRole.Viewer)]
        [TestCase(1, TestHelper.ProjectRole.AuthorFullAccess)]
        [TestCase(1, TestHelper.ProjectRole.None)]
        [TestRail(182374)]
        [Description("Searching with the search criteria that matches with published artifacts using user have permission to certain project(s).  " +
            "Execute Search - Must return corresponding SearchResult based on user's permission per project.")]
        public void FullTextSearch_SearchWithPermissionOnProjects_VerifyCorrespondingFullTextSearchItemsOnSearchResult(
            int numberOfProjectForPermissionTesting, 
            TestHelper.ProjectRole projectRole)
        {
            // Setup: Create search criteria with search term that matches published artifact(s) description
            var searchProjectIds = _projects.ConvertAll(project => project.Id);
            var searchTerm = StringUtilities.ConvertHtmlToText(_publishedArtifacts[0].Description);
            var searchCriteria = new FullTextSearchCriteria(searchTerm, projectIds: searchProjectIds);

            // Calculate expecting values for the selected project(s) and published artifacts for the project(s)
            var selectedProjects = new List<IProject>();
            selectedProjects.AddRange(_projects.Take(numberOfProjectForPermissionTesting));
            var publishedArtifactsForSelectedProjects = new List<ArtifactWrapper>();

            foreach (var selectedProjectId in selectedProjects.ConvertAll(o => o.Id))
            {
                publishedArtifactsForSelectedProjects.AddRange(_publishedArtifacts.FindAll(a => a.ProjectId.Equals(selectedProjectId)));
            }

            if (projectRole.Equals(TestHelper.ProjectRole.None))
            {
                publishedArtifactsForSelectedProjects.Clear();
            }

            // Set the pageSize that displays all expecting search results for the user with permission on selected project(s)
            var customSearchPageSize = publishedArtifactsForSelectedProjects.Count();

            // Execute: Execute FullTextSearch with the search term using the user with the specific permission on project(s)
            var userWithSelectiveProjectPermission = Helper.CreateUserWithProjectRolePermissions(projectRole, selectedProjects);
            FullTextSearchResult fullTextSearchResult = null;

            Assert.DoesNotThrow(() =>
            {
                fullTextSearchResult = Helper.SearchService.FullTextSearch(userWithSelectiveProjectPermission, searchCriteria,
                    pageSize: customSearchPageSize);
            },
                "POST {0} call failed when using search term {1} which matches with published artifacts!",
                FULLTEXTSEARCH_PATH, searchCriteria.Query);

            // Validation: Verify that searchResult contains list of FullTextSearchItems depending on permission for project(s)
            FullTextSearchResultValidation(fullTextSearchResult, artifactsToBeFound: publishedArtifactsForSelectedProjects, pageSize: customSearchPageSize);
        }

        [TestCase]
        [TestRail(182344)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [Description("Searching with the search criteria that matches with older version of published artifacts.  Execute Search - " +
            "Must return SearchResult with empty list of FullTextSearchItems.")]
        public void FullTextSearch_SearchOlderVersionArtifact_VerifyEmptySearchResults()
        {
            // Setup: Create few artifacts to search
            var selectedBasedArtifactTypes = new List<ItemTypePredefined> { ItemTypePredefined.Actor };
            var publishedArtifacts = SearchServiceTestHelper.SetupFullTextSearchData(_projects, _user, Helper, selectedBasedArtifactTypes);

            // Create search criteria with search term that matches with current version of artifact(s) description
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchTerm = StringUtilities.ConvertHtmlToText(publishedArtifacts[0].Description);
            var searchCriteria = new FullTextSearchCriteria(searchTerm, selectedProjectIds);

            // Create search criteria with search term that matches with new version of artifact(s) description
            var newSearchTerm = "NewDescription" + RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);
            var newSearchCriteria = new FullTextSearchCriteria(newSearchTerm, selectedProjectIds);

            // Update all published artifacts with the new description value that matches with the new search criteria
            foreach (var publishedArtifact in publishedArtifacts)
            {
                publishedArtifact.Lock(_user);
                publishedArtifact.SaveWithNewDescription(_user, newSearchTerm);
                publishedArtifact.Publish(_user);
            }

            // Wait until user can see the artifact in the search results or timeout occurs
            SearchServiceTestHelper.WaitForFullTextSearchIndexerToUpdate(_user, Helper, newSearchCriteria, publishedArtifacts.Count());

            // Execute: Execute FullTextSearch with the old search term
            FullTextSearchResult fullTextSearchResult = null;
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_userSecond, searchCriteria),
                "POST {0} call failed when using search term {1} which matches with older version of artifacts!", FULLTEXTSEARCH_PATH, searchCriteria.Query);

            // Validation: Verify that searchResult contains empty list of FullTextSearchItems
            FullTextSearchResultValidation(fullTextSearchResult);
        }

        [TestCase(2)]
        [TestCase(6)]
        [TestRail(182447)]
        [Description("Searching with the search criteria that returns multiple pages for SearchResult.  Execute Search - " +
            "Verify that number of result items matches with expecting search result items.")]
        public void FullTextSearch_SearchWithSearchTermReturnsMultiplePages_VerifyResultItemCountWithExpected(int pageSize)
        {
            // Setup: Create search criteria that will return multiple page for SearchResult
            var searchTerm = StringUtilities.ConvertHtmlToText(_publishedArtifacts[0].Description);
            var searchCriteria = new FullTextSearchCriteria(searchTerm, _projects.ConvertAll(o => o.Id));

            // Calculate expecting search result counts
            var expectedSearchResultCount = _publishedArtifacts.Count();
            var expectedPageCount = (expectedSearchResultCount % pageSize).Equals(0) ? expectedSearchResultCount / pageSize : expectedSearchResultCount / pageSize + 1;
            var expectedArtifactsStackDescOrderedByLastEditedOn = CreateDescendingOrderedLastEditedOnArtifactStack(_publishedArtifacts);

            // Execute: Execute FullTextSearch with pageSize
            var returnedSearchCount = 0;
            var pageCount = 1;

            while ( pageCount <= expectedPageCount)
            {
                // Execute Search with page and pageSize
                FullTextSearchResult fullTextSearchResult = null;
                Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_user, searchCriteria, pageCount, pageSize),
                    "POST {0} call failed when using search term {1}, page: {2}, pageSize: {3}",
                    FULLTEXTSEARCH_PATH, searchCriteria.Query, pageCount, pageSize);

                // Adds search result per page into total returned search count
                returnedSearchCount += fullTextSearchResult.Items.Count();

                // Create a artifact list per page, decending ordered by Last Edited On
                var pagedArtifacts = CreateArtifactListPerPage(expectedArtifactsStackDescOrderedByLastEditedOn, pageSize);

                // Validation: Verify that searchResult contains list of FullTextSearchItems
                FullTextSearchResultValidation(fullTextSearchResult, artifactsToBeFound: pagedArtifacts, page: pageCount, pageSize: pageSize);

                pageCount++;
            }

            // Validation: Verify that expected search count is equal to returned search count
            Assert.That(returnedSearchCount.Equals(expectedSearchResultCount),
                "Expected search result count is {0} but {1} was returned", expectedSearchResultCount, returnedSearchCount);
        }

        [TestCase]
        [TestRail(182448)]
        [Description("Searching with the search criteria. Execute Search - Verify that result are sorted by Last Edited On")]
        public void FullTextSearch_SearchWithCriteria_VerifyFullTextSearchItemsAreSortedByLastEditedOn()
        {
            // Setup: Create search criteria with search term that matches published artifact(s) description
            FullTextSearchResult fullTextSearchResult = null;
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchTerm = StringUtilities.ConvertHtmlToText(_publishedArtifacts[0].Description);
            var searchCriteria = new FullTextSearchCriteria(searchTerm, selectedProjectIds);

            // Set the pageSize that displays all expecting search results for the user with permission on selected project(s)
            var customSearchPageSize = _publishedArtifacts.Count();

            // Execute: Execute FullTextSearch with search term that matches published artifact(s) description
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_user, searchCriteria, pageSize: customSearchPageSize),
                "POST {0} call failed when using search term {1} which matches with published artifacts!", FULLTEXTSEARCH_PATH, searchCriteria.Query);

            // Validation: Verify that searchResult contains published artifacts
            FullTextSearchResultValidation(fullTextSearchResult, artifactsToBeFound: _publishedArtifacts, pageSize: customSearchPageSize);

            // Create an expected artifact list, decending ordered by Last Edited On
            var expectedArtifactsStackDescOrderedByLastEditedOn = CreateDescendingOrderedLastEditedOnArtifactStack(_publishedArtifacts);
            var lastEditedOnOrderedArtifacts = CreateArtifactListPerPage(expectedArtifactsStackDescOrderedByLastEditedOn, customSearchPageSize);

            // Compare returned FullTextSearchItems from search result with the expected ordered artifacts
            for (int i = 0; i < fullTextSearchResult.Items.Count(); i++ )
            {
                Assert.That(fullTextSearchResult.Items.ToList()[i].ArtifactId.
                    Equals(lastEditedOnOrderedArtifacts[i].Id),
                    "artfiact with ID {0} was expected from the returned FullTextSearchItems but artifact with ID {1} is found on data row {2}.",
                    lastEditedOnOrderedArtifacts[i].Id, fullTextSearchResult.Items.ToList()[i].ArtifactId, i);
            }
        }

        [Category(Categories.CustomData)]
        [TestCase ("Std-Text-Required-HasDefault", 1)]
        [TestRail(234325)]
        [Description("Search for search term that is present in artifact textual property.  Execute Search - " +
            "Must return SearchResult with list of FullTextSearchItems matching artifacts with textual property.")]
        public void FullTextSearch_SearchPublishedArtifactTextualProperties_VerifySearchResultIncludesItem(
            string propertyName, int expectedHits)
        {
            // Setup: 
            var selectedProjectIds = _projects.ConvertAll(p => p.Id);
            var project = Helper.GetProject(TestHelper.GoldenDataProject.EmptyProjectWithSubArtifactRequiredProperties, _user);
            var artifact = Helper.CreateNovaArtifact(_user, project, ItemTypePredefined.Process,
                artifactTypeName: "Process");

            string searchTerm = "SearchText_" + RandomGenerator.RandomAlphaNumericUpperAndLowerCase(25);

            // Update custom property in artifact.
            var artifactDetailsForUpdate = ArtifactStoreHelper.CreateNovaArtifactDetailsForUpdate(artifact);
            var customProperty = artifact.CustomPropertyValues.Find(property => property.Name == propertyName);
            artifactDetailsForUpdate.CustomPropertyValues.Remove(customProperty);
            customProperty.CustomPropertyValue = searchTerm;
            artifactDetailsForUpdate.CustomPropertyValues.Add(customProperty);
            artifact.Update(_user, artifactDetailsForUpdate);
            artifact.Publish(_user);
            
            var searchCriteria = new FullTextSearchCriteria(searchTerm, selectedProjectIds);
            SearchServiceTestHelper.WaitForFullTextSearchIndexerToUpdate(_user, Helper, searchCriteria, 1, timeoutInMilliseconds: 30000);

            // Execute: 
            FullTextSearchResult fullTextSearchResult = null;
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_user, searchCriteria),
                "POST {0} call failed when using search term {1} which matches with published artifacts!", FULLTEXTSEARCH_PATH, searchCriteria.Query);

            // Verify: 
            Assert.AreEqual(expectedHits, fullTextSearchResult.PageItemCount,
                "The number of hits was {0} but {1} was expected", fullTextSearchResult.PageItemCount, expectedHits);

            FullTextSearchResultValidation(searchResult: fullTextSearchResult, artifactsToBeFound: new List<ArtifactWrapper> { artifact });
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [Category(Categories.CustomData)]
        [TestCase("Std-Text-Required-HasDefault", 1)]
        [TestRail(234326)]
        [Description("Search for search term that is present in subartifact textual property.  Execute Search - " +
            "Must return SearchResult with list of FullTextSearchItems matching artifacts with subartifact textual property.")]
        public void FullTextSearch_SearchPublishedSubArtifactTextualProperties_VerifySearchResultIncludesItem(
            string propertyName, int expectedHits)
        {
            // Setup: 
            var selectedProjectIds = _projects.ConvertAll(p => p.Id);
            var project = Helper.GetProject(TestHelper.GoldenDataProject.EmptyProjectWithSubArtifactRequiredProperties, _user);
            var artifact = Helper.CreateNovaArtifact(_user, project, ItemTypePredefined.Process);

            string searchTerm = "SearchText_" + RandomGenerator.RandomAlphaNumericUpperAndLowerCase(25);

            // Get nova subartifact
            var novaProcess = Helper.Storyteller.GetNovaProcess(_user, artifact.Id);
            var processShape = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            var novaSubArtifact = Helper.ArtifactStore.GetSubartifact(_user, artifact.Id, processShape.Id);

            var artifactDetailsForUpdate = ArtifactStoreHelper.CreateNovaArtifactDetailsForUpdate(artifact);
            var customProperty = novaSubArtifact.CustomPropertyValues.Find(property => property.Name == propertyName);
            novaSubArtifact.CustomPropertyValues.Remove(customProperty);
            customProperty.CustomPropertyValue = searchTerm;
            novaSubArtifact.CustomPropertyValues.Add(customProperty);
            artifactDetailsForUpdate.SubArtifacts = new List<NovaSubArtifact> { novaSubArtifact };
            artifact.Update(_user, artifactDetailsForUpdate);
            artifact.Publish(_user);

            var searchCriteria = new FullTextSearchCriteria(searchTerm, selectedProjectIds);
            SearchServiceTestHelper.WaitForFullTextSearchIndexerToUpdate(_user, Helper, searchCriteria, 1, timeoutInMilliseconds: 30000);

            // Execute: 
            FullTextSearchResult fullTextSearchResult = null;
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_user, searchCriteria),
                "POST {0} call failed when using search term {1} which matches with published artifacts!", FULLTEXTSEARCH_PATH, searchCriteria.Query);

            // Verify: 
            Assert.AreEqual(expectedHits,fullTextSearchResult.PageItemCount,
                "The number of hits was {0} but {1} was expected", fullTextSearchResult.PageItemCount, expectedHits);

            FullTextSearchResultValidation(searchResult: fullTextSearchResult, artifactsToBeFound: new List<ArtifactWrapper> { artifact });
        }

        #endregion 200 OK Tests

        #region 400 Bad Request Tests

        [TestCase]
        [TestRail(166162)]
        [Description("Searching with invalid search criteria. Execute Search - Must return 400 bad request")]
        public void FullTextSearch_SearchWithInvalidSearchCriteria_400BadRequest()
        {
            // Setup: Create invalid search criteria with empty contents
            var invalidSearchCriteria = new FullTextSearchCriteria();

            // Execute: Execute FullTextSearch with invalid Search criteria
            var ex = Assert.Throws<Http400BadRequestException>(() => Helper.SearchService.FullTextSearch(_user, invalidSearchCriteria),
                "POST {0} call should exit with 400 BadRequestException when using invalid search criteria!", FULLTEXTSEARCH_PATH);

            var serviceErrorMessage = SerializationUtilities.DeserializeObject<ServiceErrorMessage>(ex.RestResponse.Content);

            // Validation: Exception should contain proper errorCode in the response content.
            Assert.That(serviceErrorMessage.ErrorCode.Equals(ErrorCodes.IncorrectSearchCriteria),
                "POST {0} call with invalid searchCriteria should return {1} errorCode but {2} is returned",
                FULLTEXTSEARCH_PATH, ErrorCodes.IncorrectSearchCriteria, serviceErrorMessage.ErrorCode);
        }

        [TestCase]
        [TestRail(166164)]
        [Description("Searching with the search term less than mininum size. Execute Search - Must return 400 bad request")]
        public void FullTextSearch_SearchWithSearchTermLessThanMinimumSize_400BadRequest()
        {
            // Setup: Create searchable artifact(s) with the search term less than minimum size
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var lessThanMinimumSearchTermSearchCriteria = new FullTextSearchCriteria("ox", selectedProjectIds);

            // Execute: Execute FullTextSearch with the search term less than minimum size
            var ex = Assert.Throws<Http400BadRequestException>(() => Helper.SearchService.FullTextSearch(_user, lessThanMinimumSearchTermSearchCriteria),
                "POST {0} call shuold exit with 400 BadRequestException when using less than minimum length search term!", FULLTEXTSEARCH_PATH);

            var serviceErrorMessage = SerializationUtilities.DeserializeObject<ServiceErrorMessage>(ex.RestResponse.Content);

            // Validation: Exception should contain proper errorCode in the  response content.
            Assert.That(serviceErrorMessage.ErrorCode.Equals(ErrorCodes.IncorrectSearchCriteria),
                "POST {0} call with the searchCriteria less than minimum length search term should return {1} errorCode but {2} is returned",
                FULLTEXTSEARCH_PATH, ErrorCodes.IncorrectSearchCriteria, serviceErrorMessage.ErrorCode);
        }

        [TestCase("*")]
        [TestCase("&")]
        [TestRail(246562)]
        [Description("FullTextSearch using the invalid URL containing a special character. Verify that 400 bad request is returned.")]
        public void FullTextSearch_SendInvalidUrl_400BadRequest(string invalidCharacter)
        {
            // Setup:
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria("NonExistingSearchTerm", selectedProjectIds);
            string invalidPath = FULLTEXTSEARCH_PATH + invalidCharacter;

            var restApi = new RestApiFacade(Helper.ArtifactStore.Address, _user?.Token?.AccessControlToken);

            // Execute & Verify:
            var ex = Assert.Throws<Http400BadRequestException>(() => restApi.SendRequestAndDeserializeObject<FullTextSearchResult, FullTextSearchCriteria>(
                invalidPath,
                RestRequestMethod.POST,
                searchCriteria,
                shouldControlJsonChanges: false
                ),
                "POST {0} call should return a 400 Bad Request exception when trying with invalid URL.", FULLTEXTSEARCH_PATH);

            // Verify:
            string expectedMessage = I18NHelper.FormatInvariant("A potentially dangerous Request.Path value was detected from the client ({0}).", invalidCharacter);

            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, expectedMessage);
        }

        #endregion 400 Bad Request Tests

        #region 401 Unauthorized Tests

        [TestCase]
        [TestRail(190966)]
        [Description("Searching with missing 'Session-Token' header in the request.  Execute Search - Must return 401 Unautorized")]
        public void FullTextSearch_SearchWithMissingSessionTokenHeader_401Unauthorized()
        {
            // Setup: Create searchable artifact(s) with unique search term
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria("NonExistingSearchTerm", selectedProjectIds);

            // Execute: Execute FullTextSearch using the user with missing session token header
            var ex = Assert.Throws<Http401UnauthorizedException>(() => Helper.SearchService.FullTextSearch(user: null, searchCriteria: searchCriteria),
                "POST {0} call should return 401 Unauthorized if no Session-Token header was passed!", FULLTEXTSEARCH_PATH);

            // Validation: Exception should contain expected message.
            const string expectedExceptionMessage = "Token is missing or malformed";
            StringAssert.Contains(expectedExceptionMessage, ex.RestResponse.Content,
                "{0} was not found in returned message of Nova FullTextSearch which has no session token.", expectedExceptionMessage);
        }

        [TestCase]
        [TestRail(166163)]
        [Description("Searching with invalid 'Session-Token' header in the request. Execute Search - Must return 401 Unautorized")]
        public void FullTextSearch_SearchWithInvalidSessionToken_401Unauthorized()
        {
            // Setup: Create searchable artifact(s) with unique search term
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria("NonExistingSearchTerm", selectedProjectIds);
            var userWithBadToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            // Execute: Execute FullTextSearch using the user with invalid session token
            var ex = Assert.Throws<Http401UnauthorizedException>(() => Helper.SearchService.FullTextSearch(userWithBadToken, searchCriteria),
                "POST {0} call should exit with 401 UnauthorizedException when using invalid session!", FULLTEXTSEARCH_PATH);

            // Validation: Exception should contain expected message.
            const string expectedExceptionMessage = "Token is invalid";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "{0} was not found in returned message of Nova FullTextSearch which has invalid token.",
                expectedExceptionMessage);
        }

        #endregion 401 Unauthorized Tests

        #region Private Functions

        /// <summary>
        /// Create a stack of artifact descending ordered by Last Edited On datetime value
        /// </summary>
        /// <param name="artifactList">The list of artifact</param>
        private static Stack<ArtifactWrapper> CreateDescendingOrderedLastEditedOnArtifactStack(List<ArtifactWrapper> artifactList)
        {
            return new Stack<ArtifactWrapper>(artifactList.OrderBy(a =>
                Convert.ToDateTime(a.LastEditedOn.Value, CultureInfo.InvariantCulture)));
        }

        /// <summary>
        /// Creates the artifact list per page with artifact stack decending ordered by Edited On datetime value
        /// </summary>
        /// <param name="artifactStack">a stack of artifact descending ordered by Last Edited On datetime</param>
        /// <param name="pageSize">maximum number of artifacts that will be on the paged artifact list</param>
        private static List<ArtifactWrapper> CreateArtifactListPerPage (Stack<ArtifactWrapper> artifactStack, int pageSize)
        {
            var pagedArtifacts = new List<ArtifactWrapper>();

            for (int i = 0; i < pageSize; i++)
            {
                if (artifactStack.Any())
                {
                    pagedArtifacts.Add(artifactStack.Pop());
                }
            }
            return pagedArtifacts;
        }

        #endregion Private Functions

    }

    [TestFixture]
    [Category(Categories.SearchService)]
    public class FullTextSearchTests : FullTextSearchTestsBase
    {
        [SetUp]
        public void SetUp()
        {
            SetupData();
        }

        [TearDown]
        public void TearDown()
        {
            TearDownData();
        }

        [TestCase]
        [TestRail(182343)]
        [Description("Searching with the search criteria that matches with deleted and published artifacts.  Execute Search with the same user - " +
            "Must return SearchResult with empty list of FullTextSearchItems.")]
        public void FullTextSearch_SearchDeletedAndPublishedArtifact_VerifyWithSameUserEmptySearchResult()
        {
            // Setup: Delete all published artifacts
            foreach (var publishedArtifact in _publishedArtifacts)
            {
                publishedArtifact.Delete(_user);
                publishedArtifact.Publish(_user);
            }

            // Create search criteria with search term that matches with deleted and published artifact(s) description
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchTerm = StringUtilities.ConvertHtmlToText(_publishedArtifacts[0].Description);
            var searchCriteria = new FullTextSearchCriteria(searchTerm, selectedProjectIds);

            // Execute: Execute FullTextSearch with the search term using the same user
            FullTextSearchResult fullTextSearchResult = null;
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_user, searchCriteria),
                "POST {0} call failed when using search term {1} which matches with deleted and published artifacts!",
                FULLTEXTSEARCH_PATH, searchCriteria.Query);

            // Validation: Verify that searchResult contains empty list of FullTextSearchItems
            FullTextSearchResultValidation(fullTextSearchResult);
        }

        [TestCase]
        [TestRail(182371)]
        [Description("Searching with the search criteria that matches with deleted and published artifacts.  Execute Search with different user - " +
            "Must return SearchResult with empty list of FullTextSearchItems.")]
        public void FullTextSearch_SearchDeletedAndPublishedArtifact_VerifyWithDifferentUserEmptySearchResult()
        {
            // Setup: Delete all published artifacts
            foreach (var publishedArtifact in _publishedArtifacts)
            {
                publishedArtifact.Delete(_user);
                publishedArtifact.Publish(_user);
            }

            // Create search criteria with search term that matches with deleted and published artifact(s) description
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchTerm = StringUtilities.ConvertHtmlToText(_publishedArtifacts[0].Description);
            var searchCriteria = new FullTextSearchCriteria(searchTerm, selectedProjectIds);

            // Execute: Execute FullTextSearch with the search term using the different user
            FullTextSearchResult fullTextSearchResult = null;
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_userSecond, searchCriteria),
                "POST {0} call failed when using search term {1} which matches with deleted and published artifacts!",
                FULLTEXTSEARCH_PATH, searchCriteria.Query);

            // Validation: Verify that searchResult contains empty list of FullTextSearchItems
            FullTextSearchResultValidation(fullTextSearchResult);
        }

        [TestCase]
        [TestRail(182342)]
        [Description("Searching with the search criteria that matches with deleted but not published artifacts.  Execute Search with the same user - " +
            "Must return SearchResult with empty list of FullTextSearchItems.")]
        public void FullTextSearch_SearchDeletedNotPublishedArtifact_VerifyWithSameUserEmptySearchResult()
        {
            // Setup: Delete all published artifacts.
            foreach (var publishedArtifact in _publishedArtifacts)
            {
                publishedArtifact.Delete(_user);
            }

            var artifact = Helper.CreateAndPublishArtifact(_projects.First(), _user, BaseArtifactType.Actor);

            // Create search criteria with search term that matches the new published (not deleted) artifact.
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria(artifact.Name, selectedProjectIds);

            // Wait until the new published artifact appears in search results.
            SearchServiceTestHelper.WaitForFullTextSearchIndexerToUpdate(_user, Helper, searchCriteria, 1);

            // Execute: Execute FullTextSearch with the search term using the same user.
            // Create search criteria with search term that matches with deleted but not published artifact(s) description.
            var searchTerm = StringUtilities.ConvertHtmlToText(_publishedArtifacts[0].Description);
            searchCriteria.Query = searchTerm;
            FullTextSearchResult fullTextSearchResult = null;

            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_user, searchCriteria),
                "POST {0} call failed when using search term {1} which matches with deleted but not published artifacts!",
                FULLTEXTSEARCH_PATH, searchCriteria.Query);

            // Validation: Verify that searchResult contains empty list of FullTextSearchItems.
            FullTextSearchResultValidation(fullTextSearchResult);
        }

        [TestCase]
        [TestRail(182370)]
        [Description("Searching with the search criteria that matches with deleted but not published artifacts.  Execute Search - " +
            "Must return SearchResult with list of FullTextSearchItems.")]
        public void FullTextSearch_SearchDeletedNotPublishedArtifact_VerifyWithDifferentUserSearchResultIncludesItem()
        {
            // Setup: Delete all published artifacts.
            foreach (var publishedArtifact in _publishedArtifacts)
            {
                publishedArtifact.Delete(_user);
            }

            var artifact = Helper.CreateAndPublishNovaArtifact(_user, _projects[0], ItemTypePredefined.Actor);

            // Create search criteria with search term that matches with deleted but not published artifact(s) description.
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria(artifact.Name, selectedProjectIds);

            // Set the pageSize that can accomodate all expecting search results for the user.
            var customSearchPageSize = _publishedArtifacts.Count();

            // Wait until the new published artifact appears in search results.
            SearchServiceTestHelper.WaitForFullTextSearchIndexerToUpdate(_user, Helper, searchCriteria, 1);

            // Execute: Execute FullTextSearch with the search term using the different user.
            // Create search criteria with search term that matches with deleted but not published artifact(s) description.
            var searchTerm = StringUtilities.ConvertHtmlToText(_publishedArtifacts[0].Description);
            searchCriteria.Query = searchTerm;
            FullTextSearchResult fullTextSearchResult = null;

            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_userSecond, searchCriteria, pageSize: customSearchPageSize),
                "POST {0} call failed when using search term {1} which matches with deleted but not published artifacts!",
                FULLTEXTSEARCH_PATH, searchCriteria.Query);

            // Validation: Verify that searchResult contains list of FullTextSearchItems.
            FullTextSearchResultValidation(fullTextSearchResult, artifactsToBeFound: _publishedArtifacts, pageSize: customSearchPageSize);
        }
    }
}
