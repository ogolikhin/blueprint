using Common;
using CustomAttributes;
using Model;
using Model.Factories;
using Model.Impl;
using NUnit.Framework;
using System.Collections.Generic;

namespace StorytellerTests
{
    [Explicit(IgnoreReasons.UnderDevelopment)]
    public class UserStoryTests
    {
        private IAdminStore _adminStore;
        private IStoryteller _storyteller;
        private IUser _user;
        private IProject _project;
        private IOpenApiArtifact _processArtifact;
        private ISession _session;

        #region SetUp
        [SetUp]
        public void SetUp()
        {
            _adminStore = AdminStoreFactory.GetAdminStoreFromTestConfig();
            _storyteller = StorytellerFactory.GetStorytellerFromTestConfig();
            _user = UserFactory.CreateUserAndAddToDatabase();
            _project = ProjectFactory.GetProject(_user);
            // Set session for StoryTeller Interal Api Operation 
            _session = _adminStore.AddSession(_user.Username, _user.Password);
            _user.SetToken(_session.SessionId);
        }
        #endregion SetUp

        #region TearDown
        [TearDown]
        public void TearDown()
        {
            if (_user != null)
            {
                //_user.DeleteUser(deleteFromDatabase: true);
                _user = null;
            }
        }
        #endregion TearDown

        #region Tests
        [Test]
        public void GenerateUserStories()
        {
            // Create an Process artifact
            _processArtifact = _storyteller.CreateProcessArtifact(project: _project, user: _user, artifactType: BaseArtifactType.Process);
            
            // Publish the Process artifact - using RestApi
            _storyteller.PublishProcessArtifacts(_user);

            // Find number of UserTasks from the published Process
            var process = _storyteller.GetProcess(_user, _processArtifact.Id);
            var userTasksOnProcess = process.Shapes.FindAll(p => (p.Name.Equals(Process.DefaultUserTaskName))).Count;

            // Generate User Story artfact(s) from the Process artifact
            List<IStorytellerUserStory> userStories = _storyteller.GenerateUserStories(_user, _processArtifact);

            Logger.WriteDebug("Total number of UserTasks inside of Process is: {0}", userTasksOnProcess);
            Logger.WriteDebug("Total number of UserStoryGenerated or Updated is: {0}", userStories.Count);

            // Verify that the number of UserTasks from the published Process is equal to the number of UserStoryGenerated or Updated
            Assert.That(userStories.Count == userTasksOnProcess, "The number of UserStoryGenerated or Updated from tje process is {0} but the process has {1} UserTasks.", userStories.Count, userTasksOnProcess);
        }
        #endregion Tests
    }
}
