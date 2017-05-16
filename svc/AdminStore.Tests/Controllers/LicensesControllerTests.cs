using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using AdminStore.Models;
using AdminStore.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;
using System.Web.Http.Hosting;

namespace AdminStore.Controllers
{
    [TestClass]
    public class LicensesControllerTests
    {
        #region Constructor

        [TestMethod]
        public void Constructor_CreatesDefaultDependencies()
        {
            // Arrange

            // Act
            var controller = new LicensesController();

            // Assert
            Assert.IsInstanceOfType(controller._httpClientProvider, typeof(HttpClientProvider));
            Assert.IsInstanceOfType(controller._userRepository, typeof(SqlUserRepository));
            Assert.IsInstanceOfType(controller._log, typeof(ServiceLogRepository));
        }

        #endregion Constructor

        #region GetLicenseTransactions

        [TestMethod]
        public async Task GetLicenseTransactions_HttpClientAndRepositoryReturnResults_ReturnsCombinedResults()
        {
            // Arrange
            var transactions = new[]
            {
                new LicenseTransaction { LicenseActivityId = 1, UserId = 1, LicenseType = 1, TransactionType = 1, ActionType = 1, ConsumerType = 1 },
                new LicenseTransaction { LicenseActivityId = 2, UserId = 2, LicenseType = 3, TransactionType = 1, ActionType = 1, ConsumerType = 1 },
                new LicenseTransaction { LicenseActivityId = 3, UserId = 1, LicenseType = 1, TransactionType = 2, ActionType = 2, ConsumerType = 1 },
            };
            var httpClientProvider = CreateTestHttpClientProvider(transactions);
            var users = new[]
            {
                new LicenseTransactionUser { Id = 1, Login = "user", Department = "Department" },
                new LicenseTransactionUser { Id = 2, Login = "another_user" }
            };
            var userRepository = new Mock<ISqlUserRepository>();
            var logMock = new Mock<IServiceLogRepository>();
            userRepository.Setup(r => r.GetLicenseTransactionUserInfoAsync(new[] { 1, 2 })).ReturnsAsync(users);
            var controller = new LicensesController(httpClientProvider, userRepository.Object, logMock.Object) { Request = new HttpRequestMessage() };
            controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            controller.Request.Headers.Add("Session-Token", string.Empty);
            int days = 1;

            // Act
            var result = await controller.GetLicenseTransactions(days) as ResponseMessageResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.Response.StatusCode);
            AddUserInfo(transactions, users);
            var resultContent = await result.Response.Content.ReadAsAsync<IEnumerable<LicenseTransaction>>();
            CollectionAssert.AreEqual(transactions, resultContent.ToList());
        }

        [TestMethod]
        public async Task GetLicenseTransactions_SessionTokenIsNull_UnauthorizedResult()
        {
            // Arrange
            var httpClientProvider = CreateTestHttpClientProvider(null);
            var userRepository = new Mock<ISqlUserRepository>();
            var logMock = new Mock<IServiceLogRepository>();
            var controller = new LicensesController(httpClientProvider, userRepository.Object, logMock.Object) { Request = new HttpRequestMessage() };
            int days = 1;

            // Act
            IHttpActionResult result = await controller.GetLicenseTransactions(days);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }

        [TestMethod]
        public async Task GetLicenseTransactions_RepositoryThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var transactions = new[]
            {
                new LicenseTransaction { LicenseActivityId = 1, UserId = 1, TransactionType = 1, ActionType = 1, ConsumerType = 1 },
                new LicenseTransaction { LicenseActivityId = 2, UserId = 2, TransactionType = 1, ActionType = 1, ConsumerType = 1 },
                new LicenseTransaction { LicenseActivityId = 3, UserId = 1, TransactionType = 2, ActionType = 2, ConsumerType = 1 },
            };
            var httpClientProvider = CreateTestHttpClientProvider(transactions);
            var userRepository = new Mock<ISqlUserRepository>();
            var logMock = new Mock<IServiceLogRepository>();
            userRepository.Setup(r => r.GetLicenseTransactionUserInfoAsync(new[] { 1, 2 })).Throws<Exception>();
            var controller = new LicensesController(httpClientProvider, userRepository.Object, logMock.Object) { Request = new HttpRequestMessage() };
            controller.Request.Headers.Add("Session-Token", string.Empty);
            int days = 1;

            // Act
            IHttpActionResult result = await controller.GetLicenseTransactions(days);

            // Assert
            Assert.IsInstanceOfType(result, typeof(InternalServerErrorResult));
        }

        #endregion GetLicenseTransactions

        private static TestHttpClientProvider CreateTestHttpClientProvider(LicenseTransaction[] transactions)
        {
            var content = new ObjectContent<IEnumerable<LicenseTransaction>>(transactions, new JsonMediaTypeFormatter());
            var httpClientProvider = new TestHttpClientProvider(request =>
                request.RequestUri.PathAndQuery.EqualsOrdinalIgnoreCase("/svc/accesscontrol/licenses/transactions?days=1&consumerType=1")
                    ? new HttpResponseMessage(HttpStatusCode.OK) { Content = content }
                    : null);
            return httpClientProvider;
        }

        private static void AddUserInfo(LicenseTransaction[] transactions, LicenseTransactionUser[] users)
        {
            var usersById = users.ToDictionary(u => u.Id);
            foreach (var transaction in transactions)
            {
                var user = usersById[transaction.UserId];
                transaction.Username = user.Login;
                transaction.Department = user.Department;
            }
        }
    }
}
