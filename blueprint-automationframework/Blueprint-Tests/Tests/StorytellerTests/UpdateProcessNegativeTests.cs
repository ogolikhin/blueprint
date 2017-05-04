using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel.Enums;
using Model.Factories;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using TestCommon;
using Utilities;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class UpdateProcessNegativeTests : TestBase
    {
        private const string STORYLINKSKEY = "storyLinks";

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
        [Description("Add a story link to a user task without a story link and execute UpdateProcess().  Verify that" +
                     "the returned process does not include the story link since story links can only be created" +
                     "by GenerateUserStories().")]
        public void AddStoryLinkToUserTaskWithoutStoryLink_UpdateProcess_VerifyReturnedProcessDoesNotHaveStoryLink()
        {
            // Create and get the default process
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Get default user task
            var defaultUserTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Create and publish textual requirement artifact to simulate user story artifact
            //var addedArtifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.TextualRequirement);
            var addedArtifact = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.TextualRequirement);

            // Add a story link to the default user task
            var storyLink = new StoryLink(defaultUserTask.Id, addedArtifact.Id, 1, addedArtifact.Id);
            defaultUserTask.PropertyValues[STORYLINKSKEY].Value = storyLink;

            // Update the process using UpdateProcess in attempt to add a story link
            var modifiedReturnedProcess = Helper.Storyteller.UpdateNovaProcess(_user, novaProcess.NovaProcess);

            Assert.IsNotNull(modifiedReturnedProcess, "The process returned from UpdateProcess() was null.");

            // Verify returned process does not contain a story link
            Assert.IsNull(modifiedReturnedProcess.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName).PropertyValues[STORYLINKSKEY].Value,
                "The story link was saved using UpdateProcess but should not have been saved");
        }

        [TestCase]
        [Description("Add a story link to a user task that already has an existing story link and execute UpdateProcess()." +
                     "Verify that the returned process only contains the single original story link since story links " +
                     "can only be added by GenerateUserStories().")]
        public void AddStoryLinkToUserTaskWithStoryLink_UpdateProcess_VerifyReturnedProcessDoesNotHaveStoryLink()
        {
            // Create and get the default process
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Publish process; enable recursive delete flag
            novaProcess.Publish(_user);
            //deleteChildren = true;

            // Generate user stories for process
            Helper.Storyteller.GenerateUserStories(_user, novaProcess.Process);

            // Get default process again with updated story link
            novaProcess.RefreshArtifactFromServer(_user);

            // Get default user task
            var defaultUserTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Get original story link
            var originalStoryLinksProperty = defaultUserTask.PropertyValues[STORYLINKSKEY];
            var originalStoryLink = SerializationUtilities.DeserializeObject<StoryLink>(originalStoryLinksProperty.Value.ToString());

            // Create and publish textual requirement artifact to simulate user story artifact
            var addedArtifact = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.TextualRequirement);

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
            novaProcess.Lock(_user);
            var modifiedReturnedProcess = Helper.Storyteller.UpdateProcess(_user, novaProcess.Process);

            Assert.IsNotNull(modifiedReturnedProcess, "The process returned from UpdateProcess() was null.");

            // Get returned story link
            var returnedStoryLinksProperty = modifiedReturnedProcess.GetProcessShapeByShapeName(Process.DefaultUserTaskName).PropertyValues[STORYLINKSKEY];
            var returnedStoryLink = SerializationUtilities.DeserializeObject<StoryLink>(returnedStoryLinksProperty.Value.ToString());

            // Verify that the returned story link is identical to the sent story link
            AssertThatOriginalAndReturnedStoryLinksAreIdentical(originalStoryLink, returnedStoryLink);
        }

        [TestCase]
        [Description("Delete an existing story link in a user task and execute UpdateProcess().  Verify that" +
                     "the returned process maintains the original story link since story links can only be " +
                     "modified by GenerateUserStories().")]
        public void DeleteStorylinkFromUserTask_VerifyReturnedProcessHasStoryLink()
        {
            // Create and get the default process
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Publish process; enable recursive delete flag
            novaProcess.Publish(_user);
            //deleteChildren = true;

            // Generate user stories for process
            Helper.Storyteller.GenerateUserStories(_user, novaProcess.Process);

            // Get default process again with updated story link
            novaProcess.RefreshArtifactFromServer(_user);

            // Get original story link
            var originalStoryLinksProperty =
                novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName).PropertyValues[STORYLINKSKEY];
            var originalStoryLink = SerializationUtilities.DeserializeObject<StoryLink>(originalStoryLinksProperty.Value.ToString());

            // Delete the story link for the default user task
            novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName).PropertyValues[STORYLINKSKEY].Value = null;

            // Update the process using UpdateProcess in attempt to delete the story link
            novaProcess.Lock(_user);
            var modifiedReturnedProcess = Helper.Storyteller.UpdateProcess(_user, novaProcess.Process);

            Assert.IsNotNull(modifiedReturnedProcess, "The process returned from UpdateProcess() was null.");

            // Get returned story link
            var returnedStoryLinksProperty =
                modifiedReturnedProcess.GetProcessShapeByShapeName(Process.DefaultUserTaskName).PropertyValues[STORYLINKSKEY];
            var returnedStoryLink = SerializationUtilities.DeserializeObject<StoryLink>(returnedStoryLinksProperty.Value.ToString());

            // Verify that the returned story link is identical to the sent story link
            AssertThatOriginalAndReturnedStoryLinksAreIdentical(originalStoryLink, returnedStoryLink);
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
