using Newtonsoft.Json;

namespace Model.ModelHelpers
{
    public interface IArtifactStateWrapper<T> where T : IHaveAnId
    {
        /// <summary>
        /// This is a convenience property that acts as a proxy for Artifact.Id.
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// The artifact that is being wrapped.
        /// </summary>
        T Artifact { get; set; }

        /// <summary>
        /// The project where the artifact exists.
        /// </summary>
        IProject Project { get; set; }

        /// <summary>
        /// The user who created this artifact.
        /// </summary>
        IUser CreatedBy { get; set; }

        /// <summary>
        /// The user who holds the lock on this artifact.  This should be set to null once the lock is released.
        /// </summary>
        IUser LockOwner { get; set; }

        /// <summary>
        /// True if this artifact was deleted and published.
        /// </summary>
        bool IsDeleted { get; set; }

        /// <summary>
        /// True if this artifact has any saved but unpublished changes.
        /// </summary>
        bool IsDraft { get; set; }

        /// <summary>
        /// True if this artifact was locked.
        /// </summary>
        bool IsLocked { get; }

        /// <summary>
        /// True if this artifact was deleted (but not published).
        /// </summary>
        bool IsMarkedForDeletion { get; set; }

        /// <summary>
        /// True if this artifact was ever published.
        /// </summary>
        bool IsPublished { get; set; }
    }

    /// <summary>
    /// This class wraps Blueprint Artifacts and tracks the state of the artifact.
    /// The artifact state is only needed by the automation framework (mostly for the Dispose() method).
    /// </summary>
    /// <typeparam name="T">The artifact type being wrapped.</typeparam>
    public class ArtifactStateWrapper<T> : IArtifactStateWrapper<T> where T : IHaveAnId
    {
        /// <summary>
        /// This is a convenience property that acts as a proxy for Artifact.Id.
        /// </summary>
        public int Id { get { return Artifact.Id; } set { Artifact.Id = value; } }

        /// <summary>
        /// The artifact that is being wrapped.
        /// </summary>
        [JsonIgnore]
        public T Artifact { get; set; }

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
