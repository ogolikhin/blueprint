﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SearchService.Models;
using SearchService.Repositories;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;

namespace SearchService.Services
{
    [TestClass]
    public class SemanticSearchServiceTests
    {
        private ISemanticSearchService _semanticSearchService;
        private Mock<ISemanticSearchRepository> _semanticSearchRepository;
        private Mock<IArtifactPermissionsRepository> _artifactPermissionsRepository;
        private Mock<IUsersRepository> _usersRepository;
        private Mock<ISqlArtifactRepository> _artifactRepository;


        [TestInitialize]
        public void Setup()
        {
            _semanticSearchRepository = new Mock<ISemanticSearchRepository>();
            _artifactPermissionsRepository = new Mock<IArtifactPermissionsRepository>();
            _usersRepository = new Mock<IUsersRepository>();
            _artifactRepository = new Mock<ISqlArtifactRepository>();
            _semanticSearchService = new SemanticSearchService(_semanticSearchRepository.Object, _artifactPermissionsRepository.Object, _usersRepository.Object, _artifactRepository.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetSemanticSearchSuggestions_WhenArtifactIdInvalid_ThrowsBadRequestException()
        {
            var parameters = new SemanticSearchSuggestionParameters(0, 1);

            await _semanticSearchService.GetSemanticSearchSuggestions(parameters);
        }
        #region GetSemanticSearchSuggestions - Different item type tests
        private async Task ExecuteItemTypeTests(ItemTypePredefined itemTypePredefined)
        {
            // arrange
            var parameters = new SemanticSearchSuggestionParameters(1, 1);

            _artifactRepository.Setup(a => a.GetArtifactBasicDetails(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new ArtifactBasicDetails()
                {
                    PrimitiveItemTypePredefined = (int)itemTypePredefined
                });

            // act
            await _semanticSearchService.GetSemanticSearchSuggestions(parameters);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetSemanticSearchSuggestions_WhenItemTypeIsFolder_ThrowsBadRequestException()
        {
            await ExecuteItemTypeTests(ItemTypePredefined.PrimitiveFolder);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetSemanticSearchSuggestions_WhenItemTypeIsBaseline_ThrowsBadRequestException()
        {
            await ExecuteItemTypeTests(ItemTypePredefined.Baseline);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetSemanticSearchSuggestions_WhenItemTypeIsReview_ThrowsBadRequestException()
        {
            await ExecuteItemTypeTests(ItemTypePredefined.ArtifactReviewPackage);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetSemanticSearchSuggestions_WhenItemTypeIsProject_ThrowsBadRequestException()
        {
            await ExecuteItemTypeTests(ItemTypePredefined.Project);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetSemanticSearchSuggestions_WhenItemTypeIsSubartifact_ThrowsBadRequestException()
        {
            await ExecuteItemTypeTests(ItemTypePredefined.PROShape);
        }
        #endregion

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetSemanticSearchSuggestions_WhenNoPermissions_ThrowsAuthorizationException()
        {
            // arrange
            var parameters = new SemanticSearchSuggestionParameters(1, 1);

            _artifactRepository.Setup(a => a.GetArtifactBasicDetails(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new ArtifactBasicDetails()
                {
                    PrimitiveItemTypePredefined = (int)ItemTypePredefined.Actor
                });
            _artifactPermissionsRepository.Setup(
                a =>
                    a.GetArtifactPermissions(It.IsAny<IEnumerable<int>>(), It.IsAny<int>(), It.IsAny<bool>(),
                        It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(new Dictionary<int, RolePermissions>() { { 1, RolePermissions.None } });

            // act
            await _semanticSearchService.GetSemanticSearchSuggestions(parameters);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetSemanticSearchSuggestions_WhenArtifactNotFound_ThrowsNotFoundException()
        {
            // arrange
            var parameters = new SemanticSearchSuggestionParameters(1, 1);

            _artifactRepository.Setup(a => a.GetArtifactBasicDetails(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((ArtifactBasicDetails)null);

            // act
            await _semanticSearchService.GetSemanticSearchSuggestions(parameters);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetSemanticSearchSuggestions_WhenArtifactIsLatestDeleted_ThrowsNotFoundException()
        {
            // arrange
            var parameters = new SemanticSearchSuggestionParameters(1, 1);

            _artifactRepository.Setup(a => a.GetArtifactBasicDetails(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new ArtifactBasicDetails() { LatestDeleted = true });

            // act
            await _semanticSearchService.GetSemanticSearchSuggestions(parameters);
        }
        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetSemanticSearchSuggestions_WhenArtifactIsDraftDeleted_ThrowsNotFoundException()
        {
            // arrange
            var parameters = new SemanticSearchSuggestionParameters(1, 1);

            _artifactRepository.Setup(a => a.GetArtifactBasicDetails(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new ArtifactBasicDetails() { DraftDeleted = true });

            // act
            await _semanticSearchService.GetSemanticSearchSuggestions(parameters);
        }


        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetSemanticSearchSuggestions_WhenSubartifactId_ThrowsBadRequestException()
        {
            // arrange
            var parameters = new SemanticSearchSuggestionParameters(1, 1);


            _artifactRepository.Setup(a => a.GetArtifactBasicDetails(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new ArtifactBasicDetails()
                {
                    PrimitiveItemTypePredefined = (int)ItemTypePredefined.Actor,
                    ArtifactId = 2,
                    ItemId = parameters.ArtifactId
                });

            // act
            await _semanticSearchService.GetSemanticSearchSuggestions(parameters);
        }
    }
}
