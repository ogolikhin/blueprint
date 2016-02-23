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
        private const string STORYTELLER_BASE_URL = "/Web/#/Storyteller/{0}/";

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
        }

        #endregion Setup and Cleanup

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase(3)]
        public void GetDefaultProcessWithAccessibleArtifactsInPath_VerifyReturnedBreadcrumb(int numberOfArtifacts)
        {
            List<IOpenApiArtifact>  artifacts = _storyteller.CreateProcessArtifacts(_storyteller, _project, _primaryUser, numberOfArtifacts);
            List<int> artifactIds = artifacts.Select(artifact => artifact.Id).ToList();

            IProcess process = _storyteller.GetProcessWithBreadcrumb(_primaryUser, artifactIds);

            AssertBreadcrumb(numberOfArtifacts, artifacts, process);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase(3, 1, 99999999)]
        public void GetDefaultProcessWithNonexistentArtifactInPath_VerifyReturnedBreadcrumb(int numberOfArtifacts, int nonexistentArtifactIndex, int nonexistentArtifactId)
        {
            List<IOpenApiArtifact> artifacts = _storyteller.CreateProcessArtifacts(_storyteller, _project, _primaryUser, numberOfArtifacts);
            List<int> artifactIds = artifacts.Select(artifact => artifact.Id).ToList();

            artifactIds[nonexistentArtifactIndex] = nonexistentArtifactId;

            IProcess process = _storyteller.GetProcessWithBreadcrumb(_primaryUser, artifactIds);

            // Assert nonexistant artifact in artifact path links (breadcrumb) as expected
            Assert.That(process.ArtifactPathLinks[nonexistentArtifactIndex].Id == artifactIds[nonexistentArtifactIndex], I18NHelper.FormatInvariant("Expected nonexistent artifact Id is {0} but artifact Id {1} was returned", artifactIds[nonexistentArtifactIndex], process.ArtifactPathLinks[nonexistentArtifactIndex].Id));
            Assert.That(process.ArtifactPathLinks[nonexistentArtifactIndex].Name == "<Inaccessible>", I18NHelper.FormatInvariant("Expected nonexistent artifact Name is {0} but artifact name {1} was returned", "<Inaccessible>", process.ArtifactPathLinks[nonexistentArtifactIndex].Name));
            Assert.That(process.ArtifactPathLinks[nonexistentArtifactIndex].Link == null, I18NHelper.FormatInvariant("Expected nonexistent artifact link is {0} but artifact link {1} was returned", null, process.ArtifactPathLinks[nonexistentArtifactIndex].Link));

            AssertBreadcrumb(numberOfArtifacts, artifacts, process, nonexistentArtifactIndex);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase(3, 1)]
        public void GetDefaultProcessWithInaccessibleArtifactInPath_VerifyReturnedBreadcrumb(int numberOfArtifacts, int inaccessibleArtifactIndex)
        {
            List<IOpenApiArtifact> artifacts = _storyteller.CreateProcessArtifacts(_storyteller, _project, _primaryUser, numberOfArtifacts);
            List<int> artifactIds = artifacts.Select(artifact => artifact.Id).ToList();

            var inaccessibleArtifact = _storyteller.CreateProcessArtifact(_project, BaseArtifactType.Process, _secondaryUser);

            artifactIds[inaccessibleArtifactIndex] = inaccessibleArtifact.Id;

            IProcess process = _storyteller.GetProcessWithBreadcrumb(_primaryUser, artifactIds);

            // Assert inaccessible artifact in artifact path links (breadcrumb) as expected
            Assert.That(process.ArtifactPathLinks[inaccessibleArtifactIndex].Id == artifactIds[inaccessibleArtifactIndex], I18NHelper.FormatInvariant("Expected inaccessible artifact Id is {0} but artifact Id {1} was returned", artifactIds[inaccessibleArtifactIndex], process.ArtifactPathLinks[inaccessibleArtifactIndex].Id));
            Assert.That(process.ArtifactPathLinks[inaccessibleArtifactIndex].Name == "<Inaccessible>", I18NHelper.FormatInvariant("Expected inaccessible artifact Name is {0} but artifact name {1} was returned", "<Inaccessible>", process.ArtifactPathLinks[inaccessibleArtifactIndex].Name));
            Assert.That(process.ArtifactPathLinks[inaccessibleArtifactIndex].Link == null, I18NHelper.FormatInvariant("Expected inaccessible artifact link is {0} but artifact link {1} was returned", null, process.ArtifactPathLinks[inaccessibleArtifactIndex].Link));

            AssertBreadcrumb(numberOfArtifacts, artifacts, process, inaccessibleArtifactIndex);
        }

        private static void AssertBreadcrumb(int numberOfArtifacts, List<IOpenApiArtifact> artifacts, IProcess process, int? artifactIndex = null)
        {
            Assert.IsNotNull(process, "The returned process was null.");
            Assert.That(process.Id == artifacts.Last().Id);

            // Assert final process in artifact path links (breadcrumb) as expected
            Assert.That(process.ArtifactPathLinks.Last().Id == process.Id, I18NHelper.FormatInvariant("Expected final process artifact Id is {0} but artifact Id {1} was returned", process.Id, process.ArtifactPathLinks.Last().Id));
            Assert.That(process.ArtifactPathLinks.Last().Name == process.Name, I18NHelper.FormatInvariant("Expected final process artifact Name is {0} but artifact name {1} was returned", process.Name, process.ArtifactPathLinks.Last().Name));
            Assert.That(process.ArtifactPathLinks.Last().Link == null, I18NHelper.FormatInvariant("Expected final process artifact link is {0} but artifact link {1} was returned", null, process.ArtifactPathLinks.Last().Link));

            for (int i = 0; i < numberOfArtifacts - 1; i++)
            {
                if (artifactIndex != null && i!= artifactIndex)
                {
                    // Assert all other artifacts in artifact path links (breadcrumb) as expected
                    Assert.That(process.ArtifactPathLinks[i].Id == artifacts[i].Id, I18NHelper.FormatInvariant("Expected artifact Id is {0} but artifact Id {1} was returned", artifacts[i].Id, process.ArtifactPathLinks[i].Id));
                    Assert.That(process.ArtifactPathLinks[i].Name == artifacts[i].Name, I18NHelper.FormatInvariant("Expected artifact Name is {0} but artifact name {1} was returned", artifacts[i].Name, process.ArtifactPathLinks[i].Name));
                    Assert.That(process.ArtifactPathLinks[i].Link == I18NHelper.FormatInvariant(STORYTELLER_BASE_URL, artifacts[i].Id), I18NHelper.FormatInvariant("Expected artifact link is {0} but artifact link {1} was returned", I18NHelper.FormatInvariant(STORYTELLER_BASE_URL, artifacts[i].Id), process.ArtifactPathLinks[i].Link));
                }
            }
        }
    }
}
