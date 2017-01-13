﻿using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using System.Collections.Generic;
using TestCommon;
using Utilities;
using Utilities.Factories;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class DiscussionsTests : TestBase
    {
        private IUser _authorUser = null;
        private IUser _adminUser = null;
        private IProject _project = null;
        private IGroup _authorsGroup = null;

        private IProjectRole _authorRole = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _authorsGroup = Helper.CreateGroupAndAddToDatabase();

            _authorUser = Helper.CreateUserAndAddToDatabase(instanceAdminRole: null);
            _authorsGroup.AddUser(_authorUser);
            
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_adminUser);

            _authorRole = ProjectRoleFactory.GetDeployedProjectRole(ProjectRoleFactory.DeployedProjectRole.Author);

            _authorsGroup.AssignRoleToProjectOrArtifact(_project, role: _authorRole);

            Helper.AdminStore.AddSession(_authorUser);
            Helper.BlueprintServer.LoginUsingBasicAuthorization(_authorUser);
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

            var postedRaptorComment = artifact.PostRaptorDiscussions("draft", _authorUser);
            Discussions discussions = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _authorUser);
            }, "GetArtifactDiscussions shouldn't throw any error.");

            // Verify:
            Assert.AreEqual(1, discussions.Comments.Count, "Artifact should have 1 comment, but it has {0}",
                discussions.Comments.Count);
            Assert.True(postedRaptorComment.Equals(discussions.Comments[0]),
                "The discussion comment returned from ArtifactStore doesn't match what was posted!");
        }

        [TestCase]
        [TestRail(146054)]
        [Description("Add comment to saved artifact, then get discussion for this artifact.  Verify it returns 404.")]
        public void GetDiscussions_DraftArtifact_Returns404()
        {
            // Setup:
            IArtifact artifact = Helper.CreateArtifact(_project, _adminUser, BaseArtifactType.Actor);
            artifact.Save(_adminUser);

            artifact.PostRaptorDiscussions("draft", _adminUser);
            Discussions discussions = null;

            // In Nova UI doesn't allow to post discussion for never published artifact, it is possible to do by adding comment in SL and saving it
            // In this case it would be a conflict - in SL user will see comment and in Nova he will not.
            // For consistency server shouldn't allow to create comment for never published artifact.
            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _adminUser);
            }, "GetArtifactDiscussions should throw 404 error, but it doesn't.");
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
            Assert.AreEqual(-1, postedRaptorComment.Version,
                "Comment for draft should have version -1, but it has version {0}", postedRaptorComment.Version);
            Assert.AreEqual(-1, discussions.Comments[0].Version,
                "Comment for draft should have version -1, but it has version {0}", discussions.Comments[0].Version);
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
                postedRaptorComment, "This is a reply to a comment.", _authorUser);

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

        [TestCase]
        [TestRail(155659)]
        [Description("Update discussion for the published artifact. Verify that text was updated.")]
        public void UpdateDiscussion_PublishedArtifact_ReturnsUpdatedDiscussion()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UseCase);

            var postedRaptorComment = artifact.PostRaptorDiscussions("original discussion text", _authorUser);
            var discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _authorUser);
            Assert.AreEqual(1, discussions.Comments.Count, "There should be 1 comment returned!");
            Assert.True(postedRaptorComment.Equals(discussions.Comments[0]),
                "The discussion comment returned from ArtifactStore doesn't match what was posted!");
            IRaptorComment updatedComment = null;
            

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                updatedComment = artifact.UpdateRaptorDiscussions("updated text", _authorUser, postedRaptorComment);
            }, "UpdateDiscussions shouldn't throw any error.");

            // Verify:
            discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _authorUser);
            Assert.AreEqual(1, discussions.Comments.Count, "Artifact should have 1 comment, but it has {0}",
                discussions.Comments.Count);
            Assert.True(updatedComment.Equals(discussions.Comments[0]),
                "The discussion comment returned from ArtifactStore doesn't match what was posted!");
        }

        [TestCase]
        [TestRail(155675)]
        [Description("Update comment of published subartifact, get discussion for this subartifact, check that it has expected values.")]
        public void UpdateDiscussion_PublishedSubArtifact_ReturnsUpdatedDiscussion()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.Process);
            var postedRaptorComment = AddCommentToSubArtifactOfStorytellerProcess(artifact, _authorUser);

            var process = Helper.Storyteller.GetProcess(_authorUser, artifact.Id);
            var userTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            string newText = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(100);
            IRaptorComment updatedComment = null;
            
            // Execute:
            Assert.DoesNotThrow(() =>
            {
                updatedComment = OpenApiArtifact.UpdateRaptorDiscussion(Helper.BlueprintServer.Address,
                    userTask.Id, postedRaptorComment, newText, _authorUser);
            }, "UpdateDiscussions shouldn't throw any error.");

            // Verify:
            Discussions discussions = Helper.ArtifactStore.GetArtifactDiscussions(userTask.Id, _authorUser);
            Assert.AreEqual(1, discussions.Comments.Count, "Artifact should have 1 comment, but it has {0}",
                discussions.Comments.Count);
            Assert.True(updatedComment.Equals(discussions.Comments[0]),
                "The discussion comment returned from ArtifactStore doesn't match what was posted!");
            Assert.AreEqual(StringUtilities.WrapInDiv(newText), updatedComment.CommentValue, "Updated comment must have updated value, but it didn't.");
        }

        [TestCase]
        [TestRail(155771)]
        [Description("Try to delete comment created by other user, no privilege to delete any comment, check that it returns 403.")]
        public void DeleteComment_UserHasNoRightsToDelete_403Forbidden()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UIMockup);
            string commentText = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(100);
            IRaptorComment raptorComment = artifact.PostRaptorDiscussions(commentText, _adminUser);
            Assert.AreEqual(StringUtilities.WrapInDiv(commentText), raptorComment.CommentValue);

            // Execute:
            Assert.Throws<Http403ForbiddenException>(() =>
            {
                artifact.DeleteRaptorDiscussion(_authorUser, raptorComment);
            }, "DeleteDiscussions should throw 403 error, but it doesn't.");

            // Verify:
            Discussions discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _authorUser);
            Assert.AreEqual(1, discussions.Comments.Count, "Artifact should have 1 comment, but it has {0} comments",
                discussions.Comments.Count);
            Assert.AreEqual(StringUtilities.WrapInDiv(commentText), discussions.Comments[0].CommentText, "Comment shouldn't change, but it did.");
        }

        [TestCase]
        [TestRail(156529)]
        [Description("Try to delete reply created by other user, no privilege to delete any comment, check that it returns 403.")]
        public void DeleteReply_UserHasNoRightsToDelete_403Forbidden()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UIMockup);
            string commentText = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(100);
            IRaptorComment raptorComment = artifact.PostRaptorDiscussions(commentText, _adminUser);
            Assert.AreEqual(StringUtilities.WrapInDiv(commentText), raptorComment.CommentValue);
            string replyText = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(100);
            IRaptorReply raptorReply = OpenApiArtifact.PostRaptorDiscussionReply(Helper.BlueprintServer.Address,
                raptorComment, replyText, _adminUser);

            // Execute:
            Assert.Throws<Http403ForbiddenException>(() =>
            {
                OpenApiArtifact.DeleteRaptorReply(Helper.BlueprintServer.Address, artifact.Id,
                    raptorReply, _authorUser);
            }, "DeleteRaptorReply should throw 403 error, but it doesn't.");

            // Verify:
            Discussions discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _authorUser);
            Assert.AreEqual(1, discussions.Comments.Count, "Artifact should have 1 comment, but it has {0} comments",
                discussions.Comments.Count);
            Assert.AreEqual(1, discussions.Comments[0].RepliesCount, "Discussion should have 1 reply, but it has {0} replies",
                discussions.Comments[0].RepliesCount);
        }

        [TestCase]
        [TestRail(156531)]
        [Description("Delete reply created by the current user, check that reply was deleted.")]
        public void DeleteReply_DeleteByReplyAuthor_SuccessfullyDeleted()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UIMockup);
            string commentText = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(100);
            IRaptorComment raptorComment = artifact.PostRaptorDiscussions(commentText, _adminUser);
            Assert.AreEqual(StringUtilities.WrapInDiv(commentText), raptorComment.CommentValue);
            string replyText = null;
            IRaptorReply raptorReply = null;
            for (int i = 0; i < 2; i++)
            {
                replyText = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(100);
                raptorReply = OpenApiArtifact.PostRaptorDiscussionReply(Helper.BlueprintServer.Address,
                    raptorComment, replyText, _authorUser);
            }

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                OpenApiArtifact.DeleteRaptorReply(Helper.BlueprintServer.Address, artifact.Id,
                    raptorReply, _authorUser);
            }, "DeleteReply shouldn't throw any error, but it did.");

            // Verify:
            Discussions discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _adminUser);
            Assert.AreEqual(1, discussions.Comments.Count, "Artifact should have 1 comment, but it has {0} comments",
                discussions.Comments.Count);
            Assert.AreEqual(1, discussions.Comments[0].RepliesCount, "Discussion should have 1 reply, but it has {0} replies",
                discussions.Comments[0].RepliesCount);
        }

        [TestCase]
        [TestRail(156532)]
        [Description("Update comment created by the current user, check that comment was updated.")]
        public void UpdateOwnComment_NonAdminUser_SuccessfullyUpdated()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UseCase);
            string commentText = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(100);
            IRaptorComment raptorComment = artifact.PostRaptorDiscussions(commentText, _authorUser);
            Assert.AreEqual(StringUtilities.WrapInDiv(commentText), raptorComment.CommentValue);
            string newText = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(100);
            IRaptorComment updatedRaptorReply = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                updatedRaptorReply = artifact.UpdateRaptorDiscussions(newText, _authorUser, raptorComment);
            }, "UpdateDiscussion shouldn't throw any error, but it did.");

            // Verify:
            Assert.AreEqual(StringUtilities.WrapInDiv(newText), updatedRaptorReply.CommentValue, "Updated comment must have proper text.");
        }

        [TestCase]
        [TestRail(156533)]
        [Description("Admin tries to update comment created by user - it returns 403, check that comment wasn't updated.")]
        public void UpdateOtherUserComment_AdminUser_403Forbidden()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UseCase);
            string commentText = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(100);
            IRaptorComment raptorComment = artifact.PostRaptorDiscussions(commentText, _authorUser);
            Assert.AreEqual(StringUtilities.WrapInDiv(commentText), raptorComment.CommentValue);
            string newText = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(100);

            // Execute:
            Assert.Throws<Http403ForbiddenException>(() =>
            {
                artifact.UpdateRaptorDiscussions(newText, _adminUser, raptorComment);
                
            }, "UpdateDiscussion should throw 403 error, but it doesn't.");

            // Verify:
            var discussion = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _authorUser);
            Assert.AreEqual(1, discussion.Comments.Count, "Discussions must have 1 comment, but it has {0}", discussion.Comments.Count);
            Assert.AreEqual(raptorComment.CommentValue, discussion.Comments[0].CommentText, "Comment must remain unmodified.");
        }

        [TestCase]
        [TestRail(156534)]
        [Description("Delete own comment, check that comment was deleted.")]
        public void DeleteOwnComment_UserHasRightsToDelete_SuccessfullyDeleted()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UIMockup);
            string commentText = null;
            IRaptorComment raptorComment = null;
            for (int i = 0; i < 2; i++)
            {
                commentText = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(100);
                raptorComment = artifact.PostRaptorDiscussions(commentText, _authorUser);
            }
            Assert.AreEqual(StringUtilities.WrapInDiv(commentText), raptorComment.CommentValue);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                artifact.DeleteRaptorDiscussion(_authorUser, raptorComment);
            }, "DeleteDiscussions shouldn't throw any error, but it does.");

            // Verify:
            Discussions discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _adminUser);
            Assert.AreEqual(1, discussions.Comments.Count, "Artifact should have 1 comment, but it has {0} comments",
                discussions.Comments.Count);
        }

        [TestCase]
        [TestRail(156535)]
        [Description("Admin deletes other user's comment, check that comment was deleted.")]
        public void DeleteOtherUserComment_Admin_SuccessfullyDeleted()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UIMockup);
            string commentText = null;
            IRaptorComment raptorComment = null;
            for (int i = 0; i < 2; i++)
            {
                commentText = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(100);
                raptorComment = artifact.PostRaptorDiscussions(commentText, _authorUser);
            }
            Assert.AreEqual(StringUtilities.WrapInDiv(commentText), raptorComment.CommentValue);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                artifact.DeleteRaptorDiscussion(_adminUser, raptorComment);
            }, "DeleteDiscussions shouldn't throw any error, but it does.");

            // Verify:
            Discussions discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _authorUser);
            Assert.AreEqual(1, discussions.Comments.Count, "Artifact should have 1 comment, but it has {0} comments",
                discussions.Comments.Count);
        }

        [TestCase]
        [TestRail(156536)]
        [Description("User updates its own reply, check that reply was updated.")]
        public void UpdateOwnReply_NonAdminUser_SuccessfullyUpdated()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.Glossary);
            string commentText = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(100);
            IRaptorComment raptorComment = artifact.PostRaptorDiscussions(commentText, _adminUser);
            Assert.AreEqual(StringUtilities.WrapInDiv(commentText), raptorComment.CommentValue);
            string replyText = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(100);
            IRaptorReply raptorReply = OpenApiArtifact.PostRaptorDiscussionReply(Helper.BlueprintServer.Address,
                raptorComment, replyText, _authorUser);
            Assert.AreEqual(StringUtilities.WrapInDiv(replyText), raptorReply.ReplyText);

            string newReplyText = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(100);
            IRaptorReply updatedReply = null;
            // Execute:
            Assert.DoesNotThrow(() =>
            {
                updatedReply = OpenApiArtifact.UpdateRaptorDiscussionReply(Helper.BlueprintServer.Address,
                    artifact.Id, raptorComment, raptorReply, newReplyText, _authorUser);
            }, "UpdateReply shouldn't throw any error, but it does.");

            // Verify:
            Discussions discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _adminUser);
            Assert.AreEqual(1, discussions.Comments.Count, "Artifact should have 1 comment, but it has {0} comments",
                discussions.Comments.Count);
            Assert.AreEqual(1, discussions.Comments[0].RepliesCount, "Discussion should have 1 reply, but it has {0} replies",
                discussions.Comments[0].RepliesCount);
            Assert.AreEqual(StringUtilities.WrapInDiv(newReplyText), updatedReply.ReplyText);
        }

        [TestCase]
        [TestRail(156537)]
        [Description("Post comment for artifact with version 2, check that comment has version 2.")]
        public void PostNewComment_ArtifactHasVersion2_CheckCommentHasVersion2()
        {
            // Setup:
            IOpenApiArtifact artifact = Helper.CreateAndPublishOpenApiArtifact(_project, _adminUser, BaseArtifactType.UseCase,
                numberOfVersions: 2);//artifact version is 2

            string commentText = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(100);
            IRaptorComment postedRaptorComment = null;
            Discussions discussions = null;
            
            // Execute:
            Assert.DoesNotThrow(() =>
            {
                postedRaptorComment = Artifact.PostRaptorDiscussions(artifact.Address, artifact.Id, commentText, _authorUser);
                artifact.Save(_authorUser);
                artifact.Publish(_authorUser);//artifact version is 3
                discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _adminUser);
            }, "UpdateDiscussions and GetDiscussion shouldn't throw any error.");

            // Verify:
            Assert.AreEqual(1, discussions.Comments.Count, "Artifact should have 1 comment, but it has {0}",
                discussions.Comments.Count);
            Assert.AreEqual(2, discussions.Comments[0].Version, "Version should be 2, but it is {0}",
                discussions.Comments[0].Version);
            Assert.AreEqual(2, postedRaptorComment.Version, "Version should be 2, but it is {0}",
                postedRaptorComment.Version);
        }

        /// <summary>
        /// Adds a discussion comment to the specified Storyteller Process artifact.
        /// </summary>
        /// <param name="artifact">The Process artifact to add a comment to.</param>
        /// <param name="user">The user credentials for the operation.</param>
        /// <returns>The IRaptorComment returned after posting the comment.</returns>
        private IRaptorComment AddCommentToSubArtifactOfStorytellerProcess(IArtifact artifact, IUser user = null)
        {
            if (user == null)
            {
                user = _adminUser;
            }
            var process = Helper.Storyteller.GetProcess(user, artifact.Id);
            var userTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            var postedRaptorComment = Artifact.PostRaptorDiscussions(Helper.BlueprintServer.Address,
                userTask.Id, "text for UT", user);

            return postedRaptorComment;
        }
    }
}