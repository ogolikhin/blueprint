using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.ModelHelpers;
using Model.StorytellerModel;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using System.Collections.Generic;
using TestCommon;
using Utilities;
using Utilities.Facades;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class BasicTests : TestBase
    {
        private IUser _adminUser;
        private IProject _project;

        private const string SVC_PATH = RestPaths.Svc.ArtifactStore.PROCESS_id_;
        private const string SVC_STORYTELLER_PROCESSES_PATH = RestPaths.Svc.Components.Storyteller.Projects_id_.PROCESSES;

        private const int DEFAULT_SHAPES_COUNT = 5;
        private const int DEFAULT_LINKS_COUNT = 4;
        private const int DEFAULT_PROPERTYVALUES_COUNT = 2;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_adminUser);
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region 200 OK Tests

        [TestCase]
        [TestRail(303355)]
        [Description("Get the default process after creating and saving a new process artifact.  Verify that the" +
                     "returned process has the same Id as the process artifact Id and that the numbers of " +
                     "shapes, links and property values are as expected.")]
        public void GetNovaProcess_GetDefaultProcess_VerifyReturnedProcess()
        {
            // Setup:
            var novaProcessArtifact = Helper.CreateNovaProcessArtifact(_adminUser, _project);

            // Execute:
            INovaProcess returnedNovaProcess = null;
            Assert.DoesNotThrow(() => returnedNovaProcess = Helper.Storyteller.GetNovaProcess(_adminUser, novaProcessArtifact.Id),
                "'GET {0}' should return 200 when valid process ID is passed.", SVC_PATH);

            // Verify: Verify that returned nova process is default nova process
            VerifyDefaultNovaProcess(novaProcessArtifact, returnedNovaProcess);
        }

        [TestCase]
        [TestRail(303359)]
        [Description("Create a default process on the project and get the list of all available processes on the project." +
            "Verify that the returned process list contains the process created.")]
        public void GetProcesses_CreatProcess_ReturnedListContainsCreatedProcess()
        {
            // Setup:
            var artifact = Helper.CreateNovaArtifact(_adminUser, _project, ItemTypePredefined.Process);

            // Execute:
            List<IProcess> processList = null;
            Assert.DoesNotThrow(() =>
            {
                processList = (List<IProcess>)Helper.Storyteller.GetProcesses(_adminUser, _project.Id);
            }, "GET {0} should return 200 when valid project ID is passe.", SVC_STORYTELLER_PROCESSES_PATH);

            // Verify: Verify that returned process from list of processes
            var returnedProcess = processList.Find(p => p.Name == artifact.Name);

            Assert.IsNotNull(returnedProcess, "List of processes must have newly created process, but it doesn't.");
        }

        [TestCase]
        [TestRail(107376)]
        [Description("Add a user task after an existing user task, discard")]
        public void DiscardArtifactAddedUserTask_VerifyResult()
        {
            // Setup:
            // Create and get the default process
            var novaProcess = Helper.CreateNovaProcessArtifact(_adminUser, _project);

            // Find the end shape
            var endShape = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);
            
            // Find the incoming link for the end shape
            var endIncomingLink = novaProcess.Process.GetIncomingLinkForShape(endShape);

            Assert.IsNotNull(endIncomingLink, "Process link was not found.");

            // Add a user/system task immediately before the end shape
            novaProcess.Process.AddUserAndSystemTask(endIncomingLink);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess.NovaProcess, _adminUser);

            // Execute: 
            INovaArtifactsAndProjectsResponse discardArtifactResponse = null;

            Assert.DoesNotThrow(() =>
            {
                discardArtifactResponse = novaProcess.Discard(_adminUser);
            }, "Discard must return no errors.");

            // Verify: 
            Assert.AreEqual(1, discardArtifactResponse.Artifacts.Count, "Only 1 artifact should be returned in discard results!");
            Assert.AreEqual(1, discardArtifactResponse.Projects.Count, "Only 1 project should be returned in discard results!");
        }

        #endregion 200 OK Tests

        #region 400 Bad Request Tests

        [TestRail(246536)]
        [TestCase("9999999999", "The request is invalid.")]
        [TestCase("&amp;", "A potentially dangerous Request.Path value was detected from the client (&).")]
        [Description("Create a rest path that tries to get a process with an invalid artifact Id. " +
                     "Attempt to get the process. Verify that HTTP 400 Bad Request exception is thrown.")]
        public void GetProcess_InvalidArtifactId_400BadRequest(string artifactId, string expectedErrorMessage)
        {
            // Setup:
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.Components.Storyteller.PROCESSES_id_, artifactId);

            var restApi = new RestApiFacade(Helper.ArtifactStore.Address, _adminUser?.Token?.AccessControlToken);

            // Execute & Verify:
            var ex = Assert.Throws<Http400BadRequestException>(() => restApi.SendRequestAndDeserializeObject<NovaArtifactResponse, object>(
               path,
               RestRequestMethod.GET,
               jsonObject: null),
                "We should get a 400 Bad Request when the artifact Id is invalid!");

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, expectedErrorMessage);
        }

        #endregion 400 Bad Request Tests

        #region private functions

        /// <summary>
        /// Verify the default nova process
        /// </summary>
        /// <param name="expectedDefaultArtifact">the nova artifact used to verify actual returned nova process</param>
        /// <param name="actualNovaProcess">the nova process to verify</param>
        private static void VerifyDefaultNovaProcess(ArtifactWrapper expectedDefaultArtifact, INovaProcess actualNovaProcess)
        {
            ThrowIf.ArgumentNull(expectedDefaultArtifact, nameof(expectedDefaultArtifact));
            ThrowIf.ArgumentNull(actualNovaProcess, nameof(actualNovaProcess));

            Assert.AreEqual(expectedDefaultArtifact.Id, actualNovaProcess.Id,
                "The expected ID of the returned process is '{0}', but '{1}' was returned.",
                actualNovaProcess.Id, expectedDefaultArtifact.Id);
            Assert.AreEqual(DEFAULT_SHAPES_COUNT, actualNovaProcess.Process.Shapes.Count,
                "The expected number of shapes in a default process is {0} but {1} shapes were returned.",
                DEFAULT_SHAPES_COUNT, actualNovaProcess.Process.Shapes.Count);
            Assert.AreEqual(DEFAULT_LINKS_COUNT, actualNovaProcess.Process.Links.Count,
                "The expected number of links in a default process is {0} but {1} links were returned.",
                DEFAULT_LINKS_COUNT, actualNovaProcess.Process.Links.Count);
            Assert.AreEqual(DEFAULT_PROPERTYVALUES_COUNT, actualNovaProcess.Process.PropertyValues.Count,
                "The expected number of property values in a default process is {0} but {1} property values were returned.",
                DEFAULT_PROPERTYVALUES_COUNT, actualNovaProcess.Process.PropertyValues.Count);
        }

        #endregion private functions
    }
}
