using System.Collections.Generic;
using System.Linq;
using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.StorytellerModel;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using TestCommon;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class SubArtifactsTests : TestBase
    {
        private IUser _user = null;

        private List<IProject> _projects;

        private int useCaseId = 24;
        private int businessProcessDiagramId = 33;
        private int domainDiagramId = 31;
        private int genericDiagramId = 49;
        private int glossaryId = 40;
        private int storyboardId = 32;
        private int uiMockupId = 22;
        private int useCaseDiagramId = 29;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _projects = ProjectFactory.GetAllProjects(_user, shouldRetrievePropertyTypes: true);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #region Process tests

        [TestCase]
        [TestRail(165964)]//regression see https://trello.com/c/yyqvXZa1
        [Description("Create default process, add user task, delete default user task, save process, get list of subartifacts - check that it has expected content.")]
        public void GetSubArtifacts_ProcessWithDeletedDefaultAndAddedNewUserTask_ReturnsCorrectSubArtifactsList()
        {
            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _projects.FirstOrDefault(), _user);

            // Get list containing default user task
            var userTask = returnedProcess.GetProcessShapesByShapeType(ProcessShapeType.UserTask);

            // Find precondition task
            var preconditionTask = returnedProcess.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition task
            var preconditionOutgoingLink = returnedProcess.GetOutgoingLinkForShape(preconditionTask);

            Assert.IsNotNull(preconditionOutgoingLink, "Process link was not found.");

            // Add user/system Task immediately after the precondition
            returnedProcess.AddUserAndSystemTask(preconditionOutgoingLink);

            returnedProcess.DeleteUserAndSystemTask(userTask[0]);
                        
            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, Helper.Storyteller, _user);

            CheckSubArtifacts(_user, returnedProcess.Id, 5);//at this stage Process should have 5 subartifacts
        }

        [TestCase]
        [TestRail(165965)]
        [Description("Create process, add decision point, save process, get list of subartifacts, check that returned list has expected content.")]
        public void GetSubArtifacts_ProcessWithUserDecisionAfterPrecondition_ReturnsCorrectSubArtifactsList()
        {
            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _projects.FirstOrDefault(), _user);

            // Find precondition task
            var preconditionTask = returnedProcess.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var preconditionOutgoingLink = returnedProcess.GetOutgoingLinkForShape(preconditionTask);

            // Get the branch end point
            var branchEndPoint = returnedProcess.GetProcessShapeByShapeName(Process.EndName);

            // Add Decision point with branch to end
            returnedProcess.AddUserDecisionPointWithBranchAfterShape(preconditionTask, preconditionOutgoingLink.Orderindex + 1, branchEndPoint.Id);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, Helper.Storyteller, _user);

            CheckSubArtifacts(_user, returnedProcess.Id, 8);//at this stage Process should have 8 subartifacts
        }

        [TestCase]
        [TestRail(165966)]
        [Description("Create default process, get list of subartifacts for it, check that it has expected content.")]
        public void GetSubArtifacts_Process_ReturnsCorrectSubArtifactsList()
        {
            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _projects.FirstOrDefault(), _user);

            CheckSubArtifacts(_user, returnedProcess.Id, 5);//at this stage Process should have 5 subartifacts
        }

        [TestCase]
        [TestRail(165967)]
        [Description("Create default process, delete it, get list of subartifacts - check that it is empty.")]
        public void GetSubArtifacts_DeletedProcess_ReturnsEmptySubArtifactsList()
        {
            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _projects.FirstOrDefault(), _user);

            Helper.Storyteller.Artifacts[0].Delete(_user);

            List<INovaSubArtifact> subArtifacts = null;
            Assert.DoesNotThrow(() =>
            {
                subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, returnedProcess.Id);
            }, "GetSubartifacts shouldn't throw an error.");

            Assert.AreEqual(0, subArtifacts.Count, "For deleted process GetSubartifacts must return empty list (for Instance Admin).");
        }

        [Explicit(IgnoreReasons.UnderDevelopment)] // will be updated after backend portion of User Story 2338:[Nova] [ST] Edit Process custom properties in Utility Panel
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForOpenApiRestMethods))]
        [TestRail(182511)]
        [Description("Create default process and new artifact. Add inline trace that points to the new artifact to a process subartifact." +
                     "Verify inline trace added. Modify new artifact name and publish.  Verify inline trace in process subartifact is updated with " +
                     "the modifed artifact name.")]
        public void GetSubArtifacts_CreateInlineTraceFromProcessSubArtifactToArtifactThenModifyArtifactName_VerifyInlineTraceUpdatedInProcess(
            BaseArtifactType baseArtifactType)
        {
            // Setup:
            // Create artifact
            var inlineTraceArtifact = Helper.CreateAndPublishArtifact(_projects.FirstOrDefault(), _user, baseArtifactType);

            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _projects.FirstOrDefault(), _user);

            // Add an inline trace to the default user task in the process and publish the process
            var defaultUserTask = returnedProcess.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            var descriptionProperty = StorytellerTestHelper.FindPropertyValue("description", defaultUserTask.PropertyValues).Value;
            descriptionProperty.Value = ArtifactStoreHelper.CreateTextForProcessInlineTrace(new List<IArtifact> {inlineTraceArtifact});

            Helper.Storyteller.UpdateProcess(_user, returnedProcess);
            Helper.Storyteller.PublishProcess(_user, returnedProcess);

            // Get the process with the updated inline trace and verify that the trace was added
            var updatedProcess = Helper.Storyteller.GetProcess(_user, returnedProcess.Id);
            var updatedDefaultUserTask = updatedProcess.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            var updatedDescriptionProperty = StorytellerTestHelper.FindPropertyValue("description", updatedDefaultUserTask.PropertyValues).Value;

            StringAssert.Contains(descriptionProperty.Value.ToString(), updatedDescriptionProperty.Value.ToString(),
                "Description properties don't match.");

            // Execute:
            inlineTraceArtifact.Lock();

            // Change the name of artifact
            var artifactDetailsToUpdateInlineTraceArtifact = ArtifactStoreHelper.CreateNovaArtifactDetailsWithArtifact(inlineTraceArtifact);
            artifactDetailsToUpdateInlineTraceArtifact.Name += "_NameUpdate";

            // Update the artifact with the new name
            Artifact.UpdateArtifact(inlineTraceArtifact, _user, artifactDetailsChanges: artifactDetailsToUpdateInlineTraceArtifact);
            Helper.ArtifactStore.PublishArtifact(inlineTraceArtifact, _user);

            // Get process subartifact details via Nova call
            NovaSubArtifactDetails subArtifactDetails = null;

            Assert.DoesNotThrow(() => subArtifactDetails = Helper.ArtifactStore.GetSubartifactDetails(_user, updatedProcess.Id,
                updatedDefaultUserTask.Id), "GetSubartifactDetails call failed when using the following subartifact ID: {0}!", updatedDefaultUserTask.Id);

            // Verify:
            ArtifactStoreHelper.ValidateInlineTraceLinkFromSubArtifactDetails(subArtifactDetails, inlineTraceArtifact, validInlineTraceLink: true);

            CheckSubArtifacts(_user, returnedProcess.Id, 5);//at this stage Process should have 5 subartifacts
        }

        [Explicit(IgnoreReasons.UnderDevelopment)] // will be updated after backend portion of User Story 2338:[Nova] [ST] Edit Process custom properties in Utility Panel
        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForOpenApiRestMethods))]
        [TestRail(182596)]
        [Description("Create default process and new artifact in a different project from the process. Add inline trace that points to the new " +
                     "artifact to a process subartifact.  Verify inline trace added. Modify new artifact name and publish.  Verify inline trace " +
                     "in process subartifact is updated with the modifed artifact name from the other project.")]
        public void GetSubArtifacts_CreateInlineTraceFromProcessSubArtifactToArtifactInDifferentProjectThenModifyArtifactName_VerifyInlineTraceUpdatedInProcess(
            BaseArtifactType baseArtifactType)
        {
            // Setup:
            var mainProject = _projects.FirstOrDefault();
            var secondProject = _projects.LastOrDefault();

            // Create artifact
            var inlineTraceArtifact = Helper.CreateAndPublishArtifact(mainProject, _user, baseArtifactType);

            // Create and get the default process in a different project from the previously created artifact
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, secondProject, _user);

            // Add an inline trace to the default user task in the process and publish the process
            var defaultUserTask = returnedProcess.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            var descriptionProperty = StorytellerTestHelper.FindPropertyValue("description", defaultUserTask.PropertyValues).Value;
            descriptionProperty.Value = ArtifactStoreHelper.CreateTextForProcessInlineTrace(new List<IArtifact> { inlineTraceArtifact });

            Helper.Storyteller.UpdateProcess(_user, returnedProcess);
            Helper.Storyteller.PublishProcess(_user, returnedProcess);

            // Get the process with the updated inline trace and verify that the trace was added
            var updatedProcess = Helper.Storyteller.GetProcess(_user, returnedProcess.Id);
            var updatedDefaultUserTask = updatedProcess.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            var updatedDescriptionProperty = StorytellerTestHelper.FindPropertyValue("description", updatedDefaultUserTask.PropertyValues).Value;

            StringAssert.Contains(descriptionProperty.Value.ToString(), updatedDescriptionProperty.Value.ToString(),
                "Description properties don't match.");

            // Execute:
            inlineTraceArtifact.Lock();

            // Change the name of artifact
            var artifactDetailsToUpdateInlineTraceArtifact = ArtifactStoreHelper.CreateNovaArtifactDetailsWithArtifact(inlineTraceArtifact);
            artifactDetailsToUpdateInlineTraceArtifact.Name += "_NameUpdate"; 

            // Update the artifact with the new name
            Artifact.UpdateArtifact(inlineTraceArtifact, _user, artifactDetailsChanges: artifactDetailsToUpdateInlineTraceArtifact);
            Helper.ArtifactStore.PublishArtifact(inlineTraceArtifact, _user);

            // Get process subartifact details via Nova call
            NovaSubArtifactDetails subArtifactDetails = null;

            Assert.DoesNotThrow(() => subArtifactDetails = Helper.ArtifactStore.GetSubartifactDetails(_user, updatedProcess.Id,
                updatedDefaultUserTask.Id), "GetSubartifactDetails call failed when using the following subartifact ID: {0}!", updatedDefaultUserTask.Id);

            // Verify:
            ArtifactStoreHelper.ValidateInlineTraceLinkFromSubArtifactDetails(subArtifactDetails, inlineTraceArtifact, validInlineTraceLink: true);

            CheckSubArtifacts(_user, returnedProcess.Id, 5);//at this stage Process should have 5 subartifacts
        }

        [Explicit(IgnoreReasons.UnderDevelopment)] // will be updated after backend portion of User Story 2338:[Nova] [ST] Edit Process custom properties in Utility Panel
        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForOpenApiRestMethods))]
        [TestRail(182559)]
        [Description("Create default process and new artifact. Add inline trace that points to the new artifact to a process subartifact." +
                     "Verify inline trace added. Delete new artifact name and publish.  Verify inline trace in process subartifact is marked " +
                     "tas invalid.")]
        public void GetSubArtifacts_CreateInlineTraceFromProcessSubArtifactToArtifactThenDeleteArtifact_VerifyInlineTraceIsMarkedInvalid(
               BaseArtifactType baseArtifactType)
        {
            // Setup:
            // Create artifact
            var inlineTraceArtifact = Helper.CreateAndPublishArtifact(_projects.FirstOrDefault(), _user, baseArtifactType);

            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _projects.FirstOrDefault(), _user);

            // Add an inline trace to the default user task in the process and publish the process
            var defaultUserTask = returnedProcess.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            var descriptionProperty = StorytellerTestHelper.FindPropertyValue("description", defaultUserTask.PropertyValues).Value;
            descriptionProperty.Value = ArtifactStoreHelper.CreateTextForProcessInlineTrace(new List<IArtifact> { inlineTraceArtifact });

            Helper.Storyteller.UpdateProcess(_user, returnedProcess);
            Helper.Storyteller.PublishProcess(_user, returnedProcess);

            // Get the process with the updated inline trace and verify that the trace was added
            var updatedProcess = Helper.Storyteller.GetProcess(_user, returnedProcess.Id);
            var updatedDefaultUserTask = updatedProcess.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            var updatedDescriptionProperty = StorytellerTestHelper.FindPropertyValue("description", updatedDefaultUserTask.PropertyValues).Value;

            StringAssert.Contains(descriptionProperty.Value.ToString(), updatedDescriptionProperty.Value.ToString(),
                "Description properties don't match.");

            // Execute:
            inlineTraceArtifact.Lock();
            inlineTraceArtifact.Delete();
            inlineTraceArtifact.Publish();

            // Get process subartifact details via Nova call
            NovaSubArtifactDetails subArtifactDetails = null;

            Assert.DoesNotThrow(() => subArtifactDetails = Helper.ArtifactStore.GetSubartifactDetails(_user, updatedProcess.Id,
                updatedDefaultUserTask.Id), "GetSubartifactDetails call failed when using the following subartifact ID: {0}!", updatedDefaultUserTask.Id);

            // Verify:
            ArtifactStoreHelper.ValidateInlineTraceLinkFromSubArtifactDetails(subArtifactDetails, inlineTraceArtifact, validInlineTraceLink: false);

            CheckSubArtifacts(_user, returnedProcess.Id, 5);//at this stage Process should have 5 subartifacts
        }

        [Explicit(IgnoreReasons.UnderDevelopment)] // will be updated after backend portion of User Story 2338:[Nova] [ST] Edit Process custom properties in Utility Panel
        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForOpenApiRestMethods))]
        [TestRail(182600)]
        [Description("Create default process and new artifact. Add inline trace that points to the new artifact to a process subartifact." +
                     "Verify inline trace added. Delete new artifact name and publish.  Verify that GetArtifactDetails call returns invalid " +
                     "inline trace link if the user doesn't have the access permission for the inline trace artifact")]
        public void GetSubArtifacts_CreateInlineTraceFromProcessSubArtifactToArtifactGetSubArtifactDetailsUsingUserWithoutPermissionToInlineTraceArtifact_VerifyInlineTraceIsMarkedInvalid(
            BaseArtifactType baseArtifactType)
        {
            // Setup: Get projects available from testing environment
            var mainProject = _projects.FirstOrDefault();
            var secondProject = _projects.LastOrDefault();

            // Create artifact
            var inlineTraceArtifact = Helper.CreateAndPublishArtifact(mainProject, _user, baseArtifactType);

            // Create and get the default process in a different project from the previously created artifact
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, secondProject, _user);

            // Add an inline trace to the default user task in the process and publish the process
            var defaultUserTask = returnedProcess.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            var descriptionProperty = StorytellerTestHelper.FindPropertyValue("description", defaultUserTask.PropertyValues).Value;
            descriptionProperty.Value = ArtifactStoreHelper.CreateTextForProcessInlineTrace(new List<IArtifact> { inlineTraceArtifact });

            Helper.Storyteller.UpdateProcess(_user, returnedProcess);
            Helper.Storyteller.PublishProcess(_user, returnedProcess);

            // Get the process with the updated inline trace and verify that the trace was added
            var updatedProcess = Helper.Storyteller.GetProcess(_user, returnedProcess.Id);
            var updatedDefaultUserTask = updatedProcess.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            var updatedDescriptionProperty = StorytellerTestHelper.FindPropertyValue("description", updatedDefaultUserTask.PropertyValues).Value;

            StringAssert.Contains(descriptionProperty.Value.ToString(), updatedDescriptionProperty.Value.ToString(),
                "Description properties don't match.");

            // Create user with a permission only on second project
            var userWithPermissionOnSecondProject = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, new List<IProject> { secondProject });

            // Get process subartifact details via Nova call
            NovaSubArtifactDetails subArtifactDetails = null;

            Assert.DoesNotThrow(() => subArtifactDetails = Helper.ArtifactStore.GetSubartifactDetails(userWithPermissionOnSecondProject, updatedProcess.Id,
                updatedDefaultUserTask.Id), "GetSubartifactDetails call failed when using the following subartifact ID: {0}!", updatedDefaultUserTask.Id);

            // Verify:
            ArtifactStoreHelper.ValidateInlineTraceLinkFromSubArtifactDetails(subArtifactDetails, inlineTraceArtifact, validInlineTraceLink: false);

            CheckSubArtifacts(_user, returnedProcess.Id, 5);//at this stage Process should have 5 subartifacts
        }

        #endregion Process tests

        #region Custom data tests

        [Category(Categories.CustomData)]
        [TestCase]
        [TestRail(165855)]
        [Description("GetSubartifacts for Use Case from Custom Data project. Check that results have expected content.")]
        public void GetSubArtifacts_CustomProjectUseCase_ReturnsCorrectSubArtifactsList()
        {
            List<INovaSubArtifact> subArtifacts = null;
            Assert.DoesNotThrow(() =>
            {
                subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, useCaseId);
            }, "GetSubartifacts shouldn't throw an error.");

            //Test that returned JSON corresponds to the Use Case structure
            Assert.AreEqual(4, subArtifacts.Count, "Use Case must have 4 subartifacts - Pre Condition, Post Condition and 2 steps.");
            foreach (var s in subArtifacts)
            {
                Assert.AreEqual(useCaseId, s.ParentId, "ParentId for subartifact of Use Case must be equal to Use Case Id.");
            }
            Assert.AreEqual(UseCaseDisplayNames.PRECONDITION, subArtifacts[0].DisplayName,
                "DisplayName for Precondition should have expected name.");
            Assert.AreEqual(UseCaseDisplayNames.POSTCONDITION, subArtifacts[1].DisplayName,
                "DisplayName for Postcondition should have expected name.");
            for (int i = 2; i < subArtifacts.Count; i++)
            {
                Assert.AreEqual(I18NHelper.FormatInvariant(UseCaseDisplayNames.STEP, i - 1),
                    subArtifacts[i].DisplayName, "DisplayName for Step should have expected name.");
            }
            for (int i = 0; i < 3; i++)
            {
                Assert.IsFalse(subArtifacts[i].HasChildren, "This subartifacts shouldn't have children.");
            }
            Assert.IsTrue(subArtifacts[3].HasChildren, "This step must have child.");
            Assert.AreEqual(1, subArtifacts[3].Children.Count, "This step must have child.");
        }

        [Category(Categories.CustomData)]
        [TestCase]
        [TestRail(165957)]
        [Description("GetSubartifacts for Business Process Diagram from Custom Data project. Check that results have expected content.")]
        public void GetSubArtifacts_CustomProjectBPDiagram_ReturnsCorrectSubArtifactsList()
        {
            CheckSubArtifacts(_user, businessProcessDiagramId, 17);
        }

        [Category(Categories.CustomData)]
        [TestCase]
        [TestRail(165958)]
        [Description("GetSubartifacts for Business Process Diagram from Custom Data project. Check that results have expected content.")]
        public void GetSubArtifacts_CustomProjectDomainDiagram_ReturnsCorrectSubArtifactsList()
        {
            CheckSubArtifacts(_user, domainDiagramId, 7);
        }

        [Category(Categories.CustomData)]
        [TestCase]
        [TestRail(165959)]
        [Description("GetSubartifacts for Business Process Diagram from Custom Data project. Check that results have expected content.")]
        public void GetSubArtifacts_CustomProjectGenericDiagram_ReturnsCorrectSubArtifactsList()
        {
            CheckSubArtifacts(_user, genericDiagramId, 14);
        }

        [Category(Categories.CustomData)]
        [TestCase]
        [TestRail(165960)]
        [Description("GetSubartifacts for Glossary from Custom Data project. Check that results have expected content.")]
        public void GetSubArtifacts_CustomProjectGlossary_ReturnsCorrectSubArtifactsList()
        {
            CheckSubArtifacts(_user, glossaryId, 2);
        }

        [Category(Categories.CustomData)]
        [TestCase]
        [TestRail(165961)]
        [Description("GetSubartifacts for Glossary from Custom Data project. Check that results have expected content.")]
        public void GetSubArtifacts_CustomProjectStoryboard_ReturnsCorrectSubArtifactsList()
        {
            List<INovaSubArtifact> subArtifacts = null;
            Assert.DoesNotThrow(() =>
            {
                subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, storyboardId);
            }, "GetSubartifacts shouldn't throw an error.");

            //Test that returned JSON corresponds to the Generic Diagram structure
            Assert.AreEqual(3, subArtifacts.Count, ".");
            foreach (var s in subArtifacts)
            {
                Assert.AreEqual(storyboardId, s.ParentId, "..");
            }
            Assert.IsTrue(subArtifacts[1].HasChildren, "This subartifact should have child.");
            Assert.AreEqual(subArtifacts[1].Id, subArtifacts[1].Children[0].ParentId, "ParentId of subartifact's child must be equal to subartifact's Id.");
            Assert.AreEqual(1, subArtifacts[1].Children.Count, "This subartifact should have child.");
            Assert.IsTrue(subArtifacts[2].HasChildren, "This subartifact should have child.");
            Assert.AreEqual(subArtifacts[2].Id, subArtifacts[2].Children[0].ParentId, "ParentId of subartifact's child must be equal to subartifact's Id.");
            Assert.AreEqual(1, subArtifacts[2].Children.Count, "This subartifact should have child.");
        }

        [Category(Categories.CustomData)]
        [TestCase]
        [TestRail(165962)]
        [Description("GetSubartifacts for Glossary from Custom Data project. Check that results have expected content.")]
        public void GetSubArtifacts_CustomProjectUIMockup_ReturnsCorrectSubArtifactsList()
        {
            CheckSubArtifacts(_user, uiMockupId, 27);
        }

        [Category(Categories.CustomData)]
        [TestCase]
        [TestRail(165963)]
        [Description("GetSubartifacts for Glossary from Custom Data project. Check that results have expected content.")]
        public void GetSubArtifacts_CustomProjectUseCaseDiagram_ReturnsCorrectSubArtifactsList()
        {
            CheckSubArtifacts(_user, useCaseDiagramId, 7);
        }

        #endregion Custom Data

        #region Private Methods

        private static class UseCaseDisplayNames
        {
            public const string PRECONDITION = "Pre Condition";
            public const string POSTCONDITION = "Post Condition";
            public const string STEP = "Step {0}";
        }

        private void CheckSubArtifacts(IUser user, int artifactId, int expectedSubArtifactsNumber)
        {
            List<INovaSubArtifact> subArtifacts = null;
            Assert.DoesNotThrow(() =>
            {
                subArtifacts = Helper.ArtifactStore.GetSubartifacts(user, artifactId);
            }, "GetSubartifacts shouldn't throw an error.");

            Assert.AreEqual(expectedSubArtifactsNumber, subArtifacts.Count, "Number of subartifacts must be correct.");
            foreach (var s in subArtifacts)
            {
                Assert.AreEqual(artifactId, s.ParentId, "ParentId of subartifact must be equal to Id of the process.");
                Assert.IsFalse(s.HasChildren, "Process subartifacts doesn't have children.");
            }
        }

        #endregion Private Methods
    }
}
