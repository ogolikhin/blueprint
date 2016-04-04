using CustomAttributes;
using Model;
using Model.Factories;
using NUnit.Framework;
using Helper;
using Model.StorytellerModel;
using Model.StorytellerModel.Impl;
using Model.OpenApiModel;
using System.Collections.Generic;
using System.Linq;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class DeleteTaskTests
    {
        private const int NumberOfAdditionalUserTasks = 5;
        private IAdminStore _adminStore;
        private IBlueprintServer _blueprintServer;
        private IStoryteller _storyteller;
        private IUser _user;
        private IProject _project;
        private bool _deleteChildren = true;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            _adminStore = AdminStoreFactory.GetAdminStoreFromTestConfig();
            _blueprintServer = BlueprintServerFactory.GetBlueprintServerFromTestConfig();
            _storyteller = StorytellerFactory.GetStorytellerFromTestConfig();
            _user = UserFactory.CreateUserAndAddToDatabase();
            _project = ProjectFactory.GetProject(_user);

            // Get a valid Access Control token for the user (for the new Storyteller REST calls).
            ISession session = _adminStore.AddSession(_user.Username, _user.Password);
            _user.SetToken(session.SessionId);

            Assert.IsFalse(string.IsNullOrWhiteSpace(_user.Token.AccessControlToken),
                "The user didn't get an Access Control token!");

            // Get a valid OpenApi token for the user (for the OpenApi artifact REST calls).
            _blueprintServer.LoginUsingBasicAuthorization(_user, string.Empty);

            Assert.IsFalse(string.IsNullOrWhiteSpace(_user.Token.OpenApiToken),
                "The user didn't get an OpenApi token!");
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            if (_storyteller.Artifacts != null)
            {
                // Delete or Discard all the artifacts that were added.
                var savedArtifactsList = new List<IOpenApiArtifact>();
                foreach (var artifact in _storyteller.Artifacts.ToArray())
                {
                    if (artifact.IsPublished)
                    {
                        _storyteller.DeleteProcessArtifact(artifact, deleteChildren: _deleteChildren);
                    }
                    else
                    {
                        savedArtifactsList.Add(artifact);
                    }
                }
                if (!(savedArtifactsList.Count().Equals(0)))
                {
                    Storyteller.DiscardProcessArtifacts(savedArtifactsList, _blueprintServer.Address, _user);
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

        [TestCase]
        [Description("Delete a user and system task and verify that the user and system task are not" +
                     "present in the returned process.")]
        public void DeleteUserAndSystemTask_VerifyReturnedProcess()
        {
            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find precondition task
            var preconditionTask = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition task
            var preconditionOutgoingLink = process.GetOutgoingLinkForShape(preconditionTask);

            Assert.IsNotNull(preconditionOutgoingLink, "Process link was not found.");

            // Add user/system Task immediately after the precondition
            var userTask = process.AddUserAndSystemTask(preconditionOutgoingLink);

            // Save the process
            var returnedProcess = _storyteller.UpdateProcess(_user, process);

            var userTaskToBeDeleted = returnedProcess.GetProcessShapeByShapeName(userTask.Name);

            returnedProcess.DeleteUserAndSystemTask(userTaskToBeDeleted);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateVerifyProcess(returnedProcess, _storyteller, _user);
        }

        [TestCase(NumberOfAdditionalUserTasks)]
        [Description("Delete the user and accompanying system task multiple times and verify that " +
                     "the user and system task are not present in the returned process.")]
        public void DeleteMultipleUserAndSystemTasks_VerifyReturnedProcess(int iteration)
        {
            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find precondition task
            var preconditionTask = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition task
            var preconditionOutgoingLink = process.GetOutgoingLinkForShape(preconditionTask);

            Assert.IsNotNull(preconditionOutgoingLink, "Process link was not found.");

            // Add multiple user task with associated system tasks
            for (int i = 0; i < iteration; i++)
            {
                var userTask = process.AddUserAndSystemTask(preconditionOutgoingLink);
                var processShape = process.GetNextShape(userTask);

                preconditionOutgoingLink = process.GetOutgoingLinkForShape(processShape);
            }

            // Save the process
            var returnedProcess = _storyteller.UpdateProcess(_user, process);

            // Delete multiple user tasks with associated system tasks except the default User Task and its associated system task
            var userTasksToBeDeleted = returnedProcess.GetProcessShapesByShapeType(ProcessShapeType.UserTask);

            foreach (var userTask in userTasksToBeDeleted)
            {
                if (!(userTask.Name.Equals(Process.DefaultUserTaskName)))
                {
                    returnedProcess.DeleteUserAndSystemTask(userTask);
                }
            }

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateVerifyProcess(returnedProcess, _storyteller, _user);
        }

        [TestCase]
        [Description("Add an additonal User Task and generate User Storiese for the updated process then " +
                     "delete a user and associated system task. Verify that the deleting user task doesn't" +
                     "delete user stories generated prior to the User Task deletion.")]
        public void GenerateUserStoriesDeleteUserAndSystemTask_VerifyUserStoriesExistence()
        {
            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find precondition task
            var preconditionTask = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition task
            var preconditionOutgoingLink = process.GetOutgoingLinkForShape(preconditionTask);

            Assert.IsNotNull(preconditionOutgoingLink, "Process link was not found.");

            // Add user/system Task immediately after the precondition
            var userTask = process.AddUserAndSystemTask(preconditionOutgoingLink);

            // Save the process
            var returnedProcess = _storyteller.UpdateProcess(_user, process);

            // Publish the process prior to user story generation
            _storyteller.PublishProcess(_user, returnedProcess);

            // Generate User Story artfact(s) from the Process artifact
            var userStoriesPriorToUserTaskDeletion = _storyteller.GenerateUserStories(_user, returnedProcess);

            // Get the total number of user stories generated from the process
            int totalUserStoriesPriorToUserTaskDeletion = userStoriesPriorToUserTaskDeletion.Count();

            // Delete a single User Task with a associated system task
            var userTaskToBeDeleted = returnedProcess.GetProcessShapeByShapeName(userTask.Name);

            returnedProcess.DeleteUserAndSystemTask(userTaskToBeDeleted);

            // save process with deleted user task and associated system task
            returnedProcess = _storyteller.UpdateProcess(_user, returnedProcess);

            // publish process
            _storyteller.PublishProcess(_user, returnedProcess);

            // checking the total number of user story artifacts from blueprint 
            // by using delete the process artifact returned body type
            int deletedChildArtfacts = 0;
            if (_storyteller.Artifacts != null)
            {  
                // Delete the process artifact that were added from the test.
                var artifact = _storyteller.Artifacts
                    .Find(a => a.IsPublished && a.Id.Equals(returnedProcess.Id));

                // Delete with existing child artifacts which are any existing user story artifact(s)
                var deleteArtifacts = _storyteller.DeleteProcessArtifact(artifact,
                    deleteChildren: _deleteChildren);
                deletedChildArtfacts = deleteArtifacts
                    .FindAll(d => !d.ArtifactId.Equals(returnedProcess.Id)).Count();
            }

            // Assert that total number of user stories on blueprint main experience is still same as
            // the total number of user stories generated prior to the single user task deletion
            Assert.That(totalUserStoriesPriorToUserTaskDeletion.Equals(deletedChildArtfacts),
                "After a single User Task Deletion, total number of user stories {0} is expected" +
                " but {1} user stories were remained.", totalUserStoriesPriorToUserTaskDeletion,
                deletedChildArtfacts);
        }
    }
}
