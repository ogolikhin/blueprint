﻿using System.Collections.Generic;
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
using Model.StorytellerModel;
using Model.StorytellerModel.Impl;
using System;

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

        private IArtifact WrappedArtifact
        {
            get { return _wrappedArtifacts.FirstOrDefault(); }
        }
        private List<IArtifact> _wrappedArtifacts = new List<IArtifact>();

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
            if (_wrappedArtifacts.Any())
            {
                Assert.DoesNotThrow(() => Helper.ArtifactStore.DiscardArtifacts(artifacts: null, user: WrappedArtifact.CreatedBy, all: true),
                    "Failed to discard the copied artifact(s)!");
                _wrappedArtifacts.Clear();
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

            var sourceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, sourceArtifact.Id);

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

            var sourceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, sourceArtifact.Id);

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

            var sourceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, sourceArtifact.Id);

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

            var sourceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, sourceArtifact.Id);

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
        [Description("Create & publish an artifact then create & save a folder.  Add an attachment to the artifact (save or publish).  Copy the artifact into the folder.  " +
            "Verify the source artifact is unchanged and the new artifact is identical to the source artifact.  New copied artifact should not be published.")]
        public void CopyArtifact_SinglePublishedArtifactWithAttachment_ToNewSavedFolder_ReturnsNewArtifactWithAttachment(
            BaseArtifactType artifactType, bool shouldPublishAttachment)
        {
            // Setup:
            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            // Create & add attachment to the source artifact:
            var attachmentFile = FileStoreTestHelper.CreateNovaFileWithRandomByteArray();
            var sourceArtifact = ArtifactStoreHelper.CreateArtifactWithAttachment(Helper, _project, author, artifactType, attachmentFile, shouldPublishArtifact: true);
            var targetArtifact = Helper.CreateAndSaveArtifact(_project, author, BaseArtifactType.PrimitiveFolder);

            int expectedVersionOfOriginalArtifact = 1;

            if (shouldPublishAttachment)
            {
                sourceArtifact.Publish();
                ++expectedVersionOfOriginalArtifact;
            }

            var sourceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, sourceArtifact.Id);

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

            // A new attachment reference is created in the copy which has different AttachmentId & UploadedDate, so we need to exclude those when comparing.
            Attachments.CompareOptions compareOptions = new Attachments.CompareOptions
            {
                CompareAttachmentIds = false,
                CompareUploadedDates = false
            };

            // Nova copy does a shallow copy of attachments, so sourceArtifactAttachments should equal copiedArtifactAttachments.
            AttachedFile.AssertAreEqual(sourceArtifactAttachments.AttachedFiles[0], copiedArtifactAttachments.AttachedFiles[0],
                compareOptions: compareOptions);

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

            var sourceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, sourceArtifact.Id);

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

        [TestCase(BaseArtifactType.TextualRequirement, TraceDirection.TwoWay, true)]
        [TestRail(195487)]
        [Description("Create & save an artifact and create & publish a folder.   Publish a manual trace between the artifact & folder.  Create user that does not have " +
            "trace permissions and copy the artifact into the folder.  Verify the source artifact is unchanged and the new artifact is identical to the source" +
            "artifact, except no Manual Trace should exist.  New copied artifact should not be published.")]
        public void CopyArtifact_SinglePublishedArtifactWithManualTrace_ToNewFolder_NoTracePermissions_ReturnsNewArtifactWithoutManualTrace(
            BaseArtifactType artifactType, TraceDirection direction, bool isSuspect)
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndSaveArtifact(_project, _user, artifactType);
            var targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.PrimitiveFolder);

            // Create & add manual trace to the source artifact:
            ArtifactStoreHelper.UpdateManualArtifactTraceAndSave(_user, sourceArtifact, targetArtifact, ChangeType.Create,
                Helper.ArtifactStore, direction, isSuspect);

            sourceArtifact.Publish();

            var sourceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, sourceArtifact.Id);

            // Execute:
            IUser userNoTracePermission = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.None, _project);
            Helper.AssignProjectRolePermissionsToUser(userNoTracePermission,
                        RolePermissions.Edit |
                        RolePermissions.CanReport |
                        RolePermissions.Comment |
                        RolePermissions.Delete |
                        RolePermissions.DeleteAnyComment |
                        RolePermissions.CreateRapidReview |
                        RolePermissions.ExcelUpdate |
                        RolePermissions.Read |
                        RolePermissions.Reuse |
                        RolePermissions.Share,
                        _project);

            CopyNovaArtifactResultSet copyResult = null;

            Assert.DoesNotThrow(() => copyResult = CopyArtifactAndWrap(sourceArtifact, targetArtifact.Id, userNoTracePermission),
                "'POST {0}' should return 201 Created when valid parameters are passed.", SVC_PATH);

            // Verify:
            AssertCopiedArtifactPropertiesAreIdenticalToOriginal(sourceArtifactDetails, copyResult, userNoTracePermission, skipCreatedBy: true, skipPermissions: true);

            // Get traces & compare.
            Relationships copyRelationships = ArtifactStore.GetRelationships(Helper.ArtifactStore.Address, userNoTracePermission, copyResult.Artifact.Id, addDrafts: true);
            Relationships targetRelationships = Helper.ArtifactStore.GetRelationships(_user, targetArtifact, addDrafts: true);

            Assert.AreEqual(0, copyRelationships.ManualTraces.Count, "Copied artifact should have no manual traces.");

            // Verify that source and target artifacts still have the same traces they had before the copy.
            Assert.AreEqual(1, targetRelationships.ManualTraces.Count, "Target artifact should have 1 manual trace.");
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

            var sourceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, preCreatedArtifact.Id);

            // Execute:
            CopyNovaArtifactResultSet copyResult = null;

            Assert.DoesNotThrow(() => copyResult = CopyArtifactAndWrap(preCreatedArtifact, targetFolder.Id, author),
                "'POST {0}' should return 201 Created when valid parameters are passed.", SVC_PATH);

            // Verify:
            const int expectedVersionOfOriginalArtifact = 2;    // The pre-created artifacts in use here have 2 versions.
            AssertCopiedArtifactPropertiesAreIdenticalToOriginal(sourceArtifactDetails, copyResult, author,
                skipCreatedBy: true, expectedVersionOfOriginalArtifact: expectedVersionOfOriginalArtifact);

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
        [TestCase(ItemTypePredefined.Actor, PropertyPrimitiveType.Date,   "Std-Date-Required-Validated-Min-Max-HasDefault", "2016-12-24T00:00:00")]
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
            var sourceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, sourceArtifact.Id);
            CustomProperty property = null;

            if (propertyType == PropertyPrimitiveType.User)
            {
                property = ArtifactStoreHelper.UpdateArtifactCustomProperty(sourceArtifactDetails, _project, propertyType, propertyName, _user);
            }
            else
            {
                property = ArtifactStoreHelper.UpdateArtifactCustomProperty(sourceArtifactDetails, _project, propertyType, propertyName, newValue);
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

            var artifactDetailsAfter = Helper.ArtifactStore.GetArtifactDetails(_user, sourceArtifact.Id);
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

            var sourceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, sourceArtifact.Id);

            // Execute:
            CopyNovaArtifactResultSet copyResult = null;

            Assert.DoesNotThrow(() => copyResult = CopyArtifactAndWrap(sourceArtifact, parentFolder.Id, _user, orderIndex),
                "'POST {0}?orderIndex={1}' should return 201 Created when valid parameters are passed.", SVC_PATH, orderIndex);

            // Verify:
            AssertCopiedArtifactPropertiesAreIdenticalToOriginal(sourceArtifactDetails, copyResult, _user, expectedVersionOfOriginalArtifact: -1);

            Assert.AreEqual(orderIndex, copyResult.Artifact.OrderIndex, "The OrderIndex of the copied artifact should be: {0}", orderIndex);
        }

        [TestCase(BaseArtifactType.Document, 3, 1, 0.1)]
        [TestCase(BaseArtifactType.Glossary, 3, 1, 2)]
        [TestCase(BaseArtifactType.TextualRequirement, 3, 1, 999)]
        [TestRail(195486)]
        [Description("Create and publish several artifacts with children in a folder.  Copy one of the artifacts under the same folder and specify a positive OrderIndex.  " +
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

            var sourceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, sourceArtifact.Id);

            // Execute:
            CopyNovaArtifactResultSet copyResult = null;

            Assert.DoesNotThrow(() => copyResult = CopyArtifactAndWrap(sourceArtifact, parentFolder.Id, _user, orderIndex),
                "'POST {0}?orderIndex={1}' should return 201 Created when valid parameters are passed.", SVC_PATH, orderIndex);

            // Verify:
            AssertCopiedArtifactPropertiesAreIdenticalToOriginal(sourceArtifactDetails, copyResult, _user,
                expectedNumberOfArtifactsCopied: numberOfArtifacts);

            Assert.AreEqual(orderIndex, copyResult.Artifact.OrderIndex, "The OrderIndex of the copied artifact should be: {0}", orderIndex);

            VerifyChildrenWereCopied(_user, sourceArtifactDetails, copyResult.Artifact);
        }

        [TestCase(BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder, BaseArtifactType.Document, BaseArtifactType.Glossary, BaseArtifactType.Actor)]
        [TestRail(195554)]
        [Description("Create and publish several artifacts with children.  Copy the top level artifact to be under one of its children.  " +
            "Verify the source artifact is unchanged and the new artifact is identical to the source artifact.  New copied artifact should not be published " +
            "and has the OrderIndex that was specified.")]
        public void CopyArtifact_MultiplePublishedArtifacts_ToChildOfItself_ReturnsNewArtifactWithSpecifiedOrderIndex(params BaseArtifactType[] artifactTypeChain)
        {
            // Setup:
            var artifactChain = Helper.CreatePublishedArtifactChain(_project, _user, artifactTypeChain);

            var sourceArtifact = artifactChain[0];
            var targetArtifact = artifactChain[1];

            var sourceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, sourceArtifact.Id);

            // Execute:
            CopyNovaArtifactResultSet copyResult = null;

            Assert.DoesNotThrow(() => copyResult = CopyArtifactAndWrap(sourceArtifact, targetArtifact.Id, _user),
                "'POST {0}' should return 201 Created when valid parameters are passed.", SVC_PATH);

            // Verify:
            AssertCopiedArtifactPropertiesAreIdenticalToOriginal(sourceArtifactDetails, copyResult, _user,
                expectedNumberOfArtifactsCopied: artifactChain.Count);

            VerifyChildrenWereCopied(_user, sourceArtifactDetails, copyResult.Artifact, parentWasCopiedToChild: true);
        }

        [Category(Categories.CustomData)]
        [Category(Categories.GoldenData)]
        [TestCase(BaseArtifactType.BusinessProcess, 33, "Business Process Diagram", 2)]
        [TestCase(BaseArtifactType.DomainDiagram, 31, "Domain Diagram", 2)]
        [TestCase(BaseArtifactType.GenericDiagram, 49, "Generic Diagram", 2)]
        [TestCase(BaseArtifactType.Storyboard, 32, "Storyboard", 2)]
        [TestCase(BaseArtifactType.UIMockup, 22, "UI Mockup", 4)]
        [TestCase(BaseArtifactType.UseCase, 17, "MainUseCase", 2)]
        [TestCase(BaseArtifactType.UseCaseDiagram, 29, "Use Case Diagram", 3)]
        [TestRail(195562)]
        [Description("Create & publish a destination folder.  Copy the pre-created source artifact to the destination artifact.  Verify the source artifact is " +
            "unchanged and the new artifact is identical to the source artifact.  New copied artifact should not be published.")]
        public void CopyArtifact_SinglePublishedLegacyDiagramArtifact_ToNewFolder_NewArtifactIsIdenticalToOriginal(
            BaseArtifactType artifactType, int artifactId, string artifactName, int expectedVersionOfOriginalArtifact)
        {
            // Setup:
            IProject customDataProject = ArtifactStoreHelper.GetCustomDataProject(_user);
            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, customDataProject);

            var targetFolder = Helper.CreateAndPublishArtifact(customDataProject, author, BaseArtifactType.PrimitiveFolder);
            var preCreatedArtifact = ArtifactFactory.CreateOpenApiArtifact(customDataProject, author, artifactType, artifactId, name: artifactName);

            var sourceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, preCreatedArtifact.Id);

            // Execute:
            CopyNovaArtifactResultSet copyResult = null;

            Assert.DoesNotThrow(() => copyResult = CopyArtifactAndWrap(preCreatedArtifact, targetFolder.Id, author),
                "'POST {0}' should return 201 Created when valid parameters are passed.", SVC_PATH);

            // Verify:
            AssertCopiedArtifactPropertiesAreIdenticalToOriginal(sourceArtifactDetails, copyResult, author,
                expectedVersionOfOriginalArtifact: expectedVersionOfOriginalArtifact, skipCreatedBy: true);

            // Publish the copied artifact so we can add a breakpoint and check it in the UI.
            WrappedArtifact.Publish(author);

            AssertCopiedSubArtifactsAreEqualToOriginal(author, sourceArtifactDetails, copyResult.Artifact);
        }

        [Category(Categories.CustomData)]
        [Category(Categories.GoldenData)]
        [TestCase(BaseArtifactType.PrimitiveFolder, 7, "BaseArtifacts", 1)]
        [TestRail(195567)]
        [Description("Create & publish a destination folder.  Copy a folder containing pre-created source artifacts to the destination folder.  " +
            "Verify the source artifacts are unchanged and the new artifacts are identical to the source artifacts.  New copied artifacts should not be published.")]
        public void CopyArtifact_MultiplePublishedLegacyDiagramArtifacts_ToNewFolder_NewArtifactsAreIdenticalToOriginal(
            BaseArtifactType artifactType, int artifactId, string artifactName, int expectedVersionOfOriginalArtifact)
        {
            // Setup:
            IProject customDataProject = ArtifactStoreHelper.GetCustomDataProject(_user);
            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, customDataProject);

            var targetFolder = Helper.CreateAndPublishArtifact(customDataProject, author, BaseArtifactType.PrimitiveFolder);
            var preCreatedArtifact = ArtifactFactory.CreateOpenApiArtifact(customDataProject, author, artifactType, artifactId, name: artifactName);

            var sourceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, preCreatedArtifact.Id);

            // Execute:
            CopyNovaArtifactResultSet copyResult = null;

            Assert.DoesNotThrow(() => copyResult = CopyArtifactAndWrap(preCreatedArtifact, targetFolder.Id, author),
                "'POST {0}' should return 201 Created when valid parameters are passed.", SVC_PATH);

            // Verify:
            AssertCopiedArtifactPropertiesAreIdenticalToOriginal(sourceArtifactDetails, copyResult, author,
                expectedNumberOfArtifactsCopied: 15, expectedVersionOfOriginalArtifact: expectedVersionOfOriginalArtifact, skipCreatedBy: true);

            VerifyChildrenWereCopied(author, sourceArtifactDetails, copyResult.Artifact, skipSubArtifactTraces: true);

            Attachments.CompareOptions compareOptions = new Attachments.CompareOptions
            {
                CompareAttachmentIds = false,
                CompareUploadedDates = false,
                CompareUsers = false
            };

            AssertCopiedSubArtifactsAreEqualToOriginal(author, sourceArtifactDetails, copyResult.Artifact, compareOptions: compareOptions);
        }

        [Category(Categories.CustomData)]
        [Category(Categories.GoldenData)]
        [TestCase(BaseArtifactType.BusinessProcess, 271, "Business Process Diagram", 3)]
        [TestCase(BaseArtifactType.DomainDiagram, 356, "Domain Diagram", 3)]
        [TestCase(BaseArtifactType.GenericDiagram, 310, "Generic Diagram", 3)]
        [TestCase(BaseArtifactType.Glossary, 80, "Glossary", 3)]
        [TestCase(BaseArtifactType.Process, 89, "Process", 3)]
        [TestCase(BaseArtifactType.Storyboard, 231, "Storyboard", 3)]
        [TestCase(BaseArtifactType.UIMockup, 168, "UI Mockup", 3)]
        [TestCase(BaseArtifactType.UseCase, 351, "MainUseCase", 3)]
        [TestCase(BaseArtifactType.UseCaseDiagram, 245, "Use Case Diagram", 3)]
        [TestRail(195646)]
        [Description("Create & publish a destination folder.  Copy the pre-created source artifact to the destination artifact.  Verify the source artifact is " +
            "unchanged and the new artifact is identical to the source artifact (including sub-artifact traces, attachments and document references.  " +
            "New copied artifact should not be published.")]
        public void CopyArtifact_SinglePublishedLegacyDiagramArtifactWithSubArtifactTracesAttachmentsAndDocumentReferences_ToNewFolder_NewArtifactIsIdenticalToOriginal(
            BaseArtifactType artifactType, int artifactId, string artifactName, int expectedVersionOfOriginalArtifact)
        {
            // Setup:
            IProject customDataProject = ArtifactStoreHelper.GetCustomDataProject(_user);
            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, customDataProject);

            var targetFolder = Helper.CreateAndPublishArtifact(customDataProject, author, BaseArtifactType.PrimitiveFolder);
            var preCreatedArtifact = ArtifactFactory.CreateArtifact(customDataProject, author, artifactType, artifactId, name: artifactName);

            var sourceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, preCreatedArtifact.Id);

            // Execute:
            CopyNovaArtifactResultSet copyResult = null;

            Assert.DoesNotThrow(() => copyResult = CopyArtifactAndWrap(preCreatedArtifact, targetFolder.Id, author),
                "'POST {0}' should return 201 Created when valid parameters are passed.", SVC_PATH);

            // Verify:
            AssertCopiedArtifactPropertiesAreIdenticalToOriginal(sourceArtifactDetails, copyResult, author,
                expectedVersionOfOriginalArtifact: expectedVersionOfOriginalArtifact, skipCreatedBy: true);

            // Publish the copied artifact so we can add a breakpoint and check it in the UI.
            WrappedArtifact.Publish(author);

            // A new attachment reference is created in the copy which has different AttachmentId, UploadedDate & ReferenceDate,
            // so we need to exclude those when comparing.  Also, the copy was done by a different user than the original, so we can't compare Users.
            Attachments.CompareOptions compareOptions = new Attachments.CompareOptions
            {
                CompareAttachmentIds = false,
                CompareUploadedDates = false,
                CompareReferencedDates = false,
                CompareUsers = false
            };

            AssertCopiedSubArtifactsAreEqualToOriginal(author, sourceArtifactDetails, copyResult.Artifact,
                compareOptions: compareOptions);
        }

        [TestCase]
        [TestRail(195958)]
        [Description("Create and publish a source Process with generated User Stories, and save a destination folder.  Copy the source artifact under the destination folder.  " +
            "Verify the source Process is unchanged and the new Process is identical to the source Process.  Verify the source User Stories are unchanged (except for new Traces " +
            "to the copied Process).  New copied Process & User Stories should not be published.")]
        public void CopyArtifact_SinglePublishedProcessWithUserStories_ToNewFolder_ReturnsNewProcessWithUserStories()
        {
            // Setup:
            List<IStorytellerUserStory> userStories;
            var sourceArtifact = CreateComplexProcessAndGenerateUserStories(_user, out userStories);
            var targetFolder = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.PrimitiveFolder);

            var sourceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, sourceArtifact.Id);

            // Execute:
            CopyNovaArtifactResultSet copyResult = null;

            Assert.DoesNotThrow(() => copyResult = CopyArtifactAndWrap(sourceArtifact, targetFolder.Id, _user),
                "'POST {0}' should return 201 Created when valid parameters are passed.", SVC_PATH);

            // Verify:
            int expectedNumberOfArtifactsCopied = userStories.Count + 1;
            AssertCopiedArtifactPropertiesAreIdenticalToOriginal(sourceArtifactDetails, copyResult, _user, expectedNumberOfArtifactsCopied,
                expectedVersionOfOriginalArtifact: sourceArtifactDetails.Version.Value);

            // Verify User Stories were copied.
            VerifyChildrenWereCopied(_user, sourceArtifactDetails, copyResult.Artifact, skipSubArtifactTraces: true);

            Attachments.CompareOptions compareOptions = new Attachments.CompareOptions
            {
                CompareAttachmentIds = false,
                CompareUploadedDates = false
            };

            AssertCopiedSubArtifactsAreEqualToOriginal(_user, sourceArtifactDetails, copyResult.Artifact,
                skipSubArtifactTraces: true, compareOptions: compareOptions);

            var childArtifacts = Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, copyResult.Artifact.Id, _user);

            Assert.AreEqual(userStories.Count, childArtifacts.Count, "The User Stories of the Process didn't get copied!");
        }

        [TestCase]
        [TestRail(195959)]
        [Description("Create and publish a source Process with generated User Stories, and publish a destination folder.  Copy the source artifact under the destination folder " +
            "generate User Stories from the copied Process.  Verify the source Process is unchanged and the new Process is identical to the source Process.  Verify the source " +
            "User Stories are unchanged (except for new Traces to the copied Process).  New copied Process & User Stories should not be published.")]
        public void CopyArtifact_SinglePublishedProcessWithUserStories_ToNewFolder_GenerateUserStories_NewUserStoriesAreCreated()
        {
            // Setup:
            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            List<IStorytellerUserStory> sourceUserStories;
            var sourceArtifact = CreateComplexProcessAndGenerateUserStories(author, out sourceUserStories);
            var targetFolder = Helper.CreateAndPublishArtifact(_project, author, BaseArtifactType.PrimitiveFolder);

            var sourceChildrenBefore = Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, sourceArtifact.Id, author);
            Assert.AreEqual(sourceUserStories.Count, sourceChildrenBefore.Count,
                "Wrong number of children under the source Process artifact!");

            var sourceArtifactDetailsBefore = Helper.ArtifactStore.GetArtifactDetails(author, sourceArtifact.Id);

            // Execute:
            CopyNovaArtifactResultSet copyResult = null;

            Assert.DoesNotThrow(() => copyResult = CopyArtifactAndWrap(sourceArtifact, targetFolder.Id, author),
                "'POST {0}' should return 201 Created when valid parameters are passed.", SVC_PATH);

            // Generate User Stories on the copied Process.
            var copiedProcess = Helper.Storyteller.GetProcess(author, copyResult.Artifact.Id);

            // The Process needs to be published before we can generate User Stories.
            var copiedArtifact = _wrappedArtifacts.Find(a => a.Id.Equals(copiedProcess.Id));
            copiedArtifact.Publish(author);

            var copiedUserStories = Helper.Storyteller.GenerateUserStories(author, copiedProcess, shouldDeleteChildren: false);
            Assert.NotNull(copiedUserStories, "No User Stories were generated!");
            Assert.AreEqual(3, copiedUserStories.Count, "There should be 3 User Stories generated!");

            // Verify:
            var sourceArtifactDetailsAfter = Helper.ArtifactStore.GetArtifactDetails(author, sourceArtifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(sourceArtifactDetailsBefore, sourceArtifactDetailsAfter, skipIdAndVersion: true, skipPublishedProperties: true);

            // Verify the original User Stories didn't get modified by generating User Stories on the copied Process.
            var sourceChildrenAfter = Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, sourceArtifact.Id, author);
            Assert.AreEqual(sourceChildrenBefore.Count, sourceChildrenAfter.Count, "The number of User Stories under the source Process changed!");

            for (int i = 0; i < sourceChildrenBefore.Count; ++i)
            {
                var originalChild = sourceChildrenBefore[i];
                var newChild = sourceChildrenAfter[i];

                NovaArtifact.AssertAreEqual(originalChild, newChild);
            }

            // Verify User Stories generated on source and copied Processes are equal.
            for (int i = 0; i < sourceUserStories.Count; ++i)
            {
                StorytellerUserStory.AssertAreEqual(sourceUserStories[i], copiedUserStories[i], skipIds: true);
            }

            var childArtifacts = Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, copyResult.Artifact.Id, author);

            Assert.AreEqual(sourceUserStories.Count * 2, childArtifacts.Count, "The User Stories of the Process didn't get copied!");
        }

        [TestCase]
        [TestRail(195995)]
        [Description("Create and publish a source Process with Link Labels, and save a destination folder.  Copy the source artifact under the destination folder.  " +
            "Verify the source Process is unchanged and the new Process is identical to the source Process.  Verify the Link Labels were also copied. " +
            "New copied Process should not be published.")]
        public void CopyArtifact_SinglePublishedProcessWithLinkLabels_ToNewFolder_ReturnsNewProcessWithLinkLabels()
        {
            // Setup:
            var sourceProcess = StorytellerTestHelper.CreateAndGetDefaultProcessWithUserAndSystemDecisions(
                Helper.Storyteller, _project, _user);

            sourceProcess = StorytellerTestHelper.AddRandomLinkLabelsToProcess(Helper.Storyteller, sourceProcess, _user);
            sourceProcess = StorytellerTestHelper.UpdateVerifyAndPublishProcess(sourceProcess, Helper.Storyteller, _user);

            var sourceArtifact = Helper.Storyteller.Artifacts.Find(a => a.Id.Equals(sourceProcess.Id));
            var targetFolder = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.PrimitiveFolder);

            var sourceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, sourceArtifact.Id);

            // Execute:
            CopyNovaArtifactResultSet copyResult = null;

            Assert.DoesNotThrow(() => copyResult = CopyArtifactAndWrap(sourceArtifact, targetFolder.Id, _user),
                "'POST {0}' should return 201 Created when valid parameters are passed.", SVC_PATH);

            // Verify:
            AssertCopiedArtifactPropertiesAreIdenticalToOriginal(sourceArtifactDetails, copyResult, _user,
                expectedVersionOfOriginalArtifact: sourceArtifactDetails.Version.Value);

            Attachments.CompareOptions compareOptions = new Attachments.CompareOptions
            {
                CompareAttachmentIds = false,
                CompareUploadedDates = false
            };

            AssertCopiedSubArtifactsAreEqualToOriginal(_user, sourceArtifactDetails, copyResult.Artifact,
                skipSubArtifactTraces: true, compareOptions: compareOptions);

            var copiedProcess = Helper.Storyteller.GetProcess(_user, copyResult.Artifact.Id);

            StorytellerTestHelper.AssertProcessesAreEqual(sourceProcess, copiedProcess, isCopiedProcess: true);

            // Compare the Process Links.
            for (int i = 0; i < sourceProcess.Links.Count; ++i)
            {
                Assert.AreEqual(sourceProcess.Links[i].Label, copiedProcess.Links[i].Label, "Link labels do not match!");
                Assert.AreEqual(sourceProcess.Links[i].Orderindex, copiedProcess.Links[i].Orderindex, "Link OrderIndexes do not match!");
            }
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(227354)]
        [Description("Create and publish a source artifact. Add random inline image to source artifact description. Copy the source artifact into the same parent.  " +
            "Verify the source artifact is unchanged and the new artifact is identical to the source artifact, except inline image. Copying should create new entry for inline image.")]
        public void CopyArtifact_SavedArtifactWithInlineImageInDescription_ToProjectRoot_ReturnsNewArtifact(BaseArtifactType sourceArtifactType)
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndSaveArtifact(_project, _user, sourceArtifactType);
            
            var sourceArtifactDetails = ArtifactStoreHelper.AddRandomImageToArtifactProperty(sourceArtifact, _user, Helper.ArtifactStore);
            string sourceArtifactInlineImageId = ArtifactStoreHelper.GetInlineImageId(sourceArtifactDetails.Description);
            var sourceArtifactImageFile = Helper.ArtifactStore.GetImage(_user, sourceArtifactInlineImageId);

            // Execute:
            CopyNovaArtifactResultSet copyResult = null;

            Assert.DoesNotThrow(() => copyResult = CopyArtifactAndWrap(sourceArtifact, _project.Id, _user),
                "'POST {0}' should return 201 Created when valid parameters are passed.", SVC_PATH);
            string copiedArtifactInlineImageId = ArtifactStoreHelper.GetInlineImageId(copyResult.Artifact.Description);
            var copiedArtifactImageFile = Helper.ArtifactStore.GetImage(_user, copiedArtifactInlineImageId);

            // Verify:
            Assert.AreNotEqual(sourceArtifactInlineImageId, copiedArtifactInlineImageId,
                "ImageId's for source and copied artifacts should be different.");
            AssertCopiedArtifactPropertiesAreIdenticalToOriginal(sourceArtifactDetails, copyResult, _user,
                expectedVersionOfOriginalArtifact: -1, skipDescription: true);
            FileStoreTestHelper.AssertFilesAreIdentical(sourceArtifactImageFile, copiedArtifactImageFile, compareFileNames: false);
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(227355)]
        [Description("Create and publish a source artifact. Add random inline image to source artifact description. Copy the source artifact into the same parent.  " +
            "Verify the source artifact is unchanged and the new artifact is identical to the source artifact, except inline image. Copying should create new entry for inline image.")]
        public void CopyArtifact_PublishedArtifactWithInlineImageInDescription_ToProjectRoot_ReturnsNewArtifact(BaseArtifactType sourceArtifactType)
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, sourceArtifactType);

            sourceArtifact.Lock(_user);
            var sourceArtifactDetails = ArtifactStoreHelper.AddRandomImageToArtifactProperty(sourceArtifact, _user, Helper.ArtifactStore);
            sourceArtifact.Publish(_user);

            sourceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, sourceArtifact.Id);
            int expectedVersion = 2;

            string sourceArtifactInlineImageId = ArtifactStoreHelper.GetInlineImageId(sourceArtifactDetails.Description);
            var sourceArtifactImageFile = Helper.ArtifactStore.GetImage(_user, sourceArtifactInlineImageId);

            // Execute:
            CopyNovaArtifactResultSet copyResult = null;

            Assert.DoesNotThrow(() => copyResult = CopyArtifactAndWrap(sourceArtifact, _project.Id, _user),
                "'POST {0}' should return 201 Created when valid parameters are passed.", SVC_PATH);
            string copiedArtifactEmbeddedImageId = ArtifactStoreHelper.GetInlineImageId(copyResult.Artifact.Description);

            Assert.AreNotEqual(sourceArtifactInlineImageId, copiedArtifactEmbeddedImageId,
                "ImageId in the copied artifact should be different from the ImageId from source artifact.");
            var copiedArtifactImageFile = Helper.ArtifactStore.GetImage(_user, copiedArtifactEmbeddedImageId);

            // Verify:
            AssertCopiedArtifactPropertiesAreIdenticalToOriginal(sourceArtifactDetails, copyResult, _user,
                expectedVersionOfOriginalArtifact: expectedVersion, skipDescription: true);
            FileStoreTestHelper.AssertFilesAreIdentical(sourceArtifactImageFile, copiedArtifactImageFile, compareFileNames: false);
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(227356)]
        [Description("Create and publish a source artifact. Add random inline image to source artifact description. Copy the source artifact into the same parent.  " +
            "Verify the source artifact is unchanged and the new artifact is identical to the source artifact, except inline image. Copying should create new entry for inline image.")]
        public void CopyArtifact_PublishedArtifactWithDeletedInlineImageInDescription_ToProjectRoot_ReturnsNewArtifact(BaseArtifactType sourceArtifactType)
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, sourceArtifactType);
            var sourceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, sourceArtifact.Id);

            var imageFile = ArtifactStoreHelper.CreateRandomImageFile();

            var addedFile = Helper.ArtifactStore.AddImage(_user, imageFile);

            string propertyContent = ArtifactStoreHelper.CreateEmbeddedImageHtml(addedFile.EmbeddedImageId);

            CSharpUtilities.SetProperty("Description", propertyContent, sourceArtifactDetails);

            sourceArtifact.Lock(_user);
            Artifact.UpdateArtifact(sourceArtifact, _user, sourceArtifactDetails, address: Helper.BlueprintServer.Address);
            CSharpUtilities.SetProperty("Description", string.Empty, sourceArtifactDetails);
            Artifact.UpdateArtifact(sourceArtifact, _user, sourceArtifactDetails, address: Helper.BlueprintServer.Address);
            sourceArtifact.Publish(_user);

            sourceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, sourceArtifact.Id);
            int expectedVersion = 2;

            // Execute:
            CopyNovaArtifactResultSet copyResult = null;

            Assert.DoesNotThrow(() => copyResult = CopyArtifactAndWrap(sourceArtifact, _project.Id, _user),
                "'POST {0}' should return 201 Created when valid parameters are passed.", SVC_PATH);
            string copiedArtifactEmbeddedImageId = ArtifactStoreHelper.GetInlineImageId(copyResult.Artifact.Description);

            Assert.AreNotEqual(addedFile.EmbeddedImageId, copiedArtifactEmbeddedImageId,
                "ImageId in the copied artifact should be different from the ImageId from source artifact.");

            // Verify:
            AssertCopiedArtifactPropertiesAreIdenticalToOriginal(sourceArtifactDetails, copyResult, _user,
                expectedVersionOfOriginalArtifact: expectedVersion);
            Assert.IsEmpty(copiedArtifactEmbeddedImageId, "Copied artifact description shouldn't have inline image, but it has.");
        }

        #endregion 201 Created tests

        [TestCase(ItemTypePredefined.Actor)]
        [TestRail(1)]
        [Description("" +
                     ".")]
        public void CopyArtifact_PublishedArtifactWithInlineImageInRichText_ToProjectRoot_ReturnsNewArtifact(ItemTypePredefined itemType)
        {
            // Setup:
            string customPropertyName = "Std-Text-Required-RT-Multi-HasDefault";
            IProject project = Helper.GetProject(TestHelper.GoldenDataProject.CustomData, _user);
            IArtifact sourceArtifact = Helper.CreateWrapAndPublishNovaArtifactForStandardArtifactType(project, _user, itemType);
            sourceArtifact.Lock(_user);
            
            var sourceArtifactDetails = ArtifactStoreHelper.AddRandomImageToArtifactProperty(sourceArtifact, _user,
                Helper.ArtifactStore, propertyName: customPropertyName);

            int expectedVersion = -1;

            Func<INovaArtifactDetails, string, string> GetCustomPropertyStringValueByName = (details, propertyName) =>
            {
                return details.CustomPropertyValues.Find(p => p.Name == propertyName).CustomPropertyValue.ToString();
            };

            string sourceArtifactInlineImageId = ArtifactStoreHelper.GetInlineImageId(GetCustomPropertyStringValueByName(sourceArtifactDetails, customPropertyName));
            var sourceArtifactImageFile = Helper.ArtifactStore.GetImage(_user, sourceArtifactInlineImageId);

            // Execute:
            CopyNovaArtifactResultSet copyResult = null;

            Assert.DoesNotThrow(() => copyResult = CopyArtifactAndWrap(sourceArtifact, project.Id, _user),
                "'POST {0}' should return 201 Created when valid parameters are passed.", SVC_PATH);
            string copiedArtifactEmbeddedImageId = ArtifactStoreHelper.GetInlineImageId(GetCustomPropertyStringValueByName(copyResult.Artifact, customPropertyName));

            Assert.AreNotEqual(sourceArtifactInlineImageId, copiedArtifactEmbeddedImageId,
                "ImageId in the copied artifact should be different from the ImageId from source artifact.");
            var copiedArtifactImageFile = Helper.ArtifactStore.GetImage(_user, copiedArtifactEmbeddedImageId);

            // Verify:
            AssertCopiedArtifactPropertiesAreIdenticalToOriginal(sourceArtifactDetails, copyResult, _user,
                expectedVersionOfOriginalArtifact: expectedVersion, skipDescription: true);
            FileStoreTestHelper.AssertFilesAreIdentical(sourceArtifactImageFile, copiedArtifactImageFile, compareFileNames: false);
        }

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
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.IncorrectInputParameters,
                "Parameter orderIndex cannot be equal to or less than 0.");
        }

        #endregion 400 Bad Request tests

        #region 401 Unauthorized tests

        [TestCase(BaseArtifactType.Process)]
        [TestRail(191209)]
        [Description("Create & publish two artifacts.  Copy one artifact to be a child of the other with invalid token in a request.  " +
            "Verify response returns code 401 Unauthorized.")]
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
                "'POST {0}' should return 403 Forbidden when user tries to copy an artifact to be a child of another artifact to which he/she has viewer permissions only",
                SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden,
                "You do not have permissions to copy the artifact in the selected location.");
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
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot copy artifacts to a different project.");
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
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot copy a folder artifact to non folder/project parent.");
        }

        [TestCase(BaselineAndCollectionTypePredefined.ArtifactCollection)]
        [TestCase(BaselineAndCollectionTypePredefined.CollectionFolder)]
        [TestRail(192081)]
        [Description("Create a collection or collection folder.  Copy a regular artifact to be a child of the collection or collection folder.  " +
            "Verify returned code 403 Forbidden.")]
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
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot copy artifacts outside of the artifact section.");
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
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot copy artifacts that are not from the artifact section.");
        }

        [Category(Execute.Weekly)] 
        [TestCase(BaselineAndCollectionTypePredefined.ArtifactCollection)]
        [TestCase(BaselineAndCollectionTypePredefined.CollectionFolder)]
        [TestRail(192083)]
        [Description("Create a collection or collection folder.  Copy a collection or collection folder to be a child of a collection folder.  " +
            "Verify returned code 403 Forbidden.")]
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
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot copy artifacts that are not from the artifact section.");
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
                TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.NotFound,
                    I18NHelper.FormatInvariant("Artifact where to copy with ID {0} is not found.", nonExistingArtifactId));
            }
        }

        [TestCase(BaseArtifactType.Actor, 0)]
        [TestCase(BaseArtifactType.Actor, int.MaxValue)]
        [TestRail(195511)]
        [Description("Create & publish an artifact. Copy a non existing artifact to be a child of the published artifact.  Verify returned code 404 Not Found.")]
        public void CopyArtifact_NonExistingArtifact_ToPublishedArtifact_404NotFound(BaseArtifactType artifactType, int nonExistingArtifactId)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => ArtifactStore.CopyArtifact(Helper.ArtifactStore.Address, nonExistingArtifactId, artifact.Id, _user),
                "'POST {0}' should return 404 Not Found when user tries to copy artifact to be a child of non existing artifact", SVC_PATH);

            // Verify:
            if (nonExistingArtifactId > 0)
            {
                TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.NotFound,
                    I18NHelper.FormatInvariant("Artifact to copy with ID {0} is not found.", nonExistingArtifactId));
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
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.NotFound,
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
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.NotFound,
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
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.NotFound,
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
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.NotFound,
                I18NHelper.FormatInvariant("Artifact where to copy with ID {0} is not found.", targetArtifact.Id));
        }

        [TestCase(BaseArtifactType.Process)]
        [TestCase(BaseArtifactType.PrimitiveFolder)]
        [TestRail(192085)]
        [Description("Create & publish two artifacts with sub-artifacts.  Copy an artifact to be a child of another artifact sub-artifact.  " +
            "Verify returned code 404 Not Found.")]
        public void CopyArtifact_SavedArtifact_ToSubArtifact_404NotFound(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact sourceArtifact = Helper.CreateAndSaveArtifact(_project, _user, artifactType);
            IArtifact targetArtifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Process);

            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, targetArtifact.Id);

            Assert.IsNotNull(subArtifacts, "This artifact does not have sub-artifacts!");
            Assert.IsNotEmpty(subArtifacts, "This artifact does not have sub-artifacts!");

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() =>
                ArtifactStore.CopyArtifact(Helper.ArtifactStore.Address, sourceArtifact.Id, subArtifacts.First().Id, _user),
                "'POST {0}' should return 404 Not Found when user tries to copy an artifact to be a child of a sub-artifact", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.NotFound,
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
            var ex = Assert.Throws<Http404NotFoundException>(() =>
                ArtifactStore.CopyArtifact(Helper.ArtifactStore.Address, subArtifacts.First().Id, targetArtifact.Id, _user),
                "'POST {0}' should return 404 Not Found when user tries to copy a sub-artifact to be a child of another artifact", SVC_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.NotFound,
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
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.NotFound,
                I18NHelper.FormatInvariant("Artifact to copy with ID {0} is not found.", _project.Id));
        }

        #endregion 404 Not Found tests

        #region 409 Conflict tests    

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]    // Ignore for now.
        [Category(Categories.CannotRunInParallel)]
        [TestCase(BaseArtifactType.PrimitiveFolder, // PrimitiveFolders can only have projects as parents.
            BaseArtifactType.Actor,
            BaseArtifactType.Glossary,
            BaseArtifactType.Document,
            BaseArtifactType.TextualRequirement)]
        [TestRail(195434)]
        [Description("Create & publish a chain of artifacts.  Change MaxNumberArtifactsToCopy DB setting in ApplicationSettings table to number of artifacts to copy - 1.  " +
            "Copy parent artifact into the project root.  Verify returned code 409 Conflict.")]
        public void CopyArtifact_PublishedArtifacts_ToProjectRoot_409Conflict(params BaseArtifactType[] artifactType)
        {
            ThrowIf.ArgumentNull(artifactType, nameof(artifactType));

            // Setup:
            int newMaxNumberArtifactsToCopy = artifactType.Length - 1;
            const string MAX_NUMBER_ARTIFACTS_TO_COPY = "MaxNumberArtifactsToCopy";

            var artifactChain = Helper.CreatePublishedArtifactChain(_project, _user, artifactType);

            // Save original MaxNumberArtifactsToCopy setting and then change it to smaller value.
            var originalMaxNumberArtifactsToCopy = TestHelper.GetApplicationSetting(MAX_NUMBER_ARTIFACTS_TO_COPY);

            TestHelper.UpdateApplicationSettings(MAX_NUMBER_ARTIFACTS_TO_COPY, newMaxNumberArtifactsToCopy.ToStringInvariant());

            try
            {
                // Execute:
                var ex =
                    Assert.Throws<Http409ConflictException>(
                        () => ArtifactStore.CopyArtifact(Helper.ArtifactStore.Address, artifactChain.First().Id, _project.Id, _user),
                        "'POST {0}' should return 409 Conflict when user tries to copy a artifact with large amount of children " +
                        "(larger than MaxNumberArtifactsToCopy setting in ApplicationSettings table)", SVC_PATH);

                // Verify:
                TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ExceedsLimit,
                    I18NHelper.FormatInvariant("The number of artifacts to copy exceeds the limit - {0}.", newMaxNumberArtifactsToCopy));
            }
            finally
            {
                // Restore MaxNumberArtifactsToCopy back to original value.
                TestHelper.UpdateApplicationSettings(MAX_NUMBER_ARTIFACTS_TO_COPY, originalMaxNumberArtifactsToCopy);
            }
        }

        #endregion 409 Conflict tests

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
        /// <param name="skipPermissions">(optional) Pass true to skip comparison of the Permissions properties.</param>
        /// <param name="skipDescription">(optional) Pass true to skip comparison of the Description properties.</param>
        /// <exception cref="AssertionException">If any expectations failed.</exception>
        private void AssertCopiedArtifactPropertiesAreIdenticalToOriginal(INovaArtifactDetails originalArtifact,
            CopyNovaArtifactResultSet copyResult,
            IUser user,
            int expectedNumberOfArtifactsCopied = 1,
            int expectedVersionOfOriginalArtifact = 1,
            bool skipCreatedBy = false,
            bool skipPermissions = false,
            bool skipDescription = false)
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
                skipIdAndVersion: true, skipParentId: true, skipOrderIndex: true, skipCreatedBy: skipCreatedBy,
                skipPublishedProperties: true, skipPermissions: skipPermissions, skipDescription: skipDescription);

            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(user, copyResult.Artifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, copyResult.Artifact, skipDescription: skipDescription);
            Assert.AreEqual(-1, artifactDetails.Version, "Version of copied artifact should be -1 (i.e. not published)!");

            // Make sure original artifact didn't change.
            var originalArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(user, originalArtifact.Id);
            // We need to skip Permissions comparison in cases where we use pre-created data that was created by a different user than used in the GET call.
            ArtifactStoreHelper.AssertArtifactsEqual(originalArtifactDetails, originalArtifact, skipPermissions: skipPermissions,
                skipDescription: skipDescription);
            Assert.AreEqual(expectedVersionOfOriginalArtifact, originalArtifactDetails.Version,
                "The Version of the original artifact shouldn't have changed after the copy!");
        }

        /// <summary>
        /// Asserts that the sub-artifacts of the copied artifact are equal to those in the source artifact (except for the IDs).
        /// </summary>
        /// <param name="user">User to authenticate with.</param>
        /// <param name="sourceArtifact">The original source artifact.</param>
        /// <param name="copiedArtifact">The new copied artifact.</param>
        /// <param name="skipSubArtifactTraces">(optional) Pass true to skip comparison of the SubArtifact trace Relationships.</param>
        /// <param name="compareOptions">(optional) Specifies which Attachments properties to compare.  By default, all properties are compared.</param>
        /// <exception cref="AssertionException">If any of the sub-artifact properties are different between the source and copied artifacts.</exception>
        private void AssertCopiedSubArtifactsAreEqualToOriginal(IUser user, INovaArtifactBase sourceArtifact, INovaArtifactBase copiedArtifact,
            bool skipSubArtifactTraces = false, Attachments.CompareOptions compareOptions = null)
        {
            ThrowIf.ArgumentNull(sourceArtifact, nameof(sourceArtifact));
            ThrowIf.ArgumentNull(copiedArtifact, nameof(copiedArtifact));

            var sourceSubArtifacts = Helper.ArtifactStore.GetSubartifacts(user, sourceArtifact.Id);
            var copiedSubArtifacts = Helper.ArtifactStore.GetSubartifacts(user, copiedArtifact.Id);

            Assert.AreEqual(sourceSubArtifacts.Count, copiedSubArtifacts.Count, "Number of sub-artifacts copied doesn't match the original artifact!");

            // NOTE: We're assuming the copied sub-artifacts are returned in the same order as those in the source artifact.
            for (int i = 0; i < sourceSubArtifacts.Count; ++i)
            {
                var sourceSubArtifact = Helper.ArtifactStore.GetSubartifact(user, sourceArtifact.Id, sourceSubArtifacts[i].Id);
                var copiedSubArtifact = Helper.ArtifactStore.GetSubartifact(user, copiedArtifact.Id, copiedSubArtifacts[i].Id);

                ArtifactStoreHelper.AssertSubArtifactsAreEqual(sourceSubArtifact, copiedSubArtifact, Helper.ArtifactStore, user,
                    skipId: true, skipTraces: skipSubArtifactTraces, expectedParentId: copiedArtifact.Id, compareOptions: compareOptions);
            }
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

                _wrappedArtifacts.Add(Helper.WrapNovaArtifact(copyResult.Artifact, project, user, artifact.BaseArtifactType));
            }

            return copyResult;
        }

        /// <summary>
        /// Creates a complex Process diagram and generates User Stories.
        /// </summary>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="userStories">[out] The generated User Stories will be returned in this list.</param>
        /// <returns>The new Process artifact.</returns>
        private IArtifact CreateComplexProcessAndGenerateUserStories(IUser user, out List<IStorytellerUserStory> userStories)
        {
            /*  Create and publish a Process diagram that looks like this:
                [S]--[P]--+--<UD1>--+--[UT]---+--[ST]---+--[UT4]--<SD1>--+--[ST5]--+--[E]
                               |                        |           |              |
                               +-------[UT2]--+--[ST3]--+           +----+--[ST7]--+
            */
            var sourceProcess = StorytellerTestHelper.CreateAndGetDefaultProcessWithUserAndSystemDecisions(
                Helper.Storyteller, _project, user);

            StorytellerTestHelper.UpdateVerifyAndPublishProcess(sourceProcess, Helper.Storyteller, user);

            // Generate User Stories.
            userStories = Helper.Storyteller.GenerateUserStories(user, sourceProcess);
            Assert.NotNull(userStories, "No User Stories were generated!");
            Assert.AreEqual(3, userStories.Count, "There should be 3 User Stories generated!");

            var sourceArtifact = Helper.Storyteller.Artifacts.Find(a => a.Id.Equals(sourceProcess.Id));
            return sourceArtifact;
        }

        /// <summary>
        /// Veifies that all the children of the source artifact were copied to the target.
        /// </summary>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="sourceArtifact">The source artifact.</param>
        /// <param name="copiedArtifact">The copied artifact.</param>
        /// <param name="parentWasCopiedToChild">(optional) Pass true if the source artifact was copied to one of its children.</param>
        /// <param name="previousParentArtifact">(optional) Should only be used internally by this function to specify the parent from the previous recursive call.</param>
        /// <param name="skipSubArtifactTraces">(optional) Pass true to skip comparison of the SubArtifact trace Relationships.</param>
        private void VerifyChildrenWereCopied(IUser user, INovaArtifactBase sourceArtifact, INovaArtifactBase copiedArtifact,
            bool parentWasCopiedToChild = false, INovaArtifactBase previousParentArtifact = null, bool skipSubArtifactTraces = false)
        {
            ThrowIf.ArgumentNull(sourceArtifact, nameof(sourceArtifact));
            ThrowIf.ArgumentNull(copiedArtifact, nameof(copiedArtifact));
            Assert.AreEqual(copiedArtifact.Name, sourceArtifact.Name, "The wrong source artifact was provided.");

            var sourceChildren = Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(sourceArtifact.ProjectId.Value,
                sourceArtifact.Id, user);
            var copiedChildren = Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(copiedArtifact.ProjectId.Value,
                copiedArtifact.Id, user);

            // If a parent was copied to one of its children, remove the copy from the source list so the Count comparison doesn't fail.
            if (parentWasCopiedToChild)
            {
                sourceChildren.RemoveAll(a => a.Name == previousParentArtifact?.Name);
            }

            Assert.AreEqual(sourceChildren.Count, copiedChildren.Count, "The number of artifacts copied is incorrect!");

            for (int i = 0; i < sourceChildren.Count; ++i)
            {
                var sourceChild = sourceChildren[i];
                var copiedChild = copiedChildren[i];

                AssertCopiedSubArtifactsAreEqualToOriginal(user, sourceChild, copiedChild, skipSubArtifactTraces: skipSubArtifactTraces);

                NovaArtifact.AssertAreEqual(sourceChild, copiedChild,
                    skipIdAndVersion: true, skipParentId: true, skipOrderIndex: true, skipPublishedProperties: true);

                // Recursively verify all children below this one.
                if (sourceChild.HasChildren)
                {
                    VerifyChildrenWereCopied(user, sourceChild, copiedChild, parentWasCopiedToChild, sourceArtifact);
                }
            }
        }

        #endregion Private functions
    }
}
