using Helper;
using Model;
using NUnit.Framework;
using CustomAttributes;
using System.Net;
using System.Collections.Generic;
using TestCommon;
using Model.ArtifactModel;
using Model.Factories;
using Model.ArtifactModel.Impl;
using Utilities;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class ArtifactHistoryTests : TestBase
    {
        private IUser _user = null;
        //private IUser _user2 = null;
        IProject _project = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_user);
            //_user2 = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        [Test]
        [TestRail(145867)]
        [Description("...")]
        public void GetHistoryForPublishedArtifact_VerifyHistoryHasExpectedValue()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Actor);
            artifact.Save(_user);
            artifact.Publish(_user);

            ArtifactHistoryVersion artifactVersion = null;
            Assert.DoesNotThrow(() =>
            {
                artifactVersion = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user)[0];
            });
            Assert.AreEqual(1, artifactVersion.versionId);
            Assert.AreEqual(false, artifactVersion.hasUserIcon);
        }
    }
}
