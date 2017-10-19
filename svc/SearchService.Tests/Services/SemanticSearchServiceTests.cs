using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SearchService.Helpers.SemanticSearch;
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
        private Mock<ISemanticSearchRepository> _semanticSearchRepository ;
        private Mock<IArtifactPermissionsRepository> _artifactPermissionsRepository;
        private Mock<IUsersRepository> _usersRepository;
        private Mock<IArtifactRepository> _artifactRepository;


        [TestInitialize]
        public void Setup()
        {
            _semanticSearchRepository = new Mock<ISemanticSearchRepository>();
            _artifactPermissionsRepository = new Mock<IArtifactPermissionsRepository>();
            _usersRepository = new Mock<IUsersRepository>();
            _artifactRepository = new Mock<IArtifactRepository>();
            _semanticSearchService =  new SemanticSearchService(_semanticSearchRepository.Object, _artifactPermissionsRepository.Object, _usersRepository.Object, _artifactRepository.Object);
        }

        #region GetSemanticSearchSuggestions negative tests
        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetSemanticSearchSuggestions_WhenArtifactIdInvalid_ThrowsBadRequestException()
        {
            var parameters = new SemanticSearchSuggestionParameters(0, 1);

            await _semanticSearchService.GetSemanticSearchSuggestions(parameters, null);
        }
        #region GetSemanticSearchSuggestions - Different item type tests
        private async Task ExecuteItemTypeTests(ItemTypePredefined itemTypePredefined)
        {
            //arrange
            var parameters = new SemanticSearchSuggestionParameters(1, 1);

            _artifactRepository.Setup(a => a.GetArtifactBasicDetails(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new ArtifactBasicDetails()
                {
                    PrimitiveItemTypePredefined = (int)itemTypePredefined
                });

            //act
            await _semanticSearchService.GetSemanticSearchSuggestions(parameters, null);
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
            //arrange
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
                .ReturnsAsync( new Dictionary<int, RolePermissions>() { {1, RolePermissions.None} });

            //act
            await _semanticSearchService.GetSemanticSearchSuggestions(parameters, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetSemanticSearchSuggestions_WhenArtifactNotFound_ThrowsNotFoundException()
        {
            //arrange
            var parameters = new SemanticSearchSuggestionParameters(1, 1);

            _artifactRepository.Setup(a => a.GetArtifactBasicDetails(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((ArtifactBasicDetails)null);

            //act
            await _semanticSearchService.GetSemanticSearchSuggestions(parameters, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetSemanticSearchSuggestions_WhenArtifactIsLatestDeleted_ThrowsNotFoundException()
        {
            //arrange
            var parameters = new SemanticSearchSuggestionParameters(1, 1);

            _artifactRepository.Setup(a => a.GetArtifactBasicDetails(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new ArtifactBasicDetails() {LatestDeleted = true});

            //act
            await _semanticSearchService.GetSemanticSearchSuggestions(parameters, null);
        }
        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetSemanticSearchSuggestions_WhenArtifactIsDraftDeleted_ThrowsNotFoundException()
        {
            //arrange
            var parameters = new SemanticSearchSuggestionParameters(1, 1);

            _artifactRepository.Setup(a => a.GetArtifactBasicDetails(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new ArtifactBasicDetails() { DraftDeleted = true });

            //act
            await _semanticSearchService.GetSemanticSearchSuggestions(parameters, null);
        }


        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetSemanticSearchSuggestions_WhenSubartifactId_ThrowsBadRequestException()
        {
            //arrange
            var parameters = new SemanticSearchSuggestionParameters(1, 1);


            _artifactRepository.Setup(a => a.GetArtifactBasicDetails(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new ArtifactBasicDetails()
                {
                    PrimitiveItemTypePredefined = (int)ItemTypePredefined.Actor,
                    ArtifactId = 2,
                    ItemId = parameters.ArtifactId
                });

            //act
            await _semanticSearchService.GetSemanticSearchSuggestions(parameters, null);
        }
        #endregion

        #region GetSemanticSearchSuggestions positive tests
        [TestMethod]
        public async Task GetSemanticSearchSuggestions_WhenIsInstanceAdmin_DoesNotQueryAccessibleProjects()
        {
            //arrange
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
                .ReturnsAsync(new Dictionary<int, RolePermissions>() { { 1, RolePermissions.Read } });

            _usersRepository.Setup(u => u.IsInstanceAdmin(It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(true);
            
            GetSemanticSearchSuggestionsAsyncDelegate searchDelegate = async (searchEngineParameters) => await Task.FromResult(new List<ArtifactSearchResult>() { new ArtifactSearchResult() { ItemId = 1 } });

            //act
            await _semanticSearchService.GetSemanticSearchSuggestions(parameters, searchDelegate);

            //assert
            _semanticSearchRepository.Verify(s=>s.GetAccessibleProjectIds(It.IsAny<int>()), Times.Never);
        }

        [TestMethod]
        public async Task GetSemanticSearchSuggestions_WhenNotInstanceAdmin_QueriesAccessibleProjects()
        {
            //arrange
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
                .ReturnsAsync(new Dictionary<int, RolePermissions>() { { 1, RolePermissions.Read } });

            _usersRepository.Setup(u => u.IsInstanceAdmin(It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(false);

            _semanticSearchRepository.Setup(s => s.GetAccessibleProjectIds(It.IsAny<int>()))
                .ReturnsAsync(new List<int>());

            GetSemanticSearchSuggestionsAsyncDelegate searchDelegate = async (searchEngineParameters) => await Task.FromResult(new List<ArtifactSearchResult>() { new ArtifactSearchResult() { ItemId = 1 } });

            //act
            await _semanticSearchService.GetSemanticSearchSuggestions(parameters, searchDelegate);

            //assert
            _semanticSearchRepository.Verify(s => s.GetAccessibleProjectIds(It.IsAny<int>()), Times.Once);
        }
        
        [TestMethod]
        public async Task GetSemanticSearchSuggestions_WhenHasPermissionToResult_ReturnsArtifactWithReadPermissions()
        {
            //arrange
            var artifactIdWithPermissions = 2;
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
                .ReturnsAsync(new Dictionary<int, RolePermissions>() { { 1, RolePermissions.Read } });

            _usersRepository.Setup(u => u.IsInstanceAdmin(It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(false);

            _semanticSearchRepository.Setup(s => s.GetAccessibleProjectIds(It.IsAny<int>()))
                .ReturnsAsync(new List<int>());

            _artifactPermissionsRepository.Setup(
                s =>
                    s.GetArtifactPermissions(It.Is<IEnumerable<int>>(a=> a.FirstOrDefault() == artifactIdWithPermissions), It.IsAny<int>(), It.IsAny<bool>(),
                        It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(new Dictionary<int, RolePermissions>() {{2, RolePermissions.Read}});

            GetSemanticSearchSuggestionsAsyncDelegate searchDelegate = async (searchEngineParameters) => await Task.FromResult(new List<ArtifactSearchResult>() { new ArtifactSearchResult() { ItemId = artifactIdWithPermissions }} );

            //act
            var result = await _semanticSearchService.GetSemanticSearchSuggestions(parameters, searchDelegate);

            //assert
            Assert.IsTrue(result.Items.Count() == 1);
            Assert.IsTrue(result.Items.First().HasReadPermission);
        }

        [TestMethod]
        public async Task GetSemanticSearchSuggestions_WhenDoesNotHavePermissionToResult_ReturnsArtifactWithNoReadPermissions()
        {
            //arrange
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
                .ReturnsAsync(new Dictionary<int, RolePermissions>() { { 1, RolePermissions.Read } });

            _usersRepository.Setup(u => u.IsInstanceAdmin(It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(false);

            _semanticSearchRepository.Setup(s => s.GetAccessibleProjectIds(It.IsAny<int>()))
                .ReturnsAsync(new List<int>());

            _artifactPermissionsRepository.Setup(
                s =>
                    s.GetArtifactPermissions(It.IsIn(new List<int>() { 2 }), It.IsAny<int>(), It.IsAny<bool>(),
                        It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(new Dictionary<int, RolePermissions>() { { 2, RolePermissions.None } });

            GetSemanticSearchSuggestionsAsyncDelegate searchDelegate = async (searchEngineParameters) => await Task.FromResult(new List<ArtifactSearchResult>() { new ArtifactSearchResult() { ItemId = 2 } });

            //act
            var result = await _semanticSearchService.GetSemanticSearchSuggestions(parameters, searchDelegate);

            //assert
            Assert.IsTrue(result.Items.Count() == 1);
            Assert.IsFalse(result.Items.First().HasReadPermission);
        }
        #endregion
    }
}
