using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.StorytellerModel.Impl;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TestCommon;
using Utilities;
using Utilities.Factories;
using Model.ModelHelpers;

namespace ArtifactStoreTests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class SaveArtifactTests : TestBase
    {
        private const string UPDATE_ARTIFACT_ID_PATH = RestPaths.Svc.ArtifactStore.ARTIFACTS_id_;

        private IUser _user = null;
        private IProject _project = null;

        #region Setup and Cleanup

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_user);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region 200 OK tests

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForNovaRestMethods))]
        [TestRail(156656)]
        [Description("Create & publish an artifact.  Update the artifact.  Get the artifact.  Verify the artifact returned has the same properties as the artifact we updated.")]
        public void UpdateArtifact_PublishedArtifact_CanGetArtifact(ItemTypePredefined artifactType)
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);
            var artifact = Helper.CreateAndPublishNovaArtifact(author, _project, artifactType);

            artifact.Lock(author);
            artifact.RefreshArtifactFromServer(author);

            string newDescription = "NewDescription_" + RandomGenerator.RandomAlphaNumeric(5);
            CSharpUtilities.SetProperty(nameof(artifact.Description), newDescription, artifact.Artifact);

            // Execute:
            INovaArtifactDetails updateResult = null;

            Assert.DoesNotThrow(() => updateResult = UpdateArtifact(author, artifact),
                "'PATCH {0}' should return 200 OK for artifact with valid properties!", UPDATE_ARTIFACT_ID_PATH);

            // Verify:
            VerifyUpdateResult(artifact, updateResult);
            VerifyArtifactAfterUpdate(artifact, author);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForNovaRestMethods))]
        [TestRail(156917)]
        [Description("Create & save an artifact.  Update the artifact.  Get the artifact.  Verify the artifact returned has the same properties as the artifact we updated.")]
        public void UpdateArtifact_UnpublishedArtifact_CanGetArtifact(ItemTypePredefined artifactType)
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);
            var artifact = Helper.CreateNovaArtifact(author, _project, artifactType);

            artifact.RefreshArtifactFromServer(author);

            string newDescription = "NewDescription_" + RandomGenerator.RandomAlphaNumeric(5);
            CSharpUtilities.SetProperty(nameof(artifact.Description), newDescription, artifact.Artifact);

            // Execute:
            INovaArtifactDetails updateResult = null;

            Assert.DoesNotThrow(() => updateResult = UpdateArtifact(author, artifact),
                "'PATCH {0}' should return 200 OK for artifact with valid properties!", UPDATE_ARTIFACT_ID_PATH);

            // Verify:
            VerifyUpdateResult(artifact, updateResult);
            VerifyArtifactAfterUpdate(artifact, author);
        }

        [TestCase]  // It is working as designed for now. There is no check on user's permissions after artifact was locked
        [TestRail(190881)]
        [Description("Create & publish an artifact.  Lock artifact with an author, change permissions to viewer and update the artifact.  " +
                     "Verify the artifact is successfully updated and properties match expected values.")]
        public void UpdateArtifact_UserLosesPermissionsToArtifact_CanGetArtifact()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Process);
            var user = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);

            artifact.Lock(user);

            Helper.AssignProjectRolePermissionsToUser(user, TestHelper.ProjectRole.Viewer, _project);

            artifact.RefreshArtifactFromServer(user);

            string newName = "NewName_" + RandomGenerator.RandomAlphaNumeric(5);
            CSharpUtilities.SetProperty(nameof(artifact.Name), newName, artifact.Artifact);

            // Execute:
            INovaArtifactDetails updateResult = null;

            Assert.DoesNotThrow(() => updateResult = UpdateArtifact(user, artifact),
                "'PATCH {0}' should return 200 OK for artifact with valid properties!", UPDATE_ARTIFACT_ID_PATH);

            // Verify:
            VerifyUpdateResult(artifact, updateResult);
            VerifyArtifactAfterUpdate(artifact, user);
        }

        #region Artifact Properties tests

        [Category(Categories.CustomData)]
        [TestCase(ItemTypePredefined.Actor, PropertyPrimitiveType.Date,                 "Std-Date-Required-Validated-Min-Max-HasDefault", "2016-12-24T00:00:00")]   // Add now + 1 day, 1 hour, 1 min & 13 seconds.
        [TestCase(ItemTypePredefined.Process, PropertyPrimitiveType.Date,               "Std-Date-Required-Validated-Min-Max-HasDefault","2016-12-24T00:00:00")]
        [TestCase(ItemTypePredefined.PrimitiveFolder, PropertyPrimitiveType.Date,       "Std-Date-Required-Validated-Min-Max-HasDefault", "2016-12-24T00:00:00")]
        [TestCase(ItemTypePredefined.Document, PropertyPrimitiveType.Date,              "Std-Date-Required-Validated-Min-Max-HasDefault", "2016-12-24T00:00:00")]
        [TestCase(ItemTypePredefined.TextualRequirement, PropertyPrimitiveType.Date,    "Std-Date-Required-Validated-Min-Max-HasDefault", "2016-12-24T00:00:00")]
        [TestCase(ItemTypePredefined.Actor, PropertyPrimitiveType.Number,               "Std-Number-Required-Validated-DecPlaces-Min-Max-HasDefault", 4.2)]
        [TestCase(ItemTypePredefined.Process, PropertyPrimitiveType.Number,             "Std-Number-Required-Validated-DecPlaces-Min-Max-HasDefault", 5)]
        [TestCase(ItemTypePredefined.PrimitiveFolder, PropertyPrimitiveType.Number,     "Std-Number-Required-Validated-DecPlaces-Min-Max-HasDefault", -5)]
        [TestCase(ItemTypePredefined.Document, PropertyPrimitiveType.Number,            "Std-Number-Required-Validated-DecPlaces-Min-Max-HasDefault", -3)]
        [TestCase(ItemTypePredefined.TextualRequirement, PropertyPrimitiveType.Number,  "Std-Number-Required-Validated-DecPlaces-Min-Max-HasDefault", 0)]
        [TestCase(ItemTypePredefined.Actor, PropertyPrimitiveType.Text,                 "Std-Text-Required-RT-Multi-HasDefault", "This is the new text")]
        [TestCase(ItemTypePredefined.Process, PropertyPrimitiveType.Text,               "Std-Text-Required-RT-Multi-HasDefault", "This is the new text")]
        [TestCase(ItemTypePredefined.PrimitiveFolder, PropertyPrimitiveType.Text,       "Std-Text-Required-RT-Multi-HasDefault", "This is the new text")]
        [TestCase(ItemTypePredefined.Document, PropertyPrimitiveType.Text,              "Std-Text-Required-RT-Multi-HasDefault", "This is the new text")]
        [TestCase(ItemTypePredefined.TextualRequirement, PropertyPrimitiveType.Text,    "Std-Text-Required-RT-Multi-HasDefault", "This is the new text")]
        [TestCase(ItemTypePredefined.Actor, PropertyPrimitiveType.Choice,               "Std-Choice-Required-AllowMultiple-DefaultValue", new[] { "Blue" })]
        [TestCase(ItemTypePredefined.PrimitiveFolder, PropertyPrimitiveType.Choice,     "Std-Choice-Required-AllowMultiple-DefaultValue", new[] { "Green" })]
        [TestCase(ItemTypePredefined.Actor, PropertyPrimitiveType.Choice,               "Std-Choice-Required-AllowMultiple-DefaultValue", new[] { "Yellow" })]
        [TestCase(ItemTypePredefined.Document, PropertyPrimitiveType.Choice,            "Std-Choice-Required-AllowMultiple-DefaultValue", new[] { "Purple" })]
        [TestCase(ItemTypePredefined.TextualRequirement, PropertyPrimitiveType.Choice,  "Std-Choice-Required-AllowMultiple-DefaultValue", new[] { "Orange" })]
        [TestCase(ItemTypePredefined.Process, PropertyPrimitiveType.Choice,             "Std-Choice-Required-AllowMultiple-DefaultValue", new[] { "Green", "Blue" })]
        [TestCase(ItemTypePredefined.Process, PropertyPrimitiveType.Choice,             "Std-Choice-Required-AllowMultiple-DefaultValue", new[] { "Yellow", "Orange", "Purple" })]
        [TestCase(ItemTypePredefined.Actor, PropertyPrimitiveType.User,                 "Std-User-Required-HasDefault-User", "")] // newValue not used here, so pass empty string.
        [TestCase(ItemTypePredefined.Process, PropertyPrimitiveType.User,               "Std-User-Required-HasDefault-User", "")]
        [TestCase(ItemTypePredefined.PrimitiveFolder, PropertyPrimitiveType.User,       "Std-User-Required-HasDefault-User", "")]
        [TestCase(ItemTypePredefined.Document, PropertyPrimitiveType.User,              "Std-User-Required-HasDefault-User", "")]
        [TestCase(ItemTypePredefined.TextualRequirement, PropertyPrimitiveType.User,    "Std-User-Required-HasDefault-User", "")]
        [TestRail(191102)]
        [Description("Create and publish an artifact (that has custom properties). Change custom property. Verify the saved artifact has " +
                     "expected custom property change.")]
        public void UpdateArtifact_ChangePropertyAndSave_VerifyPropertyChange<T>(ItemTypePredefined itemType, PropertyPrimitiveType propertyType, 
            string propertyName, T newValue)
        {
            // Setup:
            var project = Helper.GetProject(TestHelper.GoldenDataProject.CustomData, _user);
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, project);
            var artifact = Helper.CreateWrapAndPublishNovaArtifactForStandardArtifactType(project, author, itemType);

            artifact.Lock(author);

            CustomProperty property = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                if (propertyType == PropertyPrimitiveType.User)
                {
                    property = UpdateArtifactCustomProperty(author, artifact, project, propertyType, propertyName, author);
                }
                else
                {
                    property = UpdateArtifactCustomProperty(author, artifact, project, propertyType, propertyName, newValue);
                }
            }, "'PATCH {0}' should return 200 OK for artifact with valid properties!", UPDATE_ARTIFACT_ID_PATH);

            // Verify:
            var artifactDetailsAfter = Helper.ArtifactStore.GetArtifactDetails(author, artifact.Id);
            var returnedProperty = artifactDetailsAfter.CustomPropertyValues.Find(p => p.Name == propertyName);

            ArtifactStoreHelper.AssertCustomPropertiesAreEqual(property, returnedProperty);
        }

        [TestCase(ItemTypePredefined.TextualRequirement)]
        [TestRail(2673578)]
        [Description("Create & publish.  Add discussion, attachment and trace to the artifact (save or publish).  Verify IndicatorFlags has all indicators.")]
        public void UpdateArtifact_WithDiscussionAttachmentAndTraceToFolder_ReturnsNewArtifactWithAttachment(ItemTypePredefined artifactType)
        {
            // Setup:
            // Create & add attachment to the source artifact:
            var attachmentFile = FileStoreTestHelper.CreateNovaFileWithRandomByteArray();
            var sourceArtifact = ArtifactStoreHelper.CreateArtifactWithAttachment(Helper, _project, _user, artifactType, attachmentFile, shouldPublishArtifact: true);
            Assert.IsNotNull(sourceArtifact, "Artifact with attachment is not created!");

            // Add discussion
            var discussion = Helper.SvcComponents.PostRapidReviewDiscussion(_user, sourceArtifact.Id, "Comment");
            Assert.IsNotNull(discussion, "Discussion is not created for the artifact!");

            // Add trace
            var targetArtifact = Helper.CreateNovaArtifact(_user, _project, ItemTypePredefined.PrimitiveFolder);
            ArtifactStoreHelper.AddManualArtifactTrace(_user, sourceArtifact, targetArtifact.Id, _project.Id);

            // Execute:
            Assert.DoesNotThrow(() => UpdateArtifact(_user, sourceArtifact),
                "'PATCH {0}' should return 200 OK for artifact with valid properties!", UPDATE_ARTIFACT_ID_PATH);

            // Verify:
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, sourceArtifact.Id, ItemIndicatorFlags.HasComments);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, sourceArtifact.Id, ItemIndicatorFlags.HasAttachmentsOrDocumentRefs);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, sourceArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, targetArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
        }

        #endregion Artifact Properties tests

        #region Subartifact Properties tests

        //TODO: Refactor artifact & subartifact properties tests to use changesets as in the example below

        [TestCase]
        [TestRail(267358)]
        [Description("Create a Process artifact.  Add discussion, attachment and trace to the artifact and save.  Verify IndicatorFlags has all indicators.")]
        public void UpdateSubArtifact_WithDiscussionAttachmentAndTraceToFolder_ReturnsNewArtifactWithAttachment()
        {
            // Setup:
            var sourceArtifact = Helper.CreateNovaArtifact(_user, _project, ItemTypePredefined.Process);
            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, sourceArtifact.Id);

            // Create & add attachment to sub-artifact:
            var attachmentFile = FileStoreTestHelper.CreateNovaFileWithRandomByteArray();
            ArtifactStoreHelper.AddSubArtifactAttachmentAndSave(_user, sourceArtifact, subArtifacts[0].Id, attachmentFile, Helper.ArtifactStore, shouldLockArtifact: false);

            // Add discussion
            var discussion = Helper.SvcComponents.PostRapidReviewDiscussion(_user, subArtifacts[0].Id, "Comment");
            Assert.IsNotNull(discussion, "Discussion is not created for the artifact!");

            // Add trace
            var targetArtifact = Helper.CreateNovaArtifact(_user, _project, ItemTypePredefined.PrimitiveFolder);
            var novaSubArtifacts = ArtifactStoreHelper.GetDetailsForAllSubArtifacts(sourceArtifact.Id, subArtifacts, _user);

            var trace = new NovaTrace(targetArtifact);
            novaSubArtifacts[0].Traces = new List<NovaTrace> { trace };

            sourceArtifact.RefreshArtifactFromServer(_user);
            sourceArtifact.SubArtifacts = novaSubArtifacts;

            // Execute:
            Assert.DoesNotThrow(() => UpdateArtifact(_user, sourceArtifact),
                "'PATCH {0}' should return 200 OK for artifact with valid properties!", UPDATE_ARTIFACT_ID_PATH);

            // Verify:
            sourceArtifact.RefreshArtifactFromServer(_user);

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, sourceArtifact.Id, ItemIndicatorFlags.HasComments, subArtifacts[0].Id);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, sourceArtifact.Id, ItemIndicatorFlags.HasAttachmentsOrDocumentRefs, subArtifacts[0].Id);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, sourceArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces, subArtifacts[0].Id);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, targetArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
        }

        [Category(Categories.CustomData)]
        [TestCase(ItemTypePredefined.Process, PropertyPrimitiveType.Date,   Process.DefaultUserTaskName, "Cust-Date",            "2016-12-24T00:00:00")]
        [TestCase(ItemTypePredefined.Process, PropertyPrimitiveType.Number, Process.DefaultUserTaskName, "Cust-Number",          5)]
        [TestCase(ItemTypePredefined.Process, PropertyPrimitiveType.Text,   Process.DefaultUserTaskName, "Cust-Text",            "This is new text")]
        [TestCase(ItemTypePredefined.Process, PropertyPrimitiveType.Choice, Process.DefaultUserTaskName, "Cust-Choice-Multiple", new[] { "Math" })]
        [TestCase(ItemTypePredefined.Process, PropertyPrimitiveType.Choice, Process.DefaultUserTaskName, "Cust-Choice-Multiple", new[] { "Math", "English" })]
        [TestCase(ItemTypePredefined.Process, PropertyPrimitiveType.User,   Process.DefaultUserTaskName, "Cust-User",            "")] // newValue not used here, so pass empty string.
        [TestRail(191159)]
        [Description("Create and publish an artifact with subartifact (that has custom properties). Change custom property in subartifact. " +
                     "Verify the saved subartifact artifact has the expected custom property change.")]
        public void UpdateSubArtifact_ChangePropertyAndSave_VerifyPropertyChanged<T>(ItemTypePredefined itemType,
            PropertyPrimitiveType propertyType, string subArtifactDisplayName, string propertyName, T newValue)
        {
            // Setup:
            var project = Helper.GetProject(TestHelper.GoldenDataProject.EmptyProjectNonRequiredCustomPropertiesAssigned, _user);
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, project);
            var artifact = Helper.CreateWrapAndPublishNovaArtifactForCustomArtifactType(project, author, itemType);

            // Update custom property in artifact.
            var subArtifact = Helper.ArtifactStore.GetSubartifacts(author, artifact.Id).Find(sa => sa.DisplayName.Equals(subArtifactDisplayName));
            var novaSubArtifact = Helper.ArtifactStore.GetSubartifact(author, artifact.Id, subArtifact.Id);
            artifact.Lock(author);

            CustomProperty property = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                if (propertyType == PropertyPrimitiveType.User)
                {
                    property = UpdateSubArtifactCustomProperty(author, artifact, novaSubArtifact, project, propertyType, propertyName,
                        author);
                }
                else
                {
                    property = UpdateSubArtifactCustomProperty(author, artifact, novaSubArtifact, project, propertyType, propertyName,
                        newValue);
                }
            }, "'PATCH {0}' should return 200 OK for artifact with valid properties!", UPDATE_ARTIFACT_ID_PATH);

            // Verify:
            var subArtifactAfter = Helper.ArtifactStore.GetSubartifact(author, artifact.Id, subArtifact.Id);

            var returnedProperty = subArtifactAfter.CustomPropertyValues.Find(p => p.Name.Equals(propertyName));

            ArtifactStoreHelper.AssertCustomPropertiesAreEqual(property, returnedProperty);
        }

        [Category(Categories.CustomData)]
        [TestCase(ItemTypePredefined.Process, Process.DefaultUserTaskName, "Std-Text-Required-RT-Multi-HasDefault")]
        [TestRail(191159)]
        [Description("Create & publish an artifact.  Update a text property in a subartifact, save and publish.  " +
                     "Verify the sub artifact returned the text property updated.")]
        public void UpdateSubArtifact_ChangeTextPropertySaveAndPublish_VerifyPropertyChanged(ItemTypePredefined itemType,
            string subArtifactDisplayName, string subArtifactCustomPropertyName)
        {
            // Setup:
            var projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, projectCustomData);
            var artifact = Helper.CreateWrapAndPublishNovaArtifactForStandardArtifactType(projectCustomData, author, itemType);

            // Change custom property text value
            var subArtifactCustomPropertyValue = StringUtilities.WrapInHTML(WebUtility.HtmlEncode(
                RandomGenerator.RandomAlphaNumericUpperAndLowerCaseAndSpecialCharactersWithSpaces()));

            var subArtifactChangeSet = CreateSubArtifactChangeSet(author, projectCustomData, artifact, subArtifactDisplayName, subArtifactCustomPropertyName, subArtifactCustomPropertyValue);
            artifact.SubArtifacts = new List<NovaSubArtifact> { subArtifactChangeSet };

            var requestedCustomProperty = subArtifactChangeSet.CustomPropertyValues.Find(p => p.Name.Equals(subArtifactCustomPropertyName));

            // Execute:
            artifact.Lock(author);

            Assert.DoesNotThrow(() => UpdateArtifact(author, artifact),
                "'PATCH {0}' should return 200 OK for artifact with valid properties!", UPDATE_ARTIFACT_ID_PATH);

            artifact.Publish(author);

            // Verify:
            Assert.NotNull(subArtifactChangeSet.Id, "The SubArtifact ID shouldn't be null!");
            var subArtifactAfter = Helper.ArtifactStore.GetSubartifact(author, artifact.Id, subArtifactChangeSet.Id.Value);

            var returnedCustomProperty = subArtifactAfter.CustomPropertyValues.Find(p => p.Name.Equals(subArtifactCustomPropertyName));

            ArtifactStoreHelper.AssertCustomPropertiesAreEqual(requestedCustomProperty, returnedCustomProperty);
        }

        [Category(Categories.CustomData)]
        [TestCase(ItemTypePredefined.Process, Process.DefaultUserTaskName, "Std-Number-Required-Validated-DecPlaces-Min-Max-HasDefault", 5)]
        [TestCase(ItemTypePredefined.Process, Process.DefaultUserTaskName, "Std-Number-Required-Validated-DecPlaces-Min-Max-HasDefault", -5)]
        [TestCase(ItemTypePredefined.Process, Process.DefaultUserTaskName, "Std-Number-Required-Validated-DecPlaces-Min-Max-HasDefault", 4.2)]
        [TestCase(ItemTypePredefined.Process, Process.DefaultUserTaskName, "Std-Number-Required-Validated-DecPlaces-Min-Max-HasDefault", -3)]
        [TestCase(ItemTypePredefined.Process, Process.DefaultUserTaskName, "Std-Number-Required-Validated-DecPlaces-Min-Max-HasDefault", 0)]
        [TestRail(191160)]
        [Description("Create & publish an artifact.  Update a number property in a subartifact, save and publish.  " +
                     "Verify the sub artifact returned the number property updated.")]
        public void UpdateSubArtifact_ChangeNumberPropertySaveAndPublish_VerifyPropertyChanged(ItemTypePredefined itemType, 
            string subArtifactDisplayName, string subArtifactCustomPropertyName, double newNumber)
        {
            // Setup:
            var projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, projectCustomData);
            var artifact = Helper.CreateWrapAndPublishNovaArtifactForStandardArtifactType(projectCustomData, author, itemType);

            // Change custom property number value
            var subArtifactCustomPropertyValue = newNumber;
            var subArtifactChangeSet = CreateSubArtifactChangeSet(author, projectCustomData, artifact, subArtifactDisplayName, subArtifactCustomPropertyName, subArtifactCustomPropertyValue);
            artifact.SubArtifacts = new List<NovaSubArtifact> { subArtifactChangeSet };

            var requestedCustomProperty = subArtifactChangeSet.CustomPropertyValues.Find(p => p.Name.Equals(subArtifactCustomPropertyName));

            // Execute:
            artifact.Lock(author);

            Assert.DoesNotThrow(() => UpdateArtifact(author, artifact),
                "'PATCH {0}' should return 200 OK for artifact with valid properties!", UPDATE_ARTIFACT_ID_PATH);

            artifact.Publish(author);

            // Verify:
            Assert.NotNull(subArtifactChangeSet.Id, "The SubArtifact ID shouldn't be null!");
            NovaSubArtifact subArtifactAfter = Helper.ArtifactStore.GetSubartifact(author, artifact.Id, subArtifactChangeSet.Id.Value);

            var returnedCustomProperty = subArtifactAfter.CustomPropertyValues.Find(p => p.Name.Equals(subArtifactCustomPropertyName));

            ArtifactStoreHelper.AssertCustomPropertiesAreEqual(requestedCustomProperty, returnedCustomProperty);
        }

        [Category(Categories.CustomData)]
        [TestCase(ItemTypePredefined.Process, Process.DefaultUserTaskName, "Std-Date-Required-Validated-Min-Max-HasDefault")]
        [TestRail(191161)]
        [Description("Create & publish an artifact.  Update a date property in a subartifact, save and publish.  " +
                     "Verify the sub artifact returned the date property updated.")]
        public void UpdateSubArtifact_ChangeDatePropertySaveAndPublish_VerifyPropertyChanged(ItemTypePredefined itemType, 
            string subArtifactDisplayName, string subArtifactCustomPropertyName)
        {
            // Setup:
            var projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, projectCustomData);
            var artifact = Helper.CreateWrapAndPublishNovaArtifactForStandardArtifactType(projectCustomData, author, itemType);

            // Change custom property date value
            var subArtifactCustomPropertyValue = DateTimeUtilities.ConvertDateTimeToSortableDateTime(DateTime.Now);
            var subArtifactChangeSet = CreateSubArtifactChangeSet(author, projectCustomData, artifact, subArtifactDisplayName, subArtifactCustomPropertyName, subArtifactCustomPropertyValue);
            var artifactDetails = TestHelper.CreateArtifactChangeSet(artifact.Artifact);
            artifactDetails.SubArtifacts = new List<NovaSubArtifact> { subArtifactChangeSet };
            var requestedCustomProperty = subArtifactChangeSet.CustomPropertyValues.Find(p => p.Name.Equals(subArtifactCustomPropertyName));

            // Execute:
            artifact.Lock(author);

            Assert.DoesNotThrow(() => UpdateArtifact(author, artifact, artifactDetails),
                "'PATCH {0}' should return 200 OK for artifact with valid properties!", UPDATE_ARTIFACT_ID_PATH);

            artifact.Publish(author);

            // Verify:
            Assert.NotNull(subArtifactChangeSet.Id, "The SubArtifact ID shouldn't be null!");
            var subArtifactAfter = Helper.ArtifactStore.GetSubartifact(author, artifact.Id, subArtifactChangeSet.Id.Value);

            var returnedCustomProperty = subArtifactAfter.CustomPropertyValues.Find(p => p.Name.Equals(subArtifactCustomPropertyName));

            ArtifactStoreHelper.AssertCustomPropertiesAreEqual(requestedCustomProperty, returnedCustomProperty);
        }

        [Category(Categories.CustomData)]
        [TestCase(ItemTypePredefined.Process, Process.DefaultUserTaskName, "Std-Choice-Required-AllowMultiple-DefaultValue", "Blue")]
        [TestCase(ItemTypePredefined.Process, Process.DefaultUserTaskName, "Std-Choice-Required-AllowMultiple-DefaultValue", "Green")]
        [TestCase(ItemTypePredefined.Process, Process.DefaultUserTaskName, "Std-Choice-Required-AllowMultiple-DefaultValue", "Yellow")]
        [TestCase(ItemTypePredefined.Process, Process.DefaultUserTaskName, "Std-Choice-Required-AllowMultiple-DefaultValue", "Purple")]
        [TestCase(ItemTypePredefined.Process, Process.DefaultUserTaskName, "Std-Choice-Required-AllowMultiple-DefaultValue", "Orange")]
        [TestRail(191162)]
        [Description("Create & publish an artifact.  Update a choice property in a subartifact, save and publish. " +
                     "Verify the sub artifact returned the choice property updated.")]
        public void UpdateSubArtifact_ChangeChoicePropertySaveAndPublish_VerifyPropertyChanged(ItemTypePredefined itemType, 
            string subArtifactDisplayName, string subArtifactCustomPropertyName, string newChoiceValue)
        {
            // Setup:
            var projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, projectCustomData);
            var artifact = Helper.CreateWrapAndPublishNovaArtifactForStandardArtifactType(projectCustomData, author, itemType);

            // Change custom property choice value
            var subArtifactCustomPropertyValue = new List<string> { newChoiceValue };
            var subArtifactChangeSet = CreateSubArtifactChangeSet(author, projectCustomData, artifact, subArtifactDisplayName, subArtifactCustomPropertyName, subArtifactCustomPropertyValue);
            artifact.SubArtifacts = new List<NovaSubArtifact> { subArtifactChangeSet };

            var requestedCustomProperty = subArtifactChangeSet.CustomPropertyValues.Find(p => p.Name.Equals(subArtifactCustomPropertyName));

            // Execute:
            artifact.Lock(author);

            Assert.DoesNotThrow(() => UpdateArtifact(author, artifact),
                "'PATCH {0}' should return 200 OK for artifact with valid properties!", UPDATE_ARTIFACT_ID_PATH);

            artifact.Publish(author);

            // Verify:
            Assert.NotNull(subArtifactChangeSet.Id, "The SubArtifact ID shouldn't be null!");
            var subArtifactAfter = Helper.ArtifactStore.GetSubartifact(author, artifact.Id, subArtifactChangeSet.Id.Value);

            var returnedCustomProperty = subArtifactAfter.CustomPropertyValues.Find(p => p.Name.Equals(subArtifactCustomPropertyName));

            ArtifactStoreHelper.AssertCustomPropertiesAreEqual(requestedCustomProperty, returnedCustomProperty);
        }

        [Category(Categories.CustomData)]
        [TestCase(ItemTypePredefined.Process, Process.DefaultUserTaskName, "Std-Choice-Required-AllowMultiple-DefaultValue", new[] { "Green","Blue" } )]
        [TestCase(ItemTypePredefined.Process, Process.DefaultUserTaskName, "Std-Choice-Required-AllowMultiple-DefaultValue", new[] { "Yellow", "Orange", "Purple" })]
        [TestRail(195423)]
        [Description("Create & publish an artifact. Update a choice property with multiple selection in a subartifact, save and publish. " +
                     "Verify the sub artifact returned the choice property updated.")]
        public void UpdateSubArtifact_ChangeChoicePropertyWithMultipleSelectionSaveAndPublish_VerifyPropertyChanged(ItemTypePredefined itemType,
            string subArtifactDisplayName, string subArtifactCustomPropertyName, string[] newChoiceValues)
        {
            // Setup:
            var projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, projectCustomData);
            var artifact = Helper.CreateWrapAndPublishNovaArtifactForStandardArtifactType(projectCustomData, author, itemType);

            // Change custom property choice value
            var subArtifactCustomPropertyValue = new List<string>();
            subArtifactCustomPropertyValue.AddRange(newChoiceValues);

            var subArtifactChangeSet = CreateSubArtifactChangeSet(author, projectCustomData, artifact, subArtifactDisplayName, subArtifactCustomPropertyName, subArtifactCustomPropertyValue);
            artifact.SubArtifacts = new List<NovaSubArtifact> { subArtifactChangeSet };

            var requestedCustomProperty = subArtifactChangeSet.CustomPropertyValues.Find(p => p.Name.Equals(subArtifactCustomPropertyName));

            // Execute:
            artifact.Lock(author);

            Assert.DoesNotThrow(() => UpdateArtifact(author, artifact),
                "'PATCH {0}' should return 200 OK for artifact with valid properties!", UPDATE_ARTIFACT_ID_PATH);

            artifact.Publish(author);

            // Verify:
            Assert.NotNull(subArtifactChangeSet.Id, "The SubArtifact ID shouldn't be null!");
            var subArtifactAfter = Helper.ArtifactStore.GetSubartifact(author, artifact.Id, subArtifactChangeSet.Id.Value);

            var returnedCustomProperty = subArtifactAfter.CustomPropertyValues.Find(p => p.Name.Equals(subArtifactCustomPropertyName));

            ArtifactStoreHelper.AssertCustomPropertiesAreEqual(requestedCustomProperty, returnedCustomProperty);
        }

        [Category(Categories.CustomData)]
        [TestRail(191163)]
        [TestCase(ItemTypePredefined.Process, Process.DefaultUserTaskName, "Std-User-Required-HasDefault-User")]
        [Description("Create & publish an artifact.  Update a user property in a subartifact, save and publish. " +
                     "Verify the sub artifact returned the user property updated.")]
        public void UpdateSubArtifact_ChangeUserPropertySaveAndPublish_VerifyPropertyChanged(ItemTypePredefined itemType, 
            string subArtifactDisplayName, string subArtifactCustomPropertyName)
        {
            // Setup:
            var projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, projectCustomData);
            var artifact = Helper.CreateWrapAndPublishNovaArtifactForStandardArtifactType(projectCustomData, author, itemType);

            // Change custom property user value
            var subArtifactCustomPropertyValue = author;
            var subArtifactChangeSet = CreateSubArtifactChangeSet(author, projectCustomData, artifact, subArtifactDisplayName, subArtifactCustomPropertyName, subArtifactCustomPropertyValue);
            artifact.SubArtifacts = new List<NovaSubArtifact> { subArtifactChangeSet };

            var requestedCustomProperty = subArtifactChangeSet.CustomPropertyValues.Find(p => p.Name.Equals(subArtifactCustomPropertyName));

            // Execute:
            artifact.Lock(author);

            Assert.DoesNotThrow(() => UpdateArtifact(author, artifact),
                "'PATCH {0}' should return 200 OK for artifact with valid properties!", UPDATE_ARTIFACT_ID_PATH);

            artifact.Publish(author);

            // Verify:
            Assert.NotNull(subArtifactChangeSet.Id, "The SubArtifact ID shouldn't be null!");
            var subArtifactAfter = Helper.ArtifactStore.GetSubartifact(author, artifact.Id, subArtifactChangeSet.Id.Value);

            var returnedCustomProperty = subArtifactAfter.CustomPropertyValues.Find(p => p.Name.Equals(subArtifactCustomPropertyName));

            ArtifactStoreHelper.AssertCustomPropertiesAreEqual(requestedCustomProperty, returnedCustomProperty);
        }

        #endregion Subartifact Properties tests

        #endregion 200 OK tests

        #region Negative tests

        #region 400 Bad Request

        private const string ARTIFACT_NOT_PROVIDED = "Artifact not provided.";

        [TestCase]
        [TestRail(156662)]
        [Description("Try to update an artifact, but send an empty request body.  Verify 400 Bad Request is returned.")]
        public void UpdateArtifact_EmptyBody_400BadRequest()
        {
            // Setup:
            var artifact = Helper.CreateNovaArtifact(_user, _project, ItemTypePredefined.Actor);
            string requestBody = string.Empty;

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                ArtifactStoreHelper.UpdateInvalidArtifact(Helper.ArtifactStore.Address, requestBody, artifact.Id, _user);
            }, "'PATCH {0}' should return 400 Bad Request if an empty body is sent!",
                UPDATE_ARTIFACT_ID_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.IncorrectInputParameters, ARTIFACT_NOT_PROVIDED);
        }

        [TestCase]
        [TestRail(156663)]
        [Description("Try to update an artifact, but send a corrupt JSON request body.  Verify 400 Bad Request is returned.")]
        public void UpdateArtifact_CorruptBody_400BadRequest()
        {
            // Setup:
            var artifact = Helper.CreateNovaArtifact(_user, _project, ItemTypePredefined.Document);
            string requestBody = JsonConvert.SerializeObject(artifact);

            // Remove first 5 characters to corrupt the JSON string, thereby corrupting the JSON structure.
            requestBody = requestBody.Remove(0, 5);

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                ArtifactStoreHelper.UpdateInvalidArtifact(Helper.ArtifactStore.Address, requestBody, artifact.Id, _user);
            }, "'PATCH {0}' should return 400 Bad Request if a corrupt JSON body is sent!",
                UPDATE_ARTIFACT_ID_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.IncorrectInputParameters, ARTIFACT_NOT_PROVIDED);
        }

        [TestCase]
        [TestRail(157057)]
        [Description("Try to update an artifact, but send a JSON request body without an 'Id' property.  Verify 400 Bad Request is returned.")]
        public void UpdateArtifact_MissingIdInJsonBody_400BadRequest()
        {
            // Setup:
            var artifact = Helper.CreateNovaArtifact(_user, _project, ItemTypePredefined.Actor);
            string requestBody = JsonConvert.SerializeObject(artifact);

            // Remove the 'Id' property by renaming it.
            requestBody = requestBody.Replace("\"Id\"", "\"NotId\"");

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                ArtifactStoreHelper.UpdateInvalidArtifact(Helper.ArtifactStore.Address, requestBody, artifact.Id, _user);
            }, "'PATCH {0}' should return 400 Bad Request if the 'Id' property is missing in the JSON body!", UPDATE_ARTIFACT_ID_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.IncorrectInputParameters, ARTIFACT_NOT_PROVIDED);
        }

        [TestCase]
        [TestRail(156664)]
        [Description("Try to update an artifact, but send a different Artifact ID in the URL vs request body.  Verify 400 Bad Request is returned.")]
        public void UpdateArtifact_DifferentArtifactIdsInUrlAndBody_400BadRequest()
        {
            // Setup:
            var artifact1 = Helper.CreateNovaArtifact(_user, _project, ItemTypePredefined.Actor);
            var artifact2 = Helper.CreateNovaArtifact(_user, _project, ItemTypePredefined.Actor);

            string requestBody = JsonConvert.SerializeObject(artifact1);
            int wrongArtifactId = artifact2.Id;

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                ArtifactStoreHelper.UpdateInvalidArtifact(Helper.ArtifactStore.Address, requestBody, wrongArtifactId, _user);
            }, "'PATCH {0}' should return 400 Bad Request if the Artifact ID in the URL is different than in the body!",
                UPDATE_ARTIFACT_ID_PATH);

            // Verify:
            const string expectedMessage = "Artifact does not match Id of request.";
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.IncorrectInputParameters, expectedMessage);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForNovaRestMethods))]
        [TestRail(164531)]
        [Description("Create & publish an artifact.  Update the artifact property 'Name' with Empty space. Verify 400 Bad Request returned.")]
        public void UpdateArtifact_PublishedArtifact_SetEmptyNameProperty_400BadRequest(ItemTypePredefined artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, _project, artifactType);
            artifact.Lock(_user);
            artifact.Name = string.Empty;

            string requestBody = JsonConvert.SerializeObject(artifact.Artifact);

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                ArtifactStoreHelper.UpdateInvalidArtifact(Helper.ArtifactStore.Address, requestBody, artifact.Id, _user);
            }, "'PATCH {0}' should return 400 Bad Request if the artifact name property is empty!", UPDATE_ARTIFACT_ID_PATH);

            // Verify
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.NameCannotBeEmpty, "The Item name cannot be empty");
        }

        #endregion 400 Bad Request

        #region 401 Unauthorized

        [TestCase]
        [TestRail(156657)]
        [Description("Create & save an artifact.  Try to update the artifact but don't send a 'Session-Token' header in the request.  Verify 400 Bad Request is returned.")]
        public void UpdateArtifact_NoTokenHeader_401Unauthorized()
        {
            // Setup:
            var artifact = Helper.CreateNovaArtifact(_user, _project, ItemTypePredefined.Glossary);

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() => UpdateArtifact(null, artifact),
                "'PATCH {0}' should return 401 Unauthorized if no Session-Token header is passed!", UPDATE_ARTIFACT_ID_PATH);

            // Verify:
            TestHelper.ValidateBodyContents(ex.RestResponse, "Unauthorized call");
        }

        [TestCase]
        [TestRail(156658)]
        [Description("Create & save an artifact.  Try to update the artifact but pass an unauthorized token.  Verify 401 Unauthorized is returned.")]
        public void UpdateArtifact_UnauthorizedToken_401Unauthorized()
        {
            // Setup:
            var artifact = Helper.CreateNovaArtifact(_user, _project, ItemTypePredefined.PrimitiveFolder);
            var userWithBadToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() => UpdateArtifact(userWithBadToken, artifact),
                "'PATCH {0}' should return 401 Unauthorized if an invalid token is passed!", UPDATE_ARTIFACT_ID_PATH);

            // Verify
            TestHelper.ValidateBodyContents(ex.RestResponse, "Unauthorized call");
        }

        #endregion 401 Unauthorized

        // NOTE: No 403 tests are possible because the user needs to lock the artifact before updating it, so the lock would fail if the user has no access to it.

        #region 404 Not Found

        private const string ARTIFACT_NOT_FOUND = "You have attempted to access an artifact that does not exist or has been deleted.";

        [TestCase(0)]
        [TestCase(int.MaxValue)]
        [TestRail(156660)]
        [Description("Try to update an artifact with a non-existent Artifact ID.  Verify 404 Not Found is returned.")]
        public void UpdateArtifact_NonExistentArtifactId_404NotFound(int nonExistentArtifactId)
        {
            // Setup:
            var artifact = Helper.CreateNovaArtifact(_user, _project, ItemTypePredefined.UseCaseDiagram);
            var updateArtifact = ArtifactStoreHelper.CreateNovaArtifactDetailsForUpdate(artifact);

            // Replace ProjectId with a fake ID that shouldn't exist.
            updateArtifact.Id = nonExistentArtifactId;

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.UpdateArtifact(_user, updateArtifact),
                "'PATCH {0}' should return 404 Not Found if the Artifact ID doesn't exist!", UPDATE_ARTIFACT_ID_PATH);

            // Verify:
            // Skip for Id == 0 because in that case IIS returns the generic HTML 404 response.
            if (nonExistentArtifactId != 0)
            {
                TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound, ARTIFACT_NOT_FOUND);
            }
        }

        [TestCase]
        [TestRail(156661)]
        [Description("Create & publish an artifact, then delete & publish it.  Try to update the deleted artifact.  Verify 404 Not Found is returned.")]
        public void UpdateArtifact_DeletedArtifact_404NotFound()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.TextualRequirement);
            artifact.Delete(_user);
            artifact.Publish(_user);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => UpdateArtifact(_user, artifact),
                "'PATCH {0}' should return 404 Not Found if the artifact was deleted!", UPDATE_ARTIFACT_ID_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound, ARTIFACT_NOT_FOUND);
        }

        #endregion 404 Not Found

        #region 409 Conflict

        #region Artifact Properties tests

        [Category(Categories.CustomData)]
        [TestCase(ItemTypePredefined.Process, "Std-Text-Required-RT-Multi-HasDefault", null)]
        [TestCase(ItemTypePredefined.Process, "Std-Text-Required-RT-Multi-HasDefault", "", Explicit = true, Reason = IgnoreReasons.ProductBug)] // Bug: 5086
        [TestRail(195433)]
        [Description("Create & publish an artifact.  Update a text property in a sub artifact with no contents, save and publish.  " +
                     "Verify 409 Conflict is returned at the event of publishing the invalid change.")]
        public void UpdateArtifact_ChangeTextPropertyWithEmptyOrNull_Verify409Conflict(ItemTypePredefined itemType, string artifactCustomPropertyName, string subArtifactCustomPropertyValue)
        {
            // Setup: Set the required custom text property value for the target sub artifact with empty content
            var projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, projectCustomData);
            var artifact = Helper.CreateWrapAndPublishNovaArtifactForStandardArtifactType(projectCustomData, author, itemType);

            var artifactDetailsChangeSet = CreateArtifactChangeSet(author, projectCustomData, artifact, artifactCustomPropertyName, subArtifactCustomPropertyValue);

            // Execute: Attempt to update the target sub artifact with empty content
            artifact.Lock(author);
            Helper.ArtifactStore.UpdateArtifact(author, artifactDetailsChangeSet);
            var ex = Assert.Throws<Http409ConflictException>(() => artifact.Publish(author),
                "'POST {0}' should return 409 Conflict if the artifact containing invalid change!", RestPaths.Svc.ArtifactStore.ARTIFACTS);

            // Verify: Check that returned custom property name equals to default custom property since the requsted updated is invalid
            // Validation: Exception should contain proper errorCode in the response content
            string expectedError = I18NHelper.FormatInvariant("Artifact with ID {0} has validation errors.", artifact.Id);
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotPublishOverValidationErrors, expectedError);
        }

        #endregion Artifact Properties tests

        #region Subartifact Properties tests

        // TODO: Should returns 409 Conflict error if server side validation works for customproperties of sub artifact but it's disabled for December release.
        // TODO: Currently, artifact publish pass without serverside validation when there is invalid data for custom property of sub artifact 
        [Category(Categories.CustomData)]
        [TestCase(ItemTypePredefined.Process, Process.DefaultUserTaskName, "Std-Text-Required-RT-Multi-HasDefault")]
        [TestRail(195408)]
        [Explicit(IgnoreReasons.ProductBug)]
        [Description("Create & publish an artifact.  Update a text property in a sub artifact with no contents, save and publish.  " +
                     "Verify 409 Conflict is returned at the event of publishing the invalid change.")]
        public void UpdateSubArtifact_ChangeTextPropertyWithEmpty_Verify409Conflict(ItemTypePredefined itemType,
            string subArtifactDisplayName, string subArtifactCustomPropertyName)
        {
            // Setup: Set the required custom text property value for the target sub artifact with empty content
            var projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, projectCustomData);
            var artifact = Helper.CreateWrapAndPublishNovaArtifactForStandardArtifactType(projectCustomData, author, itemType);

            var subArtifactCustomPropertyValue = "";
            var subArtifactChangeSet = CreateSubArtifactChangeSet(
                author, projectCustomData, artifact, subArtifactDisplayName, subArtifactCustomPropertyName, subArtifactCustomPropertyValue);
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, artifact.Id);
            artifactDetails.SubArtifacts = new List<NovaSubArtifact> { subArtifactChangeSet };

            // Execute: Attempt to update the target sub artifact with empty content
            artifact.Lock(author);
            Helper.ArtifactStore.UpdateArtifact(author, artifactDetails);

            var ex = Assert.Throws<Http409ConflictException>(() => artifact.Publish(author),
                "'POST {0}' should return 409 Conflict if the artifact containing invalid change!", RestPaths.Svc.ArtifactStore.ARTIFACTS);

            // Verify: Check that returned custom property name equals to default custom property since the requsted updated is invalid
            // Validation: Exception should contain proper errorCode in the response content
            string expectedError = I18NHelper.FormatInvariant("Artifact with ID {0} has validation errors.", artifact.Id);
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotPublishOverValidationErrors, expectedError);
        }

        #endregion Subartifact Properties tests

        #endregion Negative tests

        #endregion 409 Conflict

        #region Custom data tests

        private const int CU_NUMBER_PROPERTY_ID = 120;
        private const int CU_DATE_PROPERTY_ID = 119;

        [Explicit(IgnoreReasons.ProductBug)]    // Bug: 5133
        [Category(Categories.CustomData)]
        [TestCase("value\":10.0", "value\":\"A\"", CU_NUMBER_PROPERTY_ID)]   // Insert String into Numeric field.
        [TestCase("value\":\"20", "value\":\"A", CU_DATE_PROPERTY_ID)]       // Insert String into Date field.
        [TestRail(164561)]
        [Description("Try to update an artifact properties with a improper value types. Verify 200 OK Request is returned.")]
        public void UpdateArtifact_WrongTypeInProperty_CanGetArtifact(string toChange, string changeTo, int propertyTypeId)
        {
            // Setup:
            var projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, projectCustomData, ItemTypePredefined.Actor);
            artifact.Lock(_user);

            string requestBody = JsonConvert.SerializeObject(artifact.Artifact);

            string modifiedRequestBody = requestBody.Replace(toChange, changeTo);
            Assert.AreNotEqual(requestBody, modifiedRequestBody, "Check that RequestBody was updated.");

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                ArtifactStoreHelper.UpdateInvalidArtifact(Helper.ArtifactStore.Address, modifiedRequestBody, artifact.Id, _user);
            }, "'PATCH {0}' should return 200 OK even if the value is set to wrong type!", UPDATE_ARTIFACT_ID_PATH);

            // Verify:
            var artifactDetailsAfter = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);

            Assert.NotNull(artifactDetailsAfter.LastSaveInvalid, "LastSaveInvalid should not be null for artifacts with invalid properties!");
            Assert.IsTrue(artifactDetailsAfter.LastSaveInvalid.Value, "LastSaveInvalid should be true for artifacts with invalid properties!");

            var customProperty = GetCustomPropertyByPropertyTypeId(artifactDetailsAfter, "CustomPropertyValues", propertyTypeId);

            Assert.IsNull(customProperty.CustomPropertyValue, "Value of this custom property with Id {0} has to be null", propertyTypeId);

            // Set LastSaveInvalid equal to original so that doesn't cause the comparison to fail.
            artifactDetailsAfter.LastSaveInvalid = artifact.LastSaveInvalid;
            ArtifactStoreHelper.AssertArtifactsEqual(artifact, artifactDetailsAfter);
        }

        private const int NUMBER_OUT_OF_RANGE = 999;

        [Category(Categories.CustomData)]
        [TestRail(164595)]
        [TestCase(NUMBER_OUT_OF_RANGE, CU_NUMBER_PROPERTY_ID)]   //Insert value into Numeric field which is out of range    
        [Description("Try to update an artifact properties with a number value that is out of its permitted range. Verify 200 OK Request is returned.")]
        public void UpdateArtifact_NumberPropertyOutOfRange_200OK(int outOfRangeNumber, int propertyTypeId)
        {
            // Setup:
            string stringToReplace = "value\":10.0";

            var projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, projectCustomData, ItemTypePredefined.Actor);
            artifact.Lock(_user);

            string requestBody = JsonConvert.SerializeObject(artifact.Artifact);
            string changedValue = "value\":" + outOfRangeNumber;

            requestBody = requestBody.Replace(stringToReplace, changedValue);

            // Execute:
            Assert.DoesNotThrow(() => ArtifactStoreHelper.UpdateInvalidArtifact(Helper.ArtifactStore.Address, requestBody, artifact.Id, _user),
                "'PATCH {0}' should return 200 OK if properties are out of range!", UPDATE_ARTIFACT_ID_PATH);

            // Verify:
            var artifactDetailsAfter = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);

            Assert.NotNull(artifactDetailsAfter.LastSaveInvalid, "LastSaveInvalid should not be null for artifacts with invalid properties!");
            Assert.IsTrue(artifactDetailsAfter.LastSaveInvalid.Value, "LastSaveInvalid should be true for artifacts with invalid properties!");

            var customPropertyAfter = GetCustomPropertyByPropertyTypeId(artifactDetailsAfter, "CustomPropertyValues", propertyTypeId);

            Assert.AreEqual(outOfRangeNumber, customPropertyAfter.CustomPropertyValue,
                    "Value of this custom property with id {0} should be {1} but was {2}!", propertyTypeId, outOfRangeNumber, customPropertyAfter.CustomPropertyValue);
        }

        [Category(Categories.CustomData)]
        [TestRail(190895)]
        [TestCase("value\":10.0", 15, CU_NUMBER_PROPERTY_ID)]   // Insert value in range into Numeric field.
        [Description("Update a custom property of an artifact with a new in-range value.  Verify 200 OK Request is returned and the new value is saved.")]
        public void UpdateArtifact_ChangeCustomPropertyValue_NewValueIsUpdated(string textToReplace, int newNumberValue, int propertyTypeId)
        {
            // TODO: Make this test work with any data type, not just int.
            // Setup:
            var projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, projectCustomData, ItemTypePredefined.Actor);
            artifact.Lock(_user);

            string requestBody = JsonConvert.SerializeObject(artifact.Artifact);
            string changedValue = "value\":" + newNumberValue;

            requestBody = requestBody.Replace(textToReplace, changedValue);

            // Execute:
            Assert.DoesNotThrow(() => ArtifactStoreHelper.UpdateInvalidArtifact(Helper.ArtifactStore.Address, requestBody, artifact.Id, _user),
                "'PATCH {0}' should return 200 OK if properties are out of range!", UPDATE_ARTIFACT_ID_PATH);

            // Verify:
            var artifactDetailsAfter = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);

            var customPropertyAfter = GetCustomPropertyByPropertyTypeId(artifactDetailsAfter, "CustomPropertyValues", propertyTypeId);

            Assert.AreEqual(newNumberValue, customPropertyAfter.CustomPropertyValue, "Value of this custom property with id {0} should be {1} but was {2}!",
                    propertyTypeId, newNumberValue, customPropertyAfter.CustomPropertyValue);
        }

        [Category(Categories.CustomData)]
        [TestRail(190817)]
        [TestCase(CU_DATE_PROPERTY_ID)]     //Insert value into Date field which is out of range
        [Description("Try to update an artifact date property with a value that out of its permitted range. Verify 200 OK Request is returned.")]
        public void UpdateArtifact_DatePropertyOutOfRange_200OK(int propertyTypeId)
        {
            // Setup:
            var projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, projectCustomData, ItemTypePredefined.Actor);
            artifact.Lock(_user);

            int thisYear = DateTime.Now.Year;

            string toChange = "value\":\"2016";

            string requestBody = JsonConvert.SerializeObject(artifact.Artifact);

            requestBody = requestBody.Replace(toChange, "value\":\"" + thisYear);

            // Execute:
            Assert.DoesNotThrow(() => ArtifactStoreHelper.UpdateInvalidArtifact(Helper.ArtifactStore.Address, requestBody, artifact.Id, _user),
                "'PATCH {0}' should return 200 OK if properties are out of range!", UPDATE_ARTIFACT_ID_PATH);

            // Verify:
            var artifactDetailsAfter = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);

            Assert.NotNull(artifactDetailsAfter.LastSaveInvalid, "LastSaveInvalid should not be null for artifacts with invalid properties!");
            Assert.IsTrue(artifactDetailsAfter.LastSaveInvalid.Value, "LastSaveInvalid should be true for artifacts with invalid properties!");

            var customPropertyAfter = GetCustomPropertyByPropertyTypeId(artifactDetailsAfter, "CustomPropertyValues", propertyTypeId);

            var newDate = (DateTime)customPropertyAfter.CustomPropertyValue;

            Assert.AreEqual(thisYear, newDate.Year,
                    "Value of year in this custom property with id {0} should be {1} but was {2}!", propertyTypeId, thisYear, newDate.Year);
        }

        private const string ChoiceValueIncorrectFormat = "The value for the property CU-Choice Required with Single Choice is invalid.";
        private const string UserValueIncorrectFormat = "The value for the property CU-User Required is invalid.";

        [Category(Categories.CustomData)]
        [TestCase("validValues\":[{\"id\":27", "validValues\":[{\"id\":0", ChoiceValueIncorrectFormat)]     // Insert non-existant choice.
        [TestCase("usersGroups\":[{\"id\":1", "usersGroups\":[{\"id\":0", UserValueIncorrectFormat)]        // Insert non-existant User ID.
        [TestRail(190804)]
        [Description("Try to update an artifact properties with a improper value types. Verify 400 Bad Request is returned.")]
        public void UpdateArtifact_NonExistingValueInProperty_400BadRequest(string toChange, string changeTo, string expectedError)
        {
            // Setup:
            var projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, projectCustomData, ItemTypePredefined.Actor);
            artifact.Lock(_user);

            string requestBody = JsonConvert.SerializeObject(artifact.Artifact);

            string modifiedRequestBody = requestBody.Replace(toChange, changeTo);
            Assert.AreNotEqual(requestBody, modifiedRequestBody, "Check that RequestBody was updated.");

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
                ArtifactStoreHelper.UpdateInvalidArtifact(Helper.ArtifactStore.Address, modifiedRequestBody, artifact.Id, _user),
                "'PATCH {0}' should return 400 Bad Request if the value is set to wrong type!", UPDATE_ARTIFACT_ID_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ValidationFailed, expectedError);
        }

        [TestCase]
        [Category(Categories.CustomData)]
        [TestRail(164624)]
        [Description("Try to update an artifact which is not locked by current user. Verify 409 Conflict is returned.")]
        public void UpdateArtifact_NotLockedByUser_409Conflict()
        {
            // Setup:
            var projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, projectCustomData, ItemTypePredefined.Actor);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => UpdateArtifact(_user, artifact),
                "'PATCH {0}' should return 409 Conflict if the user didn't lock on the artifact first", UPDATE_ARTIFACT_ID_PATH);

            // Verify:
            const string expectedError = "The artifact is not locked.";
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.UnexpectedLockException, expectedError);
        }

        #endregion Custom Data

        #region Private functions

        /// <summary>
        /// Updates the specified artifact and returns the result.
        /// </summary>
        /// <param name="artifact">The artifact to update.</param>
        /// <param name="user">The user updating the artifact.</param>
        /// <param name="updateArtifact">(optional) The minimal artifact changeset to update.  By default the whole artifact is passed to UpdateArtifact.</param>
        /// <returns>The result returned by UpdateArtifact.</returns>
        private INovaArtifactDetails UpdateArtifact(IUser user, ArtifactWrapper artifact, INovaArtifactDetails updateArtifact = null)
        {
            updateArtifact = updateArtifact ?? artifact.Artifact;
            INovaArtifactDetails updateResult = Helper.ArtifactStore.UpdateArtifact(user, updateArtifact);

            artifact.UpdateArtifactState(ArtifactWrapper.ArtifactOperation.Update);

            return updateResult;
        }

        /// <summary>
        /// Update an artifact custom property and save.  The artifact should be locked before calling this function.
        /// </summary>
        /// <typeparam name="T">The property value type.</typeparam>
        /// <param name="user">The user updating the artifact.</param>
        /// <param name="artifact">The artifact to update.</param>
        /// <param name="project">The project where the artifact exists.</param>
        /// <param name="propertyType">The primitive property type of the property.</param>
        /// <param name="propertyName">The name of the artifact property to be updated.</param>
        /// <param name="propertyValue">The new value for the subartifact property.</param>
        /// <returns>The updated property.</returns>
        private CustomProperty UpdateArtifactCustomProperty<T>(IUser user, ArtifactWrapper artifact, IProject project,
            PropertyPrimitiveType propertyType, string propertyName, T propertyValue)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(project, nameof(project));

            // Set custom property in artifact.
            var property = ArtifactStoreHelper.SetArtifactCustomProperty(artifact, project, propertyType, propertyName, propertyValue);
            var artifactDetailsChangeset = TestHelper.CreateArtifactChangeSet(artifact, customProperty: property);

            UpdateArtifact(user, artifact, artifactDetailsChangeset);

            return property;
        }

        /// <summary>
        /// Update a subartifact custom property and save.  The artifact should be locked before calling this function.
        /// </summary>
        /// <typeparam name="T">The property value type.</typeparam>
        /// <param name="user">The user updating the subartifact.</param>
        /// <param name="artifact">The artifact to update.</param>
        /// <param name="subArtifact">The subartifact to update.</param>
        /// <param name="project">The project where the artifact exists.</param>
        /// <param name="propertyType">The primitive type of the property.</param>
        /// <param name="propertyName">The name of the subartifact property to be updated.</param>
        /// <param name="propertyValue">The new value for the subartifact property.</param>
        /// <returns>The updated property.</returns>
        public CustomProperty UpdateSubArtifactCustomProperty<T>(IUser user, ArtifactWrapper artifact, NovaSubArtifact subArtifact, IProject project,
            PropertyPrimitiveType propertyType, string propertyName, T propertyValue)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            ThrowIf.ArgumentNull(subArtifact, nameof(subArtifact));
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(project, nameof(project));

            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(user, artifact.Id);

            // Set custom property in subartifact.
            var property = ArtifactStoreHelper.SetSubArtifactCustomProperty(subArtifact, project,
                propertyType, propertyName, propertyValue);

            var subArtifactChangeSet = TestHelper.CreateSubArtifactChangeSet(subArtifact, customProperty: property);
            var artifactDetailsChangeset = TestHelper.CreateArtifactChangeSet(artifactDetails, subArtifact: subArtifactChangeSet);

            UpdateArtifact(user, artifact, artifactDetailsChangeset);

            return property;
        }

        /// <summary>
        /// Verifies that the properties returned by UpdateArtifact are correct.
        /// NOTE: Only Id, LastSavedOn, LastSavedInvalid and Version are returned by UpdateArtifact.
        /// </summary>
        /// <param name="artifact">The artifact before it was updated (but with the changed property).</param>
        /// <param name="updateResult">The result from the UpdateArtifact call.</param>
        private static void VerifyUpdateResult(INovaArtifactDetails artifact, INovaArtifactDetails updateResult)
        {
            Assert.AreEqual(artifact.Id, updateResult.Id, "The Id property returned from the UpdateArtifact call is wrong!");
            Assert.NotNull(artifact.LastSavedOn, "LastSavedOn should not be null for artifacts with valid properties!");
            Assert.Greater(updateResult.LastSavedOn, artifact.LastSavedOn, "LastSavedOn should be greater after calling UpdateArtifact!");
            Assert.NotNull(artifact.LastSaveInvalid, "LastSaveInvalid should not be null for artifacts with valid properties!");
            Assert.IsFalse(artifact.LastSaveInvalid.Value, "LastSaveInvalid should be false for artifacts with valid properties!");
            Assert.AreEqual(artifact.Version, updateResult.Version, "The Version property returned from the UpdateArtifact call is wrong!");
        }

        /// <summary>
        /// Verifies that the artifact properties are identical before and after calling UpdateArtifact.
        /// </summary>
        /// <param name="artifact">The artifact before it was updated (but with the changed property).</param>
        /// <param name="user">The user that updated the artifact.</param>
        private void VerifyArtifactAfterUpdate(INovaArtifactDetails artifact, IUser user)
        {
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(user, artifact.Id);

            if (artifactDetails.Description != null)
            {
                artifactDetails.Description = StringUtilities.ConvertHtmlToText(artifactDetails.Description);
            }

            ArtifactStoreHelper.AssertArtifactsEqual(artifact, artifactDetails);
        }
        
        // TODO: UpdateCustomProperty can be used for the tests that use this method after update on UpdateCustomProperty
        /// <summary>
        /// Create the changeset for the target artifact.
        /// </summary>
        /// <param name="user">The user updating the artifact.</param>
        /// <param name="project">The project with all the avaliable Nova artifact types.</param>
        /// <param name="artifact">The artifact that the target sub artifact resides.</param>
        /// <param name="artifactCustomPropertyName">custom property name for the artifact to update.</param>
        /// <param name="artifactCustomPropertyValue">custom property value for the artifact to update.</param>
        /// <returns>INovaArtifactDetails that contains the change for the artifact.</returns>
        private NovaArtifactDetails CreateArtifactChangeSet(
            IUser user,
            IProject project,
            ArtifactWrapper artifact,
            string artifactCustomPropertyName,
            object artifactCustomPropertyValue)
        {
            var artifactDetailsChangeSet = Helper.ArtifactStore.GetArtifactDetails(user, artifact.Id);
            var customPropertyValueToUpdate = artifactDetailsChangeSet.CustomPropertyValues.Find(p => p.Name.Equals(artifactCustomPropertyName));

            artifactDetailsChangeSet.CustomPropertyValues.Clear();
            artifactDetailsChangeSet.SpecificPropertyValues.Clear();

            switch (artifactCustomPropertyName)
            {
                case CustomPropertyName.TextRequiredRTMultiHasDefault:
                case CustomPropertyName.NumberRequiredValidatedDecPlacesMinMaxHasDefault:
                case CustomPropertyName.DateRequiredValidatedMinMaxHasDefault:
                    {
                        customPropertyValueToUpdate.CustomPropertyValue = artifactCustomPropertyValue;
                        artifactDetailsChangeSet.CustomPropertyValues.Add(customPropertyValueToUpdate);
                        break;
                    }
                case CustomPropertyName.ChoiceRequiredAllowMultipleDefaultValue:
                    {
                        var choicePropertyValidValues = project.NovaPropertyTypes.Find(pt => pt.Name.Equals(artifactCustomPropertyName)).ValidValues;
                        var collectedChoicePropertyValueList = new List<NovaPropertyType.ValidValue>();

                        foreach (string choiceValue in (List<string>)artifactCustomPropertyValue)
                        {
                            collectedChoicePropertyValueList.Add(choicePropertyValidValues.Find(vv => vv.Value.Equals(choiceValue)));
                        }

                        customPropertyValueToUpdate.CustomPropertyValue = new ArtifactStoreHelper.ChoiceValues { ValidValues = collectedChoicePropertyValueList };
                        artifactDetailsChangeSet.CustomPropertyValues.Add(customPropertyValueToUpdate);
                        break;
                    }
                case CustomPropertyName.UserRequiredHasDefaultUser:
                    {
                        var userData = (IUser)artifactCustomPropertyValue;
                        var newIdentification = new Identification { DisplayName = userData.DisplayName, Id = userData.Id };
                        var newUserPropertyValue = new List<Identification> { newIdentification };

                        customPropertyValueToUpdate.CustomPropertyValue = new ArtifactStoreHelper.UserGroupValues { UsersGroups = newUserPropertyValue };
                        artifactDetailsChangeSet.CustomPropertyValues.Add(customPropertyValueToUpdate);
                        break;
                    }
            }

            return artifactDetailsChangeSet;
        }

        // TODO: UpdateCustomProperty can be used for the tests that use this method after update on UpdateCustomProperty
        /// <summary>
        /// Create the changeset for the target sub artifact.
        /// </summary>
        /// <param name="user">The user updating the sub artifact.</param>
        /// <param name="project">The project with all the avaliable Nova artifact types.</param>
        /// <param name="artifact">The artifact that the target sub artifact resides.</param>
        /// <param name="subArtifactDisplayName">display name for the sub artifact to update.</param>
        /// <param name="subArtifactCustomPropertyName">custom property name for the sub artifact to update.</param>
        /// <param name="subArtifactCustomPropertyValue">custom property value for the sub artifact to update.</param>
        /// <returns>NovaSubArtifact that contains the change for the sub artifact.</returns>
        private NovaSubArtifact CreateSubArtifactChangeSet(
            IUser user,
            IProject project,
            ArtifactWrapper artifact,
            string subArtifactDisplayName,
            string subArtifactCustomPropertyName,
            object subArtifactCustomPropertyValue)
        {
            var subArtifact = Helper.ArtifactStore.GetSubartifacts(user, artifact.Id).Find(sa => sa.DisplayName.Equals(subArtifactDisplayName));
            var subArtifactChangeSet = Helper.ArtifactStore.GetSubartifact(user, artifact.Id, subArtifact.Id);
            var customPropertyValueToUpdate = subArtifactChangeSet.CustomPropertyValues.Find(p => p.Name.Equals(subArtifactCustomPropertyName));

            subArtifactChangeSet.CustomPropertyValues.Clear();
            subArtifactChangeSet.SpecificPropertyValues.Clear();

            switch (subArtifactCustomPropertyName)
            {
                case CustomPropertyName.TextRequiredRTMultiHasDefault:
                case CustomPropertyName.NumberRequiredValidatedDecPlacesMinMaxHasDefault:
                case CustomPropertyName.DateRequiredValidatedMinMaxHasDefault:
                    {
                        customPropertyValueToUpdate.CustomPropertyValue = subArtifactCustomPropertyValue;
                        subArtifactChangeSet.CustomPropertyValues.Add(customPropertyValueToUpdate);
                        break;
                    }
                case CustomPropertyName.ChoiceRequiredAllowMultipleDefaultValue:
                    {
                        var choicePropertyValidValues = project.NovaPropertyTypes.Find(pt => pt.Name.Equals(subArtifactCustomPropertyName)).ValidValues;
                        var collectedChoicePropertyValueList = new List<NovaPropertyType.ValidValue>();

                        foreach (string choiceValue in (List<string>)subArtifactCustomPropertyValue)
                        {
                            collectedChoicePropertyValueList.Add(choicePropertyValidValues.Find(vv => vv.Value.Equals(choiceValue)));
                        }

                        customPropertyValueToUpdate.CustomPropertyValue = new ArtifactStoreHelper.ChoiceValues { ValidValues = collectedChoicePropertyValueList };
                        subArtifactChangeSet.CustomPropertyValues.Add(customPropertyValueToUpdate);
                        break;
                    }
                case CustomPropertyName.UserRequiredHasDefaultUser:
                    {
                        var userData = (IUser)subArtifactCustomPropertyValue;
                        var newIdentification = new Identification { DisplayName = userData.DisplayName, Id = userData.Id };
                        var newUserPropertyValue = new List<Identification> { newIdentification };

                        customPropertyValueToUpdate.CustomPropertyValue = new ArtifactStoreHelper.UserGroupValues { UsersGroups = newUserPropertyValue };
                        subArtifactChangeSet.CustomPropertyValues.Add(customPropertyValueToUpdate);
                        break;
                    }
            }

            return subArtifactChangeSet;
        }

        /// <summary>
        /// Gets sub-property using property name and propertytypeId
        /// </summary>
        /// <param name="objectToSearchCustomProperty">Object in which to look sub-property</param>
        /// <param name="propertyName">Property name of property that has sub-properties</param>
        /// <param name="propertyTypeId">Id of specific property to look for</param>
        /// <returns>Custom property. Null if not found</returns>
        private static CustomProperty GetCustomPropertyByPropertyTypeId(object objectToSearchCustomProperty, string propertyName, int propertyTypeId)
        {
            Assert.IsNotNull(objectToSearchCustomProperty, "Object send to this function cannot be null!");
            var properties = (List<CustomProperty>)objectToSearchCustomProperty.GetType().GetProperty(propertyName).GetValue(objectToSearchCustomProperty);

            return properties.FirstOrDefault(property => property.PropertyTypeId == propertyTypeId);
        }

        #endregion Private functions
    }
}
