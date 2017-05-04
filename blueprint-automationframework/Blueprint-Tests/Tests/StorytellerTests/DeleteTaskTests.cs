using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.StorytellerModel.Enums;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using System.Linq;
using TestCommon;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class DeleteTaskTests : TestBase
    {
        private IUser _user;
        private IProject _project;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_user);
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        [TestCase]
        [Description("Delete a user and system task and verify that the user and system task are not" +
                     "present in the returned process.")]
        public void DeleteUserAndSystemTask_VerifyReturnedProcess()
        {
            // Create and get the default process
            var novaProcess = Helper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Find precondition task
            var preconditionTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition task
            var preconditionOutgoingLink = novaProcess.Process.GetOutgoingLinkForShape(preconditionTask);

            Assert.IsNotNull(preconditionOutgoingLink, "Process link was not found.");

            // Add user/system Task immediately after the precondition
            var userTask = novaProcess.Process.AddUserAndSystemTask(preconditionOutgoingLink);

            // Save the process
            novaProcess.Update(_user, novaProcess.Artifact);
            novaProcess.RefreshArtifactFromServer(_user);

            var userTaskToBeDeleted = novaProcess.Process.GetProcessShapeByShapeName(userTask.Name);

            novaProcess.Process.DeleteUserAndSystemTask(userTaskToBeDeleted);

            // Update and Verify the modified process
            Helper.UpdateAndVerifyNovaProcess(novaProcess.NovaProcess, _user);
        }

        [TestCase(5)]
        [Description("Delete the user and accompanying system task multiple times and verify that " +
                     "the user and system task are not present in the returned process.")]
        public void DeleteMultipleUserAndSystemTasks_VerifyReturnedProcess(int numberOfAdditionalUserTasks)
        {
            // Create and get the default process
            var novaProcess = Helper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Find precondition task
            var preconditionTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition task
            var preconditionOutgoingLink = novaProcess.Process.GetOutgoingLinkForShape(preconditionTask);

            Assert.IsNotNull(preconditionOutgoingLink, "Outgoing link for the default precondition was not found.");

            // Add multiple user task with associated system tasks
            var targetProcessLink = preconditionOutgoingLink;

            for (int i = 0; i < numberOfAdditionalUserTasks; i++)
            {
                var userTask = novaProcess.Process.AddUserAndSystemTask(targetProcessLink);
                var processShape = novaProcess.Process.GetNextShape(userTask);
                //update the targetProcessLink
                targetProcessLink = novaProcess.Process.GetOutgoingLinkForShape(processShape);
            }

            // Save the process
            var returnedNovaProcess = Helper.UpdateAndVerifyNovaProcess(novaProcess.NovaProcess, _user);

            // Delete multiple user tasks with associated system tasks except the default User Task and its associated system task
            var userTasksToBeDeleted = returnedNovaProcess.Process.GetProcessShapesByShapeType(ProcessShapeType.UserTask);

            foreach (var userTask in userTasksToBeDeleted)
            {
                if (!(userTask.Name.Equals(Process.DefaultUserTaskName)))
                {
                    returnedNovaProcess.Process.DeleteUserAndSystemTask(userTask);
                }
            }

            // Update and Verify the modified process
            Helper.UpdateAndVerifyNovaProcess(returnedNovaProcess, _user);
        }

        [TestCase]
        [Description("Add an additonal User Task and generate User Storiese for the updated process then " +
                     "delete a user and associated system task. Verify that deleting the user task doesn't" +
                     "delete user stories generated prior to the User Task deletion.")]
        public void GenerateUserStoriesDeleteUserAndSystemTask_VerifyUserStoriesExistence()
        {
            // Create and get the default processArtifacts 
            var novaProcess = Helper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Find precondition task
            var preconditionTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition task
            var preconditionOutgoingLink = novaProcess.Process.GetOutgoingLinkForShape(preconditionTask);

            Assert.IsNotNull(preconditionOutgoingLink, "Outgoing link for the default precondition was not found.");

            // Add user/system Task immediately after the precondition
            var userTask = novaProcess.Process.AddUserAndSystemTask(preconditionOutgoingLink);

            // Save the process
            novaProcess.Update(_user, novaProcess.Artifact);
            novaProcess.RefreshArtifactFromServer(_user);

            // Publish the process prior to user story generation
            novaProcess.Publish(_user);

            // Generate User Story artfact(s) from the Process artifact
            var userStoriesPriorToUserTaskDeletion = Helper.Storyteller.GenerateUserStories(_user, novaProcess.Process);

            // Get the total number of user stories generated from the process
            int totalUserStoriesPriorToUserTaskDeletion = userStoriesPriorToUserTaskDeletion.Count();

            // Delete a single User Task with a associated system task
            var userTaskToBeDeleted = novaProcess.Process.GetProcessShapeByShapeName(userTask.Name);

            novaProcess.Process.DeleteUserAndSystemTask(userTaskToBeDeleted);

            // save process with deleted user task and associated system task
            novaProcess.Lock(_user);
            novaProcess.Update(_user, novaProcess.Artifact);
            novaProcess.RefreshArtifactFromServer(_user);

            // publish process
            novaProcess.Publish(_user);

            // checking the total number of user story artifacts from blueprint 
            // by using delete the process artifact returned body type
            Assert.That(Helper.Storyteller.Artifacts != null, "Artifact List is missing.");
            
            // Delete the process artifact that were added from the test.
            //var deletedArtifacts = Helper.Storyteller.DeleteNovaProcessArtifact(_user, novaProcess.NovaProcess);
            var deletedArtifacts = novaProcess.Delete(_user);

            int deletedChildArtfacts = deletedArtifacts.FindAll(d => !d.Id.Equals(novaProcess.Id)).Count();

            // Assert that total number of user stories on blueprint main experience is still same as
            // the total number of user stories generated prior to the single user task deletion
            Assert.That(totalUserStoriesPriorToUserTaskDeletion.Equals(deletedChildArtfacts),
                "After a single User Task Deletion, total number of user stories {0} is expected" +
                " but {1} user stories remained.", totalUserStoriesPriorToUserTaskDeletion,
                deletedChildArtfacts);
        }
    }
}
