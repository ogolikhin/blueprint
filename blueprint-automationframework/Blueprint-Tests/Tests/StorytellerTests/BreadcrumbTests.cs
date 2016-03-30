using Common;
using System.Linq;
using CustomAttributes;
using Model;
using Model.Factories;
using NUnit.Framework;
using System.Collections.Generic;
using Model.OpenApiModel;
using Model.StorytellerModel;
using Utilities;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class BreadcrumbTests
    {
        private const string STORYTELLER_BASE_URL = "/Web/#/Storyteller/";
        private const string INACCESSIBLE_ARTIFACT_NAME = "<Inaccessible>";
        private const int NONEXISTENT_ARTIFACT_ID = 99999999;

        private IAdminStore _adminStore;
        private IBlueprintServer _blueprintServer;
        private IStoryteller _storyteller;
        private IUser _primaryUser;
        private IUser _secondaryUser;
        private IProject _project;
        private bool _deleteChildren = true;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            _adminStore = AdminStoreFactory.GetAdminStoreFromTestConfig();
            _blueprintServer = BlueprintServerFactory.GetBlueprintServerFromTestConfig();
            _storyteller = StorytellerFactory.GetStorytellerFromTestConfig();
            _primaryUser = UserFactory.CreateUserAndAddToDatabase();
            _secondaryUser = UserFactory.CreateUserAndAddToDatabase();
            _project = ProjectFactory.GetProject(_primaryUser);

            // Get a valid Access Control token for the user (for the new Storyteller REST calls).
            ISession primaryUserSession = _adminStore.AddSession(_primaryUser.Username, _primaryUser.Password);
            _primaryUser.SetToken(primaryUserSession.SessionId);
            Assert.IsFalse(string.IsNullOrWhiteSpace(_primaryUser.Token.AccessControlToken), "The primary user didn't get an Access Control token!");

            ISession secondaryUserSession = _adminStore.AddSession(_secondaryUser.Username, _secondaryUser.Password);
            _secondaryUser.SetToken(secondaryUserSession.SessionId);
            Assert.IsFalse(string.IsNullOrWhiteSpace(_secondaryUser.Token.AccessControlToken), "The secondary user didn't get an Access Control token!");

            // Get a valid OpenApi token for the user (for the OpenApi artifact REST calls).
            _blueprintServer.LoginUsingBasicAuthorization(_primaryUser, string.Empty);
            _blueprintServer.LoginUsingBasicAuthorization(_secondaryUser, string.Empty);

            Assert.IsFalse(string.IsNullOrWhiteSpace(_primaryUser.Token.OpenApiToken), "The primary user didn't get an OpenApi token!");
            Assert.IsFalse(string.IsNullOrWhiteSpace(_secondaryUser.Token.OpenApiToken), "The secondary user didn't get an OpenApi token!");
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            if (_storyteller.Artifacts != null)
            {
                // Delete all the artifacts that were added.
                foreach (var artifact in _storyteller.Artifacts.ToArray())
                {
                    if (artifact.Id != NONEXISTENT_ARTIFACT_ID)
                    {
                        _storyteller.DeleteProcessArtifact(artifact, deleteChildren: _deleteChildren);
                        Logger.WriteDebug("deleting process artifact which exist!");
                    }

                    if (artifact.IsPublished)
                    {
                        _storyteller.DeleteProcessArtifact(artifact, deleteChildren: _deleteChildren);
                        Logger.WriteDebug("deleting process artifact which is published!");

                    }
                    else
                    {
                        _storyteller.DiscardProcessArtifact(artifact);
                        Logger.WriteDebug("discarding process artifact which is saved!");

                    }
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

            if (_primaryUser != null)
            {
                _primaryUser.DeleteUser();
                _primaryUser = null;
            }

            if (_secondaryUser != null)
            {
                _secondaryUser.DeleteUser();
                _secondaryUser = null;
            }
        }

        #endregion Setup and Cleanup

        [TestCase(3)]
        [TestCase(15)]
        [Description("Get the default process when accessible artifacts are in the GET url path. Verify" +
                     "that the returned artifact path links contains all process Ids in the url path.")]
        public void GetDefaultProcessWithAccessibleArtifactsInPath_VerifyReturnedBreadcrumb(int numberOfArtifacts)
        {
            List<IOpenApiArtifact> artifacts = _storyteller.CreateAndSaveProcessArtifacts(_project, _primaryUser, numberOfArtifacts);

            List<int> artifactIds = artifacts.Select(artifact => artifact.Id).ToList();

            IProcess process = _storyteller.GetProcessWithBreadcrumb(_primaryUser, artifactIds, sendAuthorizationAsCookie: false);

            AssertBreadcrumb(numberOfArtifacts, artifacts, process);


        }

        [TestCase(3, 1)]
        [TestCase(4, 1)]
        [TestCase(4, 2)]
        [TestCase(15, 13)]
        [Description("Get the default process with a non-existent artifact in the GET url path.  Verify" +
                     "that the non-existent artifact is marked as <Inaccessible> in the returned " +
                     "artifact path links. ")]
        public void GetDefaultProcessWithNonexistentArtifactInPath_VerifyReturnedBreadcrumb(
            int numberOfArtifacts, 
            int nonexistentArtifactIndex)
        {
            //List<IOpenApiArtifact> artifacts = _storyteller.CreateAndPublishProcessArtifacts(_project, _primaryUser, numberOfArtifacts);
            List<IOpenApiArtifact> artifacts = _storyteller.CreateAndSaveProcessArtifacts(_project, _primaryUser, numberOfArtifacts);

            List<int> artifactIds = artifacts.Select(artifact => artifact.Id).ToList();

            // Inject nonexistent artifact id into artifact ids list used for breadcrumb
            artifactIds[nonexistentArtifactIndex] = NONEXISTENT_ARTIFACT_ID;
            artifacts[nonexistentArtifactIndex].Id = NONEXISTENT_ARTIFACT_ID;

            IProcess process = _storyteller.GetProcessWithBreadcrumb(_primaryUser, artifactIds);

            AssertBreadcrumb(numberOfArtifacts, artifacts, process, new List<int> { nonexistentArtifactIndex});
        }

        [TestCase(4, new[] { 1, 2 }, Description ="Test for sequential nonexistent artifacts in breadcrumb")]
        [TestCase(15, new[] { 2, 6, 13 }, Description = "Test for nonsequential nonexistent artifacts in breadcrumb")]
        [Description("Get the default process with multiple non-existent artifacts in the GET url path. " +
                     "Verify that the non-existent artifacts are marked as <Inaccessible> in the returned " +
                     "artifact path links. ")]
        public void GetDefaultProcessWithMultipleNonexistentArtifactsInPath_VerifyReturnedBreadcrumb(
            int numberOfArtifacts, 
            int[] nonexistentArtifactIndexes)
        {
            ThrowIf.ArgumentNull(nonexistentArtifactIndexes,nameof(nonexistentArtifactIndexes));

            List<IOpenApiArtifact> artifacts = _storyteller.CreateAndPublishProcessArtifacts(_project, _primaryUser, numberOfArtifacts);
            List<int> artifactIds = artifacts.Select(artifact => artifact.Id).ToList();

            // Inject nonexistent artifact id into artifact ids list used for breadcrumb
            foreach (var nonexistentArtifactIndex in nonexistentArtifactIndexes)
            {
                artifactIds[nonexistentArtifactIndex] = NONEXISTENT_ARTIFACT_ID;
                artifacts[nonexistentArtifactIndex].Id = NONEXISTENT_ARTIFACT_ID;
            }

            IProcess process = _storyteller.GetProcessWithBreadcrumb(_primaryUser, artifactIds);

            AssertBreadcrumb(numberOfArtifacts, artifacts, process, nonexistentArtifactIndexes.ToList());
        }

        [TestCase(3, 1)]
        [TestCase(4, 1)]
        [TestCase(4, 2)]
        [TestCase(15, 13)]
        [Description("Get the default process with an inaccessible artifact in the GET url path. " +
                     "Verify that the inaccessible artifact is marked as <Inaccessible> in the returned " +
                     "artifact path links. ")]
        public void GetDefaultProcessWithInaccessibleArtifactsInPath_VerifyReturnedBreadcrumb(
            int numberOfArtifacts, 
            int inaccessibleArtifactIndex)
        {
            List<IOpenApiArtifact> artifacts = _storyteller.CreateAndPublishProcessArtifacts(_project, _primaryUser, numberOfArtifacts);
            List<int> artifactIds = artifacts.Select(artifact => artifact.Id).ToList();

            // create and inject artifact ids created by another user
            var inaccessibleArtifact = _storyteller.CreateAndSaveProcessArtifact(_project, BaseArtifactType.Process, _secondaryUser);
            artifactIds[inaccessibleArtifactIndex] = inaccessibleArtifact.Id;
            artifacts[inaccessibleArtifactIndex] = inaccessibleArtifact;

            IProcess process = _storyteller.GetProcessWithBreadcrumb(_primaryUser, artifactIds);

            AssertBreadcrumb(numberOfArtifacts, artifacts, process, new List<int> { inaccessibleArtifactIndex });

            // Must be published after assert so that the artifact is deletable in teardown by primary user
            inaccessibleArtifact.Publish(_secondaryUser);
        }

        [TestCase(4, new int[] { 1, 2 }, Description = "Test for sequential inaccessible artifacts in breadcrumb")]
        [TestCase(15, new int[] { 2, 6, 13 }, Description = "Test for nonsequential inaccessible artifacts in breadcrumb")]
        [Description("Get the default process with multiple inaccessible artifacts in the GET url path. " +
                     "Verify that the inaccessible artifacts are marked as <Inaccessible> in the returned " +
                     "artifact path links. ")]
        public void GetDefaultProcessWithMultipleInaccessibleArtifactInPath_VerifyReturnedBreadcrumb(
            int numberOfArtifacts, 
            int[] inaccessibleArtifactIndexes)
        {
            ThrowIf.ArgumentNull(inaccessibleArtifactIndexes, nameof(inaccessibleArtifactIndexes));

            List<IOpenApiArtifact> artifacts = _storyteller.CreateAndPublishProcessArtifacts(_project, _primaryUser, numberOfArtifacts);
            List<int> artifactIds = artifacts.Select(artifact => artifact.Id).ToList();

            // create and inject artifact ids created by another user
            foreach (var inaccessibleArtifactIndex in inaccessibleArtifactIndexes)
            {
                var inaccessibleArtifact = _storyteller.CreateAndSaveProcessArtifact(_project, BaseArtifactType.Process, _secondaryUser);
                artifactIds[inaccessibleArtifactIndex] = inaccessibleArtifact.Id;
                artifacts[inaccessibleArtifactIndex] = inaccessibleArtifact;
            }

            IProcess process = _storyteller.GetProcessWithBreadcrumb(_primaryUser, artifactIds);

            AssertBreadcrumb(numberOfArtifacts, artifacts, process, inaccessibleArtifactIndexes.ToList());

            // Must be published after assert so that the artifact is deletable in teardown by primary user
            foreach (var inaccessibleArtifactIndex in inaccessibleArtifactIndexes)
            {
                artifacts[inaccessibleArtifactIndex].Publish(_secondaryUser);
            }
        }

        /// <summary>
        /// Assertions for breadcrumb tests
        /// </summary>
        /// <param name="numberOfArtifacts">The number of artifacts in the breadcrumb</param>
        /// <param name="artifacts">The list of artifact objects in the breadcrumb</param>
        /// <param name="process">The process object that was returned</param>
        /// <param name="artifactIndexes">A list of artifact indexes for nonexistent or inaccessible artifacts</param>
        private static void AssertBreadcrumb(int numberOfArtifacts, IReadOnlyList<IOpenApiArtifact> artifacts, IProcess process, ICollection<int> artifactIndexes = null)
        {
            Assert.IsNotNull(process, "The returned process was null.");
            Assert.That(process.Id == artifacts.Last().Id, "Expected process artifact Id is {0} but artifact Id {1} was returned", artifacts.Last().Id, process.Id);

            // Assert final process in artifact path links (breadcrumb) as expected
            Assert.That(process.ArtifactPathLinks.Last().Id == process.Id,
                "Expected final process artifact Id is {0} but artifact Id {1} was returned", process.Id, process.ArtifactPathLinks.Last().Id);
            Assert.That(process.ArtifactPathLinks.Last().Name == process.Name,
                "Expected final process artifact Name is {0} but artifact name {1} was returned", process.Name, process.ArtifactPathLinks.Last().Name);
            Assert.That(process.ArtifactPathLinks.Last().Link == null,
                "Expected final process artifact link is {0} but artifact link {1} was returned", null, process.ArtifactPathLinks.Last().Link);

            string link = STORYTELLER_BASE_URL;

            // Assert all other artifacts in artifact path links (breadcrumb) as expected
            for (int i = 0; i < numberOfArtifacts - 1; i++)
            {
                link = I18NHelper.FormatInvariant("{0}{1}/", link, artifacts[i].Id);

                Assert.That(process.ArtifactPathLinks[i].Id == artifacts[i].Id,
                    "Expected artifact Id is {0} but artifact Id {1} was returned", artifacts[i].Id, process.ArtifactPathLinks[i].Id);

                if (artifactIndexes !=null && artifactIndexes.Contains(i))
                {
                    // Name is "<Inaccessible>" for nonexistent/inaccessible artifact
                    Assert.That(process.ArtifactPathLinks[i].Name == INACCESSIBLE_ARTIFACT_NAME,
                        "Expected artifact Name is {0} but artifact name {1} was returned", INACCESSIBLE_ARTIFACT_NAME, process.ArtifactPathLinks[i].Name);
                    // Link is null for nonexistent/inaccessible artifact
                    Assert.IsNull(process.ArtifactPathLinks[i].Link,
                        "Artifact Link for '{0}' should be null, but artifact link '{1}' was returned", link, process.ArtifactPathLinks[i].Link);
                }
                else
                {
                    Assert.That(process.ArtifactPathLinks[i].Name == artifacts[i].Name,
                        "Expected artifact Name is {0} but artifact name {1} was returned", artifacts[i].Name, process.ArtifactPathLinks[i].Name);
                    Assert.That(process.ArtifactPathLinks[i].Link == link,
                        "Expected artifact link is {0} but artifact link {1} was returned", link, process.ArtifactPathLinks[i].Link);
                }
            }
        }
    }
}
