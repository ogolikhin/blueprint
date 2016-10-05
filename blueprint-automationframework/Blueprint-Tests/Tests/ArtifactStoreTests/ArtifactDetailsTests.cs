using System.Collections.Generic;
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

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class ArtifactDetailsTests : TestBase
    {
        /// <summary>
        /// This is the structure returned by the REST call to display error messages.
        /// </summary>
        public class MessageResult
        {
            public int ErrorCode { get; set; }
            public string Message { get; set; }
        }

        private IUser _user = null;
        private IProject _project = null;

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

        #region 200 OK Tests

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForOpenApiRestMethods))]
        [TestRail(154601)]
        [Description("Create & publish an artifact, GetArtifactDetails.  Verify the artifact details are returned.")]
        public void GetArtifactDetails_PublishedArtifact_ReturnsArtifactDetails(BaseArtifactType artifactType)
        {
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            var retrievedArtifact = OpenApiArtifact.GetArtifact(artifact.Address, _project, artifact.Id, _user);

            NovaArtifactDetails artifactDetails = null;

            Assert.DoesNotThrow(() =>
            {
                artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);
             
            artifactDetails.AssertEquals(retrievedArtifact);

            Assert.AreEqual(8159, artifactDetails.Permissions, "Instance Admin should have all permissions (i.e. 8159)!");
        }

        [TestCase(2)]
        [TestCase(11)]
        [TestRail(154706)]
        [Description("Create & publish an artifact, modify & publish it again, GetArtifactDetails.  Verify the artifact details for the latest version are returned.")]
        public void GetArtifactDetails_PublishedArtifactWithMultipleVersions_ReturnsArtifactDetailsForLatestVersion(int numberOfVersions)
        {
            var openApiArtifacts = new List<IOpenApiArtifact>();
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);

            var retrievedArtifactVersion = OpenApiArtifact.GetArtifact(artifact.Address, _project, artifact.Id, _user);
            openApiArtifacts.Add(retrievedArtifactVersion);

            // Create several artifact versions.
            for (int i = 1; i < numberOfVersions; ++i)
            {
                // These are internal properties used by automation, so OpenAPI doesn't set them for us.
                retrievedArtifactVersion.Address = artifact.Address;
                retrievedArtifactVersion.CreatedBy = artifact.CreatedBy;

                // Modify & publish the artifact.
                retrievedArtifactVersion.Name = I18NHelper.FormatInvariant("{0}-version{1}", retrievedArtifactVersion.Name, i + 1);

                Artifact.SaveArtifact(retrievedArtifactVersion, _user);
                retrievedArtifactVersion.Publish();

                // Get the artifact from OpenAPI.
                retrievedArtifactVersion = OpenApiArtifact.GetArtifact(artifact.Address, _project, artifact.Id, _user);
                openApiArtifacts.Add(retrievedArtifactVersion);
            }

            NovaArtifactDetails artifactDetails = null;

            Assert.DoesNotThrow(() =>
            {
                artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            artifactDetails.AssertEquals(retrievedArtifactVersion);

            Assert.IsEmpty(artifactDetails.SpecificPropertyValues,
                "SpecificPropertyValues isn't implemented yet so it should be empty!");

            Assert.AreEqual(8159, artifactDetails.Permissions, "Instance Admin should have all permissions (i.e. 8159)!");
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(154700)]
        [Description("Create & publish an artifact, modify & publish it again, GetArtifactDetails with versionId=1.  Verify the artifact details for the first version are returned.")]
        public void GetArtifactDetailsWithVersionId1_PublishedArtifactWithMultipleVersions_ReturnsArtifactDetailsForFirstVersion(BaseArtifactType artifactType)
        {
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            var retrievedArtifactVersion1 = OpenApiArtifact.GetArtifact(artifact.Address, _project, artifact.Id, _user);

            // These are internal properties used by automation, so OpenAPI doesn't set them for us.
            retrievedArtifactVersion1.Address = artifact.Address;
            retrievedArtifactVersion1.CreatedBy = artifact.CreatedBy;

            // Modify & publish the artifact.
            var retrievedArtifactVersion2 = retrievedArtifactVersion1.DeepCopy();
            retrievedArtifactVersion2.Name = I18NHelper.FormatInvariant("{0}-version2", retrievedArtifactVersion1.Name);

            Artifact.SaveArtifact(retrievedArtifactVersion2, _user);
            retrievedArtifactVersion2.Publish();

            NovaArtifactDetails artifactDetails = null;

            Assert.DoesNotThrow(() =>
            {
                artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id, versionId: 1);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            artifactDetails.AssertEquals(retrievedArtifactVersion1);

            Assert.IsEmpty(artifactDetails.SpecificPropertyValues,
                "SpecificPropertyValues isn't implemented yet so it should be empty!");

            Assert.AreEqual(8159, artifactDetails.Permissions, "Instance Admin should have all permissions (i.e. 8159)!");
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(166146)]
        [Description("Create & publish an artifact, modify & publish it again, then delete & publish it.  GetArtifactDetails with versionId=1.  " +
            "Verify the artifact details for the first version are returned.")]
        public void GetArtifactDetailsWithVersionId1_DeletedArtifactWithMultipleVersions_ReturnsArtifactDetailsForFirstVersion(BaseArtifactType artifactType)
        {
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            var retrievedArtifactVersion1 = OpenApiArtifact.GetArtifact(artifact.Address, _project, artifact.Id, _user);

            // These are internal properties used by automation, so OpenAPI doesn't set them for us.
            retrievedArtifactVersion1.Address = artifact.Address;
            retrievedArtifactVersion1.CreatedBy = artifact.CreatedBy;

            // Modify & publish the artifact.
            var retrievedArtifactVersion2 = retrievedArtifactVersion1.DeepCopy();
            retrievedArtifactVersion2.Name = I18NHelper.FormatInvariant("{0}-version2", retrievedArtifactVersion1.Name);

            Artifact.SaveArtifact(retrievedArtifactVersion2, _user);
            retrievedArtifactVersion2.Publish();

            artifact.Delete();
            artifact.Publish();

            NovaArtifactDetails artifactDetails = null;

            Assert.DoesNotThrow(() =>
            {
                artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id, versionId: 1);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            artifactDetails.AssertEquals(retrievedArtifactVersion1);

            Assert.IsEmpty(artifactDetails.SpecificPropertyValues,
                "SpecificPropertyValues isn't implemented yet so it should be empty!");

            Assert.AreEqual(8159, artifactDetails.Permissions, "Instance Admin should have all permissions (i.e. 8159)!");
        }

        [TestCase]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestRail(0)]
        [Description("GetArtifactDetailsForTheArtifactContainingInlineTrace. Update regular inline trace artifact - Verify that GetArtifactDetails returned updated inline trace information.")]
        public void GetArtifactDetailsForArtiactWithInlineTrace_UpdateRegularInlineTraceArtifact_VerifyGetArtifactDetailsContainsUpdatedInlineTranceInformation()
        {
            // Setup: Create inline trace artifact
            var mainArtifact = Helper.CreateAndPublishArtifact(_project, _user, Model.ArtifactModel.BaseArtifactType.Actor);
            var inlineTraceArtifact = Helper.CreateAndPublishArtifact(_project, _user, Model.ArtifactModel.BaseArtifactType.Actor);
            NovaArtifactDetails inlineTraceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, inlineTraceArtifact.Id);

            // Setup: Update main artifact to have inline trace to inline trace artifact created
            NovaArtifactDetails artifactDetailsToUpdateMainArtifact = new NovaArtifactDetails
            {
                Id = mainArtifact.Id,
                ProjectId = mainArtifact.ProjectId,
                ParentId = mainArtifact.ParentId,
                Version = mainArtifact.Version,
                Description = CreateArtifactInlineTraceValue(inlineTraceArtifact, inlineTraceArtifactDetails),
            };

            // Execute: Update main artifact with inline trace to target artifact
            mainArtifact.Lock();
            Assert.DoesNotThrow(() => inlineTraceArtifactDetails = Artifact.UpdateArtifact(mainArtifact, _user, artifactDetailsChanges: artifactDetailsToUpdateMainArtifact),
                "UpdateArtifact call failed when using the following artifact ID: {0}!", mainArtifact.Id);

            // Setup: Update inlinetrace artifact information
            NovaArtifactDetails artifactDetailsToUpdateInlineTraceArtifact = new NovaArtifactDetails
            {
                Id = inlineTraceArtifact.Id,
                ProjectId = inlineTraceArtifact.ProjectId,
                ParentId = inlineTraceArtifact.ParentId,
                Version = inlineTraceArtifact.Version,
                Name = inlineTraceArtifact.Name + "_NameUpdated"
            };

            // Execute: Update inlinetrace artifact
            inlineTraceArtifact.Lock();
            Assert.DoesNotThrow(() => inlineTraceArtifactDetails = Artifact.UpdateArtifact(inlineTraceArtifact, _user, artifactDetailsChanges: artifactDetailsToUpdateInlineTraceArtifact),
                "UpdateArtifact call failed when using the following artifact ID: {0}!", inlineTraceArtifact.Id);

            // Execute: Get ArtifactDetails for main artifact
            var mainArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, mainArtifact.Id);

            // Validation: Verify that returned ArtifactDeatils contains the updated information for InlineTrace
            Assert.That(mainArtifactDetails.Description.Contains(artifactDetailsToUpdateInlineTraceArtifact.Name), "Expected outcome should contains {0} on returned artifactdetails. Returned inline trace content is {1}.", artifactDetailsToUpdateInlineTraceArtifact.Name, mainArtifactDetails.Description);
        }

        #endregion 200 OK Tests

        [TestCase]
        [TestRail(154701)]
        [Description("Create & publish an artifact, GetArtifactDetails but don't send any Session-Token header.  Verify it returns 401 Unauthorized.")]
        public void GetArtifactDetails_PublishedArtifactNoTokenHeader_401Unauthorized()
        {
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);

            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDetails(user: null, artifactId: artifact.Id);
            }, "'GET {0}' should return 401 Unauthorized when passed a valid artifact ID but no Session-Token in the header!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);
        }

        [TestCase]
        [TestRail(154702)]
        [Description("Create & publish an artifact, GetArtifactDetails but use an unauthorized token.  Verify it returns 401 Unauthorized.")]
        public void GetArtifactDetails_PublishedArtifactUnauthorizedToken_401Unauthorized()
        {
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);
            IUser unauthorizedUser = Helper.CreateUserAndAddToDatabase();

            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDetails(unauthorizedUser, artifact.Id);
            }, "'GET {0}' should return 401 Unauthorized when passed a valid artifact ID but an unauthorized token!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);
        }

        [TestCase]
        [TestRail(154703)]
        [Description("Create & publish an artifact, GetArtifactDetails with a user that doesn't have access to the artifact.  Verify it returns 403 Forbidden.")]
        public void GetArtifactDetails_PublishedArtifactUserWithoutPermissions_403Forbidden()
        {
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);
            IUser unauthorizedUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken,
                InstanceAdminRole.BlueprintAnalytics);

            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDetails(unauthorizedUser, artifact.Id);
            }, "'GET {0}' should return 403 Forbidden when passed a valid artifact ID but the user doesn't have permission to view the artifact!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            string expectedMessage = I18NHelper.FormatInvariant("You do not have permission to access the artifact (ID: {0})", artifact.Id);
            AssertJsonResponseEquals(expectedMessage, ex.RestResponse.Content,
                "If called by a user without permission to the artifact, we should get an error message of '{0}'!", expectedMessage);
        }

        [TestCase]
        [TestRail(166147)]
        [Description("Create & publish an artifact, modify save & publish it again.  GetArtifactDetails with version=1 with a user that doesn't have access to the artifact.  " +
            "Verify it returns 403 Forbidden.")]
        public void GetArtifactDetailsWithVersion1_PublishedArtifactWithMultipleVersions_UserWithoutPermissions_403Forbidden()
        {
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);
            IUser unauthorizedUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken,
                InstanceAdminRole.BlueprintAnalytics);

            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDetails(unauthorizedUser, artifact.Id, versionId: 1);
            }, "'GET {0}' should return 403 Forbidden when passed a valid artifact ID but the user doesn't have permission to view the artifact!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            string expectedMessage = I18NHelper.FormatInvariant("You do not have permission to access the artifact (ID: {0})", artifact.Id);
            AssertJsonResponseEquals(expectedMessage, ex.RestResponse.Content,
                "If called by a user without permission to the artifact, we should get an error message of '{0}'!", expectedMessage);
        }

        [TestCase]
        [TestRail(154704)]
        [Description("Create & save (but don't publish) an artifact, GetArtifactDetails with a different user.  Verify it returns 404 Not Found.")]
        public void GetArtifactDetails_UnpublishedArtifactOtherUser_404NotFound()
        {
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Process);
            IUser user2 = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDetails(user2, artifact.Id);
            }, "'GET {0}' should return 404 Not Found when passed an unpublished artifact ID with a different user!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);
        }

        [TestCase(0)]
        [TestCase(int.MaxValue)]
        [TestRail(154705)]
        [Description("GetArtifactDetails and pass a non-existent Artifact ID (ex. 0 or MaxInt).  Verify it returns 404 Not Found.")]
        public void GetArtifactDetails_NonExistentArtifactId_404NotFound(int artifactId)
        {
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDetails(_user, artifactId);
            }, "'GET {0}' should return 404 Not Found when passed an artifact ID that doesn't exist!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);
        }

        [TestCase(0)]
        [TestCase(2)]
        [TestRail(166149)]
        [Description("Create & publish an artifact.  GetArtifactDetails and pass a non-existent Version ID (ex. 0 or 2).  Verify it returns 404 Not Found.")]
        public void GetArtifactDetailsWithVersion_NonExistentVersionId_404NotFound(int versionId)
        {
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);

            var ex = Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id, versionId);
            }, "'GET {0}' should return 404 Not Found when passed an artifact ID that doesn't exist!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            const string expectedMessage = "You have attempted to access an item that does not exist or you do not have permission to view.";
            AssertJsonResponseEquals(expectedMessage, ex.RestResponse.Content,
                "If called by a user without permission to the artifact, we should get an error message of '{0}'!", expectedMessage);
        }

        #region Private functions.

        /// <summary>
        /// Asserts that the returned JSON content has the specified error message.
        /// </summary>
        /// <param name="expectedMessage">The error message expected in the JSON content.</param>
        /// <param name="jsonContent">The JSON content.</param>
        /// <param name="assertMessage">The message to display if the expected message isn't found in the JSON content.</param>
        /// <param name="assertMessageParams">(optional) Parameters to use if assertMessage is a format string.</param>
        private static void AssertJsonResponseEquals(string expectedMessage, string jsonContent, string assertMessage, params object[] assertMessageParams)
        {
            ThrowIf.ArgumentNull(assertMessage, nameof(assertMessage));

            JsonSerializerSettings jsonSettings = new JsonSerializerSettings()
            {
                // This will alert us if new properties are added to the return JSON format.
                MissingMemberHandling = MissingMemberHandling.Error
            };

            MessageResult messageResult = JsonConvert.DeserializeObject<MessageResult>(jsonContent, jsonSettings);

            Assert.AreEqual(expectedMessage, messageResult.Message, assertMessage, assertMessageParams);
        }

        /// <summary>
        /// Creates inline trace text for the provided artifact. For use with RTF properties.
        /// </summary>
        /// <param name="inlineTraceArtifact">target artifact for inline traces</param>
        /// <param name="inlineTraceArtifactDetails">target artifactDetails for inline traces</param>
        /// <returns>inline trace text</returns>
        private static string CreateArtifactInlineTraceValue(IArtifact inlineTraceArtifact, INovaArtifactDetails inlineTraceArtifactDetails)
        {
            string inlineTraceText = null;

            inlineTraceText = I18NHelper.FormatInvariant("<html><head></head><body style=\"padding: 1px 0px 0px; font-family: 'Portable User Interface'; font-size: 10.67px\"><div style=\"padding: 0px\"><p style=\"margin: 0px\">&#x200b;<a linkassemblyqualifiedname=\"BluePrintSys.RC.Client.SL.RichText.RichTextArtifactLink, BluePrintSys.RC.Client.SL.RichText, Version=7.4.0.0, Culture=neutral, PublicKeyToken=null\" canclick=\"True\" isvalid=\"True\" href=\"{0}?ArtifactId={1}\" target=\"_blank\" artifactid=\"{1}\" style=\"font-family: 'Portable User Interface'; font-size: 11px; font-style: normal; font-weight: normal; text-decoration: underline; color: #0000FF\" title=\"Project: akim_project\"><span style=\"font-family: 'Portable User Interface'; font-size: 11px; font-style: normal; font-weight: normal; text-decoration: underline; color: #0000FF\">{2}{1}: {3}</span></a><span style=\"-c1-editable: true; font-family: 'Portable User Interface'; font-size: 10.67px; font-style: normal; font-weight: normal; color: Black\">&#x200b;</span></p></div></body></html>",
                inlineTraceArtifact.Address, inlineTraceArtifact.Id, inlineTraceArtifactDetails.Prefix, inlineTraceArtifactDetails.Name);

            return inlineTraceText;
        }

        #endregion Private functions.
    }
}