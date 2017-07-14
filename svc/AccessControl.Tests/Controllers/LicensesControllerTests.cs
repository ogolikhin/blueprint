using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using AccessControl.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;

namespace AccessControl.Controllers
{
    [TestClass]
    public class LicensesControllerTests
    {
        #region GetActiveLicenses

        [TestMethod]
        public async Task GetActiveLicenses_RepositoryReturnsResult_ReturnsResult()
        {
            // Arrange
            var sessions = new Mock<ISessionsRepository>();
            var repo = new Mock<ILicensesRepository>();
            var licenses = new[]
            {
                new LicenseInfo { LicenseLevel = 1, Count = 1 },
                new LicenseInfo { LicenseLevel = 3, Count = 2 }
            };
            repo.Setup(r => r.GetActiveLicenses(It.IsAny<DateTime>(), It.IsAny<int>())).ReturnsAsync(licenses);
            var controller = CreateController(repo.Object, sessions.Object);

            // Act
            var result = await controller.GetActiveLicenses() as ResponseMessageResult;

            // Assert
            Assert.IsNotNull(result);
            var content = await result.Response.Content.ReadAsAsync<IEnumerable<LicenseInfo>>();
            CollectionAssert.AreEquivalent(licenses, content.ToList());
        }

        [TestMethod]
        public async Task GetActiveLicenses_RepositoryThrowsException_InternalServerError()
        {
            // Arrange
            var sessions = new Mock<ISessionsRepository>();
            var repo = new Mock<ILicensesRepository>();
            repo.Setup(r => r.GetActiveLicenses(It.IsAny<DateTime>(), It.IsAny<int>())).Throws<Exception>();
            var controller = CreateController(repo.Object, sessions.Object);

            // Act
            var result = await controller.GetActiveLicenses();

            // Assert
            Assert.IsInstanceOfType(result, typeof(InternalServerErrorResult));
        }

        #endregion GetActiveLicenses

        #region GetLockedLicenses

        [TestMethod]
        public async Task GetLockedLicenses_RepositoryReturnsResult_ReturnsResult()
        {
            // Arrange
            var sessions = new Mock<ISessionsRepository>();
            var token = new Guid("11111111111111111111111111111111");
            var session = new Session { UserId = 1234, LicenseLevel = 3 };
            sessions.Setup(s => s.GetSession(token)).ReturnsAsync(session);
            var repo = new Mock<ILicensesRepository>();
            int lockedLicenses = 42;
            repo.Setup(r => r.GetLockedLicenses(session.UserId, session.LicenseLevel, It.IsAny<int>())).ReturnsAsync(lockedLicenses);
            var controller = CreateController(repo.Object, sessions.Object, token);

            // Act
            var result = await controller.GetLockedLicenses() as ResponseMessageResult;

            // Assert
            Assert.IsNotNull(result);
            var content = await result.Response.Content.ReadAsAsync<LicenseInfo>();
            Assert.AreEqual(session.LicenseLevel, content.LicenseLevel);
            Assert.AreEqual(lockedLicenses, content.Count);
        }

        [TestMethod]
        public async Task GetLockedLicenses_NoSessionToken_Unauthorised()
        {
            // Arrange
            var sessions = new Mock<ISessionsRepository>();
            var repo = new Mock<ILicensesRepository>();
            var controller = CreateController(repo.Object, sessions.Object);

            // Act
            var result = await controller.GetLockedLicenses();

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }

        [TestMethod]
        public async Task GetLockedLicenses_RepositoryThrowsException_InternalServerError()
        {
            // Arrange
            var sessions = new Mock<ISessionsRepository>();
            var token = new Guid("22222222222222222222222222222222");
            var session = new Session { UserId = 1234, LicenseLevel = 3 };
            sessions.Setup(s => s.GetSession(token)).ReturnsAsync(session);
            var repo = new Mock<ILicensesRepository>();
            repo.Setup(r => r.GetLockedLicenses(session.UserId, session.LicenseLevel, It.IsAny<int>())).Throws<Exception>();
            var controller = CreateController(repo.Object, sessions.Object, token);

            // Act
            var result = await controller.GetLockedLicenses();

            // Assert
            Assert.IsInstanceOfType(result, typeof(InternalServerErrorResult));
        }

        #endregion GetLockedLicenses

        #region GetLicenseTransactions

        [TestMethod]
        public async Task GetLicenseTransactions_RepositoryReturnsResult_ReturnsResult()
        {
            // Arrange
            var sessions = new Mock<ISessionsRepository>();
            var repo = new Mock<ILicensesRepository>();
            int consumerType = 1;
            var transactions = new[]
            {
                new LicenseTransaction { LicenseActivityId = 1, UserId = 1, LicenseType = 1, TransactionType = 1, ActionType = 1, ConsumerType = 1 },
                new LicenseTransaction { LicenseActivityId = 2, UserId = 2, LicenseType = 3, TransactionType = 1, ActionType = 1, ConsumerType = 1 },
                new LicenseTransaction { LicenseActivityId = 3, UserId = 1, LicenseType = 1, TransactionType = 2, ActionType = 2, ConsumerType = 1 },
            };
            repo.Setup(r => r.GetLicenseTransactions(It.IsAny<DateTime>(), consumerType)).ReturnsAsync(transactions);
            var controller = CreateController(repo.Object, sessions.Object);
            int days = 1;

            // Act
            var result = await controller.GetLicenseTransactions(days, consumerType) as ResponseMessageResult;

            // Assert
            Assert.IsNotNull(result);
            var content = await result.Response.Content.ReadAsAsync<IEnumerable<LicenseTransaction>>();
            CollectionAssert.AreEquivalent(transactions, content.ToList());
        }

        [TestMethod]
        public async Task GetLicenseTransactions_RepositoryThrowsException_InternalServerError()
        {
            // Arrange
            var sessions = new Mock<ISessionsRepository>();
            var repo = new Mock<ILicensesRepository>();
            int consumerType = 2;
            repo.Setup(r => r.GetLicenseTransactions(It.IsAny<DateTime>(), consumerType)).Throws<Exception>();
            var controller = CreateController(repo.Object, sessions.Object);
            int days = 1;

            // Act
            var result = await controller.GetLicenseTransactions(days, consumerType);

            // Assert
            Assert.IsInstanceOfType(result, typeof(InternalServerErrorResult));
        }

        #endregion GetLicenseTransactions

        private static LicensesController CreateController(ILicensesRepository repo, ISessionsRepository sessions, Guid? token = null)
        {
            var logMock = new Mock<IServiceLogRepository>();
            var controller = new LicensesController(repo, sessions, logMock.Object)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            if (token.HasValue)
            {
                controller.Request.Headers.Add("Session-Token", Session.Convert(token.Value));
            }

            return controller;
        }
    }
}
