using Newtonsoft.Json;
using System;

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

        public ReviewSource Source { get; set; }

        public ReviewPackageStatus ReviewPackageStatus { get; set; }

        public ReviewStatus Status { get; set; }

        public ReviewArtifactsStatus ArtifactsStatus { get; set; }

        public int RevisionId { get; set; }

        public int ProjectId { get; set; }

        public bool RequireAllArtifactsReviewed { get; set; }

        public bool ShowOnlyDescription { get; set; }
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

        public bool ShowOnlyDescription { get; set; }

        public int RevisionId { get; set; }

        public int TotalReviewers { get; set; }

        public int TotalArtifacts { get; set; }

        public int TotalViewable { get; set; }

        public int Approved { get; set; }

        public int Disapproved { get; set; }

        public int Pending { get; set; }

        public int Viewed { get; set; }

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

   
}