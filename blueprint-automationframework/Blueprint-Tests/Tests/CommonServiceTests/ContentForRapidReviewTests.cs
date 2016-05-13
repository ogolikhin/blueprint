using CustomAttributes;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using NUnit.Framework;

namespace CommonServiceTests
{
    public class ContentForRapidReviewTests
    {
        private IAdminStore _adminStore;
        private IBlueprintServer _blueprintServer;
        private IUser _user;
        private IProject _project;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            _adminStore = AdminStoreFactory.GetAdminStoreFromTestConfig();
            _blueprintServer = BlueprintServerFactory.GetBlueprintServerFromTestConfig();
            _user = UserFactory.CreateUserAndAddToDatabase();
            _project = ProjectFactory.GetProject(_user);

            // Get a valid Access Control token for the user (for the new Storyteller REST calls).
            ISession session = _adminStore.AddSession(_user);
            _user.SetToken(session.SessionId);

            Assert.IsFalse(
                string.IsNullOrWhiteSpace(_user.Token.AccessControlToken),
                "The user didn't get an Access Control token!");

            // Get a valid OpenApi token for the user (for the OpenApi artifact REST calls).
            _blueprintServer.LoginUsingBasicAuthorization(_user, string.Empty);

            Assert.IsFalse(string.IsNullOrWhiteSpace(_user.Token.OpenApiToken), "The user didn't get an OpenApi token!");
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            if (_adminStore != null)
            {
                // Delete all the sessions that were created.
                foreach (var session in _adminStore.Sessions.ToArray())
                {
                    _adminStore.DeleteSession(session);
                }
            }

            if (_user != null)
            {
                _user.DeleteUser();
                _user = null;
            }
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
            var artifact = ArtifactFactory.CreateArtifact(_project, _user, artifactType: artifactType);

            artifact.Save(_user);
            artifact.Publish(_user);

            RapidReviewDiagram artifactContent = null;
            try
            {
                Assert.DoesNotThrow(() =>
                {
                    artifactContent = artifact.GetDiagramContentForRapidReview(_user);
                }, "GetDiagramContentForRapidReview must not throw errors.");
            }

            finally
            {
                artifact.Delete(_user);
                artifact.Publish(_user);
            }
        }

        [TestCase]
        [TestRail(107389)]
        [Description("Create glossary artifact, get RapidReview representation for it. Check that representation has id and terms.")]
        public void GetGlossaryForRapidReview_VerifyResults()
        {
            var artifact = ArtifactFactory.CreateArtifact(_project, _user, artifactType: BaseArtifactType.Glossary);

            artifact.Save(_user);
            artifact.Publish(_user);

            RapidReviewGlossary artifactContent = null;
            try
            {
                Assert.DoesNotThrow(() =>
                {
                    artifactContent = artifact.GetGlossaryContentForRapidReview(_user);
                }, "GetGlossaryContentForRapidReview must not throw errors.");
            }

            finally
            {
                artifact.Delete(_user);
                artifact.Publish(_user);
            }
        }

        [TestCase]
        [TestRail(107390)]
        [Description("Create Use Case artifact, get RapidReview representation for it. Check that representation has proper scheme.")]
        public void GetUseCaseContentForRapidReview_VerifyResults()
        {
            var artifact = ArtifactFactory.CreateArtifact(_project, _user, artifactType: BaseArtifactType.UseCase);

            artifact.Save(_user);
            artifact.Publish(_user);

            RapidReviewUseCase artifactContent = null;
            try
            {
                Assert.DoesNotThrow(() =>
                {
                    artifactContent = artifact.GetUseCaseContentForRapidReview(_user);
                }, "GetUseCaseContentForRapidReview must not throw errors.");
            }

            finally
            {
                artifact.Delete(_user);
                artifact.Publish(_user);
            }
        }

        [TestCase(BaseArtifactType.Glossary)]
        [TestCase(BaseArtifactType.UseCase)]
        [TestCase(BaseArtifactType.BusinessProcess)]
        [TestCase(BaseArtifactType.DomainDiagram)]
        [TestCase(BaseArtifactType.GenericDiagram)]
        [TestCase(BaseArtifactType.Storyboard)]
        [TestCase(BaseArtifactType.UseCaseDiagram)]
        [TestCase(BaseArtifactType.UIMockup)]
        [TestRail(107391)]
        [Description("Create artifact, get properties for it. Check that returned JSON has proper scheme.")]
        public void GetArtifactPropertiesForRapidReview_VerifyResults(BaseArtifactType artifactType)
        {
            var artifact = ArtifactFactory.CreateArtifact(_project, _user, artifactType: artifactType);

            artifact.Save(_user);
            artifact.Publish(_user);

            RapidReviewProperties propertiesContent = null;
            try
            {
                Assert.DoesNotThrow(() =>
                {
                    propertiesContent = artifact.GetPropertiesForRapidReview(_user);
                }, "GetPropertiesForRapidReview must not throw errors.");
            }

            finally
            {
                artifact.Delete(_user);
                artifact.Publish(_user);
            }
        }
    }
}
