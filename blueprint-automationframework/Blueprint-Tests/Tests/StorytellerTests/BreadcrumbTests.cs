using Common;
using System.Linq;
using System.Collections;
using CustomAttributes;
using Model;
using Model.Factories;
using Model.Impl;
using NUnit.Framework;
using System.Collections.Generic;
using Helper;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    [Explicit(IgnoreReasons.DeploymentNotReady)]
    public class BreadcrumbTests
    {
        private IAdminStore _adminStore;
        private IStoryteller _storyteller;
        private IUser _user;
        private IProject _project;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            _adminStore = AdminStoreFactory.GetAdminStoreFromTestConfig();
            _storyteller = StorytellerFactory.GetStorytellerFromTestConfig();
            _user = UserFactory.CreateUserAndAddToDatabase();
            _project = ProjectFactory.GetProject(_user);

            // Get a valid token for the user.
            ISession session = _adminStore.AddSession(_user.Username, _user.Password);
            _user.SetToken(session.SessionId);
            Assert.IsFalse(string.IsNullOrWhiteSpace(_user.Token.AccessControlToken), "The user didn't get an Access Control token!");

            Assert.IsFalse(string.IsNullOrWhiteSpace(_user.Token.AccessControlToken), "The user didn't get an Access Control token!");
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            if (_storyteller.Artifacts != null)
            {
                // TODO:    Add functionality to Artifact to discard unpublished artifacts
                // Delete all the artifacts that were added.
                //foreach (var artifact in _storyteller.Artifacts)
                //{ 
                //    _storyteller.DeleteProcessArtifact(artifact, _user);
                //}
            }

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

        #endregion Setup and Cleanup

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase(3)]
        public void GetDefaultProcessWithValidPaths_VerifyReturnedBreadcrumb(int numberOfArtifacts)
        {
            List<IOpenApiArtifact>  artifacts = _storyteller.CreateProcessArtifacts(_storyteller, _project, _user, numberOfArtifacts);
            List<int> artifactIds = artifacts.Select(artifact => artifact.Id).ToList();

            IProcess process = _storyteller.GetProcessWithBreadcrumb(_user, artifactIds);

            Assert.IsNotNull(process, "The returned process was null.");
        }
    }
}
