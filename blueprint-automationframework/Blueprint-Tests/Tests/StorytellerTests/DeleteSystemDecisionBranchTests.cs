using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using System.Linq;
using Model.StorytellerModel.Enums;
using TestCommon;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class DeleteSystemDecisionBranchTests : TestBase
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

        [TestCase(1)]
        [TestCase(2)]
        [Description("Delete a branch from a system decision point that has more than 2 conditions and verify that the " +
                     "returned process still contains the system decision with all branches except the deleted branch.")]
        public void DeleteBranchFromSystemDecisionWithMoreThanTwoConditions_VerifyReturnedProcess(double orderIndexOfBranch)
        {
            /*
            Before:
            [S]--[P]--+--[UT1]--+--<SD1>--+--[ST1]--+--[E]
                                     |              |
                                     +----+--[ST2]--+
                                     |              |
                                     +----+--[ST3]--+

            After:
            [S]--[P]--+--[UT1]--+--<SD1>--+--[ST1]--+--[E]
                                     |              |
                                     +----+--[ST2]--+
            */

            // Create and Save the process with one system decision with two branches plus main branch
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcessWithOneSystemDecisionContainingMultipleConditions(
                    Helper.Storyteller, _project, _user, additionalBranches: 1);

            // Find the endShape for the system decision
            var endShape = returnedProcess.GetProcessShapeByShapeName(Process.EndName);

            // Find the System Decision with a branch merging to endShape
            var systemDecisionForBranchDeletion = returnedProcess.GetProcessShapesByShapeType(ProcessShapeType.SystemDecision).First();

            // Delete the specified system decision branch
            returnedProcess.DeleteSystemDecisionBranch(systemDecisionForBranchDeletion, orderIndexOfBranch, endShape);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, Helper.Storyteller, _user);
        }

        [TestCase]
        [Description("Delete a branch from a system decision point that has a nested system decision on one of the" +
                     " branches and verify that the returned process still contains the system decision with all" +
                     " branches except the deleted branch.")]
        public void DeleteBranchFromSystemDecisionThatContainsNestedSystemDecision_VerifyReturnedProcess()
        {
            /*
            Before:
            [S]--[P]--+--[UT1]--+--<SD1>--+-------[ST1]--------+--[E]
                                     |                         |
                                     +----+-------[ST2]--------+
                                     |                         |
                                     +--<SD2>--+--[ST3]--------+
                                          |                    |
                                          +----+--[ST4]--------+
            After:
            [S]--[P]--+--[UT1]--+--<SD1>--+--[ST1]--+--[E]
                                     |              |
                                     +----+--[ST2]--+
            */

            // Create the process with one system decision with three branches plus main branch
            var process = StorytellerTestHelper.CreateAndGetDefaultProcessWithOneSystemDecisionContainingMultipleConditions(
                    Helper.Storyteller, _project, _user, additionalBranches: 1, updateProcess: false);

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

            // Update and Verify the process after updating the default process for the test
            var returnedProcess = StorytellerTestHelper.UpdateAndVerifyProcess(process, Helper.Storyteller, _user);

            var systemDecisionForBranchDeletion = returnedProcess.GetNextShape(returnedProcess.GetNextShape(precondition));

            // Delete the specified system decision branch
            returnedProcess.DeleteSystemDecisionBranch(systemDecisionForBranchDeletion, outgoingLinkForPrecondition.Orderindex + 2, endShape);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, Helper.Storyteller, _user);
        }

        [TestCase]
        [Description("Delete a branch from a system decision point that has a nested user decision on one of the" +
                     " branches and verify that the returned process still contains the system decision with all" +
                     " branches except the deleted branch.")]
        public void DeleteBranchFromSystemDecisionThatContainsNestedUserDecision_VerifyReturnedProcess()
        {
            /*
            Before:
            [S]--[P]--+--[UT1]--+--<SD1>--+--[ST1]---+-----------------------------+--[E]
                                     |                                             |
                                     +----+--[ST2]---+-----------------------------|
                                     |                                             |
                                     +----+--[ST3]---+--<UD1>--+--[UT2]--+--[ST4]--+
                                                          |                        |
                                                          +----+--[UT3]--+--[ST5]--+
            After:
            [S]--[P]--+--[UT1]--+--<SD1>--+--[ST1]--+--[E]
                                     |              |
                                     +----+--[ST2]--+
            */

            // Create the process with one system decision with three branches plus main branch
            var process = StorytellerTestHelper.CreateAndGetDefaultProcessWithOneSystemDecisionContainingMultipleConditions(
                    Helper.Storyteller, _project, _user, additionalBranches: 1, updateProcess: false);

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

            // Update and Verify the process after updating the default process for the test
            var returnedProcess = StorytellerTestHelper.UpdateAndVerifyProcess(process, Helper.Storyteller, _user);

            var systemDecisionForBranchDeletion = returnedProcess.GetNextShape(returnedProcess.GetNextShape(precondition));

            // Delete the specified system decision branch
            returnedProcess.DeleteSystemDecisionBranch(systemDecisionForBranchDeletion, outgoingLinkForPrecondition.Orderindex + 2, endShape);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, Helper.Storyteller, _user);
        }
    }
}
