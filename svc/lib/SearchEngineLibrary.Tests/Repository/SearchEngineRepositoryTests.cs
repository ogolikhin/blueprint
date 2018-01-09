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
        private readonly Pagination pagination = new Pagination() { Limit = 10, Offset = 0 };
        private readonly SearchArtifactsResult searchArtifactsResult = new SearchArtifactsResult() { Total = 3, ArtifactIds = new List<int> { 1, 2, 3 } };

        [TestInitialize]
        public void Init()
        {
            _sqlConnectionWrapperMock = new SqlConnectionWrapperMock();

            _searchEngineRepository = new SearchEngineRepository(_sqlConnectionWrapperMock.Object);
        }


        [TestMethod]
        public async Task GetArtifactIds_AllSearchItemsExists_ReturnedSearchArtifactsResult()
        {
            // arrange           
            var returnResult = new Tuple<IEnumerable<int>, IEnumerable<int>>(new int[] { searchArtifactsResult.Total }, searchArtifactsResult.ArtifactIds);
            _sqlConnectionWrapperMock.SetupQueryMultipleAsync(QueryBuilder.GetCollectionArtifactIds(ScopeId, pagination, true, UserId), null, returnResult, commandType: CommandType.Text);

            // act
            var result = await _searchEngineRepository.GetCollectionArtifactIds(ScopeId, pagination, true, UserId);

            // assert
            _sqlConnectionWrapperMock.Verify();
            Assert.AreEqual(searchArtifactsResult.Total, result.Total);
            Assert.AreEqual(searchArtifactsResult.ArtifactIds, result.ArtifactIds);
        }
    }
}
