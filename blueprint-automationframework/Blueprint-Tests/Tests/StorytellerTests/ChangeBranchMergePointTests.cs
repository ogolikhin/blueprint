using System.Collections.Generic;
using System.Linq;
using CustomAttributes;
using Model;
using Model.Factories;
using Model.ArtifactModel;
using Model.StorytellerModel;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using Helper;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class ChangeBranchMergePointTests
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
            // TODO create a factory class that combines AdminStoreFactory, BlueprintServerFactory, StorytellerFactory, UserFactory, ProjectFactory
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
                var savedArtifactsList = new List<IArtifactBase>();
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
        [TestRail(96091)]
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
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcessWithTwoSequentialUserTasksAndOneSystemDecision(_storyteller, _project, _user);

            // Locate the start shape to locate the outgoing link to Precondition
            var startShape = returnedProcess.GetProcessShapeByShapeName(Process.StartName);

            // Find the outgoing link for start shape
            var outgoingLinkForStartShape = returnedProcess.GetOutgoingLinkForShape(startShape);

            // Find the system decision
            var systemDecision = returnedProcess.GetProcessShapesByShapeType(ProcessShapeType.SystemDecision).First();

            // Find the system task next from  the system decision to second branch
            var systemTaskFromSecondBranch = returnedProcess.GetNextShape(systemDecision, outgoingLinkForStartShape.Orderindex + 1);

            // Find the outgoing link for the system task from the second branch
            var outgoingLinkForSystemTaskFromSecondBranch =
                returnedProcess.GetOutgoingLinkForShape(systemTaskFromSecondBranch);

            // Locate the end shape for changing the merge point of system decision
            var endShape = returnedProcess.GetProcessShapeByShapeName(Process.EndName);

            // Change the second branch merge point of the system decision to the endShape
            returnedProcess.ChangeBranchMergePoint(systemDecision, outgoingLinkForStartShape.Orderindex + 1, outgoingLinkForSystemTaskFromSecondBranch, endShape);

            // Update and Verify the modified process
            var updatedProcess = StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);

            // Verify that DecisionBranchDestinationLinks contained updated information for the updated merge point

            var secondDecisionBranchDestinationLink =
                updatedProcess.GetDecisionBranchDestinationLinkForDecisionShape(systemDecision,
                    outgoingLinkForStartShape.Orderindex + 1);

            Assert.That(secondDecisionBranchDestinationLink.DestinationId == endShape.Id,
                "The destination Id from the DecisionBranchDestinationLink for the second branch after updating merge point should be {0} but {1} was returned.",
                endShape.Id, secondDecisionBranchDestinationLink.DestinationId);
        }

        [TestCase]
        [TestRail(96092)]
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
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcessWithTwoSequentialUserTasksAndOneUserDecision(_storyteller, _project, _user);

            // Find the user decision
            var userDecision = returnedProcess.GetProcessShapesByShapeType(ProcessShapeType.UserDecision).First();

            // Locate the start shape to locate the outgoing link to Precondition
            var startShape = returnedProcess.GetProcessShapeByShapeName(Process.StartName);

            // Find the outgoing link for start shape
            var outgoingLinkForStartShape = returnedProcess.GetOutgoingLinkForShape(startShape);

            // Find the user task from the second branch
            var userTaskFromSecondBranch = returnedProcess.GetNextShape(userDecision,
                outgoingLinkForStartShape.Orderindex + 1);

            // Find the system task from the second branch
            var systemTaskFromSecondBranch = returnedProcess.GetNextShape(userTaskFromSecondBranch);

            // Find the outgoing link for the system task from the second branch
            var outgoingLinkForSystemTaskFromSecondBranch =
                returnedProcess.GetOutgoingLinkForShape(systemTaskFromSecondBranch);

            // Locate the end shape for changing the merge point of user decision
            var endShape = returnedProcess.GetProcessShapeByShapeName(Process.EndName);

            // Change the second branch merge point of the user decision to the endShape
            returnedProcess.ChangeBranchMergePoint(userDecision, outgoingLinkForStartShape.Orderindex + 1, outgoingLinkForSystemTaskFromSecondBranch, endShape);

            // Update and Verify the modified process
            var updatedProcess = StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);

            // Verify that DecisionBranchDestinationLinks contained updated information for the updated merge point
            var secondDecisionBranchDestinationLink =
                updatedProcess.GetDecisionBranchDestinationLinkForDecisionShape(userDecision,
                    outgoingLinkForStartShape.Orderindex + 1);

            Assert.That(secondDecisionBranchDestinationLink.DestinationId == endShape.Id,
                "The destination Id from the DecisionBranchDestinationLink for the second branch after updating merge point should be {0} but {1} was returned.",
                endShape.Id, secondDecisionBranchDestinationLink.DestinationId);
        }

        [TestCase]
        [TestRail(96093)]
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
            var returnedProcess =
                StorytellerTestHelper
                    .CreateAndGetDefaultProcessWithTwoSequentialUserTasksAndOneSystemDecisionContainingMultipleConditions
                    (_storyteller, _project, _user, additionalBranches: 1);
            
            // Locate the start shape to locate the outgoing link to Precondition
            var startShape = returnedProcess.GetProcessShapeByShapeName(Process.StartName);

            // Find the outgoing link for start shape
            var outgoingLinkForStartShape = returnedProcess.GetOutgoingLinkForShape(startShape);

            // Locate the end shape for changing the merge point of user decision
            var endShape = returnedProcess.GetProcessShapeByShapeName(Process.EndName);

            // Find the system decision
            var systemDecision = returnedProcess.GetProcessShapesByShapeType(ProcessShapeType.SystemDecision).First();

            // Find the system task From the third branch
            var systemTaskFromThirdBranch =
                returnedProcess.GetNextShape(systemDecision, outgoingLinkForStartShape.Orderindex + 2);

            // Find the outgoing link for the system task from the third branch
            var outgoingLinkForSystemTaskFromThirdBranch =
                returnedProcess.GetOutgoingLinkForShape(systemTaskFromThirdBranch);

            // Change the third branch merge point of the system decision to the endShape
            returnedProcess.ChangeBranchMergePoint(systemDecision, outgoingLinkForStartShape.Orderindex + 2, outgoingLinkForSystemTaskFromThirdBranch, endShape);

            // Update the process
            var updatedProcess = StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);

            // Verify that DecisionBranchDestinationLinks contained updated information for the updated merge point

            var thirdDecisionBranchDestinationLink =
                updatedProcess.GetDecisionBranchDestinationLinkForDecisionShape(systemDecision,
                    outgoingLinkForStartShape.Orderindex + 2);

            Assert.That(thirdDecisionBranchDestinationLink.DestinationId == endShape.Id,
                "The destination Id from the DecisionBranchDestinationLink for the third branch after updating merge point should be {0} but {1} was returned.",
                endShape.Id, thirdDecisionBranchDestinationLink.DestinationId);
        }

        [TestCase]
        [TestRail(96094)]
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
            var returnedProcess =
                StorytellerTestHelper
                    .CreateAndGetDefaultProcessWithTwoSequentialUserTasksAndOneUserDecisionContainingMultipleConditions
                    (_storyteller, _project, _user, additionalBranches: 1);

            // Locate the start shape to locate the outgoing link to Precondition
            var startShape = returnedProcess.GetProcessShapeByShapeName(Process.StartName);

            // Find the outgoing link for start shape
            var outgoingLinkForStartShape = returnedProcess.GetOutgoingLinkForShape(startShape);


            // Locate the end shape for changing the merge point of user decision
            var endShape = returnedProcess.GetProcessShapeByShapeName(Process.EndName);

            // Find the user decision
            var userDecision = returnedProcess.GetProcessShapesByShapeType(ProcessShapeType.UserDecision).First();

            // Find the user task From the third branch
            var userTaskFromThirdBranch = returnedProcess.GetNextShape(userDecision,
                outgoingLinkForStartShape.Orderindex + 2);

            // Find the system task From the third branch
            var systemTaskFromThirdBranch = returnedProcess.GetNextShape(userTaskFromThirdBranch);

            // Find the outgoing link for the system task from the third branch
            var outgoingLinkForSystemTaskFromThirdBranch = returnedProcess.Links.Find(l => l.SourceId.Equals(systemTaskFromThirdBranch.Id));

            // Change the third branch merge point of the user decision to the endShape
            returnedProcess.ChangeBranchMergePoint(userDecision, outgoingLinkForStartShape.Orderindex + 2, outgoingLinkForSystemTaskFromThirdBranch, endShape);

            // Update the process
            var updatedProcess = StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);

            // Verify that DecisionBranchDestinationLinks contained updated information for the updated merge point
            var thirdDecisionBranchDestinationLink =
                updatedProcess.GetDecisionBranchDestinationLinkForDecisionShape(userDecision,
                    outgoingLinkForStartShape.Orderindex + 2);

            Assert.That(thirdDecisionBranchDestinationLink.DestinationId == endShape.Id,
                "The destination Id from the DecisionBranchDestinationLink for the third branch after updating merge point should be {0} but {1} was returned.",
                endShape.Id, thirdDecisionBranchDestinationLink.DestinationId);
        }
        #endregion Tests

    }
}
