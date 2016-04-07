
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
using System.Net;
using Utilities;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class DeleteDecisionBranchValidationTests
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
        [Description("Deleting an only available additional branch from system decision and verify that " +
                     "the returned response contains error message when save the invalid process model")]
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
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find the default UserTask
            var defaultUserTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the target SystemTask
            var targetSystemTask = process.GetProcessShapeByShapeName(Process.DefaultSystemTaskName);

            // Find the branch end point for system decision points
            var branchEndPoint = process.GetProcessShapeByShapeName(Process.EndName);

            // Find the outgoing process link from the default UserTask
            var defaultUserTaskOutgoingProcessLink = process.GetOutgoingLinkForShape(defaultUserTask);

            // Add a System Decision point with branch merging to branchEndPoint
            var systemDecision = process.AddSystemDecisionPointWithBranchBeforeSystemTask(targetSystemTask, defaultUserTaskOutgoingProcessLink.Orderindex + 1, branchEndPoint.Id);

            // Save the process
            var returnedProcess = _storyteller.UpdateProcess(_user, process);

            // Get the system decison after saving the process
            var systemDecisionForDeletionProcess = returnedProcess.GetProcessShapeByShapeName(systemDecision.Name);

            // Delete the specified system decision branch
            returnedProcess.DeleteSystemDecisionBranch(systemDecisionForDeletionProcess, defaultUserTaskOutgoingProcessLink.Orderindex + 1, branchEndPoint);

            // Get and deserialize response
            var response = _storyteller.UpdateProcessReturnResponseOnly(_user, returnedProcess, new List<HttpStatusCode> { HttpStatusCode.BadRequest });
            var deserializedResponse = Deserialization.DeserializeObject<ProcessValidationResponse>(response);

            // Assert that the deserialized response indicates that the process name is required
            Assert.That(deserializedResponse.Message.Contains(ProcessValidationResponse.MinimumNumberBranchValidationPart1 + systemDecisionForDeletionProcess.Id + ProcessValidationResponse.MinimumNumberBranchValidationPart2),
                "Expected response message: {0} => Actual response message {1}", ProcessValidationResponse.MinimumNumberBranchValidationPart1 + systemDecisionForDeletionProcess.Id + ProcessValidationResponse.MinimumNumberBranchValidationPart2, deserializedResponse.Message
                );

        }

        [TestCase]
        [Description("Deleting an only available additional branch from user decision and verify that " +
                     "the returned response contains error message when save the invalid process model")]
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
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find precondition task
            var preconditionTask = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var preconditionOutgoingLink = process.GetOutgoingLinkForShape(preconditionTask);

            // Find the branch endpoint for the new branch
            var branchEndPoint = process.GetProcessShapeByShapeName(Process.EndName);

            // Add user decision with branch to end
            var userDecision = process.AddUserDecisionPointWithBranchAfterShape(preconditionTask, preconditionOutgoingLink.Orderindex + 1);

            // Save the process
            var returnedProcess = _storyteller.UpdateProcess(_user, process);

            // Get the user decison after saving the process
            var userDecisionForDeletionProcess = returnedProcess.GetProcessShapeByShapeName(userDecision.Name);

            // Delete the specified user decision branch - work in progress
            returnedProcess.DeleteUserDecisionBranch(userDecisionForDeletionProcess, preconditionOutgoingLink.Orderindex + 1, branchEndPoint);

            // Get and deserialize response
            var response = _storyteller.UpdateProcessReturnResponseOnly(_user, returnedProcess, new List<HttpStatusCode> { HttpStatusCode.BadRequest });
            var deserializedResponse = Deserialization.DeserializeObject<ProcessValidationResponse>(response);

            // Assert that the deserialized response indicates that the process name is required
            Assert.That(deserializedResponse.Message.Contains(ProcessValidationResponse.MinimumNumberBranchValidationPart1 + userDecisionForDeletionProcess.Id + ProcessValidationResponse.MinimumNumberBranchValidationPart2),
                "Expected response message: {0} => Actual response message {1}", ProcessValidationResponse.MinimumNumberBranchValidationPart1 + userDecisionForDeletionProcess.Id + ProcessValidationResponse.MinimumNumberBranchValidationPart2, deserializedResponse.Message
                );

        }

        #endregion Tests

        /// <summary>
        /// 
        /// </summary>
        public class ProcessValidationResponse
        {
            public static readonly string MinimumNumberBranchValidationPart1 = "Decision shape with Id ";
            public static readonly string MinimumNumberBranchValidationPart2 = " contains less than the minimum of 2 outgoing links.";
            public string Message { get; set; }
        }
    }
}
