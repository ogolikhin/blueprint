using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nest;
using SearchService.Models;
using SearchService.Repositories;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace SearchService.Helpers.SemanticSearch
{
    [TestClass]
    public class ElasticSearchEngineTests
    {
        private ISearchEngine _elasticSearchEngine;
        private Mock<ISemanticSearchRepository> _semanticSearchRepository;
        private Mock<IElasticClient> _elasticClient;

        [TestInitialize]
        public void Setup()
        {
            _elasticClient = new Mock<IElasticClient>();
            _semanticSearchRepository = new Mock<ISemanticSearchRepository>();
        }

        [TestMethod]
        [ExpectedException(typeof(ElasticsearchConfigurationException))]
        public void PerformHealthCheck_WhenCannotPing_ThrowsException()
        {
            var mockResponse = new Mock<IPingResponse>();
            mockResponse.SetupGet(m => m.IsValid).Returns(false);

            _elasticClient.Setup(e => e.Ping(It.IsAny<Func<PingDescriptor, IPingRequest>>()))
                .Returns(mockResponse.Object);

            _elasticSearchEngine = new ElasticSearchEngine(_elasticClient.Object, _semanticSearchRepository.Object);

            _elasticSearchEngine.PerformHealthCheck();
        }

        [TestMethod]
        [ExpectedException(typeof(ElasticsearchConfigurationException))]
        public async Task GetSemanticSearchSuggestions_WhenIndexNotExists_ThrowsException()
        {
            SetupIndexAndTypeExists(false, false);

            _semanticSearchRepository.Setup(s => s.GetSemanticSearchIndex()).ReturnsAsync("test");

            _elasticSearchEngine = new ElasticSearchEngine(_elasticClient.Object, _semanticSearchRepository.Object);

            await _elasticSearchEngine.GetSemanticSearchSuggestions(new SearchEngineParameters(1, 1, true, null));
        }

        [TestMethod]
        [ExpectedException(typeof(ElasticsearchConfigurationException))]
        public async Task GetSemanticSearchSuggestions_WhenTypeNotExists_ThrowsException()
        {

            SetupIndexAndTypeExists(true, false);

            _semanticSearchRepository.Setup(s => s.GetSemanticSearchIndex()).ReturnsAsync("test");

            _elasticSearchEngine = new ElasticSearchEngine(_elasticClient.Object, _semanticSearchRepository.Object);

            await _elasticSearchEngine.GetSemanticSearchSuggestions(new SearchEngineParameters(1, 1, true, null));
        }

        [TestMethod]
        public async Task GetSemanticSearchSuggestions_WhenIndexNotPopulated_ReturnsEmptyArtifactDetails()
        {
            var searchParameters = new SearchEngineParameters(1, 1, true, new HashSet<int>());
             _semanticSearchRepository.Setup(s => s.GetSemanticSearchIndex()).ReturnsAsync((string)null);

            _elasticSearchEngine = new ElasticSearchEngine(_elasticClient.Object, _semanticSearchRepository.Object);

            var result = await _elasticSearchEngine.GetSemanticSearchSuggestions(searchParameters);

            Assert.IsTrue(result.IsEmpty());
        }

        [TestMethod]
        public async Task GetSemanticSearchSuggestions_WhenQuerying_ReturnsArtifactDetails()
        {

            var searchParameters = new SearchEngineParameters(1, 1, true, new HashSet<int>());
            _semanticSearchRepository.Setup(s => s.GetSemanticSearchText(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new SemanticSearchText()
                {
                    Name = "test",
                    SearchText = "searchTest"
                });
            SetupIndexAndTypeExists(true, true);
            _semanticSearchRepository.Setup(s => s.GetSemanticSearchIndex()).ReturnsAsync("test");

            var searchItemHit = new Mock<IHit<SemanticSearchItem>>();
            searchItemHit.SetupGet(s => s.Id).Returns("2");
            var searchResponse = new Mock<ISearchResponse<SemanticSearchItem>>();
            searchResponse.SetupGet(s => s.Hits).Returns(new List<IHit<SemanticSearchItem>>() { searchItemHit.Object });

            var connectionSettings = new Mock<IConnectionSettingsValues>();
            connectionSettings.SetupGet(c => c.DefaultIndices).Returns(new FluentDictionary<Type, string>());
            _elasticClient.Setup(
                e => e.SearchAsync<SemanticSearchItem>(It.IsAny<ISearchRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(searchResponse.Object);

            _elasticClient.SetupGet(e => e.ConnectionSettings).Returns(connectionSettings.Object);

            _semanticSearchRepository.Setup(s => s.GetSuggestedArtifactDetails(It.IsAny<List<int>>(), It.IsAny<int>()))
                .ReturnsAsync(new List<ArtifactSearchResult>() { new ArtifactSearchResult() { ItemId = 2 } });

            _elasticSearchEngine = new ElasticSearchEngine(_elasticClient.Object, _semanticSearchRepository.Object);

            var result = await _elasticSearchEngine.GetSemanticSearchSuggestions(searchParameters);

            Assert.IsTrue(result.Count() == 1);
            Assert.IsTrue(result.FirstOrDefault().Id == 2);
        }

        private void SetupIndexAndTypeExists(bool expectIndexExist, bool expectTypeExist)
        {
            var mockExistsIndexResponse = new Mock<IExistsResponse>();
            mockExistsIndexResponse.SetupGet(m => m.Exists).Returns(expectIndexExist);

            var mockExistsTypeResponse = new Mock<IExistsResponse>();
            mockExistsTypeResponse.SetupGet(m => m.Exists).Returns(expectTypeExist);

            _elasticClient.Setup(e => e.IndexExists(It.IsAny<Indices>(), It.IsAny<Func<IndexExistsDescriptor, IIndexExistsRequest>>()))
                .Returns(mockExistsIndexResponse.Object);

            _elasticClient.Setup(e => e.TypeExists(It.IsAny<Indices>(), It.IsAny<Types>(), It.IsAny<Func<TypeExistsDescriptor, ITypeExistsRequest>>()))
                .Returns(mockExistsTypeResponse.Object);
        }
    }
}
