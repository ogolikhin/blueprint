using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using TestCommon;
using Utilities;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class DeleteDecisionBranchValidationTests : TestBase
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

        [TestCase]
        [Description("Deleting a branch from system decision containing only 2 branches." +
                     "verify that the returned response contains error message " +
                     "stating that decision shape requres an minimum of 2 branches")]
        public void DeleteOnlyAdditonalBranchFromSystemDecision_VerifyUpdateProcessReturnsValidationError()
        {
            /*
            If you start with this:
            [S]--[P]--+--[UT1]--+--<SD1>--+-------[ST1]--------+--[E]
                                     |                         |
                                     +----+-------[ST2]--------+
     
            This test validatates if the saving process API call returns the bad request (HTTP 400) error
            to prevent the process from updating with the invalid graph below:
            [S]--[P]--+--[UT1]--+--<SD1>--+--[ST1]--+--[E]

            */

            // Create and get the default process
            var novaProcess = Helper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Find the default UserTask
            var defaultUserTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the target SystemTask
            var targetSystemTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultSystemTaskName);

            // Find the branch end point for system decision points
            var branchEndPoint = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Find the outgoing process link from the default UserTask
            var defaultUserTaskOutgoingProcessLink = novaProcess.Process.GetOutgoingLinkForShape(defaultUserTask);

            // Add a System Decision point with a branch merging to branchEndPoint
            var systemDecision = novaProcess.Process.AddSystemDecisionPointWithBranchBeforeSystemTask(targetSystemTask, defaultUserTaskOutgoingProcessLink.Orderindex + 1, branchEndPoint.Id);

            // Save the process
            novaProcess.Update(_user, novaProcess.Artifact);
            novaProcess.RefreshArtifactFromServer(_user);

            // Get the system decison after saving the process
            var systemDecisionForDeletionProcess = novaProcess.Process.GetProcessShapeByShapeName(systemDecision.Name);

            // Delete the specified system decision branch
            novaProcess.Process.DeleteSystemDecisionBranch(systemDecisionForDeletionProcess, defaultUserTaskOutgoingProcessLink.Orderindex + 1, branchEndPoint);

            // Get and deserialize response 
            var ex = Assert.Throws<Http400BadRequestException>(
                () => Helper.ArtifactStore.UpdateNovaProcess( _user, novaProcess.NovaProcess)
                );

            var deserializedResponse = SerializationUtilities.DeserializeObject<ProcessValidationResponse>(ex.RestResponse.Content);

            var expectedValidationResponseContent = I18NHelper.FormatInvariant(
                ProcessValidationResponse.MinimumNumberBranchValidationFormat,
                systemDecisionForDeletionProcess.Id);

            // Assert that the response error message that the current process shape to be saved contains 
            // less than the mininum of 2 outgoing links
            AssertValidationResponse(deserializedResponse, expectedValidationResponseContent);
        }

        [TestCase]
        [Description("Deleting a branch from user decision containing only 2 branches." +
                     "verify that the returned response contains error message " +
                     "stating that decision shape requres an minimum of 2 branches")]
        public void DeleteOnlyAdditonalBranchFromUserDecision_VerifyUpdateProcessReturnsValidationError()
        {
            /*
            If you start with this:
                [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+--[E]
                               |                        |
                               +--[UT3]--+--[ST4]-------+

            This test validatates if the saving process API call returns the bad request (HTTP 400) error
            to prevent the process from updating with the invalid graph below:
                [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+--[E]
            */

            // Create and get the default process
            var novaProcess = Helper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Find precondition task
            var preconditionTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var preconditionOutgoingLink = novaProcess.Process.GetOutgoingLinkForShape(preconditionTask);

            // Find the branch endpoint for the new branch
            var branchEndPoint = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Add user decision with branch to end
            var userDecision = novaProcess.Process.AddUserDecisionPointWithBranchAfterShape(preconditionTask, preconditionOutgoingLink.Orderindex + 1);

            // Save the process
            novaProcess.Update(_user, novaProcess.Artifact);
            novaProcess.RefreshArtifactFromServer(_user);

            // Get the user decison after saving the process
            var userDecisionForDeletionProcess = novaProcess.Process.GetProcessShapeByShapeName(userDecision.Name);

            // Delete the specified user decision branch - work in progress
            novaProcess.Process.DeleteUserDecisionBranch(userDecisionForDeletionProcess, preconditionOutgoingLink.Orderindex + 1, branchEndPoint);

            // Get and deserialize response 
            var ex = Assert.Throws<Http400BadRequestException>(
                () => Helper.ArtifactStore.UpdateNovaProcess(_user, novaProcess.NovaProcess)
                );

            var deserializedResponse = SerializationUtilities.DeserializeObject<ProcessValidationResponse>(ex.RestResponse.Content);

            var expectedValidationResponseContent = I18NHelper.FormatInvariant(
                ProcessValidationResponse.MinimumNumberBranchValidationFormat,
                userDecisionForDeletionProcess.Id);

            // Assert that the response error message that the current process shape to be saved contains 
            // less than the mininum of 2 outgoing links
            AssertValidationResponse(deserializedResponse, expectedValidationResponseContent);
        }

        #endregion Tests

        #region Private Methods

        private static void AssertValidationResponse(ProcessValidationResponse deserializedResponse, string expectedContent)
        {
            ThrowIf.ArgumentNull(deserializedResponse, nameof(deserializedResponse));
            ThrowIf.ArgumentNull(expectedContent, nameof(expectedContent));

            Assert.That(
                deserializedResponse.Message.Contains(expectedContent),
                "Response message should have included: {0} => But Actual response message was: {1}",
                expectedContent,
                deserializedResponse.Message);
        }

        #endregion Private Methods
    }
}
