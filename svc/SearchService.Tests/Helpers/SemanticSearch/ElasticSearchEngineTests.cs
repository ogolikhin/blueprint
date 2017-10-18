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

namespace SearchService.Helpers.SemanticSearch
{
    [TestClass]
    public class ElasticSearchEngineTests
    {
        private ISearchEngine _elasticSearchEngine;
        private string _fakeTenantId = "FAKETENANT_1234567890";
        private Mock<ISemanticSearchRepository> _semanticSearchRepository;
        private Mock<IElasticClient> _elasticClient;

        [TestInitialize]
        public void Setup()
        {
            _elasticClient = new Mock<IElasticClient>();
            _semanticSearchRepository = new Mock<ISemanticSearchRepository>();

            //_elasticSearchEngine = new ElasticSearchEngine(_fakeTenantId, _elasticClient.Object, _semanticSearchRepository.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ElasticsearchConfigurationException))]
        public void PerformHealthCheck_WhenCannotPing_ThrowsException()
        {
            var mockResponse = new Mock<IPingResponse>();
            mockResponse.SetupGet(m => m.IsValid).Returns(false);

            _elasticClient.Setup(e => e.Ping(It.IsAny<Func<PingDescriptor, IPingRequest>>()))
                .Returns(mockResponse.Object);

            _elasticSearchEngine = new ElasticSearchEngine(_fakeTenantId, _elasticClient.Object, _semanticSearchRepository.Object);

            _elasticSearchEngine.PerformHealthCheck();
        }

        [TestMethod]
        [ExpectedException(typeof(ElasticsearchConfigurationException))]
        public void PerformHealthCheck_WhenIndexNotExists_ThrowsException()
        {
            var mockResponse = new Mock<IPingResponse>();
            mockResponse.SetupGet(m => m.IsValid).Returns(true);

            var mockIndexResponse = new Mock<IExistsResponse>();
            mockIndexResponse.SetupGet(m => m.Exists).Returns(false);

            _elasticClient.Setup(e => e.Ping(It.IsAny<Func<PingDescriptor, IPingRequest>>()))
                .Returns(mockResponse.Object);
            _elasticClient.Setup(e => e.IndexExists(It.IsAny<Indices>(), It.IsAny<Func<IndexExistsDescriptor, IIndexExistsRequest>>()))
                .Returns(mockIndexResponse.Object);

            _elasticSearchEngine = new ElasticSearchEngine(_fakeTenantId, _elasticClient.Object, _semanticSearchRepository.Object);

            _elasticSearchEngine.PerformHealthCheck();
        }

        [TestMethod]
        [ExpectedException(typeof(ElasticsearchConfigurationException))]
        public void PerformHealthCheck_WhenTypeNotExists_ThrowsException()
        {
            var mockResponse = new Mock<IPingResponse>();
            mockResponse.SetupGet(m => m.IsValid).Returns(true);

            var mockExistsTrueResponse = new Mock<IExistsResponse>();
            mockExistsTrueResponse.SetupGet(m => m.Exists).Returns(true);

            var mockExistsFalseResponse = new Mock<IExistsResponse>();
            mockExistsFalseResponse.SetupGet(m => m.Exists).Returns(false);

            _elasticClient.Setup(e => e.Ping(It.IsAny<Func<PingDescriptor, IPingRequest>>()))
                .Returns(mockResponse.Object);
            _elasticClient.Setup(e => e.IndexExists(It.IsAny<Indices>(), It.IsAny<Func<IndexExistsDescriptor, IIndexExistsRequest>>()))
                .Returns(mockExistsTrueResponse.Object);

            _elasticClient.Setup(e => e.TypeExists(It.IsAny<Indices>(), It.IsAny<Types>(), It.IsAny<Func<TypeExistsDescriptor, ITypeExistsRequest>>()))
                .Returns(mockExistsFalseResponse.Object);

            _elasticSearchEngine = new ElasticSearchEngine(_fakeTenantId, _elasticClient.Object, _semanticSearchRepository.Object);

            _elasticSearchEngine.PerformHealthCheck();
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

            var searchItem = new SemanticSearchItem() {ItemId = 2};
            var searchResponse = new Mock<ISearchResponse<SemanticSearchItem>>();
            searchResponse.SetupGet(s => s.Documents).Returns(new List<SemanticSearchItem>() {searchItem});

            _elasticClient.Setup(
                e => e.SearchAsync<SemanticSearchItem>(It.IsAny<ISearchRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(searchResponse.Object);
            
            _semanticSearchRepository.Setup(s => s.GetSuggestedArtifactDetails(It.IsAny<List<int>>(), It.IsAny<int>()))
                .ReturnsAsync(new List<ArtifactSearchResult>() { new ArtifactSearchResult() {ItemId = 2}});

            _elasticSearchEngine = new ElasticSearchEngine(_fakeTenantId, _elasticClient.Object, _semanticSearchRepository.Object);

            var result = await _elasticSearchEngine.GetSemanticSearchSuggestions(searchParameters);

            Assert.IsTrue(result.Count() == 1);
            Assert.IsTrue(result.FirstOrDefault().Id == 2);
        }
    }
}
