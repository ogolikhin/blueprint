using Model.ArtifactModel.Adaptors;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Model.ArtifactModel.Impl
{
    // This representation is used for Discussion in ArtifactStore (NOVA)
    // See:  blueprint/svc/ArtifactStore/Models/DiscussionResultSet.cs
    public class DiscussionResultSet
    {
        #region Serialized JSON properties

        public bool CanCreate { get; set; }
        public bool CanDelete { get; set; }
        public bool EmailDiscussionsEnabled { get; set; }
        public List<Discussion> Discussions { get; } = new List<Discussion>();

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public List<ThreadStatus> ThreadStatuses { get; set; }

        #endregion Serialized JSON properties

        public DiscussionResultSet()
        { }
    }

    // See:  blueprint/svc/ArtifactStore/Models/DiscussionResultSet.cs
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

    // See:  blueprint/svc/ArtifactStore/Models/DiscussionResultSet.cs
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

    // See:  blueprint/svc/ArtifactStore/Models/DiscussionResultSet.cs
    public class ThreadStatus
    {
        public int StatusId { get; set; }
        public bool? ReadOnly { get; set; }
        public bool IsClosed { get; set; }
        public string Name { get; set; }
    }
}
