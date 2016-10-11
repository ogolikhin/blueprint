﻿using CustomAttributes;
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
        public void VersionControlInfo_PublishedArtifact_NoChanges_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType, int numberOfVersions)
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

            VerifyBasicInformationResponse(basicArtifactInfo, hasChanges : false, isDeleted : false);
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(182453)]
        [Description("Create & save an artifact.  Verify the basic artifact information returned with HasChanges flag set to true.")]
        public void VersionControlInfo_SavedArtifact_HasChanges_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndSaveArtifact(_project, _user, artifactType);

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, artifact.Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            VerifyBasicInformationResponse(basicArtifactInfo, hasChanges: true, isDeleted: false);
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(182500)]
        [Description("Create & publish an artifact.  Create manual trace to the artifact using another user. Verify another user gets basic artifact information with HasChanges flag set to true.")]
        public void VersionControlInfo_PublishArtifactWithTrace_ReturnsArtifactInfoWithHasChangesTrue_200OK(BaseArtifactType artifactType)
        {
            //Setup
            IArtifact artifact = CreateArtifactWithTrace(artifactType, TraceDirection.From, _user);

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            //Execute
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, artifact.Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, basicArtifactInfo.Id);
            artifactDetails.AssertEquals(basicArtifactInfo);

            Assert.IsTrue(basicArtifactInfo.HasChanges.HasValue, "HasChanges property should have value");
            Assert.IsTrue(basicArtifactInfo.HasChanges.Value, "HasChanges property should be true");
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(182504)]
        [Description("Create & publish an artifact.  Create manual trace to the artifact with current user. Verify another user gets basic artifact information")]
        public void VersionControlInfo_PublishArtifactWithTrace_AnotherUserGetsBasicInfo_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType)
        {
            //Setup
            IUser anotherUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            IArtifact artifact = CreateArtifactWithTrace(artifactType, TraceDirection.From, _user);

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            // Execute            
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(anotherUser, artifact.Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, basicArtifactInfo.Id);
            artifactDetails.AssertEquals(basicArtifactInfo);

            VerifyBasicInformationResponse(basicArtifactInfo, hasChanges: false, isDeleted: false);
        }

        [TestCase(BaseArtifactType.Actor, 3)]
        [TestRail(182499)]
        [Description("Create, publish & lock an artifact.  Verify the basic artifact information for another user returned with HasChanges flag set to false.")]
        public void VersionControlInfo_PublishedAndLockedArtifact_NoChangesForAnotherUser_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType, int numberOfVersions)
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

            VerifyBasicInformationResponse(basicArtifactInfo, hasChanges: false, isDeleted: false);
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(182551)]
        [Description("Create, publish & lock an artifact.  Verify the basic artifact information returned with HasChanges flag set to true.")]
        public void VersionControlInfo_PublishedAndLockedArtifact_HasChanges_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            artifact.Lock();

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, artifact.Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            VerifyBasicInformationResponse(basicArtifactInfo, hasChanges: true, isDeleted: false);
        }
        #endregion Artifact Changes

        #region Sub-Artifact

        [TestCase(BaseArtifactType.UseCase)]
        [TestCase(BaseArtifactType.Process)]
        [TestRail(182512)]
        [Description("Create & save an artifact with sub-artifacts. Verify user gets basic artifact information with subartifact Id.")]
        public void VersionControlInfo_SavedArtifactWithSubArtifact_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _user, artifactType);

            List<INovaSubArtifact> subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, artifact.Id);

            Assert.IsTrue(subArtifacts.Count > 0, "There is no sub-artifact in this artifact");

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            // Execute
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, subArtifacts[0].Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            VerifyBasicInformationResponse(basicArtifactInfo, hasChanges: true, isDeleted: false, subArtifactId : subArtifacts[0].Id);
        }

        [TestCase(BaseArtifactType.UseCase, 2)]
        [TestCase(BaseArtifactType.Process, 3)]
        [TestRail(182544)]
        [Description("Create & publish multiple versions of an artifact with sub-artifacts.  Verify user gets basic artifact information with subartifact Id.")]
        public void VersionControlInfo_PublishedMultipleVersionsOfArtifactWithSubArtifacts_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType, int numberOfVersions)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, numberOfVersions : numberOfVersions);

            List<INovaSubArtifact> subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, artifact.Id);

            Assert.IsTrue(subArtifacts.Count > 0, "There is no sub-artifact in this artifact");

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            // Execute
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, subArtifacts[0].Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, basicArtifactInfo.Id);
            artifactDetails.AssertEquals(basicArtifactInfo);

            VerifyBasicInformationResponse(basicArtifactInfo, hasChanges: false, isDeleted: false, subArtifactId : subArtifacts[0].Id);
        }

        [TestCase]
        [TestRail(182606)]
        [Description("Create & publish an artifact.  Change sub-artifact & save. Verify another user gets basic artifact information with subartifact Id.")]
        public void VersionControlInfo_PublishedArtifactSubartifactUpdated_AnotherUserGetsBasicInfo_ReturnsArtifactInfo_200OK()
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

            VerifyBasicInformationResponse(basicArtifactInfo, hasChanges: false, isDeleted: false, subArtifactId: subArtifacts[0].Id);
        }

        [TestCase(BaseArtifactType.UseCase)]
        [TestCase(BaseArtifactType.Process)]
        [TestRail(182554)]
        [Description("Create, publish & lock an artifact with subartifact. Verify user gets basic artifact information with subartifact Id.")]
        public void VersionControlInfo_PublishedeSubArtifactLockedArtifact_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType)
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

            // Verify
            VerifyBasicInformationResponse(basicArtifactInfo, hasChanges: true, isDeleted: false, subArtifactId: subArtifacts[0].Id);
        }

        [TestCase(BaseArtifactType.UseCase)]
        [TestCase(BaseArtifactType.Process)]
        [TestRail(182555)]
        [Description("Create, publish & lock an artifact with subartifact. Verify another user gets basic artifact information with subartifact Id.")]
        public void VersionControlInfo_PublishedSubArtifactLockedArtifactAnotherUser_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType)
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

            // Verify
            VerifyBasicInformationResponse(basicArtifactInfo, hasChanges: false, isDeleted: false, subArtifactId: subArtifacts[0].Id);
        }

        #endregion Sub-Artifact

        #region Delete

        [TestCase(BaseArtifactType.Actor, 1)]
        [TestRail(182543)]
        [Description("Create, publish, delete & publish again an artifact. Verify user gets basic artifact information.")]
        public void VersionControlInfo_PublishedArtifactDeleted_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType, int numberOfVersions)
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

            VerifyBasicInformationResponse(basicArtifactInfo, hasChanges: false, isDeleted: true);
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(182563)]
        [Description("Create, publish & delete an artifact. Verify user gets basic artifact information.")]
        public void VersionControlInfo_PublishArtifactDeleted_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            artifact.Delete();

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            // Execute
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, artifact.Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify
            VerifyBasicInformationResponse(basicArtifactInfo, hasChanges: true, isDeleted: true);
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(182564)]
        [Description("Create, publish & delete an artifact. Another user gets basic information. Verify user gets basic artifact information.")]
        public void VersionControlInfo_PublishArtifactDeletedAndAccessedByAnotherUser_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            artifact.Delete();

            IUser anotherUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            // Execute
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(anotherUser, artifact.Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify
            VerifyBasicInformationResponse(basicArtifactInfo, hasChanges: false, isDeleted: false);
        }

        [TestCase(BaseArtifactType.UseCase)]
        [TestCase(BaseArtifactType.Process)]
        [TestRail(182565)]
        [Description("Create, publish & delete an artifact with sub-artifacts. Verify user gets basic artifact information.")]
        public void VersionControlInfo_PublishArtifactWithSubArtifactDeleted_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            List<INovaSubArtifact> subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, artifact.Id);
            Assert.IsTrue(subArtifacts.Count > 0, "There is no sub-artifact in this artifact");

            artifact.Delete();

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            // Execute
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, subArtifacts[0].Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify
            VerifyBasicInformationResponse(basicArtifactInfo, hasChanges: true, isDeleted: true, subArtifactId: subArtifacts[0].Id);
        }

        [TestCase(BaseArtifactType.UseCase)]
        [TestCase(BaseArtifactType.Process)]
        [TestRail(182593)]
        [Description("Create, publish & delete an artifact with sub-artifacts. Another user gets basic information. Verify basic artifact information.")]
        public void VersionControlInfo_PublishArtifactWithSubArtifactDeleted_AnotherUserGetsBasicInfo_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            List<INovaSubArtifact> subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, artifact.Id);
            Assert.IsTrue(subArtifacts.Count > 0, "There is no sub-artifact in this artifact");

            artifact.Delete();

            IUser anotherUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            // Execute
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(anotherUser, subArtifacts[0].Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify
            VerifyBasicInformationResponse(basicArtifactInfo, hasChanges: false, isDeleted: false, subArtifactId: subArtifacts[0].Id);
        }

        [TestCase]
        [TestRail(182601)]
        [Description("Create & publish process artifact.  Add to this artifact sub-artifact & publish. Verify user gets basic artifact information with subartifact Id.")]
        public void VersionControlInfo_PublishArtifact_CreateAndPublishSubArtifact_ReturnsArtifactInfo_200OK()
        {
            // Setup:

            // Create a Process artifact
            var processArtifact = Helper.Storyteller.CreateAndPublishProcessArtifact(project: _project, user: _user);

            // Get the process artifact
            var process = Helper.Storyteller.GetProcess(_user, processArtifact.Id);

            // Add UserTasks - iteration
            var precondition = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition task
            var processLink = process.GetOutgoingLinkForShape(precondition);

            var userTask = process.AddUserAndSystemTask(processLink);

            StorytellerTestHelper.UpdateVerifyAndPublishProcess(process, Helper.Storyteller, _user);

            // Get the process artifact
            process = Helper.Storyteller.GetProcess(_user, processArtifact.Id);

            userTask = process.GetProcessShapeByShapeName(userTask.Name);

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            // Execute
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, userTask.Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, basicArtifactInfo.Id);
            artifactDetails.AssertEquals(basicArtifactInfo);

            VerifyBasicInformationResponse(basicArtifactInfo, hasChanges: false, isDeleted: false, subArtifactId: userTask.Id);
        }

        [TestCase]
        [TestRail(182602)]
        [Description("Create & publish process artifact.  Add to this artifact sub-artifact & save. Verify user gets basic artifact information with subartifact Id.")]
        public void VersionControlInfo_PublishArtifact_CreateAndSaveSubArtifact_ReturnsArtifactInfo_200OK()
        {
            // Setup:

            // Create a Process artifact
            var processArtifact = Helper.Storyteller.CreateAndPublishProcessArtifact(project: _project, user: _user);

            // Get the process artifact
            var process = Helper.Storyteller.GetProcess(_user, processArtifact.Id);

            // Add UserTasks - iteration
            var precondition = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition task
            var processLink = process.GetOutgoingLinkForShape(precondition);

            var userTask = process.AddUserAndSystemTask(processLink);

            StorytellerTestHelper.UpdateAndVerifyProcess(process, Helper.Storyteller, _user);

            // Get the process artifact
            process = Helper.Storyteller.GetProcess(_user, processArtifact.Id);

            userTask = process.GetProcessShapeByShapeName(userTask.Name);

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            // Execute
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, userTask.Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify
            VerifyBasicInformationResponse(basicArtifactInfo, hasChanges: true, isDeleted: false, subArtifactId: userTask.Id);
        }

        [TestCase]
        [TestRail(182605)]
        [Description("Create & publish process artifact.  Add to this artifact sub-artifact & publish. Verify user gets basic artifact information with subartifact Id.")]
        public void VersionControlInfo_PublishArtifact_CreateAndPublishSubArtifact_AnotherUserReturnsArtifactInfo_200OK()
        {
            // Setup:

            // Create a Process artifact
            var processArtifact = Helper.Storyteller.CreateAndPublishProcessArtifact(project: _project, user: _user);

            // Get the process artifact
            var process = Helper.Storyteller.GetProcess(_user, processArtifact.Id);

            // Add UserTasks - iteration
            var precondition = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition task
            var processLink = process.GetOutgoingLinkForShape(precondition);

            var userTask = process.AddUserAndSystemTask(processLink);

            StorytellerTestHelper.UpdateVerifyAndPublishProcess(process, Helper.Storyteller, _user);

            // Get the process artifact
            process = Helper.Storyteller.GetProcess(_user, processArtifact.Id);

            userTask = process.GetProcessShapeByShapeName(userTask.Name);

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            IUser anotherUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            // Execute
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(anotherUser, userTask.Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, basicArtifactInfo.Id);
            artifactDetails.AssertEquals(basicArtifactInfo);

            VerifyBasicInformationResponse(basicArtifactInfo, hasChanges: false, isDeleted: false, subArtifactId: userTask.Id);
        }

        #endregion Delete

        #endregion 200 OK tests

        #region Negative tests

        #region Bad request

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(182607)]
        [Description("Create & publish an artifact.  Send no token in the request.  Verify 400 Bad Request is returned.")]
        public void VersionControlInfo_PublishArtifact__NoTokenHeader_400Badrequest(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            IUser userWithNoToken = Helper.CreateUserAndAddToDatabase();

            // Execute & Verify:
            var ex = Assert.Throws <Http400BadRequestException>(() => Helper.ArtifactStore.GetVersionControlInfo(userWithNoToken, artifact.Id),
                "'GET {0}' should return 400 Bad Request when token is not passed!", SVC_PATH);

            string expectedExceptionMessage = "Token is missing or malformed.";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "{0} when user tries to get basic information and not send token in a header", expectedExceptionMessage);
        }

        #endregion Bad request

        #region Unauthorized

        [TestRail(182858)]
        [TestCase(BaseArtifactType.Actor, "Alex")]
        [TestCase(BaseArtifactType.Actor, "00000000-0000-0000-0000-000000000000")]
        [Description("Create & publish an artifact.  Send invalid token in the request.  Verify 401 Unauthorized is returned.")]
        public void VersionControlInfo_PublishArtifact__InvalidTokenHeader_401UnAuthorized(BaseArtifactType artifactType, string malformedToken)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            _user.SetToken(malformedToken);

            // Execute & Verify:
            var ex = Assert.Throws<Http401UnauthorizedException>(() => Helper.ArtifactStore.GetVersionControlInfo(_user, artifact.Id),
                "'GET {0}' should return 401 Unauthorized when passed invalid token!", SVC_PATH);

            string expectedExceptionMessage = "Token is invalid.";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "{0} when user tries to get basic information and send invalid token in a header", expectedExceptionMessage);
        }

        #endregion Unauthorized

        #region Forbidden

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182859)]
        [Description("Create & publish an artifact. User without permissions tries to access basic artifact information.  Verify returned code 403 Forbidden.")]
        public void VersionControlInfo_PublishArtifact_UserWithoutProperPermissions_403Forbidden(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            IUser userWithoutPermissions = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken,
                InstanceAdminRole.BlueprintAnalytics);

            // Execute & Verify:
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.GetVersionControlInfo(userWithoutPermissions, artifact.Id),
                "'GET {0}' should return 403 Forbidden when user without permissions tries to access basic artifact information!", SVC_PATH);

            string expectedExceptionMessage = "User does not have permissions for Artifact (Id:" + artifact.Id + ").";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "{0} when user without permissions tries to get basic artifact information", expectedExceptionMessage);
        }
        // TODO: Call GetVersionControlInfo with an artifact the git user doesn't have access to.  Verify 403 Forbiden.

        #endregion Forbidden

        #region Not found

        [TestCase(int.MaxValue)]
        [TestRail(0)]
        [Description("User tries to get basic information of artifact that does not exist.  Verify returned code 404 Not Found.")]
        public void VersionControlInfo_CannotFindNonExistingArtifact_404NotFound(int artifactId)
        {
            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetVersionControlInfo(_user, artifactId),
                 "'GET {0}' should return 404 Not found when user tries to access basic artifact information of artifact that does not exist!", SVC_PATH);

            // Verify:
            string expectedExceptionMessage = "Item (Id:" + artifactId + ") is not found.";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "{0} when user tries to get basic information of artifact that does not exist", expectedExceptionMessage);
        }

        [TestCase(1)]
        [TestRail(0)]
        [Description("User tries to get basic information of artifact that does not exist.  Verify returned code 404 Not Found.")]
        public void VersionControlInfo_CannotFindNonExistingArtifact_404NotFound_Id0(int artifactId)
        {
            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetVersionControlInfo(_user, artifactId),
                 "'GET {0}' should return 404 Not found when user tries to access basic artifact information of artifact that does not exist!", SVC_PATH);

            // Verify:
            string expectedExceptionMessage = "Item (Id:" + artifactId + ") is not found.";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "{0} when user tries to get basic information of artifact that does not exist", expectedExceptionMessage);
        }

        // TODO: Call GetVersionControlInfo with a non-existent artifact ID.  Verify 404 Not Found.
        // TODO: Call GetVersionControlInfo with an unpublished artifact with a different user.  Verify 404 Not Found.
        // TODO: Call GetVersionControlInfo with an unpublished sub-artifact of a published artifact with a different user.  Verify 404 Not Found.
        // TODO: Call GetVersionControlInfo with an unpublished sub-artifact of an unpublished artifact with a different user.  Verify 404 Not Found.

        #endregion Not found

        #endregion Negative tests

        #region private calls

        /// <summary>
        /// Creates two artifacts and adds trace by user specified in parameteres
        /// </summary>
        /// <param name="artifactType">Artifact type of artifacts to be created</param>
        /// <param name="trace">Direction of trace</param>
        /// <param name="user"></param>
        /// <returns>Artifact with the trace</returns>
        private IArtifact CreateArtifactWithTrace(BaseArtifactType artifactType, TraceDirection trace, IUser user)
        {
            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(_project, user, artifactType);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_project, user, BaseArtifactType.UseCase);

            OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact,
                    targetArtifact, trace, user);

            return sourceArtifact;
        }

        /// <summary>
        /// Verifies if returned basic information gets proper values for hasChanges, isDeleted and subArtifactId properties
        /// </summary>
        /// <param name="basicArtifactInfo">Returned basic information about artifact</param>
        /// <param name="hasChanges">Indicator of changes in an artifact</param>
        /// <param name="isDeleted">Indicator if artifact was deleted</param>
        /// <param name="subArtifactId">Id of requested subartifact</param>
        private static void VerifyBasicInformationResponse(INovaVersionControlArtifactInfo basicArtifactInfo, bool hasChanges, bool isDeleted, int? subArtifactId = null)
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
                Assert.IsNotNull(basicArtifactInfo.LockedByUser.Id, "LockedDateTime should have value");
                Assert.IsNotNull(basicArtifactInfo.LockedDateTime, "LockedDateTime should have value");
            }

            if (basicArtifactInfo.SubArtifactId != null)
            {
                Assert.AreEqual(subArtifactId, basicArtifactInfo.SubArtifactId.Value, "Sub-artifact Id is " + basicArtifactInfo.SubArtifactId.Value + " and different from expected " + subArtifactId);
            }
        }

        #endregion private calls
    }
}
