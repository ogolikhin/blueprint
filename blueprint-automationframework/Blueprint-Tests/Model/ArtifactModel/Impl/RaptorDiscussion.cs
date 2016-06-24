using System;
using System.Collections.Generic;

namespace Model.Impl
{
    public class RaptorDiscussion : IRaptorDiscussion
    {
        #region
        public int ArtifactId { get; set; }

        public int SubArtifactId { get; set; }

        public bool CanCreate { get; set; }

        public bool AreEmailDiscusssionsEnabled { get; set; }

        public List<IRaptorComment> Comments { get; }
        #endregion
        public RaptorDiscussion()
        { }
    }

    public class RaptorComment : IRaptorComment
    {
        public bool IsClosed { get; set; }

        public int ItemId { get; set; }

        public int DiscussionId { get; set; }

        public int Version { get; set; }

        public int UserId { get; set; }

        public DateTime LastEditedOnUtc { get; set; }

        public string Username { get; set; }

        public bool IsGuest { get; set; }

        public string CommentValue { get; set; }

        public bool CanEdit { get; set; }

        public bool CanDelete { get; set; }
        public RaptorComment()
        { }
    }
}
