using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace ServiceLibrary.Repositories
{
    public class CollectionsRepository : ICollectionsRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;
        private readonly IArtifactRepository _artifactRepository;
        private readonly ISqlHelper _sqlHelper;

        public CollectionsRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain),
                   new SqlArtifactRepository(),
                   new SqlHelper())
        {
        }

        public CollectionsRepository(ISqlConnectionWrapper connectionWrapper, IArtifactRepository artifactRepository, ISqlHelper sqlHelper)
        {
            _connectionWrapper = connectionWrapper;
            _artifactRepository = artifactRepository;
            _sqlHelper = sqlHelper;
        }

        public async Task<AssignArtifactsResult> AddArtifactsToCollectionAsync(int userId, int collectionId, OperationScope scope)
        {
            if (userId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(userId));
            }

            if (collectionId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(collectionId));
            }

            var parameters = new DynamicParameters();

            parameters.Add("@UserId", userId);
            parameters.Add("@CollectionId", collectionId);
            parameters.Add("@ArtifactsIds", SqlConnectionWrapper.ToDataTable(scope.Ids, "Int32Collection", "Int32Value"));
            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await _connectionWrapper.ExecuteAsync("RemoveDeletedArtifactsFromCollection", parameters, commandType: CommandType.StoredProcedure);

            var result = await _connectionWrapper.QueryAsync<AssignArtifactsResult>("AddArtifactsToCollection", parameters, commandType: CommandType.StoredProcedure);
            var errorCode = parameters.Get<int?>("ErrorCode");

            if (errorCode.HasValue)
            {
                switch (errorCode.Value)
                {
                    case (int)SqlErrorCodes.UserLoginNotExist:
                        throw new ResourceNotFoundException(ErrorMessages.UserNotExist, ErrorCodes.ResourceNotFound);
                    // case (int)SqlErrorCodes.CollectionDoesNotExist:
                    //    throw new ResourceNotFoundException(ErrorMessages.CollectionDoesNotExist, ErrorCodes.ResourceNotFound);
                    case (int)SqlErrorCodes.GeneralSqlError:
                        throw new Exception(ErrorMessages.GeneralErrorOfAddingArtifactsToCollection);

                }
            }

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

        public async Task RunInTransactionAsync(Func<IDbTransaction, Task> action)
        {
            await _sqlHelper.RunInTransactionAsync(ServiceConstants.RaptorMain, action);
        }
    }
}
