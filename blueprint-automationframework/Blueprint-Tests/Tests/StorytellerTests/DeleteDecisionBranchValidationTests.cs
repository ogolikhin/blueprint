﻿
using System.Collections.Generic;
using System.Linq;
using CustomAttributes;
using Model;
using Model.Factories;
using Model.OpenApiModel;
using Model.StorytellerModel;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using Helper;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class DeleteDecisionBranchValidationTests
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
                var savedArtifactsList = new List<IOpenApiArtifact>();
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
        [Description("Deleting an only available branch from system decision and verify that " +
                     "the returned response contains error message when save process call gets made")]
        public void DeleteOnlyBranchFromSystemDecision_VerifyUpdateProcessReturnsValidationError()
        {
            /*
            If you start with this:
            [S]--[P]--+--[UT1]--+--<SD1>--+-------[ST1]--------+--[E]
                                     |                         |
                                     +----+-------[ST2]--------+
     
            This test validatates if saving the process after deleting only additional branch
            returns the 400 error so that it prevents the process ends up like the invalid graph below:
            [S]--[P]--+--[UT1]--+--<SD1>--+--[ST1]--+--[E]

            */

            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find the default UserTask
            var defaultUserTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the target SystemTask
            var targetSystemTask = process.GetProcessShapeByShapeName(Process.DefaultSystemTaskName);

            // Find the branch end point for system decision points
            var branchEndPoint = process.GetProcessShapeByShapeName(Process.EndName);

            // Find the outgoing process link from the default UserTask
            var defaultUserTaskOutgoingProcessLink = process.GetOutgoingLinkForShape(defaultUserTask);

            // Add a System Decision point (root System Decision point) with branch merging to branchEndPoint
            var rootSystemDecisionPoint = process.AddSystemDecisionPointWithBranchBeforeSystemTask(targetSystemTask, defaultUserTaskOutgoingProcessLink.Orderindex + 1, branchEndPoint.Id);

            // Save the process
            var returnedProcess = _storyteller.UpdateProcess(_user, process);

            // Get the link between the system decision point and the System task on the second branch
            var branchingProcessLink = process.Links.Find(l => l.Orderindex.Equals(defaultUserTaskOutgoingProcessLink.Orderindex + 1));

            // Get the System Task shape on the second branch from the System Decision
            var systemTaskOnTheSecondBranch = process.GetProcessShapeById(branchingProcessLink.DestinationId);

            // Delete the specified system decision branch - work in progress
            //returnedProcess.DeleteSystemDecisionBranch(systemDecisionPointForDeletionProcess, defaultUserTaskOutgoingProcessLink.Orderindex + 1, endShape);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(process, _storyteller, _user);


        }

        [TestCase]
        [Description("Deleting an only available branch from user decision and verify that " +
                     "the returned response contains error message when save process call gets made")]
        public void DeleteOnlyBranchFromUserDecision_VerifyUpdateProcessReturnsValidationError()
        {
            /*
            If you start with this:
                [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+--[E]
                               |                        |
                               +--[UT3]--+--[ST4]-------+

            This test validatates if saving the process after deleting only additional branch
            returns the 400 error so that it prevents the process ends up like the invalid graph below:
                [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+--[E]
            */

            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find precondition task
            var preconditionTask = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var preconditionOutgoingLink = process.GetOutgoingLinkForShape(preconditionTask);

            // Find the branch endpoint for the new branch
            var branchEndPoint = process.GetProcessShapeByShapeName(Process.EndName);

            // Find the outgoing process link from the default SystemTask
            var defaultSystemTaskOutgoingProcessLink = process.GetOutgoingLinkForShape(preconditionTask);

            // Add decision point with branch to end
            var userDecision = process.AddUserDecisionPointWithBranchAfterShape(preconditionTask, preconditionOutgoingLink.Orderindex + 1);

            // Save the process
            var returnedProcess = _storyteller.UpdateProcess(_user, process);

            // Get the link between the user decision and the user task on the second branch
            var branchingProcessLink = process.Links.Find(l => l.Orderindex.Equals(preconditionOutgoingLink.Orderindex + 1));

            // Get the User Task shape on the second branch from the User Decision
            var userTaskOnTheSecondBranch = process.GetProcessShapeById(branchingProcessLink.DestinationId);

            // Delete the specified user decision branch - work in progress
            //returnedProcess.DeleteUserDecisionBranch(systemDecisionPointForDeletionProcess, preconditionOutgoingLink.Orderindex + 1, endShape);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);
        }

        #endregion Tests

    }
}
