using CustomAttributes;
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

            INovaVersionControlArtifactInfo artifactBaseInfo = null;

            // Execute:
            Assert.DoesNotThrow(() => artifactBaseInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, artifact.Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);

            artifactDetails.AssertEquals(artifactBaseInfo);

            Assert.IsTrue(artifactBaseInfo.HasChanges.HasValue, "HasChanges property should have value");
            Assert.IsFalse(artifactBaseInfo.HasChanges.Value, "HasChanges property should be false");
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(182453)]
        [Description("Create & save an artifact.  Verify the basic artifact information returned with HasChanges flag set to true.")]
        public void VersionControlInfo_SavedArtifact_HasChanges_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndSaveArtifact(_project, _user, artifactType);

            INovaVersionControlArtifactInfo artifactBaseInfo = null;

            // Execute:
            Assert.DoesNotThrow(() => artifactBaseInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, artifact.Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            Assert.IsTrue(artifactBaseInfo.HasChanges.HasValue, "HasChanges property should be to null");
            Assert.IsTrue(artifactBaseInfo.HasChanges.Value, "HasChanges property should be true");

            Assert.IsNotNull(artifactBaseInfo.LockedDateTime, "LockedDateTime should have value");
            Assert.AreEqual(artifactBaseInfo.LockedByUser.Id, _user.Id, "GetArtifactDetails should have the same Id as GetVersionControlInfo");
        }

        [TestCase(BaseArtifactType.Actor, 1)]
        [TestCase(BaseArtifactType.Process, 2)]
        [TestCase(BaseArtifactType.UseCase, 3)]
        [TestRail(182499)]
        [Description("Create, publish & save an artifact.  Verify the basic artifact information for another user returned with HasChanges flag set to false.")]
        public void VersionControlInfo_PublishedArtifact_NoChangesForAnotherUser_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType, int numberOfVersions)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, numberOfVersions: numberOfVersions);
            artifact.Save(_user);

            IUser anotherUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            INovaVersionControlArtifactInfo artifactBaseInfo = null;

            // Execute:
            Assert.DoesNotThrow(() => artifactBaseInfo = Helper.ArtifactStore.GetVersionControlInfo(anotherUser, artifact.Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(anotherUser, artifact.Id);

            artifactDetails.AssertEquals(artifactBaseInfo);

            Assert.IsTrue(artifactBaseInfo.HasChanges.HasValue, "HasChanges property should be null");
            Assert.IsFalse(artifactBaseInfo.HasChanges.Value, "HasChanges property should be false");
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(182500)]
        [Description("Create, publish & save an artifact.  Create manual trace to the artifact using another user. Verify another user gets basic artifact information with HasChanges flag set to true.")]
        public void VersionControlInfo_CreateTraceForArtifact_ReturnsArtifactInfoWithHasChangesTrue_200OK(BaseArtifactType artifactType)
        {
            //Setup
            IArtifact artifact = CreateArtifactWithTrace(artifactType, TraceDirection.From, _user);

            //Execute
            var basicArtifactInfo = GetArtifactBaseInfo(artifact.Id, _user);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, basicArtifactInfo.Id);
            artifactDetails.AssertEquals(basicArtifactInfo);

            Assert.IsTrue(basicArtifactInfo.HasChanges.HasValue, "HasChanges property should be null");
            Assert.IsTrue(basicArtifactInfo.HasChanges.Value, "HasChanges property should be true");
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(182504)]
        [Description("Create, publish & save an artifact.  Create manual trace to the artifact with current user. Verify another user gets basic artifact information with HasChanges flag set to false.")]
        public void VersionControlInfo_CreateTraceForArtifactByDifferentUser_ReturnsArtifactInfoWithHasChangesTrue_200OK(BaseArtifactType artifactType)
        {
            //Setup
            IUser anotherUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            IArtifact artifact = CreateArtifactWithTrace(artifactType, TraceDirection.From, _user);

            //Execute
            var basicArtifactInfo = GetArtifactBaseInfo(artifact.Id, anotherUser);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, basicArtifactInfo.Id);
            artifactDetails.AssertEquals(basicArtifactInfo);

            Assert.IsTrue(basicArtifactInfo.HasChanges.HasValue, "HasChanges property should be null");
            Assert.IsFalse(basicArtifactInfo.HasChanges.Value, "HasChanges property should be false");
        }

        #endregion Artifact Changes

        #region Sub-Artifact

        [TestCase(BaseArtifactType.UseCase)]
        [TestCase(BaseArtifactType.Process)]
        [TestRail(182512)]
        [Description("Create & publish an artifact.  Add to this artifact sub-artifact & save. Verify user gets basic artifact information with subartifact Id.")]
        public void VersionControlInfo_CreateAndSaveSubArtifactInPublishedArtifact_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            List<INovaSubArtifact> subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, artifact.Id);

            Assert.IsTrue(subArtifacts.Count > 0, "There is no sub-artifact in this artifact");
            subArtifacts[0].DisplayName = "Sub-artifact";

            artifact.Save();

            // Execute
            var basicArtifactInfo = GetArtifactBaseInfo(subArtifacts[0].Id, _user);

            // Verify
            Assert.IsNotNull(basicArtifactInfo.SubArtifactId, "There is no sub-artifact id in the returned basic artifact info responce");
            Assert.AreEqual(subArtifacts[0].Id, basicArtifactInfo.SubArtifactId, "Sub-artifact Id in Basic Artifact info is different from Id of sub-artifact sent in get artifact base infor request");

            Assert.IsTrue(basicArtifactInfo.HasChanges.HasValue, "HasChanges property should be null");
            Assert.IsTrue(basicArtifactInfo.HasChanges.Value, "HasChanges property should be true");
        }

        [TestCase(BaseArtifactType.UseCase)]
        [TestCase(BaseArtifactType.Process)]
        [TestRail(182543)]
        [Description("Create & save an artifact.  Add to this artifact sub-artifact & save. Verify user gets basic artifact information with subartifact Id.")]
        public void VersionControlInfo_CreateAndSaveSubArtifactInSavedArtifact_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            List<INovaSubArtifact> subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, artifact.Id);

            Assert.IsTrue(subArtifacts.Count > 0, "There is no sub-artifact in this artifact");
            subArtifacts[0].DisplayName = "Sub-artifact";

            artifact.Save();

            // Execute
            var basicArtifactInfo = GetArtifactBaseInfo(subArtifacts[0].Id, _user);

            // Verify
            Assert.IsNotNull(basicArtifactInfo.SubArtifactId, "There is no sub-artifact id in the returned basic artifact info responce");
            Assert.AreEqual(subArtifacts[0].Id, basicArtifactInfo.SubArtifactId, "Sub-artifact Id in Basic Artifact info is different from Id of sub-artifact sent in get artifact base infor request");

            Assert.IsTrue(basicArtifactInfo.HasChanges.HasValue, "HasChanges property should be null");
            Assert.IsTrue(basicArtifactInfo.HasChanges.Value, "HasChanges property should be true");
        }

        [TestCase(BaseArtifactType.UseCase)]
        [TestCase(BaseArtifactType.Process)]
        [TestRail(182544)]
        [Description("Create & save an artifact.  Add to this artifact sub-artifact & save. Verify user gets basic artifact information with subartifact Id.")]
        public void VersionControlInfo_CreateAndPublishSubArtifactInPublishedArtifact_ReturnsArtifactInfo_200OK(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            List<INovaSubArtifact> subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, artifact.Id);

            Assert.IsTrue(subArtifacts.Count > 0, "There is no sub-artifact in this artifact");
            subArtifacts[0].DisplayName = "Sub-artifact";

            artifact.Save();
            artifact.Publish();

            // Execute
            var basicArtifactInfo = GetArtifactBaseInfo(subArtifacts[0].Id, _user);

            // Verify
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, basicArtifactInfo.Id);
            artifactDetails.AssertEquals(basicArtifactInfo);

            Assert.IsNotNull(basicArtifactInfo.SubArtifactId, "There is no sub-artifact id in the returned basic artifact info responce");
            Assert.AreEqual(subArtifacts[0].Id, basicArtifactInfo.SubArtifactId, "Sub-artifact Id in Basic Artifact info is different from Id of sub-artifact sent in get artifact base infor request");

            Assert.IsTrue(basicArtifactInfo.HasChanges.HasValue, "HasChanges property should be null");
            Assert.IsFalse(basicArtifactInfo.HasChanges.Value, "HasChanges property should be false");
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

            // Execute
            var basicArtifactInfo = GetArtifactBaseInfo(userTask.Id, anotherUser);

            // Verify
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, basicArtifactInfo.Id);
            artifactDetails.AssertEquals(basicArtifactInfo);

            Assert.IsNotNull(basicArtifactInfo.SubArtifactId, "There is no sub-artifact id in the returned basic artifact info responce");
            Assert.AreEqual(userTask.Id, basicArtifactInfo.SubArtifactId, "Sub-artifact Id in Basic Artifact info is different from Id of sub-artifact sent in get artifact base infor request");

            Assert.IsTrue(basicArtifactInfo.HasChanges.HasValue, "HasChanges property should be null");
            Assert.IsFalse(basicArtifactInfo.HasChanges.Value, "HasChanges property should be false");
        }

        // TODO: Unpublished Sub-artifact in published artifact.
        // TODO: Unpublished Sub-artifact in unpublished artifact.
        // TODO: Published Sub-artifact.
        // TODO: Published Sub-artifact in locked Artifact without changes.
        // TODO: Published Sub-artifact in locked Artifact with changes.
        // TODO: Published Sub-artifact in locked Artifact without changes for another user.
        // TODO: Published Sub-artifact in locked Artifact with changes for another user.

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
        /// Gets Artifact Base Information from artifact with specific Id
        /// </summary>
        /// <param name="artifactId">The Id of artifact</param>
        /// <param name="user">User who tries to get basic artifact information</param>
        /// <returns>Basic artifact information</returns>
        private INovaVersionControlArtifactInfo GetArtifactBaseInfo(int artifactId, IUser user)
        {
            INovaVersionControlArtifactInfo artifactBaseInfo = null;

            // Execute:
            Assert.DoesNotThrow(() => artifactBaseInfo = Helper.ArtifactStore.GetVersionControlInfo(user, artifactId),
                    "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            return artifactBaseInfo;
        }

        #endregion private calls
    }
}
