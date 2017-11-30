using System.Linq;
using ArtifactStore.Helpers;

namespace ArtifactStore.Models.Review
{
    public class ReviewData
    {
        public int Id { get; set; }

        public string ReviewPackageRawDataXml { get; set; }

        public string ReviewContentsXml { get; set; }

        public int? BaselineId { get; set; }

        private RDReviewContents _reviewContents;
        internal RDReviewContents ReviewContents
        {
            get
            {
                if (_reviewContents == null)
                {
                    if (!ReviewRawDataHelper.TryRestoreData(ReviewContentsXml, out _reviewContents))
                    {
                        _reviewContents = new RDReviewContents();
                    }
                }
                return _reviewContents;
            }
        }

        private ReviewPackageRawData _reviewPackageRawData;
        internal ReviewPackageRawData ReviewPackageRawData
        {
            get
            {
                if (_reviewPackageRawData == null)
                {
                    if (!ReviewRawDataHelper.TryRestoreData(ReviewPackageRawDataXml, out _reviewPackageRawData))
                    {
                        _reviewPackageRawData = new ReviewPackageRawData();
                    }
                }
                return _reviewPackageRawData;
            }
        }

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
                    || ReviewContents.Artifacts == null
                    || ReviewContents.Artifacts.All(a => a.ApprovalNotRequested.Value))
                {
                    return ReviewType.Informal;
                }
                return ReviewType.Formal;
            }
        }
    }
}