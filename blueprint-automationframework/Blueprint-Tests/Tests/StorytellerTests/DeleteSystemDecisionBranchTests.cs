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
                // Delete or Discard all the artifacts that were added.
                var savedArtifactsList = new List<IOpenApiArtifact>();
                foreach (var artifact in _storyteller.Artifacts.ToArray())
                {
                    if (artifact.IsPublished)
                    {
                        _storyteller.DeleteProcessArtifact(artifact, deleteChildren: _deleteChildren);
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

        [TestCase(0)]
        [TestCase(1)]
        [Description("Delete a branch from a system decision point that has more than 2 conditions and verify that the " +
                     "returned process still contains the system decision with all branches except the deleted branch.")]
        public void DeleteBranchfromSystemDecisionWithMoreThanTwoConditions_VerifyReturnedProcess(double orderIndexOfBranch)
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

            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find the default UserTask
            var defaultUserTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the target SystemTask
            var targetSystemTask = process.GetProcessShapeByShapeName(Process.DefaultSystemTaskName);

            // Find the branch end point for system decision points
            var branchEndPoint = process.GetProcessShapeByShapeName(Process.EndName);

            // Find the outgoing process link from the default UserTask
            var defaultUserTaskOutgoingProcessLink = process.GetOutgoingLinkForShape(defaultUserTask);

            // Add System Decision point with branch merging to branchEndPoint
            var systemDecisionPoint = process.AddSystemDecisionPointWithBranchBeforeSystemTask(targetSystemTask, defaultUserTaskOutgoingProcessLink.Orderindex + 1, branchEndPoint.Id);

            // Add additonal branch to the System Decision point
            process.AddBranchWithSystemTaskToSystemDecisionPoint(systemDecisionPoint, defaultUserTaskOutgoingProcessLink.Orderindex + 2, branchEndPoint.Id);

            // Save the process
            var returnedProcess = _storyteller.UpdateProcess(_user, process);

            var systemDecisionPointForDeletionProcess = returnedProcess.GetProcessShapeByShapeName(systemDecisionPoint.Name);

            // Merge point for the outer user decision is the process end point
            var endShape = process.GetProcessShapeByShapeName(Process.EndName);

            // Delete the specified system decision branch - work in progress
            returnedProcess.DeleteSystemDecisionBranch(systemDecisionPointForDeletionProcess, orderIndexOfBranch, endShape);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(process, _storyteller, _user);
        }

        [TestCase]
        [Description("Delete a branch from a system decision point that has a nested system decision on one of the" +
                     " branches and verify that the returned process still contains the system decision with all" +
                     " branches except the deleted branch.")]
        public void DeleteBranchfromSystemDecisionThatContainsNestedSystemDecision_VerifyReturnedProcess()
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

            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find the default UserTask
            var defaultUserTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the target SystemTask
            var targetSystemTask = process.GetProcessShapeByShapeName(Process.DefaultSystemTaskName);

            // Find the branch end point for system decision points
            var branchEndPoint = process.GetProcessShapeByShapeName(Process.EndName);

            // Find the outgoing process link from the default UserTask
            var defaultUserTaskOutgoingProcessLink = process.GetOutgoingLinkForShape(defaultUserTask);

            // Add a System Decision point (root System Decision point) with branch merging to branchEndPoint
            var rootSystemDecisionPoint = process.AddSystemDecisionPointWithBranchBeforeSystemTask(targetSystemTask, defaultUserTaskOutgoingProcessLink.Orderindex + 1, branchEndPoint.Id);

            // Add additonal branch on the root System Decision point
            process.AddBranchWithSystemTaskToSystemDecisionPoint(rootSystemDecisionPoint, defaultUserTaskOutgoingProcessLink.Orderindex + 2, branchEndPoint.Id);

            // Get the link between the system decision point and the System task on the third branch
            var branchingProcessLink = process.Links.Find(l => l.Orderindex.Equals(defaultUserTaskOutgoingProcessLink.Orderindex + 2));

            // Get the System Task shape on the third branch for adding the additional System Decision Point
            var systemTaskOnTheThirdBranch = process.GetProcessShapeById(branchingProcessLink.DestinationId);

            // Add a System Decision point on the third branch that merges to branchEndPoint
            process.AddSystemDecisionPointWithBranchBeforeSystemTask(systemTaskOnTheThirdBranch, defaultUserTaskOutgoingProcessLink.Orderindex + 1, branchEndPoint.Id);

            // Save the process
            var returnedProcess = _storyteller.UpdateProcess(_user, process);

            var systemDecisionPointForDeletionProcess = returnedProcess.GetProcessShapeByShapeName(rootSystemDecisionPoint.Name);

            // Merge point for the outer user decision is the process end point
            var endShape = process.GetProcessShapeByShapeName(Process.EndName);

            // Delete the specified system decision branch - work in progress
            returnedProcess.DeleteSystemDecisionBranch(systemDecisionPointForDeletionProcess, defaultUserTaskOutgoingProcessLink.Orderindex + 2, endShape);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(process, _storyteller, _user);
        }


        [TestCase]
        [Description("Delete a branch from a system decision point that has a nested user decision on one of the" +
                     " branches and verify that the returned process still contains the system decision with all" +
                     " branches except the deleted branch.")]
        public void DeleteBranchfromSystemDecisionThatContainsNestedUserDecision_VerifyReturnedProcess()
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

            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find the default UserTask
            var defaultUserTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the target SystemTask
            var targetSystemTask = process.GetProcessShapeByShapeName(Process.DefaultSystemTaskName);

            // Find the branch end point for system decision points
            var branchEndPoint = process.GetProcessShapeByShapeName(Process.EndName);

            // Find the outgoing process link from the default UserTask
            var defaultUserTaskOutgoingProcessLink = process.GetOutgoingLinkForShape(defaultUserTask);

            // Add a System Decision point (root System Decision point) with branch merging to branchEndPoint
            var rootSystemDecisionPoint = process.AddSystemDecisionPointWithBranchBeforeSystemTask(targetSystemTask, defaultUserTaskOutgoingProcessLink.Orderindex + 1, branchEndPoint.Id);

            // Add additonal branch on the root System Decision point
            process.AddBranchWithSystemTaskToSystemDecisionPoint(rootSystemDecisionPoint, defaultUserTaskOutgoingProcessLink.Orderindex + 2, branchEndPoint.Id);
            
            // Get the link between the system decision point and the System task on the third branch
            var branchingProcessLink = process.Links.Find(l => l.Orderindex.Equals(defaultUserTaskOutgoingProcessLink.Orderindex + 2));

            // Get the System Task shape on the third branch for adding the additional System Decision Point
            var systemTaskOnTheThirdBranch = process.GetProcessShapeById(branchingProcessLink.DestinationId);

            // Add a user decision point <UD1> with branch to end after new system task
            process.AddUserDecisionPointWithBranchAfterShape(systemTaskOnTheThirdBranch, defaultUserTaskOutgoingProcessLink.Orderindex + 1, branchEndPoint.Id);


            // Save the process
            var returnedProcess = _storyteller.UpdateProcess(_user, process);

            var systemDecisionPointForDeletionProcess = returnedProcess.GetProcessShapeByShapeName(rootSystemDecisionPoint.Name);

            // Merge point for the outer user decision is the process end point
            var endShape = process.GetProcessShapeByShapeName(Process.EndName);

            // Delete the specified system decision branch - work in progress
            returnedProcess.DeleteSystemDecisionBranch(systemDecisionPointForDeletionProcess, defaultUserTaskOutgoingProcessLink.Orderindex + 2, endShape);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(process, _storyteller, _user);
        }
    }
}
