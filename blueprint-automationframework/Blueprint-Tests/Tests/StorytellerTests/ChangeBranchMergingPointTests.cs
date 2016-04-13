using System.Collections.Generic;
using System.Linq;
using CustomAttributes;
using Model;
using Model.Factories;
using Model.OpenApiModel;
using Model.StorytellerModel;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using Helper;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class ChangeBranchMergingPointTests
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

        #region Tests

        [TestCase]
        [Description("Change the merging point of the second branch of system decision. Verify that returned process after the save " +
                     "contains valid merging point information.")]
        public void ChangeSystemDecisionMeringPointForSecondBranch_VerifyReturnProcess()
        {
            /*
            If you start with this:
            [S]--[P]--+--[UT1]--<SD1>--+--[ST1]--+--+--[UT2]--+--[ST2]--+--[E]
                                  |              |
                                  +----+--[ST3]--+

            It becomes this:
            [S]--[P]--+--[UT1]--<SD1>--+--[ST1]--+--+--[UT2]--+--[ST3]--+--[E]
                                  |                                     |
                                  +----+--[ST3]--+----------------------+
            */
            // Create and get the process with two sequential user tasks and one system decision
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcessWithTwoSequentialUserTasksAndOneSystemDecision(_storyteller, _project, _user);

            // Locate the start shape to locate the outgoing link to Precondition
            var startShape = returnedProcess.GetProcessShapeByShapeName(Process.StartName);

            // Find the outgoing link for start shape
            var outgoingLinkTForStartShape = returnedProcess.GetOutgoingLinkForShape(startShape);

            // Find the link from the system decision to second branch
            var secondBranchLinkFromSystemDecision =
                returnedProcess.Links.Find(l => l.Orderindex.Equals(outgoingLinkTForStartShape.Orderindex + 1));

            // Find the outgoing link for the system task from the second branch
            var outgoingLinkForSystemTaskFromSecondBranch = returnedProcess.Links.Find(l => l.SourceId.Equals(secondBranchLinkFromSystemDecision.DestinationId));

            // Locate the end shape for changing the merging point of system decision
            var endShape = returnedProcess.GetProcessShapeByShapeName(Process.EndName);

            // Change the second branch merging point of the system decision to the endShape
            outgoingLinkForSystemTaskFromSecondBranch.DestinationId = endShape.Id;

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);
        }

        [TestCase]
        [Description("Change the merging point of the second branch of user decision. Verify that returned process after the save " +
                     "contains valid merging point information.")]
        public void ChangeUserDecisionMeringPointForSecondBranch_VerifyReturnProcess()
        {
            /*
            If you start with this:
            [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+--+--[UT3]--+--[ST4]--+--[E]
                           |                        |
                           +-------[UT5]--+--[ST6]--+

            It becomes this:
            [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+--+--[UT3]--+--[ST4]--+--[E]
                           |                                               |
                           +-------[UT5]--+--[ST6]--+----------------------+
            */
            // Create and get the process with two sequential user tasks and one user decision
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcessWithTwoSequentialUserTasksAndOneUserDecision(_storyteller, _project, _user);

            // Find the user decision to delete from the updated process
            var userDecision = returnedProcess.GetProcessShapesByShapeType(ProcessShapeType.UserDecision).First();

            // Locate the start shape to locate the outgoing link to Precondition
            var startShape = returnedProcess.GetProcessShapeByShapeName(Process.StartName);

            // Find the outgoing link for start shape
            var outgoingLinkForStartShape = returnedProcess.GetOutgoingLinkForShape(startShape);

            // Find the link from the user decision to user task in the second branch
            var secondBranchLinkFromUserDecision = returnedProcess.Links.Find(l => l.SourceId.Equals(userDecision.Id) && l.Orderindex.Equals(outgoingLinkForStartShape.Orderindex + 1));

            // Find the system task from the second branch
            var systemTaskFromSecondBranch =
                returnedProcess.GetNextShape(
                    returnedProcess.GetProcessShapeById(secondBranchLinkFromUserDecision.DestinationId));

            // Find the outgoing link for the system task from the second branch
            var outgoingLinkForSystemTaskFromSecondBranch = returnedProcess.Links.Find(l => l.SourceId.Equals(systemTaskFromSecondBranch.Id));

            // Locate the end shape for changing the merging point of user decision
            var endShape = returnedProcess.GetProcessShapeByShapeName(Process.EndName);

            // Change the second branch merging point of the use decision to the endShape
            outgoingLinkForSystemTaskFromSecondBranch.DestinationId = endShape.Id;

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase]
        [Description("Save the process with a system decision which contains two additional branches beside the main branch. " +
                     "Each additional branch merges to different locations. Verify that returned process after the save " +
                     "contains valid merging point information.")]
        public void SystemDecisionWithMultipleMeringPoints_VerifyDecisionBranchDestinationLinksFromReturnProcess()
        {
            /*
            Save the following change:
            
            Before the merging point change:
            [S]--[P]--+--[UT1]--<SD1>--+--[ST1]--+--+--[UT2]--+--[ST2]--+--[E]
                                  |              |
                                  +----+--[ST3]--+
                                  |              |
                                  +----+--[ST4]--+    <--- additionalBranches: 1
            
            After the merging point change:
            [S]--[P]--+--[UT1]--<SD1>--+--[ST1]--+--+--[UT2]--+--[ST2]--+--[E]
                                  |              |                      |
                                  +----+--[ST3]--+                      |
                                  |                                     |
                                  +----+--[ST4]--+----------------------+    <--- additionalBranches: 1
            Verify that returned process model contains correct values on DecisionBranchDestinationLinks, section of the process model contains information for merging points
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

            // Locate the end shape for changing the merging point of user decision
            var endShape = returnedProcess.GetProcessShapeByShapeName(Process.EndName);

            // Find the system decision
            var systemDecision = returnedProcess.GetProcessShapesByShapeType(ProcessShapeType.SystemDecision).First();

            // Find the sytem task From the third branch
            var systmeTaskFromThirdBranch =
                returnedProcess.GetProcessShapeById(
                    returnedProcess.Links.Find(l => l.SourceId.Equals(systemDecision.Id) && l.Orderindex.Equals(outgoingLinkForStartShape.Orderindex + 2)).DestinationId);

            // Find the outgoing link for the system task from the third branch
            var outgoingLinkForSystemTaskFromThirdBranch = returnedProcess.Links.Find(l => l.SourceId.Equals(systmeTaskFromThirdBranch.Id));

            // Change the third branch merging point of the use decision to the endShape
            outgoingLinkForSystemTaskFromThirdBranch.DestinationId = endShape.Id;

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase]
        [Description("Save the process with a user decision which contains two additional branches beside the main branch. " +
             "Each additional branch merges to different locations. Verify that returned process after the save " +
             "contains valid merging point information.")]
        public void UserDecisionWithMultipleMeringPoints_VerifyDecisionBranchDestinationLinksFromReturnProcess()
        {
            /*
            Save the following process:
            
            Before the merging point change:
            [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+--+--[UT3]--+--[ST4]--+--[E]
                           |                        |
                           +-------[UT5]--+--[ST6]--+
                           |                        |
                           +-------[UT7]--+--[ST8]--+    <--- additionalBranches: 1
            
            After the merging point change:
            [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+--+--[UT3]--+--[ST4]--+--[E]
                           |                        |                      |
                           +-------[UT5]--+--[ST6]--+                      |
                           |                                               |
                           +-------[UT7]--+--[ST8]--+--+-------------------+    <--- additionalBranches: 1
            Verify that returned process model contains correct values on DecisionBranchDestinationLinks, section of the process model contains information for merging points
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


            // Locate the end shape for changing the merging point of user decision
            var endShape = returnedProcess.GetProcessShapeByShapeName(Process.EndName);

            // Find the user decision
            var userDecision = returnedProcess.GetProcessShapesByShapeType(ProcessShapeType.UserDecision).First();

            // Find the user task From the third branch
            var userTaskFromThirdBranch =
                returnedProcess.GetProcessShapeById(
                    returnedProcess.Links.Find(l => l.SourceId.Equals(userDecision.Id) && l.Orderindex.Equals(outgoingLinkForStartShape.Orderindex + 2)).DestinationId);

            // Find the sytem task From the third branch
            var systemTaskFromThirdBranch = returnedProcess.GetNextShape(userTaskFromThirdBranch);

            // Find the outgoing link for the system task from the third branch
            var outgoingLinkForSystemTaskFromThirdBranch = returnedProcess.Links.Find(l => l.SourceId.Equals(systemTaskFromThirdBranch.Id));

            // Change the third branch merging point of the use decision to the endShape
            outgoingLinkForSystemTaskFromThirdBranch.DestinationId = endShape.Id;

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);
        }
        #endregion Tests

    }
}
