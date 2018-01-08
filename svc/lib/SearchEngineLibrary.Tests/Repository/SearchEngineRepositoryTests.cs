﻿using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngineLibrary.Repository;
using ServiceLibrary.Repositories;
using SearchEngineLibrary.Model;
using ServiceLibrary.Models;
using SearchEngineLibrary.Helpers;
using Moq;

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
            //_sqlConnectionWrapperMock.(@"SELECT DISTINCT([ArtifactId]) FROM [dbo].[SearchItems]", null, searchArtifactsResult, commandType: CommandType.Text);
            var query = QueryBuilder.GetArtifactIdsQuery(ScopeId, pagination, true, UserId);
            var dbConnection = _sqlConnectionWrapperMock.Object.CreateConnection();
            var readerMock = new Mock<IDataReader>(MockBehavior.Strict);
            object[][] values =
                {
                new object[] { "Total", 2}
            };



            // act
            var result = await _searchEngineRepository.GetArtifactIds(ScopeId, pagination, true, UserId);

            // assert
            _sqlConnectionWrapperMock.Verify();
            Assert.AreEqual(searchArtifactsResult, result);
        }
    }
}
