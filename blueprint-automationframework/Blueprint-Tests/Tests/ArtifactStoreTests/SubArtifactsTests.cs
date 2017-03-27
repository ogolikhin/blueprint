using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Model.StorytellerModel.Enums;
using TestCommon;
using Utilities;
using Utilities.Facades;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class SubArtifactsTests : TestBase
    {
        private IUser _user = null;
        private IProject _project = null;

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
            _project = ProjectFactory.GetProject(_user);
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
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _project, _user);

            var userTask = returnedProcess.GetProcessShapesByShapeType(ProcessShapeType.UserTask);

            var preconditionTask = returnedProcess.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            var preconditionOutgoingLink = returnedProcess.GetOutgoingLinkForShape(preconditionTask);

            Assert.IsNotNull(preconditionOutgoingLink, "Process link was not found.");

            returnedProcess.AddUserAndSystemTask(preconditionOutgoingLink);

            returnedProcess.DeleteUserAndSystemTask(userTask[0]);

            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, Helper.Storyteller, _user);

            // Execute & Verify:
            CheckSubArtifacts(_user, returnedProcess.Id, expectedSubArtifactsNumber: 5, itemTypeVersionId: 2);
        }

        [TestCase]
        [TestRail(165965)]
        [Description("Create process, add decision point, save process, get list of subartifacts, check that returned list has expected content.")]
        public void GetSubArtifacts_ProcessWithUserDecisionAfterPrecondition_ReturnsCorrectSubArtifactsList()
        {
            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _project, _user);

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

            CheckSubArtifacts(_user, returnedProcess.Id, expectedSubArtifactsNumber: 8, itemTypeVersionId: 2);
        }

        [TestCase]
        [TestRail(165966)]
        [Description("Create default process, get list of subartifacts for it, check that it has expected content.")]
        public void GetSubArtifacts_Process_ReturnsCorrectSubArtifactsList()
        {
            // Setup:
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _project, _user);

            // Execute & Verify:
            CheckSubArtifacts(_user, returnedProcess.Id, expectedSubArtifactsNumber: 5, itemTypeVersionId: 2);
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(165967)]
        [Description("Create default process, delete it, get list of subartifacts - check that it is empty.")]
        public void GetSubArtifacts_DeletedProcess_ReturnsEmptySubArtifactsList(BaseArtifactType artifactType)
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var process = Helper.CreateAndPublishArtifact(_project, author, artifactType);

            process.Delete(author);

            // Execute:
            List<SubArtifact> subArtifacts = null;

            Assert.DoesNotThrow(() =>
            {
                subArtifacts = Helper.ArtifactStore.GetSubartifacts(author, process.Id);
            }, "GetSubartifacts should return 200 OK when sent with valid parameters!");

            // Verify:
            Assert.AreEqual(0, subArtifacts.Count, "For deleted process GetSubartifacts must return empty list.");
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForOpenApiRestMethods))]
        [TestRail(182596)]
        [Description("Create default process and new artifact in a different project from the process. Add inline trace that points to the artifact from a process " +
            "subartifact.  Verify inline trace added. Modify the artifact name and publish.  Verify inline trace in process subartifact is updated with a new name.")]
        public void GetSubArtifact_CreateInlineTraceFromProcessSubArtifactToArtifactInDifferentProject_ModifyArtifactName_VerifyInlineTraceUpdatedInProcess(
            BaseArtifactType baseArtifactType)
        {
            // Setup:
            var projects = ProjectFactory.GetProjects(_user, numberOfProjects: 2);

            projects[1].GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            var artifact = Helper.CreateAndPublishArtifact(projects[0], _user, baseArtifactType);

            var processArtifact = Helper.CreateWrapAndPublishNovaArtifactForStandardArtifactType(projects[1], _user, ItemTypePredefined.Process);

            var expectedDescriptionProperty = CreateInlineTraceFromProcessSubArtifactToArtifactAndPublish(processArtifact, artifact);

            // Get the process with the updated inline trace and verify that the trace was added
            var updatedProcess = Helper.Storyteller.GetProcess(_user, processArtifact.Id);
            var updatedDefaultUserTask = updatedProcess.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            var updatedDescriptionProperty = PropertyValueInformation.FindPropertyValue("description", updatedDefaultUserTask.PropertyValues).Value;

            Assert.AreEqual(expectedDescriptionProperty, updatedDescriptionProperty.Value.ToString(), "Description properties don't match.");

            artifact.Lock();

            // Change the name of artifact
            var artifactDetailsToUpdateInlineTraceArtifact = ArtifactStoreHelper.CreateNovaArtifactDetailsWithArtifact(artifact);
            artifactDetailsToUpdateInlineTraceArtifact.Name += "_NameUpdate";

            // Update the artifact with the new name
            Artifact.UpdateArtifact(artifact, _user, artifactDetailsChanges: artifactDetailsToUpdateInlineTraceArtifact);
            Helper.ArtifactStore.PublishArtifact(artifact, _user);

            // Get process subartifact via Nova call
            NovaSubArtifact subArtifact = null;

            // Execute:
            Assert.DoesNotThrow(() => subArtifact = Helper.ArtifactStore.GetSubartifact(_user, updatedProcess.Id, updatedDefaultUserTask.Id), 
                "GetSubartifactDetails call failed when using the following subartifact ID: {0}!", updatedDefaultUserTask.Id);

            // Verify:
            ArtifactStoreHelper.ValidateInlineTraceLinkFromSubArtifactDetails(subArtifact, artifact, validInlineTraceLink: true);

            CheckSubArtifacts(_user, processArtifact.Id, expectedSubArtifactsNumber: 5, itemTypeVersionId: 2);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForOpenApiRestMethods))]
        [TestRail(182559)]
        [Description("Create default process and new artifact. Add inline trace that points to the artifact from a process subartifact.  Verify inline trace added. " +
            "Delete the artifact and publish.  Verify inline trace in process subartifact is marked as invalid.")]
        public void GetSubArtifact_CreateInlineTraceFromProcessSubArtifactToArtifact_DeleteArtifact_VerifyInlineTraceIsMarkedInvalid(
            BaseArtifactType baseArtifactType)
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            var artifact = Helper.CreateAndPublishArtifact(_project, _user, baseArtifactType);

            var processArtifact = Helper.CreateWrapAndPublishNovaArtifactForStandardArtifactType(_project, _user, ItemTypePredefined.Process);

            var expectedDescriptionProperty = CreateInlineTraceFromProcessSubArtifactToArtifactAndPublish(processArtifact, artifact);

            // Get the process with the updated inline trace and verify that the trace was added
            var updatedProcess = Helper.Storyteller.GetProcess(_user, processArtifact.Id);
            var updatedDefaultUserTask = updatedProcess.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            var updatedDescriptionProperty = PropertyValueInformation.FindPropertyValue("description", updatedDefaultUserTask.PropertyValues).Value;

            Assert.AreEqual(expectedDescriptionProperty, updatedDescriptionProperty.Value.ToString(), "Description properties don't match.");

            artifact.Delete();
            artifact.Publish();

            NovaSubArtifact subArtifact = null;

            // Execute:
            Assert.DoesNotThrow(() => subArtifact = Helper.ArtifactStore.GetSubartifact(_user, updatedProcess.Id, updatedDefaultUserTask.Id), 
                "GetSubartifactDetails call failed when using the following subartifact ID: {0}!", updatedDefaultUserTask.Id);

            // Verify:
            ArtifactStoreHelper.ValidateInlineTraceLinkFromSubArtifactDetails(subArtifact, artifact, validInlineTraceLink: false);

            CheckSubArtifacts(_user, processArtifact.Id, expectedSubArtifactsNumber: 5, itemTypeVersionId: 2);

            Assert.AreEqual(subArtifact.IndicatorFlags, null, "IndicatorFlags property should be null!");
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForOpenApiRestMethods))]
        [TestRail(182600)]
        [Description("Create default process and new artifact in deifferent project. Add inline trace that points to the artifact from a process subartifact." +
                     "Verify inline trace added. Verify that GetArtifactDetails call returns invalid inline trace link if the user doesn't have the access permission " + 
                     "for the inline trace artifact")]
        public void GetSubArtifact_CreateInlineTraceFromProcessSubArtifactToArtifact_UserWithoutPermissionToInlineTraceArtifact_VerifyInlineTraceIsMarkedInvalid(
            BaseArtifactType baseArtifactType)
        {
            // Setup:
            var projects = ProjectFactory.GetProjects(_user, numberOfProjects: 2);
            var mainProject = projects[0];
            var secondProject = projects[1];
            secondProject.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            var artifact = Helper.CreateAndPublishArtifact(mainProject, _user, baseArtifactType);

            var processArtifact = Helper.CreateWrapAndPublishNovaArtifactForStandardArtifactType(secondProject, _user, ItemTypePredefined.Process);

            var expectedDescriptionProperty = CreateInlineTraceFromProcessSubArtifactToArtifactAndPublish(processArtifact, artifact);

            // Get the process with the updated inline trace and verify that the trace was added
            var updatedProcess = Helper.Storyteller.GetProcess(_user, processArtifact.Id);
            var updatedDefaultUserTask = updatedProcess.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            var updatedDescriptionProperty = PropertyValueInformation.FindPropertyValue("description", updatedDefaultUserTask.PropertyValues).Value;

            Assert.AreEqual(expectedDescriptionProperty, updatedDescriptionProperty.Value.ToString(), "Description properties don't match.");

            var userWithPermissionOnSecondProject = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, secondProject);

            NovaSubArtifact subArtifact = null;

            // Execute:
            Assert.DoesNotThrow(() => subArtifact = Helper.ArtifactStore.GetSubartifact(userWithPermissionOnSecondProject, updatedProcess.Id,
                updatedDefaultUserTask.Id), "GetSubartifactDetails call failed when using the following subartifact ID: {0}!", updatedDefaultUserTask.Id);

            // Verify:
            ArtifactStoreHelper.ValidateInlineTraceLinkFromSubArtifactDetails(subArtifact, artifact, validInlineTraceLink: false);

            CheckSubArtifacts(_user, processArtifact.Id, expectedSubArtifactsNumber: 5, itemTypeVersionId: 2);
        }

        [TestCase]
        [TestRail(234614)]
        [Description("Create & publish a UseCase artifact.  Get use case sub-artifacts.  Verify that properties are set to default values.")]
        public void GetSubArtifacts_PublishAndGetUseCaseSubArtifacts_ReturnsCorrectSubArtifactsList()
        {
            // Setup:
            var useCaseArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase);

            var viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);

            // Execute
            List<SubArtifact> subArtifacts = null;

            Assert.DoesNotThrow(() =>
            {
                subArtifacts = Helper.ArtifactStore.GetSubartifacts(viewer, useCaseArtifact.Id);
            }, "GetSubartifacts should return 200 OK when sent with valid parameters!");

            // Verify
            VerifyUseCaseSubArtifacts(subArtifacts, useCaseArtifact.Id, expectedSubArtifactCount: 3);
        }

        [TestCase]
        [TestRail(234615)]
        [Description("Create & publish a use case artifact.  Get use case sub-artifacts one by one.  Verify that properties are set to default values.")]
        public void GetSubArtifact_PublishAndGetUseCaseSubArtifacts_ReturnsCorrectSubArtifactsList()
        {
            // Setup:
            var useCaseArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase);

            var viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);

            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(viewer, useCaseArtifact.Id);

            var propertyCompareOptions = new NovaItem.PropertyCompareOptions()
            {
                CompareOrderIndeces = false,
                CompareDescriptions = false,
                CompareCustomProperties = false,
                CompareSpecificPropertyValues = false
            };

            // Execute & Verify
            foreach (var s in subArtifacts)
            {
                NovaSubArtifact subArtifact = null;

                Assert.DoesNotThrow(() =>
                {
                    subArtifact = Helper.ArtifactStore.GetSubartifact(viewer, useCaseArtifact.Id, s.Id);
                }, "GetSubartifact should return 200 OK when sent with valid parameters!");

                ArtifactStoreHelper.AssertSubArtifactsAreEqual(subArtifact, new NovaSubArtifact(s, itemTypeVersionId: 1), Helper.ArtifactStore,
                    _user, s.ParentId, propertyCompareOptions);
            }
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
            VerifyUseCaseSubArtifacts(subArtifacts, USECASE_ID, expectedSubArtifactCount: 4);

            Assert.IsTrue(subArtifacts[3].HasChildren, "This step must have child.");
            Assert.AreEqual(1, subArtifacts[3].Children.Count(), "This step must have child.");
        }

        [Category(Categories.CustomData)]
        [TestCase]
        [TestRail(165957)]
        [Description("GetSubartifacts for Business Process Diagram from Custom Data project. Check that results have expected content.")]
        public void GetSubArtifacts_CustomProjectBPDiagram_ReturnsCorrectSubArtifactsList()
        {
            //Execute & Verify:
            CheckSubArtifacts(_user, businessProcessDiagramId, expectedSubArtifactsNumber: 17);
        } 

        [Category(Categories.CustomData)]
        [TestCase]
        [TestRail(165958)]
        [Description("GetSubartifacts for Business Process Diagram from Custom Data project. Check that results have expected content.")]
        public void GetSubArtifacts_CustomProjectDomainDiagram_ReturnsCorrectSubArtifactsList()
        {
            // Execute & Verify:
            CheckSubArtifacts(_user, domainDiagramId, expectedSubArtifactsNumber: 7);
        }

        [Category(Categories.CustomData)]
        [TestCase]
        [TestRail(165959)]
        [Description("GetSubartifacts for Business Process Diagram from Custom Data project. Check that results have expected content.")]
        public void GetSubArtifacts_CustomProjectGenericDiagram_ReturnsCorrectSubArtifactsList()
        {
            // Execute & Verify:
            CheckSubArtifacts(_user, genericDiagramId, expectedSubArtifactsNumber: 14);
        }

        [Category(Categories.CustomData)]
        [TestCase]
        [TestRail(165960)]
        [Description("GetSubartifacts for Glossary from Custom Data project. Check that results have expected content.")]
        public void GetSubArtifacts_CustomProjectGlossary_ReturnsCorrectSubArtifactsList()
        {
            // Execute & Verify:
            CheckSubArtifacts(_user, glossaryId, expectedSubArtifactsNumber: 2);
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
            // Execute & Verify:
            CheckSubArtifacts(_user, uiMockupId, expectedSubArtifactsNumber: 27);
        }

        [Category(Categories.CustomData)]
        [TestCase]
        [TestRail(165963)]
        [Description("GetSubartifacts for Glossary from Custom Data project. Check that results have expected content.")]
        public void GetSubArtifacts_CustomProjectUseCaseDiagram_ReturnsCorrectSubArtifactsList()
        {
            // Execute & Verify:
            CheckSubArtifacts(_user, useCaseDiagramId, 7);
        }

        #endregion Custom Data

        #endregion Positive Tests

        #region 400 Bad Request

        [TestRail(246534)]
        [TestCase("9999999999", "The request is invalid.")]
        [TestCase("&amp;", "A potentially dangerous Request.Path value was detected from the client (&).")]
        [Description("Create a rest path that tries to get a subartifact with an invalid subartifact Id. " +
             "Attempt to get the subartifact. Verify that HTTP 400 Bad Request exception is thrown.")]
        public void GetSubArtifact_InvalidSubArtifactId_400BadRequest(string subArtifactId, string expectedErrorMessage)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Artifacts_id_.SUBARTIFACTS_id_, artifact.Id, subArtifactId);

            var restApi = new RestApiFacade(Helper.ArtifactStore.Address, _user?.Token?.AccessControlToken);

            // Execute & Verify:
            var ex = Assert.Throws<Http400BadRequestException>(() => restApi.SendRequestAndDeserializeObject<NovaArtifactResponse, object>(
               path,
               RestRequestMethod.GET,
               jsonObject: null),
                "We should get a 400 Bad Request when the subArtifact Id is invalid!");

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, expectedErrorMessage);
        }

        #endregion 400 Bad Request

        #region 401 Unauthorized

        [TestCase(BaseArtifactType.Process)]
        [TestRail(234585)]
        [Description("Create & publish artifact with subartifacts.  User tries to get sub-artifact with bad tokens.  Verify it returns 401 Unauthorized.")]
        public void GetSubArtifact_UserWithBadToken_401Unauthorized(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            var userWithBadToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, artifact.Id);

            // Execute
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.ArtifactStore.GetSubartifact(userWithBadToken, artifact.Id, subArtifacts[0].Id);
            }, "'GET {0}' should return 401 Unauthorized when user has bad token!", RestPaths.Svc.ArtifactStore.Artifacts_id_.SUBARTIFACTS_id_);

            // Verify
            const string expectedExceptionMessage = "Unauthorized call";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                    "{0} was not found in returned message of copy published artifact which has no token in a header.", expectedExceptionMessage);
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(234586)]
        [Description("Create & publish artifact with subartifacts.  User tries to get sub-artifact with no token in header.  Verify it returns 401 Unauthorized.")]
        public void GetSubArtifact_NoTokenInHeader_401Unauthorized(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, artifact.Id);

            // Execute
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.ArtifactStore.GetSubartifact(user: null, artifactId: artifact.Id, subArtifactId: subArtifacts[0].Id);
            }, "'GET {0}' should return 401 Unauthorized when there is no token in a header!", RestPaths.Svc.ArtifactStore.Artifacts_id_.SUBARTIFACTS_id_);

            // Verify
            const string expectedExceptionMessage = "Unauthorized call";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                    "{0} was not found in returned message of copy published artifact which has no token in a header.", expectedExceptionMessage);
        }

        #endregion 401 Unauthorized

        #region 403 Forbidden

        [TestCase(BaseArtifactType.Process)]
        [TestRail(191097)]
        [Description("Create & publish an artifact with sub-artifacts, GetSubArtifacts with a user that doesn't have access to the artifact.  Verify it returns 403 Forbidden.")]
        public void GetSubArtifacts_PublishedArtifactUserWithoutPermissions_403Forbidden(BaseArtifactType artifactType)
        {
            // Setup
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            var viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);
            Helper.AssignProjectRolePermissionsToUser(viewer, TestHelper.ProjectRole.None, _project, artifact);

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
            var parent = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            var child = Helper.CreateAndPublishArtifact(_project, _user, artifactType, parent);

            var viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);
            Helper.AssignProjectRolePermissionsToUser(viewer, TestHelper.ProjectRole.None, _project, parent);

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
        public void GetSubArtifact_PublishedArtifact_UserWithoutPermissions_403Forbidden(BaseArtifactType artifactType)
        {
            // Setup
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            var viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);
            Helper.AssignProjectRolePermissionsToUser(viewer, TestHelper.ProjectRole.None, _project, artifact);

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
        public void GetSubArtifact_PublishedArtifactWithAChild_UserWithoutPermissionsToParent_403Forbidden(BaseArtifactType artifactType)
        {
            // Setup:
            var parent = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            var child = Helper.CreateAndPublishArtifact(_project, _user, artifactType, parent);

            var viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);
            Helper.AssignProjectRolePermissionsToUser(viewer, TestHelper.ProjectRole.None, _project, parent);

            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, child.Id);

            Assert.IsNotNull(subArtifacts, "This artifact does not have sub-artifacts!");
            Assert.IsNotEmpty(subArtifacts, "This artifact does not have sub-artifacts!");
            int subArtifactId = subArtifacts[0].Id;

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                Helper.ArtifactStore.GetSubartifact(viewer, child.Id, subArtifactId);
            }, "'GET {0}' should return 403 Forbidden when passed a valid child artifact ID but the user doesn't have permission to view parent artifact!",
               RestPaths.Svc.ArtifactStore.Artifacts_id_.SUBARTIFACTS_id_);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden,
                I18NHelper.FormatInvariant("You do not have permission to access the artifact (ID: {0})", child.Id));
        }

        #endregion 403 Forbidden

        #region 404 Not Found

        [TestCase(BaseArtifactType.Process, int.MaxValue)]
        [TestRail(182511)]
        [Description("Create & publish artifact with subartifacts.  User tries to find sub-artifact that does not exists.  Verify it returns 404 Not Found.")]
        public void GetSubArtifact_NonExistingSubArtifact_404NotFound(BaseArtifactType artifactType, int subArtifactId)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetSubartifact(_user, artifact.Id, subArtifactId);
            }, "'GET {0}' should return 404 Not Found when passed a non-existing ID of sub-artifact!", RestPaths.Svc.ArtifactStore.Artifacts_id_.SUBARTIFACTS_id_);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound,
                "You have attempted to access an artifact that does not exist or has been deleted.");
        }

        [TestCase]
        [TestRail(234587)]
        [Description("Create & save artifact with sub-artifacts.  Delete sub-artifact.  Verify it returns 404 Not Found.")]
        public void GetSubArtifact_DeletedSubArtifact_404NotFound()
        {
            // Setup:
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _project, _user);

            // Find precondition task
            var preconditionTask = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition task
            var preconditionOutgoingLink = process.GetOutgoingLinkForShape(preconditionTask);

            Assert.IsNotNull(preconditionOutgoingLink, "Process link was not found.");

            // Add user/system Task immediately after the precondition
            var userTask = process.AddUserAndSystemTask(preconditionOutgoingLink);

            // Save the process
            var returnedProcess = Helper.Storyteller.UpdateProcess(_user, process);

            var userTaskToBeDeleted = returnedProcess.GetProcessShapeByShapeName(userTask.Name);

            returnedProcess.DeleteUserAndSystemTask(userTaskToBeDeleted);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, Helper.Storyteller, _user);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetSubartifact(_user, process.Id, userTaskToBeDeleted.Id);
            }, "'GET {0}' should return 404 Not Found when passed a non-existing ID of sub-artifact!", RestPaths.Svc.ArtifactStore.Artifacts_id_.SUBARTIFACTS_id_);

            // Verify:
            // Bug: Wrong message returned http://svmtfs2015:8080/tfs/svmtfs2015/Blueprint/_workitems?_a=edit&id=5106
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound,
                "You have attempted to access an artifact that does not exist or has been deleted.");
        }

        #endregion 404 Not Found

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
        /// <param name="itemTypeVersionId">ItemTypeVersionId for NovaSubArtifact created from SubArtifact</param>
        private void CheckSubArtifacts(IUser user, int artifactId, int expectedSubArtifactsNumber, int itemTypeVersionId = 1)
        {
            List<SubArtifact> subArtifacts = null;

            Assert.DoesNotThrow(() =>
            {
                subArtifacts = Helper.ArtifactStore.GetSubartifacts(user, artifactId);
            }, "GetSubartifacts shouldn't throw an error.");

            Assert.AreEqual(expectedSubArtifactsNumber, subArtifacts.Count, "Number of subartifacts must be correct.");

            foreach (var s in subArtifacts)
            {
                Assert.IsFalse(s.HasChildren, "Process sub-artifacts should never have children.");

                var subArtifact = Helper.ArtifactStore.GetSubartifact(user, artifactId, s.Id);

                var propertyCompareOptions = new NovaItem.PropertyCompareOptions()
                {
                    CompareOrderIndeces = false,
                    CompareDescriptions = false,
                    CompareCustomProperties = false,
                    CompareSpecificPropertyValues = false
                };

                ArtifactStoreHelper.AssertSubArtifactsAreEqual(subArtifact, new NovaSubArtifact(s, itemTypeVersionId), Helper.ArtifactStore, _user, s.ParentId,
                    propertyCompareOptions);
            }
        }

        /// <summary>
        /// This is to verify properties of UseCase sub-artifacts returned from getSubArtifacts call
        /// </summary>
        /// <param name="subArtifacts">List of sub-artifacts</param>
        /// <param name="useCaseId">UseCase Id</param>
        /// <param name="expectedSubArtifactCount">Amount of sub-artifact returned</param>
        private static void VerifyUseCaseSubArtifacts(List<SubArtifact> subArtifacts, int useCaseId, int expectedSubArtifactCount)
        {
            Assert.AreEqual(expectedSubArtifactCount, subArtifacts.Count, "Use Case must have 4 subartifacts - Pre Condition, Post Condition and 2 steps.");

            foreach (var s in subArtifacts)
            {
                Assert.AreEqual(useCaseId, s.ParentId, "ParentId for subartifact of Use Case must be equal to Use Case Id.");
            }

            Assert.AreEqual(UseCaseDisplayNames.PRECONDITION, subArtifacts[0].DisplayName, "DisplayName for Precondition should have expected name.");
            Assert.AreEqual(UseCaseDisplayNames.POSTCONDITION, subArtifacts[1].DisplayName, "DisplayName for Postcondition should have expected name.");

            for (int i = 2; i < subArtifacts.Count; i++)
            {
                Assert.AreEqual(I18NHelper.FormatInvariant(UseCaseDisplayNames.STEP, i - 1), subArtifacts[i].DisplayName,
                    "DisplayName for Step should have expected name.");
            }

            for (int i = 0; i < 3; i++)
            {
                Assert.IsFalse(subArtifacts[i].HasChildren, "This subartifacts shouldn't have children.");
            }
        }

        /// <summary>
        /// Creates inline trace between process sub-artifact and another artifact
        /// </summary>
        /// <param name="processArtifact">Process artifact</param>
        /// <param name="artifact">Another artifact</param>
        /// <returns>Expected updated description with inline trace</returns>
        private string CreateInlineTraceFromProcessSubArtifactToArtifactAndPublish(IArtifact processArtifact, IArtifact artifact)
        {
            var userTaskSubArtifact = Helper.ArtifactStore.GetSubartifacts(_user, processArtifact.Id).Find(sa => sa.DisplayName.Equals(Process.DefaultUserTaskName));
            var subArtifactChangeSet = Helper.ArtifactStore.GetSubartifact(_user, processArtifact.Id, userTaskSubArtifact.Id);
            subArtifactChangeSet.Description = ArtifactStoreHelper.CreateTextForProcessInlineTrace(new List<IArtifact> { artifact });

            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, processArtifact.Id);
            artifactDetails.SubArtifacts = new List<NovaSubArtifact>() { subArtifactChangeSet };

            processArtifact.Lock(_user);
            Helper.ArtifactStore.UpdateArtifact(_user, artifactDetails);
            Helper.ArtifactStore.PublishArtifact(processArtifact, _user);

            return subArtifactChangeSet.Description;
        }

        #endregion Private Methods
    }
}
