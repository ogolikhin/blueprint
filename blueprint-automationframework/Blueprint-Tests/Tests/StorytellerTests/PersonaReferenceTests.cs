using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.StorytellerModel;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Model.StorytellerModel.Enums;
using TestCommon;
using Utilities;
using Utilities.Factories;
using Model.Common.Enums;

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

        #region 200 OK Tests

        [TestCase(Process.DefaultUserTaskName)]
        [TestCase(Process.DefaultSystemTaskName)]
        [TestRail(227359)]
        [Description("Add a persona reference to a Process task. Move the persona reference into folder. Verify that no change on persona referance after the move.")]
        public void PersonaReference_MoveReferenceInTask_VerifyNoChangeInPersonaReference(string taskName)
        {
            // Setup: Create a default process and update with the added persona reference
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _project, _authorFullAccess);
            var addedPersonaReference = AddPersonaReferenceToTask(taskName, process, _authorFullAccess, _project);
            var personaReferenceArtifact = new ArtifactBase {Id = addedPersonaReference.Id, ProjectId = addedPersonaReference.ProjectId};

            // Execution: Move the persona reference to a new folder and update the process
            var folderArtifact = Helper.CreateAndPublishNovaArtifact(_authorFullAccess, _project, ItemTypePredefined.PrimitiveFolder);
            Helper.SvcShared.LockArtifact(_authorFullAccess, personaReferenceArtifact);
            Helper.ArtifactStore.MoveArtifact(_authorFullAccess, personaReferenceArtifact.Id, folderArtifact.Id);

            // Update and save the process; Get the persona reference from the saved process
            var savedProcess = StorytellerTestHelper.UpdateAndVerifyProcess(process, Helper.Storyteller, _authorFullAccess);
            var savedPersonaReference = GetPersonaReferenceFromTask(taskName, savedProcess);

            // Validation: Verify that there is no change in personaReference before and after the Move
            ArtifactReference.AssertAreEqual(addedPersonaReference, savedPersonaReference);

            AssertPersonaReferenceEqualsPersonaPropertyForTaskWithinProcess(taskName, savedProcess);
        }

        [TestCase(Process.DefaultUserTaskName)]
        [TestCase(Process.DefaultSystemTaskName)]
        [TestRail(227360)]
        [Description("Add a persona reference from different project to a Process task. Verify that no change on persona reference after the update.")]
        public void PersonaReference_AddReferenceFromDifferentProjectToTask_VerifyNoChangeInPersonaReference(string taskName)
        {
            // Setup: Create a default process and update with the added persona reference from different project
            var projects = ProjectFactory.GetProjects(_adminUser, numberOfProjects: 2);
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, projects[0], _adminUser);
            var addedPersonaReferenceFromDifferentProject = AddPersonaReferenceToTask(taskName, process, _adminUser, projects[1]);

            // Execution: update the process with addedPersonaReference from different project
            var savedProcess = StorytellerTestHelper.UpdateAndVerifyProcess(process, Helper.Storyteller, _adminUser);
            var savedPersonaReference = GetPersonaReferenceFromTask(taskName, savedProcess);

            // Validation: Verify that there is no change in personaReference before and after the process update
            ArtifactReference.AssertAreEqual(addedPersonaReferenceFromDifferentProject, savedPersonaReference);

            AssertPersonaReferenceEqualsPersonaPropertyForTaskWithinProcess(taskName, savedProcess);
        }

        [TestCase(Process.DefaultUserTaskName)]
        [TestCase(Process.DefaultSystemTaskName)]
        [TestRail(230660)]
        [Description("Update a default process with an inaccessible persona reference. Verify that default persona reference is used after the update.")]
        public void PersonaReference_UpdateDefaultProcessWithInaccessibleReference_VerifyDefaultPersonaReference(string taskName)
        {
            // Setup: Create a default process and add an inaccessible persona reference 
            var userWithoutPermissionToPersonaReference = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _project, userWithoutPermissionToPersonaReference);
            var defaultPersonaReference = GetPersonaReferenceFromTask(taskName, process);
            var addedPersonaReference = AddPersonaReferenceToTask(taskName, process, _authorFullAccess, _project);
            var personaReferenceArtifact = new ArtifactBase { Id = addedPersonaReference.Id, ProjectId = addedPersonaReference.ProjectId };

            Helper.AssignProjectRolePermissionsToUser(userWithoutPermissionToPersonaReference, TestHelper.ProjectRole.None, _project, personaReferenceArtifact);

            // Execute: Update the process with inaccessible persona reference
            IProcess savedProcess = null;
            Assert.DoesNotThrow(() => savedProcess = Helper.Storyteller.UpdateProcess(userWithoutPermissionToPersonaReference, process),
                "PATCH process call failed when using process whose Id is {0}!", process.Id);

            // Validation: Verify that persona reference from updated process is default persona
            var savedPersonaReference = GetPersonaReferenceFromTask(taskName, savedProcess);
            ArtifactReference.AssertAreEqual(defaultPersonaReference, savedPersonaReference);

            Assert.AreEqual(defaultPersonaReference.Name, savedPersonaReference.Name,
                "The persona reference name from savedProcess {0} should be the same as default persona reference {1}!",
                savedPersonaReference.Name, defaultPersonaReference.Name);
        }

        [TestCase(Process.DefaultUserTaskName)]
        [TestCase(Process.DefaultSystemTaskName)]
        [TestRail(230672)]
        [Description("Set up a process with inaccessible persona reference to a process task. Publish the process with user that doesn't" +
            "have a permission to the persona reference. Verify that retrieved persona refence with both users with and without permission to" + 
            "the persona reference on the process and make sure it's not updated from last published.")]
        public void PersonaReference_PublishProcessContainingInaccessibleReferenceForUser_VerifyPersonaReferenceWithPermissionBasedUsers(string taskName)
        {
            // Setup: Create and publish the process with a persona reference
            var userWithoutPermission= Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _project, _authorFullAccess);
            var addedPersonaReference = AddPersonaReferenceToTask(taskName, process, _authorFullAccess, _project);
            var personaReferenceArtifact = new ArtifactBase { Id = addedPersonaReference.Id, ProjectId = addedPersonaReference.ProjectId };

            Helper.AssignProjectRolePermissionsToUser(userWithoutPermission, TestHelper.ProjectRole.None, _project, personaReferenceArtifact);

            var publishedProcess = StorytellerTestHelper.UpdateVerifyAndPublishProcess(process, Helper.Storyteller, _authorFullAccess);
            var personaReferencBeforeUpdate = GetPersonaReferenceFromTask(taskName, publishedProcess);

            var anotherPersonaReference = AddPersonaReferenceToTask(taskName, publishedProcess, _authorFullAccess, _project);
            var anotherPersonaReferenceArtifact = new ArtifactBase { Id = anotherPersonaReference.Id, ProjectId = anotherPersonaReference.ProjectId};

            Helper.AssignProjectRolePermissionsToUser(userWithoutPermission, TestHelper.ProjectRole.None, _project, anotherPersonaReferenceArtifact);

            // Execute: update and publish process with user that doesn't have permission to the persona reference.
            Helper.Storyteller.UpdateProcess(userWithoutPermission, publishedProcess);

            Assert.DoesNotThrow(() => Helper.Storyteller.PublishProcess(userWithoutPermission, publishedProcess),
                "POST process call failed when using process whose Id is {0}!", publishedProcess.Id);

            // Validation: Get persona references with users with and without permission to the persona reference
            var novaProcessWithoutPermission = Helper.Storyteller.GetNovaProcess(userWithoutPermission, publishedProcess.Id);
            var personaReferenceWithoutPermission = GetPersonaReferenceFromTask(taskName, novaProcessWithoutPermission.Process);
            var novaProcessWithPermission = Helper.Storyteller.GetNovaProcess(_authorFullAccess, publishedProcess.Id);
            var personaReferenceWithPermission = GetPersonaReferenceFromTask(taskName, novaProcessWithPermission.Process);

            var inaccessiblePersonaReference = CreateInaccessiblePersonaReference(personaReferenceArtifact);

            // Validation: Verify that persona reference from the updated process using the users with and without permission to the persona reference
            ArtifactReference.AssertAreEqual(inaccessiblePersonaReference, personaReferenceWithoutPermission);
            ArtifactReference.AssertAreEqual(personaReferencBeforeUpdate, personaReferenceWithPermission);
        }

        #endregion 200 OK Tests

        #region 400 Bad Request Tests

        // TODO: update and enable this test cases once the TFS Bug 4823 gets updated
        [Explicit(IgnoreReasons.ProductBug)]
        [TestCase(Process.DefaultUserTaskName)]
        [TestCase(Process.DefaultSystemTaskName)]
        [TestRail(227361)]
        [Description("Add non-actor artifact as a persona reference to a process artifact task. Verify that 400 Bad Request is returned.")]
        public void PersonaReference_AddNonActorAsReferenceToTask_400BadRequest(string taskName)
        {
            // Setup: Create a process and add non-actor artifact as a persona reference to a process artifact task
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _project, _authorFullAccess);

            AddPersonaReferenceToTask(taskName, process, _authorFullAccess, _project, ItemTypePredefined.Document);

            // Execute: Update the process with non-actor artifact as a persona reference
            var ex = Assert.Throws<Http500InternalServerErrorException>(() => StorytellerTestHelper.UpdateAndVerifyProcess(process, Helper.Storyteller, _authorFullAccess),
                "Update process call should return 400 Bad Request when using with invalid non-actor persona reference!");

            // Validation: Verify that error code returned from the error response
            // TODO: update the expectedExceptionMessage and ValidateServiceError part after the bug is updated
            string expectedExceptionMessage = "";
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.InvalidCredentials, expectedExceptionMessage);
        }

        #endregion 400 Bad Request Tests

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

            ArtifactReference.AssertAreEqual(addedPersonaReference, savedPersonaReference);

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

            ArtifactReference.AssertAreEqual(addedPersonaReference, publishedPersonaReference);

            AssertPersonaReferenceEqualsPersonaPropertyForTaskWithinProcess(taskName, publishedProcess);

            var defaultPersonaReference = DeletePersonaReferenceFromTask(taskName, publishedProcess);

            // Execute & Verify:
            var savedProcess = StorytellerTestHelper.UpdateAndVerifyProcess(publishedProcess, Helper.Storyteller, _authorFullAccess);

            var savedPersonaReference = GetPersonaReferenceFromTask(taskName, savedProcess);

            ArtifactReference.AssertAreEqual(defaultPersonaReference, savedPersonaReference);

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

            ArtifactReference.AssertAreEqual(addedPersonaReference, publishedPersonaReference);

            AssertPersonaReferenceEqualsPersonaPropertyForTaskWithinProcess(taskName, publishedProcess);

            // Changes the persona reference to a new artifact reference
            var changedPersonaReference = AddPersonaReferenceToTask(taskName, publishedProcess, _authorFullAccess, _project);

            // Execute & Verify:
            var savedProcess = StorytellerTestHelper.UpdateAndVerifyProcess(publishedProcess, Helper.Storyteller, _authorFullAccess);

            var savedPersonaReference = GetPersonaReferenceFromTask(taskName, savedProcess);

            ArtifactReference.AssertAreEqual(changedPersonaReference, savedPersonaReference);

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
            Helper.SvcShared.LockArtifact(_authorFullAccess, actorArtifactDetails.Id);
            Helper.UpdateNovaArtifact(_authorFullAccess, actorArtifactDetails);

            // Verify:
            var updatedNovaProcess = Helper.Storyteller.GetNovaProcess(_authorFullAccess, process.Id);
            var updatedProcess = updatedNovaProcess.Process;
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
            var personaActorArtifact = Helper.ArtifactStore.GetArtifactDetails(_authorFullAccess, personaReference.Id);

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
            var personaActorArtifact = Helper.ArtifactStore.GetArtifactDetails(_authorFullAccess, personaReference.Id);

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
            var userStories = Helper.Storyteller.GenerateUserStories(_authorFullAccess, process);

            // Verify:
            Assert.IsNotNull(userStories, "There is no user story generated!");

            var returnedProperty = userStories.First().CustomProperties.Find(p => p.Name == "ST-Acceptance Criteria");

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

            // Delete Actor
            var actor = new ArtifactBase { Id = addedPersonaReference.Id, ProjectId = addedPersonaReference.ProjectId};
            Helper.ArtifactStore.DeleteArtifact(actor, _authorFullAccess);

            // Execute:
            var userStories = Helper.Storyteller.GenerateUserStories(_authorFullAccess, process);

            // Verify:
            Assert.IsNotNull(userStories, "There is no user story generated!");

            var returnedProperty = userStories.First().CustomProperties.Find(p => p.Name == "ST-Acceptance Criteria");
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

            var actor = new ArtifactBase { Id = addedPersonaReference.Id, ProjectId = addedPersonaReference.ProjectId};

            var userWithoutPermissions = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
            Helper.AssignProjectRolePermissionsToUser(userWithoutPermissions, TestHelper.ProjectRole.None, _project, actor);

            // Execute:
            var userStories = Helper.Storyteller.GenerateUserStories(userWithoutPermissions, process);

            // Verify:
            Assert.IsNotNull(userStories, "There is no user story generated!");

            var returnedProperty = userStories.First().CustomProperties.Find(p => p.Name == "ST-Acceptance Criteria");

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

            var projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_adminUser);

            var process = Helper.Storyteller.GetProcess(_adminUser, STORYTELLER1_PROCESS_ID);

            List<IStorytellerUserStory> userStories = null;
            try
            {
                // Execute:
               userStories = Helper.Storyteller.GenerateUserStories(_adminUser, process, shouldDeleteChildren: false);

                // Verify:
                Assert.IsNotNull(userStories, "There is no user story generated!");

                var returnedProperty = userStories.First().CustomProperties.Find(p => p.Name == "ST-Acceptance Criteria");

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
        /// <param name="itemType">The ItemTypePredefined for PersonaReference, if not specified, Actor ItemTypePredefined will be used.</param>
        /// <returns>The created persona reference</returns>
        private ArtifactReference AddPersonaReferenceToTask(string taskName, IProcess process, IUser user, IProject project, ItemTypePredefined itemType = ItemTypePredefined.Actor)
        {
            // Create actor for persona reference
            var actor = Helper.CreateAndPublishNovaArtifact(user, project, itemType);
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
            var processArtifact = new Artifact { Id = process.Id, Address = Helper.ArtifactStore.Address };
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
            var personaPropertyValue = task.PropertyValues[PropertyTypePredefined.Persona.ToString().LowerCaseFirstCharacter()].Value.ToString();

            Assert.AreEqual(task.PersonaReference.Name, personaPropertyValue,
                "The persona reference name and persona property value should be the same " +
                "but the persona reference name is {0} and the persona property vaue is {1}!", task.PersonaReference.Name,
                personaPropertyValue);
        }

        /// <summary>
        /// Create an inaccessible persona reference
        /// </summary>
        /// <param name="artifact">artifact that represent the inaccessible persona refence</param>
        /// <returns>inaccesible persona reference</returns>
        private static ArtifactReference CreateInaccessiblePersonaReference(IArtifactBase artifact)
        {
            var inaccessiblePersonaReference = new ArtifactReference()
            {
                Id = -4,
                Link = null,
                Name = "Inaccessible Actor",
                ProjectId = artifact.ProjectId,
                TypePrefix = "<Inaccessible>",
                BaseItemTypePredefined = ItemTypePredefined.Actor,
                Version = null
            };
            return inaccessiblePersonaReference;
        } 

        #endregion Private Methods
    }
}
