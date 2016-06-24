using System;
using System.Collections.Generic;

namespace Model.ArtifactModel.Impl
{
    //This representation is used for Discussion in ArtifactStore (NOVA)
    //TODO: refactor to extract common entities for NOVA, RR/IA/Storyteller and OpenApi
    public class Discussions
    {
        public bool CanCreate { get; set; }
        public bool CanDelete { get; set; }
        public List<Comment> Comments { get; } = new List<Comment>();
    }

    public class Comment
    {
        public bool IsClosed { get; set; }
        //Open, Closed but can also include custom status
        public string Status { get; set; }
        public int ItemId { get; set; }
        public int DiscussionId { get; set; }
        public int UserId { get; set; }
        public string LastEditedOn { get; set; }
        public string UserName { get; set; }
        public bool IsGuest { get; set; }
        public string CommentText { get; set; }
    }
}
