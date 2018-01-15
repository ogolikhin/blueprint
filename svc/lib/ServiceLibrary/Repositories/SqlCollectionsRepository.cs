using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace ServiceLibrary.Repositories
{
    public class SqlCollectionsRepository : ICollectionsRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;
        private readonly IArtifactRepository _artifactRepository;
        private readonly ISqlHelper _sqlHelper;

        public SqlCollectionsRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain),
                   new SqlArtifactRepository(),
                   new SqlHelper())
        {
        }

        public SqlCollectionsRepository(ISqlConnectionWrapper connectionWrapper, IArtifactRepository artifactRepository, ISqlHelper sqlHelper)
        {
            _connectionWrapper = connectionWrapper;
            _artifactRepository = artifactRepository;
            _sqlHelper = sqlHelper;
        }

        public async Task<AssignArtifactsResult> AddArtifactsToCollectionAsync(int userId, int collectionId, List<int> artifactIds)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@UserId", userId);
            parameters.Add("@CollectionId", collectionId);
            parameters.Add("@ArtifactIds", SqlConnectionWrapper.ToDataTable(artifactIds, "Int32Collection", "Int32Value"));
            var result = await _connectionWrapper.QueryAsync<AssignArtifactsResult>("AddArtifactsToCollection", parameters, commandType: CommandType.StoredProcedure);
            return result.FirstOrDefault();
        }

        public async Task<ArtifactBasicDetails> GetCollectionInfoAsync(int userId, int collectionId)
        {
            var collectionDetails = await _artifactRepository.GetArtifactBasicDetails(collectionId, userId);

            if (collectionDetails == null)
            {
                throw new ResourceNotFoundException(ErrorMessages.CollectionDoesNotExist, ErrorCodes.ResourceNotFound);
            }

            if (collectionDetails.RevisionId != int.MaxValue)
            {
                var errorMessage = I18NHelper.FormatInvariant(ErrorMessages.CollectionInRevisionDoesNotExist, int.MaxValue);
                throw new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
            }

            if (collectionDetails.PrimitiveItemTypePredefined != (int)ItemTypePredefined.ArtifactCollection)
            {
                throw new ResourceNotFoundException(ErrorMessages.CollectionDoesNotExist, ErrorCodes.IncorrectType);
            }

            if (collectionDetails.LockedByUserId != null // this clause is missing in Raptor - this means collection is not published
                && collectionDetails.LockedByUserId != userId)
            {
                var errorMessage = I18NHelper.FormatInvariant(ErrorMessages.CollectionIsLockedByAnotherUser, collectionId, collectionDetails.LockedByUserId);
                throw new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
            }

            return collectionDetails;
        }

        public async Task RemoveDeletedArtifactsFromCollection(int collectionId, int userId)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@UserId", userId);
            parameters.Add("@CollectionId", collectionId);

            await _connectionWrapper.ExecuteAsync("RemoveDeletedArtifactsFromCollection", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task RunInTransactionAsync(Func<IDbTransaction, Task> action)
        {
            await _sqlHelper.RunInTransactionAsync(ServiceConstants.RaptorMain, action);
        }
    }
}
