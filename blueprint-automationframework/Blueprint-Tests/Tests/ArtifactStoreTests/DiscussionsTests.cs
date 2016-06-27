using Helper;
using Model;
using NUnit.Framework;
using CustomAttributes;
using System.Collections.Generic;
using TestCommon;
using Utilities;
using Model.ArtifactModel;
using Model.Factories;
using Model.ArtifactModel.Impl;
using Model.StorytellerModel.Impl;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class DiscussionsTests : TestBase
    {
        private IUser _user = null;
        private IUser _user2 = null;
        IProject _project = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _user2 = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_user);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        [TestCase]
        [TestRail(01)]
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
                discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _user);
            }, "GetArtifactHistory shouldn't throw any error.");
            Assert.AreEqual(1, discussions.Comments.Count, "...");
            Assert.True(postedRaptorComment.Equals(discussions.Comments[0]), "...");
        }

        [TestCase]
        [TestRail(02)]
        [Description("..")]
        public void GetDiscussionsForDraftArtifact_VerifyDiscussionsHasExpectedValue()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Actor);
            artifact.Save(_user);
            var postedRaptorComment = artifact.PostRaptorDiscussions("draft", _user);
            Discussions discussions = null;

            Assert.DoesNotThrow(() =>
            {
                discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _user);
            }, "... shouldn't throw any error.");
            Assert.AreEqual(1, discussions.Comments.Count, "...");
            Assert.True(postedRaptorComment.Equals(discussions.Comments[0]), "...");
        }

        [TestCase]
        [TestRail(03)]
        [Description("..")]
        public void GetDiscussionsForDeletedArtifact_VerifyDiscussionsHasExpectedValue()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Actor);
            artifact.Save(_user);
            artifact.Publish(_user);
            artifact.PostRaptorDiscussions("draft", _user);
            artifact.Delete(_user);

            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _user);
            }, "... should throw 404 error.");
        }

        [TestCase]
        [TestRail(04)]
        [Description("..")]
        public void GetDiscussionsForSubArtifact_VerifyDiscussionsHasExpectedValue()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Process);
            artifact.Save(_user);
            artifact.Publish(_user);
            var process = Helper.Storyteller.GetProcess(_user, artifact.Id);
            var userTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            var postedRaptorComment = Artifact.PostRaptorDiscussions(Helper.BlueprintServer.Address,
                userTask.Id, "text for UT", _user);
            Discussions discussions = null;

            Assert.DoesNotThrow(() =>
            {
                discussions = Helper.ArtifactStore.GetArtifactDiscussions(userTask.Id, _user);
            }, "... shouldn't throw any error.");
            Assert.AreEqual(1, discussions.Comments.Count, "...");
            Assert.True(postedRaptorComment.Equals(discussions.Comments[0]), "...");
        }

        [TestCase]
        [TestRail(05)]
        [Description("..")]
        public void GetDiscussionsForUnpublishedSubArtifact_VerifyDiscussionsHasExpectedValue()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Process);
            artifact.Save(_user);
            var process = Helper.Storyteller.GetProcess(_user, artifact.Id);
            var userTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            var postedRaptorComment = Artifact.PostRaptorDiscussions(Helper.BlueprintServer.Address,
                userTask.Id, "text for UT", _user);
            Discussions discussions = null;

            Assert.DoesNotThrow(() =>
            {
                discussions = Helper.ArtifactStore.GetArtifactDiscussions(userTask.Id, _user);
            }, "... shouldn't throw any error.");
            Assert.AreEqual(1, discussions.Comments.Count, "...");
            Assert.True(postedRaptorComment.Equals(discussions.Comments[0]), "...");
        }

        [TestCase]
        [TestRail(06)]
        [Description("..")]
        public void GetDiscussionsForUnpublishedSubArtifactOtherUser_VerifyDiscussionsHasExpectedValue()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Process);
            artifact.Save(_user);
            var process = Helper.Storyteller.GetProcess(_user, artifact.Id);
            var userTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            Artifact.PostRaptorDiscussions(Helper.BlueprintServer.Address,
                userTask.Id, "text for UT", _user);

            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDiscussions(userTask.Id, _user2);
            }, "... should throw 404 error.");
        }

        [TestCase]
        [TestRail(07)]
        [Description("..")]
        public void GetRepliesForSubartifactDiscussion_VerifyReplyHasExpectedValue()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Process);
            artifact.Save(_user);
            artifact.Publish(_user);
            var process = Helper.Storyteller.GetProcess(_user, artifact.Id);
            var userTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            var postedRaptorComment = Artifact.PostRaptorDiscussions(Helper.BlueprintServer.Address,
                userTask.Id, "text for UT", _user);
            Discussions discussions = null;
            discussions = Helper.ArtifactStore.GetArtifactDiscussions(userTask.Id, _user);
            IRaptorReply postedReply = Artifact.PostRaptorDiscussionReply(Helper.BlueprintServer.Address,
                postedRaptorComment, "let replace it with random", _user);

            List<Reply> replies = null;
            Assert.DoesNotThrow(() =>
            {
                replies = Helper.ArtifactStore.GetDiscussionsReplies(discussions.Comments[0], _user);
            }, "... shouldn't throw any error.");
            Assert.AreEqual(1, replies.Count, "...");
            Assert.True(postedReply.Equals(replies[0]));
        }

        [TestCase]
        [TestRail(08)]
        [Description("..")]
        public void GetDiscussionsForSubArtifactOfDeletedArtifact_VerifyDiscussionsHasExpectedValue()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Process);
            artifact.Save(_user);
            artifact.Publish(_user);
            var process = Helper.Storyteller.GetProcess(_user, artifact.Id);
            var userTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            Artifact.PostRaptorDiscussions(Helper.BlueprintServer.Address,
                userTask.Id, "text for UT", _user);
            artifact.Delete(_user);

            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDiscussions(userTask.Id, _user);
            }, "... should throw 404 error.");
        }
    }
}