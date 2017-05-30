using System;

namespace ArtifactStore.Models.Review
{
    public class ReviewedArtifact : BaseReviewArtifact
    {
        public ViewStateType ViewState { get; set; }

        public string Approval { get; set; }

        public ApprovalType ApprovalFlag { get; set; }

        public int ArtifactVersion { get; set; }

        public string UserDisplayName { get; set; }
        /// <summary>
        /// Viewed artifact version
        /// </summary>
        public int? ViewedArtifactVersion { get; set; }

        private DateTime _publishedOnTimestamp;
        public DateTime PublishedOnTimestamp
        {
            get
            {
                return _publishedOnTimestamp;
            }
            set
            {
                _publishedOnTimestamp = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            }
        }

        private DateTime? _signedOnTimestamp;
        /// <summary>
        /// e-signed by UserId on that UTC date time
        /// </summary>
        public DateTime? SignedOnTimestamp
        {
            get
            {
                return _signedOnTimestamp;
            }
            set
            {
                _signedOnTimestamp = value.HasValue? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc): (DateTime?)null;
            }
        }
    }
}