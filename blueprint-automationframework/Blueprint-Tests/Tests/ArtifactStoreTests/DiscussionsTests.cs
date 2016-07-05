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

        [TestCase]
        [TestRail(146053)]
        [Description("Add comment to published artifact, then get discussion for this artifact.  Verify it returns the comment that we added.")]
        public void GetDiscussions_PublishedArtifact_ReturnsCorrectDiscussion()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);

            var postedRaptorComment = artifact.PostRaptorDiscussions("draft", _user);
            Discussions discussions = null;
            
            // Execute:
            Assert.DoesNotThrow(() =>
            {
                discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _user);
            }, "GetArtifactDiscussions shouldn't throw any error.");

            // Verify:
            Assert.AreEqual(1, discussions.Comments.Count, "Artifact should have 1 comment, but it has {0}",
                discussions.Comments.Count);
            Assert.True(postedRaptorComment.Equals(discussions.Comments[0]),
                "The discussion comment returned from ArtifactStore doesn't match what was posted!");
        }

        [TestCase]
        [TestRail(146054)]
        [Description("Add comment to saved artifact, then get discussion for this artifact.  Verify it returns the comment that we added.")]
        public void GetDiscussions_DraftArtifact_ReturnsCorrectDiscussion()
        {
            // Setup:
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Actor);
            artifact.Save(_user);

            var postedRaptorComment = artifact.PostRaptorDiscussions("draft", _user);
            Discussions discussions = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _user);
            }, "GetArtifactDiscussions shouldn't throw any error, but it doesn't.");

            // Verify:
            Assert.AreEqual(1, discussions.Comments.Count, "Artifact should have 1 comment, but it has {0}",
                discussions.Comments.Count);
            Assert.True(postedRaptorComment.Equals(discussions.Comments[0]),
                "The discussion comment returned from ArtifactStore doesn't match what was posted!");
        }

        [TestCase]
        [TestRail(146055)]
        [Description("Add comment to published artifact, delete artifact (don't publish), get discussion for this artifact.  Verify it returns 404 Not Found.")]
        public void GetDiscussions_MarkedForDeleteArtifact_404NotFound()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            artifact.PostRaptorDiscussions("draft", _user);
            artifact.Delete(_user);

            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _user);
            }, "GetArtifactDiscussions should throw 404 error for artifacts marked for deletion, but it doesn't.");
        }

        [TestCase]
        [TestRail(146056)]
        [Description("Add comment to published subartifact, get discussion for this subartifact, check that it has expected values.")]
        public void GetDiscussions_PublishedSubArtifact_ReturnsCorrectDiscussion()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);

            var process = Helper.Storyteller.GetProcess(_user, artifact.Id);
            var userTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            var postedRaptorComment = Artifact.PostRaptorDiscussions(Helper.BlueprintServer.Address,
                userTask.Id, "text for UT", _user);
            Discussions discussions = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                discussions = Helper.ArtifactStore.GetArtifactDiscussions(userTask.Id, _user);
            }, "GetArtifactDiscussions shouldn't throw any error.");

            // Verify:
            Assert.AreEqual(1, discussions.Comments.Count, "Artifact should have 1 comment, but it has {0}",
                discussions.Comments.Count);
            Assert.True(postedRaptorComment.Equals(discussions.Comments[0]),
                "The discussion comment returned from ArtifactStore doesn't match what was posted!");
        }

        [TestCase]
        [TestRail(146057)]
        [Description("Add comment to subartifact of saved (unpublished) artifact, get discussion for this subartifact.  Verify it returns the comment that we added.")]
        public void GetDiscussions_UnpublishedSubArtifact_ReturnsCorrectDiscussion()
        {
            // Setup:
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Process);
            artifact.Save(_user);

            var process = Helper.Storyteller.GetProcess(_user, artifact.Id);
            var userTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            var postedRaptorComment = Artifact.PostRaptorDiscussions(Helper.BlueprintServer.Address,
                userTask.Id, "text for UT", _user);
            Discussions discussions = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                discussions = Helper.ArtifactStore.GetArtifactDiscussions(userTask.Id, _user);
            }, "GetArtifactDiscussions shouldn't throw any error.");

            // Verify:
            Assert.AreEqual(1, discussions.Comments.Count, "Subartifact should have 1 comment, but it has {0}",
                discussions.Comments.Count);
            Assert.True(postedRaptorComment.Equals(discussions.Comments[0]),
                "The discussion comment returned from ArtifactStore doesn't match what was posted!");
        }

        [TestCase]
        [TestRail(146059)]
        [Description("Add comment to subartifact of saved (unpublished) artifact, try to get discussion for this subartifact with user other than author.  Verify it returns 404 Not Found.")]
        public void GetDiscussions_UnpublishedSubArtifactOtherUser_404NotFound()
        {
            // Setup:
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Process);
            artifact.Save(_user);

            var process = Helper.Storyteller.GetProcess(_user, artifact.Id);
            var userTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            Artifact.PostRaptorDiscussions(Helper.BlueprintServer.Address,
                userTask.Id, "text for UT", _user);
            IUser user2 = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDiscussions(userTask.Id, user2);
            }, "GetArtifactDiscussions should return 404 error, but it doesn't.");
        }

        [TestCase]
        [TestRail(146060)]
        [Description("Add comment & a reply to subartifact of published artifact, get discussion for this subartifact.  Verify it returns the reply that we added.")]
        public void GetReplies_PublishedSubArtifactWithDiscussionAndReply_ReturnsCorrectDiscussion()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);

            var process = Helper.Storyteller.GetProcess(_user, artifact.Id);
            var userTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            var postedRaptorComment = Artifact.PostRaptorDiscussions(Helper.BlueprintServer.Address,
                userTask.Id, "text for UT", _user);

            Discussions discussions = Helper.ArtifactStore.GetArtifactDiscussions(userTask.Id, _user);
            IRaptorReply postedReply = Artifact.PostRaptorDiscussionReply(Helper.BlueprintServer.Address,
                postedRaptorComment, "This is a reply to a comment.", _user);

            List<Reply> replies = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                replies = Helper.ArtifactStore.GetDiscussionsReplies(discussions.Comments[0], _user);
            }, "GetDiscussionsReplies shouldn't throw any error.");

            // Verify:
            Assert.AreEqual(1, replies.Count, "Subartifact should have 1 comment, but it has {0}",
                discussions.Comments.Count);
            Assert.True(postedReply.Equals(replies[0]),
                "The discussion reply returned from ArtifactStore doesn't match what was posted!");
        }

        [TestCase]
        [TestRail(146063)]
        [Description("Add comment to subartifact of published artifact, delete artifact (don't publish), get discussion for this subartifact.  Verify it returns 404 Not Found.")]
        public void GetDiscussions_MarkedForDeleteSubArtifact_404NotFound()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);

            var process = Helper.Storyteller.GetProcess(_user, artifact.Id);
            var userTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            Artifact.PostRaptorDiscussions(Helper.BlueprintServer.Address,
                userTask.Id, "text for UT", _user);
            artifact.Delete(_user);

            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDiscussions(userTask.Id, _user);
            }, "GetArtifactDiscussions should return 404 Not Found for artifacts marked for deletion, but it doesn't.");
        }
    }
}