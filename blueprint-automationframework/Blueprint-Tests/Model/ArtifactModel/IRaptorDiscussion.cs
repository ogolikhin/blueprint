using System;
using System.Collections.Generic;
using Model.ArtifactModel.Adapters;

namespace Model.ArtifactModel
{
    //This representation is used for Discussion in RapidReview,
    //Impact Analysis and Storyteller
    // Found in:  blueprint-current/Source/BluePrintSys.RC.Business.Internal/Components/RapidReview/Models/DiscussionsInfo.cs
    public interface IRaptorDiscussionsInfo
    {
        #region Serialized JSON properties

        int ArtifactId { get; }
        int? SubArtifactId { get; }
        bool CanCreate { get; }
        bool AreEmailDiscusssionsEnabled { get; }
        List<IRaptorDiscussion> Discussions { get; }

        #endregion Serialized JSON properties
    }

    //This representation is used for Discussion in RapidReview,
    //Impact Analysis and Storyteller
    // Found in:  blueprint-current/Source/BluePrintSys.RC.Business.Internal/Components/RapidReview/Models/DiscussionsInfo.cs
    public interface IRaptorDiscussion : IDiscussionAdapter
    {
        #region Serialized JSON properties

        string Username { get; set; }
        List<IReplyAdapter> Replies { get; set; }

        #endregion Serialized JSON properties
    }

    //This representation is used for Discussion in RapidReview,
    //Impact Analysis and Storyteller
    // Found in:  blueprint-current/Source/BluePrintSys.RC.Business.Internal/Components/RapidReview/Models/DiscussionsInfo.cs
    public interface IRaptorReply : IReplyAdapter
    {
        #region Serialized JSON properties

        string Username { get; set; }

        #endregion Serialized JSON properties
    }
}
