using System;

namespace Model.ArtifactModel.Adapters
{
    public interface ICommentBaseAdapter
    {
        #region Serialized JSON properties

        int ItemId { get; set; }
        int DiscussionId { get; set; }
        int Version { get; set; }
        int UserId { get; set; }
        DateTime LastEditedOn { get; set; }
        string Username { get; set; }
        bool IsGuest { get; set; }
        string Comment { get; set; }
        bool CanEdit { get; set; }
        bool CanDelete { get; set; }

        #endregion Serialized JSON properties
    }

    public interface IDiscussionAdapter : ICommentBaseAdapter
    {
        #region Serialized JSON properties

        bool IsClosed { get; }

        #endregion Serialized JSON properties
    }

    public interface IReplyAdapter : ICommentBaseAdapter
    {
        #region Serialized JSON properties

        int ReplyId { get; set; }

        #endregion Serialized JSON properties
    }
}
