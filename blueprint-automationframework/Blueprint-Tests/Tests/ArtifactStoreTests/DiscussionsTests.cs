using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using System.Collections.Generic;
using Model.ArtifactModel.Adaptors;
using TestCommon;
using Utilities;
using Newtonsoft.Json;
using Common;
using Utilities.Facades;
using Model.Common.Constants;

namespace ArtifactStoreTests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]    // Ignore for now.
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class DiscussionsTests : TestBase
    {
        private const string UPDATED_TEXT = "Updated text";
        private const string ORIGINAL_COMMENT = "Original comment";

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

            var postedRaptorDiscussion = artifact.PostRaptorDiscussion("draft", _authorUser);
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
        }

        [TestCase]
        [TestRail(146055)]
        [Description("Add comment to published artifact, delete artifact (don't publish), get discussion for this artifact.  Verify returned discussions.")]
        public void GetDiscussions_MarkedForDeleteArtifact_ValidateReturnedDiscussion()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.Actor);
            var postedRaptorComment = artifact.PostRaptorDiscussion("draft", _adminUser);
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
        }

        [TestCase]
        [TestRail(146056)]
        [Description("Add comment to published subartifact, get discussion for this subartifact, check that it has expected values.")]
        public void GetDiscussions_PublishedSubArtifact_ReturnsCorrectDiscussion()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.Process);
            var postedRaptorComment = AddCommentToSubArtifactOfStorytellerProcess(artifact);

            DiscussionResultSet discussions = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                discussions = Helper.ArtifactStore.GetArtifactDiscussions(postedRaptorComment.ItemId, _adminUser);
            }, "GetArtifactDiscussions shouldn't throw any error.");

            // Verify:
            Assert.AreEqual(1, discussions.Discussions.Count, "Artifact should have 1 comment, but it has {0}", discussions.Discussions.Count);
            RaptorDiscussion.AssertAreEqual(postedRaptorComment, discussions.Discussions[0]);
            Assert.IsFalse(discussions.Discussions[0].IsClosed, "IsClosed flag should be set to false!");
        }

        [TestCase]
        [TestRail(146060)]
        [Description("Add comment & a reply to subartifact of published artifact, get discussion for this subartifact.  Verify it returns the reply that we added.")]
        public void GetReplies_PublishedSubArtifactWithDiscussionAndReply_ReturnsCorrectDiscussion()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.Process);
            var postedRaptorComment = AddCommentToSubArtifactOfStorytellerProcess(artifact);

            DiscussionResultSet discussions = Helper.ArtifactStore.GetArtifactDiscussions(postedRaptorComment.ItemId, _adminUser);
            var postedReply = Artifact.PostRaptorDiscussionReply(Helper.BlueprintServer.Address,
                postedRaptorComment, "This is a reply to a comment.", _authorUser);

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
        }

        [TestCase]
        [TestRail(146063)]
        [Description("Add comment to subartifact of published artifact, delete artifact (don't publish), get discussion for this subartifact.  Verify it returns expected discussion.")]
        public void GetDiscussions_MarkedForDeleteSubArtifact_ValidateReturnedDiscussion()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.Process);
            var postedRaptorComment = AddCommentToSubArtifactOfStorytellerProcess(artifact);

            artifact.Delete(_adminUser);

            // Execute:
            DiscussionResultSet discussions = null;
            Assert.DoesNotThrow(() =>
            {
                discussions = Helper.ArtifactStore.GetArtifactDiscussions(postedRaptorComment.ItemId, _adminUser);
            }, "GetArtifactDiscussions shouldn't throw any exception, but it does.");

            // Verify:
            Assert.AreEqual(1, discussions.Discussions.Count, "Artifact should have 1 comment, but it has {0}", discussions.Discussions.Count);
            RaptorDiscussion.AssertAreEqual(postedRaptorComment, discussions.Discussions[0]);
            Assert.IsFalse(discussions.Discussions[0].IsClosed, "IsClosed flag should be set to false!");
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
        }

        [TestCase]
        [TestRail(155659)]
        [Description("Update discussion for the published artifact. Verify that text & IsClosed flag were updated.")]
        public void UpdateDiscussion_PublishedArtifact_ReturnsUpdatedDiscussion()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UseCase);

            var postedRaptorComment = artifact.PostRaptorDiscussion("original discussion text", _authorUser);
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
            IDiscussionAdaptor updatedDiscussion = null;
            Assert.DoesNotThrow(() =>
            {
                updatedDiscussion = artifact.UpdateRaptorDiscussion(comment, _authorUser, postedRaptorComment);
            }, "UpdateDiscussions shouldn't throw any error.");

            // Verify:
            discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _authorUser);
            Assert.AreEqual(1, discussions.Discussions.Count, "Artifact should have 1 comment, but it has {0}", discussions.Discussions.Count);
            Assert.AreEqual(StringUtilities.WrapInDiv(comment.Comment), updatedDiscussion.Comment, "Updated comment must have proper text.");
            Assert.IsTrue(discussions.Discussions[0].IsClosed, "IsClosed flag should be set to true!");
            RaptorDiscussion.AssertAreEqual(updatedDiscussion, discussions.Discussions[0], skipCanEdit: true);
        }

        [TestCase]
        [TestRail(155675)]
        [Description("Update discussion of published subartifact, get discussion for this subartifact, check that it has expected values.")]
        public void UpdateDiscussion_PublishedSubArtifact_ReturnsUpdatedDiscussion()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.Process);
            var postedRaptorComment = AddCommentToSubArtifactOfStorytellerProcess(artifact, _authorUser);

            var process = Helper.Storyteller.GetProcess(_authorUser, artifact.Id);
            var userTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            var discussions = Helper.ArtifactStore.GetArtifactDiscussions(userTask.Id, _authorUser);
            var statusId = GetStatusId(discussions, ThreadStatus.CLOSED);
            var comment = new RaptorComment()
            {
                Comment = UPDATED_TEXT,
                StatusId = statusId
            };

            // Execute:
            IDiscussionAdaptor updatedDiscussion = null;
            Assert.DoesNotThrow(() =>
            {
                updatedDiscussion = OpenApiArtifact.UpdateRaptorDiscussion(Helper.BlueprintServer.Address,
                    userTask.Id, postedRaptorComment, comment, _authorUser);
            }, "UpdateDiscussions shouldn't throw any error.");

            // Verify:
            discussions = Helper.ArtifactStore.GetArtifactDiscussions(userTask.Id, _authorUser);
            Assert.AreEqual(1, discussions.Discussions.Count, "Artifact should have 1 comment, but it has {0}", discussions.Discussions.Count);
            Assert.AreEqual(StringUtilities.WrapInDiv(comment.Comment), updatedDiscussion.Comment, "Updated comment must have updated value, but it didn't.");
            Assert.IsTrue(discussions.Discussions[0].IsClosed, "IsClosed flag should be set to true!");
            RaptorDiscussion.AssertAreEqual(updatedDiscussion, discussions.Discussions[0], skipCanEdit: true);
        }

        [TestCase]
        [TestRail(156531)]
        [Description("Delete reply created by the current user, check that reply was deleted.")]
        public void DeleteReply_DeleteByReplyAuthor_SuccessfullyDeleted()
        {
            // Setup:
            const string REPLY = "Reply";
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UIMockup);
            var raptorComment = artifact.PostRaptorDiscussion(ORIGINAL_COMMENT, _adminUser);
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
                OpenApiArtifact.DeleteRaptorReply(Helper.BlueprintServer.Address, artifact.Id, raptorReply, _authorUser);
            }, "DeleteReply shouldn't throw any error, but it did.");

            // Verify:
            var discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _adminUser);
            Assert.AreEqual(1, discussions.Discussions.Count, "Artifact should have 1 comment, but it has {0} comments", discussions.Discussions.Count);
            Assert.AreEqual(1, discussions.Discussions[0].RepliesCount, "Discussion should have 1 reply, but it has {0} replies",
                discussions.Discussions[0].RepliesCount);
        }

        [TestCase]
        [TestRail(156532)]
        [Description("Update discussion created by the current user, check that comment was updated.")]
        public void UpdateOwnComment_NonAdminUser_SuccessfullyUpdated()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UseCase);
            var raptorComment = artifact.PostRaptorDiscussion(ORIGINAL_COMMENT, _authorUser);
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
            IDiscussionAdaptor updatedDiscussion = null;
            Assert.DoesNotThrow(() =>
            {
                updatedDiscussion = artifact.UpdateRaptorDiscussion(comment, _authorUser, raptorComment);
            }, "UpdateDiscussion shouldn't throw any error, but it did.");

            // Verify:
            discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _authorUser);
            Assert.AreEqual(1, discussions.Discussions.Count, "Artifact should have 1 comment, but it has {0}", discussions.Discussions.Count);
            Assert.AreEqual(StringUtilities.WrapInDiv(comment.Comment), updatedDiscussion.Comment, "Updated comment must have proper text.");
            Assert.IsTrue(discussions.Discussions[0].IsClosed, "IsClosed flag should be set to true!");
            RaptorDiscussion.AssertAreEqual(updatedDiscussion, discussions.Discussions[0], skipCanEdit: true);
        }

        [TestCase]
        [TestRail(266910)]
        [Description("Close discussion created by user, reopen discussion. Check that comment was updated and discussion reopened.")]
        public void UpdateComment_CloseAndReopenDiscussion_SuccessfullyUpdated()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UseCase);
            var raptorComment = artifact.PostRaptorDiscussion(ORIGINAL_COMMENT, _authorUser);
            Assert.AreEqual(StringUtilities.WrapInDiv(ORIGINAL_COMMENT), raptorComment.Comment, 
                "Original comment and comment returned after discussion created different!");

            var discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _authorUser);
            var statusId = GetStatusId(discussions, ThreadStatus.CLOSED);

            var comment = new RaptorComment()
            {
                StatusId = statusId
            };

            IDiscussionAdaptor updatedDiscussion = null;
            Assert.DoesNotThrow(() =>
            {
                updatedDiscussion = artifact.UpdateRaptorDiscussion(comment, _authorUser, raptorComment);
            }, "UpdateDiscussion shouldn't throw any error, but it did.");

            Assert.IsTrue(updatedDiscussion.IsClosed, "The discussion should be closed at this point, but it is open!");

            statusId = GetStatusId(discussions, ThreadStatus.OPEN);
            comment.Comment = "Reopened text";
            comment.StatusId = statusId;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                updatedDiscussion = artifact.UpdateRaptorDiscussion(comment, _authorUser, raptorComment);
            }, "UpdateDiscussion shouldn't throw any error, but it did.");

            // Verify:
            discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _authorUser);
            Assert.AreEqual(1, discussions.Discussions.Count, "Artifact should have 1 comment, but it has {0}", discussions.Discussions.Count);
            Assert.AreEqual(StringUtilities.WrapInDiv(comment.Comment), updatedDiscussion.Comment, "Updated comment must have proper text.");
            Assert.IsFalse(discussions.Discussions[0].IsClosed, "IsClosed flag should be set to false!");
            RaptorDiscussion.AssertAreEqual(updatedDiscussion, discussions.Discussions[0]);
        }

        [Category(Categories.CustomData)]
        [Category(Categories.GoldenData)]
        [TestCase("Custom Status Open", false)]
        [TestCase("Custom Status Close", true)]
        [TestRail(266915)]
        [Description("Update discussion created by user to custom discussion status. Check that comment was updated and discussion status applied.")]
        public void UpdateComment_CustomDiscussionStatuses_SuccessfullyUpdated(string customStatus, bool isClosed)
        {
            // Setup:
            var customDataProject = ArtifactStoreHelper.GetCustomDataProject(_adminUser);
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, customDataProject);

            var artifact = Helper.CreateAndPublishArtifact(customDataProject, author, BaseArtifactType.UseCase);
            var raptorComment = artifact.PostRaptorDiscussion(ORIGINAL_COMMENT, author);
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
            IDiscussionAdaptor updatedDiscussion = null;
            Assert.DoesNotThrow(() =>
            {
                updatedDiscussion = artifact.UpdateRaptorDiscussion(comment, author, raptorComment);
            }, "UpdateDiscussion shouldn't throw any error, but it did.");

            // Verify:
            discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, author);
            Assert.AreEqual(1, discussions.Discussions.Count, "Artifact should have 1 comment, but it has {0}", discussions.Discussions.Count);
            Assert.AreEqual(StringUtilities.WrapInDiv(comment.Comment), updatedDiscussion.Comment, "Updated comment must have proper text.");
            Assert.AreEqual(isClosed, discussions.Discussions[0].IsClosed, "IsClosed flag should be set to {0}!", isClosed);
            RaptorDiscussion.AssertAreEqual(updatedDiscussion, discussions.Discussions[0], skipCanEdit: isClosed);
        }

        [TestCase(int.MaxValue)]
        [TestRail(266927)]
        [Description("Update discussion created by user to custom discussion status. Check that comment was updated and discussion status applied.")]
        public void UpdateComment_NonExistingStatus_SuccessfullyUpdatedOnlyComment(int nonExistingStatusId)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UseCase);
            var raptorComment = artifact.PostRaptorDiscussion(ORIGINAL_COMMENT, _adminUser);
            Assert.AreEqual(StringUtilities.WrapInDiv(ORIGINAL_COMMENT), raptorComment.Comment,
                "Original comment and comment returned after discussion created different!");

            var comment = new RaptorComment()
            {
                Comment = UPDATED_TEXT,
                StatusId = nonExistingStatusId
            };

            // Execute:
            IDiscussionAdaptor updatedDiscussion = null;
            Assert.DoesNotThrow(() =>
            {
                updatedDiscussion = artifact.UpdateRaptorDiscussion(comment, _adminUser, raptorComment);
            }, "UpdateDiscussion shouldn't throw any error, but it did.");

            // Verify:
            var discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _adminUser);
            Assert.AreEqual(1, discussions.Discussions.Count, "Artifact should have 1 comment, but it has {0}", discussions.Discussions.Count);
            Assert.AreEqual(StringUtilities.WrapInDiv(comment.Comment), updatedDiscussion.Comment, "Updated comment must have proper text.");
            RaptorDiscussion.AssertAreEqual(updatedDiscussion, discussions.Discussions[0]);
        }

        [TestCase]
        [TestRail(156534)]
        [Description("Delete own comment, check that comment was deleted.")]
        public void DeleteOwnComment_UserHasRightsToDelete_SuccessfullyDeleted()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UIMockup);
            IRaptorDiscussion raptorComment = null;

            for (int i = 0; i < 2; i++)
            {
                raptorComment = artifact.PostRaptorDiscussion(ORIGINAL_COMMENT, _authorUser);
            }
            Assert.AreEqual(StringUtilities.WrapInDiv(ORIGINAL_COMMENT), raptorComment.Comment,
                "Original comment and comment returned after discussion created different!");

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                artifact.DeleteRaptorDiscussion(_authorUser, raptorComment);
            }, "DeleteDiscussions shouldn't throw any error, but it does.");

            // Verify:
            var discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _adminUser);
            Assert.AreEqual(1, discussions.Discussions.Count, "Artifact should have 1 comment, but it has {0} comments", discussions.Discussions.Count);
        }

        [TestCase]
        [TestRail(156535)]
        [Description("Admin deletes other user's comment, check that comment was deleted.")]
        public void DeleteOtherUserComment_Admin_SuccessfullyDeleted()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UIMockup);
            IRaptorDiscussion raptorComment = null;

            for (int i = 0; i < 2; i++)
            {
                raptorComment = artifact.PostRaptorDiscussion(ORIGINAL_COMMENT, _authorUser);
            }
            Assert.AreEqual(StringUtilities.WrapInDiv(ORIGINAL_COMMENT), raptorComment.Comment,
                "Original comment and comment returned after discussion created different!");

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                artifact.DeleteRaptorDiscussion(_adminUser, raptorComment);
            }, "DeleteDiscussions shouldn't throw any error, but it does.");

            // Verify:
            var discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _authorUser);
            Assert.AreEqual(1, discussions.Discussions.Count, "Artifact should have 1 comment, but it has {0} comments", discussions.Discussions.Count);
        }

        [TestCase]
        [TestRail(156536)]
        [Description("User updates its own reply, check that reply was updated.")]
        public void UpdateOwnReply_NonAdminUser_SuccessfullyUpdated()
        {
            // Setup:
            const string REPLY = "Reply";

            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.Glossary);
            var raptorComment = artifact.PostRaptorDiscussion(ORIGINAL_COMMENT, _adminUser);
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
                updatedReply = OpenApiArtifact.UpdateRaptorDiscussionReply(Helper.BlueprintServer.Address,
                    artifact.Id, raptorComment, raptorReply, newReplyText, _authorUser);
            }, "UpdateReply shouldn't throw any error, but it does.");

            // Verify:
            var discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _adminUser);
            Assert.AreEqual(1, discussions.Discussions.Count, "Artifact should have 1 comment, but it has {0} comments", discussions.Discussions.Count);
            Assert.AreEqual(1, discussions.Discussions[0].RepliesCount, "Discussion should have 1 reply, but it has {0} replies",
                discussions.Discussions[0].RepliesCount);
            Assert.AreEqual(StringUtilities.WrapInDiv(newReplyText), updatedReply.Comment,
                "Original comment and comment returned after discussion created different!");
        }

        [TestCase]
        [TestRail(156537)]
        [Description("Post comment for artifact with version 2, check that comment has version 2.")]
        public void PostNewComment_ArtifactHasVersion2_CheckCommentHasVersion2()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishOpenApiArtifact(_project, _adminUser, BaseArtifactType.UseCase, numberOfVersions: 2); //artifact version is 2

            IDiscussionAdaptor postedRaptorComment = null;
            DiscussionResultSet discussions = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                postedRaptorComment = Artifact.PostRaptorDiscussion(artifact.Address, artifact.Id, ORIGINAL_COMMENT, _authorUser);
                artifact.Save(_authorUser);
                artifact.Publish(_authorUser);  //artifact version is 3
                discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _adminUser);
            }, "UpdateDiscussions and GetDiscussion shouldn't throw any error.");

            // Verify:
            Assert.AreEqual(1, discussions.Discussions.Count, "Artifact should have 1 comment, but it has {0}", discussions.Discussions.Count);
            Assert.AreEqual(2, discussions.Discussions[0].Version, "Version should be 2, but it is {0}", discussions.Discussions[0].Version);
            Assert.AreEqual(2, postedRaptorComment.Version, "Version should be 2, but it is {0}", postedRaptorComment.Version);
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
            var raptorComment = artifact.PostRaptorDiscussion(ORIGINAL_COMMENT, _adminUser);
            Assert.AreEqual(StringUtilities.WrapInDiv(ORIGINAL_COMMENT), raptorComment.Comment);

            // Execute:
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Artifacts_id_.DISCUSSIONS, artifact.Id);

            var ex = Assert.Throws<Http401UnauthorizedException>(() => CreateRequestFromJson(
                Helper.ArtifactStore.Address, user: null, method: RestRequestMethod.GET, path: path, requestBody: ""),
                "'GET {0}' should return 401 Unauthorized when request is done without token!", path);

            // Verify:
            const string expectedExceptionMessage = "Token is missing or malformed.";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "{0} was not found in returned message of get discussions call which has no token in a header.", expectedExceptionMessage);
        }

        [TestCase]
        [TestRail(266929)]
        [Description("Update discussion with no token in the header. Verify 401 Unauthorized HTTP status was returned")]
        public void UpdateDiscussion_MissingSessionToken_401Unauthorized()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.Glossary);
            var raptorComment = artifact.PostRaptorDiscussion(ORIGINAL_COMMENT, _adminUser);
            Assert.AreEqual(StringUtilities.WrapInDiv(ORIGINAL_COMMENT), raptorComment.Comment);

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

            var validJson = JsonConvert.SerializeObject(comment);

            var ex = Assert.Throws<Http401UnauthorizedException>(() => CreateRequestFromJson(
                Helper.ArtifactStore.Address, user: null, method: RestRequestMethod.PATCH, path: path, requestBody: validJson),
                "'PATCH {0}' should return 401 Unauthorized when request is done without token!", path);

            // Verify:
            const string expectedExceptionMessage = "Unauthorized call";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "{0} was not found in returned message of update discussion call which has no token in a header.", expectedExceptionMessage);
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
            var raptorComment = artifact.PostRaptorDiscussion(ORIGINAL_COMMENT, _adminUser);
            Assert.AreEqual(StringUtilities.WrapInDiv(ORIGINAL_COMMENT), raptorComment.Comment,
                "Original comment and comment returned after discussion created different!");

            // Execute:
            Assert.Throws<Http403ForbiddenException>(() =>
            {
                artifact.DeleteRaptorDiscussion(_authorUser, raptorComment);
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
            var raptorComment = artifact.PostRaptorDiscussion(ORIGINAL_COMMENT, _adminUser);
            Assert.AreEqual(StringUtilities.WrapInDiv(ORIGINAL_COMMENT), raptorComment.Comment,
                "Original comment and comment returned after discussion created different!");

            var raptorReply = OpenApiArtifact.PostRaptorDiscussionReply(Helper.BlueprintServer.Address, raptorComment, REPLY, _adminUser);

            // Execute:
            Assert.Throws<Http403ForbiddenException>(() =>
            {
                OpenApiArtifact.DeleteRaptorReply(Helper.BlueprintServer.Address, artifact.Id, raptorReply, _authorUser);
            }, "DeleteRaptorReply should throw 403 error, but it doesn't.");

            // Verify:
            var discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _authorUser);
            Assert.AreEqual(1, discussions.Discussions.Count, "Artifact should have 1 comment, but it has {0} comments", discussions.Discussions.Count);
            Assert.AreEqual(1, discussions.Discussions[0].RepliesCount, "Discussion should have 1 reply, but it has {0} replies",
                discussions.Discussions[0].RepliesCount);
        }

        [TestCase]
        [TestRail(266958)]
        [Description("Get discussion with user that does not have permissions for this project. Verify 403 Forbidden HTTP status was returned")]
        public void GetDiscussion_InsufficientPermissionsToProject_403Forbidden()
        {
            // Setup:
            var customDataProject = ArtifactStoreHelper.GetCustomDataProject(_adminUser);
            var userWithoutPermissionsToProject = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, customDataProject);

            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.Glossary);
            var raptorComment = artifact.PostRaptorDiscussion(ORIGINAL_COMMENT, _adminUser);
            Assert.AreEqual(StringUtilities.WrapInDiv(ORIGINAL_COMMENT), raptorComment.Comment);

            Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, userWithoutPermissionsToProject),
                "'GET {0}' should return 403 Forbidden when user tries to get comment from project to which he/she does not have permissions!",
                I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Artifacts_id_.DISCUSSIONS, artifact.Id));

            // Verify:
            // TODO: No internal error & message for this case only 403 HTTP Code Bug: https://trello.com/c/XQwBZ2zk
        }

        [TestCase]
        [TestRail(266959)]
        [Description("Update discussion with user that does not have permissions for this project. Verify 403 Forbidden HTTP status was returned")]
        public void UpdatetDiscussion_InsufficientPermissionsToProject_403Forbidden()
        {
            // Setup:
            var customDataProject = ArtifactStoreHelper.GetCustomDataProject(_adminUser);
            var userWithoutPermissionsToProject = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, customDataProject);

            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.Glossary);
            var raptorComment = artifact.PostRaptorDiscussion(ORIGINAL_COMMENT, _adminUser);
            Assert.AreEqual(StringUtilities.WrapInDiv(ORIGINAL_COMMENT), raptorComment.Comment);

            var discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _adminUser);
            var statusId = GetStatusId(discussions, ThreadStatus.CLOSED);
            var comment = new RaptorComment()
            {
                Comment = UPDATED_TEXT,
                StatusId = statusId
            };

            // Execute:
            var path = I18NHelper.FormatInvariant(
                    RestPaths.Svc.Components.RapidReview.Artifacts_id_.Discussions_id_.COMMENT, artifact.Id, raptorComment.DiscussionId);
            var ex = Assert.Throws<Http403ForbiddenException>(() => artifact.UpdateRaptorDiscussion(comment, userWithoutPermissionsToProject, raptorComment),
                "'PATCH {0}' should return 403 Forbidden when user tries to update comment for project to which he/she does not have permissions!", path);

            // Verify:
            const string expectedExceptionMessage = "You do not have permissions to complete this operation";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "{0} was not found in returned message of update discussion call which user has isufficient permissions to project.",
                expectedExceptionMessage);
        }

        [TestCase]
        [TestRail(156533)]
        [Description("Admin tries to update comment created by user - it returns 403, check that comment wasn't updated.")]
        public void UpdateOtherUserComment_AdminUser_403Forbidden()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UseCase);
            var raptorComment = artifact.PostRaptorDiscussion(ORIGINAL_COMMENT, _authorUser);
            Assert.AreEqual(StringUtilities.WrapInDiv(ORIGINAL_COMMENT), raptorComment.Comment,
                "Original comment and comment returned after discussion created different!");

            var comment = new RaptorComment()
            {
                Comment = UPDATED_TEXT
            };

            // Execute:
            Assert.Throws<Http403ForbiddenException>(() =>
            {
                artifact.UpdateRaptorDiscussion(comment, _adminUser, raptorComment);
            }, "UpdateDiscussion should throw 403 error, but it doesn't.");

            // Verify:
            var discussion = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _authorUser);
            Assert.AreEqual(1, discussion.Discussions.Count, "Discussions must have 1 comment, but it has {0}", discussion.Discussions.Count);
            Assert.AreEqual(raptorComment.Comment, discussion.Discussions[0].Comment, "Comment must remain unmodified.");
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

            artifact.PostRaptorDiscussion("draft", _adminUser);
            DiscussionResultSet discussions = null;

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
        [TestRail(146057)]
        [Description("Add comment to subartifact of saved (unpublished) artifact, get discussion for this subartifact.  Verify it returns 404.")]
        public void GetDiscussions_UnpublishedSubArtifact_Returns404()
        {
            // Setup:
            var artifact = Helper.CreateArtifact(_project, _adminUser, BaseArtifactType.Process);
            artifact.Save(_adminUser);
            var postedRaptorComment = AddCommentToSubArtifactOfStorytellerProcess(artifact);

            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDiscussions(postedRaptorComment.ItemId, _adminUser);
            }, "GetArtifactDiscussions should throw 404 error, but it doesn't.");
        }

        [TestCase]
        [TestRail(146059)]
        [Description("Add comment to subartifact of saved (unpublished) artifact, try to get discussion for this subartifact with user other than author.  Verify it returns 404 Not Found.")]
        public void GetDiscussions_UnpublishedSubArtifactOtherUser_404NotFound()
        {
            // Setup:
            var artifact = Helper.CreateArtifact(_project, _adminUser, BaseArtifactType.Process);
            artifact.Save(_adminUser);
            var postedRaptorComment = AddCommentToSubArtifactOfStorytellerProcess(artifact);

            var user2 = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDiscussions(postedRaptorComment.ItemId, user2);
            }, "GetArtifactDiscussions should return 404 error, but it doesn't.");
        }

        [Ignore(IgnoreReasons.UnderDevelopment)]  // Bug: https://trello.com/c/LJKgXQnW
        [TestRail(266962)]
        [Description("Update discussion that does not exist in an artifact. Verify 404 Not Found HTTP status was returned")]
        public void UpdatetDiscussion_NonExistingDiscussion_404NotFound()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.Glossary);
            var raptorComment = artifact.PostRaptorDiscussion(ORIGINAL_COMMENT, _adminUser);

            var discussions = Helper.ArtifactStore.GetArtifactDiscussions(artifact.Id, _adminUser);
            var statusId = GetStatusId(discussions, ThreadStatus.CLOSED);
            var comment = new RaptorComment()
            {
                Comment = UPDATED_TEXT,
                StatusId = statusId
            };

            // Execute:
            raptorComment.DiscussionId = int.MaxValue;
            var path = I18NHelper.FormatInvariant(
                    RestPaths.Svc.Components.RapidReview.Artifacts_id_.Discussions_id_.COMMENT, artifact.Id, raptorComment.DiscussionId);
            var ex = Assert.Throws<Http403ForbiddenException>(() => artifact.UpdateRaptorDiscussion(comment, _adminUser, raptorComment),
                "'PATCH {0}' should return 404 Not Found when user tries to update comment for discussion that does not exist in artifact!", path);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, BusinessLayerErrorCodes.DoesNotExistForRevision,
                "Does Not Exist For Revision. Invalid Discussion ID.");
        }

        #endregion 404 Not Found

        #region Private functions

        /// <summary>
        /// Adds a discussion comment to the specified Storyteller Process artifact.
        /// </summary>
        /// <param name="artifact">The Process artifact to add a comment to.</param>
        /// <param name="user">The user credentials for the operation.</param>
        /// <returns>The IRaptorComment returned after posting the comment.</returns>
        private IRaptorDiscussion AddCommentToSubArtifactOfStorytellerProcess(IArtifact artifact, IUser user = null)
        {
            if (user == null)
            {
                user = _adminUser;
            }
            var process = Helper.Storyteller.GetProcess(user, artifact.Id);
            var userTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            var postedRaptorComment = Artifact.PostRaptorDiscussion(Helper.BlueprintServer.Address, userTask.Id, "text for UT", user);

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
        /// Creates an object with specific json body.  Use this for testing cases where the call is expected to fail.
        /// </summary>
        /// <param name="address">The base address used for the REST call.</param>
        /// <param name="requestBody">The request body (i.e. user to be created).</param>
        /// <param name="user">The user creating another user.</param>
        /// <param name="path">URL for call.</param>
        /// <returns>The call response returned.</returns>
        private static RestResponse CreateRequestFromJson(string address, IUser user, RestRequestMethod method, string path, string requestBody)
        {
            string tokenValue = user?.Token?.AccessControlToken;
            RestApiFacade restApi = new RestApiFacade(address, tokenValue);

            const string contentType = "application/json";

            return restApi.SendRequestBodyAndGetResponse(
                path,
                method,
                requestBody,
                contentType);
        }

        #endregion Private functions
    }
}