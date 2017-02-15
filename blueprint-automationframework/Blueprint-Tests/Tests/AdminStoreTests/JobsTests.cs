using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.Factories;
using Model.JobModel;
using Model.JobModel.Enums;
using Model.JobModel.Impl;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TestCommon;
using Utilities;

namespace AdminStoreTests
{
    [TestFixture]
    [Category(Categories.AdminStore)]
    [Category(Categories.CustomData)]
    public class JobsTests : TestBase
    {
        protected const string JOB_PATH = RestPaths.Svc.AdminStore.JOBS_id_;
        protected const string JOBS_PATH = RestPaths.Svc.AdminStore.JOBS;
        protected const int MAXIMUM_PAGESIZE_VALUE = 200;
        protected const int DEFAULT_BASELINEORREVIEWID = 83;

        private static string PageNullOrNegativeErrMsg = "Page value must be provided and be greater than 0";
        private static string PageSizeNullOrOutOfRangeErrMsg = I18NHelper.FormatInvariant("Page Size value must be provided and value between 1 and {0}", MAXIMUM_PAGESIZE_VALUE);

        private Dictionary<int, string> ErrorCodeToMessageMap { get; } = new Dictionary<int, string>
        {
            { ErrorCodes.PageNullOrNegative, PageNullOrNegativeErrMsg },
            { ErrorCodes.PageSizeNullOrOutOfRange, PageSizeNullOrOutOfRangeErrMsg}
        };

        private List<IProject> _allProjects = null;
        private IProject _projectCustomData = null;
        private IUser _adminUser = null;
        private IUser _authorUser = null;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _allProjects = ProjectFactory.GetAllProjects(_adminUser);
            _projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_adminUser);
            _projectCustomData.GetAllNovaArtifactTypes(Helper.ArtifactStore, _adminUser);
            _authorUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _projectCustomData);
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region 200 OK Tests

        [TestCase(DEFAULT_BASELINEORREVIEWID, 2, 1, 1)]
        [TestCase(DEFAULT_BASELINEORREVIEWID, 2, 1, 10)]
        [TestRail(227081)]
        [Description("GET Jobs using page and pageSize parameters. Verify that the returned JobResult use page and pageSize parameters.")]
        public void GetJobs_GetJobsWithPageAndPageSize_VerifyJobResultUsesPageAndPageSize(
            int baselineOrReviewId,
            int numberOfJobsToBeCreated,
            int page,
            int pageSize
            )
        {
            // Setup: Create ALM Change Summary jobs
            var jobsToBeFound = Helper.CreateALMSummaryJobsSetup(Helper.ArtifactStore.Address, _adminUser, baselineOrReviewId, numberOfJobsToBeCreated, _projectCustomData);

            // Execute: GetJobs with page and pageSize parameters
            JobResult jobResult = null;
            Assert.DoesNotThrow(() => jobResult = Helper.AdminStore.GetJobs(_adminUser, page: page, pageSize: pageSize),
                "GET {0} call failed when using it with page ({1}) and pageSize ({2})!", JOBS_PATH, page, pageSize);

            // Validation: Verify that page and pageSize works
            AdminStoreHelper.GetJobsValidation(jobResult: jobResult, pageSize: pageSize, expectedOpenAPIJobs: jobsToBeFound);
        }

        [TestCase(10, 4)]
        [TestRail(227295)]
        [Description("GET Jobs that returns multiple pages for JobResult. Verify that number of result items matches with expecting search result items.")]
        public void GetJobs_CreateMoreJobsThanThePageSize_VerifyCorrectNumberOfPagesAvailable(
            int numberOfJobsToBeCreated,
            int pageSize
            )
        {
            //Setup: Schedules Process test generation job(s) with the provided process using the author user
            var publishedProcessArtifacts = Helper.CreateAndPublishMultipleArtifacts(_projectCustomData, _adminUser, BaseArtifactType.Process, 1);
            var processTestJobParameterRequest = AdminStoreHelper.GenerateProcessTestsJobParameters(_projectCustomData, publishedProcessArtifacts);
            var jobsToBeFound = QueueMultipleGenerateProcessTestsJobs(_authorUser, processTestJobParameterRequest, numberOfJobsToBeCreated);

            // Calculate expecting job result counts
            var expectedJobResultCount = jobsToBeFound.Count();
            var expectedPageCount = (expectedJobResultCount % pageSize).Equals(0) ? expectedJobResultCount / pageSize : expectedJobResultCount / pageSize + 1;
            var expectedJobsStack = new Stack<AddJobResult>(jobsToBeFound);

            // Execute: Execute GetJobs with page and pageSize
            var returnedJobCount = 0;
            var pageCount = 1;

            while ( pageCount <= expectedPageCount)
            {
                // Execute GetJobs with page and pageSize
                JobResult jobResult = null;
                Assert.DoesNotThrow(() => jobResult = Helper.AdminStore.GetJobs(user: _authorUser, page: pageCount, pageSize: pageSize, jobType: JobType.GenerateProcessTests),
                    "GET {0} call failed when using it with page ({1}) and pageSize ({2})!", JOBS_PATH, pageCount, pageSize);

                // Adds job result per page into returned job count
                returnedJobCount += jobResult.JobInfos.Count();

                // Create a paged job list per page, decending ordered by Job Id
                List<AddJobResult> pagedJobs = ExtractAddJobResultsFromAddJobResultStack(expectedJobsStack, pageSize);

                // Validation: Verify that jobResult contains list of expectedJobs
                AdminStoreHelper.GetJobsValidation(jobResult: jobResult, pageSize: pageSize, expectedAddJobResults: pagedJobs);

                pageCount++;
            }

            // Validation: Verify that expected job count is equal to returned job count
            Assert.That(returnedJobCount.Equals(expectedJobResultCount),
                "Expected job result count is {0} but {1} was returned", expectedJobResultCount, returnedJobCount);
        }

        [TestCase(DEFAULT_BASELINEORREVIEWID, 2, 1, 1)]
        [TestRail(227082)]
        [Description("GET Jobs using the jobType that doesn't match with jobs created for the test. Verify that the returned empty JobResult.")]
        public void GetJobs_GetJobsWithJobTypeDifferentThanJobsStarted_VerifyEmptyJobResult(
            int baselineOrReviewId,
            int numberOfJobsToBeCreated,
            int page,
            int pageSize
            )
        {
            // Setup: Create ALM Change Summary jobs
            Helper.CreateALMSummaryJobsSetup(Helper.ArtifactStore.Address, _adminUser, baselineOrReviewId, numberOfJobsToBeCreated, _projectCustomData);

            // Execute: GetJobs with JobType not which is not ALM Change Summary (DocGen)
            JobResult jobResult = null;
            Assert.DoesNotThrow(() => jobResult = Helper.AdminStore.GetJobs(_adminUser, jobType: JobType.QcExport, page: page, pageSize: pageSize),
                "GET {0} call failed when using it with jobType ({1})!", JOBS_PATH, JobType.DocGen);

            // Validation: Verify that jobType filter works by checking the empty jobResult from Get Jobs call
            AdminStoreHelper.GetJobsValidation(jobResult: jobResult, pageSize: pageSize, expectedOpenAPIJobs: null);

        }

        [TestCase(2, 1, 1)]
        [TestRail(227084)]
        [Description("GET Jobs using the author user (author1) after creating jobs with different author (author2).  " +
            "Verify that the returned jobResult contains jobs that belong to the user which should be empty.")]
        public void GetJobs_GetJobsWithUserWithNoAccessToJobsCreatedByAnotherAuthor_VerifyEmtpyJobResult(
            int numberOfJobsToBeCreated,
            int page,
            int pageSize
            )
        {
            //Setup: Schedules Process test generation job(s) using the different author (author2)
            var author1 = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _allProjects.First());
            var author2 = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _allProjects.First());
            var publishedProcessArtifacts = Helper.CreateAndPublishMultipleArtifacts(_projectCustomData, _adminUser, BaseArtifactType.Process, 1);
            var processTestJobParameterRequest = AdminStoreHelper.GenerateProcessTestsJobParameters(_projectCustomData, publishedProcessArtifacts);
            QueueMultipleGenerateProcessTestsJobs(author2, processTestJobParameterRequest, numberOfJobsToBeCreated);

            // Execute: Execute GetJobs using the author user with no access to the sheduled process test generation job(s)
            JobResult jobResult = null;
            Assert.DoesNotThrow(() => jobResult = Helper.AdminStore.GetJobs(author1, page: page, pageSize: pageSize),
                "GET {0} call failed when using an author user!", JOBS_PATH);

            // Validation: Verify that jobResult is empty since the author user doesn't have permission to view jobs created by another author
            AdminStoreHelper.GetJobsValidation(jobResult: jobResult, pageSize: pageSize, expectedAddJobResults: null);
        }

        [TestCase(DEFAULT_BASELINEORREVIEWID, 1, 1)]
        [TestRail(213052)]
        [Description("GET Jobs using a user that doesn't have permission to projects. Verify that an empty JobResult is returned.")]
        public void GetJobs_SearchWithoutPermissionOnProjects_VerifyEmptyJobResult(
            int baselineOrReviewId,
            int page,
            int pageSize
            )
        {
            // Setup: Create an ALM ChangeSummary job using the prepared ALM target
            Helper.CreateALMSummaryJobsSetup(Helper.ArtifactStore.Address, _adminUser, baselineOrReviewId, 1, _projectCustomData);

            // Create user with no permission on any project
            var userWithNoPermissionOnAnyProject = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.None, _allProjects);

            // Execute: Execute GetJobs using the user with no permission on any project
            JobResult jobResult = null;
            Assert.DoesNotThrow(() => jobResult = Helper.AdminStore.GetJobs(userWithNoPermissionOnAnyProject, page: page, pageSize: pageSize),
                "GET {0} call failed when using a user doesn't have permission to projects!",
                JOBS_PATH);

            // Validation: Verify that jobResult is empty
            AdminStoreHelper.GetJobsValidation(jobResult: jobResult, pageSize: pageSize, expectedOpenAPIJobs: null);
        }

        [TestCase(DEFAULT_BASELINEORREVIEWID)]
        [TestRail(227220)]
        [Description("Get a Job with a user that created the job. Verify that the returned JobResult")]
        public void GetJob_GetTheJobCreated_VerifyJobResult(int baselineOrReviewId)
        {
            // Setup: Create a ALM ChangeSummary job using the prepared ALM target
            var createdJob = Helper.CreateALMSummaryJobsSetup(Helper.ArtifactStore.Address, _adminUser, baselineOrReviewId, 1, _projectCustomData).First();

            // Execute: Execute GetJob to retrieve the job using job ID and user
            IJobInfo returnedJobInfo = null;
            Assert.DoesNotThrow(() => returnedJobInfo = Helper.AdminStore.GetJob(_adminUser, createdJob.JobId),
                "Get {0} call failed when using job Id {1}!", JOB_PATH, createdJob.JobId);

            // Validation: Verify that jobResult is identical with job
            AdminStoreHelper.GetJobValidation(returnedJobInfo, createdJob);
        }

        #endregion 200 OK Tests

        #region 400 Bad Request Tests

        [TestCase(null, 1, ErrorCodes.PageNullOrNegative)]
        [TestCase(1, null, ErrorCodes.PageSizeNullOrOutOfRange)]
        [TestRail(213053)]
        [Description("GET Jobs without either page or pageSize. Verify that 400 bad request is returned.")]
        public void GetJobs_GetJobsWithoutEitherPageOrPageSize_400BadRequest(
            int page,
            int pageSize,
            int errorCode
            )
        {
            // Setup: Not required

            // Execute: GetJobs without page or pageSize optional parameters
            var ex = Assert.Throws<Http400BadRequestException>(() => Helper.AdminStore.GetJobs(_adminUser, page: page, pageSize: pageSize),
                "GET {0} call should return 400 Bad Request when using without page and pageSize optional parameters!", JOBS_PATH);

            // Validation: Verify that error code returned from the error response
            string expectedExceptionMessage = ErrorCodeToMessageMap[errorCode];
            TestHelper.ValidateServiceError(ex.RestResponse, errorCode, expectedExceptionMessage);
        }

        [TestCase(-1, -1, ErrorCodes.PageNullOrNegative)]
        [TestCase(-1, 1, ErrorCodes.PageNullOrNegative)]
        [TestCase(1, -1, ErrorCodes.PageSizeNullOrOutOfRange)]
        [TestCase(0, 0, ErrorCodes.PageNullOrNegative)]
        [TestRail(227083)]
        [Description("GET Jobs using invalid values for page and pageSize parameters. Verify that 400 bad request is returned.")]
        public void GetJobs_GetJobsWithInvalidPageAndPageSize_400BadRequest(
            int page,
            int pageSize,
            int errorCode
            )
        {
            // Setup: Not required

            // Execute: GetJobs with invalid page and pageSize parameter values
            var ex = Assert.Throws<Http400BadRequestException>(() => Helper.AdminStore.GetJobs(_adminUser, page: page, pageSize: pageSize),
                "GET {0} call should return 400 Bad Request when using invalid page ({1}) and invalid pageSize ({2}) parameters!", JOBS_PATH, page, pageSize);

            // Validation: Verify that error code returned from the error response
            string expectedExceptionMessage = ErrorCodeToMessageMap[errorCode];
            TestHelper.ValidateServiceError(ex.RestResponse, errorCode, expectedExceptionMessage);
        }

        #endregion 400 Bad Request Tests

        #region 401 Unauthorized Tests

        [TestCase]
        [TestRail(213050)]
        [Description("GET Jobs with missing 'Session-Token' header in the request. Verify that the call returns 401 Unautorized")]
        public void GetJobs_GetJobsWithMissingSessionTokenHeader_401Unauthorized()
        {
            // Setup: Not required

            // Execute: Execute GetJobs using the user with missing session token header
            var ex = Assert.Throws<Http401UnauthorizedException>(() => Helper.AdminStore.GetJobs(user: null),
                "GET {0} call should return 401 Unauthorized if no Session-Token header was passed!", JOBS_PATH);

            // Validation: Exception should contain expected message.
            const string expectedExceptionMessage = "Token is missing or malformed";
            StringAssert.Contains(expectedExceptionMessage, ex.RestResponse.Content,
                "{0} was not found in returned message of Nova GET Jobs which has no session token.", expectedExceptionMessage);
        }

        [TestCase]
        [TestRail(213051)]
        [Description("GET Jobs with invalid 'Session-Token' header in the request. Verify that the call return 401 Unautorized")]
        public void GetJobs_GetJobsWithInvalidSessionToken_401Unauthorized()
        {
            // Setup: Not required
            IUser userWithBadToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            // Execute: Execute GetJobs using the user with invalid session token
            var ex = Assert.Throws<Http401UnauthorizedException>(() => Helper.AdminStore.GetJobs(user: userWithBadToken),
                "GET {0} call should return 401 Unauthorized when using invalid session!", JOBS_PATH);

            // Validation: Exception should contain expected message.
            const string expectedExceptionMessage = "Token is invalid";
            StringAssert.Contains(expectedExceptionMessage, ex.RestResponse.Content,
                "{0} was not found in returned message of Nova GET Jobs which has invalid token", expectedExceptionMessage);
        }

        [TestCase(int.MaxValue)]
        [TestRail(227221)]
        [Description("GET a Job with missing 'Session-Token' header in the request. Verify that the call returns 401 Unautorized")]
        public void GetJob_GetJobWithMissingSessionTokenHeader_401Unauthorized(int jobId)
        {
            // Setup: not required

            // Execute: Execute GetJob using the user with missing session token header and dummy job Id
            var ex = Assert.Throws<Http401UnauthorizedException>(() => Helper.AdminStore.GetJob(user: null, jobId: jobId),
                "GET {0} call should return 401 Unauthorized if no Session-Token header was passed!", JOB_PATH);

            // Validation: Exception should contain expected message.
            const string expectedExceptionMessage = "Token is missing or malformed";
            StringAssert.Contains(expectedExceptionMessage, ex.RestResponse.Content,
                "{0} was not found in returned message of Nova GET Job which has no session token.", expectedExceptionMessage);
        }

        [TestCase(int.MaxValue)]
        [TestRail(227222)]
        [Description("GET a Job with invalid 'Session-Token' header in the request. Verify that the call return 401 Unautorized")]
        public void GetJob_GetJobWithInvalidSessionToken_401Unauthorized(int jobId)
        {
            // Setup: Not required
            var userWithBadToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            // Execute: Execute GetJob using the user with invalid session token and dummy job Id
            var ex = Assert.Throws<Http401UnauthorizedException>(() => Helper.AdminStore.GetJob(user: userWithBadToken, jobId: jobId),
                "GET {0} call should return 401 Unauthorized when using invalid session!", JOB_PATH);

            // Validation: Exception should contain expected message.
            const string expectedExceptionMessage = "Token is invalid";
            StringAssert.Contains(expectedExceptionMessage, ex.RestResponse.Content,
                "{0} was not found in returned message of Nova GET Job which has invalid token", expectedExceptionMessage);
        }

        #endregion 401 Unauthorized Tests

        #region 404 Not Found Tests

        [TestCase(int.MaxValue)]
        [TestCase(-1)]
        [TestCase(0)]
        [TestRail(227223)]
        [Description("GET a Job using invalid job Id. Verify that 404 Not Found is returned.")]
        public void GetJob_GetJobWithInvalidJobId_404NotFound(int jobId)
        {
            // Setup: Not required

            // Execute and Validation: GetJob with invalid job Id and Verify that 404 Not Found is returned.
            Assert.Throws<Http404NotFoundException>(() => Helper.AdminStore.GetJob(_adminUser, jobId),
                "GET {0} call should return 404 Not Found when using invalid job Id ({1})!", JOB_PATH, jobId);
        }

        #endregion 404 Not Found Tests

        #region private functions

        /// <summary>
        /// Creates the jobs list decending ordered by Job Id, extracted from the AddJobResult stack
        /// </summary>
        /// <param name="addJobResultStack">AddJobResult stack descending ordered by Job Id, jobs will be removed from this stack</param>
        /// <param name="pageSize">maximum number of jobs that will be on the paged job list</param>
        /// <returns>AddJobResultList representing the job list decending ordered by Job Id</returns>
        private static List<AddJobResult> ExtractAddJobResultsFromAddJobResultStack(Stack<AddJobResult> addJobResultStack, int pageSize)
        {
            var pagedAddJobResultList = new List<AddJobResult>();

            for (int i = 0; i < pageSize; i++)
            {
                if (addJobResultStack.Any())
                {
                    pagedAddJobResultList.Add(addJobResultStack.Pop());
                }
                else
                {
                    break;
                }
            }
            return pagedAddJobResultList;
        }

        /// <summary>
        /// Schedules multiple numbers of process test generation job with the provided processes
        /// </summary>
        /// <param name="user">The user who will schedule job(s).</param>
        /// <param name="processTestJobParametersRequest">parameter form required for adding process test generation job</param>
        /// <param name="numberOfArtifacts">The number of jobs to create.</param>
        /// <returns>List of AddJobResult</returns>
        private List<AddJobResult> QueueMultipleGenerateProcessTestsJobs(
            IUser user,
            GenerateProcessTestsJobParameters processTestJobParametersRequest,
            int numberOfArtifacts
            )
        {
            var addJobResultList = new List<AddJobResult>();

            for (int i = 0; i < numberOfArtifacts; i++)
            {
                var addJobResult = Helper.AdminStore.QueueGenerateProcessTestsJob(user, processTestJobParametersRequest);
                addJobResultList.Add(addJobResult);
            }

            return addJobResultList;
        }

        #endregion private functions
    }
}
