using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.ArtifactModel;
using Model.StorytellerModel;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Utilities;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class UpdateProcessValidationTests
    {
        private IAdminStore _adminStore;
        private IBlueprintServer _blueprintServer;
        private IStoryteller _storyteller;
        private IUser _user;
        private IProject _project;

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

            Assert.IsFalse(string.IsNullOrWhiteSpace(_user.Token.AccessControlToken), "The user didn't get an Access Control token!");

            // Get a valid OpenApi token for the user (for the OpenApi artifact REST calls).
            _blueprintServer.LoginUsingBasicAuthorization(_user, string.Empty);

            Assert.IsFalse(string.IsNullOrWhiteSpace(_user.Token.OpenApiToken), "The user didn't get an OpenApi token!");
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            if (_storyteller.Artifacts != null)
            {
                // Delete or Discard all the artifacts that were added.
                var savedArtifactsList = new List<IArtifactBase>();
                foreach (var artifact in _storyteller.Artifacts.ToArray())
                {
                    if (artifact.IsPublished)
                    {
                        _storyteller.DeleteProcessArtifact(artifact, deleteChildren: true);
                    }
                    else
                    {
                        savedArtifactsList.Add(artifact);
                    }
                }
                if (savedArtifactsList.Any())
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

        #region Tests

        [TestCase]
        [Description("Clear the name of process and verify the returned process has a validation error" +
                     "indicating that the process name is required.")]
        public void UpdateProcessWithoutProcessName_VerifyGetProcessReturnsValidationError()
        {
            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Modify default process Name
            process.Name = string.Empty;

            var ex = Assert.Throws<Http400BadRequestException>(
                () =>
                   // Get and deserialize response
                   _storyteller.UpdateProcessReturnResponseOnly(
                        _user,
                        process)
                );

            var deserializedResponse = Deserialization.DeserializeObject<ProcessValidationResponse>(ex.RestResponse.Content);

            // Assert that the deserialized response indicates that the process name is required
            AssertValidationResponse(deserializedResponse, ProcessValidationResponse.NameRequired);
        }

        [TestCase]
        [Description("Add an orphaned task to a process and verify the returned process has a validation error" +
                     "indicating that an orphaned task was detected.  The validation error message includes" +
                     "the sub-artifact Id of the orphaned task.")]
        public void UpdateProcessWithOrphanedTask_VerifyGetProcessReturnsValidationError()
        {
            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find precondition task
            var preconditionTask = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var processLink = process.GetOutgoingLinkForShape(preconditionTask);

            // Remove the process link between the precondition and the default user task
            process.Links.Remove((ProcessLink)processLink);

            var ex = Assert.Throws<Http400BadRequestException>(
                () =>
                   // Get and deserialize response
                   _storyteller.UpdateProcessReturnResponseOnly(
                        _user,
                        process)
                );

            var deserializedResponse = Deserialization.DeserializeObject<ProcessValidationResponse>(ex.RestResponse.Content);

            // Asser that the deserialized response indicates that an orphaned shape was found
            AssertValidationResponse(deserializedResponse, ProcessValidationResponse.OrphanedShapes);

            // Assert that the shape id of the orphaned shape is the one expected
            AssertValidationResponse(deserializedResponse, processLink.DestinationId.ToString(CultureInfo.InvariantCulture));
        }

        [TestCase]
        [Description("Delete the only user task in a process.  Verify that the returned process has a validation" +
                     "error indicating that a process must have at least 1 user task.")]
        public void DeleteTheOnlyUserTaskInProcess_VerifyGetProcessReturnsValidationError()
        {
            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            var defaultUserTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            process.DeleteUserAndSystemTask(defaultUserTask);

            var ex = Assert.Throws<Http400BadRequestException>(
                () =>
                   // Get and deserialize response
                   _storyteller.UpdateProcessReturnResponseOnly(
                        _user,
                        process)
                );

            var deserializedResponse = Deserialization.DeserializeObject<ProcessValidationResponse>(ex.RestResponse.Content);

            // Assert that the deserialized response indicates that no user tasks were found
            AssertValidationResponse(deserializedResponse, ProcessValidationResponse.NoUserTasksFound);

            // Assert that the deserialized response indicates that no system tasks were found
            AssertValidationResponse(deserializedResponse, ProcessValidationResponse.NoSystemTasksFound);
        }

        [TestCase]
        [Description("Delete the only user task in a process that is betwen two user decisions.  Verify that " +
                     " the returned process has a validation error indicating that a process must have " +
                     "at least 1 user task.")]
        public void DeleteTheOnlyUserTaskBetweenTwoUserDecisionsInProcess_VerifyGetProcessReturnsValidationError()
        {
            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcessWithTwoSequentialUserDecisions(_storyteller, _project, _user);

            var precondition = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            var firstUserDecision = process.GetNextShape(precondition);

            // Find the outgoing link of the lowest order from the first user decision
            var userDecisionOutgoingLinkOfLowestOrder = process.GetOutgoingLinkForShape(
                firstUserDecision,
                Process.DefaultOrderIndex);

            // The user task to delete us the shape immediately after the first user decision on the lowest order branch
            var userTaskIdToDelete = userDecisionOutgoingLinkOfLowestOrder.DestinationId;

            var userTaskToDelete = process.GetProcessShapeById(userTaskIdToDelete);

            process.DeleteUserAndSystemTask(userTaskToDelete);

            // Find all the user decisions in the process
            var userDecisions = process.GetProcessShapesByShapeType(ProcessShapeType.UserDecision);

            var secondUserDecision = userDecisions.Find(ud => ud.Id != firstUserDecision.Id);

            var ex = Assert.Throws<Http400BadRequestException>(
                () =>
                   // Get and deserialize response
                   _storyteller.UpdateProcessReturnResponseOnly(
                        _user,
                        process)
                );

            var deserializedResponse = Deserialization.DeserializeObject<ProcessValidationResponse>(ex.RestResponse.Content);

            var expectedValidationResponseContent = I18NHelper.FormatInvariant(
                ProcessValidationResponse.TwoSequentialUserDecisionsNotAllowed,
                firstUserDecision.Id,
                secondUserDecision.Id);

            // Assert that the deserialized response indicates that an invalid link between two user decisions was found
            AssertValidationResponse(deserializedResponse, expectedValidationResponseContent);
        }

        #endregion Tests

        #region Private Methods

        private static void AssertValidationResponse(ProcessValidationResponse deserializedResponse, string expectedContent)
        {
            Assert.That(
                deserializedResponse.Message.Contains(expectedContent),
                "Response message should have included: {0} => But Actual response message was: {1}",
                expectedContent,
                deserializedResponse.Message);
        }

        #endregion Private Methods
    }

    /// <summary>
    /// The Update Process Validation Response Message
    /// </summary>
    public class ProcessValidationResponse
    {
        public static readonly string TwoSequentialUserDecisionsNotAllowed = "Invalid link detected: User Decision with id {0} is directly linked with another User Decision with id {1}.";

        public static readonly string NoUserTasksFound = "No User Task shapes provided";

        public static readonly string NoSystemTasksFound = "No System Task shapes provided";

        public static readonly string NameRequired = "Name is required for Process";

        public static readonly string OrphanedShapes = "Orphaned shapes discovered";

        // The message returned in the update process validation response
        public string Message { get; set; }
    }
}
