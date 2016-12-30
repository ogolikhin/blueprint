using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.Impl;
using Model.JobModel;
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
    public class JobsTests : TestBase
    {
        protected const string JOBS_PATH = RestPaths.Svc.AdminStore.JOBS;
        protected const int DEFAULT_PAGE_VALUE = 1;
        protected const int DEFAULT_PAGESIZE_VALUE = 10;
        protected const int DEFAULT_BASELINEORREVIEWID = 83;

        private List<IProject> _allProjects = null;
        private IProject _project = null;
        private IUser _adminUser = null;
        //private IUser _authorUser = null; // TODO add test cases using this

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _allProjects = ProjectFactory.GetAllProjects(_adminUser);
            _project = _allProjects.First();
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _adminUser);
            //_authorUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);
            // TODO add test cases using this
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region 200 OK Tests

        // TODO: Create the dedicated baseline artifact for creating ALM ChangeSummary Job
        [Category(Categories.CustomData)]
        [TestCase(DEFAULT_BASELINEORREVIEWID)]
        [TestRail(213053)]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [Description("Add ALM jobs. Execute GET Jobs - Verify that the returned JobResult.")]
        public void GetJobs_AddJobs_VerifyJobResult(int baselineArtifactId)
        {
            // Setup: Create an ALM ChangeSummary job using the prepared ALM target
            IProject projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_adminUser);
            var almTarget = AlmTarget.GetAlmTargets(Helper.ArtifactStore.Address, _adminUser, projectCustomData).First();
            OpenAPIJob.AddAlmChangeSummaryJob(Helper.ArtifactStore.Address, _adminUser, projectCustomData, baselineArtifactId, almTarget);

            // Execute: Execute GetJobs without using any optional parameters (page, pageSize, and jobType)
            List<IJobInfo> jobResult = null;
            Assert.DoesNotThrow(() => jobResult = Helper.AdminStore.GetJobs(_adminUser),
                "GET {0} call failed when using it without using any optional parameter!", JOBS_PATH);

            // Validation: Verify that jobResult uses DefaultPage, DefaultPageSize
            JobResultValidation(jobResult: jobResult, page: DEFAULT_PAGE_VALUE, pageSize: DEFAULT_PAGESIZE_VALUE);
        }

        // TODO: Create the dedicated baseline artifact for creating ALM ChangeSummary Job
        [Category(Categories.CustomData)]
        [TestCase(DEFAULT_BASELINEORREVIEWID)]
        [TestRail(213052)]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [Description("GET Jobs using a user doesn't have permission to projects. Execute Get Jobs - Verify that the returned empty JobResult.")]
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
        [Description("Retrieve Jobs with missing 'Session-Token' header in the request. Execute GET Jobs - Must return 401 Unautorized")]
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
        [Description("Retrieve Jobs with invalid 'Session-Token' header in the request. Execute GET Jobs - Must return 401 Unautorized")]
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
        /// Asserts that returned jobResult from the Nova GET Jobs call match with jobs that are being retrieved.
        /// </summary>
        /// <param name="jobResult">The jobResult from Nova GET jobs call.</param>
        /// <param name="jobsToBeFound"> (optional) jobs that are expected to be found, if this is null, job content validation step gets skipped.</param>
        /// <param name="page"> (optional) page value that represents displaying page number of the jobResult</param>
        /// <param name="pageSize"> (optional) pageSize value that indicates number of items that get displayed per page</param>
        private static void JobResultValidation(List<IJobInfo> jobResult,
            List<IJobInfo> jobsToBeFound = null,
            int? page = null,
            int? pageSize = null
            )
        {
            ThrowIf.ArgumentNull(jobResult, nameof(jobResult));

            // Setup: Set comparison values
            jobsToBeFound = jobsToBeFound ?? new List<IJobInfo>();
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
