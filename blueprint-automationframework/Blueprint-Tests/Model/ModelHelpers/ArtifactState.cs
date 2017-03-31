using Newtonsoft.Json;

namespace Model.ModelHelpers
{
    /// <summary>
    /// This class wraps Blueprint Artifacts and tracks the state of the artifact.
    /// The artifact state is only needed by the automation framework (mostly for the Dispose() method).
    /// </summary>
    public class ArtifactState
    {
        /// <summary>
        /// The project where the artifact exists.
        /// </summary>
        [JsonIgnore]
        public IProject Project { get; set; }

        /// <summary>
        /// The user who created this artifact.
        /// </summary>
        [JsonIgnore]
        public IUser CreatedBy { get; set; }

        /// <summary>
        /// The user who holds the lock on this artifact.  This should be set to null once the lock is released.
        /// </summary>
        [JsonIgnore]
        public IUser LockOwner { get; set; }

        /// <summary>
        /// True if this artifact was deleted and published.
        /// </summary>
        [JsonIgnore]
        public bool IsDeleted { get; set; }

        /// <summary>
        /// True if this artifact has any saved but unpublished changes.
        /// </summary>
        [JsonIgnore]
        public bool IsDraft { get; set; }

        /// <summary>
        /// True if this artifact was locked.
        /// </summary>
        [JsonIgnore]
        public bool IsLocked { get { return (LockOwner == null); } }

        /// <summary>
        /// True if this artifact was deleted (but not published).
        /// </summary>
        [JsonIgnore]
        public bool IsMarkedForDeletion { get; set; }

        /// <summary>
        /// True if this artifact was ever published.
        /// </summary>
        [JsonIgnore]
        public bool IsPublished { get; set; }
    }
}
