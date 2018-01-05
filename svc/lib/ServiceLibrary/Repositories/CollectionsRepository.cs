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

        public CollectionsRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        public CollectionsRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
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
                    case (int)SqlErrorCodes.CollectionDoesNotExist:
                        throw new ResourceNotFoundException(ErrorMessages.CollectionDoesNotExist, ErrorCodes.ResourceNotFound);
                    case (int)SqlErrorCodes.GeneralSqlError:
                        throw new Exception(ErrorMessages.GeneralErrorOfAddingArtifactsToCollection);

                }
            }

            return result.FirstOrDefault();
        }
    }
}
