using Common;
using CustomAttributes;
using Helper;
using Model;
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
    public class JobResultFileTests : TestBase
    {
        protected const string JOBRESULTFILE_PATH = RestPaths.Svc.AdminStore.Jobs_id_.Result.FILE;
        protected const int DEFAULT_BASELINEORREVIEWID = 83;

        private IProject _projectCustomData = null;
        private IUser _adminUser = null;

        private const int PROJECTEXPORT_NONEXPIREDFILE_JOBID = 3;
        private const int PROJECTEXPORT_EXPIREDFILE_JOBID = 4;
        private const int DOCGEN_JOBID = 5;
        private const string PROJECTEXPORT_NONEXPIREDFILE_FILEGUID = "b909ba6c-b1d6-e611-8144-12f00423610b";
        private const string PROJECTEXPORT_EXPIREDFILE_FILEGUID = "4d056821-b2d6-e611-8144-12f00423610b";
        private Dictionary<int, string> JobIdToFileGuidMap { get; } = new Dictionary<int, string>
        {
            { PROJECTEXPORT_NONEXPIREDFILE_JOBID, PROJECTEXPORT_NONEXPIREDFILE_FILEGUID },
            { PROJECTEXPORT_EXPIREDFILE_JOBID, PROJECTEXPORT_EXPIREDFILE_FILEGUID}
        };

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_adminUser);
            _projectCustomData.GetAllNovaArtifactTypes(Helper.ArtifactStore, _adminUser);
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region 200 OK Tests

        [Category(Categories.GoldenData)]
        [TestCase(PROJECTEXPORT_NONEXPIREDFILE_JOBID)]
        [TestRail(227239)]
        [Description("GET JobResultFile using the jobId of ProjectExport which has non expired output file. Verify that valid project file is returned.")]
        public void GetJobResultFile_ExecuteWithNonExpiredProjectExportJobId_VerifyGetExportedProjectFile(int jobId)
        {
            // Setup: Use the prepared ProjectExport job

            // Execute: Execute GetJobResultFile to get the exported project data and update guid for file validation step
            IFile outputFile = null;
            Assert.DoesNotThrow(() => outputFile = Helper.AdminStore.GetJobResultFile(_adminUser, jobId),
                "Get {0} call failed using job Id {1}!", JOBRESULTFILE_PATH, jobId);
            outputFile.Guid = JobIdToFileGuidMap[jobId];

            // Validation: Verify that the call returned exported project data
            var storedFile = Helper.FileStore.GetFile(JobIdToFileGuidMap[jobId], _adminUser);
            FileStoreTestHelper.AssertFilesAreIdentical(storedFile, outputFile);
        }

        #endregion 200 OK Tests

        #region 400 Bad Request Tests

        [Category(Categories.GoldenData)]
        [TestCase(DEFAULT_BASELINEORREVIEWID)]
        [TestRail(227241)]
        [Description("GET JobResultFile using the job which is not completed. Verify that 400 bad request is returned.")]
        public void GetJobResultFile_ExecuteWithNotCompletedJobId_400BadRequest(int baselineOrReviewId)
        {
            // Setup: Create a ALM ChangeSummary job using the prepared ALM target
            var createdJob = Helper.CreateALMSummaryJobsSetup(Helper.ArtifactStore.Address, _adminUser, baselineOrReviewId, 1, _projectCustomData).First();

            // Execute: Execute GetJobResultFile using the ALM ChangeSummary job Id which is not completed.
            var ex = Assert.Throws<Http400BadRequestException>(() => Helper.AdminStore.GetJobResultFile(_adminUser, createdJob.JobId),
                "GET {0} call should return 400 Bad Request when using with not completed job Id ({1})!", JOBRESULTFILE_PATH, createdJob.JobId);

            // Validation: Verify that the expected error code and error message are returned
            string expectedExceptionMessage = "Job is not completed";
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.JobNotCompleted, expectedExceptionMessage);
        }

        [Category(Categories.GoldenData)]
        [TestCase(DOCGEN_JOBID)]
        [TestRail(227242)]
        [Description("GET JobResultFile using the job which is not supported (DocGen). Verify that 400 bad request is returned.")]
        public void GetJobResultFile_ExecuteWithNonSupportedJobId_400BadRequest(int jobId)
        {
            // Setup: Use the prepared DocGen job which is not supported by GetJobResultFile call yet.

            // Execute: Execute GetJobResultFile using the ALM ChangeSummary job Id which is not being supported.
            var ex = Assert.Throws<Http400BadRequestException>(() => Helper.AdminStore.GetJobResultFile(_adminUser, jobId),
                "GET {0} call should return 400 Bad Request when using with non-supported job Id ({1})!", JOBRESULTFILE_PATH, jobId);

            // Validation: Verify that the expected error code and error message are returned
            string expectedExceptionMessage = "Job doesn't support downloadable result files";
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.ResultFileNotSupported, expectedExceptionMessage);
        }

        #endregion 400 Bad Request Tests

        #region 401 Unauthorized Tests

        [TestCase]
        [TestRail(227237)]
        [Description("GET JobResultFile with missing 'Session-Token' header in the request. Verify that the call returns 401 Unautorized.")]
        public void GetJobResultFile_ExecuteWithMissingSessionTokenHeader_401Unauthorized()
        {
            // Setup: Not required

            // Execute: Execute GetJobResultFile using the user with missing session token header
            var ex = Assert.Throws<Http401UnauthorizedException>(() => Helper.AdminStore.GetJobResultFile(user: null, jobId: int.MaxValue),
                "GET {0} call should return 401 Unauthorized if no Session-Token header was passed!", JOBRESULTFILE_PATH);

            // Validation: Exception should contain expected message.
            const string expectedExceptionMessage = "Token is missing or malformed";
            StringAssert.Contains(expectedExceptionMessage, ex.RestResponse.Content,
                "{0} was not found in returned message of Nova GET Jobs which has no session token.", expectedExceptionMessage);
        }

        [TestCase]
        [TestRail(227238)]
        [Description("GET JobResultFile with invalid 'Session-Token' header in the request. Verify that the call return 401 Unautorized.")]
        public void GetJobResultFile_ExecuteWithInvalidSessionToken_401Unauthorized()
        {
            // Setup: Not required
            var userWithBadToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            // Execute: Execute GetJobResultFile using the user with invalid session token
            var ex = Assert.Throws<Http401UnauthorizedException>(() => Helper.AdminStore.GetJobResultFile(user: userWithBadToken, jobId: int.MaxValue),
                "GET {0} call should return 401 Unauthorized when using invalid session!", JOBRESULTFILE_PATH);

            // Validation: Exception should contain expected message.
            const string expectedExceptionMessage = "Token is invalid";
            StringAssert.Contains(expectedExceptionMessage, ex.RestResponse.Content,
                "{0} was not found in returned message of Nova GET Jobs which has invalid token", expectedExceptionMessage);
        }

        #endregion 401 Unauthorized Tests

        #region 404 Not Found Tests

        [Category(Categories.GoldenData)]
        [TestCase(PROJECTEXPORT_EXPIREDFILE_JOBID)]
        [TestRail(227240)]
        [Description("GET JobResultFile using the jobId of ProjectExport which has expired output file. Verify that 404 NotFound is returned.")]
        public void GetJobResultFile_ExecuteWithExpiredProjectExportedJobId_404NotFound(int jobId)
        {
            // Setup: Use the prepared ProjectExport job

            // Execute: Execute GetJobResultFile to get the exported project data which is expired
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.AdminStore.GetJobResultFile(_adminUser, jobId),
                "GET {0} call should return 404 Not Found when using the job whose Id is {1}, which has expired output file!", JOBRESULTFILE_PATH, jobId);

            // Validation: Verify that expected error code and error message are returned
            string expectedExceptionMessage = I18NHelper.FormatInvariant("File with id {0} is not found", JobIdToFileGuidMap[jobId]);
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.ResourceNotFound, expectedExceptionMessage);
        }

        [TestCase(0)]
        [TestCase(-10)]
        [TestCase(int.MaxValue)]
        [TestRail(227244)]
        [Description("GET JobResultFile using the invalid jobId which doesn't exist. Verify that 404 NotFound is returned.")]
        public void GetJobResultFile_ExecuteWithInvalidJobId_404NotFound(int jobId)
        {
            // Setup: Not required

            // Execute and Verify: Execute GetJobResultFile with invalid job Id and verified that 404 NotFound is returned
            Assert.Throws<Http404NotFoundException>(() => Helper.AdminStore.GetJobResultFile(_adminUser, jobId),
                "GET {0} call should return 404 Not Found when using invalid job Id ({1})!", JOBRESULTFILE_PATH, jobId);
        }

        #endregion 404 Not Found Tests
    }
}
