using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using NUnit.Framework;
using TestCommon;

namespace CommonServiceTests
{
    public class ContentForRapidReviewTests : TestBase
    {
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

        #endregion

        [TestCase(BaseArtifactType.BusinessProcess)]
        [TestCase(BaseArtifactType.DomainDiagram)]
        [TestCase(BaseArtifactType.GenericDiagram)]
        [TestCase(BaseArtifactType.Storyboard)]
        [TestCase(BaseArtifactType.UIMockup)]
        [TestCase(BaseArtifactType.UseCaseDiagram)]
        [TestRail(107388)]
        [Description("Check that Blueprint returns proper content for Diagram artifacts")]
        public void GetArtifactDiagramForRapidReview_VerifyResults(BaseArtifactType artifactType)
        {
            var artifact = Helper.CreateArtifact(_project, _user, artifactType: artifactType);

            artifact.Save(_user);
            artifact.Publish(_user);

            RapidReviewDiagram diagramContent = null;

            Assert.DoesNotThrow(() =>
            {
                diagramContent = artifact.GetDiagramContentForRapidReview(_user);
            }, "GetDiagramContentForRapidReview must not throw errors.");

            Assert.That(diagramContent.DiagramType, Is.EqualTo(artifactType.ToString()).IgnoreCase, "Returned diagram type must be {0}, but it is {1}", artifactType, diagramContent.DiagramType);
            Assert.AreEqual(artifact.Id, diagramContent.Id, "Returned properties must have artifact Id {0}, but it is {1}", artifact.Id, diagramContent.Id);
            Assert.IsEmpty(diagramContent.Shapes, "Newly created {0} shouldn't have any shape", artifactType);
            Assert.IsEmpty(diagramContent.Connections, "Newly created {0} shouldn't have any connections", artifactType);
            // TODO: add assertions about diagram size
        }

        [TestCase]
        [TestRail(107389)]
        [Description("Create glossary artifact, get RapidReview representation for it. Check that representation has id and terms.")]
        public void GetGlossaryForRapidReview_VerifyResults()
        {
            var artifact = Helper.CreateArtifact(_project, _user, artifactType: BaseArtifactType.Glossary);

            artifact.Save(_user);
            artifact.Publish(_user);

            RapidReviewGlossary glossaryContent = null;

            Assert.DoesNotThrow(() =>
            {
                glossaryContent = artifact.GetGlossaryContentForRapidReview(_user);
            }, "GetGlossaryContentForRapidReview must not throw errors.");

            Assert.AreEqual(artifact.Id, glossaryContent.Id, "Returned properties must have artifact Id {0}, but it is {1}", artifact.Id, glossaryContent.Id);
            Assert.IsEmpty(glossaryContent.Terms, "Newly created Glossary shouldn't have any terms.");
        }

        [TestCase]
        [TestRail(107390)]
        [Description("Create Use Case artifact, get RapidReview representation for it. Check that representation has proper scheme.")]
        public void GetUseCaseContentForRapidReview_VerifyResults()
        {
            var artifact = Helper.CreateArtifact(_project, _user, artifactType: BaseArtifactType.UseCase);

            artifact.Save(_user);
            artifact.Publish(_user);

            RapidReviewUseCase artifactContent = null;

            Assert.DoesNotThrow(() =>
            {
                artifactContent = artifact.GetUseCaseContentForRapidReview(_user);
            }, "GetUseCaseContentForRapidReview must not throw errors.");

            Assert.AreEqual(artifact.Id, artifactContent.Id, "Returned properties must have artifact Id {0}, but it is {1}", artifact.Id, artifactContent.Id);
            Assert.AreEqual(1, artifactContent.Steps.Count, "Newly created Use Case must have 1 step, but it has {0}", artifactContent.Steps.Count);
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestCase(BaseArtifactType.Process)]
        [TestCase(BaseArtifactType.UseCase)]
        [TestCase(BaseArtifactType.UIMockup)]
        [TestCase(BaseArtifactType.UseCaseDiagram)]
        [TestCase(BaseArtifactType.GenericDiagram)]
        [TestCase(BaseArtifactType.BusinessProcess)]
        [TestCase(BaseArtifactType.Document)]
        [TestCase(BaseArtifactType.DomainDiagram)]
        [TestCase(BaseArtifactType.Glossary)]
        [TestCase(BaseArtifactType.Storyboard)]
        [TestCase(BaseArtifactType.TextualRequirement)]
        [TestCase(BaseArtifactType.PrimitiveFolder)]
        [TestRail(107391)]
        [Description("Create artifact, get properties for it. Check that returned JSON has proper scheme.")]
        public void GetArtifactPropertiesForRapidReview_VerifyResults(BaseArtifactType artifactType)
        {
            var artifact = Helper.CreateArtifact(_project, _user, artifactType: artifactType);

            artifact.Save(_user);
            artifact.Publish(_user);

            RapidReviewProperties propertiesContent = null;

            Assert.DoesNotThrow(() =>
            {
                propertiesContent = artifact.GetPropertiesForRapidReview(_user);
            }, "GetPropertiesForRapidReview must not throw errors.");

            Assert.AreEqual(artifact.Id, propertiesContent.ArtifactId, "Returned properties must have artifact Id {0}, but it is {1}", artifact.Id, propertiesContent.ArtifactId);
            Assert.AreEqual(_user.DisplayName, propertiesContent.AuthorHistory[0].Value, "Returned properties must have Author {0}, but it is {1}", _user.DisplayName, propertiesContent.AuthorHistory[0].Value);
        }
    }
}
