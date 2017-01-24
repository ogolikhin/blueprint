using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.Factories;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TestCommon;
using Utilities;

namespace CommonServiceTests
{
    [TestFixture]
    [Category(Categories.Navigation)]
    public class NavigationTests : TestBase
    {
        private IUser _primaryUser;
        private IUser _secondaryUser;
        private IProject _project;
        private List<IArtifact> _artifacts;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            Helper = new TestHelper();
            _primaryUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _secondaryUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_primaryUser);
        }

        [SetUp]
        public void SetUp()
        {
            _artifacts = new List<IArtifact>();
        }

        [TearDown]
        public void TearDown()
        {
            _artifacts.Clear();
            _artifacts = null;
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region Tests

        [TestRail(107167)]
        [TestCase]
        [Description("Get the navigation with all available artifact types for the project in the URL path which are accessible by the user. " +
            "Verify the returned artifact reference list contains the expected values.")]
        public void GetNavigation_AllAccessibleArtifactTypes_ReturnsCorrectNavigationReferenceList()
        {
            // Setup:
            //Create artifacts with distinct available artifactTypes 
            var baseArtifactTypes = _project.ArtifactTypes.ConvertAll(o => o.BaseArtifactType);

            // Because of HTTP path size limits, we need to make sure we don't create more artifacts that can fit in the path.
            int artifactCounter = 0;
            const int MAX_ARTIFACTS_IN_PATH = 27;

            foreach (var baseArtifactType in baseArtifactTypes)
            {
                var artifact = Helper.CreateArtifact(_project, _primaryUser, baseArtifactType);
                artifact.Save();

                //Add an artifact to artifact list for navigation call
                _artifacts.Add(artifact);

                if (++artifactCounter >= MAX_ARTIFACTS_IN_PATH)
                {
                    break;
                }
            }

            // Execute:
            //Get Navigation
            var resultArtifactReferenceList = _artifacts.First().GetNavigation(_primaryUser, _artifacts);

            // Verify:
            //Navigation Assertions
            CommonServiceHelper.VerifyNavigation(_project, _primaryUser, resultArtifactReferenceList, _artifacts);
        }

        [TestRail(107168)]
        [Explicit(IgnoreReasons.ProductBug)]  // https://trello.com/c/v8zXJTty  Passing more than 24 artifact ID's in GET navigation URL gives an error.
        [TestCase]
        [Description("Get the navigation with maxium allowable number of artifacts in the URL path. " +
            "Verify the returned artifact reference list contains the expected values.")]
        public void GetNavigation_MaximumNumberOfArtifacts_ReturnsCorrectNavigationReferenceList()
        {
            // Setup:
            //Create an artifact with process artifact type
            var artifact = Helper.CreateArtifact(_project, _primaryUser, BaseArtifactType.Process);
            artifact.Save();

            const int MAXIUM_ALLOWABLE_NAVIGATION = 23;     // TODO: Development needs to define a limit for us...  This is just what currently works because of IIS URL size limit.

            //Add the same artifact repeatedly in the artifact list to create a navigation list which exceeds the maximum
            //allowable number of artifacts
            for (int i = 0; i < MAXIUM_ALLOWABLE_NAVIGATION; i++)
            {
                var nonExistingArtifact = CreateNonExistentArtifact();
                _artifacts.Add(nonExistingArtifact);
            }

            //Add the artifact at the end of artifact list for navigation call
            _artifacts.Add(artifact);

            // Execute:
            //Get Navigation
            var resultArtifactReferenceList = artifact.GetNavigation(_primaryUser, _artifacts);

            // Verify:
            //Navigation Assertions
            CommonServiceHelper.VerifyNavigation(_project, _primaryUser, resultArtifactReferenceList, _artifacts);
        }

        [TestRail(107169)]
        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(int.MaxValue)]
        [Description("Get the navigation with invalid artifact ID data in the URL path. " +
            "Verify the Not Found exception.")]
        public void GetNavigation_InvalidArtifactId_404NotFound(int artifactId)
        {
            // Setup:
            //Create invalid artifact
            var invalidArtifact = CreateInvalidArtifact(artifactId);

            //Add the artifact to artifact list for navigation call
            _artifacts.Add(invalidArtifact);

            // Execute & Verify:
            //Get Navigation and check the Not Found exception
            Assert.Throws<Http404NotFoundException>(() =>
            {
                invalidArtifact.GetNavigation(_primaryUser, _artifacts);
            }, "The GET {0} endpoint should return 404 NotFound when we pass an invalid artifact ID in the URL!",
                RestPaths.Svc.Shared.NAVIGATION_ids_);
        }

        [TestRail(107170)]
        [TestCase(1)]
        [TestCase(3)]
        [Description("Get the navigation with process artifact(s) in the URL path are accessible by the user.  " +
             "Verify the returned artifact reference lists.")]
        public void GetNavigation_AccessibleProcessArtifacts_ReturnsCorrectNavigationReferenceList(int numberOfArtifacts)
        {
            // Setup:
            //Create artifact(s) with process artifact type and add to artifact list for navigation call
            for (int i = 0; i < numberOfArtifacts; i++)
            {
                var artifact = Helper.CreateArtifact(_project, _primaryUser, BaseArtifactType.Process);
                artifact.Save();
                _artifacts.Add(artifact);
            }

            // Execute:
            //Get Navigation
            var resultArtifactReferenceList = _artifacts.First().GetNavigation(_primaryUser, _artifacts);

            // Verify:
            //Navigation Assertions
            CommonServiceHelper.VerifyNavigation(_project, _primaryUser, resultArtifactReferenceList, _artifacts);
        }

        [TestRail(107171)]
        [TestCase(2, new[] { 1 }, Description = "Test for a single nonexistent artifact in breadcrumb, a>NE>a")]
        [TestCase(3, new[] { 2 }, Description = "Test for a single nonexistent artifact in breadcrumb, a>a>NE>a")]
        [TestCase(5, new[] { 3 }, Description = "Test for a single nonexistent artifact in breadcrumb, a>a>a>NE>a>a")]
        [TestCase(4, new[] { 1, 2 }, Description = "Test for sequential nonexistent artifacts in breadcrumb, a>NE>NE>a>a>a")]
        [TestCase(6, new[] { 1, 3, 6 }, Description = "Test for non-sequential nonexistent artifacts in breadcrumb, a>NE>a>NE>a>a>NE>a>a")]
        [Description("Get navigation with a single or multiple non-existent artifacts in the URL path. " +
             "Verify that the non-existent artifacts are marked as <Inaccessible> in the returned artifact reference lists.")]
        public void GetNavigation_NonExistentArtifacts_ReturnsCorrectNavigationReferenceList(
            int numberOfArtifacts,
            int[] nonExistentArtifactIndexes)
        {
            // Setup:
            ThrowIf.ArgumentNull(nonExistentArtifactIndexes, nameof(nonExistentArtifactIndexes));

            //Create artifact(s) with process artifact type and add to artifact list for navigation call
            for (int i = 0; i < numberOfArtifacts; i++)
            {
                var artifact = Helper.CreateArtifact(_project, _primaryUser, BaseArtifactType.Process);
                artifact.Save();
                _artifacts.Add(artifact);
            }

            //Inject nonexistent artifact(s) into artifact list used for navigation
            foreach (var nonExistentArtifactIndex in nonExistentArtifactIndexes)
            {
                var nonExistingArtifact = CreateNonExistentArtifact();
                _artifacts.Insert(nonExistentArtifactIndex, nonExistingArtifact);
            }

            // Execute:
            //Get Navigation
            var resultArtifactReferenceList = _artifacts.First().GetNavigation(_primaryUser, _artifacts);

            // Verify:
            //Navigation Assertions
            CommonServiceHelper.VerifyNavigation(_project, _primaryUser, resultArtifactReferenceList, _artifacts);
        }

        [TestRail(107172)]
        [TestCase(2, new int[] { 1 }, Description = "Test for a single inaccessible artifact in breadcrumb, a>IA>a")]
        [TestCase(3, new int[] { 2 }, Description = "Test for a single inaccessible artifact in breadcrumb, a>a>IA>a")]
        [TestCase(5, new int[] { 3 }, Description = "Test for a single inaccessible artifact in breadcrumb, a>a>a>IA>a>a")]
        [TestCase(4, new int[] { 1, 2 }, Description = "Test for sequential inaccessible artifacts in breadcrumb, a>IA>IA>a>a>a")]
        [TestCase(6, new int[] { 1, 3, 6 }, Description = "Test for non-sequential inaccessible artifacts in breadcrumb, a>IA>a>IA>a>a>IA>a>")]
        [Description("Get navigation with a single or multiple inaccessible artifacts in the URL path. " +
                     "Verify that the inaccessible artifacts are marked as <Inaccessible> in the returned " +
                     "artifact reference lists.")]
        public void GetNavigation_InaccessibleArtifacts_ReturnsCorrectNavigationReferenceList(
            int numberOfArtifacts,
            int[] inaccessibleArtifactIndexes)
        {
            // Setup:
            ThrowIf.ArgumentNull(inaccessibleArtifactIndexes, nameof(inaccessibleArtifactIndexes));

            //Create artifact(s) with process artifact type and add to artifact list for navigation call
            for (int i = 0; i < numberOfArtifacts; i++)
            {
                var artifact = Helper.CreateArtifact(_project, _primaryUser, BaseArtifactType.Process);
                artifact.Save();
                _artifacts.Add(artifact);
            }

            //Create and inject artifacts created by another user, which are inaccessible by the main user
            foreach (var inaccessibleArtifactIndex in inaccessibleArtifactIndexes)
            {
                var inaccessbileArtifact = Helper.CreateArtifact(_project, _secondaryUser, BaseArtifactType.Actor);
                inaccessbileArtifact.Save();
                _artifacts.Insert(inaccessibleArtifactIndex, inaccessbileArtifact);
            }

            // Execute:
            //Get Navigation
            var resultArtifactReferenceList = _artifacts.First().GetNavigation(_primaryUser, _artifacts);

            // Verify:
            //Navigation Assertions
            CommonServiceHelper.VerifyNavigation(_project, _primaryUser, resultArtifactReferenceList, _artifacts);
        }

        #endregion Tests

        /// <summary>
        /// Creates an artifact with an ID that doesn't exist.
        /// </summary>
        /// <param name="baseArtifactType">(optional) The artifact type.</param>
        /// <returns>The non-existent artifact.</returns>
        private IArtifact CreateNonExistentArtifact(BaseArtifactType baseArtifactType = BaseArtifactType.Actor)
        {
            var nonExistingArtifact = ArtifactFactory.CreateArtifact(_project, _primaryUser, baseArtifactType);
            nonExistingArtifact.Id = CommonServiceHelper.NONEXISTENT_ARTIFACT_ID;
            return nonExistingArtifact;
        }

        /// <summary>
        /// Creates an artifact with an invalid ID.
        /// </summary>
        /// <param name="artifactId">The invalid artifact ID to give the artifact.</param>
        /// <param name="baseArtifactType">(optional) The artifact type.</param>
        /// <returns>The invalid artifact.</returns>
        private IArtifact CreateInvalidArtifact(int artifactId, BaseArtifactType baseArtifactType = BaseArtifactType.Actor)
        {
            var invalidArtifact = ArtifactFactory.CreateArtifact(_project, _primaryUser, baseArtifactType);
            invalidArtifact.Id = artifactId;
            return invalidArtifact;
        }
    }
}
