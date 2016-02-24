using Common;
using System.Linq;
using CustomAttributes;
using Model;
using Model.Factories;
using NUnit.Framework;
using System.Collections.Generic;
using Utilities;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class BreadcrumbTests
    {
        private const string STORYTELLER_BASE_URL = "/Web/#/Storyteller/";

        private IAdminStore _adminStore;
        private IStoryteller _storyteller;
        private IUser _primaryUser;
        private IUser _secondaryUser;
        private IProject _project;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            _adminStore = AdminStoreFactory.GetAdminStoreFromTestConfig();
            _storyteller = StorytellerFactory.GetStorytellerFromTestConfig();
            _primaryUser = UserFactory.CreateUserAndAddToDatabase();
            _secondaryUser = UserFactory.CreateUserAndAddToDatabase();
            _project = ProjectFactory.GetProject(_primaryUser);

            // Get a valid token for the user.
            ISession primaryUserSession = _adminStore.AddSession(_primaryUser.Username, _primaryUser.Password);
            _primaryUser.SetToken(primaryUserSession.SessionId);
            Assert.IsFalse(string.IsNullOrWhiteSpace(_primaryUser.Token.AccessControlToken), "The primary user didn't get an Access Control token!");

            ISession secondaryUserSession = _adminStore.AddSession(_secondaryUser.Username, _secondaryUser.Password);
            _secondaryUser.SetToken(secondaryUserSession.SessionId);
            Assert.IsFalse(string.IsNullOrWhiteSpace(_secondaryUser.Token.AccessControlToken), "The secondary user didn't get an Access Control token!");
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
        public void GetDefaultProcessWithAccessibleArtifactsInPath_VerifyReturnedBreadcrumb(int numberOfArtifacts)
        {
            List<IOpenApiArtifact> artifacts = _storyteller.CreateProcessArtifacts(_project, _primaryUser, numberOfArtifacts);
            List<int> artifactIds = artifacts.Select(artifact => artifact.Id).ToList();

            IProcess process = _storyteller.GetProcessWithBreadcrumb(_primaryUser, artifactIds, sendAuthorizationAsCookie: false);

            AssertBreadcrumb(numberOfArtifacts, artifacts, process);
        }

        [TestCase(3, 1, 99999999)]
        [TestCase(4, 1, 99999999)]
        [TestCase(4, 2, 99999999)]
        [TestCase(15, 13, 99999999)]
        public void GetDefaultProcessWithNonexistentArtifactInPath_VerifyReturnedBreadcrumb(int numberOfArtifacts, int nonexistentArtifactIndex, int nonexistentArtifactId)
        {
            List<IOpenApiArtifact> artifacts = _storyteller.CreateProcessArtifacts(_project, _primaryUser, numberOfArtifacts);
            List<int> artifactIds = artifacts.Select(artifact => artifact.Id).ToList();

            // Inject nonexistent artifact id into artifact ids list used for breadcrumb
            artifactIds[nonexistentArtifactIndex] = nonexistentArtifactId;
            artifacts[nonexistentArtifactIndex].Id = nonexistentArtifactId;

            IProcess process = _storyteller.GetProcessWithBreadcrumb(_primaryUser, artifactIds);

            AssertBreadcrumb(numberOfArtifacts, artifacts, process, new List<int> { nonexistentArtifactIndex});
        }

        [TestCase(4, new int[] { 1, 2 }, 99999999, Description="Test for sequential nonexistent artifacts in breadcrumb")]
        [TestCase(15, new int[] { 2, 6, 13 }, 99999999, Description = "Test for nonsequential nonexistent artifacts in breadcrumb")]
        public void GetDefaultProcessWithMultipleNonexistentArtifactsInPath_VerifyReturnedBreadcrumb(int numberOfArtifacts, int[] nonexistentArtifactIndexes, int nonexistentArtifactId)
        {
            ThrowIf.ArgumentNull(nonexistentArtifactIndexes,nameof(nonexistentArtifactIndexes));

            List<IOpenApiArtifact> artifacts = _storyteller.CreateProcessArtifacts(_project, _primaryUser, numberOfArtifacts);
            List<int> artifactIds = artifacts.Select(artifact => artifact.Id).ToList();

            // Inject nonexistent artifact id into artifact ids list used for breadcrumb
            foreach (var nonexistentArtifactIndex in nonexistentArtifactIndexes)
            {
                artifactIds[nonexistentArtifactIndex] = nonexistentArtifactId;
                artifacts[nonexistentArtifactIndex].Id = nonexistentArtifactId;
            }

            IProcess process = _storyteller.GetProcessWithBreadcrumb(_primaryUser, artifactIds);

            AssertBreadcrumb(numberOfArtifacts, artifacts, process, nonexistentArtifactIndexes.ToList());
        }

        [TestCase(3, 1)]
        [TestCase(4, 1)]
        [TestCase(4, 2)]
        [TestCase(15, 13)]
        public void GetDefaultProcessWithInaccessibleArtifactsInPath_VerifyReturnedBreadcrumb(int numberOfArtifacts, int inaccessibleArtifactIndex)
        {
            List<IOpenApiArtifact> artifacts = _storyteller.CreateProcessArtifacts(_project, _primaryUser, numberOfArtifacts);
            List<int> artifactIds = artifacts.Select(artifact => artifact.Id).ToList();

            // create and inject artifact ids created by another user
            var inaccessibleArtifact = _storyteller.CreateProcessArtifact(_project, BaseArtifactType.Process, _secondaryUser);
            artifactIds[inaccessibleArtifactIndex] = inaccessibleArtifact.Id;
            artifacts[inaccessibleArtifactIndex].Id = inaccessibleArtifact.Id;

            IProcess process = _storyteller.GetProcessWithBreadcrumb(_primaryUser, artifactIds);

            AssertBreadcrumb(numberOfArtifacts, artifacts, process, new List<int> { inaccessibleArtifactIndex });
        }

        [TestCase(4, new int[] { 1, 2 }, Description = "Test for sequential inaccessible artifacts in breadcrumb")]
        [TestCase(15, new int[] { 2, 6, 13 }, Description = "Test for nonsequential inaccessible artifacts in breadcrumb")]
        public void GetDefaultProcessWithMultipleInaccessibleArtifactInPath_VerifyReturnedBreadcrumb(int numberOfArtifacts, int[] inaccessibleArtifactIndexes)
        {
            ThrowIf.ArgumentNull(inaccessibleArtifactIndexes, nameof(inaccessibleArtifactIndexes));

            List<IOpenApiArtifact> artifacts = _storyteller.CreateProcessArtifacts(_project, _primaryUser, numberOfArtifacts);
            List<int> artifactIds = artifacts.Select(artifact => artifact.Id).ToList();

            // create and inject artifact ids created by another user
            foreach (var inaccessibleArtifactIndex in inaccessibleArtifactIndexes)
            {
                var inaccessibleArtifact = _storyteller.CreateProcessArtifact(_project, BaseArtifactType.Process, _secondaryUser);
                artifactIds[inaccessibleArtifactIndex] = inaccessibleArtifact.Id;
                artifacts[inaccessibleArtifactIndex].Id = inaccessibleArtifact.Id;
            }

            IProcess process = _storyteller.GetProcessWithBreadcrumb(_primaryUser, artifactIds);

            AssertBreadcrumb(numberOfArtifacts, artifacts, process, inaccessibleArtifactIndexes.ToList());
        }

        /// <summary>
        /// Assertions for breadcrumb tests
        /// </summary>
        /// <param name="numberOfArtifacts">The number of artifacts in the breadcrumb</param>
        /// <param name="artifacts">The list of artifact objects in the breadcrumb</param>
        /// <param name="process">The process object that was returned</param>
        /// <param name="artifactIndexes">A list of artifact indexes for nonexistent or inaccessible artifacts</param>
        private static void AssertBreadcrumb(int numberOfArtifacts, List<IOpenApiArtifact> artifacts, IProcess process, List<int> artifactIndexes = null)
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
                    const string INACCESSIBLE_ARTIFACT_NAME = "<Inaccessible>";
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
