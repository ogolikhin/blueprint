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
    public class ChangeBranchMergePointTests : TestBase
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

        #region Tests

        [TestRail(96091)]
        [TestCase]
        [Description("Change the merge point of the second branch of system decision. Verify that returned process after the save " +
                     "contains valid merge point information.")]
        public void ChangeSystemDecisionMergePointForSecondBranch_VerifyReturnProcess()
        {
            /*
            Before:
            [S]--[P]--+--[UT1]--<SD1>--+--[ST1]--+--+--[UT2]--+--[ST2]--+--[E]
                                  |              |
                                  +----+--[ST3]--+

            After:
            [S]--[P]--+--[UT1]--<SD1>--+--[ST1]--+--+--[UT2]--+--[ST3]--+--[E]
                                  |                                     |
                                  +----+--[ST3]--+----------------------+
            */
            // Create and get the process with two sequential user tasks and one system decision
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcessWithTwoSequentialUserTasksAndOneSystemDecision(_project, _user);

            // Locate the start shape to locate the outgoing link to Precondition
            var startShape = novaProcess.Process.GetProcessShapeByShapeName(Process.StartName);

            // Find the outgoing link for start shape
            var outgoingLinkForStartShape = novaProcess.Process.GetOutgoingLinkForShape(startShape);

            // Find the system decision
            var systemDecision = novaProcess.Process.GetProcessShapesByShapeType(ProcessShapeType.SystemDecision).First();

            // Find the system task next from  the system decision to second branch
            var systemTaskFromSecondBranch = novaProcess.Process.GetNextShape(systemDecision, outgoingLinkForStartShape.Orderindex + 1);

            // Find the outgoing link for the system task from the second branch
            var outgoingLinkForSystemTaskFromSecondBranch =
                novaProcess.Process.GetOutgoingLinkForShape(systemTaskFromSecondBranch);

            // Locate the end shape for changing the merge point of system decision
            var endShape = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Change the second branch merge point of the system decision to the endShape
            novaProcess.Process.ChangeBranchMergePoint(systemDecision, outgoingLinkForStartShape.Orderindex + 1, outgoingLinkForSystemTaskFromSecondBranch, endShape);

            // Update and Verify the modified process
            var updatedProcess = StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess.NovaProcess, _user);

            // Verify that DecisionBranchDestinationLinks contained updated information for the updated merge point

            var secondDecisionBranchDestinationLink =
                updatedProcess.Process.GetDecisionBranchDestinationLinkForDecisionShape(systemDecision,
                    outgoingLinkForStartShape.Orderindex + 1);

            Assert.That(secondDecisionBranchDestinationLink.DestinationId == endShape.Id,
                "The destination Id from the DecisionBranchDestinationLink for the second branch after updating merge point should be {0} but {1} was returned.",
                endShape.Id, secondDecisionBranchDestinationLink.DestinationId);
        }

        [TestRail(96092)]
        [TestCase]
        [Description("Change the merge point of the second branch of user decision. Verify that returned process after the save " +
                     "contains valid merge point information.")]
        public void ChangeUserDecisionMergePointForSecondBranch_VerifyReturnProcess()
        {
            /*
            Before:
            [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+--+--[UT3]--+--[ST4]--+--[E]
                           |                        |
                           +-------[UT5]--+--[ST6]--+

            After:
            [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+--+--[UT3]--+--[ST4]--+--[E]
                           |                                               |
                           +-------[UT5]--+--[ST6]--+----------------------+
            */
            // Create and get the process with two sequential user tasks and one user decision
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcessWithTwoSequentialUserTasksAndOneUserDecision(_project, _user);

            // Find the user decision
            var userDecision = novaProcess.Process.GetProcessShapesByShapeType(ProcessShapeType.UserDecision).First();

            // Locate the start shape to locate the outgoing link to Precondition
            var startShape = novaProcess.Process.GetProcessShapeByShapeName(Process.StartName);

            // Find the outgoing link for start shape
            var outgoingLinkForStartShape = novaProcess.Process.GetOutgoingLinkForShape(startShape);

            // Find the user task from the second branch
            var userTaskFromSecondBranch = novaProcess.Process.GetNextShape(userDecision,
                outgoingLinkForStartShape.Orderindex + 1);

            // Find the system task from the second branch
            var systemTaskFromSecondBranch = novaProcess.Process.GetNextShape(userTaskFromSecondBranch);

            // Find the outgoing link for the system task from the second branch
            var outgoingLinkForSystemTaskFromSecondBranch =
                novaProcess.Process.GetOutgoingLinkForShape(systemTaskFromSecondBranch);

            // Locate the end shape for changing the merge point of user decision
            var endShape = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Change the second branch merge point of the user decision to the endShape
            novaProcess.Process.ChangeBranchMergePoint(userDecision, outgoingLinkForStartShape.Orderindex + 1, outgoingLinkForSystemTaskFromSecondBranch, endShape);

            // Update and Verify the modified process
            var updatedProcess = StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess.NovaProcess, _user);

            // Verify that DecisionBranchDestinationLinks contained updated information for the updated merge point
            var secondDecisionBranchDestinationLink =
                updatedProcess.Process.GetDecisionBranchDestinationLinkForDecisionShape(userDecision,
                    outgoingLinkForStartShape.Orderindex + 1);

            Assert.That(secondDecisionBranchDestinationLink.DestinationId == endShape.Id,
                "The destination Id from the DecisionBranchDestinationLink for the second branch after updating merge point should be {0} but {1} was returned.",
                endShape.Id, secondDecisionBranchDestinationLink.DestinationId);
        }

        [TestRail(96093)]
        [TestCase]
        [Description("Save the process with a system decision which contains two branches beside the main branch. " +
                     "Each branch merges to different locations. Verify that returned process after the save " +
                     "contains valid merge point information.")]
        public void ChangeMergePointForSystemDecisionWithMultipleBranches_VerifyDecisionBranchDestinationLinksFromReturnProcess()
        {
            /*
            Save the following change:
            
            Before the merge point change:
            [S]--[P]--+--[UT1]--<SD1>--+--[ST1]--+--+--[UT2]--+--[ST2]--+--[E]
                                  |              |
                                  +----+--[ST3]--+
                                  |              |
                                  +----+--[ST4]--+    <--- additionalBranches: 1
            
            After the merge point change:
            [S]--[P]--+--[UT1]--<SD1>--+--[ST1]--+--+--[UT2]--+--[ST2]--+--[E]
                                  |              |                      |
                                  +----+--[ST3]--+                      |
                                  |                                     |
                                  +----+--[ST4]--+----------------------+    <--- additionalBranches: 1
            Verify that returned process model contains correct values on DecisionBranchDestinationLinks, section of the process model contains information for merge points
            */
            // Create and get the process with two sequential user tasks and one system decision contains three branches
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcessWithTwoSequentialUserTasksAndOneSystemDecisionContainingMultipleConditions
                    (_project, _user, additionalBranches: 1);
            
            // Locate the start shape to locate the outgoing link to Precondition
            var startShape = novaProcess.Process.GetProcessShapeByShapeName(Process.StartName);

            // Find the outgoing link for start shape
            var outgoingLinkForStartShape = novaProcess.Process.GetOutgoingLinkForShape(startShape);

            // Locate the end shape for changing the merge point of user decision
            var endShape = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Find the system decision
            var systemDecision = novaProcess.Process.GetProcessShapesByShapeType(ProcessShapeType.SystemDecision).First();

            // Find the system task From the third branch
            var systemTaskFromThirdBranch =
                novaProcess.Process.GetNextShape(systemDecision, outgoingLinkForStartShape.Orderindex + 2);

            // Find the outgoing link for the system task from the third branch
            var outgoingLinkForSystemTaskFromThirdBranch =
                novaProcess.Process.GetOutgoingLinkForShape(systemTaskFromThirdBranch);

            // Change the third branch merge point of the system decision to the endShape
            novaProcess.Process.ChangeBranchMergePoint(systemDecision, outgoingLinkForStartShape.Orderindex + 2, outgoingLinkForSystemTaskFromThirdBranch, endShape);

            // Update the process
            var updatedProcess = StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess.NovaProcess, _user);

            // Verify that DecisionBranchDestinationLinks contained updated information for the updated merge point

            var thirdDecisionBranchDestinationLink =
                updatedProcess.Process.GetDecisionBranchDestinationLinkForDecisionShape(systemDecision,
                    outgoingLinkForStartShape.Orderindex + 2);

            Assert.That(thirdDecisionBranchDestinationLink.DestinationId == endShape.Id,
                "The destination Id from the DecisionBranchDestinationLink for the third branch after updating merge point should be {0} but {1} was returned.",
                endShape.Id, thirdDecisionBranchDestinationLink.DestinationId);
        }

        [TestRail(96094)]
        [TestCase]
        [Description("Save the process with a user decision which contains two branches beside the main branch. " +
             "Each branch merges to different locations. Verify that returned process after the save " +
             "contains valid merge point information.")]
        public void ChangeMergePointForUserDecisionWithMultipleBranches_VerifyDecisionBranchDestinationLinksFromReturnProcess()
        {
            /*
            Save the following process:
            
            Before the merge point change:
            [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+--+--[UT3]--+--[ST4]--+--[E]
                           |                        |
                           +-------[UT5]--+--[ST6]--+
                           |                        |
                           +-------[UT7]--+--[ST8]--+    <--- additionalBranches: 1
            
            After the merge point change:
            [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+--+--[UT3]--+--[ST4]--+--[E]
                           |                        |                      |
                           +-------[UT5]--+--[ST6]--+                      |
                           |                                               |
                           +-------[UT7]--+--[ST8]--+--+-------------------+    <--- additionalBranches: 1
            Verify that returned process model contains correct values on DecisionBranchDestinationLinks, section of the process model contains information for merge points
            */
            // Create and get the process with two sequential user tasks and one user decision contains three branches
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcessWithTwoSequentialUserTasksAndOneUserDecisionContainingMultipleConditions
                    (_project, _user, additionalBranches: 1);

            // Locate the start shape to locate the outgoing link to Precondition
            var startShape = novaProcess.Process.GetProcessShapeByShapeName(Process.StartName);

            // Find the outgoing link for start shape
            var outgoingLinkForStartShape = novaProcess.Process.GetOutgoingLinkForShape(startShape);


            // Locate the end shape for changing the merge point of user decision
            var endShape = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Find the user decision
            var userDecision = novaProcess.Process.GetProcessShapesByShapeType(ProcessShapeType.UserDecision).First();

            // Find the user task From the third branch
            var userTaskFromThirdBranch = novaProcess.Process.GetNextShape(userDecision,
                outgoingLinkForStartShape.Orderindex + 2);

            // Find the system task From the third branch
            var systemTaskFromThirdBranch = novaProcess.Process.GetNextShape(userTaskFromThirdBranch);

            // Find the outgoing link for the system task from the third branch
            var outgoingLinkForSystemTaskFromThirdBranch = novaProcess.Process.Links.Find(l => l.SourceId.Equals(systemTaskFromThirdBranch.Id));

            // Change the third branch merge point of the user decision to the endShape
            novaProcess.Process.ChangeBranchMergePoint(userDecision, outgoingLinkForStartShape.Orderindex + 2, outgoingLinkForSystemTaskFromThirdBranch, endShape);

            // Update the process
            var updatedProcess = StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess.NovaProcess, _user);

            // Verify that DecisionBranchDestinationLinks contained updated information for the updated merge point
            var thirdDecisionBranchDestinationLink =
                updatedProcess.Process.GetDecisionBranchDestinationLinkForDecisionShape(userDecision,
                    outgoingLinkForStartShape.Orderindex + 2);

            Assert.That(thirdDecisionBranchDestinationLink.DestinationId == endShape.Id,
                "The destination Id from the DecisionBranchDestinationLink for the third branch after updating merge point should be {0} but {1} was returned.",
                endShape.Id, thirdDecisionBranchDestinationLink.DestinationId);
        }
        #endregion Tests

    }
}
