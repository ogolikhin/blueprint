using Common;
using CustomAttributes;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using NUnit.Framework;
using System.Collections.Generic;

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

        [TestCase(BaseArtifactType.Glossary)]
        [TestCase(BaseArtifactType.UseCase)]
        [TestCase(BaseArtifactType.BusinessProcess)]
        [TestCase(BaseArtifactType.DomainDiagram)]
        [TestCase(BaseArtifactType.GenericDiagram)]
        [TestCase(BaseArtifactType.Storyboard)]
        [TestCase(BaseArtifactType.UseCaseDiagram)]
        [TestCase(BaseArtifactType.UIMockup)]
        [TestRail(01)]
        [Description("")]
        public void GetArtifactContentForRapidReview_VerifyResults(BaseArtifactType artifactType)
        {
            var artifact = ArtifactFactory.CreateArtifact(_project, _user, artifactType: artifactType);

            artifact.Save(_user);
            artifact.Publish(_user);

            string artifactContent = string.Empty;
            string propertiesContent = string.Empty;
            try
            {
                Assert.DoesNotThrow(() =>
                {
                    artifactContent = artifact.GetContentForRapidReview(_user);
                    propertiesContent = artifact.GetPropertiesForRapidReview(_user);
                }, "must not throw errors.");
                //Assert.AreEqual("diagram", artifactContent, "error");
            }

            finally
            {
                artifact.Delete(_user);
                artifact.Publish(_user);
            }
        }
    }
}
