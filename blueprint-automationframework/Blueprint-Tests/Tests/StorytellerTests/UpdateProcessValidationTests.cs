﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.OpenApiModel;
using Model.StorytellerModel;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using Utilities;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class UpdateProcessValidationTests
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
        [Description("Clear the name of process and verify the returned process has a validation error" +
                     "indicating that the process name is required.")]
        public void UpdateProcessWithoutProcessName_VerifyGetProcessReturnsValidationError()
        {
            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

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

        [TestCase]
        [Description("Add an orphaned task to a process and verify the returned process has a validation error" +
                     "indicating that an orphaned task was detected.  The validation error message includes" +
                     "the sub-artifact Id of the orphaned task.")]
        public void UpdateProcessWithOrphanedTask_VerifyGetProcessReturnsValidationError()
        {
            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find precondition task
            var preconditionTask = returnedProcess.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var processLink = returnedProcess.GetOutgoingLinkForShape(preconditionTask);

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
