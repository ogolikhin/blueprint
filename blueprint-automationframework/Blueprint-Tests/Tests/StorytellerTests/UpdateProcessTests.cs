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
                //Delete all the artifacts that were added.
                foreach (var artifact in _storyteller.Artifacts)
                {
                    _storyteller.DeleteProcessArtifact(artifact, _user);
                }
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "tempProcess")]
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

            // Verify the modified process
            VerifyProcess(defaultProcess);
        }


        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase, Description("Add a user and system task and verify returned process")]
        public void AddUserAndSystemTaskAfterPrecondition_VerifyReturnedProcess()
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
            defaultProcess.AddUserTask(processLink.SourceId, processLink.DestinationId);

            // Verify the modified process
            VerifyProcess(defaultProcess);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase, Description("Add a user and system task and verify returned process")]
        public void AddUserAndSystemTaskBeforeEnd_VerifyReturnedProcess()
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
            defaultProcess.AddUserTask(processLink.SourceId, processLink.DestinationId);

            // Verify the modified process
            VerifyProcess(defaultProcess);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase, Description("Add a user and system task and verify returned process")]
        public void AddTwoSequentialUserAndSystemTasksBeforeEnd_VerifyReturnedProcess()
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
            defaultProcess.AddUserTask(processLink.SourceId, processLink.DestinationId);

            //Find the new incoming link to the end shape
            processLink = defaultProcess.FindIncomingLinkForShape(endShape.Id);

            // Add another user/system task immediately before the end shape
            defaultProcess.AddUserTask(processLink.SourceId, processLink.DestinationId);

            // Verify the modified process
            VerifyProcess(defaultProcess);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase, Description("Add a user and system task and verify returned process")]
        public void AddUserDecisionWithBranchAfterPrecondition_VerifyReturnedProcess()
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

            // Add user decision point after the precondition task
            var userDecisionPoint = defaultProcess.AddUserDecisionPoint(processLink.SourceId, processLink.DestinationId);

            // Find end shape
            var endShape = defaultProcess.FindProcessShapeByShapeName(Process.EndName);

            // Add new branch to user decision point (order index of new branch = 2)
            defaultProcess.AddBranch(userDecisionPoint.Id, endShape.Id, 2);

            // Add new user/system task to branch
            defaultProcess.AddUserTask(userDecisionPoint.Id, endShape.Id);

            // Verify the modified process
            VerifyProcess(defaultProcess);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase, Description("Add a user and system task and verify returned process")]
        public void AddUserDecisionBeforeEnd_VerifyReturnedProcess()
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

            // Add user decision point before end shape
            var userDecisionPoint = defaultProcess.AddUserDecisionPoint(processLink.SourceId, processLink.DestinationId);

            // Find end shape
            endShape = defaultProcess.FindProcessShapeByShapeName(Process.EndName);

            // Add new user/system task to branch
            defaultProcess.AddUserTask(userDecisionPoint.Id, endShape.Id);

            // Add new branch to user decision point (order index of new branch = 2)
            defaultProcess.AddBranch(userDecisionPoint.Id, endShape.Id, 2);

            // Add new user/system task to branch 
            defaultProcess.AddUserTask(userDecisionPoint.Id, endShape.Id);

            // Verify the modified process
            VerifyProcess(defaultProcess);
        }

        /// <summary>
        /// Updates process and verifies process returned from UpdateProcess and GetProcess
        /// </summary>
        /// <param name="processToVerify">The process to verify</param>
        private void VerifyProcess(IProcess processToVerify)
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
