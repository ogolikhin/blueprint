using System;

namespace Model.StorytellerModel.Impl
{
    public class VersionInfo
    {
        public VersionInfo(
            int? artifactId,
            DateTime? utcLockedDateTime,
            string lockOwnerLogin,
            int? projectId,
            int? versionId,
            int? revisionId,
            int? baselineId)
        {
            ArtifactId = artifactId;
            UtcLockedDateTime = utcLockedDateTime;
            LockOwnerLogin = lockOwnerLogin;
            ProjectId = projectId;
            VersionId = versionId;
            RevisionId = revisionId;
            BaselineId = baselineId;
        }

        /// <summary>
        /// Artifact Id of the Process artifact containing the process model
        /// </summary>
        public int? ArtifactId { get; set; }

        /// <summary>
        /// UTC Date/Time when the artifact was locked
        /// </summary>
        public DateTime? UtcLockedDateTime { get; set; }

        /// <summary>
        /// Login user that has a lock on the artifact
        /// </summary>
        public string LockOwnerLogin { get; set; }

        /// <summary>
        /// Project ID for the project containing the artifact
        /// </summary>
        public int? ProjectId { get; set; }

        /// <summary>
        /// Version Id of the artifact
        /// </summary>
        public int? VersionId { get; set; }

        /// <summary>
        /// Revision Id of the artifact
        /// </summary>
        public int? RevisionId { get; set; }

        /// <summary>
        /// Baseline Id of the baseline containing the artifact (if it exists)
        /// </summary>
        public int? BaselineId { get; set; }

        /// <summary>
        /// Flag indicating if full version information is provided
        /// </summary>
        public bool IsVersionInformationProvided { get; set; }

        /// <summary>
        /// Flag indicating if the artifact is at Head or Saved Draft version
        /// </summary>
        public bool IsHeadOrSavedDraftVersion { get; set; }
    }
}