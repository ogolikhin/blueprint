﻿using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.Factories;
using Model.Impl;
using Model.SearchServiceModel.Impl;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
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

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _userSecond = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _projects = ProjectFactory.GetAllProjects(_user, shouldRetrievePropertyTypes: true);
            _publishedArtifacts = SearchServiceTestHelper.SetupFullTextSearchData(_projects, _user, Helper);
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            Helper?.Dispose();
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
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_user, searchCriteria: searchCriteria, page: page),
                "POST {0} call failed when using search term {1} with page parameter = {2}!", FULLTEXTSEARCH_PATH, searchCriteria.Query, page);

            // Validation: Verify that searchResult uses poptional page value and and DefaultPageSize
            FullTextSearchResultValidation(searchResult: fullTextSearchResult, page: page);
        }

        [TestCase(3)]
        [TestCase(72)]
        [TestRail(181023)]
        [Description("Searching with optional pageSize parameter. Execute Search - Must return SearchResult that uses the default page value (FirstPage) and the requested pageSize value.")]
        public void FullTextSearch_SearchWithPageSizeOnly_VerifySearchResult(int pageSize)
        {
            // Setup: Create searchable artifact(s) with unique search term
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria("NonExistingSearchTerm", selectedProjectIds);

            // Execute: Execute FullTextSearch with search term with pageSize parameter value
            FullTextSearchResult fullTextSearchResult = null;
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_user, searchCriteria: searchCriteria, pageSize: pageSize),
                "POST {0} call failed when using search term {1} with pageSize parameter = {2}!", FULLTEXTSEARCH_PATH, searchCriteria.Query, pageSize);

            // Validation: Verify that searchResult uses FirstPage and optional pageSize value
            FullTextSearchResultValidation(searchResult: fullTextSearchResult, pageSize: pageSize);
        }

        [TestCase(4, 5)]
        [TestCase(12, 38)]
        [TestRail(166156)]
        [Description("Searching with both optional page and pageSize parameters. Execute Search - Must return SearchResult that uses requested page and pageSize values.")]
        public void FullTextSearch_SearchWithBothPageAndPageSize_VerifySearchResult(int page, int pageSize)
        {
            // Setup: Create searchable artifact(s) with unique search term
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria("NonExistingSearchTerm", selectedProjectIds);

            // Execute: Execute FullTextSearch with search term with both page and pageSize parameter values
            FullTextSearchResult fullTextSearchResult = null;
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_user, searchCriteria: searchCriteria, page: page, pageSize: pageSize),
                "POST {0} call failed when using search term {1} with page paramter = {2} and pageSize parameter = {3}!", FULLTEXTSEARCH_PATH, searchCriteria.Query, page, pageSize);

            // Validation: Verify that searchResult uses optional page and pageSize values
            FullTextSearchResultValidation(searchResult: fullTextSearchResult, page: page, pageSize: pageSize);
        }

        [TestCase]
        [TestRail(166155)]
        [Description("Searching without both optional parameters, (page and pagesize). Execute Search - Must return SearchResult that uses default page value (FirstPage) and default pageSize values.")]
        public void FullTextSearch_SearchWithoutBothPageAndPageSize_VerifySearchResultUsesFirstPageAndDefaultPageSize()
        {
            // Setup: Create searchable artifact(s) with unique search term
            var projectIds = new List<int>() { 2, 4 };
            var searchCriteria = new FullTextSearchCriteria("NonExistingSearchTerm", projectIds);

            // Execute: Execute FullTextSearch with search term
            FullTextSearchResult fullTextSearchResult = null;
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_user, searchCriteria: searchCriteria),
                "POST {0} call failed when using search term {1}!", FULLTEXTSEARCH_PATH, searchCriteria.Query);

            // Validation: Verify that searchResult uses default page value (FirstPage) and the Default pageSize value
            FullTextSearchResultValidation(searchResult: fullTextSearchResult, page: DEFAULT_PAGE_VALUE);
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
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_user, searchCriteria: searchCriteria, page: page),
                "POST {0} call failed when using search term {1} with invalid page paramter = {2}!", FULLTEXTSEARCH_PATH, searchCriteria.Query, page);

            // Validation: Verify that searchResult uses default page value (FirstPage)
            FullTextSearchResultValidation(searchResult: fullTextSearchResult, page: DEFAULT_PAGE_VALUE);
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
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_user, searchCriteria: searchCriteria, pageSize: pageSize),
                "POST {0} call failed when using search term {1} with invalid pageSize parameter = {2}!", FULLTEXTSEARCH_PATH, searchCriteria.Query, pageSize);

            // Validation: Verify that searchResult uses default PageSize value
            FullTextSearchResultValidation(searchResult: fullTextSearchResult, pageSize: DEFAULT_PAGESIZE_VALUE);
        }

        [TestCase(-12, -100)]
        [TestCase(int.MinValue, int.MinValue)]
        [TestRail(181021)]
        [Description("Searching with both invalid page and pageSize values. Execute Search - Must return SearchResult that uses default page value (FirstPage) and default pageSize.")]
        public void FullTextSearch_SearchWithInvalidPageAndPageSize_VerifySearchResultUsesFirstPageAndDefaultPageSize(int page, int pageSize)
        {
            // Setup: Create searchable artifact(s) with unique search term
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria("NonExistingSearchTerm", selectedProjectIds);

            // Execute: Execute FullTextSearch with search term with invalid page and invalid pageSize parameter values
            FullTextSearchResult fullTextSearchResult = null;
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_user, searchCriteria: searchCriteria, page: page, pageSize: pageSize),
                "POST {0} call failed when using search term {1} with invalid page parameter = {2} and invalid pageSize parameter = {3}!", FULLTEXTSEARCH_PATH, searchCriteria.Query, page, pageSize);

            // Validation: Verify that searchResult uses default page value (FirstPage) and default PageSize value
            FullTextSearchResultValidation(searchResult: fullTextSearchResult, page: DEFAULT_PAGE_VALUE, pageSize: DEFAULT_PAGESIZE_VALUE);
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(182340)]
        [Ignore(IgnoreReasons.UnderDevelopment)]
        [Description("Searching with the search criteria that matches with saved artifacts. Execute Search with the same user - Must return SearchResult with empty list of FullTextSearchItems.")]
        public void FullTextSearch_SearchSavedNotPublishedArtifact_VerifyWithSameUserEmptySearchResult(BaseArtifactType baseArtifactType)
        {
            // Setup: Create and save single artifact for each project
            List<IArtifactBase> savedOnlyArtifacts = new List<IArtifactBase>();
            _projects.ForEach(project => savedOnlyArtifacts.Add(Helper.CreateAndSaveArtifact(project, _user, baseArtifactType)));

            // Create search criteria with search term that matches with saved artifacts on each project.
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria(savedOnlyArtifacts.First().Name, selectedProjectIds);

            // Execute: Execute FullTextSearch with the search term using the same user
            FullTextSearchResult fullTextSearchResult = null;
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_user, searchCriteria: searchCriteria),
                "POST {0} call failed when using search term {1} which matches with saved-only artifacts!", FULLTEXTSEARCH_PATH, searchCriteria.Query);

            // Validation: Verify that searchResult contains empty list of FullTextSearchItems 
            FullTextSearchResultValidation(searchResult: fullTextSearchResult);
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(182369)]
        [Ignore(IgnoreReasons.UnderDevelopment)]
        [Description("Searching with the search criteria that matches with saved artifacts. Execute Search with different user - Must return SearchResult with empty list of FullTextSearchItems.")]
        public void FullTextSearch_SearchSavedNotPublishedArtifact_VerifyWithDifferentUserEmptySearchResult(BaseArtifactType baseArtifactType)
        {
            // Setup: Create and save single artifact for each project
            List<IArtifactBase> savedOnlyArtifacts = new List<IArtifactBase>();
            _projects.ForEach(project => savedOnlyArtifacts.Add(Helper.CreateAndSaveArtifact(project, _user, baseArtifactType)));

            // Create search criteria with search term that matches with saved artifacts on each project.
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria(savedOnlyArtifacts.First().Name, selectedProjectIds);

            // Execute: Execute FullTextSearch with search term using the different user
            FullTextSearchResult fullTextSearchResult = null;
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_userSecond, searchCriteria: searchCriteria),
                "POST {0} call failed when using search term {1} which matches with saved-only artifacts!", FULLTEXTSEARCH_PATH, searchCriteria.Query);

            // Validation: Verify that searchResult contains empty list of FullTextSearchItems 
            FullTextSearchResultValidation(searchResult: fullTextSearchResult);
        }

        [TestCase]
        [TestRail(182341)]
        [Description("Searching with the search criteria that matches with published artifacts. Execute Search - Must return SearchResult with list of FullTextSearchItems.")]
        public void FullTextSearch_SearchPublishedArtifact_VerifySearchResultIncludesItem()
        {
            // Setup: Create search criteria with search term that matches with description value of all published artifacts
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchTerm = WebUtility.HtmlDecode(_publishedArtifacts.First().Properties.Find(p => p.Name.Equals("Description")).TextOrChoiceValue);
            var searchCriteria = new FullTextSearchCriteria(searchTerm, selectedProjectIds);

            // Set the pageSize that can accomodate all expecting search results for the user
            var customSearchPageSize = _publishedArtifacts.Count();

            // Execute: Execute FullTextSearch with search term that matches published artifact(s) description
            FullTextSearchResult fullTextSearchResult = null;
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_user, searchCriteria: searchCriteria, pageSize: customSearchPageSize),
                "POST {0} call failed when using search term {1} which matches with published artifacts!", FULLTEXTSEARCH_PATH, searchCriteria.Query);

            // Validation: Verify that searchResult contains all published artifacts
            FullTextSearchResultValidation(searchResult: fullTextSearchResult, artifactsToBeFound: _publishedArtifacts, pageSize: customSearchPageSize);
        }

        [TestCase]
        [TestRail(182342)]
        [Ignore(IgnoreReasons.UnderDevelopment)]
        [Description("Searching with the search criteria that matches with deleted but not published artifacts. Execute Search with the same user - Must return SearchResult with empty list of FullTextSearchItems.")]
        public void FullTextSearch_SearchDeletedNotPublishedArtifact_VerifyWithSameUserEmptySearchResult()
        {
            // Setup: Delete all published artifacts
            foreach (var publishedArtifact in _publishedArtifacts)
            {
                publishedArtifact.Delete(_user);
            }

            // Create search criteria with search term that matches with deleted but not published artifact(s) description
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchTerm = WebUtility.HtmlDecode(_publishedArtifacts.First().Properties.Find(p => p.Name.Equals("Description")).TextOrChoiceValue);
            var searchCriteria = new FullTextSearchCriteria(searchTerm, selectedProjectIds);

            // Execute: Execute FullTextSearch with the search term using the same user
            FullTextSearchResult fullTextSearchResult = null;
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_user, searchCriteria: searchCriteria),
                "POST {0} call failed when using search term {1} which matches with deleted but not published artifacts!", FULLTEXTSEARCH_PATH, searchCriteria.Query);

            // Validation: Verify that searchResult contains empty list of FullTextSearchItems
            FullTextSearchResultValidation(searchResult: fullTextSearchResult);
        }

        [TestCase]
        [TestRail(182370)]
        [Ignore(IgnoreReasons.UnderDevelopment)]
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
            var searchTerm = WebUtility.HtmlDecode(_publishedArtifacts.First().Properties.Find(p => p.Name.Equals("Description")).TextOrChoiceValue);
            var searchCriteria = new FullTextSearchCriteria(searchTerm, selectedProjectIds);

            // Set the pageSize that can accomodate all expecting search results for the user
            var customSearchPageSize = _publishedArtifacts.Count();

            // Execute: Execute FullTextSearch with the search term using the different user
            FullTextSearchResult fullTextSearchResult = null;
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_userSecond, searchCriteria: searchCriteria),
                "POST {0} call failed when using search term {1} which matches with deleted but not published artifacts!", FULLTEXTSEARCH_PATH, searchCriteria.Query);

            // Validation: Verify that searchResult contains list of FullTextSearchItems
            FullTextSearchResultValidation(searchResult: fullTextSearchResult, artifactsToBeFound: _publishedArtifacts, pageSize: customSearchPageSize);
        }

        [TestCase]
        [TestRail(182343)]
        [Ignore(IgnoreReasons.UnderDevelopment)]
        [Description("Searching with the search criteria that matches with deleted and published artifacts. Execute Search with the same user - Must return SearchResult with empty list of FullTextSearchItems.")]
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
            var searchTerm = WebUtility.HtmlDecode(_publishedArtifacts.First().Properties.Find(p => p.Name.Equals("Description")).TextOrChoiceValue);
            var searchCriteria = new FullTextSearchCriteria(searchTerm, selectedProjectIds);

            // Execute: Execute FullTextSearch with the search term using the same user
            FullTextSearchResult fullTextSearchResult = null;
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_user, searchCriteria: searchCriteria),
                "POST {0} call failed when using search term {1} which matches with deleted and published artifacts!", FULLTEXTSEARCH_PATH, searchCriteria.Query);

            // Validation: Verify that searchResult contains empty list of FullTextSearchItems
            FullTextSearchResultValidation(searchResult: fullTextSearchResult);
        }

        [TestCase]
        [TestRail(182371)]
        [Ignore(IgnoreReasons.UnderDevelopment)]
        [Description("Searching with the search criteria that matches with deleted and published artifacts. Execute Search with different user - Must return SearchResult with empty list of FullTextSearchItems.")]
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
            var searchTerm = WebUtility.HtmlDecode(_publishedArtifacts.First().Properties.Find(p => p.Name.Equals("Description")).TextOrChoiceValue);
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
        [Ignore(IgnoreReasons.UnderDevelopment)]
        [Description("Searching with the search criteria that matches with published artifacts using user doesn't have permission to the project. Execute Search - Must return SearchResult with empty list of FullTextSearchItems.")]
        public void FullTextSearch_SearchWithoutPermissionOnProjects_VerifyEmptyFullTextSearchItemsOnSearchResult()
        {
            // Setup: Create search criteria with search term that matches published artifact(s) name
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria(_publishedArtifacts.First().Properties.Find(p => p.Name.Equals("Name")).TextOrChoiceValue, selectedProjectIds);

            // Create user with no permission on any project
            var userWithNoPermissionOnAnyProject = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.None, projects: _projects);

            // Execute: Execute FullTextSearch with search term that matches published artifact(s) name
            FullTextSearchResult fullTextSearchResult = null;
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(userWithNoPermissionOnAnyProject, searchCriteria: searchCriteria),
                "POST {0} call failed when using search term {1} which matches with published artifacts!", FULLTEXTSEARCH_PATH, searchCriteria.Query);

            // Validation: Verify that searchResult contains empty list of FullTextSearchItems
            FullTextSearchResultValidation(searchResult: fullTextSearchResult);
        }

        [TestCase(1, TestHelper.ProjectRole.Viewer)]
        [TestCase(1, TestHelper.ProjectRole.AuthorFullAccess)]
        [TestCase(1, TestHelper.ProjectRole.None)]
        [TestRail(182374)]
        [Description("Searching with the search criteria that matches with published artifacts using user have permission to certain project(s). Execute Search - Must return corresponding SearchResult based on user's permission per project")]
        public void FullTextSearch_SearchWithPermissionOnProjects_VerifyCorrespondingFullTextSearchItemsOnSearchResult(
            int numberOfProjectForPermissionTesting, 
            TestHelper.ProjectRole projectRole)
        {
            // Setup: Create search criteria with search term that matches published artifact(s) description
            var searchProjectIds = _projects.ConvertAll(project => project.Id);
            var searchTerm = WebUtility.HtmlDecode(_publishedArtifacts.First().Properties.Find(p => p.Name.Equals("Description")).TextOrChoiceValue);
            var searchCriteria = new FullTextSearchCriteria(searchTerm, projectIds: searchProjectIds);

            // Calculate expecting values for the selected project(s) and published artifacts for the project(s)
            List<IProject> selectedProjects = new List<IProject>();
            selectedProjects.AddRange(_projects.Take(numberOfProjectForPermissionTesting));
            List<IArtifactBase> publishedArtifactsForSelectedProjects = new List<IArtifactBase>();
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
            var userWithSelectiveProjectPermission = Helper.CreateUserWithProjectRolePermissions(projectRole, projects: selectedProjects);
            FullTextSearchResult fullTextSearchResult = null;
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(userWithSelectiveProjectPermission, searchCriteria: searchCriteria, pageSize: customSearchPageSize),
                "POST {0} call failed when using search term {1} which matches with published artifacts!", FULLTEXTSEARCH_PATH, searchCriteria.Query);

            // Validation: Verify that searchResult contains list of FullTextSearchItems depending on permission for project(s)
            FullTextSearchResultValidation(searchResult: fullTextSearchResult, artifactsToBeFound: publishedArtifactsForSelectedProjects, pageSize: customSearchPageSize);
        }

        [TestCase]
        [TestRail(182344)]
        [Description("Searching with the search criteria that matches with older version of published artifacts. Execute Search - Must return SearchResult with empty list of FullTextSearchItems.")]
        public void FullTextSearch_SearchOlderVersionArtifact_VerifyEmptySearchResults()
        {
            // Setup: Create few artifacts to search
            List<BaseArtifactType> selectedBasedArtifactTypes = new List<BaseArtifactType> { BaseArtifactType.Actor };
            var publishedArtifacts = SearchServiceTestHelper.SetupFullTextSearchData(_projects, _user, Helper, selectedBaseArtifactTypes: selectedBasedArtifactTypes);

            // Create search criteria with search term that matches with current version of artifact(s) description
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchTerm = WebUtility.HtmlDecode(publishedArtifacts.First().Properties.Find(p => p.Name.Equals("Description")).TextOrChoiceValue);
            var searchCriteria = new FullTextSearchCriteria(searchTerm, selectedProjectIds);

            // Create search criteria with search term that matches with new version of artifact(s) description
            var newSearchTerm = "NewDescription";
            var newSearchCriteria = new FullTextSearchCriteria(newSearchTerm, selectedProjectIds);

            // Update all published artifacts with the new description value that matches with the new search criteria
            foreach (var publishedArtifact in publishedArtifacts.ConvertAll(o => (IArtifact)o))
            {
                publishedArtifact.Lock(_user);
                SearchServiceTestHelper.UpdateArtifactProperty(
                    Helper, _user, _projects.Find(p=>p.Id.Equals(publishedArtifact.ProjectId)), publishedArtifact, publishedArtifact.BaseArtifactType, "Description", newSearchTerm);
                publishedArtifact.Publish(_user);
            }

            // Wait until user can see the artifact in the search results or timeout occurs
            SearchServiceTestHelper.WaitForFullTextSearchIndexerToUpdate(_user, Helper, newSearchCriteria, publishedArtifacts.Count());

            // Execute: Execute FullTextSearch with the old search term
            FullTextSearchResult fullTextSearchResult = null;
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_userSecond, searchCriteria),
                "POST {0} call failed when using search term {1} which matches with older version of artifacts!", FULLTEXTSEARCH_PATH, searchCriteria.Query);

            // Validation: Verify that searchResult contains empty list of FullTextSearchItems
            FullTextSearchResultValidation(searchResult: fullTextSearchResult);
        }

        [TestCase(2)]
        [TestCase(6)]
        [TestRail(182447)]
        [Description("Searching with the search criteria that returns multiple pages for SearchResult. Execute Search - Verify that number of result items matches with expecting search result items.")]
        public void FullTextSearch_SearchWithSearchTermReturnsMultiplePages_VerifyResultItemCountWithExpected(int pageSize)
        {
            // Setup: Create search criteria that will return multiple page for SearchResult
            var searchTerm = WebUtility.HtmlDecode(_publishedArtifacts.First().Properties.Find(p => p.Name.Equals("Description")).TextOrChoiceValue);
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
                Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_user, searchCriteria: searchCriteria, page: pageCount, pageSize: pageSize),
                    "POST {0} call failed when using search term {1}, page: {2}, pageSize: {3}", FULLTEXTSEARCH_PATH, searchCriteria.Query, pageCount, pageSize);

                // Adds search result per page into total returned search count
                returnedSearchCount += fullTextSearchResult.Items.Count();

                // Create a artifact list per page, decending ordered by Last Edited On
                List<IArtifactBase> pagedArtifacts = CreateArtifactListPerPage(expectedArtifactsStackDescOrderedByLastEditedOn, pageSize);

                // Validation: Verify that searchResult contains list of FullTextSearchItems
                FullTextSearchResultValidation(searchResult: fullTextSearchResult, artifactsToBeFound: pagedArtifacts, page: pageCount, pageSize: pageSize);

                pageCount++;
            }

            // Validation: Verify that expected search count is equal to returned search count
            Assert.That(returnedSearchCount.Equals(expectedSearchResultCount),"Expected search result count is {0} but {1} was returned", expectedSearchResultCount, returnedSearchCount);
        }

        [TestCase]
        [TestRail(182448)]
        [Description("Searching with the search criteria. Execute Search - Verify that result are sorted by Last Edited On")]
        public void FullTextSearch_SearchWithCriteria_VerifyFullTextSearchItemsAreSortedByLastEditedOn()
        {
            // Setup: Create search criteria with search term that matches published artifact(s) description
            FullTextSearchResult fullTextSearchResult = null;
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchTerm = WebUtility.HtmlDecode(_publishedArtifacts.First().Properties.Find(p => p.Name.Equals("Description")).TextOrChoiceValue);
            var searchCriteria = new FullTextSearchCriteria(searchTerm, selectedProjectIds);

            // Set the pageSize that displays all expecting search results for the user with permission on selected project(s)
            var customSearchPageSize = _publishedArtifacts.Count();

            // Execute: Execute FullTextSearch with search term that matches published artifact(s) description
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.SearchService.FullTextSearch(_user, searchCriteria, pageSize: customSearchPageSize),
                "POST {0} call failed when using search term {1} which matches with published artifacts!", FULLTEXTSEARCH_PATH, searchCriteria.Query);

            // Validation: Verify that searchResult contains published artifacts
            FullTextSearchResultValidation(searchResult: fullTextSearchResult, artifactsToBeFound: _publishedArtifacts, pageSize: customSearchPageSize);

            // Create an expected artifact list, decending ordered by Last Edited On
            var expectedArtifactsStackDescOrderedByLastEditedOn = CreateDescendingOrderedLastEditedOnArtifactStack(_publishedArtifacts);
            List<IArtifactBase> LastEditedOnOrderedArtifacts = CreateArtifactListPerPage(expectedArtifactsStackDescOrderedByLastEditedOn, customSearchPageSize);

            // Compare returned FullTextSearchItems from search result with the expected ordered artifacts
            for (int i = 0; i < fullTextSearchResult.Items.Count(); i++ )
            {
                Assert.That(fullTextSearchResult.Items.Cast<FullTextSearchItem>().ToList()[i].ArtifactId.
                    Equals(LastEditedOnOrderedArtifacts[i].Id),
                    "artfiact with ID {0} was expected from the returned FullTextSearchItems but artifact with ID {1} is found on data row {2}.",
                    LastEditedOnOrderedArtifacts[i].Id, fullTextSearchResult.Items.Cast<FullTextSearchItem>().ToList()[i].ArtifactId, i);
            }
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
            var ex = Assert.Throws<Http400BadRequestException>(() => Helper.SearchService.FullTextSearch(_user, searchCriteria: invalidSearchCriteria),
                "POST {0} call should exit with 400 BadRequestException when using invalid search criteria!", FULLTEXTSEARCH_PATH);

            var serviceErrorMessage = Deserialization.DeserializeObject<ServiceErrorMessage>(ex.RestResponse.Content);

            // Validation: Exception should contain proper errorCode in the response content.
            Assert.That(serviceErrorMessage.ErrorCode.Equals(ErrorCodes.IncorrectSearchCriteria),
                "POST {0} call with invalid searchCriteria should return {1} errorCode but {2} is returned", FULLTEXTSEARCH_PATH, ErrorCodes.IncorrectSearchCriteria, serviceErrorMessage.ErrorCode);
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

            var serviceErrorMessage = Deserialization.DeserializeObject<ServiceErrorMessage>(ex.RestResponse.Content);

            // Validation: Exception should contain proper errorCode in the  response content.
            Assert.That(serviceErrorMessage.ErrorCode.Equals(ErrorCodes.IncorrectSearchCriteria),
                "POST {0} call with the searchCriteria less than minimum length search term should return {1} errorCode but {2} is returned", FULLTEXTSEARCH_PATH, ErrorCodes.IncorrectSearchCriteria, serviceErrorMessage.ErrorCode);
        }

        [TestCase]
        [TestRail(190966)]
        [Description("Searching with empty 'Session-Token' header in the request. Execute Search - Must return 401 Unautorized")]
        public void FullTextSearch_SearchWithEmptySessionToken_400BadRequest()
        {
            // Setup: Create searchable artifact(s) with unique search term
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria("NonExistingSearchTerm", selectedProjectIds);

            // Execute: Execute FullTextSearch using the user with empty session token
            var ex = Assert.Throws<Http400BadRequestException>(() => Helper.SearchService.FullTextSearch(user: null, searchCriteria: searchCriteria),
                "POST {0} call should exit with 400 BadRequestException when using empty session!", FULLTEXTSEARCH_PATH);

            // Validation: Exception should contain expected message.
            const string expectedExceptionMessage = "Token is missing or malformed";
            StringAssert.Contains(expectedExceptionMessage, ex.RestResponse.Content, "{0} was not found in returned message of Nova FullTextSearch which has no session token.", expectedExceptionMessage);
        }

        #endregion 400 Bad Request Tests

        #region 401 Unauthorized Tests

        [TestCase]
        [TestRail(166163)]
        [Description("Searching with invalid 'Session-Token' header in the request. Execute Search - Must return 401 Unautorized")]
        public void FullTextSearch_SearchWithInvalidSessionToken_401Unauthorized()
        {
            // Setup: Create searchable artifact(s) with unique search term
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria("NonExistingSearchTerm", selectedProjectIds);
            IUser userWithBadToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            // Execute: Execute FullTextSearch using the user with invalid session token
            var ex = Assert.Throws<Http401UnauthorizedException>(() => Helper.SearchService.FullTextSearch(userWithBadToken, searchCriteria),
                "POST {0} call should exit with 401 UnauthorizedException when using invalid session!", FULLTEXTSEARCH_PATH);

            // Validation: Exception should contain expected message.
            const string expectedExceptionMessage = "Token is invalid";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage), "{0} was not found in returned message of Nova FullTextSearch which has invalid token.", expectedExceptionMessage);
        }

        #endregion 401 Unauthorized Tests

        #region 403 Forbidden Tests
        #endregion 403 Forbidden Tests

        #region 404 Not Found Tests
        #endregion 404 Not Found Tests

        #region 409 Conflict Tests
        #endregion 409 Conflict Tests

        #region Private Functions

        /// <summary>
        /// Create a stack of artifact descending ordered by Last Edited On datetime value
        /// </summary>
        /// <param name="artifactList">The list of artifact</param>
        private static Stack<IArtifactBase> CreateDescendingOrderedLastEditedOnArtifactStack(List<IArtifactBase> artifactList)
        {
            return new Stack<IArtifactBase>(artifactList.OrderBy(a => Convert.ToDateTime(a.Properties.Find(p => p.Name.Equals("Last Edited On")).DateValue, CultureInfo.InvariantCulture)));
        }

        /// <summary>
        /// Creates the artifact list per page with artifact stack decending ordered by Edited On datetime value
        /// </summary>
        /// <param name="artifactStack">a stack of artifact descending ordered by Last Edited On datetime</param>
        /// <param name="pageSize">maximum number of artifacts that will be on the paged artifact list</param>
        private static List<IArtifactBase> CreateArtifactListPerPage (Stack<IArtifactBase> artifactStack, int pageSize)
        {
            List<IArtifactBase> pagedArtifacts = new List<IArtifactBase>();
            for (int i = 0; i < pageSize; i++)
            {
                if (artifactStack.Any())
                {
                    pagedArtifacts.Add(artifactStack.Pop());
                }
            }
            return pagedArtifacts;
        }

        /// <summary>
        /// Asserts that returned searchResult from the FullTextSearch call match with artifacts that are being searched.
        /// </summary>
        /// <param name="searchResult">The searchResult from Nova search call.</param>
        /// <param name="artifactsToBeFound"> (optional) artifacts that are being searched, if this parameter is empty, searched items validation step gets skipped.</param>
        /// <param name="page"> (optional) page value that represents displaying page number of the rearch result</param>
        /// <param name="pageSize"> (optional) pageSize value that indicates number of items that get displayed per page</param>
        private static void FullTextSearchResultValidation( FullTextSearchResult searchResult, List<IArtifactBase> artifactsToBeFound = null, int? page = null, int? pageSize = null)
        {
            ThrowIf.ArgumentNull(searchResult, nameof(searchResult));

            //Setup: Set comparison values
            artifactsToBeFound = artifactsToBeFound ?? new List<IArtifactBase>();
            page = page ?? DEFAULT_PAGE_VALUE;
            pageSize = pageSize ?? DEFAULT_PAGESIZE_VALUE;

            page = page.Equals(0) ? DEFAULT_PAGE_VALUE : page;
            pageSize = pageSize.Equals(0) ? DEFAULT_PAGESIZE_VALUE : pageSize;
            
            List<int> ReturnedFullTextSearchItemArtifactIds = new List<int>();

            if (artifactsToBeFound.Any())
            {
                searchResult.Items.Cast<FullTextSearchItem>().ToList().ForEach(a => ReturnedFullTextSearchItemArtifactIds.Add(a.ArtifactId));

                for (int i = 0; i < Math.Min(artifactsToBeFound.Count, (int)pageSize); i++)
                {
                    Assert.That(ReturnedFullTextSearchItemArtifactIds.Contains(artifactsToBeFound[i].Id), "The expected artifact whose Id is {0} does not exist on the response from the Nova FullTextSearch call.", artifactsToBeFound[i].Id);
                }
            } else
            {
                Assert.That(searchResult.Items.Count().Equals(0), "The FullTextSearchItems should be null list when expected return result is empty but the response from the Nova FullTextSearch call returns {0} results", searchResult.Items.Count());
            }

            // Validation: Verify that searchResult uses page value passed as optional parameter and DefaultPageSize
            Assert.That(searchResult.Page.Equals(page), "The expected default page value is {0} but {1} was found from the returned searchResult.", page, searchResult.Page);
            Assert.That(searchResult.PageSize.Equals(pageSize), "The expected default pagesize value is {0} but {1} was found from the returned searchResult.", pageSize, searchResult.PageSize);
        }
        
        #endregion Private Functions

    }
}
