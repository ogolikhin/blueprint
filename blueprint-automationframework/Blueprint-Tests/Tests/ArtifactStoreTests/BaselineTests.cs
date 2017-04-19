using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.Impl;
using Model.NovaModel.Impl;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using TestCommon;
using Utilities;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public class BaselineTests : TestBase
    {
        private IUser _adminUser = null;
        private IUser _user = null;
        private IProject _project = null;
        private IProject _projectCustomData = null;

        private readonly string draftDescription = TestHelper.DraftDescription;
        private readonly string publishedDescription = TestHelper.PublishedDescription;

        private const string expectedInternalExceptionMessage = "Exception of type 'BluePrintSys.RC.Business.Internal.Models.InternalApiBusinessException' was thrown.";

        private static Dictionary<int, string> ProjectExpectedCreationDateMap { get; } = new Dictionary<int, string>
        {
            { 1, "2016-09-20 17:04:14.787" },
            { 4, "2016-09-20 17:04:35.690" },
            { 212, "2016-10-28 19:37:50.410" },
            { 382, "2016-12-06 18:56:49.993" },
            { 385, "2016-12-06 21:33:18.493" },
            { 388, "2017-01-09 21:19:27.043" },
            { 392, "2017-01-23 15:34:42.550" }
        };
        
        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
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
        [Description("Add published Artifact with children Artifacts to Baseline, user don't have access to one of children" +
            "artifacts, check that only artifacts accessible to user were added to the Baseline.")]
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

            int expectedArtifactsNumber = 3;

            var addArtifactResult = Helper.ArtifactStore.AddArtifactToBaseline(_adminUser, artifactToAdd.Id,
                    baseline.Id, includeDescendants: true);

            Assert.AreEqual(expectedArtifactsNumber, addArtifactResult.ArtifactCount, "AddArtifactToBaseline should return expected number of added artifacts.");
            Helper.ArtifactStore.PublishArtifacts(new List<int> { baseline.Id }, _adminUser);

            Baseline updatedBaseline = null;

            // Execute:
            Assert.DoesNotThrow(() => {
                updatedBaseline = Helper.ArtifactStore.GetBaseline(_user, baseline.Id);
            }, "Getting Baseline shouldn't throw an error.");

            // Verify:

            Assert.IsTrue(updatedBaseline.NotAllArtifactsAreShown, "NotAllArtifactsAreShown should be true.");
            Assert.IsTrue(updatedBaseline.Artifacts.Exists(item => item.Id == artifactToAdd.Id), "Artifact should be visible in Baseline, _user has access to the artifact.");
            Assert.IsTrue(updatedBaseline.Artifacts.Exists(item => item.Id == childArtifact1.Id), "Artifact should be visible in Baseline, _user has access to the artifact.");
            Assert.IsFalse(updatedBaseline.Artifacts.Exists(item => item.Id == childArtifact2.Id), "Artifact shouldn't be visible in Baseline, _user has no access to the artifact.");
        }

        #region Add Artifact to Baseline

        [TestCase(TestHelper.TestArtifactState.Published)]
        [TestCase(TestHelper.TestArtifactState.PublishedWithDraft)]
        [TestRail(266914)]
        [Description("Add published or published with draft Artifact to Baseline, check that Baseline and its artifacts have " +
            "expected values.")]
        public void AddArtifactToBaseline_PublishedOrDraftArtifact_ValidateReturnedBaseline(TestHelper.TestArtifactState artifactState)
        {
            // Setup:
            var artifactToAdd = Helper.CreateNovaArtifactInSpecificState(_user, _project, artifactState, ItemTypePredefined.Actor,
                _project.Id);

            var baseline = Helper.CreateBaseline(_user, _project);

            int numberOfAddedArtifacts = 0;

            // Execute:
            Assert.DoesNotThrow(() => {
                var addArtifactResult = Helper.ArtifactStore.AddArtifactToBaseline(_user, artifactToAdd.Id, baseline.Id);
                numberOfAddedArtifacts = addArtifactResult.ArtifactCount;
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
                var addArtifactResult = Helper.ArtifactStore.AddArtifactToBaseline(_user, artifactToAdd.Id, baseline.Id,
                    includeDescendants);
                numberOfAddedArtifacts = addArtifactResult.ArtifactCount;
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
                var addArtifactResult = Helper.ArtifactStore.AddArtifactToBaseline(_user, artifact.Id, baseline.Id);
                numberOfAddedArtifacts = addArtifactResult.ArtifactCount;
            }, "Adding artifact to Baseline shouldn't throw an error.");

            // Verify:
            Assert.AreEqual(expectedArtifactsNumber, numberOfAddedArtifacts, "After update baseline should have expected number of artifacts.");
            GetAndValidateBaseline(_user, baseline.Id, new List<int> { artifact.Id });
        }

        [TestCase]
        [TestRail(266957)]
        [Description("Add published Artifact with children Artifacts to Baseline, user don't have access to children, " +
            "check that only artifacts accessible to user were added to the Baseline.")]
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
                var addArtifactResult = Helper.ArtifactStore.AddArtifactToBaseline(_user, artifactToAdd.Id, baseline.Id,
                    includeDescendants: true);
                numberOfAddedArtifacts = addArtifactResult.ArtifactCount;
            }, "Adding artifact to Baseline shouldn't throw an error.");

            // Verify:
            Assert.AreEqual(expectedArtifactsNumber, numberOfAddedArtifacts, "AddArtifactToBaseline should return expected number of added artifacts.");

            GetAndValidateBaseline(_user, baseline.Id, new List<int> { artifactToAdd.Id, childArtifact1.Id });
            Helper.ArtifactStore.PublishArtifacts(new List<int> { baseline.Id }, _user);
            GetAndValidateBaseline(_adminUser, baseline.Id, new List<int> { artifactToAdd.Id, childArtifact1.Id }); // after Publish using Instance Admin that artifact wasn't added to the Baseline
        }

        [TestCase(-5)]
        [TestRail(267117)]
        [Description("Add published Artifact to Baseline, Baseline has timestamp before or after artifact's CreatedOn date," +
            "check that artifact was not added and call returns 1 for Nonnexistent Artifacts, when  Baseline has timestamp before artifact's CreatedOn date.")]
        public void AddArtifactToBaseline_PublishedArtifact_BaselineWithTimeStampBeforeArtifactCreatedOn_CheckWhatWasAdded(int utcTimestampMinutesFromNow)
        {
            // Setup:
            var artifactToAdd = Helper.CreateNovaArtifactInSpecificState(_user, _project, TestHelper.TestArtifactState.Published,
                ItemTypePredefined.Actor, _project.Id);

            var baselineArtifact = Helper.CreateBaseline(_user, _project);
            var baseline = Helper.ArtifactStore.GetBaseline(_user, baselineArtifact.Id);

            var utcTimestamp = DateTime.UtcNow.AddMinutes(utcTimestampMinutesFromNow);

            baseline.UtcTimestamp = utcTimestamp;
            // front-end doesn't send unchanged properties, server-side doesn't process them correctly
            baseline.SpecificPropertyValues.Remove(baseline.SpecificPropertyValues.Find(property =>
            property.PropertyType == Model.Common.Enums.PropertyTypePredefined.BaselineIsDataAnalyticsAvailable));
            baseline.SpecificPropertyValues.Remove(baseline.SpecificPropertyValues.Find(property =>
            property.PropertyType == Model.Common.Enums.PropertyTypePredefined.BaselineIsSealed));
            ArtifactStore.UpdateArtifact(Helper.ArtifactStore.Address, _user, baseline);

            int numberOfAddedArtifacts = -1;
            int numberOfNonnexistentArtifacts = -1;

            AddToBaselineResult addArtifactResult = null;

            // Execute:
            Assert.DoesNotThrow(() => {
                addArtifactResult = Helper.ArtifactStore.AddArtifactToBaseline(_user, artifactToAdd.Id, baseline.Id);
            }, "Adding artifact to Baseline shouldn't throw an error.");

            // Verify:
            numberOfAddedArtifacts = addArtifactResult.ArtifactCount;
            numberOfNonnexistentArtifacts = addArtifactResult.NonExistentArtifactCount.Value;

            Assert.AreEqual(0, numberOfAddedArtifacts, "Nothing should be added to baseline, when its TimeStamp older than Artifact.");
            Assert.AreEqual(1, numberOfNonnexistentArtifacts, "AddArtifactToBaseline should return expected number of Nonnexistent Artifacts.");
        }

        [TestCase(TestHelper.TestArtifactState.Created)]
        [TestRail(267127)]
        [Description("Add created Artifact to Baseline - artifact shouldn't be added, call should return that one artifact was never published.")]
        public void AddArtifactToBaseline_CreatedArtifact_NoArtifactAdded_CheckReturn(TestHelper.TestArtifactState artifactState)
        {
            // Setup:
            var artifactToAdd = Helper.CreateNovaArtifactInSpecificState(_user, _project, artifactState, ItemTypePredefined.Actor,
                _project.Id);

            var baselineArtifact = Helper.CreateBaseline(_user, _project);

            int numberOfAddedArtifacts = -1;
            int numberOfUnpublishedArtifacts = -1;

            // Execute:
            Assert.DoesNotThrow(() => {
                var addArtifactResult = Helper.ArtifactStore.AddArtifactToBaseline(_user, artifactToAdd.Id, baselineArtifact.Id);
                numberOfAddedArtifacts = addArtifactResult.ArtifactCount;
                numberOfUnpublishedArtifacts = addArtifactResult.UnpublishedArtifactCount.Value;
            }, "Adding artifact to Baseline shouldn't throw an error.");

            // Verify:
            Assert.AreEqual(0, numberOfAddedArtifacts, "AddArtifactToBaseline should return expected number of added artifacts.");
            Assert.AreEqual(1, numberOfUnpublishedArtifacts);
            var baseline = Helper.ArtifactStore.GetBaseline(_user, baselineArtifact.Id);
            Assert.IsEmpty(baseline.Artifacts, "List of Basline artifacts should be empty");
            Assert.IsFalse(baseline.NotAllArtifactsAreShown, "NotAllArtifactsAreShown should be false.");
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
            var collection = Helper.CreateUnpublishedCollectionWithArtifactsInSpecificState(_user, _project, TestHelper.TestArtifactState.Created,
                new List<int> { artifactToAdd.Id });

            var baseline = Helper.CreateBaseline(_user, _project);

            int numberOfAddedArtifacts = 0;

            // Execute:
            Assert.DoesNotThrow(() => {
                var addArtifactResult = Helper.ArtifactStore.AddArtifactToBaseline(_user, collection.Id, baseline.Id);
                numberOfAddedArtifacts = addArtifactResult.ArtifactCount;
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

            var collection = Helper.CreateUnpublishedCollectionWithArtifactsInSpecificState(_adminUser, _project, TestHelper.TestArtifactState.Published,
                new List<int> { artifactToAdd.Id, artifactWithNoAccess.Id });


            var baseline = Helper.CreateBaseline(_user, _project);

            int numberOfAddedArtifacts = 0;
            int expectedArtifactsNumber = 1; // user has no access to artifactWithNoAccess from collection

            // Execute:
            Assert.DoesNotThrow(() => {
                var addArtifactResult = Helper.ArtifactStore.AddArtifactToBaseline(_user, collection.Id, baseline.Id,
                    includeDescendants: true);
                numberOfAddedArtifacts = addArtifactResult.ArtifactCount;
            }, "Adding Collection to Baseline shouldn't throw an error.");
            
            // Verify:
            Assert.AreEqual(expectedArtifactsNumber, numberOfAddedArtifacts, "AddArtifactToBaseline should return expected number of added artifacts.");

            GetAndValidateBaseline(_user, baseline.Id, new List<int> { artifactToAdd.Id });
            Helper.ArtifactStore.PublishArtifacts(new List<int> { baseline.Id }, _user);
            GetAndValidateBaseline(_adminUser, baseline.Id, new List<int> { artifactToAdd.Id }); // after Publish using Instance Admin that artifact wasn't added to the Baseline
        }

        [TestCase]
        [TestRail(266978)]
        [Description("Add empty Collection to Baseline shouldn't throw an error, check that Baseline has no items.")]
        public void AddArtifactToBaseline_EmptyCollection_ValidateReturnedBaseline()
        {
            // Setup:
            var collectionArtifact = Helper.CreateUnpublishedCollection(_project, _user);
            var collection = Helper.ArtifactStore.GetCollection(_user, collectionArtifact.Id);

            var baseline = Helper.CreateBaseline(_user, _project);

            int numberOfAddedArtifacts = 0;

            // Execute:
            Assert.DoesNotThrow(() => {
                var addArtifactResult = Helper.ArtifactStore.AddArtifactToBaseline(_user, collection.Id, baseline.Id);
                numberOfAddedArtifacts = addArtifactResult.ArtifactCount;
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

            var collection = Helper.CreateUnpublishedCollectionWithArtifactsInSpecificState(_adminUser, _project, TestHelper.TestArtifactState.Published,
                new List<int> { artifactWithNoAccess.Id });


            var baseline = Helper.CreateBaseline(_user, _project);

            int numberOfAddedArtifacts = 0;

            // Execute:
            Assert.DoesNotThrow(() => {
                var addArtifactResult = Helper.ArtifactStore.AddArtifactToBaseline(_user, collection.Id, baseline.Id);
                numberOfAddedArtifacts = addArtifactResult.ArtifactCount;
            }, "Adding artifact to Baseline shouldn't throw an error.");

            // Verify:
            Assert.AreEqual(0, numberOfAddedArtifacts, "AddArtifactToBaseline should return expected number of added artifacts.");
        }

        #endregion Add Collection to Baseline

        #region Edit Baseline Content

        [TestCase]
        [TestRail(266912)]
        [Description("Add published Artifact to Baseline, check that Baseline has expected values.")]
        public void EditBaselineArtifacts_AddPublishedArtifactToBaseline_ValidateReturnedBaseline()
        {
            // Setup:
            var artifactToAdd = Helper.CreateNovaArtifactInSpecificState(_user, _project, TestHelper.TestArtifactState.PublishedWithDraft, ItemTypePredefined.Actor,
                _project.Id);

            var baselineArtifact = Helper.CreateBaseline(_user, _project);
            var baseline = Helper.ArtifactStore.GetBaseline(_user, baselineArtifact.Id);
            baseline.UpdateArtifacts(new List<int> { artifactToAdd.Id });
            // front-end doesn't send unchanged properties, server-side doesn't process them correctly
            baseline.SpecificPropertyValues.Remove(baseline.SpecificPropertyValues.Find(property =>
            property.PropertyType == Model.Common.Enums.PropertyTypePredefined.BaselineIsSealed));
            baseline.SpecificPropertyValues.Remove(baseline.SpecificPropertyValues.Find(property =>
            property.PropertyType == Model.Common.Enums.PropertyTypePredefined.BaselineIsDataAnalyticsAvailable));

            // Execute:
            Assert.DoesNotThrow(() => {
                ArtifactStore.UpdateArtifact(Helper.ArtifactStore.Address, _user, baseline);
            }, "Adding artifact to Baseline shouldn't throw an error.");

            // Verify:
            GetAndValidateBaseline(_user, baselineArtifact.Id, new List<int> { artifactToAdd.Id });
        }

        [TestCase]
        [TestRail(267192)]
        [Description("Create and publish artifact with the child artifact, add two artifact to baseline and publish changes," + 
            "remove child artifact from baseline, check that baseline has expected artifact only.")]
        public void EditBaselineArtifacts_RemoveArtifactFromBaseline_CheckBaselineDoesNotHaveRemovedArtifact()
        {
            // Setup:
            var artifactToAdd = Helper.CreateNovaArtifactInSpecificState(_adminUser, _project, TestHelper.TestArtifactState.Published, ItemTypePredefined.Actor,
                _project.Id);
            var artifactToRemove = Helper.CreateNovaArtifactInSpecificState(_adminUser, _project, TestHelper.TestArtifactState.Published, ItemTypePredefined.TextualRequirement,
                artifactToAdd.Id);

            var baselineArtifact = Helper.CreateBaseline(_adminUser, _project);
            Helper.ArtifactStore.AddArtifactToBaseline(_adminUser, artifactToAdd.Id, baselineArtifact.Id, includeDescendants: true);
            Helper.ArtifactStore.PublishArtifacts(new List<int> { baselineArtifact.Id }, _adminUser);
            GetAndValidateBaseline(_adminUser, baselineArtifact.Id, new List<int> { artifactToAdd.Id, artifactToRemove.Id });

            var baseline = Helper.ArtifactStore.GetBaseline(_user, baselineArtifact.Id);
            baseline.UpdateArtifacts(artifactsIdsToRemove: new List<int> { artifactToRemove .Id });
            // front-end doesn't send unchanged properties, server-side doesn't process them correctly
            baseline.SpecificPropertyValues.Remove(baseline.SpecificPropertyValues.Find(property =>
            property.PropertyType == Model.Common.Enums.PropertyTypePredefined.BaselineIsDataAnalyticsAvailable));
            baseline.SpecificPropertyValues.Remove(baseline.SpecificPropertyValues.Find(property =>
            property.PropertyType == Model.Common.Enums.PropertyTypePredefined.BaselineIsSealed));
            Helper.SvcShared.LockArtifacts(_user, new List<int> { baseline.Id });
            
            // Execute:
            Assert.DoesNotThrow(() => {
                ArtifactStore.UpdateArtifact(Helper.ArtifactStore.Address, _user, baseline);
            }, "Removing artifact from Baseline shouldn't throw an error.");

            // Verify:
            GetAndValidateBaseline(_user, baselineArtifact.Id, new List<int> { artifactToAdd.Id });
        }

        [TestCase]
        [TestRail(267373)]
        [Description("Add never published Artifact to Baseline, check that artifact wasn't added.")]
        public void EditBaselineArtifacts_AddCreatedArtifactToBaseline_ValidateArtifactWasNotAdded()
        {
            // Setup:
            var artifactToAdd = Helper.CreateNovaArtifactInSpecificState(_user, _project, TestHelper.TestArtifactState.Created, ItemTypePredefined.Actor,
                _project.Id);

            var baselineArtifact = Helper.CreateBaseline(_user, _project);
            var baseline = Helper.ArtifactStore.GetBaseline(_user, baselineArtifact.Id);
            baseline.UpdateArtifacts(new List<int> { artifactToAdd.Id });
            // front-end doesn't send unchanged properties, server-side doesn't process them correctly
            baseline.SpecificPropertyValues.Remove(baseline.SpecificPropertyValues.Find(property =>
            property.PropertyType == Model.Common.Enums.PropertyTypePredefined.BaselineIsDataAnalyticsAvailable));
            baseline.SpecificPropertyValues.Remove(baseline.SpecificPropertyValues.Find(property =>
            property.PropertyType == Model.Common.Enums.PropertyTypePredefined.BaselineIsSealed));

            // Execute:
            Assert.DoesNotThrow(() => {
                ArtifactStore.UpdateArtifact(Helper.ArtifactStore.Address, _user, baseline);
            }, "Adding artifact to Baseline shouldn't throw an error.");

            // Verify:
            GetAndValidateBaseline(_user, baselineArtifact.Id, new List<int>());
        }

        #endregion Edit Baseline Content

        #region Edit Baseline Properties

        [Category(Categories.GoldenData)]
        [TestCase]
        [TestRail(290069)]
        [Description("Create Baseline, get Baseline and check that MinimalUtcTimestamp is equal to the CreatedDateTime value of the project it is based on.")]
        public void GetBaseline_CreateDefaultBaseline_ValidateMinimalUtcTimestamp()
        {
            // Setup:
            var baselineArtifact = Helper.CreateBaseline(_user, _project);

            // Execute:
            Baseline receivedBaseline = null;
            Assert.DoesNotThrow(() => {
                receivedBaseline = Helper.ArtifactStore.GetBaseline(_user, baselineArtifact.Id);
            }, "Getting Baseline shouldn't throw an error.");

            // Verify:
            ValidateMinimalUtctTimestamp(_project, receivedBaseline, ProjectExpectedCreationDateMap);
        }

        [TestCase(true)]
        [TestCase(false)]
        [TestRail(267068)]
        [Description("Update Baseline - set UtcTimestamp to DateTime.UtcNow and IsSealed to true - check that baseline is sealed.")]
        public void EditBaseline_SealBaselineSetAvailableForAnalytics_CheckBaseline(bool setAvailableForAnalytics)
        {
            // Setup:
            var baselineArtifact = Helper.CreateBaseline(_adminUser, _project);
            var baseline = Helper.ArtifactStore.GetBaseline(_adminUser, baselineArtifact.Id);

            Helper.ArtifactStore.PublishArtifacts(new List<int> { baseline.Id }, _adminUser);
            Helper.SvcShared.LockArtifacts(_adminUser, new List<int> { baseline.Id });

            var sealedDate = DateTime.UtcNow.AddMinutes(-1);
            baseline.UtcTimestamp = sealedDate;
            baseline.IsSealed = true;
            if (setAvailableForAnalytics)
            {
                baseline.IsAvailableInAnalytics = true;
            }

            // Execute:
            Assert.DoesNotThrow(() => {
                ArtifactStore.UpdateArtifact(Helper.ArtifactStore.Address, _adminUser, baseline);
            }, "Update Baseline shouldn't throw an error.");
            baseline = Helper.ArtifactStore.GetBaseline(_adminUser, baselineArtifact.Id);

            // Verify:
            GetAndValidateBaseline(_adminUser, baselineArtifact.Id, new List<int>(), isSealed: true, utcTimestamp: sealedDate,
                isAvailableInAnalytics: setAvailableForAnalytics);
        }

        [TestCase]
        [TestRail(267202)]
        [Description("Add published Artifact to Baseline, Baseline has timestamp before artifact CreatedOn date," +
            "check that artifact was not added and call returns correct number for 'Nonnexistent' Artifacts.")]
        public void EditBaseline_BaselineWithArtifact_SealBaselinetWithTimeStampBeforeArtifactCreatedOn_CheckBaselineIsEmpty()
        {
            // Setup:
            var artifactToAdd = Helper.CreateNovaArtifactInSpecificState(_user, _project, TestHelper.TestArtifactState.Published,
                ItemTypePredefined.Actor, _project.Id);

            var baselineArtifact = Helper.CreateBaseline(_user, _project, artifactToAddId: artifactToAdd.Id);
            var baseline = GetAndValidateBaseline(_user, baselineArtifact.Id, new List<int> { artifactToAdd.Id });

            var timestampDate = DateTime.UtcNow.AddMinutes(-3);
            baseline.UtcTimestamp = timestampDate;
            // front-end doesn't send unchanged properties, server-side doesn't process them correctly
            baseline.SpecificPropertyValues.Remove(baseline.SpecificPropertyValues.Find(property =>
            property.PropertyType == Model.Common.Enums.PropertyTypePredefined.BaselineIsDataAnalyticsAvailable));
            baseline.SpecificPropertyValues.Remove(baseline.SpecificPropertyValues.Find(property =>
            property.PropertyType == Model.Common.Enums.PropertyTypePredefined.BaselineIsSealed));

            // Execute:
            Assert.DoesNotThrow(() => {
                ArtifactStore.UpdateArtifact(Helper.ArtifactStore.Address, _user, baseline);
            }, "Adding artifact to Baseline shouldn't throw an error.");

            // Verify:
            GetAndValidateBaseline(_user, baselineArtifact.Id, new List<int>(), utcTimestamp: timestampDate);
        }

        #endregion Edit Baseline Properties

        #region BaselineInfo Tests

        [TestCase]
        [TestRail(288884)]
        [Description("Create Baseline, get BaselineInfo and check that response has expected values.")]
        public void GetBaselineInfo_ExistingLiveBaseline_ValidateResponse()
        {
            // Setup:
            var baselineArtifact = Helper.CreateBaseline(_adminUser, _project);
            var baseline = Helper.ArtifactStore.GetBaseline(_adminUser, baselineArtifact.Id);

            List<BaselineInfo> baselineInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() => {
                baselineInfoList = Helper.ArtifactStore.GetBaselineInfo(new List<int> { baselineArtifact.Id }, _adminUser);
            }, "Getting BaselineInfo shouldn't throw an error.");

            // Verify:
            Assert.AreEqual(1, baselineInfoList?.Count, "List of BaselineInfo should have one item.");
            baselineInfoList[0].AssertBaselineInfoCorrespondsToBaseline(baseline);
        }

        [TestCase]
        [TestRail(288888)]
        [Description("Create Baseline with timestamp, get BaselineInfo, check that BaselineInfo has expected values.")]
        public void GetBaselineInfo_TimestampedBaseline_ValidateResponse()
        {
            // Setup:
            var artifactToAdd = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Document);

            var baselineArtifact = Helper.CreateBaseline(_user, _project, artifactToAddId: artifactToAdd.Id);
            var baseline = GetAndValidateBaseline(_user, baselineArtifact.Id, new List<int> { artifactToAdd.Id });

            var timestampDate = DateTime.UtcNow.AddMinutes(-3);
            baseline.UtcTimestamp = timestampDate;
            // front-end doesn't send unchanged properties, server-side doesn't process them correctly
            baseline.SpecificPropertyValues.Remove(baseline.SpecificPropertyValues.Find(property =>
            property.PropertyType == Model.Common.Enums.PropertyTypePredefined.BaselineIsDataAnalyticsAvailable));
            baseline.SpecificPropertyValues.Remove(baseline.SpecificPropertyValues.Find(property =>
            property.PropertyType == Model.Common.Enums.PropertyTypePredefined.BaselineIsSealed));
            baselineArtifact.Update(_user, baseline);

            List<BaselineInfo> baselineInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() => {
                baselineInfoList = Helper.ArtifactStore.GetBaselineInfo(new List<int> { baselineArtifact.Id }, _user);
            }, "Getting BaselineInfo shouldn't throw an error.");

            // Verify:
            Assert.AreEqual(1, baselineInfoList?.Count, "List of BaselineInfo should have one item.");
            baseline = Helper.ArtifactStore.GetBaseline(_user, baselineArtifact.Id);
            baselineInfoList[0].AssertBaselineInfoCorrespondsToBaseline(baseline);
        }

        [TestCase]
        [TestRail(288901)]
        [Description("Create sealed Baseline with timestamp, get BaselineInfo, check that BaselineInfo has expected values.")]
        public void GetBaselineInfo_TimestampedSealedBaseline_ValidateResponse()
        {
            // Setup:
            var artifactToAdd = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.UseCase);

            var baselineArtifact = Helper.CreateBaseline(_user, _project, artifactToAddId: artifactToAdd.Id);
            var baseline = GetAndValidateBaseline(_user, baselineArtifact.Id, new List<int> { artifactToAdd.Id });

            var timestampDate = DateTime.UtcNow.AddMinutes(-3);
            baseline.UtcTimestamp = timestampDate;
            baseline.IsSealed = true;
            baselineArtifact.Update(_user, baseline);

            List<BaselineInfo> baselineInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() => {
                baselineInfoList = Helper.ArtifactStore.GetBaselineInfo(new List<int> { baselineArtifact.Id }, _user);
            }, "Getting BaselineInfo shouldn't throw an error.");

            // Verify:
            Assert.AreEqual(1, baselineInfoList?.Count, "List of BaselineInfo should have one item.");
            baseline = Helper.ArtifactStore.GetBaseline(_user, baselineArtifact.Id);
            baselineInfoList[0].AssertBaselineInfoCorrespondsToBaseline(baseline);
        }

        [TestCase]
        [TestRail(288884)]
        [Description("Create two Baselines, get BaselineInfo and check that response has expected values.")]
        public void GetBaselineInfo_TwoLiveBaseline_ValidateResponse()
        {
            // Setup:
            var baselineArtifact1 = Helper.CreateBaseline(_user, _project);
            var baselineArtifact2 = Helper.CreateBaseline(_user, _project);

            var baseline1 = Helper.ArtifactStore.GetBaseline(_user, baselineArtifact1.Id);

            var baseline2 = Helper.ArtifactStore.GetBaseline(_user, baselineArtifact2.Id);
            var timestampDate = DateTime.UtcNow.AddMinutes(-3);
            baseline2.UtcTimestamp = timestampDate;
            baseline2.IsSealed = true;
            baselineArtifact2.Update(_user, baseline2);
            baseline2 = Helper.ArtifactStore.GetBaseline(_user, baselineArtifact2.Id);

            List<BaselineInfo> baselineInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() => {
                baselineInfoList = Helper.ArtifactStore.GetBaselineInfo(new List<int> { baselineArtifact1.Id,
                    baselineArtifact2.Id }, _user);
            }, "Getting BaselineInfo shouldn't throw an error.");

            // Verify:
            Assert.AreEqual(2, baselineInfoList?.Count, "List of BaselineInfo should have two items.");
            baselineInfoList[0].AssertBaselineInfoCorrespondsToBaseline(baseline1);
            baselineInfoList[1].AssertBaselineInfoCorrespondsToBaseline(baseline2);
        }

        #endregion BaselineInfo Tests

        #endregion Positive Tests

        #region Negative Tests

        [TestCase]
        [TestRail(267115)]
        [Description("Add published Artifact to sealed Baseline, check 409 error message.")]
        public void AddArtifactToBaseline_PublishedArtifact_SealedBaseline_Check409()
        {
            // Setup:
            var artifactToAdd = Helper.CreateNovaArtifactInSpecificState(_user, _project, TestHelper.TestArtifactState.Published,
                ItemTypePredefined.Actor, _project.Id);

            var baselineArtifact = Helper.CreateBaseline(_user, _project);
            var baseline = Helper.ArtifactStore.GetBaseline(_user, baselineArtifact.Id);

            baseline.UtcTimestamp = DateTime.UtcNow.AddMinutes(-1);
            baseline.IsSealed = true;
            ArtifactStore.UpdateArtifact(Helper.ArtifactStore.Address, _user, baseline);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => {
                Helper.ArtifactStore.AddArtifactToBaseline(_user, artifactToAdd.Id, baseline.Id);
            }, "Adding artifact to Baseline shouldn't throw an error.");

            // Verify:
            string expectedErrorMessage = "Artifacts have not been added to the Baseline because the Baseline is sealed.";
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.SealedBaseline, expectedErrorMessage);
        }

        [TestCase]
        [TestRail(267228)]
        [Description("Try to set Baseline timestamp to the future, check 409 error message.")]
        public void EditBaseline_SetBaselinetTimeStampToTheFutureDate_Check409()
        {
            // Setup:
            var baselineArtifact = Helper.CreateBaseline(_user, _project);
            var baseline = Helper.ArtifactStore.GetBaseline(_user, baselineArtifact.Id);

            baseline.UtcTimestamp = DateTime.UtcNow.AddMinutes(1);//one minute from now
            
            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => {
                Helper.ArtifactStore.UpdateArtifact(_user, baseline);
            }, "Adding artifact to Baseline shouldn't throw an error.");

            // Verify:
            string expectedErrorMessage = "Baseline timestamp should be between project creation date and current time.";
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotSaveBaselineBecauseOfFutureTimestamp, expectedErrorMessage);
        }

        [TestCase]
        [TestRail(267245)]
        [Description("Try to remove artifact from sealed Baseline, check 409 error message.")]
        public void RemoveArtifactFromBaseline_SealedBaseline_Check409()
        {
            // Setup:
            var artifactToAdd = Helper.CreateNovaArtifactInSpecificState(_user, _project, TestHelper.TestArtifactState.Published,
                ItemTypePredefined.Actor, _project.Id);

            var baselineArtifact = Helper.CreateBaseline(_user, _project);
            var baseline = Helper.ArtifactStore.GetBaseline(_user, baselineArtifact.Id);

            baseline.UpdateArtifacts(artifactsIdsToAdd: new List<int> { artifactToAdd.Id });
            baseline.UtcTimestamp = DateTime.UtcNow.AddMinutes(-1);
            baseline.IsSealed = true;
            ArtifactStore.UpdateArtifact(Helper.ArtifactStore.Address, _user, baseline);

            baseline.UpdateArtifacts(artifactsIdsToRemove: new List<int> { artifactToAdd.Id });

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => {
                ArtifactStore.UpdateArtifact(Helper.ArtifactStore.Address, _user, baseline);
            }, "Attempt to remove artifact from sealed Baseline should throw 409 error.");

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.SealedBaseline, expectedInternalExceptionMessage);
        }

        [Category(Categories.GoldenData)]
        [TestCase]
        [TestRail(290070)]
        [Description("Create Baseline, get Baseline and update its baselinetimestamp with invalid value, which is less than MinimalUtcTimestamp. Verify that 409 Conflict.")]
        public void GetBaseline_SaveBaselineWithInvalidBaselineTimestamp_Check409()
        {
            // Setup:
            var baselineArtifact = Helper.CreateBaseline(_user, _project);
            var baseline = Helper.ArtifactStore.GetBaseline(_user, baselineArtifact.Id);
            var projectCreationDate = DateTime.ParseExact(
                ProjectExpectedCreationDateMap[_project.Id],
                "yyyy-MM-dd HH:mm:ss.fff",
                CultureInfo.InvariantCulture);

            baseline.UtcTimestamp = projectCreationDate.AddMinutes(-1);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => {
                Helper.ArtifactStore.UpdateArtifact(_user, baseline);
            }, "Adding artifact to Baseline shouldn't throw an error.");

            // Verify:
            const string expectedErrorMessage = "Baseline timestamp should be between project creation date and current time.";

            TestHelper.ValidateServiceError(ex.RestResponse,
                InternalApiErrorCodes.CannotSaveBaselineBecauseOfFutureTimestamp,
                expectedErrorMessage);

            ValidateMinimalUtctTimestamp(_project, baseline, ProjectExpectedCreationDateMap);
        }

        [TestCase]
        [TestRail(267154)]
        [Description("Try to add default Collection folder to the Baseline, check 404 error message.")]
        public void AddArtifactToBaseline_DefaultCollectionFolder_Check404()
        {
            // Setup:
            var defaultCollectionFolder = _project.GetDefaultCollectionFolder(_user);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => {
                Helper.CreateBaseline(_user, _project, artifactToAddId: defaultCollectionFolder.Id);
            }, "Adding artifact to Baseline shouldn't throw an error.");

            // Verify:
            string expectedErrorMessage = "You have attempted to access an artifact that does not exist or has been deleted.";
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound, expectedErrorMessage);
        }

        [TestCase(TestHelper.TestArtifactState.ScheduledToDelete)]
        [TestRail(267155)]
        [Description("Add scheduled to delete Artifact to Baseline, check 404 error message.")]
        public void AddArtifactToBaseline_ScheduledToDeleteOrDeletedArtifact_Check404(TestHelper.TestArtifactState artifactState)
        {
            // Setup:
            var artifactToAdd = Helper.CreateNovaArtifactInSpecificState(_user, _project, artifactState, ItemTypePredefined.Actor,
                _project.Id);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => {
                Helper.CreateBaseline(_user, _project, artifactToAddId: artifactToAdd.Id);
            }, "Adding artifact to Baseline shouldn't throw an error.");

            // Verify:
            string expectedErrorMessage = "You have attempted to access an artifact that does not exist or has been deleted.";
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound, expectedErrorMessage);
        }

        [TestCase]
        [TestRail(267153)]
        [Description("Create and publish artifact, add it to collection, delete artifact (don't publish), " +
            "add collection to Baseline - no artifact should be added.")]
        public void AddArtifactToBaseline_CollectionWithDeletedArtifactAddToBaseline_NothingWasAdded()
        {
            // Setup:
            var artifactToAdd = Helper.CreateNovaArtifactInSpecificState(_user, _project, TestHelper.TestArtifactState.Published,
                ItemTypePredefined.Actor, _project.Id);
            var collection = Helper.CreateUnpublishedCollectionWithArtifactsInSpecificState(_user, _project, TestHelper.TestArtifactState.Created,
                new List<int> { artifactToAdd.Id });

            Helper.ArtifactStore.PublishArtifacts(new List<int> { collection.Id }, _user);
            Helper.ArtifactStore.DeleteArtifact(artifactToAdd.Id, _user);

            var baseline = Helper.CreateBaseline(_user, _project);

            int numberOfAddedArtifacts = -1;

            // Execute:
            Assert.DoesNotThrow(() => {
                var addArtifactResult = Helper.ArtifactStore.AddArtifactToBaseline(_user, collection.Id, baseline.Id);
                numberOfAddedArtifacts = addArtifactResult.ArtifactCount;
            }, "Adding artifact to Baseline shouldn't throw an error.");

            // Verify:
            Assert.AreEqual(0, numberOfAddedArtifacts, "AddArtifactToBaseline should return 0 when nothing was added.");
        }

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
                var addArtifactResult = Helper.ArtifactStore.AddArtifactToBaseline(_user, baseline1.Id, baseline2.Id);
                numberOfAddedArtifacts = addArtifactResult.ArtifactCount;
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

            Helper.ArtifactStore.PublishArtifacts(new List<int> { baseline.Id }, _adminUser);
            Helper.SvcShared.LockArtifacts(_adminUser, new List<int> { baseline.Id });

            baseline.IsAvailableInAnalytics = true;

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => {
                ArtifactStore.UpdateArtifact(Helper.ArtifactStore.Address, _adminUser, baseline);
            }, "Attempt to set IsAvailableInAnalytics for unsealed Baseline should return 409 error." +
            "This option should be available for sealed Baselines only.");

            // Verify:
            // see TFS 5107
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.BaselineNotSealed, expectedInternalExceptionMessage);
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
            _projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_adminUser);
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

        [Category(Categories.GoldenData)]
        [TestCase]
        [TestRail(267352)]
        [Description("Get list of Reviews associated with baseline from Custom Data project, check that Reviews have expected values.")]
        public void GetReviews_ExistingSealedBaseline_ValidateReviewList()
        {
            // Setup:
            _projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_adminUser);
            var viewerUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _projectCustomData);
            const int baselineWithreviewsId = 110; // id of sealed Baseline which is used in 3 reviews

            // Execute: 
            ReviewRelationshipsResultSet reviews = null;
            Assert.DoesNotThrow(() => reviews = Helper.ArtifactStore.GetReviews(baselineWithreviewsId, viewerUser),
                "Get Baseline reviews shouldn't return an error.");

            // Verify:
            Assert.AreEqual(3, reviews.reviewArtifacts.Count, "List should have expected number of reviews.");
            foreach (var review in reviews.reviewArtifacts)
            {
                var reviewArtifact = (Review)Helper.ArtifactStore.GetArtifactDetails(viewerUser, review.ItemId);
                Assert.AreEqual(reviewArtifact.Name, review.ItemName, "Review name should have expected value.");
                Assert.AreEqual(reviewArtifact.Prefix, review.ItemTypePrefix, "Review ItemTypePrefix should have expected value.");
                Assert.AreEqual(reviewArtifact.CreatedOn, review.CreatedDate, "Review CreatedDate should have expected value.");
                Assert.AreEqual(reviewArtifact.ReviewStatus, review.Status, "Review status should should have expected value.");
                Assert.IsTrue(reviewArtifact.IsFormal, "Sealed baseline can be use in Formal reviews only.");
                Assert.IsNotEmpty(reviewArtifact.ReviewLink, "Review link shouldn't be empty.");
            }
        }

        #region Private Functions

        /// <summary>
        /// Checks that Baseline contains only artifacts with expected Ids
        /// </summary>
        /// <param name="user">User to get Baseline</param>
        /// <param name="baselineId">Id of Baseline to validate</param>
        /// <param name="expectedArtifactIds">List of expected Artifact's Id</param>
        /// <param name="isAvailableInAnalytics">(optional) Expected value for isAvailableInAnalytics. 'false' by default.</param>
        /// <param name="notAllArtifactsAreShown">(optional) Expected value for notAllArtifactsAreShown. Should be 'true' when user has no access to some of baseline's artifacts. 'false by default.</param>
        /// <param name="isSealed">(optional) Expected value for isSealed. 'false' by default.</param>
        /// <param name="utcTimestamp">(optional) Expected value for utcTimestamp. Null by default.</param>
        Baseline GetAndValidateBaseline(IUser user, int baselineId, List<int> expectedArtifactIds, bool isAvailableInAnalytics = false,
            bool notAllArtifactsAreShown = false, bool isSealed = false, DateTime? utcTimestamp = null)
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

            Assert.AreEqual(isAvailableInAnalytics, baseline.IsAvailableInAnalytics, "IsAvailableInAnalytics should have expected value.");
            Assert.AreEqual(notAllArtifactsAreShown, baseline.NotAllArtifactsAreShown, "NotAllArtifactsAreShown should have expected value.");
            Assert.AreEqual(isSealed, baseline.IsSealed, "IsSealed should have expected value.");

            if (utcTimestamp!=null)
            {
                Assert.AreEqual(utcTimestamp, baseline.UtcTimestamp, "UtcTimestamp should have expected value.");
            }
            return baseline;
        }

        /// <summary>
        /// Validate MinimalUtcTimestamp
        /// </summary>
        /// <param name="project">The project that baseline artifact gets created.</param>
        /// <param name="baseline">Baseline to validate</param>
        /// <param name="projectExpectedCreationDateMap">Map contains list of project Id with project creation time.</param>
        private static void ValidateMinimalUtctTimestamp(IProject project, Baseline baseline, Dictionary<int, string> projectExpectedCreationDateMap)
        {
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(baseline, nameof(baseline));
            ThrowIf.ArgumentNull(projectExpectedCreationDateMap, nameof(projectExpectedCreationDateMap));

            Assert.NotNull(baseline.MinimalUtcTimestamp, "MinimalUtcTimestamp from baseline artifact is null!");
            var formattedReceivedMinimalUtcTimestamp = ((DateTime)baseline.MinimalUtcTimestamp).ToString(
                "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);

            Assert.AreEqual(
                projectExpectedCreationDateMap[project.Id],
                formattedReceivedMinimalUtcTimestamp,
                "{0} was expected from MinimalUtcTimestamp but {1} was returned from the receievedBaseline",
                projectExpectedCreationDateMap[project.Id], formattedReceivedMinimalUtcTimestamp);
        }

        #endregion Private Functions
    }
}
