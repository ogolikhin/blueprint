﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtifactStore.Models.Review
{
    public class ReviewArtifactApprovalCheck
    {
        public bool ReviewExists { get; set; }
        public ReviewPackageStatus ReviewStatus { get; set; }
        public bool ReviewDeleted { get; set; }
        public bool AllArtifactsInReview { get; set; }
        public bool AllArtifactsRequireApproval { get; set; }
        public bool UserInReview { get; set; }
        public ReviewParticipantRole ReviewerRole { get; set; }
        public ReviewType ReviewType { get; set; }
        public ReviewStatus ReviewerStatus { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}
