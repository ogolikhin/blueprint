using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.Impl;
using NUnit.Framework;
using System.Collections.Generic;
using TestCommon;
using Utilities;
using Utilities.Factories;

namespace ArtifactStoreTests
{
    public class BaselineTests : TestBase
    {
        private IUser _adminUser = null;
        private IUser _viewerUser = null;
        private IUser _user = null;
        private IProject _project = null;
        private IProject _projectCustomData = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_adminUser);
            _viewerUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _projectCustomData);
            _project = ProjectFactory.GetProject(_adminUser);
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _adminUser);
            _user = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #region Add artifact to Baseline tests
        [TestCase()]
        [TestRail(266596)]
        [Description("Add published Artifact to Baseline, check that Baseline has expected values.")]
        public void AddArtifactToBaseline_PublishedArtifact_ValidateReturnedBaseline()
        {
            // Setup:
            var defaultBaselineFolder = _project.GetDefaultBaselineFolder(Helper.ArtifactStore.Address, _user);
            string baselineName = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);
            var baseline = ArtifactStore.CreateArtifact(Helper.ArtifactStore.Address, _user, ItemTypePredefined.ArtifactBaseline,
                baselineName, _project, defaultBaselineFolder.Id);

            var artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);

            int numberOfAddedArtifacts = 0;

            // Execute:
            Assert.DoesNotThrow(() => {
                numberOfAddedArtifacts = Helper.ArtifactStore.AddArtifactToBaseline(_user, artifact.Id, baseline.Id);
            }, "Adding artifact to Baseline shouldn't throw an error.");

            // Verify:
            var updatedBaseline = Helper.ArtifactStore.GetBaseline(_user, baseline.Id);
            Assert.AreEqual(1, updatedBaseline.Artifacts.Count, "AddArtifactToBaseline should return excpected number of added artifacts.");
            Assert.AreEqual(1, numberOfAddedArtifacts, "After update baseline should have expected number of artifacts.");
        }

        [TestCase(TestArtifactState.Published)]
        [TestCase(TestArtifactState.PublishedWithDraft)]
        [TestRail(266914)]
        [Description("Add published Artifact to Baseline, check that Baseline has expected values.")]
        public void AddArtifactToBaseline_ArtifactAddToBaseline_ValidateReturnedBaseline(TestArtifactState artifactState)
        {
            // Setup:
            var artifactToAdd = CreateArtifactInSpecificState(Helper, _user, _project, artifactState, ItemTypePredefined.Actor,
                _project.Id);

            var defaultBaselineFolder = _project.GetDefaultBaselineFolder(Helper.ArtifactStore.Address, _user);
            string baselineName = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);
            var baseline = ArtifactStore.CreateArtifact(Helper.ArtifactStore.Address, _user, ItemTypePredefined.ArtifactBaseline,
                baselineName, _project, defaultBaselineFolder.Id);

            int numberOfAddedArtifacts = 0;

            // Execute:
            Assert.DoesNotThrow(() => {
                numberOfAddedArtifacts = Helper.ArtifactStore.AddArtifactToBaseline(_user, artifactToAdd.Id, baseline.Id);
            }, "Adding artifact to Baseline shouldn't throw an error.");

            // Verify:
            Assert.AreEqual(1, numberOfAddedArtifacts, "AddArtifactToBaseline should return excpected number of added artifacts.");

            var updatedBaseline = Helper.ArtifactStore.GetBaseline(_user, baseline.Id);
            Assert.IsNotNull(updatedBaseline.Artifacts, "List of artifacts shouldn't be empty");
            Assert.AreEqual(1, updatedBaseline.Artifacts.Count, "After update baseline should have expected number of artifacts.");
        }

        [TestCase(TestArtifactState.Published)]
        [TestCase(TestArtifactState.PublishedWithDraft)]
        [TestRail(266913)]
        [Description("Add published Artifact to Baseline, check that Baseline has expected values.")]
        public void AddArtifactToBaseline_CollectionAddToBaseline_ValidateReturnedBaseline(TestArtifactState artifactState)
        {
            // Setup:
            var collectionArtifact = Helper.CreateAndSaveCollection(_project, _user);
            var artifactToAdd = CreateArtifactInSpecificState(Helper, _user, _project, artifactState, ItemTypePredefined.Actor,
                _project.Id);
            var collection = Helper.ArtifactStore.GetCollection(_user, collectionArtifact.Id);

            collection.UpdateArtifacts(artifactsIdsToAdd: new List<int> { artifactToAdd.Id });
            collectionArtifact.Lock(_user);
            Artifact.UpdateArtifact(collectionArtifact, _user, collection);
            //Helper.ArtifactStore.PublishArtifact(collectionArtifact, _user);
            collection = Helper.ArtifactStore.GetCollection(_user, collectionArtifact.Id);

            var defaultBaselineFolder = _project.GetDefaultBaselineFolder(Helper.ArtifactStore.Address, _user);
            string baselineName = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);
            var baseline = ArtifactStore.CreateArtifact(Helper.ArtifactStore.Address, _user, ItemTypePredefined.ArtifactBaseline,
                baselineName, _project, defaultBaselineFolder.Id);

            int numberOfAddedArtifacts = 0;

            // Execute:
            Assert.DoesNotThrow(() => {
                numberOfAddedArtifacts = Helper.ArtifactStore.AddArtifactToBaseline(_user, collection.Id, baseline.Id);
            }, "Adding artifact to Baseline shouldn't throw an error.");

            // Verify:
            Assert.AreEqual(1, numberOfAddedArtifacts, "AddArtifactToBaseline should return excpected number of added artifacts.");

            var updatedBaseline = Helper.ArtifactStore.GetBaseline(_user, baseline.Id);
            Assert.IsNotNull(updatedBaseline.Artifacts, "List of artifacts shouldn't be empty");
            Assert.AreEqual(1, updatedBaseline.Artifacts.Count, "After update baseline should have expected number of artifacts.");
        }

        [Explicit(IgnoreReasons.UnderDevelopmentDev)]
        [TestCase]
        [TestRail(266912)]
        [Description("Add published Artifact to Baseline, check that Baseline has expected values.")]
        public void AddArtifactToBaseline_PublishedArtifactToBaseline_ValidateReturnedBaseline()
        {
            // Setup:
            var artifactToAdd = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            
            var defaultBaselineFolder = _project.GetDefaultBaselineFolder(Helper.ArtifactStore.Address, _user);
            string baselineName = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);
            var baselineArtifact = ArtifactStore.CreateArtifact(Helper.ArtifactStore.Address, _user, ItemTypePredefined.ArtifactBaseline,
                baselineName, _project, defaultBaselineFolder.Id);
            var baseline = Helper.ArtifactStore.GetBaseline(_user, baselineArtifact.Id);
            baseline.UpdateArtifacts(new List<int> { artifactToAdd.Id });

            ArtifactStore.UpdateArtifact(Helper.ArtifactStore.Address, _user, baseline);

            // Execute:
            Assert.DoesNotThrow(() => {
                baseline = Helper.ArtifactStore.GetBaseline(_user, baseline.Id);
            }, "Adding artifact to Baseline shouldn't throw an error.");

            // Verify:
            Assert.AreEqual(1, baseline.Artifacts.Count, "AddArtifactToBaseline should return excpected number of added artifacts.");
        }

        #endregion Add artifact to Baseline tests

        #region Custom Data Tests
        [Category(Categories.CustomData)]
        [TestCase]
        [TestRail(246581)]
        [Description("Get baseline by id from Custom Data project, check that Baseline has expected values.")]
        public void GetBaseline_ExistingBaseline_ValidateReturnedBaseline()
        {
            // Setup:
            const int baselineId = 83;
            const int expectedArtifactsNumber = 2;
            var expectedBaseline = new Baseline(isAvailableInAnalytics: false, notAllArtifactsAreShown: false,
                isSealed: false);

            // Execute: 
            Baseline baseline = null;
            Assert.DoesNotThrow(() => baseline = Helper.ArtifactStore.GetBaseline(_viewerUser, baselineId),
                "Get Baseline shouldn't return an error.");

            // Verify:
            Baseline.AssertBaselinesAreEqual(expectedBaseline, baseline);
            Assert.AreEqual(expectedArtifactsNumber, baseline.Artifacts.Count, "Baseline should have expected number of Artifacts.");
        }
        #endregion

        #region private functions
        /// <summary>
        /// Creates artifact in the specified state
        /// </summary>
        /// <param name="helper">TestHelper</param>
        /// <param name="user">User to perform operation</param>
        /// <param name="project">Project in which artifact will be created</param>
        /// <param name="state">State of the artifact(Created, Published, PublishedWithDraft, ScheduledToDelete, Deleted)</param>
        /// <param name="itemType">itemType of artifact to be created</param>
        /// <param name="parentId">Parent Id of artifact to be created</param>
        /// <returns>Artifact in the required state</returns>
        private static INovaArtifactDetails CreateArtifactInSpecificState(TestHelper helper, IUser user, IProject project, TestArtifactState state,
            ItemTypePredefined itemType, int parentId)
        {
            string artifactName = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);
            var artifact = ArtifactStore.CreateArtifact(helper.ArtifactStore.Address, user, itemType, artifactName, project, parentId);

            switch (state)
            {
                case TestArtifactState.Created:
                    return artifact;
                case TestArtifactState.Published:
                    ArtifactStore.PublishArtifacts(helper.ArtifactStore.Address, new List<int> { artifact.Id }, user);
                    return artifact;
                case TestArtifactState.PublishedWithDraft:
                    ArtifactStore.PublishArtifacts(helper.ArtifactStore.Address, new List<int> { artifact.Id }, user);
                    var artifactDetails = helper.ArtifactStore.GetArtifactDetails(user, artifact.Id);
                    CSharpUtilities.SetProperty("Description", "draft state description", artifactDetails);
                    SvcShared.LockArtifacts(helper.ArtifactStore.Address, user, new List<int> { artifact.Id });
                    return ArtifactStore.UpdateArtifact(helper.ArtifactStore.Address, user, artifactDetails);
                case TestArtifactState.ScheduledToDelete:
                    ArtifactStore.PublishArtifacts(helper.ArtifactStore.Address, new List<int> { artifact.Id }, user);
                    ArtifactStore.DeleteArtifact(helper.ArtifactStore.Address, artifact.Id, user);
                    return artifact;
                case TestArtifactState.Deleted:
                    ArtifactStore.PublishArtifacts(helper.ArtifactStore.Address, new List<int> { artifact.Id }, user);
                    ArtifactStore.DeleteArtifact(helper.ArtifactStore.Address, artifact.Id, user);
                    ArtifactStore.PublishArtifacts(helper.ArtifactStore.Address, new List<int> { artifact.Id }, user);
                    return artifact;
                default:
                    Assert.Fail("Unexpected value of Artifact state");
                    return artifact;
            }
        }
        #endregion private functions
    }

    public enum TestArtifactState
    {
        Created = 0,
        Published = 1,
        PublishedWithDraft = 2,
        ScheduledToDelete = 3,
        Deleted = 4
    }
}
