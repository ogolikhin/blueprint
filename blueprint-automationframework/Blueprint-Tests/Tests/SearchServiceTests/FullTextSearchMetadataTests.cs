using System;
using System.Collections.Generic;
using CustomAttributes;
using Helper;
using Model;
using NUnit.Framework;
using System.Linq;
using Model.ArtifactModel;
using Model.Factories;
using Model.SearchServiceModel.Impl;
using TestCommon;
using Utilities;

namespace SearchServiceTests
{
    [TestFixture]
    [Category(Categories.SearchService)]
    public class FullTextSearchMetadataTests : TestBase
    {
        private static IUser _user;
        private static IUser _user2;
        private static List<IProject> _projects;
        private static List<IArtifactBase> _artifacts;

        const int DEFAULT_PAGE_SIZE_VALUE = 10;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _user2 = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _projects = ProjectFactory.GetAllProjects(_user, true);
            _artifacts = SearchServiceTestHelper.SetupFullTextSearchData(_projects, _user, Helper);
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            Helper?.Dispose();
        }

        #region 200 OK Tests

        [TestCase]
        [TestRail(182247)]
        [Description("Search without optional parameter pagesize. Executed search must return search metadata result that indicates default page size.")]
        public void FullTextSearchMetadata_SearchMetadataWithoutPageSize_VerifySearchMetadataResultUsesDefaultPageSize()
        {
            FullTextSearchMetaDataResult fullTextSearchMetaDataResult = null;

            // Setup: 
            var searchTerm = _artifacts.First().Name;
            var searchCriteria = new FullTextSearchCriteria(searchTerm, _projects.Select(p => p.Id));

            // Execute: Execute FullTextSearch with search term
            Assert.DoesNotThrow(() => fullTextSearchMetaDataResult =
                Helper.SearchService.FullTextSearchMetaData(_user, searchCriteria),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // Validation:
            ValidateSearchMetadataTest(fullTextSearchMetaDataResult, searchCriteria, "Name");
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(100)]
        [TestRail(182245)]
        [Description("Search with optional parameter pagesize. Executed search must return search metadata result that indicates requested page size")]
        public void FullTextSearchMetadata_SearchMetadataWithValidPageSize_VerifySearchMetadataResult(int requestedPageSize)
        {
            FullTextSearchMetaDataResult fullTextSearchMetaDataResult = null;

            // Setup: 
            var searchTerm = _artifacts.First().Name;
            var searchCriteria = new FullTextSearchCriteria(searchTerm, _projects.Select(p => p.Id));

            // Execute: Execute FullTextSearch with search terms
            Assert.DoesNotThrow(() => fullTextSearchMetaDataResult =
                Helper.SearchService.FullTextSearchMetaData(_user, searchCriteria, requestedPageSize),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // Validation:
            ValidateSearchMetadataTest(fullTextSearchMetaDataResult, searchCriteria, "Name", requestedPageSize);
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestRail(182248)]
        [Description("Search with invalid pagesize. Executed search must return search metadata result that indicates default page size.")]
        public void FullTextSearchMetadata_SearchMetadataWithInvalidPageSize_VerifySearchMetadataResultUsesDefaultPageSize(int requestedPageSize)
        {
            FullTextSearchMetaDataResult fullTextSearchMetaDataResult = null;

            // Setup: 
            var searchTerm = _artifacts.First().Name;
            var searchCriteria = new FullTextSearchCriteria(searchTerm, _projects.Select(p => p.Id));

            // Execute: Execute FullTextSearch with search term
            Assert.DoesNotThrow(() => fullTextSearchMetaDataResult =
                Helper.SearchService.FullTextSearchMetaData(_user, searchCriteria, requestedPageSize),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // Validation:
            ValidateSearchMetadataTest(fullTextSearchMetaDataResult, searchCriteria, "Name", requestedPageSize);
        }

        [TestCase]
        [TestRail(182252)]
        [Description("Search a single project where the search term is valid across multiple projects. Executed search must return" +
                     " search metadata result that indicates only the single project was searched.")]
        public void FullTextSearchMetadata_SearchMetadataFromSingleProject_VerifySearchMetadataResultIncludesOnlyProjectsSpecified()
        {
            FullTextSearchMetaDataResult fullTextSearchMetaDataResultForSingleProject = null;

            // Setup: 
            var searchTerm = _artifacts.First().Name;

            // Seaarch only a single project from the list of projects.
            var searchCriteria = new FullTextSearchCriteria(searchTerm, new List<int>{_projects.First().Id});

            // Execute: Execute FullTextSearch with search term
            Assert.DoesNotThrow(() => fullTextSearchMetaDataResultForSingleProject =
                Helper.SearchService.FullTextSearchMetaData(_user, searchCriteria),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // Validation:
            ValidateSearchMetadataTest(fullTextSearchMetaDataResultForSingleProject, searchCriteria, "Name");

            // Search all projects from the list of projects and compare to result for single project.
            searchCriteria = new FullTextSearchCriteria(searchTerm, _projects.Select(p => p.Id));

            FullTextSearchMetaDataResult fullTextSearchMetaDataResultForMultipleProjects = null;

            Assert.DoesNotThrow(() => fullTextSearchMetaDataResultForMultipleProjects =
                Helper.SearchService.FullTextSearchMetaData(_user, searchCriteria),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            Assert.That(!fullTextSearchMetaDataResultForMultipleProjects.TotalCount.Equals(fullTextSearchMetaDataResultForSingleProject.TotalCount), 
                "The search hit count for multiple projects was the same as for a single project but should be different.");
        }

        [TestCase(-1)]
        [TestRail(182363)]
        [Description("Search with one valid project and one invalid project. Executed search must return valid data for the valid project and" +
                     " ignore the invalid project.")]
        public void FullTextSearchMetadata_SearchMetadataWithOneValidAndOneInvalidProject_VerifySearchMetadataResultIgnoresInvalidProject(int invalidProjectId)
        {
            // Setup: 
            FullTextSearchMetaDataResult fullTextSearchMetaDataResult = null;

            var searchTerm = _artifacts.First().Name;
            var projectIds = new List<int>
            {
                _projects.First().Id,
                invalidProjectId
            };

            var searchCriteria = new FullTextSearchCriteria(searchTerm, projectIds);

            // Execute: Execute FullTextSearch with search term
            Assert.DoesNotThrow(() => fullTextSearchMetaDataResult =
                Helper.SearchService.FullTextSearchMetaData(_user, searchCriteria),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // Validation:
            ValidateSearchMetadataTest(fullTextSearchMetaDataResult, searchCriteria, "Name");
        }

        [TestCase(new [] {BaseArtifactType.Actor})]
        [TestCase(new[] { BaseArtifactType.Actor, BaseArtifactType.Document, BaseArtifactType.Process })]
        [TestCase(new[] {
            BaseArtifactType.Actor,
            BaseArtifactType.Document,
            BaseArtifactType.Process,
            BaseArtifactType.DomainDiagram,
            BaseArtifactType.BusinessProcess,
            BaseArtifactType.GenericDiagram,
            BaseArtifactType.Glossary,
            BaseArtifactType.PrimitiveFolder, 
            BaseArtifactType.Storyboard,
            BaseArtifactType.TextualRequirement,
            BaseArtifactType.UIMockup,
            BaseArtifactType.UseCase,
            BaseArtifactType.UseCaseDiagram })]
        [TestRail(182253)]
        [Description("Search over specific artifact types. Executed search must return search metadata result that match only the artifact .")]
        public void FullTextSearchMetadata_SearchMetadataForSpecificItemTypes_VerifySearchMetadataResultIncludesOnlyTypesSpecified(BaseArtifactType[] baseArtifactTypes)
        {
            ThrowIf.ArgumentNull(baseArtifactTypes, nameof(baseArtifactTypes));

            FullTextSearchMetaDataResult fullTextSearchMetaDataResult = null;

            // Setup: 
            var openApiProperty = _artifacts.First().Properties.FirstOrDefault(p => p.Name == "Description");

            Assert.That(openApiProperty != null, "Description property for artifact could not be found!");

            // Search for Description property value which is common to all artifacts
            var searchTerm = openApiProperty.TextOrChoiceValue;

            var itemTypeIds = SearchServiceTestHelper.GetItemTypeIdsForBaseArtifactTypes(_projects, baseArtifactTypes.ToList());

            var searchCriteria = new FullTextSearchCriteria(searchTerm, _projects.Select(p => p.Id), itemTypeIds);

            // Execute: Execute FullTextSearch with search term
            Assert.DoesNotThrow(() => fullTextSearchMetaDataResult =
                Helper.SearchService.FullTextSearchMetaData(_user, searchCriteria),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // Validation:
            ValidateSearchMetadataTest(fullTextSearchMetaDataResult, searchCriteria, "Description");
        }

        #region Permissions Tests

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase(0, 0, BaseArtifactType.Actor)]
        [TestRail(182364)]
        [Description("Save artifact but don't publish. Search artifact with other user. Executed search must return no search hits.")]
        public void FullTextSearchMetadata_SavedNotPublishedArtifactSearchedByOtherUser_VerifyEmptySearchResult(
            int expectedHitCount,
            int expectedTotalPageCount,
            BaseArtifactType baseArtifactType)
        {
            FullTextSearchMetaDataResult fullTextSearchMetaDataResult = null;

            // Setup: 
            // Create artifact in first project with random Name
            var artifact = Helper.CreateAndSaveArtifact(_projects.First(), _user, baseArtifactType);

            var searchTerm = artifact.Name;
            var searchCriteria = new FullTextSearchCriteria(searchTerm, _projects.Select(p => p.Id));

            SearchServiceTestHelper.WaitForFullTextSearchIndexerToUpdate(_user, Helper, searchCriteria, 1);

            // Execute: Perform search with another user
            Assert.DoesNotThrow(() => fullTextSearchMetaDataResult =
                Helper.SearchService.FullTextSearchMetaData(_user2, searchCriteria),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // Validation:
            ValidateSearchMetaDataPermissionsTest(fullTextSearchMetaDataResult, expectedHitCount, expectedTotalPageCount);
        }



        [TestCase(1, 1, BaseArtifactType.Actor)]
        [TestRail(182365)]
        [Description("Save artifact and publish. Search artifact with other user. Executed search must return search hits for search criteria.")]
        public void FullTextSearchMetadata_PublishedArtifactSearchedByOtherUser_VerifySearchResultIncludesItem(
            int expectedHitCount,
            int expectedTotalPageCount,
            BaseArtifactType baseArtifactType)
        {
            FullTextSearchMetaDataResult fullTextSearchMetaDataResult = null;

            // Setup: 
            // Create artifact in first project with random Name
            var artifact = Helper.CreateAndPublishArtifact(_projects.First(), _user, baseArtifactType);

            var searchTerm = artifact.Name;
            var searchCriteria = new FullTextSearchCriteria(searchTerm, _projects.Select(p => p.Id));

            SearchServiceTestHelper.WaitForFullTextSearchIndexerToUpdate(_user, Helper, searchCriteria, 1);

            // Execute: Perform search with another user
            Assert.DoesNotThrow(() => fullTextSearchMetaDataResult =
                Helper.SearchService.FullTextSearchMetaData(_user2, searchCriteria),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // Validation:
            ValidateSearchMetaDataPermissionsTest(fullTextSearchMetaDataResult, expectedHitCount, expectedTotalPageCount);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase(1, 1, BaseArtifactType.Actor)]
        [TestRail(182366)]
        [Description("Delete artifact but don't publish. Search artifact with other user. Executed search must return search hits for search criteria.")]
        public void FullTextSearchMetadata_DeletedNotPublishedArtifactSearchedByOtherUser_VerifySearchResultIncludesItem(
            int expectedHitCount,
            int expectedTotalPageCount,
            BaseArtifactType baseArtifactType)
        {
            FullTextSearchMetaDataResult fullTextSearchMetaDataResult = null;

            // Setup: 
            // Create artifact in first project with unique random Name
            var artifact = Helper.CreateAndPublishArtifact(_projects.First(), _user, baseArtifactType);

            var searchTerm = artifact.Name;
            var searchCriteria = new FullTextSearchCriteria(searchTerm, _projects.Select(p => p.Id));

            // Wait until second user can see the artifact in the search results or timeout occurs
            SearchServiceTestHelper.WaitForFullTextSearchIndexerToUpdate(_user2, Helper, searchCriteria, 1);

            Assert.DoesNotThrow(() => fullTextSearchMetaDataResult =
                Helper.SearchService.FullTextSearchMetaData(_user2, searchCriteria),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // After create and publish by first user, second user should be able to see the artifact
            Assert.That(fullTextSearchMetaDataResult.TotalCount.Equals(1), "The expected search hit count is {0} but {1} was returned.", 1, fullTextSearchMetaDataResult.TotalCount);

            // First user deletes artifact but doesn't save
            artifact.Delete(_user);

            // Wait until first user no longer sees the artifact in search results or timeout occurs
            SearchServiceTestHelper.WaitForFullTextSearchIndexerToUpdate(_user, Helper, searchCriteria, 0, waitForArtifactsToDisappear: true);

            Assert.DoesNotThrow(() => fullTextSearchMetaDataResult =
                Helper.SearchService.FullTextSearchMetaData(_user, searchCriteria),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // After delete but not publish by the original user, the original user should not be able to see the artifact
            Assert.That(fullTextSearchMetaDataResult.TotalCount.Equals(0), "The expected search hit count is {0} but {1} was returned.", 0, fullTextSearchMetaDataResult.TotalCount);

            Assert.DoesNotThrow(() => fullTextSearchMetaDataResult =
                Helper.SearchService.FullTextSearchMetaData(_user2, searchCriteria),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // Validation: 
            // After delete but not publish by the original user, the second user should still be able to see the artifact
            ValidateSearchMetaDataPermissionsTest(fullTextSearchMetaDataResult, expectedHitCount, expectedTotalPageCount);
        }

        [TestCase(0, 0, BaseArtifactType.Actor)]
        [TestRail(182367)]
        [Description("Delete artifact and publish. Search artifact with other user. Executed search must return no search hits.")]
        public void FullTextSearchMetadata_DeletedAndPublishedArtifactSearchedByOtherUser_VerifyEmptySearchResult(
            int expectedHitCount,
            int expectedTotalPageCount,
            BaseArtifactType baseArtifactType)
        {
            FullTextSearchMetaDataResult fullTextSearchMetaDataResult = null;

            // Setup: 
            // Create artifact in first project with unique random Name
            var artifact = Helper.CreateAndPublishArtifact(_projects.First(), _user, baseArtifactType);

            var searchTerm = artifact.Name;
            var searchCriteria = new FullTextSearchCriteria(searchTerm, _projects.Select(p => p.Id));

            // Wait until second user can see the artifact in the search results or timeout occurs
            SearchServiceTestHelper.WaitForFullTextSearchIndexerToUpdate(_user2, Helper, searchCriteria, 1);

            Assert.DoesNotThrow(() => fullTextSearchMetaDataResult =
                Helper.SearchService.FullTextSearchMetaData(_user2, searchCriteria),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // Verify that second user can see the artifact 
            Assert.That(fullTextSearchMetaDataResult.TotalCount.Equals(1), "The expected search hit count is {0} but {1} was returned.", 1, fullTextSearchMetaDataResult.TotalCount);

            // Delete and publish artifact by first user
            artifact.Delete(_user);
            artifact.Publish(_user);

            // Wait until second user can no longer see the artifact or timeout occurs
            SearchServiceTestHelper.WaitForFullTextSearchIndexerToUpdate(_user2, Helper, searchCriteria, 0, waitForArtifactsToDisappear: true);

            Assert.DoesNotThrow(() => fullTextSearchMetaDataResult =
                Helper.SearchService.FullTextSearchMetaData(_user2, searchCriteria),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // Validation: 
            // Second user should not be able to see the artifact
            ValidateSearchMetaDataPermissionsTest(fullTextSearchMetaDataResult, expectedHitCount, expectedTotalPageCount);
        }

        [TestCase(13, 2, TestHelper.ProjectRole.AuthorFullAccess)]
        [TestCase(13, 2, TestHelper.ProjectRole.Viewer)]
        [TestCase(0, 0, TestHelper.ProjectRole.None)]
        [TestRail(182372)]
        [Description("Search for artifact in a single project with user with different permissions. Executed search must return search metadata result " +
                     "that indicates whether the user was able to see the artifacts in the search results or not.")]
        public void FullTextSearchMetadata_UserWithProjectRolePermissionsOnSingleProject_VerifyResultIndicatesResultsFromSingleProject(
            int expectedHitCount,
            int expectedTotalPageCount,
            TestHelper.ProjectRole projectRole)
        {
            FullTextSearchMetaDataResult fullTextSearchMetaDataResult = null;

            // Setup: 
            var openApiProperty = _artifacts.First().Properties.FirstOrDefault(p => p.Name == "Description");
            Assert.That(openApiProperty != null, "Description property for artifact could not be found!");

            // Search for Description property value which is common to all artifacts
            var searchTerm = openApiProperty.TextOrChoiceValue;

            // Set search criteria over all projects
            var searchCriteria = new FullTextSearchCriteria(searchTerm, _projects.Select(p => p.Id));

            // Create user with project role to only 1 project
            var userWithProjectRole = TestHelper.CreateUserWithProjectRolePermissions(
                Helper,
                projectRole,
                new List<IProject> {_projects.First()});

            Assert.DoesNotThrow(() => fullTextSearchMetaDataResult =
                Helper.SearchService.FullTextSearchMetaData(userWithProjectRole, searchCriteria),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // Validation:
            // Although 2 artifacts exist with the search criteria, the new user only has permissions to a single project
            ValidateSearchMetaDataPermissionsTest(fullTextSearchMetaDataResult, expectedHitCount, expectedTotalPageCount);
        }

        [TestCase(26, 3, TestHelper.ProjectRole.AuthorFullAccess)]
        [TestCase(26, 3, TestHelper.ProjectRole.Viewer)]
        [TestCase(0, 0, TestHelper.ProjectRole.None)]
        [TestRail(182376)]
        [Description("Search for artifact in multiple projects with user with varying permissions. Executed search must return search metadata result " +
             "that indicates whether the user was able to see the artifacts in the search results or not.")]
        public void FullTextSearchMetadata_UserWithProjectRolePermissionsOnMultipleProjects_VerifyResultIndicatesResultsFromMultipleProjects(
            int expectedHitCount,
            int expectedTotalPageCount,
            TestHelper.ProjectRole projectRole)
        {
            FullTextSearchMetaDataResult fullTextSearchMetaDataResult = null;

            // Setup:
            var openApiProperty = _artifacts.First().Properties.FirstOrDefault(p => p.Name == "Description");
            Assert.That(openApiProperty != null, "Description property for artifact could not be found!");

            // Search for Description property value which is common to all artifacts
            var searchTerm = openApiProperty.TextOrChoiceValue;

            // Set search criteria over all projects
            var searchCriteria = new FullTextSearchCriteria(searchTerm, _projects.Select(p => p.Id));

            // Create user with project role with permissions to all projects
            var userWithProjectRole = TestHelper.CreateUserWithProjectRolePermissions(
                Helper,
                projectRole,
                _projects);

            Assert.DoesNotThrow(() => fullTextSearchMetaDataResult =
                Helper.SearchService.FullTextSearchMetaData(userWithProjectRole, searchCriteria),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // Validation:
            ValidateSearchMetaDataPermissionsTest(fullTextSearchMetaDataResult, expectedHitCount, expectedTotalPageCount);
        }

        [TestCase(0, 0, BaseArtifactType.Actor)]
        [TestRail(182368)]
        [Description("Search older version of artifact. Executed search must return search metadata result that indicates empty search results.")]
        public void FullTextSearchMetadata_SearchOlderVersionArtifact_VerifyEmptySearchResults(
            int expectedHitCount,
            int expectedTotalPageCount,
            BaseArtifactType baseArtifactType)
        {
            FullTextSearchMetaDataResult fullTextSearchMetaDataResult = null;

            // Setup: 
            IArtifact artifact = Helper.CreateAndPublishArtifact(_projects.First(), _user, baseArtifactType);

            var searchTerm = artifact.Name;
            var searchCriteria = new FullTextSearchCriteria(searchTerm, _projects.Select(p => p.Id));

            // Wait until user can see the artifact in the search results or timeout occurs
            SearchServiceTestHelper.WaitForFullTextSearchIndexerToUpdate(_user, Helper, searchCriteria, 1);

            // update artifact with name that doesn't match old search criteria
            var newSearchTerm = "NewName";

            artifact.Lock();
            SearchServiceTestHelper.UpdateArtifactProperty(Helper, _user, _projects.First(), artifact, baseArtifactType, "Name", newSearchTerm);

            artifact.Publish();

            var newSearchCriteria = new FullTextSearchCriteria(newSearchTerm, _projects.Select(p => p.Id));

            // Wait until user can see the artifact in the search results or timeout occurs
            SearchServiceTestHelper.WaitForFullTextSearchIndexerToUpdate(_user, Helper, newSearchCriteria, 1);

            // Attempt to search for artifact with original search criteria
            Assert.DoesNotThrow(() => fullTextSearchMetaDataResult =
                Helper.SearchService.FullTextSearchMetaData(_user, searchCriteria),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // Validation:
            ValidateSearchMetaDataPermissionsTest(fullTextSearchMetaDataResult, expectedHitCount, expectedTotalPageCount);
        }

        #endregion Permissions Tests

        #endregion 200 OK Tests

        #region 400 Bad Request Tests

        [TestCase]
        [TestRail(182254)]
        [Description("Search with no projects in search criteria. Executed search must return HTTP Exception 400: Bad Request.")]
        public void FullTextSearchMetadata_SearchMetadataWithNoProjects_400BadRequest()
        {
            // Setup: 
            var searchTerm = _artifacts.First().Name;
            var searchCriteria = new FullTextSearchCriteria(searchTerm, null);

            // Execute & Vaidation: 
            Assert.Throws<Http400BadRequestException>(() => Helper.SearchService.FullTextSearchMetaData(_user, searchCriteria),
                "SearchMetaData() should have thrown a HTTP 400 Bad Request exception but didn't.");
        }

        [TestCase]
        [TestRail(182361)]
        [Description("Search with no search criteria. Executed search must return HTTP Exception 400: Bad Request.")]
        public void FullTextSearchMetadata_SearchMetadataWithNoSearchCriteria_400BadRequest()
        {
            // Execute & Vaidation: 
            Assert.Throws<Http400BadRequestException>(() => Helper.SearchService.FullTextSearchMetaData(_user, null),
                "SearchMetaData() should have thrown a HTTP 400 Bad Request exception but didn't.");
        }

        [TestCase("")]
        [TestCase("XX")]
        [TestRail(182362)]
        [Description("Search with search term less than required length (3). Executed search must return HTTP Exception 400: Bad Request.")]
        public void FullTextSearchMetadata_SearchMetadataWithInvalidSearchTerm_400BadRequest(string searchTerm)
        {
            // Setup: 
            var searchCriteria = new FullTextSearchCriteria(searchTerm, _projects.Select(p => p.Id));

            // Execute & Vaidation: 
            Assert.Throws<Http400BadRequestException>(() => Helper.SearchService.FullTextSearchMetaData(_user, searchCriteria),
                "SearchMetaData() should have thrown a HTTP 400 Bad Request exception but didn't.");
        }

        #endregion 400 Bad Request Tests

        #region 401 Unauthorized Tests

        [TestRail(182255)]
        [TestCase(BaseArtifactType.Actor, "4f2cfd40d8994b8b812534b51711100d")]
        [TestCase(BaseArtifactType.Actor, "BADTOKEN")]
        [Description("Create an artifact and publish. Attempt to perform search metadata with a user that does not have authorization " +
             "to delete. Verify that HTTP 401 Unauthorized exception is thrown.")]
        public void FullTextSearchMetadata_SearchMetadataWithInvalidSession_401Unauthorized(BaseArtifactType artifactType, string invalidAccessControlToken)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_projects.First(), _user, artifactType);

            var searchTerm = artifact.Name;
            var searchCriteria = new FullTextSearchCriteria(searchTerm, _projects.Select(p => p.Id));

            // Create user with author project role to project
            var userWithProjectRole = TestHelper.CreateUserWithProjectRolePermissions(
                Helper,
                TestHelper.ProjectRole.AuthorFullAccess,
                _projects);

            // Replace the valid AccessControlToken with an invalid token
            userWithProjectRole.SetToken(invalidAccessControlToken);

            // Execute & Verify:
            Assert.Throws<Http401UnauthorizedException>(() => Helper.SearchService.FullTextSearchMetaData(userWithProjectRole, searchCriteria),
                "We should get a 401 Unauthorized when a user does not have authorization to search metadata!");
        }

        #endregion 401 Unauthorized Tests

        #region 404 Not Found Tests
        #endregion 404 Not Found Tests

        #region 409 Conflict Tests
        #endregion 409 Conflict Tests

        #region Private Functions

        /// <summary>
        /// Asserts that returned searchResult from the FullTextSearchMetadata call match with artifacts that are being searched.
        /// </summary>
        /// <param name="searchResult">The search result</param>
        /// <param name="searchCriteria">The criteria used for the search</param>
        /// <param name="propertyToSearch">The property name to be used for the search</param>
        /// <param name="pageSize"> (optional) pageSize value that indicates number of items that get displayed per page</param>
        private static void ValidateSearchMetadataTest(
            FullTextSearchMetaDataResult searchResult, 
            FullTextSearchCriteria searchCriteria, 
            string propertyToSearch, 
            int? pageSize = null)
        {
            ThrowIf.ArgumentNull(searchResult, nameof(searchResult));

            var expectedSearchResult = CreateExpectedSearchMetaDataResult(searchCriteria, propertyToSearch, pageSize);

            // Validation:
            Assert.That(searchResult.PageSize.Equals(expectedSearchResult.PageSize), "The expected default pagesize value is {0} but {1} was found from the returned searchResult.", expectedSearchResult.PageSize, searchResult.PageSize);
            Assert.That(searchResult.TotalCount.Equals(expectedSearchResult.TotalCount), "The expected total hit count is {0} but {1} was found from the returned searchResult.", expectedSearchResult.TotalCount, searchResult.TotalCount);
            Assert.That(searchResult.TotalPages.Equals(expectedSearchResult.TotalPages), "The expected total pages is {0} but {1} was found from the returned searchResult.", expectedSearchResult.TotalPages, searchResult.TotalPages);

            Assert.That(searchResult.FullTextSearchTypeItems.Count().Equals(expectedSearchResult.FullTextSearchTypeItems.Count()),"The expected item type count is {0} but {1} was found from the returned searchResult.", expectedSearchResult.FullTextSearchTypeItems.Count(), searchResult.FullTextSearchTypeItems.Count());

            foreach (var expectedFullTextSearchTypeItem in expectedSearchResult.FullTextSearchTypeItems)
            {
                var fullTextSearchTypeItem =
                    searchResult.FullTextSearchTypeItems.FirstOrDefault(
                        i => i.ItemTypeId == expectedFullTextSearchTypeItem.ItemTypeId);

                Assert.IsNotNull(fullTextSearchTypeItem, "Item type id {0} was expected but not found", expectedFullTextSearchTypeItem.ItemTypeId);
                Assert.That(fullTextSearchTypeItem.Count.Equals(expectedFullTextSearchTypeItem.Count), "A hit count of {0} was expected but {1} was returned.", expectedFullTextSearchTypeItem.Count, fullTextSearchTypeItem.Count);
                Assert.That(fullTextSearchTypeItem.ItemTypeId.Equals(expectedFullTextSearchTypeItem.ItemTypeId), "An item type id of {0} was expected but {1} was returned.", expectedFullTextSearchTypeItem.ItemTypeId, fullTextSearchTypeItem.ItemTypeId);
                Assert.That(fullTextSearchTypeItem.TypeName.Equals(expectedFullTextSearchTypeItem.TypeName), "A type name of {0} was expected but {1} was returned.", expectedFullTextSearchTypeItem.TypeName, fullTextSearchTypeItem.TypeName);
            }
        }

        /// <summary>
        /// Creates the expected serach metadata result for the specified serch criteria
        /// </summary>
        /// <param name="searchCriteria">The criteria used for the search</param>
        /// <param name="propertyToSearch">The property name to be used for the search</param>
        /// <param name="pageSize"> (optional) pageSize value that indicates number of items that get displayed per page</param>
        /// <returns>The expected search metadata result</returns>
        private static FullTextSearchMetaDataResult CreateExpectedSearchMetaDataResult(
            FullTextSearchCriteria searchCriteria, 
            string propertyToSearch, 
            int? pageSize = null)
        {
            pageSize = pageSize ?? DEFAULT_PAGE_SIZE_VALUE;
            pageSize = pageSize < 1 ? DEFAULT_PAGE_SIZE_VALUE : pageSize;

            var selectedArtifacts = new List<IArtifactBase>();

            foreach (var artifact in _artifacts)
            {
                if (artifact.Properties.Find(p => p.Name == propertyToSearch).TextOrChoiceValue == searchCriteria.Query)
                {
                    selectedArtifacts.Add(artifact);
                }
            }

            selectedArtifacts = selectedArtifacts.Where(a => searchCriteria.ProjectIds.Contains(a.ProjectId)).ToList();

            if (searchCriteria.ItemTypeIds != null)
            {
                selectedArtifacts = selectedArtifacts.Where(a => searchCriteria.ItemTypeIds.Contains(a.ArtifactTypeId)).ToList();

            }

            Assert.IsNotNull(selectedArtifacts, "No artifacts meet the search criteria!");

            var fullTextSearchTypeItems = new List<FullTextSearchTypeItem>();

            foreach (var artifactTypeId in selectedArtifacts.Select(a => a.ArtifactTypeId).Distinct())
            {
                var firstOrDefault = selectedArtifacts.FirstOrDefault(a => a.ArtifactTypeId == artifactTypeId);
                if (firstOrDefault != null)
                {
                    var fullTextSearchTypeItem = new FullTextSearchTypeItem
                    {
                        Count = selectedArtifacts.Count(a => a.ArtifactTypeId == artifactTypeId),
                        TypeName = firstOrDefault.ArtifactTypeName,
                        ItemTypeId = artifactTypeId
                    };

                    fullTextSearchTypeItems.Add(fullTextSearchTypeItem);
                }
            }

            var expectedFullTestSearchMetaDataResult = new FullTextSearchMetaDataResult
            {
                PageSize = (int)pageSize,
                TotalCount = selectedArtifacts.Count,
                TotalPages = (int)Math.Ceiling((decimal)selectedArtifacts.Count / (int) pageSize),
                FullTextSearchTypeItems = fullTextSearchTypeItems
            };

            return expectedFullTestSearchMetaDataResult;
        }

        /// <summary>
        ///  For permissions tests, asserts that returned searchResult from the FullTextSearchMetadata call matches what was expected.
        /// </summary>
        /// <param name="searchResult">The full text metadata search result</param>
        /// <param name="expectedHitCount">The expected number of hits resulting from the search</param>
        /// <param name="expectedTotalPageCount">The expected total page count returned by the search</param>

        /// <param name="pageSize"></param>
        private static void ValidateSearchMetaDataPermissionsTest(
            FullTextSearchMetaDataResult searchResult, 
            int expectedHitCount, 
            int expectedTotalPageCount,
            int pageSize = DEFAULT_PAGE_SIZE_VALUE)
        {
            ThrowIf.ArgumentNull(searchResult, nameof(searchResult));

            Assert.That(searchResult.FullTextSearchTypeItems.Count().Equals(expectedHitCount),
                "Returned Full text search type items count of {0} did not match expected count of {1}",
                searchResult.FullTextSearchTypeItems.Count(), expectedHitCount);
            Assert.That(searchResult.TotalCount.Equals(expectedHitCount),
                "The expected search hit count is {0} but {1} was returned.",
                expectedHitCount, searchResult.TotalCount);
            Assert.That(searchResult.TotalPages.Equals(expectedTotalPageCount),
                "The expected total page count is {0} but {1} was returned.",
                expectedTotalPageCount, searchResult.TotalPages);
            Assert.That(searchResult.PageSize.Equals(pageSize),
                "The expected default pagesize value is {0} but {1} was found from the returned searchResult.",
                pageSize, searchResult.PageSize);
        }

        #endregion Private Functions
    }
}
