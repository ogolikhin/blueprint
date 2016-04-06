using CustomAttributes;
using Model;
using Model.Factories;
using NUnit.Framework;
using Helper;
using Model.StorytellerModel;
using Model.StorytellerModel.Impl;
using System.Collections.Generic;
using Model.OpenApiModel;
using System.Linq;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class DeleteUserDecisionBranchTests
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

        [TestCase(0)]
        [TestCase(1)]
        [Description("Delete a branch from a user decision point that has more than 2 conditions and verify that the " +
                     "returned process still contains the user decision with all branches except the deleted branch.")]
        public void DeleteBranchfromUserDecisionWithMoreThanTwoConditions_VerifyReturnedProcess(double orderIndexOfBranch)
        {
            /*
            If you start with this:
                [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+--[E]
                               |                        |
                               +--[UT3]--+--[ST4]-------+
                               |                        |
                               +--[UT5]--+--[ST6]-------+

            It becomes this (Shape names depend on branch that was deleted):
                [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+--[E]
                                |                       |
                                +--[UT3]--+--[ST4]------+
            */

            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find precondition task
            var preconditionTask = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var preconditionOutgoingLink = process.GetOutgoingLinkForShape(preconditionTask);

            // Find the endpoint for the new branch
            var branchEndPoint = process.GetProcessShapeByShapeName(Process.EndName);

            // Add decision point with branch to end
            var userDecision = process.AddUserDecisionPointWithBranchAfterShape(preconditionTask, preconditionOutgoingLink.Orderindex + 1);

            // Add a second branch with task to decision point
            process.AddBranchWithUserAndSystemTaskToUserDecisionPoint(userDecision, preconditionOutgoingLink.Orderindex + 2, branchEndPoint.Id);

            // Save the process
            var returnedProcess = _storyteller.UpdateProcess(_user, process);

            var userDecisionWithBranchToBeDeleted= returnedProcess.GetProcessShapeByShapeName(userDecision.Name);

            // Delete the specified user decision branch
            returnedProcess.DeleteUserDecisionBranch(userDecisionWithBranchToBeDeleted, orderIndexOfBranch, branchEndPoint);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);
        }

        [TestCase]
        [Description("Delete a branch from a user decision point that has a nested user decision on one of its branches." +
                     "Verify that the returned process has the branch deleted along with the nested user decision and " +
                     "associated branches.")]
        public void DeleteBranchfromUserDecisionThatContainsNestedUserDecision_VerifyReturnedProcess()
        {
            /*
            If you start with this:
                [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+------------------------+--[E]
                               |                                                 |
                               +--[UT3]--+--[ST4]--------------------------------+ 
                               |                                                 |
                               +--[UT5]--+--[ST6]--+--<UD2>--+--[UT7]--+--[ST8]--+
                                                        |                        |
                                                        +--[UT9]--+--[ST10]------+

            And becomes this if third branch is deleted:
                [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+------------------------+--[E]
                               |                                                 |
                               +--[UT3]--+--[ST4]--------------------------------+ 
            */

            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find precondition task
            var preconditionTask = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var preconditionOutgoingLink = process.GetOutgoingLinkForShape(preconditionTask);

            // Determine the branch endpoint
            var branchEndPoint = process.GetProcessShapeByShapeName(Process.EndName);

            // Add decision point <UD1> with branch after precondition (adds 2 branches)
            var firstUserDecision = process.AddUserDecisionPointWithBranchAfterShape(
                preconditionTask,
                preconditionOutgoingLink.Orderindex + 1,
                branchEndPoint.Id);

            // Add third branch to userDecision
            process.AddBranchWithUserAndSystemTaskToUserDecisionPoint(
                firstUserDecision,
                preconditionOutgoingLink.Orderindex + 2,
                branchEndPoint.Id);

            // Outgoing process link for third decision point branch
            var outgoingProcessLinkForUserDecision = process.GetOutgoingLinkForShape(
                firstUserDecision,
                preconditionOutgoingLink.Orderindex + 2);

            // Determine new user task from decision point outgoing process link
            var newUserTaskOnThirdBranch = process.GetProcessShapeById(outgoingProcessLinkForUserDecision.DestinationId);

            var newSystemTaskOnThirdBranch = process.GetNextShape(newUserTaskOnThirdBranch);

            // Add second user decision point <UD2> with branch to end after new system task
            process.AddUserDecisionPointWithBranchAfterShape(
                newSystemTaskOnThirdBranch,
                preconditionOutgoingLink.Orderindex + 1,
                branchEndPoint.Id);

            // Save the process
            var returnedProcess = _storyteller.UpdateProcess(_user, process);

            // Find the first user decision in the returned process
            var returnedFirstUserDecision = returnedProcess.GetProcessShapeByShapeName(firstUserDecision.Name);

            // Delete the third branch for the first user decision
            returnedProcess.DeleteUserDecisionBranch(returnedFirstUserDecision, preconditionOutgoingLink.Orderindex + 2, branchEndPoint);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);
        }

        [TestCase]
        [Description("Delete a branch from a user decision point that has a nested system decision on one of its branches." +
             "Verify that the returned process has the branch deleted along with the nested system decision and " +
             "associated branches.")]
        public void DeleteBranchfromUserDecisionThatContainsNestedSystemDecision_VerifyReturnedProcess()
        {
            /*
            If you start with this:
                [S]--[P]--+--<UD>--+--[UT1]--+--[ST2]--+-----+--[E]
                               |                             |
                               +--[UT3]--+--[ST4]------------+ 
                               |                             |
                               +--[UT5]--+--<SD>--+--[ST6]---+
                                              |              |
                                              +----+--[ST7]--+

            And becomes this if third branch is deleted:
                [S]--[P]--+--<UD>--+--[UT1]--+--[ST2]--+-----+--[E]
                               |                             |
                               +--[UT3]--+--[ST4]------------+ 
            */

            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find precondition task
            var preconditionTask = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var preconditionOutgoingLink = process.GetOutgoingLinkForShape(preconditionTask);

            // Determine the branch endpoint
            var branchEndPoint = process.GetProcessShapeByShapeName(Process.EndName);

            // Add decision point <UD> with branch after precondition (adds 2 branches)
            var firstUserDecision = process.AddUserDecisionPointWithBranchAfterShape(
                preconditionTask,
                preconditionOutgoingLink.Orderindex + 1,
                branchEndPoint.Id);

            // Add third branch to userDecision
            process.AddBranchWithUserAndSystemTaskToUserDecisionPoint(
                firstUserDecision,
                preconditionOutgoingLink.Orderindex + 2,
                branchEndPoint.Id);

            // Outgoing process link for third decision point branch
            var outgoingProcessLinkForUserDecision = process.GetOutgoingLinkForShape(
                firstUserDecision,
                preconditionOutgoingLink.Orderindex + 2);

            // Determine new user task from decision point outgoing process link
            var newUserTaskOnThirdBranch = process.GetProcessShapeById(outgoingProcessLinkForUserDecision.DestinationId);

            var newSystemTaskOnThirdBranch = process.GetNextShape(newUserTaskOnThirdBranch);

            // Add system decision point <SD> with branch after user task on third branch
            process.AddSystemDecisionPointWithBranchBeforeSystemTask(
                newSystemTaskOnThirdBranch,
                preconditionOutgoingLink.Orderindex + 2,
                branchEndPoint.Id);

            // Save the process
            var returnedProcess = _storyteller.UpdateProcess(_user, process);

            // Find the first user decision in the returned process
            var returnedFirstUserDecision = returnedProcess.GetProcessShapeByShapeName(firstUserDecision.Name);

            // Delete the third branch for the first user decision
            returnedProcess.DeleteUserDecisionBranch(returnedFirstUserDecision, preconditionOutgoingLink.Orderindex + 2, branchEndPoint);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);
        }
    }
}
