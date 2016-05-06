using System;
using System.Collections.Generic;
using Model.ArtifactModel;

namespace Model
{
    public interface IDiscussion
    {
        #region
        int ArtifactId { get; }

        int SubArtifactId { get; }

        bool CanCreate { get; }

        bool AreEmailDiscusssionsEnabled { get; }
        
        List<IComment> Comments { get; }
        #endregion
    }

    public interface IComment
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
