using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SearchEngineLibrary.Model;
using SearchEngineLibrary.Repository;
using SearchEngineLibrary.Service;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Repositories;

namespace SearchEngineLibrary.Tests.Service
{
    [TestClass]
    public class SearchEngineServiceTests
    {
        private ISearchEngineService _searchEngineService;
        private Mock<ISearchEngineRepository> _searchEngineRepositoryMock;
        private Mock<IArtifactRepository> _sqlArtifactRepositoryMock;
        private const int ProjectId = 1;
        private const int ScopeId = 1;
        private const int UserId = 1;
        private Pagination _pagination;
        private SearchArtifactsResult _searchArtifactsResult;

        [TestInitialize]
        public void Init()
        {
            _searchEngineRepositoryMock = new Mock<ISearchEngineRepository>();
            _sqlArtifactRepositoryMock = new Mock<IArtifactRepository>();
            _searchEngineService = new SearchEngineService(_searchEngineRepositoryMock.Object, _sqlArtifactRepositoryMock.Object);
            _pagination = new Pagination { Limit = 10, Offset = 0 };
            _searchArtifactsResult = new SearchArtifactsResult { Total = 3, ArtifactIds = new List<int> { 1, 2, 3 } };
        }

        [TestMethod]
        public async Task Search_AllSearchItemsExists_ReturnedSearchArtifactsResult()
        {
            // arrange
            _sqlArtifactRepositoryMock
                .Setup(q => q.GetArtifactBasicDetails(ScopeId, UserId, null))
                .ReturnsAsync(new ArtifactBasicDetails
                {
                    ProjectId = ProjectId,
                    PrimitiveItemTypePredefined = (int)ItemTypePredefined.ArtifactCollection
                });
            _searchEngineRepositoryMock
                .Setup(q => q.GetCollectionContentSearchArtifactResults(ScopeId, ProjectId, _pagination, true, UserId, null))
                .ReturnsAsync(_searchArtifactsResult);

            // act
            var result = await _searchEngineService.Search(ScopeId, ProjectId, _pagination, ScopeType.Contents, true, UserId);

            // assert
            Assert.AreEqual(_searchArtifactsResult, result);
        }

        [TestMethod]
        public async Task Search_NotFoundArtifact_ResourceNotFoundException()
        {
            // arrange
            _sqlArtifactRepositoryMock
                .Setup(q => q.GetArtifactBasicDetails(ScopeId, UserId, null))
                .ReturnsAsync((ArtifactBasicDetails)null);
            _searchEngineRepositoryMock
                .Setup(q => q.GetCollectionContentSearchArtifactResults(ScopeId, ProjectId, _pagination, true, UserId, null))
                .ReturnsAsync(_searchArtifactsResult);
            var errorMessage = I18NHelper.FormatInvariant(ErrorMessages.ArtifactNotFound, ScopeId);
            var excectedResult = new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
            ResourceNotFoundException exception = null;

            // act
            try
            {
                await _searchEngineService.Search(ScopeId, ProjectId, _pagination, ScopeType.Contents, true, UserId);
            }
            catch (ResourceNotFoundException ex)
            {
                exception = ex;
            }

            // assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(excectedResult.Message, exception.Message);
        }
    }
}
