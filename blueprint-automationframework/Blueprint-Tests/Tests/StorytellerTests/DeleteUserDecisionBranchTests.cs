﻿using CustomAttributes;
using Model;
using Model.Factories;
using NUnit.Framework;
using Helper;
using Model.StorytellerModel.Impl;
using TestCommon;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class DeleteUserDecisionBranchTests : TestBase
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
        [Description("Delete a branch from a user decision point that has more than 2 conditions and verify that the " +
                     "returned process still contains the user decision with all branches except the deleted branch.")]
        public void DeleteBranchFromUserDecisionWithMoreThanTwoConditions_VerifyReturnedProcess(double orderIndexOfBranch)
        {
            /*
            Before:
                [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+--[E]
                               |                        |
                               +--[UT3]--+--[ST4]-------+
                               |                        |
                               +--[UT5]--+--[ST6]-------+

            After:
                [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+--[E]
                                |                       |
                                +--[UT3]--+--[ST4]------+
            */

            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _project, _user);

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

            // Update and Verify the process after updating the default process for the test
            var returnedProcess = StorytellerTestHelper.UpdateAndVerifyProcess(process, Helper.Storyteller, _user);

            var userDecisionWithBranchToBeDeleted= returnedProcess.GetProcessShapeByShapeName(userDecision.Name);

            // Delete the specified user decision branch
            returnedProcess.DeleteUserDecisionBranch(userDecisionWithBranchToBeDeleted, orderIndexOfBranch, branchEndPoint);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, Helper.Storyteller, _user);
        }

        [TestCase]
        [Description("Delete a branch from a user decision point that has a nested user decision on one of its branches." +
                     "Verify that the returned process has the branch deleted along with the nested user decision and " +
                     "associated branches.")]
        public void DeleteBranchFromUserDecisionThatContainsNestedUserDecision_VerifyReturnedProcess()
        {
            /*
            Before:
                [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+------------------------+--[E]
                               |                                                 |
                               +--[UT3]--+--[ST4]--------------------------------+ 
                               |                                                 |
                               +--[UT5]--+--[ST6]--+--<UD2>--+--[UT7]--+--[ST8]--+
                                                        |                        |
                                                        +--[UT9]--+--[ST10]------+

            After:
                [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+------------------------+--[E]
                               |                                                 |
                               +--[UT3]--+--[ST4]--------------------------------+ 
            */

            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcessWithOneUserDecisionContainingMultipleConditions(Helper.Storyteller, _project, _user, additionalBranches: 1, updateProcess: false);

            // Find precondition
            var precondition = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var preconditionOutgoingLink = process.GetOutgoingLinkForShape(precondition);

            // Determine the branch endpoint
            var branchEndPoint = process.GetProcessShapeByShapeName(Process.EndName);

            // Find the first user decision
            var firstUserDecision = process.GetNextShape(precondition);

            // Find the user task from decision point outgoing process link
            var userTaskOnThirdBranch = process.GetNextShape(firstUserDecision, preconditionOutgoingLink.Orderindex + 2);

            var systemTaskOnThirdBranch = process.GetNextShape(userTaskOnThirdBranch);

            // Add second user decision point <UD2> with branch to end after new system task
            process.AddUserDecisionPointWithBranchAfterShape(
                systemTaskOnThirdBranch,
                preconditionOutgoingLink.Orderindex + 1,
                branchEndPoint.Id);

            // Update and Verify the process after updating the default process for the test
            var returnedProcess = StorytellerTestHelper.UpdateAndVerifyProcess(process, Helper.Storyteller, _user);

            // Find the first user decision in the returned process
            var returnedFirstUserDecision = returnedProcess.GetProcessShapeByShapeName(firstUserDecision.Name);

            // Delete the third branch for the first user decision
            returnedProcess.DeleteUserDecisionBranch(returnedFirstUserDecision, preconditionOutgoingLink.Orderindex + 2, branchEndPoint);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, Helper.Storyteller, _user);
        }

        [TestCase]
        [Description("Delete a branch from a user decision point that has a nested system decision on one of its branches." +
             "Verify that the returned process has the branch deleted along with the nested system decision and " +
             "associated branches.")]
        public void DeleteBranchFromUserDecisionThatContainsNestedSystemDecision_VerifyReturnedProcess()
        {
            /*
            Before:
                [S]--[P]--+--<UD>--+--[UT1]--+--[ST2]--+-----+--[E]
                               |                             |
                               +--[UT3]--+--[ST4]------------+ 
                               |                             |
                               +--[UT5]--+--<SD>--+--[ST6]---+
                                              |              |
                                              +----+--[ST7]--+

            After:
                [S]--[P]--+--<UD>--+--[UT1]--+--[ST2]--+-----+--[E]
                               |                             |
                               +--[UT3]--+--[ST4]------------+ 
            */

            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcessWithOneUserDecisionContainingMultipleConditions(Helper.Storyteller, _project, _user, additionalBranches: 1, updateProcess: false);

            // Find precondition
            var precondition = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var preconditionOutgoingLink = process.GetOutgoingLinkForShape(precondition);

            // Determine the branch endpoint
            var branchEndPoint = process.GetProcessShapeByShapeName(Process.EndName);

            // Find the first user decision
            var firstUserDecision = process.GetNextShape(precondition);

            // Find the user task from decision point outgoing process link
            var userTaskOnThirdBranch = process.GetNextShape(firstUserDecision, preconditionOutgoingLink.Orderindex + 2);

            var systemTaskOnThirdBranch = process.GetNextShape(userTaskOnThirdBranch);

            // Add system decision point <SD> with branch after user task on third branch
            process.AddSystemDecisionPointWithBranchBeforeSystemTask(
                systemTaskOnThirdBranch,
                preconditionOutgoingLink.Orderindex + 1,
                branchEndPoint.Id);

            // Update and Verify the process after updating the default process for the test
            var returnedProcess = StorytellerTestHelper.UpdateAndVerifyProcess(process, Helper.Storyteller, _user);

            // Find the first user decision in the returned process
            var returnedFirstUserDecision = returnedProcess.GetProcessShapeByShapeName(firstUserDecision.Name);

            // Delete the third branch for the first user decision
            returnedProcess.DeleteUserDecisionBranch(returnedFirstUserDecision, preconditionOutgoingLink.Orderindex + 2, branchEndPoint);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, Helper.Storyteller, _user);
        }
    }
}
