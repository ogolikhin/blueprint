using Newtonsoft.Json;

namespace Model.ModelHelpers
{
    /// <summary>
    /// This class wraps Blueprint Artifacts and tracks the state of the artifact.
    /// The artifact state is only needed by the automation framework (mostly for the Dispose() method).
    /// </summary>
    /// <typeparam name="T">The artifact type being wrapped.</typeparam>
    public class ArtifactStateWrapper<T> where T : IArtifactId
    {
        [JsonIgnore]
        public T Artifact { get; set; }

        [JsonIgnore]
        public IProject Project { get; set; }
        [JsonIgnore]
        public IUser CreatedBy { get; set; }
        [JsonIgnore]
        public IUser LockOwner { get; set; }

        [JsonIgnore]
        public bool IsDeleted { get; set; }
        [JsonIgnore]
        public bool IsMarkedForDeletion { get; set; }
        [JsonIgnore]
        public bool IsPublished { get; set; }
        [JsonIgnore]
        public bool IsSaved { get; set; }
    }
}
