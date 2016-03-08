using System.Collections.Generic;
using System.Globalization;
using System.Net;
using CustomAttributes;
using Model;
using Model.Factories;
using Model.Impl;
using NUnit.Framework;
using Utilities;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class UpdateProcessValidationTests
    {
        private IAdminStore _adminStore;
        private IStoryteller _storyteller;
        private IUser _user;
        private IProject _project;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            _adminStore = AdminStoreFactory.GetAdminStoreFromTestConfig();
            _storyteller = StorytellerFactory.GetStorytellerFromTestConfig();
            _user = UserFactory.CreateUserAndAddToDatabase();
            _project = ProjectFactory.GetProject(_user);

            // Get a valid token for the user.
            ISession session = _adminStore.AddSession(_user.Username, _user.Password);
            _user.SetToken(session.SessionId);

            Assert.IsFalse(string.IsNullOrWhiteSpace(_user.Token.AccessControlToken), "The user didn't get an Access Control token!");
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
             if (_storyteller.Artifacts != null)
            {
                // TODO: Uncomment when new Publish Process is implemented
                //Delete all the artifacts that were added.
                //foreach (var artifact in _storyteller.Artifacts)
                //{
                //    _storyteller.DeleteProcessArtifact(artifact, _user);
                //}
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

        [TestCase, Description("Remove name of process and verify returned validation error")]
        public void UpdateProcessWithoutProcessName_VerifyGetProcessReturnsValidationError()
        {
            // Create default process
            var addedProcessArtifact = _storyteller.CreateProcessArtifact(_project, BaseArtifactType.Process, _user);

            // Get default process
            var returnedProcess = _storyteller.GetProcess(_user, addedProcessArtifact.Id);

            Assert.IsNotNull(returnedProcess, "The returned process was null.");

            // Modify default process Name
            returnedProcess.Name = string.Empty;

            // Get and deserialize response
            var response = _storyteller.UpdateProcessReturnResponseOnly(_user, returnedProcess, new List<HttpStatusCode> { HttpStatusCode.InternalServerError});
            var deserializedResponse = Deserialization.DeserializeObject<ProcessValidationResponse>(response);

            // Assert that the deserialized response indicates that the process name is required
            Assert.That( deserializedResponse.Message == ProcessValidationResponse.NameRequired,
                "Expected response message: {0} => Actual response message {1}", ProcessValidationResponse.NameRequired, deserializedResponse.Message
                );
        }

        [TestCase, Description("Update artifact with orphaned task and verify returned validation error")]
        public void UpdateProcessWithOrphanedTask_VerifyGetProcessReturnsValidationError()
        {
            // Create default process
            var addedProcessArtifact = _storyteller.CreateProcessArtifact(_project, BaseArtifactType.Process, _user);

            // Get default process
            var returnedProcess = _storyteller.GetProcess(_user, addedProcessArtifact.Id);

            Assert.IsNotNull(returnedProcess, "The returned process was null.");

            // Find precondition task
            var preconditionTask = returnedProcess.FindProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var processLink = returnedProcess.FindOutgoingLinkForShape(preconditionTask.Id);

            // Remove the process link between the precondition and the default user task
            returnedProcess.Links.Remove((ProcessLink)processLink);

            // Get and deserialize response
            var response = _storyteller.UpdateProcessReturnResponseOnly(_user, returnedProcess, new List<HttpStatusCode> { HttpStatusCode.InternalServerError });
            var deserializedResponse = Deserialization.DeserializeObject<ProcessValidationResponse>(response);

            // Asser that the deserialized response indicates that an orphaned shape was found
            Assert.That(deserializedResponse.Message.Contains(ProcessValidationResponse.OrphanedShapes),
                "Expected response message: {0} => Actual response message {1}", ProcessValidationResponse.OrphanedShapes, deserializedResponse.Message
                );

            // Assert that the shape id of the orphaned shape is the one expected
            Assert.That(deserializedResponse.Message.Contains(processLink.DestinationId.ToString(CultureInfo.InvariantCulture)),
                "Expected response message: {0} => Actual response message {1}", ProcessValidationResponse.OrphanedShapes, deserializedResponse.Message
                );
        }

        #endregion Tests
    }

    /// <summary>
    /// 
    /// </summary>
    public class ProcessValidationResponse
    {
        public static readonly string NameRequired = "Name is required for Process";

        public static readonly string OrphanedShapes = "Orphaned shapes discovered";

        public string Message { get; set; }
    }
}
