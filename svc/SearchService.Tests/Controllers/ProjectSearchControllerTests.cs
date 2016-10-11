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
        public async Task GetProjects_Success()
        {
            //Arrange
            const int projectId = 10;
            var searchCriteria = new ProjectSearchCriteria {Query = "Test"};
            var project = new ProjectSearchResult { ProjectId = projectId, ProjectName = searchCriteria.Query };
            _projectSearchRepositoryMock.Setup(m => m.GetProjectsByName(1, searchCriteria.Query, 20, '/')).ReturnsAsync(new[] { project });
            var controller = new ProjectSearchController(_projectSearchRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties[ServiceConstants.SessionProperty] = _session;
            //Act
            var result = await controller.GetProjectsByName(searchCriteria, 20);

            //Assert
            Assert.IsNotNull(result);
            var projectSearchResults = result as IList<ProjectSearchResult> ?? result.ToList();
            Assert.AreEqual(projectSearchResults.Count, 1);
            Assert.AreEqual(projectId, projectSearchResults[0].ProjectId);
        }

        [TestMethod]
        public async Task GetProjects_ResultCountIsNull_Success()
        {
            //Arrange
            const int projectId = 10;
            var searchCriteria = new ProjectSearchCriteria { Query = "Test" };
            var project = new ProjectSearchResult { ProjectId = projectId, ProjectName = searchCriteria.Query };
            _projectSearchRepositoryMock.Setup(m => m.GetProjectsByName(1, searchCriteria.Query, 20, '/')).ReturnsAsync(new[] { project });
            var controller = new ProjectSearchController(_projectSearchRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties[ServiceConstants.SessionProperty] = _session;
            //Act
            var result = await controller.GetProjectsByName(searchCriteria, null);

            //Assert
            Assert.IsNotNull(result);
            var projectSearchResults = result as IList<ProjectSearchResult> ?? result.ToList();
            Assert.AreEqual(projectSearchResults.Count, 1);
            Assert.AreEqual(projectId, projectSearchResults[0].ProjectId);
        }

        [TestMethod]
        public async Task GetProjects_ResultCountMoreThanMax_Success()
        {
            //Arrange
            const int projectId = 10;
            var searchCriteria = new ProjectSearchCriteria { Query = "Test" };
            var project = new ProjectSearchResult { ProjectId = projectId, ProjectName = searchCriteria.Query };
            _projectSearchRepositoryMock.Setup(m => m.GetProjectsByName(1, searchCriteria.Query, 100, '/')).ReturnsAsync(new[] { project });
            var controller = new ProjectSearchController(_projectSearchRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties[ServiceConstants.SessionProperty] = _session;
            //Act
            var result = await controller.GetProjectsByName(searchCriteria, 1000);

            //Assert
            Assert.IsNotNull(result);
            var projectSearchResults = result as IList<ProjectSearchResult> ?? result.ToList();
            Assert.AreEqual(projectSearchResults.Count, 1);
            Assert.AreEqual(projectId, projectSearchResults[0].ProjectId);
        }

        [TestMethod]
        public async Task GetProjects_QueryIsEmpty_BadRequest()
        {
            //Arrange
            var controller = new ProjectSearchController(_projectSearchRepositoryMock.Object);
            //Act
            try
            {
                await controller.GetProjectsByName(new ProjectSearchCriteria { Query = "" });
            }
            catch (HttpResponseException e)
            {
                //Assert
                Assert.AreEqual(HttpStatusCode.BadRequest, e.Response.StatusCode);
            }
        }

        [TestMethod]
        public async Task GetProjects_ResultCountIsNegative_BadRequest()
        {
            //Arrange
            var controller = new ProjectSearchController(_projectSearchRepositoryMock.Object);
            //Act
            try
            {
                await controller.GetProjectsByName(new ProjectSearchCriteria { Query = "test" }, -1);
            }
            catch (HttpResponseException e)
            {
                //Assert
                Assert.AreEqual(HttpStatusCode.BadRequest, e.Response.StatusCode);
            }
        }

        [TestMethod]
        public async Task GetProjects_NullSerachCriteria_BadRequest()
        {
            //Arrange
            var controller = new ProjectSearchController(_projectSearchRepositoryMock.Object);
            //Act
            try
            {
                await controller.GetProjectsByName(null);
            }
            catch (HttpResponseException e)
            {
                //Assert
                Assert.AreEqual(HttpStatusCode.BadRequest, e.Response.StatusCode);
            }
        }

        [TestMethod]
        public async Task GetProjects_Forbidden()
        {
            //Arrange
            const int projectId = 10;
            var searchCriteria = new ProjectSearchCriteria { Query = "Test" };
            var project = new ProjectSearchResult { ProjectId = projectId, ProjectName = searchCriteria.Query };
            _projectSearchRepositoryMock.Setup(m => m.GetProjectsByName(1, searchCriteria.Query, 100, '/')).ReturnsAsync(new[] { project });
            var controller = new ProjectSearchController(_projectSearchRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            //Act
            try
            {
                await controller.GetProjectsByName(searchCriteria, 1000);
            }
            catch (HttpResponseException e)
            {
                //Assert
                Assert.AreEqual(HttpStatusCode.Forbidden, e.Response.StatusCode);
            }
        }
    }
}
