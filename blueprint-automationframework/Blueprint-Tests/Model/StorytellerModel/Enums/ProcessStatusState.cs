namespace Model.StorytellerModel.Enums
{
    /// <summary>
    /// Enumeration of Process Status State
    /// </summary>
    public enum ProcessStatusState
    {
        // isLocked: true, isLockedByMe: true, isDeleted: false,
        // isReadOnly: false, isUnpublished: true,
        // hasEverBeenPublished: false
        NeverPublishedAndUpdated,
        // isLocked: false, isLockedByMe: false, isDeleted: false,
        // isReadOnly: false, isUnpublished: false,
        // hasEverBeenPublished: true
        PublishedAndNotLocked,
        // isLocked: true, isLockedByMe: false, isDeleted: false,
        // isReadOnly: true, isUnpublished: false,
        // hasEverBeenPublished: true
        PublishedAndLockedByAnotherUser,
        // isLocked: true, isLockedByMe: true, isDeleted: false,
        // isReadOnly: false, isUnpublished: true,
        // hasEverBeenPublished: true
        PublishedAndLockedByMe
    };
}