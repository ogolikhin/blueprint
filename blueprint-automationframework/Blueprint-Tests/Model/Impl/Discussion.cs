using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Impl
{
    public class Discussion : IDiscussion
    {
        #region
        public int ArtifactId { get; set; }

        public int SubArtifactId { get; set; }

        public bool CanCreate { get; set; }

        public bool AreEmailDiscusssionsEnabled { get; set; }

        public List<IComment> Comments { get; }
        #endregion
        public Discussion()
        { }
    }

    public class Comment : IComment
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
        public Comment()
        { }
    }
}
