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
        public async Task GetCountArtifactIdsFromSearchItemsAsync_WeHaveAllSearchItem_QueryReturnCountArtifactIds()
        {
            // arrange
            var countArtifactIds = 5;
            _searchEngineRepositoryMock.Setup(q => q.GetCountArtifactIdsSearchItems()).ReturnsAsync(countArtifactIds);

            // act
            var result = await _searchEngineService.GetCountArtifactIdsSearchItems();

            // assert
            Assert.AreEqual(countArtifactIds, result);
        }
    }
}
