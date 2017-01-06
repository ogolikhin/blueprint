﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using AdminStore.Repositories.Jobs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
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

        #endregion

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
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetLatestJobs_NegativePage_ThrowsBadRequest()
        {
            // Arrange
            var userId = 1;
            var session = new Session { UserName = "admin", UserId = userId };
            var token = Guid.NewGuid().ToString();

            var sqlUserRepositoryMock = new Mock<IUsersRepository>();
            var jobsRepositoryMock = new Mock<IJobsRepository>();
            var serviceLogRepositoryMock = new Mock<IServiceLogRepository>();
            sqlUserRepositoryMock.Setup(a => a.IsInstanceAdmin(It.IsAny<bool>(), It.IsAny<int>())).
                Returns(Task.FromResult(true));

            var controller = new JobsController(jobsRepositoryMock.Object, serviceLogRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Headers.Add("Session-Token", token);
            controller.Request.Properties[ServiceConstants.SessionProperty] = session;

            // Act
            try { 
                await controller.GetLatestJobs(-5);
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(BadRequestException));
                Assert.AreEqual(((ExceptionWithErrorCode)ex).ErrorCode, ErrorCodes.PageNullOrNegative);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetLatestJobs_NullPage_ThrowsBadRequest()
        {
            // Arrange
            var userId = 1;
            var session = new Session { UserName = "admin", UserId = userId };
            var token = Guid.NewGuid().ToString();

            var sqlUserRepositoryMock = new Mock<IUsersRepository>();
            var jobsRepositoryMock = new Mock<IJobsRepository>();
            var serviceLogRepositoryMock = new Mock<IServiceLogRepository>();
            sqlUserRepositoryMock.Setup(a => a.IsInstanceAdmin(It.IsAny<bool>(), It.IsAny<int>())).
                Returns(Task.FromResult(true));

            var controller = new JobsController(jobsRepositoryMock.Object, serviceLogRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Headers.Add("Session-Token", token);
            controller.Request.Properties[ServiceConstants.SessionProperty] = session;

            // Act
            try
            {
                await controller.GetLatestJobs();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(BadRequestException));
                Assert.AreEqual(((ExceptionWithErrorCode)ex).ErrorCode, ErrorCodes.PageNullOrNegative);
                throw ex;
            }

        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetLatestJobs_NullPageSize_ThrowsBadRequest()
        {
            // Arrange
            var userId = 1;
            var session = new Session { UserName = "admin", UserId = userId };
            var token = Guid.NewGuid().ToString();

            var sqlUserRepositoryMock = new Mock<IUsersRepository>();
            var jobsRepositoryMock = new Mock<IJobsRepository>();
            var serviceLogRepositoryMock = new Mock<IServiceLogRepository>();
            sqlUserRepositoryMock.Setup(a => a.IsInstanceAdmin(It.IsAny<bool>(), It.IsAny<int>())).
                Returns(Task.FromResult(true));

            var controller = new JobsController(jobsRepositoryMock.Object, serviceLogRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Headers.Add("Session-Token", token);
            controller.Request.Properties[ServiceConstants.SessionProperty] = session;

            // Act
            try { 
                await controller.GetLatestJobs(1, null);
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(BadRequestException));
                Assert.AreEqual(((ExceptionWithErrorCode)ex).ErrorCode, ErrorCodes.PageSizeNullOrOutOfRange);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetLatestJobs_NegativePageSize_ThrowsBadRequest()
        {
            // Arrange
            var userId = 1;
            var session = new Session { UserName = "admin", UserId = userId };
            var token = Guid.NewGuid().ToString();

            var sqlUserRepositoryMock = new Mock<IUsersRepository>();
            var jobsRepositoryMock = new Mock<IJobsRepository>();
            var serviceLogRepositoryMock = new Mock<IServiceLogRepository>();
            sqlUserRepositoryMock.Setup(a => a.IsInstanceAdmin(It.IsAny<bool>(), It.IsAny<int>())).
                Returns(Task.FromResult(true));

            var controller = new JobsController(jobsRepositoryMock.Object, serviceLogRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Headers.Add("Session-Token", token);
            controller.Request.Properties[ServiceConstants.SessionProperty] = session;

            // Act
            try { 
                await controller.GetLatestJobs(1, -10);
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(BadRequestException));
                Assert.AreEqual(((ExceptionWithErrorCode)ex).ErrorCode, ErrorCodes.PageSizeNullOrOutOfRange);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetLatestJobs_ExceededMaxPageSize_ThrowsBadRequest()
        {
            // Arrange
            var userId = 1;
            var session = new Session { UserName = "admin", UserId = userId };
            var token = Guid.NewGuid().ToString();

            var sqlUserRepositoryMock = new Mock<IUsersRepository>();
            var jobsRepositoryMock = new Mock<IJobsRepository>();
            var serviceLogRepositoryMock = new Mock<IServiceLogRepository>();
            sqlUserRepositoryMock.Setup(a => a.IsInstanceAdmin(It.IsAny<bool>(), It.IsAny<int>())).
                Returns(Task.FromResult(true));

            var controller = new JobsController(jobsRepositoryMock.Object, serviceLogRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Headers.Add("Session-Token", token);
            controller.Request.Properties[ServiceConstants.SessionProperty] = session;

            // Act
            try
            {
                await controller.GetLatestJobs(1, 201);
            }
            catch(Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(BadRequestException));
                Assert.AreEqual(((ExceptionWithErrorCode)ex).ErrorCode, ErrorCodes.PageSizeNullOrOutOfRange);
                throw ex;
            }
        }

        #endregion

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

        #region GetJobResultFile

        [TestMethod]
        [ExpectedException(typeof(AuthenticationException))]
        public async Task GetJobResultFile_UnauthenticatedUser_ThrowsAuthenticationException()
        {
            // Arrange
            var jobId = 1;
            var jobsRepositoryMock = new Mock<IJobsRepository>();
            var serviceLogRepositoryMock = new Mock<IServiceLogRepository>();
            var controller = new JobsController(jobsRepositoryMock.Object, serviceLogRepositoryMock.Object)
            {
                Request = new HttpRequestMessage
                {
                    RequestUri = new Uri("http://bptest.com/")
                }
            };

            // Act
            await controller.GetJobResultFile(jobId);
        }

        #endregion GetJobResultFile
    }
}
