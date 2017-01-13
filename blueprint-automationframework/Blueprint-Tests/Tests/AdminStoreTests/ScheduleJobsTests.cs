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

        private IUser _adminUser = null;
        private IProject _project = null;

        #region Setup and Clearnup

        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_adminUser,shouldRetrievePropertyTypes: true);
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _adminUser);
        }

        [TestFixtureTearDown]
        public void TestFixtureTeardown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region 200 OK Tests

        [Explicit(IgnoreReasons.UnderDevelopmentDev)]
        [TestCase]
        [TestRail(227305)]
        [Description("POST QueueGenerateProcessTestsJob using the published process. Verify that the returned AddJobResult contains valid information (job Id).")]
        public void QueueGenerateProcessTestsJob_ScheduleProcessTestGenJobWithValidTestsJobParameters_VerifyAddJobResult()
        {
            // Setup: Create and publish a process
            var publishedProcessArtifactResult = CreateAndPublishRandomlyNamedProcess(_adminUser, _project);
            var generatedProcessInfo = new GenerateProcessTestInfo (publishedProcessArtifactResult.Artifacts.First().Id);

            var processTestJobParameterRequest = new GenerateProcessTestsJobParameters(
                publishedProcessArtifactResult.Projects.First().Id,
                publishedProcessArtifactResult.Projects.First().Name,
                new List<GenerateProcessTestInfo>() { generatedProcessInfo }
                );

            // Execution: Execute QueueGenerateProcessTestsJob call
            AddJobResult addJobResult = null;
            Assert.DoesNotThrow(() => addJobResult = Helper.AdminStore.QueueGenerateProcessTestsJob(_adminUser, processTestJobParameterRequest),
                "POST {0} call failed!", TESTGEN_PATH);

            // Validation: Vrify that the returned ProcessTestGenerationResult contain valid information (e.g. job id)
            AddJobResultValidation(addJobResult);
        }

        #endregion 200 OK Tests

        #region 400 Bad Request Tests

        [Explicit(IgnoreReasons.UnderDevelopmentDev)]
        [TestCase(-1)]
        [TestCase(int.MaxValue)]
        [TestCase(null)]
        [TestRail(227306)]
        [Description("POST QueueGenerateProcessTestsJob with invalid TestsJobParameters. Verify that that 400 bad request is returned.")]
        public void QueueGenerateProcessTestsJob_ScheduleProcessTestGenJobWithInvalidTestsJobParameters_400BadRequest(int invalidProcessId)
        {
            // Setup: Create and publish a process then create a invalid TestJobParameters by using invalid ProcessId
            var publishedProcessArtifactResult = CreateAndPublishRandomlyNamedProcess(_adminUser, _project);
            var invalidProcessInfo = new GenerateProcessTestInfo(invalidProcessId);

            var processTestJobParameterRequest = new GenerateProcessTestsJobParameters(
                publishedProcessArtifactResult.Projects.First().Id,
                publishedProcessArtifactResult.Projects.First().Name,
                new List<GenerateProcessTestInfo>() { invalidProcessInfo }
                );

            // Execution: Execute QueueGenerateProcessTestsJob with invalid TestsJobParameters
            var ex = Assert.Throws<Http400BadRequestException>(() => Helper.AdminStore.QueueGenerateProcessTestsJob(_adminUser, processTestJobParameterRequest),
                "POST {0} call should return 400 Bad Request when used with invalid TestsJobParameters!", TESTGEN_PATH);

            // Validation: Verify that error code returned from the error response
            string expectedExceptionMessage = "";
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.UnhandledException, expectedExceptionMessage);
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
