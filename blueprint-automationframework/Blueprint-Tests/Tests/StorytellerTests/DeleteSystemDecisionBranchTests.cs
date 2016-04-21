using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.OpenApiModel;
using Model.StorytellerModel;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class DeleteSystemDecisionBranchTests
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

        [TestCase(1)]
        [TestCase(2)]
        [Description("Delete a branch from a system decision point that has more than 2 conditions and verify that the " +
                     "returned process still contains the system decision with all branches except the deleted branch.")]
        public void DeleteBranchFromSystemDecisionWithMoreThanTwoConditions_VerifyReturnedProcess(double orderIndexOfBranch)
        {
            /*
            If you start with this:
            [S]--[P]--+--[UT1]--+--<SD1>--+--[ST1]--+--[E]
                                     |              |
                                     +----+--[ST2]--+
                                     |              |
                                     +----+--[ST3]--+

            It becomes this:
            [S]--[P]--+--[UT1]--+--<SD1>--+--[ST1]--+--[E]
                                     |              |
                                     +----+--[ST2]--+
            */

            // Create and Save the process with one system decision with two branches plus main branch
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcessWithOneSystemDecisionContainingMultipleConditions(
                    _storyteller, _project, _user, 1);

            // Find the endShape for the system decision
            var endShape = returnedProcess.GetProcessShapeByShapeName(Process.EndName);

            // Find the System Decision with a branch merging to endShape
            var systemDecisionForBranchDeletion = returnedProcess.GetProcessShapesByShapeType(ProcessShapeType.SystemDecision).First();

            // Delete the specified system decision branch - work in progress
            returnedProcess.DeleteSystemDecisionBranch(systemDecisionForBranchDeletion, orderIndexOfBranch, endShape);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);
        }

        [TestCase]
        [Description("Delete a branch from a system decision point that has a nested system decision on one of the" +
                     " branches and verify that the returned process still contains the system decision with all" +
                     " branches except the deleted branch.")]
        public void DeleteBranchFromSystemDecisionThatContainsNestedSystemDecision_VerifyReturnedProcess()
        {
            /*
            If you start with this:
            [S]--[P]--+--[UT1]--+--<SD1>--+-------[ST1]--------+--[E]
                                     |                         |
                                     +----+-------[ST2]--------+
                                     |                         |
                                     +--<SD2>--+--[ST3]--------+
                                          |                    |
                                          +----+--[ST4]--------+
            It becomes this:
            [S]--[P]--+--[UT1]--+--<SD1>--+--[ST1]--+--[E]
                                     |              |
                                     +----+--[ST2]--+
            */

            // Create the process with one system decision with three branches plus main branch
            var process = StorytellerTestHelper.CreateAndGetDefaultProcessWithOneSystemDecisionContainingMultipleConditions(
                    _storyteller, _project, _user, 1, updateProcess: false);

            // Find the precondition
            var precondition = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find the endShape for the system decision
            var endShape = process.GetProcessShapeByShapeName(Process.EndName);

            // Find the outgoing link for the precondition
            var outgoingLinkForPrecondition = process.GetOutgoingLinkForShape(precondition);

            // Find the root System Decision with a branch merging to endShape
            var rootSystemDecision = process.GetNextShape(process.GetNextShape(precondition));

            // Find the System Task on the third branch to add a System Decision
            var systemTaskOnTheThirdBranch = process.GetNextShape(rootSystemDecision,
                outgoingLinkForPrecondition.Orderindex + 2);

            // Find the outgoing link for the system task on the third branch
            var outgoingLinkForSystemTaskOnTheThirdBranch = process.GetOutgoingLinkForShape(systemTaskOnTheThirdBranch);

            // Add a System Decision on the third branch that merges to branchEndPoint
            process.AddSystemDecisionPointWithBranchBeforeSystemTask(systemTaskOnTheThirdBranch, outgoingLinkForSystemTaskOnTheThirdBranch.Orderindex + 1, endShape.Id);

            // Save the process
            var returnedProcess = _storyteller.UpdateProcess(_user, process);

            var systemDecisionForBranchDeletion = returnedProcess.GetNextShape(returnedProcess.GetNextShape(precondition));

            // Delete the specified system decision branch
            returnedProcess.DeleteSystemDecisionBranch(systemDecisionForBranchDeletion, outgoingLinkForPrecondition.Orderindex + 2, endShape);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(process, _storyteller, _user);
        }


        [TestCase]
        [Description("Delete a branch from a system decision point that has a nested user decision on one of the" +
                     " branches and verify that the returned process still contains the system decision with all" +
                     " branches except the deleted branch.")]
        public void DeleteBranchFromSystemDecisionThatContainsNestedUserDecision_VerifyReturnedProcess()
        {
            /*
            If you start with this:
            [S]--[P]--+--[UT1]--+--<SD1>--+--[ST1]---+-----------------------------+--[E]
                                     |                                             |
                                     +----+--[ST2]---+-----------------------------|
                                     |                                             |
                                     +----+--[ST3]---+--<UD1>--+--[UT2]--+--[ST4]--+
                                                          |                        |
                                                          +----+--[UT3]--+--[ST5]--+
            It becomes this:
            [S]--[P]--+--[UT1]--+--<SD1>--+--[ST1]--+--[E]
                                     |              |
                                     +----+--[ST2]--+
            */

            // Create the process with one system decision with three branches plus main branch
            var process = StorytellerTestHelper.CreateAndGetDefaultProcessWithOneSystemDecisionContainingMultipleConditions(
                    _storyteller, _project, _user, 1, updateProcess: false);

            // Find the precondition
            var precondition = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find the endShape for the system decision
            var endShape = process.GetProcessShapeByShapeName(Process.EndName);

            // Find the outgoing link for the precondition
            var outgoingLinkForPrecondition = process.GetOutgoingLinkForShape(precondition);

            // Find the root System Decision with a branch merging to endShape
            var rootSystemDecision = process.GetNextShape(process.GetNextShape(precondition));

            // Find the System Task on the third branch to add a System Decision
            var systemTaskOnTheThirdBranch = process.GetNextShape(rootSystemDecision,
                outgoingLinkForPrecondition.Orderindex + 2);

            // Find the outgoing link for the system task on the third branch
            var outgoingLinkForSystemTaskOnTheThirdBranch = process.GetOutgoingLinkForShape(systemTaskOnTheThirdBranch);

            // Add a user decision point <UD1> with branch to end after new system task
            process.AddUserDecisionPointWithBranchAfterShape(systemTaskOnTheThirdBranch, outgoingLinkForSystemTaskOnTheThirdBranch.Orderindex + 1, endShape.Id);

            // Save the process
            var returnedProcess = _storyteller.UpdateProcess(_user, process);

            var systemDecisionForBranchDeletion = returnedProcess.GetNextShape(returnedProcess.GetNextShape(precondition));

            // Delete the specified system decision branch - work in progress
            returnedProcess.DeleteSystemDecisionBranch(systemDecisionForBranchDeletion, outgoingLinkForPrecondition.Orderindex + 2, endShape);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);
        }
    }
}
