using System;
using System.Collections.Generic;
using System.Linq;
using ArtifactStore.Helpers;

namespace ArtifactStore.Models.Review
{
    public class ParticipantArtifactStats
    {
        public int Id { get; set; }
        public string Prefix { get; set; }
        public string Name { get; set; }
        public int ItemTypeId { get; set; }
        public int ItemTypePredefined { get; set; }
        public int? IconImageId { get; set; }
        public bool ArtifactRequiresApproval { get; set; }
        public ViewStateType ViewState { get; set; }
        public string ApprovalStatus { get; set; }
        public bool HasAccess { get; set; }
        public DateTime? ESignatureTimestamp { get; set; }
        public IEnumerable<string> MeaningsOfSignature { get; set; }

        public static explicit operator ParticipantArtifactStats(ReviewedArtifact reviewedArtifact)
        {
            return new ParticipantArtifactStats()
            {
                Id = reviewedArtifact.Id,
                Prefix = reviewedArtifact.Prefix,
                Name = reviewedArtifact.Name,
                ItemTypeId = reviewedArtifact.ItemTypeId,
                ItemTypePredefined = reviewedArtifact.ItemTypePredefined,
                IconImageId = reviewedArtifact.IconImageId,
                ArtifactRequiresApproval = reviewedArtifact.IsApprovalRequired,
                ApprovalStatus = reviewedArtifact.Approval,
                ViewState = GetViewState(reviewedArtifact),
                HasAccess = reviewedArtifact.HasAccess,
                ESignatureTimestamp = reviewedArtifact.SignedOnTimestamp,
                MeaningsOfSignature = reviewedArtifact.MeaningOfSignatures.Select(mos => mos.GetMeaningOfSignatureDisplayValue())
            };
        }

        private static ViewStateType GetViewState(ReviewedArtifact reviewedArtifact)
        {
            if (reviewedArtifact.ViewState == ViewStateType.NotViewed || !reviewedArtifact.ViewedArtifactVersion.HasValue)
            {
                return ViewStateType.NotViewed;
            }

            return reviewedArtifact.ViewedArtifactVersion == reviewedArtifact.ArtifactVersion ? ViewStateType.Viewed : ViewStateType.Changed;
        }
    }
}
