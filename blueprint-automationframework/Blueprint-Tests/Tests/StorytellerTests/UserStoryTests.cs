using Common;
using Model;
using Model.Factories;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using Model.StorytellerModel;
using Model.StorytellerModel.Impl;

namespace StorytellerTests
{
    public class UserStoryTests
    {
        private IAdminStore _adminStore;
        private IStoryteller _storyteller;
        private IUser _user;
        private IProject _project;
        private ISession _session;
        private int defaultUserTaskCount = 1;
        private const int numberOfAdditionalUserTasks = 5;
        private bool deleteChildren = false;

        #region SetUp and Teardown
        [SetUp]
        public void SetUp()
        {
            _adminStore = AdminStoreFactory.GetAdminStoreFromTestConfig();
            _storyteller = StorytellerFactory.GetStorytellerFromTestConfig();
            _user = UserFactory.CreateUserAndAddToDatabase();
            _project = ProjectFactory.GetProject(_user);
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
            if (_storyteller.Artifacts != null)
            {
                // Delete all the artifacts that were added.
                foreach (var artifact in _storyteller.Artifacts.ToArray())
                {
                    _storyteller.DeleteProcessArtifact(artifact, _user, deleteChildren: deleteChildren);
                }
            }
            if (_user != null)
            {
                _user.DeleteUser();
                _user = null;
            }
        }
        #endregion SetUp and TearDown

        #region Tests

        [Test]
        [Description("Verify that total number of generated or updated user stories are equal to total number of user tasks for the default process")]
        public void UserStoryGenerationProcessWithDefaultUserTask_NumberOfUserTasksAndGeneratedUserStoriesAreEqual()
        {
            // Create an Process artifact
            var _processArtifact = _storyteller.CreateProcessArtifact(project: _project, user: _user, artifactType: BaseArtifactType.Process);
            
            // Publish the Process artifact; enable recursive delete flag
            _storyteller.PublishProcessArtifacts(_user);
            deleteChildren = true;

            // Find number of UserTasks from the published Process
            var _process = _storyteller.GetProcess(_user, _processArtifact.Id);

            var userTasksOnProcess = _process.GetProcessesShapesByShapeType(ProcessShapeType.UserTask).Count;

            // Assert that the number of UserTasks the published process is equal to the number of UserTasks returned from GetProcess call
            Assert.That(userTasksOnProcess == defaultUserTaskCount, "The default number of UserTasks for the new Process is {0} but The number of UserTasks returned from GetProcess call is {1}.", defaultUserTaskCount, userTasksOnProcess);

            Logger.WriteDebug("The number of UserTasks inside of Process is: {0}", userTasksOnProcess);

            // Generate User Story artfact(s) from the Process artifact
            List<IStorytellerUserStory> userStories = _storyteller.GenerateUserStories(_user, _process);

            Logger.WriteDebug("The number of UserStories generated is: {0}", userStories.Count);

            // Assert that the number of UserTasks from the published Process is equal to the number of UserStoryGenerated or Updated
            Assert.That(userStories.Count == userTasksOnProcess, "The number of UserStories generated from the process is {0} but The process has {1} UserTasks.", userStories.Count, userTasksOnProcess);
        }

        [Test]
        [Description("Verify the contents of generated or updated user stories")]
        public void UserStoryGenerationProcessWithDefaultUserTask_VerifyingContents()
        {
            // Create an Process artifact
            var _processArtifact = _storyteller.CreateProcessArtifact(project: _project, user: _user, artifactType: BaseArtifactType.Process);

            // Publish the Process artifact; enable recursive delete flag
            _storyteller.PublishProcessArtifacts(_user);
            deleteChildren = true;

            // Checking Object: The Process that contains shapes including user task shapes
            var _process = _storyteller.GetProcess(_user, _processArtifact.Id);

            // Test Object: Generated User Stories from the Process
            List<IStorytellerUserStory> userStories = _storyteller.GenerateUserStories(_user, _process);

            // Assert that there is only one to one maching between UserTask and generated UserStory
            foreach (IProcessShape shape in _process.GetProcessesShapesByShapeType(ProcessShapeType.UserTask))
            {
                var userStoryCounter = 0;
                foreach (IStorytellerUserStory us in userStories)
                {
                    if (us.ProcessTaskId.Equals(shape.Id))
                    {
                        userStoryCounter++;

                        // -- Verifying userStory contents -- 
                        Assert.That(us.Name.Equals(shape.Name),"Generated US name {0} doesn't match with the source UT name {1}", us.Name, shape.Name);

                        // TODO Assert that UserStory ID == 
                        //Assert.That(userStory.Id.Equals(processShape.PropertyValues["storyLinks"]), "Generated US name {0} doesn't match with the source UT name {1}", userStory.Name, processShape.Name);

                        // Assert that UserStory Property's Name value with Shape's Name Value 
                        Assert.That(us.SystemProperties.Find(s => s.Name.Equals("Name")).Value.Equals(shape.Name), "Generated US's Property Name {0} doesn't match with the source UT name {1}", us.SystemProperties.Find(s => s.Name.Equals("Name")).Value, shape.Name);
                        
                        // Assert that UserStory ST-Title ==
                        // Assert that UserStory ST-Acceptance Criteria ==
                    }
                }
                Assert.That(!userStoryCounter.Equals(0), "No UserStory matches with the UserTask whose ID: {0} is created", shape.Id);
                Assert.That(userStoryCounter.Equals(1), "More than one UserStories are generated for the UserTask whose ID: {0}.", shape.Id);
            }
        }


        [Test]
        [Description("Retrieve UserStoryArtifactType if Storyteller Pack is installed on the target Blueprint")]
        public void GetUserStoryArtifactType_ReceiveUserStoryArtifactType()
        {
            var userStoryArtifactType = _storyteller.GetUserStoryArtifactType(_user, _project.Id);

            Assert.NotNull(userStoryArtifactType.Id,"UserStoryArtifactType Id is null");
            Assert.NotNull(userStoryArtifactType.Name, "UserStoryArtifactType Name is null");
        }

        [TestCase(numberOfAdditionalUserTasks)]
        [Description("Verify that total number of generated or updated user stories are equal to total number of user tasks for the process with multi user tasks")]
        public void UserStoryGenerationProcessWithMultipleUserTasks_NumberOfUserTasksAndUserStoriesAreEqual(int iteration)
        {
            int UserTaskExpectedCount = iteration + defaultUserTaskCount;
            if (UserTaskExpectedCount == int.MaxValue)
            {
                throw new OverflowException("overflow exception");
            }

            // Create an Process artifact
            var _processArtifact = _storyteller.CreateProcessArtifact(project: _project, user: _user, artifactType: BaseArtifactType.Process);

            // Get the process artifact
            var _process = _storyteller.GetProcess(_user, _processArtifact.Id);

            // Add UserTasks - iteration
            var preconditionId = _process.Shapes.Find(p => p.Name.Equals(Process.DefaultPreconditionName)).Id;

            // Find outgoing process link for precondition task
            var processLink = _process.GetOutgoingLinkForShape(preconditionId);

            for (int i = 0; i < iteration; i++)
            {
                var userTask = _process.AddUserTask(processLink);
                processLink = _process.GetOutgoingLinkForShape(_process.GetOutgoingLinkForShape(userTask.Id).DestinationId);
            }

            // Update the process
            _process = _storyteller.UpdateProcess(_user, _process);

            // Publish the Process artifact; enable recursive delete flag
            _storyteller.PublishProcessArtifacts(_user);
            deleteChildren = true;

            // Find number of UserTasks from the published Process
            _process = _storyteller.GetProcess(_user, _processArtifact.Id);

            var userTasksOnProcess = _process.GetProcessesShapesByShapeType(ProcessShapeType.UserTask).Count;

            // Assert that the number of UserTasks the published process is equal to the number of UserTasks returned from GetProcess call
            Assert.That(userTasksOnProcess == UserTaskExpectedCount, "The number of UserTasks expected for the Process is {0} but The number of UserTasks returned from GetProcess call is {1}.", UserTaskExpectedCount, userTasksOnProcess);

            Logger.WriteDebug("The number of UserTasks inside of Process is: {0}", userTasksOnProcess);

            // Generate User Story artfact(s) from the Process artifact
            List<IStorytellerUserStory> userStories = _storyteller.GenerateUserStories(_user, _process);

            Logger.WriteDebug("The number of UserStories generated is: {0}", userStories.Count);

            // Assert that the number of UserTasks from the published Process is equal to the number of UserStoryGenerated or Updated
            Assert.That(userStories.Count == userTasksOnProcess, "The number of UserStories generated from the process is {0} but The process has {1} UserTasks.", userStories.Count, userTasksOnProcess);
        }

        [TestCase(numberOfAdditionalUserTasks)]
        [Description("Verify that every generated or updated user stories are mapped to user tasks for the process with multi user tasks")]
        public void UserStoryGenerationProcessWithMultipleUserTasks_UserTaskUserStoryMapping(int iteration)
        {
            // Create an Process artifact
            var _processArtifact = _storyteller.CreateProcessArtifact(project: _project, user: _user, artifactType: BaseArtifactType.Process);

            // Get the process artifact
            var _process = _storyteller.GetProcess(_user, _processArtifact.Id);

            // Add UserTasks - iteration
            var preconditionId = _process.Shapes.Find(p => p.Name.Equals(Process.DefaultPreconditionName)).Id;

            // Find outgoing process link for precondition task
            var processLink = _process.GetOutgoingLinkForShape(preconditionId);

            for (int i = 0; i < iteration; i++)
            {
                var userTask = _process.AddUserTask(processLink);
                processLink = _process.GetOutgoingLinkForShape(_process.GetOutgoingLinkForShape(userTask.Id).DestinationId);
            }

            // Update the process
            _process = _storyteller.UpdateProcess(_user, _process);

            // Publish the Process artifact; enable recursive delete flag
            _storyteller.PublishProcessArtifacts(_user);
            deleteChildren = true;

            // Checking Object: The Process that contains shapes including user task shapes
            _process = _storyteller.GetProcess(_user, _processArtifact.Id);

            // Test Object: Generated User Stories from the Process
            List<IStorytellerUserStory> userStories = _storyteller.GenerateUserStories(_user, _process);

            // Assert that there is one to one maching between UserTask and generated UserStory
            foreach (IProcessShape shape in _process.GetProcessesShapesByShapeType(ProcessShapeType.UserTask))
            {
                var userStoryCounter = 0;
                foreach (IStorytellerUserStory us in userStories)
                {  
                    if (us.ProcessTaskId.Equals(shape.Id))
                    {
                        userStoryCounter++;
                    }
                }
                Assert.That(!userStoryCounter.Equals(0), "No UserStory matches with the UserTask whose ID: {0} is created", shape.Id);
                Assert.That(userStoryCounter.Equals(1), "More than one UserStories are generated for the UserTask whose ID: {0}.", shape.Id);
            }
        }


        [TestCase(numberOfAdditionalUserTasks)]
        [Description("Verify that Genearate UserStories updates user stories if there are existing user stories for user tasks for the process with multi user tasks")]
        public void UserStoryGenerationProcessWithMultipleUserTasks_VerifyingUpdateFlagsForExistingUserStories(int iteration)
        {
            var InitialUserTaskExpectedCount = iteration / 2 + defaultUserTaskCount;
            var AdditionalUserTaskExpectedCount = iteration - (iteration/2);

            // Create an Process artifact
            var _processArtifact = _storyteller.CreateProcessArtifact(project: _project, user: _user, artifactType: BaseArtifactType.Process);

            // Get the process artifact
            var _process = _storyteller.GetProcess(_user, _processArtifact.Id);


            // Add UserTasks - InitialUserTaskExpected - DEFAULTUSERTASK_COUNT since default UT counts
            var preconditionId = _process.Shapes.Find(p => p.Name.Equals(Process.DefaultPreconditionName)).Id;
            
            // Find outgoing process link for precondition task
            var processLink = _process.GetOutgoingLinkForShape(preconditionId);

            for (int i = 0; i < InitialUserTaskExpectedCount - defaultUserTaskCount; i++)
            {
                var userTask = _process.AddUserTask(processLink);
                processLink = _process.GetOutgoingLinkForShape(_process.GetOutgoingLinkForShape(userTask.Id).DestinationId);
            }

            // Update the process
            _process = _storyteller.UpdateProcess(_user, _process);

            // Publish the Process artifact
            _storyteller.PublishProcessArtifacts(_user);

            // User Stories from the Process artifact
            List<IStorytellerUserStory> userStories_FirstBatch = _storyteller.GenerateUserStories(_user, _process);

            Logger.WriteDebug("The number of user stories generated is: {0}", userStories_FirstBatch.Count);

            // Add UserTasks - AdditionalUserTaskExpected
            preconditionId = _process.Shapes.Find(p => p.Name.Equals(Process.DefaultPreconditionName)).Id;

            // Find outgoing process link for precondition task
            processLink = _process.GetOutgoingLinkForShape(preconditionId);

            for (int i = 0; i < AdditionalUserTaskExpectedCount; i++)
            {
                var userTask = _process.AddUserTask(processLink);
                processLink = _process.GetOutgoingLinkForShape(_process.GetOutgoingLinkForShape(userTask.Id).DestinationId);
            }

            // Update the process
            _process = _storyteller.UpdateProcess(_user, _process);

            // Publish the Process artifact
            _storyteller.PublishProcessArtifacts(_user);

            // enable recursive delete flag
            deleteChildren = true;

            // User Stories from the Process artifact
            List<IStorytellerUserStory> userStories_SecondBatch = _storyteller.GenerateUserStories(_user, _process);

            Logger.WriteDebug("The number of user stories generated or updated is: {0}", userStories_SecondBatch.Count);

            //Assert that the count of generated user stories from first batch is equal to the count of updated user stories from the second batch
            var createdUserStories_FirstBatch_Count = userStories_FirstBatch.Count;
            var totalUserStories_SecondBatch_Count = userStories_SecondBatch.Count;
            var createdUserStories_SecondBatch_Count = userStories_SecondBatch.FindAll(u => u.IsNew.Equals(true)).Count;
            var updatedUserStories_SecondBatch_Count = userStories_SecondBatch.FindAll(u => u.IsNew.Equals(false)).Count;

            Assert.That(totalUserStories_SecondBatch_Count == createdUserStories_SecondBatch_Count + updatedUserStories_SecondBatch_Count, "The user stories either updated or created: {0} should be equal to addition of the created: {1} and updated: {2}", totalUserStories_SecondBatch_Count, createdUserStories_SecondBatch_Count, updatedUserStories_SecondBatch_Count);
            Assert.That(createdUserStories_FirstBatch_Count == updatedUserStories_SecondBatch_Count, "The expected number of user stories from UserStoryGeneration call is {0} but {1} are updated.", createdUserStories_FirstBatch_Count, updatedUserStories_SecondBatch_Count);
        }

        #endregion Tests

    }
}
