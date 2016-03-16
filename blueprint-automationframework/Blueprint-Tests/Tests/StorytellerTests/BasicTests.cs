﻿using System.Linq;
using CustomAttributes;
using Model;
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
        private IOpenApiArtifact _artifact;
        private bool _deleteChildren = false;

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

            // Create and publish artifact for test.
            _artifact = ArtifactFactory.CreateOpenApiArtifact(_project, _user, BaseArtifactType.Document);
            _artifact.Save(_user);
            _artifact.Publish(_user);
            Assert.IsTrue(_artifact.IsArtifactPublished(_user), "Artifact wasn't published!");
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
                    _storyteller.DeleteProcessArtifact(artifact, _user, deleteChildren: _deleteChildren);
                }
            }

            if (_artifact != null)
            {
                _artifact.Delete(_user);
                _artifact.Publish(_user);
                _artifact = null;
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
        [TestCase(5, 4, 1, 2, ProcessType.BusinessProcess)]
        [Description("Get the default process after creating and saving a new process artifact.  Verify that the" +
                     "returned process has the same Id as the process artifact Id and that the numbers of " +
                     "shapes, links, artifact path links and property values are as expected.")]
        public void GetDefaultProcess_VerifyReturnedProcess(
            int defaultShapesCount, 
            int defaultLinksCount, 
            int defaultArtifactPathLinksCount, 
            int defaultPropertyValuesCount, 
            ProcessType processType)
        {
            var artifact = _storyteller.CreateAndSaveProcessArtifact(_project, BaseArtifactType.Process, _user);

            var process = _storyteller.GetProcess(_user, artifact.Id);

            Assert.IsNotNull(process, "The returned process was null.");

            Assert.That(process.Id == artifact.Id,
                "The ID of the returned process was '{0}', but '{1}' was expected.", process.Id, artifact.Id);
            Assert.That(process.Shapes.Count == defaultShapesCount,
                "The number of shapes in a default process is {0} but {1} shapes were returned.", defaultShapesCount, process.Shapes.Count);
            Assert.That(process.Links.Count == defaultLinksCount,
                "The number of links in a default process is {0} but {1} links were returned.", defaultLinksCount, process.Links.Count);
            Assert.That(process.ArtifactPathLinks.Count == defaultArtifactPathLinksCount,
                "The number of artifact path links in a default process is {0} but {1} artifact path links were returned.", defaultArtifactPathLinksCount, process.ArtifactPathLinks.Count);
            Assert.That(process.PropertyValues.Count == defaultPropertyValuesCount,
                "The number of property values in a default process is {0} but {1} property values were returned.", defaultPropertyValuesCount, process.PropertyValues.Count);
        }

        [TestCase]
        public void GetProcesses_ReturnedListContainsCreatedProcess()
        {
            IOpenApiArtifact artifact = _storyteller.CreateAndSaveProcessArtifact(_project, BaseArtifactType.Process, _user);
            IList<IProcess> processList = null;

            Assert.DoesNotThrow(() =>
            {
                processList = _storyteller.GetProcesses(_user, 1);
            }, "GetProcesses must not return an error.");

            var results = processList.Where(p => (p.Name == artifact.Name)).ToList();
            Assert.IsTrue(results.Count > 0, "List of processes must have newly created process, but it doesn't.");
        }

        [TestCase]
        public void GetSearchArtifactResults_ReturnedListContainsCreatedArtifact()
        {
            Assert.DoesNotThrow(() =>
            {
                var artifactsList = _artifact.SearchArtifactsByName(user: _user, searchSubstring: _artifact.Name);
                Assert.IsTrue(artifactsList.Count > 0);
            }, "Couldn't find an artifact named '{0}'.", _artifact.Name);
        }
    }
}
