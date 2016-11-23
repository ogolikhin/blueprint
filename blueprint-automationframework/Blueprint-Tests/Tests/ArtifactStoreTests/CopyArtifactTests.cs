using System.Collections.Generic;
using System.Net;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.Impl;
using Newtonsoft.Json;
using NUnit.Framework;
using TestCommon;
using Utilities;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class CopyArtifactTests : TestBase
    {
        private const string SVC_PATH = RestPaths.Svc.ArtifactStore.Artifacts_id_.COPY_TO_id_;

        private IUser _user = null;
        private IProject _project = null;
        private List<IProject> _projects = null;
        private IArtifact _wrappedArtifact = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _projects = ProjectFactory.GetAllProjects(_user, shouldRetrievePropertyTypes: true);
            _project = _projects[0];
        }

        [TearDown]
        public void TearDown()
        {
            _wrappedArtifact?.Delete();
            Helper?.Dispose();
        }

        #region 201 Created tests

        [TestCase(BaseArtifactType.Actor, BaseArtifactType.Glossary)]
        [TestCase(BaseArtifactType.Document, BaseArtifactType.Actor)]
        [TestCase(BaseArtifactType.Glossary, BaseArtifactType.TextualRequirement)]
        [TestCase(BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder)]  // Folders can only be children of other folders.
        [TestCase(BaseArtifactType.TextualRequirement, BaseArtifactType.Document)]
        [TestRail(191047)]
        [Description("Create and save a source & destination artifact.  Copy the source artifact under the destination artifact.  Verify the source artifact is unchanged " +
            "and the new artifact is identical to the source artifact.  New copied artifact should not be published.")]
        public void CopyArtifact_SingleSavedArtifact_ToNewParent_ReturnsNewArtifact(BaseArtifactType sourceArtifactType, BaseArtifactType targetArtifactType)
        {
            // Setup:
            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var sourceArtifact = Helper.CreateAndSaveArtifact(_project, author, sourceArtifactType);
            var targetArtifact = Helper.CreateAndSaveArtifact(_project, author, targetArtifactType);

            // Execute:
            CopyNovaArtifactResultSet copyResult = null;

            Assert.DoesNotThrow(() => copyResult = CopyArtifactAndWrap(sourceArtifact, targetArtifact.Id, author),
                "'POST {0}' should return 201 Created when valid parameters are passed.", SVC_PATH);

            // Verify:
            AssertCopiedArtifactPropertiesAreIdenticalToOriginal(sourceArtifact, copyResult, author);
        }
        
        [TestCase(BaseArtifactType.Actor, BaseArtifactType.Glossary)]
        [TestCase(BaseArtifactType.Document, BaseArtifactType.Actor)]
        [TestCase(BaseArtifactType.Glossary, BaseArtifactType.TextualRequirement)]
        [TestCase(BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder)]  // Folders can only be children of other folders.
        [TestCase(BaseArtifactType.TextualRequirement, BaseArtifactType.Document)]
        [TestRail(191048)]
        [Description("Create and publish a source & parent artifact (source should have 2 published versions).  Copy the source artifact into the project root.  " +
            "Verify the source artifact is unchanged and the new artifact is identical to the source artifact.  New copied artifact should not be published.")]
        public void CopyArtifact_SinglePublishedChildArtifact_ToProjectRoot_ReturnsNewArtifact(BaseArtifactType sourceArtifactType, BaseArtifactType parentArtifactType)
        {
            // Setup:
            var parentArtifact = Helper.CreateAndPublishArtifact(_project, _user, parentArtifactType);
            var sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, sourceArtifactType, parentArtifact, numberOfVersions: 2);

            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            // Execute:
            CopyNovaArtifactResultSet copyResult = null;

            Assert.DoesNotThrow(() => copyResult = CopyArtifactAndWrap(sourceArtifact, _project.Id, author),
                "'POST {0}' should return 201 Created when valid parameters are passed.", SVC_PATH);

            // Verify:
            AssertCopiedArtifactPropertiesAreIdenticalToOriginal(sourceArtifact, copyResult, author);
        }
        
        [Explicit(IgnoreReasons.UnderDevelopment)]  // Returns 400 Bad Request.
        [TestCase(BaseArtifactType.Actor, false)]
        [TestCase(BaseArtifactType.TextualRequirement, true)]
        [TestRail(191049)]
        [Description("Create & publish an artifact then create & save a folder.  Add an attachment to the artifact.  Copy the artifact into the folder.  " +
            "Verify the source artifact is unchanged and the new artifact is identical to the source artifact.  New copied artifact should not be published.")]
        public void CopyArtifact_SinglePublishedArtifactWithAttachment_ToNewSavedFolder_ReturnsNewArtifactWithAttachment(
            BaseArtifactType artifactType, bool shouldPublishAttachment)
        {
            // Setup:
            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var sourceArtifact = Helper.CreateAndPublishArtifact(_project, author, artifactType);
            var targetArtifact = Helper.CreateAndSaveArtifact(_project, author, BaseArtifactType.PrimitiveFolder);

            // Create & add attachment to the source artifact:
            var attachmentFile = FileStoreTestHelper.CreateNovaFileWithRandomByteArray();
            ArtifactStoreHelper.AddArtifactAttachmentAndSave(author, sourceArtifact, attachmentFile, Helper.ArtifactStore);

            if (shouldPublishAttachment)
            {
                sourceArtifact.Publish();
            }

            // Execute:
            CopyNovaArtifactResultSet copyResult = null;

            Assert.DoesNotThrow(() => copyResult = CopyArtifactAndWrap(sourceArtifact, targetArtifact.Id, author),
                "'POST {0}' should return 201 Created when valid parameters are passed.", SVC_PATH);

            // Verify:
            AssertCopiedArtifactPropertiesAreIdenticalToOriginal(sourceArtifact, copyResult, author);

            // Verify the attachment was copied.
            var copiedArtifactAttachments = Helper.ArtifactStore.GetAttachments(sourceArtifact, author, addDrafts: true);
            Assert.AreEqual(1, copiedArtifactAttachments.AttachedFiles.Count, "Copied artifact should have 1 attachments at this point.");
            Assert.AreEqual(attachmentFile.FileName, copiedArtifactAttachments.AttachedFiles[0].FileName, "Filename of copied artifact attachment must have expected value.");
            Assert.AreEqual(0, copiedArtifactAttachments.DocumentReferences.Count, "Copied artifact shouldn't have any Document References.");

            var sourceArtifactAttachments = ArtifactStore.GetAttachments(Helper.ArtifactStore.Address, copyResult.Artifact.Id, author, addDrafts: true);
            Assert.AreEqual(1, sourceArtifactAttachments.AttachedFiles.Count, "Source artifact should have 1 attachments at this point.");
            Assert.AreEqual(attachmentFile.FileName, sourceArtifactAttachments.AttachedFiles[0].FileName, "Filename of source artifact attachment must have expected value.");
            Assert.AreEqual(0, sourceArtifactAttachments.DocumentReferences.Count, "Source artifact shouldn't have any Document References.");

            // Nova copy does a shallow copy of attachments, so sourceArtifactAttachments should equal copiedArtifactAttachments.
            AttachedFile.AssertEquals(sourceArtifactAttachments.AttachedFiles[0], copiedArtifactAttachments.AttachedFiles[0]);

            // TODO: Get the file contents and compare.
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]  // Returns 500 error "You do not have permission to edit the artifact".
        [TestCase(BaseArtifactType.Actor, TraceDirection.From, false, false)]
        [TestCase(BaseArtifactType.Glossary, TraceDirection.To, true, false)]
        [TestCase(BaseArtifactType.TextualRequirement, TraceDirection.TwoWay, true, true)]
        [TestRail(191050)]
        [Description("Create & save an artifact then create & publish a folder.  Add a manual trace between the artifact & folder.  Copy the artifact into the folder.  " +
            "Verify the source artifact is unchanged and the new artifact (and trace) is identical to the source artifact.  New copied artifact should not be published.")]
        public void CopyArtifact_SinglePublishedArtifactWithManualTrace_ToNewFolder_ReturnsNewArtifactWithManualTrace(
            BaseArtifactType artifactType, TraceDirection direction, bool isSuspect, bool shouldPublishTrace)
        {
            // Setup:
            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var sourceArtifact = Helper.CreateAndSaveArtifact(_project, author, artifactType);
            var targetArtifact = Helper.CreateAndPublishArtifact(_project, author, BaseArtifactType.PrimitiveFolder);

            // Create & add manual trace to the source artifact:
            ArtifactStoreHelper.UpdateManualArtifactTraceAndSave(author, sourceArtifact, targetArtifact, ChangeType.Create,
                Helper.ArtifactStore, direction, isSuspect);

            if (shouldPublishTrace)
            {
                sourceArtifact.Publish();
            }

            // Execute:
            CopyNovaArtifactResultSet copyResult = null;

            Assert.DoesNotThrow(() => copyResult = CopyArtifactAndWrap(sourceArtifact, targetArtifact.Id, author),
                "'POST {0}' should return 201 Created when valid parameters are passed.", SVC_PATH);

            // Verify:
            AssertCopiedArtifactPropertiesAreIdenticalToOriginal(sourceArtifact, copyResult, author);

            // Get traces & compare.
            Relationships sourceRelationships = ArtifactStore.GetRelationships(Helper.ArtifactStore.Address, author, copyResult.Artifact.Id, addDrafts: true);
            Relationships targetRelationships = Helper.ArtifactStore.GetRelationships(author, targetArtifact, addDrafts: true);

            Assert.AreEqual(1, sourceRelationships.ManualTraces.Count, "Copied artifact should have 1 manual trace.");
            Assert.AreEqual(2, targetRelationships.ManualTraces.Count, "Target artifact should have 2 manual traces.");

            ArtifactStoreHelper.ValidateTrace(sourceRelationships.ManualTraces[0], targetArtifact);
            ArtifactStoreHelper.ValidateTrace(targetRelationships.ManualTraces[0], sourceArtifact);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]  // Returns 500 error "You do not have permission to edit the artifact".
        [Category(Categories.CustomData)]
        [Category(Categories.GoldenData)]
        [TestCase(BaseArtifactType.TextualRequirement, 85, "User Story[reuse source]")]
        [TestCase(BaseArtifactType.TextualRequirement, 86, "User Story[reuse target]")]
        [TestRail(191051)]
        [Description("Create and publish a folder.  Copy a reused artifact into the folder.  Verify the source artifact is unchanged and the new artifact " +
            "is identical to the source artifact (except no Reuse relationship).  New copied artifact should not be published.")]
        public void CopyArtifact_SinglePublishedReusedArtifact_ToNewFolder_ReturnsNewArtifactNotReused(BaseArtifactType artifactType, int artifactId, string artifactName)
        {
            // Setup:
            IProject customDataProject = ArtifactStoreHelper.GetCustomDataProject(_user);

            var targetFolder = Helper.CreateAndPublishArtifact(customDataProject, _user, BaseArtifactType.PrimitiveFolder);
            var preCreatedArtifact = ArtifactFactory.CreateOpenApiArtifact(customDataProject, _user, artifactType, artifactId, name: artifactName);

            // Verify preCreatedArtifact is Reused.
            var sourceBeforeCopy = preCreatedArtifact.GetArtifact(customDataProject, _user,
                getTraces: OpenApiArtifact.ArtifactTraceType.Reuse);

            var reuseTracesBefore = sourceBeforeCopy.Traces.FindAll(t => t.TraceType == OpenApiTraceTypes.Reuse);
            Assert.NotNull(reuseTracesBefore, "No Reuse traces were found in the reused artifact before the copy!");

            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, customDataProject);

            // Execute:
            CopyNovaArtifactResultSet copyResult = null;

            Assert.DoesNotThrow(() => copyResult = CopyArtifactAndWrap(preCreatedArtifact, targetFolder.Id, author),
                "'POST {0}' should return 201 Created when valid parameters are passed.", SVC_PATH);

            // Verify:
            AssertCopiedArtifactPropertiesAreIdenticalToOriginal(preCreatedArtifact, copyResult, author);

            // Verify Reuse traces of source artifact didn't change.
            var sourceAfterCopy = preCreatedArtifact.GetArtifact(customDataProject, _user,
                getTraces: OpenApiArtifact.ArtifactTraceType.Reuse);

            var reuseTracesAfter = sourceAfterCopy.Traces.FindAll(t => t.TraceType == OpenApiTraceTypes.Reuse);
            Assert.NotNull(reuseTracesAfter, "No Reuse traces were found in the reused artifact after the copy!");

            CompareTwoOpenApiTraceLists(reuseTracesBefore, reuseTracesAfter);

            // Verify the copied artifact has no Reuse traces.
            var copiedArtifact = ArtifactFactory.CreateOpenApiArtifact(customDataProject, _user, artifactType, artifactId, name: artifactName);

            // Verify preCreatedArtifact is Reused.
            var reuseTracesOfCopy = copiedArtifact.GetArtifact(customDataProject, _user,
                getTraces: OpenApiArtifact.ArtifactTraceType.Reuse);
            Assert.IsNull(reuseTracesOfCopy, "There should be no Reuse traces on the copied artifact!");
        }
        /*
        [Category(Categories.CustomData)]
        [TestCase(BaseArtifactType.Actor)]
        [TestRail(191052)]
        [Description("Create and publish an artifact (that has custom properties) and a folder.  Copy the artifact into the folder.  Verify the source artifact is unchanged " +
            "and the new artifact is identical to the source artifact (including custom properties).  New copied artifact should not be published.")]
        public void CopyArtifact_SinglePublishedArtifactWithCustomProperties_ToNewFolder_ReturnsNewArtifactWithCustomProperties(BaseArtifactType artifactType)
        {
            Assert.Fail("Test not implemented yet.");
        }
        */
        #endregion 201 Created tests

        // TODO ---------------- POSITIVE TESTS
        // TODO - Copy artifact to itself
        // TODO - Copy artifact (possibly with decendants) to one of its child

        // TODO ---------------- NEGATIVE TESTS
        // TODO - Copy artifact to sub-artifact
        // TODO - Copy collection to another collection 

        #region 400 Bad Request tests

        [TestCase(-1.1)]
        [TestCase(0)]
        [TestRail(191207)]
        [Description("Create & save an artifact.  Copy the artifact and specify an OrderIndex <= 0.  Verify 400 Bad Request is returned.")]
        public void CopyArtifact_SavedArtifact_NotPositiveOrderIndex_400BadRequest(double orderIndex)
        {
            // Setup:
            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            IArtifact sourceArtifact = Helper.CreateAndSaveArtifact(_project, author, BaseArtifactType.Process);

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
                ArtifactStore.CopyArtifact(Helper.ArtifactStore.Address, sourceArtifact, _project.Id, author, orderIndex),
                "'POST {0}?orderIndex={1}' should return 400 Bad Request for non-positive OrderIndex values", SVC_PATH, orderIndex);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.IncorrectInputParameters,
                "Parameter orderIndex cannot be equal to or less than 0.");
        }

        [TestCase(ItemTypePredefined.ArtifactCollection, -0.0001)]
        [TestCase(ItemTypePredefined.CollectionFolder, 0)]
        [TestRail(191208)]
        [Description("Create & save a Collection or Collection Folder artifact.  Copy the Collection or Collection Folder and specify an OrderIndex <= 0.  " +
            "Verify 400 Bad Request is returned.")]
        public void CopyArtifact_SavedCollectionOrCollectionFolder_NotPositiveOrderIndex__400BadRequest(
            ItemTypePredefined artifactType, double orderIndex)
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var collectionFolder = _project.GetDefaultCollectionFolder(Helper.ArtifactStore.Address, author);
            var fakeBaseType = BaseArtifactType.PrimitiveFolder;
            IArtifact sourceArtifact = Helper.CreateWrapAndSaveNovaArtifact(_project, author, artifactType, collectionFolder.Id, baseType: fakeBaseType);

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
                ArtifactStore.CopyArtifact(Helper.ArtifactStore.Address, sourceArtifact, _project.Id, author, orderIndex),
                "'POST {0}?orderIndex={1}' should return 400 Bad Request for non-positive OrderIndex values", SVC_PATH, orderIndex);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.IncorrectInputParameters,
                "Parameter orderIndex cannot be equal to or less than 0.");
        }

        #endregion 400 Bad Request tests

        #region 401 Unauthorized tests

        [TestCase(BaseArtifactType.Process)]
        [TestRail(191225)]
        [Description("Create & publish an artifact.  Copy an artifact with call that does not have token in a header.  Verify response returns code 401 Unauthorized.")]
        public void CopyArtifact_PublishedArtifact_CopyWithNoTokenInAHeader_401Unauthorized(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                ArtifactStore.CopyArtifact(Helper.ArtifactStore.Address, sourceArtifact, _project.Id, user : null);
            }, "'POST {0}' should return 401 Unauthorized when called with no token in a header!", SVC_PATH);

            // Verify:
            string errorMessage = JsonConvert.DeserializeObject<string>(ex.RestResponse.Content);
            Assert.AreEqual("Unauthorized call", errorMessage);
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(191209)]
        [Description("Create & publish two artifacts.  Copy one artifact to be a child of the other with invalid token in a request.  Verify response returns code 401 Unauthorized.")]
        public void CopyArtifact_PublishedArtifact_CopyToParentArtifactWithInvalidToken_401Unauthorized(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            IArtifact newParentArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            IUser userWithBadToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                ArtifactStore.CopyArtifact(Helper.ArtifactStore.Address, sourceArtifact, newParentArtifact.Id, userWithBadToken);
            }, "'POST {0}' should return 401 Unauthorized when called with an invalid token!", SVC_PATH);

            // Verify:
            string errorMessage = JsonConvert.DeserializeObject<string>(ex.RestResponse.Content);
            Assert.AreEqual("Unauthorized call", errorMessage);
        }

        #endregion 401 Unauthorized tests

        #region Private functions

        /// <summary>
        /// Asserts that the properties of the copied artifact are the same as the original artifact (except Id and Version)
        /// and that the the expected number of files were copied.
        /// </summary>
        /// <param name="originalArtifact">The original artifact that was copied.</param>
        /// <param name="copyResult">The result returned from the Nova copy call.</param>
        /// <param name="user">The user to use for getting artifact details.</param>
        /// <param name="expectedNumberOfArtifactsCopied">(optional) The number of artifacts that were expected to be copied.</param>
        /// <exception cref="AssertionException">If any expectations failed.</exception>
        private void AssertCopiedArtifactPropertiesAreIdenticalToOriginal(IArtifactBase originalArtifact,
            CopyNovaArtifactResultSet copyResult,
            IUser user,
            int expectedNumberOfArtifactsCopied = 1)
        {
            Assert.NotNull(copyResult, "The result returned from CopyArtifact() shouldn't be null!");
            Assert.NotNull(copyResult.Artifact, "The Artifact property returned by CopyArtifact() shouldn't be null!");
            Assert.AreEqual(-1, copyResult.Artifact.Version, "Version of a copied artifact should always be -1 (i.e. not published)!");
            Assert.AreEqual(expectedNumberOfArtifactsCopied, copyResult.CopiedArtifactsCount,
                "There should be exactly {0} artifact copied, but the result reports {1} artifacts were copied.",
                expectedNumberOfArtifactsCopied, copyResult.CopiedArtifactsCount);
            Assert.AreNotEqual(originalArtifact.Id, copyResult.Artifact.Id,
                "The ID of the copied artifact should not be the same as the original artifact!");

            ArtifactStoreHelper.AssertArtifactsEqual(originalArtifact, copyResult.Artifact, skipIdAndVersion: true, skipParentIds: true);

            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(user, copyResult.Artifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, copyResult.Artifact);
        }

        /// <summary>
        /// Compares two lists of OpenApiTrace's and asserts they are equal.
        /// </summary>
        /// <param name="expectedTraces">The list of expected OpenApiTrace's.</param>
        /// <param name="actualTraces">The list of actual OpenApiTrace's.</param>
        /// <exception cref="AssertionException">If any OpenApiTrace properties don't match between the two lists.</exception>
        private static void CompareTwoOpenApiTraceLists(List<OpenApiTrace> expectedTraces, List<OpenApiTrace> actualTraces)
        {
            ThrowIf.ArgumentNull(expectedTraces, nameof(expectedTraces));
            ThrowIf.ArgumentNull(actualTraces, nameof(actualTraces));

            Assert.AreEqual(expectedTraces.Count, actualTraces.Count, "The number of traces are different!");

            foreach (var expectedTrace in expectedTraces)
            {
                var actualTrace = actualTraces.Find(t => (t.TraceType == expectedTrace.TraceType) && (t.ArtifactId == expectedTrace.ArtifactId));
                Assert.NotNull(actualTrace, "Couldn't find actual trace type '{0}' with ArtifactId: {1}",
                    expectedTrace.TraceType, expectedTrace.ArtifactId);

                OpenApiTrace.AssertAreEqual(expectedTrace, actualTrace);
            }
        }

        /// <summary>
        /// Copies the specified artifact to the new parent, wraps it in an IArtifact that gets disposed automatically,
        /// and returns the result of the CopyArtifact call.
        /// </summary>
        /// <param name="artifact">The artifact to copy.</param>
        /// <param name="newParentId">The Id of the new parent where this artifact will be copied to.</param>
        /// <param name="user">(optional) The user to authenticate with.  By default it uses the user that created the artifact.</param>
        /// <param name="orderIndex">(optional) The order index (relative to other artifacts) where this artifact should be copied to.
        ///     By default the artifact is copied to the end (after the last artifact).</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 201 Created is expected.</param>
        /// <returns>The details of the artifact that we copied and the number of artifacts copied.</returns>
        private CopyNovaArtifactResultSet CopyArtifactAndWrap(
            IArtifactBase artifact,
            int newParentId,
            IUser user = null,
            double? orderIndex = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            var copyResult = ArtifactStore.CopyArtifact(Helper.ArtifactStore.Address, artifact, newParentId, user, orderIndex, expectedStatusCodes);

            if (copyResult?.Artifact != null)
            {
                IProject project = _projects.Find(p => p.Id == copyResult.Artifact.ProjectId);

                _wrappedArtifact = Helper.WrapNovaArtifact(copyResult.Artifact, project, user, artifact.BaseArtifactType);
            }

            return copyResult;
        }

        #endregion Private functions
    }
}
