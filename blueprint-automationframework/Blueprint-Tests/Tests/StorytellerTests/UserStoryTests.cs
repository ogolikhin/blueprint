using Common;
using CustomAttributes;
using Model;
using Model.Factories;
using Model.Impl;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;

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
        private IProcess _process;

        #region SetUp and Teardown
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
            Assert.IsFalse(string.IsNullOrWhiteSpace(_user.Token.AccessControlToken), "The user didn't get an Access Control token!");

            if (_storyteller.GetUserStoryArtifactType(_user, _project.Id) == null )
            {
                Assert.Ignore("StorytellerPack is not installed successfully on the environment. Omitting.");
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (_user != null)
            {
                _user.DeleteUser();
                _user = null;
            }
        }
        #endregion SetUp and TearDown

        #region Tests
        [Ignore]
        [Test]
        public void PublishProcessArtifactsAndGenerateUserStories_NumberOfUserTasksAndGeneratedUserStoriesAreEqual()
        {
            // Create an Process artifact
            _processArtifact = _storyteller.CreateProcessArtifact(project: _project, user: _user, artifactType: BaseArtifactType.Process);
            
            // Publish the Process artifact - using RestApi
            _storyteller.PublishProcessArtifacts(_user);

            // Find number of UserTasks from the published Process
            _process = _storyteller.GetProcess(_user, _processArtifact.Id);
            //var userTasksOnProcess = process.Shapes.FindAll(p => (p.Name.Equals(Process.DefaultUserTaskName))).Count;
            var userTasksOnProcess = _process.Shapes.FindAll(p => (Convert.ToInt32(p.PropertyValues["clientType"].Value, CultureInfo.CurrentCulture) == Convert.ToInt32(ProcessType.UserToSystemProcess, CultureInfo.CurrentCulture))).Count;

            // Generate User Story artfact(s) from the Process artifact
            List<IStorytellerUserStory> userStories = _storyteller.GenerateUserStories(_user, _process);

            Logger.WriteDebug("Total number of UserTasks inside of Process is: {0}", userTasksOnProcess);
            Logger.WriteDebug("Total number of UserStoryGenerated or Updated is: {0}", userStories.Count);

            // Verify that the number of UserTasks from the published Process is equal to the number of UserStoryGenerated or Updated
            Assert.That(userStories.Count == userTasksOnProcess, "The number of UserStoryGenerated or Updated from the process is {0} but the process has {1} UserTasks.", userStories.Count, userTasksOnProcess);
        }

        [Test]
        public void GetUserStoryArtifactType_ReceiveUserStoryArtifactType()
        {
            var userStoryArtifactType = _storyteller.GetUserStoryArtifactType(_user, _project.Id);

            Assert.NotNull(userStoryArtifactType.Id,"UserStoryArtifactType Id is null");
            Assert.NotNull(userStoryArtifactType.Name, "UserStoryArtifactType Name is null");
        }

        [Test]
        public void UserStoryGenerationProcessWithMultipleUserTasks_NumberOfUserTasksAndUserStoriesAreEqual()
        {
            // Create an Process artifact
            _processArtifact = _storyteller.CreateProcessArtifact(project: _project, user: _user, artifactType: BaseArtifactType.Process);

            // Get the process artifact
            _process = _storyteller.GetProcess(_user, _processArtifact.Id);

            // Add additional UserTasks
            var preconditionId = _process.Shapes.Find(p => p.Name.Equals(Process.DefaultPreconditionName)).Id;
            for (int i = 0; i < 5; i++)
            {
                //AddUserTask("Precondition Shape's ID", "Postcondition Shape's ID", 1);
                var userTask= _process.AddUserTask(preconditionId, _process.Links.Find(p => p.SourceId.Equals(preconditionId)).DestinationId, 1);
                //Assign preconditionId with new Postcondition Shape's ID\
                preconditionId = _process.Links.Find(p => p.DestinationId.Equals(userTask.Id)).SourceId;    

                //Update Precondition Shape with New User Shape
                /// case of adding on top
                //// original precondition -> new ST
                //// new UT preCondition -> original precondition
                //// new UserTask
                //// new UT postCondition -> new ST
                ///  
                /// case of adding on bottom
                //// new UT preCondition -> origial postcondition
                //// new Usertask
                //// new UT postCondition -> new ST

                /// case of general adding - target to be nth position
                //// n th preCondition -> n-1 th postCondition
                //// new UserTask
                //// n th postCondition -> new ST
                //// n+1 th preCondition -> new ST
            }

            //update the process
            _process = _storyteller.UpdateProcess(_user, _process);

            // Publish the Process artifact - using RestApi
            _storyteller.PublishProcessArtifacts(_user);

            // Find number of UserTasks from the published Process
            _process = _storyteller.GetProcess(_user, _processArtifact.Id);
            var userTasksOnProcess = _process.Shapes.FindAll(p => (Convert.ToInt32(p.PropertyValues["clientType"].Value, CultureInfo.CurrentCulture) == Convert.ToInt32(ProcessType.UserToSystemProcess, CultureInfo.CurrentCulture))).Count;

            // Generate User Story artfact(s) from the Process artifact
            List<IStorytellerUserStory> userStories = _storyteller.GenerateUserStories(_user, _process);

            Logger.WriteDebug("Total number of UserTasks inside of Process is: {0}", userTasksOnProcess);
            Logger.WriteDebug("Total number of UserStoryGenerated or Updated is: {0}", userStories.Count);

            // Verify that the number of UserTasks from the published Process is equal to the number of UserStoryGenerated or Updated
            Assert.That(userStories.Count == userTasksOnProcess, "The number of UserStoryGenerated or Updated from the process is {0} but the process has {1} UserTasks.", userStories.Count, userTasksOnProcess);
        }


        #endregion Tests

    }
}
