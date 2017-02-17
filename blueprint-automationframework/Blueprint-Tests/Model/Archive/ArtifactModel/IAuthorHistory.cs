using System;

namespace Model.Archive.ArtifactModel
{
    public interface IAuthorHistory
    {
        IUser    CreatedBy { get; }
        DateTime CreatedOn { get; }
        IUser    LastEditedBy { get; }
        DateTime LastEditedOn { get; }
    }
}
