using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.Impl;
using NUnit.Framework;
using TestCommon;

namespace ArtifactStoreTests
{
    [Explicit(IgnoreReasons.UnderDevelopment)]  // Dev hasn't finished the story yet.
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class CopyArtifactTests : TestBase
    {
        private const string SVC_PATH = RestPaths.Svc.ArtifactStore.Artifacts_id_.COPY_TO_id_;

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

            Assert.DoesNotThrow(() => copyResult = Helper.ArtifactStore.CopyArtifact(sourceArtifact, targetArtifact, author),
                "'POST {0}' should return 201 Created when valid parameters are passed.", SVC_PATH);

            // Verify:
            Assert.NotNull(copyResult, "The result returned from CopyArtifact() shouldn't be null!");
            Assert.NotNull(copyResult.Artifact, "The Artifact property returned by CopyArtifact() shouldn't be null!");
            Assert.AreEqual(-1, copyResult.Artifact.Version, "Version of a copied artifact should always be -1 (i.e. not published)!");
            Assert.AreEqual(1, copyResult.CopiedArtifactsCount,
                "There should be exactly 1 artifact copied, but the result reports {0} were copied.", copyResult.CopiedArtifactsCount);
            Assert.AreNotEqual(sourceArtifact.Id, copyResult.Artifact.Id,
                "The ID of the copied artifact should not be the same as the original artifact!");

            ArtifactStoreHelper.AssertArtifactsEqual(copyResult.Artifact, sourceArtifact, skipIdAndVersion: true);

            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, copyResult.Artifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, copyResult.Artifact);
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

            // Execute:
            CopyNovaArtifactResultSet copyResult = null;

            Assert.DoesNotThrow(() => copyResult = ArtifactStore.CopyArtifact(Helper.ArtifactStore.Address, sourceArtifact, _project.Id, author),
                "'POST {0}' should return 201 Created when valid parameters are passed.", SVC_PATH);

            // Verify:
            Assert.NotNull(copyResult, "The result returned from CopyArtifact() shouldn't be null!");
            Assert.NotNull(copyResult.Artifact, "The Artifact property returned by CopyArtifact() shouldn't be null!");
            Assert.AreEqual(-1, copyResult.Artifact.Version, "Version of a copied artifact should always be -1 (i.e. not published)!");
            Assert.AreEqual(1, copyResult.CopiedArtifactsCount,
                "There should be exactly 1 artifact copied, but the result reports {0} were copied.", copyResult.CopiedArtifactsCount);
            Assert.AreNotEqual(sourceArtifact.Id, copyResult.Artifact.Id,
                "The ID of the copied artifact should not be the same as the original artifact!");

            ArtifactStoreHelper.AssertArtifactsEqual(copyResult.Artifact, sourceArtifact, skipIdAndVersion: true);

            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, copyResult.Artifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, copyResult.Artifact);
        }

        [TestCase(BaseArtifactType.TextualRequirement)]
        [TestRail(191049)]
        [Description("Create and publish an artifact and a folder.  Add an attachment to the artifact.  Copy the artifact into the folder.  Verify the source " +
            "artifact is unchanged and the new artifact is identical to the source artifact.  New copied artifact should not be published.")]
        public void CopyArtifact_SinglePublishedArtifactWithAttachment_ToNewFolder_ReturnsNewArtifactWithAttachment(BaseArtifactType artifactType)
        {
            Assert.Fail("Test not implemented yet.");
        }

        [TestCase(BaseArtifactType.Actor, TraceDirection.From, false)]
        [TestCase(BaseArtifactType.Glossary, TraceDirection.To, true)]
        [TestRail(191050)]
        [Description("Create and publish an artifact and a folder.  Add a manual trace between the artifact & folder.  Copy the artifact into the folder.  " +
            "Verify the source artifact is unchanged and the new artifact (and trace) is identical to the source artifact.  New copied artifact should not be published.")]
        public void CopyArtifact_SinglePublishedArtifactWithManualTrace_ToNewFolder_ReturnsNewArtifactWithManualTrace(
            BaseArtifactType artifactType, TraceDirection direction, bool isSuspect)
        {
            Assert.Fail("Test not implemented yet.");
        }

        [Category(Categories.CustomData)]
        [TestCase(BaseArtifactType.TextualRequirement)]
        [TestRail(191051)]
        [Description("Create and publish a folder.  Copy a reused artifact into the folder.  Verify the source artifact is unchanged and the new artifact " +
            "is identical to the source artifact (except no Reuse relationship).  New copied artifact should not be published.")]
        public void CopyArtifact_SinglePublishedReusedArtifact_ToNewFolder_ReturnsNewArtifactNotReused(BaseArtifactType artifactType)
        {
            Assert.Fail("Test not implemented yet.");
        }

        [Category(Categories.CustomData)]
        [TestCase(BaseArtifactType.Actor)]
        [TestRail(191052)]
        [Description("Create and publish an artifact (that has custom properties) and a folder.  Copy the artifact into the folder.  Verify the source artifact is unchanged " +
            "and the new artifact is identical to the source artifact (including custom properties).  New copied artifact should not be published.")]
        public void CopyArtifact_SinglePublishedArtifactWithCustomProperties_ToNewFolder_ReturnsNewArtifactWithCustomProperties(BaseArtifactType artifactType)
        {
            Assert.Fail("Test not implemented yet.");
        }

        #endregion 201 Created tests

    }
}
