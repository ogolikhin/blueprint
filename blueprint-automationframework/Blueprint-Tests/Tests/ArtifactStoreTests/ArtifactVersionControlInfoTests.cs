using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.StorytellerModel;
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
            artifactDetails.AssertEquals(basicArtifactInfo);

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
            artifactDetails.AssertEquals(basicArtifactInfo, compareLockInfo: false);

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
            artifactDetails.AssertEquals(basicArtifactInfo);

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
            artifactDetails.AssertEquals(basicArtifactInfo);

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
            artifactDetails.AssertEquals(basicArtifactInfo);

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
            artifactDetails.AssertEquals(basicArtifactInfo);

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
            artifactDetails.AssertEquals(basicArtifactInfo, compareLockInfo: false);

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
            artifactDetails.AssertEquals(basicArtifactInfo);

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
            artifactDetails.AssertEquals(basicArtifactInfo);

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
            artifactDetails.AssertEquals(basicArtifactInfo);

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
            artifactDetails.AssertEquals(basicArtifactInfo);

            VerifyBasicInformationResponse(basicArtifactInfo, hasChanges: false, isDeleted: false, subArtifactId: subArtifacts[0].Id,
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
            artifactDetails.AssertEquals(basicArtifactInfo);

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
            
            artifactDetails.AssertEquals(basicArtifactInfo);

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
            artifactDetails.AssertEquals(basicArtifactInfo);

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
            artifactDetails.AssertEquals(basicArtifactInfo);

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
            artifactDetails.AssertEquals(basicArtifactInfo);

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
            artifactDetails.AssertEquals(basicArtifactInfo, compareVersions: false);

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
            artifactDetails.AssertEquals(basicArtifactInfo, compareVersions: false);

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
            artifactDetails.AssertEquals(basicArtifactInfo);

            VerifyBasicInformationResponse(basicArtifactInfo, hasChanges: false, isDeleted: false, subArtifactId: userTask.Id,
                versionCount: artifactDetails.Version);
        }

        #endregion Delete

        #endregion 200 OK tests

        #region Negative tests

        // TODO: Call GetVersionControlInfo without a token header.  Verify 400 Bad Request.
        // TODO: Call GetVersionControlInfo with a bad token.  Verify 401 Unauthorized.
        // TODO: Call GetVersionControlInfo with an artifact the user doesn't have access to.  Verify 403 Forbiden.
        // TODO: Call GetVersionControlInfo with an artifact in a project the user doesn't have access to.  Verify 403 Forbidden.
        // TODO: Call GetVersionControlInfo with a non-existent artifact ID.  Verify 404 Not Found.
        // TODO: Call GetVersionControlInfo with an unpublished artifact with a different user.  Verify 404 Not Found.
        // TODO: Call GetVersionControlInfo with an unpublished sub-artifact of a published artifact with a different user.  Verify 404 Not Found.
        // TODO: Call GetVersionControlInfo with an unpublished sub-artifact of an unpublished artifact with a different user.  Verify 404 Not Found.

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
