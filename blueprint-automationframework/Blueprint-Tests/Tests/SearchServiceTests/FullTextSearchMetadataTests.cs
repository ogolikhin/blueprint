using System.Collections.Generic;
using CustomAttributes;
using Helper;
using Model;
using NUnit.Framework;
using System.Linq;
using Model.ArtifactModel;
using Model.Factories;
using Model.FullTextSearchModel.Impl;
using TestCommon;
using Utilities;

namespace SearchServiceTests
{
    [TestFixture]
    [Category(Categories.SearchService)]
    public class FullTextSearchMetadataTests : TestBase
    {
        private IUser _user;
        private IUser _user2;
        private List<IProject> _projects;
        private List<IArtifactBase> _artifacts;
        const int DEFAULT_PAGE_VALUE = 1;
        const int DEFAULT_PAGE_SIZE_VALUE = 10;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _user2 = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _projects = ProjectFactory.GetAllProjects(_user, true);
            _artifacts = SearchServiceTestHelper.SetupSearchData(_projects, _user, Helper);
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            Helper?.Dispose();
        }

        #region 200 OK Tests

        [TestCase(2, 1)]
        [TestRail(182247)]
        [Description("Search without optional parameter pagesize. Executed search must return search metadata result that indicates default page size.")]
        public void FullTextSearchMetadata_SearchMetadataWithoutPageSize_VerifySearchMetadataResultUsesDefaultPageSize(
            int expectedHitCount,
            int expectedTotalPageCount)
        {
            FullTextSearchMetaDataResult fullTextSearchMetaDataResult = null;

            // Setup: 
            var searchTerm = _artifacts.First().Name;
            var searchCriteria = new FullTextSearchCriteria(searchTerm, _projects.Select(p => p.Id));

            // Execute: Execute FullTextSearch with search term
            Assert.DoesNotThrow(() => fullTextSearchMetaDataResult =
                Helper.FullTextSearch.SearchMetaData(_user, searchCriteria),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // Validation:
            Assert.That(fullTextSearchMetaDataResult.TotalCount.Equals(expectedHitCount), "The expected search hit count is {0} but {1} was returned.", expectedHitCount, fullTextSearchMetaDataResult.TotalCount);
            Assert.That(fullTextSearchMetaDataResult.TotalPages.Equals(expectedTotalPageCount), "The expected total page count is {0} but {1} was returned.", expectedTotalPageCount, fullTextSearchMetaDataResult.TotalPages);
            Assert.That(fullTextSearchMetaDataResult.PageSize.Equals(DEFAULT_PAGE_SIZE_VALUE), "The expected default pagesize value is {0} but {1} was found from the returned searchResult.", DEFAULT_PAGE_SIZE_VALUE, fullTextSearchMetaDataResult.PageSize);
        }

        [TestCase(2, 2, 1)]
        [TestCase(2, 1, 5)]
        [TestCase(2, 1, 10)]
        [TestCase(2, 1, 100)]
        [TestRail(182245)]
        [Description("Search with optional parameter pagesize. Executed search must return search metadata result that indicates requested page size")]
        public void FullTextSearchMetadata_SearchMetadataWithValidPageSize_VerifySearchMetadataResult(
            int expectedHitCount,
            int expectedTotalPageCount,
            int requestedPageSize)
        {
            FullTextSearchMetaDataResult fullTextSearchMetaDataResult = null;

            // Setup: 
            var searchTerm = _artifacts.First().Name;
            var searchCriteria = new FullTextSearchCriteria(searchTerm, _projects.Select(p => p.Id));

            // Execute: Execute FullTextSearch with search terms
            Assert.DoesNotThrow(() => fullTextSearchMetaDataResult =
                Helper.FullTextSearch.SearchMetaData(_user, searchCriteria, requestedPageSize),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // Validation:
            Assert.That(fullTextSearchMetaDataResult.TotalCount.Equals(expectedHitCount), "The expected search hit count is {0} but {1} was returned.", expectedHitCount, fullTextSearchMetaDataResult.TotalCount);
            Assert.That(fullTextSearchMetaDataResult.TotalPages.Equals(expectedTotalPageCount), "The expected total page count is {0} but {1} was returned.", expectedTotalPageCount, fullTextSearchMetaDataResult.TotalPages);
            Assert.That(fullTextSearchMetaDataResult.PageSize.Equals(requestedPageSize), "The expected pagesize value is {0} but {1} was found from the returned searchResult.", requestedPageSize, fullTextSearchMetaDataResult.PageSize);
        }

        [TestCase(2, 1, 0)]
        [TestCase(2, 1, -1)]
        [TestRail(182247)]
        [Description("Search with invalid pagesize. Executed search must return search metadata result that indicates default page size.")]
        public void FullTextSearchMetadata_SearchMetadataWithInvalidPageSize_VerifySearchMetadataResultUsesDefaultPageSize(
            int expectedHitCount,
            int expectedTotalPageCount,
            int requestedPageSize)
        {
            FullTextSearchMetaDataResult fullTextSearchMetaDataResult = null;

            // Setup: 
            var searchTerm = _artifacts.First().Name;
            var searchCriteria = new FullTextSearchCriteria(searchTerm, _projects.Select(p => p.Id));

            // Execute: Execute FullTextSearch with search term
            Assert.DoesNotThrow(() => fullTextSearchMetaDataResult =
                Helper.FullTextSearch.SearchMetaData(_user, searchCriteria, requestedPageSize),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // Validation:
            Assert.That(fullTextSearchMetaDataResult.TotalCount.Equals(expectedHitCount), "The expected search hit count is {0} but {1} was returned.", expectedHitCount, fullTextSearchMetaDataResult.TotalCount);
            Assert.That(fullTextSearchMetaDataResult.TotalPages.Equals(expectedTotalPageCount), "The expected total page count is {0} but {1} was returned.", expectedTotalPageCount, fullTextSearchMetaDataResult.TotalPages);
            Assert.That(fullTextSearchMetaDataResult.PageSize.Equals(DEFAULT_PAGE_SIZE_VALUE), "The expected pagesize value is {0} but {1} was found from the returned searchResult.", DEFAULT_PAGE_SIZE_VALUE, fullTextSearchMetaDataResult.PageSize);
        }

        [TestCase(1, 1)]
        [TestRail(182247)]
        [Description("Search a single project where the search term is valid across multiple projects. Executed search must return" +
                     " search metadata result that indicates only the single project was searched.")]
        public void FullTextSearchMetadata_SearchMetadataFromSingleProject_VerifySearchMetadataResultIncludesOnlyProjectsSpecified(
            int expectedHitCount,
            int expectedTotalPageCount)
        {
            FullTextSearchMetaDataResult fullTextSearchMetaDataResultForSingleProject = null;

            // Setup: 
            var searchTerm = _artifacts.First().Name;

            // Seaarch only a single project from the list of projects.
            var searchCriteria = new FullTextSearchCriteria(searchTerm, new List<int>{_projects.First().Id});

            // Execute: Execute FullTextSearch with search term
            Assert.DoesNotThrow(() => fullTextSearchMetaDataResultForSingleProject =
                Helper.FullTextSearch.SearchMetaData(_user, searchCriteria),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // Validation:
            Assert.That(fullTextSearchMetaDataResultForSingleProject.TotalCount.Equals(expectedHitCount), "The expected search hit count is {0} but {1} was returned.", expectedHitCount, fullTextSearchMetaDataResultForSingleProject.TotalCount);
            Assert.That(fullTextSearchMetaDataResultForSingleProject.TotalPages.Equals(expectedTotalPageCount), "The expected total page count is {0} but {1} was returned.", expectedTotalPageCount, fullTextSearchMetaDataResultForSingleProject.TotalPages);
            Assert.That(fullTextSearchMetaDataResultForSingleProject.PageSize.Equals(DEFAULT_PAGE_SIZE_VALUE), "The expected pagesize value is {0} but {1} was found from the returned searchResult.", DEFAULT_PAGE_SIZE_VALUE, fullTextSearchMetaDataResultForSingleProject.PageSize);

            // Seaarch all projects from the list of projects and compare to result for single project.
            searchCriteria = new FullTextSearchCriteria(searchTerm, _projects.Select(p => p.Id));

            FullTextSearchMetaDataResult fullTextSearchMetaDataResultForMultipleProjects = null;

            Assert.DoesNotThrow(() => fullTextSearchMetaDataResultForMultipleProjects =
                Helper.FullTextSearch.SearchMetaData(_user, searchCriteria),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            Assert.That(!fullTextSearchMetaDataResultForMultipleProjects.TotalCount.Equals(fullTextSearchMetaDataResultForSingleProject.TotalCount), 
                "The search hit count for multiple projects was the same as for a single project but should be different.");
        }

        [TestCase(1, 1, -1)]
        [TestRail(182254)]
        [Description("Search with one valid project and one invalid project. Executed search must return valid data for the valid project and" +
                     " ignore the invalid project.")]
        public void FullTextSearchMetadata_SearchMetadataWithOneValidAndOneInvalidProject_VerifySearchMetadataResultIgnoresInvalidProject(
            int expectedHitCount,
            int expectedTotalPageCount,
            int invalidProjectId)
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
                Helper.FullTextSearch.SearchMetaData(_user, searchCriteria),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // Validation:
            Assert.That(fullTextSearchMetaDataResult.TotalCount.Equals(expectedHitCount), "The expected search hit count is {0} but {1} was returned.", expectedHitCount, fullTextSearchMetaDataResult.TotalCount);
            Assert.That(fullTextSearchMetaDataResult.TotalPages.Equals(expectedTotalPageCount), "The expected total page count is {0} but {1} was returned.", expectedTotalPageCount, fullTextSearchMetaDataResult.TotalPages);
            Assert.That(fullTextSearchMetaDataResult.PageSize.Equals(DEFAULT_PAGE_SIZE_VALUE), "The expected default pagesize value is {0} but {1} was found from the returned searchResult.", DEFAULT_PAGE_SIZE_VALUE, fullTextSearchMetaDataResult.PageSize);

        }

        [TestCase(2, 1, new [] {BaseArtifactType.Actor})]
        [TestCase(6, 1, new[] { BaseArtifactType.Actor, BaseArtifactType.Document, BaseArtifactType.Process })]
        [TestCase(24, 3, new[] {
            BaseArtifactType.Actor,
            BaseArtifactType.Document,
            BaseArtifactType.Process,
            BaseArtifactType.DomainDiagram,
            BaseArtifactType.BusinessProcess,
            BaseArtifactType.GenericDiagram,
            BaseArtifactType.Glossary,
            BaseArtifactType.Storyboard,
            BaseArtifactType.TextualRequirement,
            BaseArtifactType.UIMockup,
            BaseArtifactType.UseCase,
            BaseArtifactType.UseCaseDiagram })]
        [TestRail(182253)]
        [Description("Search over specific artifact types. Executed search must return search metadata result that match only the artifact .")]
        public void FullTextSearchMetadata_SearchMetadataForSpecificItemTypes_VerifySearchMetadataResultIncludesOnlyTypesSpecified(
            int expectedHitCount,
            int expectedTotalPageCount,
            BaseArtifactType[] baseArtifactTypes)
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
                Helper.FullTextSearch.SearchMetaData(_user, searchCriteria),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // Validation:
            Assert.That(fullTextSearchMetaDataResult.TotalCount.Equals(expectedHitCount), "The expected search hit count is {0} but {1} was returned.", expectedHitCount, fullTextSearchMetaDataResult.TotalCount);
            Assert.That(fullTextSearchMetaDataResult.TotalPages.Equals(expectedTotalPageCount), "The expected total page count is {0} but {1} was returned.", expectedTotalPageCount, fullTextSearchMetaDataResult.TotalPages);
            Assert.That(fullTextSearchMetaDataResult.PageSize.Equals(DEFAULT_PAGE_SIZE_VALUE), "The expected default pagesize value is {0} but {1} was found from the returned searchResult.", DEFAULT_PAGE_SIZE_VALUE, fullTextSearchMetaDataResult.PageSize);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase(2, 1, BaseArtifactType.Actor)]
        [TestRail(182364)]
        [Description("Save artifact but don't publish. Search artifact with other user. Executed search must return no search hits.")]
        public void FullTextSearchMetadata_SavedNotPublishedArtifactSearchedByOtherUser_VerifyEmptySearchResult(
            int expectedHitCount,
            int expectedTotalPageCount,
            BaseArtifactType baseArtifactType)
        {
            //TODO:  Is this test case even feasible?

            FullTextSearchMetaDataResult fullTextSearchMetaDataResult = null;

            // Setup: 
            // Create artifact in first project with random Name
            var artifact = Helper.CreateAndSaveArtifact(_projects.First(), _user, baseArtifactType);

            var searchTerm = artifact.Name;
            var searchCriteria = new FullTextSearchCriteria(searchTerm, _projects.Select(p => p.Id));

            SearchServiceTestHelper.WaitForSearchIndexerToUpdate(_user, Helper, searchCriteria, 1);

            // Execute: Execute FullTextSearch with search term
            Assert.DoesNotThrow(() => fullTextSearchMetaDataResult =
                Helper.FullTextSearch.SearchMetaData(_user, searchCriteria),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // Validation:
            Assert.That(fullTextSearchMetaDataResult.TotalCount.Equals(expectedHitCount), "The expected search hit count is {0} but {1} was returned.", expectedHitCount, fullTextSearchMetaDataResult.TotalCount);
            Assert.That(fullTextSearchMetaDataResult.TotalPages.Equals(expectedTotalPageCount), "The expected total page count is {0} but {1} was returned.", expectedTotalPageCount, fullTextSearchMetaDataResult.TotalPages);
            Assert.That(fullTextSearchMetaDataResult.PageSize.Equals(DEFAULT_PAGE_SIZE_VALUE), "The expected default pagesize value is {0} but {1} was found from the returned searchResult.", DEFAULT_PAGE_SIZE_VALUE, fullTextSearchMetaDataResult.PageSize);
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

            SearchServiceTestHelper.WaitForSearchIndexerToUpdate(_user, Helper, searchCriteria, 1);

            // Execute: Perform search with another user
            Assert.DoesNotThrow(() => fullTextSearchMetaDataResult =
                Helper.FullTextSearch.SearchMetaData(_user2, searchCriteria),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // Validation:
            Assert.That(fullTextSearchMetaDataResult.TotalCount.Equals(expectedHitCount), "The expected search hit count is {0} but {1} was returned.", expectedHitCount, fullTextSearchMetaDataResult.TotalCount);
            Assert.That(fullTextSearchMetaDataResult.TotalPages.Equals(expectedTotalPageCount), "The expected total page count is {0} but {1} was returned.", expectedTotalPageCount, fullTextSearchMetaDataResult.TotalPages);
            Assert.That(fullTextSearchMetaDataResult.PageSize.Equals(DEFAULT_PAGE_SIZE_VALUE), "The expected default pagesize value is {0} but {1} was found from the returned searchResult.", DEFAULT_PAGE_SIZE_VALUE, fullTextSearchMetaDataResult.PageSize);
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
            SearchServiceTestHelper.WaitForSearchIndexerToUpdate(_user2, Helper, searchCriteria, 1);

            Assert.DoesNotThrow(() => fullTextSearchMetaDataResult =
                Helper.FullTextSearch.SearchMetaData(_user2, searchCriteria),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // After create and publish by first user, second user should be able to see the artifact
            Assert.That(fullTextSearchMetaDataResult.TotalCount.Equals(1), "The expected search hit count is {0} but {1} was returned.", 1, fullTextSearchMetaDataResult.TotalCount);

            // First user deletes artifact but doesn't save
            Helper.ArtifactStore.DeleteArtifact(artifact, _user);
            artifact.Save(_user);

            // Wait until first user no longer sees the artifact in search results or timeout occurs
            SearchServiceTestHelper.WaitForSearchIndexerToUpdate(_user, Helper, searchCriteria, 0, waitForArtifactsToDisappear: true);

            Assert.DoesNotThrow(() => fullTextSearchMetaDataResult =
                Helper.FullTextSearch.SearchMetaData(_user, searchCriteria),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // After delete but not publish by the original user, the original user should not be able to see the artifact
            Assert.That(fullTextSearchMetaDataResult.TotalCount.Equals(0), "The expected search hit count is {0} but {1} was returned.", 0, fullTextSearchMetaDataResult.TotalCount);

            Assert.DoesNotThrow(() => fullTextSearchMetaDataResult =
                Helper.FullTextSearch.SearchMetaData(_user2, searchCriteria),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // Validation: 
            // After delete but not publish by the original user, the second user should still be able to see the artifact
            Assert.That(fullTextSearchMetaDataResult.TotalCount.Equals(expectedHitCount), "The expected search hit count is {0} but {1} was returned.", expectedHitCount, fullTextSearchMetaDataResult.TotalCount);
            Assert.That(fullTextSearchMetaDataResult.TotalPages.Equals(expectedTotalPageCount), "The expected total page count is {0} but {1} was returned.", expectedTotalPageCount, fullTextSearchMetaDataResult.TotalPages);
            Assert.That(fullTextSearchMetaDataResult.PageSize.Equals(DEFAULT_PAGE_SIZE_VALUE), "The expected default pagesize value is {0} but {1} was found from the returned searchResult.", DEFAULT_PAGE_SIZE_VALUE, fullTextSearchMetaDataResult.PageSize);
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
            SearchServiceTestHelper.WaitForSearchIndexerToUpdate(_user2, Helper, searchCriteria, 1);

            Assert.DoesNotThrow(() => fullTextSearchMetaDataResult =
                Helper.FullTextSearch.SearchMetaData(_user2, searchCriteria),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // Verify that second user can see the artifact 
            Assert.That(fullTextSearchMetaDataResult.TotalCount.Equals(1), "The expected search hit count is {0} but {1} was returned.", 1, fullTextSearchMetaDataResult.TotalCount);

            // Delete and publish artifact by first user
            artifact.Delete(_user);
            artifact.Publish(_user);

            // Wait until second user can no longer see the artifact or timeout occurs
            SearchServiceTestHelper.WaitForSearchIndexerToUpdate(_user2, Helper, searchCriteria, 0, waitForArtifactsToDisappear: true);

            Assert.DoesNotThrow(() => fullTextSearchMetaDataResult =
                Helper.FullTextSearch.SearchMetaData(_user2, searchCriteria),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // Validation: 
            // Second user should not be able to see the artifact
            Assert.That(fullTextSearchMetaDataResult.TotalCount.Equals(expectedHitCount), "The expected search hit count is {0} but {1} was returned.", expectedHitCount, fullTextSearchMetaDataResult.TotalCount);
            Assert.That(fullTextSearchMetaDataResult.TotalPages.Equals(expectedTotalPageCount), "The expected total page count is {0} but {1} was returned.", expectedTotalPageCount, fullTextSearchMetaDataResult.TotalPages);
            Assert.That(fullTextSearchMetaDataResult.PageSize.Equals(DEFAULT_PAGE_SIZE_VALUE), "The expected default pagesize value is {0} but {1} was found from the returned searchResult.", DEFAULT_PAGE_SIZE_VALUE, fullTextSearchMetaDataResult.PageSize);
        }

        [TestCase(1, 1, SearchServiceTestHelper.ProjectRole.Author)]
        [TestCase(1, 1, SearchServiceTestHelper.ProjectRole.Viewer)]
        [TestCase(0, 0, SearchServiceTestHelper.ProjectRole.None)]
        [TestRail(182372)]
        [Description("Search without optional parameter pagesize. Executed search must return search metadata result that indicates default page size.")]
        public void FullTextSearchMetadata_UserWithProjectRolePermissionsOnSingleProject_VerifyResultIndicatesResultsFromSingleProject(
            int expectedHitCount,
            int expectedTotalPageCount,
            SearchServiceTestHelper.ProjectRole projectRole)
        {
            FullTextSearchMetaDataResult fullTextSearchMetaDataResult = null;

            // Setup: 
            var searchTerm = _artifacts.First().Name;
            // Set search criteria over all projects
            var searchCriteria = new FullTextSearchCriteria(searchTerm, _projects.Select(p => p.Id));

            // Create user with project role to only 1 project
            var userWithProjectRole = SearchServiceTestHelper.CreateUserWithProjectRolePermissions(
                Helper,
                projectRole,
                _projects.First());

            Assert.DoesNotThrow(() => fullTextSearchMetaDataResult =
                Helper.FullTextSearch.SearchMetaData(userWithProjectRole, searchCriteria),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // Validation:
            // Although 2 artifacts exist with the search criteria, the new user only has permissions to a single project
            Assert.That(fullTextSearchMetaDataResult.TotalCount.Equals(expectedHitCount), "The expected search hit count is {0} but {1} was returned.", expectedHitCount, fullTextSearchMetaDataResult.TotalCount);
            Assert.That(fullTextSearchMetaDataResult.TotalPages.Equals(expectedTotalPageCount), "The expected total page count is {0} but {1} was returned.", expectedTotalPageCount, fullTextSearchMetaDataResult.TotalPages);
            Assert.That(fullTextSearchMetaDataResult.PageSize.Equals(DEFAULT_PAGE_SIZE_VALUE), "The expected default pagesize value is {0} but {1} was found from the returned searchResult.", DEFAULT_PAGE_SIZE_VALUE, fullTextSearchMetaDataResult.PageSize);
        }


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
            Assert.Throws<Http400BadRequestException>(() => Helper.FullTextSearch.SearchMetaData(_user, searchCriteria),
                "SearchMetaData() should have thrown a HTTP 400 Bad Request exception but didn't.");
        }

        [TestCase]
        [TestRail(182361)]
        [Description("Search with no search criteria. Executed search must return HTTP Exception 400: Bad Request.")]
        public void FullTextSearchMetadata_SearchMetadataWithNoSearchCriteria_400BadRequest()
        {
            // Execute & Vaidation: 
            Assert.Throws<Http400BadRequestException>(() => Helper.FullTextSearch.SearchMetaData(_user, null),
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
            Assert.Throws<Http400BadRequestException>(() => Helper.FullTextSearch.SearchMetaData(_user, searchCriteria),
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
            var userWithProjectRole = SearchServiceTestHelper.CreateUserWithProjectRolePermissions(
                Helper,
                SearchServiceTestHelper.ProjectRole.Author, 
                _projects.First());

            // Replace the valid AccessControlToken with an invalid token
            userWithProjectRole.SetToken(invalidAccessControlToken);

            // Execute & Verify:
            Assert.Throws<Http401UnauthorizedException>(() => Helper.FullTextSearch.SearchMetaData(userWithProjectRole, searchCriteria),
                "We should get a 401 Unauthorized when a user does not have authorization to search metadata!");
        }

        #endregion 401 Unauthorized Tests

        #region 404 Not Found Tests
        #endregion 404 Not Found Tests

        #region 409 Conflict Tests
        #endregion 409 Conflict Tests

        #region Private Functions


        #endregion Private Functions

    }
}
