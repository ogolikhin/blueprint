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
    public class DeleteSystemDecisionTests : TestBase
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
        [Description("Delete the system decision with a branch and verify that the returned process " +
                     "has the user contain only the main branch without the system decison and the second branch.")]
        public void DeleteSystemDecisionWithBranchBeforeDefaultSystemTask_VerifyReturnedProcess()
        {
            /*
            If you start with this:
            [S]--[P]--+--[UT1]--<SD1>--+--[ST1]--+--[E]
                                  |              |
                                  +----+--[ST2]--+

            It becomes this:
            [S]--[P]--+--[UT1]--+--[ST1]--[E]
            */
            // Create and get the default process with one system decision
            var novaProcess = Helper.CreateAndGetDefaultNovaProcessWithOneSystemDecision(_project, _user);

            // Find the system decision to delete from the updated process
            var systemDecisionToDelete = novaProcess.Process.GetProcessShapesByShapeType(ProcessShapeType.SystemDecision).First();

            // Find the branch end shape for system decision
            var endShape = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Delete the specified system decision
            novaProcess.Process.DeleteSystemDecisionWithBranchesNotOfTheLowestOrder(systemDecisionToDelete, endShape);

            // Update and Verify the modified process
            Helper.UpdateAndVerifyNovaProcess(novaProcess.NovaProcess, _user);
        }

        [TestCase]
        [Description("Delete a system decision with branch before a second system decision" +
                     "with branch and verify that the returned process has the first decision" +
                     "with all branches removed except the the main branch. The second system" +
                     "decision and its branches are not altered")]
        public void DeleteFirstSystemDecisionPointWithMergePointBeforeSecondSystemDecision_VerifyReturnedProcess()
        {
            /*
            If you start with this:
            [S]--[P]--+--[UT1]--<SD1>--+--[ST1]--+--[UT2]--<SD2>--+--[ST3]--+--[E]
                                  |              |           |              |  
                                  +----+--[ST2]--+           +----+--[ST4]--+
            
            It becomes this:
            [S]--[P]--+--[UT1]--+--[ST1]--+--[UT2]--<SD2>--+--[ST3]--+--[E]
                                                      |              |
                                                      +----+--[ST4]--+  
            */
            var novaProcess = Helper.CreateAndGetDefaultNovaProcessWithTwoSequentialSystemDecisions(_project, _user);

            // Find the first system decision to delete from the updated process
            var firstSystemDecisionToDelete = novaProcess.Process.GetProcessShapesByShapeType(ProcessShapeType.SystemDecision).First();

            // Merge point for first system decision is the second system decision on the process
            var mergePointForFirstSystemDecision = novaProcess.Process.GetProcessShapesByShapeType(ProcessShapeType.UserTask).Find(
                    ut => !ut.Name.Equals(Process.DefaultUserTaskName));

            // Delete the first system decision that merges before the added user task
            novaProcess.Process.DeleteSystemDecisionWithBranchesNotOfTheLowestOrder(firstSystemDecisionToDelete, mergePointForFirstSystemDecision);

            // Update and Verify the modified process
            Helper.UpdateAndVerifyNovaProcess(novaProcess.NovaProcess, _user);
        }

        [TestCase]
        [Description("Delete a system decision with branch that is on the main branch but within an" +
                     "outer branch of another sytem decision. Verfiy that the returned process has" +
                     "the inner system decision removed with all branches except the main branch." +
                     "The outer system decision and branch must remain present.")]
        public void DeleteInnerSystemDecisionContainedWithinMainBranchOfOuterSystemDecision_VerifyReturnedProcess()
        {
            /*
            If you start with this:
            [S]--[P]--+--[UT1]--<SD2>--+--<SD1>--+--[ST1]--+--[E]
                                  |         |              |
                                  |         +----+--[ST2]--+
                                  |                        |
                                  +----+--[ST3]--+---------+

            It becomes this:
            [S]--[P]--+--[UT1]--<SD2>--+--[ST1]--+--[E]
                                  |              |
                                  +----+--[ST3]--+            
            */
            // Create and get the default process with inner and outer system decisions
            var novaProcess = Helper.CreateAndGetDefaultNovaProcessWithInnerAndOuterSystemDecisions(
                _project, _user);

            // Find the default SystemTask
            var defaultSystemTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultSystemTaskName);

            // Find link between the inner system decision and the default system task
            var targetLink = novaProcess.Process.GetIncomingLinkForShape(defaultSystemTask);

            // Find the inner system decision before the defaut system task
            var innerSystemDecisionToDelete = novaProcess.Process.GetProcessShapeById(targetLink.SourceId);

            // Find the branch end point for system decision points
            var endShape = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Delete the inner system decision that merges before the end point
            novaProcess.Process.DeleteSystemDecisionWithBranchesNotOfTheLowestOrder(innerSystemDecisionToDelete, endShape);

            // Update and Verify the modified process
            Helper.UpdateAndVerifyNovaProcess(novaProcess.NovaProcess, _user);
        }

        [TestCase]
        [Description("Delete a sytem decision with branch that is on the main branch but contains an" +
                     "inner branch with another system decision.  Verfiy that the returned process has" +
                     "the outer system decision removed with all branches except the main branch." +
                     "The inner system decision and branch must remain present.")]
        public void DeleteOuterSystenDecisionContainingInnerBranchWithSecondSystemDecision_VerifyReturnedProcess()
        {
            /*
            If you start with this:
            [S]--[P]--+--[UT1]--<SD2>--+--<SD1>--+--[ST1]--+--[E]
                                  |         |              |
                                  |         +----+--[ST2]--+
                                  |                        |
                                  +----+--[ST3]--+---------+

            It becomes this:
            [S]--[P]--+--[UT1]--<SD1>--+--[ST1]--+--[E]
                                  |              |
                                  +----+--[ST2]--+            
            */
            // Create and get the default process with inner and outer system decisions
            var novaProcess = Helper.CreateAndGetDefaultNovaProcessWithInnerAndOuterSystemDecisions(
                _project, _user);

            // Find the default UserTask
            var defaultUserTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the inner system decision before the defaut system task
            var outerSystemDecisionToDelete = novaProcess.Process.GetNextShape(defaultUserTask);

            // Find the branch end point for system decision points
            var endShape = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Delete the outer system decision that merges before the end point
            novaProcess.Process.DeleteSystemDecisionWithBranchesNotOfTheLowestOrder(outerSystemDecisionToDelete, endShape);

            // Update and Verify the modified process
            Helper.UpdateAndVerifyNovaProcess(novaProcess.NovaProcess, _user);
        }

        [TestCase]
        [Description("Delete a system decision with more than 2 conditions and verify that " +
                     "the returned process has the system decision removed with all branches" +
                     "except the main branch.")]
        public void DeleteSystemDecisionWithMoreThanTwoConditions_VerifyReturnedProcess()
        {
            /*
            If you start with this:
            [S]--[P]--+--[UT1]--<SD1>--+--[ST1]--+--[E]
                                  |              |
                                  +----+--[ST2]--+
                                  |              |
                                  +----+--[ST3]--+    <--- additionalBranches: 1

            It becomes this:
            [S]--[P]--+--[UT1]--+--[ST1]--[E]
            */
            // Create and get the default process
            var returnedProcess =
                Helper.CreateAndGetDefaultNovaProcessWithOneSystemDecisionContainingMultipleConditions(
                    _project, _user, additionalBranches: 1);

            // Find the system decision to delete from the updated process
            var systemDecisionToDelete = returnedProcess.Process.GetProcessShapesByShapeType(ProcessShapeType.SystemDecision).First();

            // Find the branch end point for system decision points
            var endShape = returnedProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Delete the specified system decision
            returnedProcess.Process.DeleteSystemDecisionWithBranchesNotOfTheLowestOrder(systemDecisionToDelete, endShape);

            // Update and Verify the modified process
            Helper.UpdateAndVerifyNovaProcess(returnedProcess.NovaProcess, _user);
        }

        [TestCase]
        [Description("Delete a system decision that exists on the second branch of the main" +
                     "system decision and verify that the returned process has the second branch" +
                     "with system decision removed with all associated branches except the second" +
                     "branch itself")]
        public void DeleteSystemDecisionOnSecondBranchOfMainSystemDecision_VerifyReturnedProcess()
        {
            /*
            If you start with this:
            [S]--[P]--+--[UT1]--<SD1>--+--[ST1]---------+--[E]
                                  |                     |
                                  +----<SD2>--+--[ST2]--+
                                         |              |
                                         +----+--[ST3]--+

            It becomes this:
            [S]--[P]--+--[UT1]--<SD1>--+--[ST1]--+--[E]
                                  |              |
                                  +----+--[ST2]--+
            */
            // Create and get the default process with System Decision which contains another System Decision on the second branch
            var novaProcess =
                Helper.CreateAndGetDefaultNovaProcessWithSystemDecisionContainingSystemDecisionOnBranch(
                    _project, _user);

            // Find the default UserTask
            var defaultUserTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the outgoing process link from the default user task
            var defaultUserTaskOutgoingProcessLink = novaProcess.Process.GetOutgoingLinkForShape(defaultUserTask);

            // Find the link between the system decision point and the System task on the second branch
            var branchingProcessLink = novaProcess.Process.Links.Find(l => l.Orderindex.Equals(defaultUserTaskOutgoingProcessLink.Orderindex + 1));

            // Find the system decision on the second branch for deletion
            var nestedSystemDecisionToDelete = novaProcess.Process.GetProcessShapeById(branchingProcessLink.DestinationId);

            // Find the branch end point for system decision points
            var endShape = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Delete the nested system decision that merges before the end point
            novaProcess.Process.DeleteSystemDecisionWithBranchesNotOfTheLowestOrder(nestedSystemDecisionToDelete, endShape);

            // Update and Verify the modified process
            Helper.UpdateAndVerifyNovaProcess(novaProcess.NovaProcess, _user);
        }

        [TestCase]
        [Description("Delete a system decision that has an second branch that contains a second" +
                     "system decision with a branch. Verify that the returned process has the" +
                     "first system decision point and all branches deleted, including the second system " +
                     "decision and its associated branches. The main branch must remained.")]
        public void DeleteSystemDecisionContainingSecondSystemDecisionOnSecondBranch_VerifyReturnedProcess()
        {
            /*
            If you start with this:
            [S]--[P]--+--[UT1]--<SD1>--+--[ST1]---------+--[E]
                                  |                     |
                                  +----<SD2>--+--[ST2]--+
                                         |              |
                                         +----+--[ST3]--+

            It becomes this:
            [S]--[P]--+--[UT1]--+--[ST1]--[E]
            */
            // Create and get the default process with System Decision which contains another System Decision on the second branch
            var novaProcess =
                Helper.CreateAndGetDefaultNovaProcessWithSystemDecisionContainingSystemDecisionOnBranch(
                    _project, _user);

            // Find the default UserTask
            var defaultUserTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the system decision to delete from the updated process
            var rootSystemDecisionToDelete = novaProcess.Process.GetNextShape(defaultUserTask);

            // Find the branch end point for system decision points
            var endShape = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Delete the root system decision that merges before the end point
            novaProcess.Process.DeleteSystemDecisionWithBranchesNotOfTheLowestOrder(rootSystemDecisionToDelete, endShape);

            // Update and Verify the modified process
            Helper.UpdateAndVerifyNovaProcess(novaProcess.NovaProcess, _user);
        }
    }
}