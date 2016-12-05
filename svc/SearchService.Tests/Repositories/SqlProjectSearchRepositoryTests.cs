using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchService.Models;
using ServiceLibrary.Repositories;
using SearchService.Helpers;
using Moq;
using System;
using System.Data;
using System.Data.SqlClient;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

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
            SearchProjectResult[] queryResult =
            {
                new SearchProjectResult()
            };
            var projectSearchRepository = CreateRepository(userId, searchCriteria, resultCount, separatorString, queryResult);

            // Act
            var result = await projectSearchRepository.SearchName(userId, searchCriteria, resultCount, separatorString);

            // Assert
            CollectionAssert.AreEqual(queryResult, result.Items.ToList());
        }

        [TestMethod]
        public async Task SearchName_WithSqlTimeoutException_SqlTimeoutExceptionOccurs()
        {
            // Arrange
            const int userId = 1;
            var searchCriteria = new SearchCriteria { Query = "test" };
            const int resultCount = 1;
            const string separatorString = "/";
            Exception sqlException = SqlExceptionCreator.NewSqlException(ErrorCodes.SqlTimeoutNumber);
            var projectSearchRepository = CreateRepositoryWithExceptionExpectation<SearchProjectResult>(sqlException);
            SqlTimeoutException sqlTimeoutException = null;

            // Act
            try
            {
                await projectSearchRepository.SearchName(userId, searchCriteria, resultCount, separatorString);
            }
            catch (SqlTimeoutException exception)
            {
                sqlTimeoutException = exception;
            }

            // Assert
            Assert.IsNotNull(sqlTimeoutException, "sqlTimeoutException != null");
            Assert.IsTrue(sqlTimeoutException.ErrorCode == ErrorCodes.Timeout, "Timeout exception should occur");
        }

        [TestMethod]
        public async Task SearchName_WithSqlException_SqlExceptionOccurs()
        {
            // Arrange
            const int userId = 1;
            var searchCriteria = new SearchCriteria { Query = "test" };
            const int resultCount = 1;
            const string separatorString = "/";
            Exception expectedException = SqlExceptionCreator.NewSqlException(-4);
            var projectSearchRepository = CreateRepositoryWithExceptionExpectation<SearchProjectResult>(expectedException);
            SqlException sqlException = null;

            // Act
            try
            {
                await projectSearchRepository.SearchName(userId, searchCriteria, resultCount, separatorString);
            }
            catch (SqlException exception)
            {
                sqlException = exception;
            }

            // Assert
            Assert.IsNotNull(sqlException, "sqlException != null");
            Assert.IsTrue(sqlException.Number == -4, "exception should occur");
        }

        #endregion SearchName

        private static SqlProjectSearchRepository CreateRepository(int userId, SearchCriteria searchCriteria, int resultCount, string separatorString, SearchProjectResult[] result)
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

            var configuration = new Mock<ISearchConfiguration>();
            return new SqlProjectSearchRepository(connectionWrapper.Object, configuration.Object);
        }

        private static SqlProjectSearchRepository CreateRepositoryWithExceptionExpectation<T>(Exception exception)
        {
            var connectionWrapper = new SqlConnectionWrapperMock();

            connectionWrapper.Setup(
                t => t.QueryAsync<T>("GetProjectsByName", It.IsAny<object>(), It.IsAny<IDbTransaction>(),
                It.IsAny<int?>(), It.IsAny<CommandType?>())).Throws(exception);

            var configuration = new Mock<ISearchConfiguration>();
            return new SqlProjectSearchRepository(connectionWrapper.Object, configuration.Object);
        }
    }
}
