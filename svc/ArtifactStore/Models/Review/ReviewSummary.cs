using Newtonsoft.Json;
using ServiceLibrary.Models;
using ServiceLibrary.Models.ProjectMeta;
using System;
using System.Collections.Generic;

namespace ArtifactStore.Models.Review
{
    public class ReviewSummary
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Prefix { get; set; }

        public string ArtifactType { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        public int TotalArtifacts { get; set; }

        public int TotalViewable { get; set; }

        public ReviewType ReviewType { get; set; }

        public ReviewParticipantRole? ReviewParticipantRole { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? ExpirationDate { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? ClosedDate { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool IsExpired { get; set; }

        public ReviewSource Source { get; set; }

        public ReviewPackageStatus ReviewPackageStatus { get; set; }

        public ReviewStatus Status { get; set; }

        public ReviewParticipantArtifactsStats ArtifactsStatus { get; set; }

        public int RevisionId { get; set; }

        public int ProjectId { get; set; }

        public bool RequireAllArtifactsReviewed { get; set; }

        public bool RequireESignature { get; set; }

        public bool RequireMeaningOfSignature { get; set; }

        public bool ShowOnlyDescription { get; set; }

        public IEnumerable<SelectedMeaningOfSignature> MeaningOfSignatures { get; set; }
    }

    internal class ReviewSummaryDetails
    {
        public int? BaselineId { get; set; }

        public string Prefix { get; set; }

        public string ArtifactType { get; set; }

        public ReviewParticipantRole? ReviewParticipantRole { get; set; }

        public ReviewPackageStatus ReviewPackageStatus { get; set; }

        public ReviewStatus ReviewStatus { get; set; }

        public bool RequireAllArtifactsReviewed { get; set; }

        public bool RequireESignature { get; set; }

        public bool RequireMeaningOfSignature { get; set; }

        public bool ShowOnlyDescription { get; set; }

        public int RevisionId { get; set; }

        public int TotalReviewers { get; set; }

        public int TotalArtifacts { get; set; }

        public int TotalViewable { get; set; }

        public int Approved { get; set; }

        public int Disapproved { get; set; }

        public int Pending { get; set; }

        public int Viewed { get; set; }

        private DateTime? _expirationDate;
        public DateTime? ExpirationDate
        {
            get
            {
                return _expirationDate;
            }
            set
            {
                if (value.HasValue)
                {
                    _expirationDate = value.Value.Kind != DateTimeKind.Utc ? value.Value.ToUniversalTime() : value;
                }
                else
                {
                    _expirationDate = value;
                }
            }
        }

    }

    /// <summary>
    /// Review Summary Metrics
    /// </summary>

    public class ReviewSummaryMetrics
    {
        public int Id { get; set; }
        public int RevisionId { get; set; }
        public ReviewStatus Status { get; set; }
        public ArtifactsMetrics Artifacts { get; set; }
        public ParticipantsMetrics Participants { get; set; }
    }

    public class ArtifactsMetrics
    {
        // Number of Artifacts included into Review
        public int Total { get; set; }

        // Number of approved/disapproved/viewed/unviewed Artifacts
        public ReviewArtifactsStatus ArtifactStatus { get; set; }

        // Number of Approval/Review Requests for included Artifacts
        public ReviewRequestStatus RequestStatus { get; set; }
    }

    public class ReviewRequestStatus
    {
        // Number of Approval Requests
        public int ApprovalRequested { get; set; }

        // Number of Review Requests
        public int ReviewRequested { get; set; }
    }

    public class ParticipantsMetrics
    {
        // Total number of Participants
        public int Total { get; set; }

        // Number of Participants in each Role
        public ParticipantRoles RoleStatus { get; set; }

        // Number of approvers in each status
        public ParticipantStatus ApproverStatus { get; set; }

        // Number of reviewers in each status
        public ParticipantStatus ReviewerStatus { get; set; }
    }

    public class ParticipantRoles
    {
        // Number of Approvers
        public int Approvers { get; set; }

        // Number of Reviewers
        public int Reviewers { get; set; }
    }

    public class ParticipantStatus
    {
        // Number of Participants with Completed status
        public int Completed { get; set; }

        // Number of Participants with InProgress status
        public int InProgress { get; set; }

        // Number of Participants with NotStarted status
        public int NotStarted { get; set; }
    }

    public class FlatReviewSummaryMetrics
    {
        public int ReviewId { get; set; }
        public int RevisionId { get; set; }
        public string ReviewPackageStatus { get; set; }
        public string ReviewStatus { get; set; }
        public int TotalArtifacts { get; set; }
        public int ArtifactsApprovedByAll { get; set; }
        public int ArtifactsDisapproved { get; set; }
        public int ArtifactsPending { get; set; }
        public int ArtifactApprovalRequired { get; set; }
        public int ArtifactReviewRequired { get; set; }
        public int ArtifactsViewedByAll { get; set; }
        public int ArtifactsViewedByNone { get; set; }
        public int ArtifactsViewedBySome { get; set; }
        public int TotalParticipants { get; set; }
        public int NumberOfApprovers { get; set; }
        public int NumberOfReviewers { get; set; }
        public int ApproverStatusCompleted { get; set; }
        public int ApproverStatusInProgress { get; set; }
        public int ApproverStatusNotStarted { get; set; }
        public int ReviewerStatusCompleted { get; set; }
        public int ReviewerStatusInProgress { get; set; }
        public int ReviewerStatusNotStarted { get; set; }
    }
}

