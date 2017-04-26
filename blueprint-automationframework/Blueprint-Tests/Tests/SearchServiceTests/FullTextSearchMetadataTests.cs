using System;
using System.Collections.Generic;
using CustomAttributes;
using Helper;
using Model;
using Model.ModelHelpers;
using NUnit.Framework;
using System.Linq;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.SearchServiceModel.Impl;
using Model.StorytellerModel.Impl;
using TestCommon;
using Utilities;
using Utilities.Factories;

namespace SearchServiceTests
{
    [TestFixture]
    [Category(Categories.SearchService)]
    public class FullTextSearchMetadataTests : TestBase
    {
        private static IUser _user;
        private static IUser _user2;
        private static List<IProject> _projects;
        private static List<ArtifactWrapper> _artifacts;

        protected const string FULLTEXTSEARCHMETADATA_PATH = RestPaths.Svc.SearchService.FullTextSearch.METADATA;
        const int DEFAULT_PAGE_SIZE_VALUE = 10;
        const string DESCRIPTION = "Description";

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _user2 = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _projects = ProjectFactory.GetAllProjects(_user, true, true);
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
            // Setup: 
            var searchTerm = _artifacts.First().Name;
            var searchCriteria = new FullTextSearchCriteria(searchTerm, _projects.Select(p => p.Id));

            // Execute: Execute FullTextSearch with search term
            FullTextSearchMetaDataResult fullTextSearchMetaDataResult = null;

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
            // Setup: 
            var searchTerm = _artifacts.First().Name;
            var searchCriteria = new FullTextSearchCriteria(searchTerm, _projects.Select(p => p.Id));

            // Execute: Execute FullTextSearch with search terms
            FullTextSearchMetaDataResult fullTextSearchMetaDataResult = null;

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
            // Setup: 
            var searchTerm = _artifacts.First().Name;

            // Search only a single project from the list of projects.
            var searchCriteria = new FullTextSearchCriteria(searchTerm, new List<int> { _projects.First().Id });

            // Execute: Execute FullTextSearch with search term
            FullTextSearchMetaDataResult fullTextSearchMetaDataResultForSingleProject = null;

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

            Assert.AreNotEqual(fullTextSearchMetaDataResultForSingleProject.TotalCount, fullTextSearchMetaDataResultForMultipleProjects.TotalCount,
                "The search hit count for multiple projects was the same as for a single project but should be different.");
        }

        [TestCase(-1)]
        [TestRail(182363)]
        [Description("Search with one valid project and one invalid project. Executed search must return valid data for the valid project and" +
                     " ignore the invalid project.")]
        public void FullTextSearchMetadata_SearchMetadataWithOneValidAndOneInvalidProject_VerifySearchMetadataResultIgnoresInvalidProject(int invalidProjectId)
        {
            // Setup:
            var searchTerm = _artifacts.First().Name;
            var projectIds = new List<int>
            {
                _projects.First().Id,
                invalidProjectId
            };

            var searchCriteria = new FullTextSearchCriteria(searchTerm, projectIds);

            // Execute: Execute FullTextSearch with search term
            FullTextSearchMetaDataResult fullTextSearchMetaDataResult = null;

            Assert.DoesNotThrow(() => fullTextSearchMetaDataResult =
                Helper.SearchService.FullTextSearchMetaData(_user, searchCriteria),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // Validation:
            ValidateSearchMetadataTest(fullTextSearchMetaDataResult, searchCriteria, "Name");
        }

        [TestCase(new[] { ItemTypePredefined.Actor })]
        [TestCase(new[] { ItemTypePredefined.Document, ItemTypePredefined.ArtifactBaseline })]
        [TestCase(new[] { ItemTypePredefined.TextualRequirement, ItemTypePredefined.ArtifactCollection })]
        [TestCase(new[] { ItemTypePredefined.Actor, ItemTypePredefined.Document, ItemTypePredefined.Process })]
        [TestCase(new[] {
            ItemTypePredefined.Actor,
            ItemTypePredefined.Document,
            ItemTypePredefined.Process,
            ItemTypePredefined.DomainDiagram,
            ItemTypePredefined.BusinessProcess,
            ItemTypePredefined.GenericDiagram,
            ItemTypePredefined.Glossary,
            ItemTypePredefined.PrimitiveFolder,
            ItemTypePredefined.Storyboard,
            ItemTypePredefined.TextualRequirement,
            ItemTypePredefined.UIMockup,
            ItemTypePredefined.UseCase,
            ItemTypePredefined.UseCaseDiagram })]
        [TestRail(182253)]
        [Description("Search over specific artifact types. Executed search must return search metadata result that match only the artifact .")]
        public void FullTextSearchMetadata_SearchMetadataForSpecificItemTypes_VerifySearchMetadataResultIncludesOnlyTypesSpecified(ItemTypePredefined[] artifactTypes)
        {
            ThrowIf.ArgumentNull(artifactTypes, nameof(artifactTypes));

            // Setup: 
            string description = _artifacts[0].Description;

            // Search for Description property value which is common to all artifacts
            var searchTerm = StringUtilities.ConvertHtmlToText(description);
            var itemTypeIds = SearchServiceTestHelper.GetItemTypeIdsForBaseArtifactTypes(_projects, artifactTypes.ToList());
            var searchCriteria = new FullTextSearchCriteria(searchTerm, _projects.Select(p => p.Id), itemTypeIds);

            // Execute: Execute FullTextSearch with search term
            FullTextSearchMetaDataResult fullTextSearchMetaDataResult = null;

            Assert.DoesNotThrow(() => fullTextSearchMetaDataResult =
                Helper.SearchService.FullTextSearchMetaData(_user, searchCriteria),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // Validation:
            ValidateSearchMetadataTest(fullTextSearchMetaDataResult, searchCriteria, DESCRIPTION);
        }

        [Category(Categories.CustomData)]
        [TestCase("Std-Text-Required-HasDefault")]
        [TestRail(234334)]
        [Description("Search for search term that is present in artifact textual property.  Execute metadata Search - " +
            "Must return Metadata SearchResult with expected hit count.")]
        public void FullTextSearchMetadata_SearchPublishedArtifactTextualProperties_VerifyMetadataSearchResult(
            string propertyName)
        {
            // Setup: 
            var selectedProjectIds = _projects.ConvertAll(p => p.Id);
            var project = Helper.GetProject(TestHelper.GoldenDataProject.EmptyProjectWithSubArtifactRequiredProperties, _user);
            var artifact = Helper.CreateWrapAndSaveNovaArtifact(project, _user, ItemTypePredefined.Process, artifactTypeName: "Process");

            string searchTerm = "SearchText_" + RandomGenerator.RandomAlphaNumericUpperAndLowerCase(25);

            // Update custom property in artifact.
            ArtifactStoreHelper.UpdateArtifactCustomProperty(artifact, _user, project, PropertyPrimitiveType.Text, propertyName, searchTerm, Helper.ArtifactStore);
            Helper.ArtifactStore.PublishArtifact(artifact, _user);

            var searchCriteria = new FullTextSearchCriteria(searchTerm, selectedProjectIds);
            SearchServiceTestHelper.WaitForFullTextSearchIndexerToUpdate(_user, Helper, searchCriteria, 1, timeoutInMilliseconds: 30000);

            // Execute: 
            FullTextSearchMetaDataResult fullTextSearchMetaDataResult = null;
            Assert.DoesNotThrow(() => fullTextSearchMetaDataResult = Helper.SearchService.FullTextSearchMetaData(_user, searchCriteria),
                "POST {0} call failed when using search term {1} which matches with published artifacts!", FULLTEXTSEARCHMETADATA_PATH, searchCriteria.Query);

            // Verify: 
            Assert.AreEqual(1, fullTextSearchMetaDataResult.TotalCount,
                "The number of hits was {0} but {1} was expected", fullTextSearchMetaDataResult.TotalCount, 1);

            Assert.AreEqual(DEFAULT_PAGE_SIZE_VALUE, fullTextSearchMetaDataResult.PageSize,
                "The page size was {0} but {1} was expected", fullTextSearchMetaDataResult.TotalCount, DEFAULT_PAGE_SIZE_VALUE);

            Assert.AreEqual(1, fullTextSearchMetaDataResult.TotalPages,
                "The total pages was {0} but {1} was expected", fullTextSearchMetaDataResult.TotalCount, 1);

            Assert.AreEqual("Process", fullTextSearchMetaDataResult.Items.First().TypeName,
                "The artifact type name was {0} but {1} was expected", fullTextSearchMetaDataResult.TotalCount, "Process");

            Assert.AreEqual(1, fullTextSearchMetaDataResult.Items.First().Count,
                "The hit count for artifact type was {0} but {1} was expected", fullTextSearchMetaDataResult.TotalCount, 1);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [Category(Categories.CustomData)]
        [TestCase("Std-Text-Required-HasDefault")]
        [TestRail(234335)]
        [Description("Search for search term that is present in subartifact textual property.  Execute Search - " +
            "Must return SearchResult with list of FullTextSearchItems matching artifacts with subartifact textual property.")]
        public void FullTextSearchMetadata_SearchPublishedSubArtifactTextualProperties_VerifyMetadataSearchResult(
            string propertyName)
        {
            // Setup: 
            var selectedProjectIds = _projects.ConvertAll(p => p.Id);
            var project = Helper.GetProject(TestHelper.GoldenDataProject.EmptyProjectWithSubArtifactRequiredProperties, _user);
            var artifact = Helper.CreateWrapAndSaveNovaArtifact(project, _user, ItemTypePredefined.Process, artifactTypeName: "Process");

            string searchTerm = "SearchText_" + RandomGenerator.RandomAlphaNumericUpperAndLowerCase(25);

            // Get nova subartifact
            var novaProcess = Helper.Storyteller.GetNovaProcess(_user, artifact.Id);
            var processShape = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            var novaSubArtifact = Helper.ArtifactStore.GetSubartifact(_user, artifact.Id, processShape.Id);

            ArtifactStoreHelper.UpdateSubArtifactCustomProperty(artifact, novaSubArtifact, _user, project, PropertyPrimitiveType.Text, propertyName, searchTerm, Helper.ArtifactStore);
            Helper.ArtifactStore.PublishArtifact(artifact, _user);

            var searchCriteria = new FullTextSearchCriteria(searchTerm, selectedProjectIds);
            SearchServiceTestHelper.WaitForFullTextSearchIndexerToUpdate(_user, Helper, searchCriteria, 1, timeoutInMilliseconds: 30000);

            // Execute: 
            FullTextSearchMetaDataResult fullTextSearchMetaDataResult = null;
            Assert.DoesNotThrow(() => fullTextSearchMetaDataResult = Helper.SearchService.FullTextSearchMetaData(_user, searchCriteria),
                "POST {0} call failed when using search term {1} which matches with published artifacts!", FULLTEXTSEARCHMETADATA_PATH, searchCriteria.Query);

            // Verify: 
            Assert.AreEqual(1, fullTextSearchMetaDataResult.TotalCount,
                "The number of hits was {0} but {1} was expected", fullTextSearchMetaDataResult.TotalCount, 1);

            Assert.AreEqual(DEFAULT_PAGE_SIZE_VALUE, fullTextSearchMetaDataResult.PageSize,
                "The page size was {0} but {1} was expected", fullTextSearchMetaDataResult.TotalCount, DEFAULT_PAGE_SIZE_VALUE);

            Assert.AreEqual(1, fullTextSearchMetaDataResult.TotalPages,
                "The total pages was {0} but {1} was expected", fullTextSearchMetaDataResult.TotalCount, 1);

            Assert.AreEqual("Process: Shape", fullTextSearchMetaDataResult.Items.First().TypeName,
                "The artifact type name was {0} but {1} was expected", fullTextSearchMetaDataResult.TotalCount, "Process: Shape");

            Assert.AreEqual(1, fullTextSearchMetaDataResult.Items.First().Count,
                "The hit count for artifact type was {0} but {1} was expected", fullTextSearchMetaDataResult.TotalCount, 1);
        }

        #region Permissions Tests

        [TestCase(0, 0, BaseArtifactType.Actor)]
        [TestRail(182364)]
        [Description("Save artifact but don't publish. Search artifact with other user. Executed search must return no search hits.")]
        public void FullTextSearchMetadata_SavedNotPublishedArtifactSearchedByOtherUser_VerifyEmptySearchResult(
            int expectedHitCount,
            int expectedTotalPageCount,
            BaseArtifactType baseArtifactType)
        {
            // Setup:
            // Create artifact in first project with random Name.
            var savedArtifact = Helper.CreateAndSaveArtifact(_projects.First(), _user, baseArtifactType);
            var publishedArtifact = Helper.CreateAndPublishArtifact(_projects.First(), _user, baseArtifactType);

            var searchTerm = publishedArtifact.Name;
            var searchCriteria = new FullTextSearchCriteria(searchTerm, _projects.Select(p => p.Id));

            // Wait until the published artifact is found by search index.
            SearchServiceTestHelper.WaitForFullTextSearchIndexerToUpdate(_user, Helper, searchCriteria, 1);

            // Execute: Perform search for saved artifact with another user.
            FullTextSearchMetaDataResult fullTextSearchMetaDataResult = null;

            searchCriteria.Query = savedArtifact.Name;

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
            // Setup: 
            // Create artifact in first project with random Name
            var artifact = Helper.CreateAndPublishArtifact(_projects.First(), _user, baseArtifactType);

            var searchTerm = artifact.Name;
            var searchCriteria = new FullTextSearchCriteria(searchTerm, _projects.Select(p => p.Id));

            SearchServiceTestHelper.WaitForFullTextSearchIndexerToUpdate(_user, Helper, searchCriteria, 1);

            // Execute: Perform search with another user
            FullTextSearchMetaDataResult fullTextSearchMetaDataResult = null;

            Assert.DoesNotThrow(() => fullTextSearchMetaDataResult =
                Helper.SearchService.FullTextSearchMetaData(_user2, searchCriteria),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // Validation:
            ValidateSearchMetaDataPermissionsTest(fullTextSearchMetaDataResult, expectedHitCount, expectedTotalPageCount);
        }

        [TestCase(1, 1, BaseArtifactType.Actor)]
        [TestRail(182366)]
        [Description("Delete artifact but don't publish. Search artifact with other user. Executed search must return search hits for search criteria.")]
        public void FullTextSearchMetadata_DeletedNotPublishedArtifactSearchedByOtherUser_VerifySearchResultIncludesItem(
            int expectedHitCount,
            int expectedTotalPageCount,
            BaseArtifactType baseArtifactType)
        {
            // Setup:
            // Create artifact in first project with unique random Name.
            var artifactToDelete = Helper.CreateAndPublishArtifact(_projects.First(), _user, baseArtifactType);
            var artifactToDeleteAndPublish = Helper.CreateAndPublishArtifact(_projects.First(), _user, baseArtifactType);

            var searchTerm = artifactToDeleteAndPublish.Name;
            var searchCriteria = new FullTextSearchCriteria(searchTerm, _projects.Select(p => p.Id));

            // Wait until second user can see the artifact in the search results or timeout occurs.
            SearchServiceTestHelper.WaitForFullTextSearchIndexerToUpdate(_user2, Helper, searchCriteria, 1);
            FullTextSearchMetaDataResult fullTextSearchMetaDataResult = null;

            Assert.DoesNotThrow(() => fullTextSearchMetaDataResult =
                Helper.SearchService.FullTextSearchMetaData(_user2, searchCriteria),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // After create and publish by first user, second user should be able to see the artifact.
            Assert.AreEqual(1, fullTextSearchMetaDataResult.TotalCount, "The expected search hit count is {0} but {1} was returned.",
                1, fullTextSearchMetaDataResult.TotalCount);

            // First user deletes artifact but doesn't publish.
            artifactToDelete.Delete(_user);

            // Delete & publish another artifact to know when search index is updated.
            artifactToDeleteAndPublish.Delete(_user);
            artifactToDeleteAndPublish.Publish(_user);

            // Wait until first user no longer sees the artifact that was deleted & published in search results or timeout occurs.
            SearchServiceTestHelper.WaitForFullTextSearchIndexerToUpdate(_user, Helper, searchCriteria, 0, waitForArtifactsToDisappear: true);

            Assert.DoesNotThrow(() => fullTextSearchMetaDataResult =
                Helper.SearchService.FullTextSearchMetaData(_user, searchCriteria),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // After delete but not publish by the original user, the original user should not be able to see the artifact
            Assert.AreEqual(0, fullTextSearchMetaDataResult.TotalCount, "The expected search hit count is {0} but {1} was returned.",
                0, fullTextSearchMetaDataResult.TotalCount);

            // Execute:
            searchCriteria.Query = artifactToDelete.Name;

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
            // Setup: 
            // Create artifact in first project with unique random Name
            var artifact = Helper.CreateAndPublishArtifact(_projects.First(), _user, baseArtifactType);

            var searchTerm = artifact.Name;
            var searchCriteria = new FullTextSearchCriteria(searchTerm, _projects.Select(p => p.Id));

            // Wait until second user can see the artifact in the search results or timeout occurs
            SearchServiceTestHelper.WaitForFullTextSearchIndexerToUpdate(_user2, Helper, searchCriteria, 1);
            FullTextSearchMetaDataResult fullTextSearchMetaDataResult = null;

            Assert.DoesNotThrow(() => fullTextSearchMetaDataResult =
                Helper.SearchService.FullTextSearchMetaData(_user2, searchCriteria),
                "SearchMetaData() call failed when using following search term: {0}!",
                searchCriteria.Query);

            // Verify that second user can see the artifact 
            Assert.AreEqual(1, fullTextSearchMetaDataResult.TotalCount,
                "The expected search hit count is {0} but {1} was returned.",
                1, fullTextSearchMetaDataResult.TotalCount);

            // Delete and publish artifact by first user
            artifact.Delete(_user);
            artifact.Publish(_user);

            // Wait until second user can no longer see the artifact or timeout occurs
            SearchServiceTestHelper.WaitForFullTextSearchIndexerToUpdate(_user2, Helper, searchCriteria, 0, waitForArtifactsToDisappear: true);

            // Execute:
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
            // Setup: 
            string description = _artifacts[0].Description;
            
            // Search for Description property value which is common to all artifacts
            var searchTerm = StringUtilities.ConvertHtmlToText(description);

            // Set search criteria over all projects
            var searchCriteria = new FullTextSearchCriteria(searchTerm, _projects.Select(p => p.Id));

            // Create user with project role to only 1 project
            var userWithProjectRole = Helper.CreateUserWithProjectRolePermissions(
                projectRole,
                new List<IProject> { _projects.First() });

            // Execute:
            FullTextSearchMetaDataResult fullTextSearchMetaDataResult = null;

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
            // Setup:
            string description = _artifacts[0].Description;
            
            // Search for Description property value which is common to all artifacts
            var searchTerm = StringUtilities.ConvertHtmlToText(description);

            // Set search criteria over all projects
            var searchCriteria = new FullTextSearchCriteria(searchTerm, _projects.Select(p => p.Id));

            // Create user with project role with permissions to all projects
            var userWithProjectRole = Helper.CreateUserWithProjectRolePermissions(
                projectRole,
                _projects);

            // Execute:
            FullTextSearchMetaDataResult fullTextSearchMetaDataResult = null;

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
            // Setup: 
            var artifact = Helper.CreateAndPublishArtifact(_projects.First(), _user, baseArtifactType);

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

            // Execute:
            FullTextSearchMetaDataResult fullTextSearchMetaDataResult = null;

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
            var artifact = Helper.CreateAndPublishArtifact(_projects.First(), _user, artifactType);

            var searchTerm = artifact.Name;
            var searchCriteria = new FullTextSearchCriteria(searchTerm, _projects.Select(p => p.Id));

            // Create user with author project role to project
            var userWithProjectRole = Helper.CreateUserWithProjectRolePermissions(
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
            Assert.AreEqual(expectedSearchResult.PageSize, searchResult.PageSize,
                "The expected default pagesize value is {0} but {1} was found from the returned searchResult.",
                expectedSearchResult.PageSize, searchResult.PageSize);
            Assert.AreEqual(expectedSearchResult.TotalCount, searchResult.TotalCount,
                "The expected total hit count is {0} but {1} was found from the returned searchResult.",
                expectedSearchResult.TotalCount, searchResult.TotalCount);
            Assert.AreEqual(expectedSearchResult.TotalPages, searchResult.TotalPages,
                "The expected total pages is {0} but {1} was found from the returned searchResult.",
                expectedSearchResult.TotalPages, searchResult.TotalPages);

            Assert.IsNotNull(searchResult.Items, "List of items shouldn't be empty");
            Assert.AreEqual(expectedSearchResult.Items.Count(), searchResult.Items.Count(), 
                "The expected item type count is {0} but {1} was found from the returned searchResult.",
                expectedSearchResult.Items.Count(), searchResult.Items.Count());

            foreach (var expectedFullTextSearchTypeItem in expectedSearchResult.Items)
            {
                var fullTextSearchTypeItem =
                    searchResult.Items.FirstOrDefault(
                        i => i.ItemTypeId == expectedFullTextSearchTypeItem.ItemTypeId);

                Assert.IsNotNull(fullTextSearchTypeItem, "Item type id {0} was expected but not found",
                    expectedFullTextSearchTypeItem.ItemTypeId);
                Assert.AreEqual(expectedFullTextSearchTypeItem.Count, fullTextSearchTypeItem.Count,
                    "A hit count of {0} was expected but {1} was returned.",
                    expectedFullTextSearchTypeItem.Count, fullTextSearchTypeItem.Count);
                Assert.AreEqual(expectedFullTextSearchTypeItem.ItemTypeId, fullTextSearchTypeItem.ItemTypeId,
                    "An item type id of {0} was expected but {1} was returned.",
                    expectedFullTextSearchTypeItem.ItemTypeId, fullTextSearchTypeItem.ItemTypeId);
                Assert.AreEqual(expectedFullTextSearchTypeItem.TypeName, fullTextSearchTypeItem.TypeName,
                    "A type name of {0} was expected but {1} was returned.",
                    expectedFullTextSearchTypeItem.TypeName, fullTextSearchTypeItem.TypeName);
            }
        }

        /// <summary>
        /// Creates the expected serach metadata result for the specified search criteria
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

            var selectedArtifacts = new List<ArtifactWrapper>();

            foreach (var artifact in _artifacts)
            {
                switch (propertyToSearch)
                {
                    case "Name":
                        if (artifact.Name == searchCriteria.Query)
                        {
                            selectedArtifacts.Add(artifact);
                        }
                        break;
                    case "Description":
                        if (StringUtilities.ConvertHtmlToText(artifact.Description) == searchCriteria.Query)
                        {
                            selectedArtifacts.Add(artifact);
                        }
                        break;
                    default:
                        break;
                }
            }

            selectedArtifacts = selectedArtifacts.Where(a => searchCriteria.ProjectIds.Contains(a.ProjectId.Value)).ToList();

            if (searchCriteria.ItemTypeIds != null)
            {
                selectedArtifacts = selectedArtifacts.Where(a => searchCriteria.ItemTypeIds.Contains(a.ItemTypeId.Value)).ToList();
            }

            Assert.IsNotNull(selectedArtifacts, "No artifacts meet the search criteria!");

            var fullTextSearchTypeItems = new List<FullTextSearchTypeItem>();

            foreach (var artifactTypeId in selectedArtifacts.Select(a => a.ItemTypeId.Value).Distinct())
            {
                var firstOrDefault = selectedArtifacts.FirstOrDefault(a => a.ItemTypeId.Value == artifactTypeId);

                if (firstOrDefault != null)
                {
                    var fullTextSearchTypeItem = new FullTextSearchTypeItem
                    {
                        Count = selectedArtifacts.Count(a => a.ItemTypeId == artifactTypeId),
                        TypeName = firstOrDefault.ItemTypeName,
                        ItemTypeId = artifactTypeId
                    };

                    fullTextSearchTypeItems.Add(fullTextSearchTypeItem);
                }
            }

            var expectedFullTestSearchMetaDataResult = new FullTextSearchMetaDataResult
            {
                PageSize = (int)pageSize,
                TotalCount = selectedArtifacts.Count,
                TotalPages = (int)Math.Ceiling((decimal)selectedArtifacts.Count / (int)pageSize),
                Items = fullTextSearchTypeItems
            };

            return expectedFullTestSearchMetaDataResult;
        }

        /// <summary>
        ///  For permissions tests, asserts that returned searchResult from the FullTextSearchMetadata call matches what was expected.
        /// </summary>
        /// <param name="searchResult">The full text metadata search result</param>
        /// <param name="expectedHitCount">The expected number of hits resulting from the search</param>
        /// <param name="expectedTotalPageCount">The expected total page count returned by the search</param>
        /// <param name="expectedPageSize">(optional) The expected page size returned by the search.</param>
        private static void ValidateSearchMetaDataPermissionsTest(
            FullTextSearchMetaDataResult searchResult,
            int expectedHitCount,
            int expectedTotalPageCount,
            int expectedPageSize = DEFAULT_PAGE_SIZE_VALUE)
        {
            ThrowIf.ArgumentNull(searchResult, nameof(searchResult));

            Assert.AreEqual(expectedHitCount, searchResult.Items.Count(),
                "Returned Full text search type items count of {0} did not match expected count of {1}",
                searchResult.Items.Count(), expectedHitCount);
            Assert.AreEqual(expectedHitCount, searchResult.TotalCount,
                "The expected search hit count is {0} but {1} was returned.",
                expectedHitCount, searchResult.TotalCount);
            Assert.AreEqual(expectedTotalPageCount, searchResult.TotalPages,
                "The expected total page count is {0} but {1} was returned.",
                expectedTotalPageCount, searchResult.TotalPages);
            Assert.AreEqual(expectedPageSize, searchResult.PageSize,
                "The expected default pagesize value is {0} but {1} was found from the returned searchResult.",
                expectedPageSize, searchResult.PageSize);
        }

        #endregion Private Functions
    }
}
