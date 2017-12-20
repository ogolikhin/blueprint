using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public async Task GetCountArtifactIdsFromSearchItemsAsync_WeHaveAllSearchItem_QueryReturnListArtifactIds()
        {
            // arrange
            var countArtifactIds = new List<int>() {1, 2, 3};
            _searchEngineRepositoryMock.Setup(q => q.GetArtifactIdsFromSearchItems()).ReturnsAsync(countArtifactIds);

            // act
            var result = await _searchEngineService.GetArtifactIdsFromSearchItems();

            // assert
            Assert.AreEqual(countArtifactIds, result);
        }
    }
}
