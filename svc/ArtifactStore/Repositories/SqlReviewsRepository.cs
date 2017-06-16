using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.Models.Review;
using Dapper;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using ArtifactStore.Helpers;

namespace ArtifactStore.Repositories
{
    public class SqlReviewsRepository: IReviewsRepository
    {
        internal readonly ISqlConnectionWrapper ConnectionWrapper;

        private readonly IArtifactVersionsRepository _artifactVersionsRepository;

        private readonly ISqlItemInfoRepository _itemInfoRepository;

        private readonly IArtifactPermissionsRepository _artifactPermissionsRepository;

        internal readonly IApplicationSettingsRepository _applicationSettingsRepository;

        internal const string ReviewArtifactHierarchyRebuildIntervalInMinutesKey = "ReviewArtifactHierarchyRebuildIntervalInMinutes";

        internal const int DefaultReviewArtifactHierarchyRebuildIntervalInMinutes = 20;

        private const string NOT_SPECIFIED = "Not Specified";

        private const string PENDING = "Pending";

        private const string UNATHORIZED = "Unauthorized";

        public SqlReviewsRepository(): this(new SqlConnectionWrapper(ServiceConstants.RaptorMain), 
                                            new SqlArtifactVersionsRepository(), 
                                            new SqlItemInfoRepository(),
                                            new SqlArtifactPermissionsRepository(),
                                            new ApplicationSettingsRepository())
        {
        }

        public SqlReviewsRepository(ISqlConnectionWrapper connectionWrapper, 
                                    IArtifactVersionsRepository artifactVersionsRepository, 
                                    ISqlItemInfoRepository itemInfoRepository,
                                    IArtifactPermissionsRepository artifactPermissionsRepository,
                                    IApplicationSettingsRepository applicationSettingsRepository)
        {
            ConnectionWrapper = connectionWrapper;
            _artifactVersionsRepository = artifactVersionsRepository;
            _itemInfoRepository = itemInfoRepository;
            _artifactPermissionsRepository = artifactPermissionsRepository;
            _applicationSettingsRepository = applicationSettingsRepository;
        }

        public async Task<ReviewSummary> GetReviewSummary(int containerId, int userId)
        {
            var reviewInfo = await _artifactVersionsRepository.GetVersionControlArtifactInfoAsync(containerId, null, userId);
            if (reviewInfo.IsDeleted || reviewInfo.PredefinedType != ItemTypePredefined.ArtifactReviewPackage)
            {
                ThrowReviewNotFoundException(containerId);
            }

            var reviewDetails = await GetReviewSummaryDetails(containerId, userId);

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
                ArtifactsStatus = new ReviewArtifactsStatus
                {
                    Approved = reviewDetails.Approved,
                    Disapproved = reviewDetails.Disapproved,
                    Viewed = reviewDetails.Viewed
                },
                ReviewType = reviewDetails.BaselineId.HasValue ? ReviewType.Formal : ReviewType.Informal,
                RevisionId = reviewDetails.RevisionId
            };
            return reviewContainer;
        }

        private async Task<ReviewSummaryDetails> GetReviewSummaryDetails(int reviewId, int userId)
        {
            var param = new DynamicParameters();
            param.Add("@reviewId", reviewId);
            param.Add("@userId", userId);

            return (await ConnectionWrapper.QueryAsync<ReviewSummaryDetails>(
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
            var artifactStatusDictionary = reviewArtifactStatuses.ItemStatuses.ToDictionary(a => a.ArtifactId);

            ReviewArtifactStatus reviewArtifactStatus;

            foreach (var reviewArtifact in reviewArtifacts.Items)
            {
                if (SqlArtifactPermissionsRepository.HasPermissions(reviewArtifact.Id, artifactPermissionsDictionary, RolePermissions.Read))
                {
                    if (artifactStatusDictionary.TryGetValue(reviewArtifact.Id, out reviewArtifactStatus))
                    {
                        reviewArtifact.Pending = reviewArtifactStatus.Pending;
                        reviewArtifact.Approved = reviewArtifactStatus.Approved;
                        reviewArtifact.Disapproved = reviewArtifactStatus.Disapproved;
                        reviewArtifact.Viewed = reviewArtifactStatus.Viewed;
                        reviewArtifact.Unviewed = reviewArtifactStatus.Unviewed;
                    } else {
                        reviewArtifact.Pending = numUsers;
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
            reviewArtifact.Prefix = null;
            reviewArtifact.HasComments = false;
            reviewArtifact.ItemTypePredefined = 0;
            reviewArtifact.IconImageId = null;
            reviewArtifact.HasAccess = false;
            reviewArtifact.IsApprovalRequired = false;
        }

        public async Task<AddArtifactsResult> AddArtifactsToReviewAsync (int reviewId, int userId, AddArtifactsParameter content)
        {
            if (content.ArtifactIds == null || content.ArtifactIds.Count() == 0)
            {
                throw new BadRequestException("There is nothing to add to review.", ErrorCodes.OutOfRangeParameter);
            }

            int alreadyIncludedCount;
            var propertyResult = await GetReviewPropertyString(reviewId, userId, content);

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

            return new AddArtifactsResult() {
                ArtifactCount = effectiveIds.ArtifactIds.Count() - alreadyIncludedCount,
                AlreadyIncludedArtifactCount = alreadyIncludedCount,
                NonexistentArtifactCount = effectiveIds.Nonexistent,
                UnpublishedArtifactCount = effectiveIds.Unpublished
            };
        }


        private async Task<PropertyValueString> GetReviewPropertyString(int reviewId, int userId, AddArtifactsParameter content)
        {
            var param = new DynamicParameters();
            param.Add("@reviewId", reviewId);
            param.Add("@userId", userId);

            return (await ConnectionWrapper.QueryAsync<PropertyValueString>("GetReviewPropertyString", param, commandType: CommandType.StoredProcedure)).SingleOrDefault();
        }

        private async Task<EffectiveArtifactIdsResult> GetEffectiveArtifactIds(int userId, AddArtifactsParameter content, int projectId)
        {
            var param = new DynamicParameters();
            param.Add("@artifactIds", SqlConnectionWrapper.ToDataTable(content.ArtifactIds));
            param.Add("@userId", userId);
            param.Add("@projectId", projectId);

            var result = await ConnectionWrapper.QueryMultipleAsync<int, int, int, int>("GetEffectiveArtifactIds", param, commandType: CommandType.StoredProcedure);
            return new EffectiveArtifactIdsResult()
            {
                ArtifactIds = result.Item1.ToList(),
                Unpublished = result.Item2.SingleOrDefault(),
                Nonexistent = result.Item3.SingleOrDefault(),
                ProjectMoved = result.Item4.SingleOrDefault()
            };
        }

        private string AddArtifactsToXML (string xmlArtifacts, ISet<int> artifactsToAdd, out int alreadyIncluded)
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
                        ApprovalNotRequested = false

                    };
                    rdReviewContents.Artifacts.Add(addedArtifact);
                }
                else
                    ++alreadyIncluded;
            }

            return ReviewRawDataHelper.GetStoreData(rdReviewContents);
        }

        public async Task<QueryResult<ReviewedArtifact>> GetReviewedArtifacts(int reviewId, int userId, Pagination pagination, int revisionId)
        {
            var reviewArtifacts = await GetReviewArtifactsAsync<ReviewedArtifact>(reviewId, userId, pagination, revisionId, false);

            var reviewArtifactIds = reviewArtifacts.Items.Select(a => a.Id).ToList();

            var artifactPermissionsDictionary = await _artifactPermissionsRepository
                .GetArtifactPermissions(reviewArtifactIds.Union(new [] { reviewId }), userId);

            if (!SqlArtifactPermissionsRepository.HasPermissions(reviewId, artifactPermissionsDictionary, RolePermissions.Read))
            {
                ThrowUserCannotAccessReviewException(reviewId);
            }

            var reviewedArtifacts = (await GetReviewArtifactsByParticipant(reviewArtifactIds, userId, reviewId, revisionId)).ToDictionary(k => k.Id);
            foreach (var artifact in reviewArtifacts.Items)
            {
                if (SqlArtifactPermissionsRepository.HasPermissions(artifact.Id, artifactPermissionsDictionary, RolePermissions.Read))
                {
                    ReviewedArtifact reviewedArtifact;
                    if (reviewedArtifacts.TryGetValue(artifact.Id, out reviewedArtifact))
                    {
                        artifact.Approval = GetApprovalStatus(reviewedArtifact, artifact.IsApprovalRequired);
                        artifact.ApprovalFlag = reviewedArtifact.ApprovalFlag;
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
            
            return ConnectionWrapper.QueryAsync<ReviewedArtifact>("GetReviewArtifactsByParticipant", param, commandType: CommandType.StoredProcedure);
        }

        private async Task<QueryResult<T>> GetReviewArtifactsAsync<T>(int reviewId, int userId, Pagination pagination, int? revisionId = null, bool? addDrafts = true)
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

            var result = await ConnectionWrapper.QueryAsync<T>("GetReviewArtifacts", param, commandType: CommandType.StoredProcedure);
            return new QueryResult<T>()
            {
                Items = result.ToList(),
                Total = param.Get<int>("@numResult")
            };
        }

        private async Task<int> UpdateReviewArtifacts(int reviewId, int userId, string xmlArtifacts)
        {
            var param = new DynamicParameters();
            param.Add("@reviewId", reviewId);
            param.Add("@userId", userId);
            param.Add("@xmlArtifacts", xmlArtifacts);

            return await ConnectionWrapper.ExecuteAsync("UpdateReviewArtifacts", param, commandType: CommandType.StoredProcedure);
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
            var result = await ConnectionWrapper.QueryMultipleAsync<ReviewArtifactStatus, int>("GetReviewArtifactsStatus", param, commandType: CommandType.StoredProcedure);
            return new ContentStatusDetails
            {
                ItemStatuses = result.Item1.ToList(),
                NumUsers = result.Item2.SingleOrDefault()
            };
        }

        public async Task<ReviewParticipantsContent> GetReviewParticipantsAsync(int reviewId, int? offset, int? limit, int userId, int? versionId = null, bool? addDrafts = true)
        {
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
            var participants = await ConnectionWrapper.QueryMultipleAsync<ReviewParticipant, int, int, int>("GetReviewParticipants", param, commandType: CommandType.StoredProcedure);
            var reviewersRoot = new ReviewParticipantsContent()
            {
                Items = participants.Item1.ToList(),
                Total = participants.Item2.SingleOrDefault(),
                TotalArtifacts = participants.Item3.SingleOrDefault(),
                TotalArtifactsRequestedApproval = participants.Item4.SingleOrDefault()
            };
            return reviewersRoot;
        }
        public async Task<ArtifactReviewContent> GetReviewArtifactStatusesByParticipant(int artifactId, int reviewId, int? offset, int? limit, int userId, int? versionId = null, bool? addDrafts = true)
        {
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
            var participants = await ConnectionWrapper.QueryMultipleAsync<ReviewArtifactDetails, int>("GetReviewArtifactStatusesByParticipant", param, commandType: CommandType.StoredProcedure);
            var reviewersRoot = new ArtifactReviewContent()
            {
                Items = participants.Item1.ToList(),
                Total = participants.Item2.SingleOrDefault()
            };
            return reviewersRoot;
        }

        public async Task<AddParticipantsResult> AddParticipantsToReviewAsync(int reviewId, int userId, AddParticipantsParameter content)
        {
            var TotalUsers = 3; // For testing purpose. Needs to be changed 
            var AlreadyIncludedUsers = 1; // For testing purpose. Needs to be changed 
            string xmlResult = "";

            //TODO: Validate content parameters

            //TODO: implement the loginc to add participants to review

            await UpdateReviewParticipants(reviewId, userId, xmlResult);
            return new AddParticipantsResult
            {
                ParticipantCount = TotalUsers,
                AlreadyIncludedCount = AlreadyIncludedUsers
            };
            
        }

        private async Task<int> UpdateReviewParticipants(int reviewId, int userId, string xmlString)
        {
            var param = new DynamicParameters();
            param.Add("@reviewId", reviewId);
            param.Add("@userId", userId);
            param.Add("@xmlString", xmlString);

            return await ConnectionWrapper.ExecuteAsync("UpdateReviewParticipants", param, commandType: CommandType.StoredProcedure);
        }

        private async Task<ReviewTableOfContent> GetTableOfContentAsync(int reviewId, int revisionId, int userId, Pagination pagination)
        {
            int refreshInterval = await GetRebuildReviewArtifactHierarchyInterval();
            var param = new DynamicParameters();
            param.Add("@reviewId", reviewId);
            param.Add("@offset", pagination.Offset);
            param.Add("@limit", pagination.Limit);
            param.Add("@revisionId", revisionId);
            //param.Add("@addDrafts", false);
            param.Add("@userId", userId);
            param.Add("@refreshInterval", refreshInterval);
            param.Add("@total", dbType: DbType.Int32, direction: ParameterDirection.Output);
            param.Add("@retResult", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);


            var result = await ConnectionWrapper.QueryAsync<ReviewTableOfContentItem>("GetReviewTableOfContent", param, commandType: CommandType.StoredProcedure);
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

            return new ReviewTableOfContent
            {
                Items = result.ToList(),
                Total = param.Get<int>("@total")
            };
        }


        
        public async Task<ReviewTableOfContent> GetReviewTableOfContent(int reviewId, int revisionId, int userId, Pagination pagination)
        {

            //get all review content item in a hierarchy list
            var toc = await GetTableOfContentAsync(reviewId, revisionId, userId, pagination);

            var artifactIds = new List<int>{reviewId}.Concat(toc.Items.Select(a => a.Id).ToList());

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
                    tocItem.ApprovalStatus = (ApprovalType)artifact?.ApprovalFlag;
                    tocItem.Viewed = artifact?.ViewedArtifactVersion != null;
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

        public Task AssignApprovalRequiredToArtifacts(int reviewId, int userId, AssignArtifactsApprovalParameter content)
        {
            return new Task(() => { });
        }

        private void UnauthorizedItem(ReviewTableOfContentItem item)
        {
            item.Name = UNATHORIZED; // unauthorize
            item.Included = false;
            item.Viewed = false;
            item.HasAccess = false;
            item.IsApprovalRequired = false;
        }

        private static void ThrowUserCannotAccessReviewException(int reviewId)
        {
            var errorMessage = I18NHelper.FormatInvariant("User does not have permissions to access the review (Id:{0}).", reviewId);
            throw new AuthorizationException(errorMessage, ErrorCodes.UnauthorizedAccess);
        }

        private static void ThrowReviewNotFoundException(int reviewId, int? revisionId = null)
        {
            var errorMessage = revisionId.HasValue ? 
                I18NHelper.FormatInvariant("Review (Id:{0}) or its revision (#{1}) is not found.", reviewId, revisionId) :
                I18NHelper.FormatInvariant("Review (Id:{0}) is not found.", reviewId);
            throw new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
        }
    }
}

