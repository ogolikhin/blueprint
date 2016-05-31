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
        public void PublishArtifact_VerifyPublishWasSuccessful(BaseArtifactType artifactType)
        {
            var artifact = Helper.CreateArtifact(_project, _user, artifactType);
            artifact.Save(_user);
            
            NovaPublishArtifactResult publishResult = null;
            Assert.DoesNotThrow(() =>
                {
                    publishResult = artifact.NovaPublish(_user);
                }, "Publish must throw no errors.");
                Assert.AreEqual(publishResult.StatusCode, NovaPublishArtifactResult.Result.Success);
                string expectedMessage = "Successfully published";
                Assert.AreEqual(expectedMessage, publishResult.Message);
        }

        [TestCase]
        [TestRail(125504)]
        [Description("Create, save, publish Process artifact, publish again, check returned results.")]
        public void PublishArtifactWhenNothingToPublish_VerifyArtifactAlreadyPublishedMessage()
        {
            var artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Process);
            artifact.Save(_user);
            artifact.Publish(_user);

            NovaPublishArtifactResult publishResult = null;
            Assert.DoesNotThrow(() =>
                {
                    publishResult = artifact.NovaPublish(_user);
                }, "Publish must throw no errors.");
                Assert.AreEqual(NovaPublishArtifactResult.Result.ArtifactAlreadyPublished, publishResult.StatusCode);
                string expectedMessage = I18NHelper.FormatInvariant("Artifact {0} is already published in the project", artifact.Id);
                Assert.AreEqual(expectedMessage, publishResult.Message);
        }
    }
}