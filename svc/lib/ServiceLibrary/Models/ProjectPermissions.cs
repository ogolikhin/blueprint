using System;

namespace ServiceLibrary.Models
{
    [Flags]
    public enum ProjectPermissions
    {
        None = 0x0,
        CommentsModificationDisabled = 1,
        CommentsDeletionDisabled = 2,
        IsReviewESignatureEnabled = 4,
        AreEmailNotificationsEnabled = 8,
        AreEmailRepliesEnabled = 16,
        IsMeaningOfSignatureEnabled = 32
    }
}
