using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchService.Models;
using ServiceLibrary.Repositories;

namespace SearchService.Repositories
{
    [TestClass]
    public class SqlProjectSearchRepositoryTests
    {
        #region Constructor

        [TestMethod]
        public void Constructor_CreatesConnectionToBlueprint()
        {
            // Arrange

            // Act
            var repository = new SqlProjectSearchRepository();

            // Assert
            Assert.AreEqual(WebApiConfig.BlueprintConnectionString, repository.ConnectionWrapper.CreateConnection().ConnectionString);
        }

        #endregion Constructor

        #region GetProjectsByName

        [TestMethod]
        public async Task GetProjectsByName_QueryReturnsResult_ReturnsResult()
        {
            // Arrange
            const int userId = 1;
            const string searchText = "test";
            const int resultCount = 1;
            const string separatorString = "/";
            ProjectSearchResult[] queryResult =
            {
                new ProjectSearchResult()
            };
            var projectSearchRepository = CreateRepository(userId, searchText, resultCount, separatorString, queryResult);

            // Act
            var result = await projectSearchRepository.GetProjectsByName(userId, searchText, resultCount, separatorString);

            // Assert
            CollectionAssert.AreEqual(queryResult, result.ToList());
        }

        #endregion GetProjectsByName

        private static IProjectSearchRepository CreateRepository(int userId, string searchText, int resultCount, string separatorString, params ProjectSearchResult[] result)
        {
            var connectionWrapper = new SqlConnectionWrapperMock();
            connectionWrapper.SetupQueryAsync("GetProjectsByName",
                new Dictionary<string, object>
                {
                    { "userId", userId },
                    { "projectName", searchText },
                    { "resultCount", resultCount },
                    { "separatorString", separatorString }
                },
                result);
            return new SqlProjectSearchRepository(connectionWrapper.Object);
        }
    }
}
