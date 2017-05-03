using Model.ArtifactModel;
using Model.StorytellerModel.Impl;
using Newtonsoft.Json;
using Utilities;

namespace Model.ModelHelpers
{
    public class ProcessArtifactWrapper : ArtifactWrapper, INovaProcess
    {
        /// <summary>
        /// The artifact that is being wrapped.
        /// </summary>
        [JsonIgnore]
        public  INovaProcess NovaProcess {
            get { return (INovaProcess)Artifact; }
            set {Artifact = value; } }

        public Process Process
        {
            get
            {
                return NovaProcess.Process;
            }

            set
            {
                NovaProcess.Process = value;
            }
        }

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="artifact">The nova process artifact to wrap.</param>
        /// <param name="artifactStore">The ArtifactStore to use for REST calls.</param>
        /// <param name="svcShared">The SvcShared to use for REST calls.</param>
        /// <param name="project">The project where the artifact was created.</param>
        /// <param name="createdBy">The user who created the artifact.</param>
        /// <exception cref="AssertionException">If the Project ID of the artifact is different than the ID of the IProject.</exception>
        public ProcessArtifactWrapper(INovaProcess artifact, IArtifactStore artifactStore, ISvcShared svcShared, IProject project, IUser createdBy) : base(artifact, artifactStore, svcShared, project, createdBy)
        {
        }

        #endregion Constructors

        /// <summary>
        /// Updates this process artifact with the properties specified in the updateProcessArtifact.  You must lock the artifact before updating.
        /// NOTE: This method does not update the wrapped artifact with the properties you updated.  If you need this object to have all
        /// of the properties the same as they are on the server, call RefreshArtifactFromServer().
        /// </summary>
        /// <param name="user">The user to perform the update.</param>
        /// <param name="updateArtifact">The artifact whose non-null properties will be used to update this artifact.</param>
        /// <returns>The result of the update artifact call.</returns>
        public override INovaArtifactDetails Update(IUser user, INovaArtifactDetails updateArtifact)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(updateArtifact, nameof(updateArtifact));

            var updatedArtifact = ArtifactStore.UpdateNovaProcess(user, (INovaProcess)updateArtifact);

            ArtifactState.IsDraft = true;

            return updatedArtifact;
        }

        /// <summary>
        /// Gets the artifact from ArtifactStore and replaces the current artifact with the properties returned from the server.
        /// </summary>
        /// <param name="user">The user to authenticate with.</param>
        public override void RefreshArtifactFromServer(IUser user)
        {
            NovaProcess = ArtifactStore.GetNovaProcess(user, Id);
        }
    }
}
