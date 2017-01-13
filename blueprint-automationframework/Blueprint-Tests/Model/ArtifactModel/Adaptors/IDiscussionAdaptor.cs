using System;

namespace Model.ArtifactModel.Adaptors
{
    public interface IDiscussionAdaptor
    {
        bool CanDelete { get; }
        bool CanEdit { get; }
        string Comment { get; }
        int DiscussionId { get; }
        bool IsClosed { get; }
        bool IsGuest { get; }
        int ItemId { get; }
        DateTime LastEditedOn { get; }
        int UserId { get; }
        string Username { get; }
        int Version { get; }
    }
}
