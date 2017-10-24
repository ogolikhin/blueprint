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

        public ArtifactsMetrics(ReviewArtifactContent artifactsReview, ReviewParticipantsContent participants)
        {
            Total = participants.TotalArtifacts;
            ArtifactStatus = new ReviewArtifactsStatus
            {
                Approved = artifactsReview.TotalApproved,
                Disapproved = artifactsReview.TotalDisapproved,
                ViewedAll = artifactsReview.TotalViewed,
                UnviewedAll = artifactsReview.TotalUnviewed,
                ViewedSome = artifactsReview.TotalViewedSome,
                Pending = artifactsReview.TotalPending
            };
            RequestStatus = new ReviewRequestStatus
            {
                ApprovalRequested = participants.TotalArtifactsRequestedApproval,
                ReviewRequested = participants.TotalArtifacts - participants.TotalArtifactsRequestedApproval
            };
        }

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

        public ParticipantsMetrics(ReviewParticipantsContent participants)
        {
            Total = participants.Total;

            int approvers = 0, reviewers = 0;
            int aprCompleted = 0, aprInprogress = 0, aprNotstarted = 0;
            int rwrCompleted = 0, rwrInprogress = 0, rwrNotstarted = 0;
            foreach (var p in participants.Items)
            {
                if (p.Role == ReviewParticipantRole.Approver)
                    ++approvers;
                if (p.Role == ReviewParticipantRole.Reviewer)
                    ++reviewers;
                if (p.Role == ReviewParticipantRole.Approver && p.Status == ReviewStatus.Completed)
                    ++aprCompleted;
                if (p.Role == ReviewParticipantRole.Approver && p.Status == ReviewStatus.InProgress)
                    ++aprInprogress;
                if (p.Role == ReviewParticipantRole.Approver && p.Status == ReviewStatus.NotStarted)
                    ++aprNotstarted;
                if (p.Role == ReviewParticipantRole.Reviewer && p.Status == ReviewStatus.Completed)
                    ++rwrCompleted;
                if (p.Role == ReviewParticipantRole.Reviewer && p.Status == ReviewStatus.InProgress)
                    ++rwrInprogress;
                if (p.Role == ReviewParticipantRole.Reviewer && p.Status == ReviewStatus.NotStarted)
                    ++rwrNotstarted;
            }
            RoleStatus = new ParticipantRoles
            {
                Approvers = approvers,
                Reviewers = reviewers
            };

            ApproverStatus = new ParticipantStatus
            {
                Completed = aprCompleted,
                InProgress = aprInprogress,
                NotStarted = aprNotstarted
            };

            ReviewerStatus = new ParticipantStatus
            {
                Completed = rwrCompleted,
                InProgress = rwrInprogress,
                NotStarted = rwrNotstarted
            };
        }
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
        private int _disapproved = 0;
        private int _approved = 0;
        private int _viewed = 0;
        private int _unviewed = 0;

        public int ArtifactId { get; set; }
        public bool ApprovalRequired { get; set; }

        public void CalculateReviewStates()
        {
            foreach (var p in Participants)
            {
                if (p.ApprovalState == ApprovalType.Disapproved)
                    ++_disapproved;
                else if (p.ApprovalState == ApprovalType.Approved)
                    ++_approved;

                if (p.ViewState == ViewStateType.Viewed)
                    ++_viewed;
                else if (p.ViewState == ViewStateType.NotViewed)
                    ++_unviewed;

                if (p.Role == ReviewParticipantRole.Approver)
                    ++Approvers;
                else if (p.Role == ReviewParticipantRole.Reviewer)
                    ++Viewers;
            }
        }
        public ApprovalType ReviewState
        {
            get
            {
                if (_disapproved > 0)
                {
                    return ApprovalType.Disapproved;
                }
                else if (_approved == Approvers)
                {
                    return ApprovalType.Approved;
                }
                return ApprovalType.NotSpecified;
            }
        }

        public bool ViewedAll
        {
            get
            {
                return _viewed == Participants.Count;
            }
        }

        public bool UnviewedAll
        {
            get
            {
                return _unviewed == Participants.Count;
            }
        }

        public bool ViewedSome
        {
            get
            {
                return _viewed > 0 && _viewed < Participants.Count;
            }
        }

        public int Approvers { get; set; }

        public int Viewers { get; set; }

        public List<ParticipantReviewState> Participants { get; }

        public ArtifactReviewState()
        {
            Participants = new List<ParticipantReviewState>();
        }
    }

    public class ReviewArtifactContent
    {
        public int TotalApproved { get; set; }
        public int TotalDisapproved { get; set; }
        public int TotalPending { get; set; }
        public int TotalViewed { get; set; }
        public int TotalUnviewed { get; set; }
        public int TotalViewedSome { get; set; }
        public List<ArtifactReviewState> ReviewArtifactStates { get; }

        public ReviewArtifactContent()
        {
            ReviewArtifactStates = new List<ArtifactReviewState>();
        }

        public void CalculateReviewStates()
        {
            foreach (var a in ReviewArtifactStates)
            {
                a.CalculateReviewStates();

                if (a.ReviewState == ApprovalType.Approved)
                    ++TotalApproved;
                else if (a.ReviewState == ApprovalType.Disapproved)
                    ++TotalDisapproved;
                else if (a.ReviewState == ApprovalType.NotSpecified)
                    ++TotalPending;

                if (a.ViewedAll == true)
                    ++TotalViewed;

                if (a.UnviewedAll == true)
                    ++TotalUnviewed;

                if (a.ViewedSome == true)
                    ++TotalViewedSome;
            }
        }
    }
}

