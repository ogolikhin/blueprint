using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Results;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SearchService.Helpers;
using SearchService.Models;
using SearchService.Repositories;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace SearchService.Controllers
{
    [TestClass]
    public class ItemNameSearchControllerTests
    {
        private Mock<IItemSearchRepository> _itemSearchRepositoryMock;

        private Session _session;

        [TestInitialize]
        public void Initialize()
        {
            // Arrange
            const int userId = 1;
            _session = new Session { UserId = userId };

            _itemSearchRepositoryMock = new Mock<IItemSearchRepository>();
        }

        [TestMethod]
        public async Task FindArtifactName_Success()
        {
            // Arrange
            var searchCriteria = new ItemSearchCriteria {Query = "Test", ProjectIds = new List<int> {5} };
            var searchResult = new ItemSearchResult { PageItemCount = 1, SearchItems = new List<ItemSearchResultItem>()};
            var startOffset = 0;
            var pageSize = 20;
            _itemSearchRepositoryMock.Setup(m => m.SearchName(1, searchCriteria, startOffset, pageSize)).ReturnsAsync(searchResult);
            var controller = new ItemNameSearchController(_itemSearchRepositoryMock.Object, new SearchConfiguration())
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties[ServiceConstants.SessionProperty] = _session;
            // Act
            var result = await controller.Post(searchCriteria, startOffset, pageSize);

            // Assert
            Assert.IsNotNull(result);
            var projectSearchResults = ((OkNegotiatedContentResult<ItemSearchResult>)result).Content;
            Assert.AreEqual(0, projectSearchResults.PageItemCount);
        }

        [TestMethod]
        public async Task FindArtifactName_Forbidden()
        {
            // Arrange
            var searchCriteria = new ItemSearchCriteria { Query = "Test", ProjectIds = new List<int> { 5 } };
            var searchResult = new ItemSearchResult { PageItemCount = 1, SearchItems = new List<ItemSearchResultItem>() };
            var startOffset = 0;
            var pageSize = 20;
            _itemSearchRepositoryMock.Setup(m => m.SearchName(1, searchCriteria, startOffset, pageSize)).ReturnsAsync(searchResult);
            var controller = new ItemNameSearchController(_itemSearchRepositoryMock.Object, new SearchConfiguration())
            {
                Request = new HttpRequestMessage()
            };

            // Act
            try
            {
                await controller.Post(searchCriteria, startOffset, pageSize);
            }
            catch (AuthenticationException e)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.UnauthorizedAccess, e.ErrorCode);
            }
        }

        [TestMethod]
        public async Task FindArtifactName_NullSerachCriteria_BadRequest()
        {
            // Arrange
            var controller = new ItemNameSearchController(_itemSearchRepositoryMock.Object, new SearchConfiguration())
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties[ServiceConstants.SessionProperty] = _session;

            // Act
            try
            {
                await controller.Post(null);
            }
            catch (BadRequestException e)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.IncorrectSearchCriteria, e.ErrorCode);
            }
        }

        [TestMethod]
        public async Task FindArtifactName_QueryIsEmpty_BadRequest()
        {
            // Arrange
            var controller = new ItemNameSearchController(_itemSearchRepositoryMock.Object, new SearchConfiguration())
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties[ServiceConstants.SessionProperty] = _session;

            // Act
            try
            {
                await controller.Post(new ItemSearchCriteria { Query = "" });
            }
            catch (BadRequestException e)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.IncorrectSearchCriteria, e.ErrorCode);
            }
        }

        [TestMethod]
        public async Task FindArtifactName_ResultCountMoreThanMax_Success()
        {
            // Arrange
            var searchCriteria = new ItemSearchCriteria { Query = "Test", ProjectIds = new List<int> { 5 } };
            var searchItems = new List<ItemSearchResultItem>();
            var searchResult = new ItemSearchResult { PageItemCount = 1, SearchItems = searchItems };
            var startOffset = 0;
            var pageSize = 200;
            _itemSearchRepositoryMock.Setup(m => m.SearchName(1, searchCriteria, startOffset, ItemNameSearchController.MaxResultCount)).ReturnsAsync(searchResult);
            var controller = new ItemNameSearchController(_itemSearchRepositoryMock.Object, new SearchConfiguration())
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties[ServiceConstants.SessionProperty] = _session;

            // Act
            var result = await controller.Post(searchCriteria, startOffset, pageSize);

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
