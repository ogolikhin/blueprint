using Newtonsoft.Json;
using ServiceLibrary.Models;
using ServiceLibrary.Models.ProjectMeta;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

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

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool IsExpired { get; set; }

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

    public class ParticipantReviewState
    {
        public int UserId { get; set; }
        public ReviewParticipantRole Role { get; set; }
        public RolePermissions Permission { get; set; }
        public ReviewStatus Status { get; set; }
        public ApprovalType ApprovalState { get; set; }
        public ViewStateType ViewState { get; set; }
    }

    public class ArtifactReviewState
    {
        public int ArtifactId { get; set; }
        public bool ApprovalRequired { get; set; }
        public ApprovalType ReviewState
        {
            get
            {
                var disapproved = Participants.Count(p => p.ApprovalState == ApprovalType.Disapproved);
                if (disapproved > 0)
                {
                    return ApprovalType.Disapproved;
                }
                else
                {
                    var approved = Participants.Count(p => p.ApprovalState == ApprovalType.Approved);
                    if (approved == Approvers)
                    {
                        return ApprovalType.Approved;
                    }
                }
                return ApprovalType.Pending;
            }
        }

        public bool ViewedAll
        {
            get
            {
                var viewed = Participants.Count(p => p.ViewState == ViewStateType.Viewed);
                return viewed == Participants.Count;
            }
        }

        public bool UnviewedAll
        {
            get
            {
                var unviewed = Participants.Count(p => p.ViewState == ViewStateType.NotViewed);
                return unviewed == Participants.Count;
            }
        }

        public bool ViewedSome
        {
            get
            {
                var viewed = Participants.Count(p => p.ViewState == ViewStateType.Viewed);
                return viewed > 0 && viewed < Participants.Count;
            }
        }

        public int Approvers
        {
            get
            {
                return Participants.Count(p => p.Role == ReviewParticipantRole.Approver);
            }
        }

        public int Viewers
        {
            get
            {
                return Participants.Count(p => p.Role == ReviewParticipantRole.Reviewer);
            }
        }

        public List<ParticipantReviewState> Participants { get; }

        public ArtifactReviewState()
        {
            Participants = new List<ParticipantReviewState>();
        }
    }

    public class ReviewArtifactContent
    {
        public int TotalApproved
        {
            get
            {
                return ReviewArtifactStates.Count(a => a.ReviewState == ApprovalType.Approved);
            }
        }
        public int TotalDisapproved
        {
            get
            {
                return ReviewArtifactStates.Count(a => a.ReviewState == ApprovalType.Disapproved);
            }
        }
        public int TotalPending
        {
            get
            {
                return ReviewArtifactStates.Count(a => a.ReviewState == ApprovalType.Pending);
            }
        }

        public int TotalViewed
        {
            get
            {
                return ReviewArtifactStates.Count(a => a.ViewedAll == true);
            }
        }
        public int TotalUnviewed
        {
            get
            {
                return ReviewArtifactStates.Count(a => a.UnviewedAll == true);
            }
        }

        public int TotalViewedSome
        {
            get
            {
                return ReviewArtifactStates.Count(a => a.ViewedSome == true);
            }
        }
        public List<ArtifactReviewState> ReviewArtifactStates { get; }

        public ReviewArtifactContent()
        {
            ReviewArtifactStates = new List<ArtifactReviewState>();
        }
    }
}

