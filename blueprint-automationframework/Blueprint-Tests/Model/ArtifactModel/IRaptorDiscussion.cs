using System;
using System.Collections.Generic;
using Model.ArtifactModel.Adaptors;
using Model.ArtifactModel.Impl;

namespace Model
{
    //This representation is used for Discussion in RapidReview,
    //Impact Analysis and Storyteller
    // Found in:  blueprint-current/Source/BluePrintSys.RC.Business.Internal/Components/RapidReview/Models/DiscussionsInfo.cs
    public interface IRaptorDiscussionsInfo
    {
        #region Serialized JSON properties

        int ArtifactId { get; }
        int SubArtifactId { get; }
        bool CanCreate { get; }
        bool AreEmailDiscusssionsEnabled { get; }
        List<IRaptorDiscussion> Discussions { get; }

        #endregion Serialized JSON properties
    }

    //This representation is used for Discussion in RapidReview,
    //Impact Analysis and Storyteller
    // Found in:  blueprint-current/Source/BluePrintSys.RC.Business.Internal/Components/RapidReview/Models/DiscussionsInfo.cs
    public interface IRaptorDiscussion : IDiscussionAdaptor
    {
        List<IRaptorReply> Replies { get; set; }

        bool Equals(Discussion comment);
    }

    //This representation is used for Discussion in RapidReview,
    //Impact Analysis and Storyteller
    // Found in:  blueprint-current/Source/BluePrintSys.RC.Business.Internal/Components/RapidReview/Models/DiscussionsInfo.cs
    public interface IRaptorReply
    {
        #region Serialized JSON properties

        int ReplyId { get; set; }
        int ItemId { get; set; }
        int DiscussionId { get; set; }
        int Version { get; set; }
        int UserId { get; set; }
        DateTime LastEditedOnUtc { get; set; }
        string UserName { get; set; }
        bool IsGuest { get; set; }
        string Comment { get; set; }
        bool CanEdit { get; set; }
        bool CanDelete { get; set; }
        bool Equals(Reply reply);

        #endregion Serialized JSON properties
    }
}
