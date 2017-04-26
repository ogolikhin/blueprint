using ArtifactStore.Models.Review;
using Dapper;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
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

        public SqlReviewsRepository(): this(new SqlConnectionWrapper(ServiceConstants.RaptorMain), new SqlArtifactVersionsRepository())
        {
        }

        public SqlReviewsRepository(ISqlConnectionWrapper connectionWrapper, IArtifactVersionsRepository artifactVersionsRepository)
        {
            ConnectionWrapper = connectionWrapper;
            _artifactVersionsRepository = artifactVersionsRepository;
            _itemInfoRepository = new SqlItemInfoRepository(connectionWrapper);
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
                throw new ResourceNotFoundException();
            }

            var reviewer = await GetReviewer(containerId, userId);
            if (reviewer == null)
            {
                throw new AuthorizationException();
            }

            var reviewContainer = await GetReviewAsync(containerId, userId, int.MaxValue);
            reviewContainer.Name = artifactInfo.Name;
            //TODO Description
            return reviewContainer;
        }

        public async Task<ReviewContent> GetContentAsync(int reviewId, int userId, int? offset, int? limit, int? versionId = null, bool? addDrafts = true)
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
    }    
}