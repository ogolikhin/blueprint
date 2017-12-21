using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SearchEngineLibrary.Repository;
using SearchEngineLibrary.Service;

namespace SearchEngineLibrary.Tests.Service
{
    [TestClass]
    public class SearchEngineServiceTests
    {
        private ISearchEngineService _searchEngineService;
        private Mock<ISearchEngineRepository> _searchEngineRepositoryMock;

        [TestInitialize]
        public void Init()
        {
            _searchEngineRepositoryMock = new Mock<ISearchEngineRepository>();
            _searchEngineService = new SearchEngineService(_searchEngineRepositoryMock.Object);
        }

        [TestMethod]
        public async Task GetListArtifactIdsFromSearchItemsAsync_AllSearchItemsExists_QueryReturnListArtifactIds()
        {
            // arrange
            var listArtifactIds = new List<int> {1, 2, 3};
            _searchEngineRepositoryMock.Setup(q => q.GetArtifactIds()).ReturnsAsync(listArtifactIds);

            // act
            var result = await _searchEngineService.GetArtifactIds();

            // assert
            Assert.AreEqual(listArtifactIds, result);
        }
    }
}
