using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.StorytellerModel;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TestCommon;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class UpdateNovaProcessTests : TestBase
    {
        private IUser _user;
        private IProject _project;
        private List<IProject> _allProjects = null;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _allProjects = ProjectFactory.GetAllProjects(_user);
            _project = _allProjects.First();
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            if (Helper?.Storyteller != null)
            {
                foreach (var novaProcess in Helper.Storyteller.NovaProcesses)
                {
                    Helper.Storyteller.DeleteNovaProcessArtifact(_user, novaProcess);
                }
            }

            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region Tests

        [TestCase]
        [TestRail(211705)]
        [Description("Update the process type of the process and verify that the returned Nova process " +
                     "has the updated process type. This verifies that the process type toggle in the" +
                     "process diagram is working.")]
        public void UpdateNovaProcess_ModifyProcessType_VerifyReturnedNovaProcess()
        {
            // Setup:
            // Create and get the default Nova process
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(Helper.Storyteller, _project, _user);
            var process = novaProcess.Process;
            var processType = process.ProcessType;

            Assert.IsTrue(processType == ProcessType.BusinessProcess, "Process type should be Business Process but is not!");

            // Modify default process Type
            process.ProcessType = ProcessType.UserToSystemProcess;

            // Execute & Verify:
            // Update and Verify the modified process
            var updatedNovaProcess = StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess, Helper.Storyteller, _user);

            Assert.IsTrue(updatedNovaProcess.Process.ProcessType == ProcessType.UserToSystemProcess,
                "Process Type was not updated to UserToSystemProcess!");
        }

        [TestCase]
        [TestRail(211706)]
        [Description("Add a new user task after the precondition and verify that the returned Nova process " +
                     "has the new user task in the correct position and has the correct properties.")]
        public void UpdateNovaProcess_AddUserTaskAfterPrecondition_VerifyReturnedNovaProcess()
        {
            // Setup:
            // Create and get the default Nova process
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(Helper.Storyteller, _project, _user);
            var process = novaProcess.Process;

            // Find precondition task
            var preconditionTask = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition task
            var preconditionOutgoingLink = process.GetOutgoingLinkForShape(preconditionTask);
                
            Assert.IsNotNull(preconditionOutgoingLink, "Process link was not found.");

            // Add user/system Task immediately after the precondition
            process.AddUserAndSystemTask(preconditionOutgoingLink);

            // Execute & Verify:
            // Update and Verify the modified Nova process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess, Helper.Storyteller, _user);
        }

        #endregion Tests

        #region Process Validation Tests

        [TestCase]
        [TestRail(2)]
        [Description(" " +
                     ".")]
        public void UpdateNovaProcess_InvalidProcess_VerifyReturnedCode()
        {
            // Setup:
            // Create and get the default Nova process
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(Helper.Storyteller, _project, _user);
            var process = novaProcess.Process;

            // Find precondition task
            // Find precondition task
            var preconditionTask = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition task
            var preconditionOutgoingLink = process.GetOutgoingLinkForShape(preconditionTask);

            var newTask = process.AddUserAndSystemTask(preconditionOutgoingLink);
            newTask.Name = string.Empty;
            // Execute & Verify:
            // Update and Verify the modified Nova process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess, Helper.Storyteller, _user);
        }

        #endregion Process Validation Tests
    }
}
