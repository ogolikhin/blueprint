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
            Assert.DoesNotThrow(() => addJobResult = Helper.AdminStore.QueueGenerateProcessTestsJob(_adminUser, processTestJobParameterRequest),
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
            Assert.DoesNotThrow(() => addJobResult = Helper.AdminStore.QueueGenerateProcessTestsJob(_authorUser, processTestJobParameterRequest),
                "POST {0} call failed when used with an author user!", TESTGEN_PATH);

            // Validation: Verify that the returned ProcessTestGenerationResult contains valid information
            AddJobResultValidation(addJobResult);
        }

        #endregion 201 Created Tests

        #region 400 Bad Request Tests

        [Explicit(IgnoreReasons.UnderDevelopmentDev)]
        [TestCase(-1, null, null, ErrorCodes.QueueJobProcessesInvalid)]
        [TestCase(0, null, null, ErrorCodes.QueueJobProcessesInvalid)]
        [TestCase(null, -1, null, ErrorCodes.QueueJobProjectIdInvalid)]
        [TestCase(null, 0, null, ErrorCodes.QueueJobProjectIdInvalid)]
        [TestCase(null, null, null, ErrorCodes.QueueJobProjectNameEmpty)]
        [TestCase(null, null, "", ErrorCodes.QueueJobProjectNameEmpty)]
        [TestRail(227306)]
        [Description("POST QueueGenerateProcessTestsJob with invalid TestsJobParameters. Verify that that 400 bad request is returned.")]
        public void QueueGenerateProcessTestsJob_ScheduleProcessTestGenJobWithInvalidTestsJobParameters_400BadRequest(
            int? invalidProcessId = null,
            int? invalidProjectId = null,
            string invalidProjectName = null,
            int? errorCode = null
            )
        {
            // Setup: Create and publish a process then create a invalid TestJobParameters by using invalid ProcessId
            var publishedProcessArtifactResult = CreateAndPublishRandomlyNamedProcess(_adminUser, _project);
            var invalidProcessInfo = new GenerateProcessTestInfo(
                invalidProcessId?? publishedProcessArtifactResult.Artifacts.First().Id );
            var processTestJobParameterRequest = new GenerateProcessTestsJobParameters(
                invalidProjectId?? _project.Id,
                string.IsNullOrEmpty(invalidProjectName) ? invalidProjectName : _project.Name,
                new List<GenerateProcessTestInfo>() { invalidProcessInfo }
                );

            // Execution: Execute QueueGenerateProcessTestsJob with invalid TestsJobParameters
            var ex = Assert.Throws<Http400BadRequestException>(() => Helper.AdminStore.QueueGenerateProcessTestsJob(_adminUser, processTestJobParameterRequest),
                "POST {0} call should return 400 Bad Request when used with invalid TestsJobParameters!", TESTGEN_PATH);

            // Validation: Verify that error code returned from the error response
            string expectedExceptionMessage = ErrorCodeToMessageMap[(int)errorCode];
            TestHelper.ValidateServiceError(ex.RestResponse, (int)errorCode, expectedExceptionMessage);
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
            var ex = Assert.Throws<Http400BadRequestException>(() => Helper.AdminStore.QueueGenerateProcessTestsJob(_adminUser, processTestJobParameterRequest),
                "POST {0} call should return 400 Bad Request when used with invalid TestsJobParameters!", TESTGEN_PATH);

            // Validation: Verify that error code returned from the error response
            string expectedExceptionMessage = "Please provide the project name";
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.QueueJobProjectNameEmpty, expectedExceptionMessage);
        }

        #endregion 400 Bad Request Tests

        #region 401 Unauthorized Tests
        // TODO: Add tests here after the user story is completed
        #endregion 401 Unauthorized Tests

        #region 404 Not Found Tests
        // TODO: Add tests here after the user story is completed
        #endregion 404 Not Found Tests

        #region private functions

        /// <summary>
        /// Create and Publish process artifact with randomized name
        /// </summary>
        /// <param name="user">user for authentication</param>
        /// <param name="project">The target project</param>
        /// <returns></returns>
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
            Assert.IsNotNull(addJobResult.JobMessageId, "job Id from the returned AddJobResult should not be null!");
        }

        #endregion private functions

    }
}
