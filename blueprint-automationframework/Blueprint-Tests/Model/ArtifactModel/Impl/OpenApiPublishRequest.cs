using Utilities;

namespace Model.ArtifactModel.Impl
{
    /// <summary>
    /// A class containing the minimum required values needed by an OpenAPI publish request.
    /// </summary>
    public class OpenApiPublishRequest
    {
        #region Serialized JSON properties

        public int Id { get; set; }
        public int ProjectId { get; set; }

        #endregion Serialized JSON properties

        /// <summary>
        /// Constructs a new OpenApiPublishRequest from the specified artifact.
        /// </summary>
        /// <param name="artifact">The artifact to be published.</param>
        public OpenApiPublishRequest(IArtifactBase artifact)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            Id = artifact.Id;
            ProjectId = artifact.ProjectId;
        }
    }
}
