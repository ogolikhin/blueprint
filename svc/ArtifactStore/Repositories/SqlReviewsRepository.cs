using ArtifactStore.Models;
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

        private async Task<ReviewContainer> GetReviewAsync(int reviewId, int userId, int revisionId)
        {
            var param = new DynamicParameters();
            param.Add("@reviewId", reviewId);
            param.Add("@userId", userId);
            param.Add("@revisionId", revisionId);

            var result = await ConnectionWrapper.QueryMultipleAsync<int?, int, ReviewStatus, ReviewArtifactsStatus>(
                "GetReviewDetails", param,
                commandType: CommandType.StoredProcedure);
            var reviewSource = new ReviewSource();
            var baselineId = result.Item1.SingleOrDefault();
            if (baselineId.HasValue)
            {
                var artifactInfo = await _artifactVersionsRepository.GetVersionControlArtifactInfoAsync(baselineId.Value, null, userId);
                reviewSource.Id = artifactInfo.Id;
                reviewSource.Name = artifactInfo.Name;
                reviewSource.Prefix = artifactInfo.Prefix;
            }

            return new ReviewContainer
            {
                Id = reviewId,
                Source = reviewSource,
                TotalArtifacts = result.Item2.SingleOrDefault(),
                Status = result.Item3.SingleOrDefault(),
                ArtifactsStatus = result.Item4.SingleOrDefault(),
                ReviewType = result.Item1.SingleOrDefault() == null? ReviewType.Informal: ReviewType.Formal
            };
        }

        public async Task<ReviewContainer> GetReviewContainerAsync(int containerId, int userId)
        {
            var artifactInfo = await _artifactVersionsRepository.GetVersionControlArtifactInfoAsync(containerId, null, userId);
            if (artifactInfo.IsDeleted || artifactInfo.PredefinedType != ItemTypePredefined.ArtifactReviewPackage)
            {
                string errorMessage = I18NHelper.FormatInvariant("Review (Id:{0}) is not found.", containerId);
                throw new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
            }
            var reviewer = await GetReviewer(containerId, userId);
            if (reviewer == null)
            {
                string errorMessage = I18NHelper.FormatInvariant("User does not have permissions to access the review (Id:{0}).", containerId);
                throw new AuthorizationException(errorMessage, ErrorCodes.UnauthorizedAccess);
            }

            var reviewContainer = await GetReviewAsync(containerId, userId, int.MaxValue);
            reviewContainer.Name = artifactInfo.Name;
            //TODO Description
            return reviewContainer;
        }

        public async Task<ReviewContent> GetContentAsync(int reviewId, int userId, int? offset, int? limit, int? versionId = null, bool? addDrafts = true)
        {
            var reviewArtifacts = await GetReviewArtifactsAsync(reviewId, userId, offset, limit, versionId, addDrafts);
            var reviewArtifactIds = reviewArtifacts.Items.Select(a => a.Id).ToList();

            reviewArtifactIds.Add(reviewId);

            var artifactPermissionsDictionary = await _artifactPermissionsRepository.GetArtifactPermissions(reviewArtifactIds, userId);

            if (!SqlArtifactPermissionsRepository.HasPermissions(reviewId, artifactPermissionsDictionary, RolePermissions.Read))
            {
                string errorMessage = I18NHelper.FormatInvariant("User does not have permissions to access the review (Id:{0}).", reviewId);
                throw new AuthorizationException(errorMessage, ErrorCodes.UnauthorizedAccess);
            }

            var reviewArtifactStatuses = await GetReviewArtifactStatusesAsync(reviewId, userId, offset, limit, versionId, addDrafts, reviewArtifactIds);
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
                    reviewArtifact.IsApprovalRequired = false;
                    reviewArtifact.HasComments = false;
                    reviewArtifact.ItemTypePredefined = 0;
                    reviewArtifact.HasAccess = false;
                }
            }
            return reviewArtifacts;
        }

        private async Task<ReviewContent> GetReviewArtifactsAsync(int reviewId, int userId, int? offset, int? limit, int? versionId = null, bool? addDrafts = true)
        {
            int? revisionId = await _itemInfoRepository.GetRevisionId(reviewId, userId, versionId);
            if (revisionId < int.MaxValue) {
                addDrafts = false;
            }
            var param = new DynamicParameters();
            param.Add("@reviewId", reviewId);
            param.Add("@offset", offset);
            param.Add("@limit", limit);
            param.Add("@revisionId", revisionId);
            param.Add("@addDrafts", addDrafts);
            param.Add("@userId", userId);
            var result = await ConnectionWrapper.QueryMultipleAsync<ReviewArtifact, int>("GetReviewArtifacts", param, commandType: CommandType.StoredProcedure);
            return new ReviewContent
            {
                Items = result.Item1.ToList(),
                Total = result.Item2.SingleOrDefault()
            };
        }

        private async Task<ContentStatusDetails> GetReviewArtifactStatusesAsync(int reviewId, int userId, int? offset, int? limit,
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
            param.Add("@offset", offset);
            param.Add("@limit", limit);
            param.Add("@revisionId", revisionId);
            param.Add("@addDrafts", addDrafts);
            param.Add("@userId", userId);
            param.Add("@itemIds", SqlConnectionWrapper.ToDataTable(reviewArtifactIds, "Int32Collection"));
            var result = await ConnectionWrapper.QueryMultipleAsync<ReviewArtifactStatus, int>("GetReviewArtifactsStatus", param, commandType: CommandType.StoredProcedure);
            return new ContentStatusDetails
            {
                ItemStatuses = result.Item1.ToList(),
                NumUsers = result.Item2.SingleOrDefault()
            };
        }

        /// <summary>
        /// Returns reviewer basic information:
        ///     UserId
        ///     Role
        /// </summary>
        /// <param name="reviewId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<Reviewer> GetReviewer(int reviewId, int userId)
        {
            var param = new DynamicParameters();
            param.Add("@reviewId", reviewId);
            param.Add("@userId", userId);
            return (await ConnectionWrapper.QueryAsync<Reviewer>(
                "GetReviewer", param,
                commandType: CommandType.StoredProcedure)).SingleOrDefault();
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
            var participants = await ConnectionWrapper.QueryMultipleAsync<Reviewer, int>("GetReviewParticipants", param, commandType: CommandType.StoredProcedure);
            var reviewersRoot = new ReviewParticipantsContent()
            {
                Items = participants.Item1.ToList(),
                Total = participants.Item2.SingleOrDefault()
            };
            return reviewersRoot;
        }
    }    
}