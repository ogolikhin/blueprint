using System.Linq;
using CustomAttributes;
using Model;
using Model.OpenApiModel;
using Model.Factories;
using NUnit.Framework;
using System.Collections.Generic;
using Model.StorytellerModel;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class BasicTests
    {
        private IAdminStore _adminStore;
        private IStoryteller _storyteller;
        private IUser _user;
        private IProject _project;
        private bool _deleteChildren = true;

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
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            if (_storyteller.Artifacts != null)
            {
                // TODO: implement discard artifacts for test cases that doesn't publish artifacts
                // Delete all the artifacts that were added.
                foreach (var artifact in _storyteller.Artifacts.ToArray())
                {
                    _storyteller.DeleteProcessArtifact(artifact, deleteChildren: _deleteChildren);
                }
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "processType")]
        [TestCase(5, 4, 1, 2)]
        [Description("Get the default process after creating and saving a new process artifact.  Verify that the" +
                     "returned process has the same Id as the process artifact Id and that the numbers of " +
                     "shapes, links, artifact path links and property values are as expected.")]
        public void GetDefaultProcess_VerifyReturnedProcess(
            int defaultShapesCount, 
            int defaultLinksCount, 
            int defaultArtifactPathLinksCount, 
            int defaultPropertyValuesCount)
        {
            var artifact = _storyteller.CreateAndSaveProcessArtifact(_project, BaseArtifactType.Process, _user);

            var returnedProcess = _storyteller.GetProcess(_user, artifact.Id);

            Assert.IsNotNull(returnedProcess, "The returned process was null.");

            Assert.That(returnedProcess.Id == artifact.Id,
                "The ID of the returned process was '{0}', but '{1}' was expected.", returnedProcess.Id, artifact.Id);
            Assert.That(returnedProcess.Shapes.Count == defaultShapesCount,
                "The number of shapes in a default process is {0} but {1} shapes were returned.", defaultShapesCount, returnedProcess.Shapes.Count);
            Assert.That(returnedProcess.Links.Count == defaultLinksCount,
                "The number of links in a default process is {0} but {1} links were returned.", defaultLinksCount, returnedProcess.Links.Count);
            Assert.That(returnedProcess.ArtifactPathLinks.Count == defaultArtifactPathLinksCount,
                "The number of artifact path links in a default process is {0} but {1} artifact path links were returned.", defaultArtifactPathLinksCount, returnedProcess.ArtifactPathLinks.Count);
            Assert.That(returnedProcess.PropertyValues.Count == defaultPropertyValuesCount,
                "The number of property values in a default process is {0} but {1} property values were returned.", defaultPropertyValuesCount, returnedProcess.PropertyValues.Count);

            // Publish the process so teardown can properly delete the process
            _storyteller.PublishProcess(_user, returnedProcess);
        }

        [TestCase]
        public void GetProcesses_ReturnedListContainsCreatedProcess()
        {
            IOpenApiArtifact artifact = _storyteller.CreateAndSaveProcessArtifact(_project, BaseArtifactType.Process, _user);
            List<IProcess> processList = null;

            Assert.DoesNotThrow(() =>
            {
                processList = (List<IProcess>)_storyteller.GetProcesses(_user, _project.Id);
            }, "GetProcesses must not return an error.");

            // Get returned process from list of processes
            var returnedProcess = processList.Find(p => p.Name == artifact.Name);

            Assert.IsNotNull(returnedProcess, "List of processes must have newly created process, but it doesn't.");

            // Publish the process so teardown can properly delete the process
            _storyteller.PublishProcess(_user, returnedProcess);
        }

        [TestCase]
        public void GetSearchArtifactResults_ReturnedListContainsCreatedArtifact()
        {
            IOpenApiArtifact artifact = _storyteller.CreateAndSaveProcessArtifact(_project, BaseArtifactType.Process, _user);

            artifact.Publish(_user);

            Assert.DoesNotThrow(() =>
            {
                var artifactsList = artifact.SearchArtifactsByName(user: _user, searchSubstring: artifact.Name);
                Assert.IsTrue(artifactsList.Count > 0);
            }, "Couldn't find an artifact named '{0}'.", artifact.Name);
        }
    }
}
