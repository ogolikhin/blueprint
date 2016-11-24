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
using Utilities.Facades;
using Utilities.Factories;

namespace ArtifactStoreTests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class SaveArtifactTests : TestBase
    {
        private IUser _user = null;
        private IProject _project = null;
        private List<IProject> _allProjects = null;

        #region Setup and Cleanup

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            _allProjects = ProjectFactory.GetAllProjects(_user);
            _project = _allProjects.First();
            _project.GetAllArtifactTypes(ProjectFactory.Address, _user);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region 200 OK tests

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForOpenApiRestMethods))]
        [TestRail(156656)]
        [Description("Create & publish an artifact.  Update the artifact.  Get the artifact.  Verify the artifact returned has the same properties as the artifact we updated.")]
        public void UpdateArtifact_PublishedArtifact_CanGetArtifact(BaseArtifactType artifactType)
        {
            // Setup:
            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);

            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            artifact.Lock(author);

            NovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);

            // Execute:
            UpdateArtifact_CanGetArtifact(artifact, artifactType, "Description", "NewDescription_" + RandomGenerator.RandomAlphaNumeric(5), author);

            // Verify:
            NovaArtifactDetails artifactDetailsAfter = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);

            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, artifactDetailsAfter);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForOpenApiRestMethods))]
        [TestRail(156917)]
        [Description("Create & save an artifact.  Update the artifact.  Get the artifact.  Verify the artifact returned has the same properties as the artifact we updated.")]
        public void UpdateArtifact_UnpublishedArtifact_CanGetArtifact(BaseArtifactType artifactType)
        {
            // Setup:
            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);

            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, author, artifactType);

            string description = StringUtilities.WrapInHTML("NewDescription_" + RandomGenerator.RandomAlphaNumeric(5));

            // Execute:
            UpdateArtifact_CanGetArtifact(artifact, artifactType, "Description", description, author);

            // Verify:
            NovaArtifactDetails artifactDetailsAfter = Helper.ArtifactStore.GetArtifactDetails(author, artifact.Id);

            Assert.IsNotNull(artifactDetailsAfter.Description);
            Assert.AreEqual(description, artifactDetailsAfter.Description);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForOpenApiRestMethods))]
        [TestRail(164531)]
        [Description("Create & publish an artifact. Update the artifact property 'Name' with Empty space. Get the artifact. Verify the artifact returned has the same properties as the artifact we updated.")]
        public void UpdateArtifact_PublishedArtifact_SetEmptyNameProperty_CanGetArtifact(BaseArtifactType artifactType)
        {
            // Setup:
            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);

            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            artifact.Lock(author);

            NovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);

            // Execute:
            UpdateArtifact_CanGetArtifact(artifact, artifactType, "Name", string.Empty, author);

            // Verify:
            NovaArtifactDetails artifactDetailsAfter = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);

            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, artifactDetailsAfter);
        }


        [TestCase]  // It is working as designed for now. There is no check on user's permissions after artifact was locked
        [TestRail(190881)]
        [Description("Create & publish an artifact.  Lock artifact with an author, change permissions to viewer and update the artifact.  Verify 403 Forbidden is returned.")]
        public void UpdateArtifact_UserLosesPermissionsToArtifact_CanGetArtifact()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);

            IUser user = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);

            artifact.Lock(user);

            Helper.AssignProjectRolePermissionsToUser(user, TestHelper.ProjectRole.Viewer, _project);

            string newName = "NewName_" + RandomGenerator.RandomAlphaNumeric(5);

            // Execute:
            UpdateArtifact_CanGetArtifact(artifact, BaseArtifactType.Process, "Name", newName, user);

            // Verify:
            NovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetArtifactDetails(user, artifact.Id);

            Assert.AreEqual(newName, artifactDetails.Name);
        }

        #region Artifact Properties tests

        [Category(Categories.CustomData)]
        [TestCase(ItemTypePredefined.Process, "Std-Text-Required-RT-Multi-HasDefault")]
        [TestCase(ItemTypePredefined.PrimitiveFolder, "Std-Text-Required-RT-Multi-HasDefault")]
        [TestCase(ItemTypePredefined.Actor, "Std-Text-Required-RT-Multi-HasDefault")]
        [TestCase(ItemTypePredefined.Document, "Std-Text-Required-RT-Multi-HasDefault")]
        [TestCase(ItemTypePredefined.TextualRequirement, "Std-Text-Required-RT-Multi-HasDefault")]
        [TestRail(191102)]
        [Description("Create & publish an artifact.  Update a text property, save and publish.  Verify the artifact returned the text property updated.")]
        public void UpdateArtifact_ChangeTextPropertySaveAndPublish_VerifyPropertyChanged(ItemTypePredefined itemType, string propertyName)
        {
            // Setup:
            IProject projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);
            projectCustomData.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, projectCustomData);

            IArtifact artifact = Helper.CreateWrapAndPublishNovaArtifactForStandardArtifactType(projectCustomData, author, itemType);

            NovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, artifact.Id);

            CustomProperty property = artifactDetails.CustomPropertyValues.Find(p => p.Name == propertyName);

            // Change custom property text value
            property.CustomPropertyValue = StringUtilities.WrapInHTML(WebUtility.HtmlEncode(
                RandomGenerator.RandomAlphaNumericUpperAndLowerCaseAndSpecialCharactersWithSpaces()));

            // Execute:
            artifact.Lock(author);
            Helper.ArtifactStore.UpdateArtifact(author, projectCustomData, artifactDetails);
            Helper.ArtifactStore.PublishArtifact(artifact, author);

            // Verify:
            NovaArtifactDetails artifactDetailsAfter = Helper.ArtifactStore.GetArtifactDetails(author, artifact.Id);

            CustomProperty returnedProperty = artifactDetailsAfter.CustomPropertyValues.Find(p => p.Name == propertyName);

            ArtifactStoreHelper.AssertCustomPropertiesAreEqual(property, returnedProperty);
        }

        [Category(Categories.CustomData)]
        [TestCase(ItemTypePredefined.Process, "Std-Number-Required-Validated-DecPlaces-Min-Max-HasDefault", 5)]
        [TestCase(ItemTypePredefined.PrimitiveFolder, "Std-Number-Required-Validated-DecPlaces-Min-Max-HasDefault", -5)]
        [TestCase(ItemTypePredefined.Actor, "Std-Number-Required-Validated-DecPlaces-Min-Max-HasDefault", 4.2)]
        [TestCase(ItemTypePredefined.Document, "Std-Number-Required-Validated-DecPlaces-Min-Max-HasDefault", -3)]
        [TestCase(ItemTypePredefined.TextualRequirement, "Std-Number-Required-Validated-DecPlaces-Min-Max-HasDefault", 0)]
        [TestRail(191103)]
        [Description("Create & publish anrtifact.  Update a number property, save and publish.  Verify the artifact returned the number property updated.")]
        public void UpdateArtifact_ChangeNumberPropertySaveAndPublish_VerifyPropertyChanged(ItemTypePredefined itemType, string propertyName, double newNumber)
        {
            // Setup:
            IProject projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);
            projectCustomData.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, projectCustomData);

            IArtifact artifact = Helper.CreateWrapAndPublishNovaArtifactForStandardArtifactType(projectCustomData, author, itemType);

            NovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, artifact.Id);

            CustomProperty property = artifactDetails.CustomPropertyValues.Find(p => p.Name == propertyName);

            // Change custom property number value
            property.CustomPropertyValue = newNumber;

            // Execute:
            artifact.Lock(author);
            Helper.ArtifactStore.UpdateArtifact(author, projectCustomData, artifactDetails);
            Helper.ArtifactStore.PublishArtifact(artifact, author);

            // Verify:
            NovaArtifactDetails artifactDetailsAfter = Helper.ArtifactStore.GetArtifactDetails(author, artifact.Id);

            CustomProperty returnedProperty = artifactDetailsAfter.CustomPropertyValues.Find(p => p.Name == propertyName);

            ArtifactStoreHelper.AssertCustomPropertiesAreEqual(property, returnedProperty);
        }

        [Category(Categories.CustomData)]
        [TestCase(ItemTypePredefined.Process, "Std-Date-Required-Validated-Min-Max-HasDefault")]
        [TestCase(ItemTypePredefined.PrimitiveFolder, "Std-Date-Required-Validated-Min-Max-HasDefault")]
        [TestCase(ItemTypePredefined.Actor, "Std-Date-Required-Validated-Min-Max-HasDefault")]
        [TestCase(ItemTypePredefined.Document, "Std-Date-Required-Validated-Min-Max-HasDefault")]
        [TestCase(ItemTypePredefined.TextualRequirement, "Std-Date-Required-Validated-Min-Max-HasDefault")]
        [TestRail(191104)]
        [Description("Create & publish an artifact.  Update a date property, save and publish.  Verify the artifact returned the date property updated.")]
        public void UpdateArtifact_ChangeDatePropertySaveAndPublish_VerifyPropertyChanged(ItemTypePredefined itemType, string propertyName)
        {
            // Setup:
            IProject projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);
            projectCustomData.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, projectCustomData);

            IArtifact artifact = Helper.CreateWrapAndPublishNovaArtifactForStandardArtifactType(projectCustomData, author, itemType);

            NovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, artifact.Id);

            CustomProperty property = artifactDetails.CustomPropertyValues.Find(p => p.Name == propertyName);

            // Change custom property date value
            property.CustomPropertyValue = DateTimeUtilities.ConvertDateTimeToSortableDateTime(DateTime.Now);

            // Execute:
            artifact.Lock(author);
            Helper.ArtifactStore.UpdateArtifact(author, projectCustomData, artifactDetails);
            Helper.ArtifactStore.PublishArtifact(artifact, author);

            // Verify:
            NovaArtifactDetails artifactDetailsAfter = Helper.ArtifactStore.GetArtifactDetails(author, artifact.Id);

            CustomProperty returnedProperty = artifactDetailsAfter.CustomPropertyValues.Find(p => p.Name == propertyName);

            ArtifactStoreHelper.AssertCustomPropertiesAreEqual(property, returnedProperty);
        }

        [Category(Categories.CustomData)]
        [TestCase(ItemTypePredefined.Process,         "Std-Choice-Required-AllowMultiple-DefaultValue", "Blue")]
        [TestCase(ItemTypePredefined.PrimitiveFolder, "Std-Choice-Required-AllowMultiple-DefaultValue", "Green")]
        [TestCase(ItemTypePredefined.Actor,           "Std-Choice-Required-AllowMultiple-DefaultValue", "Yellow")]
        [TestCase(ItemTypePredefined.Document, "Std-Choice-Required-AllowMultiple-DefaultValue", "Purple")]
        [TestCase(ItemTypePredefined.TextualRequirement, "Std-Choice-Required-AllowMultiple-DefaultValue", "Orange")]
        [TestRail(191105)]
        [Description("Create & publish an artifact.  Update a choice property, save and publish.  Verify the artifact returned the choice property updated.")]
        public void UpdateArtifact_ChangeChoicePropertySaveAndPublish_VerifyPropertyChanged(ItemTypePredefined itemType, string propertyName, string newChoiceValue)
        {
            // Setup:
            IProject projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);
            projectCustomData.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, projectCustomData);

            IArtifact artifact = Helper.CreateWrapAndPublishNovaArtifactForStandardArtifactType(projectCustomData, author, itemType);

            NovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, artifact.Id);

            CustomProperty property = artifactDetails.CustomPropertyValues.Find(p => p.Name == propertyName);

            var novaPropertyType = projectCustomData.NovaPropertyTypes.Find(pt => pt.Name.EqualsOrdinalIgnoreCase(propertyName));
            var choicePropertyValidValues = novaPropertyType.ValidValues;
            var newPropertyValue = choicePropertyValidValues.Find(vv => vv.Value == newChoiceValue);

            var newChoicePropertyValue = new List<NovaPropertyType.ValidValue> { newPropertyValue };

            // Change custom property choice value
            property.CustomPropertyValue = new ArtifactStoreHelper.ChoiceValues { ValidValues = newChoicePropertyValue };

            // Execute:
            artifact.Lock(author);
            Helper.ArtifactStore.UpdateArtifact(author, projectCustomData, artifactDetails);
            Helper.ArtifactStore.PublishArtifact(artifact, author);

            // Verify:
            NovaArtifactDetails artifactDetailsAfter = Helper.ArtifactStore.GetArtifactDetails(author, artifact.Id);

            CustomProperty returnedProperty = artifactDetailsAfter.CustomPropertyValues.Find(p => p.Name == propertyName);

            ArtifactStoreHelper.AssertCustomPropertiesAreEqual(property, returnedProperty);
        }

        [Category(Categories.CustomData)]
        [TestRail(191106)]
        [TestCase(ItemTypePredefined.Process, "Std-User-Required-HasDefault-User")]
        [TestCase(ItemTypePredefined.PrimitiveFolder, "Std-User-Required-HasDefault-User")]
        [TestCase(ItemTypePredefined.Actor, "Std-User-Required-HasDefault-User")]
        [TestCase(ItemTypePredefined.Document, "Std-User-Required-HasDefault-User")]
        [TestCase(ItemTypePredefined.TextualRequirement, "Std-User-Required-HasDefault-User")]
        [Description("Create & publish an artifact.  Update a user property, save and publish.  Verify the artifact returned the user property updated.")]
        public void UpdateArtifact_ChangeUserPropertySaveAndPublish_VerifyPropertyChanged(ItemTypePredefined itemType, string propertyName)
        {
            // Setup:
            IProject projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);
            projectCustomData.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, projectCustomData);

            IArtifact artifact = Helper.CreateWrapAndPublishNovaArtifactForStandardArtifactType(projectCustomData, author, itemType);

            NovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, artifact.Id);

            CustomProperty property = artifactDetails.CustomPropertyValues.Find(p => p.Name == propertyName);

            var newIdentification = new Identification() {DisplayName = author.DisplayName, Id = author.Id};
            var newUserPropertyValue = new List<Identification> {newIdentification};

            // Change custom property user value
            property.CustomPropertyValue = new ArtifactStoreHelper.UserGroupValues() { UsersGroups = newUserPropertyValue };

            // Execute:
            artifact.Lock(author);
            Helper.ArtifactStore.UpdateArtifact(author, projectCustomData, artifactDetails);
            Helper.ArtifactStore.PublishArtifact(artifact, author);

            // Verify:
            NovaArtifactDetails artifactDetailsAfter = Helper.ArtifactStore.GetArtifactDetails(author, artifact.Id);

            CustomProperty returnedProperty = artifactDetailsAfter.CustomPropertyValues.Find(p => p.Name == propertyName);

            ArtifactStoreHelper.AssertCustomPropertiesAreEqual(property, returnedProperty);
        }

        #endregion Artifact Properties tests

        #region Subartifact Properties tests

        //TODO: Refactor artifact & subartifact properties tests to use changesets as in the example below

        [Category(Categories.CustomData)]
        [TestCase(ItemTypePredefined.Process, "UT", "Std-Text-Required-RT-Multi-HasDefault")]
        [TestRail(191159)]
        [Description("Create & publish an artifact.  Update a text property in a subartifact, save and publish.  " +
                     "Verify the sub artifact returned the text property updated.")]
        public void UpdateSubArtifact_ChangeTextPropertySaveAndPublish_VerifyPropertyChanged(ItemTypePredefined itemType,
            string subArtifactDisplayName,
            string subArtifactCustomPropertyName)
        {
            // Setup:
            IProject projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);
            projectCustomData.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, projectCustomData);

            IArtifact artifact = Helper.CreateWrapAndPublishNovaArtifactForStandardArtifactType(projectCustomData, author, itemType);

            // Change custom property text value
            var subArtifactCustomPropertyValue = StringUtilities.WrapInHTML(WebUtility.HtmlEncode(
                RandomGenerator.RandomAlphaNumericUpperAndLowerCaseAndSpecialCharactersWithSpaces()));
            var subArtifactChangeSet = CreateSubArtifactChangeSet(author, projectCustomData, artifact, subArtifactDisplayName, subArtifactCustomPropertyName, subArtifactCustomPropertyValue);
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, artifact.Id);
            artifactDetails.SubArtifacts = new List<NovaSubArtifact>() { subArtifactChangeSet };
            var requestedCustomProperty = subArtifactChangeSet.CustomPropertyValues.Find(p => p.Name.Equals(subArtifactCustomPropertyName));

            // Execute:
            artifact.Lock(author);
            Helper.ArtifactStore.UpdateArtifact(author, projectCustomData, artifactDetails);
            Helper.ArtifactStore.PublishArtifact(artifact, author);

            // Verify:
            Assert.NotNull(subArtifactChangeSet.Id, "The SubArtifact ID shouldn't be null!");
            NovaSubArtifact subArtifactAfter = Helper.ArtifactStore.GetSubartifact(author, artifact.Id, subArtifactChangeSet.Id.Value);

            CustomProperty returnedCustomProperty = subArtifactAfter.CustomPropertyValues.Find(p => p.Name.Equals(subArtifactCustomPropertyName));

            ArtifactStoreHelper.AssertCustomPropertiesAreEqual(requestedCustomProperty, returnedCustomProperty);
        }

        [Category(Categories.CustomData)]
        [TestCase(ItemTypePredefined.Process, "UT", "Std-Number-Required-Validated-DecPlaces-Min-Max-HasDefault", 5)]
        [TestCase(ItemTypePredefined.Process, "UT", "Std-Number-Required-Validated-DecPlaces-Min-Max-HasDefault", -5)]
        [TestCase(ItemTypePredefined.Process, "UT", "Std-Number-Required-Validated-DecPlaces-Min-Max-HasDefault", 4.2)]
        [TestCase(ItemTypePredefined.Process, "UT", "Std-Number-Required-Validated-DecPlaces-Min-Max-HasDefault", -3)]
        [TestCase(ItemTypePredefined.Process, "UT", "Std-Number-Required-Validated-DecPlaces-Min-Max-HasDefault", 0)]
        [TestRail(191160)]
        [Description("Create & publish an artifact.  Update a number property in a subartifact, save and publish.  " +
                     "Verify the sub artifact returned the number property updated.")]
        public void UpdateSubArtifact_ChangeNumberPropertySaveAndPublish_VerifyPropertyChanged(ItemTypePredefined itemType, 
            string subArtifactDisplayName, 
            string subArtifactCustomPropertyName, 
            double newNumber)
        {
            // Setup:
            IProject projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);
            projectCustomData.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, projectCustomData);

            IArtifact artifact = Helper.CreateWrapAndPublishNovaArtifactForStandardArtifactType(projectCustomData, author, itemType);

            // Change custom property number value
            var subArtifactCustomPropertyValue = newNumber;
            var subArtifactChangeSet = CreateSubArtifactChangeSet(author, projectCustomData, artifact, subArtifactDisplayName, subArtifactCustomPropertyName, subArtifactCustomPropertyValue);
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, artifact.Id);
            artifactDetails.SubArtifacts = new List<NovaSubArtifact>() { subArtifactChangeSet };
            var requestedCustomProperty = subArtifactChangeSet.CustomPropertyValues.Find(p => p.Name.Equals(subArtifactCustomPropertyName));

            // Execute:
            artifact.Lock(author);
            Helper.ArtifactStore.UpdateArtifact(author, projectCustomData, artifactDetails);
            Helper.ArtifactStore.PublishArtifact(artifact, author);

            // Verify:
            Assert.NotNull(subArtifactChangeSet.Id, "The SubArtifact ID shouldn't be null!");
            NovaSubArtifact subArtifactAfter = Helper.ArtifactStore.GetSubartifact(author, artifact.Id, subArtifactChangeSet.Id.Value);

            CustomProperty returnedCustomProperty = subArtifactAfter.CustomPropertyValues.Find(p => p.Name.Equals(subArtifactCustomPropertyName));

            ArtifactStoreHelper.AssertCustomPropertiesAreEqual(requestedCustomProperty, returnedCustomProperty);
        }

        [Category(Categories.CustomData)]
        [TestCase(ItemTypePredefined.Process, "UT", "Std-Date-Required-Validated-Min-Max-HasDefault")]
        [TestRail(191161)]
        [Description("Create & publish an artifact.  Update a date property in a subartifact, save and publish.  " +
                     "Verify the sub artifact returned the date property updated.")]
        public void UpdateSubArtifact_ChangeDatePropertySaveAndPublish_VerifyPropertyChanged(ItemTypePredefined itemType, 
            string subArtifactDisplayName, 
            string subArtifactCustomPropertyName)
        {
            // Setup:
            IProject projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);
            projectCustomData.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, projectCustomData);

            IArtifact artifact = Helper.CreateWrapAndPublishNovaArtifactForStandardArtifactType(projectCustomData, author, itemType);

            // Change custom property date value
            var subArtifactCustomPropertyValue = DateTimeUtilities.ConvertDateTimeToSortableDateTime(DateTime.Now);
            var subArtifactChangeSet = CreateSubArtifactChangeSet(author, projectCustomData, artifact, subArtifactDisplayName, subArtifactCustomPropertyName, subArtifactCustomPropertyValue);
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, artifact.Id);
            artifactDetails.SubArtifacts = new List<NovaSubArtifact>() { subArtifactChangeSet };
            var requestedCustomProperty = subArtifactChangeSet.CustomPropertyValues.Find(p => p.Name.Equals(subArtifactCustomPropertyName));

            // Execute:
            artifact.Lock(author);
            Helper.ArtifactStore.UpdateArtifact(author, projectCustomData, artifactDetails);
            Helper.ArtifactStore.PublishArtifact(artifact, author);

            // Verify:
            Assert.NotNull(subArtifactChangeSet.Id, "The SubArtifact ID shouldn't be null!");
            NovaSubArtifact subArtifactAfter = Helper.ArtifactStore.GetSubartifact(author, artifact.Id, subArtifactChangeSet.Id.Value);

            CustomProperty returnedCustomProperty = subArtifactAfter.CustomPropertyValues.Find(p => p.Name.Equals(subArtifactCustomPropertyName));

            ArtifactStoreHelper.AssertCustomPropertiesAreEqual(requestedCustomProperty, returnedCustomProperty);
        }

        [Category(Categories.CustomData)]
        [TestCase(ItemTypePredefined.Process, "UT", "Std-Choice-Required-AllowMultiple-DefaultValue", "Blue")]
        [TestCase(ItemTypePredefined.Process, "UT", "Std-Choice-Required-AllowMultiple-DefaultValue", "Green")]
        [TestCase(ItemTypePredefined.Process, "UT", "Std-Choice-Required-AllowMultiple-DefaultValue", "Yellow")]
        [TestCase(ItemTypePredefined.Process, "UT", "Std-Choice-Required-AllowMultiple-DefaultValue", "Purple")]
        [TestCase(ItemTypePredefined.Process, "UT", "Std-Choice-Required-AllowMultiple-DefaultValue", "Orange")]
        [TestRail(191162)]
        [Description("Create & publish an artifact.  Update a choice property in a subartifact, save and publish.  " +
                     "Verify the sub artifact returned the choice property updated.")]
        public void UpdateSubArtifact_ChangeChoicePropertySaveAndPublish_VerifyPropertyChanged(ItemTypePredefined itemType, 
            string subArtifactDisplayName,
            string subArtifactCustomPropertyName, 
            string newChoiceValue)
        {
            // Setup:
            IProject projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);
            projectCustomData.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, projectCustomData);

            IArtifact artifact = Helper.CreateWrapAndPublishNovaArtifactForStandardArtifactType(projectCustomData, author, itemType);

            // Change custom property choice value
            var subArtifactCustomPropertyValue = new List<string>() { newChoiceValue };
            var subArtifactChangeSet = CreateSubArtifactChangeSet(author, projectCustomData, artifact, subArtifactDisplayName, subArtifactCustomPropertyName, subArtifactCustomPropertyValue);
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, artifact.Id);
            artifactDetails.SubArtifacts = new List<NovaSubArtifact>() { subArtifactChangeSet };
            var requestedCustomProperty = subArtifactChangeSet.CustomPropertyValues.Find(p => p.Name.Equals(subArtifactCustomPropertyName));

            // Execute:
            artifact.Lock(author);
            Helper.ArtifactStore.UpdateArtifact(author, projectCustomData, artifactDetails);
            Helper.ArtifactStore.PublishArtifact(artifact, author);

            // Verify:
            Assert.NotNull(subArtifactChangeSet.Id, "The SubArtifact ID shouldn't be null!");
            NovaSubArtifact subArtifactAfter = Helper.ArtifactStore.GetSubartifact(author, artifact.Id, subArtifactChangeSet.Id.Value);

            CustomProperty returnedCustomProperty = subArtifactAfter.CustomPropertyValues.Find(p => p.Name.Equals(subArtifactCustomPropertyName));

            ArtifactStoreHelper.AssertCustomPropertiesAreEqual(requestedCustomProperty, returnedCustomProperty);
        }

        [Category(Categories.CustomData)]
        [TestRail(191163)]
        [TestCase(ItemTypePredefined.Process, "UT", "Std-User-Required-HasDefault-User")]
        [Description("Create & publish an artifact.  Update a user property in a subartifact, save and publish.  " +
                     "Verify the sub artifact returned the user property updated.")]
        public void UpdateSubArtifact_ChangeUserPropertySaveAndPublish_VerifyPropertyChanged(ItemTypePredefined itemType, 
            string subArtifactDisplayName,
            string subArtifactCustomPropertyName)
        {
            // Setup:
            IProject projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);
            projectCustomData.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, projectCustomData);

            IArtifact artifact = Helper.CreateWrapAndPublishNovaArtifactForStandardArtifactType(projectCustomData, author, itemType);

            // Change custom property user value
            var subArtifactCustomPropertyValue = author;
            var subArtifactChangeSet = CreateSubArtifactChangeSet(author, projectCustomData, artifact, subArtifactDisplayName, subArtifactCustomPropertyName, subArtifactCustomPropertyValue);
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, artifact.Id);
            artifactDetails.SubArtifacts = new List<NovaSubArtifact>() { subArtifactChangeSet };
            var requestedCustomProperty = subArtifactChangeSet.CustomPropertyValues.Find(p => p.Name.Equals(subArtifactCustomPropertyName));

            // Execute:
            artifact.Lock(author);
            Helper.ArtifactStore.UpdateArtifact(author, projectCustomData, artifactDetails);
            Helper.ArtifactStore.PublishArtifact(artifact, author);

            // Verify:
            Assert.NotNull(subArtifactChangeSet.Id, "The SubArtifact ID shouldn't be null!");
            NovaSubArtifact subArtifactAfter = Helper.ArtifactStore.GetSubartifact(author, artifact.Id, subArtifactChangeSet.Id.Value);

            CustomProperty returnedCustomProperty = subArtifactAfter.CustomPropertyValues.Find(p => p.Name.Equals(subArtifactCustomPropertyName));

            ArtifactStoreHelper.AssertCustomPropertiesAreEqual(requestedCustomProperty, returnedCustomProperty);
        }

        #endregion Subartifact Properties tests

        #endregion 200 OK tests

        #region Negative tests

        [TestCase]
        [TestRail(156662)]
        [Description("Try to update an artifact, but send an empty request body.  Verify 400 Bad Request is returned.")]
        public void UpdateArtifact_EmptyBody_400BadRequest()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Process);
            string requestBody = string.Empty;

            // Execute & Verify:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                ArtifactStoreHelper.UpdateInvalidArtifact(Helper.ArtifactStore.Address, requestBody, artifact.Id, _user);
            }, "'PATCH {0}' should return 400 Bad Request if an empty body is sent!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            const string expectedMessage = "Artifact not provided.";
            AssertRestResponseMessageIsCorrect(ex.RestResponse, expectedMessage);
        }

        [TestCase]
        [TestRail(156663)]
        [Description("Try to update an artifact, but send a corrupt JSON request body.  Verify 400 Bad Request is returned.")]
        public void UpdateArtifact_CorruptBody_400BadRequest()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Process);
            string requestBody = JsonConvert.SerializeObject(artifact);

            // Remove first 5 characters to corrupt the JSON string, thereby corrupting the JSON structure.
            requestBody = requestBody.Remove(0, 5);

            // Execute & Verify:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                ArtifactStoreHelper.UpdateInvalidArtifact(Helper.ArtifactStore.Address, requestBody, artifact.Id, _user);
            }, "'PATCH {0}' should return 400 Bad Request if a corrupt JSON body is sent!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            const string expectedMessage = "Artifact not provided.";
            AssertRestResponseMessageIsCorrect(ex.RestResponse, expectedMessage);
        }

        [TestCase]
        [TestRail(157057)]
        [Description("Try to update an artifact, but send a JSON request body without an 'Id' property.  Verify 400 Bad Request is returned.")]
        public void UpdateArtifact_MissingIdInJsonBody_400BadRequest()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Process);
            string requestBody = JsonConvert.SerializeObject(artifact);

            // Remove the 'Id' property by renaming it.
            requestBody = requestBody.Replace("\"Id\"", "\"NotId\"");

            // Execute & Verify:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                ArtifactStoreHelper.UpdateInvalidArtifact(Helper.ArtifactStore.Address, requestBody, artifact.Id, _user);
            }, "'PATCH {0}' should return 400 Bad Request if the 'Id' property is missing in the JSON body!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            const string expectedMessage = "Artifact not provided.";
            AssertRestResponseMessageIsCorrect(ex.RestResponse, expectedMessage);
        }

        [TestCase]
        [TestRail(156664)]
        [Description("Try to update an artifact, but send a different Artifact ID in the URL vs request body.  Verify 400 Bad Request is returned.")]
        public void UpdateArtifact_DifferentArtifactIdsInUrlAndBody_400BadRequest()
        {
            // Setup:
            IArtifact artifact1 = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Process);
            IArtifact artifact2 = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Process);

            string requestBody = JsonConvert.SerializeObject(artifact1);
            int wrongArtifactId = artifact2.Id;

            // Execute & Verify:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                ArtifactStoreHelper.UpdateInvalidArtifact(Helper.ArtifactStore.Address, requestBody, wrongArtifactId, _user);
            }, "'PATCH {0}' should return 400 Bad Request if the Artifact ID in the URL is different than in the body!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            const string expectedMessage = "Artifact does not match Id of request.";
            AssertRestResponseMessageIsCorrect(ex.RestResponse, expectedMessage);
        }

        [TestCase]
        [TestRail(156657)]
        [Description("Create & save an artifact.  Try to update the artifact but don't send a 'Session-Token' header in the request.  Verify 400 Bad Request is returned.")]
        public void UpdateArtifact_NoTokenHeader_401Unauthorized()
        {
            // Setup:
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Process);
            IUser userWithNoToken = Helper.CreateUserAndAddToDatabase();

            // Execute & Verify:
            var ex = Assert.Throws<Http401UnauthorizedException>(() => artifact.Save(userWithNoToken),
                "'PATCH {0}' should return 401 Unauthorized if no Session-Token header is passed!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            const string expectedMessage = "Unauthorized call.";
            AssertStringMessageIsCorrect(ex.RestResponse, expectedMessage);
        }

        [TestCase]
        [TestRail(156658)]
        [Description("Create & save an artifact.  Try to update the artifact but pass an unauthorized token.  Verify 401 Unauthorized is returned.")]
        public void UpdateArtifact_UnauthorizedToken_401Unauthorized()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Process);
            IUser userWithBadToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);
            artifact.Lock();

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() => artifact.Save(userWithBadToken, shouldGetLockForUpdate: false),
                "'PATCH {0}' should return 401 Unauthorized if an invalid token is passed!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            // Verify
            const string expectedMessage = "Unauthorized call";
            AssertStringMessageIsCorrect(ex.RestResponse, expectedMessage);
        }

        [TestCase]
        [TestRail(156659)]
        [Description("Create & publish an artifact.  Try to update the artifact as a user that doesn't have permission to update artifacts in the project.  Verify 403 Forbidden is returned.")]
        public void UpdateArtifact_UserWithoutPermissions_403Forbidden()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);

            IUser userWithoutPermission = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken,
                InstanceAdminRole.BlueprintAnalytics);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => artifact.Save(userWithoutPermission, shouldGetLockForUpdate: false),
                "'PATCH {0}' should return 403 Forbidden if the user doesn't have permission to update artifacts!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            // Verify:
            string expectedMessage = I18NHelper.FormatInvariant("You do not have permission to access the artifact (ID: {0})", artifact.Id);
            AssertRestResponseMessageIsCorrect(ex.RestResponse, expectedMessage);
        }

        [TestCase(0)]
        [TestCase(int.MaxValue)]
        [TestRail(156660)]
        [Description("Try to update an artifact with a non-existent Artifact ID.  Verify 404 Not Found is returned.")]
        public void UpdateArtifact_NonExistentArtifactId_404NotFound(int nonExistentArtifactId)
        {
            // Setup:
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Process);

            // Replace ProjectId with a fake ID that shouldn't exist.
            artifact.Id = nonExistentArtifactId;

            // Execute & Verify:
            var ex = Assert.Throws<Http404NotFoundException>(() => Artifact.UpdateArtifact(artifact, _user),
                "'PATCH {0}' should return 404 Not Found if the Artifact ID doesn't exist!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            // Verify the message, but skip for Id == 0 because in that case IIS returns the generic HTML 404 response.
            if (artifact.Id != 0)
            {
                const string expectedMessage = "You have attempted to access an artifact that does not exist or has been deleted.";
                AssertRestResponseMessageIsCorrect(ex.RestResponse, expectedMessage);
            }
        }

        [TestCase]
        [TestRail(156661)]
        [Description("Create & publish an artifact, then delete & publish it.  Try to update the deleted artifact.  Verify 404 Not Found is returned.")]
        public void UpdateArtifact_DeletedArtifact_404NotFound()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);
            artifact.Delete();
            artifact.Publish();

            // Execute & Verify:
            var ex = Assert.Throws<Http404NotFoundException>(() => Artifact.UpdateArtifact(artifact, _user),
                "'PATCH {0}' should return 404 Not Found if the artifact was deleted!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            const string expectedMessage = "You have attempted to access an artifact that does not exist or has been deleted.";
            AssertRestResponseMessageIsCorrect(ex.RestResponse, expectedMessage);
        }

        #region Subartifact Properties tests

        // TODO: This test is not reviewed and will get updated on next pull request for full review
        [Category(Categories.CustomData)]
        [TestCase(ItemTypePredefined.Process, Process.DefaultUserTaskName, "Std-Text-Required-RT-Multi-HasDefault")]
        [TestRail(195408)]
        [Explicit(IgnoreReasons.UnderDevelopment)] // TODO: (Trello case: https://trello.com/c/hKTwhfFM) Should returns other than 500 internal server error
        [Description("Create & publish an artifact.  Update a text property in a sub artifact with no contents, save and publish.  " +
             "Verify that the sub artifact returned the default text property.")]
        public void UpdateSubArtifact_ChangeTextPropertyWithEmpty_VerifyPropertyUnchanged(ItemTypePredefined itemType,
    string subArtifactDisplayName,
    string subArtifactCustomPropertyName)
        {
            // Setup: Set the required custom text property value for the target sub artifact with empty content
            IProject projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);
            projectCustomData.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, projectCustomData);
            var artifact = Helper.CreateWrapAndPublishNovaArtifactForStandardArtifactType(projectCustomData, author, itemType);
            var subArtifact = Helper.ArtifactStore.GetSubartifacts(author, artifact.Id).Find(sa => sa.DisplayName.Equals(subArtifactDisplayName));
            var defaultCustomProperty = Helper.ArtifactStore.GetSubartifact(author, artifact.Id, subArtifact.Id).CustomPropertyValues.Find(p => p.Name.Equals(subArtifactCustomPropertyName));

            var subArtifactCustomPropertyValue = "";
            var subArtifactChangeSet = CreateSubArtifactChangeSet(author, projectCustomData, artifact, subArtifactDisplayName, subArtifactCustomPropertyName, subArtifactCustomPropertyValue);
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, artifact.Id);
            artifactDetails.SubArtifacts = new List<NovaSubArtifact>() { subArtifactChangeSet };

            // Execute:Attempt to update the target sub artifact with empty content
            artifact.Lock(author);
            var ex = Assert.Throws < Http500InternalServerErrorException >(() => Helper.ArtifactStore.UpdateArtifact(author, projectCustomData, artifactDetails), "'PATCH {0}' should return 500 Internal Server Error Exception if the invalid subartifact changeset is requested!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            // Verify: Check that returned custom property name equals to default custom property since the requsted updated is invalid
            const string currentErrorMsg = "Unable to cast object of type 'BluePrintSys.RC.Data.AccessAPI.Model.DSubArtifact' to type 'BluePrintSys.RC.Data.AccessAPI.Model.DArtifact'.";
            Assert.That(ex.RestResponse.Content.Contains(currentErrorMsg));

            Assert.NotNull(subArtifact.Id, "The SubArtifact ID shouldn't be null!");
            var subArtifactAfter = Helper.ArtifactStore.GetSubartifact(author, artifact.Id, subArtifact.Id);
            var returnedProperty = subArtifactAfter.CustomPropertyValues.Find(p => p.Name.Equals(subArtifactCustomPropertyName));
            ArtifactStoreHelper.AssertCustomPropertiesAreEqual(defaultCustomProperty, returnedProperty);
        }

        #endregion Subartifact Properties tests

        #endregion Negative tests

        #region Custom data tests

        private const int CU_NUMBER_PROPERTY_ID = 120;
        private const int CU_DATE_PROPERTY_ID = 119;

        [Category(Categories.CustomData)]
        [TestCase("value\":10.0", "value\":\"A\"", CU_NUMBER_PROPERTY_ID)]   // Insert String into Numeric field.
        [TestCase("value\":\"20", "value\":\"A", CU_DATE_PROPERTY_ID)]       // Insert String into Date field.
        [TestRail(164561)]
        [Description("Try to update an artifact properties with a improper value types. Verify 200 OK Request is returned.")]
        public void UpdateArtifact_WrongTypeInProperty_CanGetArtifact(string toChange, string changeTo, int propertyTypeId)
        {
            // Setup:
            var projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);
            IArtifact artifact = Helper.CreateAndPublishArtifact(projectCustomData, _user, BaseArtifactType.Actor);
            artifact.Lock();

            NovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);

            string requestBody = JsonConvert.SerializeObject(artifactDetails);

            string modifiedRequestBody = requestBody.Replace(toChange, changeTo);
            Assert.AreNotEqual(requestBody, modifiedRequestBody, "Check that RequestBody was updated.");

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                ArtifactStoreHelper.UpdateInvalidArtifact(Helper.ArtifactStore.Address, modifiedRequestBody, artifact.Id, _user);
            }, "'PATCH {0}' should return 200 OK even if the value is set to wrong type!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            // Verify:
            NovaArtifactDetails artifactDetailsAfter = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);

            CustomProperty customProperty = GetCustomPropertyByPropertyTypeId(artifactDetailsAfter, "CustomPropertyValues", propertyTypeId);

            Assert.IsNull(customProperty.CustomPropertyValue, "Value of this custom property with Id {0} has to be null", propertyTypeId);

            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, artifactDetailsAfter);
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
            IArtifact artifact = Helper.CreateAndPublishArtifact(projectCustomData, _user, BaseArtifactType.Actor);
            artifact.Lock();

            NovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);

            string requestBody = JsonConvert.SerializeObject(artifactDetails);

            string changedValue = "value\":" + outOfRangeNumber;

            requestBody = requestBody.Replace(stringToReplace, changedValue);

            // Execute:
            Assert.DoesNotThrow(() => ArtifactStoreHelper.UpdateInvalidArtifact(Helper.ArtifactStore.Address, requestBody, artifact.Id, _user),
                "'PATCH {0}' should return 200 OK if properties are out of range!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            // Verify:
            NovaArtifactDetails artifactDetailsAfter = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);

            CustomProperty customPropertyAfter = GetCustomPropertyByPropertyTypeId(artifactDetailsAfter, "CustomPropertyValues", propertyTypeId);

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
            IArtifact artifact = Helper.CreateAndPublishArtifact(projectCustomData, _user, BaseArtifactType.Actor);
            artifact.Lock();

            NovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);

            string requestBody = JsonConvert.SerializeObject(artifactDetails);
            string changedValue = "value\":" + newNumberValue;

            requestBody = requestBody.Replace(textToReplace, changedValue);

            // Execute:
            Assert.DoesNotThrow(() => ArtifactStoreHelper.UpdateInvalidArtifact(Helper.ArtifactStore.Address, requestBody, artifact.Id, _user),
                "'PATCH {0}' should return 200 OK if properties are out of range!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            // Verify:
            NovaArtifactDetails artifactDetailsAfter = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);

            CustomProperty customPropertyAfter = GetCustomPropertyByPropertyTypeId(artifactDetailsAfter, "CustomPropertyValues", propertyTypeId);

            Assert.AreEqual(newNumberValue, customPropertyAfter.CustomPropertyValue,
                    "Value of this custom property with id {0} should be {1} but was {2}!",
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
            IArtifact artifact = Helper.CreateAndPublishArtifact(projectCustomData, _user, BaseArtifactType.Actor);
            artifact.Lock();

            int thisYear = DateTime.Now.Year;

            string toChange = "value\":\"" + thisYear;

            int yearOutPropertyRange = thisYear + 100;

            string requestBody = JsonConvert.SerializeObject(Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id));

            requestBody = requestBody.Replace(toChange, "value\":\"" + yearOutPropertyRange);

            // Execute:
            Assert.DoesNotThrow(() => ArtifactStoreHelper.UpdateInvalidArtifact(Helper.ArtifactStore.Address, requestBody, artifact.Id, _user),
                "'PATCH {0}' should return 200 OK if properties are out of range!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            // Verify:
            NovaArtifactDetails artifactDetailsAfter = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);

            CustomProperty customPropertyAfter = GetCustomPropertyByPropertyTypeId(artifactDetailsAfter, "CustomPropertyValues", propertyTypeId);

            DateTime newDate = (DateTime)customPropertyAfter.CustomPropertyValue;

            Assert.AreEqual(yearOutPropertyRange, newDate.Year,
                    "Value of year in this custom property with id {0} should be {1} but was {2}!", propertyTypeId, yearOutPropertyRange, newDate.Year);
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
            IArtifact artifact = Helper.CreateAndPublishArtifact(projectCustomData, _user, BaseArtifactType.Actor);
            artifact.Lock();

            NovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);

            string requestBody = JsonConvert.SerializeObject(artifactDetails);

            string modifiedRequestBody = requestBody.Replace(toChange, changeTo);
            Assert.AreNotEqual(requestBody, modifiedRequestBody, "Check that RequestBody was updated.");

            // Execute & Verify:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
                ArtifactStoreHelper.UpdateInvalidArtifact(Helper.ArtifactStore.Address, modifiedRequestBody, artifact.Id, _user),
                "'PATCH {0}' should return 400 Bad Request if the value is set to wrong type!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            AssertRestResponseMessageIsCorrect(ex.RestResponse, expectedError);
        }

        [TestCase]
        [Category(Categories.CustomData)]
        [TestRail(164624)]
        [Description("Try to update an artifact which is not locked by current user. Verify 409 Conflict is returned.")]
        public void UpdateArtifact_NotLockedByUser_409Conflict()
        {
            // Setup:
            var projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);
            IArtifact artifact = Helper.CreateAndPublishArtifact(projectCustomData, _user, BaseArtifactType.Actor);

            // Execute & Verify:
            var ex = Assert.Throws<Http409ConflictException>(() => Artifact.UpdateArtifact(artifact, _user),
                "'PATCH {0}' should return 409 Conflict if the user didn't lock on the artifact first",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            const string expectedError = "The artifact is not locked.";
            AssertRestResponseMessageIsCorrect(ex.RestResponse, expectedError);
        }

        #endregion Custom Data

        #region Private functions

        /// <summary>
        /// Common code for UpdateArtifact_PublishedArtifact_CanGetArtifact and UpdateArtifact_UnpublishedArtifact_CanGetArtifact tests.
        /// </summary>
        /// <param name="artifact">The artifact to update.</param>
        /// <param name="artifactType">The type of artifact.</param>
        /// <param name="propertyToChange">Property to change.</param>
        /// <param name="value">The value to what property will be changed</param>
        /// <param name="user">The user updating the artifact</param>
        private void UpdateArtifact_CanGetArtifact<T>(IArtifact artifact, BaseArtifactType artifactType, string propertyToChange, T value, IUser user)
        {
            // Setup:
            NovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetArtifactDetails(user, artifact.Id);

            SetProperty(propertyToChange, value, ref artifactDetails);

            NovaArtifactDetails updateResult = null;

            // Execute:
            Assert.DoesNotThrow(() => updateResult = Artifact.UpdateArtifact(artifact, user, artifactDetails, address: Helper.BlueprintServer.Address),
                "Exception caught while trying to update an artifact of type: '{0}'!", artifactType);

            // Verify:
            Assert.AreEqual(artifactDetails.CreatedBy?.DisplayName, updateResult.CreatedBy?.DisplayName, "The CreatedBy properties don't match!");

            IOpenApiArtifact openApiArtifact = OpenApiArtifact.GetArtifact(Helper.ArtifactStore.Address, _project, artifact.Id, user);
            ArtifactStoreHelper.AssertArtifactsEqual(updateResult, artifactDetails);
            TestHelper.AssertArtifactsAreEqual(artifact, openApiArtifact);
        }

        /// <summary>
        /// Create the changeset for the target sub artifact
        /// </summary>
        /// <param name="user">The user updating the sub artifact</param>
        /// <param name="projectCustomData">The project with all the avaliable Nova artifact types</param>
        /// <param name="artifact">The artifact that the target sub artifact resides</param>
        /// <param name="subArtifactDisplayName">display name for the sub artifact to update</param>
        /// <param name="subArtifactCustomPropertyName">custom property name for the sub artifact to update</param>
        /// <param name="subArtifactCustomPropertyValue">custom property value for the sub artifact to update</param>
        /// <returns>NovaSubArtifact that contains the change for the sub artifact</returns>
        private NovaSubArtifact CreateSubArtifactChangeSet(IUser user, IProject projectCustomData, IArtifact artifact, string subArtifactDisplayName, string subArtifactCustomPropertyName, object subArtifactCustomPropertyValue)
        {
            var subArtifact = Helper.ArtifactStore.GetSubartifacts(user, artifact.Id).Find(sa => sa.DisplayName.Equals(subArtifactDisplayName));
            var subArtifactChangeSet = Helper.ArtifactStore.GetSubartifact(user, artifact.Id, subArtifact.Id);
            var customPropertyValueToUpdate = subArtifactChangeSet.CustomPropertyValues.Find(p => p.Name.Equals(subArtifactCustomPropertyName));

            subArtifactChangeSet.CustomPropertyValues.Clear();
            subArtifactChangeSet.SpecificPropertyValues.Clear();

            switch (subArtifactCustomPropertyName)
            {
                case CustomPropertyName.TextRequiredRTMultiHasDefault:
                    {
                        customPropertyValueToUpdate.CustomPropertyValue = subArtifactCustomPropertyValue;
                        subArtifactChangeSet.CustomPropertyValues.Add(customPropertyValueToUpdate);
                        break;
                    }
                case CustomPropertyName.NumberRequiredValidatedDecPlacesMinMaxHasDefault:
                    {
                        customPropertyValueToUpdate.CustomPropertyValue = subArtifactCustomPropertyValue;
                        subArtifactChangeSet.CustomPropertyValues.Add(customPropertyValueToUpdate);
                        break;
                    }
                case CustomPropertyName.DateRequiredValidatedMinMaxHasDefault:
                    {
                        customPropertyValueToUpdate.CustomPropertyValue = subArtifactCustomPropertyValue;
                        subArtifactChangeSet.CustomPropertyValues.Add(customPropertyValueToUpdate);
                        break;
                    }
                case CustomPropertyName.ChoiceRequiredAllowMultipleDefaultValue:
                    {
                        var choicePropertyValidValues = projectCustomData.NovaPropertyTypes.Find(pt => pt.Name.Equals(subArtifactCustomPropertyName)).ValidValues;

                        var collectedChoicePropertyValueList = new List<NovaPropertyType.ValidValue>();
                        foreach (string choiceValue in (List<string>)subArtifactCustomPropertyValue)
                        {
                            collectedChoicePropertyValueList.Add(choicePropertyValidValues.Find(vv => vv.Value.Equals(choiceValue)));
                        }
                        customPropertyValueToUpdate.CustomPropertyValue = new ArtifactStoreHelper.ChoiceValues() { ValidValues = collectedChoicePropertyValueList };
                        subArtifactChangeSet.CustomPropertyValues.Add(customPropertyValueToUpdate);
                        break;
                    }
                case CustomPropertyName.UserRequiredHasDefaultUser:
                    {
                        var userData = (IUser)subArtifactCustomPropertyValue;
                        var newIdentification = new Identification() { DisplayName = userData.DisplayName, Id = userData.Id };
                        var newUserPropertyValue = new List<Identification> { newIdentification };
                        customPropertyValueToUpdate.CustomPropertyValue = new ArtifactStoreHelper.UserGroupValues() { UsersGroups = newUserPropertyValue };
                        subArtifactChangeSet.CustomPropertyValues.Add(customPropertyValueToUpdate);
                        break;
                    }
            }

            return subArtifactChangeSet;
        }

        /// <summary>
        /// Set one primary property to specific value.
        /// </summary>
        /// <param name="propertyName">Name of the property in which value will be changed.</param>
        /// <param name="propertyValue">The value to set the property to.</param>
        /// <param name="objectToUpdate">Object that contains the property to be changed.</param>
        private static void SetProperty<T>(string propertyName, T propertyValue, ref NovaArtifactDetails objectToUpdate)
        {
            objectToUpdate.GetType().GetProperty(propertyName).SetValue(objectToUpdate, propertyValue, null);
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

        /// <summary>
        /// Asserts that the specified RestResponse contains the expected error message.
        /// </summary>
        /// <param name="restReponse">The RestResponse that contains the message.</param>
        /// <param name="expectedMessage">The expected error message.</param>
        /// <param name="requestMethod">(optional) The REST request method of the call.  This is used for the assert message.</param>
        private static void AssertRestResponseMessageIsCorrect(RestResponse restReponse, string expectedMessage, string requestMethod = "PATCH")
        {
            SaveArtifactResult result = JsonConvert.DeserializeObject<SaveArtifactResult>(restReponse.Content);

            Assert.AreEqual(expectedMessage, result.Message, "The wrong message was returned by '{0} {1}'.",
                requestMethod, RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);
        }

        /// <summary>
        /// Asserts that the specified RestResponse contains the expected error message.
        /// </summary>
        /// <param name="restResponse">The RestResponse that contains the message.</param>
        /// <param name="expectedMessage">The expected error message.</param>
        /// <param name="requestMethod">(optional) The REST request method of the call.  This is used for the assert message.</param>
        private static void AssertStringMessageIsCorrect(RestResponse restResponse, string expectedMessage, string requestMethod = "PATCH")
        {
            string result = JsonConvert.DeserializeObject<string>(restResponse.Content);

            Assert.AreEqual(expectedMessage, result, "The wrong message was returned by '{0} {1}'.",
                requestMethod, RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);
        }

        #endregion Private functions
    }
}
