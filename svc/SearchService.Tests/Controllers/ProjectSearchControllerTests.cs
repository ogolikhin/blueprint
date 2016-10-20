using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SearchService.Models;
using SearchService.Repositories;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace SearchService.Controllers
{
    [TestClass]
    public class ProjectSearchControllerTests
    {
        #region Constuctor

        [TestMethod]
        public void Constructor_CreatesDefaultDependencies()
        {
            // Arrange

            // Act
            var controller = new ProjectSearchController();

            // Assert
            Assert.IsInstanceOfType(controller._projectSearchRepository, typeof(SqlProjectSearchRepository));
        }

        #endregion

        #region SearchName

        [TestMethod]
        public async Task SearchName_Success()
        {
            // Arrange
            const int projectId = 10;
            var searchCriteria = new ProjectSearchCriteria {Query = "Test"};
            var project = new ProjectSearchResult { Id = projectId, Name = searchCriteria.Query };
            var controller = CreateController(searchCriteria, project);

            // Act
            var result = await controller.SearchName(searchCriteria, 20);

            // Assert
            Assert.IsNotNull(result);
            var projectSearchResults = result as IList<ProjectSearchResult> ?? result.ToList();
            Assert.AreEqual(projectSearchResults.Count, 1);
            Assert.AreEqual(projectId, projectSearchResults[0].Id);
        }

        [TestMethod]
        public async Task SearchName_ResultCountIsNull_Success()
        {
            // Arrange
            const int projectId = 10;
            var searchCriteria = new ProjectSearchCriteria { Query = "Test" };
            var project = new ProjectSearchResult { Id = projectId, Name = searchCriteria.Query };
            var controller = CreateController(searchCriteria, project);

            // Act
            var result = await controller.SearchName(searchCriteria, null);

            // Assert
            Assert.IsNotNull(result);
            var projectSearchResults = result as IList<ProjectSearchResult> ?? result.ToList();
            Assert.AreEqual(projectSearchResults.Count, 1);
            Assert.AreEqual(projectId, projectSearchResults[0].Id);
        }

        [TestMethod]
        public async Task SearchName_ResultCountMoreThanMax_Success()
        {
            // Arrange
            const int projectId = 10;
            var searchCriteria = new ProjectSearchCriteria { Query = "Test" };
            var project = new ProjectSearchResult { Id = projectId, Name = searchCriteria.Query };
            var controller = CreateController(searchCriteria, project);

            // Act
            var result = await controller.SearchName(searchCriteria, 1000);

            // Assert
            Assert.IsNotNull(result);
            var projectSearchResults = result as IList<ProjectSearchResult> ?? result.ToList();
            Assert.AreEqual(projectSearchResults.Count, 1);
            Assert.AreEqual(projectId, projectSearchResults[0].Id);
        }

        [TestMethod]
        public async Task SearchName_QueryIsEmpty_BadRequest()
        {
            // Arrange
            var searchCriteria = new ProjectSearchCriteria
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
            var searchCriteria = new ProjectSearchCriteria
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
            var searchCriteria = new ProjectSearchCriteria { Query = "Test" };
            var project = new ProjectSearchResult { Id = projectId, Name = searchCriteria.Query };
            var controller = CreateController(searchCriteria, project);
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

        #endregion SearchName

        public static ProjectSearchController CreateController(ProjectSearchCriteria searchCriteria, params ProjectSearchResult[] result)
        {
            var projectSearchRepository = new Mock<IProjectSearchRepository>();
            string searchText = searchCriteria?.Query;
            projectSearchRepository.Setup(m => m.GetProjectsByName(1, searchText, It.IsAny<int>(), "/")).ReturnsAsync(result);

            var request = new HttpRequestMessage();
            request.Properties.Add(ServiceConstants.SessionProperty, new Session { UserId = 1 });
            return new ProjectSearchController(projectSearchRepository.Object)
            {
                Request = request
            };
        }
    }
}
