using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Jobs;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.Files;
using ServiceLibrary.Repositories.Jobs;
using File = ServiceLibrary.Models.Files.File;
using FileInfo = ServiceLibrary.Models.Files.FileInfo;

namespace AdminStore.Repositories.Jobs
{
    [TestClass]
    public class JobsRepositoryTests
    {
        [TestMethod]
        public async Task GetJobResultFile_JobDoesNotExist_ThrowsResourceNotFoundException()
        {
            // Arrange
            const int jobId = 1;
            const int userId = 1;
            var fileRepositoryMock = new Mock<IFileRepository>();
            var jobsRepository = CreateJobsRepository();
            ResourceNotFoundException exception = null;

            // Act
            try
            {
                await jobsRepository.GetJobResultFile(jobId, userId, fileRepositoryMock.Object);
            }
            catch (ResourceNotFoundException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(ErrorCodes.ResourceNotFound, exception.ErrorCode);
        }

        [TestMethod]
        public async Task GetJobResultFile_JobIncomplete_ThrowsBadRequestException()
        {
            // Arrange
            const int jobId = 1;
            const int userId = 1;
            var job = CreateJob(jobId, JobType.ProjectExport, JobStatus.Running);
            var fileRepositoryMock = new Mock<IFileRepository>();
            var jobsRepository = CreateJobsRepository(job);
            BadRequestException exception = null;

            // Act
            try
            {
                await jobsRepository.GetJobResultFile(jobId, userId, fileRepositoryMock.Object);
            }
            catch (BadRequestException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(ErrorCodes.JobNotCompleted, exception.ErrorCode);
        }

        [TestMethod]
        public async Task GetJobResultFile_ProjectImport_ThrowsBadRequestException()
        {
            // Arrange
            const int jobId = 1;
            const int userId = 1;
            var job = CreateJob(jobId, JobType.ProjectImport);
            var fileRepositoryMock = new Mock<IFileRepository>();
            var jobsRepository = CreateJobsRepository(job);
            BadRequestException exception = null;

            // Act
            try
            {
                await jobsRepository.GetJobResultFile(jobId, userId, fileRepositoryMock.Object);
            }
            catch (BadRequestException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(ErrorCodes.ResultFileNotSupported, exception.ErrorCode);
        }

        [TestMethod]
        public async Task GetJobResultFile_NoFileRepository_ThrowsArgumentNullExceptionException()
        {
            // Arrange
            const int jobId = 1;
            const int userId = 1;
            var job = CreateJob(jobId, JobType.ProjectExport);
            var jobsRepository = CreateJobsRepository(job);
            ArgumentNullException exception = null;

            // Act
            try
            {
                await jobsRepository.GetJobResultFile(jobId, userId, null);
            }
            catch (ArgumentNullException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
        }

        [TestMethod]
        public async Task GetJobResultFile_ProjectExport_ReturnsFileWithContextStream()
        {
            // Arrange
            const int jobId = 1;
            const int userId = 1;
            const string result = "<ProjectExportTaskStatus><Id>0</Id><Details><FileGuid>891c41c0-bfd3-e611-a91e-d811ec5c30e3</FileGuid></Details></ProjectExportTaskStatus>";
            var job = CreateJob(jobId, JobType.ProjectExport, JobStatus.Completed, result);
            var fileRepositoryMock = new Mock<IFileRepository>();
            fileRepositoryMock
                .Setup(m => m.GetFileAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(new File { Info = new FileInfo(), ContentStream = new MemoryStream() }));
            var jobsRepository = CreateJobsRepository(job);

            // Act
            var file = await jobsRepository.GetJobResultFile(jobId, userId, fileRepositoryMock.Object);

            // Assert
            Assert.IsNotNull(file);
        }

        [TestMethod]
        public async Task GetJob_NullJob_Throws404ResourceNotFoundException()
        {
            const int jobId = 1;
            const int userId = 1;
            var job = CreateJob(jobId, JobType.ProjectExport, JobStatus.Running);
            var fileRepositoryMock = new Mock<IFileRepository>();
            var jobsRepository = CreateJobsRepository();
            ResourceNotFoundException exception = null;

            // Act
            try
            {
                await jobsRepository.GetJob(jobId, userId);
            }
            catch (ResourceNotFoundException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(ErrorCodes.ResourceNotFound, exception.ErrorCode);
        }

        private static DJobMessage CreateJob(int jobId, JobType type, JobStatus status = JobStatus.Completed, string result = null)
        {
            return new DJobMessage
            {
                JobMessageId = jobId,
                Type = type,
                Status = status,
                SubmittedTimestamp = DateTime.UtcNow,
                Result = result
            };
        }

        private static IJobsRepository CreateJobsRepository(params DJobMessage[] jobMessages)
        {
            var connection = new SqlConnectionWrapperMock();
            connection.SetupQueryAsync("GetJobMessage", new Dictionary<string, object>(), jobMessages);
            var artifactsMock = new Mock<ISqlArtifactRepository>();
            var permissionsMock = new Mock<IArtifactPermissionsRepository>();
            var usersMock = new Mock<IUsersRepository>();

            var jobsRepository = new JobsRepository(connection.Object, artifactsMock.Object, permissionsMock.Object, usersMock.Object);
            return jobsRepository;
        }
    }
}
