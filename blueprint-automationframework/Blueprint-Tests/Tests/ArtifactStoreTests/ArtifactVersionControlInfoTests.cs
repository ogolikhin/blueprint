using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.StorytellerModel;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using System.Collections.Generic;
using TestCommon;
using Utilities;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class ArtifactVersionControlInfoTests : TestBase
    {
        private const string SVC_PATH = RestPaths.Svc.ArtifactStore.Artifacts.VERSION_CONTROL_INFO_id_;

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

        #region 200 OK tests

        #region Artifact Changes

        [TestCase(BaseArtifactType.Actor, 2)]
        [TestRail(182452)]
        [Description("Create & publish an artifact.  Verify the basic artifact information returned with HasChanges flag set to false.")]
        public void VersionControlInfoWithArtifactId_PublishedArtifact_NoChanges_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType, int numberOfVersions)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, numberOfVersions: numberOfVersions);

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, artifact.Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, basicArtifactInfo);

            VerifyBasicInformationResponse(basicArtifactInfo, hasChanges : false, isDeleted : false, versionCount: artifactDetails.Version);
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(182453)]
        [Description("Create & save an artifact.  Verify the basic artifact information returned with HasChanges flag set to true.")]
        public void VersionControlInfoWithArtifactId_SavedArtifact_HasChanges_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndSaveArtifact(_project, _user, artifactType);

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, artifact.Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, basicArtifactInfo, compareLockInfo: false);

            VerifyBasicInformationResponse(artifact, basicArtifactInfo, hasChanges: true, isDeleted: false, versionCount: 0);
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(182500)]
        [Description("Create & publish an artifact.  Create manual trace to the artifact using another user.  " +
            "Verify another user gets basic artifact information with HasChanges flag set to true.")]
        public void VersionControlInfoWithArtifactId_PublishArtifactWithTrace_ReturnsArtifactInfoWithHasChangesTrue_200OK(BaseArtifactType artifactType)
        {
            //Setup
            IArtifact artifact = CreatePublishedArtifactWithTrace(artifactType, TraceDirection.From, _user);
            artifact.Lock();

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            //Execute
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, artifact.Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, basicArtifactInfo.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, basicArtifactInfo);

            VerifyBasicInformationResponse(artifact, basicArtifactInfo, hasChanges: true, isDeleted: false,
                versionCount: artifactDetails.Version);
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(182504)]
        [Description("Create & publish an artifact.  Create manual trace to the artifact with current user. Verify another user gets basic artifact information")]
        public void VersionControlInfoWithArtifactId_PublishArtifactWithTrace_AnotherUserGetsBasicInfo_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType)
        {
            //Setup
            IUser anotherUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            IArtifact artifact = CreatePublishedArtifactWithTrace(artifactType, TraceDirection.From, _user);

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            // Execute            
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(anotherUser, artifact.Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, basicArtifactInfo.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, basicArtifactInfo);

            VerifyBasicInformationResponse(basicArtifactInfo, hasChanges: false, isDeleted: false,
                versionCount: artifactDetails.Version);
        }

        [TestCase(BaseArtifactType.Actor, 3)]
        [TestRail(182499)]
        [Description("Create, publish & lock an artifact.  Verify the basic artifact information for another user returned with HasChanges flag set to false.")]
        public void VersionControlInfoWithArtifactId_PublishedAndLockedArtifact_NoChangesForAnotherUser_ReturnsArtifactInfo_200OK(
            BaseArtifactType artifactType, int numberOfVersions)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, numberOfVersions: numberOfVersions);
            artifact.Lock(_user);

            IUser anotherUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            // Execute:     
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(anotherUser, artifact.Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(anotherUser, artifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, basicArtifactInfo);

            VerifyBasicInformationResponse(basicArtifactInfo, hasChanges: false, isDeleted: false, versionCount: artifactDetails.Version);
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(182551)]
        [Description("Create, publish & lock an artifact.  Verify the basic artifact information returned with HasChanges flag set to true.")]
        public void VersionControlInfoWithArtifactId_PublishedAndLockedArtifact_HasChanges_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            artifact.Lock();

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, artifact.Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, basicArtifactInfo);

            VerifyBasicInformationResponse(artifact, basicArtifactInfo, hasChanges: true, isDeleted: false,
                versionCount: artifactDetails.Version);
        }

        #endregion Artifact Changes

        #region Sub-Artifact

        [TestCase(BaseArtifactType.UseCase)]
        [TestCase(BaseArtifactType.Process)]
        [TestRail(182512)]
        [Description("Create & save an artifact with sub-artifacts. Verify user gets basic artifact information with subartifact Id.")]
        public void VersionControlInfoWithSubArtifactId_SavedArtifactWithSubArtifact_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _user, artifactType);

            List<INovaSubArtifact> subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, artifact.Id);

            Assert.IsTrue(subArtifacts.Count > 0, "There is no sub-artifact in this artifact");

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            // Execute
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, subArtifacts[0].Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, basicArtifactInfo, compareLockInfo: false);

            VerifyBasicInformationResponse(artifact, basicArtifactInfo, hasChanges: true, isDeleted: false, subArtifactId: subArtifacts[0].Id,
                versionCount: 0);
        }

        [TestCase(BaseArtifactType.UseCase, 2)]
        [TestCase(BaseArtifactType.Process, 3)]
        [TestRail(182544)]
        [Description("Create & publish multiple versions of an artifact with sub-artifacts.  Verify user gets basic artifact information with subartifact Id.")]
        public void VersionControlInfoWithSubArtifactId_PublishedMultipleVersionsOfArtifactWithSubArtifacts_ReturnsArtifactInfo_200OK(
            BaseArtifactType artifactType, int numberOfVersions)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, numberOfVersions: numberOfVersions);

            List<INovaSubArtifact> subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, artifact.Id);

            Assert.IsTrue(subArtifacts.Count > 0, "There is no sub-artifact in this artifact");

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            // Execute
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, subArtifacts[0].Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, basicArtifactInfo.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, basicArtifactInfo);

            VerifyBasicInformationResponse(basicArtifactInfo, hasChanges: false, isDeleted: false, subArtifactId: subArtifacts[0].Id,
                versionCount: artifactDetails.Version);
        }

        [TestCase]
        [TestRail(182606)]
        [Description("Create & publish an artifact.  Change sub-artifact & save. Verify another user gets basic artifact information with subartifact Id.")]
        public void VersionControlInfoWithSubArtifactId_PublishedArtifactSubartifactUpdated_AnotherUserGetsBasicInfo_ReturnsArtifactInfo_200OK()
        {
            // Setup:
            // Create a Process artifact
            var processArtifact = Helper.Storyteller.CreateAndPublishProcessArtifact(project: _project, user: _user);

            // Get the process artifact
            IProcess process = Helper.Storyteller.GetProcess(_user, processArtifact.Id);

            List<INovaSubArtifact> subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, processArtifact.Id);

            subArtifacts[0].DisplayName = "Sub-artifact_" + process.Name;

            Helper.Storyteller.UpdateProcess(_user, process);

            IUser anotherUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            // Execute
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(anotherUser, subArtifacts[0].Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, processArtifact.Id);

            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, basicArtifactInfo);

            VerifyBasicInformationResponse(basicArtifactInfo, hasChanges: false, isDeleted: false, subArtifactId: subArtifacts[0].Id,
                versionCount: artifactDetails.Version);
        }

        [TestCase(BaseArtifactType.UseCase)]
        [TestCase(BaseArtifactType.Process)]
        [TestRail(182554)]
        [Description("Create, publish & lock an artifact with subartifact. Verify user gets basic artifact information with subartifact Id.")]
        public void VersionControlInfoWithSubArtifactId_PublishedSubArtifactLockedArtifact_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            List<INovaSubArtifact> subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, artifact.Id);

            Assert.IsTrue(subArtifacts.Count > 0, "There is no sub-artifact in this artifact");

            artifact.Lock();

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            // Execute
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, subArtifacts[0].Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, basicArtifactInfo);

            VerifyBasicInformationResponse(artifact, basicArtifactInfo, hasChanges: true, isDeleted: false, subArtifactId: subArtifacts[0].Id,
                versionCount: artifactDetails.Version);
        }

        [TestCase(BaseArtifactType.UseCase)]
        [TestCase(BaseArtifactType.Process)]
        [TestRail(182555)]
        [Description("Create, publish & lock an artifact with subartifact. Verify another user gets basic artifact information with subartifact Id.")]
        public void VersionControlInfoWithSubArtifactId_PublishedSubArtifactLockedArtifactAnotherUser_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            List<INovaSubArtifact> subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, artifact.Id);

            Assert.IsTrue(subArtifacts.Count > 0, "There is no sub-artifact in this artifact");

            artifact.Lock();

            IUser anotherUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            // Execute
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(anotherUser, subArtifacts[0].Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, basicArtifactInfo);

            VerifyBasicInformationResponse(basicArtifactInfo, hasChanges: false, isDeleted: false, subArtifactId: subArtifacts[0].Id,
                versionCount: artifactDetails.Version);
        }

        [TestCase]
        [TestRail(183444)]
        [Description("Create & publish an artifact with subartifacts. Update sub-artifact. Verify user gets basic artifact information with subartifact Id.")]
        public void VersionControlInfoWithSubArtifactId_PublishedArtifactLockedArtifactWithChanges_ReturnsArtifactInfo_200OK()
        {
            // Setup
            // Create a Process artifact
            var processArtifact = Helper.Storyteller.CreateAndPublishProcessArtifact(project: _project, user: _user);

            // Get the process artifact
            IProcess process = Helper.Storyteller.GetProcess(_user, processArtifact.Id);

            List<INovaSubArtifact> subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, processArtifact.Id);

            subArtifacts[0].DisplayName = "Sub-artifact_" + process.Name;

            Helper.Storyteller.UpdateProcess(_user, process);

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            // Execute
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, subArtifacts[0].Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, processArtifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, basicArtifactInfo);

            VerifyBasicInformationResponse(basicArtifactInfo, hasChanges: true, isDeleted: false, subArtifactId: subArtifacts[0].Id,
                versionCount: artifactDetails.Version);
        }

        [TestCase(BaseArtifactType.UseCase)]
        [TestCase(BaseArtifactType.Process)]
        [TestRail(183445)]
        [Description("Create, publish & lock an artifact with subartifact. Lock artifact. Verify user gets basic artifact information with subartifact Id.")]
        public void VersionControlInfoWithSubArtifactId_PublishedSubArtifactLockedArtifactWithChanges_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType)
        {
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            artifact.Save();
            List<INovaSubArtifact> subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, artifact.Id);
            Assert.IsTrue(subArtifacts.Count > 0, "There is no sub-artifact in this artifact");

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            // Execute
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, subArtifacts[0].Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, basicArtifactInfo);

            VerifyBasicInformationResponse(basicArtifactInfo, hasChanges: true, isDeleted: false, subArtifactId: subArtifacts[0].Id,
                versionCount: artifactDetails.Version);
        }

        #endregion Sub-Artifact

        #region Delete

        [TestCase(BaseArtifactType.Actor, 1)]
        [TestRail(182543)]
        [Description("Create & publish an artifact, then delete & publish the artifact.  Verify user gets basic artifact information.")]
        public void VersionControlInfoWithArtifactId_PublishedArtifactDeleteAndPublish_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType, int numberOfVersions)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, numberOfVersions: numberOfVersions);

            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);

            artifact.Delete();
            artifact.Publish();

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            // Execute
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, artifact.Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, basicArtifactInfo);

            VerifyBasicInformationResponse(basicArtifactInfo, hasChanges: false, isDeleted: true,
                version: artifactDetails.Version, versionCount: artifactDetails.Version);
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(182563)]
        [Description("Create, publish & delete an artifact.  Verify user gets basic artifact information.")]
        public void VersionControlInfoWithArtifactId_PublishedArtifactDeleted_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            artifact.Lock();

            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);

            artifact.Delete();

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            // Execute
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, artifact.Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, basicArtifactInfo);

            VerifyBasicInformationResponse(artifact, basicArtifactInfo, hasChanges: true, isDeleted: true,
                version: artifactDetails.Version, versionCount: artifactDetails.Version);
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(182564)]
        [Description("Create & publish an artifact as user1.  Delete the artifact as user2.  Get basic information as user1.  " +
            "Verify user gets basic artifact information.")]
        public void VersionControlInfoWithArtifactId_PublishedArtifactDeletedByAnotherUser_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            IUser anotherUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            artifact.Delete(anotherUser);

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            // Execute
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, artifact.Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, basicArtifactInfo);

            VerifyBasicInformationResponse(basicArtifactInfo, hasChanges: false, isDeleted: false,
                versionCount: artifactDetails.Version);
        }

        [TestCase(BaseArtifactType.UseCase)]
        [TestCase(BaseArtifactType.Process)]
        [TestRail(182565)]
        [Description("Create, publish & delete an artifact with sub-artifacts.  Verify user gets basic artifact information when sending a sub-artifact ID.")]
        public void VersionControlInfoWithSubArtifactId_PublishedArtifactDeleted_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            List<INovaSubArtifact> subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, artifact.Id);
            Assert.IsTrue(subArtifacts.Count > 0, "There is no sub-artifact in this artifact!");

            artifact.Lock();
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);

            artifact.Delete();

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            // Execute
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, subArtifacts[0].Id),
                "'GET {0}' should return 200 OK when passed a valid sub-artifact ID!", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, basicArtifactInfo);

            VerifyBasicInformationResponse(artifact, basicArtifactInfo, hasChanges: true, isDeleted: true, subArtifactId: subArtifacts[0].Id,
                version: artifactDetails.Version, versionCount: artifactDetails.Version);
        }

        [TestCase(BaseArtifactType.UseCase)]
        [TestCase(BaseArtifactType.Process)]
        [TestRail(182593)]
        [Description("Create, publish an artifact as user1.  Delete the artifact as user2.  " +
            "Verify user2 get basic artifact information when sending a sub-artifact ID.")]
        public void VersionControlInfoWithSubArtifactId_PublishedArtifactDeletedByAnotherUser_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            List<INovaSubArtifact> subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, artifact.Id);
            Assert.IsTrue(subArtifacts.Count > 0, "There is no sub-artifact in this artifact");

            IUser anotherUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            artifact.Delete(anotherUser);

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            // Execute
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, subArtifacts[0].Id),
                "'GET {0}' should return 200 OK when passed a valid sub-artifact ID!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, basicArtifactInfo);

            VerifyBasicInformationResponse(basicArtifactInfo, hasChanges: false, isDeleted: false, subArtifactId: subArtifacts[0].Id,
                versionCount: artifactDetails.Version);
        }

        [TestCase]
        [TestRail(182601)]
        [Description("Create & publish process artifact.  Delete a sub-artifact & publish.  " +
            "Verify user gets basic artifact information when sending the sub-artifact Id.")]
        public void VersionControlInfoWithSubArtifactId_PublishedArtifact_DeleteAndPublishSubArtifact_ReturnsArtifactInfo_200OK()
        {
            // Setup:

            // Create & publish a Process artifact with 2 sequential User Tasks.
            IProcess processArtifact = StorytellerTestHelper.CreateAndGetDefaultProcessWithTwoSequentialUserTasks(Helper.Storyteller, _project, _user);
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(processArtifact, Helper.Storyteller, _user);

            var userTasks = processArtifact.GetProcessShapesByShapeType(ProcessShapeType.UserTask);
            Assert.That(userTasks.Count > 1, "There should be more than one User Task!");

            // Delete a user task & publish.
            var userTask = userTasks[0];
            processArtifact.DeleteUserAndSystemTask(userTask);
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(processArtifact, Helper.Storyteller, _user);

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            // Execute
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, userTask.Id),
                "'GET {0}' should return 200 OK when passed a valid sub-artifact ID!", SVC_PATH);

            // Verify
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, processArtifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, basicArtifactInfo, compareVersions: false);

            VerifyBasicInformationResponse(basicArtifactInfo, hasChanges: false, isDeleted: false, subArtifactId: userTask.Id,
                version: 1, versionCount: artifactDetails.Version);
        }

        [TestCase]
        [TestRail(182602)]
        [Description("Create & publish process artifact.  Delete a sub-artifact & save.  " +
            "Verify user gets basic artifact information when sending the sub-artifact Id.")]
        public void VersionControlInfoWithSubArtifactId_PublishedArtifact_DeleteAndSaveSubArtifact_ReturnsArtifactInfo_200OK()
        {
            // Setup:

            // Create & publish a Process artifact with 2 sequential User Tasks.
            IProcess processArtifact = StorytellerTestHelper.CreateAndGetDefaultProcessWithTwoSequentialUserTasks(Helper.Storyteller, _project, _user);
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(processArtifact, Helper.Storyteller, _user);

            var userTasks = processArtifact.GetProcessShapesByShapeType(ProcessShapeType.UserTask);
            Assert.That(userTasks.Count > 1, "There should be more than one User Task!");

            // Delete a user task & save.
            var userTask = userTasks[0];
            processArtifact.DeleteUserAndSystemTask(userTask);
            StorytellerTestHelper.UpdateAndVerifyProcess(processArtifact, Helper.Storyteller, _user);

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            // Execute
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, userTask.Id),
                "'GET {0}' should return 200 OK when passed a valid sub-artifact ID!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, processArtifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, basicArtifactInfo, compareVersions: false);

            VerifyBasicInformationResponse(basicArtifactInfo, hasChanges: true, isDeleted: false, subArtifactId: userTask.Id,
                version: artifactDetails.Version, versionCount: artifactDetails.Version);
        }

        [TestCase]
        [TestRail(182605)]
        [Description("Create & publish process artifact.  Delete a sub-artifact & save.  " +
            "Verify another user gets basic artifact information when sending the sub-artifact Id.")]
        public void VersionControlInfoWithSubArtifactId_PublishedArtifact_DeleteAndSaveSubArtifact_AnotherUserReturnsArtifactInfo_200OK()
        {
            // Setup:

            // Create & publish a Process artifact with 2 sequential User Tasks.
            IProcess processArtifact = StorytellerTestHelper.CreateAndGetDefaultProcessWithTwoSequentialUserTasks(Helper.Storyteller, _project, _user);
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(processArtifact, Helper.Storyteller, _user);

            var userTasks = processArtifact.GetProcessShapesByShapeType(ProcessShapeType.UserTask);
            Assert.That(userTasks.Count > 1, "There should be more than one User Task!");

            // Delete a user task & save.
            var userTask = userTasks[0];
            processArtifact.DeleteUserAndSystemTask(userTask);
            StorytellerTestHelper.UpdateAndVerifyProcess(processArtifact, Helper.Storyteller, _user);

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            IUser anotherUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            // Execute
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(anotherUser, userTask.Id),
                "'GET {0}' should return 200 OK when passed a valid sub-artifact ID!", SVC_PATH);

            // Verify
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, basicArtifactInfo.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, basicArtifactInfo);

            VerifyBasicInformationResponse(basicArtifactInfo, hasChanges: false, isDeleted: false, subArtifactId: userTask.Id,
                versionCount: artifactDetails.Version);
        }

        #endregion Delete

        #endregion 200 OK tests

        #region Negative tests

        #region 400 Bad Request

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(182607)]
        [Description("Create & publish an artifact.  Send no token header in the request.  Verify 400 Bad Request is returned.")]
        public void VersionControlInfoWithArtifactId_PublishedArtifact_NoTokenHeader_400BadRequest(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            // Execute:
            var ex = Assert.Throws <Http400BadRequestException>(() => Helper.ArtifactStore.GetVersionControlInfo(user: null, itemId: artifact.Id),
                "'GET {0}' should return 400 Bad Request when no token header is passed!", SVC_PATH);

            // Verify:
            const string expectedExceptionMessage = "Token is missing or malformed.";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "Expected '{0}' error when user tries to get basic information without a token header in the request.",
                expectedExceptionMessage);
        }

        #endregion 400 Bad Request

        #region 401 Unauthorized

        [TestRail(182858)]
        [TestCase(BaseArtifactType.Actor, "")]
        [TestCase(BaseArtifactType.Actor, "00000000-0000-0000-0000-000000000000")]
        [Description("Create & publish an artifact.  Send invalid token in the request.  Verify 401 Unauthorized is returned.")]
        public void VersionControlInfoWithArtifactId_PublishedArtifact_InvalidTokenHeader_401Unauthorized(BaseArtifactType artifactType, string invalidToken)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            IUser unauthorizedUser = UserFactory.CreateUserAndAddToDatabase();
            unauthorizedUser.SetToken(invalidToken);

            // Execute & Verify:
            var ex = Assert.Throws<Http401UnauthorizedException>(() => Helper.ArtifactStore.GetVersionControlInfo(unauthorizedUser, artifact.Id),
                "'GET {0}' should return 401 Unauthorized when passed invalid token!", SVC_PATH);

            const string expectedExceptionMessage = "Token is invalid.";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "Expected '{0}' error when user tries to get basic information and sends an invalid token.", expectedExceptionMessage);
        }

        #endregion 401 Unauthorized

        #region 403 Forbidden

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182859)]
        [Description("Create & publish an artifact.  User without permissions to project tries to access basic artifact information.  Verify returned code 403 Forbidden.")]
        public void VersionControlInfoWithArtifactId_PublishedArtifact_UserWithoutPermissionsToProject_403Forbidden(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            IUser userWithoutPermissions = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken,
                InstanceAdminRole.BlueprintAnalytics);

            // Execute & Verify:
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.GetVersionControlInfo(userWithoutPermissions, artifact.Id),
                "'GET {0}' should return 403 Forbidden when user without permissions tries to access basic artifact information!", SVC_PATH);

            string expectedExceptionMessage = I18NHelper.FormatInvariant("User does not have permissions for Artifact (Id:{0}).", artifact.Id);
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "Expected '{0}' error when user without permissions tries to get basic artifact information.", expectedExceptionMessage);
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(183363)]
        [Description("Create & publish an artifact. User without permissions to artifact tries to access basic artifact information.  Verify returned code 403 Forbidden.")]
        public void VersionControlInfoWithArtifactId_PublishedArtifact_UserWithoutPermissionsToArtifact_403Forbidden(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            // Create a user that has access to the project but not the artifact.
            IUser userWithoutPermissions = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);
            Helper.AssignProjectRolePermissionsToUser(userWithoutPermissions, TestHelper.ProjectRole.None, _project, artifact);

            // Execute & Verify:
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.GetVersionControlInfo(userWithoutPermissions, artifact.Id),
                "'GET {0}' should return 403 Forbidden when user without permissions tries to access basic artifact information!", SVC_PATH);

            string expectedExceptionMessage = I18NHelper.FormatInvariant("User does not have permissions for Artifact (Id:{0}).", artifact.Id);
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "Expected '{0}' error when user without permissions tries to get basic artifact information.", expectedExceptionMessage);
        }

        #endregion 403 Forbidden

        #region 404 Not Found

        [TestCase(int.MaxValue)]
        [TestRail(182860)]
        [Description("User tries to get basic information of artifact that does not exist.  Verify returned code 404 Not Found.")]
        public void VersionControlInfoWithArtifactId_NonExistingArtifactId_404NotFound(int artifactId)
        {
            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetVersionControlInfo(_user, artifactId),
                 "'GET {0}' should return 404 Not found when user tries to access basic artifact information of artifact that does not exist!", SVC_PATH);

            // Verify:
            string expectedExceptionMessage = I18NHelper.FormatInvariant("Item (Id:{0}) is not found.", artifactId);
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "Expected '{0}' error when user tries to get basic information of artifact that does not exist.", expectedExceptionMessage);
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182861)]
        [Description("User tries to get basic information of artifact that was saved but not published by another user.  Verify returned code 404 Not Found.")]
        public void VersionControlInfoWithArtifactId_SavedArtifactFromAnotherUser_404NotFound(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndSaveArtifact(_project, _user, artifactType);

            IUser anotherUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetVersionControlInfo(anotherUser, artifact.Id),
                 "'GET {0}' should return 404 Not Found when a user tries to access basic artifact information of artifact that was saved by another user but not published!",
                 SVC_PATH);

            // Verify:
            string expectedExceptionMessage = I18NHelper.FormatInvariant("Item (Id:{0}) is not found.", artifact.Id);
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "Expected '{0}' error when another user tries to get basic information of artifact that was saved but not published.",
                expectedExceptionMessage);
        }

        [TestCase]
        [TestRail(182862)]
        [Description("Another user tries to get basic information of sub-artifact that was saved but not published in published artifact.  " +
            "Verify returned code 404 Not Found.")]
        public void VersionControlInfoWithSubArtifactId_PublishedArtifactWithSavedSubArtifact_AnotherUser_404NotFound()
        {
            // Setup:
            // Create a Process artifact.
            var processArtifact = Helper.Storyteller.CreateAndPublishProcessArtifact(_project, _user);
            var process = Helper.Storyteller.GetProcess(_user, processArtifact.Id);

            // Add UserTask.
            var precondition = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);
            var processLink = process.GetOutgoingLinkForShape(precondition);
            var userTask = process.AddUserAndSystemTask(processLink);

            // Save the process.
            process = Helper.Storyteller.UpdateProcess(_user, process);
            userTask = process.GetProcessShapeByShapeName(userTask.Name);

            IUser anotherUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetVersionControlInfo(anotherUser, userTask.Id),
                 "'GET {0}' should return 404 Not found when another user tries to access basic artifact information of sub-artifact that was saved but not published!",
                 SVC_PATH);

            // Verify:
            string expectedExceptionMessage = I18NHelper.FormatInvariant("Item (Id:{0}) is not found.", userTask.Id);
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "Expected '{0}' error when user tries to get basic information of sub-artifact that was saved but not published by another user",
                expectedExceptionMessage);
        }

        [TestCase]
        [TestRail(182864)]
        [Description("Another user tries to get basic information of sub-artifact that was saved but not published in saved artifact.  Verify returned code 404 Not Found.")]
        public void VersionControlInfoWithSubArtifactId_SavedArtifactWithSavedSubArtifact_AnotherUser_404NotFound()
        {
            // Setup:
            // Create a Process artifact
            var artifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Process);
            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, artifact.Id);
            Assert.IsTrue(subArtifacts.Count > 0, "There is no sub-artifact in this artifact!");

            IUser anotherUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetVersionControlInfo(anotherUser, subArtifacts[0].Id),
                 "'GET {0}' should return 404 Not found when another user tries to access basic artifact information of sub-artifact that was saved but not published!",
                 SVC_PATH);

            // Verify:
            string expectedExceptionMessage = I18NHelper.FormatInvariant("Item (Id:{0}) is not found.", subArtifacts[0].Id);
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "Expected '{0}' error when another user tries to get basic information of sub-artifact that was saved but not published",
                expectedExceptionMessage);
        }

        [TestCase(BaseArtifactType.Glossary)]
        [TestRail(183377)]
        [Description("User tries to get basic information of an artifact that was deleted.  Verify returned code 404 Not Found.")]
        public void VersionControlInfoWithArtifactId_SavedArtifactDeleted_404NotFound(BaseArtifactType artifactType)
        {
            // Setup:
            // Create a Process artifact
            var artifact = Helper.CreateAndSaveArtifact(_project, _user, artifactType);

            artifact.Delete(_user);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetVersionControlInfo(_user, artifact.Id),
                 "'GET {0}' should return 404 Not found when user tries to access basic artifact information of artifact that was removed!",
                 SVC_PATH);

            // Verify:
            string expectedExceptionMessage = I18NHelper.FormatInvariant("Item (Id:{0}) is not found.", artifact.Id);
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "Expected '{0}' error when user tries to get basic information of an artifact that was removed",
                expectedExceptionMessage);
        }

        [TestCase(BaseArtifactType.Glossary)]
        [TestRail(183378)]
        [Description("Another user tries to get basic information of an artifact that was deleted.  Verify returned code 404 Not Found.")]
        public void VersionControlInfoWithArtifactId_SavedArtifactDeletedForAnotherUser_404NotFound(BaseArtifactType artifactType)
        {
            // Setup:
            // Create a Process artifact
            var artifact = Helper.CreateAndSaveArtifact(_project, _user, artifactType);

            artifact.Delete(_user);

            IUser anotherUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetVersionControlInfo(anotherUser, artifact.Id),
                 "'GET {0}' should return 404 Not found when another user tries to access basic artifact information of artifact that was removed!",
                 SVC_PATH);

            // Verify:
            string expectedExceptionMessage = I18NHelper.FormatInvariant("Item (Id:{0}) is not found.", artifact.Id);
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "Expected '{0}' error when another user tries to get basic information of an artifact that was removed",
                expectedExceptionMessage);
        }

        [TestCase(BaseArtifactType.UseCase)]
        [TestCase(BaseArtifactType.Process)]
        [TestRail(183402)]
        [Description("User tries to get basic information with sub-artifact id of an artifact that was deleted.  Verify returned code 404 Not Found.")]
        public void VersionControlInfoWithSubArtifactId_SavedArtifactWithSubArtifact_ArtifactDeleted_404NotFound(BaseArtifactType artifactType)
        {
            // Setup:
            // Create a Process artifact
            var artifact = Helper.CreateAndSaveArtifact(_project, _user, artifactType);

            List<INovaSubArtifact> subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, artifact.Id);

            Assert.IsTrue(subArtifacts.Count > 0, "There is no sub-artifact in this artifact");

            artifact.Delete(_user);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetVersionControlInfo(_user, subArtifacts[0].Id),
                 "'GET {0}' should return 404 Not found when user tries to access basic information of an artifact that was removed!",
                 SVC_PATH);

            // Verify:
            string expectedExceptionMessage = I18NHelper.FormatInvariant("Item (Id:{0}) is not found.", subArtifacts[0].Id);
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "Expected '{0}' error when user tries to get basic information of an artifact that was removed",

                expectedExceptionMessage);
        }

        #endregion 404 Not Found

        #endregion Negative tests

        #region private calls

        /// <summary>
        /// Create and publish two artifacts and adds trace by user specified in parameters.
        /// </summary>
        /// <param name="artifactType">Artifact type of artifacts to be created.</param>
        /// <param name="traceDirection">Direction of trace.</param>
        /// <param name="user">The user that will create the artifacts and traces.</param>
        /// <returns>Artifact with the trace.</returns>
        private IArtifact CreatePublishedArtifactWithTrace(BaseArtifactType artifactType, TraceDirection traceDirection, IUser user)
        {
            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(_project, user, artifactType);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_project, user, BaseArtifactType.UseCase);

            OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact,
                    targetArtifact, traceDirection, user);

            return sourceArtifact;
        }

        /// <summary>
        /// Verifies if returned basic information gets proper values for hasChanges, isDeleted and subArtifactId properties
        /// </summary>
        /// <param name="artifact">The artifact.</param>
        /// <param name="basicArtifactInfo">Returned basic information about artifact</param>
        /// <param name="hasChanges">Indicator of changes in an artifact</param>
        /// <param name="isDeleted">Indicator if artifact was deleted</param>
        /// <param name="subArtifactId">(optional) Id of requested subartifact</param>
        /// <param name="version">(optional) The expected Version to be returned.</param>
        /// <param name="versionCount">(optional) The expected VersionCount to be returned.</param>
        private static void VerifyBasicInformationResponse(IArtifactBase artifact,
            INovaVersionControlArtifactInfo basicArtifactInfo,
            bool hasChanges,
            bool isDeleted,
            int? subArtifactId = null,
            int? version = null,
            int? versionCount = null)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            VerifyBasicInformationResponse(basicArtifactInfo, hasChanges, isDeleted, subArtifactId, version, versionCount);

            if (artifact.LockOwner != null)
            {
                Assert.AreEqual(artifact.LockOwner.Id, basicArtifactInfo.LockedByUser.Id, "LockedByUser value doesn't have the expected value!");
                Assert.IsNotNull(basicArtifactInfo.LockedDateTime, "LockedDateTime should have value");
            }
        }

        /// <summary>
        /// Verifies if returned basic information gets proper values for hasChanges, isDeleted and subArtifactId properties
        /// </summary>
        /// <param name="basicArtifactInfo">Returned basic information about artifact</param>
        /// <param name="hasChanges">Indicator of changes in an artifact</param>
        /// <param name="isDeleted">Indicator if artifact was deleted</param>
        /// <param name="subArtifactId">(optional) Id of requested subartifact</param>
        /// <param name="version">(optional) The expected Version to be returned.</param>
        /// <param name="versionCount">(optional) The expected VersionCount to be returned.</param>
        private static void VerifyBasicInformationResponse(INovaVersionControlArtifactInfo basicArtifactInfo,
            bool hasChanges,
            bool isDeleted,
            int? subArtifactId = null,
            int? version = null,
            int? versionCount = null)
        {
            Assert.NotNull(basicArtifactInfo, "basicArtifactInfo shouldn't be null!");

            Assert.IsTrue(basicArtifactInfo.HasChanges.HasValue, "HasChanges property should have value");
            Assert.AreEqual(hasChanges, basicArtifactInfo.HasChanges.Value, "HasChanges property should be " + hasChanges.ToString());

            Assert.IsTrue(basicArtifactInfo.IsDeleted.HasValue, "IsDeleted property should have value");
            Assert.AreEqual(isDeleted, basicArtifactInfo.IsDeleted.Value, "IsDeleted property should be " + isDeleted.ToString());

            if (isDeleted == true)
            {
                Assert.IsNotNull(basicArtifactInfo.DeletedByUser, "DeletedByUser should have value");
                Assert.IsNotNull(basicArtifactInfo.DeletedDateTime, "DeletedDateTime should have value");
            }
            
            if (hasChanges == true)
            {
                Assert.IsNotNull(basicArtifactInfo.LockedByUser.Id, "LockedByUser should have value");
                Assert.IsNotNull(basicArtifactInfo.LockedDateTime, "LockedDateTime should have value");
            }
            
            if (basicArtifactInfo.SubArtifactId != null)
            {
                Assert.AreEqual(subArtifactId, basicArtifactInfo.SubArtifactId.Value,
                    "Sub-artifact Id is {0} and different from expected {1}.",
                    basicArtifactInfo.SubArtifactId.Value, subArtifactId);
            }

            Assert.AreEqual(version, basicArtifactInfo.Version, "The Version property doesn't match the expected value!");
            Assert.AreEqual(versionCount, basicArtifactInfo.VersionCount, "The VersionCount property doesn't match the expected value!");
        }

        #endregion private calls
    }
}
