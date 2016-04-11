using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Impl
{
    public class Discussion : IDiscussion
    {
        #region
        public int ArtifactId { get; }

        public int SubArtifactId { get; }

        public bool CanCreate { get; }

        public bool AreEmailDiscusssionsEnabled { get; }

        public List<IComment> Comments { get; }
        #endregion
        public Discussion()
        { }
    }

    public class Comment : IComment
    {
        public bool IsClosed { get; }

        public int ItemId { get; }

        public int DiscussionId { get; }

        public int Version { get; }

        public int UserId { get; }

        public DateTime LastEditedOnUtc { get; }

        public string Username { get; }

        public bool IsGuest { get; }

        public string CommentValue { get; }

        public bool CanEdit { get; }

        public bool CanDelete { get; }
        public Comment()
        { }
    }
}
