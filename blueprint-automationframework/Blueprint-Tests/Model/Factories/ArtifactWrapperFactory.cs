using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ModelHelpers;
using Utilities;

namespace Model.Factories
{
    public static class ArtifactWrapperFactory
    {
        private static IArtifactStore ArtifactStore { get; } = ArtifactStoreFactory.GetArtifactStoreFromTestConfig();

        /// <summary>
        /// Wraps the specified raw artifact in an ArtifactWrapper (or appropriate sub-class).
        /// </summary>
        /// <param name="artifact">The artifact to wrap.</param>
        /// <param name="project">The project where the artifact exists.</param>
        /// <param name="createdBy">The user that created the artifact.</param>
        /// <returns>The wrapped artifact.</returns>
        public static ArtifactWrapper CreateArtifactWrapper(
            INovaArtifactDetails artifact,
            IProject project,
            IUser createdBy)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            // Wrap the artifact in the proper type of ArtifactWrapper.
            switch (artifact.PredefinedType.Value)
            {
                case (int) ItemTypePredefined.Document:
                    return new DocumentArtifactWrapper(artifact, project, createdBy);

                case (int) ItemTypePredefined.Process:
                    var novaProcess = ArtifactStore.GetNovaProcess(createdBy, artifact.Id);
                    return new ProcessArtifactWrapper(novaProcess, project, createdBy);

                default:
                    return new ArtifactWrapper(artifact, project, createdBy);
            }
        }
    }
}
