﻿using System.Linq;
using Common;
using CustomAttributes;
using Model;
using Model.OpenApiModel;
using Model.Factories;
using NUnit.Framework;
using System.Collections.Generic;
using Helper;
using Model.StorytellerModel;
using Model.StorytellerModel.Impl;
using Utilities.Factories;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class BranchLabelTests
    {
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
                // TODO: implement discard artifacts for test cases that doesn't publish artifacts
                // Delete all the artifacts that were added.
                foreach (var artifact in _storyteller.Artifacts.ToArray())
                {
                    _storyteller.DeleteProcessArtifact(artifact, deleteChildren: _deleteChildren);
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

        [TestCase(1, 0.0)]
        [TestCase(5, 0.0)]
        [TestCase(30, 0.0)]
        [TestCase(1, 1.0)]
        [TestCase(5, 1.0)]
        [TestCase(30, 1.0)]
        [Description("Add a randomized user decision branch label for a specific branch and verify that the label is returned " +
                     "after saving the process.")]
        public void AddUserDecisionBranchWithPlainTextLabelOfVaryingLength_VerifyReturnedBranchLabel(
            int lengthOfLabelSent, 
            double orderIndexOfUserDecisionBranch)
        {
            var process = CreateProcessWithSingleUserDecision();

            // Get precondition shape in process
            var precondition = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Get user decision shape in process
            var userDecision = process.GetNextShape(precondition);

            // Get link for specified branch by order index
            var branchLink = process.GetOutgoingLinkForShape(userDecision, orderIndexOfUserDecisionBranch);

            // Set a randomized link label for the specified branch
            branchLink.Label = RandomGenerator.RandomAlphaNumericUpperAndLowerCase((uint)lengthOfLabelSent);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(process, _storyteller, _user);
        }

        [TestCase(5, 0.0, ProcessType.BusinessProcess)]
        [TestCase(5, 1.0, ProcessType.UserToSystemProcess)]
        [Description("Add a randomized user decision label for different process types and verify the returned label" +
                     "is returned after saving the process.")]
        public void AddUserDecisionBranchLabelForDifferentProcessTypes_VerifyReturnedBranchLabelIsPresent(
            int lengthOfLabelSent,
            double orderIndexOfUserDecisionBranch,
            ProcessType processType)
        {
            var process = CreateProcessWithSingleUserDecision();

            // Get precondition shape in process
            var precondition = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Get user decision shape in process
            var userDecision = process.GetNextShape(precondition);

            // Get link for specified branch by order index
            var branchLink = process.GetOutgoingLinkForShape(userDecision, orderIndexOfUserDecisionBranch);

            // Set a randomized link label for the specified branch
            branchLink.Label = RandomGenerator.RandomAlphaNumericUpperAndLowerCase((uint)lengthOfLabelSent);

            // Set the process type
            process.ProcessType = processType;

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(process, _storyteller, _user);
        }

        [TestCase(5, 0.0)]
        [TestCase(5, 1.0)]
        [Description("Modify a user decision branch label and verify that the returned label is changed " +
                     "after saving the process.")]
        public void ModifyUserDecisionBranchLabel_VerifyReturnedBranchLabelIsModified(
            int lengthOfLabelSent,
            double orderIndexOfUserDecisionBranch)
        {
            var process = CreateProcessWithSingleUserDecision();

            // Save the process
            _storyteller.UpdateProcess(_user, process);

            // Get the process
            var returnedProcess = _storyteller.GetProcess(_user, process.Id);

            // Get precondition shape in process
            var precondition = returnedProcess.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Get user decision shape in process
            var returnedUserDecision = returnedProcess.GetNextShape(precondition);

            // Get link for specified branch by order index
            var branchLink = returnedProcess.GetOutgoingLinkForShape(returnedUserDecision, orderIndexOfUserDecisionBranch);

            // Set a randomized link label for the specified branch
            branchLink.Label = RandomGenerator.RandomAlphaNumericUpperAndLowerCase((uint)lengthOfLabelSent);

            // Update and Verify the returned process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(returnedProcess, _storyteller, _user);
        }

        [TestCase(0.0)]
        [TestCase(1.0)]
        [Description("Delete a user decision branch label and verify that the returned label is null " +
                     "after saving the process.")]
        public void DeleteUserDecisionBranchLabel_VerifyReturnedBranchHasLabelRemoved(
            double orderIndexOfUserDecisionBranch)
        {
            var process = CreateProcessWithSingleUserDecision();

            // Save the process
            _storyteller.UpdateProcess(_user, process);

            // Get the process
            var returnedProcess = _storyteller.GetProcess(_user, process.Id);

            // Get precondition shape in process
            var precondition = returnedProcess.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Get user decision shape in process
            var returnedUserDecision = returnedProcess.GetNextShape(precondition);

            // Get link for specified branch by order index
            var branchLink = returnedProcess.GetOutgoingLinkForShape(returnedUserDecision, orderIndexOfUserDecisionBranch);

            // Clear the link label for the specified branch
            branchLink.Label = null;

            // Update and Verify the returned process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(returnedProcess, _storyteller, _user);
        }

        /// <summary>
        /// Create a Process that Contains a Single User Decision
        /// </summary>
        /// <returns>The created process</returns>
        private IProcess CreateProcessWithSingleUserDecision()
        {
            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find precondition task
            var preconditionTask = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var preconditionOutgoingLink = process.GetOutgoingLinkForShape(preconditionTask);

            // Get the branch end point
            var branchEndPoint = process.GetProcessShapeByShapeName(Process.EndName);

            // Add Decision point with branch to end
            process.AddUserDecisionPointWithBranchAfterShape(
                preconditionTask,
                preconditionOutgoingLink.Orderindex + 1,
                branchEndPoint.Id);

            return process;
        }
    }
}
