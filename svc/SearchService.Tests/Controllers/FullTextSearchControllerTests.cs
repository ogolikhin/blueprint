using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SearchService.Repositories;
using System.Net.Http;
using ServiceLibrary.Models;
using SearchService.Models;
using System.Web.Http.Results;
using ServiceLibrary.Helpers;
using SearchService.Helpers;
using ServiceLibrary.Exceptions;

namespace SearchService.Controllers
{
    [TestClass]
    public class FullTextSearchControllerTests
    {
        #region POST Tests

        [TestMethod]
        public async Task Post_PageSizeNegative_ReturnsConstant()
        {
            // Arrange
            var configuration = new Mock<ISearchConfiguration>();
            var controller = initializeController(configuration);
            var searchCriteria = new SearchCriteria { Query = "empty", ProjectIds = new [] { 1 } };

            // Act
            var actionResult = await controller.Post(searchCriteria, null, -1);
            
            // Assert
            var result = actionResult as OkNegotiatedContentResult<FullTextSearchResult>;
            Assert.IsNotNull(result, "Result was not retrieved");
            Assert.AreEqual(result.Content.PageSize, ServiceConstants.SearchPageSize);
        }

        [TestMethod]
        public async Task Post_SearchCriteriaIsNull_ThrowsBadRequestException()
        {
            // Arrange
            var configuration = new Mock<ISearchConfiguration>();
            var controller = initializeController(configuration);
            var searchCriteria = new SearchCriteria { Query = null, ProjectIds = new [] { 1 } };
            BadRequestException badRequestException = null;
            // Act
            try
            {
                await controller.Post(searchCriteria, null, -1);
            }
            catch (BadRequestException e)
            {
                badRequestException = e;
            }

            //Assert
            Assert.IsNotNull(badRequestException, "Bad Request Exception should have been thrown");
            Assert.AreEqual(ErrorCodes.IncorrectSearchCriteria, badRequestException.ErrorCode, "IncorrectSearchCriteria should be provided as Error code");
        }

        [TestMethod]
        public async Task Post_SearchCriteriaIsEmpty_ThrowsBadRequestException()
        {
            // Arrange
            var configuration = new Mock<ISearchConfiguration>();
            var controller = initializeController(configuration);
            var searchCriteria = new SearchCriteria { Query = "", ProjectIds = new [] { 1 } };
            BadRequestException badRequestException = null;
            // Act
            try
            {
                await controller.Post(searchCriteria, null, -1);
            }
            catch (BadRequestException e)
            {
                badRequestException = e;
            }

            //Assert
            Assert.IsNotNull(badRequestException, "Bad Request Exception should have been thrown");
            Assert.AreEqual(ErrorCodes.IncorrectSearchCriteria, badRequestException.ErrorCode, "IncorrectSearchCriteria should be provided as Error code");
        }

        [TestMethod]
        public async Task Post_SearchCriteriaIsLessThanThreeCharacters_ThrowsBadRequestException()
        {
            // Arrange
            var configuration = new Mock<ISearchConfiguration>();
            var controller = initializeController(configuration);
            var searchCriteria = new SearchCriteria { Query = "12", ProjectIds = new [] { 1 } };
            BadRequestException badRequestException = null;
            // Act
            try
            {
                await controller.Post(searchCriteria, null, -1);
            }
            catch (BadRequestException e)
            {
                badRequestException = e;
            }

            //Assert
            Assert.IsNotNull(badRequestException, "Bad Request Exception should have been thrown");
            Assert.AreEqual(ErrorCodes.IncorrectSearchCriteria, badRequestException.ErrorCode, "IncorrectSearchCriteria should be provided as Error code");
        }

        [TestMethod]
        public async Task Post_ZeropPageSize_ReturnsConstant()
        {
            // Arrange
            var configuration = new Mock<ISearchConfiguration>();
            var controller = initializeController(configuration);
            var searchCriteria = new SearchCriteria { Query = "empty", ProjectIds = new[] { 1 } };

            // Act
            var actionResult = await controller.Post(searchCriteria, null, 0);

            // Assert
            var result = actionResult as OkNegotiatedContentResult<FullTextSearchResult>;
            Assert.IsNotNull(result, "Result was not retrieved");
            Assert.AreEqual(result.Content.PageSize, ServiceConstants.SearchPageSize);
        }

        [TestMethod]
        public async Task Post_50PageSize_ReturnsConstant()
        {
            // Arrange
            var configuration = new Mock<ISearchConfiguration>();
            var controller = initializeController(configuration);
            var searchCriteria = new SearchCriteria { Query = "empty", ProjectIds = new[] { 1 } };

            // Act
            var actionResult = await controller.Post(searchCriteria, null, 50);

            // Assert
            var result = actionResult as OkNegotiatedContentResult<FullTextSearchResult>;
            Assert.IsNotNull(result, "Result was not retrieved");
            Assert.AreEqual(result.Content.PageSize, 50);
        }

        [TestMethod]
        public async Task Post_RequestNegativePage_ReturnsPageOne()
        {
            // Arrange
            var configuration = new Mock<ISearchConfiguration>();
            var controller = initializeController(configuration);
            var searchCriteria = new SearchCriteria { Query = "empty", ProjectIds = new[] { 1 } };

            // Act
            var actionResult = await controller.Post(searchCriteria, -10, 1);

            // Assert
            var result = actionResult as OkNegotiatedContentResult<FullTextSearchResult>;
            Assert.IsNotNull(result, "Result was not retrieved");
            Assert.AreEqual(result.Content.Page, 1);
        }

        [TestMethod]
        public async Task Post_RequestZeroPage_ReturnsPageOne()
        {
            // Arrange
            var configuration = new Mock<ISearchConfiguration>();
            var controller = initializeController(configuration);
            var searchCriteria = new SearchCriteria { Query = "empty", ProjectIds = new[] { 1 } };

            // Act
            var actionResult = await controller.Post(searchCriteria, 0, 1);

            // Assert
            var result = actionResult as OkNegotiatedContentResult<FullTextSearchResult>;
            Assert.IsNotNull(result, "Result was not retrieved");
            Assert.AreEqual(result.Content.Page, 1);
        }

        [TestMethod]
        public async Task Post_RequestPositivePage_ReturnsPageOne()
        {
            // Arrange
            var configuration = new Mock<ISearchConfiguration>();
            var controller = initializeController(configuration);
            var searchCriteria = new SearchCriteria { Query = "empty", ProjectIds = new[] { 1 } };

            // Act
            var actionResult = await controller.Post(searchCriteria, 10, 1);

            // Assert
            var result = actionResult as OkNegotiatedContentResult<FullTextSearchResult>;
            Assert.IsNotNull(result, "Result was not retrieved");
            Assert.AreEqual(result.Content.Page, 10);
        }

        #endregion

        #region METADATA Test
        public async Task Metadata_PageSizeNegative_ReturnsConstant()
        {
            // Arrange
            var configuration = new Mock<ISearchConfiguration>();
            var controller = initializeController(configuration);
            var searchCriteria = new SearchCriteria { Query = "empty", ProjectIds = new [] { 1 } };

            // Act
            var actionResult = await controller.MetaData(searchCriteria, -1);

            // Assert
            var result = actionResult as OkNegotiatedContentResult<FullTextSearchResult>;
            Assert.IsNotNull(result, "Result was not retrieved");
            Assert.AreEqual(result.Content.PageSize, ServiceConstants.SearchPageSize);
        }
        #endregion
        private FullTextSearchController initializeController(Mock<ISearchConfiguration> configuration)
        {
            var fullTextSearchRepositoryMock = new Mock<IFullTextSearchRepository>();
            fullTextSearchRepositoryMock.Setup(a => a.Search(It.IsAny<int>(), It.IsAny<SearchCriteria>(), It.IsAny<int>(), It.IsAny<int>())).
                Returns(Task.FromResult
                (
                    new FullTextSearchResult
                    {
                        FullTextSearchItems = new List<FullTextSearchItem>()
                    }
                ));
            var request = new HttpRequestMessage();
            request.Properties.Add("Session", new Session { UserId = 1 });

            return new FullTextSearchController(fullTextSearchRepositoryMock.Object, configuration.Object)
            {
                Request = request
            };

        }
    }
}
