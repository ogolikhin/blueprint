using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.OpenApiModel;
using Model.StorytellerModel;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using System.Linq;
using System.Collections.Generic;
using Utilities;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class UpdateProcessNegativeTests
    {
        private const string STORYLINKSKEY = "storyLinks";

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
        [Description("Add a story link to a user task without a story link and execute UpdateProcess().  Verify that" +
                     "the returned process does not include the story link since story links can only be created" +
                     "by GenerateUserStories().")]
        public void AddStoryLinkToUserTaskWithoutStoryLink_UpdateProcess_VerifyReturnedProcessDoesNotHaveStoryLink()
        {
            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Get default user task
            var defaultUserTask = returnedProcess.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Create and publish textual requirement artifact to simulate user story artifact
            var addedArtifact = ArtifactFactory.CreateOpenApiArtifact(_project, _user, BaseArtifactType.TextualRequirement);
            addedArtifact.Save(_user);
            addedArtifact.Publish(_user);

            // Add a story link to the default user task
            var storyLink = new StoryLink(defaultUserTask.Id, addedArtifact.Id, 1, addedArtifact.Id);
            defaultUserTask.PropertyValues[STORYLINKSKEY].Value = storyLink;

            // Update the process using UpdateProcess in attempt to add a story link
            var modifiedReturnedProcess = _storyteller.UpdateProcess(_user, returnedProcess);

            Assert.IsNotNull(modifiedReturnedProcess, "The process returned from UpdateProcess() was null.");

            // Verify returned process does not contain a story link
            Assert.IsNull(modifiedReturnedProcess.GetProcessShapeByShapeName(Process.DefaultUserTaskName).PropertyValues[STORYLINKSKEY].Value,
                "The story link was saved using UpdateProcess but should not have been saved");
        }

        [TestCase]
        [Description("Add a story link to a user task that already has an existing story link and execute UpdateProcess()." +
                     "Verify that the returned process only contains the single original story link since story links " +
                     "can only be added by GenerateUserStories().")]
        public void AddStoryLinkToUserTaskWithStoryLink_UpdateProcess_VerifyReturnedProcessDoesNotHaveStoryLink()
        {
            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Publish process; enable recursive delete flag
            _storyteller.PublishProcess(_user, returnedProcess);
            //deleteChildren = true;

            // Generate user stories for process
            _storyteller.GenerateUserStories(_user, returnedProcess);

            // Get default process again with updated story link
            returnedProcess = _storyteller.GetProcess(_user, returnedProcess.Id);

            // Get default user task
            var defaultUserTask = returnedProcess.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Get original story link
            var originalStoryLinksProperty = defaultUserTask.PropertyValues[STORYLINKSKEY];
            var originalStoryLink = Deserialization.DeserializeObject<StoryLink>(originalStoryLinksProperty.Value.ToString());

            // Create and publish textual requirement artifact to simulate user story artifact
            var addedArtifact = ArtifactFactory.CreateOpenApiArtifact(_project, _user, BaseArtifactType.TextualRequirement);
            addedArtifact.Save(_user);
            addedArtifact.Publish(_user);

            // Change the default user task's story link to the added artifact
            var newStoryLink = new StoryLink(defaultUserTask.Id, addedArtifact.Id, 1, addedArtifact.Id);
            var newStoryLinksProperty = new PropertyValueInformation
                {
                    PropertyName = originalStoryLinksProperty.PropertyName,
                    TypeId = originalStoryLinksProperty.TypeId,
                    TypePredefined = originalStoryLinksProperty.TypePredefined,
                    Value = newStoryLink
                };

            // Update the story link
            defaultUserTask.PropertyValues[STORYLINKSKEY] = newStoryLinksProperty;

            // Update the process using UpdateProcess in attempt to change the story link
            var modifiedReturnedProcess = _storyteller.UpdateProcess(_user, returnedProcess);

            Assert.IsNotNull(modifiedReturnedProcess, "The process returned from UpdateProcess() was null.");

            // Get returned story link
            var returnedStoryLinksProperty = modifiedReturnedProcess.GetProcessShapeByShapeName(Process.DefaultUserTaskName).PropertyValues[STORYLINKSKEY];
            var returnedStoryLink = Deserialization.DeserializeObject<StoryLink>(returnedStoryLinksProperty.Value.ToString());

            // Verify that the returned story link is identical to the sent story link
            AssertThatOriginalAndReturnedStoryLinksAreIdentical(originalStoryLink, returnedStoryLink);

            // Publish the process so teardown can properly delete the process
            _storyteller.PublishProcess(_user, modifiedReturnedProcess);
        }

        [TestCase]
        [Description("Delete an existing story link in a user task and execute UpdateProcess().  Verify that" +
                     "the returned process maintains the original story link since story links can only be " +
                     "modified by GenerateUserStories().")]
        public void DeleteStorylinkFromUserTask_VerifyReturnedProcessHasStoryLink()
        {
            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Publish process; enable recursive delete flag
            _storyteller.PublishProcess(_user, returnedProcess);
            //deleteChildren = true;

            // Generate user stories for process
            _storyteller.GenerateUserStories(_user, returnedProcess);

            // Get default process again with updated story link
            returnedProcess = _storyteller.GetProcess(_user, returnedProcess.Id);

            // Get original story link
            var originalStoryLinksProperty =
                returnedProcess.GetProcessShapeByShapeName(Process.DefaultUserTaskName).PropertyValues[STORYLINKSKEY];
            var originalStoryLink = Deserialization.DeserializeObject<StoryLink>(originalStoryLinksProperty.Value.ToString());

            // Delete the story link for the default user task
            returnedProcess.GetProcessShapeByShapeName(Process.DefaultUserTaskName).PropertyValues[STORYLINKSKEY].Value = null;

            // Update the process using UpdateProcess in attempt to delete the story link
            var modifiedReturnedProcess = _storyteller.UpdateProcess(_user, returnedProcess);

            Assert.IsNotNull(modifiedReturnedProcess, "The process returned from UpdateProcess() was null.");

            // Get returned story link
            var returnedStoryLinksProperty =
                modifiedReturnedProcess.GetProcessShapeByShapeName(Process.DefaultUserTaskName).PropertyValues[STORYLINKSKEY];
            var returnedStoryLink = Deserialization.DeserializeObject<StoryLink>(returnedStoryLinksProperty.Value.ToString());

            // Verify that the returned story link is identical to the sent story link
            AssertThatOriginalAndReturnedStoryLinksAreIdentical(originalStoryLink, returnedStoryLink);

            // Publish the process  so teardown can properly delete the process
            _storyteller.PublishProcess(_user, modifiedReturnedProcess);
        }

        #endregion Tests

        /// <summary>
        /// Asserts that the Original and Returned Story Links are Identical
        /// </summary>
        /// <param name="originalStoryLink">The story link sent via the UpdateProcess method</param>
        /// <param name="returnedStoryLink">The story link returned from the UpdateProcess method</param>
        private static void AssertThatOriginalAndReturnedStoryLinksAreIdentical(StoryLink originalStoryLink, StoryLink returnedStoryLink)
        {
            Assert.IsNotNull(returnedStoryLink, "The returned story link was null but should not have been null");
            Assert.AreEqual(originalStoryLink.AssociatedReferenceArtifactId, returnedStoryLink.AssociatedReferenceArtifactId, "Link associated reference artifact ids do not match");
            Assert.AreEqual(originalStoryLink.DestinationId, returnedStoryLink.DestinationId, "Link destinations do not match");
            Assert.AreEqual(originalStoryLink.SourceId, returnedStoryLink.SourceId, "Link sources do not match");
            Assert.AreEqual(originalStoryLink.Orderindex, returnedStoryLink.Orderindex, "Link order indexes do not match");
        }
    }
}
