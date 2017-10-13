using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SearchService.Models;
using SearchService.Repositories;
using ServiceLibrary.Exceptions;

namespace SearchService.Helpers.SemanticSearch
{
    [TestClass]
    public class SearchEngineFactoryTests
    {
        private Mock<ISemanticSearchRepository> _semanticSearchRepository;

        [TestInitialize]
        public void Setup()
        {
            _semanticSearchRepository = new Mock<ISemanticSearchRepository>();
        }

        [TestMethod]
        [ExpectedException(typeof(SearchEngineNotFoundException))]
        public void CreateSearchEngine_WhenSettingsNull_ThrowsException()
        {
            _semanticSearchRepository.Setup(s => s.GetSemanticSearchSetting()).ReturnsAsync((SemanticSearchSetting)null);

            SearchEngineFactory.CreateSearchEngine(_semanticSearchRepository.Object);
        }


        [TestMethod]
        public void CreateSearchEngine_WhenSettingsElasticSearch_ReturnsElasticSearchEngine()
        {
            _semanticSearchRepository.Setup(s => s.GetSemanticSearchSetting()).ReturnsAsync(
                new SemanticSearchSetting()
                {
                    TenantId = "some id",
                    ConnectionString = "http://localhost",
                    SemanticSearchEngineType = SemanticSearchEngine.ElasticSearch
                });

            var searchEngine = SearchEngineFactory.CreateSearchEngine(_semanticSearchRepository.Object);

            Assert.IsInstanceOfType(searchEngine, typeof(ElasticSearchEngine));
        }

        [TestMethod]
        public void CreateSearchEngine_WhenSettingsSql_ReturnsSqlSearchEngine()
        {
            _semanticSearchRepository.Setup(s => s.GetSemanticSearchSetting()).ReturnsAsync(
                new SemanticSearchSetting()
                {
                    ConnectionString = "http://localhost",
                    SemanticSearchEngineType = SemanticSearchEngine.Sql
                });

            var searchEngine = SearchEngineFactory.CreateSearchEngine(_semanticSearchRepository.Object);

            Assert.IsInstanceOfType(searchEngine, typeof(SqlSearchEngine));
        }
    }
}
