using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngineLibrary.Repository;
using ServiceLibrary.Repositories;

namespace SearchEngineLibrary.Tests.Repository
{
    [TestClass]
    public class SearchEngineRepositoryTests
    {
        private ISearchEngineRepository _searchEngineRepository;
        private SqlConnectionWrapperMock _sqlConnectionWrapperMock;

        [TestInitialize]
        public void Init()
        {            
            _sqlConnectionWrapperMock = new SqlConnectionWrapperMock();

            _searchEngineRepository = new SearchEngineRepository(_sqlConnectionWrapperMock.Object);
        }

        [TestMethod]
        public async Task GetListArtifactIdsAsync_AllSearchItemsExists_QueryReturnListArtifactIds()
        {
            // arrange
            var listArtifactIds = new List<int> {1, 2, 3};
            
            _sqlConnectionWrapperMock.SetupQueryAsync(@"SELECT DISTINCT([ArtifactId]) FROM [dbo].[SearchItems]", null, listArtifactIds, commandType:CommandType.Text);

            // act
            var result = await _searchEngineRepository.GetArtifactIds();

            // assert
            _sqlConnectionWrapperMock.Verify();
            Assert.AreEqual(result, listArtifactIds);
        }
    }
}
