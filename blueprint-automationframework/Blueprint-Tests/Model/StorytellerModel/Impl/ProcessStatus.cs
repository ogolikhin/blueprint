namespace Model.StorytellerModel.Impl
{
    public class ProcessStatus
    {
        public ProcessStatus (
            bool isLocked, 
            bool isLockedByMe, 
            bool isDeleted, 
            bool isReadOnly, 
            bool isUnpublished, 
            bool hasEverBeenPublished)
        {
            IsLocked = isLocked;
            IsLockedByMe = isLockedByMe;
            IsDeleted = isDeleted;
            IsReadOnly = isReadOnly;
            IsUnpublished = isUnpublished;
            HasEverBeenPublished = hasEverBeenPublished;
        }

        /// <summary>
        /// Check if the process is locked
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Check if the process is locked by the user who retrieve the process model
        /// </summary>
        public bool IsLockedByMe { get; set; }

        /// <summary>
        /// Check if the process is deleted by the user who retrieve the process model
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Check if the process is read-only
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Check if the process has saved changes which are not published yet
        /// </summary>
        public bool IsUnpublished { get; set; }

        /// <summary>
        /// Check if the process is ever been published
        /// </summary>
        public bool HasEverBeenPublished { get; set; }

        //TODO: process model contains the property but not clear if it is being used to verify the status of the process yet
        public bool HasReadOnlyReuse { get; set; }

        //TODO: process model contains the property but not clear if it is being used to verify the status of the process yet
        public bool HasReuse { get; set; }

        //TODO: process model contains the property but not clear if it is being used to verify the status of the process yet
        public int LockOwnerId { get; set; }

        //TODO: process model contains the property but not clear if it is being used to verify the status of the process yet
        public int RevisionId { get; set; }

        //TODO: process model contains the property but not clear if it is being used to verify the status of the process yet
        public int UserId { get; set; }

        //TODO: process model contains the property but not clear if it is being used to verify the status of the process yet
        public int VersionId { get; set; } 
    }
}