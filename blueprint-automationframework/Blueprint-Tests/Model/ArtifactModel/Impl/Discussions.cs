using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using Model.ArtifactModel.Adaptors;

namespace Model.ArtifactModel.Impl
{
    //This representation is used for Discussion in ArtifactStore (NOVA)
    public class DiscussionResultSet
    {
        #region Serialized JSON properties

        public bool CanCreate { get; set; }
        public bool CanDelete { get; set; }
        public bool EmailDiscussionsEnabled { get; set; }
        public List<Discussion> Discussions { get; } = new List<Discussion>();

        #endregion Serialized JSON properties

        public DiscussionResultSet()
        { }
    }

    public class Discussion : IDiscussionAdaptor
    {
        #region Serialized JSON properties

        public bool IsClosed { get; set; }  //Open, Closed but can also include custom status
        public string Status { get; set; }
        public int RepliesCount { get; set; }
        public int ItemId { get; set; }
        public int DiscussionId { get; set; }
        public int Version { get; set; }
        public int UserId { get; set; }
        public DateTime LastEditedOn { get; set; }
        public string Username { get; set; }
        public bool IsGuest { get; set; }
        public string Comment { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }

        #endregion Serialized JSON properties

        public Discussion()
        { }
    }

    public class Reply : IReplyAdapter
    {
        public int ReplyId { get; set; }
        public int ItemId { get; set; }
        public int DiscussionId { get; set; }
        public int Version { get; set; }
        public int UserId { get; set; }
        public DateTime LastEditedOn { get; set; }
        public string Username { get; set; }
        public bool IsGuest { get; set; }
        public string Comment { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}
