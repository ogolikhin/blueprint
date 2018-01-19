using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngineLibrary.Repository;
using ServiceLibrary.Repositories;
using SearchEngineLibrary.Model;
using ServiceLibrary.Models;
using SearchEngineLibrary.Helpers;
using Moq;
using System;

namespace SearchEngineLibrary.Tests.Repository
{
    [TestClass]
    public class SearchEngineRepositoryTests
    {
        private ISearchEngineRepository _searchEngineRepository;
        private SqlConnectionWrapperMock _sqlConnectionWrapperMock;
        private const int ScopeId = 1;
        private const int UserId = 1;
        private Pagination _pagination;
        private SearchArtifactsResult _searchArtifactsResult;

        [TestInitialize]
        public void Init()
        {
            _sqlConnectionWrapperMock = new SqlConnectionWrapperMock();
            _searchEngineRepository = new SearchEngineRepository(_sqlConnectionWrapperMock.Object);
            _pagination = new Pagination { Limit = 10, Offset = 0 };
            _searchArtifactsResult = new SearchArtifactsResult { Total = 3, ArtifactIds = new List<int> {1, 2, 3} };
        }


        [TestMethod]
        public async Task GetCollectionContentSearchArtifactResults_AllSearchItemsExists_ReturnedSearchArtifactsResult()
        {
            // arrange           
            var returnResult = new Tuple<IEnumerable<int>, IEnumerable<int>>(new int[] { _searchArtifactsResult.Total }, _searchArtifactsResult.ArtifactIds);
            _sqlConnectionWrapperMock.SetupQueryMultipleAsync(QueryBuilder.GetCollectionContentSearchArtifactResults(ScopeId, _pagination, true, UserId), null, returnResult, commandType: CommandType.Text);

            // act
            var result = await _searchEngineRepository.GetCollectionContentSearchArtifactResults(ScopeId, _pagination, true, UserId);

            // assert
            _sqlConnectionWrapperMock.Verify();
            Assert.AreEqual(_searchArtifactsResult.Total, result.Total);
            Assert.AreEqual(_searchArtifactsResult.ArtifactIds, result.ArtifactIds);
        }
    }
}
