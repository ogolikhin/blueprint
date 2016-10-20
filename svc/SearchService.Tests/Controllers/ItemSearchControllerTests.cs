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
    public class ItemSearchControllerTests
    {
        #region SearchFullText

        [TestMethod]
        public async Task SearchFullText_PageSizeNegative_ReturnsConstant()
        {
            // Arrange
            var configuration = new Mock<ISearchConfiguration>();
            var controller = initializeController(configuration);
            var searchCriteria = new SearchCriteria { Query = "empty", ProjectIds = new [] { 1 } };

            // Act
            var actionResult = await controller.SearchFullText(searchCriteria, null, -1);
            
            // Assert
            var result = actionResult as OkNegotiatedContentResult<FullTextSearchResult>;
            Assert.IsNotNull(result, "Result was not retrieved");
            Assert.AreEqual(result.Content.PageSize, ServiceConstants.SearchPageSize);
        }

        [TestMethod]
        public async Task SearchFullText_SearchCriteriaIsNull_ThrowsBadRequestException()
        {
            // Arrange
            var configuration = new Mock<ISearchConfiguration>();
            var controller = initializeController(configuration);
            var searchCriteria = new SearchCriteria { Query = null, ProjectIds = new [] { 1 } };
            BadRequestException badRequestException = null;
            // Act
            try
            {
                await controller.SearchFullText(searchCriteria, null, -1);
            }
            catch (BadRequestException e)
            {
                badRequestException = e;
            }

            // Assert
            Assert.IsNotNull(badRequestException, "Bad Request Exception should have been thrown");
            Assert.AreEqual(ErrorCodes.IncorrectSearchCriteria, badRequestException.ErrorCode, "IncorrectSearchCriteria should be provided as Error code");
        }

        [TestMethod]
        public async Task SearchFullText_SearchCriteriaIsEmpty_ThrowsBadRequestException()
        {
            // Arrange
            var configuration = new Mock<ISearchConfiguration>();
            var controller = initializeController(configuration);
            var searchCriteria = new SearchCriteria { Query = "", ProjectIds = new [] { 1 } };
            BadRequestException badRequestException = null;
            // Act
            try
            {
                await controller.SearchFullText(searchCriteria, null, -1);
            }
            catch (BadRequestException e)
            {
                badRequestException = e;
            }

            // Assert
            Assert.IsNotNull(badRequestException, "Bad Request Exception should have been thrown");
            Assert.AreEqual(ErrorCodes.IncorrectSearchCriteria, badRequestException.ErrorCode, "IncorrectSearchCriteria should be provided as Error code");
        }

        [TestMethod]
        public async Task SearchFullText_SearchCriteriaIsLessThanThreeCharacters_ThrowsBadRequestException()
        {
            // Arrange
            var configuration = new Mock<ISearchConfiguration>();
            var controller = initializeController(configuration);
            var searchCriteria = new SearchCriteria { Query = "12", ProjectIds = new [] { 1 } };
            BadRequestException badRequestException = null;
            // Act
            try
            {
                await controller.SearchFullText(searchCriteria, null, -1);
            }
            catch (BadRequestException e)
            {
                badRequestException = e;
            }

            // Assert
            Assert.IsNotNull(badRequestException, "Bad Request Exception should have been thrown");
            Assert.AreEqual(ErrorCodes.IncorrectSearchCriteria, badRequestException.ErrorCode, "IncorrectSearchCriteria should be provided as Error code");
        }

        [TestMethod]
        public async Task SearchFullText_ZeropPageSize_ReturnsConstant()
        {
            // Arrange
            var configuration = new Mock<ISearchConfiguration>();
            var controller = initializeController(configuration);
            var searchCriteria = new SearchCriteria { Query = "empty", ProjectIds = new[] { 1 } };

            // Act
            var actionResult = await controller.SearchFullText(searchCriteria, null, 0);

            // Assert
            var result = actionResult as OkNegotiatedContentResult<FullTextSearchResult>;
            Assert.IsNotNull(result, "Result was not retrieved");
            Assert.AreEqual(result.Content.PageSize, ServiceConstants.SearchPageSize);
        }

        [TestMethod]
        public async Task SearchFullText_50PageSize_ReturnsConstant()
        {
            // Arrange
            var configuration = new Mock<ISearchConfiguration>();
            var controller = initializeController(configuration);
            var searchCriteria = new SearchCriteria { Query = "empty", ProjectIds = new[] { 1 } };

            // Act
            var actionResult = await controller.SearchFullText(searchCriteria, null, 50);

            // Assert
            var result = actionResult as OkNegotiatedContentResult<FullTextSearchResult>;
            Assert.IsNotNull(result, "Result was not retrieved");
            Assert.AreEqual(result.Content.PageSize, 50);
        }

        [TestMethod]
        public async Task SearchFullText_RequestNegativePage_ReturnsPageOne()
        {
            // Arrange
            var configuration = new Mock<ISearchConfiguration>();
            var controller = initializeController(configuration);
            var searchCriteria = new SearchCriteria { Query = "empty", ProjectIds = new[] { 1 } };

            // Act
            var actionResult = await controller.SearchFullText(searchCriteria, -10, 1);

            // Assert
            var result = actionResult as OkNegotiatedContentResult<FullTextSearchResult>;
            Assert.IsNotNull(result, "Result was not retrieved");
            Assert.AreEqual(result.Content.Page, 1);
        }

        [TestMethod]
        public async Task SearchFullText_RequestZeroPage_ReturnsPageOne()
        {
            // Arrange
            var configuration = new Mock<ISearchConfiguration>();
            var controller = initializeController(configuration);
            var searchCriteria = new SearchCriteria { Query = "empty", ProjectIds = new[] { 1 } };

            // Act
            var actionResult = await controller.SearchFullText(searchCriteria, 0, 1);

            // Assert
            var result = actionResult as OkNegotiatedContentResult<FullTextSearchResult>;
            Assert.IsNotNull(result, "Result was not retrieved");
            Assert.AreEqual(result.Content.Page, 1);
        }

        [TestMethod]
        public async Task SearchFullText_RequestPositivePage_ReturnsPageOne()
        {
            // Arrange
            var configuration = new Mock<ISearchConfiguration>();
            var controller = initializeController(configuration);
            var searchCriteria = new SearchCriteria { Query = "empty", ProjectIds = new[] { 1 } };

            // Act
            var actionResult = await controller.SearchFullText(searchCriteria, 10, 1);

            // Assert
            var result = actionResult as OkNegotiatedContentResult<FullTextSearchResult>;
            Assert.IsNotNull(result, "Result was not retrieved");
            Assert.AreEqual(result.Content.Page, 10);
        }

        #endregion SearchFullText

        #region FullTextMetaData

        public async Task FullTextMetaData_PageSizeNegative_ReturnsConstant()
        {
            // Arrange
            var configuration = new Mock<ISearchConfiguration>();
            var controller = initializeController(configuration);
            var searchCriteria = new SearchCriteria { Query = "empty", ProjectIds = new [] { 1 } };

            // Act
            var actionResult = await controller.FullTextMetaData(searchCriteria, -1);

            // Assert
            var result = actionResult as OkNegotiatedContentResult<FullTextSearchResult>;
            Assert.IsNotNull(result, "Result was not retrieved");
            Assert.AreEqual(result.Content.PageSize, ServiceConstants.SearchPageSize);
        }

        #endregion FullTextMetaData

        #region SearchName

        [TestMethod]
        public async Task SearchName_Success()
        {
            // Arrange
            var searchCriteria = new ItemSearchCriteria { Query = "Test", ProjectIds = new List<int> { 5 } };
            var searchResult = new ItemSearchResult { PageItemCount = 1, SearchItems = new List<ItemSearchResultItem>() };
            var startOffset = 0;
            var pageSize = 20;
            var itemSearchRepositoryMock = new Mock<IItemSearchRepository>();
            itemSearchRepositoryMock.Setup(m => m.SearchName(1, searchCriteria, startOffset, pageSize)).ReturnsAsync(searchResult);
            var controller = new ItemSearchController(itemSearchRepositoryMock.Object, new SearchConfiguration())
            {
                Request = new HttpRequestMessage()
            };
            const int userId = 1;
            var session = new Session { UserId = userId };
            controller.Request.Properties[ServiceConstants.SessionProperty] = session;
            // Act
            var result = await controller.SearchName(searchCriteria, startOffset, pageSize);

            // Assert
            Assert.IsNotNull(result);
            var projectSearchResults = ((OkNegotiatedContentResult<ItemSearchResult>)result).Content;
            Assert.AreEqual(0, projectSearchResults.PageItemCount);
        }

        [TestMethod]
        public async Task SearchName_Forbidden()
        {
            // Arrange
            var searchCriteria = new ItemSearchCriteria { Query = "Test", ProjectIds = new List<int> { 5 } };
            var searchResult = new ItemSearchResult { PageItemCount = 1, SearchItems = new List<ItemSearchResultItem>() };
            var startOffset = 0;
            var pageSize = 20;
            var itemSearchRepositoryMock = new Mock<IItemSearchRepository>();
            itemSearchRepositoryMock.Setup(m => m.SearchName(1, searchCriteria, startOffset, pageSize)).ReturnsAsync(searchResult);
            var controller = new ItemSearchController(itemSearchRepositoryMock.Object, new SearchConfiguration())
            {
                Request = new HttpRequestMessage()
            };

            // Act
            try
            {
                await controller.SearchName(searchCriteria, startOffset, pageSize);
            }
            catch (AuthenticationException e)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.UnauthorizedAccess, e.ErrorCode);
            }
        }

        [TestMethod]
        public async Task SearchName_NullSerachCriteria_BadRequest()
        {
            // Arrange
            var itemSearchRepositoryMock = new Mock<IItemSearchRepository>();
            var controller = new ItemSearchController(itemSearchRepositoryMock.Object, new SearchConfiguration())
            {
                Request = new HttpRequestMessage()
            };
            const int userId = 1;
            var session = new Session { UserId = userId };
            controller.Request.Properties[ServiceConstants.SessionProperty] = session;

            // Act
            try
            {
                await controller.SearchName(null);
            }
            catch (BadRequestException e)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.IncorrectSearchCriteria, e.ErrorCode);
            }
        }

        [TestMethod]
        public async Task SearchName_QueryIsEmpty_BadRequest()
        {
            // Arrange
            var itemSearchRepositoryMock = new Mock<IItemSearchRepository>();
            var controller = new ItemSearchController(itemSearchRepositoryMock.Object, new SearchConfiguration())
            {
                Request = new HttpRequestMessage()
            };
            const int userId = 1;
            var session = new Session { UserId = userId };
            controller.Request.Properties[ServiceConstants.SessionProperty] = session;

            // Act
            try
            {
                await controller.SearchName(new ItemSearchCriteria { Query = "" });
            }
            catch (BadRequestException e)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.IncorrectSearchCriteria, e.ErrorCode);
            }
        }

        [TestMethod]
        public async Task SearchName_ResultCountMoreThanMax_Success()
        {
            // Arrange
            var searchCriteria = new ItemSearchCriteria { Query = "Test", ProjectIds = new List<int> { 5 } };
            var searchItems = new List<ItemSearchResultItem>();
            var searchResult = new ItemSearchResult { PageItemCount = 1, SearchItems = searchItems };
            var startOffset = 0;
            var pageSize = 200;
            var itemSearchRepositoryMock = new Mock<IItemSearchRepository>();
            itemSearchRepositoryMock.Setup(m => m.SearchName(1, searchCriteria, startOffset, ItemSearchController.MaxResultCount)).ReturnsAsync(searchResult);
            var controller = new ItemSearchController(itemSearchRepositoryMock.Object, new SearchConfiguration())
            {
                Request = new HttpRequestMessage()
            };
            const int userId = 1;
            var session = new Session { UserId = userId };
            controller.Request.Properties[ServiceConstants.SessionProperty] = session;

            // Act
            var result = await controller.SearchName(searchCriteria, startOffset, pageSize);

            // Assert
            Assert.IsNotNull(result);
        }

        private ItemSearchController initializeController(Mock<ISearchConfiguration> configuration)
        {
            var itemSearchRepositoryMock = new Mock<IItemSearchRepository>();
            itemSearchRepositoryMock.Setup(a => a.Search(It.IsAny<int>(), It.IsAny<SearchCriteria>(), It.IsAny<int>(), It.IsAny<int>())).
                Returns(Task.FromResult
                (
                    new FullTextSearchResult
                    {
                        FullTextSearchItems = new List<FullTextSearchItem>()
                    }
                ));
            var request = new HttpRequestMessage();
            request.Properties.Add("Session", new Session { UserId = 1 });

            return new ItemSearchController(itemSearchRepositoryMock.Object, configuration.Object)
            {
                Request = request
            };
        }

        #endregion SearchName
    }
}
