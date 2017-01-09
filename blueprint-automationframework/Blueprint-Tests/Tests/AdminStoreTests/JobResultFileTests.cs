using CustomAttributes;
using Helper;
using Model;
using NUnit.Framework;
using TestCommon;
using Utilities;

namespace AdminStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    [Category(Categories.CustomData)]
    public class JobResultFileTests : TestBase
    {
        protected const string JOBRESULTFILE_PATH = RestPaths.Svc.AdminStore.Jobs_id_.RESULT.FILE;
        protected const int PROJECTEXPORT_NONEXPIREDFILE_JOBID = 3;
        protected const int PROJECTEXPORT_EXPIREDFILE_JOBID = 4;

        //private List<IProject> _allProjects = null;
        private IProject _projectCustomData = null;
        private IUser _adminUser = null;
        //private IUser _authorUser = null;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            //_allProjects = ProjectFactory.GetAllProjects(_adminUser);
            _projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_adminUser);
            _projectCustomData.GetAllNovaArtifactTypes(Helper.ArtifactStore, _adminUser);
            //_authorUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _projectCustomData);
            // TODO: implement test cases use _authorUser
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region 200 OK Tests

        // TODO: complete the test cases
        [TestCase(PROJECTEXPORT_NONEXPIREDFILE_JOBID)]
        [TestRail(0)]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [Description("GET JobResultFile using the jobId of ProjectExport which has non expired output file. Verify that valid project file is returned.")]
        public void GetJobResultFile_ExecuteWithNonExpiredProjectExportJobId_VerifyGetExportedProjectFile(int jobId)
        {
            // Setup: Use the prepared ProjectExport job

            // Execute: Execute GetJobResultFile to get exported project data
            Assert.DoesNotThrow(() => Helper.AdminStore.GetJobResultFile(_adminUser, jobId),
                "Get {0} call failed using job Id {1}!", JOBRESULTFILE_PATH, jobId);

            // Validation: Verify that the call returned exported project data
            // TODO: implement validation steps
        }

        #endregion 200 OK Tests

        #region 401 Unauthorized Tests

        [TestCase(int.MaxValue)]
        [TestRail(227237)]
        [Description("GET JobResultFile with missing 'Session-Token' header in the request. Verify that the call returns 401 Unautorized.")]
        public void GetJobResultFile_ExecuteWithMissingSessionTokenHeader_401Unauthorized(int jobId)
        {
            // Setup: Not required

            // Execute: Execute GetJobResultFile using the user with missing session token header
            var ex = Assert.Throws<Http401UnauthorizedException>(() => Helper.AdminStore.GetJobResultFile(user: null, jobId: jobId),
                "GET {0} call should return 401 Unauthorized if no Session-Token header was passed!", JOBRESULTFILE_PATH);

            // Validation: Exception should contain expected message.
            const string expectedExceptionMessage = "Token is missing or malformed";
            StringAssert.Contains(expectedExceptionMessage, ex.RestResponse.Content,
                "{0} was not found in returned message of Nova GET Jobs which has no session token.", expectedExceptionMessage);
        }

        [TestCase(int.MaxValue)]
        [TestRail(227238)]
        [Description("GET JobResultFile with invalid 'Session-Token' header in the request. Verify that the call return 401 Unautorized.")]
        public void GetJobResultFile_ExecuteWithInvalidSessionToken_401Unauthorized(int jobId)
        {
            // Setup: Not required
            IUser userWithBadToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            // Execute: Execute GetJobResultFile  using the user with invalid session token
            var ex = Assert.Throws<Http401UnauthorizedException>(() => Helper.AdminStore.GetJobResultFile(user: userWithBadToken, jobId: jobId),
                "GET {0} call should return 401 Unauthorized when using invalid session!", JOBRESULTFILE_PATH);

            // Validation: Exception should contain expected message.
            const string expectedExceptionMessage = "Token is invalid";
            StringAssert.Contains(expectedExceptionMessage, ex.RestResponse.Content,
                "{0} was not found in returned message of Nova GET Jobs which has invalid token", expectedExceptionMessage);
        }

        #endregion 401 Unauthorized Tests
    }
}
