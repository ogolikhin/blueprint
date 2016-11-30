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
using Utilities;
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
        [TestRail(195416)]
        [Description("Add a persona reference to a Process artifact task. Verify the persona reference was added.")]
        public void PersonaReference_AddReferenceToTask_VerifyPersonaAdded(string taskName)
        {
            // Setup:
            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _project, _authorFullAccess);

            var addedPersonaReference = AddPersonaReferenceToTask(taskName, process, _authorFullAccess, _project);

            // Execute & Verify:
            var savedProcess = StorytellerTestHelper.UpdateAndVerifyProcess(process, Helper.Storyteller, _authorFullAccess);

            var savedPersonaReference = GetPersonaReferenceFromTask(taskName, savedProcess);

            StorytellerTestHelper.AssertArtifactReferencesAreEqual(addedPersonaReference, savedPersonaReference);

            AssertPersonaReferenceEqualsPersonaProperty(taskName, savedProcess);
        }

        [TestCase(Process.DefaultUserTaskName)]
        [TestCase(Process.DefaultSystemTaskName)]
        [TestRail(195417)]
        [Description("Delete a persona reference from a Process artifact task. Verify the persona reference was deleted.")]
        public void PersonaReference_DeleteReferenceFromTask_VerifyPersonaDeleted(string taskName)
        {
            // Setup:
            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _project, _authorFullAccess);

            var addedPersonaReference = AddPersonaReferenceToTask(taskName, process, _authorFullAccess, _project);

            // Publish Process with added persona reference
            var publishedProcess = StorytellerTestHelper.UpdateVerifyAndPublishProcess(process, Helper.Storyteller, _authorFullAccess);

            var publishedPersonaReference = GetPersonaReferenceFromTask(taskName, publishedProcess);

            StorytellerTestHelper.AssertArtifactReferencesAreEqual(addedPersonaReference, publishedPersonaReference);

            AssertPersonaReferenceEqualsPersonaProperty(taskName, publishedProcess);

            var defaultPersonaReference = DeletePersonaReferenceFromTask(taskName, publishedProcess);

            // Execute & Verify:
            var savedProcess = StorytellerTestHelper.UpdateAndVerifyProcess(publishedProcess, Helper.Storyteller, _authorFullAccess);

            var savedPersonaReference = GetPersonaReferenceFromTask(taskName, savedProcess);

            StorytellerTestHelper.AssertArtifactReferencesAreEqual(defaultPersonaReference, savedPersonaReference);

            AssertPersonaReferenceEqualsPersonaProperty(taskName, savedProcess);
        }

        [TestCase(Process.DefaultUserTaskName)]
        [TestCase(Process.DefaultSystemTaskName)]
        [TestRail(195418)]
        [Description("Change a persona reference from a Process artifact task. Verify the persona reference was changed.")]
        public void PersonaReference_ChangeReferenceInTask_VerifyPersonaChange(string taskName)
        {
            // Setup:
            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _project, _authorFullAccess);

            var addedPersonaReference = AddPersonaReferenceToTask(taskName, process, _authorFullAccess, _project);

            // Publish Process with added persona reference
            var publishedProcess = StorytellerTestHelper.UpdateVerifyAndPublishProcess(process, Helper.Storyteller, _authorFullAccess);

            var publishedPersonaReference = GetPersonaReferenceFromTask(taskName, publishedProcess);

            StorytellerTestHelper.AssertArtifactReferencesAreEqual(addedPersonaReference, publishedPersonaReference);

            AssertPersonaReferenceEqualsPersonaProperty(taskName, publishedProcess);

            // Changes the persona reference to a new artifact reference
            var changedPersonaReference = AddPersonaReferenceToTask(taskName, publishedProcess, _authorFullAccess, _project);

            // Execute & Verify:
            var savedProcess = StorytellerTestHelper.UpdateAndVerifyProcess(publishedProcess, Helper.Storyteller, _authorFullAccess);

            var savedPersonaReference = GetPersonaReferenceFromTask(taskName, savedProcess);

            StorytellerTestHelper.AssertArtifactReferencesAreEqual(changedPersonaReference, savedPersonaReference);

            AssertPersonaReferenceEqualsPersonaProperty(taskName, savedProcess);
        }

        [TestCase(Process.DefaultUserTaskName)]
        [TestCase(Process.DefaultSystemTaskName)]
        [TestRail(195419)]
        [Description("Change the name of a persona reference by changing the artifact name. Verify the persona reference name was changed.")]
        public void PersonaReference_ChangeReferenceActorNameInTask_VerifyPersonaNameChange(string taskName)
        {
            // Setup:
            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _project, _authorFullAccess);

            var personaReference = AddPersonaReferenceToTask(taskName, process, _authorFullAccess, _project);

            // Publish Process with added persona reference
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(process, Helper.Storyteller, _authorFullAccess);

            AssertPersonaReferenceEqualsPersonaProperty(taskName, process);

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
            var task = updatedProcess.GetProcessShapeByShapeName(taskName);
            var updatedPersonaReferenceName = task.PersonaReference.Name;

            Assert.AreEqual(updatedPersonaReferenceName, actorArtifactDetails.Name, "The persona reference name was {0} but {1} was expected!",
                updatedPersonaReferenceName, actorArtifactDetails.Name);

            AssertPersonaReferenceEqualsPersonaProperty(taskName, updatedProcess);
        }

        [TestCase(Process.DefaultUserTaskName)]
        [TestCase(Process.DefaultSystemTaskName)]
        [TestRail(195421)]
        [Description("Add a persona reference to a Process artifact task. Verify a relationship to the actor was added to the Process.")]
        public void PersonaReference_AddReferenceToTask_VerifyRelationshipAdded(string taskName)
        {
            // Setup:
            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _project, _authorFullAccess);

            // Get persona reference and persona actor artifact
            var personaReference = AddPersonaReferenceToTask(taskName, process, _authorFullAccess, _project);
            IArtifact personaActorArtifact = Helper.Storyteller.Artifacts.Find(a => a.Id == personaReference.Id);

            //Execute:
            // Update process and publish
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(process, Helper.Storyteller, _authorFullAccess);

            var personaRelationship = GetPersonaRelationship(taskName, process, personaReference);

            // Verify:
            StorytellerTestHelper.AssertPersonaReferenceRelationshipIsCorrect(personaRelationship, _project, personaActorArtifact);
        }

        [TestCase(Process.DefaultUserTaskName)]
        [TestCase(Process.DefaultSystemTaskName)]
        [TestRail(195422)]
        [Description("Delete a persona reference from a Process artifact task. Verify the relationship to the actor was deleted from the Process.")]
        public void PersonaReference_DeleteReferenceFromTask_VerifyRelationshipRemoved(string taskName)
        {
            // Setup:
            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _project, _authorFullAccess);

            // Get persona reference and persona actor artifact
            var personaReference = AddPersonaReferenceToTask(taskName, process, _authorFullAccess, _project);
            IArtifact personaActorArtifact = Helper.Storyteller.Artifacts.Find(a => a.Id == personaReference.Id);

            // Update process and publish
            var updatedProcess = StorytellerTestHelper.UpdateVerifyAndPublishProcess(process, Helper.Storyteller, _authorFullAccess);

            var personaRelationship = GetPersonaRelationship(taskName, process, personaReference);

            StorytellerTestHelper.AssertPersonaReferenceRelationshipIsCorrect(personaRelationship, _project, personaActorArtifact);

            DeletePersonaReferenceFromTask(taskName, updatedProcess);

            // Execute & Verify:
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(updatedProcess, Helper.Storyteller, _authorFullAccess);

            var updatedPersonaRelationship = GetPersonaRelationship(taskName, process, personaReference);

            Assert.IsNull(updatedPersonaRelationship, "There should no longer be a persona relationship, but one exists!");
        }

        #endregion Tests

        #region Private Methods

        /// <summary>
        /// Gets a persona reference from a task
        /// </summary>
        /// <param name="taskName">The name of the task that contains the persona reference.</param>
        /// <param name="process">The process containing the task </param>
        /// <returns>The persona reference</returns>
        private static ArtifactReference GetPersonaReferenceFromTask(string taskName, IProcess process)
        {
            var task = process.GetProcessShapeByShapeName(taskName);

            return task.PersonaReference;
        }

        /// <summary>
        /// Creates an actor artifact and adds it to a task as a persona reference
        /// </summary>
        /// <param name="taskName">The name of the task that will contain the persona reference.</param>
        /// <param name="process">The process containing the task </param>
        /// <param name="user">The user that will create the actor artifact.</param>
        /// <param name="project">The project containing the actor artifact.</param>
        /// <returns>The created persona reference</returns>
        private ArtifactReference AddPersonaReferenceToTask(string taskName, IProcess process, IUser user, IProject project)
        {
            // Create actor for persona reference
            var actor = Helper.CreateAndPublishArtifact(project, user, BaseArtifactType.Actor);

            Helper.Storyteller.Artifacts.Add(actor);

            var actorDetails = Helper.ArtifactStore.GetArtifactDetails(user, actor.Id);

            // Add persona reference to default user task
            var task = process.GetProcessShapeByShapeName(taskName);

            return task.AddPersonaReference(actorDetails);
        }

        /// <summary>
        /// Deletes a persona reference from a task
        /// </summary>
        /// <param name="taskName">The name of the task that contains the persona reference.</param>
        /// <param name="process">The process containing the task </param>
        /// <returns>The default persona reference</returns>
        private static ArtifactReference DeletePersonaReferenceFromTask(string taskName, IProcess process)
        {
            var task = process.GetProcessShapeByShapeName(taskName);

            var processShapeType = (ProcessShapeType)task.PropertyValues["clientType"].Value.ToInt32Invariant();

            // Deletes persona reference by replacing it with the default persona reference
            return task.AddDefaultPersonaReference(processShapeType);
        }

        /// <summary>
        /// Gets the persona relationship for the process task
        /// </summary>
        /// <param name="taskName">The name of the task containing the persona relationship.</param>
        /// <param name="process">The process containing the task.</param>
        /// <param name="personaReference">The persona reference contained in the task</param>
        /// <returns>The persona relationship that has been added to the task.</returns>
        private Relationship GetPersonaRelationship(string taskName, IProcess process, ArtifactReference personaReference)
        {
            ThrowIf.ArgumentNull(personaReference, nameof(personaReference));

            // Get task relationships
            IArtifact processArtifact = Helper.Storyteller.Artifacts.Find(a => a.Id == process.Id);
            var task = process.GetProcessShapeByShapeName(taskName);
            var taskRelationships = Helper.ArtifactStore.GetRelationships(_authorFullAccess, processArtifact, task.Id);

            // Return the persona relationship
            return taskRelationships.OtherTraces.Find(ot => ot.ArtifactId == personaReference.Id);
        }

        /// <summary>
        /// Assert that the persona reference name equals the persona property value
        /// </summary>
        /// <param name="taskName">The name of the task containing the persona reference.</param>
        /// <param name="savedProcess">The process containing the task.</param>
        private static void AssertPersonaReferenceEqualsPersonaProperty(string taskName, IProcess savedProcess)
        {
            var task = savedProcess.GetProcessShapeByShapeName(taskName);
            var personaPropertyValue = task.PropertyValues[Model.StorytellerModel.PropertyTypePredefined.Persona.ToString()].Value.ToString();

            Assert.AreEqual(task.PersonaReference.Name, personaPropertyValue,
                "The persona reference name and persona property value should be the same " +
                "but the persona reference name is {0} and the persona property vaue is {1}!", task.PersonaReference.Name,
                personaPropertyValue);
        }

        #endregion
    }
}
