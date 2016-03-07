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
        private const int DEFAULTUSERTASK_COUNT = 1;
        private const int ITERATION_COUNT = 5;

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

        [Test]
        public void UserStoryGenerationProcessWithDefaultUserTask_NumberOfUserTasksAndGeneratedUserStoriesAreEqual()
        {
            // Create an Process artifact
            _processArtifact = _storyteller.CreateProcessArtifact(project: _project, user: _user, artifactType: BaseArtifactType.Process);
            
            // Publish the Process artifact - using RestApi
            _storyteller.PublishProcessArtifacts(_user);

            // Find number of UserTasks from the published Process
            _process = _storyteller.GetProcess(_user, _processArtifact.Id);

            var userTasksOnProcess = _process.Shapes.FindAll(p => (Convert.ToInt32(p.PropertyValues["clientType"].Value, CultureInfo.CurrentCulture) == Convert.ToInt32(ProcessType.UserToSystemProcess, CultureInfo.CurrentCulture))).Count;

            // Assert that the number of UserTasks the published process is equal to the number of UserTasks returned from GetProcess call
            Assert.That(userTasksOnProcess == DEFAULTUSERTASK_COUNT, "The default number of UserTasks for the new Process is {0} but The number of UserTasks returned from GetProcess call is {1}.", DEFAULTUSERTASK_COUNT, userTasksOnProcess);

            Logger.WriteDebug("The number of UserTasks inside of Process is: {0}", userTasksOnProcess);

            // Generate User Story artfact(s) from the Process artifact
            List<IStorytellerUserStory> userStories = _storyteller.GenerateUserStories(_user, _process);

            Logger.WriteDebug("The number of UserStories generated is: {0}", userStories.Count);

            // Assert that the number of UserTasks from the published Process is equal to the number of UserStoryGenerated or Updated
            Assert.That(userStories.Count == userTasksOnProcess, "The number of UserStories generated from the process is {0} but The process has {1} UserTasks.", userStories.Count, userTasksOnProcess);
        }

        [Test]
        public void UserStoryGenerationProcessWithDefaultUserTask_VerifyingContents()
        {
            // Create an Process artifact
            _processArtifact = _storyteller.CreateProcessArtifact(project: _project, user: _user, artifactType: BaseArtifactType.Process);

            // Publish the Process artifact
            _storyteller.PublishProcessArtifacts(_user);

            // Checking Object: The Process that contains shapes including user task shapes
            _process = _storyteller.GetProcess(_user, _processArtifact.Id);

            // Test Object: Generated User Stories from the Process
            List<IStorytellerUserStory> userStories = _storyteller.GenerateUserStories(_user, _process);

            // Assert that there is only one to one maching between UserTask and generated UserStory
            foreach (IProcessShape shape in _process.Shapes.FindAll(s => (Convert.ToInt32(s.PropertyValues["clientType"].Value, CultureInfo.CurrentCulture) == Convert.ToInt32(ProcessType.UserToSystemProcess, CultureInfo.CurrentCulture))))
            {
                var UserStoryCounter = 0;
                foreach (IStorytellerUserStory us in userStories)
                {
                    if (us.ProcessTaskId.Equals(shape.Id))
                    {
                        UserStoryCounter++;

                        /// -- Verifying userStory contents -- ///
                        Assert.That(us.Name.Equals(shape.Name),"Genearated US name {0} doesn't match with the source UT name {1}", us.Name, shape.Name);

                        // TODO Assert that UserStory ID == 
                        //Assert.That(userStory.Id.Equals(processShape.PropertyValues["storyLinks"]), "Genearated US name {0} doesn't match with the source UT name {1}", userStory.Name, processShape.Name);

                        // Assert that UserStory Property's Name value with Shape's Name Value 
                        Assert.That(us.SystemProperties.Find(s => s.Name.Equals("Name")).Value.Equals(shape.Name), "Genearated US's Property Name {0} doesn't match with the source UT name {1}", us.SystemProperties.Find(s => s.Name.Equals("Name")).Value, shape.Name);
                        
                        // Assert that UserStory ST-Title ==
                        // Assert that UserStory ST-Acceptance Criteria ==
                    }
                }
                Assert.That(!UserStoryCounter.Equals(0), "No UserStory matches with the UserTask whose ID: {0} is created", shape.Id);
                Assert.That(UserStoryCounter > 0 && UserStoryCounter.Equals(1), "More than one UserStories are generated for the UserTask whose ID: {0}.", shape.Id);
            }
        }


        [Test]
        public void GetUserStoryArtifactType_ReceiveUserStoryArtifactType()
        {
            var userStoryArtifactType = _storyteller.GetUserStoryArtifactType(_user, _project.Id);

            Assert.NotNull(userStoryArtifactType.Id,"UserStoryArtifactType Id is null");
            Assert.NotNull(userStoryArtifactType.Name, "UserStoryArtifactType Name is null");
        }

        [TestCase(ITERATION_COUNT)]
        public void UserStoryGenerationProcessWithMultipleUserTasks_NumberOfUserTasksAndUserStoriesAreEqual(int iteration)
        {
            int UserTaskExpectedCount = iteration + DEFAULTUSERTASK_COUNT;
            if (UserTaskExpectedCount == int.MaxValue)
            {
                throw new OverflowException("overflow exception");
            }

            // Create an Process artifact
            _processArtifact = _storyteller.CreateProcessArtifact(project: _project, user: _user, artifactType: BaseArtifactType.Process);

            // Get the process artifact
            _process = _storyteller.GetProcess(_user, _processArtifact.Id);

            // Add UserTasks - iteration
            var preconditionId = _process.Shapes.Find(p => p.Name.Equals(Process.DefaultPreconditionName)).Id;
            for (int i = 0; i < iteration; i++)
            {
                var userTask= _process.AddUserTask(preconditionId, _process.Links.Find(p => p.SourceId.Equals(preconditionId)).DestinationId);
                
                //Assign preconditionId with new Postcondition Shape's ID
                preconditionId = _process.FindOutgoingLinkForShape(userTask.Id).DestinationId;
            }

            // Update the process
            _process = _storyteller.UpdateProcess(_user, _process);

            // Publish the Process artifact - using RestApi
            _storyteller.PublishProcessArtifacts(_user);

            // Find number of UserTasks from the published Process
            _process = _storyteller.GetProcess(_user, _processArtifact.Id);

            var userTasksOnProcess = _process.Shapes.FindAll(p => (Convert.ToInt32(p.PropertyValues["clientType"].Value, CultureInfo.CurrentCulture) == Convert.ToInt32(ProcessType.UserToSystemProcess, CultureInfo.CurrentCulture))).Count;

            // Assert that the number of UserTasks the published process is equal to the number of UserTasks returned from GetProcess call
            Assert.That(userTasksOnProcess == UserTaskExpectedCount, "The number of UserTasks expected for the Process is {0} but The number of UserTasks returned from GetProcess call is {1}.", UserTaskExpectedCount, userTasksOnProcess);

            Logger.WriteDebug("The number of UserTasks inside of Process is: {0}", userTasksOnProcess);

            // Generate User Story artfact(s) from the Process artifact
            List<IStorytellerUserStory> userStories = _storyteller.GenerateUserStories(_user, _process);

            Logger.WriteDebug("The number of UserStories generated is: {0}", userStories.Count);

            // Assert that the number of UserTasks from the published Process is equal to the number of UserStoryGenerated or Updated
            Assert.That(userStories.Count == userTasksOnProcess, "The number of UserStories generated from the process is {0} but The process has {1} UserTasks.", userStories.Count, userTasksOnProcess);
        }

        [TestCase(ITERATION_COUNT)]
        public void UserStoryGenerationProcessWithMultipleUserTasks_UserTaskUserStoryMapping(int iteration)
        {
            // Create an Process artifact
            _processArtifact = _storyteller.CreateProcessArtifact(project: _project, user: _user, artifactType: BaseArtifactType.Process);

            // Get the process artifact
            _process = _storyteller.GetProcess(_user, _processArtifact.Id);

            // Add UserTasks - iteration
            var preconditionId = _process.Shapes.Find(p => p.Name.Equals(Process.DefaultPreconditionName)).Id;
            for (int i = 0; i < iteration; i++)
            {
                var userTask = _process.AddUserTask(preconditionId, _process.Links.Find(p => p.SourceId.Equals(preconditionId)).DestinationId);

                //Assign preconditionId with new Postcondition Shape's ID
                preconditionId = _process.FindOutgoingLinkForShape(userTask.Id).DestinationId;
            }

            // Update the process
            _process = _storyteller.UpdateProcess(_user, _process);

            // Publish the Process artifact - using RestApi
            _storyteller.PublishProcessArtifacts(_user);

            // Checking Object: The Process that contains shapes including user task shapes
            _process = _storyteller.GetProcess(_user, _processArtifact.Id);

            // Test Object: Generated User Stories from the Process
            List<IStorytellerUserStory> userStories = _storyteller.GenerateUserStories(_user, _process);

            // Assert that there is one to one maching between UserTask and generated UserStory
            foreach (IProcessShape shape in _process.Shapes.FindAll(s => (Convert.ToInt32(s.PropertyValues["clientType"].Value, CultureInfo.CurrentCulture) == Convert.ToInt32(ProcessType.UserToSystemProcess, CultureInfo.CurrentCulture))))
            {
                var UserStoryCounter = 0;
                foreach (IStorytellerUserStory us in userStories)
                {  
                    if (us.ProcessTaskId.Equals(shape.Id))
                    {
                        UserStoryCounter++;
                    }
                }
                Assert.That(!UserStoryCounter.Equals(0), "No UserStory matches with the UserTask whose ID: {0} is created", shape.Id);
                Assert.That(UserStoryCounter > 0 && UserStoryCounter.Equals(1), "More than one UserStories are generated for the UserTask whose ID: {0}.", shape.Id);
            }
        }


        [TestCase(ITERATION_COUNT)]
        public void UserStoryGenerationProcessWithMultipleUserTasks_VerifyingUpdateFlagsForExistingUserStories(int iteration)
        {
            var InitialUserTaskExpectedCount = iteration / 2 + DEFAULTUSERTASK_COUNT;
            var AdditionalUserTaskExpectedCount = iteration - (iteration/2);

            // Create an Process artifact
            _processArtifact = _storyteller.CreateProcessArtifact(project: _project, user: _user, artifactType: BaseArtifactType.Process);

            // Get the process artifact
            _process = _storyteller.GetProcess(_user, _processArtifact.Id);

            // Add UserTasks - InitialUserTaskExpected - DEFAULTUSERTASK_COUNT since default UT counts
            var preconditionId = _process.Shapes.Find(p => p.Name.Equals(Process.DefaultPreconditionName)).Id;
            for (int i = 0; i < InitialUserTaskExpectedCount - DEFAULTUSERTASK_COUNT; i++)
            {
                var userTask = _process.AddUserTask(preconditionId, _process.Links.Find(p => p.SourceId.Equals(preconditionId)).DestinationId);

                //Assign preconditionId with new Postcondition Shape's ID
                preconditionId = _process.FindOutgoingLinkForShape(userTask.Id).DestinationId;
            }

            // Update the process
            _process = _storyteller.UpdateProcess(_user, _process);

            // Publish the Process artifact - using RestApi
            _storyteller.PublishProcessArtifacts(_user);

            // User Stories from the Process artifact
            List<IStorytellerUserStory> userStories_FirstBatch = _storyteller.GenerateUserStories(_user, _process);

            Logger.WriteDebug("The number of user stories generated is: {0}", userStories_FirstBatch.Count);

            // Add UserTasks - AdditionalUserTaskExpected
            preconditionId = _process.Shapes.Find(p => p.Name.Equals(Process.DefaultPreconditionName)).Id;
            for (int i = 0; i < AdditionalUserTaskExpectedCount; i++)
            {
                var userTask = _process.AddUserTask(preconditionId, _process.Links.Find(p => p.SourceId.Equals(preconditionId)).DestinationId);

                //Assign preconditionId with new Postcondition Shape's ID
                preconditionId = _process.FindOutgoingLinkForShape(userTask.Id).DestinationId;
            }

            // Update the process
            _process = _storyteller.UpdateProcess(_user, _process);

            // Publish the Process artifact - using RestApi
            _storyteller.PublishProcessArtifacts(_user);

            // User Stories from the Process artifact
            List<IStorytellerUserStory> userStories_SecondBatch = _storyteller.GenerateUserStories(_user, _process);

            Logger.WriteDebug("The number of user stories generated or updated is: {0}", userStories_SecondBatch.Count);

            //Assert that the count of generated user stories from first batch is equal to the count of updated user stories from the second batch
            var generatedUserStories_FirstBatch_Count = userStories_FirstBatch.Count;
            var updatedUserStories_SecondBatch_Count = userStories_SecondBatch.FindAll(u => u.IsNew.Equals(false)).Count;
            Assert.That(generatedUserStories_FirstBatch_Count == updatedUserStories_SecondBatch_Count, "The expected number of user stories from UserStoryGeneration call is {0} but {1} are updated.", generatedUserStories_FirstBatch_Count, updatedUserStories_SecondBatch_Count);
        }

        #endregion Tests

    }
}
