using System;
using System.Collections.Generic;

namespace ArtifactStore.Models
{
    public class DiscussionResultSet
    {
        //public int ArtifactId { get; set; }

        // Not used for artifacts
        // public int? SubArtifactId { get; set; }

        public bool CanCreate { get; set; }

        public bool CanDelete { get; set; }

        public bool EmailDiscussionsEnabled { get; set; }

        public IEnumerable<Discussion> Discussions { get; set; }
    }

    public class CommentBase
    {
        public int ItemId { get; set; }

        public int DiscussionId { get; set; }

        public int Version { get; set; }

        public int UserId { get; set; }

        public DateTime LastEditedOn { get; set; }

        public string UserName { get; set; }

        public bool IsGuest { get; set; }

        public string Comment { get; set; }

        public bool CanEdit { get; set; }

        public bool CanDelete { get; set; }
    }

    public class Discussion : CommentBase
    {
        //public IList<Reply> Replies { get; set; }

        public bool IsClosed { get; set; }

        public string Status { get; set; }

        public int RepliesCount { get; set; }
    }

    public class Reply : CommentBase
    {
        public int ReplyId { get; set; }
    }

    public class DiscussionState
    {
        public DateTime LastEditedOn { get; set; }

        public bool IsClosed { get; set; }

        public string Status { get; set; }

        public int DiscussionId { get; set; }
    }

}