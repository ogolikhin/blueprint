using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nest;
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
            _elasticClient.Setup(e => e.IndexExists(It.IsAny<string>(),It.IsAny<Func<IndexExistsDescriptor, IIndexExistsRequest>>()))
                .Returns(mockIndexResponse.Object);

            _elasticSearchEngine = new ElasticSearchEngine(_fakeTenantId, _elasticClient.Object, _semanticSearchRepository.Object);
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
            mockExistsTrueResponse.SetupGet(m => m.Exists).Returns(false);

            _elasticClient.Setup(e => e.Ping(It.IsAny<Func<PingDescriptor, IPingRequest>>()))
                .Returns(mockResponse.Object);
            _elasticClient.Setup(e => e.IndexExists(It.IsAny<string>(), It.IsAny<Func<IndexExistsDescriptor, IIndexExistsRequest>>()))
                .Returns(mockExistsTrueResponse.Object);

            _elasticClient.Setup(e => e.TypeExists(It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<Func<TypeExistsDescriptor, ITypeExistsRequest>>()))
                .Returns(mockExistsFalseResponse.Object);

            _elasticSearchEngine = new ElasticSearchEngine(_fakeTenantId, _elasticClient.Object, _semanticSearchRepository.Object);
        }
    }
}
