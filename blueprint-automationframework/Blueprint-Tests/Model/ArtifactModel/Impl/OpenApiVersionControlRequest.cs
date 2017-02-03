using Utilities;

namespace Model.ArtifactModel.Impl
{
    /// <summary>
    /// A class containing the minimum required values needed by an OpenAPI discard or publish request.
    /// </summary>
    public class OpenApiVersionControlRequest
    {
        #region Serialized JSON properties

        public int Id { get; set; }
        public int ProjectId { get; set; }

        #endregion Serialized JSON properties

        /// <summary>
        /// Constructs a new OpenApiVersionControlRequest from the specified artifact.
        /// </summary>
        /// <param name="artifact">The artifact to be discarded/published.</param>
        public OpenApiVersionControlRequest(IArtifactBase artifact)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            Id = artifact.Id;
            ProjectId = artifact.ProjectId;
        }
    }
}
