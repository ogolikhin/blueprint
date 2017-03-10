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
        private IUser _user = null;
        private IProject _project = null;
        private IProject _projectCustomData = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_adminUser);
            _project = ProjectFactory.GetProject(_adminUser);
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _adminUser);
            _user = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #region Positive Tests

        #region Add Artifact to Baseline

        [TestCase(TestArtifactState.Published)]
        [TestCase(TestArtifactState.PublishedWithDraft)]
        [TestRail(266914)]
        [Description("Add published or published with draft Artifact to Baseline, check that Baseline has expected values.")]
        public void AddArtifactToBaseline_ArtifactAddToBaseline_ValidateReturnedBaseline(TestArtifactState artifactState)
        {
            // Setup:
            var artifactToAdd = CreateArtifactInSpecificState(Helper, _user, _project, artifactState, ItemTypePredefined.Actor,
                _project.Id);

            var baseline = Helper.CreateBaseline(_user, _project);

            int numberOfAddedArtifacts = 0;

            // Execute:
            Assert.DoesNotThrow(() => {
                numberOfAddedArtifacts = Helper.ArtifactStore.AddArtifactToBaseline(_user, artifactToAdd.Id, baseline.Id);
            }, "Adding artifact to Baseline shouldn't throw an error.");

            // Verify:
            Assert.AreEqual(1, numberOfAddedArtifacts, "AddArtifactToBaseline should return excpected number of added artifacts.");
            ValidateBaseline(_user, baseline.Id, new List<int> { artifactToAdd.Id });
        }

        [TestCase(true)]
        [TestCase(false)]
        [TestRail(266953)]
        [Description("Add published Artifact with children Artifacts to Baseline, check that Baseline has expected values.")]
        public void AddArtifactToBaseline_ArtifactWithDescendantsAddToBaseline_ValidateReturnedBaseline(bool includeDescendants)
        {
            // Setup:
            var artifactToAdd = CreateArtifactInSpecificState(Helper, _user, _project, TestArtifactState.Published,
                ItemTypePredefined.Actor, _project.Id);
            var childArtifact = CreateArtifactInSpecificState(Helper, _user, _project, TestArtifactState.Published,
                ItemTypePredefined.Document, artifactToAdd.Id);
            var childArtifact1 = CreateArtifactInSpecificState(Helper, _user, _project, TestArtifactState.Published, ItemTypePredefined.TextualRequirement,
                childArtifact.Id);
            var baseline = Helper.CreateBaseline(_user, _project);

            int numberOfAddedArtifacts = 0;
            int expectedArtifactsNumber = includeDescendants ? 3 : 1; // adding with Descendants should add 3 artifacts, adding without Descendants should add 1 artifact

            // Execute:
            Assert.DoesNotThrow(() => {
                numberOfAddedArtifacts = Helper.ArtifactStore.AddArtifactToBaseline(_user, artifactToAdd.Id,
                    baseline.Id, includeDescendants);
            }, "Adding artifact to Baseline shouldn't throw an error.");

            // Verify:
            Assert.AreEqual(expectedArtifactsNumber, numberOfAddedArtifacts, "AddArtifactToBaseline should return excpected number of added artifacts.");

            if (includeDescendants)
            {
                ValidateBaseline(_user, baseline.Id, new List<int> { artifactToAdd.Id, childArtifact.Id, childArtifact1.Id });
            }
            else
            {
                ValidateBaseline(_user, baseline.Id, new List<int> { artifactToAdd.Id });
            }
            
        }

        [TestCase]
        [TestRail(266596)]
        [Description("Add published Artifact to Baseline, check that Baseline has expected values.")]
        public void AddArtifactToBaseline_PublishedArtifact_ValidateReturnedBaseline()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.Actor);
            Helper.AssignProjectRolePermissionsToUser(_user, RolePermissions.Read, _project, artifact);

            var baseline = Helper.CreateBaseline(_user, _project);

            int numberOfAddedArtifacts = 0;
            int expectedArtifactsNumber = 1;

            // Execute:
            Assert.DoesNotThrow(() => {
                numberOfAddedArtifacts = Helper.ArtifactStore.AddArtifactToBaseline(_user, artifact.Id, baseline.Id);
            }, "Adding artifact to Baseline shouldn't throw an error.");

            // Verify:
            Assert.AreEqual(expectedArtifactsNumber, numberOfAddedArtifacts, "After update baseline should have expected number of artifacts.");
            ValidateBaseline(_user, baseline.Id, new List<int> { artifact.Id });
        }

        [TestCase]
        [TestRail(266957)]
        [Description("Add published Artifact with children Artifacts to Baseline, user don't have access to children, check that Baseline has expected values.")]
        public void AddArtifactToBaseline_ArtifactWithDescendants_UserHasAccessToParentArtifactOnly_AddToBaseline_ValidateReturnedBaseline()
        {
            // Setup:
            var artifactToAdd = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            var childArtifact1 = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UseCase, artifactToAdd);
            var childArtifact2 = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UseCase, artifactToAdd);
            Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.TextualRequirement, childArtifact2);

            Helper.AssignProjectRolePermissionsToUser(_user, RolePermissions.None, _project, childArtifact2);

            var baseline = Helper.CreateBaseline(_user, _project);

            int numberOfAddedArtifacts = 0;
            int expectedArtifactsNumber = 2; // user has no access to childArtifact2 and its descendants

            // Execute:
            Assert.DoesNotThrow(() => {
                numberOfAddedArtifacts = Helper.ArtifactStore.AddArtifactToBaseline(_user, artifactToAdd.Id,
                    baseline.Id, includeDescendants: true);
            }, "Adding artifact to Baseline shouldn't throw an error.");

            // Verify:
            Assert.AreEqual(expectedArtifactsNumber, numberOfAddedArtifacts, "AddArtifactToBaseline should return excpected number of added artifacts.");

            ValidateBaseline(_user, baseline.Id, new List<int> { artifactToAdd.Id, childArtifact1.Id });
        }

        #endregion Add Artifact to Baseline

        #region Add Collection to Baseline

        [TestCase(TestArtifactState.Published)]
        [TestCase(TestArtifactState.PublishedWithDraft)]
        [TestRail(266913)]
        [Description("Add one published or published with draft Artifact to Collection, add Collection to Baseline, check that Baseline has expected values.")]
        public void AddArtifactToBaseline_CollectionAddToBaseline_ValidateReturnedBaseline(TestArtifactState artifactState)
        {
            // Setup:
            var artifactToAdd = CreateArtifactInSpecificState(Helper, _user, _project, artifactState, ItemTypePredefined.Actor,
                _project.Id);
            var collection = CreateCollectionWithArtifactsInSpecificState(Helper, _user, _project, TestArtifactState.Created,
                new List<int> { artifactToAdd.Id });

            var baseline = Helper.CreateBaseline(_user, _project);

            int numberOfAddedArtifacts = 0;

            // Execute:
            Assert.DoesNotThrow(() => {
                numberOfAddedArtifacts = Helper.ArtifactStore.AddArtifactToBaseline(_user, collection.Id, baseline.Id);
            }, "Adding artifact to Baseline shouldn't throw an error.");

            // Verify:
            Assert.AreEqual(1, numberOfAddedArtifacts, "AddArtifactToBaseline should return excpected number of added artifacts.");

            ValidateBaseline(_user, baseline.Id, new List<int> { artifactToAdd.Id });
        }

        [TestCase]
        [TestRail(266960)]
        [Description("Add published Artifact to Baseline, check that Baseline has expected values.")]
        public void AddArtifactToBaseline_CollectionWithTwoArtifacts_UserHasAccessToOneArtifact_AddToBaseline_ValidateReturnedBaseline()
        {
            // Setup:
            var artifactToAdd = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            var artifactWithNoAccess = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UseCase);
            
            Helper.AssignProjectRolePermissionsToUser(_user, RolePermissions.None, _project, artifactWithNoAccess);

            var collection = CreateCollectionWithArtifactsInSpecificState(Helper, _adminUser, _project, TestArtifactState.Published,
                new List<int> { artifactToAdd.Id, artifactWithNoAccess.Id });


            var baseline = Helper.CreateBaseline(_user, _project);

            int numberOfAddedArtifacts = 0;
            int expectedArtifactsNumber = 1; // user has no access to artifactWithNoAccess from collection

            // Execute:
            Assert.DoesNotThrow(() => {
                numberOfAddedArtifacts = Helper.ArtifactStore.AddArtifactToBaseline(_user, collection.Id,
                    baseline.Id, includeDescendants: true);}, "Adding artifact to Baseline shouldn't throw an error.");
            
            // Verify:
            Assert.AreEqual(expectedArtifactsNumber, numberOfAddedArtifacts, "AddArtifactToBaseline should return excpected number of added artifacts.");

            ValidateBaseline(_user, baseline.Id, new List<int> { artifactToAdd.Id });
        }

        [TestCase]
        [TestRail(266978)]
        [Description("Add published Artifact to Baseline, check that Baseline has expected values.")]
        public void AddArtifactToBaseline_EmptyCollectionAddToBaseline_ValidateReturnedBaseline()
        {
            // Setup:
            var collectionArtifact = Helper.CreateAndSaveCollection(_project, _user);
            var collection = Helper.ArtifactStore.GetCollection(_user, collectionArtifact.Id);

            var baseline = Helper.CreateBaseline(_user, _project);

            int numberOfAddedArtifacts = 0;

            // Execute:
            Assert.DoesNotThrow(() => {
                numberOfAddedArtifacts = Helper.ArtifactStore.AddArtifactToBaseline(_user, collection.Id, baseline.Id);
            }, "Adding artifact to Baseline shouldn't throw an error.");

            // Verify:
            Assert.AreEqual(0, numberOfAddedArtifacts, "AddArtifactToBaseline should return excpected number of added artifacts.");

            ValidateBaseline(_user, baseline.Id, new List<int>());
        }

        [TestCase]
        [TestRail(266971)]
        [Description("Add published Artifact to Collection, user has no access to Artifact, add Collection to Baseline, check that Baseline has expected values.")]
        public void AddArtifactToBaseline_CollectionWithArtifactToWhichUserHasNoAccess_AddToBaseline_BaselineShouldBeEmpty()
        {
            // Setup:
            var artifactWithNoAccess = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UseCase);
            Helper.AssignProjectRolePermissionsToUser(_user, RolePermissions.None, _project, artifactWithNoAccess);

            var collection = CreateCollectionWithArtifactsInSpecificState(Helper, _adminUser, _project, TestArtifactState.Published,
                new List<int> { artifactWithNoAccess.Id });


            var baseline = Helper.CreateBaseline(_user, _project);

            int numberOfAddedArtifacts = 0;

            // Execute:
            Assert.DoesNotThrow(() => {
                numberOfAddedArtifacts = Helper.ArtifactStore.AddArtifactToBaseline(_user, collection.Id, baseline.Id);
            }, "Adding artifact to Baseline shouldn't throw an error.");

            // Verify:
            Assert.AreEqual(0, numberOfAddedArtifacts, "AddArtifactToBaseline should return excpected number of added artifacts.");
        }

        #endregion Add Collection to Baseline

        #region Edit Baseline Content

        [Explicit(IgnoreReasons.UnderDevelopmentDev)]
        [TestCase]
        [TestRail(266912)]
        [Description("Add published Artifact to Baseline, check that Baseline has expected values.")]
        public void AddArtifactToBaseline_PublishedArtifactToBaseline_ValidateReturnedBaseline()
        {
            // Setup:
            var artifactToAdd = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);

            var baselineArtifact = Helper.CreateBaseline(_user, _project);
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

        #endregion Edit Baseline Content

        #endregion Positive Tests

        #region Negative Tests
        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)]
        [TestCase]
        [TestRail(266968)]
        [Description("Add published Artifact to Baseline, check that Baseline has expected values.")]
        public void AddArtifactToBaseline_EmptyBaselineAddToBaseline_Validate404()
        {
            // Setup:
            var baseline1 = Helper.CreateBaseline(_user, _project);
            var baseline2 = Helper.CreateBaseline(_user, _project);

            int numberOfAddedArtifacts = 0;

            // Execute:
            Assert.DoesNotThrow(() => {
                numberOfAddedArtifacts = Helper.ArtifactStore.AddArtifactToBaseline(_user, baseline1.Id, baseline2.Id);
            }, "Adding artifact to Baseline shouldn't throw an error.");

            // Verify:
            Assert.AreEqual(0, numberOfAddedArtifacts, "AddArtifactToBaseline should return excpected number of added artifacts.");

            var updatedBaseline = Helper.ArtifactStore.GetBaseline(_user, baseline2.Id);
            Assert.IsNotNull(updatedBaseline.Artifacts, "List of artifacts shouldn't be empty");
            Assert.AreEqual(0, updatedBaseline.Artifacts.Count, "After update baseline should have expected number of artifacts.");
        }

        #endregion Negative Tests

        #region Custom Data Tests

        [Category(Categories.CustomData)]
        [TestCase]
        [TestRail(246581)]
        [Description("Get baseline by id from Custom Data project, check that Baseline has expected values.")]
        public void GetBaseline_ExistingBaseline_ValidateReturnedBaseline()
        {
            // Setup:
            var _viewerUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _projectCustomData);
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

        #region Private Functions

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
            NovaArtifactDetails artifactDetails = null;

            switch (state)
            {
                case TestArtifactState.Created:
                    return artifact;
                case TestArtifactState.Published:
                    artifactDetails = helper.ArtifactStore.GetArtifactDetails(user, artifact.Id);
                    CSharpUtilities.SetProperty("Description", "published state description", artifactDetails);
                    ArtifactStore.UpdateArtifact(helper.ArtifactStore.Address, user, artifactDetails);
                    ArtifactStore.PublishArtifacts(helper.ArtifactStore.Address, new List<int> { artifact.Id }, user);
                    return artifact;
                case TestArtifactState.PublishedWithDraft:
                    ArtifactStore.PublishArtifacts(helper.ArtifactStore.Address, new List<int> { artifact.Id }, user);
                    artifactDetails = helper.ArtifactStore.GetArtifactDetails(user, artifact.Id);
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

        /// <summary>
        /// Creates collection in the specified state with artifacts
        /// </summary>
        /// <param name="helper">TestHelper</param>
        /// <param name="user">User to perform operation</param>
        /// <param name="project">Project in which artifact will be created</param>
        /// <param name="collectionState">State of the collection(Created, Published)</param>
        /// <param name="artifactsIdsToAdd">List of artifact's id to be added</param>
        /// <returns>Collection in the required state</returns>
        private static Collection CreateCollectionWithArtifactsInSpecificState(TestHelper helper, IUser user, IProject project,
            TestArtifactState collectionState, List<int> artifactsIdsToAdd)
        {
            var collectionArtifact = helper.CreateAndSaveCollection(project, user);
            var collection = helper.ArtifactStore.GetCollection(user, collectionArtifact.Id);

            collection.UpdateArtifacts(artifactsIdsToAdd: artifactsIdsToAdd);
            collectionArtifact.Lock(user);
            Artifact.UpdateArtifact(collectionArtifact, user, collection);
            collection = helper.ArtifactStore.GetCollection(user, collectionArtifact.Id);

            switch (collectionState)
            {
                case TestArtifactState.Created:
                    return collection;
                case TestArtifactState.Published:
                    ArtifactStore.PublishArtifacts(helper.ArtifactStore.Address, new List<int> { collection.Id }, user);
                    return helper.ArtifactStore.GetCollection(user, collectionArtifact.Id);
                default:
                    Assert.Fail("Unexpected value of Collection state");
                    return collection;
            }
        }

        /// <summary>
        /// Checks that Baseline contains only artifacts with expected Ids
        /// </summary>
        /// <param name="user">User to get Baseline</param>
        /// <param name="baselineId">Id of Baseline to validate</param>
        /// <param name="expectedArtifactIds">List of excpected Artifact's Id</param>
        void ValidateBaseline(IUser user, int baselineId, List<int> expectedArtifactIds)
        {
            int excpectedArtifactsNumber = expectedArtifactIds.Count;
            var baseline = Helper.ArtifactStore.GetBaseline(user, baselineId);

            Assert.IsNotNull(baseline.Artifacts, "List of artifacts in Baseline shouldn't be empty");
            Assert.AreEqual(excpectedArtifactsNumber, baseline.Artifacts.Count,
                "After update baseline should have expected number of artifacts.");
            foreach (int id in expectedArtifactIds)
            {
                Assert.IsTrue(baseline.Artifacts.Exists(artifact => artifact.Id == id),
                    "List of artifacts in Baseline should have expected values.");
            }
        }

        #endregion Private Functions
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
