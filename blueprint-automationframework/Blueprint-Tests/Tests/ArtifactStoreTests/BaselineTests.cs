using System;
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

namespace ArtifactStoreTests
{
    public class BaselineTests : TestBase
    {
        private IUser _adminUser = null;
        private IUser _user = null;
        private IProject _project = null;
        private IProject _projectCustomData = null;

        private const string draftDescription = "description of item in the Draft state";
        private const string publishedDescription = "description of item in the Published state";

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

        [TestCase]
        [TestRail(267070)]
        [Description("Add published Artifact with children Artifacts to Baseline, user don't have access to children, check that only artifacts accessible to user were added to the Baseline.")]
        public void GetBaseline_ArtifactWithDescendants_UserHasAccessToParentArtifactOnly_OnlyAccessibleArtifactsVisibleInBaseline()
        {
            // Setup:
            var artifactToAdd = Helper.CreateNovaArtifactInSpecificState(_user, _project, TestHelper.TestArtifactState.Published,
                ItemTypePredefined.Actor, _project.Id);
            var childArtifact1 = Helper.CreateNovaArtifactInSpecificState(_user, _project, TestHelper.TestArtifactState.Published,
                ItemTypePredefined.Document, artifactToAdd.Id);
            var childArtifact2 = Helper.CreateNovaArtifactInSpecificState(_adminUser, _project,
                TestHelper.TestArtifactState.Published, ItemTypePredefined.TextualRequirement, artifactToAdd.Id);
            
            Helper.AssignNovaProjectRolePermissionsToUser(_user, RolePermissions.None, _project, childArtifact2);

            var baseline = Helper.CreateBaseline(_adminUser, _project);

            int numberOfAddedArtifacts = 0;
            int expectedArtifactsNumber = 3;

            numberOfAddedArtifacts = Helper.ArtifactStore.AddArtifactToBaseline(_adminUser, artifactToAdd.Id,
                    baseline.Id, includeDescendants: true);
            ArtifactStore.PublishArtifacts(Helper.ArtifactStore.Address, new List<int> { baseline.Id }, _adminUser);

            Baseline updatedBaseline = null;

            // Execute:
            Assert.DoesNotThrow(() => {
                updatedBaseline = Helper.ArtifactStore.GetBaseline(_user, baseline.Id);
            }, "Getting Baseline shouldn't throw an error.");

            // Verify:
            Assert.AreEqual(expectedArtifactsNumber, numberOfAddedArtifacts, "AddArtifactToBaseline should return expected number of added artifacts.");

            Assert.IsTrue(updatedBaseline.NotAllArtifactsAreShown, "Should be true.");
            Assert.IsTrue(updatedBaseline.Artifacts.Exists(item => item.Id == artifactToAdd.Id), "Artifact should be visible in Baseline, _user has access to the artifact.");
            Assert.IsTrue(updatedBaseline.Artifacts.Exists(item => item.Id == childArtifact1.Id), "Artifact should be visible in Baseline, _user has access to the artifact.");
            Assert.IsFalse(updatedBaseline.Artifacts.Exists(item => item.Id == childArtifact2.Id), "Artifact shouldn't be visible in Baseline, _user has no access to the artifact.");
        }

        #region Add Artifact to Baseline

        [TestCase(TestHelper.TestArtifactState.Published)]
        [TestCase(TestHelper.TestArtifactState.PublishedWithDraft)]
        [TestRail(266914)]
        [Description("Add published or published with draft Artifact to Baseline, check that Baseline has expected values.")]
        public void AddArtifactToBaseline_PublishedOrDraftArtifact_ValidateReturnedBaseline(TestHelper.TestArtifactState artifactState)
        {
            // Setup:
            var artifactToAdd = Helper.CreateNovaArtifactInSpecificState(_user, _project, artifactState, ItemTypePredefined.Actor,
                _project.Id);

            var baseline = Helper.CreateBaseline(_user, _project);

            int numberOfAddedArtifacts = 0;

            // Execute:
            Assert.DoesNotThrow(() => {
                numberOfAddedArtifacts = Helper.ArtifactStore.AddArtifactToBaseline(_user, artifactToAdd.Id, baseline.Id);
            }, "Adding artifact to Baseline shouldn't throw an error.");

            // Verify:
            Assert.AreEqual(1, numberOfAddedArtifacts, "AddArtifactToBaseline should return expected number of added artifacts.");
            var updatedBaseline = GetAndValidateBaseline(_user, baseline.Id, new List<int> { artifactToAdd.Id });
            switch (artifactState)
            {
                case TestHelper.TestArtifactState.Published:
                    Assert.AreEqual(publishedDescription, updatedBaseline.Artifacts[0].Description, "Artifact in Baseline should have expected description.");
                    break;
                case TestHelper.TestArtifactState.PublishedWithDraft:
                    Assert.AreEqual(draftDescription, updatedBaseline.Artifacts[0].Description, "Artifact in Baseline should have expected description.");
                    break;
                default:
                    break;
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        [TestRail(266953)]
        [Description("Add published Artifact with children Artifacts to Baseline, check that Baseline has expected values.")]
        public void AddArtifactToBaseline_ArtifactWithDescendantsAddToBaseline_ValidateReturnedBaseline(bool includeDescendants)
        {
            // Setup:
            var artifactToAdd = Helper.CreateNovaArtifactInSpecificState(_user, _project, TestHelper.TestArtifactState.Published,
                ItemTypePredefined.Actor, _project.Id);
            var childArtifact = Helper.CreateNovaArtifactInSpecificState(_user, _project, TestHelper.TestArtifactState.Published,
                ItemTypePredefined.Document, artifactToAdd.Id);
            var childArtifact1 = Helper.CreateNovaArtifactInSpecificState(_user, _project, TestHelper.TestArtifactState.Published, ItemTypePredefined.TextualRequirement,
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
            Assert.AreEqual(expectedArtifactsNumber, numberOfAddedArtifacts, "AddArtifactToBaseline should return expected number of added artifacts.");

            if (includeDescendants)
            {
                GetAndValidateBaseline(_user, baseline.Id, new List<int> { artifactToAdd.Id, childArtifact.Id, childArtifact1.Id });
            }
            else
            {
                GetAndValidateBaseline(_user, baseline.Id, new List<int> { artifactToAdd.Id });
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
            GetAndValidateBaseline(_user, baseline.Id, new List<int> { artifact.Id });
        }

        [TestCase]
        [TestRail(266957)]
        [Description("Add published Artifact with children Artifacts to Baseline, user don't have access to children, check that only artifacts accessible to user were added to the Baseline.")]
        public void AddArtifactToBaseline_ArtifactWithDescendants_UserHasAccessToParentArtifactOnly_OnlyAccessibleArtifactsAddedToBaseline()
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
            Assert.AreEqual(expectedArtifactsNumber, numberOfAddedArtifacts, "AddArtifactToBaseline should return expected number of added artifacts.");

            GetAndValidateBaseline(_user, baseline.Id, new List<int> { artifactToAdd.Id, childArtifact1.Id });
            ArtifactStore.PublishArtifacts(Helper.ArtifactStore.Address, new List<int> { baseline.Id }, _user);
            GetAndValidateBaseline(_adminUser, baseline.Id, new List<int> { artifactToAdd.Id, childArtifact1.Id }); // after Publish using Instance Admin that artifact wasn't added to the Baseline
        }

        #endregion Add Artifact to Baseline

        #region Add Collection to Baseline

        [TestCase(TestHelper.TestArtifactState.Published)]
        [TestCase(TestHelper.TestArtifactState.PublishedWithDraft)]
        [TestRail(266913)]
        [Description("Add one published or published with draft Artifact to Collection, add Collection to Baseline, check that Baseline has expected values.")]
        public void AddArtifactToBaseline_CollectionAddToBaseline_ValidateReturnedBaseline(TestHelper.TestArtifactState artifactState)
        {
            // Setup:
            var artifactToAdd = Helper.CreateNovaArtifactInSpecificState(_user, _project, artifactState, ItemTypePredefined.Actor,
                _project.Id);
            var collection = CreateCollectionWithArtifactsInSpecificState(Helper, _user, _project, TestHelper.TestArtifactState.Created,
                new List<int> { artifactToAdd.Id });

            var baseline = Helper.CreateBaseline(_user, _project);

            int numberOfAddedArtifacts = 0;

            // Execute:
            Assert.DoesNotThrow(() => {
                numberOfAddedArtifacts = Helper.ArtifactStore.AddArtifactToBaseline(_user, collection.Id, baseline.Id);
            }, "Adding artifact to Baseline shouldn't throw an error.");

            // Verify:
            Assert.AreEqual(1, numberOfAddedArtifacts, "AddArtifactToBaseline should return expected number of added artifacts.");

            GetAndValidateBaseline(_user, baseline.Id, new List<int> { artifactToAdd.Id });
        }

        [TestCase]
        [TestRail(266960)]
        [Description("Add published Collection with 2 artifacts (user has access to one artifact only) to Baseline, check that Baseline contains expected artifact only.")]
        public void AddArtifactToBaseline_CollectionWithTwoArtifacts_UserHasAccessToOneArtifact_AddToBaseline_ValidateReturnedBaseline()
        {
            // Setup:
            var artifactToAdd = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            var artifactWithNoAccess = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UseCase);
            
            Helper.AssignProjectRolePermissionsToUser(_user, RolePermissions.None, _project, artifactWithNoAccess);

            var collection = CreateCollectionWithArtifactsInSpecificState(Helper, _adminUser, _project, TestHelper.TestArtifactState.Published,
                new List<int> { artifactToAdd.Id, artifactWithNoAccess.Id });


            var baseline = Helper.CreateBaseline(_user, _project);

            int numberOfAddedArtifacts = 0;
            int expectedArtifactsNumber = 1; // user has no access to artifactWithNoAccess from collection

            // Execute:
            Assert.DoesNotThrow(() => {
                numberOfAddedArtifacts = Helper.ArtifactStore.AddArtifactToBaseline(_user, collection.Id,
                    baseline.Id, includeDescendants: true);}, "Adding Collection to Baseline shouldn't throw an error.");
            
            // Verify:
            Assert.AreEqual(expectedArtifactsNumber, numberOfAddedArtifacts, "AddArtifactToBaseline should return expected number of added artifacts.");

            GetAndValidateBaseline(_user, baseline.Id, new List<int> { artifactToAdd.Id });
            ArtifactStore.PublishArtifacts(Helper.ArtifactStore.Address, new List<int> { baseline.Id }, _user);
            GetAndValidateBaseline(_adminUser, baseline.Id, new List<int> { artifactToAdd.Id }); // after Publish using Instance Admin that artifact wasn't added to the Baseline
        }

        [TestCase]
        [TestRail(266978)]
        [Description("Add empty Collection to Baseline shouldn't throw an error, check that Baseline has no items.")]
        public void AddArtifactToBaseline_EmptyCollection_ValidateReturnedBaseline()
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
            Assert.AreEqual(0, numberOfAddedArtifacts, "AddArtifactToBaseline should return expected number of added artifacts.");

            GetAndValidateBaseline(_user, baseline.Id, new List<int>());
        }

        [TestCase]
        [TestRail(266971)]
        [Description("Add published Artifact to Collection, user has no access to Artifact, add Collection to Baseline, check that Baseline is empty.")]
        public void AddArtifactToBaseline_CollectionWhereUserHasNoAccessToArtifact_BaselineShouldBeEmpty()
        {
            // Setup:
            var artifactWithNoAccess = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UseCase);
            Helper.AssignProjectRolePermissionsToUser(_user, RolePermissions.None, _project, artifactWithNoAccess);

            var collection = CreateCollectionWithArtifactsInSpecificState(Helper, _adminUser, _project, TestHelper.TestArtifactState.Published,
                new List<int> { artifactWithNoAccess.Id });


            var baseline = Helper.CreateBaseline(_user, _project);

            int numberOfAddedArtifacts = 0;

            // Execute:
            Assert.DoesNotThrow(() => {
                numberOfAddedArtifacts = Helper.ArtifactStore.AddArtifactToBaseline(_user, collection.Id, baseline.Id);
            }, "Adding artifact to Baseline shouldn't throw an error.");

            // Verify:
            Assert.AreEqual(0, numberOfAddedArtifacts, "AddArtifactToBaseline should return expected number of added artifacts.");
        }

        #endregion Add Collection to Baseline

        #region Edit Baseline Content

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
            Assert.AreEqual(1, baseline.Artifacts.Count, "AddArtifactToBaseline should return expected number of added artifacts.");
        }

        #endregion Edit Baseline Content

        #region Edit Baseline Properties

        [TestCase]
        [TestRail(267068)]
        [Description(".")]
        public void EditBaseline_SealBaseline_CheckBaselineIsSealed()
        {
            // Setup:
            var baselineArtifact = Helper.CreateBaseline(_adminUser, _project);
            var baseline = Helper.ArtifactStore.GetBaseline(_adminUser, baselineArtifact.Id);

            ArtifactStore.PublishArtifacts(Helper.ArtifactStore.Address, new List<int> { baseline.Id }, _adminUser);
            SvcShared.LockArtifacts(Helper.ArtifactStore.Address, _adminUser, new List<int> { baseline.Id });

            baseline.SetUtcTimestamp(DateTime.Now);
            baseline.SetIsSealed(true);

            // Execute:
            Assert.DoesNotThrow(() => {
                ArtifactStore.UpdateArtifact(Helper.ArtifactStore.Address, _adminUser, baseline);
            }, "." +
            ".");
            baseline = Helper.ArtifactStore.GetBaseline(_adminUser, baselineArtifact.Id);

            // Verify:
            Assert.IsTrue(baseline.IsSealed, "Should be sealed.");
        }

        #endregion Edit Baseline Properties

        #endregion Positive Tests

        #region Negative Tests

        [TestCase]
        [TestRail(266968)]
        [Description("Add Baseline to another Baseline, check that it returns 404 with the expected error message.")]
        public void AddArtifactToBaseline_EmptyBaselineAddToBaseline_Validate404()
        {
            // Setup:
            var baseline1 = Helper.CreateBaseline(_user, _project);
            var baseline2 = Helper.CreateBaseline(_user, _project);

            int numberOfAddedArtifacts = 0;

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => {
                numberOfAddedArtifacts = Helper.ArtifactStore.AddArtifactToBaseline(_user, baseline1.Id, baseline2.Id);
            }, "Adding Baseline to another Baseline should throw 404 error.");

            // Verify:
            string expectedErrorMessage = "You have attempted to access an artifact that does not exist or has been deleted.";
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound, expectedErrorMessage);
        }

        [TestCase]
        [TestRail(267021)]
        [Description("Try to set IsAvailableInAnalytics for unsealed Baseline, check 409 and error message.")]
        public void EditBaseline_SetAvailableForAnalytics_ValidateReturned409()
        {
            // Setup:
            var baselineArtifact = Helper.CreateBaseline(_adminUser, _project);
            var baseline = Helper.ArtifactStore.GetBaseline(_adminUser, baselineArtifact.Id);

            ArtifactStore.PublishArtifacts(Helper.ArtifactStore.Address, new List<int> { baseline.Id }, _adminUser);
            SvcShared.LockArtifacts(Helper.ArtifactStore.Address, _adminUser, new List<int> { baseline.Id });

            baseline.SetIsAvailableInAnalytics(true);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => {
                ArtifactStore.UpdateArtifact(Helper.ArtifactStore.Address, _adminUser, baseline);
            }, "Attempt to set IsAvailableInAnalytics for unsealed Baseline should return 409 error." +
            "This option should be available for sealed Baselines only.");

            // Verify:
            // see TFS 5107
            string expectedErrorMessage = "Exception of type 'BluePrintSys.RC.Business.Internal.Models.InternalApiBusinessException' was thrown.";
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotSaveOverDependencies, expectedErrorMessage);
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
            var viewerUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _projectCustomData);
            const int baselineId = 83;
            const int expectedArtifactsNumber = 2;
            var expectedBaseline = new Baseline(isAvailableInAnalytics: false, notAllArtifactsAreShown: false,
                isSealed: false);

            // Execute: 
            Baseline baseline = null;
            Assert.DoesNotThrow(() => baseline = Helper.ArtifactStore.GetBaseline(viewerUser, baselineId),
                "Get Baseline shouldn't return an error.");

            // Verify:
            Baseline.AssertBaselinesAreEqual(expectedBaseline, baseline);
            Assert.AreEqual(expectedArtifactsNumber, baseline.Artifacts.Count, "Baseline should have expected number of Artifacts.");
        }

        #endregion

        #region Private Functions

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
            TestHelper.TestArtifactState collectionState, List<int> artifactsIdsToAdd)
        {
            var collectionArtifact = helper.CreateAndSaveCollection(project, user);
            var collection = helper.ArtifactStore.GetCollection(user, collectionArtifact.Id);

            collection.UpdateArtifacts(artifactsIdsToAdd: artifactsIdsToAdd);
            collectionArtifact.Lock(user);
            Artifact.UpdateArtifact(collectionArtifact, user, collection);
            collection = helper.ArtifactStore.GetCollection(user, collectionArtifact.Id);

            switch (collectionState)
            {
                case TestHelper.TestArtifactState.Created:
                    return collection;
                case TestHelper.TestArtifactState.Published:
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
        /// <param name="expectedArtifactIds">List of expected Artifact's Id</param>
        Baseline GetAndValidateBaseline(IUser user, int baselineId, List<int> expectedArtifactIds)
        {
            int expectedArtifactsNumber = expectedArtifactIds.Count;
            var baseline = Helper.ArtifactStore.GetBaseline(user, baselineId);

            Assert.IsNotNull(baseline?.Artifacts, "List of artifacts in Baseline shouldn't be empty");
            Assert.AreEqual(expectedArtifactsNumber, baseline.Artifacts.Count,
                "After update baseline should have expected number of artifacts.");
            foreach (int id in expectedArtifactIds)
            {
                Assert.IsTrue(baseline.Artifacts.Exists(artifact => artifact.Id == id),
                    "List of artifacts in Baseline should have expected values.");
            }

            return baseline;
        }

        #endregion Private Functions
    }
}
