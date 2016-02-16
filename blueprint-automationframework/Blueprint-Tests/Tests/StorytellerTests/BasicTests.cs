using Common;
using System.Linq;
using CustomAttributes;
using Model;
using Model.Factories;
using Model.Impl;
using NUnit.Framework;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class BasicTests
    {
        private IAdminStore _adminStore;
        private IStoryteller _storyteller;
        private IUser _user;
        private IArtifact _artifact;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            _adminStore = AdminStoreFactory.GetAdminStoreFromTestConfig();
            _storyteller = StorytellerFactory.GetStorytellerFromTestConfig();
            _user = UserFactory.CreateUserAndAddToDatabase();

            // Get a valid token for the user.
            ISession session = _adminStore.AddSession(_user.Username, _user.Password);
            _user.SetToken(session.SessionId);

            int projectId = 1; // using default project id: 1
            var process = new Artifact()
            {
                Id = 0,
                Name = "Test Process",
                ParentId = projectId, //we can use Project as a parent
                ProjectId = projectId,
                ArtifactTypeId = _storyteller.GetProcessTypeId(user: _user, projectId: projectId)
                //ArtifactTypeId = 369 // Need to find a way to determine the artifact type id from server-side
            };

            _artifact = _storyteller.AddProcessArtifact(process, _user);

            Assert.IsFalse(string.IsNullOrWhiteSpace(_user.Token.AccessControlToken), "The user didn't get an Access Control token!");
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            if (_artifact != null)
            {
                _storyteller.DeleteProcessArtifact(_artifact, _user);
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
        [TestCase(5, 4, ProcessType.BusinessProcess)]
        public void GetDefaultProcess_VerifyReturnedProcess(int defaultShapesLength, int defaultLinksLength, ProcessType processType)
        {
            var process = _storyteller.GetProcess(_user, _artifact.Id);
        }

        [Explicit(IgnoreReasons.DeploymentNotReady)]
        [Test]
        public void GetProcesses_ReturnedListContainsCreatedProcess()
        {
            Assert.DoesNotThrow(() =>
            {
                var processList = _storyteller.GetProcesses(_user, 1);
                var results = processList.Where(p => (p.Name == _artifact.Name && p.TypePreffix == "SP")).ToList();
                Assert.IsTrue(results.Count > 0, "List of processes must have newly created process, but it doesn't.");
            });
            
        }
    }
}
