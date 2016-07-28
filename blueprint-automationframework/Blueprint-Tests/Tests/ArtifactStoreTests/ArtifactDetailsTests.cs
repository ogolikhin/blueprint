using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.Impl;
using NUnit.Framework;
using TestCommon;
using Utilities;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class ArtifactDetailsTests : TestBase
    {
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

        [TestCase(BaseArtifactType.Actor)]
        [TestCase(BaseArtifactType.BusinessProcess)]
        [TestCase(BaseArtifactType.Document)]
        [TestCase(BaseArtifactType.DomainDiagram)]
        [TestCase(BaseArtifactType.GenericDiagram)]
        [TestCase(BaseArtifactType.Glossary)]
        [TestCase(BaseArtifactType.PrimitiveFolder)]
        [TestCase(BaseArtifactType.Process)]
        [TestCase(BaseArtifactType.Storyboard)]
        [TestCase(BaseArtifactType.TextualRequirement)]
        [TestCase(BaseArtifactType.UIMockup)]
        [TestCase(BaseArtifactType.UseCase)]
        [TestCase(BaseArtifactType.UseCaseDiagram)]
        [TestRail(154601)]
        [Description("Create & publish an artifact, GetArtifactDetails.  Verify the artifact details are returned.")]
        public void GetArtifactDetails_PublishedArtifactId_ReturnsArtifactDetails(BaseArtifactType artifactType)
        {
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            var retrievedArtifact = OpenApiArtifact.GetArtifact(artifact.Address, _project, artifact.Id, _user);

            ArtifactDetails artifactDetails = null;

            Assert.DoesNotThrow(() =>
            {
                artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            artifactDetails.AssertEquals(retrievedArtifact);

            Assert.IsEmpty(artifactDetails.CustomProperties,
                "We found Custom Properties in an artifact that shouldn't have any Custom Properties!");

            Assert.IsEmpty(artifactDetails.SpecificPropertyValues,
                "SpecificPropertyValues isn't implemented yet so it should be empty!");

            Assert.AreEqual(8159, artifactDetails.Permissions, "Instance Admin should have all permissions (i.e. 8159)!");
        }

        [TestCase(BaseArtifactType.Process)]
        [Explicit(IgnoreReasons.UnderDevelopment)]  // XXX: Currently fails with a 501 Not Implemented error.
        [TestRail(154700)]
        [Description("Create & publish an artifact, modify & publish it again, GetArtifactDetails with versionId=1.  Verify the artifact details for the first version are returned.")]
        public void GetArtifactDetailsWithVersionId1_PublishedArtifactWithMultipleVersions_ReturnsArtifactDetails(BaseArtifactType artifactType)
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

            ArtifactDetails artifactDetails = null;

            Assert.DoesNotThrow(() =>
            {
                artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id, versionId: 1);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            artifactDetails.AssertEquals(retrievedArtifactVersion1);

            Assert.IsEmpty(artifactDetails.CustomProperties,
                "We found Custom Properties in an artifact that shouldn't have any Custom Properties!");

            Assert.IsEmpty(artifactDetails.SpecificPropertyValues,
                "SpecificPropertyValues isn't implemented yet so it should be empty!");

            Assert.AreEqual(8159, artifactDetails.Permissions, "Instance Admin should have all permissions (i.e. 8159)!");
        }
    }
}