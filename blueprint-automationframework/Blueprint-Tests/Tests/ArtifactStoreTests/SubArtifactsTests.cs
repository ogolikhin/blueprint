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

        private IProject _project;

        private int useCaseId = 11;
        private int businessProcessDiagramId = 30;
        private int domainDiagramId = 24;
        private int genericDiagramId = 53;
        private int glossaryId = 52;
        private int storyboardId = 22;
        private int uiMockupId = 25;
        private int useCaseDiagramId = 21;

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

        #region Process tests

        [TestCase]
        [TestRail(165964)]//regression see https://trello.com/c/yyqvXZa1
        [Description("Create default process, add user task, delete default user task, save process, get list of subartifacts - check that it has expected content.")]
        public void GetSubArtifacts_ProcessWithDeletedDefaultAndAddedNewUserTask_ReturnsCorrectSubArtifactsList()
        {
            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _project, _user);

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

            CheckSubArtifacts(_user, returnedProcess.Id, 8);//at this stage Process should have 8 subartifacts
        }

        [TestCase]
        [TestRail(165966)]
        [Description("Create default process, get list of subartifacts for it, check that it has expected content.")]
        public void GetSubArtifacts_Process_ReturnsCorrectSubArtifactsList()
        {
            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _project, _user);

            CheckSubArtifacts(_user, returnedProcess.Id, 5);//at this stage Process should have 5 subartifacts
        }

        [TestCase]
        [TestRail(165967)]
        [Description("Create default process, delete it, get list of subartifacts - check that it is empty.")]
        public void GetSubArtifacts_DeletedProcess_ReturnsEmptySubArtifactsList()
        {
            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _project, _user);

            Helper.Storyteller.Artifacts[0].Delete(_user);

            List<INovaSubArtifact> subArtifacts = null;
            Assert.DoesNotThrow(() =>
            {
                subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, returnedProcess.Id);
            }, "GetSubartifacts shouldn't throw an error.");

            Assert.AreEqual(0, subArtifacts.Count, "For deleted process GetSubartifacts must return empty list (for Instance Admin).");
        }

        [TestCase]
        [TestRail(182511)]
        [Description("Create default process and new artifact. Add inline trace that points to the new artifact to a process subartifact." +
                     "Verify inline trace added. Modify new artifact name and publish.  Verify inline trace in process subartifact is updated with " +
                     "the modifed artifact name.")]
        public void GetSubArtifacts_CreateInlineTraceFromProcessToArtifactThenModifyArtifactName_VerifyInlineTraceUpdatedInProcess()
        {
            // Setup:
            // Create artifact
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);

            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _project, _user);

            // Add an inline trace to the default user task in the process and publish the process
            var defaultUserTask = returnedProcess.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            var descriptionProperty = defaultUserTask.PropertyValues.FirstOrDefault(p => p.Key == "description").Value;
            descriptionProperty.Value = CreateTextForProcessInlineTrace(new List<IArtifact> {artifact});

            Helper.Storyteller.UpdateProcess(_user, returnedProcess);
            Helper.Storyteller.PublishProcess(_user, returnedProcess);

            // Get the process with the updated inline trace and verify that the trace was added
            var updatedProcess = Helper.Storyteller.GetProcess(_user, returnedProcess.Id);
            var updatedDefaultUserTask = updatedProcess.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            var updatedDescriptionProperty = StorytellerTestHelper.FindPropertyValue("description", updatedDefaultUserTask.PropertyValues).Value;

            Assert.That(updatedDescriptionProperty.Value.ToString().Contains(descriptionProperty.Value.ToString()), "Description properties don't match.");

            // Execute:
            artifact.Lock();

            // Change the name of artifact
            var artifactDetailsToUpdateInlineTraceArtifact = new NovaArtifactDetails
            {
                Id = artifact.Id,
                ProjectId = artifact.ProjectId,
                ParentId = artifact.ParentId,
                Name = artifact.Name + "_NameUpdated",
                Version = artifact.Version
            };

            // Update the artifact with the new name
            var updatedArtifactDetails = Artifact.UpdateArtifact(artifact, _user, artifactDetailsChanges: artifactDetailsToUpdateInlineTraceArtifact);
            Helper.ArtifactStore.PublishArtifact(artifact, _user);

            // Get process subartifact details via Nova call
            NovaSubArtifactDetails subartifactDetails = null;

            Assert.DoesNotThrow(() => subartifactDetails = Helper.ArtifactStore.GetSubartifactDetails(_user, updatedProcess.Id,
                updatedDefaultUserTask.Id), "GetSubartifactDetails call failed when using the following subartifact ID: {0}!", updatedDefaultUserTask.Id);
 

            // Verify:
            Assert.That(subartifactDetails.Description.Contains(updatedArtifactDetails.Name), "The artifact name was not updated in the sub artifact inline trace.");

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

        /// <summary>
        /// Creates new rich text that includes inline trace(s)
        /// </summary>
        /// <param name="artifacts">The artifacts being added as inline traces</param>
        /// <returns>A formatted rich text string with inlne traces(s)</returns>
        private static string CreateTextForProcessInlineTrace(IList<IArtifact> artifacts)
        {
            var text = string.Empty;

            foreach (var artifact in artifacts)
            {
                var openApiProperty = artifact.Properties.FirstOrDefault(p => p.Name == "ID");
                if (openApiProperty != null)
                {
                    text = text + I18NHelper.FormatInvariant("<a " +
                        "href=\"{0}/?/ArtifactId={1}\" target=\"\" artifactid=\"{1}\"" +
                        " linkassemblyqualifiedname=\"BluePrintSys.RC.Client.SL.RichText.RichTextArtifactLink, BluePrintSys.RC.Client.SL.RichText, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\"" +
                        " canclick=\"True\" isvalid=\"True\" title=\"Project: {3}\"><span style=\"text-decoration: underline; color: #0000ff\">{4}: {2}</span></a>",
                        artifact.Address, artifact.Id, artifact.Name, artifact.Project.Name,
                        openApiProperty.TextOrChoiceValue);
                }
            }

            Assert.IsFalse(string.IsNullOrWhiteSpace(text), "Text for inline trace was null or whitespace!");

            return I18NHelper.FormatInvariant("<p>{0}</p>", text);
        }

        #endregion Private Methods
    }
}
