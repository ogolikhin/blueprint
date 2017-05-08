﻿using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using System.Collections.Generic;
using System.Globalization;
using Model.StorytellerModel.Enums;
using TestCommon;
using Utilities;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class UpdateProcessValidationTests : TestBase
    {
        private IUser _user;
        private IUser _user2;
        private IProject _project;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _user2 = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_user);
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region Tests

        // TODO: This test case is currently duplicate copy of [TestRail(164531)]
        // Will remove this shortly since there is no value on this test case
        [TestCase]
        [Description("Clear the name of process and verify the returned process has a validation error" +
                     "indicating that the process name is required.")]
        public void UpdateNovaProcessWithoutProcessName_VerifyGetProcessReturnsValidationError()
        {
            // Create and get the default process
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Modify default process Name
            novaProcess.NovaProcess.Name = string.Empty;

            var ex = Assert.Throws<Http400BadRequestException>(
                () =>
                   // Get and deserialize response
                   Helper.Storyteller.UpdateNovaProcess(
                        _user,
                        novaProcess.NovaProcess)
                );

            // Assert that the deserialized response indicates that the process name is required
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.NameCannotBeEmpty, "The Item name cannot be empty");
        }

        [TestCase]
        [Description("Add an orphaned task to a process and verify the returned process has a validation error" +
                     "indicating that an orphaned task was detected.  The validation error message includes" +
                     "the sub-artifact Id of the orphaned task.")]
        public void UpdateProcessWithOrphanedTask_VerifyGetProcessReturnsValidationError()
        {
            // Create and get the default process
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Find precondition task
            var preconditionTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var processLink = novaProcess.Process.GetOutgoingLinkForShape(preconditionTask);

            // Remove the process link between the precondition and the default user task
            novaProcess.Process.Links.Remove(processLink);

            var ex = Assert.Throws<Http400BadRequestException>(
                () =>
                   // Get and deserialize response
                   Helper.Storyteller.UpdateProcess(
                        _user,
                        novaProcess.Process)
                );

            var deserializedResponse = SerializationUtilities.DeserializeObject<ProcessValidationResponse>(ex.RestResponse.Content);

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
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(_project, _user);

            var defaultUserTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            novaProcess.Process.DeleteUserAndSystemTask(defaultUserTask);

            var ex = Assert.Throws<Http400BadRequestException>(
                () =>
                   // Get and deserialize response
                   Helper.Storyteller.UpdateProcess(
                        _user,
                        novaProcess.Process)
                );

            var deserializedResponse = SerializationUtilities.DeserializeObject<ProcessValidationResponse>(ex.RestResponse.Content);

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
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcessWithTwoSequentialUserDecisions(_project, _user);

            var precondition = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            var firstUserDecision = novaProcess.Process.GetNextShape(precondition);

            // Find the outgoing link of the lowest order from the first user decision
            var userDecisionOutgoingLinkOfLowestOrder = novaProcess.Process.GetOutgoingLinkForShape(
                firstUserDecision,
                Process.DefaultOrderIndex);

            // The user task to delete us the shape immediately after the first user decision on the lowest order branch
            var userTaskIdToDelete = userDecisionOutgoingLinkOfLowestOrder.DestinationId;

            var userTaskToDelete = novaProcess.Process.GetProcessShapeById(userTaskIdToDelete);

            novaProcess.Process.DeleteUserAndSystemTask(userTaskToDelete);

            // Find all the user decisions in the process
            var userDecisions = novaProcess.Process.GetProcessShapesByShapeType(ProcessShapeType.UserDecision);

            var secondUserDecision = userDecisions.Find(ud => ud.Id != firstUserDecision.Id);

            var ex = Assert.Throws<Http400BadRequestException>(
                () =>
                   // Get and deserialize response
                   Helper.Storyteller.UpdateProcess(
                        _user,
                        novaProcess.Process)
                );

            var deserializedResponse = SerializationUtilities.DeserializeObject<ProcessValidationResponse>(ex.RestResponse.Content);

            var expectedValidationResponseContent = I18NHelper.FormatInvariant(
                ProcessValidationResponse.TwoSequentialUserDecisionsNotAllowed,
                firstUserDecision.Id,
                secondUserDecision.Id);

            // Assert that the deserialized response indicates that an invalid link between two user decisions was found
            AssertValidationResponse(deserializedResponse, expectedValidationResponseContent);
        }

        [TestCase]
        [TestRail(107369)]
        [Description("Update a process without having a lock on the artifact (Another user has the lock). Verify that" +
             "the update process does not succeed.")]
        public void UpdateProcessWithoutArtifactLock_VerifyUpdateDoesNotSucceed()
        {
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(_project, _user);

            StorytellerTestHelper.UpdateVerifyAndPublishNovaProcess(novaProcess.NovaProcess, _user);

            // Create an artifact representing the process artifact that was created and add it to the 
            // list of artifacts to lock

            var processArtifact = new Artifact { Id = novaProcess.Id, Address = Helper.ArtifactStore.Address };

            var artifactsToLock = new List<IArtifactBase> { processArtifact };

            // Second user locks the artifact
            Helper.SvcShared.LockArtifacts(_user2, artifactsToLock);

            var ex = Assert.Throws<Http409ConflictException>(() =>
                // First user attempts to update the process
                Helper.Storyteller.UpdateProcess(_user, novaProcess.Process, lockArtifactBeforeUpdate: false),
                "The first user attempted to update the process locked by another user and either an unexpected exception was thrown or" +
                "the first user's attempted publish was successful."
                );

            var deserializedResponse = SerializationUtilities.DeserializeObject<ProcessValidationResponse>(ex.RestResponse.Content);

            // Assert that the deserialized response indicates that the artifact is locked
            // by the second user
            AssertValidationResponse(deserializedResponse, ProcessValidationResponse.ArtifactAlreadyLocked);
        }
        
        [TestCase]
        [TestRail(134647)]
        [Description(
            "Publish a process that has more than the shape limit in the backend.")]
        public void PublishProcess_ExceedShapeLimit_VerifyPublishDoesNotSucceed()
        {
            // Get limit from the database
            int limit = Helper.Storyteller.GetStorytellerShapeLimitFromDb;
            
            // Number of pairs to create.  Subtract 5, for the number of shapes in the default process.
            int pairs = (limit - Process.NumberOfShapesInDefaultProcess)/2;

            // Create and get the default process
            var process = StorytellerTestHelper.CreateProcessWithXAdditionalTaskPairs(_project, _user, pairs);

            // Get the end point
            var endShape = process.GetProcessShapeByShapeName(Process.EndName);

            // Find outgoing process link for precondition
            var endPointIncomingLink = process.GetIncomingLinkForShape(endShape);

            // Adds a pair of user/system tasks to exceed over the limit.
            process.AddUserAndSystemTask(endPointIncomingLink);

            var ex = Assert.Throws<Http400BadRequestException>(
                () =>
                    // Get and deserialize response
                    Helper.Storyteller.UpdateProcess(
                        _user,
                        process),
                    "Expected the update process to return error due to the number of shapes to update exceeds the limit in the database."
                );

            var deserializedResponse = SerializationUtilities.DeserializeObject<ProcessValidationResponse>(ex.RestResponse.Content);

            // Assert that the deserialized response indicates that the process name is required
            var expectedReturnResponse = I18NHelper.FormatInvariant(ProcessValidationResponse.ArtifactLimitExceeded, limit);

            AssertValidationResponse(deserializedResponse, expectedReturnResponse);
        }

        #endregion Tests

        #region Private Methods

        private static void AssertValidationResponse(ProcessValidationResponse deserializedResponse, string expectedContent)
        {
            ThrowIf.ArgumentNull(deserializedResponse, nameof(deserializedResponse));
            ThrowIf.ArgumentNull(expectedContent, nameof(expectedContent));

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

        public static readonly string ArtifactAlreadyLocked = "Artifact locked by another user.";

        public static readonly string ArtifactLimitExceeded = "The Process cannot be saved or published. It has exceeded the maximum {0} shapes. Please refactor it and move more detailed user tasks to included Processes.";

        public static readonly string MinimumNumberBranchValidationFormat = "Decision shape with Id {0} contains less than the minimum of 2 outgoing links.";

        // The message returned in the update process validation response
        public string Message { get; set; }
    }
}
