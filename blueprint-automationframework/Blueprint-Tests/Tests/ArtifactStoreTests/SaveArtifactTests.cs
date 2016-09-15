using System.Collections.Generic;
using System.Linq;
using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Newtonsoft.Json;
using NUnit.Framework;
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

        #region SaveArtifact tests

        [Explicit(IgnoreReasons.UnderDevelopment)]  // POST (Save) functionality isn't implemented yet, only PATCH.
        [TestCase(BaseArtifactType.Actor)]
        [TestCase(BaseArtifactType.BusinessProcess)]
        [TestCase(BaseArtifactType.Document)]
        [TestCase(BaseArtifactType.DomainDiagram)]
        [TestCase(BaseArtifactType.GenericDiagram)]
        [TestCase(BaseArtifactType.Glossary)]
        [TestCase(BaseArtifactType.PrimitiveFolder)]
        [TestCase(BaseArtifactType.Process)]
        [TestCase(BaseArtifactType.Storyboard)]
        [TestCase(BaseArtifactType.TextualRequirement)]
        [TestCase(BaseArtifactType.UIMockup)]
        [TestCase(BaseArtifactType.UseCase)]
        [TestCase(BaseArtifactType.UseCaseDiagram)]
        [TestRail(154745)]
        [Description("Create & save an artifact.  Get the artifact.  Verify the artifact returned has the same properties as the artifact we saved.")]
        public void SaveArtifact_UnpublishedArtifact_CanGetArtifact(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateArtifact(_project, _user, artifactType);

            // Execute:
            Assert.DoesNotThrow(() => artifact.Save(),
                "Exception caught while trying to save an artifact of type: '{0}'!", artifactType);

            // Verify:
            IOpenApiArtifact openApiArtifact = OpenApiArtifact.GetArtifact(Helper.BlueprintServer.Address, _project, artifact.Id, _user);
            TestHelper.AssertArtifactsAreEqual(artifact, openApiArtifact);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]  // POST (Save) functionality isn't implemented yet, only PATCH.
        [TestCase]
        [TestRail(154746)]
        [Description("Create & save an artifact but don't send a 'Session-Token' header in the request.  Verify 400 Bad Request is returned.")]
        public void SaveArtifact_NoTokenHeader_400BadRequest()
        {
            // Setup:
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Process);
            IUser userWithNoToken = Helper.CreateUserAndAddToDatabase();

            // Execute & Verify:
            Assert.Throws<Http400BadRequestException>(() => artifact.Save(userWithNoToken),
                "'POST {0}' should return 400 Bad Request if no Session-Token header is passed!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]  // POST (Save) functionality isn't implemented yet, only PATCH.
        [TestCase]
        [TestRail(154747)]
        [Description("Create & save an artifact but pass an unauthorized token.  Verify 401 Unauthorized is returned.")]
        public void SaveArtifact_UnauthorizedToken_401Unauthorized()
        {
            // Setup:
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Process);
            IUser userWithBadToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            // Execute & Verify:
            Assert.Throws<Http401UnauthorizedException>(() => artifact.Save(userWithBadToken),
                "'POST {0}' should return 401 Unauthorized if an invalid token is passed!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]  // POST (Save) functionality isn't implemented yet, only PATCH.
        [TestCase]
        [TestRail(154748)]
        [Description("Create & save an artifact as a user that doesn't have permission to add artifacts to the project.  Verify 403 Forbidden is returned.")]
        public void SaveArtifact_UserWithoutPermissions_403Forbidden()
        {
            // Setup:
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Process);

            IUser userWithoutPermission = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken,
                InstanceAdminRole.BlueprintAnalytics);

            // Execute & Verify:
            Assert.Throws<Http403ForbiddenException>(() => artifact.Save(userWithoutPermission),
                "'POST {0}' should return 403 Forbidden if the user doesn't have permission to add artifacts!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]  // POST (Save) functionality isn't implemented yet, only PATCH.
        [TestCase(0)]
        [TestCase(int.MaxValue)]
        [TestRail(154749)]
        [Description("Create & save an artifact with a non-existent Project ID.  Verify 404 Not Found is returned.")]
        public void SaveArtifact_NonExistentProjectId_404NotFound(int projectId)
        {
            // Setup:
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Process);

            // Replace ProjectId with a fake ID that shouldn't exist.
            artifact.ProjectId = projectId;

            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() => artifact.Save(),
                "'POST {0}' should return 404 Not Found if the Project ID doesn't exist!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);
        }

        #endregion SaveArtifact tests

        #region UpdateArtifact tests

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForOpenApiRestMethods))]
        [TestRail(156656)]
        [Description("Create & publish an artifact.  Update the artifact.  Get the artifact.  Verify the artifact returned has the same properties as the artifact we updated.")]
        public void UpdateArtifact_PublishedArtifact_CanGetArtifact(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            artifact.Lock();

            UpdateArtifact_CanGetArtifact(artifact, artifactType, "Description", "NewDescription_" + RandomGenerator.RandomAlphaNumeric(5));
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForOpenApiRestMethods))]
        [TestRail(156917)]
        [Description("Create & save an artifact.  Update the artifact.  Get the artifact.  Verify the artifact returned has the same properties as the artifact we updated.")]
        public void UpdateArtifact_UnpublishedArtifact_CanGetArtifact(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _user, artifactType);

            // Execute & Verify:
            UpdateArtifact_CanGetArtifact(artifact, artifactType, "Description", "NewDescription_" + RandomGenerator.RandomAlphaNumeric(5));
        }

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
                UpdateInvalidArtifact(requestBody, artifact.Id, _user);
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
                UpdateInvalidArtifact(requestBody, artifact.Id, _user);
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
                UpdateInvalidArtifact(requestBody, artifact.Id, _user);
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
                UpdateInvalidArtifact(requestBody, wrongArtifactId, _user);
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

            // Execute & Verify:
            var ex = Assert.Throws<Http401UnauthorizedException>(() => artifact.Save(userWithBadToken, shouldGetLockForUpdate: false),
                "'PATCH {0}' should return 401 Unauthorized if an invalid token is passed!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

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

            artifact.Lock();

            // Execute & Verify:
            var ex = Assert.Throws<Http403ForbiddenException>(() => artifact.Save(userWithoutPermission, shouldGetLockForUpdate: false),
                "'PATCH {0}' should return 403 Forbidden if the user doesn't have permission to update artifacts!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

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

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForOpenApiRestMethods))]
        [TestRail(164531)]
        [Description("Create & publish an artifact. Update the artifact property 'Name' with Empty space. Get the artifact. Verify the artifact returned has the same properties as the artifact we updated.")]
        public void UpdateArtifact_PublishedArtifact_SetEmptyNameProperty_CanGetArtifact(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            artifact.Lock();

            // Execute & Verify:
            UpdateArtifact_CanGetArtifact(artifact, artifactType, "Name", "");
        }

        #region Custom data tests

        [Category(Categories.CustomData)]
        [TestCase("Value\":10.0", "Value\":999.0")] //Insert value into Numeric field which is out of range
        [TestCase("Value\":\"20", "Value\":\"21")] //Insert value into Date field which is out of range
        [TestRail(164595)]
        [Description("Try to update an artifact properties with a value that out of its permitted range. Verify 200 OK Request is returned.")]
        public void UpdateArtifact_PropertyOutOfRange_200OK(string toChange, string changeTo)
        {
            // Setup:
            var projectCustomData = GetCustomDataProject();
            IArtifact artifact = Helper.CreateAndPublishArtifact(projectCustomData, _user, BaseArtifactType.Actor);
            artifact.Lock();

            NovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);

            string requestBody = JsonConvert.SerializeObject(artifactDetails);

            requestBody = requestBody.Replace(toChange, changeTo);

            // Execute:
            string resultContent = null;
            Assert.DoesNotThrow(() => resultContent = UpdateInvalidArtifact(requestBody, artifact.Id, _user),
                "'PATCH {0}' should return 200 OK if properties are out of range!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);
        }

        private const string NumberValueIncorrectFormat = "The property CU-Number Required with Min & Max was supplied a value in an incorrect format.";
        private const string DateValueIncorrectFormat   = "The property CU-Date Required with Min & Max was supplied a value in an incorrect format.";
        private const string ChoiceValueIncorrectFormat = "The value for the property CU-Choice Required with Single Choice is invalid.";
        private const string UserValueIncorrectFormat   = "The value for the property CU-User Required is invalid.";

        [Category(Categories.CustomData)]
        [TestCase("value\":10.0", "value\":\"A\"", NumberValueIncorrectFormat)]                         // Insert String into Numeric field.
        [TestCase("value\":\"20", "value\":\"A", DateValueIncorrectFormat)]                             // Insert String into Date field.
        [TestCase("validValueIds\":[22]", "validValueIds\":[0]", ChoiceValueIncorrectFormat)]           // Insert non-existant choice.
        [TestCase("usersGroups\":[{\"id\":1", "usersGroups\":[{\"id\":0", UserValueIncorrectFormat)]    // Insert non-existant User ID.
        [TestRail(164561)]
        [Description("Try to update an artifact properties with a improper value types. Verify 400 Bad Request is returned.")]
        public void UpdateArtifact_WrongType1InProperty_400BadRequest(string toChange, string changeTo, string expectedError)
        {
            // Setup:
            var projectCustomData = GetCustomDataProject();
            IArtifact artifact = Helper.CreateAndPublishArtifact(projectCustomData, _user, BaseArtifactType.Actor);
            artifact.Lock();

            NovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);

            string requestBody = JsonConvert.SerializeObject(artifactDetails);
            
            string  modifiedRequestBody = requestBody.Replace(toChange, changeTo);
            Assert.AreNotEqual(requestBody, modifiedRequestBody, "Check that RequestBody was updated.");

            // Execute & Verify:
            var ex = Assert.Throws<Http400BadRequestException>(() => UpdateInvalidArtifact(modifiedRequestBody, artifact.Id, _user),
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
            var projectCustomData = GetCustomDataProject();
            IArtifact artifact = Helper.CreateAndPublishArtifact(projectCustomData, _user, BaseArtifactType.Actor);

            // Execute & Verify:
            var ex = Assert.Throws<Http409ConflictException>(() => Artifact.UpdateArtifact(artifact, _user),
                "'PATCH {0}' should return 409 Conflict if the user didn't lock on the artifact first",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            const string expectedError = "The artifact is not locked.";
            AssertRestResponseMessageIsCorrect(ex.RestResponse, expectedError);
        }

        #endregion Custom Data

        #endregion UpdateArtifact tests

        #region Private functions

        /// <summary>
        /// Gets the custom data project.
        /// </summary>
        /// <returns>The custom data project.</returns>
        private IProject GetCustomDataProject()
        {
            const string customDataProjectName = "Custom Data";

            Assert.That(_allProjects.Exists(p => (p.Name == customDataProjectName)),
                "No project was found named '{0}'!", customDataProjectName);
            
            var projectCustomData = _allProjects.First(p => (p.Name == customDataProjectName));
            projectCustomData.GetAllArtifactTypes(ProjectFactory.Address, _user);

            return projectCustomData;
        }

        /// <summary>
        /// Common code for UpdateArtifact_PublishedArtifact_CanGetArtifact and UpdateArtifact_UnpublishedArtifact_CanGetArtifact tests.
        /// </summary>
        /// <param name="artifact">The artifact to update.</param>
        /// <param name="artifactType">The type of artifact.</param>
        /// <param name="propertyToChange">Property to change.</param>
        /// <param name="value">The value to what property will be changed</param>
        private void UpdateArtifact_CanGetArtifact<T>(IArtifact artifact, BaseArtifactType artifactType, string propertyToChange, T value)
        {
            // Setup:
            NovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);

            SetProperty(propertyToChange, value, ref artifactDetails);

            NovaArtifactDetails updateResult = null;

            // Execute:
            Assert.DoesNotThrow(() => updateResult = Artifact.UpdateArtifact(artifact, _user, artifactDetails, Helper.BlueprintServer.Address),
                "Exception caught while trying to update an artifact of type: '{0}'!", artifactType);

            // Verify:
            Assert.AreEqual(artifactDetails.CreatedBy?.DisplayName, updateResult.CreatedBy?.DisplayName, "The CreatedBy properties don't match!");

            IOpenApiArtifact openApiArtifact = OpenApiArtifact.GetArtifact(Helper.BlueprintServer.Address, _project, artifact.Id, _user);
            updateResult.AssertEquals(artifactDetails);

            //wrapped artifact name with html format to compared with the updated artifact with Artifact.UpdateArtifact
            if (artifact.Name.Any())
            {
                artifact.Name = StringUtilities.WrapInHTML(artifact.Name);
            }

            TestHelper.AssertArtifactsAreEqual(artifact, openApiArtifact);
        }

        /// <summary>
        /// Set one primary property to specific value.
        /// </summary>
        /// <param name="propertyName">Name of the property in which value will be changed.</param>
        /// <param name="propertyValue">The value to set the property to.</param>
        /// <param name="objectToUpadate">Object that contains the property to be changed.</param>
        private static void SetProperty<T>(string propertyName, T propertyValue, ref NovaArtifactDetails objectToUpadate)
        {
            objectToUpadate.GetType().GetProperty(propertyName).SetValue(objectToUpadate, propertyValue, null);
        }

        /// <summary>
        /// Try to update an invalid Artifact with Property Changes.  Use this for testing cases where the save is expected to fail.
        /// </summary>
        /// <param name="requestBody">The request body (i.e. artifact to be updated).</param>
        /// <param name="artifactId">The ID of the artifact to save.</param>
        /// <param name="user">The user updating the artifact.</param>
        /// <returns>The body content returned from ArtifactStore.</returns>
        private string UpdateInvalidArtifact(string requestBody,
            int artifactId,
            IUser user)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            string tokenValue = user.Token?.AccessControlToken;

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.ARTIFACTS_id_, artifactId);
            RestApiFacade restApi = new RestApiFacade(Helper.BlueprintServer.Address, tokenValue);
            const string contentType = "application/json";

            var response = restApi.SendRequestBodyAndGetResponse(
                path,
                RestRequestMethod.PATCH,
                requestBody,
                contentType);

            return response.Content;
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

        #endregion Private functions
    }
}
