using System.Linq;
using CustomAttributes;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using NUnit.Framework;
using System.Collections.Generic;
using Model.StorytellerModel;
using Model.StorytellerModel.Impl;
using Helper;
using System.Net;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class BasicTests
    {
        private IAdminStore _adminStore;
        private IBlueprintServer _blueprintServer;
        private IStoryteller _storyteller;
        private IUser _user;
        private IProject _project;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            _adminStore = AdminStoreFactory.GetAdminStoreFromTestConfig();
            _blueprintServer = BlueprintServerFactory.GetBlueprintServerFromTestConfig();
            _storyteller = StorytellerFactory.GetStorytellerFromTestConfig();
            _user = UserFactory.CreateUserAndAddToDatabase();
            _project = ProjectFactory.GetProject(_user);

            // Get a valid Access Control token for the user (for the new Storyteller REST calls).
            ISession session = _adminStore.AddSession(_user.Username, _user.Password);
            _user.SetToken(session.SessionId);

            Assert.IsFalse(string.IsNullOrWhiteSpace(_user.Token.AccessControlToken), "The user didn't get an Access Control token!");

            // Get a valid OpenApi token for the user (for the OpenApi artifact REST calls).
            _blueprintServer.LoginUsingBasicAuthorization(_user, string.Empty);

            Assert.IsFalse(string.IsNullOrWhiteSpace(_user.Token.OpenApiToken), "The user didn't get an OpenApi token!");
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            if (_storyteller.Artifacts != null)
            {
                // Delete or Discard all the artifacts that were added.
                var savedArtifactsList = new List<IArtifactBase>();
                foreach (var artifact in _storyteller.Artifacts.ToArray())
                {
                    if (artifact.IsPublished)
                    {
                        _storyteller.DeleteProcessArtifact(artifact, deleteChildren: true);
                    }
                    else
                    {
                        savedArtifactsList.Add(artifact);
                    }
                }
                if (savedArtifactsList.Any())
                {
                    Storyteller.DiscardProcessArtifacts(savedArtifactsList, _blueprintServer.Address, _user);
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
        [TestCase(5, 4, 2)]
        [Description("Get the default process after creating and saving a new process artifact.  Verify that the" +
                     "returned process has the same Id as the process artifact Id and that the numbers of " +
                     "shapes, links and property values are as expected.")]
        public void GetDefaultProcess_VerifyReturnedProcess(
            int defaultShapesCount, 
            int defaultLinksCount, 
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
            Assert.That(returnedProcess.PropertyValues.Count == defaultPropertyValuesCount,
                "The number of property values in a default process is {0} but {1} property values were returned.", defaultPropertyValuesCount, returnedProcess.PropertyValues.Count);
        }

        [TestCase]
        public void GetProcesses_ReturnedListContainsCreatedProcess()
        {
            IArtifact artifact = _storyteller.CreateAndSaveProcessArtifact(_project, BaseArtifactType.Process, _user);
            List<IProcess> processList = null;

            Assert.DoesNotThrow(() =>
            {
                processList = (List<IProcess>)_storyteller.GetProcesses(_user, _project.Id);
            }, "GetProcesses must not return an error.");

            // Get returned process from list of processes
            var returnedProcess = processList.Find(p => p.Name == artifact.Name);

            Assert.IsNotNull(returnedProcess, "List of processes must have newly created process, but it doesn't.");
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestCase(BaseArtifactType.Process)]
        [TestCase(BaseArtifactType.UseCase)]
        [TestCase(BaseArtifactType.UIMockup)]
        [TestCase(BaseArtifactType.UseCaseDiagram)]
        [TestCase(BaseArtifactType.GenericDiagram)]
        [TestRail(102883)]
        [Description("Create artifact, save and publish it. Search created artifact by name. Search must return created artifact.")]
        public void GetSearchArtifactResults_ReturnedListContainsCreatedArtifact(BaseArtifactType artifactType)
        {
            //Create an artifact with ArtifactType and populate all required values without properties
            var artifact = ArtifactFactory.CreateArtifact(_project, _user, artifactType);

            artifact.Save(_user);
            artifact.Publish(_user);

            try
            {
                Assert.DoesNotThrow(() =>
                {
                    var artifactsList = Artifact.SearchArtifactsByName(address: _storyteller.Address, user: _user, searchSubstring: artifact.Name);
                    Assert.IsTrue(artifactsList.Count > 0);
                }, "Couldn't find an artifact named '{0}'.", artifact.Name);
            }

            finally
            {
                artifact.Delete(_user);
                artifact.Publish(_user);
            }
        }

        [TestCase]
        [TestRail(102884)]
        [Description("Check that search artifact by name returns 10 artifacts only.")]
        public void GetSearchArtifactResults_ReturnedListHasExpectedLength()
        {
            //Create an artifact with ArtifactType and populate all required values without properties
            var artifactList = new List<IArtifact>();

            for (int i = 0; i < 12; i++)
            {
                var artifact = ArtifactFactory.CreateArtifact(_project, _user, BaseArtifactType.Actor);
                artifact.Save(_user);
                artifact.Publish(_user);
                artifactList.Add(artifact);
            }

            //Implementation of CreateArtifact uses Artifact_ prefix to name artifacts
            string searchString = "Artifact_";
            try
            {
                Assert.DoesNotThrow(() =>
                {
                    var searchResultList = Artifact.SearchArtifactsByName(address: _storyteller.Address, user: _user, searchSubstring: searchString);
                    Assert.IsTrue(searchResultList.Count == 10, "Search results must have 10 artifacts, but they have '{0}'.", searchResultList.Count);
                });
            }

            finally
            {
                foreach (var artifactToDelete in artifactList)
                {
                    artifactToDelete.Delete(_user);
                    artifactToDelete.Publish(_user);
                }
            }
        }

        [TestCase]
        [TestRail(107376)]
        [Description("change artifact name, discard")]
        public void DiscardArtifactWithChangedName_VerifyResult()
        {
            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Modify default process Name
            process.Name = "new name";

            // Update and Verify the modified process
            var changedProcess = StorytellerTestHelper.UpdateAndVerifyProcess(process, _storyteller, _user);
            var processArtifact = new Artifact(_storyteller.Address, changedProcess.Id, changedProcess.ProjectId);

            List<DiscardArtifactResult> discardResultList = null;
            Assert.DoesNotThrow(() =>
            {
                discardResultList = processArtifact.NovaDiscard(_user);
                string expectedMessage = "Successfully discarded";
                Assert.AreEqual(expectedMessage, discardResultList[0].Message, "Returned message must be {0}, but {1} was returned",
                    expectedMessage, discardResultList[0].Message);
                Assert.AreEqual((HttpStatusCode)0, discardResultList[0].ResultCode, "Returned code must be {0}, but {1} was returned",
                    (HttpStatusCode)0, discardResultList[0].ResultCode);
            }, "Must return no errors.");
        }
    }
}
