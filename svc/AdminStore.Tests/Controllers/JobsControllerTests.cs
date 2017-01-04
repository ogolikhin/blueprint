using System;
using System.Net.Http;
using System.Threading.Tasks;
using AdminStore.Repositories.Jobs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Jobs;
using ServiceLibrary.Repositories.ConfigControl;

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
        }

        #endregion Constuctor

        #region GetLatestJobs

        [TestMethod]
        [ExpectedException(typeof(AuthenticationException))]
        public async Task GetLatestJobs_UnauthenticatedUser_ThrowsAuthenticationException()
        {
            // Arrange
            var jobsRepositoryMock = new Mock<IJobsRepository>();
            var serviceLogRepositoryMock = new Mock<IServiceLogRepository>();
            var controller = new JobsController(jobsRepositoryMock.Object, serviceLogRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            // Act
            await controller.GetLatestJobs();
        }

        [TestMethod]
        public async Task GetLatestJobs_NegativePage_UsesMinimumOffset()
        {
            // Arrange
            var userId = 1;
            var session = new Session { UserName = "admin", UserId = userId };
            var token = Guid.NewGuid().ToString();
            var jobsRepositoryMock = new Mock<IJobsRepository>();
            var serviceLogRepositoryMock = new Mock<IServiceLogRepository>();
            var controller = new JobsController(jobsRepositoryMock.Object, serviceLogRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Headers.Add("Session-Token", token);
            controller.Request.Properties[ServiceConstants.SessionProperty] = session;

            // Act
            await controller.GetLatestJobs(-5);

            // Assert
            jobsRepositoryMock.Verify
            (
                a => a.GetVisibleJobs(userId, 0, It.IsAny<int?>(), It.IsAny<JobType>()),
                Times.Once()
            );
        }

        [TestMethod]
        public async Task GetLatestJobs_NegativePageSize_UsesDefaultLimit()
        {
            // Arrange
            var userId = 1;
            var session = new Session { UserName = "admin", UserId = userId };
            var token = Guid.NewGuid().ToString();
            var jobsRepositoryMock = new Mock<IJobsRepository>();
            var serviceLogRepositoryMock = new Mock<IServiceLogRepository>();
            var controller = new JobsController(jobsRepositoryMock.Object, serviceLogRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Headers.Add("Session-Token", token);
            controller.Request.Properties[ServiceConstants.SessionProperty] = session;

            // Act
            await controller.GetLatestJobs(null, -10);

            // Assert
            jobsRepositoryMock.Verify
            (
                a => a.GetVisibleJobs(userId, It.IsAny<int?>(), 10, It.IsAny<JobType>()),
                Times.Once()
            );
        }

        #endregion GetLatestJobs

        #region GetJob

        [TestMethod]
        [ExpectedException(typeof(AuthenticationException))]
        public async Task GetJob_UnauthenticatedUser_ThrowsAuthenticationException()
        {
            // Arrange
            var jobId = 1;
            var jobsRepositoryMock = new Mock<IJobsRepository>();
            var serviceLogRepositoryMock = new Mock<IServiceLogRepository>();
            var controller = new JobsController(jobsRepositoryMock.Object, serviceLogRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            // Act
            await controller.GetJob(jobId);
        }

        #endregion GetJob
    }
}
