using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.FullTextSearchModel.Impl;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using TestCommon;

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
        //private IProject _project = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            //_project = ProjectFactory.GetProject(_user);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #region 200 OK Tests

        [TestCase]
        [TestRail(166155)]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [Description("Searching without both optional parameters, page and pagesize. Execute Search - Must return SearchResult that uses FirstPage and DefaultPageSize")]
        public void FullTextSearch_SearchWithoutBothPageAndPageSize_VerifySearchResultUsesFirstPageAndDefaultPageSize()
        {
            FullTextSearchResult fullTextSearchResult = null;

            // Setup: Create searchable artifact(s) with unique search terms
            var projectIds = new List<int>() { 2, 4 };
            FullTextSearchCriteria searchCriteria = new FullTextSearchCriteria("NonExistingSearchTerm", projectIds);

            // Execute: Execute FullTextSearch with search terms
            Assert.DoesNotThrow(() => fullTextSearchResult = Helper.FullTextSearch.Search(_user, searchCriteria), "Search() call failed when using following search term: {0}artifact(s)!", searchCriteria.Query);

            // Validation: Verify that searchResult uses FirstPage and DefaultPageSize
            Assert.That(fullTextSearchResult.Page.Equals(DEFAULT_PAGE_VALUE), "The expected default page value is {0} but {1} was found from the returned searchResult.", DEFAULT_PAGE_VALUE, fullTextSearchResult.Page);
            Assert.That(fullTextSearchResult.PageSize.Equals(DEFAULT_PAGESIZE_VALUE), "The expected default pagesize value is {0} but {1} was found from the returned searchResult.", DEFAULT_PAGESIZE_VALUE, fullTextSearchResult.PageSize);
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
        /// <summary>
        /// Asserts that returned searchResult from the FullTextSearch call match with artifacts that are being searched.
        /// </summary>
        /// <param name="discardArtifactResponse">The response from Nova discard call.</param>
        /// <param name="artifactsTodiscard">artifacts that are being discarded</param>
        //private static void FullTextSearchResultValidation()
        //{

        //}
        #endregion Private Functions

    }
}
