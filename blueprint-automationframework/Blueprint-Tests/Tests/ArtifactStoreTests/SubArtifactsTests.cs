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
using TestCommon;
using Utilities;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class SubArtifactsTests : TestBase
    {
        private IUser _user = null;

        private List<IProject> _projects;

        private int businessProcessDiagramId = 33;
        private int domainDiagramId = 31;
        private int genericDiagramId = 49;
        private int glossaryId = 40;
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

        #region Positive tests

        [TestCase]
        [TestRail(165964)]
        [Description("Create default process, add user task, delete default user task, save process, get list of subartifacts - check that it has expected content.")]
        public void GetSubArtifacts_ProcessWithDeletedDefaultAndAddedNewUserTask_ReturnsCorrectSubArtifactsList()
        {
            // Setup:
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _projects.FirstOrDefault(), _user);

            // Get list containing default user task
            var userTask = returnedProcess.GetProcessShapesByShapeType(ProcessShapeType.UserTask);

            // Find precondition task
            var preconditionTask = returnedProcess.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition task
            var preconditionOutgoingLink = returnedProcess.GetOutgoingLinkForShape(preconditionTask);

            Assert.IsNotNull(preconditionOutgoingLink, "Process link was not found.");

            returnedProcess.AddUserAndSystemTask(preconditionOutgoingLink);

            returnedProcess.DeleteUserAndSystemTask(userTask[0]);

            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, Helper.Storyteller, _user);

            // Verify:
            CheckSubArtifacts(_user, returnedProcess.Id, 5); //at this stage Process should have 5 subartifacts
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

        [TestCase(BaseArtifactType.Process)]
        [TestRail(165967)]
        [Description("Create default process, delete it, get list of subartifacts - check that it is empty.")]
        public void GetSubArtifacts_DeletedProcess_ReturnsEmptySubArtifactsList(BaseArtifactType artifactType)
        {
            // Setup
            var project = _projects[0];

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, project);

            var process = Helper.CreateAndPublishArtifact(project, author, artifactType);

            process.Delete(author);

            // Execute
            List<SubArtifact> subArtifacts = null;

            Assert.DoesNotThrow(() =>
            {
                subArtifacts = Helper.ArtifactStore.GetSubartifacts(author, process.Id);
            }, "GetSubartifacts should return 200 OK when sent with valid parameters!");

            // Verify
            Assert.AreEqual(0, subArtifacts.Count, "For deleted process GetSubartifacts must return empty list.");
        }

        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)] // will be updated under User Story 3419:[Nova] Backend for editing properties (properties and sub-artifacts)
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

            StringAssert.Contains(descriptionProperty.Value.ToString(), updatedDescriptionProperty.Value.ToString(), "Description properties don't match.");

            // Execute:
            inlineTraceArtifact.Lock();

            // Change the name of artifact
            var artifactDetailsToUpdateInlineTraceArtifact = ArtifactStoreHelper.CreateNovaArtifactDetailsWithArtifact(inlineTraceArtifact);
            artifactDetailsToUpdateInlineTraceArtifact.Name += "_NameUpdate";

            // Update the artifact with the new name
            Artifact.UpdateArtifact(inlineTraceArtifact, _user, artifactDetailsChanges: artifactDetailsToUpdateInlineTraceArtifact);
            Helper.ArtifactStore.PublishArtifact(inlineTraceArtifact, _user);

            // Get process subartifact via Nova call
            NovaSubArtifact subArtifact = null;

            Assert.DoesNotThrow(() => subArtifact = Helper.ArtifactStore.GetSubartifact(_user, updatedProcess.Id,
                updatedDefaultUserTask.Id), "GetSubartifactDetails call failed when using the following subartifact ID: {0}!", updatedDefaultUserTask.Id);

            // Verify:
            ArtifactStoreHelper.ValidateInlineTraceLinkFromSubArtifactDetails(subArtifact, inlineTraceArtifact, validInlineTraceLink: true);

            CheckSubArtifacts(_user, returnedProcess.Id, 5);//at this stage Process should have 5 subartifacts
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]    // Ignore for now.
        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)] // will be updated under User Story 3419:[Nova] Backend for editing properties (properties and sub-artifacts)
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

            StringAssert.Contains(descriptionProperty.Value.ToString(), updatedDescriptionProperty.Value.ToString(), "Description properties don't match.");

            // Execute:
            inlineTraceArtifact.Lock();

            // Change the name of artifact
            var artifactDetailsToUpdateInlineTraceArtifact = ArtifactStoreHelper.CreateNovaArtifactDetailsWithArtifact(inlineTraceArtifact);
            artifactDetailsToUpdateInlineTraceArtifact.Name += "_NameUpdate"; 

            // Update the artifact with the new name
            Artifact.UpdateArtifact(inlineTraceArtifact, _user, artifactDetailsChanges: artifactDetailsToUpdateInlineTraceArtifact);
            Helper.ArtifactStore.PublishArtifact(inlineTraceArtifact, _user);

            // Get process subartifact via Nova call
            NovaSubArtifact subArtifact = null;

            Assert.DoesNotThrow(() => subArtifact = Helper.ArtifactStore.GetSubartifact(_user, updatedProcess.Id,
                updatedDefaultUserTask.Id), "GetSubartifactDetails call failed when using the following subartifact ID: {0}!", updatedDefaultUserTask.Id);

            // Verify:
            ArtifactStoreHelper.ValidateInlineTraceLinkFromSubArtifactDetails(subArtifact, inlineTraceArtifact, validInlineTraceLink: true);

            CheckSubArtifacts(_user, returnedProcess.Id, 5);//at this stage Process should have 5 subartifacts
        }

        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)] // will be updated under User Story 3419:[Nova] Backend for editing properties (properties and sub-artifacts)
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

            StringAssert.Contains(descriptionProperty.Value.ToString(), updatedDescriptionProperty.Value.ToString(), "Description properties don't match.");

            // Execute:
            inlineTraceArtifact.Lock();
            inlineTraceArtifact.Delete();
            inlineTraceArtifact.Publish();

            // Get process subartifact via Nova call
            NovaSubArtifact subArtifact = null;

            Assert.DoesNotThrow(() => subArtifact = Helper.ArtifactStore.GetSubartifact(_user, updatedProcess.Id,
                updatedDefaultUserTask.Id), "GetSubartifactDetails call failed when using the following subartifact ID: {0}!", updatedDefaultUserTask.Id);

            // Verify:
            ArtifactStoreHelper.ValidateInlineTraceLinkFromSubArtifactDetails(subArtifact, inlineTraceArtifact, validInlineTraceLink: false);

            CheckSubArtifacts(_user, returnedProcess.Id, 5);//at this stage Process should have 5 subartifacts
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)] // will be updated under User Story 3419:[Nova] Backend for editing properties (properties and sub-artifacts)
        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForOpenApiRestMethods))]
        [TestRail(182600)]
        [Description("Create default process and new artifact. Add inline trace that points to the new artifact to a process subartifact." +
                     "Verify inline trace added. Delete new artifact name and publish.  Verify that GetArtifactDetails call returns invalid " +
                     "inline trace link if the user doesn't have the access permission for the inline trace artifact")]
        public void GetSubArtifacts_CreateInlineTraceFromProcessSubArtifactToArtifactGetSubArtifactDetailsUsingUserWithoutPermissionToInlineTraceArtifact_VerifyInlineTraceIsMarkedInvalid(
            BaseArtifactType baseArtifactType)
        {
            // Setup:
            var mainProject = _projects.FirstOrDefault();
            mainProject.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            var secondProject = _projects.LastOrDefault();
            var inlineTraceArtifact = Helper.CreateAndPublishArtifact(secondProject, _user, baseArtifactType);

            var processArtifact = Helper.CreateWrapAndPublishNovaArtifactForStandardArtifactType(mainProject, _user, ItemTypePredefined.Process);

            var userTaskSubArtifact = Helper.ArtifactStore.GetSubartifacts(_user, processArtifact.Id).Find(sa => sa.DisplayName.Equals(Process.DefaultUserTaskName));
            var subArtifactChangeSet = Helper.ArtifactStore.GetSubartifact(_user, processArtifact.Id, userTaskSubArtifact.Id);
            subArtifactChangeSet.Description = ArtifactStoreHelper.CreateTextForProcessInlineTrace(new List<IArtifact> { inlineTraceArtifact }); ;

            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, processArtifact.Id);
            artifactDetails.SubArtifacts = new List<NovaSubArtifact>() { subArtifactChangeSet };

            processArtifact.Lock(_user);
            Helper.ArtifactStore.UpdateArtifact(_user, artifactDetails);
            Helper.ArtifactStore.PublishArtifact(processArtifact, _user);

            // Get the process with the updated inline trace and verify that the trace was added
            var updatedProcess = Helper.Storyteller.GetProcess(_user, processArtifact.Id);
            var updatedDefaultUserTask = updatedProcess.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            var updatedDescriptionProperty = StorytellerTestHelper.FindPropertyValue("description", updatedDefaultUserTask.PropertyValues).Value;

            StringAssert.Contains(subArtifactChangeSet.Description, updatedDescriptionProperty.Value.ToString(), "Description properties don't match.");

            // Create user with a permission only on second project
            var userWithPermissionOnSecondProject = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, secondProject);

            // Get process subartifact via Nova call
            NovaSubArtifact subArtifact = null;

            Assert.DoesNotThrow(() => subArtifact = Helper.ArtifactStore.GetSubartifact(userWithPermissionOnSecondProject, updatedProcess.Id,
                updatedDefaultUserTask.Id), "GetSubartifactDetails call failed when using the following subartifact ID: {0}!", updatedDefaultUserTask.Id);

            // Verify:
            ArtifactStoreHelper.ValidateInlineTraceLinkFromSubArtifactDetails(subArtifact, inlineTraceArtifact, validInlineTraceLink: false);

            CheckSubArtifacts(_user, processArtifact.Id, 5); //at this stage Process should have 5 subartifacts
            /*            

                        var artifact = Helper.CreateWrapAndSaveNovaArtifact(mainProject, _user, ItemTypePredefined.Process, artifactTypeName: "Process");

                        // Get nova subartifact
                        var novaProcess = Helper.Storyteller.GetNovaProcess(_user, artifact.Id);
                        var processShape = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
                        var novaSubArtifact = Helper.ArtifactStore.GetSubartifact(_user, artifact.Id, processShape.Id);


                        var property = ArtifactStoreHelper.SetCustomPropertyToNull(novaSubArtifact.CustomPropertyValues, "description");

                        // Add subartifact changeset to NovaProcess
                        var subArtifactChangeSet = TestHelper.CreateSubArtifactChangeSet(novaSubArtifact, customProperty: property);
                        novaProcess.SubArtifacts = new List<NovaSubArtifact> { subArtifactChangeSet };

                        Helper.Storyteller.UpdateNovaProcess(_user, novaProcess);
                        /*var inlineTraceArtifact = */
            //            Helper.CreateAndPublishArtifact(secondProject, _user, baseArtifactType);
            /*


                        var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(Helper.Storyteller, mainProject, _user);



                        var process = novaProcess.Process;
                        var defaultUserTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

                        new SubArtifact();
                        var descriptionProperty = StorytellerTestHelper.FindPropertyValue("description", defaultUserTask.PropertyValues).Value;
                        descriptionProperty.Value = ArtifactStoreHelper.CreateTextForProcessInlineTrace(new List<IArtifact> { inlineTraceArtifact });

                        StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess, Helper.Storyteller, _user);
                        /*            
                         *            
                        //            var process = novaProcess.Process;
                        //            var processType = process.ProcessType;
                        /*var updatedNovaProcess = */

            //            var novaProcess = Helper.Storyteller.GetNovaProcess(_user, artifact.Id);
            /*            var inlineTraceArtifact = Helper.CreateAndPublishArtifact(mainProject, _user, BaseArtifactType.UseCase);

                                                var process = Helper.CreateAndPublishArtifact(mainProject, _user, BaseArtifactType.Process);

                                                var subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, process.Id);
                                                var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, process.Id);
                                                var novaSubArtifacts = ArtifactStoreHelper.GetDetailsForAllSubArtifacts(Helper.ArtifactStore, process, subArtifacts, _user);
            */
            // Create artifact


            /*            // Create and get the default process in a different project from the previously created artifact
                        var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, secondProject, _user);

                                                // Add an inline trace to the default user task in the process and publish the process
            //                                    var defaultUserTask = returnedProcess.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
                                                var descriptionProperty = StorytellerTestHelper.FindPropertyValue("description", defaultUserTask.PropertyValues).Value;
                                                descriptionProperty.Value = ArtifactStoreHelper.CreateTextForProcessInlineTrace(new List<IArtifact> { inlineTraceArtifact });

            //            CSharpUtilities.SetProperty(defaultUserTask.PropertyValues[2].PropertyName, descriptionProperty.Value, defaultUserTask);

            //var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, returnedProcess.Id);

            //            var subArtifact = Helper.ArtifactStore.GetSubartifact(_user, artifactDetails.Id, defaultUserTask.Id);

            //            subArtifact.Description = ArtifactStoreHelper.CreateTextForProcessInlineTrace(new List<IArtifact> { inlineTraceArtifact });

                        Helper.Storyteller.UpdateProcess(_user, returnedProcess);
                                    Helper.Storyteller.PublishProcess(_user, returnedProcess);

            /*
                        var descriptionProperty = StorytellerTestHelper.FindPropertyValue("description", novaSubArtifacts[0].SpecificPropertyValues.).Value;
                        descriptionProperty.Value = ArtifactStoreHelper.CreateTextForProcessInlineTrace(new List<IArtifact> { inlineTraceArtifact });



                        StringAssert.Contains(descriptionProperty.Value.ToString(), updatedDescriptionProperty.Value.ToString(), "Description properties don't match.");

                        // Create user with a permission only on second project
                        var userWithPermissionOnSecondProject = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, new List<IProject> { secondProject });

                        // Get process subartifact via Nova call
                        NovaSubArtifact subArtifact = null;

                        Assert.DoesNotThrow(() => subArtifact = Helper.ArtifactStore.GetSubartifact(userWithPermissionOnSecondProject, updatedProcess.Id,
                            updatedDefaultUserTask.Id), "GetSubartifactDetails call failed when using the following subartifact ID: {0}!", updatedDefaultUserTask.Id);

                        // Verify:
                        ArtifactStoreHelper.ValidateInlineTraceLinkFromSubArtifactDetails(subArtifact, inlineTraceArtifact, validInlineTraceLink: false);

                        CheckSubArtifacts(_user, returnedProcess.Id, 5);//at this stage Process should have 5 subartifacts
            */
        }

        #region Custom data tests

        [Category(Categories.CustomData)]
        [TestCase]
        [TestRail(165855)]
        [Description("GetSubartifacts for Use Case from Custom Data project. Check that results have expected content.")]
        public void GetSubArtifacts_CustomProjectUseCase_ReturnsCorrectSubArtifactsList()
        {
            // Setup
            const int USECASE_ID = 24;

            var project = ArtifactStoreHelper.GetCustomDataProject(_user);

            var viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, project);

            // Execute
            List<SubArtifact> subArtifacts = null;

            Assert.DoesNotThrow(() =>
            {
                subArtifacts = Helper.ArtifactStore.GetSubartifacts(viewer, USECASE_ID);
            }, "GetSubartifacts should return 200 OK when sent with valid parameters!");

            // Verify
            // Test that returned JSON corresponds to the Use Case structure
            Assert.AreEqual(4, subArtifacts.Count, "Use Case must have 4 subartifacts - Pre Condition, Post Condition and 2 steps.");

            foreach (var s in subArtifacts)
            {
                Assert.AreEqual(USECASE_ID, s.ParentId, "ParentId for subartifact of Use Case must be equal to Use Case Id.");
            }

            Assert.AreEqual(UseCaseDisplayNames.PRECONDITION, subArtifacts[0].DisplayName, "DisplayName for Precondition should have expected name.");
            Assert.AreEqual(UseCaseDisplayNames.POSTCONDITION, subArtifacts[1].DisplayName, "DisplayName for Postcondition should have expected name.");

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
            Assert.AreEqual(1, subArtifacts[3].Children.Count(), "This step must have child.");
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
            // Setup
            const int STORYBOARD_ID = 32;

            var project = ArtifactStoreHelper.GetCustomDataProject(_user);

            var viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, project);

            // Execute
            List<SubArtifact> subArtifacts = null;

            Assert.DoesNotThrow(() =>
            {
                subArtifacts = Helper.ArtifactStore.GetSubartifacts(viewer, STORYBOARD_ID);
            }, "GetSubartifacts should return 200 OK when sent with valid parameters");

            // Verify
            // Test that returned JSON corresponds to the Generic Diagram structure
            Assert.AreEqual(3, subArtifacts.Count, ".");
            foreach (var s in subArtifacts)
            {
                Assert.AreEqual(STORYBOARD_ID, s.ParentId, "..");
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

        #endregion Positive Tests

        #region 403 Forbidden

        [TestCase(BaseArtifactType.Process)]
        [TestRail(191097)]
        [Description("Create & publish an artifact with sub-artifacts, GetSubArtifacts with a user that doesn't have access to the artifact.  Verify it returns 403 Forbidden.")]
        public void GetSubArtifacts_PublishedArtifactUserWithoutPermissions_403Forbidden(BaseArtifactType artifactType)
        {
            // Setup
            var artifact = Helper.CreateAndPublishArtifact(_projects[0], _user, artifactType);

            var viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _projects[0]);
            Helper.AssignProjectRolePermissionsToUser(viewer, TestHelper.ProjectRole.None, _projects[0], artifact);

            // Execute
            Assert.Throws<Http403ForbiddenException>(() =>
            {
                Helper.ArtifactStore.GetSubartifacts(viewer, artifact.Id);
            }, "'GET {0}' should return 403 Forbidden when passed a valid artifact ID but the user doesn't have permission to view the artifact!",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.SUBARTIFACTS);

            // Verify
            // TODO : Currently impossible to verify error message. Bug: http://svmtfs2015:8080/tfs/svmtfs2015/Blueprint/_workitems?_a=edit&id=3571
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(191098)]
        [Description("Create & publish parent & child artifacts.  Make sure viewer does not have access to parent.  Viewer request GetSubArtifacts from child artifact.  " +
            "Verify it returns 403 Forbidden.")]
        public void GetSubArtifacts_PublishedArtifactWithAChild_UserWithoutPermissionsToParent_403Forbidden(BaseArtifactType artifactType)
        {
            // Setup
            var parent = Helper.CreateAndPublishArtifact(_projects[0], _user, artifactType);
            var child = Helper.CreateAndPublishArtifact(_projects[0], _user, artifactType, parent);

            var viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _projects[0]);
            Helper.AssignProjectRolePermissionsToUser(viewer, TestHelper.ProjectRole.None, _projects[0], parent);

            // Execute
            Assert.Throws<Http403ForbiddenException>(() =>
            {
                Helper.ArtifactStore.GetSubartifacts(viewer, child.Id);
            }, "'GET {0}' should return 403 Forbidden when passed a valid child artifact ID but the user doesn't have permission to view parent artifact!",
                            RestPaths.Svc.ArtifactStore.Artifacts_id_.SUBARTIFACTS);

            // Verify
            // TODO : Currently impossible to verify error message. Bug: http://svmtfs2015:8080/tfs/svmtfs2015/Blueprint/_workitems?_a=edit&id=3571
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(191099)]
        [Description("Create & publish an artifact with sub-artifacts, GetSubArtifactDetails with a user that doesn't have access to the artifact.  Verify it returns 403 Forbidden.")]
        public void GetSubArtifactDetails_PublishedArtifactUserWithoutPermissions_403Forbidden(BaseArtifactType artifactType)
        {
            // Setup
            var artifact = Helper.CreateAndPublishArtifact(_projects[0], _user, artifactType);

            var viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _projects[0]);
            Helper.AssignProjectRolePermissionsToUser(viewer, TestHelper.ProjectRole.None, _projects[0], artifact);

            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, artifact.Id);

            Assert.IsNotNull(subArtifacts, "This artifact does not have sub-artifacts!");
            Assert.IsNotEmpty(subArtifacts, "This artifact does not have sub-artifacts!");
            int subArtifactId = subArtifacts[0].Id;

            // Execute
            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                Helper.ArtifactStore.GetSubartifact(viewer, artifact.Id, subArtifactId);
            }, "'GET {0}' should return 403 Forbidden when passed a valid artifact ID and sub-artifact ID but the user doesn't have permission to view the artifact!",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.SUBARTIFACTS_id_);

            // Verify
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, 
                I18NHelper.FormatInvariant("You do not have permission to access the artifact (ID: {0})", artifact.Id));
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(191100)]
        [Description("Create & publish parent & child artifacts.  Make sure viewer does not have access to parent.  Viewer request GetSubArtifactDetails from child artifact.  " +
            "Verify it returns 403 Forbidden.")]
        public void GetSubArtifactDetails_PublishedArtifactWithAChild_UserWithoutPermissionsToParent_403Forbidden(BaseArtifactType artifactType)
        {
            // Setup
            var parent = Helper.CreateAndPublishArtifact(_projects[0], _user, artifactType);
            var child = Helper.CreateAndPublishArtifact(_projects[0], _user, artifactType, parent);

            var viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _projects[0]);
            Helper.AssignProjectRolePermissionsToUser(viewer, TestHelper.ProjectRole.None, _projects[0], parent);

            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, child.Id);

            Assert.IsNotNull(subArtifacts, "This artifact does not have sub-artifacts!");
            Assert.IsNotEmpty(subArtifacts, "This artifact does not have sub-artifacts!");
            int subArtifactId = subArtifacts[0].Id;

            // Execute
            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                Helper.ArtifactStore.GetSubartifact(viewer, child.Id, subArtifactId);
            }, "'GET {0}' should return 403 Forbidden when passed a valid child artifact ID but the user doesn't have permission to view parent artifact!",
               RestPaths.Svc.ArtifactStore.Artifacts_id_.SUBARTIFACTS_id_);

            // Verify
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden,
                I18NHelper.FormatInvariant("You do not have permission to access the artifact (ID: {0})", child.Id));
        }

        #endregion 403 Forbidden

        #region Private Methods

        /// <summary>
        /// Possible use case display names
        /// </summary>
        private static class UseCaseDisplayNames
        {
            public const string PRECONDITION = "Pre Condition";
            public const string POSTCONDITION = "Post Condition";
            public const string STEP = "Step {0}";
        }

        /// <summary>
        /// Checks sub-artifacts within an artifact for number of sub-artifacts, sub-artifact parent Id and if sub-artifact has children
        /// </summary>
        /// <param name="user">User to authenticate with</param>
        /// <param name="artifactId">artifact Id</param>
        /// <param name="expectedSubArtifactsNumber">Number of expected sub-artifacts in the artifact</param>
        private void CheckSubArtifacts(IUser user, int artifactId, int expectedSubArtifactsNumber)
        {
            List<SubArtifact> subArtifacts = null;

            Assert.DoesNotThrow(() =>
            {
                subArtifacts = Helper.ArtifactStore.GetSubartifacts(user, artifactId);
            }, "GetSubartifacts shouldn't throw an error.");

            Assert.AreEqual(expectedSubArtifactsNumber, subArtifacts.Count, "Number of subartifacts must be correct.");

            foreach (var s in subArtifacts)
            {
                Assert.AreEqual(artifactId, s.ParentId, "ParentId of sub-artifact must be equal to Id of the process.");
                Assert.IsFalse(s.HasChildren, "Process subartifacts doesn't have children.");

                var subArtifact = Helper.ArtifactStore.GetSubartifact(user, artifactId, s.Id);

                ArtifactStoreHelper.AssertSubArtifactsAreEqual(subArtifact, new NovaSubArtifact(s), Helper.ArtifactStore, _user, skipOrderIndex: true,
                    skipSpecificPropertyValues: true);
            }
        }

        #endregion Private Methods
    }
}
