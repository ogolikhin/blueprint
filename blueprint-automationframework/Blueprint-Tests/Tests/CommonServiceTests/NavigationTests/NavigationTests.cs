using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.NavigationModel;
using Model.NavigationModel.Impl;
using Model.OpenApiModel;
using Model.OpenApiModel.Impl;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace CommonServiceTests.NavigationTests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
    [TestFixture]
    [Category(Categories.Navigation)]
    public class NavigationTests
    {
        private const int NONEXISTENT_ARTIFACT_ID = 99999999;
        private const string INVALID_TOKEN = "Invalid_Token_value";

        private IAdminStore _adminStore;
        private IBlueprintServer _blueprintServer;
        private IUser _primaryUser;
        private IUser _secondaryUser;
        private IProject _project;
        private INavigation _navigation;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            _adminStore = AdminStoreFactory.GetAdminStoreFromTestConfig();
            _blueprintServer = BlueprintServerFactory.GetBlueprintServerFromTestConfig();
            _primaryUser = UserFactory.CreateUserAndAddToDatabase();
            _secondaryUser = UserFactory.CreateUserAndAddToDatabase();
            _project = ProjectFactory.GetProject(_primaryUser, shouldRetrievePropertyTypes: true);

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

        [SetUp]
        public void SetUp()
        {
            _navigation = new Navigation(_blueprintServer.Address);
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
            if (_navigation.Artifacts != null)
            {
                // Delete or Discard all the artifacts that were added.
                var savedArtifactsListPrimaryUser = new List<IOpenApiArtifact>();
                var savedArtifactsListSecondaryUser = new List<IOpenApiArtifact>();
                foreach (var artifact in _navigation.Artifacts.ToArray())
                {
                    if (!artifact.Id.Equals(NONEXISTENT_ARTIFACT_ID) && artifact.IsPublished)
                    {
                        _navigation.DeleteNavigationArtifact(artifact, deleteChildren: true);
                    }

                    if (!artifact.Id.Equals(NONEXISTENT_ARTIFACT_ID) && !artifact.IsPublished && artifact.CreatedBy.Equals(_primaryUser))
                    {
                        savedArtifactsListPrimaryUser.Add(artifact);
                    }

                    if (!artifact.Id.Equals(NONEXISTENT_ARTIFACT_ID) && !artifact.IsPublished && artifact.CreatedBy.Equals(_secondaryUser))
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
                _navigation.Artifacts.Clear();
            }
        }

        #endregion Setup and Cleanup

        #region Tests

        [TestCase(1)]
        [TestCase(3)]
        [Description("Get the navigation where all process artifacts in the GET url path are accessible by the user." +
             "Verify that the returned artifact reference list contains all process artifacts' Ids and url path links.")]
        public void GetNavigationWithAccessibleProcessArtifacts_VerifyReturnedNavigation(int numberOfArtifacts)
        {
            //Create an artifact with ArtifactType and populate all required values
            var artifacts = ArtifactFactory.CreateAndSaveOpenApiArtifacts(project: _project, user: _primaryUser,
                artifactType: BaseArtifactType.Process, numberOfArtifacts: numberOfArtifacts);

            //Updating the artifacts of _navigation for tearndown purpose
            artifacts.ForEach(artifact => _navigation.Artifacts.Add(artifact));

            //Get breadcrumb information
            var resultArtifactReferenceList = _navigation.GetNavigation(_primaryUser, artifacts);

            //Navigation Assertions
            CommonServiceHelper.VerifyNavigation(_project, _primaryUser, resultArtifactReferenceList, _navigation);
        }

        [TestCase]
        [Description("Get the navigation where all non-process artifacts in the GET url path are accessible by the user." +
            "Verify that the returned artifact reference list contains all process artifacts' Ids and url path links.")]
        public void GetNavigationWithAccessibleNonProcessArtifacts_VerifyReturnedNavigation()
        {
            var availableArtifactTypes = _project.ArtifactTypes.ConvertAll(o => o.BaseArtifactType);
            //Create an artifact with ArtifactType and populate all required values

            //IOpenApiArtifact artifact = new OpenApiArtifact();
            foreach (var availableArtifactType in availableArtifactTypes)
            {
                var artifact = ArtifactFactory.CreateAndSaveOpenApiArtifacts(project: _project, user: _primaryUser,
                artifactType: availableArtifactType, numberOfArtifacts: 1).First();
                //Updating the artifacts of _navigation for tearndown purpose
                _navigation.Artifacts.Add(artifact);
            }

            //Get breadcrumb information
            var resultArtifactReferenceList = _navigation.GetNavigation(_primaryUser, _navigation.Artifacts);

            //Navigation Assertions
            CommonServiceHelper.VerifyNavigation(_project, _primaryUser, resultArtifactReferenceList, _navigation);
        }

        [TestCase(2, new[] { 1 }, Description = "Test for a single nonexistent artifact in breadcrumb, a>NE>a")]
        [TestCase(3, new[] { 2 }, Description = "Test for a single nonexistent artifact in breadcrumb, a>a>NE>a")]
        [TestCase(5, new[] { 3 }, Description = "Test for a single nonexistent artifact in breadcrumb, a>a>a>NE>a>a")]
        [TestCase(4, new[] { 1, 2 }, Description = "Test for sequential nonexistent artifacts in breadcrumb, a>NE>NE>a>a>a")]
        [TestCase(6, new[] { 1, 3, 6 }, Description = "Test for non-sequential nonexistent artifacts in breadcrumb, a>NE>a>NE>a>a>NE>a>a")]
        [Description("Get navigation with a single or multiple non-existent artifacts in the GET url path. " +
             "Verify that the non-existent artifacts are marked as <Inaccessible> in the returned " +
             "artifact reference lists. ")]
        public void GetNavigationWithNonexistentArtifactsInPath_VerifyReturnedNavigation(
            int numberOfArtifacts,
            int[] nonExistentArtifactIndexes)
        {
            ThrowIf.ArgumentNull(nonExistentArtifactIndexes, nameof(nonExistentArtifactIndexes));

            //Create an artifact with ArtifactType and populate all required values
            var artifacts = ArtifactFactory.CreateAndSaveOpenApiArtifacts(project: _project, user: _primaryUser,
                artifactType: BaseArtifactType.Process, numberOfArtifacts: numberOfArtifacts);

            //Inject nonexistent artifact into artifact list used for breadcrumb
            foreach (var nonExistentArtifactIndex in nonExistentArtifactIndexes)
            {
                var nonExistingArtifact = ArtifactFactory.CreateOpenApiArtifact(_project, _primaryUser, BaseArtifactType.Actor);
                nonExistingArtifact.Id = NONEXISTENT_ARTIFACT_ID;
                
                artifacts.Insert(nonExistentArtifactIndex, nonExistingArtifact);
            }

            //Updating the artifacts of _navigation for tearndown purpose
            artifacts.ForEach(artifact => _navigation.Artifacts.Add(artifact));

            //Get breadcrumb information
            var resultArtifactReferenceList = _navigation.GetNavigation(_primaryUser, artifacts);

            //Navigation Assertions
            CommonServiceHelper.VerifyNavigation(_project, _primaryUser, resultArtifactReferenceList, _navigation);
        }

        [TestCase(2, new int[] { 1 }, Description = "Test for a single inaccessible artifact in breadcrumb, a>IA>a")]
        [TestCase(3, new int[] { 2 }, Description = "Test for a single inaccessible artifact in breadcrumb, a>a>IA>a")]
        [TestCase(5, new int[] { 3 }, Description = "Test for a single inaccessible artifact in breadcrumb, a>a>a>IA>a>a")]
        [TestCase(4, new int[] { 1, 2 }, Description = "Test for sequential inaccessible artifacts in breadcrumb, a>IA>IA>a>a>a")]
        [TestCase(6, new int[] { 1, 3, 6 }, Description = "Test for nonsequential inaccessible artifacts in breadcrumb, a>IA>a>IA>a>a>IA>a>")]
        [Description("Get navigation with a single or multiple inaccessible artifacts in the GET url path. " +
                     "Verify that the inaccessible artifacts are marked as <Inaccessible> in the returned " +
                     "artifact reference lists. ")]
        public void GetNavigationWithInaccessibleArtifactsInPath_VerifyReturnedNavigation(
            int numberOfArtifacts,
            int[] inaccessibleArtifactIndexes)
        {
            ThrowIf.ArgumentNull(inaccessibleArtifactIndexes, nameof(inaccessibleArtifactIndexes));

            //Create an artifact with ArtifactType and populate all required values
            var artifacts = ArtifactFactory.CreateAndSaveOpenApiArtifacts(project: _project, user: _primaryUser,
                artifactType: BaseArtifactType.Process, numberOfArtifacts: numberOfArtifacts);

            // create and inject artifact ids created by another user
            foreach (var inaccessibleArtifactIndex in inaccessibleArtifactIndexes)
            {
                var inaccessbileArtifact = ArtifactFactory.CreateAndSaveOpenApiArtifacts(_project, _secondaryUser, BaseArtifactType.Actor, 1).First();

                artifacts.Insert(inaccessibleArtifactIndex, inaccessbileArtifact);
            }

            //Updating the artifacts of _navigation for tearndown purpose
            artifacts.ForEach(artifact => _navigation.Artifacts.Add(artifact));

            //Get breadcrumb information
            var resultArtifactReferenceList = _navigation.GetNavigation(_primaryUser, artifacts);

            //Navigation Assertions
            CommonServiceHelper.VerifyNavigation(_project, _primaryUser, resultArtifactReferenceList, _navigation);
        }

        #endregion Tests

    }
}
