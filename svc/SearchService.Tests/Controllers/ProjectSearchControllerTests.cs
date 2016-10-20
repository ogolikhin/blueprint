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
        private Mock<IProjectSearchRepository> _projectSearchRepositoryMock;

        private Session _session;

        [TestInitialize]
        public void Initialize()
        {
            // Arrange
            const int userId = 1;
            _session = new Session { UserId = userId };

            _projectSearchRepositoryMock = new Mock<IProjectSearchRepository>();
        }

        [TestMethod]
        public async Task SearchName_Success()
        {
            //Arrange
            const int projectId = 10;
            var searchCriteria = new ProjectSearchCriteria {Query = "Test"};
            var project = new ProjectSearchResult { Id = projectId, Name = searchCriteria.Query };
            _projectSearchRepositoryMock.Setup(m => m.GetProjectsByName(1, searchCriteria.Query, 20, "/")).ReturnsAsync(new[] { project });
            var controller = new ProjectSearchController(_projectSearchRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties[ServiceConstants.SessionProperty] = _session;
            //Act
            var result = await controller.SearchName(searchCriteria, 20);

            //Assert
            Assert.IsNotNull(result);
            var projectSearchResults = result as IList<ProjectSearchResult> ?? result.ToList();
            Assert.AreEqual(projectSearchResults.Count, 1);
            Assert.AreEqual(projectId, projectSearchResults[0].Id);
        }

        [TestMethod]
        public async Task SearchName_ResultCountIsNull_Success()
        {
            //Arrange
            const int projectId = 10;
            var searchCriteria = new ProjectSearchCriteria { Query = "Test" };
            var project = new ProjectSearchResult { Id = projectId, Name = searchCriteria.Query };
            _projectSearchRepositoryMock.Setup(m => m.GetProjectsByName(1, searchCriteria.Query, 20, "/")).ReturnsAsync(new[] { project });
            var controller = new ProjectSearchController(_projectSearchRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties[ServiceConstants.SessionProperty] = _session;
            //Act
            var result = await controller.SearchName(searchCriteria, null);

            //Assert
            Assert.IsNotNull(result);
            var projectSearchResults = result as IList<ProjectSearchResult> ?? result.ToList();
            Assert.AreEqual(projectSearchResults.Count, 1);
            Assert.AreEqual(projectId, projectSearchResults[0].Id);
        }

        [TestMethod]
        public async Task SearchName_ResultCountMoreThanMax_Success()
        {
            //Arrange
            const int projectId = 10;
            var searchCriteria = new ProjectSearchCriteria { Query = "Test" };
            var project = new ProjectSearchResult { Id = projectId, Name = searchCriteria.Query };
            _projectSearchRepositoryMock.Setup(m => m.GetProjectsByName(1, searchCriteria.Query, 100, "/")).ReturnsAsync(new[] { project });
            var controller = new ProjectSearchController(_projectSearchRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties[ServiceConstants.SessionProperty] = _session;
            //Act
            var result = await controller.SearchName(searchCriteria, 1000);

            //Assert
            Assert.IsNotNull(result);
            var projectSearchResults = result as IList<ProjectSearchResult> ?? result.ToList();
            Assert.AreEqual(projectSearchResults.Count, 1);
            Assert.AreEqual(projectId, projectSearchResults[0].Id);
        }

        [TestMethod]
        public async Task SearchName_QueryIsEmpty_BadRequest()
        {
            //Arrange
            var controller = new ProjectSearchController(_projectSearchRepositoryMock.Object);
            //Act
            try
            {
                await controller.SearchName(new ProjectSearchCriteria { Query = "" });
            }
            catch (BadRequestException e)
            {
                //Assert
                Assert.AreEqual(ErrorCodes.IncorrectSearchCriteria, e.ErrorCode);
            }
        }

        [TestMethod]
        public async Task SearchName_ResultCountIsNegative_BadRequest()
        {
            //Arrange
            var controller = new ProjectSearchController(_projectSearchRepositoryMock.Object);
            //Act
            try
            {
                await controller.SearchName(new ProjectSearchCriteria { Query = "test" }, -1);
            }
            catch (BadRequestException e)
            {
                //Assert
                Assert.AreEqual(ErrorCodes.OutOfRangeParameter, e.ErrorCode);
            }
        }

        [TestMethod]
        public async Task SearchName_NullSerachCriteria_BadRequest()
        {
            //Arrange
            var controller = new ProjectSearchController(_projectSearchRepositoryMock.Object);
            //Act
            try
            {
                await controller.SearchName(null);
            }
            catch (BadRequestException e)
            {
                //Assert
                Assert.AreEqual(ErrorCodes.IncorrectSearchCriteria, e.ErrorCode);
            }
        }

        [TestMethod]
        public async Task SearchName_Forbidden()
        {
            //Arrange
            const int projectId = 10;
            var searchCriteria = new ProjectSearchCriteria { Query = "Test" };
            var project = new ProjectSearchResult { Id = projectId, Name = searchCriteria.Query };
            _projectSearchRepositoryMock.Setup(m => m.GetProjectsByName(1, searchCriteria.Query, 100, "/")).ReturnsAsync(new[] { project });
            var controller = new ProjectSearchController(_projectSearchRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            //Act
            try
            {
                await controller.SearchName(searchCriteria, 1000);
            }
            catch (HttpResponseException e)
            {
                //Assert
                Assert.AreEqual(HttpStatusCode.Forbidden, e.Response.StatusCode);
            }
        }
    }
}
