using System;
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
using NUnit.Framework;
using TestCommon;
using Utilities;
using System.Linq;
using Common;
using Utilities.Factories;

namespace ArtifactStoreTests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]    // Ignore for now.
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
            if (_wrappedArtifact != null)
            {
                Assert.DoesNotThrow(() => Helper.ArtifactStore.DiscardArtifacts(artifacts: null, user: _wrappedArtifact.CreatedBy, all: true),
                    "Failed to discard the copied artifact(s)!");
                _wrappedArtifact = null;
            }

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

            NovaArtifactDetails sourceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, sourceArtifact.Id);

            // Execute:
            CopyNovaArtifactResultSet copyResult = null;

            Assert.DoesNotThrow(() => copyResult = CopyArtifactAndWrap(sourceArtifact, targetArtifact.Id, author),
                "'POST {0}' should return 201 Created when valid parameters are passed.", SVC_PATH);

            // Verify:
            AssertCopiedArtifactPropertiesAreIdenticalToOriginal(sourceArtifactDetails, copyResult, author, expectedVersionOfOriginalArtifact: -1);
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
            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var parentArtifact = Helper.CreateAndPublishArtifact(_project, author, parentArtifactType);
            var sourceArtifact = Helper.CreateAndPublishArtifact(_project, author, sourceArtifactType, parentArtifact, numberOfVersions: 2);

            NovaArtifactDetails sourceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, sourceArtifact.Id);

            // Execute:
            CopyNovaArtifactResultSet copyResult = null;

            Assert.DoesNotThrow(() => copyResult = CopyArtifactAndWrap(sourceArtifact, _project.Id, author),
                "'POST {0}' should return 201 Created when valid parameters are passed.", SVC_PATH);

            // Verify:
            AssertCopiedArtifactPropertiesAreIdenticalToOriginal(sourceArtifactDetails, copyResult, author, expectedVersionOfOriginalArtifact: 2);
        }

        [TestCase(BaseArtifactType.Actor, BaseArtifactType.Glossary)]
        [TestRail(195425)]
        [Description("Create and publish a source & parent artifact (source should have 2 published versions).  Copy the source artifact into the same parent.  " +
            "Verify the source artifact is unchanged and the new artifact is identical to the source artifact.  New copied artifact should not be published.")]
        public void CopyArtifact_SinglePublishedChildArtifact_ToSameParent_ReturnsNewArtifact(BaseArtifactType sourceArtifactType, BaseArtifactType parentArtifactType)
        {
            // Setup:
            var parentArtifact = Helper.CreateAndPublishArtifact(_project, _user, parentArtifactType);
            var sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, sourceArtifactType, parentArtifact, numberOfVersions: 2);

            NovaArtifactDetails sourceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, sourceArtifact.Id);

            // Execute:
            CopyNovaArtifactResultSet copyResult = null;

            Assert.DoesNotThrow(() => copyResult = CopyArtifactAndWrap(sourceArtifact, parentArtifact.Id, _user),
                "'POST {0}' should return 201 Created when valid parameters are passed.", SVC_PATH);

            // Verify:
            AssertCopiedArtifactPropertiesAreIdenticalToOriginal(sourceArtifactDetails, copyResult, _user, expectedVersionOfOriginalArtifact: 2);
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(195426)]
        [Description("Create and publish a source artifact.  Copy the source artifact into itself (as a child of itself).  " +
            "Verify the source artifact is unchanged and the new artifact is identical to the source artifact.  New copied artifact should not be published.")]
        public void CopyArtifact_SinglePublishedArtifact_ToItself_ReturnsNewArtifact(BaseArtifactType sourceArtifactType)
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, sourceArtifactType);

            NovaArtifactDetails sourceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, sourceArtifact.Id);

            // Execute:
            CopyNovaArtifactResultSet copyResult = null;

            Assert.DoesNotThrow(() => copyResult = CopyArtifactAndWrap(sourceArtifact, sourceArtifact.Id, _user),
                "'POST {0}' should return 201 Created when valid parameters are passed.", SVC_PATH);

            // Verify:
            AssertCopiedArtifactPropertiesAreIdenticalToOriginal(sourceArtifactDetails, copyResult, _user);
        }

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
            DateTime defaultExpireTime = DateTime.Now.AddDays(2);   //Currently Nova set ExpireTime 2 days from today for newly uploaded file
            var novaAttachmentFile = FileStoreTestHelper.UploadNovaFileToFileStore(author, attachmentFile.FileName, attachmentFile.FileType,
                defaultExpireTime, Helper.FileStore);
            ArtifactStoreHelper.AddArtifactAttachmentAndSave(author, sourceArtifact, novaAttachmentFile, Helper.ArtifactStore);

            int expectedVersionOfOriginalArtifact = 1;

            if (shouldPublishAttachment)
            {
                sourceArtifact.Publish();
                ++expectedVersionOfOriginalArtifact;
            }

            NovaArtifactDetails sourceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, sourceArtifact.Id);

            // Execute:
            CopyNovaArtifactResultSet copyResult = null;

            Assert.DoesNotThrow(() => copyResult = CopyArtifactAndWrap(sourceArtifact, targetArtifact.Id, author),
                "'POST {0}' should return 201 Created when valid parameters are passed.", SVC_PATH);

            // Verify:
            AssertCopiedArtifactPropertiesAreIdenticalToOriginal(sourceArtifactDetails, copyResult, author, expectedVersionOfOriginalArtifact: expectedVersionOfOriginalArtifact);

            // Verify the attachment was copied.
            var copiedArtifactAttachments = ArtifactStore.GetAttachments(Helper.ArtifactStore.Address, copyResult.Artifact.Id, author, addDrafts: true);
            Assert.AreEqual(1, copiedArtifactAttachments.AttachedFiles.Count, "Copied artifact should have 1 attachments at this point.");
            Assert.AreEqual(attachmentFile.FileName, copiedArtifactAttachments.AttachedFiles[0].FileName, "Filename of copied artifact attachment must have expected value.");
            Assert.AreEqual(0, copiedArtifactAttachments.DocumentReferences.Count, "Copied artifact shouldn't have any Document References.");

            var sourceArtifactAttachments = Helper.ArtifactStore.GetAttachments(sourceArtifact, author, addDrafts: true);
            Assert.AreEqual(1, sourceArtifactAttachments.AttachedFiles.Count, "Source artifact should have 1 attachments at this point.");
            Assert.AreEqual(attachmentFile.FileName, sourceArtifactAttachments.AttachedFiles[0].FileName, "Filename of source artifact attachment must have expected value.");
            Assert.AreEqual(0, sourceArtifactAttachments.DocumentReferences.Count, "Source artifact shouldn't have any Document References.");

            // Nova copy does a shallow copy of attachments, so sourceArtifactAttachments should equal copiedArtifactAttachments.
            AttachedFile.AssertEquals(sourceArtifactAttachments.AttachedFiles[0], copiedArtifactAttachments.AttachedFiles[0],
                skipAttachmentIds: true, skipUploadedDates: true);

            // Compare file contents.
            var fileFromCopy = Helper.ArtifactStore.GetAttachmentFile(author, copyResult.Artifact.Id,
                copiedArtifactAttachments.AttachedFiles[0].AttachmentId);
            var fileFromSource = Helper.ArtifactStore.GetAttachmentFile(author, sourceArtifact.Id,
                sourceArtifactAttachments.AttachedFiles[0].AttachmentId);

            FileStoreTestHelper.AssertFilesAreIdentical(fileFromSource, fileFromCopy);
        }

        [TestCase(BaseArtifactType.Actor, TraceDirection.From, false, false)]
        [TestCase(BaseArtifactType.Glossary, TraceDirection.To, true, false)]
        [TestCase(BaseArtifactType.TextualRequirement, TraceDirection.TwoWay, true, true)]
        [TestRail(191050)]
        [Description("Create & save an artifact then create & publish a folder.  Add a manual trace between the artifact & folder.  Copy the artifact into the folder.  " +
            "Verify the source artifact is unchanged and the new artifact (and trace) is identical to the source artifact.  New copied artifact should not be published.")]
        public void CopyArtifact_SingleSavedArtifactWithManualTrace_ToNewFolder_ReturnsNewArtifactWithManualTrace(
            BaseArtifactType artifactType, TraceDirection direction, bool isSuspect, bool shouldPublishTrace)
        {
            // Setup:
            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var sourceArtifact = Helper.CreateAndSaveArtifact(_project, author, artifactType);
            var targetArtifact = Helper.CreateAndPublishArtifact(_project, author, BaseArtifactType.PrimitiveFolder);

            // Create & add manual trace to the source artifact:
            ArtifactStoreHelper.UpdateManualArtifactTraceAndSave(author, sourceArtifact, targetArtifact, ChangeType.Create,
                Helper.ArtifactStore, direction, isSuspect);

            int expectedVersionOfOriginalArtifact = -1;

            if (shouldPublishTrace)
            {
                sourceArtifact.Publish();
                expectedVersionOfOriginalArtifact = 1;
            }

            NovaArtifactDetails sourceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, sourceArtifact.Id);

            // Execute:
            CopyNovaArtifactResultSet copyResult = null;

            Assert.DoesNotThrow(() => copyResult = CopyArtifactAndWrap(sourceArtifact, targetArtifact.Id, author),
                "'POST {0}' should return 201 Created when valid parameters are passed.", SVC_PATH);

            // Verify:
            AssertCopiedArtifactPropertiesAreIdenticalToOriginal(sourceArtifactDetails, copyResult, author, expectedVersionOfOriginalArtifact: expectedVersionOfOriginalArtifact);

            // Get traces & compare.
            Relationships sourceRelationships = ArtifactStore.GetRelationships(Helper.ArtifactStore.Address, author, copyResult.Artifact.Id, addDrafts: true);
            Relationships targetRelationships = Helper.ArtifactStore.GetRelationships(author, targetArtifact, addDrafts: true);

            Assert.AreEqual(1, sourceRelationships.ManualTraces.Count, "Copied artifact should have 1 manual trace.");
            Assert.AreEqual(2, targetRelationships.ManualTraces.Count, "Target artifact should have 2 manual traces.");

            ArtifactStoreHelper.ValidateTrace(sourceRelationships.ManualTraces[0], targetArtifact);
            ArtifactStoreHelper.ValidateTrace(targetRelationships.ManualTraces[0], sourceArtifact);
        }

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
            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, customDataProject);

            var targetFolder = Helper.CreateAndPublishArtifact(customDataProject, author, BaseArtifactType.PrimitiveFolder);
            var preCreatedArtifact = ArtifactFactory.CreateOpenApiArtifact(customDataProject, author, artifactType, artifactId, name: artifactName);

            // Verify preCreatedArtifact is Reused.
            var sourceBeforeCopy = preCreatedArtifact.GetArtifact(customDataProject, author,
                getTraces: OpenApiArtifact.ArtifactTraceType.Reuse);

            var reuseTracesBefore = sourceBeforeCopy.Traces.FindAll(t => t.TraceType == OpenApiTraceTypes.Reuse);
            Assert.NotNull(reuseTracesBefore, "No Reuse traces were found in the reused artifact before the copy!");

            NovaArtifactDetails sourceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, preCreatedArtifact.Id);

            // Execute:
            CopyNovaArtifactResultSet copyResult = null;

            Assert.DoesNotThrow(() => copyResult = CopyArtifactAndWrap(preCreatedArtifact, targetFolder.Id, author),
                "'POST {0}' should return 201 Created when valid parameters are passed.", SVC_PATH);

            // Verify:
            const int expectedVersionOfOriginalArtifact = 2;    // The pre-created artifacts in use here have 2 versions.
            AssertCopiedArtifactPropertiesAreIdenticalToOriginal(sourceArtifactDetails, copyResult, author, skipCreatedBy: true, expectedVersionOfOriginalArtifact: expectedVersionOfOriginalArtifact);

            // Verify Reuse traces of source artifact didn't change.
            var sourceAfterCopy = preCreatedArtifact.GetArtifact(customDataProject, _user,
                getTraces: OpenApiArtifact.ArtifactTraceType.Reuse);

            var reuseTracesAfter = sourceAfterCopy.Traces.FindAll(t => t.TraceType == OpenApiTraceTypes.Reuse);
            Assert.NotNull(reuseTracesAfter, "No Reuse traces were found in the reused artifact after the copy!");

            CompareTwoOpenApiTraceLists(reuseTracesBefore, reuseTracesAfter);

            // Verify the copied artifact has no Reuse traces.
            var copiedArtifact = ArtifactFactory.CreateOpenApiArtifact(customDataProject, _user, artifactType, copyResult.Artifact.Id, name: artifactName);

            // Verify preCreatedArtifact is Reused.
            var reuseTracesOfCopy = copiedArtifact.GetArtifact(customDataProject, author,
                getTraces: OpenApiArtifact.ArtifactTraceType.Reuse);
            Assert.IsEmpty(reuseTracesOfCopy.Traces, "There should be no Reuse traces on the copied artifact!");
        }
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]    // Ignore for now.
        [Category(Categories.CustomData)]
        [TestCase(ItemTypePredefined.Actor, PropertyPrimitiveType.Choice, "Std-Choice-Required-AllowMultiple-DefaultValue", "Blue")]
        [TestCase(ItemTypePredefined.Actor, PropertyPrimitiveType.Date,   "Std-Date-Required-Validated-Min-Max-HasDefault", 60*60*24 + 3600 + 60 + 13)]   // Add now + 1 day, 1 hour, 1 min & 13 seconds.
        [TestCase(ItemTypePredefined.Actor, PropertyPrimitiveType.Number, "Std-Number-Required-Validated-DecPlaces-Min-Max-HasDefault", 4.2)]
        [TestCase(ItemTypePredefined.Actor, PropertyPrimitiveType.Text,   "Std-Text-Required-RT-Multi-HasDefault", "This is the new text")]
        [TestCase(ItemTypePredefined.Actor, PropertyPrimitiveType.User,   "Std-User-Required-HasDefault-User", "")] // newValue not used here, so pass empty string.
        [TestRail(191052)]
        [Description("Create and publish an artifact (that has custom properties) and a folder.  Copy the artifact into the folder.  Verify the source artifact is unchanged " +
            "and the new artifact is identical to the source artifact (including custom properties).  New copied artifact should not be published.")]
        public void CopyArtifact_SinglePublishedArtifactWithCustomProperties_ToNewFolder_ReturnsNewArtifactWithCustomProperties<T>(
            ItemTypePredefined itemType, PropertyPrimitiveType propertyType, string propertyName, T newValue)
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            var artifactTypeName = ArtifactStoreHelper.GetStandardPackArtifactTypeName(itemType);
            var sourceArtifact = Helper.CreateWrapAndSaveNovaArtifact(_project, _user, itemType, artifactTypeName: artifactTypeName);
            var targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.PrimitiveFolder);

            // Add custom properties to the source artifact.
            NovaArtifactDetails sourceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, sourceArtifact.Id);
            CustomProperty property = null;

            if (propertyType == PropertyPrimitiveType.User)
            {
                property = ArtifactStoreHelper.UpdateCustomProperty(sourceArtifactDetails, _project, propertyType, propertyName, _user);
            }
            else
            {
                property = ArtifactStoreHelper.UpdateCustomProperty(sourceArtifactDetails, _project, propertyType, propertyName, newValue);
            }

            // Execute:
            sourceArtifact.Lock();
            Helper.ArtifactStore.UpdateArtifact(_user, _project, sourceArtifactDetails);
            Helper.ArtifactStore.PublishArtifact(sourceArtifact, _user);

            sourceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, sourceArtifact.Id);

            // Execute:
            CopyNovaArtifactResultSet copyResult = null;

            Assert.DoesNotThrow(() => copyResult = CopyArtifactAndWrap(sourceArtifact, targetArtifact.Id, _user),
                "'POST {0}' should return 201 Created when valid parameters are passed.", SVC_PATH);

            // Verify:
            AssertCopiedArtifactPropertiesAreIdenticalToOriginal(sourceArtifactDetails, copyResult, _user);

            NovaArtifactDetails artifactDetailsAfter = Helper.ArtifactStore.GetArtifactDetails(_user, sourceArtifact.Id);
            CustomProperty returnedProperty = artifactDetailsAfter.CustomPropertyValues.Find(p => p.Name == propertyName);
            ArtifactStoreHelper.AssertCustomPropertiesAreEqual(property, returnedProperty);
        }

        [TestCase(BaseArtifactType.Document, 3, 1, 0.1)]
        [TestCase(BaseArtifactType.Glossary, 3, 1, 2)]
        [TestCase(BaseArtifactType.TextualRequirement, 3, 1, 999)]
        [TestRail(195485)]
        [Description("Create and save several artifacts in a folder.  Copy one of the artifacts under the same folder and specify a positive OrderIndex.  " +
            "Verify the source artifact is unchanged and the new artifact is identical to the source artifact.  New copied artifact should not be published " +
            "and has the OrderIndex that was specified.")]
        public void CopyArtifactWithOrderIndex_SingleSavedArtifact_ToSameFolderWithArtifacts_ReturnsNewArtifactWithSpecifiedOrderIndex(
            BaseArtifactType sourceArtifactType, int numberOfArtifacts, int sourceArtifactIndex, double orderIndex)
        {
            // Setup:
            var parentFolder = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.PrimitiveFolder);
            var artifacts = Helper.CreateAndSaveMultipleArtifacts(_project, _user, sourceArtifactType, numberOfArtifacts, parentFolder);
            var sourceArtifact = artifacts[sourceArtifactIndex];

            NovaArtifactDetails sourceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, sourceArtifact.Id);

            // Execute:
            CopyNovaArtifactResultSet copyResult = null;

            Assert.DoesNotThrow(() => copyResult = CopyArtifactAndWrap(sourceArtifact, parentFolder.Id, _user, orderIndex),
                "'POST {0}' should return 201 Created when valid parameters are passed.", SVC_PATH);

            // Verify:
            AssertCopiedArtifactPropertiesAreIdenticalToOriginal(sourceArtifactDetails, copyResult, _user, expectedVersionOfOriginalArtifact: -1);

            Assert.AreEqual(orderIndex, copyResult.Artifact.OrderIndex, "The OrderIndex of the copied artifact should be: {0}", orderIndex);
        }

        [TestCase(BaseArtifactType.Document, 3, 1, 0.1)]
        [TestCase(BaseArtifactType.Glossary, 3, 1, 2)]
        [TestCase(BaseArtifactType.TextualRequirement, 3, 1, 999)]
        [TestRail(195486)]
        [Description("Create and save several artifacts with children in a folder.  Copy one of the artifacts under the same folder and specify a positive OrderIndex.  " +
            "Verify the source artifact is unchanged and the new artifact is identical to the source artifact.  New copied artifact should not be published " +
            "and has the OrderIndex that was specified.")]
        public void CopyArtifactWithOrderIndex_MultiplePublishedArtifacts_ToSameFolderWithArtifacts_ReturnsNewArtifactWithSpecifiedOrderIndex(
            BaseArtifactType sourceArtifactType, int numberOfArtifacts, int sourceArtifactIndex, double orderIndex)
        {
            // Setup:
            var parentFolder = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.PrimitiveFolder);
            var artifacts = new List<List<IArtifact>>();

            for (int i = 0; i < numberOfArtifacts; ++i)
            {
                var artifactTypeChain = new BaseArtifactType[] { sourceArtifactType, sourceArtifactType, sourceArtifactType };
                var artifactChain = Helper.CreatePublishedArtifactChain(_project, _user, artifactTypeChain);
                artifacts.Add(artifactChain);
            }

            var sourceArtifact = artifacts[sourceArtifactIndex][0];

            NovaArtifactDetails sourceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, sourceArtifact.Id);

            // Execute:
            CopyNovaArtifactResultSet copyResult = null;

            Assert.DoesNotThrow(() => copyResult = CopyArtifactAndWrap(sourceArtifact, parentFolder.Id, _user, orderIndex),
                "'POST {0}' should return 201 Created when valid parameters are passed.", SVC_PATH);

            // Verify:
            AssertCopiedArtifactPropertiesAreIdenticalToOriginal(sourceArtifactDetails, copyResult, _user,
                expectedNumberOfArtifactsCopied: numberOfArtifacts);

            Assert.AreEqual(orderIndex, copyResult.Artifact.OrderIndex, "The OrderIndex of the copied artifact should be: {0}", orderIndex);

            VerifyChildrenWereCopied(sourceArtifactDetails, copyResult.Artifact);
        }

        // TODO ---------------- POSITIVE TESTS
        // TODO - Copy artifact (possibly with descendants) to one of its child
        // TODO - Copy orphan artifact

        #endregion 201 Created tests

        #region 400 Bad Request tests

        [TestCase(-1.1)]
        [TestCase(0)]
        [TestRail(191207)]
        [Description("Create & save an artifact.  Copy the artifact and specify an OrderIndex <= 0.  Verify 400 Bad Request is returned.")]
        public void CopyArtifact_SavedArtifact_NotPositiveOrderIndex_400BadRequest(double orderIndex)
        {
            // Setup:
            IArtifact sourceArtifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Process);

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
                ArtifactStore.CopyArtifact(Helper.ArtifactStore.Address, sourceArtifact.Id, _project.Id, _user, orderIndex),
                "'POST {0}?orderIndex={1}' should return 400 Bad Request for non-positive OrderIndex values", SVC_PATH, orderIndex);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.IncorrectInputParameters,
                "Parameter orderIndex cannot be equal to or less than 0.");
        }

        #endregion 400 Bad Request tests

        #region 401 Unauthorized tests

        [TestCase(BaseArtifactType.Process)]
        [TestRail(191209)]
        [Description("Create & publish two artifacts.  Copy one artifact to be a child of the other with invalid token in a request.  Verify response returns code 401 Unauthorized.")]
        public void CopyArtifact_PublishedArtifact_ToParentArtifactWithInvalidToken_401Unauthorized(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            IArtifact newParentArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            IUser userWithBadToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                ArtifactStore.CopyArtifact(Helper.ArtifactStore.Address, sourceArtifact.Id, newParentArtifact.Id, userWithBadToken);
            }, "'POST {0}' should return 401 Unauthorized when called with an invalid token!", SVC_PATH);

            // Verify:
            const string expectedExceptionMessage = "Unauthorized call";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                    "{0} was not found in returned message of copy published artifact which has no token in a header.", expectedExceptionMessage);
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(191225)]
        [Description("Create & publish an artifact.  Copy an artifact with call that does not have token in a header.  Verify response returns code 401 Unauthorized.")]
        public void CopyArtifact_PublishedArtifact_NoTokenInAHeader_401Unauthorized(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                ArtifactStore.CopyArtifact(Helper.ArtifactStore.Address, sourceArtifact.Id, _project.Id, user: null);
            }, "'POST {0}' should return 401 Unauthorized when called with no token in a header!", SVC_PATH);

            // Verify:
            const string expectedExceptionMessage = "Unauthorized call";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "{0} was not found in returned message of copy published artifact which has no token in a header.", expectedExceptionMessage);
        }

        #endregion 401 Unauthorized tests

        #region 403 Forbidden tests

        [TestCase(BaseArtifactType.Process)]
        [TestRail(195358)]
        [Description("Create & publish two artifacts. User does not have edit permissions to target artifact.  Copy source artifact to be a child of the target artifact.  " +
            "Verify returned code 403 Forbidden.")]
        public void CopyArtifact_PublishedArtifact_ToNewParent_NoEditPermissionsToTargetArtifact_403Forbidden(BaseArtifactType artifactType)
        { 
            // Setup:
            IArtifact newParentArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            IUser user = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
            Helper.AssignProjectRolePermissionsToUser(user, TestHelper.ProjectRole.Viewer, _project, newParentArtifact);

            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(_project, user, artifactType);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.CopyArtifact(sourceArtifact, newParentArtifact, user),
                "'POST {0}' should return 403 Forbidden when user tries to copy an artifact to be a child of another artifact to which he/she has viewer permissions only", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "You do not have permissions to copy the artifact in the selected location.");
        }

        [TestCase(BaseArtifactType.Process)]
        [TestCase(BaseArtifactType.PrimitiveFolder)]
        [TestRail(192079)]
        [Description("Create & publish two artifacts.  Each one in different project.  Copy an artifact to be a child of the other artifact in different project.  " +
            "Verify returned code 403 Forbidden.")]
        public void CopyArtifact_PublishedArtifact_ToAnotherArtifactInDifferentProject_403Forbidden(BaseArtifactType artifactType)
        {
            // Setup:
            var projects = ProjectFactory.GetProjects(_user, numberOfProjects: 2);

            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(projects.First(), _user, artifactType);
            IArtifact newParentArtifact = Helper.CreateAndPublishArtifact(projects.Last(), _user, artifactType);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.CopyArtifact(sourceArtifact, newParentArtifact, _user),
                "'POST {0}' should return 403 Forbidden when user tries to copy an artifact to a different project", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot copy artifacts to a different project.");
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(192080)]
        [Description("Create & save folder and artifact.  Copy a folder to be a child of an artifact.  Verify returned code 403 Forbidden.")]
        public void CopyArtifact_SavedArtifact_FolderToBeAChildOfArtifact_403Forbidden(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _user, artifactType);
            IArtifact folder = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.PrimitiveFolder);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.CopyArtifact(folder, artifact, _user),

                "'POST {0}' should return 403 Forbidden when user tries to copy a folder to be a child of a regular artifact", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot copy a folder artifact to non folder/project parent.");
        }

        [TestCase(BaselineAndCollectionTypePredefined.ArtifactCollection)]
        [TestCase(BaselineAndCollectionTypePredefined.CollectionFolder)]
        [TestRail(192081)]
        [Description("Create collection or collection folder. Copy regular artifact to be a child of the collection or collection folder. Verify returned code 403 Forbidden.")]
        public void CopyArtifact_PublishedArtifact_ToCollectionOrCollectionFolder_403Forbidden(BaselineAndCollectionTypePredefined artifactType)
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            IArtifact collection = Helper.CreateCollectionOrCollectionFolder(_project, _user, artifactType);

            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.CopyArtifact(artifact, collection, _user),
               "'POST {0}' should return 403 Forbidden when user tries to copy a regular artifact to a {1} artifact type", SVC_PATH, artifactType);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot copy artifacts outside of the artifact section.");
        }

        [Category(Execute.Weekly)]
        [TestCase(BaselineAndCollectionTypePredefined.ArtifactCollection)]
        [TestCase(BaselineAndCollectionTypePredefined.CollectionFolder)]
        [TestRail(192082)]
        [Description("Create & save a collection or collection folder and a regular artifact. Copy collection or collection folder to be a child of the regular artifact.  " + 
            "Verify returned code 403 Forbidden.")]
        public void CopyArtifact_CollectionOrCollectionFolder_ToRegularArtifact_403Forbidden(BaselineAndCollectionTypePredefined artifactType)
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            var collection = Helper.CreateCollectionOrCollectionFolder(_project, _user, artifactType);

            IArtifact parentArtifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.PrimitiveFolder);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.CopyArtifact(collection, parentArtifact, _user),
                   "'POST {0}' should return 403 Forbidden when user tries to copy a collection or collection folder to be a child of a regular artifact", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot copy artifacts that are not from the artifact section.");
        }

        [Category(Execute.Weekly)] 
        [TestCase(BaselineAndCollectionTypePredefined.ArtifactCollection)]
        [TestCase(BaselineAndCollectionTypePredefined.CollectionFolder)]
        [TestRail(192083)]
        [Description("Create a collection or collection folder. Copy a collection or collection folder to be a child of a collection artifact. Verify returned code 403 Forbidden.")]
        public void CopyArtifact_CollectionOrCollectionFolder_ToCollectionArtifact_403Forbidden(BaselineAndCollectionTypePredefined artifactType)
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            var sourceCollection = Helper.CreateCollectionOrCollectionFolder(_project, _user, artifactType);

            IArtifact targetCollection = Helper.CreateAndPublishCollectionFolder(_project, _user);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.CopyArtifact(sourceCollection, targetCollection, _user),
                   "'POST {0}' should return 403 Forbidden when user tries to copy collection or collection folder to collection artifact", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot copy artifacts that are not from the artifact section.");
        }

        #endregion 403 Forbidden tests

        #region 404 Not Found tests

        [TestCase(BaseArtifactType.Actor, 0)]
        [TestCase(BaseArtifactType.Actor, int.MaxValue)]
        [TestRail(195411)]
        [Description("Create & save an artifact. Copy an artifact to be a child of the non existing artifact.  Verify returned code 404 Not Found.")]
        public void CopyArtifact_SavedArtifact_ToNonExistingArtifact_404NotFound(BaseArtifactType artifactType, int nonExistingArtifactId)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _user, artifactType);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => ArtifactStore.CopyArtifact(Helper.ArtifactStore.Address, artifact.Id, nonExistingArtifactId, _user),
                "'POST {0}' should return 404 Not Found when user tries to copy artifact to be a child of non existing artifact", SVC_PATH);

            // Verify:
            if (nonExistingArtifactId > 0)
            {
                ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.NotFound,
                    I18NHelper.FormatInvariant("Artifact where to copy with ID {0} is not found.", nonExistingArtifactId));
            }
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(195412)]
        [Description("Create & publish two artifacts.  Delete second artifact.  Copy first artifact to be a child of deleted artifact.  Verify returned code 404 Not Found.")]
        public void CopyArtifact_PublishedArtifact_ToDeletedArtifact_404NotFound(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            targetArtifact.Delete();
            targetArtifact.Publish();

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.CopyArtifact(sourceArtifact, targetArtifact, _user),
                "'POST {0}' should return 404 Not Found when user tries to copy artifact to be a child of artifact that was removed", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.NotFound,
                I18NHelper.FormatInvariant("Artifact where to copy with ID {0} is not found.", targetArtifact.Id));
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(195413)]
        [Description("Create & publish two artifacts.  Delete first artifact.  Copy deleted artifact to be a child of second artifact.  Verify returned code 404 Not Found.")]
        public void CopyArtifact_SavedDeletedArtifacts_ToSavedArtifact_404NotFound(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact sourceArtifact = Helper.CreateAndSaveArtifact(_project, _user, artifactType);
            IArtifact targetArtifact = Helper.CreateAndSaveArtifact(_project, _user, artifactType);

            sourceArtifact.Delete();

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.CopyArtifact(sourceArtifact, targetArtifact, _user),
                "'POST {0}' should return 404 Not Found when user tries to copy deleted artifact to be a child of another artifact", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.NotFound,
                I18NHelper.FormatInvariant("Artifact to copy with ID {0} is not found.", sourceArtifact.Id));
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(195414)]
        [Description("Create & publish two artifacts.  Copy an artifact to be a child of the other one with user that does not have proper permissions " +
            "to source artifact.  Verify returned code 404 Not Found.")]
        public void CopyArtifact_PublishedArtifacts_ForUserWithoutProperPermissionsToSource_404NotFound(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            var userWithoutPermissions = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
            Helper.AssignProjectRolePermissionsToUser(userWithoutPermissions, TestHelper.ProjectRole.None, _project, sourceArtifact);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.CopyArtifact(sourceArtifact, targetArtifact, userWithoutPermissions),
                "'POST {0}' should return 404 Not Found when user tries to copy artifact without proper permissions", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.NotFound,
                I18NHelper.FormatInvariant("Artifact to copy with ID {0} is not found.", sourceArtifact.Id));
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(195415)]
        [Description("Create & publish two artifacts.  Copy an artifact to be a child of the other one with user that does not have proper permissions " +
            "to target artifact.  Verify returned code 404 Not Found.")]
        public void CopyArtifact_PublishedArtifacts_ForUserWithoutProperPermissionsToTarget_404NotFound(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            var userWithoutPermissions = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
            Helper.AssignProjectRolePermissionsToUser(userWithoutPermissions, TestHelper.ProjectRole.None, _project, targetArtifact);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.CopyArtifact(sourceArtifact, targetArtifact, userWithoutPermissions),
                "'POST {0}' should return 404 Not Found when user tries to copy artifact without proper permissions", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.NotFound,
                I18NHelper.FormatInvariant("Artifact where to copy with ID {0} is not found.", targetArtifact.Id));
        }

        [TestCase(BaseArtifactType.Process)]
        [TestCase(BaseArtifactType.PrimitiveFolder)]
        [TestRail(192085)]
        [Description("Create & publish two artifacts with sub-artifacts.  Copy an artifact to be a child of another artifact sub-artifact.  Verify returned code 404 Not Found.")]
        public void CopyArtifact_SavedArtifact_ToSubArtifact_404NotFound(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact sourceArtifact = Helper.CreateAndSaveArtifact(_project, _user, artifactType);
            IArtifact targetArtifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Process);

            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, targetArtifact.Id);

            Assert.IsNotNull(subArtifacts, "This artifact does not have sub-artifacts!");
            Assert.IsNotEmpty(subArtifacts, "This artifact does not have sub-artifacts!");

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => ArtifactStore.CopyArtifact(Helper.ArtifactStore.Address, sourceArtifact.Id, subArtifacts.First().Id, _user),
                "'POST {0}' should return 404 Not Found when user tries to copy an artifact to be a child of a sub-artifact", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.NotFound,
                I18NHelper.FormatInvariant("Artifact where to copy with ID {0} is not found.", subArtifacts.First().Id));
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(192086)]
        [Description("Create & publish two artifacts with sub-artifacts.  Copy a sub-artifact to be a child of another artifact.  Verify returned code 404 Not Found.")]
        public void CopyArtifact_PublishedSubArtifact_ToArtifact_404NotFound(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, sourceArtifact.Id);

            Assert.IsNotNull(subArtifacts, "This artifact does not have sub-artifacts!");
            Assert.IsNotEmpty(subArtifacts, "This artifact does not have sub-artifacts!");

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => ArtifactStore.CopyArtifact(Helper.ArtifactStore.Address, subArtifacts.First().Id, targetArtifact.Id, _user),
                "'POST {0}' should return 404 Not Found when user tries to copy a sub-artifact to be a child of another artifact", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.NotFound,
                I18NHelper.FormatInvariant("Artifact to copy with ID {0} is not found.", subArtifacts.First().Id));
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestCase(BaseArtifactType.PrimitiveFolder)]
        [TestRail(192092)]
        [Description("Create & publish an artifact.  Copy a project to be a child of the artifact.  Verify returned code 404 Not Found.")]
        public void CopyArtifact_PublishedArtifact_ProjectToArtifact_404NotFound(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => ArtifactStore.CopyArtifact(Helper.ArtifactStore.Address, _project.Id, artifact.Id, _user),
                "'POST {0}' should return 404 Not Found when user tries to copy a project to be a child of an artifact", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.NotFound,
                I18NHelper.FormatInvariant("Artifact to copy with ID {0} is not found.", _project.Id));

        }

        #endregion 404 Not Found tests

        #region Private functions

        /// <summary>
        /// Asserts that the properties of the copied artifact are the same as the original artifact (except Id and Version)
        /// and that the the expected number of files were copied.
        /// </summary>
        /// <param name="originalArtifact">The original artifact that was copied.</param>
        /// <param name="copyResult">The result returned from the Nova copy call.</param>
        /// <param name="user">The user to use for getting artifact details.</param>
        /// <param name="expectedNumberOfArtifactsCopied">(optional) The number of artifacts that were expected to be copied.</param>
        /// <param name="expectedVersionOfOriginalArtifact">(optional) The expected version of the original artifact.</param>
        /// <param name="skipCreatedBy">(optional) Pass true to skip comparison of the CreatedBy properties.</param>
        /// <exception cref="AssertionException">If any expectations failed.</exception>
        private void AssertCopiedArtifactPropertiesAreIdenticalToOriginal(INovaArtifactDetails originalArtifact,
            CopyNovaArtifactResultSet copyResult,
            IUser user,
            int expectedNumberOfArtifactsCopied = 1,
            int expectedVersionOfOriginalArtifact = 1,
            bool skipCreatedBy = false)
        {
            Assert.NotNull(copyResult, "The result returned from CopyArtifact() shouldn't be null!");
            Assert.NotNull(copyResult.Artifact, "The Artifact property returned by CopyArtifact() shouldn't be null!");
            Assert.AreEqual(-1, copyResult.Artifact.Version, "Version of a copied artifact should always be -1 (i.e. not published)!");
            Assert.AreEqual(expectedNumberOfArtifactsCopied, copyResult.CopiedArtifactsCount,
                "There should be exactly {0} artifact copied, but the result reports {1} artifacts were copied.",
                expectedNumberOfArtifactsCopied, copyResult.CopiedArtifactsCount);
            Assert.AreNotEqual(originalArtifact.Id, copyResult.Artifact.Id,
                "The ID of the copied artifact should not be the same as the original artifact!");

            // We need to skip comparison of a lot of properties because the copy has different Id & Parent, and isn't published...
            ArtifactStoreHelper.AssertArtifactsEqual(originalArtifact, copyResult.Artifact,
                skipIdAndVersion: true, skipParentId: true, skipOrderIndex: true, skipCreatedBy: skipCreatedBy, skipPublishedProperties: true);

            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(user, copyResult.Artifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, copyResult.Artifact);
            Assert.AreEqual(-1, artifactDetails.Version, "Version of copied artifact should be -1 (i.e. not published)!");

            // Make sure original artifact didn't change.
            var originalArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(user, originalArtifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(originalArtifactDetails, originalArtifact, skipIdAndVersion: true);
            Assert.AreEqual(expectedVersionOfOriginalArtifact, originalArtifactDetails.Version,
                "The Version of the original artifact shouldn't have changed after the copy!");
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
            var copyResult = ArtifactStore.CopyArtifact(Helper.ArtifactStore.Address, artifact.Id, newParentId, user, orderIndex, expectedStatusCodes);

            if (copyResult?.Artifact != null)
            {
                IProject project = _projects.Find(p => p.Id == copyResult.Artifact.ProjectId);

                _wrappedArtifact = Helper.WrapNovaArtifact(copyResult.Artifact, project, user, artifact.BaseArtifactType);
            }

            return copyResult;
        }

        /// <summary>
        /// Veifies that all the children of the source artifact were copied to the target.
        /// </summary>
        /// <param name="sourceArtifact">The source artifact.</param>
        /// <param name="copiedArtifact">The copied artifact.</param>
        private void VerifyChildrenWereCopied(INovaArtifactBase sourceArtifact, INovaArtifactBase copiedArtifact)
        {
            ThrowIf.ArgumentNull(sourceArtifact, nameof(sourceArtifact));
            ThrowIf.ArgumentNull(copiedArtifact, nameof(copiedArtifact));
            Assert.AreEqual(copiedArtifact.Name, sourceArtifact.Name, "The wrong source artifact was provided.");

            var sourceChildren = Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(sourceArtifact.ProjectId.Value,
                sourceArtifact.Id, _user);
            var copiedChildren = Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(copiedArtifact.ProjectId.Value,
                copiedArtifact.Id, _user);

            Assert.AreEqual(sourceChildren.Count, copiedChildren.Count, "The number of artifacts copied is incorrect!");

            for (int i = 0; i < sourceChildren.Count; ++i)
            {
                var sourceChild = sourceChildren[i];
                var copiedChild = copiedChildren[i];

                sourceChild.AssertEquals(copiedChild,
                    skipIdAndVersion: true, skipParentId: true, skipOrderIndex: true, skipPublishedProperties: true);

                // Recursively verify all children below this one.
                if (sourceChild.HasChildren)
                {
                    VerifyChildrenWereCopied(sourceChild, copiedChild);
                }
            }
        }

        #endregion Private functions
    }
}
