using System;

namespace ArtifactStore.Models.Review
{
    public class ReviewInfo
    {
        public int ItemId { get; set; }

        public ReviewPackageStatus ReviewStatus { get; set; }

        public DateTime? ExpiryTimestamp { get; set; }
    }
}