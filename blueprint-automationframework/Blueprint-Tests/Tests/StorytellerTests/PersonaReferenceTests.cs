using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.StorytellerModel;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using TestCommon;
using Utilities.Factories;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class PersonaReferenceTests : TestBase
    {
        private IUser _adminUser;
        private IUser _authorFullAccess;
        private IProject _project;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            Helper = new TestHelper();

            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_adminUser);
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _adminUser);
            _authorFullAccess = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region Tests

        [TestCase(Process.DefaultUserTaskName)]
        [TestCase(Process.DefaultSystemTaskName)]
        [Description("Add a persona reference to a Process artifact task. Verify the persona reference was added.")]
        public void PersonaReference_AddReferenceToTask_VerifyPersonaAdded(string taskName)
        {
            // Setup:
            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _project, _authorFullAccess);

            AddPersonaReferenceToTask(taskName, process, _authorFullAccess);

            // Execute & Verify:
            StorytellerTestHelper.UpdateAndVerifyProcess(process, Helper.Storyteller, _authorFullAccess);
        }

        [TestCase(Process.DefaultUserTaskName)]
        [TestCase(Process.DefaultSystemTaskName)]
        [Description("Delete a persona reference from a Process artifact task. Verify the persona reference was deleted.")]
        public void PersonaReference_DeleteReferenceFromTask_VerifyPersonaDeleted(string taskName)
        {
            // Setup:
            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _project, _authorFullAccess);

            AddPersonaReferenceToTask(taskName, process, _authorFullAccess);

            // Publish Process with added persona reference
            var publishedProcess = StorytellerTestHelper.UpdateAndVerifyProcess(process, Helper.Storyteller, _authorFullAccess);

            DeletePersonaReferenceFromTask(taskName, publishedProcess);

            // Execute & Verify:
            StorytellerTestHelper.UpdateAndVerifyProcess(publishedProcess, Helper.Storyteller, _authorFullAccess);
        }

        [TestCase(Process.DefaultUserTaskName)]
        [TestCase(Process.DefaultSystemTaskName)]
        [Description("Change a persona reference from a Process artifact task. Verify the persona reference was changed.")]
        public void PersonaReference_ChangeReferenceInTask_VerifyPersonaChange(string taskName)
        {
            // Setup:
            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _project, _authorFullAccess);

            AddPersonaReferenceToTask(taskName, process, _authorFullAccess);

            // Publish Process with added persona reference
            var publishedProcess = StorytellerTestHelper.UpdateAndVerifyProcess(process, Helper.Storyteller, _authorFullAccess);

            // Changes the persona reference to a new artifact reference
            AddPersonaReferenceToTask(taskName, publishedProcess, _authorFullAccess);

            // Execute & Verify:
            StorytellerTestHelper.UpdateAndVerifyProcess(publishedProcess, Helper.Storyteller, _authorFullAccess);
        }

        [TestCase(Process.DefaultUserTaskName)]
        [TestCase(Process.DefaultSystemTaskName)]
        [Description("Change the name of a persona reference by changing the artifact name. Verify the persona reference name was changed.")]
        public void PersonaReference_ChangeReferenceActorNameInTask_VerifyPersonaNameChange(string taskName)
        {
            // Setup:
            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _project, _authorFullAccess);

            var personaReference = AddPersonaReferenceToTask(taskName, process, _authorFullAccess);

            // Publish Process with added persona reference
            StorytellerTestHelper.UpdateAndVerifyProcess(process, Helper.Storyteller, _authorFullAccess);

            // Get the actor artifact from the persona reference
            var actorArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorFullAccess, personaReference.Id);

            // Change the actor artifact name
            actorArtifactDetails.Name = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);

            //Execute:
            // Publish actor with new name
            var actorArtifact = Helper.WrapNovaArtifact(actorArtifactDetails, _project, _authorFullAccess);
            actorArtifact.Lock(_authorFullAccess);
            Helper.UpdateNovaArtifact(_project, _authorFullAccess, actorArtifactDetails);

            // Verify:
            var updatedProcess = Helper.Storyteller.GetProcess(_authorFullAccess, process.Id);
            var defaultUserTask = updatedProcess.GetProcessShapeByShapeName(taskName);
            var updatedPersonaReferenceName = defaultUserTask.PersonaReference.Name;

            Assert.That(updatedPersonaReferenceName.Equals(actorArtifactDetails.Name), "The persona reference name was {0} but {1} was expected!",
                updatedPersonaReferenceName, actorArtifactDetails.Name);
        }

        #endregion Tests

        #region Private Methods

        /// <summary>
        /// Creates an actor artifact and adds it to a task as a persona reference
        /// </summary>
        /// <param name="taskName">The name of the task that will contain the persona reference.</param>
        /// <param name="process">The process containing the task </param>
        /// <param name="user">The user that will create the actor artifact.</param>
        /// <returns>The created persona reference</returns>
        private ArtifactReference AddPersonaReferenceToTask(string taskName, IProcess process, IUser user)
        {
            // Create actor for persona reference
            var actor = Helper.CreateAndPublishArtifact(_project, user, BaseArtifactType.Actor);

            var actorDetails = Helper.ArtifactStore.GetArtifactDetails(user, actor.Id);

            // Add persona reference to default user task
            var defaultUserTask = process.GetProcessShapeByShapeName(taskName);

            return defaultUserTask.AddPersonaReference(actorDetails);
        }

        /// <summary>
        /// Deletes a persona reference from a task
        /// </summary>
        /// <param name="taskName">The name of the task that contains the persona reference.</param>
        /// <param name="process">The process containing the task </param>
        private static void DeletePersonaReferenceFromTask(string taskName, IProcess process)
        {
            var defaultUserTask = process.GetProcessShapeByShapeName(taskName);

            var processShapeType = (ProcessShapeType)defaultUserTask.PropertyValues["clientType"].Value.ToInt32Invariant();

            defaultUserTask.AddDefaultPersonaReference(processShapeType);
        }

        #endregion
    }
}
