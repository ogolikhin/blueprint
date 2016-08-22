using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Model.ArtifactModel.Impl
{
    //This representation is used for Discussion in ArtifactStore (NOVA)
    //TODO: refactor to extract common entities for NOVA, RR/IA/Storyteller and OpenApi
    public class Discussions
    {
        public bool CanCreate { get; set; }

        public bool CanDelete { get; set; }

        public bool EmailDiscussionsEnabled { get; set; }

        [JsonProperty("discussions")]
        public List<Comment> Comments { get; } = new List<Comment>();

        public Discussions()
        { }
    }

    public class Comment
    {
        public bool IsClosed { get; set; }
        //Open, Closed but can also include custom status

        public string Status { get; set; }

        public int RepliesCount { get; set; }

        public int ItemId { get; set; }

        public int DiscussionId { get; set; }

        public int Version { get; set; }

        public int UserId { get; set; }

        public DateTime LastEditedOn { get; set; }

        public string UserName { get; set; }

        public bool IsGuest { get; set; }

        [JsonProperty("comment")]
        public string CommentText { get; set; }

        public bool CanEdit { get; set; }

        public bool CanDelete { get; set; }

        public Comment()
        { }
    }

    public class Reply
    {
        public int ReplyId { get; set; }

        public int ItemId { get; set; }

        public int DiscussionId { get; set; }

        public int Version { get; set; }

        public int UserId { get; set; }

        public DateTime LastEditedOn { get; set; }

        public string UserName { get; set; }

        public bool IsGuest { get; set; }

        [JsonProperty("comment")]
        public string ReplyText { get; set; }

        public bool CanEdit { get; set; }

        public bool CanDelete { get; set; }
    }
}
