using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.Factories;
using Model.JobModel.Impl;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TestCommon;
using Utilities;
using Utilities.Factories;

namespace AdminStoreTests
{
    [TestFixture]
    [Category(Categories.AdminStore)]
    public class ScheduleJobsTests : TestBase
    {
        protected const string TESTGEN_PATH = RestPaths.Svc.AdminStore.Jobs.Process.TESTGEN;

        private static string QueueJobProcessesInvalidErrMsg = "Please provide valid processes to generate job";
        private static string QueueJobProjectIdInvalidErrMsg = "Please provide a valid project id";
        private static string QueueJobProjectNameEmptyErrMsg = "Please provide the project name";

        private Dictionary<int, string> ErrorCodeToMessageMap { get; } = new Dictionary<int, string>
        {
            { ErrorCodes.QueueJobProcessesInvalid, QueueJobProcessesInvalidErrMsg },
            { ErrorCodes.QueueJobProjectIdInvalid, QueueJobProjectIdInvalidErrMsg },
            { ErrorCodes.QueueJobProjectNameEmpty, QueueJobProjectNameEmptyErrMsg }
        };

        private IUser _adminUser = null;
        private IUser _authorUser = null;
        private IProject _project = null;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_adminUser,shouldRetrievePropertyTypes: true);
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _adminUser);
            _authorUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);
        }

        [TestFixtureTearDown]
        public void TestFixtureTeardown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region 201 Created Tests

        [Explicit(IgnoreReasons.UnderDevelopmentDev)]
        [TestCase]
        [TestRail(227305)]
        [Description("POST QueueGenerateProcessTestsJob using the published process. Verify that the returned AddJobResult contains valid information.")]
        public void QueueGenerateProcessTestsJob_ScheduleProcessTestGenJobWithValidTestsJobParameters_VerifyAddJobResult()
        {
            // Setup: Create and publish a process
            var publishedProcessArtifactResult = CreateAndPublishRandomlyNamedProcess(_adminUser, _project);
            var publishedProcessArtifacts = publishedProcessArtifactResult.Artifacts;
            var processTestJobParameterRequest = GenerateProcessTestsJobParameters(_project, publishedProcessArtifacts);

            // Execution: Execute QueueGenerateProcessTestsJob call
            AddJobResult addJobResult = null;
            Assert.DoesNotThrow(() => addJobResult = Helper.AdminStore.QueueGenerateProcessTestsJob(_adminUser,
                processTestJobParameterRequest),
                "POST {0} call failed!", TESTGEN_PATH);

            // Validation: Verify that the returned ProcessTestGenerationResult contains valid information
            AddJobResultValidation(addJobResult);
        }

        [Explicit(IgnoreReasons.UnderDevelopmentDev)]
        [TestCase]
        [TestRail(227321)]
        [Description("POST QueueGenerateProcessTestsJob using an author. Verify that the returned AddJobResult contains valid information.")]
        public void QueueGenerateProcessTestsJob_ScheduleProcessTestGenJobWithAuthor_VerifyAddJobResult()
        {
            // Setup: Create and publish a process using an author
            var publishedProcessArtifactResult = CreateAndPublishRandomlyNamedProcess(_authorUser, _project);
            var publishedProcessArtifacts = publishedProcessArtifactResult.Artifacts;
            var processTestJobParameterRequest = GenerateProcessTestsJobParameters(_project, publishedProcessArtifacts);

            // Execution: Execute QueueGenerateProcessTestsJob call using the author
            AddJobResult addJobResult = null;
            Assert.DoesNotThrow(() => addJobResult = Helper.AdminStore.QueueGenerateProcessTestsJob(_authorUser,
                processTestJobParameterRequest),
                "POST {0} call failed when used with an author user!", TESTGEN_PATH);

            // Validation: Verify that the returned ProcessTestGenerationResult contains valid information
            AddJobResultValidation(addJobResult);
        }

        #endregion 201 Created Tests

        #region 400 Bad Request Tests

        [Explicit(IgnoreReasons.UnderDevelopmentDev)]
        [TestCase(-1, ErrorCodes.QueueJobProcessesInvalid)]
        [TestCase(0, ErrorCodes.QueueJobProcessesInvalid)]
        [TestRail(227306)]
        [Description("POST QueueGenerateProcessTestsJob with invalid ProcessId in TestsJobParameters. Verify that that 400 bad request is returned.")]
        public void QueueGenerateProcessTestsJob_WithInvalidProcessIdInTestsJobParameters_400BadRequest(
            int invalidProcessId,
            int errorCode)
        {
            // Setup: Create and publish a process then create a invalid TestJobParameters by using invalid process Id
            var invalidProcessInfo = new GenerateProcessTestInfo( invalidProcessId );
            var processTestJobParameterRequest = new GenerateProcessTestsJobParameters(
                _project.Id,
                _project.Name,
                new List<GenerateProcessTestInfo>() { invalidProcessInfo }
                );

            // Execution: Execute QueueGenerateProcessTestsJob with invalid TestsJobParameters
            var ex = Assert.Throws<Http400BadRequestException>(() => Helper.AdminStore.QueueGenerateProcessTestsJob(_adminUser,
                processTestJobParameterRequest),
                "POST {0} call should return 400 Bad Request when used with invalid TestsJobParameters!", TESTGEN_PATH);

            // Validation: Verify that error code returned from the error response
            string expectedExceptionMessage = ErrorCodeToMessageMap[errorCode];
            TestHelper.ValidateServiceError(ex.RestResponse, errorCode, expectedExceptionMessage);
        }

        [Explicit(IgnoreReasons.UnderDevelopmentDev)]
        [TestCase(-1, ErrorCodes.QueueJobProjectIdInvalid)]
        [TestCase(0, ErrorCodes.QueueJobProjectIdInvalid)]
        [TestRail(227338)]
        public void QueueGenerateProcessTestsJob_WithInvalidProjectIdinTestsJobParameters_400BadRequest(
            int invalidProjectId,
            int errorCode)
        {
            // Setup: Create and publish a process then create a invalid TestJobParameters by using invalid project Id
            var publishedProcessArtifactResult = CreateAndPublishRandomlyNamedProcess(_adminUser, _project);
            var processInfo = new GenerateProcessTestInfo(publishedProcessArtifactResult.Artifacts.First().Id);
            var processTestJobParameterRequest = new GenerateProcessTestsJobParameters(
                invalidProjectId,
                _project.Name,
                new List<GenerateProcessTestInfo>() { processInfo }
                );

            // Execution: Execute QueueGenerateProcessTestsJob with invalid TestsJobParameters
            var ex = Assert.Throws<Http400BadRequestException>(() => Helper.AdminStore.QueueGenerateProcessTestsJob(_adminUser,
                processTestJobParameterRequest),
                "POST {0} call should return 400 Bad Request when used with invalid TestsJobParameters!", TESTGEN_PATH);

            // Validation: Verify that error code returned from the error response
            string expectedExceptionMessage = ErrorCodeToMessageMap[errorCode];
            TestHelper.ValidateServiceError(ex.RestResponse, errorCode, expectedExceptionMessage);
        }

        [Explicit(IgnoreReasons.UnderDevelopmentDev)]
        [TestCase("", ErrorCodes.QueueJobProjectNameEmpty)]
        [TestRail(227339)]
        public void QueueGenerateProcessTestsJob_WithInvalidProjectNameInTestsJobParameters_400BadRequest(
            string invalidProjectName,
            int errorCode)
        {
            // Setup: Create and publish a process then create a invalid TestJobParameters by using invalid project name
            var publishedProcessArtifactResult = CreateAndPublishRandomlyNamedProcess(_adminUser, _project);
            var processInfo = new GenerateProcessTestInfo(publishedProcessArtifactResult.Artifacts.First().Id);
            var processTestJobParameterRequest = new GenerateProcessTestsJobParameters(
                _project.Id,
                invalidProjectName,
                new List<GenerateProcessTestInfo>() { processInfo }
                );

            // Execution: Execute QueueGenerateProcessTestsJob with invalid TestsJobParameters
            var ex = Assert.Throws<Http400BadRequestException>(() => Helper.AdminStore.QueueGenerateProcessTestsJob(_adminUser,
                processTestJobParameterRequest),
                "POST {0} call should return 400 Bad Request when used with invalid TestsJobParameters!", TESTGEN_PATH);

            // Validation: Verify that error code returned from the error response
            string expectedExceptionMessage = ErrorCodeToMessageMap[errorCode];
            TestHelper.ValidateServiceError(ex.RestResponse, errorCode, expectedExceptionMessage);
        }

        [Explicit(IgnoreReasons.UnderDevelopmentDev)]
        [TestCase]
        [TestRail(227322)]
        [Description("POST QueueGenerateProcessTestsJob with empty TestsJobParameters. Verify that that 400 bad request is returned.")]
        public void QueueGenerateProcessTestsJob_ScheduleProcessTestGenJobWithEmptyTestsJobParameters_400BadRequest()
        {
            // Setup: Create and publish a process then create an empty GenerateProcessTestsJobParameters
            var processTestJobParameterRequest = new GenerateProcessTestsJobParameters();

            // Execution: Execute QueueGenerateProcessTestsJob with invalid TestsJobParameters
            var ex = Assert.Throws<Http400BadRequestException>(() => Helper.AdminStore.QueueGenerateProcessTestsJob(_adminUser,
                processTestJobParameterRequest),
                "POST {0} call should return 400 Bad Request when used with invalid TestsJobParameters!", TESTGEN_PATH);

            // Validation: Verify that error code returned from the error response
            string expectedExceptionMessage = ErrorCodeToMessageMap[ErrorCodes.QueueJobProjectIdInvalid];
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.QueueJobProjectIdInvalid, expectedExceptionMessage);
        }

        #endregion 400 Bad Request Tests

        #region 401 Unauthorized Tests

        [Explicit(IgnoreReasons.UnderDevelopmentDev)]
        [TestCase]
        [TestRail(227340)]
        [Description("POST QueueGenerateProcessTestsJob with missing 'Session-Token' header in the request. Verify that the call returns 401 Unautorized.")]
        public void QueueGenerateProcessTestsJob_ExecuteWithMissingTokenHeader_401Unauthorized()
        {
            // Setup: Not required

            // Execution: Execute QueueGenerateProcessTestsjobs using the user with missing session token header
            var ex = Assert.Throws<Http401UnauthorizedException>(() => Helper.AdminStore.QueueGenerateProcessTestsJob(
                user: null,
                processTestJobParametersRequest: null
                ),
                "POST {0} call should return 401 Unauthorized if no Session-Token header is provided!", TESTGEN_PATH);

            // Validation: Exception should contain expected message.
            const string expectedExceptionMessage = "Token is missing or malformed";
            StringAssert.Contains(expectedExceptionMessage, ex.RestResponse.Content,
                "{0} was not found in returned message of POST QueueGenerateProcessTestsJob which has no session token.", expectedExceptionMessage);
        }

        [Explicit(IgnoreReasons.UnderDevelopmentDev)]
        [TestCase]
        [TestRail(227341)]
        [Description("POST QueueGenerateProcessTestsJob with invalid 'Session-Token' header in the request. Verify that the call return 401 Unautorized.")]
        public void QueueGenerateProcessTestsJob_ExecuteWithInvalidSessionToken_401Unauthorized()
        {
            // Setup: Not required
            var userWithBadToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            // Execution: Execute QueueGenerateProcessTestsjobs using the user with invalid session token
            var ex = Assert.Throws<Http401UnauthorizedException>(() => Helper.AdminStore.QueueGenerateProcessTestsJob(
                user: userWithBadToken,
                processTestJobParametersRequest: null
                ),
                "POST {0} call should return 401 Unauthorized if an invalid token is provided!", TESTGEN_PATH);

            // Validation: Exception should contain expected message.
            const string expectedExceptionMessage = "Token is invalid";
            StringAssert.Contains(expectedExceptionMessage, ex.RestResponse.Content,
                "{0} was not found in returned message of POST QueueGenerateProcessTestsJob which has invalid token.", expectedExceptionMessage);
        }

        #endregion 401 Unauthorized Tests

        #region 403 Forbidden Tests
        // TODO: Add tests here after the user story is completed
        #endregion 403 Forbidden Tests

        #region 404 Not Found Tests
        // TODO: Add tests here after the user story is completed
        #endregion 404 Not Found Tests

        #region private functions

        /// <summary>
        /// Create and Publish process artifact with randomized name
        /// </summary>
        /// <param name="user">user for authentication</param>
        /// <param name="project">The target project</param>
        /// <returns>INovaArtifactsAndProjectsResponse that represents the published artifact</returns>
        private INovaArtifactsAndProjectsResponse CreateAndPublishRandomlyNamedProcess(IUser user, IProject project)
        {
            var process = Helper.ArtifactStore.CreateArtifact(
                user,
                ItemTypePredefined.Process,
                RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10),
                project
                );
            var processArtifact = Helper.WrapNovaArtifact(process, project, user, BaseArtifactType.Process);
            return Helper.ArtifactStore.PublishArtifact(processArtifact, user);
        }

        /// <summary>
        /// Create GenerateProcessTestsJobParameters used for QueueGenerateProcessTestsJob call
        /// </summary>
        /// <param name="project">The target project</param>
        /// <param name="artifacts">Nova artifact respones list which will be use for the Process Tests generation job</param>
        /// <returns></returns>
        private static GenerateProcessTestsJobParameters GenerateProcessTestsJobParameters(
            IProject project,
            List<INovaArtifactResponse> artifacts
            )
        {
            var generateProcessTestInfoList = new List<GenerateProcessTestInfo>();
            artifacts.ForEach(a => generateProcessTestInfoList.Add(new GenerateProcessTestInfo(a.Id)));
            var generateProcessTestsJobParameters = new GenerateProcessTestsJobParameters(
                project.Id, project.Name, generateProcessTestInfoList);
            return generateProcessTestsJobParameters;
        }

        // TODO: This is a work in process validation function. Once it's finalized, will be moved to AdminStoreHelper
        /// <summary>
        /// Validates AddJobResult so that it contains valid data
        /// </summary>
        /// <param name="addJobResult">The returned addJobResult returned from QueueGenerateProcessTestsJob call</param>
        private static void AddJobResultValidation(AddJobResult addJobResult)
        {
            Assert.IsNotNull(addJobResult.JobId, "job Id from the returned AddJobResult should not be null!");
        }

        #endregion private functions

    }
}
