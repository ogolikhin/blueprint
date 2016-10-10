using Common;
using CustomAttributes;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using NUnit.Framework;
using TestCommon;
using Helper;

namespace CommonServiceTests
{
    public class PublishTests : TestBase
    {
        private IUser _user;
        private IProject _project;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_user);
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            Helper?.Dispose();
        }

        #endregion
                
        [TestCase(BaseArtifactType.Process)]
        [TestRail(125503)]
        [Description("Create, save, publish Process artifact, check returned results.")]
        public void Publish_SavedArtifact_PublishWasSuccessful(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact = Helper.CreateArtifact(_project, _user, artifactType);
            artifact.Save(_user);
            
            NovaPublishArtifactResult publishResult = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                publishResult = artifact.StorytellerPublish(_user);
            }, "Publish failed when publishing a saved artifact!");

            // Verify:
            Assert.AreEqual(publishResult.StatusCode, NovaPublishArtifactResult.Result.Success);

            const string expectedMessage = "Successfully published";
            Assert.AreEqual(expectedMessage, publishResult.Message);
        }

        [TestCase]
        [TestRail(125504)]
        [Description("Create, save, publish Process artifact, publish again, check returned results.")]
        public void Publish_PublishedArtifactWithNoDraftChanges_ArtifactAlreadyPublishedMessage()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);

            NovaPublishArtifactResult publishResult = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                publishResult = artifact.StorytellerPublish(_user);
            }, "Running Publish with a published artifact should return 200 OK!");

            // Verify:
            Assert.AreEqual(NovaPublishArtifactResult.Result.ArtifactAlreadyPublished, publishResult.StatusCode);

            string expectedMessage = I18NHelper.FormatInvariant("Artifact {0} is already published in the project", artifact.Id);
            Assert.AreEqual(expectedMessage, publishResult.Message);
        }
    }
}