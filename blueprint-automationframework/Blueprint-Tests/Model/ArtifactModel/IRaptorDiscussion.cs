using System;
using System.Collections.Generic;
using Model.ArtifactModel.Impl;

namespace Model
{
    //This representation is used for Discussion in RapidReview,
    //Impact Analysis and Storyteller
    public interface IRaptorDiscussion
    {
        #region
        int ArtifactId { get; }

        int SubArtifactId { get; }

        bool CanCreate { get; }

        bool AreEmailDiscusssionsEnabled { get; }
        
        List<IRaptorComment> Comments { get; }
        #endregion
    }

    //This representation is used for Discussion in RapidReview,
    //Impact Analysis and Storyteller
    public interface IRaptorComment
    {
        bool IsClosed { get; }

        int ItemId { get; }

        int DiscussionId { get; }
        
        int Version { get; }

        int UserId { get; }

        DateTime LastEditedOnUtc { get; }

        string Username { get; }

        bool IsGuest { get; }

        string CommentValue { get; }

        bool CanEdit { get; }

        bool CanDelete { get; }

        bool Equals(Comment comment);
    }

    //This representation is used for Discussion in RapidReview,
    //Impact Analysis and Storyteller
    public interface IRaptorReply
    {
        int ReplyId { get; set; }

        int ItemId { get; set; }

        int DiscussionId { get; set; }

        int Version { get; set; }

        int UserId { get; set; }

        DateTime LastEditedOnUtc { get; set; }

        string UserName { get; set; }

        bool IsGuest { get; set; }

        string ReplyText { get; set; }

        bool CanEdit { get; set; }

        bool CanDelete { get; set; }

        bool Equals(Reply reply);
    }
}
