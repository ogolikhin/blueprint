﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SearchEngineLibrary.Repository;
using SearchEngineLibrary.Service;
using ServiceLibrary.Repositories;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Helpers;
using ServiceLibrary.Exceptions;
using System;

namespace SearchEngineLibrary.Tests.Service
{
    [TestClass]
    public class SearchEngineServiceTests
    {
        private ISearchEngineService _searchEngineService;
        private Mock<ISearchEngineRepository> _searchEngineRepositoryMock;
        private Mock<IArtifactRepository> _sqlArtifactRepositoryMock;
        private const int ScopeId = 1;
        private const int UserId = 1;
        private readonly Pagination pagination = new Pagination() { Limit = 10, Offset = 0 };

        [TestInitialize]
        public void Init()
        {
            _searchEngineRepositoryMock = new Mock<ISearchEngineRepository>();
            _sqlArtifactRepositoryMock = new Mock<IArtifactRepository>();
            _searchEngineService = new SearchEngineService(_searchEngineRepositoryMock.Object, _sqlArtifactRepositoryMock.Object);         
        }

        [TestMethod]
        public async Task SearchArtifactIds_AllSearchItemsExists_ReturnListArtifactIds()
        {
            // arrange
            IEnumerable<int> listArtifactIds = new List<int> { 1, 2, 3 };
            _sqlArtifactRepositoryMock.Setup(q => q.GetArtifactBasicDetails(ScopeId, UserId)).ReturnsAsync(new ArtifactBasicDetails() { PrimitiveItemTypePredefined = (int)ItemTypePredefined.ArtifactCollection });
            _searchEngineRepositoryMock.Setup(q => q.GetArtifactIds(ScopeId, pagination, ScopeType.Contents, true, UserId)).ReturnsAsync(listArtifactIds);

            // act
            var result = await _searchEngineService.SearchArtifactIds(ScopeId, pagination, ScopeType.Contents, true, UserId);

            // assert
            Assert.AreEqual(listArtifactIds, result);
        }

        [TestMethod]
        public async Task SearchArtifactIds_NotFoundArtifactByScopeId_ResourceNotFoundException()
        {
            // arrange
            IEnumerable<int> listArtifactIds = new List<int> { 1, 2, 3 };
            ArtifactBasicDetails artifactBasicDetails = null;
            _sqlArtifactRepositoryMock.Setup(q => q.GetArtifactBasicDetails(ScopeId, UserId)).ReturnsAsync(artifactBasicDetails);
            _searchEngineRepositoryMock.Setup(q => q.GetArtifactIds(ScopeId, pagination, ScopeType.Contents, true, UserId)).ReturnsAsync(listArtifactIds);
            var errorMessage = I18NHelper.FormatInvariant(ErrorMessages.ArtifactNotFound, ScopeId);
            var excectedResult = new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
            ResourceNotFoundException exception = null;

            // act
            try
            {
                await _searchEngineService.SearchArtifactIds(ScopeId, pagination, ScopeType.Contents, true, UserId);
            }
            catch(ResourceNotFoundException ex)
            {
                exception = ex;
            }

            // assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(excectedResult.Message, exception.Message);
        }

        [TestMethod]
        public async Task SearchArtifactIds_FoundArtifactIsNotCollection_NotImplementedException()
        {
            // arrange
            IEnumerable<int> listArtifactIds = new List<int> { 1, 2, 3 };
            _sqlArtifactRepositoryMock.Setup(q => q.GetArtifactBasicDetails(ScopeId, UserId)).ReturnsAsync(new ArtifactBasicDetails() { PrimitiveItemTypePredefined = (int)ItemTypePredefined.Actor });
            _searchEngineRepositoryMock.Setup(q => q.GetArtifactIds(ScopeId, pagination, ScopeType.Contents, true, UserId)).ReturnsAsync(listArtifactIds);
            var exceptedResult= new NotImplementedException(ErrorMessages.NotImplementedForNotCollection);
            NotImplementedException exception = null;

            // act
            try
            {
                await _searchEngineService.SearchArtifactIds(ScopeId, pagination, ScopeType.Contents, true, UserId);
            }
            catch (NotImplementedException ex)
            {
                exception = ex;
            }

            // assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(exceptedResult.Message, exception.Message);
        }

    }
}
