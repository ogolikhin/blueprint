﻿using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using System.Collections.Generic;
using TestCommon;

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

        [TestCase(BaseArtifactType.Actor, 1)]
        [TestCase(BaseArtifactType.Process, 2)]
        [TestCase(BaseArtifactType.UseCase, 3)]
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

            VerifyBasicInformationResponce(basicArtifactInfo, hasChanges : false, isDeleted : false);
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
            VerifyBasicInformationResponce(basicArtifactInfo, hasChanges: true, isDeleted: false);
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(182500)]
        [Description("Create, publish & save an artifact.  Create manual trace to the artifact using another user. Verify another user gets basic artifact information with HasChanges flag set to true.")]
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
        [Description("Create, publish & save an artifact.  Create manual trace to the artifact with current user. Verify another user gets basic artifact information with HasChanges flag set to false.")]
        public void VersionControlInfo_PublishArtifactWithTraceGetBasicInfoWithDifferentUser_ReturnsArtifactInfoWithHasChangesTrue_200OK(BaseArtifactType artifactType)
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

            VerifyBasicInformationResponce(basicArtifactInfo, hasChanges: false, isDeleted: false);
        }

        [TestCase(BaseArtifactType.Actor, 1)]
        [TestCase(BaseArtifactType.Process, 2)]
        [TestCase(BaseArtifactType.UseCase, 3)]
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

            VerifyBasicInformationResponce(basicArtifactInfo, hasChanges: false, isDeleted: false);
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
            VerifyBasicInformationResponce(basicArtifactInfo, hasChanges: true, isDeleted: false);
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

            VerifyBasicInformationResponce(basicArtifactInfo, hasChanges: true, isDeleted: false, subArtifactId : subArtifacts[0].Id);
        }

        [TestCase(BaseArtifactType.UseCase)]
        [TestCase(BaseArtifactType.Process)]
        [TestRail(182544)]
        [Description("Create & publish an artifact with sub-artifacts.  Save and publish artifact. Verify user gets basic artifact information with subartifact Id.")]
        public void VersionControlInfo_PublishedArtifactWithSubArtifactsSavedAndPublished_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            List<INovaSubArtifact> subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, artifact.Id);

            Assert.IsTrue(subArtifacts.Count > 0, "There is no sub-artifact in this artifact");

            artifact.Save();
            artifact.Publish();

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            // Execute
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, subArtifacts[0].Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, basicArtifactInfo.Id);
            artifactDetails.AssertEquals(basicArtifactInfo);

            VerifyBasicInformationResponce(basicArtifactInfo, hasChanges: false, isDeleted: false, subArtifactId: subArtifacts[0].Id);
        }

        [Ignore(IgnoreReasons.UnderDevelopment)] // Update artifact with sub-artifact is not implemented yet (Better to avoid artifact.Sae() in this test since Save changes also artifact description.
        [Description("Create & save an artifact.  Add to this artifact sub-artifact & save. Verify user gets basic artifact information with subartifact Id.")]
        public void VersionControlInfo_CreateAndSaveSubArtifactInPublishedArtifactForAnotherUser_ReturnsArtifactInfo_200OK()
        {
            // Setup:

            // Create a Process artifact
            var processArtifact = Helper.Storyteller.CreateAndSaveProcessArtifact(project: _project, user: _user);

            // Get the process artifact
            var process = Helper.Storyteller.GetProcess(_user, processArtifact.Id);

            // Add UserTasks - iteration
            var precondition = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition task
            var processLink = process.GetOutgoingLinkForShape(precondition);

            var userTask = process.AddUserAndSystemTask(processLink);

            NovaArtifactDetails retrievedArtifact = Helper.ArtifactStore.GetArtifactDetails(_user, processArtifact.Id);
            retrievedArtifact.Name = "Sub-artifact_" + retrievedArtifact.Name;
            Artifact.UpdateArtifact(processArtifact, _user, retrievedArtifact, Helper.BlueprintServer.Address);

            IUser anotherUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            INovaVersionControlArtifactInfo basicArtifactInfo = null;

            // Execute
            Assert.DoesNotThrow(() => basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(anotherUser, userTask.Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, basicArtifactInfo.Id);
            artifactDetails.AssertEquals(basicArtifactInfo);

            Assert.IsNotNull(basicArtifactInfo.SubArtifactId, "There is no sub-artifact id in the returned basic artifact info responce");
            Assert.AreEqual(userTask.Id, basicArtifactInfo.SubArtifactId, "Sub-artifact Id in Basic Artifact info is different from Id of sub-artifact sent in get artifact base infor request");

            Assert.IsTrue(basicArtifactInfo.HasChanges.HasValue, "HasChanges property should be null");
            Assert.IsFalse(basicArtifactInfo.HasChanges.Value, "HasChanges property should be false");
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
            VerifyBasicInformationResponce(basicArtifactInfo, hasChanges: true, isDeleted: false, subArtifactId: subArtifacts[0].Id);
        }

        [TestCase(BaseArtifactType.UseCase)]
        [TestCase(BaseArtifactType.Process)]
        [TestRail(182555)]
        [Description("Create, publish & lock an artifact with subartifact. Verify another user gets basic artifact information with subartifact Id.")]
        public void VersionControlInfo_PublishedeSubArtifactLockedArtifactAnotherUser_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType)
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
            VerifyBasicInformationResponce(basicArtifactInfo, hasChanges: false, isDeleted: false, subArtifactId: subArtifacts[0].Id);
        }

        #endregion Sub-Artifact
        // TODO: Published deleted Artifact(- isDeleted= false).
        // TODO: Saved deleted Artifact.
        // TODO: Saved deleted Artifact for another user.
        // TODO: Sub-Artifact in published deleted Artifact(- isDeleted= false).
        // TODO: Sub-Artifact in saved deleted Artifact.
        // TODO: Published deleted Sub-artifact in live Artifact.
        // TODO: Saved deleted Sub-artifact in live Artifact.
        // TODO: Saved deleted Sub-artifact in live Artifact for another user.

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
        private static void VerifyBasicInformationResponce(INovaVersionControlArtifactInfo basicArtifactInfo, bool hasChanges, bool isDeleted, int? subArtifactId = null)
        {
            Assert.IsTrue(basicArtifactInfo.HasChanges.HasValue, "HasChanges property should have value");
            Assert.AreEqual(basicArtifactInfo.HasChanges.Value, hasChanges, "HasChanges property should be " + hasChanges.ToString());

            Assert.IsTrue(basicArtifactInfo.IsDeleted.HasValue, "IsDeleted property should have value");
            Assert.AreEqual(basicArtifactInfo.IsDeleted.Value, isDeleted, "HasChanges property should be " + isDeleted.ToString());

            if (hasChanges == true)
            {
                Assert.IsNotNull(basicArtifactInfo.LockedByUser, "LockedDateTime should have value");
                Assert.IsNotNull(basicArtifactInfo.LockedDateTime, "LockedDateTime should have value");
            }

            if (subArtifactId != null)
                Assert.AreEqual(basicArtifactInfo.SubArtifactId.Value, subArtifactId, "HasChanges property should be " + isDeleted.ToString());
        }

        #endregion private calls
    }
}
