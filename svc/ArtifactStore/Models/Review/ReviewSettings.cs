﻿using System;

namespace ArtifactStore.Models.Review
{
    public class ReviewSettings
    {
        public ReviewSettings()
        {
        }

        public ReviewSettings(ReviewPackageRawData reviewPackageRawData)
        {
            EndDate = reviewPackageRawData?.EndDate;
            ShowOnlyDescription = reviewPackageRawData?.ShowOnlyDescription ?? false;
            CanMarkAsComplete = reviewPackageRawData?.IsAllowToMarkReviewAsCompleteWhenAllArtifactsReviewed ?? false;
            RequireESignature = reviewPackageRawData?.IsESignatureEnabled ?? false;
            RequireMeaningOfSignature = reviewPackageRawData?.IsMoSEnabled ?? false;
            IgnoreFolders = reviewPackageRawData?.IsIgnoreFolder ?? false;
        }

        public DateTime? EndDate { get; set; }

        public bool ShowOnlyDescription { get; set; }

        public bool CanMarkAsComplete { get; set; }

        public bool CanEditRequireESignature { get; set; }

        public bool RequireESignature { get; set; }

        public bool CanEditRequireMeaningOfSignature { get; set; }

        public bool RequireMeaningOfSignature { get; set; }

        public bool IsMeaningOfSignatureEnabledInProject { get; set; }

        public bool IgnoreFolders { get; set; }
    }
}
