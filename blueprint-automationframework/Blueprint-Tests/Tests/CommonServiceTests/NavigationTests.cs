using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace CommonServiceTests
{
    [TestFixture]
    [Category(Categories.Navigation)]
    public class NavigationTests
    {
        // TODO This will need to be updated with the value that cannot does not exist in the system 
        //Non-existence artifact Id sample
        private const int NONEXISTENT_ARTIFACT_ID = 99999999;
        //Invalid process artifact Id sample
        private const int INVALID_ID = -33;

        private const int MAXIUM_ALLOWABLE_NAVIGATION = 23;
        private const string INVALID_TOKEN = "Invalid_Token_value";

        private IAdminStore _adminStore;
        private IBlueprintServer _blueprintServer;
        private IUser _primaryUser;
        private IUser _secondaryUser;
        private IProject _project;
        private List<IArtifact> _artifacts;
        private IList<int> _invalidList;
        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            _invalidList = new List<int>() { NONEXISTENT_ARTIFACT_ID, INVALID_ID }.AsReadOnly();
            _adminStore = AdminStoreFactory.GetAdminStoreFromTestConfig();
            _blueprintServer = BlueprintServerFactory.GetBlueprintServerFromTestConfig();
            _primaryUser = UserFactory.CreateUserAndAddToDatabase();
            _secondaryUser = UserFactory.CreateUserAndAddToDatabase();
            _project = ProjectFactory.GetProject(_primaryUser, shouldRetrievePropertyTypes: true);

            // Get a valid Access Control token for the user (for the new Storyteller REST calls).
            _adminStore.AddSession(_primaryUser);
            Assert.IsFalse(string.IsNullOrWhiteSpace(_primaryUser.Token.AccessControlToken), "The primary user didn't get an Access Control token!");
            _adminStore.AddSession(_secondaryUser);
            Assert.IsFalse(string.IsNullOrWhiteSpace(_secondaryUser.Token.AccessControlToken), "The secondary user didn't get an Access Control token!");

            // Get a valid OpenApi token for the user (for the OpenApi artifact REST calls).
            _blueprintServer.LoginUsingBasicAuthorization(_primaryUser, string.Empty);
            Assert.IsFalse(string.IsNullOrWhiteSpace(_primaryUser.Token.OpenApiToken), "The primary user didn't get an OpenApi token!");
            _blueprintServer.LoginUsingBasicAuthorization(_secondaryUser, string.Empty);
            Assert.IsFalse(string.IsNullOrWhiteSpace(_secondaryUser.Token.OpenApiToken), "The secondary user didn't get an OpenApi token!");
        }

        [SetUp]
        public void SetUp()
        {
            _artifacts = new List<IArtifact>();
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

        [TearDown]
        public void Teardown()
        {
            if (_artifacts != null)
            {
                // Delete or Discard all the artifacts that were added.
                var savedArtifactsListPrimaryUser = new List<IArtifactBase>();
                var savedArtifactsListSecondaryUser = new List<IArtifactBase>();
                foreach (var artifact in _artifacts.ToArray())
                {
                    if (!_invalidList.Contains(artifact.Id) && artifact.IsPublished)
                    {
                        Artifact.DeleteArtifact(artifact, artifact.CreatedBy, deleteChildren: true);
                    }

                    if (!_invalidList.Contains(artifact.Id) && !artifact.IsPublished && artifact.CreatedBy.Equals(_primaryUser))
                    {
                        savedArtifactsListPrimaryUser.Add(artifact);
                    }

                    if (!_invalidList.Contains(artifact.Id) && !artifact.IsPublished && artifact.CreatedBy.Equals(_secondaryUser))
                    {
                        savedArtifactsListSecondaryUser.Add(artifact);
                    }
                }

                if (savedArtifactsListPrimaryUser.Any())
                {
                    OpenApiArtifact.DiscardArtifacts(savedArtifactsListPrimaryUser, _blueprintServer.Address, _primaryUser);
                }

                if (savedArtifactsListSecondaryUser.Any())
                {
                    OpenApiArtifact.DiscardArtifacts(savedArtifactsListSecondaryUser, _blueprintServer.Address, _secondaryUser);
                }

                // Clear all possible List Items
                savedArtifactsListPrimaryUser.Clear();
                savedArtifactsListSecondaryUser.Clear();
                _artifacts.Clear();
            }
        }

        #endregion Setup and Cleanup

        #region Tests

        [TestRail(107167)]
        [TestCase]
        [Description("Get the navigation with all available artifact types for the project in the url path " +
            "which are accessible by the user. Verify that the returned artifact reference lists.")]
        public void GetNavigationWithAllAccessibleArtifactTypes_VerifyReturnedNavigation()
        {
            //Create artifacts with distinct available artifactTypes 
            var baseArtifactTypes = _project.ArtifactTypes.ConvertAll(o => o.BaseArtifactType);

            foreach (var baseArtifactType in baseArtifactTypes)
            {
                var artifact = ArtifactFactory.CreateArtifact(project: _project, user: _primaryUser,
                artifactType: baseArtifactType);
                Artifact.SaveArtifact(artifactToSave: artifact, user: _primaryUser);

                //Add an artifact to artifact list for navigation call
                _artifacts.Add(artifact);
            }

            //Get Navigation
            var resultArtifactReferenceList = _artifacts.First().GetNavigation(_primaryUser, _artifacts);

            //Navigation Assertions
            CommonServiceHelper.VerifyNavigation(_project, _primaryUser, resultArtifactReferenceList, _artifacts);
        }

        [TestRail(107168)]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase]
        [Description("Get the navigation with maxium allowable number of artifacts in the url path. " +
            "Verify the expected error from the call.")]
        public void GetNavigationWithMaximumArtifacts_VerifyExpectedError()
        {
            //Create an artifact with process artifact type
            var artifact = ArtifactFactory.CreateArtifact(project: _project, user: _primaryUser,
                artifactType: BaseArtifactType.Process);
            Artifact.SaveArtifact(artifactToSave: artifact, user: _primaryUser);

            //Add the same artifact repeatedly in the artifact list to create a navigation list which exceeds the maximum
            //allowable number of artifacts
            for (int i =0; i< MAXIUM_ALLOWABLE_NAVIGATION; i++)
            {
                var nonExistingArtifact = ArtifactFactory.CreateArtifact(_project, _primaryUser, BaseArtifactType.Actor);
                nonExistingArtifact.Id = NONEXISTENT_ARTIFACT_ID;
                _artifacts.Add(nonExistingArtifact);
            }

            //Add the artifact at the end of artifact list for navigation call
            _artifacts.Add(artifact);

            //Get Navigation
            var resultArtifactReferenceList = artifact.GetNavigation(_primaryUser, _artifacts);

            //Navigation Assertions
            CommonServiceHelper.VerifyNavigation(_project, _primaryUser, resultArtifactReferenceList, _artifacts);
        }

        [TestRail(107169)]
        [TestCase]
        [Description("Get the navigation with invalid artifact ID data in the url path. " +
            "Verify the Not Found exception.")]
        public void GetNavigationWithInvalidData_VerifyNotFoundException()
        {
            //Create invalid artifact
            var invalidArtifact = ArtifactFactory.CreateArtifact(_project, _primaryUser, BaseArtifactType.Actor);
            invalidArtifact.Id = INVALID_ID;

            //Add the artifact to artifact list for navigation call
            _artifacts.Add(invalidArtifact);

            //Get Navigation and check the Not Found exception
            Assert.Throws<Http404NotFoundException>(() =>
            {
                invalidArtifact.GetNavigation(_primaryUser, _artifacts);
            }, "The GET /navigation endpoint should return 404 NotFound when we pass an invalid artifact ID in the url!");
        }

        [TestRail(107170)]
        [TestCase(1)]
        [TestCase(3)]
        [Description("Get the navigation with process artifact(s) in the url path are accessible by the user." +
             "Verify that the returned artifact reference lists.")]
        public void GetNavigationWithAccessibleProcessArtifacts_VerifyReturnedNavigation(int numberOfArtifacts)
        {
            //Create artifact(s) with process artifact type and add to artifact list for navigation call
            for (int i=0; i < numberOfArtifacts; i++)
            {
                var artifact = ArtifactFactory.CreateArtifact(project: _project, user: _primaryUser,
                artifactType: BaseArtifactType.Process);
                Artifact.SaveArtifact(artifactToSave: artifact, user: _primaryUser);
                _artifacts.Add(artifact);
            }

            //Get Navigation
            var resultArtifactReferenceList = _artifacts.First().GetNavigation(_primaryUser, _artifacts);

            //Navigation Assertions
            CommonServiceHelper.VerifyNavigation(_project, _primaryUser, resultArtifactReferenceList, _artifacts);
        }

        [TestRail(107171)]
        [TestCase(2, new[] { 1 }, Description = "Test for a single nonexistent artifact in breadcrumb, a>NE>a")]
        [TestCase(3, new[] { 2 }, Description = "Test for a single nonexistent artifact in breadcrumb, a>a>NE>a")]
        [TestCase(5, new[] { 3 }, Description = "Test for a single nonexistent artifact in breadcrumb, a>a>a>NE>a>a")]
        [TestCase(4, new[] { 1, 2 }, Description = "Test for sequential nonexistent artifacts in breadcrumb, a>NE>NE>a>a>a")]
        [TestCase(6, new[] { 1, 3, 6 }, Description = "Test for non-sequential nonexistent artifacts in breadcrumb, a>NE>a>NE>a>a>NE>a>a")]
        [Description("Get navigation with a single or multiple non-existent artifacts in the url path. " +
             "Verify that the non-existent artifacts are marked as <Inaccessible> in the returned " +
             "artifact reference lists.")]
        public void GetNavigationWithNonexistentArtifacts_VerifyReturnedNavigation(
            int numberOfArtifacts,
            int[] nonExistentArtifactIndexes)
        {
            ThrowIf.ArgumentNull(nonExistentArtifactIndexes, nameof(nonExistentArtifactIndexes));

            //Create artifact(s) with process artifact type and add to artifact list for navigation call
            for (int i = 0; i < numberOfArtifacts; i++)
            {
                var artifact = ArtifactFactory.CreateArtifact(project: _project, user: _primaryUser,
                artifactType: BaseArtifactType.Process);
                Artifact.SaveArtifact(artifactToSave: artifact, user: _primaryUser);
                _artifacts.Add(artifact);
            }

            //Inject nonexistent artifact(s) into artifact list used for navigation
            foreach (var nonExistentArtifactIndex in nonExistentArtifactIndexes)
            {
                var nonExistingArtifact = ArtifactFactory.CreateArtifact(_project, _primaryUser, BaseArtifactType.Actor);
                nonExistingArtifact.Id = NONEXISTENT_ARTIFACT_ID;
                _artifacts.Insert(nonExistentArtifactIndex, nonExistingArtifact);
            }

            //Get Navigation
            var resultArtifactReferenceList = _artifacts.First().GetNavigation(_primaryUser, _artifacts);

            //Navigation Assertions
            CommonServiceHelper.VerifyNavigation(_project, _primaryUser, resultArtifactReferenceList, _artifacts);
        }

        [TestRail(107172)]
        [TestCase(2, new int[] { 1 }, Description = "Test for a single inaccessible artifact in breadcrumb, a>IA>a")]
        [TestCase(3, new int[] { 2 }, Description = "Test for a single inaccessible artifact in breadcrumb, a>a>IA>a")]
        [TestCase(5, new int[] { 3 }, Description = "Test for a single inaccessible artifact in breadcrumb, a>a>a>IA>a>a")]
        [TestCase(4, new int[] { 1, 2 }, Description = "Test for sequential inaccessible artifacts in breadcrumb, a>IA>IA>a>a>a")]
        [TestCase(6, new int[] { 1, 3, 6 }, Description = "Test for non-sequential inaccessible artifacts in breadcrumb, a>IA>a>IA>a>a>IA>a>")]
        [Description("Get navigation with a single or multiple inaccessible artifacts in the url path. " +
                     "Verify that the inaccessible artifacts are marked as <Inaccessible> in the returned " +
                     "artifact reference lists.")]
        public void GetNavigationWithInaccessibleArtifacts_VerifyReturnedNavigation(
            int numberOfArtifacts,
            int[] inaccessibleArtifactIndexes)
        {
            ThrowIf.ArgumentNull(inaccessibleArtifactIndexes, nameof(inaccessibleArtifactIndexes));

            //Create artifact(s) with process artifact type and add to artifact list for navigation call
            for (int i = 0; i < numberOfArtifacts; i++)
            {
                var artifact = ArtifactFactory.CreateArtifact(project: _project, user: _primaryUser,
                artifactType: BaseArtifactType.Process);
                Artifact.SaveArtifact(artifactToSave: artifact, user: _primaryUser);
                _artifacts.Add(artifact);
            }

            //Create and inject artifacts created by another user, which are inaccessible by the main user
            foreach (var inaccessibleArtifactIndex in inaccessibleArtifactIndexes)
            {
                var inaccessbileArtifact = ArtifactFactory.CreateArtifact(_project, _secondaryUser, BaseArtifactType.Actor);
                Artifact.SaveArtifact(artifactToSave: inaccessbileArtifact, user: _secondaryUser);
                _artifacts.Insert(inaccessibleArtifactIndex, inaccessbileArtifact);
            }

            //Get Navigation
            var resultArtifactReferenceList = _artifacts.First().GetNavigation(_primaryUser, _artifacts);

            //Navigation Assertions
            CommonServiceHelper.VerifyNavigation(_project, _primaryUser, resultArtifactReferenceList, _artifacts);
        }

        #endregion Tests

    }
}
