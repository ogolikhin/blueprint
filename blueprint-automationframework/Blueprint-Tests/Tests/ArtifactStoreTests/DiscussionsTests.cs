﻿using Helper;
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
using Utilities.Factories;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class DiscussionsTests : TestBase
    {
        private IUser _user = null;
        private IUser _adminUser = null;
        private IProject _project = null;
        private IGroup _authorsGroup = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _authorsGroup = Helper.CreateGroupAndAddToDatabase();

            _user = Helper.CreateUserAndAddToDatabase(instanceAdminRole: null);
            _authorsGroup.AddUser(_user);

            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_adminUser);

            _authorsGroup.AssignRoleToProjectOrArtifact(_project, role: ProjectRole.Author);

            Helper.AdminStore.AddSession(_user);
            Helper.BlueprintServer.LoginUsingBasicAuthorization(_user);
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
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.Actor);

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
            IArtifact artifact = Helper.CreateArtifact(_project, _adminUser, BaseArtifactType.Actor);
            artifact.Save(_adminUser);

            var postedRaptorComment = artifact.PostRaptorDiscussions("draft", _adminUser);
            Discussions discussions = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _adminUser);
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
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.Actor);
            artifact.PostRaptorDiscussions("draft", _adminUser);
            artifact.Delete(_adminUser);

            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _adminUser);
            }, "GetArtifactDiscussions should throw 404 error for artifacts marked for deletion, but it doesn't.");
        }

        [TestCase]
        [TestRail(146056)]
        [Description("Add comment to published subartifact, get discussion for this subartifact, check that it has expected values.")]
        public void GetDiscussions_PublishedSubArtifact_ReturnsCorrectDiscussion()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.Process);
            var postedRaptorComment = AddCommentToSubArtifactOfStorytellerProcess(artifact);

            Discussions discussions = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                discussions = Helper.ArtifactStore.GetArtifactDiscussions(postedRaptorComment.ItemId, _adminUser);
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
            IArtifact artifact = Helper.CreateArtifact(_project, _adminUser, BaseArtifactType.Process);
            artifact.Save(_adminUser);
            var postedRaptorComment = AddCommentToSubArtifactOfStorytellerProcess(artifact);

            Discussions discussions = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                discussions = Helper.ArtifactStore.GetArtifactDiscussions(postedRaptorComment.ItemId, _adminUser);
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
            IArtifact artifact = Helper.CreateArtifact(_project, _adminUser, BaseArtifactType.Process);
            artifact.Save(_adminUser);
            var postedRaptorComment = AddCommentToSubArtifactOfStorytellerProcess(artifact);

            IUser user2 = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDiscussions(postedRaptorComment.ItemId, user2);
            }, "GetArtifactDiscussions should return 404 error, but it doesn't.");
        }

        [TestCase]
        [TestRail(146060)]
        [Description("Add comment & a reply to subartifact of published artifact, get discussion for this subartifact.  Verify it returns the reply that we added.")]
        public void GetReplies_PublishedSubArtifactWithDiscussionAndReply_ReturnsCorrectDiscussion()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.Process);
            var postedRaptorComment = AddCommentToSubArtifactOfStorytellerProcess(artifact);

            Discussions discussions = Helper.ArtifactStore.GetArtifactDiscussions(postedRaptorComment.ItemId, _adminUser);
            IRaptorReply postedReply = Artifact.PostRaptorDiscussionReply(Helper.BlueprintServer.Address,
                postedRaptorComment, "This is a reply to a comment.", _user);

            List<Reply> replies = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                replies = Helper.ArtifactStore.GetDiscussionsReplies(discussions.Comments[0], _adminUser);
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
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.Process);
            var postedRaptorComment = AddCommentToSubArtifactOfStorytellerProcess(artifact);

            artifact.Delete(_adminUser);

            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDiscussions(postedRaptorComment.ItemId, _adminUser);
            }, "GetArtifactDiscussions should return 404 Not Found for artifacts marked for deletion, but it doesn't.");
        }

        /// <summary>
        /// Adds a discussion comment to the specified Storyteller Process artifact.
        /// </summary>
        /// <param name="artifact">The Process artifact to add a comment to.</param>
        /// <returns>The IRaptorComment returned after posting the comment.</returns>
        private IRaptorComment AddCommentToSubArtifactOfStorytellerProcess(IArtifact artifact)
        {
            var process = Helper.Storyteller.GetProcess(_adminUser, artifact.Id);
            var userTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            var postedRaptorComment = Artifact.PostRaptorDiscussions(Helper.BlueprintServer.Address,
                userTask.Id, "text for UT", _adminUser);

            return postedRaptorComment;
        }
    }
}