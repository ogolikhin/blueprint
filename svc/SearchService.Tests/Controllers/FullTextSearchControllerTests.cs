using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using Moq;
using SearchService.Repositories;
using System.Net.Http;
using ServiceLibrary.Models;
using SearchService.Models;
using System.Web.Http.Results;
using ServiceLibrary.Helpers;
using SearchService.Helpers;

namespace SearchService.Controllers
{
    [TestClass]
    public class FullTextSearchControllerTests
    {
        #region POST Tests
        public async Task Post_PageSizeNegative_ReturnsConstant()
        {
            // Arrange
            var configuration = new Mock<IConfiguration>();
            var controller = initializeController(configuration);
            var searchCriteria = new Models.SearchCriteria() { Query = "empty", ProjectIds = new int[1] { 1 } };

            // Act
            var actionResult = await controller.Post(searchCriteria, null, -1);

            // Assert
            var result = actionResult as OkNegotiatedContentResult<FullTextSearchResult>;
            Assert.AreEqual(result.Content.PageSize, ServiceConstants.SearchPageSize);
        }
        #endregion

        #region METADATA Test
        public async Task Metadata_PageSizeNegative_ReturnsConstant()
        {
            // Arrange
            var configuration = new Mock<IConfiguration>();
            var controller = initializeController(configuration);
            var searchCriteria = new Models.SearchCriteria() { Query = "empty", ProjectIds = new int[1] { 1 } };

            // Act
            var actionResult = await controller.MetaData(searchCriteria, -1);

            // Assert
            var result = actionResult as OkNegotiatedContentResult<FullTextSearchResult>;
            Assert.AreEqual(result.Content.PageSize, ServiceConstants.SearchPageSize);
        }
        #endregion
        private FullTextSearchController initializeController(Mock<IConfiguration> configuration)
        {
            var fullTextSearchRepositoryMock = new Mock<IFullTextSearchRepository>();
            fullTextSearchRepositoryMock.Setup(a => a.Search(It.IsAny<int>(), It.IsAny<Models.SearchCriteria>(), It.IsAny<int>(), It.IsAny<int>())).
                Returns(Task.FromResult<FullTextSearchResult>
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
