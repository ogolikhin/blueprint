using CustomAttributes;
using Model;
using Model.Factories;
using NUnit.Framework;
using Helper;
using Model.StorytellerModel;
using Model.StorytellerModel.Impl;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class DeleteUserDecisionTests
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

        [TestCase]
        [Description("Delete a user decision with branch that is after precondition and verify that the returned " +
                     "process has the user decision with all branches except the lowest order branch removed.")]
        public void DeleteUserDecisionWithBranchAfterPrecondition_VerifyReturnedProcess()
        {
            /*
            If you start with this:
                [S]--[P]--+--<UD>--+--[UT1]--+--[ST2]--+--[E]
                              |                        |
                              +-------[UT3]--+--[ST4]--+

            It becomes this:
                [S]--[P]--+--[UT1]--+--[ST2]--+--[E]
            */

            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find precondition task
            var preconditionTask = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var preconditionOutgoingLink = process.GetOutgoingLinkForShape(preconditionTask);

            // Get the branch end point
            var branchEndPoint = process.GetProcessShapeByShapeName(Process.EndName);

            // Add Decision point with branch to end
            var userDecision = process.AddUserDecisionPointWithBranchAfterShape(preconditionTask, preconditionOutgoingLink.Orderindex + 1, branchEndPoint.Id);

            // Save the process
            var returnedProcess = _storyteller.UpdateProcess(_user, process);

            var userDecisionToBeDeleted = returnedProcess.GetProcessShapeByShapeName(userDecision.Name);

            returnedProcess.DeleteUserDecisionWithBranchesNotOfTheLowestOrder(userDecisionToBeDeleted, branchEndPoint);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(returnedProcess, _storyteller, _user);
        }

        [TestCase]
        [Description("Delete user decision with branch before the end shape and verify that the returned " +
                     "process has the user decision with all branches except the lowest order branch removed .")]
        public void DeleteUserDecisionWithBranchBeforeEnd_VerifyReturnedProcess()
        {
            /*
            If you start with this:
                [S]--[P]--+--[UT1]--+--[ST2]--+--<UD>--+--[UT3]--+--[ST4]--+--[E]
                                                   |                       |
                                                   +------[UT5]--+--[ST6]--+

            It becomes this:
                [S]--[P]--+--[UT1]--+--[ST2]--+--[UT3]--+--[ST4]--+--[E]
            */

            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find the end shape
            var endShape = process.GetProcessShapeByShapeName(Process.EndName);

            // Find incoming process link for end shape
            var endIncomingLink = process.GetIncomingLinkForShape(endShape);

            // Add Decision point with branch and 2 user tasks
            var userDecision = process.AddUserDecisionPointWithBranchBeforeShape(endShape, endIncomingLink.Orderindex + 1);

            // Save the process
            var returnedProcess = _storyteller.UpdateProcess(_user, process);

            var userDecisionToBeDeleted = returnedProcess.GetProcessShapeByShapeName(userDecision.Name);

            returnedProcess.DeleteUserDecisionWithBranchesNotOfTheLowestOrder(userDecisionToBeDeleted, endShape);

            // Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(process, _storyteller, _user);
        }

        [TestCase]
        [Description("Delete a user decision point with branch before a second user decision point" +
                     "with branch and verify that the returned process has the first decision point" +
                     "with all branches except the lowest order branch removed.")]
        public void DeleteFirstUserDecisionPointWithMergePointBeforeSecondUserDecision_VerifyReturnedProcess()
        {
            /*
            If you start with this:
                [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+--<UD2>--+--[UT3]--+--[ST4]--+--[E]
                               |                        |    |                        |
                               +-------[UT5]--+--[ST6]--+    +-------[UT7]--+--[ST8]--+
            
            It becomes this:
                [S]--[P]--+--[UT1]--+--[ST2]--+--<UD2>--+--[UT3]--+--[ST4]--+--[E]
                                                   |                        |
                                                   +-------[UT7]--+--[ST8]--+  
            */

            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find precondition task
            var preconditionTask = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var preconditionOutgoingLink = process.GetOutgoingLinkForShape(preconditionTask);

            // Determine the branch endpoint
            var branchEndPoint = process.GetProcessShapeByShapeName(Process.EndName);

            // Add Decision point with branch to end
            var secondUserDecisionInProcess = process.AddUserDecisionPointWithBranchAfterShape(
                preconditionTask, 
                preconditionOutgoingLink.Orderindex + 1, 
                branchEndPoint.Id);

            // Add decision point before decision point; will have same branch order index as previous added branch
            var firstUserDecisionInProcess = process.AddUserDecisionPointWithBranchBeforeShape(
                secondUserDecisionInProcess, 
                preconditionOutgoingLink.Orderindex + 1, 
                secondUserDecisionInProcess.Id);

            // Save the process
            var returnedProcess = _storyteller.UpdateProcess(_user, process);

            var userDecisionToBeDeleted = returnedProcess.GetProcessShapeByShapeName(firstUserDecisionInProcess.Name);

            // Merge point for first user decision is the second user decision on the process
            var mergePointForFirstUserDecision = returnedProcess.GetProcessShapeByShapeName(secondUserDecisionInProcess.Name);

            returnedProcess.DeleteUserDecisionWithBranchesNotOfTheLowestOrder(userDecisionToBeDeleted, mergePointForFirstUserDecision);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(process, _storyteller, _user);
        }

        [TestCase]
        [Description("Delete a user decision with branch that is on the main branch but within an" +
                     "outer branch of another user decision.  Verfiy that the returned process has" +
                     "the inner user decision removed with all branches except for the lowest " +
                     "order branch.  The outer user decision and branch must remain present.")]
        public void DeleteInnerUserDecisionContainedWithinMainBranchOfOuterUserDecision_VerifyReturnedProcess()
        {
            /*
            If you start with this:
                [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+--<UD2>--+--[UT3]--+--[ST4]--+--[E]
                               |                             |                        |
                               |                             +-------[UT5]--+--[ST6]--+
                               |                                                      |
                               +--[UT7]--+--[ST8]--+----------------------------------+

            It becomes this:
                [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+--[UT3]--+--[ST4]--+--[E]
                               |                                            |
                               +--[UT7]--+--[ST8]--+------------------------+
            */

            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find precondition task
            var preconditionTask = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find the outgoing link for the precondition
            var preconditionOutgoingLink = process.GetOutgoingLinkForShape(preconditionTask);

            // Add Decision point with branch
            var innerUserDecision = process.AddUserDecisionPointWithBranchAfterShape(preconditionTask, preconditionOutgoingLink.Orderindex + 1);

            // Find the updated outgoing link for the precondition
            preconditionOutgoingLink = process.GetOutgoingLinkForShape(preconditionTask);

            // Add user and system task before new user decision point
            process.AddUserAndSystemTask(preconditionOutgoingLink);

            // Find the updated outgoing link for the precondition
            preconditionOutgoingLink = process.GetOutgoingLinkForShape(preconditionTask);

            // Add an outer user decision point after the precondition and before the new user/system task
            process.AddUserDecisionPointWithBranchAfterShape(preconditionTask, preconditionOutgoingLink.Orderindex + 1);

            // Save the process
            var returnedProcess = _storyteller.UpdateProcess(_user, process);

            var userDecisionToBeDeleted = returnedProcess.GetProcessShapeByShapeName(innerUserDecision.Name);

            // Merge point for the inner user decision is the process end point
            var endShape = process.GetProcessShapeByShapeName(Process.EndName);

            returnedProcess.DeleteUserDecisionWithBranchesNotOfTheLowestOrder(userDecisionToBeDeleted, endShape);

            // Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(process, _storyteller, _user);
        }

        [TestCase]
        [Description("Delete a user decision with branch that is on the main branch but contains an" +
             "inner branch with another user decision.  Verfiy that the returned  process has" +
             "the outer user decision removed with all branches except for the lowest " +
             "order branch.  The inner user decision and branch must remain present.")]
        public void DeleteOuterUserDecisionContainingInnerBranchWithSecondUserDecision_VerifyReturnedProcess()
        {
            /*
            If you start with this:
                [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+--<UD2>--+--[UT3]--+--[ST4]--+--[E]
                               |                             |                        |
                               |                             +-------[UT5]--+--[ST6]--+
                               |                                                      |
                               +--[UT7]--+--[ST8]--+----------------------------------+

            It becomes this:
                [S]--[P]--+--[UT1]--+--[ST2]--+--<UD2>--+--[UT3]--+--[ST4]--+--[E]
                                                   |                        |
                                                   +-------[UT5]--+--[ST6]--+
            */

            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find precondition task
            var preconditionTask = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find the outgoing link for the precondition
            var preconditionOutgoingLink = process.GetOutgoingLinkForShape(preconditionTask);

            // Add Decision point with branch
            process.AddUserDecisionPointWithBranchAfterShape(preconditionTask, preconditionOutgoingLink.Orderindex + 1);

            // Find the updated outgoing link for the precondition
            preconditionOutgoingLink = process.GetOutgoingLinkForShape(preconditionTask);

            // Add user and system task before new user decision point
            process.AddUserAndSystemTask(preconditionOutgoingLink);

            // Find the updated outgoing link for the precondition
            preconditionOutgoingLink = process.GetOutgoingLinkForShape(preconditionTask);

            // Add an outer user decision point after the precondition and before the new user/system task
            var outerUserDecision = process.AddUserDecisionPointWithBranchAfterShape(preconditionTask, preconditionOutgoingLink.Orderindex + 1);

            // Save the process
            var returnedProcess = _storyteller.UpdateProcess(_user, process);

            var userDecisionToBeDeleted = returnedProcess.GetProcessShapeByShapeName(outerUserDecision.Name);

            // Merge point for the outer user decision is the process end point
            var endShape = process.GetProcessShapeByShapeName(Process.EndName);

            returnedProcess.DeleteUserDecisionWithBranchesNotOfTheLowestOrder(userDecisionToBeDeleted, endShape);

            // Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(process, _storyteller, _user);
        }

        [TestCase]
        [Description("Delete a user decision point with more than 2 conditions and verify that the returned process" +
                     "has the user decision with all branches except the lowest order branch removed.")]
        public void DeleteUserDecisionWithMoreThanTwoConditions_VerifyReturnedProcess()
        {
            /*
            If you start with this:
                [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+--[E]
                               |                        |
                               +--[UT3]--+--[ST4]-------+
                               |                        |
                               +--[UT5]--+--[ST6]-------+

            It becomes this:
                [S]--[P]--+--[UT1]--+--[ST2]--+--[E]
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

            var userDecisionToBeDeleted = returnedProcess.GetProcessShapeByShapeName(userDecision.Name);

            // Merge point for the outer user decision is the process end point
            var endShape = process.GetProcessShapeByShapeName(Process.EndName);

            returnedProcess.DeleteUserDecisionWithBranchesNotOfTheLowestOrder(userDecisionToBeDeleted, endShape);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(process, _storyteller, _user);
        }

        [TestCase]
        [Description("Delete a user decision point that exists on an outer branch of another" +
                     "user decision and verify that the returned process has the decisions" +
                     "point and all branches removed except for the lowest order branch.")]
        public void DeleteUserDecisionOnOuterBranchOfAnotherDecisionPoint_VerifyReturnedProcess()
        {
            /*
            If you start with this:
                [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+------------------------+--[E]
                               |                                                 |
                               +--[UT3]--+--[ST4]--+--[UD2]--+--[UT5]--+--[ST6]--+
                                                        |                        |
                                                        +--[UT7]--+--[ST8]-------+

            It becomes this:
                [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+--------------+--[E]
                               |                                       |
                               +--[UT3]--+--[ST4]--+--[UT5]--+--[ST6]--+
            */

            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find precondition task
            var preconditionTask = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var preconditionOutgoingLink = process.GetOutgoingLinkForShape(preconditionTask);

            // Determine the branch endpoint
            var branchEndPoint = process.GetProcessShapeByShapeName(Process.EndName);

            // Add decision point with branch after precondition
            var userDecision = process.AddUserDecisionPointWithBranchAfterShape(
                preconditionTask, 
                preconditionOutgoingLink.Orderindex + 1, 
                branchEndPoint.Id);

            // Outgoing process link for decision point branch
            var outgoingProcessLinkForUserDecision = process.GetOutgoingLinkForShape(
                userDecision,
                preconditionOutgoingLink.Orderindex + 1);

            // Determine new user task from decision point outgoing process link
            var newUserTask = process.GetProcessShapeById(outgoingProcessLinkForUserDecision.DestinationId);

            var newSystemTask = process.GetNextShape(newUserTask);

            // Add second user decision point with branch to end after new system task
            var secondUserDecision = process.AddUserDecisionPointWithBranchAfterShape(
                newSystemTask, 
                preconditionOutgoingLink.Orderindex + 1, 
                branchEndPoint.Id);

            // Save the process
            var returnedProcess = _storyteller.UpdateProcess(_user, process);

            var userDecisionToBeDeleted = returnedProcess.GetProcessShapeByShapeName(secondUserDecision.Name);

            // Merge point for the second user decision is the process end point
            var endShape = process.GetProcessShapeByShapeName(Process.EndName);

            returnedProcess.DeleteUserDecisionWithBranchesNotOfTheLowestOrder(userDecisionToBeDeleted, endShape);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(process, _storyteller, _user);
        }

        [TestCase]
        [Description("Delete a user decision point that has an outer branch that contains a second" +
                     "user decision point with a branch. Verify that the returned process has the" +
                     "first user decision point and all branches deleted, including the second user" +
                     "decision point and all branches. The lowest lowest order branch of the first" +
                     "user decision point must remain.")]
        public void DeleteUserDecisionContainingUserDecisionOnOuterBranch_VerifyReturnedProcess()
        {
            /*
            If you start with this:
                [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+------------------------+--[E]
                               |                                                 |
                               +--[UT3]--+--[ST4]--+--[UD2]--+--[UT5]--+--[ST6]--+
                                                        |                        |
                                                        +--[UT7]--+--[ST8]-------+

            It becomes this:
                [S]--[P]--+--[UT1]--+--[ST2]--+--[E]
            */

            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find precondition task
            var preconditionTask = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var preconditionOutgoingLink = process.GetOutgoingLinkForShape(preconditionTask);

            // Determine the branch endpoint
            var branchEndPoint = process.GetProcessShapeByShapeName(Process.EndName);

            // Add decision point with branch after precondition
            var firstUserDecision = process.AddUserDecisionPointWithBranchAfterShape(
                preconditionTask,
                preconditionOutgoingLink.Orderindex + 1,
                branchEndPoint.Id);

            // Outgoing process link for decision point branch
            var outgoingProcessLinkForUserDecision = process.GetOutgoingLinkForShape(
                firstUserDecision,
                preconditionOutgoingLink.Orderindex + 1);

            // Determine new user task from decision point outgoing process link
            var newUserTask = process.GetProcessShapeById(outgoingProcessLinkForUserDecision.DestinationId);

            var newSystemTask = process.GetNextShape(newUserTask);

            // Add second user decision point with branch to end after new system task
            process.AddUserDecisionPointWithBranchAfterShape(
                newSystemTask,
                preconditionOutgoingLink.Orderindex + 1,
                branchEndPoint.Id);

            // Save the process
            var returnedProcess = _storyteller.UpdateProcess(_user, process);

            var userDecisionToBeDeleted = returnedProcess.GetProcessShapeByShapeName(firstUserDecision.Name);

            // Merge point for the user decision is the process end point
            var endShape = process.GetProcessShapeByShapeName(Process.EndName);

            returnedProcess.DeleteUserDecisionWithBranchesNotOfTheLowestOrder(userDecisionToBeDeleted, endShape);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(process, _storyteller, _user);
        }


    }
}
