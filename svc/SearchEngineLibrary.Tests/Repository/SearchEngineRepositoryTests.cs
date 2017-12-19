using System;
using System.Collections.Generic;
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
        public async Task GetCountArtifactIdsFromSearchItemsAsync_WeHaveAllSearchItem_QueryReturnCountArtifactIds()
        {
            // arrange
            var countArtifactIds = 5;
            
            _sqlConnectionWrapperMock.SetupExecuteScalarAsync(v => true, It.IsAny<Dictionary<string, object>>(), countArtifactIds);

            // act
            var result = await _searchEngineRepository.GetCountArtifactIdsSearchItems();

            // assert
            _sqlConnectionWrapperMock.Verify();
            Assert.AreEqual(result, countArtifactIds);
        }
    }
}
