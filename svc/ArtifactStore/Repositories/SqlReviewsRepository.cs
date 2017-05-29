using ArtifactStore.Models.Review;
using Dapper;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ArtifactStore.Repositories
{
    public class SqlReviewsRepository: IReviewsRepository
    {
        internal readonly ISqlConnectionWrapper ConnectionWrapper;

        private readonly IArtifactVersionsRepository _artifactVersionsRepository;

        private readonly ISqlItemInfoRepository _itemInfoRepository;

        private readonly IArtifactPermissionsRepository _artifactPermissionsRepository;

        public SqlReviewsRepository(): this(new SqlConnectionWrapper(ServiceConstants.RaptorMain), 
                                            new SqlArtifactVersionsRepository(), 
                                            new SqlItemInfoRepository(),
                                            new SqlArtifactPermissionsRepository())
        {
        }

        public SqlReviewsRepository(ISqlConnectionWrapper connectionWrapper, 
                                    IArtifactVersionsRepository artifactVersionsRepository, 
                                    ISqlItemInfoRepository itemInfoRepository,
                                    IArtifactPermissionsRepository artifactPermissionsRepository)
        {
            ConnectionWrapper = connectionWrapper;
            _artifactVersionsRepository = artifactVersionsRepository;
            _itemInfoRepository = itemInfoRepository;
            _artifactPermissionsRepository = artifactPermissionsRepository;
        }

        public async Task<ReviewSummary> GetReviewSummary(int containerId, int userId)
        {
            var reviewInfo = await _artifactVersionsRepository.GetVersionControlArtifactInfoAsync(containerId, null, userId);
            if (reviewInfo.IsDeleted || reviewInfo.PredefinedType != ItemTypePredefined.ArtifactReviewPackage)
            {
                string errorMessage = I18NHelper.FormatInvariant("Review (Id:{0}) is not found.", containerId);
                throw new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
            }

            var reviewDetails = await GetReviewSummaryDetails(containerId, userId);

            if (reviewDetails.ReviewPackageStatus == ReviewPackageStatus.Draft)
            {
                string errorMessage = I18NHelper.FormatInvariant("Review (Id:{0}) is not found.", containerId);
                throw new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
            }

            if (!reviewDetails.ReviewParticipantRole.HasValue && reviewDetails.TotalReviewers > 0)
            {
                string errorMessage = I18NHelper.FormatInvariant("User does not have permissions to access the review (Id:{0}).", containerId);
                throw new AuthorizationException(errorMessage, ErrorCodes.UnauthorizedAccess);
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
                string errorMessage = I18NHelper.FormatInvariant("User does not have permissions to access the review (Id:{0}).", reviewId);
                throw new AuthorizationException(errorMessage, ErrorCodes.UnauthorizedAccess);
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
                } else {
                    reviewArtifact.Name = string.Empty;
                    reviewArtifact.ItemTypeId = 0;
                    reviewArtifact.Prefix = null;
                    reviewArtifact.IsApprovalRequired = false;
                    reviewArtifact.HasComments = false;
                    reviewArtifact.ItemTypePredefined = 0;
                    reviewArtifact.IconImageId = null;
                    reviewArtifact.HasAccess = false;
                }
            }
            return reviewArtifacts;
        }

        public Task<AddArtifactsResult> AddArtifactsToReview(int reviewId, int userId, AddArtifactsParameter content)
        {
            //TODO
            return Task.FromResult(new AddArtifactsResult
            {
                ArtifactCount = 1,
                AlreadyIncludedArtifactCount = 1,
                NonexistentArtifactCount = 1,
                UnpublishedArtifactCount = 1
            });
        }

        public async Task<QueryResult<ReviewedArtifact>> GetReviewedArtifacts(int reviewId, int userId, Pagination pagination, int revisionId)
        {
            var reviewArtifacts = await GetReviewArtifactsAsync<ReviewedArtifact>(reviewId, userId, pagination, revisionId, false);

            var reviewArtifactIds = reviewArtifacts.Items.Select(a => a.Id).ToList();
            reviewArtifactIds.Add(reviewId);

            var artifactPermissionsDictionary = await _artifactPermissionsRepository.GetArtifactPermissions(reviewArtifactIds, userId);

            if (!SqlArtifactPermissionsRepository.HasPermissions(reviewId, artifactPermissionsDictionary, RolePermissions.Read))
            {
                string errorMessage = I18NHelper.FormatInvariant("User does not have permissions to access the review (Id:{0}).", reviewId);
                throw new AuthorizationException(errorMessage, ErrorCodes.UnauthorizedAccess);
            }

            var reviewedArtifacts = (await GetReviewedArtifactsAsync(reviewArtifactIds, userId, reviewId, revisionId)).ToDictionary(k => k.Id);
            foreach (var artifact in reviewArtifacts.Items)
            {
                ReviewedArtifact reviewedArtifact;
                if (reviewedArtifacts.TryGetValue(artifact.Id, out reviewedArtifact))
                {
                    artifact.ViewState = reviewedArtifact.ViewState;
                    artifact.Approval = reviewedArtifact.Approval;
                    artifact.ApprovalFlag = reviewedArtifact.ApprovalFlag;
                    artifact.ArtifactVersion = reviewedArtifact.ArtifactVersion;
                    artifact.PublishedOnTimestamp = reviewedArtifact.PublishedOnTimestamp;
                    artifact.UserDisplayName = reviewedArtifact.UserDisplayName;
                    artifact.ViewedArtifactVersion = reviewedArtifact.ViewedArtifactVersion;
                    artifact.SignedOnTimestamp = reviewedArtifact.SignedOnTimestamp;
                    artifact.HasAttachments = reviewedArtifact.HasAttachments;
                    artifact.HasRelationships = reviewedArtifact.HasRelationships;
                    artifact.HasAttachments = reviewedArtifact.HasAttachments;
                }
            }

            return reviewArtifacts;
        }

        private Task<IEnumerable<ReviewedArtifact>> GetReviewedArtifactsAsync(IEnumerable<int> artifactIds, int userId, int reviewId, int revisionId)
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
            var param = new DynamicParameters();
            param.Add("@reviewId", reviewId);
            param.Add("@offset", pagination.Offset);
            param.Add("@limit", pagination.Limit);
            param.Add("@revisionId", revisionId);
            param.Add("@addDrafts", revisionId < int.MaxValue ? false : addDrafts);
            param.Add("@userId", userId);

            var result = await ConnectionWrapper.QueryMultipleAsync<T, int>("GetReviewArtifacts", param, commandType: CommandType.StoredProcedure);
            return new QueryResult<T>()
            {
                Items = result.Item1.ToList(),
                Total = result.Item2.SingleOrDefault()
            };
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


        private async Task<ReviewTableOfContent> GetTableOfContentAsync(int reviewId, int? revisionId, int userId, int? offset, int? limit)
        {
            
            var param = new DynamicParameters();
            param.Add("@reviewId", reviewId);
            param.Add("@offset", offset);
            param.Add("@limit", limit);
            param.Add("@revisionId", revisionId);
            param.Add("@userId", userId);

            var result = await ConnectionWrapper.QueryMultipleAsync<ReviewTableOfContentItem, int>("GetReviewTableOfContent", param, commandType: CommandType.StoredProcedure);

            return new ReviewTableOfContent
            {
                Items = result.Item1.ToList(),
                Total = result.Item2.SingleOrDefault()
            };
        }

    
        public async Task<ReviewTableOfContent> GetReviewTableOfContent(int reviewId, int revisionId, int userId, int? offset, int? limit)
        {

            //get all review content item in a hierachy list
            var toc = await GetTableOfContentAsync(reviewId, revisionId, userId, offset, limit);

            var artifactIds = new List<int>{reviewId}.Concat(toc.Items.Select(a => a.Id).ToList());

            //gets artifact permissions
            var artifactPermissionsDictionary = await _artifactPermissionsRepository.GetArtifactPermissions(artifactIds, userId);

            if (!SqlArtifactPermissionsRepository.HasPermissions(reviewId, artifactPermissionsDictionary, RolePermissions.Read))
            {
                string errorMessage = I18NHelper.FormatInvariant("User does not have permissions to access the review (Id:{0}).", reviewId);
                throw new AuthorizationException(errorMessage, ErrorCodes.UnauthorizedAccess);
            }


            //TODO: Update artifact statuses and permissions
            //
            foreach (var tocItem in toc.Items)
            {
                if (SqlArtifactPermissionsRepository.HasPermissions(tocItem.Id, artifactPermissionsDictionary, RolePermissions.Read))
                {
                    //TODO update item status
                    
                }
                else
                {
                    //not granted SES
                    //TODO: http://svmtfs2015:8080/tfs/svmtfs2015/Blueprint/_workitems?_a=edit&id=6593&fullScreen=false
                    tocItem.Included = false;
                    tocItem.Viewed = false;
                }
            }

            return toc;
        }
    }
}
