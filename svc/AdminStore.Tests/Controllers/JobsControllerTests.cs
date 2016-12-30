﻿using AdminStore.Repositories.Jobs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Jobs;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AdminStore.Controllers
{
    [TestClass]
    public class JobsControllerTests
    {
        #region Constuctor

        [TestMethod]
        public void Constructor_CreatesDefaultDependencies()
        {
            // Arrange

            // Act
            var controller = new JobsController();

            // Assert
            Assert.IsInstanceOfType(controller._jobsRepository, typeof(JobsRepository));
            Assert.IsInstanceOfType(controller._sqlUserRepository, typeof(SqlUsersRepository));
        }

        #endregion

        #region GetLatestJobs
        [TestMethod]
        public async Task GetLatestJobs_InstanceAdminUser_NullsOutUserId()
        {
            // Arrange
            var session = new Session { UserName = "admin", UserId = 1 };
            var token = Guid.NewGuid().ToString();

            var sqlUserRepositoryMock = new Mock<IUsersRepository>();
            var jobsRepositoryMock = new Mock<IJobsRepository>();
            var serviceLogRepositoryMock = new Mock<IServiceLogRepository>();
            sqlUserRepositoryMock.Setup(a => a.IsInstanceAdmin(It.IsAny<bool>(), It.IsAny<int>())).
                Returns(Task.FromResult(true));

            var controller = new JobsController(jobsRepositoryMock.Object, serviceLogRepositoryMock.Object, sqlUserRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Headers.Add("Session-Token", token);
            controller.Request.Properties[ServiceConstants.SessionProperty] = session;

            // Act
            await controller.GetLatestJobs();

            // Assert
            jobsRepositoryMock.Verify(
                a => a.GetVisibleJobs(null, It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<JobType>()), Times.Once());
        }

        [TestMethod]
        public async Task GetLatestJobs_NonInstanceAdminuser_UserIdIsUsed()
        {
            // Arrange
            var userId = 1;
            var session = new Session { UserName = "admin", UserId = userId };
            var token = Guid.NewGuid().ToString();

            var sqlUserRepositoryMock = new Mock<IUsersRepository>();
            var jobsRepositoryMock = new Mock<IJobsRepository>();
            var serviceLogRepositoryMock = new Mock<IServiceLogRepository>();
            sqlUserRepositoryMock.Setup(a => a.IsInstanceAdmin(It.IsAny<bool>(), It.IsAny<int>())).
                Returns(Task.FromResult(false));

            var controller = new JobsController(jobsRepositoryMock.Object, serviceLogRepositoryMock.Object, sqlUserRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Headers.Add("Session-Token", token);
            controller.Request.Properties[ServiceConstants.SessionProperty] = session;

            // Act
            await controller.GetLatestJobs();

            // Assert
            jobsRepositoryMock.Verify(
                a => a.GetVisibleJobs(userId, It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<JobType>()), Times.Once());
        }
        #endregion
    }
}
