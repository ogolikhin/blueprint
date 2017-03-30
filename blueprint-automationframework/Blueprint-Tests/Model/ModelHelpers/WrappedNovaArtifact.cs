using Model.ArtifactModel;

namespace Model.ModelHelpers
{
    public interface IWrappedNovaArtifact : IArtifactWrapper<INovaArtifactDetails>
    {
    }

    public class WrappedNovaArtifact : ArtifactWrapper<INovaArtifactDetails>, IWrappedNovaArtifact
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="artifact">The artifact to wrap.</param>
        /// <param name="artifactStore">The ArtifactStore to use for REST calls.</param>
        /// <param name="svcShared">The SvcShared to use for REST calls.</param>
        /// <param name="createdBy">The user who created the artifact.</param>
        public WrappedNovaArtifact(INovaArtifactDetails artifact, IArtifactStore artifactStore, ISvcShared svcShared, IUser createdBy)
            : base(artifact, artifactStore, svcShared, createdBy)
        {
            // Intentionally left blank.
        }
    }

    public class WrappedNovaArtifactState : ArtifactStateWrapper<INovaArtifactDetails>
    {
    }
}
