using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngineLibrary.Repository;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;
using Moq;
using SearchEngineLibrary.Model;

namespace SearchEngineLibrary.Tests.Repository
{
    [TestClass]
    public class SearchEngineRepositoryTests
    {
        private ISearchEngineRepository _searchEngineRepository;
        private SqlConnectionWrapperMock _sqlConnectionWrapperMock;
        private Mock<ISqlHelper> _sqlHelperMock;

        [TestInitialize]
        public void Init()
        {            
            _sqlConnectionWrapperMock = new SqlConnectionWrapperMock();
            _sqlHelperMock = new Mock<ISqlHelper>();

            _searchEngineRepository = new SearchEngineRepository(_sqlConnectionWrapperMock.Object, _sqlHelperMock.Object);
        }

        [TestMethod]
        public async Task GetCountArtifactIdsFromSearchItemsAsync_WeHaveAllSearchItem_QueryReturnListArtifactIds()
        {
            // arrange
            var countArtifactIds = new List<int>() {1};
            
            _sqlConnectionWrapperMock.SetupQueryAsync(@"SELECT DISTINCT([ArtifactId]) FROM [dbo].[SearchItems]", null, countArtifactIds, commandType:CommandType.Text);

            // act
            var result = await _searchEngineRepository.GetArtifactIdsFromSearchItems();

            // assert
            _sqlConnectionWrapperMock.Verify();
            Assert.AreEqual(result, countArtifactIds);
        }
    }
}
