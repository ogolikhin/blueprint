namespace ServiceLibrary.Models.VersionControl
{
    public class SqlDiscardPublishState
    {
        public int ItemId { get; set; }

        public bool NotExist { get; set; }

        public bool NotArtifact { get; set; }

        public bool Deleted { get; set; }

        public bool HasPublishedVersion { get; set; }

        /// <summary>
        /// There is nothing to discard or publish.
        /// </summary>
        public bool NoChanges { get; set; }

        /// <summary>
        /// Validation failed when artifact was saved.
        /// </summary>
        public bool Invalid { get; set; }

        /// <summary>
        /// Artifact must be discarded together with other artifacts.
        /// </summary>
        public bool DiscardDependent { get; set; }

        ///// <summary>
        ///// Artifact must be published together with other artifacts.
        ///// </summary>
        public bool PublishDependent { get; set; }

        public int? LockedByUserId { get; set; }

        public bool? HasDraftRelationships { get; set; }

        public bool? LastSaveInvalid { get; set; }
    }
}
