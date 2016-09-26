﻿using System.Collections.Generic;
using CustomAttributes;
using Helper;
using Model;
using NUnit.Framework;
using System.Linq;
using Model.ArtifactModel;
using Model.Factories;
using Model.FullTextSearchModel.Impl;
using TestCommon;

namespace SearchServiceTests
{
    [TestFixture]
    [Category(Categories.SearchService)]
    public class FullTextSearchMetadataTests : TestBase
    {
        private IUser _user = null;
        private List<IProject> _projects = null;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private List<IArtifactBase> _artifacts = null;
        const int DEFAULT_PAGE_VALUE = 1;
        const int DEFAULT_PAGESIZE_VALUE = 10;


        [TestFixtureSetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _projects = ProjectFactory.GetAllProjects(_user, true);
            _artifacts = SearchServiceTestHelper.SetupSearchData(_projects, _user, Helper);
        }

        [TestFixtureTearDown]
        public void TearDown()
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
            Assert.That(fullTextSearchMetaDataResult.TotalCount.Equals(expectedHitCount), "The expected search hit count is {0} but {1} was returned.", DEFAULT_PAGE_VALUE, fullTextSearchMetaDataResult.TotalCount);
            Assert.That(fullTextSearchMetaDataResult.TotalPages.Equals(expectedTotalPageCount), "The expected total page count is {0} but {1} was returned.", DEFAULT_PAGE_VALUE, fullTextSearchMetaDataResult.TotalPages);
            Assert.That(fullTextSearchMetaDataResult.PageSize.Equals(DEFAULT_PAGESIZE_VALUE), "The expected default pagesize value is {0} but {1} was found from the returned searchResult.", DEFAULT_PAGESIZE_VALUE, fullTextSearchMetaDataResult.PageSize);
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
            Assert.That(fullTextSearchMetaDataResult.TotalCount.Equals(expectedHitCount), "The expected search hit count is {0} but {1} was returned.", DEFAULT_PAGE_VALUE, fullTextSearchMetaDataResult.TotalCount);
            Assert.That(fullTextSearchMetaDataResult.TotalPages.Equals(expectedTotalPageCount), "The expected total page count is {0} but {1} was returned.", DEFAULT_PAGE_VALUE, fullTextSearchMetaDataResult.TotalPages);
            Assert.That(fullTextSearchMetaDataResult.PageSize.Equals(requestedPageSize), "The expected pagesize value is {0} but {1} was found from the returned searchResult.", requestedPageSize, fullTextSearchMetaDataResult.PageSize);
        }

        #endregion 200 OK Tests

        #region 400 Bad Request Tests
        #endregion 400 Bad Request Tests

        #region 401 Unauthorized Tests
        #endregion 401 Unauthorized Tests

        #region 404 Not Found Tests
        #endregion 404 Not Found Tests

        #region 409 Conflict Tests
        #endregion 409 Conflict Tests

        #region Private Functions
        #endregion Private Functions

    }
}
