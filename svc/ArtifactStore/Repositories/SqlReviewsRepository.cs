using ArtifactStore.Helpers;
using ArtifactStore.Models.Review;
using Dapper;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using ServiceLibrary.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Repositories.ApplicationSettings;

namespace ArtifactStore.Repositories
{
    public class SqlReviewsRepository : IReviewsRepository
    {
        private const string NOT_SPECIFIED = "Not Specified";
        private const string PENDING = "Pending";
        private const string UNATHORIZED = "Unauthorized";

        private readonly ISqlConnectionWrapper _connectionWrapper;
        private readonly IArtifactVersionsRepository _artifactVersionsRepository;
        private readonly ISqlItemInfoRepository _itemInfoRepository;
        private readonly IArtifactPermissionsRepository _artifactPermissionsRepository;
        private readonly IUsersRepository _usersRepository;
        private readonly ISqlArtifactRepository _artifactRepository;
        private readonly ICurrentDateTimeService _currentDateTimeService;
        private readonly IApplicationSettingsRepository _applicationSettingsRepository;
        private readonly ISqlHelper _sqlHelper;

        internal const string ReviewArtifactHierarchyRebuildIntervalInMinutesKey = "ReviewArtifactHierarchyRebuildIntervalInMinutes";
        internal const int DefaultReviewArtifactHierarchyRebuildIntervalInMinutes = 20;

        public SqlReviewsRepository() : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain),
                                            new SqlArtifactVersionsRepository(),
                                            new SqlItemInfoRepository(),
                                            new SqlArtifactPermissionsRepository(),
                                            new ApplicationSettingsRepository(),
                                            new SqlUsersRepository(),
                                            new SqlArtifactRepository(),
                                            new CurrentDateTimeService(),
                                            new SqlHelper())
        {
        }

        public SqlReviewsRepository(ISqlConnectionWrapper connectionWrapper,
                                    IArtifactVersionsRepository artifactVersionsRepository,
                                    ISqlItemInfoRepository itemInfoRepository,
                                    IArtifactPermissionsRepository artifactPermissionsRepository,
                                    IApplicationSettingsRepository applicationSettingsRepository,
                                    IUsersRepository usersRepository,
                                    ISqlArtifactRepository artifactRepository,
                                    ICurrentDateTimeService currentDateTimeService,
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
            _sqlHelper = sqlHelper;
        }

        public async Task<ReviewSummary> GetReviewSummary(int containerId, int userId)
        {
            var reviewInfo = await _artifactVersionsRepository.GetVersionControlArtifactInfoAsync(containerId, null, userId);
            if (reviewInfo.IsDeleted || reviewInfo.PredefinedType != ItemTypePredefined.ArtifactReviewPackage)
            {
                ThrowReviewNotFoundException(containerId);
            }

            var reviewDetails = await GetReviewSummaryDetails(containerId, userId);

            if (reviewDetails == null)
            {
                ThrowReviewNotFoundException(containerId);
            }

            if (reviewDetails.ReviewPackageStatus == ReviewPackageStatus.Draft)
            {
                ThrowReviewNotFoundException(containerId);
            }

            if (!reviewDetails.ReviewParticipantRole.HasValue && reviewDetails.TotalReviewers > 0)
            {
                ThrowUserCannotAccessReviewException(containerId);
            }

            var reviewSource = new ReviewSource();
            if (reviewDetails.BaselineId.HasValue)
            {
                var baselineInfo = await _artifactVersionsRepository.GetVersionControlArtifactInfoAsync(reviewDetails.BaselineId.Value, null, userId);
                reviewSource.Id = baselineInfo.Id;
                reviewSource.Name = baselineInfo.Name;
                reviewSource.Prefix = baselineInfo.Prefix;
            }

            var description = await _itemInfoRepository.GetItemDescription(containerId, userId, true, int.MaxValue);
            var reviewContainer = new ReviewSummary
            {
                Id = containerId,
                Name = reviewInfo.Name,
                Prefix = reviewDetails.Prefix,
                ArtifactType = reviewDetails.ArtifactType,
                Description = description,
                Source = reviewSource,
                ReviewParticipantRole = reviewDetails.ReviewParticipantRole,
                TotalArtifacts = reviewDetails.TotalArtifacts,
                Status = reviewDetails.ReviewStatus,
                ReviewPackageStatus = reviewDetails.ReviewPackageStatus,
                RequireAllArtifactsReviewed = reviewDetails.RequireAllArtifactsReviewed,
                ArtifactsStatus = new ReviewArtifactsStatus
                {
                    Approved = reviewDetails.Approved,
                    Disapproved = reviewDetails.Disapproved,
                    Pending = reviewDetails.Pending,
                    Viewed = reviewDetails.Viewed
                },
                ReviewType = GetReviewType(reviewDetails),
                RevisionId = reviewDetails.RevisionId,
                ProjectId = reviewInfo.ProjectId
            };
            return reviewContainer;
        }

        private ReviewType GetReviewType(ReviewSummaryDetails reviewDetails)
        {
            if (reviewDetails.TotalReviewers == 0)
            {
                return ReviewType.Public;
            }

            return reviewDetails.BaselineId.HasValue ? ReviewType.Formal : ReviewType.Informal;
        }

        private async Task<ReviewSummaryDetails> GetReviewSummaryDetails(int reviewId, int userId)
        {
            var param = new DynamicParameters();
            param.Add("@reviewId", reviewId);
            param.Add("@userId", userId);

            return (await _connectionWrapper.QueryAsync<ReviewSummaryDetails>(
                "GetReviewDetails", param,
                commandType: CommandType.StoredProcedure)).SingleOrDefault();
        }

        public async Task<QueryResult<ReviewArtifact>> GetReviewArtifactsContentAsync(int reviewId, int userId, Pagination pagination, int? versionId = null, bool? addDrafts = true)
        {
            int? revisionId = await _itemInfoRepository.GetRevisionId(reviewId, userId, versionId);

            var reviewArtifacts = await GetReviewArtifactsAsync<ReviewArtifact>(reviewId, userId, pagination, revisionId, addDrafts);
            var reviewArtifactIds = reviewArtifacts.Items.Select(a => a.Id).ToList();
            reviewArtifactIds.Add(reviewId);

            var artifactPermissionsDictionary = await _artifactPermissionsRepository.GetArtifactPermissions(reviewArtifactIds, userId);

            if (!SqlArtifactPermissionsRepository.HasPermissions(reviewId, artifactPermissionsDictionary, RolePermissions.Read))
            {
                ThrowUserCannotAccessReviewException(reviewId);
            }

            var reviewArtifactStatuses = await GetReviewArtifactStatusesAsync(reviewId, userId, pagination, versionId, addDrafts, reviewArtifactIds);
            var numUsers = reviewArtifactStatuses.NumUsers;
            var numApprovers = reviewArtifactStatuses.NumApprovers;
            var artifactStatusDictionary = reviewArtifactStatuses.ItemStatuses.ToDictionary(a => a.ArtifactId);

            foreach (var reviewArtifact in reviewArtifacts.Items)
            {
                if ((reviewArtifacts.IsFormal && reviewArtifact.HasMovedProject) ||
                    SqlArtifactPermissionsRepository.HasPermissions(reviewArtifact.Id, artifactPermissionsDictionary, RolePermissions.Read))
                {
                    ReviewArtifactStatus reviewArtifactStatus;

                    if (artifactStatusDictionary.TryGetValue(reviewArtifact.Id, out reviewArtifactStatus))
                    {
                        reviewArtifact.Pending = reviewArtifactStatus.Pending;
                        reviewArtifact.Approved = reviewArtifactStatus.Approved;
                        reviewArtifact.Disapproved = reviewArtifactStatus.Disapproved;
                        reviewArtifact.Viewed = reviewArtifactStatus.Viewed;
                        reviewArtifact.Unviewed = reviewArtifactStatus.Unviewed;
                    }
                    else
                    {
                        reviewArtifact.Pending = numApprovers;
                        reviewArtifact.Unviewed = numUsers;
                    }

                    reviewArtifact.HasAccess = true;
                }
                else
                {
                    ClearReviewArtifactProperties(reviewArtifact);
                }
            }
            return reviewArtifacts;
        }

        private void ClearReviewArtifactProperties(BaseReviewArtifact reviewArtifact)
        {
            reviewArtifact.Name = string.Empty;
            reviewArtifact.ItemTypeId = 0;
            reviewArtifact.HasComments = false;
            reviewArtifact.ItemTypePredefined = 0;
            reviewArtifact.IconImageId = null;
            reviewArtifact.HasAccess = false;
            reviewArtifact.IsApprovalRequired = false;
        }

        public async Task<AddArtifactsResult> AddArtifactsToReviewAsync(int reviewId, int userId, AddArtifactsParameter content)
        {
            if (content.ArtifactIds == null || content.ArtifactIds.Count() == 0)
            {
                throw new BadRequestException("There is nothing to add to review.", ErrorCodes.OutOfRangeParameter);
            }

            int alreadyIncludedCount;
            var propertyResult = await GetReviewPropertyString(reviewId, userId);

            if (propertyResult.IsReviewReadOnly)
            {
                ThrowReviewClosedException();
            }

            if (propertyResult.ProjectId == null || propertyResult.ProjectId < 1)
            {
                ThrowReviewNotFoundException(reviewId);
            }

            if (propertyResult.IsReviewLocked == false)
            {
                ExceptionHelper.ThrowArtifactNotLockedException(reviewId, userId);
            }

            var effectiveIds = await GetEffectiveArtifactIds(userId, content, propertyResult.ProjectId.Value);

            var artifactXmlResult = AddArtifactsToXML(propertyResult.ArtifactXml, new HashSet<int>(effectiveIds.ArtifactIds), out alreadyIncludedCount);
            await UpdateReviewArtifacts(reviewId, userId, artifactXmlResult);

            return new AddArtifactsResult()
            {
                ArtifactCount = effectiveIds.ArtifactIds.Count() - alreadyIncludedCount,
                AlreadyIncludedArtifactCount = alreadyIncludedCount,
                NonexistentArtifactCount = effectiveIds.Nonexistent,
                UnpublishedArtifactCount = effectiveIds.Unpublished
            };
        }


        private async Task<PropertyValueString> GetReviewPropertyString(int reviewId, int userId)
        {
            var param = new DynamicParameters();
            param.Add("@reviewId", reviewId);
            param.Add("@userId", userId);

            return (await _connectionWrapper.QueryAsync<PropertyValueString>("GetReviewPropertyString", param, commandType: CommandType.StoredProcedure)).SingleOrDefault();
        }

        private async Task<PropertyValueString> GetReviewArtifactApprovalRequestedInfo(int reviewId, int userId)
        {
            var param = new DynamicParameters();
            param.Add("@reviewId", reviewId);
            param.Add("@userId", userId);

            return (await _connectionWrapper.QueryAsync<PropertyValueString>("GetReviewArtifactApprovalRequestedInfo", param, commandType: CommandType.StoredProcedure)).SingleOrDefault();
        }

        private async Task<PropertyValueString> GetReviewApprovalRolesInfo(int reviewId, int userId, int roleUserId)
        {
            var param = new DynamicParameters();
            param.Add("@reviewId", reviewId);
            param.Add("@userId", userId);
            param.Add("@roleUserId", roleUserId);

            var result = (await _connectionWrapper.QueryAsync<PropertyValueString>("GetReviewApprovalRolesInfo", param, commandType: CommandType.StoredProcedure)).SingleOrDefault();
            return new PropertyValueString()
            {
                IsDraftRevisionExists = result.IsDraftRevisionExists,
                ArtifactXml = result.ArtifactXml,
                RevewSubartifactId = result.RevewSubartifactId,
                ProjectId = result.ProjectId,
                IsReviewLocked = result.IsReviewLocked,
                IsReviewReadOnly = result.IsReviewReadOnly,
                BaselineId = result.BaselineId,
                IsReviewDeleted = result.IsReviewDeleted,
                IsUserDisabled = result.IsUserDisabled
            };
        }

        private async Task<EffectiveArtifactIdsResult> GetEffectiveArtifactIds(int userId, AddArtifactsParameter content, int projectId)
        {
            var param = new DynamicParameters();
            param.Add("@artifactIds", SqlConnectionWrapper.ToDataTable(content.ArtifactIds));
            param.Add("@userId", userId);
            param.Add("@projectId", projectId);

            var result = await _connectionWrapper.QueryMultipleAsync<int, int, int>("GetEffectiveArtifactIds", param, commandType: CommandType.StoredProcedure);
            return new EffectiveArtifactIdsResult()
            {
                ArtifactIds = result.Item1.ToList(),
                Unpublished = result.Item2.SingleOrDefault(),
                Nonexistent = result.Item3.SingleOrDefault()
            };
        }

        private string AddArtifactsToXML(string xmlArtifacts, ISet<int> artifactsToAdd, out int alreadyIncluded)
        {
            alreadyIncluded = 0;
            RDReviewContents rdReviewContents;
            if (string.IsNullOrEmpty(xmlArtifacts))
            {
                rdReviewContents = new RDReviewContents();
                rdReviewContents.Artifacts = new List<RDArtifact>();
            }
            else
            {
                rdReviewContents = ReviewRawDataHelper.RestoreData<RDReviewContents>(xmlArtifacts);
            }

            var currentArtifactIds = rdReviewContents.Artifacts.Select(a => a.Id);

            foreach (var artifactToAdd in artifactsToAdd)
            {
                if (!currentArtifactIds.Contains(artifactToAdd))
                {
                    var addedArtifact = new RDArtifact()
                    {
                        Id = artifactToAdd,
                        ApprovalNotRequested = true

                    };
                    rdReviewContents.Artifacts.Add(addedArtifact);
                }
                else
                    ++alreadyIncluded;
            }

            return ReviewRawDataHelper.GetStoreData(rdReviewContents);
        }

        public Task<QueryResult<ReviewedArtifact>> GetReviewedArtifacts(int reviewId, int userId, Pagination pagination, int revisionId)
        {
            return GetParticipantReviewedArtifactsAsync(reviewId, userId, userId, pagination, false, revisionId);
        }

        private async Task<QueryResult<ReviewedArtifact>> GetParticipantReviewedArtifactsAsync(int reviewId, int userId, int participantId, Pagination pagination, bool showArtifactsIfFormal, int revisionId = int.MaxValue, bool addDrafts = false)
        {
            var reviewArtifacts = await GetReviewArtifactsAsync<ReviewedArtifact>(reviewId, userId, pagination, revisionId, addDrafts);

            var reviewArtifactIds = reviewArtifacts.Items.Select(a => a.Id).ToList();

            var artifactPermissionsDictionary = await _artifactPermissionsRepository
                .GetArtifactPermissions(reviewArtifactIds.Union(new[] {reviewId}), userId);

            if (!SqlArtifactPermissionsRepository.HasPermissions(reviewId, artifactPermissionsDictionary, RolePermissions.Read))
            {
                ThrowUserCannotAccessReviewException(reviewId);
            }

            var reviewedArtifacts = (await GetReviewArtifactsByParticipant(reviewArtifactIds, participantId, reviewId, revisionId)).ToDictionary(k => k.Id);

            foreach (var artifact in reviewArtifacts.Items)
            {
                var ignorePermissionCheck = showArtifactsIfFormal && reviewArtifacts.IsFormal && artifact.HasMovedProject;

                if (ignorePermissionCheck 
                    || SqlArtifactPermissionsRepository.HasPermissions(artifact.Id, artifactPermissionsDictionary, RolePermissions.Read))
                {
                    ReviewedArtifact reviewedArtifact;
                    if (reviewedArtifacts.TryGetValue(artifact.Id, out reviewedArtifact))
                    {
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
                        artifact.ViewedArtifactVersion = reviewedArtifact.ViewedArtifactVersion;
                        artifact.SignedOnTimestamp = reviewedArtifact.SignedOnTimestamp;
                        artifact.HasAttachments = reviewedArtifact.HasAttachments;
                        artifact.HasRelationships = reviewedArtifact.HasRelationships;
                        artifact.HasAccess = true;
                    }
                }
                else
                {
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
                && string.Compare(reviewedArtifact.Approval, NOT_SPECIFIED, StringComparison.OrdinalIgnoreCase) == 0
                || isApprovalRequired && string.IsNullOrEmpty(reviewedArtifact.Approval))
            {
                return PENDING;
            }
            return reviewedArtifact.Approval;
        }

        private Task<IEnumerable<ReviewedArtifact>> GetReviewArtifactsByParticipant(IEnumerable<int> artifactIds, int userId, int reviewId, int revisionId)
        {
            var param = new DynamicParameters();
            param.Add("@itemIds", SqlConnectionWrapper.ToDataTable(artifactIds));
            param.Add("@userId", userId);
            param.Add("@reviewId", reviewId);
            param.Add("@revisionId", revisionId);

            return _connectionWrapper.QueryAsync<ReviewedArtifact>("GetReviewArtifactsByParticipant", param, commandType: CommandType.StoredProcedure);
        }

        private async Task<ReviewArtifactsQueryResult<T>> GetReviewArtifactsAsync<T>(int reviewId, int userId, Pagination pagination, int? revisionId = null, bool? addDrafts = true)
            where T : BaseReviewArtifact
        {
            int refreshInterval = await GetRebuildReviewArtifactHierarchyInterval();
            var param = new DynamicParameters();
            param.Add("@reviewId", reviewId);
            param.Add("@offset", pagination.Offset);
            param.Add("@limit", pagination.Limit);
            param.Add("@revisionId", revisionId);
            param.Add("@addDrafts", revisionId < int.MaxValue ? false : addDrafts);
            param.Add("@userId", userId);
            param.Add("@refreshInterval", refreshInterval);
            param.Add("@numResult", dbType: DbType.Int32, direction: ParameterDirection.Output);
            param.Add("@isFormal", dbType: DbType.Boolean, direction: ParameterDirection.Output);

            var result = await _connectionWrapper.QueryAsync<T>("GetReviewArtifacts", param, commandType: CommandType.StoredProcedure);
            return new ReviewArtifactsQueryResult<T>()
            {
                Items = result.ToList(),
                Total = param.Get<int>("@numResult"),
                IsFormal = param.Get<bool>("@isFormal")
            };
        }

        private async Task<int> UpdateReviewArtifacts(int reviewId, int userId, string xmlArtifacts, bool addReviewSubArtifactIfNeeded = true)
        {
            var param = new DynamicParameters();
            param.Add("@reviewId", reviewId);
            param.Add("@userId", userId);
            param.Add("@xmlArtifacts", xmlArtifacts);
            param.Add("@addReviewSubArtifactIfNeeded", addReviewSubArtifactIfNeeded);

            return await _connectionWrapper.ExecuteAsync("UpdateReviewArtifacts", param, commandType: CommandType.StoredProcedure);
        }

        private async Task<int> GetRebuildReviewArtifactHierarchyInterval()
        {
            var refreshInterval = await _applicationSettingsRepository.GetValue<int>(ReviewArtifactHierarchyRebuildIntervalInMinutesKey, DefaultReviewArtifactHierarchyRebuildIntervalInMinutes);
            if (refreshInterval < 0)
            {
                refreshInterval = DefaultReviewArtifactHierarchyRebuildIntervalInMinutes;
            }

            return refreshInterval;
        }

        private async Task<ContentStatusDetails> GetReviewArtifactStatusesAsync(int reviewId, int userId, Pagination pagination,
                                                                        int? versionId = null,
                                                                        bool? addDrafts = true,
                                                                        IEnumerable<int> reviewArtifactIds = null)
        {
            int? revisionId = await _itemInfoRepository.GetRevisionId(reviewId, userId, versionId);
            if (revisionId < int.MaxValue)
            {
                addDrafts = false;
            }
            var param = new DynamicParameters();
            param.Add("@reviewId", reviewId);
            param.Add("@offset", pagination.Offset);
            param.Add("@limit", pagination.Limit);
            param.Add("@revisionId", revisionId);
            param.Add("@addDrafts", addDrafts);
            param.Add("@userId", userId);
            param.Add("@itemIds", SqlConnectionWrapper.ToDataTable(reviewArtifactIds));
            var result = await _connectionWrapper.QueryMultipleAsync<ReviewArtifactStatus, int, int>("GetReviewArtifactsStatus", param, commandType: CommandType.StoredProcedure);
            return new ContentStatusDetails
            {
                ItemStatuses = result.Item1.ToList(),
                NumUsers = result.Item2.SingleOrDefault(),
                NumApprovers = result.Item3.SingleOrDefault()
            };
        }

        public async Task<ReviewParticipantsContent> GetReviewParticipantsAsync(int reviewId, int? offset, int? limit, int userId, int? versionId = null, bool? addDrafts = true)
        {
            if (limit < 1)
            {
                throw new BadRequestException(nameof(limit) + " cannot be less than 1.", ErrorCodes.InvalidParameter);
            }

            if (offset < 0)
            {
                throw new BadRequestException(nameof(offset) + " cannot be less than 0.", ErrorCodes.InvalidParameter);
            }

            if (versionId < 1)
            {
                throw new BadRequestException(nameof(versionId) + " cannot be less than 1.", ErrorCodes.InvalidParameter);
            }

            int? revisionId = await _itemInfoRepository.GetRevisionId(reviewId, userId, versionId);

            if (revisionId < int.MaxValue)
            {
                addDrafts = false;
            }

            var param = new DynamicParameters();

            param.Add("@reviewId", reviewId);
            param.Add("@offset", offset);
            param.Add("@limit", limit);
            param.Add("@revisionId", revisionId);
            param.Add("@userId", userId);
            param.Add("@addDrafts", addDrafts);

            var participants = await _connectionWrapper.QueryMultipleAsync<ReviewParticipant, int, int, int>("GetReviewParticipants", param, commandType: CommandType.StoredProcedure);

            var reviewersRoot = new ReviewParticipantsContent()
            {
                Items = participants.Item1.ToList(),
                Total = participants.Item2.SingleOrDefault(),
                TotalArtifacts = participants.Item3.SingleOrDefault(),
                TotalArtifactsRequestedApproval = participants.Item4.SingleOrDefault()
            };

            return reviewersRoot;
        }

        public async Task<QueryResult<ReviewArtifactDetails>> GetReviewArtifactStatusesByParticipant(int artifactId, int reviewId, int? offset, int? limit, int userId, int? versionId = null, bool? addDrafts = true)
        {
            var artifactPermissionsDictionary = await _artifactPermissionsRepository.GetArtifactPermissions(new[] {reviewId, artifactId}, userId);

            if (!SqlArtifactPermissionsRepository.HasPermissions(reviewId, artifactPermissionsDictionary, RolePermissions.Read))
            {
                ThrowUserCannotAccessReviewException(reviewId);
            }

            if (!SqlArtifactPermissionsRepository.HasPermissions(artifactId, artifactPermissionsDictionary, RolePermissions.Read))
            {
                ExceptionHelper.ThrowArtifactForbiddenException(artifactId);
            }

            int? revisionId = await _itemInfoRepository.GetRevisionId(reviewId, userId, versionId);
            if (revisionId < int.MaxValue)
            {
                addDrafts = false;
            }
            var param = new DynamicParameters();
            param.Add("@artifactId", artifactId);
            param.Add("@reviewId", reviewId);
            param.Add("@offset", offset);
            param.Add("@limit", limit);
            param.Add("@revisionId", revisionId);
            param.Add("@userId", userId);
            param.Add("@addDrafts", addDrafts);
            var participants = await _connectionWrapper.QueryMultipleAsync<ReviewArtifactDetails, int>("GetReviewArtifactStatusesByParticipant", param, commandType: CommandType.StoredProcedure);
            var reviewersRoot = new QueryResult<ReviewArtifactDetails>()
            {
                Items = participants.Item1.ToList(),
                Total = participants.Item2.SingleOrDefault()
            };
            return reviewersRoot;
        }

        public async Task<AddParticipantsResult> AddParticipantsToReviewAsync(int reviewId, int userId, AddParticipantsParameter content)
        {
            //Check there is at least one user/group to add
            if ((content.GroupIds == null || !content.GroupIds.Any()) &&
               (content.UserIds == null || !content.UserIds.Any()))
            {
                throw new BadRequestException("No users were selected to be added.", ErrorCodes.OutOfRangeParameter);
            }

            var reviewXmlResult = await GetReviewXmlAsync(reviewId, userId);

            if (!reviewXmlResult.ReviewExists)
            {
                ThrowReviewNotFoundException(reviewId);
            }

            var reviewLockedByUser = await _artifactRepository.IsArtifactLockedByUserAsync(reviewId, userId);

            if (!reviewLockedByUser)
            {
                ExceptionHelper.ThrowArtifactNotLockedException(reviewId, userId);
            }

            ReviewPackageRawData reviewPackageRawData;

            if (reviewXmlResult.XmlString != null)
            {
                reviewPackageRawData = ReviewRawDataHelper.RestoreData<ReviewPackageRawData>(reviewXmlResult.XmlString);
            }
            else
            {
                reviewPackageRawData = new ReviewPackageRawData();
            }

            if (reviewPackageRawData.Status == ReviewPackageStatus.Closed)
            {
                ThrowReviewClosedException();
            }

            IEnumerable<int> groupUserIds = await GetUsersFromGroupsAsync(content.GroupIds);

            var userIds = content.UserIds ?? new int[0];

            var deletedUserIds = (await _usersRepository.FindNonExistentUsersAsync(userIds)).ToList();

            //Flatten users into a single collection and remove duplicates and non existant users
            var uniqueParticipantsSet = new HashSet<int>(userIds.Except(deletedUserIds).Concat(groupUserIds));

            if (reviewPackageRawData.Reviwers == null)
            {
                reviewPackageRawData.Reviwers = new List<ReviewerRawData>();
            }

            var participantIdsToAdd = uniqueParticipantsSet.Except(reviewPackageRawData.Reviwers.Select(r => r.UserId)).ToList();

            int newParticipantsCount = participantIdsToAdd.Count;

            reviewPackageRawData.Reviwers.AddRange(participantIdsToAdd.Select(p => new ReviewerRawData()
            {
                UserId = p,
                Permission = ReviewParticipantRole.Reviewer
            }));

            if (newParticipantsCount > 0)
            {
                //Save XML in the database
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
                NonExistentUsers = deletedUserIds.Count()
            };
        }

        private async Task<ReviewXmlResult> GetReviewXmlAsync(int reviewId, int userId)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@reviewId", reviewId);
            parameters.Add("@userId", userId);

            var result = (await _connectionWrapper.QueryAsync<string>("GetReviewParticipantsPropertyString", parameters, commandType: CommandType.StoredProcedure)).ToList();

            return new ReviewXmlResult()
            {
                ReviewExists = result.Count > 0,
                XmlString = result.SingleOrDefault()
            };
        }


        private async Task<int> UpdateReviewXmlAsync(int reviewId, int userId, string reviewXml)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@reviewId", reviewId);
            parameters.Add("@userId", userId);
            parameters.Add("@xmlString", reviewXml);
            parameters.Add("@returnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

            await _connectionWrapper.ExecuteAsync("UpdateReviewParticipants", parameters, commandType: CommandType.StoredProcedure);

            return parameters.Get<int>("@returnValue");
        }

        private async Task<IEnumerable<int>> GetUsersFromGroupsAsync(IEnumerable<int> groupIds)
        {
            if (groupIds != null && groupIds.Any())
            {
                //Get all users from all groups
                var groupUsers = await _usersRepository.GetUserInfosFromGroupsAsync(groupIds);

                return groupUsers.Select(gu => gu.UserId);
            }
            else
            {
                return new int[0];
            }
        }

        private async Task<QueryResult<ReviewTableOfContentItem>> GetTableOfContentAsync(int reviewId, int revisionId, int userId, Pagination pagination)
        {
            int refreshInterval = await GetRebuildReviewArtifactHierarchyInterval();
            var param = new DynamicParameters();
            param.Add("@reviewId", reviewId);
            param.Add("@offset", pagination.Offset);
            param.Add("@limit", pagination.Limit);
            param.Add("@revisionId", revisionId);
            param.Add("@userId", userId);
            param.Add("@refreshInterval", refreshInterval);
            param.Add("@total", dbType: DbType.Int32, direction: ParameterDirection.Output);
            param.Add("@retResult", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);


            var result = await _connectionWrapper.QueryAsync<ReviewTableOfContentItem>("GetReviewTableOfContent", param, commandType: CommandType.StoredProcedure);
            var retResult = param.Get<int>("@retResult");
            // The review is not found or not active.		
            if (retResult == 1 || retResult == 2)
            {
                ThrowReviewNotFoundException(reviewId, revisionId);
            }

            // The user is not a review participant.		 
            if (retResult == 3)
            {
                ThrowUserCannotAccessReviewException(reviewId);
            }

            return new QueryResult<ReviewTableOfContentItem>()
            {
                Items = result.ToList(),
                Total = param.Get<int>("@total")
            };
        }



        public async Task<QueryResult<ReviewTableOfContentItem>> GetReviewTableOfContent(int reviewId, int revisionId, int userId, Pagination pagination)
        {

            //get all review content item in a hierarchy list
            var toc = await GetTableOfContentAsync(reviewId, revisionId, userId, pagination);

            var artifactIds = new List<int> { reviewId }.Concat(toc.Items.Select(a => a.Id).ToList());

            //gets artifact permissions
            var artifactPermissionsDictionary = await _artifactPermissionsRepository.GetArtifactPermissions(artifactIds, userId);

            if (!SqlArtifactPermissionsRepository.HasPermissions(reviewId, artifactPermissionsDictionary, RolePermissions.Read))
            {
                ThrowUserCannotAccessReviewException(reviewId);
            }

            var reviewedArtifacts = (await GetReviewArtifactsByParticipant(toc.Items.Select(a => a.Id), userId, reviewId, revisionId)).ToList();

            //TODO: Update artifact statuses and permissions
            //
            foreach (var tocItem in toc.Items)
            {
                if (SqlArtifactPermissionsRepository.HasPermissions(tocItem.Id, artifactPermissionsDictionary, RolePermissions.Read))
                {
                    //TODO update item status
                    tocItem.HasAccess = true;

                    var artifact = reviewedArtifacts.First(it => it.Id == tocItem.Id);
                    tocItem.ArtifactVersion = artifact.ArtifactVersion;
                    tocItem.ApprovalStatus = (ApprovalType)artifact?.ApprovalFlag;
                    tocItem.ViewedArtifactVersion = artifact?.ViewedArtifactVersion;
                }
                else
                {
                    //not granted SES
                    //TODO: http://svmtfs2015:8080/tfs/svmtfs2015/Blueprint/_workitems?_a=edit&id=6593&fullScreen=false
                    UnauthorizedItem(tocItem);
                }
            }

            return toc;
        }


        public async Task RemoveArtifactsFromReviewAsync(int reviewId, ReviewArtifactsRemovalParams removeParams, int userId)
        {
            if ((removeParams.artifactIds == null || removeParams.artifactIds.Count() == 0) && removeParams.SelectionType == SelectionType.Selected)
            {
                throw new BadRequestException("Incorrect input parameters", ErrorCodes.OutOfRangeParameter);
            }
            var propertyResult = await GetReviewPropertyString(reviewId, userId);

            if (propertyResult.IsReviewReadOnly)
            {
                ThrowReviewClosedException();
            }
            if (propertyResult.BaselineId != null && propertyResult.BaselineId.Value > 0)
            {
                throw new BadRequestException("Review status changed", ErrorCodes.ReviewStatusChanged);
            }
        
            if (propertyResult.ProjectId == null || propertyResult.ProjectId < 1)
            {
                ThrowReviewNotFoundException(reviewId);
            }

            if (propertyResult.IsReviewLocked == false)
            {
                ExceptionHelper.ThrowArtifactNotLockedException(reviewId, userId);
            }

            if (string.IsNullOrEmpty(propertyResult.ArtifactXml))
            {
                ExceptionHelper.ThrowArtifactDoesNotSupportOperation(reviewId);
            }
            var rdReviewContents = ReviewRawDataHelper.RestoreData<RDReviewContents>(propertyResult.ArtifactXml);
            var currentArtifactIds = rdReviewContents.Artifacts.Select(a => a.Id);
            if (removeParams.SelectionType == SelectionType.Selected && removeParams.artifactIds != null)
            {
                rdReviewContents.Artifacts.RemoveAll(a => removeParams.artifactIds.Contains(a.Id));
            }
            else
            {
                if (removeParams.artifactIds != null && removeParams.artifactIds.Count() > 0)
                {
                    rdReviewContents.Artifacts.RemoveAll(a => !removeParams.artifactIds.Contains(a.Id));
                }
                else if (removeParams.artifactIds == null || removeParams.artifactIds.Count() == 0)
                {
                    rdReviewContents.Artifacts = new List<RDArtifact>();
                }
            }
            var artifactXmlResult = ReviewRawDataHelper.GetStoreData(rdReviewContents);
            await UpdateReviewArtifacts(reviewId, userId, artifactXmlResult);
        }

        public async Task AssignApprovalRequiredToArtifacts(int reviewId, int userId, AssignArtifactsApprovalParameter content)
        {
            if (content.ArtifactIds == null || content.ArtifactIds.Count() == 0)
            {
                throw new BadRequestException("Incorrect input parameters", ErrorCodes.OutOfRangeParameter);
            }

            var propertyResult = await GetReviewArtifactApprovalRequestedInfo(reviewId, userId);

            if (propertyResult.IsReviewDeleted)
            {
                ThrowReviewNotFoundException(reviewId);
            }

            if (propertyResult.IsReviewReadOnly)
            {
                ThrowApprovalRequiredIsReadonlyForReview();
            }

            if (propertyResult.IsReviewLocked == false)
            {
                ExceptionHelper.ThrowArtifactNotLockedException(reviewId, userId);
            }

            if (propertyResult.ProjectId == null || propertyResult.ProjectId < 1 || string.IsNullOrEmpty(propertyResult.ArtifactXml))
            {
                ExceptionHelper.ThrowArtifactDoesNotSupportOperation(reviewId);
            }

            var artifactPermissionsDictionary = await _artifactPermissionsRepository.GetArtifactPermissions(content.ArtifactIds, userId);
            var artifactsWithReadPermissions = artifactPermissionsDictionary.Where(p => p.Value.HasFlag(RolePermissions.Read)).Select(p => p.Key);
            if (artifactsWithReadPermissions.Intersect(content.ArtifactIds).Count() != content.ArtifactIds.Count())
            {
                ThrowUserCannotAccessArtifactInTheReviewException(propertyResult.ProjectId.Value);
            }

            // For Informal review
            if (propertyResult.BaselineId == null || propertyResult.BaselineId < 1)
            {
                foreach (var artifactId in content.ArtifactIds)
                {
                    var isArtifactDeleted = await _artifactVersionsRepository.IsItemDeleted(artifactId);
                    if (isArtifactDeleted)
                    {
                        ThrowUserCannotAccessArtifactInTheReviewException(propertyResult.ProjectId.Value);
                    }
                }
            }

            bool hasChanges;
            var artifactXmlResult = UpdateApprovalRequiredForArtifactsXML(propertyResult.ArtifactXml, content, reviewId, out hasChanges);
            if (hasChanges)
            {
                await UpdateReviewArtifacts(reviewId, userId, artifactXmlResult, false);
            }
        }
        private string UpdatePermissionRolesXML(string xmlArtifacts, AssignReviewerRolesParameter content, int reviewId)
        {

            var reviewPackageRawData = ReviewRawDataHelper.RestoreData<ReviewPackageRawData>(xmlArtifacts);

            var participantIdsToAdd = reviewPackageRawData.Reviwers.Where(a => a.UserId == content.UserId).FirstOrDefault<ReviewerRawData>();
            if (participantIdsToAdd == null)
            {
                ExceptionHelper.ThrowArtifactDoesNotSupportOperation(reviewId);
            }
            participantIdsToAdd.Permission = content.Role;

            return ReviewRawDataHelper.GetStoreData(reviewPackageRawData);
        }

        private string UpdateApprovalRequiredForArtifactsXML(string xmlArtifacts, AssignArtifactsApprovalParameter content, int reviewId, out bool hasChanges)
        {
            hasChanges = false;
            var rdReviewContents = ReviewRawDataHelper.RestoreData<RDReviewContents>(xmlArtifacts);
            foreach (var artifactId in content.ArtifactIds)
            {
                var updatingArtifacts = rdReviewContents.Artifacts.Where(a => a.Id == artifactId);
                if (updatingArtifacts.Count() == 0)
                {
                    ThrowApprovalRequiredArtifactNotInReview();
                }
                foreach (var updatingArtifact in updatingArtifacts)
                {
                    if (updatingArtifact.ApprovalNotRequested == content.ApprovalRequired)
                    {
                        updatingArtifact.ApprovalNotRequested = !content.ApprovalRequired;
                        hasChanges = true;
                    }
                }
            }

            if (hasChanges)
            {
                return ReviewRawDataHelper.GetStoreData(rdReviewContents);
            }

            return xmlArtifacts;
        }

        public async Task AssignRolesToReviewers(int reviewId, AssignReviewerRolesParameter content, int userId)
        {

            var propertyResult = await GetReviewApprovalRolesInfo(reviewId, userId, content.UserId);
            if (propertyResult == null)
            {
                throw new BadRequestException("Cannot update approval role as project or review couldn't be found", ErrorCodes.ResourceNotFound);
            }
            if (propertyResult.IsUserDisabled.Value)
            {
                throw new ConflictException("User deleted or not active", ErrorCodes.UserDisabled);
            }
            if (propertyResult.IsReviewDeleted)
            {

                ThrowReviewNotFoundException(reviewId);
            }

            if (propertyResult.IsReviewReadOnly)
            {
                ThrowApprovalStatusIsReadonlyForReview();
            }

            if (!propertyResult.IsReviewLocked)
            {
                ExceptionHelper.ThrowArtifactNotLockedException(reviewId, content.UserId);
            }

            if (string.IsNullOrEmpty(propertyResult.ArtifactXml))
            {
                ExceptionHelper.ThrowArtifactDoesNotSupportOperation(reviewId);
            }

            var artifactXmlResult = UpdatePermissionRolesXML(propertyResult.ArtifactXml, content, reviewId);

            var result = await UpdateReviewXmlAsync(reviewId, userId, artifactXmlResult);
            if (result != 1)
            {
                throw new BadRequestException("Cannot add participants as project or review couldn't be found", ErrorCodes.ResourceNotFound);
            }
        }



        public async Task<ReviewArtifactIndex> GetReviewArtifactIndexAsync(int reviewId, int revisionId, int artifactId, int userId, bool? addDrafts = true)
        {

            var reviewInfo = await _artifactVersionsRepository.GetVersionControlArtifactInfoAsync(reviewId, null, userId);

            if (reviewInfo.VersionCount == 0) // never published review
            {
                ThrowReviewNotFoundException(reviewId, revisionId);
            }

            int refreshInterval = await GetRebuildReviewArtifactHierarchyInterval();

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
                ThrowReviewNotFoundException(reviewId, revisionId);
            }
            else if (resultValue == 3)
            {
                ThrowUserCannotAccessReviewException(reviewId);
            }
            else if (result == null)
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
                ThrowReviewNotFoundException(reviewId, revisionId);
            }

            int refreshInterval = await GetRebuildReviewArtifactHierarchyInterval();

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
                ThrowReviewNotFoundException(reviewId, revisionId);
            }
            else if (resultValue == 3)
            {
                ThrowUserCannotAccessReviewException(reviewId);
            }
            else if (result == null)
            {
                throw new ResourceNotFoundException("Specified artifact is not found in the review", ErrorCodes.ResourceNotFound);
            }

            return result;
        }

        public async Task<IEnumerable<ReviewArtifactApprovalResult>> UpdateReviewArtifactApprovalAsync(int reviewId, IEnumerable<ReviewArtifactApprovalParameter> reviewArtifactApprovalParameters, int userId)
        {
            if (reviewArtifactApprovalParameters == null || !reviewArtifactApprovalParameters.Any())
            {
                throw new BadRequestException("No artifacts provided", ErrorCodes.OutOfRangeParameter);
            }

            var artifactIds = reviewArtifactApprovalParameters.Select(a => a.ArtifactId).ToList();

            var approvalCheck = await CheckReviewArtifactApprovalAsync(reviewId, userId, artifactIds);

            CheckReviewStatsCanBeUpdated(approvalCheck, reviewId);

            if (!approvalCheck.AllArtifactsRequireApproval)
            {
                throw new BadRequestException("Not all artifacts require approval.");
            }

            //Check user has permission for the review and all of the artifact ids
            await CheckReviewAndArtifactPermissions(userId, reviewId, artifactIds);

            var rdReviewedArtifacts = await GetReviewUserStatsXmlAsync(reviewId, userId);

            var artifactVersionDictionary = await GetVersionNumberForArtifacts(reviewId, artifactIds);

            var approvalResult = new List<ReviewArtifactApprovalResult>();

            var timestamp = _currentDateTimeService.GetUtcNow();

            //Update approvals for the specified artifacts
            foreach (var artifact in reviewArtifactApprovalParameters)
            {
                var reviewArtifactApproval = rdReviewedArtifacts.ReviewedArtifacts.FirstOrDefault(ra => ra.ArtifactId == artifact.ArtifactId);

                if (reviewArtifactApproval == null)
                {
                    reviewArtifactApproval = new ReviewArtifactXml
                    {
                        ArtifactId = artifact.ArtifactId
                    };

                    rdReviewedArtifacts.ReviewedArtifacts.Add(reviewArtifactApproval);
                }

                if (reviewArtifactApproval.ViewState == ViewStateType.NotViewed)
                {
                    reviewArtifactApproval.ViewState = ViewStateType.Viewed;
                }

                if (artifact.ApprovalFlag == ApprovalType.NotSpecified &&
                    artifact.Approval.Equals(PENDING, StringComparison.InvariantCultureIgnoreCase))
                {
                    reviewArtifactApproval.ESignedOn = null;
                }
                else if (reviewArtifactApproval.Approval != artifact.Approval
                         || reviewArtifactApproval.ApprovalFlag != artifact.ApprovalFlag)
                {
                    reviewArtifactApproval.ESignedOn = timestamp;
                }

                reviewArtifactApproval.Approval = artifact.Approval;
                reviewArtifactApproval.ApprovalFlag = artifact.ApprovalFlag;

                if (artifactVersionDictionary.ContainsKey(artifact.ArtifactId))
                {
                    reviewArtifactApproval.ArtifactVersion = artifactVersionDictionary[artifact.ArtifactId];
                }

                approvalResult.Add(new ReviewArtifactApprovalResult()
                {
                    ArtifactId = reviewArtifactApproval.ArtifactId,
                    Timestamp = reviewArtifactApproval.ESignedOn
                });
            }

            await UpdateReviewUserStatsXmlAsync(reviewId, userId, rdReviewedArtifacts);

            return approvalResult;
        }

        public async Task UpdateReviewArtifactViewedAsync(int reviewId, int artifactId, ReviewArtifactViewedInput viewedInput, int userId)
        {
            if (viewedInput == null)
            {
                throw new BadRequestException("Viewed must be provided.");
            }

            var artifactIdEnumerable = new[] {artifactId};

            var approvalCheck = await CheckReviewArtifactApprovalAsync(reviewId, userId, artifactIdEnumerable);

            CheckReviewStatsCanBeUpdated(approvalCheck, reviewId);

            //Check user has permission for the review and all of the artifact ids
            await CheckReviewAndArtifactPermissions(userId, reviewId, artifactIdEnumerable);

            Func<IDbTransaction, Task> transactionAction = async(transaction) =>
            {
                var rdReviewedArtifacts = await GetReviewUserStatsXmlAsync(reviewId, userId, transaction);

                var artifactVersionDictionary = await GetVersionNumberForArtifacts(reviewId, artifactIdEnumerable, transaction);

                var reviewedArtifact = rdReviewedArtifacts.ReviewedArtifacts.FirstOrDefault(ra => ra.ArtifactId == artifactId);

                if (reviewedArtifact == null)
                {
                    reviewedArtifact = new ReviewArtifactXml()
                    {
                        ArtifactId = artifactId
                    };

                    rdReviewedArtifacts.ReviewedArtifacts.Add(reviewedArtifact);
                }

                if (viewedInput.Viewed)
                {
                    reviewedArtifact.ViewState = ViewStateType.Viewed;
                    reviewedArtifact.ArtifactVersion = artifactVersionDictionary[artifactId];
                }
                else
                {
                    reviewedArtifact.ViewState = ViewStateType.NotViewed;
                    reviewedArtifact.ArtifactVersion = 0;
                }

                await UpdateReviewUserStatsXmlAsync(reviewId, userId, rdReviewedArtifacts, transaction);
            };

            await _sqlHelper.RunInTransactionAsync(ServiceConstants.RaptorMain, transactionAction);
        }

        public Task UpdateReviewerStatusAsync(int reviewId, ReviewStatus reviewStatus, int userId)
        {
            switch (reviewStatus)
            {
                case ReviewStatus.NotStarted:
                    throw new BadRequestException("Cannot set reviewer status to not started");
                case ReviewStatus.InProgress:
                    return UpdateReviewerStatusToInProgressAsync(reviewId, userId);
                default:
                    throw new BadRequestException("Cannot set reviewer status to this unknown reviewer status");
            }
        }

        private async Task UpdateReviewerStatusToInProgressAsync(int reviewId, int userId)
        {
            var approvalCheck = await CheckReviewArtifactApprovalAsync(reviewId, userId, new int[0]);

            CheckReviewStatsCanBeUpdated(approvalCheck, reviewId);

            var artifactPermissionsDictionary = await _artifactPermissionsRepository.GetArtifactPermissions(new[] { reviewId }, userId);

            if (!SqlArtifactPermissionsRepository.HasPermissions(reviewId, artifactPermissionsDictionary, RolePermissions.Read))
            {
                ThrowUserCannotAccessReviewException(reviewId);
            }

            await UpdateReviewUserStatsAsync(reviewId, userId, true, ReviewStatus.InProgress.ToString());
        }

        private void CheckReviewStatsCanBeUpdated(ReviewArtifactApprovalCheck approvalCheck, int reviewId)
        {
            //Check the review exists and is active
            if (!approvalCheck.ReviewExists
               || approvalCheck.ReviewStatus == ReviewPackageStatus.Draft
               || approvalCheck.ReviewDeleted)
            {
                ThrowReviewNotFoundException(reviewId);
            }

            if (approvalCheck.ReviewStatus == ReviewPackageStatus.Closed)
            {
                ThrowReviewClosedException();
            }

            //Check user is an approver for the review
            if (!approvalCheck.UserInReview)
            {
                throw new AuthorizationException("User is not assigned for review", ErrorCodes.UserNotInReview);
            }

            //Check artifacts are part of the review and require approval
            if (!approvalCheck.AllArtifactsInReview)
            {
                throw new BadRequestException("Artifact is not a part of this review.");
            }
        }

        private async Task CheckReviewAndArtifactPermissions(int userId, int reviewId, IEnumerable<int> artifactIds)
        {
            var artifactIdsList = artifactIds.ToList();

            artifactIdsList.Add(reviewId);

            var artifactPermissionsDictionary = await _artifactPermissionsRepository.GetArtifactPermissions(artifactIdsList, userId);

            if (!SqlArtifactPermissionsRepository.HasPermissions(reviewId, artifactPermissionsDictionary, RolePermissions.Read))
            {
                ThrowUserCannotAccessReviewException(reviewId);
            }

            if (artifactIds.Any(artifactId => !SqlArtifactPermissionsRepository.HasPermissions(artifactId, artifactPermissionsDictionary, RolePermissions.Read)))
            {
                throw new ResourceNotFoundException("Artifacts could not be updated because they are no longer accessible.", ErrorCodes.ArtifactNotFound);
            }
        }

        private async Task<ReviewArtifactApprovalCheck> CheckReviewArtifactApprovalAsync(int reviewId, int userId, IEnumerable<int> artifactIds)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@reviewId", reviewId);
            parameters.Add("@userId", userId);
            parameters.Add("@artifactIds", SqlConnectionWrapper.ToDataTable(artifactIds));

            return (await _connectionWrapper.QueryAsync<ReviewArtifactApprovalCheck>("CheckReviewArtifactUserApproval", parameters, commandType: CommandType.StoredProcedure)).SingleOrDefault();
        }

        private async Task<IDictionary<int, int>> GetVersionNumberForArtifacts(int reviewId, IEnumerable<int> artifactIds, IDbTransaction transaction = null)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@reviewId", reviewId);
            parameters.Add("@artifactIds", SqlConnectionWrapper.ToDataTable(artifactIds));

            IEnumerable<ReviewArtifactVersionNumber> artifactVersionNumbers;

            if (transaction == null)
            {
                artifactVersionNumbers = await _connectionWrapper.QueryAsync<ReviewArtifactVersionNumber>("GetReviewArtifactVersionNumber", parameters, commandType: CommandType.StoredProcedure);
            }
            else
            {
                artifactVersionNumbers = await transaction.Connection.QueryAsync<ReviewArtifactVersionNumber>("GetReviewArtifactVersionNumber", parameters, transaction, commandType: CommandType.StoredProcedure);
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
                xmlString = (await _connectionWrapper.QueryAsync<string>("GetReviewUserStatsXml", parameters, commandType: CommandType.StoredProcedure)).SingleOrDefault();
            }
            else
            {
                xmlString = (await transaction.Connection.QuerySingleOrDefaultAsync<string>("GetReviewUserStatsXml", parameters, transaction, commandType: CommandType.StoredProcedure));
            }

            RDReviewedArtifacts rdReviewedArtifacts;

            if (xmlString != null)
            {
                rdReviewedArtifacts = ReviewRawDataHelper.RestoreData<RDReviewedArtifacts>(xmlString);
            }
            else
            {
                rdReviewedArtifacts = new RDReviewedArtifacts();
            }

            if (rdReviewedArtifacts.ReviewedArtifacts == null)
            {
                rdReviewedArtifacts.ReviewedArtifacts = new List<ReviewArtifactXml>();
            }

            return rdReviewedArtifacts;
        }

        private Task UpdateReviewUserStatsXmlAsync(int reviewId, int userId, RDReviewedArtifacts rdReviewedArtifacts, IDbTransaction transaction = null)
        {
            string xmlString = ReviewRawDataHelper.GetStoreData(rdReviewedArtifacts);

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
                resultTask = _connectionWrapper.ExecuteAsync("UpdateReviewUserStats", parameters, commandType: CommandType.StoredProcedure);
            }
            else
            {
                resultTask = transaction.Connection.ExecuteAsync("UpdateReviewUserStats", parameters, transaction, commandType: CommandType.StoredProcedure);
            }

            return resultTask;
        }

        public async Task<QueryResult<ParticipantArtifactStats>> GetReviewParticipantArtifactStatsAsync(int reviewId, int participantId, int userId, Pagination pagination)
        {
            var reviewInfo = await _artifactVersionsRepository.GetVersionControlArtifactInfoAsync(reviewId, null, userId);

            if (reviewInfo.VersionCount == 0 || reviewInfo.IsDeleted) //Review never published or deleted
            {
                ThrowReviewNotFoundException(reviewId);
            }

            pagination.SetDefaultValues(0, 50);

            var reviewedArtifactResult = await GetParticipantReviewedArtifactsAsync(reviewId, userId, participantId, pagination, true, addDrafts: true);

            return new QueryResult<ParticipantArtifactStats>()
            {
                Total = reviewedArtifactResult.Total,
                Items = reviewedArtifactResult.Items.Select(ra => (ParticipantArtifactStats) ra)
            };
        }

        private void UnauthorizedItem(ReviewTableOfContentItem item)
        {
            item.Name = UNATHORIZED; // unauthorize
            item.Included = false;
            item.HasAccess = false;
            item.IsApprovalRequired = false;
        }

        private static void ThrowUserCannotAccessReviewException(int reviewId)
        {
            var errorMessage = I18NHelper.FormatInvariant("User does not have permissions to access the review (Id:{0}).", reviewId);
            throw new AuthorizationException(errorMessage, ErrorCodes.UnauthorizedAccess);
        }

        private static void ThrowUserCannotAccessArtifactInTheReviewException(int projectId)
        {
            var errorMessage = I18NHelper.FormatInvariant("Artifacts could not be updated because they are no longer accessible in this project (Id:{0}).", projectId);
            throw new ResourceNotFoundException(errorMessage, ErrorCodes.ArtifactNotFound);
        }

        private static void ThrowReviewNotFoundException(int reviewId, int? revisionId = null)
        {
            var errorMessage = revisionId.HasValue ?
                I18NHelper.FormatInvariant("Review (Id:{0}) or its revision (#{1}) is not found.", reviewId, revisionId) :
                I18NHelper.FormatInvariant("Review (Id:{0}) is not found.", reviewId);
            throw new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
        }

        public static void ThrowApprovalRequiredIsReadonlyForReview()
        {
            var errorMessage = I18NHelper.FormatInvariant("The artifact could not be updated because another user has changed the Review status.");
            throw new BadRequestException(errorMessage, ErrorCodes.ApprovalRequiredIsReadonlyForReview);
        }
        public static void ThrowApprovalStatusIsReadonlyForReview()
        {
            var errorMessage = I18NHelper.FormatInvariant("The approval status could not be updated because another user has changed the Review status.");
            throw new BadRequestException(errorMessage, ErrorCodes.ApprovalRequiredIsReadonlyForReview);
        }

        public static void ThrowApprovalRequiredArtifactNotInReview()
        {
            var errorMessage = I18NHelper.FormatInvariant("The artifact could not be updated because it has been removed from review.");
            throw new BadRequestException(errorMessage, ErrorCodes.ApprovalRequiredArtifactNotInReview);
        }

        private static void ThrowReviewClosedException()
        {
            var errorMessage = "This Review is now closed. No modifications can be made to its artifacts or participants.";
            throw new BadRequestException(errorMessage, ErrorCodes.ReviewClosed);
        }
    }
}

