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
using TestCommon;
using Utilities;
using Utilities.Facades;
using Utilities.Factories;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class SaveArtifactTests : TestBase
    {
        private IUser _user = null;
        private IProject _project = null;
        private List<IProject> _allProjects = null;

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

        #region private functions
        private static NovaSubArtifact CreateSubartifactChangeset(NovaSubArtifact subArtifact, string customPropertyName)
        {
            var targetCustomPropertyValue = subArtifact.CustomPropertyValues.Find(custP => custP.Name.Equals(customPropertyName));
            targetCustomPropertyValue.Value = StringUtilities.WrapInHTML("TestString_" + RandomGenerator.RandomAlphaNumeric(5));
            subArtifact.CustomPropertyValues.Clear();
            subArtifact.SpecificPropertyValues.Clear();
            subArtifact.CustomPropertyValues.Add(targetCustomPropertyValue);
            return subArtifact;
        }

        #endregion private functions
        
        #region 200 OK tests

        [TestCase]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestRail(191148)]
        [Description("Create a process artifact. Update and publish artifact. Update sub artifact properties with UpdateArtifact. Verify that sub artifact returned has the updated properties.")]
        public void UpdateArtifact_UpdateSubArtifactCustomProperties_CanGetSubArtifactsWithChanges()
        {
            // Setup:
            var customProject = ArtifactStoreHelper.GetCustomDataProject(_user);
            customProject.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);
            var artifactTypeName = GetStandardPackArtifactTypeName(ItemTypePredefined.Process);
            var processArtifact = Helper.CreateWrapAndPublishNovaArtifact(customProject, _user, ItemTypePredefined.Process, artifactTypeName: artifactTypeName);
            var process = Helper.Storyteller.GetProcess(_user, processArtifact.Id);
            var defaultUserTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            var subArtifact = Helper.ArtifactStore.GetSubartifact(_user, processArtifact.Id, defaultUserTask.Id);

            // Execute: Update using PATCH UpdateArtifact with changeset for subartifact
            processArtifact.Lock(_user);
            var targetCustomPropertyName = "Std-Text-Required-RT-Multi-HasDefault";
            var subArtifactChangeset = CreateSubartifactChangeset(subArtifact, targetCustomPropertyName);
            var artifactDetailsChangeSet = Helper.ArtifactStore.GetArtifactDetails(_user, processArtifact.Id);
            List<INovaSubArtifact> subArtifacts = new List<INovaSubArtifact> { subArtifactChangeset };
            artifactDetailsChangeSet.SubArtifacts = subArtifacts;
            Assert.DoesNotThrow(() => Artifact.UpdateArtifact(processArtifact, _user, artifactDetailsChangeSet),
                "Exception caught while trying to update an artifact of type: '{0}'!", BaseArtifactType.Process);

            // Verify: The returned subartifact contains the change made using the PATCH UpdateArtifact
            var returnedSubArtifact = Helper.ArtifactStore.GetSubartifact(_user, processArtifact.Id, defaultUserTask.Id);
            var requestedSubartifactCustomPropertyValue = subArtifactChangeset.CustomPropertyValues.Find(cp => cp.Name.Equals(targetCustomPropertyName)).Value.ToString();
            var updatedSubartifactCustomPropertyValue = returnedSubArtifact.CustomPropertyValues.Find(cp => cp.Name.Equals(targetCustomPropertyName)).Value.ToString();
            Assert.AreEqual(updatedSubartifactCustomPropertyValue, requestedSubartifactCustomPropertyValue);
        }

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

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase]
        [TestRail(999999)]
        [Description("Create & publish a Process artifact.  Update a text property, save and publish.  Verify the artifact returned the text property updated.")]
        public void UpdateProcessArtifact_ChangeTextPropertySaveAndPublish_VerifyPropertyChanged()
        {
            // Setup:
            var projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);

            projectCustomData.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, projectCustomData);

            var artifactTypeName = GetStandardPackArtifactTypeName(ItemTypePredefined.Process);

            var artifact = Helper.CreateWrapAndPublishNovaArtifact(projectCustomData, author, ItemTypePredefined.Process, artifactTypeName: artifactTypeName);

            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);

            // Execute:
            //  UpdateArtifact_CanGetArtifact(artifact, artifactType, "Description", "NewDescription_" + RandomGenerator.RandomAlphaNumeric(5), author);

            // Verify:
            var artifactDetailsAfter = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);

            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, artifactDetailsAfter);
        }

        #endregion Artifact Properties tests

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
            AssertStringMessaceIsCorrect(ex.RestResponse, expectedMessage);
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
            AssertStringMessaceIsCorrect(ex.RestResponse, expectedMessage);
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

            foreach (CustomProperty property in properties)
            {
                if (property.PropertyTypeId == propertyTypeId)
                {
                    return property;
                }
            }
            return null;
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
        /// <param name="restReponse">The RestResponse that contains the message.</param>
        /// <param name="expectedMessage">The expected error message.</param>
        /// <param name="requestMethod">(optional) The REST request method of the call.  This is used for the assert message.</param>
        private static void AssertStringMessaceIsCorrect(RestResponse restReponse, string expectedMessage, string requestMethod = "PATCH")
        {
            string result = JsonConvert.DeserializeObject<string>(restReponse.Content);

            Assert.AreEqual(expectedMessage, result, "The wrong message was returned by '{0} {1}'.",
                requestMethod, RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);
        }

        /// <summary>
        /// Gets the Standard Pack Artifact Type that matches the given ItemTypePredefined
        /// </summary>
        /// <param name="itemType">The predefined item type</param>
        /// <returns>A string indicating the name of the Standard Pack artifact name for the predefined item type.</returns>
        private static string GetStandardPackArtifactTypeName(ItemTypePredefined itemType)
        {
            return I18NHelper.FormatInvariant("{0}(Standard Pack)", Enum.GetName(typeof(ItemTypePredefined), itemType));
        }

        #endregion Private functions
    }
}
