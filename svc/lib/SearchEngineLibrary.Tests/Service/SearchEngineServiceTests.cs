using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SearchEngineLibrary.Repository;
using SearchEngineLibrary.Service;
using ServiceLibrary.Repositories;

namespace SearchEngineLibrary.Tests.Service
{
    [TestClass]
    public class SearchEngineServiceTests
    {
        private ISearchEngineService _searchEngineService;
        private Mock<ISearchEngineRepository> _searchEngineRepositoryMock;
        private Mock<IArtifactRepository> _sqlArtifactRepositoryMock;

        [TestInitialize]
        public void Init()
        {
            _searchEngineRepositoryMock = new Mock<ISearchEngineRepository>();
            _sqlArtifactRepositoryMock = new Mock<IArtifactRepository>();
            _searchEngineService = new SearchEngineService(_searchEngineRepositoryMock.Object, _sqlArtifactRepositoryMock.Object);
        }       
    }
}
