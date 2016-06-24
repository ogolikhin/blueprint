using System;
using System.Collections.Generic;

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
    }
}
