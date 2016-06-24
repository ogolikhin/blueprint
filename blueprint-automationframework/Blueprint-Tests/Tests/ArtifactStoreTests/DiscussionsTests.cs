using System;
using Helper;
using Model;
using NUnit.Framework;
using CustomAttributes;
using System.Collections.Generic;
using TestCommon;
using Model.ArtifactModel;
using Model.Factories;
using Model.ArtifactModel.Impl;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class DiscussionsTests : TestBase
    {
        private IUser _user = null;
        IProject _project = null;

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

        [TestCase]
        [TestRail(00)]
        [Description("..")]
        public void GetDiscussionsForPublishedArtifact_VerifyDiscussionsHasExpectedValue()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Actor);
            artifact.Save(_user);
            artifact.Publish(_user);
            var postedRaptorComment = artifact.PostRaptorDiscussions("draft", _user);
            Discussions discussions = null;
            
            Assert.DoesNotThrow(() =>
            {
                discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact, _user);
            }, "GetArtifactHistory shouldn't throw any error.");
            Assert.AreEqual(postedRaptorComment.CommentValue,
                discussions.Comments[0].CommentText);
        }

        [TestCase]
        [TestRail(00)]
        [Description("..")]
        public void GetDiscussionsForDraftArtifact_VerifyDiscussionsHasExpectedValue()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Actor);
            artifact.Save(_user);
            var postedRaptorComment = artifact.PostRaptorDiscussions("draft", _user);
            Discussions discussions = null;

            Assert.DoesNotThrow(() =>
            {
                discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact, _user);
            }, "GetArtifactHistory shouldn't throw any error.");
            Assert.AreEqual(postedRaptorComment.CommentValue,
                discussions.Comments[0].CommentText);
        }

        [TestCase]
        [TestRail(00)]
        [Description("..")]
        public void GetDiscussionsForDeletedArtifact_VerifyDiscussionsHasExpectedValue()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Actor);
            artifact.Save(_user);
            artifact.Publish(_user);
            var postedRaptorComment = artifact.PostRaptorDiscussions("draft", _user);
            artifact.Delete(_user);
            Discussions discussions = null;

            Assert.DoesNotThrow(() =>
            {
                discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact, _user);
            }, "GetArtifactHistory shouldn't throw any error.");
            Assert.AreEqual(postedRaptorComment.CommentValue,
                discussions.Comments[0].CommentText);
        }
    }
}