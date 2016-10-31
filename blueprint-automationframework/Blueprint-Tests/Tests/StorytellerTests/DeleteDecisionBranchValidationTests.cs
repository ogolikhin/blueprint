
using System.Collections.Generic;
using CustomAttributes;
using Model;
using Model.Factories;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using Helper;
using System.Net;
using Common;
using TestCommon;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class DeleteDecisionBranchValidationTests : TestBase
    {
        private IUser _user;
        private IProject _project;

        public static readonly string MinimumNumberBranchValidationFormat = "Decision shape with Id {0} contains less than the minimum of 2 outgoing links.";

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
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _project, _user);

            // Find the default UserTask
            var defaultUserTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the target SystemTask
            var targetSystemTask = process.GetProcessShapeByShapeName(Process.DefaultSystemTaskName);

            // Find the branch end point for system decision points
            var branchEndPoint = process.GetProcessShapeByShapeName(Process.EndName);

            // Find the outgoing process link from the default UserTask
            var defaultUserTaskOutgoingProcessLink = process.GetOutgoingLinkForShape(defaultUserTask);

            // Add a System Decision point with a branch merging to branchEndPoint
            var systemDecision = process.AddSystemDecisionPointWithBranchBeforeSystemTask(targetSystemTask, defaultUserTaskOutgoingProcessLink.Orderindex + 1, branchEndPoint.Id);

            // Save the process
            var returnedProcess = Helper.Storyteller.UpdateProcess(_user, process);

            // Get the system decison after saving the process
            var systemDecisionForDeletionProcess = returnedProcess.GetProcessShapeByShapeName(systemDecision.Name);

            // Delete the specified system decision branch
            returnedProcess.DeleteSystemDecisionBranch(systemDecisionForDeletionProcess, defaultUserTaskOutgoingProcessLink.Orderindex + 1, branchEndPoint);

            // Get and deserialize response
            var response = Helper.Storyteller.UpdateProcessReturnResponseOnly(_user, returnedProcess, expectedStatusCodes: new List<HttpStatusCode> { HttpStatusCode.BadRequest });

            var expectedMessage = I18NHelper.FormatInvariant(MinimumNumberBranchValidationFormat,
                systemDecisionForDeletionProcess.Id);

            // Assert that the response error message that the current process shape to be saved contains 
            // less than the mininum of 2 outgoing links
            Assert.That(response.Contains(expectedMessage),
                "Expected response message: {0} => Actual response message {1}", expectedMessage, response
                );

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
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _project, _user);

            // Find precondition task
            var preconditionTask = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var preconditionOutgoingLink = process.GetOutgoingLinkForShape(preconditionTask);

            // Find the branch endpoint for the new branch
            var branchEndPoint = process.GetProcessShapeByShapeName(Process.EndName);

            // Add user decision with branch to end
            var userDecision = process.AddUserDecisionPointWithBranchAfterShape(preconditionTask, preconditionOutgoingLink.Orderindex + 1);

            // Save the process
            var returnedProcess = Helper.Storyteller.UpdateProcess(_user, process);

            // Get the user decison after saving the process
            var userDecisionForDeletionProcess = returnedProcess.GetProcessShapeByShapeName(userDecision.Name);

            // Delete the specified user decision branch - work in progress
            returnedProcess.DeleteUserDecisionBranch(userDecisionForDeletionProcess, preconditionOutgoingLink.Orderindex + 1, branchEndPoint);

            // Get and deserialize response
            var response = Helper.Storyteller.UpdateProcessReturnResponseOnly(_user, returnedProcess, expectedStatusCodes: new List<HttpStatusCode> { HttpStatusCode.BadRequest });

            var expectedMessage = I18NHelper.FormatInvariant(MinimumNumberBranchValidationFormat,
                userDecisionForDeletionProcess.Id);

            // Assert that the response error message that the current process shape to be saved contains 
            // less than the mininum of 2 outgoing links
            Assert.That(response.Contains(expectedMessage),
                "Expected response message: {0} => Actual response message {1}", expectedMessage, response
                );

        }

        #endregion Tests

    }
}
