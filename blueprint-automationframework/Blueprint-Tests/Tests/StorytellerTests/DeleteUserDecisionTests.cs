using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using TestCommon;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class DeleteUserDecisionTests : TestBase
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
            var novaProcess = Helper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Find precondition task
            var preconditionTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var preconditionOutgoingLink = novaProcess.Process.GetOutgoingLinkForShape(preconditionTask);

            // Get the branch end point
            var branchEndPoint = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Add Decision point with branch to end
            var userDecision = novaProcess.Process.AddUserDecisionPointWithBranchAfterShape(preconditionTask, preconditionOutgoingLink.Orderindex + 1, branchEndPoint.Id);

            // Save the process
            novaProcess.Update(_user, novaProcess.Artifact);
            novaProcess.RefreshArtifactFromServer(_user);

            var userDecisionToBeDeleted = novaProcess.Process.GetProcessShapeByShapeName(userDecision.Name);

            novaProcess.Process.DeleteUserDecisionWithBranchesNotOfTheLowestOrder(userDecisionToBeDeleted, branchEndPoint);

            // Update and Verify the modified process
            Helper.UpdateAndVerifyNovaProcess(novaProcess.NovaProcess, _user);
        }

        [TestCase]
        [Description("Delete user decision with branch before the end shape and verify that the returned " +
                     "process has the user decision with all branches except the lowest order branch removed .")]
        public void DeleteUserDecisionWithBranchAfterDefaultSystemTask_VerifyReturnedProcess()
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
            var novaProcess = Helper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Find default system task
            var defaultSystemTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultSystemTaskName);

            // Find outgoing process link for default system task
            var defaultSystemTaskOutgoingLink = novaProcess.Process.GetOutgoingLinkForShape(defaultSystemTask);

            // Add Decision point with branch and 2 user tasks
            var userDecision = novaProcess.Process.AddUserDecisionPointWithBranchAfterShape(defaultSystemTask, defaultSystemTaskOutgoingLink.Orderindex + 1);

            // Save the process
            novaProcess.Update(_user, novaProcess.Artifact);
            novaProcess.RefreshArtifactFromServer(_user);

            var userDecisionToBeDeleted = novaProcess.Process.GetProcessShapeByShapeName(userDecision.Name);

            // Find the end shape
            var endShape = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            novaProcess.Process.DeleteUserDecisionWithBranchesNotOfTheLowestOrder(userDecisionToBeDeleted, endShape);

            // Verify the modified process
            Helper.UpdateAndVerifyNovaProcess(novaProcess.NovaProcess, _user);
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
            var novaProcess = Helper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Find precondition task
            var preconditionTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var preconditionOutgoingLink = novaProcess.Process.GetOutgoingLinkForShape(preconditionTask);

            // Determine the branch endpoint
            var branchEndPoint = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Add Decision point <UD2> with branch after precondition
            var secondUserDecisionInProcess = novaProcess.Process.AddUserDecisionPointWithBranchAfterShape(
                preconditionTask, 
                preconditionOutgoingLink.Orderindex + 1, 
                branchEndPoint.Id);

            // Find updatedoutgoing process link for precondition
            preconditionOutgoingLink = novaProcess.Process.GetOutgoingLinkForShape(preconditionTask);

            // Add new user and system task before decision point
            novaProcess.Process.AddUserAndSystemTask(preconditionOutgoingLink);

            // Find updatedoutgoing process link for precondition
            preconditionOutgoingLink = novaProcess.Process.GetOutgoingLinkForShape(preconditionTask);

            // Add decision point <UD1> after precondition
            var firstUserDecisionInProcess = novaProcess.Process.AddUserDecisionPointWithBranchAfterShape(
                preconditionTask, 
                preconditionOutgoingLink.Orderindex + 1, 
                secondUserDecisionInProcess.Id);

            // Save the process
            novaProcess.Update(_user, novaProcess.Artifact);
            novaProcess.RefreshArtifactFromServer(_user);

            var userDecisionToBeDeleted = novaProcess.Process.GetProcessShapeByShapeName(firstUserDecisionInProcess.Name);

            // Merge point for first user decision is the second user decision on the process
            var mergePointForFirstUserDecision = novaProcess.Process.GetProcessShapeByShapeName(secondUserDecisionInProcess.Name);

            novaProcess.Process.DeleteUserDecisionWithBranchesNotOfTheLowestOrder(userDecisionToBeDeleted, mergePointForFirstUserDecision);

            // Update and Verify the modified process
            Helper.UpdateAndVerifyNovaProcess(novaProcess.NovaProcess, _user);
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
            var novaProcess = Helper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Find precondition task
            var preconditionTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find the outgoing link for the precondition
            var preconditionOutgoingLink = novaProcess.Process.GetOutgoingLinkForShape(preconditionTask);

            // Merge point for the bothr user decisions is the process end point
            var endShape = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Add Decision point <UD2> with branch 
            var innerUserDecision = novaProcess.Process.AddUserDecisionPointWithBranchAfterShape(preconditionTask, preconditionOutgoingLink.Orderindex + 1, endShape.Id);

            // Find the updated outgoing link for the precondition
            preconditionOutgoingLink = novaProcess.Process.GetOutgoingLinkForShape(preconditionTask);

            // Add user and system task before new user decision point
            novaProcess.Process.AddUserAndSystemTask(preconditionOutgoingLink);

            // Find the updated outgoing link for the precondition
            preconditionOutgoingLink = novaProcess.Process.GetOutgoingLinkForShape(preconditionTask);

            // Add an outer user decision point <UD1> after the precondition and before the new user/system task
            novaProcess.Process.AddUserDecisionPointWithBranchAfterShape(preconditionTask, preconditionOutgoingLink.Orderindex + 1, endShape.Id);

            // Save the process
            novaProcess.Update(_user, novaProcess.Artifact);
            novaProcess.RefreshArtifactFromServer(_user);

            var userDecisionToBeDeleted = novaProcess.Process.GetProcessShapeByShapeName(innerUserDecision.Name);

            novaProcess.Process.DeleteUserDecisionWithBranchesNotOfTheLowestOrder(userDecisionToBeDeleted, endShape);

            // Verify the modified process
            Helper.UpdateAndVerifyNovaProcess(novaProcess.NovaProcess, _user);
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
            var novaProcess = Helper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Find precondition task
            var preconditionTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find the outgoing link for the precondition
            var preconditionOutgoingLink = novaProcess.Process.GetOutgoingLinkForShape(preconditionTask);

            // Merge point for the bothr user decisions is the process end point
            var endShape = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Add Decision point <UD2> with branch
            novaProcess.Process.AddUserDecisionPointWithBranchAfterShape(preconditionTask, preconditionOutgoingLink.Orderindex + 1, endShape.Id);

            // Find the updated outgoing link for the precondition
            preconditionOutgoingLink = novaProcess.Process.GetOutgoingLinkForShape(preconditionTask);

            // Add user and system task before new user decision point
            novaProcess.Process.AddUserAndSystemTask(preconditionOutgoingLink);

            // Find the updated outgoing link for the precondition
            preconditionOutgoingLink = novaProcess.Process.GetOutgoingLinkForShape(preconditionTask);

            // Add an outer user decision point <UD1> after the precondition and before the new user/system task
            var outerUserDecision = novaProcess.Process.AddUserDecisionPointWithBranchAfterShape(preconditionTask, preconditionOutgoingLink.Orderindex + 1, endShape.Id);

            // Save the process
            novaProcess.Update(_user, novaProcess.Artifact);
            novaProcess.RefreshArtifactFromServer(_user);

            var userDecisionToBeDeleted = novaProcess.Process.GetProcessShapeByShapeName(outerUserDecision.Name);

            novaProcess.Process.DeleteUserDecisionWithBranchesNotOfTheLowestOrder(userDecisionToBeDeleted, endShape);

            // Verify the modified process
            Helper.UpdateAndVerifyNovaProcess(novaProcess.NovaProcess, _user);
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
            var novaProcess = Helper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Find precondition task
            var preconditionTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var preconditionOutgoingLink = novaProcess.Process.GetOutgoingLinkForShape(preconditionTask);

            // Find the endpoint for the new branch
            var branchEndPoint = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Add decision point with branch to end
            var userDecision = novaProcess.Process.AddUserDecisionPointWithBranchAfterShape(preconditionTask, preconditionOutgoingLink.Orderindex + 1);

            // Add a second branch with task to decision point
            novaProcess.Process.AddBranchWithUserAndSystemTaskToUserDecisionPoint(userDecision, preconditionOutgoingLink.Orderindex + 2, branchEndPoint.Id);

            // Save the process
            novaProcess.Update(_user, novaProcess.Artifact);
            novaProcess.RefreshArtifactFromServer(_user);

            var userDecisionToBeDeleted = novaProcess.Process.GetProcessShapeByShapeName(userDecision.Name);

            // Merge point for the outer user decision is the process end point
            var endShape = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            novaProcess.Process.DeleteUserDecisionWithBranchesNotOfTheLowestOrder(userDecisionToBeDeleted, endShape);

            // Update and Verify the modified process
            Helper.UpdateAndVerifyNovaProcess(novaProcess.NovaProcess, _user);
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
                               +--[UT3]--+--[ST4]--+--<UD2>--+--[UT5]--+--[ST6]--+
                                                        |                        |
                                                        +--[UT7]--+--[ST8]-------+

            It becomes this:
                [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+--------------+--[E]
                               |                                       |
                               +--[UT3]--+--[ST4]--+--[UT5]--+--[ST6]--+
            */

            // Create and get the default process
            var novaProcess = Helper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Find precondition task
            var preconditionTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var preconditionOutgoingLink = novaProcess.Process.GetOutgoingLinkForShape(preconditionTask);

            // Determine the branch endpoint
            var branchEndPoint = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Add decision point <UD1> with branch after precondition
            var userDecision = novaProcess.Process.AddUserDecisionPointWithBranchAfterShape(
                preconditionTask, 
                preconditionOutgoingLink.Orderindex + 1, 
                branchEndPoint.Id);

            // Outgoing process link for decision point branch
            var outgoingProcessLinkForUserDecision = novaProcess.Process.GetOutgoingLinkForShape(
                userDecision,
                preconditionOutgoingLink.Orderindex + 1);

            // Determine new user task from decision point outgoing process link
            var newUserTask = novaProcess.Process.GetProcessShapeById(outgoingProcessLinkForUserDecision.DestinationId);

            var newSystemTask = novaProcess.Process.GetNextShape(newUserTask);

            // Add second user decision point <UD2> with branch to end after new system task
            var secondUserDecision = novaProcess.Process.AddUserDecisionPointWithBranchAfterShape(
                newSystemTask, 
                preconditionOutgoingLink.Orderindex + 1, 
                branchEndPoint.Id);

            // Save the process
            novaProcess.Update(_user, novaProcess.Artifact);
            novaProcess.RefreshArtifactFromServer(_user);

            var userDecisionToBeDeleted = novaProcess.Process.GetProcessShapeByShapeName(secondUserDecision.Name);

            // Merge point for the second user decision is the process end point
            var endShape = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            novaProcess.Process.DeleteUserDecisionWithBranchesNotOfTheLowestOrder(userDecisionToBeDeleted, endShape);

            // Update and Verify the modified process
            Helper.UpdateAndVerifyNovaProcess(novaProcess.NovaProcess, _user);
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
                               +--[UT3]--+--[ST4]--+--<UD2>--+--[UT5]--+--[ST6]--+
                                                        |                        |
                                                        +--[UT7]--+--[ST8]-------+

            It becomes this:
                [S]--[P]--+--[UT1]--+--[ST2]--+--[E]
            */

            // Create and get the default process
            var novaProcess = Helper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Find precondition task
            var preconditionTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var preconditionOutgoingLink = novaProcess.Process.GetOutgoingLinkForShape(preconditionTask);

            // Determine the branch endpoint
            var branchEndPoint = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Add decision point with branch after precondition
            var firstUserDecision = novaProcess.Process.AddUserDecisionPointWithBranchAfterShape(
                preconditionTask,
                preconditionOutgoingLink.Orderindex + 1,
                branchEndPoint.Id);

            // Outgoing process link for decision point branch
            var outgoingProcessLinkForUserDecision = novaProcess.Process.GetOutgoingLinkForShape(
                firstUserDecision,
                preconditionOutgoingLink.Orderindex + 1);

            // Determine new user task from decision point outgoing process link
            var newUserTask = novaProcess.Process.GetProcessShapeById(outgoingProcessLinkForUserDecision.DestinationId);

            var newSystemTask = novaProcess.Process.GetNextShape(newUserTask);

            // Add second user decision point with branch to end after new system task
            novaProcess.Process.AddUserDecisionPointWithBranchAfterShape(
                newSystemTask,
                preconditionOutgoingLink.Orderindex + 1,
                branchEndPoint.Id);

            // Save the process
            novaProcess.Update(_user, novaProcess.Artifact);
            novaProcess.RefreshArtifactFromServer(_user);

            var userDecisionToBeDeleted = novaProcess.Process.GetProcessShapeByShapeName(firstUserDecision.Name);

            // Merge point for the user decision is the process end point
            var endShape = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            novaProcess.Process.DeleteUserDecisionWithBranchesNotOfTheLowestOrder(userDecisionToBeDeleted, endShape);

            // Update and Verify the modified process
            Helper.UpdateAndVerifyNovaProcess(novaProcess.NovaProcess, _user);
        }
    }
}
