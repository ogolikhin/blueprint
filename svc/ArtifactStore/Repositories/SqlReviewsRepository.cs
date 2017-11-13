﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.Helpers;
using ArtifactStore.Models.Review;
using Dapper;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ApplicationSettings;
using ServiceLibrary.Services;

namespace ArtifactStore.Repositories
{
    public class SqlReviewsRepository : IReviewsRepository
    {
        private const string NotSpecified = "Not Specified";
        private const string Pending = "Pending";
        private const string Unauthorized = "Unauthorized";

        private readonly ISqlConnectionWrapper _connectionWrapper;
        private readonly IArtifactVersionsRepository _artifactVersionsRepository;
        private readonly IItemInfoRepository _itemInfoRepository;
        private readonly IArtifactPermissionsRepository _artifactPermissionsRepository;
        private readonly IUsersRepository _usersRepository;
        private readonly IArtifactRepository _artifactRepository;
        private readonly ICurrentDateTimeService _currentDateTimeService;
        private readonly IApplicationSettingsRepository _applicationSettingsRepository;
        private readonly ILockArtifactsRepository _lockArtifactsRepository;
        private readonly ISqlHelper _sqlHelper;

        internal const string ReviewArtifactHierarchyRebuildIntervalInMinutesKey = "ReviewArtifactHierarchyRebuildIntervalInMinutes";
        internal const int DefaultReviewArtifactHierarchyRebuildIntervalInMinutes = 20;

        public SqlReviewsRepository() : this(
            new SqlConnectionWrapper(ServiceConstants.RaptorMain),
            new SqlArtifactVersionsRepository(),
            new SqlItemInfoRepository(),
            new SqlArtifactPermissionsRepository(),
            new ApplicationSettingsRepository(),
            new SqlUsersRepository(),
            new SqlArtifactRepository(),
            new CurrentDateTimeService(),
            new SqlLockArtifactsRepository(),
            new SqlHelper())
        {
        }

        public SqlReviewsRepository(
            ISqlConnectionWrapper connectionWrapper,
            IArtifactVersionsRepository artifactVersionsRepository,
            IItemInfoRepository itemInfoRepository,
            IArtifactPermissionsRepository artifactPermissionsRepository,
            IApplicationSettingsRepository applicationSettingsRepository,
            IUsersRepository usersRepository,
            IArtifactRepository artifactRepository,
            ICurrentDateTimeService currentDateTimeService,
            ILockArtifactsRepository lockArtifactsRepository,
            ISqlHelper sqlHelper)
        {
            _connectionWrapper = connectionWrapper;
            _artifactVersionsRepository = artifactVersionsRepository;
            _itemInfoRepository = itemInfoRepository;
            _artifactPermissionsRepository = artifactPermissionsRepository;
            _applicationSettingsRepository = applicationSettingsRepository;
            _usersRepository = usersRepository;
            _artifactRepository = artifactRepository;
            _currentDateTimeService = currentDateTimeService;
            _lockArtifactsRepository = lockArtifactsRepository;
            _sqlHelper = sqlHelper;
        }

        public async Task<ReviewSummary> GetReviewSummary(int containerId, int userId)
        {
            var reviewInfo = await _artifactVersionsRepository.GetVersionControlArtifactInfoAsync(containerId, null, userId);
            if (reviewInfo.IsDeleted || reviewInfo.PredefinedType != ItemTypePredefined.ArtifactReviewPackage)
            {
                throw ReviewsExceptionHelper.ReviewNotFoundException(containerId);
            }

            var reviewDetails = await GetReviewSummaryDetails(containerId, userId);

            if (reviewDetails == null)
            {
                throw ReviewsExceptionHelper.ReviewNotFoundException(containerId);
            }

            if (reviewDetails.ReviewPackageStatus == ReviewPackageStatus.Draft)
            {
                throw ReviewsExceptionHelper.ReviewNotFoundException(containerId);
            }

            if (!reviewDetails.ReviewParticipantRole.HasValue && reviewDetails.TotalReviewers > 0)
            {
                throw ReviewsExceptionHelper.UserCannotAccessReviewException(containerId);
            }

            DateTime? closedDate = null;
            if (reviewDetails.ReviewPackageStatus == ReviewPackageStatus.Closed)
            {
                closedDate = await GetReviewCloseDateAsync(containerId);
            }

            var reviewSource = new ReviewSource();
            if (reviewDetails.BaselineId.HasValue)
            {
                var baselineInfo = await _artifactVersionsRepository.GetVersionControlArtifactInfoAsync(reviewDetails.BaselineId.Value, null, userId);
                reviewSource.Id = baselineInfo.Id;
                reviewSource.Name = baselineInfo.Name;
                reviewSource.Prefix = baselineInfo.Prefix;
            }

            var description = await _itemInfoRepository.GetItemDescription(containerId, userId);

            ReviewType reviewType;
            if (reviewDetails.TotalReviewers == 0)
            {
                reviewType = ReviewType.Public;
            }
            else
            {
                reviewType = await GetReviewTypeAsync(containerId, userId);
            }

            return new ReviewSummary
            {
                Id = containerId,
                Name = reviewInfo.Name,
                Prefix = reviewDetails.Prefix,
                ArtifactType = reviewDetails.ArtifactType,
                Description = description,
                Source = reviewSource,
                ReviewParticipantRole = reviewDetails.ReviewParticipantRole,
                TotalArtifacts = reviewDetails.TotalArtifacts,
                TotalViewable = reviewDetails.TotalViewable,
                Status = reviewDetails.ReviewStatus,
                ReviewPackageStatus = reviewDetails.ReviewPackageStatus,
                RequireAllArtifactsReviewed = reviewDetails.RequireAllArtifactsReviewed,
                RequireESignature = reviewDetails.RequireESignature,
                RequireMeaningOfSignature = reviewDetails.RequireMeaningOfSignature,
                ShowOnlyDescription = reviewDetails.ShowOnlyDescription,
                ExpirationDate = reviewDetails.ExpirationDate,
                ClosedDate = closedDate,
                IsExpired = reviewDetails.ExpirationDate < _currentDateTimeService.GetUtcNow(),
                ArtifactsStatus = new ReviewParticipantArtifactsStats
                {
                    Approved = reviewDetails.Approved,
                    Disapproved = reviewDetails.Disapproved,
                    Pending = reviewDetails.Pending,
                    Viewed = reviewDetails.Viewed
                },
                ReviewType = reviewType,
                RevisionId = reviewDetails.RevisionId,
                ProjectId = reviewInfo.ProjectId
            };
        }

        public async Task<ReviewSummaryMetrics> GetReviewSummaryMetrics(int containerId, int userId)
        {
            var reviewInfo = await _artifactVersionsRepository.GetVersionControlArtifactInfoAsync(containerId, null, userId);
            if (reviewInfo.IsDeleted || reviewInfo.PredefinedType != ItemTypePredefined.ArtifactReviewPackage)
            {
                throw ReviewsExceptionHelper.ReviewNotFoundException(containerId);
            }

            var reviewDetails = await GetReviewDetailsAsync(containerId, userId);

            var participants = await GetAllReviewParticipantsAsync(containerId, userId);

            var artifacts = await GetAllReviewArtifactsAsync<ReviewedArtifact>(containerId, userId);

            var artifactsMetrics = await GetReviewMetricsAsync(containerId, artifacts, participants);

            return new ReviewSummaryMetrics
            {
                Id = containerId,
                RevisionId = reviewDetails.RevisionId,
                Status = reviewDetails.ReviewStatus,
                Artifacts = new ArtifactsMetrics(artifactsMetrics, participants),
                Participants = new ParticipantsMetrics(participants)
            };
        }

        private async Task<ReviewSummaryDetails> GetReviewDetailsAsync(int reviewId, int userId)
        {
            var reviewDetails = await GetReviewSummaryDetails(reviewId, userId);

            if (reviewDetails == null || reviewDetails.ReviewPackageStatus == ReviewPackageStatus.Draft)
            {
                throw ReviewsExceptionHelper.ReviewNotFoundException(reviewId);
            }

            return reviewDetails;
        }

        private async Task<ReviewArtifactContent> GetReviewMetricsAsync(int reviewId, ReviewArtifactsQueryResult<ReviewedArtifact> artifacts, ReviewParticipantsContent participants)
        {
            int revisionId = int.MaxValue;
            var artifactIds = artifacts.Items.Select(a => a.Id).ToList();

            var artifactsReview = new ReviewArtifactContent();
            foreach (var art in artifacts.Items)
            {
                var state = new ArtifactReviewState
                {
                    ArtifactId = art.Id,
                    ApprovalRequired = art.IsApprovalRequired
                };
                artifactsReview.ReviewArtifactStates.Add(state);
            }

            foreach (var p in participants.Items)
            {
                var reviewArtifacts = await GetReviewArtifactsByParticipantAsync(artifactIds, p.UserId, reviewId, revisionId);
                var permissions = await _artifactPermissionsRepository.GetArtifactPermissions(artifactIds, p.UserId);
                foreach (var r in reviewArtifacts)
                {
                    var permission = permissions.FirstOrDefault(ap => ap.Key == r.Id).Value;
                    if ((permission & RolePermissions.Read) != 0)
                    {
                        var participant = new ParticipantReviewState
                        {
                            UserId = p.UserId,
                            Role = p.Role,
                            Permission = permission,
                            Status = p.Status,
                            ApprovalState = r.ApprovalFlag,
                            ViewState = r.ViewState
                        };
                        var artifactReview = artifactsReview.ReviewArtifactStates.FirstOrDefault(a => a.ArtifactId == r.Id);
                        artifactReview?.Participants.Add(participant);
                    }
                }
            }
            artifactsReview.CalculateReviewStates();

            return artifactsReview;
        }

        public Task<ReviewType> GetReviewTypeAsync(int reviewId, int userId, int revisionId = int.MaxValue, bool includeDrafts = true)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@reviewId", reviewId);
            parameters.Add("@userId", userId);
            parameters.Add("@revisionId", revisionId);
            parameters.Add("@includeDrafts", revisionId == int.MaxValue && includeDrafts);

            return _connectionWrapper.ExecuteScalarAsync<ReviewType>("GetReviewType", parameters, commandType: CommandType.StoredProcedure);
        }

        private async Task<ReviewSummaryDetails> GetReviewSummaryDetails(int reviewId, int userId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@reviewId", reviewId);
            parameters.Add("@userId", userId);

            return (await _connectionWrapper.QueryAsync<ReviewSummaryDetails>("GetReviewDetails", parameters, commandType: CommandType.StoredProcedure)).SingleOrDefault();
        }

        public async Task<QueryResult<ReviewArtifact>> GetReviewArtifactsContentAsync(int reviewId, int userId, Pagination pagination, int? versionId = null, bool? addDrafts = true)
        {
            int? revisionId = await _itemInfoRepository.GetRevisionId(reviewId, userId, versionId);
            ReviewArtifactsQueryResult<ReviewArtifact> reviewArtifacts;
            if (versionId == null)
            {
                reviewArtifacts = await GetReviewArtifactsAsync<ReviewArtifact>(reviewId, userId, pagination, revisionId, addDrafts);
            }
            else
            {
                reviewArtifacts = await GetHistoricalReviewArtifactsAsync<ReviewArtifact>(reviewId, userId, pagination, revisionId);
            }

            var reviewArtifactIds = reviewArtifacts.Items.Select(a => a.Id).ToList();
            reviewArtifactIds.Add(reviewId);

            var artifactPermissionsDictionary = await _artifactPermissionsRepository.GetArtifactPermissions(reviewArtifactIds, userId);

            if (!SqlArtifactPermissionsRepository.HasPermissions(reviewId, artifactPermissionsDictionary, RolePermissions.Read))
            {
                throw ReviewsExceptionHelper.UserCannotAccessReviewException(reviewId);
            }

            var reviewArtifactStatuses = await GetReviewArtifactStatusesAsync(reviewId, userId, pagination, versionId, addDrafts, reviewArtifactIds);
            var numUsers = reviewArtifactStatuses.NumUsers;
            var numApprovers = reviewArtifactStatuses.NumApprovers;
            var artifactStatusDictionary = reviewArtifactStatuses.ItemStatuses.ToDictionary(a => a.ArtifactId);

            foreach (var reviewArtifact in reviewArtifacts.Items)
            {
                ReviewArtifactStatus reviewArtifactStatus;

                if (artifactStatusDictionary.TryGetValue(reviewArtifact.Id, out reviewArtifactStatus))
                {
                    reviewArtifact.Pending = reviewArtifactStatus.Pending;
                    reviewArtifact.Approved = reviewArtifactStatus.Approved;
                    reviewArtifact.Disapproved = reviewArtifactStatus.Disapproved;
                    reviewArtifact.Viewed = reviewArtifactStatus.Viewed;
                    reviewArtifact.Unviewed = reviewArtifactStatus.Unviewed;
                    reviewArtifact.HasReviewComments = reviewArtifactStatus.HasComments;
                }
                else
                {
                    reviewArtifact.Pending = numApprovers;
                    reviewArtifact.Unviewed = numUsers;
                }

                if (SqlArtifactPermissionsRepository.HasPermissions(reviewArtifact.Id, artifactPermissionsDictionary, RolePermissions.Read))
                {
                    reviewArtifact.HasAccess = true;
                }
                else
                {
                    ClearReviewArtifactProperties(reviewArtifact);
                }
            }

            return reviewArtifacts;
        }

        private static void ClearReviewArtifactProperties(BaseReviewArtifact reviewArtifact)
        {
            reviewArtifact.Name = string.Empty;
            reviewArtifact.ItemTypeId = 0;
            reviewArtifact.HasComments = false;
            reviewArtifact.ItemTypePredefined = 0;
            reviewArtifact.IconImageId = null;
            reviewArtifact.HasAccess = false;
        }

        /// <summary>
        /// Adds artifacts to the given review, locks the review if it is not locked
        /// </summary>
        /// <param name="reviewId"></param>
        /// <param name="userId"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<AddArtifactsResult> AddArtifactsToReviewAsync(int reviewId, int userId, AddArtifactsParameter content)
        {
            if (content.ArtifactIds == null || !content.ArtifactIds.Any())
            {
                throw new BadRequestException("There is nothing to add to review.", ErrorCodes.OutOfRangeParameter);
            }

            if (!await _artifactPermissionsRepository.HasEditPermissions(reviewId, userId))
            {
                throw ReviewsExceptionHelper.UserCannotModifyReviewException(reviewId);
            }

            int alreadyIncludedCount;
            var propertyResult = await GetReviewPropertyStringAsync(reviewId, userId);
            var artifactIds = content.ArtifactIds;
            if (propertyResult.ReviewStatus == ReviewPackageStatus.Closed)
            {
                throw ReviewsExceptionHelper.ReviewClosedException();
            }

            if (propertyResult.ProjectId == null || propertyResult.ProjectId < 1)
            {
                throw ReviewsExceptionHelper.ReviewNotFoundException(reviewId);
            }

            if (propertyResult.LockedByUserId.HasValue)
            {
                if (propertyResult.LockedByUserId.Value != userId)
                {
                    throw ExceptionHelper.ArtifactNotLockedException(reviewId, userId);
                }
            }
            else
            {
                await _lockArtifactsRepository.LockArtifactAsync(reviewId, userId);
            }

            if (content.AddChildren)
            {
                var childIds = await GetChildrenArtifacts(userId, artifactIds);
                var setIds = new HashSet<int>(artifactIds.Union(childIds.Select(c => c.VersionItemId)));
                artifactIds = setIds.ToList();
            }

            var effectiveIds = await GetEffectiveArtifactIds(userId, artifactIds, propertyResult.ProjectId.Value);

            if (effectiveIds.ArtifactIds == null || effectiveIds.ArtifactIds.IsEmpty())
            {
                if (effectiveIds.IsBaselineAdded)
                {
                    throw ReviewsExceptionHelper.BaselineNotSealedException();
                }
            }

            // If review is active and formal we throw conflict exception. No changes allowed
            if (propertyResult.ReviewStatus == ReviewPackageStatus.Active &&
                propertyResult.ReviewType == ReviewType.Formal)
            {
                throw ReviewsExceptionHelper.ReviewActiveFormalException();
            }

            // We replace all artifacts if baseline was added or baseline was replaced
            var replaceAllArtifacts = effectiveIds.IsBaselineAdded || (propertyResult.BaselineId != null && propertyResult.BaselineId > 0);

            var artifactXmlResult = AddArtifactsToXML(propertyResult.ArtifactXml,
                new HashSet<int>(effectiveIds.ArtifactIds),
                replaceAllArtifacts,
                out alreadyIncludedCount);

            Func<IDbTransaction, Task> transactionAction = async transaction =>
            {
                await UpdateReviewArtifactsAsync(reviewId, userId, artifactXmlResult, transaction);

                int? baselineId = null;
                if (effectiveIds.IsBaselineAdded)
                {
                    baselineId = artifactIds.First();
                }

                await CreateUpdateRemoveReviewBaselineLink(reviewId, propertyResult.ProjectId.Value, userId, !effectiveIds.IsBaselineAdded, baselineId, transaction);
            };

            await _sqlHelper.RunInTransactionAsync(ServiceConstants.RaptorMain, transactionAction);

            return new AddArtifactsResult
            {
                ArtifactCount = effectiveIds.ArtifactIds.Count() - alreadyIncludedCount,
                AlreadyIncludedArtifactCount = alreadyIncludedCount,
                NonexistentArtifactCount = effectiveIds.Nonexistent,
                UnpublishedArtifactCount = effectiveIds.Unpublished
            };
        }

        private async Task<IEnumerable<ChildArtifactsResult>> GetChildrenArtifacts(int userId, IEnumerable<int> artifactIds)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            parameters.Add("@artifactIds", SqlConnectionWrapper.ToDataTable(artifactIds));
            parameters.Add("@revisionId", int.MaxValue);
            parameters.Add("@includeDrafts", false);

            return (await _connectionWrapper.QueryAsync<ChildArtifactsResult>("GetChildArtifacts", parameters, commandType: CommandType.StoredProcedure));
        }

        public async Task<PropertyValueString> GetReviewPropertyStringAsync(int reviewId, int userId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@reviewId", reviewId);
            parameters.Add("@userId", userId);

            return (await _connectionWrapper.QueryAsync<PropertyValueString>("GetReviewPropertyString", parameters, commandType: CommandType.StoredProcedure)).SingleOrDefault();
        }

        public async Task<PropertyValueString> GetReviewApprovalRolesInfoAsync(int reviewId, int userId, int roleUserId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@reviewId", reviewId);
            parameters.Add("@userId", userId);
            parameters.Add("@roleUserId", roleUserId);

            return (await _connectionWrapper.QueryAsync<PropertyValueString>("GetReviewApprovalRolesInfo", parameters, commandType: CommandType.StoredProcedure)).SingleOrDefault();
        }

        private async Task<EffectiveArtifactIdsResult> GetEffectiveArtifactIds(int userId, IEnumerable<int> artifactIds, int projectId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@artifactIds", SqlConnectionWrapper.ToDataTable(artifactIds));
            parameters.Add("@userId", userId);
            parameters.Add("@projectId", projectId);

            var result = await _connectionWrapper.QueryMultipleAsync<int, int, int, bool>("GetEffectiveArtifactIds", parameters, commandType: CommandType.StoredProcedure);

            return new EffectiveArtifactIdsResult
            {
                ArtifactIds = result.Item1.ToList(),
                Unpublished = result.Item2.SingleOrDefault(),
                Nonexistent = result.Item3.SingleOrDefault(),
                IsBaselineAdded = result.Item4.SingleOrDefault()
            };
        }

        private async Task CreateUpdateRemoveReviewBaselineLink(int reviewId, int projectId, int userId, bool onlyRemoveLink, int? baselineId, IDbTransaction transaction)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@reviewId", reviewId);
            parameters.Add("@projectId", projectId);
            parameters.Add("@userId", userId);
            parameters.Add("@onlyRemoveLink", onlyRemoveLink);
            parameters.Add("@baselineId", baselineId ?? 0);

            if (transaction == null)
            {
                await _connectionWrapper.ExecuteAsync
                (
                    "CreateUpdateRemoveReviewBaselineLink",
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            else
            {
                await transaction.Connection.ExecuteAsync
                (
                    "CreateUpdateRemoveReviewBaselineLink",
                    parameters,
                    transaction,
                    commandType: CommandType.StoredProcedure);
            }
        }

        private static string AddArtifactsToXML(string xmlArtifacts, ISet<int> artifactsToAdd, bool replaceAllArtifacts, out int alreadyIncluded)
        {
            alreadyIncluded = 0;

            RDReviewContents rdReviewContents;

            if (replaceAllArtifacts || string.IsNullOrEmpty(xmlArtifacts))
            {
                rdReviewContents = new RDReviewContents { Artifacts = new List<RDArtifact>() };
            }
            else
            {
                rdReviewContents = ReviewRawDataHelper.RestoreData<RDReviewContents>(xmlArtifacts);
            }

            var currentArtifactIds = rdReviewContents.Artifacts.Select(a => a.Id).ToList();

            foreach (var artifactToAdd in artifactsToAdd)
            {
                if (!currentArtifactIds.Contains(artifactToAdd))
                {
                    var addedArtifact = new RDArtifact
                    {
                        Id = artifactToAdd,
                        ApprovalNotRequested = true

                    };

                    rdReviewContents.Artifacts.Add(addedArtifact);
                }
                else
                {
                    ++alreadyIncluded;
                }
            }

            return ReviewRawDataHelper.GetStoreData(rdReviewContents);
        }

        public Task<QueryResult<ReviewedArtifact>> GetReviewedArtifacts(int reviewId, int userId, Pagination pagination, int revisionId)
        {
            return GetParticipantReviewedArtifactsAsync(reviewId, userId, userId, pagination, revisionId);
        }

        private async Task<QueryResult<ReviewedArtifact>> GetParticipantReviewedArtifactsAsync(int reviewId, int userId, int participantId, Pagination pagination, int revisionId = int.MaxValue, bool addDrafts = false)
        {
            var reviewArtifacts = await GetReviewArtifactsAsync<ReviewedArtifact>(reviewId, userId, pagination, revisionId, addDrafts);

            var reviewArtifactIds = reviewArtifacts.Items.Select(a => a.Id).ToList();

            var artifactIds = reviewArtifactIds.Union(new[] { reviewId });
            var artifactPermissionsDictionary = await _artifactPermissionsRepository.GetArtifactPermissions(artifactIds, userId);

            if (!SqlArtifactPermissionsRepository.HasPermissions(reviewId, artifactPermissionsDictionary, RolePermissions.Read))
            {
                throw ReviewsExceptionHelper.UserCannotAccessReviewException(reviewId);
            }

            var reviewedArtifacts = (await GetReviewArtifactsByParticipantAsync(reviewArtifactIds, participantId, reviewId, revisionId)).ToDictionary(k => k.Id);

            Dictionary<int, List<ReviewMeaningOfSignatureValue>> meaningOfSignatures;

            if (await IsMeaningOfSignatureEnabledAsync(reviewId, userId, addDrafts))
            {
                var mosList = await GetParticipantsMeaningOfSignatureValuesAsync(reviewArtifactIds, participantId, reviewId);

                meaningOfSignatures = mosList.GroupBy(mos => mos.Id).ToDictionary(mos => mos.Key, mos => mos.ToList());
            }
            else
            {
                meaningOfSignatures = new Dictionary<int, List<ReviewMeaningOfSignatureValue>>();
            }

            foreach (var artifact in reviewArtifacts.Items)
            {
                if (SqlArtifactPermissionsRepository.HasPermissions(artifact.Id, artifactPermissionsDictionary, RolePermissions.Read))
                {
                    ReviewedArtifact reviewedArtifact;

                    if (!reviewedArtifacts.TryGetValue(artifact.Id, out reviewedArtifact))
                    {
                        continue;
                    }

                    if (artifact.IsApprovalRequired)
                    {
                        artifact.Approval = GetApprovalStatus(reviewedArtifact, artifact.IsApprovalRequired);
                        artifact.ApprovalFlag = reviewedArtifact.ApprovalFlag;
                    }
                    else
                    {
                        artifact.Approval = null;
                        artifact.ApprovalFlag = 0;
                    }

                    artifact.ArtifactVersion = reviewedArtifact.ArtifactVersion;
                    artifact.PublishedOnTimestamp = reviewedArtifact.PublishedOnTimestamp;
                    artifact.UserDisplayName = reviewedArtifact.UserDisplayName;
                    artifact.ViewedArtifactVersion = reviewedArtifact.ViewState == ViewStateType.Viewed ? reviewedArtifact.ViewedArtifactVersion : 0;
                    artifact.SignedOnTimestamp = reviewedArtifact.SignedOnTimestamp;
                    artifact.HasAttachments = reviewedArtifact.HasAttachments;
                    artifact.HasRelationships = reviewedArtifact.HasRelationships;
                    artifact.HasAccess = true;
                    artifact.ViewState = reviewedArtifact.ViewState;

                    if (meaningOfSignatures.ContainsKey(artifact.Id))
                    {
                        artifact.MeaningOfSignatures = meaningOfSignatures[artifact.Id];
                    }
                    else
                    {
                        artifact.MeaningOfSignatures = new ReviewMeaningOfSignatureValue[0];
                    }
                }
                else
                {
                    artifact.MeaningOfSignatures = new ReviewMeaningOfSignatureValue[0];
                    ClearReviewArtifactProperties(artifact);
                }
            }

            return reviewArtifacts;
        }

        /// <summary>
        /// To replicate Silverlight behaviour, all 'Not Specified' labels will be converted to 'Pending'
        /// </summary>
        /// <param name="reviewedArtifact"></param>
        /// <param name="isApprovalRequired"></param>
        /// <returns></returns>
        private static string GetApprovalStatus(ReviewedArtifact reviewedArtifact, bool isApprovalRequired)
        {
            if (reviewedArtifact.ApprovalFlag == ApprovalType.NotSpecified
                && string.Compare(reviewedArtifact.Approval, NotSpecified, StringComparison.OrdinalIgnoreCase) == 0
                || isApprovalRequired && string.IsNullOrEmpty(reviewedArtifact.Approval))
            {
                return Pending;
            }

            return reviewedArtifact.Approval;
        }

        private async Task<IEnumerable<ReviewedArtifact>> GetReviewArtifactsByParticipantAsync(IEnumerable<int> artifactIds, int userId, int reviewId, int revisionId)
        {
            var param = new DynamicParameters();
            param.Add("@itemIds", SqlConnectionWrapper.ToDataTable(artifactIds));
            param.Add("@userId", userId);
            param.Add("@reviewId", reviewId);
            param.Add("@revisionId", revisionId);

            return await _connectionWrapper.QueryAsync<ReviewedArtifact>("GetReviewArtifactsByParticipant", param, commandType: CommandType.StoredProcedure);
        }

        private async Task<IEnumerable<ReviewMeaningOfSignatureValue>> GetParticipantsMeaningOfSignatureValuesAsync(IEnumerable<int> artifactIds, int userId, int reviewId)
        {
            var param = new DynamicParameters();
            param.Add("@itemIds", SqlConnectionWrapper.ToDataTable(artifactIds));
            param.Add("@userId", userId);
            param.Add("@reviewId", reviewId);

            return await _connectionWrapper.QueryAsync<ReviewMeaningOfSignatureValue>("GetParticipantsMeaningOfSignatureValues", param, commandType: CommandType.StoredProcedure);
        }

        private async Task<IEnumerable<ReviewMeaningOfSignatureValue>> GetReviewArtifactsMeaningOfSignatureValuesAsync(IEnumerable<int> participantIds, int artifactId, int reviewId)
        {
            var param = new DynamicParameters();
            param.Add("@participantIds", SqlConnectionWrapper.ToDataTable(participantIds));
            param.Add("@artifactId", artifactId);
            param.Add("@reviewId", reviewId);

            return await _connectionWrapper.QueryAsync<ReviewMeaningOfSignatureValue>("GetReviewArtifactsMeaningOfSignatureValues", param, commandType: CommandType.StoredProcedure);
        }

        private async Task<IEnumerable<int>> GetReviewArtifactsForApproveAsync(int reviewId, int userId, int? revisionId = null, bool? addDrafts = true)
        {
            var refreshInterval = await GetRebuildReviewArtifactHierarchyInterval();

            var parameters = new DynamicParameters();
            parameters.Add("@reviewId", reviewId);
            parameters.Add("@revisionId", revisionId);
            parameters.Add("@userId", userId);
            parameters.Add("@addDrafts", revisionId < int.MaxValue ? false : addDrafts);
            parameters.Add("@refreshInterval", refreshInterval);

            return (await _connectionWrapper.QueryAsync<int>("GetReviewArtifactsForApprove", parameters, commandType: CommandType.StoredProcedure)).ToList();
        }

        // Get all Review Artifacts for Summary Metrics
        private async Task<ReviewArtifactsQueryResult<T>> GetAllReviewArtifactsAsync<T>(int reviewId, int userId)
            where T : BaseReviewArtifact
        {
            var refreshInterval = await GetRebuildReviewArtifactHierarchyInterval();
            var parameters = new DynamicParameters();
            parameters.Add("@reviewId", reviewId);
            parameters.Add("@offset", 0);
            parameters.Add("@limit", int.MaxValue);
            parameters.Add("@revisionId", int.MaxValue);
            parameters.Add("@addDrafts", false);
            parameters.Add("@userId", userId);
            parameters.Add("@refreshInterval", refreshInterval);
            parameters.Add("@numResult", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@isFormal", dbType: DbType.Boolean, direction: ParameterDirection.Output);

            var result = await _connectionWrapper.QueryAsync<T>("GetReviewArtifacts", parameters, commandType: CommandType.StoredProcedure);

            return new ReviewArtifactsQueryResult<T>
            {
                Items = result.ToList(),
                Total = parameters.Get<int>("@numResult"),
                IsFormal = parameters.Get<bool>("@isFormal")
            };
        }

        private async Task<ReviewArtifactsQueryResult<T>> GetReviewArtifactsAsync<T>(int reviewId, int userId, Pagination pagination, int? revisionId = null, bool? addDrafts = true)
            where T : BaseReviewArtifact
        {
            var refreshInterval = await GetRebuildReviewArtifactHierarchyInterval();
            var parameters = new DynamicParameters();
            parameters.Add("@reviewId", reviewId);
            parameters.Add("@offset", pagination.Offset);
            parameters.Add("@limit", pagination.Limit);
            parameters.Add("@revisionId", revisionId);
            parameters.Add("@addDrafts", revisionId < int.MaxValue ? false : addDrafts);
            parameters.Add("@userId", userId);
            parameters.Add("@refreshInterval", refreshInterval);
            parameters.Add("@numResult", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@isFormal", dbType: DbType.Boolean, direction: ParameterDirection.Output);

            var result = await _connectionWrapper.QueryAsync<T>("GetReviewArtifacts", parameters, commandType: CommandType.StoredProcedure);

            return new ReviewArtifactsQueryResult<T>
            {
                Items = result.ToList(),
                Total = parameters.Get<int>("@numResult"),
                IsFormal = parameters.Get<bool>("@isFormal")
            };
        }

        private async Task<ReviewArtifactsQueryResult<T>> GetHistoricalReviewArtifactsAsync<T>(int reviewId, int userId, Pagination pagination, int? revisionId = null)
            where T : BaseReviewArtifact
        {
            var parameters = new DynamicParameters();
            parameters.Add("@reviewId", reviewId);
            parameters.Add("@revisionId", revisionId);
            parameters.Add("@userId", userId);
            parameters.Add("@offset", pagination.Offset);
            parameters.Add("@limit", pagination.Limit);
            parameters.Add("@numResult", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@isFormal", false, dbType: DbType.Boolean, direction: ParameterDirection.Output);

            var result = await _connectionWrapper.QueryAsync<T>("GetHistoricalReviewArtifacts", parameters, commandType: CommandType.StoredProcedure);

            return new ReviewArtifactsQueryResult<T>
            {
                Items = result.ToList(),
                Total = parameters.Get<int>("@numResult"),
                IsFormal = parameters.Get<bool>("@isFormal")
            };
        }

        public async Task<int> UpdateReviewArtifactsAsync(int reviewId, int userId, string xmlArtifacts, IDbTransaction transaction = null, bool addReviewSubArtifactIfNeeded = true)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@reviewId", reviewId);
            parameters.Add("@userId", userId);
            parameters.Add("@xmlArtifacts", xmlArtifacts);
            parameters.Add("@addReviewSubArtifactIfNeeded", addReviewSubArtifactIfNeeded);

            if (transaction == null)
            {
                return await _connectionWrapper.ExecuteAsync
                (
                    "UpdateReviewArtifacts",
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }

            return await transaction.Connection.ExecuteAsync
            (
                "UpdateReviewArtifacts",
                parameters,
                transaction,
                commandType: CommandType.StoredProcedure);
        }

        private async Task<int> GetRebuildReviewArtifactHierarchyInterval()
        {
            var refreshInterval = await _applicationSettingsRepository.GetValue(ReviewArtifactHierarchyRebuildIntervalInMinutesKey, DefaultReviewArtifactHierarchyRebuildIntervalInMinutes);
            if (refreshInterval < 0)
            {
                refreshInterval = DefaultReviewArtifactHierarchyRebuildIntervalInMinutes;
            }

            return refreshInterval;
        }

        private async Task<ContentStatusDetails> GetReviewArtifactStatusesAsync
        (
            int reviewId, int userId, Pagination pagination, int? versionId = null, bool? addDrafts = true, IEnumerable<int> reviewArtifactIds = null)
        {
            int? revisionId = await _itemInfoRepository.GetRevisionId(reviewId, userId, versionId);
            if (revisionId < int.MaxValue)
            {
                addDrafts = false;
            }

            var parameters = new DynamicParameters();
            parameters.Add("@reviewId", reviewId);
            parameters.Add("@offset", pagination.Offset);
            parameters.Add("@limit", pagination.Limit);
            parameters.Add("@revisionId", revisionId);
            parameters.Add("@addDrafts", addDrafts);
            parameters.Add("@userId", userId);
            parameters.Add("@itemIds", SqlConnectionWrapper.ToDataTable(reviewArtifactIds));

            var result = await _connectionWrapper.QueryMultipleAsync<ReviewArtifactStatus, int, int>("GetReviewArtifactsStatus", parameters, commandType: CommandType.StoredProcedure);

            return new ContentStatusDetails
            {
                ItemStatuses = result.Item1.ToList(),
                NumUsers = result.Item2.SingleOrDefault(),
                NumApprovers = result.Item3.SingleOrDefault()
            };
        }

        // Get all Review Participants for Summary Metrics
        public async Task<ReviewParticipantsContent> GetAllReviewParticipantsAsync(int reviewId, int userId)
        {
            int? revisionId = await _itemInfoRepository.GetRevisionId(reviewId, userId);
            var parameters = new DynamicParameters();

            parameters.Add("@reviewId", reviewId);
            parameters.Add("@offset", 0);
            parameters.Add("@limit", int.MaxValue);
            parameters.Add("@revisionId", revisionId);
            parameters.Add("@userId", userId);
            parameters.Add("@addDrafts", !(revisionId < int.MaxValue));

            var participants = await _connectionWrapper.QueryMultipleAsync<ReviewParticipant, int, int, int>("GetReviewParticipants", parameters, commandType: CommandType.StoredProcedure);

            var participantsContent = new ReviewParticipantsContent
            {
                Items = participants.Item1.ToList(),
                Total = participants.Item2.SingleOrDefault(),
                TotalArtifacts = participants.Item3.SingleOrDefault(),
                TotalArtifactsRequestedApproval = participants.Item4.SingleOrDefault()
            };

            return participantsContent;
        }
        public async Task<ReviewParticipantsContent> GetReviewParticipantsAsync(int reviewId, Pagination pagination, int userId, int? versionId = null, bool? addDrafts = true)
        {
            if (versionId < 1)
            {
                throw new BadRequestException(nameof(versionId) + " cannot be less than 1.", ErrorCodes.InvalidParameter);
            }

            int? revisionId = await _itemInfoRepository.GetRevisionId(reviewId, userId, versionId);

            if (revisionId < int.MaxValue)
            {
                addDrafts = false;
            }

            var parameters = new DynamicParameters();

            parameters.Add("@reviewId", reviewId);
            parameters.Add("@offset", pagination.Offset);
            parameters.Add("@limit", pagination.Limit);
            parameters.Add("@revisionId", revisionId);
            parameters.Add("@userId", userId);
            parameters.Add("@addDrafts", addDrafts);

            var participants = await _connectionWrapper.QueryMultipleAsync<ReviewParticipant, int, int, int>("GetReviewParticipants", parameters, commandType: CommandType.StoredProcedure);

            var participantsContent = new ReviewParticipantsContent
            {
                Items = participants.Item1.ToList(),
                Total = participants.Item2.SingleOrDefault(),
                TotalArtifacts = participants.Item3.SingleOrDefault(),
                TotalArtifactsRequestedApproval = participants.Item4.SingleOrDefault()
            };

            if (await IsMeaningOfSignatureEnabledAsync(reviewId, userId, true))
            {
                var approverIds = participantsContent.Items.Where(p => p.Role == ReviewParticipantRole.Approver).Select(r => r.UserId).ToList();

                var meaningOfSignatures = await GetMeaningOfSignaturesForParticipantsAsync(reviewId, userId, approverIds);
                var possibleMeaningOfSignatures = await GetPossibleMeaningOfSignaturesForParticipantsAsync(participantsContent.Items.Select(r => r.UserId));

                foreach (var reviewer in participantsContent.Items)
                {
                    if (meaningOfSignatures.ContainsKey(reviewer.UserId))
                    {
                        reviewer.MeaningOfSignatureIds = meaningOfSignatures[reviewer.UserId];
                    }
                    else
                    {
                        reviewer.MeaningOfSignatureIds = new int[0];
                    }

                    if (possibleMeaningOfSignatures.ContainsKey(reviewer.UserId))
                    {
                        reviewer.PossibleMeaningOfSignatures =
                            possibleMeaningOfSignatures[reviewer.UserId].Select(mos => new DropdownItem(mos.GetMeaningOfSignatureDisplayValue(), mos.RoleAssignmentId));
                    }
                    else
                    {
                        reviewer.PossibleMeaningOfSignatures = new DropdownItem[0];
                    }
                }
            }

            return participantsContent;
        }

        public async Task<bool> IsMeaningOfSignatureEnabledAsync(int reviewId, int userId, bool addDrafts)
        {
            var parameters = new DynamicParameters();

            parameters.Add("reviewId", reviewId);
            parameters.Add("userId", userId);
            parameters.Add("addDrafts", addDrafts);

            return await _connectionWrapper.ExecuteScalarAsync<bool>("GetReviewMeaningOfSignatureEnabled", parameters, commandType: CommandType.StoredProcedure);
        }

        private async Task<Dictionary<int, List<int>>> GetMeaningOfSignaturesForParticipantsAsync(int reviewId, int userId, IEnumerable<int> participantIds)
        {
            var parameters = new DynamicParameters();

            parameters.Add("reviewId", reviewId);
            parameters.Add("userId", userId);
            parameters.Add("participantIds", SqlConnectionWrapper.ToDataTable(participantIds));

            var result = await _connectionWrapper.QueryAsync<ParticipantMeaningOfSignatureResult>("GetParticipantsMeaningOfSignatures", parameters, commandType: CommandType.StoredProcedure);

            return result.GroupBy(mos => mos.ParticipantId, mos => mos.RoleAssignmentId).ToDictionary(grouping => grouping.Key, grouping => grouping.ToList());
        }

        public async Task<Dictionary<int, List<ParticipantMeaningOfSignatureResult>>> GetPossibleMeaningOfSignaturesForParticipantsAsync(IEnumerable<int> participantIds)
        {
            var parameters = new DynamicParameters();
            parameters.Add("participantIds", SqlConnectionWrapper.ToDataTable(participantIds));

            var result = await _connectionWrapper.QueryAsync<ParticipantMeaningOfSignatureResult>("GetPossibleMeaningOfSignaturesForParticipants", parameters, commandType: CommandType.StoredProcedure);

            return result.GroupBy(mos => mos.ParticipantId)
                         .ToDictionary(grouping => grouping.Key, grouping => grouping.ToList());
        }

        public async Task<QueryResult<ReviewArtifactDetails>> GetReviewArtifactStatusesByParticipant(int artifactId, int reviewId, Pagination pagination, int userId, int? versionId = null, bool? addDrafts = true)
        {
            var artifactPermissionsDictionary = await _artifactPermissionsRepository.GetArtifactPermissions(new[] { reviewId, artifactId }, userId);

            if (!SqlArtifactPermissionsRepository.HasPermissions(reviewId, artifactPermissionsDictionary, RolePermissions.Read))
            {
                throw ReviewsExceptionHelper.UserCannotAccessReviewException(reviewId);
            }

            if (!SqlArtifactPermissionsRepository.HasPermissions(artifactId, artifactPermissionsDictionary, RolePermissions.Read))
            {
                throw ExceptionHelper.ArtifactForbiddenException(artifactId);
            }

            int revisionId = await _itemInfoRepository.GetRevisionId(reviewId, userId, versionId);
            if (revisionId < int.MaxValue)
            {
                addDrafts = false;
            }

            var reviewParameters = new DynamicParameters();
            reviewParameters.Add("@reviewId", reviewId);
            var doesReviewExist = await _connectionWrapper.ExecuteScalarAsync<bool>("CheckIfReviewExists", reviewParameters, commandType: CommandType.StoredProcedure);

            if (!doesReviewExist)
            {
                throw ReviewsExceptionHelper.ReviewNotFoundException(reviewId, revisionId);
            }

            var artifactParameters = new DynamicParameters();
            artifactParameters.Add("@reviewId", reviewId);
            artifactParameters.Add("@userId", userId);

            var artifactXml = (await _connectionWrapper.QueryAsync<string>("GetReviewArtifactsXml", artifactParameters, commandType: CommandType.StoredProcedure)).SingleOrDefault();
            var rdReviewContents = ReviewRawDataHelper.RestoreData<RDReviewContents>(artifactXml);

            if (rdReviewContents.Artifacts.All(a => a.Id != artifactId))
            {
                throw new ResourceNotFoundException("Specified artifact is not found in the review", ErrorCodes.ResourceNotFound);
            }

            var parameters = new DynamicParameters();
            parameters.Add("@artifactId", artifactId);
            parameters.Add("@reviewId", reviewId);
            parameters.Add("@offset", pagination.Offset);
            parameters.Add("@limit", pagination.Limit);
            parameters.Add("@revisionId", revisionId);
            parameters.Add("@userId", userId);
            parameters.Add("@addDrafts", addDrafts);

            var participants = await _connectionWrapper.QueryMultipleAsync<ReviewArtifactDetails, int>("GetReviewArtifactStatusesByParticipant", parameters, commandType: CommandType.StoredProcedure);

            var result = new QueryResult<ReviewArtifactDetails>
            {
                Items = participants.Item1.ToList(),
                Total = participants.Item2.SingleOrDefault()
            };

            if (await IsMeaningOfSignatureEnabledAsync(reviewId, userId, true))
            {
                var mosList = await GetReviewArtifactsMeaningOfSignatureValuesAsync(result.Items.Select(p => p.UserId), artifactId, reviewId);

                var meaningOfSignatures = mosList.GroupBy(mos => mos.Id).ToDictionary(mos => mos.Key, mos => mos.ToList());

                foreach (var item in result.Items)
                {
                    if (meaningOfSignatures.ContainsKey(item.UserId))
                    {
                        item.MeaningOfSignature = meaningOfSignatures[item.UserId].Select(mos => mos.GetMeaningOfSignatureDisplayValue());
                    }
                    else
                    {
                        item.MeaningOfSignature = new string[0];
                    }
                }
            }
            else
            {
                result.Items.ForEach(r => r.MeaningOfSignature = new string[0]);
            }

            return result;
        }

        public async Task<AddParticipantsResult> AddParticipantsToReviewAsync(int reviewId, int userId, AddParticipantsParameter content)
        {
            // Check there is at least one user/group to add
            if ((content.GroupIds == null || !content.GroupIds.Any()) &&
               (content.UserIds == null || !content.UserIds.Any()))
            {
                throw new BadRequestException("No users were selected to be added.", ErrorCodes.OutOfRangeParameter);
            }

            var reviewXmlResult = await GetReviewXmlAsync(reviewId, userId);

            if (!reviewXmlResult.ReviewExists)
            {
                throw ReviewsExceptionHelper.ReviewNotFoundException(reviewId);
            }

            var reviewLockedByUser = await _artifactRepository.IsArtifactLockedByUserAsync(reviewId, userId);

            if (!reviewLockedByUser)
            {
                throw ExceptionHelper.ArtifactNotLockedException(reviewId, userId);
            }

            var reviewPackageRawData = reviewXmlResult.XmlString != null ?
                ReviewRawDataHelper.RestoreData<ReviewPackageRawData>(reviewXmlResult.XmlString) :
                new ReviewPackageRawData();

            if (reviewPackageRawData.Status == ReviewPackageStatus.Closed)
            {
                throw ReviewsExceptionHelper.ReviewClosedException();
            }

            var groupUserIds = await GetUsersFromGroupsAsync(content.GroupIds);

            var userIds = content.UserIds ?? new int[0];

            var deletedUserIds = (await _usersRepository.FindNonExistentUsersAsync(userIds)).ToList();

            // Flatten users into a single collection and remove duplicates and non existant users
            var uniqueParticipantsSet = new HashSet<int>(userIds.Except(deletedUserIds).Concat(groupUserIds));

            if (reviewPackageRawData.Reviewers == null)
            {
                reviewPackageRawData.Reviewers = new List<ReviewerRawData>();
            }

            var participantIdsToAdd = uniqueParticipantsSet.Except(reviewPackageRawData.Reviewers.Select(r => r.UserId)).ToList();

            var newParticipantsCount = participantIdsToAdd.Count;

            reviewPackageRawData.Reviewers.AddRange(participantIdsToAdd.Select(p => new ReviewerRawData
            {
                UserId = p,
                Permission = ReviewParticipantRole.Reviewer
            }));

            if (newParticipantsCount > 0)
            {
                // Save XML in the database
                var result = await UpdateReviewXmlAsync(reviewId, userId, ReviewRawDataHelper.GetStoreData(reviewPackageRawData));

                if (result != 1)
                {
                    throw new BadRequestException("Cannot add participants as project or review couldn't be found", ErrorCodes.ResourceNotFound);
                }
            }

            return new AddParticipantsResult
            {
                ParticipantCount = newParticipantsCount,
                AlreadyIncludedCount = uniqueParticipantsSet.Count - newParticipantsCount,
                NonExistentUsers = deletedUserIds.Count
            };
        }

        private async Task<ReviewXmlResult> GetReviewXmlAsync(int reviewId, int userId, int revisionId = int.MaxValue, bool includeDrafts = true)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@reviewId", reviewId);
            parameters.Add("@userId", userId);
            parameters.Add("@revisionId", revisionId);
            parameters.Add("@includeDrafts", revisionId == int.MaxValue && includeDrafts);

            var result = (await _connectionWrapper.QueryAsync<string>("GetReviewPackageRawData", parameters, commandType: CommandType.StoredProcedure)).ToList();

            return new ReviewXmlResult
            {
                ReviewExists = result.Count > 0,
                XmlString = result.SingleOrDefault()
            };
        }

        private async Task<int> UpdateReviewXmlAsync(int reviewId, int userId, string reviewXml, IDbTransaction transaction = null)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@reviewId", reviewId);
            parameters.Add("@userId", userId);
            parameters.Add("@xmlString", reviewXml);
            parameters.Add("@returnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

            if (transaction == null)
            {
                await _connectionWrapper.ExecuteAsync("UpdateReviewPackageRawData", parameters, commandType: CommandType.StoredProcedure);
            }
            else
            {
                await _connectionWrapper.ExecuteAsync("UpdateReviewPackageRawData", parameters, transaction, commandType: CommandType.StoredProcedure);
            }

            return parameters.Get<int>("@returnValue");
        }

        private async Task<IEnumerable<int>> GetUsersFromGroupsAsync(IEnumerable<int> groupIds)
        {
            if (groupIds == null || !groupIds.Any())
            {
                return new int[0];
            }

            // Get all users from all groups
            var groupUsers = await _usersRepository.GetUserInfosFromGroupsAsync(groupIds);

            return groupUsers.Select(gu => gu.UserId);
        }

        private async Task<QueryResult<ReviewTableOfContentItem>> GetTableOfContentAsync(int reviewId, int revisionId, int userId, Pagination pagination)
        {
            var refreshInterval = await GetRebuildReviewArtifactHierarchyInterval();

            var parameters = new DynamicParameters();
            parameters.Add("@reviewId", reviewId);
            parameters.Add("@offset", pagination.Offset);
            parameters.Add("@limit", pagination.Limit);
            parameters.Add("@revisionId", revisionId);
            parameters.Add("@userId", userId);
            parameters.Add("@refreshInterval", refreshInterval);
            parameters.Add("@total", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@retResult", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

            var result = await _connectionWrapper.QueryAsync<ReviewTableOfContentItem>("GetReviewTableOfContent", parameters, commandType: CommandType.StoredProcedure);
            var retResult = parameters.Get<int>("@retResult");

            // The review is not found or not active.
            if (retResult == 1 || retResult == 2)
            {
                throw ReviewsExceptionHelper.ReviewNotFoundException(reviewId, revisionId);
            }

            // The user is not a review participant.
            if (retResult == 3)
            {
                throw ReviewsExceptionHelper.UserCannotAccessReviewException(reviewId);
            }

            return new QueryResult<ReviewTableOfContentItem>
            {
                Items = result.ToList(),
                Total = parameters.Get<int>("@total")
            };
        }

        public async Task<QueryResult<ReviewTableOfContentItem>> GetReviewTableOfContent(int reviewId, int revisionId, int userId, Pagination pagination)
        {
            // get all review content item in a hierarchy list
            var toc = await GetTableOfContentAsync(reviewId, revisionId, userId, pagination);

            var artifactIds = new List<int> { reviewId }.Concat(toc.Items.Select(a => a.Id).ToList());

            // gets artifact permissions
            var artifactPermissionsDictionary = await _artifactPermissionsRepository.GetArtifactPermissions(artifactIds, userId);

            if (!SqlArtifactPermissionsRepository.HasPermissions(reviewId, artifactPermissionsDictionary, RolePermissions.Read))
            {
                throw ReviewsExceptionHelper.UserCannotAccessReviewException(reviewId);
            }

            var reviewedArtifacts = (await GetReviewArtifactsByParticipantAsync(toc.Items.Select(a => a.Id), userId, reviewId, revisionId)).ToList();

            // TODO: Update artifact statuses and permissions
            foreach (var tocItem in toc.Items)
            {
                if (SqlArtifactPermissionsRepository.HasPermissions(tocItem.Id, artifactPermissionsDictionary, RolePermissions.Read))
                {
                    // TODO update item status
                    tocItem.HasAccess = true;

                    var artifact = reviewedArtifacts.First(it => it.Id == tocItem.Id);
                    tocItem.ArtifactVersion = artifact.ArtifactVersion;
                    tocItem.ApprovalStatus = artifact.ApprovalFlag;
                    tocItem.ViewedArtifactVersion = artifact.ViewState == ViewStateType.Viewed ? artifact.ViewedArtifactVersion : 0;
                }
                else
                {
                    // not granted SES
                    // TODO: http://svmtfs2015:8080/tfs/svmtfs2015/Blueprint/_workitems?_a=edit&id=6593&fullScreen=false
                    UnauthorizedItem(tocItem);
                }
            }

            return toc;
        }

        public async Task RemoveParticipantsFromReviewAsync(int reviewId, ReviewItemsRemovalParams removeParams, int userId)
        {
            if ((removeParams.ItemIds == null || !removeParams.ItemIds.Any()) && removeParams.SelectionType == SelectionType.Selected)
            {
                throw new BadRequestException("Incorrect input parameters", ErrorCodes.OutOfRangeParameter);
            }

            var reviewXmlResult = await GetReviewXmlAsync(reviewId, userId);

            if (!reviewXmlResult.ReviewExists)
            {
                throw ReviewsExceptionHelper.ReviewNotFoundException(reviewId);
            }

            var reviewLockedByUser = await _artifactRepository.IsArtifactLockedByUserAsync(reviewId, userId);

            if (!reviewLockedByUser)
            {
                throw ExceptionHelper.ArtifactNotLockedException(reviewId, userId);
            }

            if (string.IsNullOrEmpty(reviewXmlResult.XmlString))
            {
                throw ExceptionHelper.ArtifactDoesNotSupportOperation(reviewId);
            }

            var reviewPackageRawData = ReviewRawDataHelper.RestoreData<ReviewPackageRawData>(reviewXmlResult.XmlString);

            if (reviewPackageRawData.Status == ReviewPackageStatus.Closed)
            {
                throw ReviewsExceptionHelper.ReviewClosedException();
            }

            if (removeParams.SelectionType == SelectionType.Selected)
            {
                reviewPackageRawData.Reviewers.RemoveAll(i => removeParams.ItemIds.Contains(i.UserId));
            }
            else
            {
                if (removeParams.ItemIds != null && removeParams.ItemIds.Any())
                {
                    reviewPackageRawData.Reviewers.RemoveAll(i => !removeParams.ItemIds.Contains(i.UserId));
                }
                else
                {
                    reviewPackageRawData.Reviewers = new List<ReviewerRawData>();
                }
            }

            var participantXmlResult = ReviewRawDataHelper.GetStoreData(reviewPackageRawData);

            // Save XML in the database
            var result = await UpdateReviewXmlAsync(reviewId, userId, participantXmlResult);

            if (result != 1)
            {
                throw new BadRequestException("Cannot add participants as project or review couldn't be found", ErrorCodes.ResourceNotFound);
            }
        }

        public async Task<ReviewPackageRawData> GetReviewPackageRawDataAsync(int reviewId, int userId, int revisionId = int.MaxValue)
        {
            var reviewXml = await GetReviewXmlAsync(reviewId, userId, revisionId);

            if (!reviewXml.ReviewExists)
            {
                throw ReviewsExceptionHelper.ReviewNotFoundException(reviewId, revisionId);
            }

            return ReviewRawDataHelper.RestoreData<ReviewPackageRawData>(reviewXml.XmlString);
        }

        public async Task UpdateReviewPackageRawDataAsync(int reviewId, ReviewPackageRawData reviewPackageRawData, int userId)
        {
            var reviewXml = ReviewRawDataHelper.GetStoreData(reviewPackageRawData);

            await UpdateReviewXmlAsync(reviewId, userId, reviewXml);
            await UpdateReviewLastSaveInvalidAsync(reviewId, userId);
        }

        public async Task<int> UpdateReviewLastSaveInvalidAsync(int reviewId, int userId, IDbTransaction transaction = null)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@reviewId", reviewId, DbType.Int32);
            parameters.Add("@userId", userId, DbType.Int32);

            if (transaction == null)
            {
                return await _connectionWrapper.ExecuteAsync(
                    "UpdateReviewLastSaveInvalid",
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }

            return await _connectionWrapper.ExecuteAsync(
                "UpdateReviewLastSaveInvalid",
                parameters,
                transaction,
                commandType: CommandType.StoredProcedure);
        }

        public async Task RemoveArtifactsFromReviewAsync(int reviewId, ReviewItemsRemovalParams removeParams, int userId)
        {
            if ((removeParams.ItemIds == null || !removeParams.ItemIds.Any()) && removeParams.SelectionType == SelectionType.Selected)
            {
                throw new BadRequestException("Incorrect input parameters", ErrorCodes.OutOfRangeParameter);
            }

            var propertyResult = await GetReviewPropertyStringAsync(reviewId, userId);

            if (propertyResult.ReviewStatus == ReviewPackageStatus.Closed)
            {
                throw ReviewsExceptionHelper.ReviewClosedException();
            }

            if (propertyResult.BaselineId != null && propertyResult.BaselineId.Value > 0)
            {
                throw new BadRequestException("Review status changed", ErrorCodes.ReviewStatusChanged);
            }

            if (propertyResult.ProjectId == null || propertyResult.ProjectId < 1)
            {
                throw ReviewsExceptionHelper.ReviewNotFoundException(reviewId);
            }

            if (propertyResult.LockedByUserId.GetValueOrDefault() != userId)
            {
                throw ExceptionHelper.ArtifactNotLockedException(reviewId, userId);
            }

            if (string.IsNullOrEmpty(propertyResult.ArtifactXml))
            {
                throw ExceptionHelper.ArtifactDoesNotSupportOperation(reviewId);
            }

            var rdReviewContents = ReviewRawDataHelper.RestoreData<RDReviewContents>(propertyResult.ArtifactXml);

            if (removeParams.SelectionType == SelectionType.Selected)
            {
                rdReviewContents.Artifacts.RemoveAll(i => removeParams.ItemIds.Contains(i.Id));
            }
            else
            {
                if (removeParams.ItemIds != null && removeParams.ItemIds.Any())
                {
                    rdReviewContents.Artifacts.RemoveAll(i => !removeParams.ItemIds.Contains(i.Id));
                }
                else
                {
                    rdReviewContents.Artifacts = new List<RDArtifact>();
                }
            }

            var artifactXmlResult = ReviewRawDataHelper.GetStoreData(rdReviewContents);

            await UpdateReviewArtifactsAsync(reviewId, userId, artifactXmlResult);
        }

        public async Task<IEnumerable<ReviewInfo>> GetReviewInfo(ISet<int> artifactIds, int userId, bool addDrafts = true, int revisionId = int.MaxValue)
        {
            var artifactsPermissions = await _artifactPermissionsRepository.GetArtifactPermissions(artifactIds, userId);
            var artifactsWithReadPermissions = artifactsPermissions.Where(p => p.Value.HasFlag(RolePermissions.Read)).Select(p => p.Key);
            var itemsRawData = await _itemInfoRepository.GetItemsRawDataCreatedDate(userId, artifactsWithReadPermissions, addDrafts, revisionId);

            var result = new List<ReviewInfo>();
            foreach (var rawDataEntry in itemsRawData)
            {
                var rawDataString = rawDataEntry.RawData;
                ReviewPackageRawData rawData;
                var reviewInfo = new ReviewInfo
                {
                    ItemId = rawDataEntry.ItemId
                };

                if (ReviewRawDataHelper.TryRestoreData(rawDataString, out rawData))
                {
                    reviewInfo.ReviewStatus = rawData.Status;
                    reviewInfo.ExpiryTimestamp = rawData.EndDate;
                    reviewInfo.IsFormal = rawData.Status == ReviewPackageStatus.Active && HasAtLeastOneApprover(rawData.Reviewers);
                }
                result.Add(reviewInfo);
            }

            return result;
        }

        private bool HasAtLeastOneApprover(IEnumerable<ReviewerRawData> reviewers)
        {
            return reviewers != null && reviewers.Any(r => r.Permission == ReviewParticipantRole.Approver);
        }

        public async Task<ReviewArtifactIndex> GetReviewArtifactIndexAsync(int reviewId, int revisionId, int artifactId, int userId, bool? addDrafts = true)
        {
            var reviewInfo = await _artifactVersionsRepository.GetVersionControlArtifactInfoAsync(reviewId, null, userId);

            if (reviewInfo.VersionCount == 0) // never published review
            {
                throw ReviewsExceptionHelper.ReviewNotFoundException(reviewId, revisionId);
            }

            var refreshInterval = await GetRebuildReviewArtifactHierarchyInterval();

            var parameters = new DynamicParameters();
            parameters.Add("@reviewId", reviewId);
            parameters.Add("@revisionId", revisionId);
            parameters.Add("@userId", userId);
            parameters.Add("@artifactId", artifactId);
            parameters.Add("@addDrafts", revisionId < int.MaxValue ? false : addDrafts);
            parameters.Add("@refreshInterval", refreshInterval);
            parameters.Add("@result", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

            var result = (await _connectionWrapper.QueryAsync<ReviewArtifactIndex>("GetReviewArtifactIndex", parameters, commandType: CommandType.StoredProcedure)).SingleOrDefault();
            var resultValue = parameters.Get<int>("@result");

            if (resultValue == 1 || resultValue == 2)
            {
                throw ReviewsExceptionHelper.ReviewNotFoundException(reviewId, revisionId);
            }

            if (resultValue == 3)
            {
                throw ReviewsExceptionHelper.UserCannotAccessReviewException(reviewId);
            }

            if (result == null)
            {
                throw new ResourceNotFoundException("Specified artifact is not found in the review", ErrorCodes.ResourceNotFound);
            }

            return result;
        }

        public async Task<ReviewArtifactIndex> GetReviewTableOfContentArtifactIndexAsync(int reviewId, int revisionId, int artifactId, int userId)
        {
            var reviewInfo = await _artifactVersionsRepository.GetVersionControlArtifactInfoAsync(reviewId, null, userId);

            if (reviewInfo.VersionCount == 0) // never published review
            {
                throw ReviewsExceptionHelper.ReviewNotFoundException(reviewId, revisionId);
            }

            var refreshInterval = await GetRebuildReviewArtifactHierarchyInterval();

            var parameters = new DynamicParameters();
            parameters.Add("@reviewId", reviewId);
            parameters.Add("@revisionId", revisionId);
            parameters.Add("@userId", userId);
            parameters.Add("@artifactId", artifactId);
            parameters.Add("@refreshInterval", refreshInterval);
            parameters.Add("@result", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

            var result = (await _connectionWrapper.QueryAsync<ReviewArtifactIndex>("GetReviewTableOfContentArtifactIndex", parameters, commandType: CommandType.StoredProcedure)).SingleOrDefault();
            var resultValue = parameters.Get<int>("@result");

            if (resultValue == 1 || resultValue == 2)
            {
                throw ReviewsExceptionHelper.ReviewNotFoundException(reviewId, revisionId);
            }

            if (resultValue == 3)
            {
                throw ReviewsExceptionHelper.UserCannotAccessReviewException(reviewId);
            }

            if (result == null)
            {
                throw new ResourceNotFoundException("Specified artifact is not found in the review", ErrorCodes.ResourceNotFound);
            }

            return result;
        }

        public async Task<ReviewArtifactApprovalResult> UpdateReviewArtifactApprovalAsync(int reviewId, ReviewArtifactApprovalParameter reviewArtifactApprovalParameters, int userId)
        {
            if
            (
                reviewArtifactApprovalParameters?.ArtifactIds == null ||
                reviewArtifactApprovalParameters.SelectionType == SelectionType.Selected &&
                !reviewArtifactApprovalParameters.ArtifactIds.Any())
            {
                throw new BadRequestException("Bad parameters.", ErrorCodes.OutOfRangeParameter);
            }

            if (reviewArtifactApprovalParameters.SelectionType == SelectionType.Excluded && reviewArtifactApprovalParameters.RevisionId == null)
            {
                throw new BadRequestException("Not all parameters provided.", ErrorCodes.OutOfRangeParameter);
            }

            var artifactIds = new List<int>();

            if (reviewArtifactApprovalParameters.SelectionType == SelectionType.Excluded)
            {
                artifactIds.AddRange(await GetReviewArtifactsForApproveAsync(reviewId, userId, reviewArtifactApprovalParameters.RevisionId.Value, false));

                if (reviewArtifactApprovalParameters.ArtifactIds != null && reviewArtifactApprovalParameters.ArtifactIds.Any())
                {
                    artifactIds.RemoveAll(a => reviewArtifactApprovalParameters.ArtifactIds.Contains(a));
                }
            }
            else
            {
                artifactIds.AddRange(reviewArtifactApprovalParameters.ArtifactIds);
            }

            var eligibleArtifacts = await CheckApprovalsAndPermissions(reviewId, userId, artifactIds);
            var isAllArtifactsProcessed = eligibleArtifacts.Count == artifactIds.Count;

            var rdReviewedArtifacts = await GetReviewUserStatsXmlAsync(reviewId, userId);
            var artifactVersionDictionary = await GetVersionNumberForArtifacts(reviewId, eligibleArtifacts);
            var timestamp = _currentDateTimeService.GetUtcNow();
            var approvedArtifacts = new List<ArtifactApprovalResult>();

            // Update approvals for the specified artifacts
            foreach (var id in eligibleArtifacts)
            {
                var reviewArtifactApproval = rdReviewedArtifacts.ReviewedArtifacts.FirstOrDefault(ra => ra.ArtifactId == id);

                if (reviewArtifactApproval == null)
                {
                    reviewArtifactApproval = new ReviewArtifactXml
                    {
                        ArtifactId = id
                    };

                    rdReviewedArtifacts.ReviewedArtifacts.Add(reviewArtifactApproval);
                }

                if (reviewArtifactApproval.ViewState == ViewStateType.NotViewed)
                {
                    reviewArtifactApproval.ViewState = ViewStateType.Viewed;
                }

                if (reviewArtifactApprovalParameters.ApprovalFlag == ApprovalType.NotSpecified &&
                    reviewArtifactApprovalParameters.Approval.Equals(Pending, StringComparison.InvariantCultureIgnoreCase))
                {
                    reviewArtifactApproval.ESignedOn = null;
                }
                else if (reviewArtifactApproval.Approval != reviewArtifactApprovalParameters.Approval
                         || reviewArtifactApproval.ApprovalFlag != reviewArtifactApprovalParameters.ApprovalFlag)
                {
                    reviewArtifactApproval.ESignedOn = timestamp;
                }

                var prevFlag = reviewArtifactApproval.ApprovalFlag;
                reviewArtifactApproval.Approval = reviewArtifactApprovalParameters.Approval;
                reviewArtifactApproval.ApprovalFlag = reviewArtifactApprovalParameters.ApprovalFlag;

                if (artifactVersionDictionary.ContainsKey(id))
                {
                    reviewArtifactApproval.ArtifactVersion = artifactVersionDictionary[id];
                }

                approvedArtifacts.Add(new ArtifactApprovalResult
                {
                    ArtifactId = reviewArtifactApproval.ArtifactId,
                    Timestamp = reviewArtifactApproval.ESignedOn,
                    PreviousApprovalFlag = prevFlag
                });
            }

            var approvalResult = new ReviewArtifactApprovalResult
            {
                IsAllArtifactsProcessed = isAllArtifactsProcessed,
                ApprovedArtifacts = approvedArtifacts
            };

            await UpdateReviewUserStatsXmlAsync(reviewId, userId, rdReviewedArtifacts);

            return approvalResult;
        }

        private async Task<List<int>> CheckApprovalsAndPermissions(int reviewId, int userId, IEnumerable<int> artifactIds)
        {
            var approvalCheck = await CheckReviewArtifactsUserApprovalAsync(reviewId, userId, artifactIds);

            CheckReviewStatsCanBeUpdated(approvalCheck.ReviewApprovalCheck, reviewId, true, true);

            if (approvalCheck.ReviewApprovalCheck.ReviewerStatus != ReviewStatus.InProgress)
            {
                throw new ConflictException("Cannot update approval status, the review is not in progress.");
            }

            if (approvalCheck.ReviewApprovalCheck.ReviewStatus != ReviewPackageStatus.Active)
            {
                throw new ConflictException("Cannot update approval status, the review is not active.");
            }

            if (!approvalCheck.ReviewApprovalCheck.AllArtifactsRequireApproval && (approvalCheck.ValidArtifactIds == null || !approvalCheck.ValidArtifactIds.Any()))
            {
                throw new BadRequestException("Not all artifacts require approval.");
            }

            if (approvalCheck.ReviewApprovalCheck.ReviewerRole != ReviewParticipantRole.Approver)
            {
                throw new ConflictException("Cannot update approval status, participant's role is invalid.");
            }

            if (!approvalCheck.ValidArtifactIds.Any())
            {
                throw new BadRequestException("The status cannot be updated for the requested artifacts.");
            }

            // Check user has permission for the review and all of the artifact ids
            return await CheckPermissionAndRemoveElligibleArtifacts(userId, reviewId, approvalCheck.ValidArtifactIds);
        }

        private async Task<List<int>> CheckPermissionAndRemoveElligibleArtifacts(int userId, int reviewId, IEnumerable<int> artifactIds)
        {
            var artifactIdsList = artifactIds.ToList();

            artifactIdsList.Add(reviewId);

            var artifactPermissionsDictionary = await _artifactPermissionsRepository.GetArtifactPermissions(artifactIdsList, userId);

            if (!SqlArtifactPermissionsRepository.HasPermissions(reviewId, artifactPermissionsDictionary, RolePermissions.Read))
            {
                throw ReviewsExceptionHelper.UserCannotAccessReviewException(reviewId);
            }

            artifactIdsList.RemoveAll(artifactId => !SqlArtifactPermissionsRepository.HasPermissions(artifactId, artifactPermissionsDictionary, RolePermissions.Read));
            artifactIdsList.Remove(reviewId);

            if (!artifactIdsList.Any())
            {
                throw new AuthorizationException("Artifacts could not be updated because they are no longer accessible.", ErrorCodes.UnauthorizedAccess);
            }

            return artifactIdsList;
        }

        public async Task UpdateReviewArtifactsViewedAsync(int reviewId, ReviewArtifactViewedInput viewedInput, int userId)
        {
            var artifactIds = viewedInput.ArtifactIds.ToList();

            var approvalCheck = await CheckReviewArtifactApprovalAsync(reviewId, userId, artifactIds);

            CheckReviewStatsCanBeUpdated(approvalCheck, reviewId, false);

            if (approvalCheck.ReviewerStatus == ReviewStatus.Completed)
            {
                throw new BadRequestException("Cannot update view status, reviewer has completed the review.");
            }

            var permissionIds = new List<int>(artifactIds)
            {
                reviewId
            };

            // Check user has permission for the review and all of the artifact ids
            var artifactPermissionsDictionary = await _artifactPermissionsRepository.GetArtifactPermissions(permissionIds, userId);

            if (!SqlArtifactPermissionsRepository.HasPermissions(reviewId, artifactPermissionsDictionary, RolePermissions.Read))
            {
                throw ReviewsExceptionHelper.UserCannotAccessReviewException(reviewId);
            }

            Func<IDbTransaction, Task> transactionAction = async transaction =>
            {
                var rdReviewedArtifacts = await GetReviewUserStatsXmlAsync(reviewId, userId, transaction);

                var artifactVersionDictionary = await GetVersionNumberForArtifacts(reviewId, artifactIds, transaction);

                foreach (var artifactId in artifactIds)
                {
                    var reviewedArtifact = rdReviewedArtifacts.ReviewedArtifacts.FirstOrDefault(ra => ra.ArtifactId == artifactId);

                    if (!SqlArtifactPermissionsRepository.HasPermissions(artifactId, artifactPermissionsDictionary, RolePermissions.Read))
                    {
                        continue;
                    }

                    if (reviewedArtifact == null)
                    {
                        reviewedArtifact = new ReviewArtifactXml
                        {
                            ArtifactId = artifactId
                        };

                        rdReviewedArtifacts.ReviewedArtifacts.Add(reviewedArtifact);
                    }

                    if (viewedInput.Viewed.Value)
                    {
                        reviewedArtifact.ViewState = ViewStateType.Viewed;
                        reviewedArtifact.ArtifactVersion = artifactVersionDictionary[artifactId];
                    }
                    else
                    {
                        reviewedArtifact.ViewState = ViewStateType.NotViewed;
                        reviewedArtifact.ArtifactVersion = 0;
                    }
                }

                await UpdateReviewUserStatsXmlAsync(reviewId, userId, rdReviewedArtifacts, transaction);
            };

            await _sqlHelper.RunInTransactionAsync(ServiceConstants.RaptorMain, transactionAction);
        }

        public async Task UpdateReviewerStatusAsync(int reviewId, int revisionId, ReviewStatus reviewStatus, int userId)
        {
            if (reviewStatus == ReviewStatus.NotStarted)
            {
                throw new BadRequestException("Cannot set reviewer status to not started");
            }

            var approvalCheck = await CheckReviewArtifactApprovalAsync(reviewId, userId, new int[0]);

            CheckReviewStatsCanBeUpdated(approvalCheck, reviewId, true);

            var artifactPermissionsDictionary = await _artifactPermissionsRepository.GetArtifactPermissions(new[] { reviewId }, userId);

            if (!SqlArtifactPermissionsRepository.HasPermissions(reviewId, artifactPermissionsDictionary, RolePermissions.Read))
            {
                throw ReviewsExceptionHelper.UserCannotAccessReviewException(reviewId);
            }

            switch (reviewStatus)
            {
                case ReviewStatus.InProgress:
                    await UpdateReviewerStatusToInProgressAsync(reviewId, userId);
                    break;

                case ReviewStatus.Completed:
                    await UpdateReviewerStatusToCompletedAsync(reviewId, revisionId, userId, approvalCheck);
                    break;

                default:
                    throw new BadRequestException("Cannot set reviewer status to this unknown reviewer status");
            }
        }

        private async Task UpdateReviewerStatusToInProgressAsync(int reviewId, int userId)
        {
            await UpdateReviewUserStatsAsync(reviewId, userId, true, ReviewStatus.InProgress.ToString());
        }

        private async Task UpdateReviewerStatusToCompletedAsync(int reviewId, int revisionId, int userId, ReviewArtifactApprovalCheck approvalCheck)
        {
            if (approvalCheck.ReviewerStatus == ReviewStatus.NotStarted)
            {
                throw new BadRequestException("Cannot set reviewer status to complete when reviewer status is not started.");
            }

            var requireAllReviewed = await GetRequireAllArtifactsReviewedAsync(reviewId, userId, false);

            if (requireAllReviewed)
            {
                var reviewedArtifactsResult = await GetParticipantReviewedArtifactsAsync(reviewId, userId, userId, new Pagination { Offset = 0, Limit = int.MaxValue }, revisionId);

                if (reviewedArtifactsResult.Items.Any(artifact => !IsArtifactReviewed(artifact, approvalCheck.ReviewerRole)))
                {
                    throw new ConflictException("Review cannot be completed until all artifacts have been reviewed.", ErrorCodes.NotAllArtifactsReviewed);
                }
            }

            await UpdateReviewUserStatsAsync(reviewId, userId, true, ReviewStatus.Completed.ToString());
        }

        private static bool IsArtifactReviewed(ReviewedArtifact artifact, ReviewParticipantRole reviewerRole)
        {
            if (!artifact.HasAccess)
            {
                return true;
            }

            if (reviewerRole == ReviewParticipantRole.Reviewer)
            {
                return artifact.ViewState == ViewStateType.Viewed && artifact.ViewedArtifactVersion == artifact.ArtifactVersion;
            }

            return !artifact.IsApprovalRequired
                   || artifact.ApprovalFlag != ApprovalType.NotSpecified;
        }

        private Task<bool> GetRequireAllArtifactsReviewedAsync(int reviewId, int userId, bool addDrafts = true)
        {
            var parameters = new DynamicParameters();

            parameters.Add("reviewId", reviewId);
            parameters.Add("userId", userId);
            parameters.Add("addDrafts", addDrafts);

            return _connectionWrapper.ExecuteScalarAsync<bool>("GetReviewRequireAllArtifactsReviewed", parameters, commandType: CommandType.StoredProcedure);
        }

        private void CheckReviewStatsCanBeUpdated(ReviewArtifactApprovalCheck approvalCheck, int reviewId, bool requireUserInReview, bool byPassArtifacts = false)
        {
            // Check the review exists and is active
            if (!approvalCheck.ReviewExists
               || approvalCheck.ReviewStatus == ReviewPackageStatus.Draft
               || approvalCheck.ReviewDeleted)
            {
                throw ReviewsExceptionHelper.ReviewNotFoundException(reviewId);
            }

            if (approvalCheck.ReviewStatus == ReviewPackageStatus.Closed)
            {
                throw ReviewsExceptionHelper.ReviewClosedException();
            }

            if (approvalCheck.ExpirationDate < _currentDateTimeService.GetUtcNow())
            {
                throw ReviewsExceptionHelper.ReviewExpiredException();
            }

            // Check user is an approver for the review
            if ((requireUserInReview && !approvalCheck.UserInReview)
                || (!requireUserInReview && approvalCheck.ReviewType != ReviewType.Public && !approvalCheck.UserInReview))
            {
                throw new AuthorizationException("User is not assigned for review", ErrorCodes.UserNotInReview);
            }

            // Check artifacts are part of the review and require approval
            if (!approvalCheck.AllArtifactsInReview && !byPassArtifacts)
            {
                throw new BadRequestException("Artifact is not a part of this review.", ErrorCodes.ArtifactNotFound);
            }
        }

        private async Task<ReviewApprovalCheckArtifacts> CheckReviewArtifactsUserApprovalAsync(int reviewId, int userId, IEnumerable<int> artifactIds)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@reviewId", reviewId);
            parameters.Add("@userId", userId);
            parameters.Add("@artifactIds", SqlConnectionWrapper.ToDataTable(artifactIds));
            var result = await _connectionWrapper.QueryMultipleAsync<ReviewArtifactApprovalCheck, int>("CheckReviewArtifactUserApproval", parameters, commandType: CommandType.StoredProcedure);

            return new ReviewApprovalCheckArtifacts
            {
                ReviewApprovalCheck = result.Item1.SingleOrDefault(),

                ValidArtifactIds = result.Item2.ToList()
            };
        }

        private async Task<ReviewArtifactApprovalCheck> CheckReviewArtifactApprovalAsync(int reviewId, int userId, IEnumerable<int> artifactIds)
        {
            return (await CheckReviewArtifactsUserApprovalAsync(reviewId, userId, artifactIds)).ReviewApprovalCheck;
        }

        private async Task<IDictionary<int, int>> GetVersionNumberForArtifacts(int reviewId, IEnumerable<int> artifactIds, IDbTransaction transaction = null)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@reviewId", reviewId);
            parameters.Add("@artifactIds", SqlConnectionWrapper.ToDataTable(artifactIds));

            IEnumerable<ReviewArtifactVersionNumber> artifactVersionNumbers;

            if (transaction == null)
            {
                artifactVersionNumbers = await _connectionWrapper.QueryAsync<ReviewArtifactVersionNumber>
                (
                    "GetReviewArtifactVersionNumber",
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            else
            {
                artifactVersionNumbers = await transaction.Connection.QueryAsync<ReviewArtifactVersionNumber>
                (
                    "GetReviewArtifactVersionNumber",
                    parameters,
                    transaction,
                    commandType: CommandType.StoredProcedure);
            }

            return artifactVersionNumbers.ToDictionary(avn => avn.ArtifactId, avn => avn.VersionNumber);
        }

        private async Task<RDReviewedArtifacts> GetReviewUserStatsXmlAsync(int reviewId, int userId, IDbTransaction transaction = null)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@reviewId", reviewId);
            parameters.Add("@userId", userId);

            string xmlString;

            if (transaction == null)
            {
                xmlString =
                (
                    await _connectionWrapper.QueryAsync<string>
                    (
                        "GetReviewUserStatsXml",
                        parameters,
                        commandType: CommandType.StoredProcedure))
                .SingleOrDefault();
            }
            else
            {
                xmlString = await transaction.Connection.QuerySingleOrDefaultAsync<string>
                (
                    "GetReviewUserStatsXml",
                    parameters,
                    transaction,
                    commandType: CommandType.StoredProcedure);
            }

            var rdReviewedArtifacts = xmlString != null ?
                ReviewRawDataHelper.RestoreData<RDReviewedArtifacts>(xmlString) :
                new RDReviewedArtifacts();

            if (rdReviewedArtifacts.ReviewedArtifacts == null)
            {
                rdReviewedArtifacts.ReviewedArtifacts = new List<ReviewArtifactXml>();
            }

            return rdReviewedArtifacts;
        }

        private Task UpdateReviewUserStatsXmlAsync(int reviewId, int userId, RDReviewedArtifacts rdReviewedArtifacts, IDbTransaction transaction = null)
        {
            var xmlString = ReviewRawDataHelper.GetStoreData(rdReviewedArtifacts);

            return UpdateReviewUserStatsAsync(reviewId, userId, false, xmlString, transaction);
        }

        private Task UpdateReviewUserStatsAsync(int reviewId, int userId, bool updateReviewerStatus, string value, IDbTransaction transaction = null)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@reviewId", reviewId);
            parameters.Add("@userId", userId);
            parameters.Add("@updateReviewerStatus", updateReviewerStatus);
            parameters.Add("@value", value);

            Task resultTask;

            if (transaction == null)
            {
                resultTask = _connectionWrapper.ExecuteAsync
                (
                    "UpdateReviewUserStats",
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            else
            {
                resultTask = transaction.Connection.ExecuteAsync
                (
                    "UpdateReviewUserStats",
                    parameters,
                    transaction,
                    commandType: CommandType.StoredProcedure);
            }

            return resultTask;
        }

        private async Task<DateTime> GetReviewCloseDateAsync(int reviewId)
        {
            var param = new DynamicParameters();
            param.Add("@reviewId", reviewId);

            var closedDate = await _connectionWrapper.ExecuteScalarAsync<DateTime>("GetReviewCloseDateTime", param, commandType: CommandType.StoredProcedure);
            return DateTime.SpecifyKind(closedDate, DateTimeKind.Utc);
        }

        public async Task<QueryResult<ParticipantArtifactStats>> GetReviewParticipantArtifactStatsAsync(int reviewId, int participantId, int userId, Pagination pagination)
        {
            var reviewInfo = await _artifactVersionsRepository.GetVersionControlArtifactInfoAsync(reviewId, null, userId);

            if (reviewInfo.VersionCount == 0 || reviewInfo.IsDeleted) // Review never published or deleted
            {
                throw ReviewsExceptionHelper.ReviewNotFoundException(reviewId);
            }

            pagination.SetDefaultValues(0, 50);

            var reviewedArtifactResult = await GetParticipantReviewedArtifactsAsync(reviewId, userId, participantId, pagination, addDrafts: true);

            return new QueryResult<ParticipantArtifactStats>
            {
                Total = reviewedArtifactResult.Total,
                Items = reviewedArtifactResult.Items.Select(ra => (ParticipantArtifactStats)ra)
            };
        }

        private static void UnauthorizedItem(ReviewTableOfContentItem item)
        {
            item.Name = Unauthorized; // unauthorize
            item.Included = false;
            item.HasAccess = false;
            item.IsApprovalRequired = false;
        }
    }
}
