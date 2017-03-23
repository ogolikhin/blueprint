using CustomAttributes;
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
using Common;
using Model.ArtifactModel.Adapters;
using Model.ArtifactModel.Enums;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class DiscussionsTests : TestBase
    {
        private const string UPDATED_TEXT = "Updated text";
        private const string ORIGINAL_COMMENT = "Original comment";
        private const string NO_PERMISSIONS_TO_COMPLETE_OPERATION = "You do not have permissions to complete this operation";
        private const string TOKEN_MISSING_OR_MALFORMED = "Token is missing or malformed.";
        private const string UNAUTHORIZED_CALL = "Unauthorized call";
        private const string NOT_ACCESSIBLE_ITEM = "Item is no longer accessible";

        private IUser _authorUser = null;
        private IUser _adminUser = null;
        private IProject _project = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();

            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_adminUser);

            _authorUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #region Positive tests

        [TestCase]
        [TestRail(146053)]
        [Description("Add comment to published artifact, then get discussion for this artifact.  Verify it returns the comment that we added.")]
        public void GetDiscussions_PublishedArtifact_ReturnsCorrectDiscussion()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.Actor);

            var postedRaptorDiscussion = artifact.PostRapidReviewArtifactDiscussion("draft", _authorUser);
            var viewerUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);

            DiscussionResultSet discussions = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, viewerUser);
            }, "GetArtifactDiscussions shouldn't throw any error.");

            // Verify:
            Assert.AreEqual(1, discussions.Discussions.Count, "Artifact should have 1 comment, but it has {0}", discussions.Discussions.Count);

            RaptorDiscussion.AssertAreEqual(postedRaptorDiscussion, discussions.Discussions[0], skipCanDelete: true, skipCanEdit: true);
            Assert.IsFalse(discussions.Discussions[0].IsClosed, "IsClosed flag should be set to false!");

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, viewerUser, artifact.Id, ItemIndicatorFlags.HasComments);
        }

        [TestCase]
        [TestRail(146055)]
        [Description("Add comment to published artifact, delete artifact (don't publish), get discussion for this artifact.  Verify returned discussions.")]
        public void GetDiscussions_MarkedForDeleteArtifact_ValidateReturnedDiscussion()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.Actor);
            var postedRaptorComment = artifact.PostRapidReviewArtifactDiscussion("draft", _adminUser);
            artifact.Delete(_adminUser);
            DiscussionResultSet discussions = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _adminUser);
            }, "GetArtifactDiscussions shouldn't throw any error, but it doesn't.");

            // Verify:
            Assert.AreEqual(1, discussions.Discussions.Count, "Artifact should have 1 comment, but it has {0}", discussions.Discussions.Count);
            RaptorDiscussion.AssertAreEqual(postedRaptorComment, discussions.Discussions[0]);
            Assert.IsFalse(discussions.Discussions[0].IsClosed, "IsClosed flag should be set to false!");

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, artifact.Id, ItemIndicatorFlags.HasComments);
        }

        [TestCase]
        [TestRail(146056)]
        [Description("Add comment to published subartifact, get discussion for this subartifact, check that it has expected values.")]
        public void GetDiscussions_PublishedSubArtifact_ReturnsCorrectDiscussion()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.Process);
            var postedComment = AddDiscussionToSubArtifactOfStorytellerProcess(artifact);

            DiscussionResultSet discussions = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                discussions = Helper.ArtifactStore.GetArtifactDiscussions(postedComment.ItemId, _adminUser);
            }, "GetArtifactDiscussions shouldn't throw any error.");

            // Verify:
            Assert.AreEqual(1, discussions.Discussions.Count, "Artifact should have 1 comment, but it has {0}", discussions.Discussions.Count);
            RaptorDiscussion.AssertAreEqual(postedComment, discussions.Discussions[0]);
            Assert.IsFalse(discussions.Discussions[0].IsClosed, "IsClosed flag should be set to false!");

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _adminUser, artifact.Id, ItemIndicatorFlags.HasComments, postedComment.ItemId);
        }

        [TestCase]
        [TestRail(146060)]
        [Description("Add comment & a reply to subartifact of published artifact, get discussion for this subartifact.  Verify it returns the reply that we added.")]
        public void GetReplies_PublishedSubArtifactWithDiscussionAndReply_ReturnsCorrectDiscussion()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.Process);
            var postedComment = AddDiscussionToSubArtifactOfStorytellerProcess(artifact);

            DiscussionResultSet discussions = Helper.ArtifactStore.GetArtifactDiscussions(postedComment.ItemId, _adminUser);
            var postedReply = Artifact.PostRaptorDiscussionReply(Helper.BlueprintServer.Address,
                postedComment, "This is a reply to a comment.", _authorUser);

            List<Reply> replies = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                replies = Helper.ArtifactStore.GetDiscussionsReplies(discussions.Discussions[0], _adminUser);
            }, "GetDiscussionsReplies shouldn't throw any error.");

            // Verify:
            Assert.AreEqual(1, replies.Count, "Subartifact should have 1 comment, but it has {0}", discussions.Discussions.Count);
            RaptorReply.AssertAreEqual(postedReply, replies[0], skipCanEdit: true);
            Assert.IsFalse(discussions.Discussions[0].IsClosed, "IsClosed flag should be set to false!");

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _adminUser, artifact.Id, ItemIndicatorFlags.HasComments, postedComment.ItemId);
        }

        [TestCase]
        [TestRail(146063)]
        [Description("Add comment to subartifact of published artifact, delete artifact (don't publish), get discussion for this subartifact.  Verify it returns expected discussion.")]
        public void GetDiscussions_MarkedForDeleteSubArtifact_ValidateReturnedDiscussion()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.Process);
            var postedComment = AddDiscussionToSubArtifactOfStorytellerProcess(artifact);

            artifact.Delete(_adminUser);

            // Execute:
            DiscussionResultSet discussions = null;
            Assert.DoesNotThrow(() =>
            {
                discussions = Helper.ArtifactStore.GetArtifactDiscussions(postedComment.ItemId, _adminUser);
            }, "GetArtifactDiscussions shouldn't throw any exception, but it does.");

            // Verify:
            Assert.AreEqual(1, discussions.Discussions.Count, "Artifact should have 1 comment, but it has {0}", discussions.Discussions.Count);
            RaptorDiscussion.AssertAreEqual(postedComment, discussions.Discussions[0]);
            Assert.IsFalse(discussions.Discussions[0].IsClosed, "IsClosed flag should be set to false!");

            var viewerUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, viewerUser, artifact.Id, ItemIndicatorFlags.HasComments, postedComment.ItemId);
        }

        [TestCase]
        [TestRail(266961)]
        [Description("Get discussions from an artifact that does not have any discussion. Verify no discussions were returned")]
        public void GetDiscussions_NoDiscussionsInArtifact_ValidateDiscussionsNotReturned()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.Glossary);

            // Execute:
            DiscussionResultSet discussions = null;
            Assert.DoesNotThrow(() =>
            {
                discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _adminUser);
            }, "GetArtifactDiscussions shouldn't throw any exception, but it does.");

            // Verify:
            Assert.AreEqual(0, discussions.Discussions.Count, "Artifact should have no comments, but it has {0}", discussions.Discussions.Count);

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _adminUser, artifact.Id, expectedIndicatorFlags: null);
        }

        [TestCase]
        [TestRail(155659)]
        [Description("Update discussion for the published artifact. Verify that text & IsClosed flag were updated.")]
        public void UpdateDiscussion_PublishedArtifact_ReturnsUpdatedDiscussion()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UseCase);

            var postedRaptorComment = artifact.PostRapidReviewArtifactDiscussion("original discussion text", _authorUser);
            var discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _authorUser);

            Assert.AreEqual(1, discussions.Discussions.Count, "There should be 1 comment returned!");
            RaptorDiscussion.AssertAreEqual(postedRaptorComment, discussions.Discussions[0]);

            var statusId = GetStatusId(discussions, ThreadStatus.CLOSED);
            var comment = new RaptorComment()
            {
                Comment = UPDATED_TEXT,
                StatusId = statusId
            };

            // Execute:
            IDiscussionAdapter updatedDiscussion = null;
            Assert.DoesNotThrow(() =>
            {
                updatedDiscussion = artifact.UpdateRapidReviewArtifactDiscussion(postedRaptorComment, comment, _authorUser);
            }, "UpdateDiscussions shouldn't throw any error.");

            // Verify:
            discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _authorUser);
            Assert.AreEqual(1, discussions.Discussions.Count, "Artifact should have 1 comment, but it has {0}", discussions.Discussions.Count);
            Assert.AreEqual(StringUtilities.WrapInDiv(comment.Comment), updatedDiscussion.Comment, "Updated comment must have proper text.");
            Assert.IsTrue(discussions.Discussions[0].IsClosed, "IsClosed flag should be set to true!");
            RaptorDiscussion.AssertAreEqual(updatedDiscussion, discussions.Discussions[0], skipCanEdit: true);

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _adminUser, artifact.Id, expectedIndicatorFlags: null);
        }

        [TestCase]
        [TestRail(155675)]
        [Description("Update discussion of published subartifact, get discussion for this subartifact, check that it has expected values.")]
        public void UpdateDiscussion_PublishedSubArtifact_ReturnsUpdatedDiscussion()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.Process);
            var postedDiscussion = AddDiscussionToSubArtifactOfStorytellerProcess(artifact, _authorUser);

            var discussions = Helper.ArtifactStore.GetArtifactDiscussions(postedDiscussion.ItemId, _authorUser);
            var statusId = GetStatusId(discussions, ThreadStatus.CLOSED);

            var comment = new RaptorComment
            {
                Comment = UPDATED_TEXT,
                StatusId = statusId
            };

            // Execute:
            IDiscussionAdapter updatedDiscussion = null;
            Assert.DoesNotThrow(() =>
            {
                updatedDiscussion = Helper.SvcComponents.UpdateRapidReviewArtifactDiscussion(
                    _authorUser, postedDiscussion.ItemId, postedDiscussion.DiscussionId, comment);
            }, "UpdateDiscussions shouldn't throw any error.");

            // Verify:
            discussions = Helper.ArtifactStore.GetArtifactDiscussions(postedDiscussion.ItemId, _authorUser);
            Assert.AreEqual(1, discussions.Discussions.Count, "Artifact should have 1 comment, but it has {0}", discussions.Discussions.Count);
            Assert.AreEqual(StringUtilities.WrapInDiv(comment.Comment), updatedDiscussion.Comment, "Updated comment must have updated value, but it didn't.");
            Assert.IsTrue(discussions.Discussions[0].IsClosed, "IsClosed flag should be set to true!");
            RaptorDiscussion.AssertAreEqual(updatedDiscussion, discussions.Discussions[0], skipCanEdit: true);

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _adminUser, artifact.Id, expectedIndicatorFlags: null, subArtifactId: postedDiscussion.ItemId);
        }

        [TestCase]
        [TestRail(156531)]
        [Description("Delete reply created by the current user, check that reply was deleted.")]
        public void DeleteReply_DeleteByReplyAuthor_SuccessfullyDeleted()
        {
            // Setup:
            const string REPLY = "Reply";
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UIMockup);
            var raptorComment = artifact.PostRapidReviewArtifactDiscussion(ORIGINAL_COMMENT, _adminUser);
            Assert.AreEqual(StringUtilities.WrapInDiv(ORIGINAL_COMMENT), raptorComment.Comment,
                "Original comment and comment returned after discussion created different!");
            string replyText = null;
            IReplyAdapter raptorReply = null;

            for (int i = 0; i < 2; i++)
            {
                replyText = REPLY + " " + (i + 1);
                raptorReply = OpenApiArtifact.PostRaptorDiscussionReply(Helper.BlueprintServer.Address, raptorComment, replyText, _authorUser);
            }

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                artifact.DeleteRapidReviewArtifactReply(raptorReply, _authorUser);
            }, "{0} shouldn't throw any error, but it did.", nameof(artifact.DeleteRapidReviewArtifactReply));

            // Verify:
            var discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _adminUser);
            Assert.AreEqual(1, discussions.Discussions.Count, "Artifact should have 1 comment, but it has {0} comments", discussions.Discussions.Count);
            Assert.AreEqual(1, discussions.Discussions[0].RepliesCount, "Discussion should have 1 reply, but it has {0} replies",
                discussions.Discussions[0].RepliesCount);

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _adminUser, artifact.Id, ItemIndicatorFlags.HasComments);
        }

        [TestCase]
        [TestRail(156532)]
        [Description("Update discussion created by the current user, check that comment was updated.")]
        public void UpdateOwnDiscussion_NonAdminUser_SuccessfullyUpdated()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UseCase);
            var raptorComment = artifact.PostRapidReviewArtifactDiscussion(ORIGINAL_COMMENT, _authorUser);
            Assert.AreEqual(StringUtilities.WrapInDiv(ORIGINAL_COMMENT), raptorComment.Comment,
                "Original comment and comment returned after discussion created different!");

            var discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _authorUser);
            var statusId = GetStatusId(discussions, ThreadStatus.CLOSED);
            var comment = new RaptorComment()
            {
                Comment = UPDATED_TEXT,
                StatusId = statusId
            };

            // Execute:
            IDiscussionAdapter updatedDiscussion = null;
            Assert.DoesNotThrow(() =>
            {
                updatedDiscussion = artifact.UpdateRapidReviewArtifactDiscussion(raptorComment, comment, _authorUser);
            }, "UpdateDiscussion shouldn't throw any error, but it did.");

            // Verify:
            discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _authorUser);
            Assert.AreEqual(1, discussions.Discussions.Count, "Artifact should have 1 comment, but it has {0}", discussions.Discussions.Count);
            Assert.AreEqual(StringUtilities.WrapInDiv(comment.Comment), updatedDiscussion.Comment, "Updated comment must have proper text.");
            Assert.IsTrue(discussions.Discussions[0].IsClosed, "IsClosed flag should be set to true!");
            RaptorDiscussion.AssertAreEqual(updatedDiscussion, discussions.Discussions[0], skipCanEdit: true);

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _adminUser, artifact.Id, expectedIndicatorFlags: null);
        }

        [TestCase]
        [TestRail(266910)]
        [Description("Close discussion created by user, reopen discussion. Check that comment was updated and discussion reopened.")]
        public void UpdateDiscussion_CloseAndReopenDiscussion_SuccessfullyUpdated()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UseCase);
            var discussion = artifact.PostRapidReviewArtifactDiscussion(ORIGINAL_COMMENT, _authorUser);
            Assert.AreEqual(StringUtilities.WrapInDiv(ORIGINAL_COMMENT), discussion.Comment,
                "Original comment and comment returned after discussion created different!");

            var discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _authorUser);
            var statusId = GetStatusId(discussions, ThreadStatus.CLOSED);

            var comment = new RaptorComment()
            {
                StatusId = statusId
            };

            IDiscussionAdapter updatedDiscussion = null;
            Assert.DoesNotThrow(() =>
            {
                updatedDiscussion = artifact.UpdateRapidReviewArtifactDiscussion(discussion, comment, _authorUser);
            }, "UpdateDiscussion shouldn't throw any error, but it did.");

            Assert.IsTrue(updatedDiscussion.IsClosed, "The discussion should be closed at this point, but it is open!");

            statusId = GetStatusId(discussions, ThreadStatus.OPEN);
            comment.Comment = "Reopened text";
            comment.StatusId = statusId;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                updatedDiscussion = artifact.UpdateRapidReviewArtifactDiscussion(discussion, comment, _authorUser);
            }, "UpdateDiscussion shouldn't throw any error, but it did.");

            // Verify:
            discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _authorUser);
            Assert.AreEqual(1, discussions.Discussions.Count, "Artifact should have 1 comment, but it has {0}", discussions.Discussions.Count);
            Assert.AreEqual(StringUtilities.WrapInDiv(comment.Comment), updatedDiscussion.Comment, "Updated comment must have proper text.");
            Assert.IsFalse(discussions.Discussions[0].IsClosed, "IsClosed flag should be set to false!");
            RaptorDiscussion.AssertAreEqual(updatedDiscussion, discussions.Discussions[0]);

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _adminUser, artifact.Id, ItemIndicatorFlags.HasComments);
        }

        [Category(Categories.CustomData)]
        [Category(Categories.GoldenData)]
        [TestCase("Custom Status Open", false)]
        [TestCase("Custom Status Close", true)]
        [TestRail(266915)]
        [Description("Update discussion created by user to custom discussion status. Check that comment was updated and discussion status applied.")]
        public void UpdateDiscussion_CustomDiscussionStatuses_SuccessfullyUpdated(string customStatus, bool isClosed)
        {
            // Setup:
            var customDataProject = ArtifactStoreHelper.GetCustomDataProject(_adminUser);
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, customDataProject);

            var artifact = Helper.CreateAndPublishArtifact(customDataProject, author, BaseArtifactType.UseCase);
            var raptorComment = artifact.PostRapidReviewArtifactDiscussion(ORIGINAL_COMMENT, author);
            Assert.AreEqual(StringUtilities.WrapInDiv(ORIGINAL_COMMENT), raptorComment.Comment,
                "Original comment and comment returned after discussion created different!");

            var discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, author);

            var statusId = GetStatusId(discussions, customStatus);
            var comment = new RaptorComment()
            {
                Comment = "Custom status applied",
                StatusId = statusId
            };

            // Execute:
            IDiscussionAdapter updatedDiscussion = null;
            Assert.DoesNotThrow(() =>
            {
                updatedDiscussion = artifact.UpdateRapidReviewArtifactDiscussion(raptorComment, comment, author);
            }, "UpdateDiscussion shouldn't throw any error, but it did.");

            // Verify:
            discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, author);
            Assert.AreEqual(1, discussions.Discussions.Count, "Artifact should have 1 comment, but it has {0}", discussions.Discussions.Count);
            Assert.AreEqual(StringUtilities.WrapInDiv(comment.Comment), updatedDiscussion.Comment, "Updated comment must have proper text.");
            Assert.AreEqual(isClosed, discussions.Discussions[0].IsClosed, "IsClosed flag should be set to {0}!", isClosed);
            RaptorDiscussion.AssertAreEqual(updatedDiscussion, discussions.Discussions[0], skipCanEdit: isClosed);

            var indicator = isClosed ? (ItemIndicatorFlags?)null : ItemIndicatorFlags.HasComments;

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _adminUser, artifact.Id, indicator);
        }

        [TestCase(int.MaxValue)]
        [TestRail(266927)]
        [Description("Update discussion created by user to custom discussion status. Check that comment was updated and discussion status applied.")]
        public void UpdateDiscussion_NonExistingStatus_SuccessfullyUpdatedOnlyComment(int nonExistingStatusId)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UseCase);
            var raptorComment = artifact.PostRapidReviewArtifactDiscussion(ORIGINAL_COMMENT, _adminUser);
            Assert.AreEqual(StringUtilities.WrapInDiv(ORIGINAL_COMMENT), raptorComment.Comment,
                "Original comment and comment returned after discussion created different!");

            var comment = new RaptorComment()
            {
                Comment = UPDATED_TEXT,
                StatusId = nonExistingStatusId
            };

            // Execute:
            IDiscussionAdapter updatedDiscussion = null;
            Assert.DoesNotThrow(() =>
            {
                updatedDiscussion = artifact.UpdateRapidReviewArtifactDiscussion(raptorComment, comment, _adminUser);
            }, "UpdateDiscussion shouldn't throw any error, but it did.");

            // Verify:
            var discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _adminUser);
            Assert.AreEqual(1, discussions.Discussions.Count, "Artifact should have 1 comment, but it has {0}", discussions.Discussions.Count);
            Assert.AreEqual(StringUtilities.WrapInDiv(comment.Comment), updatedDiscussion.Comment, "Updated comment must have proper text.");
            RaptorDiscussion.AssertAreEqual(updatedDiscussion, discussions.Discussions[0]);

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _adminUser, artifact.Id, ItemIndicatorFlags.HasComments);
        }

        [TestCase]
        [TestRail(156534)]
        [Description("Delete own comment, check that comment was deleted.")]
        public void DeleteOwnDiscussion_UserHasRightsToDelete_SuccessfullyDeleted()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UIMockup);
            IRaptorDiscussion raptorComment = null;

            string comment = string.Empty;
            for (int i = 0; i < 2; i++)
            {
                comment = ORIGINAL_COMMENT + " " + (i + 1);
                raptorComment = artifact.PostRapidReviewArtifactDiscussion(comment, _authorUser);
            }
            Assert.AreEqual(StringUtilities.WrapInDiv(comment), raptorComment.Comment,
                "Original comment and comment returned after discussion created different!");

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                artifact.DeleteRapidReviewArtifactDiscussion(_authorUser, raptorComment);
            }, "DeleteDiscussions shouldn't throw any error, but it does.");

            // Verify:
            var discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _adminUser);
            Assert.AreEqual(1, discussions.Discussions.Count, "Artifact should have 1 comment, but it has {0} comments", discussions.Discussions.Count);

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, artifact.Id, ItemIndicatorFlags.HasComments);
        }

        [TestCase]
        [TestRail(156535)]
        [Description("Admin deletes other user's comment, check that comment was deleted.")]
        public void DeleteOtherUserDiscussion_Admin_SuccessfullyDeleted()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UIMockup);
            IRaptorDiscussion raptorComment = null;

            string comment = string.Empty;
            for (int i = 0; i < 2; i++)
            {
                comment = ORIGINAL_COMMENT + " " + (i + 1);
                raptorComment = artifact.PostRapidReviewArtifactDiscussion(comment, _authorUser);
            }
            Assert.AreEqual(StringUtilities.WrapInDiv(comment), raptorComment.Comment,
                "Original comment and comment returned after discussion created different!");

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                artifact.DeleteRapidReviewArtifactDiscussion(_adminUser, raptorComment);
            }, "DeleteDiscussions shouldn't throw any error, but it does.");

            // Verify:
            var discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _authorUser);
            Assert.AreEqual(1, discussions.Discussions.Count, "Artifact should have 1 comment, but it has {0} comments", discussions.Discussions.Count);

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _adminUser, artifact.Id, ItemIndicatorFlags.HasComments);
        }

        [TestCase]
        [TestRail(156536)]
        [Description("User updates its own reply, check that reply was updated.")]
        public void UpdateOwnReply_NonAdminUser_SuccessfullyUpdated()
        {
            // Setup:
            const string REPLY = "Reply";

            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.Glossary);
            var raptorComment = artifact.PostRapidReviewArtifactDiscussion(ORIGINAL_COMMENT, _adminUser);
            Assert.AreEqual(StringUtilities.WrapInDiv(ORIGINAL_COMMENT), raptorComment.Comment,
                "Original comment and comment returned after discussion created different!");
            var raptorReply = OpenApiArtifact.PostRaptorDiscussionReply(Helper.BlueprintServer.Address, raptorComment, REPLY, _authorUser);
            Assert.AreEqual(StringUtilities.WrapInDiv(REPLY), raptorReply.Comment,
                "Original reply and reply returned after reply created different!");

            string newReplyText = "New " + REPLY;
            IReplyAdapter updatedReply = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                updatedReply = OpenApiArtifact.UpdateRaptorDiscussionReply(Helper.BlueprintServer.Address, raptorComment, raptorReply, newReplyText, _authorUser);
            }, "UpdateReply shouldn't throw any error, but it does.");

            // Verify:
            var discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _adminUser);
            Assert.AreEqual(1, discussions.Discussions.Count, "Artifact should have 1 comment, but it has {0} comments", discussions.Discussions.Count);
            Assert.AreEqual(1, discussions.Discussions[0].RepliesCount, "Discussion should have 1 reply, but it has {0} replies",
                discussions.Discussions[0].RepliesCount);
            Assert.AreEqual(StringUtilities.WrapInDiv(newReplyText), updatedReply.Comment,
                "Original comment and comment returned after discussion created different!");

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _adminUser, artifact.Id, ItemIndicatorFlags.HasComments);
        }

        [TestCase]
        [TestRail(156537)]
        [Description("Post comment for artifact with version 2, check that comment has version 2.")]
        public void PostNewDiscussion_ArtifactHasVersion2_CheckCommentHasVersion2()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UseCase, numberOfVersions: 2); //artifact version is 2

            IDiscussionAdapter postedRaptorComment = null;
            DiscussionResultSet discussions = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                postedRaptorComment = artifact.PostRapidReviewArtifactDiscussion(ORIGINAL_COMMENT, _authorUser);
                artifact.Save(_authorUser);
                artifact.Publish(_authorUser);  //artifact version is 3
                discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _adminUser);
            }, "UpdateDiscussions and GetDiscussion shouldn't throw any error.");

            // Verify:
            Assert.AreEqual(1, discussions.Discussions.Count, "Artifact should have 1 comment, but it has {0}", discussions.Discussions.Count);
            Assert.AreEqual(2, discussions.Discussions[0].Version, "Version should be 2, but it is {0}", discussions.Discussions[0].Version);
            Assert.AreEqual(2, postedRaptorComment.Version, "Version should be 2, but it is {0}", postedRaptorComment.Version);

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _adminUser, artifact.Id, ItemIndicatorFlags.HasComments);
        }

        #endregion Positive tests

        #region 401 Unauthorized

        [TestCase]
        [TestRail(266928)]
        [Description("Get discussion with no token in a header. Verify 401 Unauthorized HTTP status was returned")]
        public void GetDiscussion_MissingSessionToken_401Unauthorized()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.Glossary);
            var raptorComment = artifact.PostRapidReviewArtifactDiscussion(ORIGINAL_COMMENT, _adminUser);
            Assert.AreEqual(StringUtilities.WrapInDiv(ORIGINAL_COMMENT), raptorComment.Comment);

            // Execute:
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Artifacts_id_.DISCUSSIONS, artifact.Id);

            var ex = Assert.Throws<Http401UnauthorizedException>(() => Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, user: null),
                "'GET {0}' should return 401 Unauthorized when request is done without token!", path);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, TOKEN_MISSING_OR_MALFORMED);
        }

        [TestCase]
        [TestRail(266929)]
        [Description("Update discussion with no token in the header. Verify 401 Unauthorized HTTP status was returned")]
        public void UpdateDiscussion_MissingSessionToken_401Unauthorized()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.Glossary);
            var raptorComment = artifact.PostRapidReviewArtifactDiscussion(ORIGINAL_COMMENT, _adminUser);
            Assert.AreEqual(StringUtilities.WrapInDiv(ORIGINAL_COMMENT), raptorComment.Comment,
                "Original comment and comment returned after discussion created different!");

            var discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _adminUser);
            var statusId = GetStatusId(discussions, ThreadStatus.CLOSED);
            var comment = new RaptorComment()
            {
                Comment = UPDATED_TEXT,
                StatusId = statusId
            };

            // Execute:
            string path = I18NHelper.FormatInvariant(
                RestPaths.Svc.Components.RapidReview.Artifacts_id_.Discussions_id_.COMMENT, artifact.Id, raptorComment.DiscussionId);

            var ex = Assert.Throws<Http401UnauthorizedException>(() => artifact.UpdateRapidReviewArtifactDiscussion(discussionToUpdate: raptorComment, comment: comment, user: null),
                "'PATCH {0}' should return 401 Unauthorized when request is done without token!", path);
            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, UNAUTHORIZED_CALL);
        }

        #endregion 401 Unauthorized

        #region 403 Forbidden

        [TestCase]
        [TestRail(155771)]
        [Description("Try to delete comment created by other user, no privilege to delete any comment, check that it returns 403.")]
        public void DeleteComment_UserHasNoRightsToDelete_403Forbidden()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UIMockup);
            var raptorComment = artifact.PostRapidReviewArtifactDiscussion(ORIGINAL_COMMENT, _adminUser);
            Assert.AreEqual(StringUtilities.WrapInDiv(ORIGINAL_COMMENT), raptorComment.Comment,
                "Original comment and comment returned after discussion created different!");

            // Execute:
            Assert.Throws<Http403ForbiddenException>(() =>
            {
                artifact.DeleteRapidReviewArtifactDiscussion(_authorUser, raptorComment);
            }, "DeleteDiscussions should throw 403 error, but it doesn't.");

            // Verify:
            var discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _authorUser);
            Assert.AreEqual(1, discussions.Discussions.Count, "Artifact should have 1 comment, but it has {0} comments", discussions.Discussions.Count);
            Assert.AreEqual(StringUtilities.WrapInDiv(ORIGINAL_COMMENT), discussions.Discussions[0].Comment, "Comment shouldn't change, but it did.");
        }

        [TestCase]
        [TestRail(156529)]
        [Description("Try to delete reply created by other user, no privilege to delete any comment, check that it returns 403.")]
        public void DeleteReply_UserHasNoRightsToDelete_403Forbidden()
        {
            // Setup:
            const string REPLY = "Reply";

            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UIMockup);
            var raptorComment = artifact.PostRapidReviewArtifactDiscussion(ORIGINAL_COMMENT, _adminUser);
            Assert.AreEqual(StringUtilities.WrapInDiv(ORIGINAL_COMMENT), raptorComment.Comment,
                "Original comment and comment returned after discussion created different!");

            var raptorReply = OpenApiArtifact.PostRaptorDiscussionReply(Helper.BlueprintServer.Address, raptorComment, REPLY, _adminUser);

            // Execute:
            Assert.Throws<Http403ForbiddenException>(() =>
            {
                artifact.DeleteRapidReviewArtifactReply(raptorReply, _authorUser);
            }, "{0} should throw 403 error, but it doesn't.", nameof(artifact.DeleteRapidReviewArtifactReply));

            // Verify:
            var discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _authorUser);
            Assert.AreEqual(1, discussions.Discussions.Count, "Artifact should have 1 comment, but it has {0} comments", discussions.Discussions.Count);
            Assert.AreEqual(1, discussions.Discussions[0].RepliesCount, "Discussion should have 1 reply, but it has {0} replies",
                discussions.Discussions[0].RepliesCount);
        }

        [TestCase]
        [TestRail(266958)]
        [Description("Get discussion with user that created discussion and lost permissions for the artifact. " + 
            "Verify 403 Forbidden HTTP status was returned")]
        public void GetDiscussion_InsufficientPermissionsToArtifact_403Forbidden()
        {
            // Setup:
            var user = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var artifact = Helper.CreateAndPublishArtifact(_project, user, BaseArtifactType.Glossary);
            var raptorComment = artifact.PostRapidReviewArtifactDiscussion(ORIGINAL_COMMENT, user);
            Assert.AreEqual(StringUtilities.WrapInDiv(ORIGINAL_COMMENT), raptorComment.Comment,
                "Original comment and comment returned after discussion created different!");

            Helper.AssignProjectRolePermissionsToUser(user, TestHelper.ProjectRole.None, _project, artifact);

            // Execute:
            Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, user),
                "'GET {0}' should return 403 Forbidden when user tries to get comment from artifact to which he/she does not have permissions!",
                I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Artifacts_id_.DISCUSSIONS, artifact.Id));

        // Verify:
        // TODO: No internal error & message 
        // Bug: http://svmtfs2015:8080/tfs/svmtfs2015/Blueprint/Titan/_workItems?searchText=%5BTechDebt%5D&_a=edit&id=5699&triage=true
        }

        [TestCase]
        [TestRail(266959)]
        [Description("Update discussion with user that created discussion and lost permissions for the artifact. " + 
            "Verify 403 Forbidden HTTP status was returned")]
        public void UpdateDiscussion_InsufficientPermissionsToArtifact_403Forbidden()
        {
            // Setup:
            var user = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var artifact = Helper.CreateAndPublishArtifact(_project, user, BaseArtifactType.Glossary);

            var raptorComment = artifact.PostRapidReviewArtifactDiscussion(ORIGINAL_COMMENT, user);
            Assert.AreEqual(StringUtilities.WrapInDiv(ORIGINAL_COMMENT), raptorComment.Comment,
                "Original comment and comment returned after discussion created different!");

            var discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, user);
            var statusId = GetStatusId(discussions, ThreadStatus.CLOSED);
            var comment = new RaptorComment()
            {
                Comment = UPDATED_TEXT,
                StatusId = statusId
            };

            Helper.AssignProjectRolePermissionsToUser(user, TestHelper.ProjectRole.Viewer, _project, artifact);

            // Execute:
            var path = I18NHelper.FormatInvariant(
                    RestPaths.Svc.Components.RapidReview.Artifacts_id_.Discussions_id_.COMMENT, artifact.Id, raptorComment.DiscussionId);
            var ex = Assert.Throws<Http403ForbiddenException>(() => artifact.UpdateRapidReviewArtifactDiscussion(raptorComment, comment, user),
                "'PATCH {0}' should return 403 Forbidden when user tries to update comment for artifact to which he/she does not have permissions!", path);

            // Verify
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, NO_PERMISSIONS_TO_COMPLETE_OPERATION);
        }

        [TestCase]
        [TestRail(156533)]
        [Description("Admin tries to update comment created by user - it returns 403, check that comment wasn't updated.")]
        public void UpdateOtherUserDiscussion_AdminUser_403Forbidden()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UseCase);
            var raptorComment = artifact.PostRapidReviewArtifactDiscussion(ORIGINAL_COMMENT, _authorUser);
            Assert.AreEqual(StringUtilities.WrapInDiv(ORIGINAL_COMMENT), raptorComment.Comment,
                "Original comment and comment returned after discussion created different!");

            var comment = new RaptorComment()
            {
                Comment = UPDATED_TEXT
            };

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                artifact.UpdateRapidReviewArtifactDiscussion(raptorComment, comment, _adminUser);
            }, "UpdateDiscussion should throw 403 error, but it doesn't.");

            // Verify:
            var discussion = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _authorUser);
            Assert.AreEqual(1, discussion.Discussions.Count, "Discussions must have 1 comment, but it has {0}", discussion.Discussions.Count);
            Assert.AreEqual(raptorComment.Comment, discussion.Discussions[0].Comment, "Comment must remain unmodified.");
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, NO_PERMISSIONS_TO_COMPLETE_OPERATION);
        }

        #endregion 403 Forbidden

        #region 404 Not Found

        [TestCase]
        [TestRail(146054)]
        [Description("Add comment to saved artifact, then get discussion for this artifact.  Verify it returns 404.")]
        public void GetDiscussions_DraftArtifact_Returns404()
        {
            // Setup:
            var artifact = Helper.CreateArtifact(_project, _adminUser, BaseArtifactType.Actor);
            artifact.Save(_adminUser);

            artifact.PostRapidReviewArtifactDiscussion("draft", _adminUser);
            DiscussionResultSet discussions = null;

            // In Nova UI doesn't allow to post discussion for never published artifact, it is possible to do by adding comment in SL and saving it
            // In this case it would be a conflict - in SL user will see comment and in Nova he will not.
            // For consistency server shouldn't allow to create comment for never published artifact.
            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _adminUser);
            }, "GetArtifactDiscussions should throw 404 error, but it doesn't.");

            // Verify:
            // TODO: No internal error & message 
            // Bug: http://svmtfs2015:8080/tfs/svmtfs2015/Blueprint/Titan/_workItems?searchText=%5BTechDebt%5D&_a=edit&id=5699&triage=true
        }

        [TestCase]
        [TestRail(146057)]
        [Description("Add comment to subartifact of saved (unpublished) artifact, get discussion for this subartifact.  Verify it returns 404.")]
        public void GetDiscussions_UnpublishedSubArtifact_Returns404()
        {
            // Setup:
            var artifact = Helper.CreateArtifact(_project, _adminUser, BaseArtifactType.Process);
            artifact.Save(_adminUser);
            var postedRaptorComment = AddDiscussionToSubArtifactOfStorytellerProcess(artifact);

            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDiscussions(postedRaptorComment.ItemId, _adminUser);
            }, "GetArtifactDiscussions should throw 404 error, but it doesn't.");

            // Verify:
            // TODO: No internal error & message 
            // Bug: http://svmtfs2015:8080/tfs/svmtfs2015/Blueprint/Titan/_workItems?searchText=%5BTechDebt%5D&_a=edit&id=5699&triage=true
        }

        [TestCase]
        [TestRail(146059)]
        [Description("Add comment to subartifact of saved (unpublished) artifact, try to get discussion for this subartifact with user other than author.  Verify it returns 404 Not Found.")]
        public void GetDiscussions_UnpublishedSubArtifactOtherUser_404NotFound()
        {
            // Setup:
            var artifact = Helper.CreateArtifact(_project, _adminUser, BaseArtifactType.Process);
            artifact.Save(_adminUser);
            var raptorComment = AddDiscussionToSubArtifactOfStorytellerProcess(artifact);

            var user2 = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDiscussions(raptorComment.ItemId, user2);
            }, "GetArtifactDiscussions should return 404 error, but it doesn't.");

            // Verify:
            // TODO: No internal error & message 
            // Bug: http://svmtfs2015:8080/tfs/svmtfs2015/Blueprint/Titan/_workItems?searchText=%5BTechDebt%5D&_a=edit&id=5699&triage=true
        }

        [TestCase]
        [TestRail(266962)]
        [Description("Update discussion that does not exist in an artifact. Verify 404 Not Found HTTP status was returned")]
        public void UpdateDiscussion_NonExistingDiscussion_404NotFound()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.Glossary);
            
            var discussion = new RaptorDiscussion { DiscussionId = int.MaxValue, ItemId = artifact.Id };

            var discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _adminUser);
            var statusId = GetStatusId(discussions, ThreadStatus.CLOSED);

            var comment = new RaptorComment
            {
                Comment = UPDATED_TEXT,
                StatusId = statusId
            };

            // Execute:
            var path = I18NHelper.FormatInvariant(
                    RestPaths.Svc.Components.RapidReview.Artifacts_id_.Discussions_id_.COMMENT, artifact.Id, discussion.DiscussionId);
            var ex = Assert.Throws<Http404NotFoundException>(() => artifact.UpdateRapidReviewArtifactDiscussion(discussion, comment, _adminUser),
                "'PATCH {0}' should return 404 Not Found when user tries to update comment for discussion that does not exist in artifact!", path);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, "Item is no longer accessible");    // TFS bug: 5699  No Error Code returned.
        }

        #endregion 404 Not Found

        #region Private functions

        /// <summary>
        /// Adds a discussion comment to the specified Storyteller Process artifact.
        /// </summary>
        /// <param name="artifact">The Process artifact to add a comment to.</param>
        /// <param name="user">The user credentials for the operation.</param>
        /// <returns>The IRaptorComment returned after posting the comment.</returns>
        private IRaptorDiscussion AddDiscussionToSubArtifactOfStorytellerProcess(IArtifact artifact, IUser user = null)
        {
            if (user == null)
            {
                user = _adminUser;
            }

            var process = Helper.Storyteller.GetProcess(user, artifact.Id);
            var userTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            var postedRaptorComment = Helper.SvcComponents.PostRapidReviewArtifactDiscussion(user, userTask.Id, "text for UT");

            return postedRaptorComment;
        }

        /// <summary>
        /// Gets status id of ThreadStatus by its name
        /// </summary>
        /// <param name="result">DiscussionResultSet after get discussion call</param>
        /// <param name="statusName">Status name</param>
        /// <returns>Status Id</returns>
        private static int GetStatusId(DiscussionResultSet result, string statusName)
        {
            ThrowIf.ArgumentNull(statusName, nameof(statusName));

            var threadStatus = result.ThreadStatuses.Find(a => a.Name == statusName);

            Assert.IsNotNull(threadStatus, "Discussion status is not found among project statuses!");

            return threadStatus.StatusId;
        }



        #endregion Private functions
    }
}