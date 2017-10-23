using AdminStore.Repositories.Jobs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Jobs;
using ServiceLibrary.Repositories.ConfigControl;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Results;
using ServiceLibrary.Repositories.Jobs;

namespace AdminStore.Controllers
{
    [TestClass]
    public class JobsControllerProcessTestGenerationTests
    {
        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task QueueGenerateProcessTestsJob_NullParameter_ThrowsBadRequest()
        {
            var jobsRepositoryMock = new Mock<IJobsRepository>();
            var serviceLogRepositoryMock = new Mock<IServiceLogRepository>();
            var controller = new JobsController(jobsRepositoryMock.Object, serviceLogRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            // Act
            try
            {
                await controller.QueueGenerateProcessTestsJob(null);
            }
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.QueueJobEmptyRequest);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task QueueGenerateProcessTestsJob_ProjectIdLessThanOne_ThrowsBadRequest()
        {
            var jobsRepositoryMock = new Mock<IJobsRepository>();
            var serviceLogRepositoryMock = new Mock<IServiceLogRepository>();
            var controller = new JobsController(jobsRepositoryMock.Object, serviceLogRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            var param = new GenerateProcessTestsJobParameters();
            param.ProjectId = 0;
            param.ProjectName = "test";
            param.Processes.Add(new GenerateProcessTestInfo() { ProcessId = 1 });

            // Act
            try
            {
                await controller.QueueGenerateProcessTestsJob(param);
            }
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.QueueJobProjectIdInvalid);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task QueueGenerateProcessTestsJob_ProjectNameEmpty_ThrowsBadRequest()
        {
            var jobsRepositoryMock = new Mock<IJobsRepository>();
            var serviceLogRepositoryMock = new Mock<IServiceLogRepository>();
            var controller = new JobsController(jobsRepositoryMock.Object, serviceLogRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            var param = new GenerateProcessTestsJobParameters();
            param.ProjectId = 1;
            param.ProjectName = "";
            param.Processes.Add(new GenerateProcessTestInfo() { ProcessId = 1 });

            // Act
            try
            {
                await controller.QueueGenerateProcessTestsJob(param);
            }
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.QueueJobProjectNameEmpty);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task QueueGenerateProcessTestsJob_ProcessesEmpty_ThrowsBadRequest()
        {
            var jobsRepositoryMock = new Mock<IJobsRepository>();
            var serviceLogRepositoryMock = new Mock<IServiceLogRepository>();
            var controller = new JobsController(jobsRepositoryMock.Object, serviceLogRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            var param = new GenerateProcessTestsJobParameters();
            param.ProjectId = 1;
            param.ProjectName = "test";

            // Act
            try
            {
                await controller.QueueGenerateProcessTestsJob(param);
            }
            catch (BadRequestException ex)
            {
                Assert.AreEqual(ex.ErrorCode, ErrorCodes.QueueJobProcessesInvalid);
                throw;
            }
        }

        [TestMethod]
        public async Task QueueGenerateProcessTestsJob_ValidParameters_ReturnsCreatedResponse()
        {
            var session = new Session { UserName = "admin", UserId = 1 };
            var token = Guid.NewGuid().ToString();
            var jobsRepositoryMock = new Mock<IJobsRepository>();
            var serviceLogRepositoryMock = new Mock<IServiceLogRepository>();
            jobsRepositoryMock
                 .Setup(j => j.AddJobMessage(It.IsAny<JobType>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(),
                         It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                 .Returns(Task.FromResult<int?>(2));

            var controller = new JobsController(jobsRepositoryMock.Object, serviceLogRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Headers.Add("Session-Token", token);
            controller.Request.Properties[ServiceConstants.SessionProperty] = session;
            HttpContext.Current = new HttpContext(
                new HttpRequest("", "http://localhost:9801/svc/adminstore/jobs/test/process", ""),
                new HttpResponse(new StringWriter()));

            var param = new GenerateProcessTestsJobParameters();
            param.ProjectId = 1;
            param.ProjectName = "test";
            param.Processes.Add(new GenerateProcessTestInfo() { ProcessId = 1 });

            // Act
            var actionResult = (await controller.QueueGenerateProcessTestsJob(param)) as CreatedNegotiatedContentResult<AddJobResult>;


            Assert.IsNotNull(actionResult);
            Assert.AreEqual(actionResult.Content.JobId, 2);
        }

        [TestMethod]
        public async Task QueueGenerateProcessTestsJob_ValidParameters_ReturnsLocationHeader()
        {
            var session = new Session { UserName = "admin", UserId = 1 };
            var token = Guid.NewGuid().ToString();
            var jobsRepositoryMock = new Mock<IJobsRepository>();
            var serviceLogRepositoryMock = new Mock<IServiceLogRepository>();
            jobsRepositoryMock
                 .Setup(j => j.AddJobMessage(It.IsAny<JobType>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(),
                         It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                 .Returns(Task.FromResult<int?>(2));

            var controller = new JobsController(jobsRepositoryMock.Object, serviceLogRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Headers.Add("Session-Token", token);
            controller.Request.Properties[ServiceConstants.SessionProperty] = session;
            HttpContext.Current = new HttpContext(
                new HttpRequest("", "http://localhost:9801/svc/adminstore/jobs/test/process", ""),
                new HttpResponse(new StringWriter()));

            var param = new GenerateProcessTestsJobParameters();
            param.ProjectId = 1;
            param.ProjectName = "test";
            param.Processes.Add(new GenerateProcessTestInfo() { ProcessId = 1 });

            // Act
            var actionResult = (await controller.QueueGenerateProcessTestsJob(param)) as CreatedNegotiatedContentResult<AddJobResult>;

            Assert.IsNotNull(actionResult);
            Assert.AreEqual(actionResult.Location.OriginalString, "/svc/adminstore/jobs/2");
        }
    }
}
