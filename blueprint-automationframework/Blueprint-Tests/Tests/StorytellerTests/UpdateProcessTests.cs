using System;
using System.Globalization;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.Impl;
using NUnit.Framework;
using Utilities.Factories;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    [Explicit(IgnoreReasons.UnderDevelopment)]
    public class UpdateProcessTests
    {
        private IAdminStore _adminStore;
        private IStoryteller _storyteller;
        private IUser _user;
        private IProject _project;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            _adminStore = AdminStoreFactory.GetAdminStoreFromTestConfig();
            _storyteller = StorytellerFactory.GetStorytellerFromTestConfig();
            _user = UserFactory.CreateUserAndAddToDatabase();
            _project = ProjectFactory.GetProject(_user);

            // Get a valid token for the user.
            ISession session = _adminStore.AddSession(_user.Username, _user.Password);
            _user.SetToken(session.SessionId);

            Assert.IsFalse(string.IsNullOrWhiteSpace(_user.Token.AccessControlToken), "The user didn't get an Access Control token!");
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
             if (_storyteller.Artifacts != null)
            {
                // TODO: Uncomment when new Publish Process is implemented
                //Delete all the artifacts that were added.
                //foreach (var artifact in _storyteller.Artifacts)
                //{
                //    _storyteller.DeleteProcessArtifact(artifact, _user);
                //}
            }

            if (_adminStore != null)
            {
                // Delete all the sessions that were created.
                foreach (var session in _adminStore.Sessions.ToArray())
                {
                    _adminStore.DeleteSession(session);
                }
            }

            if (_user != null)
            {
                _user.DeleteUser();
                _user = null;
            }
        }

        #endregion Setup and Cleanup

        #region Tests

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase, Description("Update name of default process and verify returned process")]
        public void ModifyDefaultProcessName_VerifyReturnedProcess()
        {
            // Create default process
            var defaultProcessArtifact = _storyteller.CreateProcessArtifact(_project, BaseArtifactType.Process, _user);

            // Get default process
            var defaultProcess = _storyteller.GetProcess(_user, defaultProcessArtifact.Id);

            Assert.IsNotNull(defaultProcess, "The returned process was null.");

            // Modify default process Name
            defaultProcess.Name = RandomGenerator.RandomValueWithPrefix("DefaultProcess", 4);
            defaultProcess.ArtifactPathLinks[0].Name = defaultProcess.Name;

            // Update and Verify the modified process
            UpdateAndVerifyProcess(defaultProcess);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase, Description("Update type of default process and verify returned process")]
        public void ModifyProcessType_VerifyReturnedProcess()
        {
            // Create default process
            var defaultProcessArtifact = _storyteller.CreateProcessArtifact(_project, BaseArtifactType.Process, _user);

            // Get default process
            var defaultProcess = _storyteller.GetProcess(_user, defaultProcessArtifact.Id);

            Assert.IsNotNull(defaultProcess, "The returned process was null.");

            // Must toggle case of the first character to create lower case property name
            var clientTypePropertyName =
                StorytellerTestHelper.ToggleCaseOfFirstCharacter(PropertyTypePredefined.ClientType.ToString());

            var processType = Convert.ToInt32(defaultProcess.PropertyValues[clientTypePropertyName].Value, CultureInfo.InvariantCulture);

            Assert.That(processType == (int)ProcessType.BusinessProcess);

            // Modify default process Type
            defaultProcess.PropertyValues[clientTypePropertyName].Value = (int)ProcessType.UserToSystemProcess;

            // Update and Verify the modified process
            UpdateAndVerifyProcess(defaultProcess);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase, Description("Add a user task after the precondition and verify returned process")]
        public void AddUserTaskAfterPrecondition_VerifyReturnedProcess()
        {
            // Create default process
            var defaultProcessArtifact = _storyteller.CreateProcessArtifact(_project, BaseArtifactType.Process, _user);

            // Get default process
            var defaultProcess = _storyteller.GetProcess(_user, defaultProcessArtifact.Id);

            Assert.IsNotNull(defaultProcess, "The returned process was null.");

            // Find precondition task
            var preconditionTask = defaultProcess.FindProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition task
            var processLink = defaultProcess.FindOutgoingLinkForShape(preconditionTask.Id);
                
            Assert.IsNotNull(processLink, "Process link was not found.");

            // Add user/system Task immediately after the precondition
            defaultProcess.AddUserTask(processLink);

            // Update and Verify the modified process
            UpdateAndVerifyProcess(defaultProcess);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase, Description("Add a user task before the end and verify returned process")]
        public void AddUserTaskBeforeEnd_VerifyReturnedProcess()
        {
            // Create default process
            var defaultProcessArtifact = _storyteller.CreateProcessArtifact(_project, BaseArtifactType.Process, _user);

            // Get default process
            var defaultProcess = _storyteller.GetProcess(_user, defaultProcessArtifact.Id);

            Assert.IsNotNull(defaultProcess, "The returned process was null.");

            // Find the end shape
            var endShape = defaultProcess.FindProcessShapeByShapeName(Process.EndName);

            // Find the incoming link for the end shape
            var processLink = defaultProcess.FindIncomingLinkForShape(endShape.Id);

            Assert.IsNotNull(processLink, "Process link was not found.");

            // Add a user/system task immediately before the end shape
            defaultProcess.AddUserTask(processLink);

            // Updatea and Verify the modified process
            UpdateAndVerifyProcess(defaultProcess);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase, Description("Add a user task after an existing user task and verify returned process")]
        public void AddUserTaskAfterUserTask_VerifyReturnedProcess()
        {
            // Create default process
            var defaultProcessArtifact = _storyteller.CreateProcessArtifact(_project, BaseArtifactType.Process, _user);

            // Get default process
            var defaultProcess = _storyteller.GetProcess(_user, defaultProcessArtifact.Id);

            Assert.IsNotNull(defaultProcess, "The returned process was null.");

            var defaultUserTask = defaultProcess.FindProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the incoming link for the end shape
            var processLink = defaultProcess.FindOutgoingLinkForShape(defaultUserTask.Id);

            Assert.IsNotNull(processLink, "Process link was not found.");

            // Add a user/system task immediately before the end shape
            defaultProcess.AddUserTask(processLink);

            // Update and Verify the modified process
            UpdateAndVerifyProcess(defaultProcess);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase, Description("Add a user decision with branch after precondition and verify returned process")]
        public void AddUserDecisionWithBranchAfterPrecondition_VerifyReturnedProcess()
        {
            // Create default process
            var defaultProcessArtifact = _storyteller.CreateProcessArtifact(_project, BaseArtifactType.Process, _user);

            // Get default process
            var defaultProcess = _storyteller.GetProcess(_user, defaultProcessArtifact.Id);

            Assert.IsNotNull(defaultProcess, "The returned process was null.");

            // Find precondition task
            var preconditionTask = defaultProcess.FindProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var processLink = defaultProcess.FindOutgoingLinkForShape(preconditionTask.Id);

            var branchEndPoint = defaultProcess.FindProcessShapeByShapeName(Process.EndName);

            // Add Decision point with branch to end
            defaultProcess.AddDecisionPointWithBranchAfterShape(preconditionTask.Id, processLink.Orderindex + 1, branchEndPoint.Id);

            // Update and Verify the modified process
            UpdateAndVerifyProcess(defaultProcess);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase, Description("Add a user decision point before the end and verify returned process")]
        public void AddUserDecisionWithBranchBeforeEnd_VerifyReturnedProcess()
        {
            // Create default process
            var defaultProcessArtifact = _storyteller.CreateProcessArtifact(_project, BaseArtifactType.Process, _user);

            // Get default process
            var defaultProcess = _storyteller.GetProcess(_user, defaultProcessArtifact.Id);

            Assert.IsNotNull(defaultProcess, "The returned process was null.");

            // Find the end shape
            var endShape = defaultProcess.FindProcessShapeByShapeName(Process.EndName);

            // Find incoming process link for end shape
            var processLink = defaultProcess.FindOutgoingLinkForShape(endShape.Id);

            // Add Decision point with branch and 2 user tasks
            defaultProcess.AddDecisionPointWithBranchBeforeShape(endShape.Id, processLink.Orderindex + 1);

            // Verify the modified process
            UpdateAndVerifyProcess(defaultProcess);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase, Description("Add user task before a user decision point and verify returned process")]
        public void AddUserTaskBeforeUserDecision_VerifyReturnedProcess()
        {
            // Create default process
            var defaultProcessArtifact = _storyteller.CreateProcessArtifact(_project, BaseArtifactType.Process, _user);

            // Get default process
            var defaultProcess = _storyteller.GetProcess(_user, defaultProcessArtifact.Id);

            Assert.IsNotNull(defaultProcess, "The returned process was null.");

            // Find the default user task
            var defaultUserTask = defaultProcess.FindProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find incoming process link for the default user task shape
            var processLink = defaultProcess.FindIncomingLinkForShape(defaultUserTask.Id);

            // Add Decision point with branch before default user task
            var userDecisionPoint = defaultProcess.AddDecisionPointWithBranchBeforeShape(defaultUserTask.Id, processLink.Orderindex + 1);

            // Find the incoming link for the user decision task
             processLink = defaultProcess.FindIncomingLinkForShape(userDecisionPoint.Id);

            // Add a user/system task immediately before the user decision point
            defaultProcess.AddUserTask(processLink);

            // Verify the modified process
            UpdateAndVerifyProcess(defaultProcess);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase, Description("Add user decision point between 2 user tasks and verify returned process")]
        public void AddUserDecisionBetweenTwoUserTasks_VerifyReturnedProcess()
        {
            // Create default process
            var defaultProcessArtifact = _storyteller.CreateProcessArtifact(_project, BaseArtifactType.Process, _user);

            // Get default process
            var defaultProcess = _storyteller.GetProcess(_user, defaultProcessArtifact.Id);

            Assert.IsNotNull(defaultProcess, "The returned process was null.");

            // Find the end shape
            var defaultUserTask = defaultProcess.FindProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the outgoing link for the default user task
            var processLink = defaultProcess.FindOutgoingLinkForShape(defaultUserTask.Id);

            // Add a user/system task immediately bafter the default user task
            defaultProcess.AddUserTask(processLink);

            // Add a user decision point between 2 user/system tasks
            defaultProcess.AddDecisionPointWithBranchAfterShape(defaultUserTask.Id, processLink.Orderindex + 1);

            // Verify the modified process
            UpdateAndVerifyProcess(defaultProcess);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase, Description("Add user decision point before merge point and verify returned process")]
        public void AddUserDecisionWithinUserDecisionBeforeMergePoint_VerifyReturnedProcess()
        {
            // Create default process
            var defaultProcessArtifact = _storyteller.CreateProcessArtifact(_project, BaseArtifactType.Process, _user);

            // Get default process
            var defaultProcess = _storyteller.GetProcess(_user, defaultProcessArtifact.Id);

            Assert.IsNotNull(defaultProcess, "The returned process was null.");

            // Find precondition task
            var preconditionTask = defaultProcess.FindProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find the outgoing link for the precondition
            var processLink = defaultProcess.FindOutgoingLinkForShape(preconditionTask.Id);

            var branchEndPoint = defaultProcess.FindProcessShapeByShapeName(Process.EndName);

            // Add Decision point with branch (make order index to to give room for internal branch
            defaultProcess.AddDecisionPointWithBranchAfterShape(preconditionTask.Id, processLink.Orderindex + 2, branchEndPoint.Id);

            // Find the default user task
            var defaultUserTask = defaultProcess.FindProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the outgoing link for the default user task
            processLink = defaultProcess.FindOutgoingLinkForShape(defaultUserTask.Id);

            // Add a user decision point between 2 user/system tasks
            defaultProcess.AddDecisionPointWithBranchAfterShape(defaultUserTask.Id, processLink.Orderindex + 1, branchEndPoint.Id);

            // Verify the modified process
            UpdateAndVerifyProcess(defaultProcess);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase, Description("Add a second branch to a user decision point and verify returned process")]
        public void AddSecondBranchToUserDecision_VerifyReturnedProcess()
        {
            // Create default process
            var defaultProcessArtifact = _storyteller.CreateProcessArtifact(_project, BaseArtifactType.Process, _user);

            // Get default process
            var defaultProcess = _storyteller.GetProcess(_user, defaultProcessArtifact.Id);

            Assert.IsNotNull(defaultProcess, "The returned process was null.");

            // Find precondition task
            var preconditionTask = defaultProcess.FindProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var processLink = defaultProcess.FindOutgoingLinkForShape(preconditionTask.Id);

            var branchEndPoint = defaultProcess.FindProcessShapeByShapeName(Process.EndName);

            // Add decision point with branch to end
            defaultProcess.AddDecisionPointWithBranchAfterShape(preconditionTask.Id, processLink.Orderindex + 1, branchEndPoint.Id);

            // Add branch to decision point with task
            defaultProcess.AddBranchWithUserTaskToDecisionPoint(preconditionTask.Id, processLink.Orderindex + 1, branchEndPoint.Id); 

            // Update and Verify the modified process
            UpdateAndVerifyProcess(defaultProcess);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase, Description("Add a user decision point to a branch and verify returned process")]
        public void AddUserDecisionToBranch_VerifyReturnedProcess()
        {
            // Create default process
            var defaultProcessArtifact = _storyteller.CreateProcessArtifact(_project, BaseArtifactType.Process, _user);

            // Get default process
            var defaultProcess = _storyteller.GetProcess(_user, defaultProcessArtifact.Id);

            Assert.IsNotNull(defaultProcess, "The returned process was null.");

            // Find precondition task
            var preconditionTask = defaultProcess.FindProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var processLink = defaultProcess.FindOutgoingLinkForShape(preconditionTask.Id);

            var branchEndPoint = defaultProcess.FindProcessShapeByShapeName(Process.EndName);

            // Add decision point with branch to end
            defaultProcess.AddDecisionPointWithBranchAfterShape(preconditionTask.Id, processLink.Orderindex + 1, branchEndPoint.Id);

            // Add branch to decision point with task
            var userTask = defaultProcess.AddBranchWithUserTaskToDecisionPoint(preconditionTask.Id, processLink.Orderindex + 1, branchEndPoint.Id);

            // Add decision point with branch to end
            defaultProcess.AddDecisionPointWithBranchAfterShape(userTask.Id, processLink.Orderindex + 2, branchEndPoint.Id);

            // Update and Verify the modified process
            UpdateAndVerifyProcess(defaultProcess);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase, Description("Add a user task after a merge point and verify returned process")]
        public void AddUserTaskAfterMergePoint_VerifyReturnedProcess()
        {
            // Create default process
            var defaultProcessArtifact = _storyteller.CreateProcessArtifact(_project, BaseArtifactType.Process, _user);

            // Get default process
            var defaultProcess = _storyteller.GetProcess(_user, defaultProcessArtifact.Id);

            Assert.IsNotNull(defaultProcess, "The returned process was null.");

            // Find the default user task
            var defaultUserTask = defaultProcess.FindProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the outgoing link for the default user task
            var processLink = defaultProcess.FindOutgoingLinkForShape(defaultUserTask.Id);

            Assert.IsNotNull(processLink, "Process link was not found.");

            // Add a user/system task immediately before the end shape
            var userTask = defaultProcess.AddUserTask(processLink);

            // Add branch with merge point between 2 user tasks
            defaultProcess.AddDecisionPointWithBranchBeforeShape(defaultUserTask.Id, processLink.Orderindex + 1, userTask.Id);

            // Update and Verify the modified process
            UpdateAndVerifyProcess(defaultProcess);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase, Description("Add a user decision point after a merge point and verify returned process")]
        public void AddUserDecisionPointAfterMergePoint_VerifyReturnedProcess()
        {
            // Create default process
            var defaultProcessArtifact = _storyteller.CreateProcessArtifact(_project, BaseArtifactType.Process, _user);

            // Get default process
            var defaultProcess = _storyteller.GetProcess(_user, defaultProcessArtifact.Id);

            Assert.IsNotNull(defaultProcess, "The returned process was null.");

            // Find precondition task
            var preconditionTask = defaultProcess.FindProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var processLink = defaultProcess.FindOutgoingLinkForShape(preconditionTask.Id);

            var branchEndPoint = defaultProcess.FindProcessShapeByShapeName(Process.EndName);

            // Add Decision point with branch to end
            var userDecisionPoint = defaultProcess.AddDecisionPointWithBranchAfterShape(preconditionTask.Id, processLink.Orderindex + 1, branchEndPoint.Id);

            // Add decision point before decision point; will have same branch order index as previous added branch
            defaultProcess.AddDecisionPointWithBranchBeforeShape(userDecisionPoint.Id, processLink.Orderindex + 1);

            // Update and Verify the modified process
            UpdateAndVerifyProcess(defaultProcess);
        }

        /// <summary>
        /// Updates and verifies the processes returned from UpdateProcess and GetProcess
        /// </summary>
        /// <param name="processToVerify">The process to verify</param>
        private void UpdateAndVerifyProcess(IProcess processToVerify)
        {
            // Update the process using UpdateProcess
            var processReturnedFromUpdate = _storyteller.UpdateProcess(_user, processToVerify);

            Assert.IsNotNull(processReturnedFromUpdate, "The returned process was null.");

            // Assert that process returned from the UpdateProcess method is identical to the process sent with the UpdateProcess method
            // Allow negative shape ids in the process being verified
            StorytellerTestHelper.AssertProcessesAreIdentical(processToVerify, processReturnedFromUpdate, allowNegativeShapeIds: true);

            // Get the process using GetProcess
            var processReturnedFromGet = _storyteller.GetProcess(_user, processToVerify.Id);

            Assert.IsNotNull(processReturnedFromGet, "The returned process was null.");

            // Assert that the process returned from the GetProcess method is identical to the process returned from the UpdateProcess method
            // Don't allow and negative shape ids
            StorytellerTestHelper.AssertProcessesAreIdentical(processReturnedFromUpdate, processReturnedFromGet);
        }

        #endregion Tests
    }
}
