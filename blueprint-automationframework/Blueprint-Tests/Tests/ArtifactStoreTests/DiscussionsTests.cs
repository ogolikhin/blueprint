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
        private const string NOT_FOUND_OR_NO_PERMISSION = "You have attempted to access an item that does not exist or you do not have permission to view.";
        private const string TOKEN_MISSING_OR_MALFORMED = "Token is missing or malformed.";
        private const string UNAUTHORIZED_CALL = "Unauthorized call";

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
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.Actor);

            var postedRaptorDiscussion = PostRapidReviewDiscussion(_authorUser, artifact.Id, "draft");
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
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.Actor);
            var raptorDiscussion = PostRapidReviewDiscussion(_adminUser, artifact.Id, "draft");
            artifact.Delete(_adminUser);
            DiscussionResultSet discussions = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _adminUser);
            }, "GetArtifactDiscussions shouldn't throw any error, but it doesn't.");

            // Verify:
            Assert.AreEqual(1, discussions.Discussions.Count, "Artifact should have 1 comment, but it has {0}", discussions.Discussions.Count);
            RaptorDiscussion.AssertAreEqual(raptorDiscussion, discussions.Discussions[0]);
            Assert.IsFalse(discussions.Discussions[0].IsClosed, "IsClosed flag should be set to false!");

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, artifact.Id, ItemIndicatorFlags.HasComments);
        }

        [TestCase]
        [TestRail(146056)]
        [Description("Add comment to published subartifact, get discussion for this subartifact, check that it has expected values.")]
        public void GetDiscussions_PublishedSubArtifact_ReturnsCorrectDiscussion()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.Process);
            var postedComment = AddDiscussionToSubArtifactOfStorytellerProcess(artifact.Id);

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
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.Process);
            var postedDiscussion = AddDiscussionToSubArtifactOfStorytellerProcess(artifact.Id);

            DiscussionResultSet discussions = Helper.ArtifactStore.GetArtifactDiscussions(postedDiscussion.ItemId, _adminUser);

            var postedReply = PostRapidReviewDiscussionReply(_authorUser, postedDiscussion, "This is a reply to a comment.");

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

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _adminUser, artifact.Id, ItemIndicatorFlags.HasComments, postedDiscussion.ItemId);
        }

        [TestCase]
        [TestRail(146063)]
        [Description("Add comment to subartifact of published artifact, delete artifact (don't publish), get discussion for this subartifact.  Verify it returns expected discussion.")]
        public void GetDiscussions_MarkedForDeleteSubArtifact_ValidateReturnedDiscussion()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.Process);
            var postedComment = AddDiscussionToSubArtifactOfStorytellerProcess(artifact.Id);

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
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.Glossary);

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
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.UseCase);

            var raptorDiscussion = PostRapidReviewDiscussion(_authorUser, artifact.Id, "original discussion text");
            var discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _authorUser);

            Assert.AreEqual(1, discussions.Discussions.Count, "There should be 1 comment returned!");
            RaptorDiscussion.AssertAreEqual(raptorDiscussion, discussions.Discussions[0]);

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
                updatedDiscussion = UpdateRapidReviewDiscussion(_authorUser, raptorDiscussion, comment);
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
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.Process);
            var raptorDiscussion = AddDiscussionToSubArtifactOfStorytellerProcess(artifact.Id, _authorUser);

            var discussions = Helper.ArtifactStore.GetArtifactDiscussions(raptorDiscussion.ItemId, _authorUser);
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
                updatedDiscussion = UpdateRapidReviewDiscussion(_authorUser, raptorDiscussion, comment);
            }, "UpdateDiscussions shouldn't throw any error.");

            // Verify:
            discussions = Helper.ArtifactStore.GetArtifactDiscussions(raptorDiscussion.ItemId, _authorUser);

            Assert.AreEqual(1, discussions.Discussions.Count, "Artifact should have 1 comment, but it has {0}", discussions.Discussions.Count);
            Assert.AreEqual(StringUtilities.WrapInDiv(comment.Comment), updatedDiscussion.Comment, "Updated comment must have updated value, but it didn't.");
            Assert.IsTrue(discussions.Discussions[0].IsClosed, "IsClosed flag should be set to true!");
            RaptorDiscussion.AssertAreEqual(updatedDiscussion, discussions.Discussions[0], skipCanEdit: true);

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _adminUser, artifact.Id, expectedIndicatorFlags: null, subArtifactId: raptorDiscussion.ItemId);
        }

        [TestCase]
        [TestRail(156531)]
        [Description("Delete reply created by the current user, check that reply was deleted.")]
        public void DeleteReply_DeleteByReplyAuthor_SuccessfullyDeleted()
        {
            // Setup:
            const string REPLY = "Reply";
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.UIMockup);
            var raptorDiscussion = PostRapidReviewDiscussion(_adminUser, artifact.Id, ORIGINAL_COMMENT);

            string replyText = null;
            IRaptorReply raptorReply = null;

            for (int i = 0; i < 2; i++)
            {
                replyText = REPLY + " " + (i + 1);
                raptorReply = PostRapidReviewDiscussionReply(_authorUser, raptorDiscussion, replyText);
            }

            Assert.NotNull(raptorReply, "raptorReply should not be null!");

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                DeleteRapidReviewDiscussionReply(_authorUser, raptorReply);
            }, "{0} shouldn't throw any error, but it did.", nameof(Helper.SvcComponents.DeleteRapidReviewDiscussionReply));

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
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.UseCase);
            var raptorDiscussion = PostRapidReviewDiscussion(_authorUser, artifact.Id, ORIGINAL_COMMENT);

            var discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _authorUser);
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
                updatedDiscussion = UpdateRapidReviewDiscussion(_authorUser, raptorDiscussion, comment);
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
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.UseCase);
            var raptorDiscussion = PostRapidReviewDiscussion(_authorUser, artifact.Id, ORIGINAL_COMMENT);

            var discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _authorUser);
            var statusId = GetStatusId(discussions, ThreadStatus.CLOSED);

            var comment = new RaptorComment()
            {
                StatusId = statusId
            };

            IDiscussionAdapter updatedDiscussion = null;
            Assert.DoesNotThrow(() =>
            {
                updatedDiscussion = UpdateRapidReviewDiscussion(_authorUser, raptorDiscussion, comment);
            }, "UpdateDiscussion shouldn't throw any error, but it did.");

            Assert.IsTrue(updatedDiscussion.IsClosed, "The discussion should be closed at this point, but it is open!");

            statusId = GetStatusId(discussions, ThreadStatus.OPEN);
            comment.Comment = "Reopened text";
            comment.StatusId = statusId;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                updatedDiscussion = UpdateRapidReviewDiscussion(_authorUser, raptorDiscussion, comment);
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
        [TestCase("Custom Status Open", false)]
        [TestCase("Custom Status Close", true)]
        [TestRail(266915)]
        [Description("Update discussion created by user to custom discussion status. Check that comment was updated and discussion status applied.")]
        public void UpdateDiscussion_CustomDiscussionStatuses_SuccessfullyUpdated(string customStatus, bool isClosed)
        {
            // Setup:
            var customDataProject = ArtifactStoreHelper.GetCustomDataProject(_adminUser);
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, customDataProject);

            var artifact = Helper.CreateAndPublishNovaArtifact(author, customDataProject, ItemTypePredefined.UseCase);
            var raptorDiscussion = PostRapidReviewDiscussion(author, artifact.Id, ORIGINAL_COMMENT);

            var discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, author);

            var statusId = GetStatusId(discussions, customStatus);
            var comment = new RaptorComment
            {
                Comment = "Custom status applied",
                StatusId = statusId
            };

            // Execute:
            IDiscussionAdapter updatedDiscussion = null;
            Assert.DoesNotThrow(() =>
            {
                updatedDiscussion = UpdateRapidReviewDiscussion(author, raptorDiscussion, comment);
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
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.UseCase);
            var raptorDiscussion = PostRapidReviewDiscussion(_adminUser, artifact.Id, ORIGINAL_COMMENT);

            var comment = new RaptorComment
            {
                Comment = UPDATED_TEXT,
                StatusId = nonExistingStatusId
            };

            // Execute:
            IDiscussionAdapter updatedDiscussion = null;
            Assert.DoesNotThrow(() =>
            {
                updatedDiscussion = UpdateRapidReviewDiscussion(_adminUser, raptorDiscussion, comment);
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
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.UIMockup);
            IRaptorDiscussion raptorDiscussion = null;
            string comment = string.Empty;

            for (int i = 0; i < 2; i++)
            {
                comment = ORIGINAL_COMMENT + " " + (i + 1);
                raptorDiscussion = PostRapidReviewDiscussion(_authorUser, artifact.Id, comment);
            }

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                DeleteRapidReviewDiscussion(_authorUser, raptorDiscussion);
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
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.UIMockup);
            IRaptorDiscussion raptorDiscussion = null;
            string comment = string.Empty;

            for (int i = 0; i < 2; i++)
            {
                comment = ORIGINAL_COMMENT + " " + (i + 1);
                raptorDiscussion = PostRapidReviewDiscussion(_authorUser, artifact.Id, comment);
            }

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                DeleteRapidReviewDiscussion(_adminUser, raptorDiscussion);
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

            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.Glossary);
            var raptorDiscussion = PostRapidReviewDiscussion(_adminUser, artifact.Id, ORIGINAL_COMMENT);
            var raptorReply = PostRapidReviewDiscussionReply(_authorUser, raptorDiscussion, REPLY);

            string newReplyText = "New " + REPLY;
            IRaptorReply updatedReply = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                updatedReply = UpdateRapidReviewDiscussionReply(_authorUser, raptorReply, newReplyText);
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
            var artifact = Helper.CreateAndPublishNovaArtifactWithMultipleVersions(_adminUser, _project, ItemTypePredefined.UseCase, numberOfVersions: 2); //artifact version is 2

            IDiscussionAdapter postedRaptorComment = null;
            DiscussionResultSet discussions = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                postedRaptorComment = PostRapidReviewDiscussion(_authorUser, artifact.Id, ORIGINAL_COMMENT);
            }, "{0} shouldn't return an error.", nameof(Helper.SvcComponents.PostRapidReviewDiscussion));

            // Verify:
            artifact.Lock(_authorUser);
            artifact.SaveWithNewDescription(_authorUser);
            artifact.Publish(_authorUser);  //artifact version is 3
            discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _adminUser);

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
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.Glossary);
            PostRapidReviewDiscussion(_adminUser, artifact.Id, ORIGINAL_COMMENT);

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
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.Glossary);
            var raptorDiscussion = PostRapidReviewDiscussion(_adminUser, artifact.Id, ORIGINAL_COMMENT);

            var discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _adminUser);
            var statusId = GetStatusId(discussions, ThreadStatus.CLOSED);
            var comment = new RaptorComment
            {
                Comment = UPDATED_TEXT,
                StatusId = statusId
            };

            // Execute:
            string path = I18NHelper.FormatInvariant(
                RestPaths.Svc.Components.RapidReview.Artifacts_id_.Discussions_id_.COMMENT, artifact.Id, raptorDiscussion.DiscussionId);

            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                UpdateRapidReviewDiscussion(user: null, discussionToUpdate: raptorDiscussion, comment: comment);
            }, "'PATCH {0}' should return 401 Unauthorized when request is done without token!", path);

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
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.UIMockup);
            var raptorDiscussion = PostRapidReviewDiscussion(_adminUser, artifact.Id, ORIGINAL_COMMENT);

            // Execute:
            Assert.Throws<Http403ForbiddenException>(() =>
            {
                DeleteRapidReviewDiscussion(_authorUser, raptorDiscussion);
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

            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.UIMockup);
            var raptorDiscussion = PostRapidReviewDiscussion(_adminUser,artifact.Id, ORIGINAL_COMMENT);
            var raptorReply = PostRapidReviewDiscussionReply(_adminUser, raptorDiscussion, REPLY);

            // Execute:
            Assert.Throws<Http403ForbiddenException>(() =>
            {
                DeleteRapidReviewDiscussionReply(_authorUser, raptorReply);
            }, "{0} should throw 403 error, but it doesn't.", nameof(Helper.SvcComponents.DeleteRapidReviewDiscussionReply));

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

            var artifact = Helper.CreateAndPublishNovaArtifact(user, _project, ItemTypePredefined.Glossary);
            PostRapidReviewDiscussion(user, artifact.Id, ORIGINAL_COMMENT);

            Helper.AssignProjectRolePermissionsToUser(user, TestHelper.ProjectRole.None, _project, artifact);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, user),
                "'GET {0}' should return 403 Forbidden when user tries to get comment from artifact to which he/she does not have permissions!",
                I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Artifacts_id_.DISCUSSIONS, artifact.Id));

            // Verify:
            // Note: No internal error & message.  Bug #5699
            TestHelper.AssertResponseBodyIsEmpty(ex.RestResponse);
        }

        [TestCase]
        [TestRail(266959)]
        [Description("Update discussion with user that created discussion and lost permissions for the artifact. " + 
                     "Verify 403 Forbidden HTTP status was returned")]
        public void UpdateDiscussion_InsufficientPermissionsToArtifact_403Forbidden()
        {
            // Setup:
            var user = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var artifact = Helper.CreateAndPublishNovaArtifact(user, _project, ItemTypePredefined.Glossary);
            var raptorDiscussion = PostRapidReviewDiscussion(user, artifact.Id, ORIGINAL_COMMENT);

            var discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, user);
            var statusId = GetStatusId(discussions, ThreadStatus.CLOSED);
            var comment = new RaptorComment
            {
                Comment = UPDATED_TEXT,
                StatusId = statusId
            };

            Helper.AssignProjectRolePermissionsToUser(user, TestHelper.ProjectRole.Viewer, _project, artifact);

            // Execute:
            var path = I18NHelper.FormatInvariant(
                    RestPaths.Svc.Components.RapidReview.Artifacts_id_.Discussions_id_.COMMENT, artifact.Id, raptorDiscussion.DiscussionId);

            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                UpdateRapidReviewDiscussion(user, raptorDiscussion, comment);
            }, "'PATCH {0}' should return 403 Forbidden when user tries to update comment for artifact to which he/she does not have permissions!", path);

            // Verify
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, NO_PERMISSIONS_TO_COMPLETE_OPERATION);
        }

        [TestCase]
        [TestRail(156533)]
        [Description("Admin tries to update comment created by user - it returns 403, check that comment wasn't updated.")]
        public void UpdateOtherUserDiscussion_AdminUser_403Forbidden()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.UseCase);
            var raptorDiscussion = PostRapidReviewDiscussion(_authorUser, artifact.Id, ORIGINAL_COMMENT);

            var comment = new RaptorComment
            {
                Comment = UPDATED_TEXT
            };

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                UpdateRapidReviewDiscussion(_adminUser, raptorDiscussion, comment);
            }, "UpdateDiscussion should throw 403 error, but it doesn't.");

            // Verify:
            var discussion = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _authorUser);

            Assert.AreEqual(1, discussion.Discussions.Count, "Discussions must have 1 comment, but it has {0}", discussion.Discussions.Count);
            Assert.AreEqual(raptorDiscussion.Comment, discussion.Discussions[0].Comment, "Comment must remain unmodified.");
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, NO_PERMISSIONS_TO_COMPLETE_OPERATION);
        }

        #endregion 403 Forbidden

        #region 404 Not Found

        [TestCase]
        [TestRail(146054)]
        [Description("Add comment to unpublished artifact, then get discussion for this artifact.  Verify it returns 404.")]
        public void GetDiscussions_UnpublishedArtifact_Returns404()
        {
            // Setup:
            var artifact = Helper.CreateNovaArtifact(_adminUser, _project, ItemTypePredefined.Actor);
            PostRapidReviewDiscussion(_adminUser, artifact.Id, "draft");

            DiscussionResultSet discussions = null;

            // Execute:
            // In Nova UI doesn't allow to post discussion for never published artifact, it is possible to do by adding comment in SL and saving it
            // In this case it would be a conflict - in SL user will see comment and in Nova he will not.
            // For consistency server shouldn't allow to create comment for never published artifact.
            var ex = Assert.Throws<Http404NotFoundException>(() =>
            {
                discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _adminUser);
            }, "GetArtifactDiscussions should throw 404 error, but it doesn't.");

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.ArtifactNotFound, NOT_FOUND_OR_NO_PERMISSION);
        }

        [TestCase]
        [TestRail(146057)]
        [Description("Add comment to subartifact of saved (unpublished) artifact, get discussion for this subartifact.  Verify it returns 404.")]
        public void GetDiscussions_UnpublishedSubArtifact_Returns404()
        {
            // Setup:
            var artifact = Helper.CreateNovaArtifact(_adminUser, _project, ItemTypePredefined.Process);
            var postedRaptorComment = AddDiscussionToSubArtifactOfStorytellerProcess(artifact.Id);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDiscussions(postedRaptorComment.ItemId, _adminUser);
            }, "GetArtifactDiscussions should throw 404 error, but it doesn't.");

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.ArtifactNotFound, NOT_FOUND_OR_NO_PERMISSION);
        }

        [TestCase]
        [TestRail(146059)]
        [Description("Add comment to subartifact of saved (unpublished) artifact, try to get discussion for this subartifact with user other than author.  Verify it returns 404 Not Found.")]
        public void GetDiscussions_UnpublishedSubArtifactOtherUser_404NotFound()
        {
            // Setup:
            var artifact = Helper.CreateNovaArtifact(_adminUser, _project, ItemTypePredefined.Process);
            var raptorDiscussion = AddDiscussionToSubArtifactOfStorytellerProcess(artifact.Id);

            var user2 = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDiscussions(raptorDiscussion.ItemId, user2);
            }, "GetArtifactDiscussions should return 404 error, but it doesn't.");

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.ArtifactNotFound, NOT_FOUND_OR_NO_PERMISSION);
        }

        [TestCase]
        [TestRail(266962)]
        [Description("Update discussion that does not exist in an artifact. Verify 404 Not Found HTTP status was returned")]
        public void UpdateDiscussion_NonExistingDiscussion_404NotFound()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.Glossary);
            
            var raptorDiscussion = new RaptorDiscussion { DiscussionId = int.MaxValue, ItemId = artifact.Id };

            var discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _adminUser);
            var statusId = GetStatusId(discussions, ThreadStatus.CLOSED);

            var comment = new RaptorComment
            {
                Comment = UPDATED_TEXT,
                StatusId = statusId
            };

            // Execute:
            var path = I18NHelper.FormatInvariant(
                    RestPaths.Svc.Components.RapidReview.Artifacts_id_.Discussions_id_.COMMENT, artifact.Id, raptorDiscussion.DiscussionId);

            var ex = Assert.Throws<Http404NotFoundException>(() =>
            {
                UpdateRapidReviewDiscussion(_adminUser, raptorDiscussion, comment);
            }, "'PATCH {0}' should return 404 Not Found when user tries to update comment for discussion that does not exist in artifact!", path);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, "Item is no longer accessible");    // TFS bug: 5699  No Error Code returned.
        }

        #endregion 404 Not Found

        #region Private functions

        /// <summary>
        /// Adds a discussion comment to the specified Storyteller Process artifact.
        /// </summary>
        /// <param name="artifactId">The ID of a Process artifact to add a comment to.</param>
        /// <param name="user">The user credentials for the operation.</param>
        /// <returns>The IRaptorComment returned after posting the comment.</returns>
        private IRaptorDiscussion AddDiscussionToSubArtifactOfStorytellerProcess(int artifactId, IUser user = null)
        {
            if (user == null)
            {
                user = _adminUser;
            }

            var process = Helper.Storyteller.GetProcess(user, artifactId);
            var userTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            var postedRaptorComment = PostRapidReviewDiscussion(user, userTask.Id, "text for UT");

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

        /// <summary>
        /// Posts a new RapidReview Discussion to an artifact with the specified comment.
        /// </summary>
        /// <param name="user">The user to authenticate.</param>
        /// <param name="artifactId">The ID of the artifact to add the discussion to.</param>
        /// <param name="comment">The comment to add.</param>
        /// <returns>The RaptorDiscussion returned by the POST call.</returns>
        private IRaptorDiscussion PostRapidReviewDiscussion(IUser user, int artifactId, string comment)
        {
            var raptorDiscussion = Helper.SvcComponents.PostRapidReviewDiscussion(user, artifactId, comment);

            Assert.NotNull(raptorDiscussion, "The posted discussion shouldn't be null!");
            Assert.AreEqual(StringUtilities.WrapInDiv(comment), raptorDiscussion.Comment,
                "The comment returned by {0} is different than what was posted!", nameof(Helper.SvcComponents.PostRapidReviewDiscussion));

            return raptorDiscussion;
        }

        /// <summary>
        /// Posts a new RapidReview Reply to a discussion with the specified comment.
        /// </summary>
        /// <param name="user">The user to authenticate.</param>
        /// <param name="raptorDiscussion">The discussion to reply to.</param>
        /// <param name="reply">The reply comment to add.</param>
        /// <returns>The RaptorReply returned by the POST call.</returns>
        private IRaptorReply PostRapidReviewDiscussionReply(IUser user, IRaptorDiscussion raptorDiscussion, string reply)
        {
            var raptorReply = Helper.SvcComponents.PostRapidReviewDiscussionReply(
                user, raptorDiscussion.ItemId, raptorDiscussion.DiscussionId, reply);

            Assert.AreEqual(StringUtilities.WrapInDiv(reply), raptorReply.Comment,
                "The reply returned by {0} is different than what was posted!", nameof(Helper.SvcComponents.PostRapidReviewDiscussionReply));

            return raptorReply;
        }

        /// <summary>
        /// Deletes the specified RaptorDiscussion.
        /// </summary>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="raptorDiscussion">The RaptorDiscussion to delete.</param>
        /// <returns>A success or failure message.</returns>
        private string DeleteRapidReviewDiscussion(IUser user, IRaptorDiscussion raptorDiscussion)
        {
            return Helper.SvcComponents.DeleteRapidReviewDiscussion(user, raptorDiscussion.ItemId, raptorDiscussion.DiscussionId);
        }

        /// <summary>
        /// Deletes the specified RaptorReply.
        /// </summary>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="raptorReply">The RaptorReply to delete.</param>
        /// <returns>A success or failure message.</returns>
        private string DeleteRapidReviewDiscussionReply(IUser user, IRaptorReply raptorReply)
        {
            return Helper.SvcComponents.DeleteRapidReviewDiscussionReply(user, raptorReply.ItemId, raptorReply.ReplyId);
        }

        /// <summary>
        /// Updates the specified RaptorDiscussion with a new comment.
        /// </summary>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="discussionToUpdate">The discussion to update.</param>
        /// <param name="comment">The new comment for the discussion.</param>
        /// <returns>The updated discussion.</returns>
        private IRaptorDiscussion UpdateRapidReviewDiscussion(IUser user, IRaptorDiscussion discussionToUpdate, RaptorComment comment)
        {
            var updatedDiscussion = Helper.SvcComponents.UpdateRapidReviewDiscussion(user, discussionToUpdate.ItemId, discussionToUpdate.DiscussionId, comment);

            return updatedDiscussion;
        }

        /// <summary>
        /// Updates the specified RaptorReply with a new reply comment.
        /// </summary>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="raptorReply">The RaptorReply to update.</param>
        /// <param name="replyComment">The new comment for the RaptorReply.</param>
        /// <returns>The updated RaptorReply.</returns>
        private IRaptorReply UpdateRapidReviewDiscussionReply(IUser user, IRaptorReply raptorReply, string replyComment)
        {
            return Helper.SvcComponents.UpdateRapidReviewDiscussionReply(
                    user, raptorReply.ItemId, raptorReply.DiscussionId, raptorReply.ReplyId, replyComment);
        }

        #endregion Private functions
    }
}