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
        [TestRail(146053)]
        [Description("Add comment for published artifact, get discussion for this artifact, check that it has expected values.")]
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
            }, "GetArtifactDiscussions shouldn't throw any error.");
            Assert.AreEqual(1, discussions.Comments.Count, "Artifact should have 1 comment, but it has {0}",
                discussions.Comments.Count);
            Assert.True(postedRaptorComment.Equals(discussions.Comments[0]), "Comment should have expected value, but it doesn't.");
        }

        [TestCase]
        [TestRail(146054)]
        [Description("Add comment for saved artifact, get discussion for this artifact, check that it has expected values.")]
        public void GetDiscussionsForDraftArtifact_VerifyDiscussionsHasExpectedValue()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Actor);
            artifact.Save(_user);
            var postedRaptorComment = artifact.PostRaptorDiscussions("draft", _user);
            Discussions discussions = null;

            Assert.DoesNotThrow(() =>
            {
                discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _user);
            }, "GetArtifactDiscussions shouldn't throw any error, but it doesn't.");
            Assert.AreEqual(1, discussions.Comments.Count, "Artifact should have 1 comment, but it has {0}",
                discussions.Comments.Count);
            Assert.True(postedRaptorComment.Equals(discussions.Comments[0]), "Comment should have expected value, but it doesn't.");
        }

        [TestCase]
        [TestRail(146055)]
        [Description("Add comment for published artifact, delete artifact (don't publish), get discussion for this artifact, check that it returns 404.")]
        public void GetDiscussionsForScheduleToDeleteArtifact_Throws404()
        {
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Actor);
            artifact.Save(_user);
            artifact.Publish(_user);
            artifact.PostRaptorDiscussions("draft", _user);
            artifact.Delete(_user);

            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _user);
            }, "GetArtifactDiscussions should throw 404 error, but it doesn't.");
        }

        [TestCase]
        [TestRail(146056)]
        [Description("Add comment for published subartifact, get discussion for this subartifact, check that it has expected values.")]
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
            }, "GetArtifactDiscussions shouldn't throw any error.");
            Assert.AreEqual(1, discussions.Comments.Count, "Artifact should have 1 comment, but it has {0}",
                discussions.Comments.Count);
            Assert.True(postedRaptorComment.Equals(discussions.Comments[0]), "Comment should have expected value, but it doesn't.");
        }

        [TestCase]
        [TestRail(146057)]
        [Description("Add comment for subartifact of saved (unpublished) artifact, get discussion for this subartifact, check that it has expected values.")]
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
            }, "GetArtifactDiscussions shouldn't throw any error.");
            Assert.AreEqual(1, discussions.Comments.Count, "Subartifact should have 1 comment, but it has {0}",
                discussions.Comments.Count);
            Assert.True(postedRaptorComment.Equals(discussions.Comments[0]), "Comment should have expected value, but it doesn't.");
        }

        [TestCase]
        [TestRail(146059)]
        [Description("Add comment for subartifact of saved (unpublished) artifact, try to get discussion for this subartifact with user other than author, check that it returns 404.")]
        public void GetDiscussionsForUnpublishedSubArtifactOtherUser_Throws404()
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
            }, "GetArtifactDiscussions should return 404 error, but it doesn't.");
        }

        [TestCase]
        [TestRail(146060)]
        [Description("Add comment for subartifact of published artifact, get discussion for this subartifact, check that it has expected values.")]
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
            }, "GetDiscussionsReplies shouldn't throw any error.");
            Assert.AreEqual(1, replies.Count, "Subartifact should have 1 comment, but it has {0}",
                discussions.Comments.Count);
            Assert.True(postedReply.Equals(replies[0]), "Reply shoud have expected value, but it doesn't.");
        }

        [TestCase]
        [TestRail(146063)]
        [Description("Add comment for subartifact of published artifact, delete artifact (don't publish), get discussion for this subartifact, check that it returns 404.")]
        public void GetDiscussionsForSubArtifactOfDeletedArtifact_Throws404()
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
            }, "GetArtifactDiscussions should return 404 error, but it doesn't.");
        }
    }
}