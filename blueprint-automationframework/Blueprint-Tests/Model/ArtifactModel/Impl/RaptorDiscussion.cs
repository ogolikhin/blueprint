﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Model.ArtifactModel.Impl;

namespace Model.Impl
{
    public class RaptorDiscussion : IRaptorDiscussion
    {
        #region
        public int ArtifactId { get; set; }

        public int SubArtifactId { get; set; }

        public bool CanCreate { get; set; }

        public bool AreEmailDiscusssionsEnabled { get; set; }

        public List<IRaptorComment> Comments { get; } = new List<IRaptorComment>();
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

        [JsonProperty("comment")]
        public string CommentValue { get; set; }

        public bool CanEdit { get; set; }

        public bool CanDelete { get; set; }

        public RaptorComment()
        { }

        public bool Equals(Comment comment)
        {
            if (comment == null)
            { return false; }
            else
            {
                return (string.Equals(comment.CommentText, CommentValue)) &&
                    (comment.DiscussionId == DiscussionId) && (comment.IsClosed == IsClosed) &&
                    (comment.IsGuest == IsGuest) && (comment.ItemId == ItemId) &&
                    //(DateTime.Equals(comment.LastEditedOn, LastEditedOnUtc)) && //microseconds are different
                    (comment.UserId == UserId) && (string.Equals(comment.UserName, Username)) &&
                    (comment.Version == Version);
            }
        }
    }

    public class RaptorReply : IRaptorReply
    {
        public int ReplyId { get; set; }

        public int ItemId { get; set; }

        public int DiscussionId { get; set; }

        public int Version { get; set; }

        public int UserId { get; set; }

        public DateTime LastEditedOnUtc { get; set; }

        public string UserName { get; set; }

        public bool IsGuest { get; set; }

        [JsonProperty("comment")]
        public string ReplyText { get; set; }

        public bool CanEdit { get; set; }

        public bool CanDelete { get; set; }

        public bool Equals(Reply reply)
        {
            if (reply == null)
            { return false; }
            else
            {
                if ((string.Equals(reply.ReplyText, ReplyText)) &&
                    (reply.DiscussionId == DiscussionId) && (reply.IsGuest == IsGuest) &&
                    (reply.ItemId == ItemId) && (reply.ReplyId == ReplyId) &&
                    //(DateTime.Equals(comment.LastEditedOn, LastEditedOnUtc)) && //microseconds are different
                    (reply.UserId == UserId) && (string.Equals(reply.UserName, UserName)))
                { return true; }
                else
                { return false; }
            }
        }
    }
}
