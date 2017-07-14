using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SearchService.Models;
using SearchService.Repositories;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace SearchService.Controllers
{
    [TestClass]
    public class ProjectSearchControllerTests
    {
        #region SearchName

        [TestMethod]
        public async Task SearchName_Success()
        {
            // Arrange
            const int projectId = 10;
            var searchCriteria = new SearchCriteria { Query = "Test"};
            var project = new ProjectSearchResult { ItemId = projectId, Name = searchCriteria.Query };
            var searchResult = new ProjectSearchResultSet { Items = new[] { project } };
            var controller = CreateController(searchCriteria, searchResult);

            // Act
            var result = await controller.SearchName(searchCriteria, 20);

            // Assert
            Assert.IsNotNull(result);
            var projectSearchResults = result.Items.ToList();
            Assert.AreEqual(projectSearchResults.Count, 1);
            Assert.AreEqual(projectId, projectSearchResults[0].ItemId);
        }

        [TestMethod]
        public async Task SearchName_ResultCountIsNull_Success()
        {
            // Arrange
            const int projectId = 10;
            var searchCriteria = new SearchCriteria { Query = "Test" };
            var project = new ProjectSearchResult { ItemId = projectId, Name = searchCriteria.Query };
            var searchResult = new ProjectSearchResultSet { Items = new[] { project } };
            var controller = CreateController(searchCriteria, searchResult);

            // Act
            var result = await controller.SearchName(searchCriteria, null);

            // Assert
            Assert.IsNotNull(result);
            var projectSearchResults = result.Items.ToList();
            Assert.AreEqual(projectSearchResults.Count, 1);
            Assert.AreEqual(projectId, projectSearchResults[0].ItemId);
        }

        [TestMethod]
        public async Task SearchName_ResultCountMoreThanMax_Success()
        {
            // Arrange
            const int projectId = 10;
            var searchCriteria = new SearchCriteria { Query = "Test" };
            var project = new ProjectSearchResult { ItemId = projectId, Name = searchCriteria.Query };
            var searchResult = new ProjectSearchResultSet { Items = new[] { project } };
            var controller = CreateController(searchCriteria, searchResult);

            // Act
            var result = await controller.SearchName(searchCriteria, 1000);

            // Assert
            Assert.IsNotNull(result);
            var projectSearchResults = result.Items.ToList();
            Assert.AreEqual(projectSearchResults.Count, 1);
            Assert.AreEqual(projectId, projectSearchResults[0].ItemId);
        }

        [TestMethod]
        public async Task SearchName_QueryIsEmpty_BadRequest()
        {
            // Arrange
            var searchCriteria = new SearchCriteria
            {
                Query = ""
            };
            var controller = CreateController(searchCriteria);

            // Act
            BadRequestException badRequestException = null;
            try
            {
                await controller.SearchName(searchCriteria);
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
        public async Task SearchName_ResultCountIsNegative_BadRequest()
        {
            // Arrange
            var searchCriteria = new SearchCriteria
            {
                Query = "test"
            };
            var controller = CreateController(searchCriteria);

            // Act
            BadRequestException badRequestException = null;
            try
            {
                await controller.SearchName(searchCriteria, -1);
            }
            catch (BadRequestException e)
            {
                badRequestException = e;
            }

            // Assert
            Assert.IsNotNull(badRequestException, "Bad Request Exception should have been thrown");
            Assert.AreEqual(ErrorCodes.OutOfRangeParameter, badRequestException.ErrorCode, "OutOfRangeParameter should be provided as Error code");
        }

        [TestMethod]
        public async Task SearchName_NullSerachCriteria_BadRequest()
        {
            // Arrange
            var controller = CreateController(null);

            // Act
            BadRequestException badRequestException = null;
            try
            {
                await controller.SearchName(null);
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
        public async Task SearchName_Forbidden()
        {
            // Arrange
            const int projectId = 10;
            var searchCriteria = new SearchCriteria { Query = "Test" };
            var project = new ProjectSearchResult { ItemId = projectId, Name = searchCriteria.Query };
            var searchResult = new ProjectSearchResultSet { Items = new[] { project } };
            var controller = CreateController(searchCriteria, searchResult);
            controller.Request.Properties.Remove(ServiceConstants.SessionProperty);

            // Act
            HttpResponseException httpResponseException = null;
            try
            {
                await controller.SearchName(searchCriteria, 1000);
            }
            catch (HttpResponseException e)
            {
                httpResponseException = e;
            }

            // Assert
            Assert.IsNotNull(httpResponseException, "Bad Request Exception should have been thrown");
            Assert.AreEqual(HttpStatusCode.Forbidden, httpResponseException.Response.StatusCode, "Forbidden should be provided as Status code");
        }

        [TestMethod]
        public async Task SearchName_RepoThrowsException_LogShouldBeCalled()
        {
            // Arrange
            const int projectId = 10;
            var searchCriteria = new SearchCriteria { Query = "Test" };
            var project = new ProjectSearchResult { ItemId = projectId, Name = searchCriteria.Query };
            var searchResult = new ProjectSearchResultSet { Items = new[] { project } };
            var logMock = new Mock<IServiceLogRepository>();
            logMock.Setup(t => t.LogError(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns(Task.Delay(1));
            var exceptionToBeThrown = new Exception("MyException");

            var controller = CreateControllerForExceptionCases(searchCriteria, logMock, exceptionToBeThrown, searchResult);
            Exception actualException = null;

            // Act
            try
            {
                var result = await controller.SearchName(searchCriteria, 20);
            }
            catch (Exception ex)
            {
                actualException = ex;
            }

            // Assert
            Assert.IsNotNull(actualException);
            Assert.AreEqual(exceptionToBeThrown.Message, actualException.Message, "Incorrect message was thrown");
            logMock.Verify(t => t.LogError(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Exactly(1));
        }

        #endregion SearchName

        public static ProjectSearchController CreateController(SearchCriteria searchCriteria, ProjectSearchResultSet result = null)
        {
            var projectSearchRepository = new Mock<IProjectSearchRepository>();
            projectSearchRepository.Setup(m => m.SearchName(1, searchCriteria, It.IsAny<int>(), "/")).ReturnsAsync(result);

            var request = new HttpRequestMessage();
            request.Properties.Add(ServiceConstants.SessionProperty, new Session { UserId = 1 });

            var logMock = new Mock<IServiceLogRepository>();

            return new ProjectSearchController(projectSearchRepository.Object, logMock.Object)
            {
                Request = request
            };
        }

        public static ProjectSearchController CreateControllerForExceptionCases(SearchCriteria searchCriteria, Mock<IServiceLogRepository> logMock, Exception exceptionToBeThrown, ProjectSearchResultSet result = null)
        {
            var projectSearchRepository = new Mock<IProjectSearchRepository>();
            projectSearchRepository.Setup(m => m.SearchName(1, searchCriteria, It.IsAny<int>(), "/")).ThrowsAsync(exceptionToBeThrown);

            var request = new HttpRequestMessage();
            request.Properties.Add(ServiceConstants.SessionProperty, new Session { UserId = 1 });
            return new ProjectSearchController(projectSearchRepository.Object, logMock.Object)
            {
                Request = request
            };
        }
    }
}
