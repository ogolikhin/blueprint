﻿using Common;
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
using System.Collections.Generic;
using System.Linq;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class PersonaReferenceTests : TestBase
    {
        private IUser _adminUser;
        private IUser _authorFullAccess;
        private IProject _projectCustomData;
        private IProject _project;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            Helper = new TestHelper();

            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_adminUser);
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

        #region 200 OK Tests

        [TestCase(Process.DefaultUserTaskName)]
        [TestCase(Process.DefaultSystemTaskName)]
        [TestRail(227359)]
        [Description("Add a persona reference to a Process artifact task. Move the persona reference into folder. Verify that no change on persona referance after the move.")]
        public void PersonaReference_MoveReferenceInTask_VerifyNoChangeInPersonaReference(string taskName)
        {
            // Setup: Create and the default process and update with the added persona reference
            var addedProcessArtifact = Helper.Storyteller.CreateAndSaveProcessArtifact(_project, _authorFullAccess);
            
            var process = Helper.Storyteller.GetProcess(_authorFullAccess, addedProcessArtifact.Id);

            var addedPersonaReference = AddPersonaReferenceToTask(taskName, process, _authorFullAccess, _project);

            var folderArtifact = Helper.CreateAndPublishArtifact(_project, _authorFullAccess, BaseArtifactType.PrimitiveFolder);

            // Execution: Move the persona referece and update the process
            Helper.ArtifactStore.MoveArtifact(addedProcessArtifact, folderArtifact, _authorFullAccess);

            var savedProcess = StorytellerTestHelper.UpdateAndVerifyProcess(process, Helper.Storyteller, _authorFullAccess);

            var savedPersonaReference = GetPersonaReferenceFromTask(taskName, savedProcess);

            // Validation: Verify that there is no change in personaReference before and after the Move
            StorytellerTestHelper.AssertArtifactReferencesAreEqual(addedPersonaReference, savedPersonaReference);

            AssertPersonaReferenceEqualsPersonaPropertyForTaskWithinProcess(taskName, savedProcess);
        }

        [TestCase(Process.DefaultUserTaskName)]
        [TestCase(Process.DefaultSystemTaskName)]
        [TestRail(227360)]
        [Description("Add a persona reference from different project to a Process artifact task. Verify that no change on persona reference after the update.")]
        public void PersonaReference_AddReferenceFromDifferentProjectToTask_VerifyNoChangeInPersonaReference(string taskName)
        {
            // Setup: Create a default process and update with the added persona reference from differnt project
            var addedProcessArtifact = Helper.Storyteller.CreateAndSaveProcessArtifact(_project, _adminUser);

            var process= Helper.Storyteller.GetProcess(_adminUser, addedProcessArtifact.Id);

            var addedPersonaReferenceFromDifferentProject = AddPersonaReferenceToTask(taskName, process, _adminUser, _projectCustomData);

            // Execution: update the process with addedPersonaReference from different project
            var savedProcess = StorytellerTestHelper.UpdateAndVerifyProcess(process, Helper.Storyteller, _adminUser);

            var savedPersonaReference = GetPersonaReferenceFromTask(taskName, savedProcess);

            // Validation: Verify that there is no change in personaReference before and after the process update
            StorytellerTestHelper.AssertArtifactReferencesAreEqual(addedPersonaReferenceFromDifferentProject, savedPersonaReference);

            AssertPersonaReferenceEqualsPersonaPropertyForTaskWithinProcess(taskName, savedProcess);
        }

        #endregion 200 OK Tests

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

            AssertPersonaReferenceEqualsPersonaPropertyForTaskWithinProcess(taskName, savedProcess);
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

            AssertPersonaReferenceEqualsPersonaPropertyForTaskWithinProcess(taskName, publishedProcess);

            var defaultPersonaReference = DeletePersonaReferenceFromTask(taskName, publishedProcess);

            // Execute & Verify:
            var savedProcess = StorytellerTestHelper.UpdateAndVerifyProcess(publishedProcess, Helper.Storyteller, _authorFullAccess);

            var savedPersonaReference = GetPersonaReferenceFromTask(taskName, savedProcess);

            StorytellerTestHelper.AssertArtifactReferencesAreEqual(defaultPersonaReference, savedPersonaReference);

            AssertPersonaReferenceEqualsPersonaPropertyForTaskWithinProcess(taskName, savedProcess);
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

            AssertPersonaReferenceEqualsPersonaPropertyForTaskWithinProcess(taskName, publishedProcess);

            // Changes the persona reference to a new artifact reference
            var changedPersonaReference = AddPersonaReferenceToTask(taskName, publishedProcess, _authorFullAccess, _project);

            // Execute & Verify:
            var savedProcess = StorytellerTestHelper.UpdateAndVerifyProcess(publishedProcess, Helper.Storyteller, _authorFullAccess);

            var savedPersonaReference = GetPersonaReferenceFromTask(taskName, savedProcess);

            StorytellerTestHelper.AssertArtifactReferencesAreEqual(changedPersonaReference, savedPersonaReference);

            AssertPersonaReferenceEqualsPersonaPropertyForTaskWithinProcess(taskName, savedProcess);
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
            var publishedProcess = StorytellerTestHelper.UpdateVerifyAndPublishProcess(process, Helper.Storyteller, _authorFullAccess);

            AssertPersonaReferenceEqualsPersonaPropertyForTaskWithinProcess(taskName, publishedProcess);

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

            AssertPersonaReferenceEqualsPersonaPropertyForTaskWithinProcess(taskName, updatedProcess);
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

        [TestCase(Process.DefaultUserTaskName)]
        [TestCase(Process.DefaultSystemTaskName)]
        [TestRail(195627)]
        [Description("Persona reference added to task.  User story is generated.  Verify persona name added properly")]
        public void PersonaReference_UserStoryGenerated_VerifyPersonaNameInProperty(string taskName)
        {
            // Setup:
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _project, _authorFullAccess);

            var addedPersonaReference = AddPersonaReferenceToTask(taskName, process, _authorFullAccess, _project);
            Assert.IsNotNull(addedPersonaReference, "Persona was not added to task!");

            var publishedProcess = StorytellerTestHelper.UpdateVerifyAndPublishProcess(process, Helper.Storyteller, _authorFullAccess);
            Assert.IsNotNull(publishedProcess, "There was a problem in process verification!");

            // Execute:
            List<IStorytellerUserStory> userStories = Helper.Storyteller.GenerateUserStories(_authorFullAccess, process);

            // Verify:
            Assert.IsNotNull(userStories, "There is no user story generated!");

            StorytellerProperty returnedProperty = userStories.First().CustomProperties.Find(p => p.Name == "ST-Acceptance Criteria");

            string expectedPersonaName = null;

            if (taskName == Process.DefaultUserTaskName)
            {
                expectedPersonaName = I18NHelper.FormatInvariant("Given the System is Precondition When {0} attempts to {1} Then the System will be {2}", addedPersonaReference.Name,
                    Process.DefaultUserTaskName, Process.DefaultSystemTaskName);
            }
            else
            {
                expectedPersonaName = I18NHelper.FormatInvariant("Given the System is Precondition When User attempts to {0} Then the {1} will be {2}", Process.DefaultUserTaskName,
                    addedPersonaReference.Name, Process.DefaultSystemTaskName);
            }

            Assert.AreEqual(expectedPersonaName, ConvertHtmlToText(returnedProperty.Value), "Generated user story does not have " + addedPersonaReference.Name + "persona!");
        }

        [TestCase(Process.DefaultUserTaskName)]
        [TestCase(Process.DefaultSystemTaskName)]
        [TestRail(195628)]
        [Description("Persona reference added to task.  Actor artifact is removed.  User story is generated.  Verify default name added properly")]
        public void PersonaReference_ActorArtifactDeleted_UserStoryGenerated_VerifyDefaultNameInProperty(string taskName)
        {
            // Setup:
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _project, _authorFullAccess);

            var addedPersonaReference = AddPersonaReferenceToTask(taskName, process, _authorFullAccess, _project);
            Assert.IsNotNull(addedPersonaReference, "Persona was not added to task!");

            var publishedProcess = StorytellerTestHelper.UpdateVerifyAndPublishProcess(process, Helper.Storyteller, _authorFullAccess);
            Assert.IsNotNull(publishedProcess, "There was a problem in process verification!");

            var actor = Helper.Artifacts.Find(a => a.Id == addedPersonaReference.Id);
            Assert.IsNotNull(actor, "Cannot find actor artifact!");

            actor.Delete(_authorFullAccess);

            // Execute:
            List<IStorytellerUserStory> userStories = Helper.Storyteller.GenerateUserStories(_authorFullAccess, process);

            // Verify:
            Assert.IsNotNull(userStories, "There is no user story generated!");

            StorytellerProperty returnedProperty = userStories.First().CustomProperties.Find(p => p.Name == "ST-Acceptance Criteria");
            var expectedUserStoryText = I18NHelper.FormatInvariant("Given the System is Precondition When User attempts to {0} Then the System will be {1}",
                Process.DefaultUserTaskName, Process.DefaultSystemTaskName);
            Assert.AreEqual(expectedUserStoryText, ConvertHtmlToText(returnedProperty.Value), "Generated user story does not have default persona names!");
        }

        [TestCase(Process.DefaultUserTaskName)]
        [TestCase(Process.DefaultSystemTaskName)]
        [TestRail(195629)]
        [Description("Persona reference added to task.  Another user does not have permissions to actor.  User story is generated.  Verify default name added properly")]
        public void PersonaReference_NoPermissionsToArtifact_UserStoryGenerated_VerifyDefaultNameInProperty(string taskName)
        {
            // Setup:
            const string INACCESSIBLE_ACTOR = "Inaccessible Actor";

            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _project, _authorFullAccess);

            var addedPersonaReference = AddPersonaReferenceToTask(taskName, process, _authorFullAccess, _project);
            Assert.IsNotNull(addedPersonaReference, "Persona was not added to task!");

            var publishedProcess = StorytellerTestHelper.UpdateVerifyAndPublishProcess(process, Helper.Storyteller, _authorFullAccess);
            Assert.IsNotNull(publishedProcess, "There was a problem in process verification!");

            var actor = Helper.Artifacts.Find(a => a.Id == addedPersonaReference.Id);
            Assert.IsNotNull(actor, "Cannot find actor artifact!");

            var userWithoutPermissions = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
            Helper.AssignProjectRolePermissionsToUser(userWithoutPermissions, TestHelper.ProjectRole.None, _project, actor);

            // Execute:
            List<IStorytellerUserStory> userStories = Helper.Storyteller.GenerateUserStories(userWithoutPermissions, process);

            // Verify:
            Assert.IsNotNull(userStories, "There is no user story generated!");

            StorytellerProperty returnedProperty = userStories.First().CustomProperties.Find(p => p.Name == "ST-Acceptance Criteria");

            string expectedPersonaName = null;

            if (taskName == Process.DefaultUserTaskName)
            {
                expectedPersonaName = I18NHelper.FormatInvariant("Given the System is Precondition When {0} attempts to {1} Then the System will be {2}",
                    INACCESSIBLE_ACTOR, Process.DefaultUserTaskName, Process.DefaultSystemTaskName);
            }
            else
            {
                expectedPersonaName = I18NHelper.FormatInvariant("Given the System is Precondition When User attempts to {0} Then the {1} will be {2}",
                    Process.DefaultUserTaskName, INACCESSIBLE_ACTOR, Process.DefaultSystemTaskName);
            }

            Assert.AreEqual(expectedPersonaName, ConvertHtmlToText(returnedProperty.Value), "Generated user story does not have " + INACCESSIBLE_ACTOR + " persona!");
        }

        [Explicit(IgnoreReasons.FlakyTest)]  //This test changes parent process version
        [Category(Categories.CustomData)]
        [Category(Categories.GoldenData)]
        [TestCase]
        [TestRail(195630)]
        [Description("User story is generated from process created in Storyteller 1.  Verify task names added properly")]
        public void PersonaReference_StoryTeller1Process_UserStoryGenerated_VerifyNamesInProperty()
        {
            // Setup:
            const int STORYTELLER1_PROCESS_ID = 34;

            IProject projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_adminUser);

            var process = Helper.Storyteller.GetProcess(_adminUser, STORYTELLER1_PROCESS_ID);

            List<IStorytellerUserStory> userStories = null;
            try
            {
                // Execute:
               userStories = Helper.Storyteller.GenerateUserStories(_adminUser, process, shouldDeleteChildren: false);

                // Verify:
                Assert.IsNotNull(userStories, "There is no user story generated!");

                StorytellerProperty returnedProperty = userStories.First().CustomProperties.Find(p => p.Name == "ST-Acceptance Criteria");

                var expectedUserStoryText = I18NHelper.FormatInvariant("Given the System is Precondition When User attempts to {0} Then the System will be {1}",
                Process.DefaultUserTaskName, Process.DefaultSystemTaskName);
                Assert.AreEqual(expectedUserStoryText, ConvertHtmlToText(returnedProperty.Value),
                    "Generated user story does not have default persona names!");
            }
            finally
            {
                // Don't use Helper because this isn't a real artifact, it's just wrapping the artifact ID.
                var fakeArtifact = ArtifactFactory.CreateArtifact(projectCustomData, _adminUser, BaseArtifactType.Actor, userStories.First().Id);
                fakeArtifact.Delete(_adminUser);
                fakeArtifact.Publish(_adminUser);
            }
        }

        #endregion Tests

        #region Private Methods

        /// <summary>
        /// This function removes tags and other symbols
        /// </summary>
        /// <param name="htmlCode"></param>
        /// <returns>Plain text</returns>
        public static string ConvertHtmlToText(string htmlCode)
        {
            string str = System.Text.RegularExpressions.Regex.Replace(
              htmlCode, "<[^>]*>|\n|\t|&nbsp;", "");

            str = System.Text.RegularExpressions.Regex.Replace(
              str, "\r", " ");

            return str.Trim();
        }

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
        private static void AssertPersonaReferenceEqualsPersonaPropertyForTaskWithinProcess(string taskName, IProcess savedProcess)
        {
            var task = savedProcess.GetProcessShapeByShapeName(taskName);
            var personaPropertyValue = task.PropertyValues[Model.StorytellerModel.PropertyTypePredefined.Persona.ToString().LowerCaseFirstCharacter()].Value.ToString();

            Assert.AreEqual(task.PersonaReference.Name, personaPropertyValue,
                "The persona reference name and persona property value should be the same " +
                "but the persona reference name is {0} and the persona property vaue is {1}!", task.PersonaReference.Name,
                personaPropertyValue);
        }

        #endregion
    }
}
