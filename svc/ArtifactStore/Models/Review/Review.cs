using System;
using System.Linq;
using ArtifactStore.Helpers;

namespace ArtifactStore.Models.Review
{
    public class Review
    {
        public Review(ReviewData reviewData)
        {
            if (reviewData == null)
                throw new ArgumentNullException(nameof(reviewData));

            Id = reviewData.Id;
            BaselineId = reviewData.BaselineId;

            if (!ReviewRawDataHelper.TryRestoreData(reviewData.ReviewContentsXml, out _contents))
            {
                _contents = new RDReviewContents();
            }

            if (!ReviewRawDataHelper.TryRestoreData(reviewData.ReviewPackageRawDataXml, out _reviewPackageRawData))
            {
                _reviewPackageRawData = new ReviewPackageRawData();
            }
        }

        internal Review(int id, int? baselineId = null, RDReviewContents contents = null, ReviewPackageRawData rawData = null)
        {
            Id = id;
            BaselineId = baselineId;
            _contents = contents ?? new RDReviewContents();
            _reviewPackageRawData = rawData ?? new ReviewPackageRawData();
        }

        public int Id { get; set; }

        public int? BaselineId { get; set; }

        private readonly RDReviewContents _contents;
        internal RDReviewContents Contents => _contents;

        private readonly ReviewPackageRawData _reviewPackageRawData;
        internal ReviewPackageRawData ReviewPackageRawData => _reviewPackageRawData;

        internal ReviewPackageStatus ReviewStatus => ReviewPackageRawData.Status;

        internal ReviewType ReviewType
        {
            get
            {
                if (ReviewPackageRawData.Reviewers == null || !ReviewPackageRawData.Reviewers.Any())
                {
                    return ReviewType.Public;
                }

                if (ReviewPackageRawData.Reviewers.All(r => r.Permission == ReviewParticipantRole.Reviewer)
                    || Contents.Artifacts == null
                    || Contents.Artifacts.All(a => a.ApprovalNotRequested ?? false))
                {
                    return ReviewType.Informal;
                }

                return ReviewType.Formal;
            }
        }
    }
}
