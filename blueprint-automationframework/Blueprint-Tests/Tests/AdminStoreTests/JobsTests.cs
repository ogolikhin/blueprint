using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.Impl;
using Model.JobModel;
using Model.JobModel.Enums;
using Model.JobModel.Impl;
using NUnit.Framework;
using System;
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
            _authorUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _projectCustomData);
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region 200 OK Tests

        [TestCase(DEFAULT_BASELINEORREVIEWID, 2, 1, 1 )]
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
            List<IOpenAPIJob> jobsToBeFound = CreateALMSummaryJobsSetup(baselineOrReviewId, numberOfJobsToBeCreated, _projectCustomData);

            // Execute: GetJobs with page and pageSize parameters
            List<IJobInfo> jobResult = null;
            Assert.DoesNotThrow(() => jobResult = Helper.AdminStore.GetJobs(_adminUser, page: page, pageSize: pageSize),
                "GET {0} call failed when using it with page ({1}) and pageSize ({2})!", JOBS_PATH, page, pageSize);

            // Validation: Verify that page and pageSize works
            JobResultValidation(jobResult: jobResult, pageSize: pageSize, expectedJobs: jobsToBeFound);
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
            CreateALMSummaryJobsSetup(baselineOrReviewId, numberOfJobsToBeCreated, _projectCustomData);

            // Execute: GetJobs with JobType not which is not ALM Change Summary (DocGen)
            List<IJobInfo> jobResult = null;
            Assert.DoesNotThrow(() => jobResult = Helper.AdminStore.GetJobs(_adminUser, jobType: JobType.DocGen, page: page, pageSize: pageSize),
                "GET {0} call failed when using it with jobType ({1})!", JOBS_PATH, JobType.DocGen);

            // Validation: Verify that jobType filter works by checking the empty jobResult from Get Jobs call
            JobResultValidation(jobResult: jobResult, pageSize: pageSize );

        }

        [TestCase(DEFAULT_BASELINEORREVIEWID, 2, 1, 1)]
        [TestRail(227084)]
        [Description("GET Jobs using the author user after creating jobs with admin. Verify that the returned jobResult contains jobs belong to the user which is nothing.")]
        public void GetJobs_GetJobsWithUserWithNoAccessToJobsCreatedByAdmin_VerifyEmtpyJobResult(
            int baselineOrReviewId,
            int numberOfJobsToBeCreated,
            int page,
            int pageSize
            )
        {
            // Setup: Create ALM Change Summary jobs (using admin)
            CreateALMSummaryJobsSetup(baselineOrReviewId, numberOfJobsToBeCreated, _projectCustomData);

            // Execute: Execute GetJobs using the author user
            List<IJobInfo> jobResult = null;
            Assert.DoesNotThrow(() => jobResult = Helper.AdminStore.GetJobs(_authorUser, page: page, pageSize: pageSize),
                "GET {0} call failed when using an author user!", JOBS_PATH);

            // Validation: Verify that jobResult is empty since the author user doesn't have permission to view jobs created by admin
            JobResultValidation(jobResult: jobResult, pageSize: pageSize);
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
            CreateALMSummaryJobsSetup(baselineOrReviewId, 1, _projectCustomData);

            // Create user with no permission on any project
            var userWithNoPermissionOnAnyProject = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.None, _allProjects);

            // Execute: Execute GetJobs using the user with no permission on any project
            List<IJobInfo> jobResult = null;
            Assert.DoesNotThrow(() => jobResult = Helper.AdminStore.GetJobs(userWithNoPermissionOnAnyProject, page: page, pageSize: pageSize),
                "GET {0} call failed when using a user doesn't have permission to projects!",
                JOBS_PATH);

            // Validation: Verify that jobResult is empty
            JobResultValidation(jobResult: jobResult, pageSize: pageSize);
        }

        #endregion 200 OK Tests

        #region 400 Bad Request Tests

        [TestCase(null, 1, ErrorCodes.PageNullOrNegative)]
        [TestCase(1,null, ErrorCodes.PageSizeNullOrOutOfRange)]
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

        #endregion 401 Unauthorized Tests

        #region private functions

        /// <summary>
        /// Create ALM Change Summary Jobs as a setup for testing Nova GET Jobs API calls
        /// </summary>
        /// <param name="baselineOrReviewId">The baseline or review artifact ID.</param>
        /// <param name="numberOfJobsToBeCreated">The number of ALM Change Summary Jobs to be created.</param>
        /// <param name="project">The project where ALM targets reside.</param>
        /// <returns> List of ALM Summary Jobs created in decending order by jobId </returns>
        private List<IOpenAPIJob> CreateALMSummaryJobsSetup(int baselineOrReviewId, int numberOfJobsToBeCreated, IProject project)
        {
            var almTarget = AlmTarget.GetAlmTargets(Helper.ArtifactStore.Address, _adminUser, project).First();
            Assert.IsNotNull(almTarget, "ALM target does not exist on the project {0}!", project.Name);
            List<IOpenAPIJob> jobsToBeFound = new List<IOpenAPIJob>();
            for (int i = 0; i < numberOfJobsToBeCreated; i++)
            {
                var openAPIJob = OpenAPIJob.AddAlmChangeSummaryJob(Helper.ArtifactStore.Address, _adminUser, project, baselineOrReviewId, almTarget);
                jobsToBeFound.Add(openAPIJob);
            }
            jobsToBeFound.Reverse();
            return jobsToBeFound;
        }

        /// <summary>
        /// Asserts that returned jobResult from the Nova GET Jobs call match with jobs that are being retrieved.
        /// </summary>
        /// <param name="jobResult">The jobResult from Nova GET jobs call in decending order by jobId</param>
        /// <param name="pageSize"> pageSize value that indicates number of items that get displayed per page</param>
        /// <param name="expectedJobs"> (optional) jobs that are expected to be found in decending order by jobId, if this is null, job content validation step gets skipped.</param>
        private static void JobResultValidation(List<IJobInfo> jobResult,
            int pageSize,
            List<IOpenAPIJob> expectedJobs = null
            )
        {
            ThrowIf.ArgumentNull(jobResult, nameof(jobResult));

            expectedJobs = expectedJobs ?? new List<IOpenAPIJob>();

            if (expectedJobs.Any())
            {
                // Job Contents comparison and validation
                var compareCount = Math.Min(expectedJobs.Count, pageSize);
                var jobsToBeFoundToCompare = expectedJobs.Take(compareCount).ToList();

                for (int i = 0; i < compareCount; i++)
                {
                    Assert.AreEqual(jobsToBeFoundToCompare[i].JobId, jobResult[i].JobId,
                        "The jobId {0} was expected but jobId {1} is returned from GET jobs call.",
                        jobsToBeFoundToCompare[i].JobId, jobResult[i].JobId);

                    Assert.AreEqual(jobsToBeFoundToCompare[i].ProjectId, jobResult[i].ProjectId,
                        "The projectId {0} was expected but projectId {1} is returned from GET jobs call.",
                        jobsToBeFoundToCompare[i].ProjectId, jobResult[i].ProjectId);

                    Assert.IsTrue(jobsToBeFoundToCompare[i].ProjectName.Contains(jobResult[i].Project),
                        "The projectName {0} was expected to contain project value {1} from GET jobs call.",
                        jobsToBeFoundToCompare[i].ProjectName, jobResult[i].Project);

                    Assert.AreEqual(jobsToBeFoundToCompare[i].JobType, jobResult[i].JobType,
                        "The jobType {0} was expected but jobType {1} is returned from GET jobs call.",
                        jobsToBeFoundToCompare[i].JobType, jobResult[i].JobType);

                    Assert.AreEqual(jobsToBeFoundToCompare[i].SubmittedDateTime.ToStringInvariant(),
                        jobResult[i].SubmittedDateTime.ToStringInvariant(), "The SubmittedDateTime {0} was expected but SubmittedDateTime {1} is returned from GET jobs call.",
                        jobsToBeFoundToCompare[i].SubmittedDateTime.ToStringInvariant(), jobResult[i].SubmittedDateTime.ToStringInvariant());
                }
            }
            else
            {
                Assert.AreEqual(0, jobResult.Count(),
                    "The jobResult should be null list when expected return result is empty but the response from the Nova GET Jobs call returns {0} results",
                    jobResult.Count());
            }

            // Validation: Verify that jobResult uses pageSize values passed as optional parameters
            Assert.That(jobResult.Count() <= pageSize,
                "The expected pagesize value is {0} but {1} was found from the returned searchResult.",
                pageSize, jobResult.Count());
        }

        #endregion private functions
    }
}
