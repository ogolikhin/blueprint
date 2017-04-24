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
    public class SqlReviewsRepository
    {
        internal readonly ISqlConnectionWrapper ConnectionWrapper;

        private readonly IArtifactVersionsRepository _artifactVersionsRepository;

        public SqlReviewsRepository(): this(new SqlConnectionWrapper(ServiceConstants.RaptorMain), new SqlArtifactVersionsRepository())
        {
        }

        public SqlReviewsRepository(ISqlConnectionWrapper connectionWrapper, IArtifactVersionsRepository artifactVersionsRepository)
        {
            ConnectionWrapper = connectionWrapper;
            _artifactVersionsRepository = artifactVersionsRepository;
        }

        private async Task<ReviewContainer> GetReviewAsync(int reviewId, int userId, int revisionId)
        {
            var param = new DynamicParameters();
            param.Add("@reviewId", reviewId);
            param.Add("@userId", userId);
            param.Add("@revisionId", revisionId);

            var result = await ConnectionWrapper.QueryMultipleAsync<ReviewSource, int, ReviewArtifactsStatus>(
                "GetReviewDetails", param,
                commandType: CommandType.StoredProcedure);

            return new ReviewContainer
            {
                Id = reviewId,
                Source = result.Item1.SingleOrDefault(),
                TotalArtifacts = result.Item2.SingleOrDefault(),
                ArtifactsStatus = result.Item3.SingleOrDefault(),
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
            var reviewContainer = await GetReviewAsync(containerId, userId, int.MaxValue);
            reviewContainer.Name = artifactInfo.Name;
            //TODO Description
            return reviewContainer;
        }
    }    
}