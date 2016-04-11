﻿using CustomAttributes;
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
    public class DeleteSystemDecisionTests
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

            Assert.IsFalse(string.IsNullOrWhiteSpace(_user.Token.AccessControlToken),
                "The user didn't get an Access Control token!");

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
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcessWithOneSystemDecision(_storyteller, _project, _user);

            // Find the system decision to delete from the updated process
            var systemDecisionToDelete = returnedProcess.GetProcessShapesByShapeType(ProcessShapeType.SystemDecision).First();

            // Find the branch end shape for system decision
            var endShape = returnedProcess.GetProcessShapeByShapeName(Process.EndName);

            // Delete the specified system decision
            returnedProcess.DeleteSystemDecisionWithBranchesNotOfTheLowestOrder(systemDecisionToDelete, endShape);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);
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
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcessWithTwoSequentialSystemDecisions(_storyteller, _project, _user);

            // Find the first system decision to delete from the updated process
            var firstSystemDecisionToDelete = returnedProcess.GetProcessShapesByShapeType(ProcessShapeType.SystemDecision).First();

            // Merge point for first system decision is the second system decision on the process
            var mergePointForFirstSystemDecision = returnedProcess.GetProcessShapesByShapeType(ProcessShapeType.UserTask).Find(
                    ut => !ut.Name.Equals(Process.DefaultUserTaskName));

            // Delete the first system decision that merges before the added user task
            returnedProcess.DeleteSystemDecisionWithBranchesNotOfTheLowestOrder(firstSystemDecisionToDelete, mergePointForFirstSystemDecision);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);
        }


        [TestCase]
        [Description("Delete a system decision with branch that is on the main branch but within an" +
                     "outer branch of another sytem decision. Verfiy that the returned process has" +
                     "the inner system decision removed with all branches except the main branch." +
                     "The outer system decision and branch must remain present.")]
        public void DeleteInnerSystenDecisionContainedWithinMainBranchOfOuterSystemDecision_VerifyReturnedProcess()
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
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcessWithInnerAndOuterSystemDecisions(_storyteller,
                _project, _user);

            // Find the default SystemTask
            var defaultSystemTask = returnedProcess.GetProcessShapeByShapeName(Process.DefaultSystemTaskName);

            // Find link between the inner system decision and the default system task
            var targetLink = returnedProcess.GetIncomingLinkForShape(defaultSystemTask);

            // Find the inner system decision before the defaut system task
            var innerSystemDecision = returnedProcess.GetProcessShapesByShapeType(ProcessShapeType.SystemDecision).Find(
                    sd => sd.Id.Equals(targetLink.SourceId));

            // Find the inner system decision to delete from the updated process
            var innerSystemDecisionToDelete = returnedProcess.GetProcessShapeByShapeName(innerSystemDecision.Name);

            // Find the branch end point for system decision points
            var endShape = returnedProcess.GetProcessShapeByShapeName(Process.EndName);

            // Delete the inner system decision that merges before the end point
            returnedProcess.DeleteSystemDecisionWithBranchesNotOfTheLowestOrder(innerSystemDecisionToDelete, endShape);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);
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
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcessWithInnerAndOuterSystemDecisions(_storyteller,
                _project, _user);

            // Find the default UserTask
            var defaultUserTask = returnedProcess.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the inner system decision before the defaut system task
            var outerSystemDecision = returnedProcess.GetNextShape(defaultUserTask);

            // Find the outer system decision to delete from the updated process
            var outerSystemDecisionToDelete = returnedProcess.GetProcessShapeByShapeName(outerSystemDecision.Name);

            // Find the branch end point for system decision points
            var endShape = returnedProcess.GetProcessShapeByShapeName(Process.EndName);

            // Delete the outer system decision that merges before the end point
            returnedProcess.DeleteSystemDecisionWithBranchesNotOfTheLowestOrder(outerSystemDecisionToDelete, endShape);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);
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
                                  +----+--[ST3]--+

            It becomes this:
            [S]--[P]--+--[UT1]--+--[ST1]--[E]
            */
            // Create and get the default process
            var returnedProcess =
                StorytellerTestHelper.CreateAndGetDefaultProcessWithSystemDecisionContainingMultipleConditions(
                    _storyteller, _project, _user, 1);

            // Find the system decision to delete from the updated process
            var systemDecisionToDelete = returnedProcess.GetProcessShapesByShapeType(ProcessShapeType.SystemDecision).First();

            // Find the branch end point for system decision points
            var endShape = returnedProcess.GetProcessShapeByShapeName(Process.EndName);

            // Delete the specified system decision
            returnedProcess.DeleteSystemDecisionWithBranchesNotOfTheLowestOrder(systemDecisionToDelete, endShape);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);
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
            var returnedProcess =
                StorytellerTestHelper.CreateAndGetDefaultProcessWithSystemDecisionContainsSystemDecisionOnBranch(
                    _storyteller, _project, _user);

            // Find the default UserTask
            var defaultUserTask = returnedProcess.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the branch end point for system decision points
            var endShape = returnedProcess.GetProcessShapeByShapeName(Process.EndName);

            // Find the outgoing process link from the default user task
            var defaultUserTaskOutgoingProcessLink = returnedProcess.GetOutgoingLinkForShape(defaultUserTask);

            // Find the link between the system decision point and the System task on the second branch
            var branchingProcessLink = returnedProcess.Links.Find(l => l.Orderindex.Equals(defaultUserTaskOutgoingProcessLink.Orderindex + 1));

            // Find the system decision on the second branch for deletion
            var nestedSystemDecisionToDelete = returnedProcess.GetProcessShapeById(branchingProcessLink.DestinationId);

            // Delete the nested system decision that merges before the end point
            returnedProcess.DeleteSystemDecisionWithBranchesNotOfTheLowestOrder(nestedSystemDecisionToDelete, endShape);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);
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
            var returnedProcess =
                StorytellerTestHelper.CreateAndGetDefaultProcessWithSystemDecisionContainsSystemDecisionOnBranch(
                    _storyteller, _project, _user);

            // Find the branch end point for system decision points
            var endShape = returnedProcess.GetProcessShapeByShapeName(Process.EndName);

            // Find the system decision to delete from the updated process
            var rootSystemDecisionToDelete = returnedProcess.GetProcessShapesByShapeType(ProcessShapeType.SystemDecision).First();

            // Delete the root system decision that merges before the end point
            returnedProcess.DeleteSystemDecisionWithBranchesNotOfTheLowestOrder(rootSystemDecisionToDelete, endShape);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);
        }
    }
}