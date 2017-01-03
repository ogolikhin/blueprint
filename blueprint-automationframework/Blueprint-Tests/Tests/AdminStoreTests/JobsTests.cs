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

namespace CommonServiceTests
{
    [TestFixture]
    [Category(Categories.AdminStore)]
    [Category(Categories.CustomData)]
    public class JobsTests : TestBase
    {
        protected const string JOBS_PATH = RestPaths.Svc.AdminStore.JOBS;
        protected const int DEFAULT_PAGE_VALUE = 1;
        protected const int DEFAULT_PAGESIZE_VALUE = 10;
        protected const int DEFAULT_BASELINEORREVIEWID = 83;

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

        [TestCase(DEFAULT_BASELINEORREVIEWID, 2)]
        [TestRail(213053)]
        [Description("GET Jobs without using optional parameters. Verify that the returned JobResult uses default page and pageSize values.")]
        public void GetJobs_GetJobsWithoutOptionalParameters_VerifyJobResult(int baselineOrReviewId, int numberOfJobsToBeCreated)
        {
            // Setup: Create an ALM Change Summary job using the prepared ALM target
            List<IOpenAPIJob> jobsToBeFound = CreateALMSummaryJobsSetup(baselineOrReviewId, numberOfJobsToBeCreated, _projectCustomData);

            // Execute: GetJobs without using any optional parameters (page, pageSize, and jobType)
            List<IJobInfo> jobResult = null;
            Assert.DoesNotThrow(() => jobResult = Helper.AdminStore.GetJobs(_adminUser),
                "GET {0} call failed when using it without using any optional parameter!", JOBS_PATH);

            // Validation: Verify that jobResult uses DefaultPage and DefaultPageSize
            JobResultValidation(jobResult: jobResult, jobsToBeFound: jobsToBeFound, page: DEFAULT_PAGE_VALUE, pageSize: DEFAULT_PAGESIZE_VALUE);
        }

        [TestCase(DEFAULT_BASELINEORREVIEWID, 2, 1, 1 )]
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
            JobResultValidation(jobResult: jobResult, jobsToBeFound: jobsToBeFound, page: page, pageSize: pageSize);
        }

        [TestCase(DEFAULT_BASELINEORREVIEWID, 2)]
        [TestRail(227082)]
        [Description("GET Jobs using the jobType that doesn't match with jobs created for the test. Verify that the returned empty JobResult.")]
        public void GetJobs_GetJobsWithJobType_VerifyJobResultUsesJobType(
            int baselineOrReviewId,
            int numberOfJobsToBeCreated
            )
        {
            // Setup: Create ALM Change Summary jobs
            CreateALMSummaryJobsSetup(baselineOrReviewId, numberOfJobsToBeCreated, _projectCustomData);

            // Execute: GetJobs with JobType not which is not ALM Change Summary (DocGen)
            List<IJobInfo> jobResult = null;
            Assert.DoesNotThrow(() => jobResult = Helper.AdminStore.GetJobs(_adminUser, jobType: JobType.DocGen),
                "GET {0} call failed when using it with jobType ({1})!", JOBS_PATH, JobType.DocGen);

            // Validation: Verify that jobType filter works by checking the empty jobResult from Get Jobs call
            JobResultValidation(jobResult);

        }

        [TestCase(DEFAULT_BASELINEORREVIEWID, 2, -1, -1)]
        [TestCase(DEFAULT_BASELINEORREVIEWID, 2, 0, 0)]
        [TestRail(227083)]
        [Description("GET Jobs using invalid page and pageSize parameters. Verify that the default page and pageSize values are used for returned JobResult.")]
        public void GetJobs_GetJobsWithInvdlidPageAndPageSize_VerifyJobResultUsesFirstPageAndDefaultPageSize(
            int baselineOrReviewId,
            int numberOfJobsToBeCreated,
            int page,
            int pageSize
            )
        {
            // Setup: Create ALM Change Summary jobs
            List<IOpenAPIJob> jobsToBeFound = CreateALMSummaryJobsSetup(baselineOrReviewId, numberOfJobsToBeCreated, _projectCustomData);

            // Execute: GetJobs with invalid page and pageSize parameter values
            List<IJobInfo> jobResult = null;
            Assert.DoesNotThrow(() => jobResult = Helper.AdminStore.GetJobs(_adminUser, page: page, pageSize: pageSize),
                "GET {0} call failed when using it with invalid page ({1}) and invalid pageSize ({2})!", JOBS_PATH, page, pageSize);

            // Validation: Verify that page and pageSize works
            JobResultValidation(jobResult: jobResult, jobsToBeFound: jobsToBeFound, page: DEFAULT_PAGE_VALUE,pageSize: DEFAULT_PAGESIZE_VALUE);
        }

        [TestCase(DEFAULT_BASELINEORREVIEWID, 2)]
        [TestRail(227084)]
        [Description("GET Jobs using the author user after creating jobs with admin. Verify that the returned jobResult contains jobs belong to the user which is nothing.")]
        public void GetJobs_GetJobsWithAuthor_VerifJobResultContainsJobsBelongToTheUser(
            int baselineOrReviewId,
            int numberOfJobsToBeCreated
            )
        {
            // Setup: Create ALM Change Summary jobs (using admin)
            CreateALMSummaryJobsSetup(baselineOrReviewId, numberOfJobsToBeCreated, _projectCustomData);

            // Execute: Execute GetJobs using the author user
            List<IJobInfo> jobResult = null;
            Assert.DoesNotThrow(() => jobResult = Helper.AdminStore.GetJobs(_authorUser),
                "GET {0} call failed when using a author user!", JOBS_PATH);

            // Validation: Verify that empty jobResult since the author user doesn't have permission to view jobs created by admin
            JobResultValidation(jobResult);
        }

        [TestCase(DEFAULT_BASELINEORREVIEWID)]
        [TestRail(213052)]
        [Description("GET Jobs using a user doesn't have permission to projects. Verify that the returned empty JobResult.")]
        public void GetJobs_SearchWithoutPermissionOnProjects_VerifyEmptyJobResult(int baselineArtifactId)
        {
            // Setup: Create an ALM ChangeSummary job using the prepared ALM target
            IProject projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_adminUser);
            var almTarget = AlmTarget.GetAlmTargets(Helper.ArtifactStore.Address, _adminUser, projectCustomData).First();
            OpenAPIJob.AddAlmChangeSummaryJob(Helper.ArtifactStore.Address, _adminUser, projectCustomData, baselineArtifactId, almTarget);

            // Create user with no permission on any project
            var userWithNoPermissionOnAnyProject = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.None, _allProjects);

            // Execute: Execute GetJobs using the user with no permission on any project
            List<IJobInfo> jobResult = null;
            Assert.DoesNotThrow(() => jobResult = Helper.AdminStore.GetJobs(userWithNoPermissionOnAnyProject),
                "GET {0} call failed when using a user doesn't have permission to projects!",
                JOBS_PATH);

            // Validation: Verify that jobResult is empty
            JobResultValidation(jobResult);
        }

        #endregion 200 OK Tests

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
        /// <returns></returns>
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
        /// <param name="jobResult">The jobResult from Nova GET jobs call.</param>
        /// <param name="jobsToBeFound"> (optional) jobs that are expected to be found, if this is null, job content validation step gets skipped.</param>
        /// <param name="page"> (optional) page value that represents displaying page number of the jobResult</param>
        /// <param name="pageSize"> (optional) pageSize value that indicates number of items that get displayed per page</param>
        private static void JobResultValidation(List<IJobInfo> jobResult,
            List<IOpenAPIJob> jobsToBeFound = null,
            int? page = null,
            int? pageSize = null
            )
        {
            ThrowIf.ArgumentNull(jobResult, nameof(jobResult));

            // Setup: Set comparison values
            jobsToBeFound = jobsToBeFound ?? new List<IOpenAPIJob>();
            page = page ?? DEFAULT_PAGE_VALUE;
            pageSize = pageSize ?? DEFAULT_PAGESIZE_VALUE;

            page = page.Equals(0) ? DEFAULT_PAGE_VALUE : page;
            pageSize = pageSize.Equals(0) ? DEFAULT_PAGESIZE_VALUE : pageSize;

            List<int> returnedJobIds = new List<int>();

            if (jobsToBeFound.Any())
            {
                jobResult.ForEach(a => returnedJobIds.Add(a.JobId));

                for (int i = 0; i < Math.Min(jobsToBeFound.Count, (int)pageSize); i++)
                {
                    Assert.That(returnedJobIds.Contains(jobsToBeFound[i].JobId),
                        "The expected job whose JobId is {0} does not exist on the response from the Nova GET Jobs call.",
                        jobsToBeFound[i].JobId);
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
