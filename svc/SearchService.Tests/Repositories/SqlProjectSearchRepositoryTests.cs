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

        #region SearchName

        [TestMethod]
        public async Task SearchName_QueryReturnsResult_ReturnsResult()
        {
            // Arrange
            const int userId = 1;
            var searchCriteria = new SearchCriteria { Query = "test" };
            const int resultCount = 1;
            const string separatorString = "/";
            SearchResult[] queryResult =
            {
                new SearchResult()
            };
            var projectSearchRepository = CreateRepository(userId, searchCriteria, resultCount, separatorString, queryResult);

            // Act
            var result = await projectSearchRepository.SearchName(userId, searchCriteria, resultCount, separatorString);

            // Assert
            CollectionAssert.AreEqual(queryResult, result.Items.ToList());
        }

        #endregion SearchName

        private static SqlProjectSearchRepository CreateRepository(int userId, SearchCriteria searchCriteria, int resultCount, string separatorString, SearchResult[] result)
        {
            var connectionWrapper = new SqlConnectionWrapperMock();
            connectionWrapper.SetupQueryAsync("GetProjectsByName",
                new Dictionary<string, object>
                {
                    { "userId", userId },
                    { "projectName", searchCriteria.Query },
                    { "resultCount", resultCount },
                    { "separatorString", separatorString }
                },
                result);
            return new SqlProjectSearchRepository(connectionWrapper.Object);
        }
    }
}
