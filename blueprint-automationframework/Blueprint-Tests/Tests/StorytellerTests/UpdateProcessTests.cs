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
            // XXX: This is commented out because it will fail since we didn't publish any artifacts.  Need to implement a DiscardChanges() method instead.
//            if (_storyteller.Artifacts != null)
//            {
                // Delete all the artifacts that were added.
//                foreach (var artifact in _storyteller.Artifacts)
//                {
//                    _storyteller.DeleteProcessArtifact(artifact, _user);
//                }
//            }

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
            var defaultProcessArtifact = _storyteller.CreateProcessArtifact(_project, BaseArtifactType.Process, _user);

            var tempProcess = _storyteller.GetProcess(_user, 3171);

            var defaultProcess = _storyteller.GetProcess(_user, defaultProcessArtifact.Id);

            Assert.IsNotNull(defaultProcess, "The returned process was null.");

            // Modify default process name and update process
            defaultProcess.Name = RandomGenerator.RandomValueWithPrefix("DefaultProcess", 4);
            defaultProcess.ArtifactPathLinks[0].Name = defaultProcess.Name;

            // Update the default process with a new process name
            var modifiedProcess = _storyteller.UpdateProcess(_user, defaultProcess);

            // Assert that process returned from the UpdateProcess method is identical to the process sent with the UpdateProcess method
            StorytellerTestHelper.AssertProcessesAreIdentical(defaultProcess, modifiedProcess);

            // Get the updated default process
            var returnedProcess = _storyteller.GetProcess(_user, defaultProcess.Id);

            // Assert that the process returned from the GetProcess method is identical to the process returned from the UpdateProcess method
            StorytellerTestHelper.AssertProcessesAreIdentical(modifiedProcess, returnedProcess);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase, Description("Add a user and system task and verify returned process")]
        public void AddUserAndSystemTaskBeforeDefaultUserTask_VerifyReturnedProcess()
        {
            var defaultProcessArtifact = _storyteller.CreateProcessArtifact(_project, BaseArtifactType.Process, _user);

            var defaultProcess = _storyteller.GetProcess(_user, defaultProcessArtifact.Id);

            Assert.IsNotNull(defaultProcess, "The returned process was null.");

            var defaultUserTask = defaultProcess.FindProcessShapeByShapeName(Process.DefaultUserTaskName);

            var processLink = defaultProcess.FindIncomingLinkForShape(defaultUserTask.Id);
                
            Assert.IsNotNull(processLink, "Process link was not found.");

            defaultProcess.AddUserTask(processLink.SourceId, processLink.DestinationId, processLink.Orderindex);

            // Update the default process with new user task
            var modifiedProcess = _storyteller.UpdateProcess(_user, defaultProcess);

            // Assert that process returned from the UpdateProcess method is the same process sent with the UpdateProcess method
            StorytellerTestHelper.AssertProcessesAreIdentical(defaultProcess, modifiedProcess, allowNegativeShapeIds: true);

            // Get the updated default process
            var returnedProcess = _storyteller.GetProcess(_user, defaultProcess.Id);

            // Assert that the process returned from the GetProcess method is identical to the process returned from the UpdateProcess method
            StorytellerTestHelper.AssertProcessesAreIdentical(modifiedProcess, returnedProcess);

            _storyteller.PublishProcessArtifacts(_user);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase, Description("Add a user and system task and verify returned process")]
        public void AddUserAndSystemTaskBeforeEnd_VerifyReturnedProcess()
        {
            var defaultProcessArtifact = _storyteller.CreateProcessArtifact(_project, BaseArtifactType.Process, _user);

            var defaultProcess = _storyteller.GetProcess(_user, defaultProcessArtifact.Id);

            Assert.IsNotNull(defaultProcess, "The returned process was null.");

            var endShape = defaultProcess.FindProcessShapeByShapeName(Process.EndName);

            var processLink = defaultProcess.FindIncomingLinkForShape(endShape.Id);

            Assert.IsNotNull(processLink, "Process link was not found.");

            defaultProcess.AddUserTask(processLink.SourceId, processLink.DestinationId, processLink.Orderindex);

            // Update the default process with new user task
            var modifiedProcess = _storyteller.UpdateProcess(_user, defaultProcess);

            // Assert that process returned from the UpdateProcess method is the same process sent with the UpdateProcess method
            StorytellerTestHelper.AssertProcessesAreIdentical(defaultProcess, modifiedProcess, allowNegativeShapeIds: true);

            // Get the updated default process
            var returnedProcess = _storyteller.GetProcess(_user, defaultProcess.Id);

            // Assert that the process returned from the GetProcess method is identical to the process returned from the UpdateProcess method
            StorytellerTestHelper.AssertProcessesAreIdentical(modifiedProcess, returnedProcess);
        }

        #endregion Tests
    }
}
