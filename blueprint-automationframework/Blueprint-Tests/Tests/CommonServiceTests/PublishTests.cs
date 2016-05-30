using Common;
using CustomAttributes;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using NUnit.Framework;
using System.Net;
using System.Collections.Generic;
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
            _user = Helper.CreateUserAndAddToDatabase();
            _project = ProjectFactory.GetProject(_user);
            ISession session = Helper.AdminStore.AddSession(_user.Username, _user.Password);
            _user.SetToken(session.SessionId);

            Helper.BlueprintServer.LoginUsingBasicAuthorization(_user, string.Empty);

            Assert.IsFalse(string.IsNullOrWhiteSpace(_user.Token.OpenApiToken), "The user didn't get an OpenApi token!");
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            Helper?.Dispose();
        }

        #endregion
                
        [TestCase]
        [TestRail(125503)]
        [Description("Create, save, publish Process artifact, check returned results.")]
        public void PublishArtifact_VerifyResult()
        {
            var artifact = ArtifactFactory.CreateArtifact(_project, _user, BaseArtifactType.Process);
            artifact.Save(_user);

            PublishArtifactResult publishResult = null;
            try
            {
                Assert.DoesNotThrow(() =>
                {
                    publishResult = artifact.NovaPublish(_user);
                }, "Publish must throw no errors.");
                Assert.AreEqual(publishResult.ResultCode, (HttpStatusCode)0);
                string expectedMessage = "Successfully published";
                Assert.AreEqual(expectedMessage, publishResult.Message);
            }

            finally
            {
                artifact.Delete(_user);
                artifact.Publish(_user);
            }
        }

        [TestCase]
        [TestRail(125504)]
        [Description("Create, save, publish Process artifact, publish again, check returned results.")]
        public void PublishArtifactWnenNothingToPublish_VerifyResult()
        {
            var artifact = ArtifactFactory.CreateArtifact(_project, _user, BaseArtifactType.Process);
            artifact.Save(_user);
            artifact.Publish(_user);

            PublishArtifactResult publishResult = null;
            try
            {
                Assert.DoesNotThrow(() =>
                {
                    publishResult = artifact.NovaPublish(_user);
                }, "Publish must throw no errors.");
                Assert.AreEqual(publishResult.ResultCode, (HttpStatusCode)0);
                string expectedMessage = I18NHelper.FormatInvariant("Artifact {0} is already published in the project", artifact.Id);
                Assert.AreEqual(expectedMessage, publishResult.Message);
            }

            finally
            {
                artifact.Delete(_user);
                artifact.Publish(_user);
            }
        }
    }
}