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

        internal const string GetArtifactIdsInCollectionQuery =
            "SELECT * FROM [dbo].[GetArtifactIdsInCollection](@userId, @collectionId, @addDrafts)";

        public SqlCollectionsRepository() : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        public SqlCollectionsRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public async Task<IReadOnlyList<int>> GetContentArtifactIdsAsync(int collectionId, int userId, bool addDrafts = true)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            parameters.Add("@collectionId", collectionId);
            parameters.Add("@addDrafts", addDrafts);

            var result = await _connectionWrapper.QueryAsync<int>(
                GetArtifactIdsInCollectionQuery,
                parameters,
                commandType: CommandType.Text);

            return result.ToList();
        }


        public async Task<AssignArtifactsResult> AddArtifactsToCollectionAsync(int userId, int collectionId, List<int> artifactIds, IDbTransaction transaction = null)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@UserId", userId);
            parameters.Add("@CollectionId", collectionId);
            parameters.Add("@ArtifactIds", SqlConnectionWrapper.ToDataTable(artifactIds, "Int32Collection", "Int32Value"));

            if (transaction == null)
            {
                return (await _connectionWrapper.QueryAsync<AssignArtifactsResult>("AddArtifactsToCollection", parameters, commandType: CommandType.StoredProcedure)).FirstOrDefault();
            }
            else
            {
                return (await transaction.Connection.QueryAsync<AssignArtifactsResult>("AddArtifactsToCollection", parameters, transaction, commandType: CommandType.StoredProcedure)).FirstOrDefault();
            }
        }

        public async Task RemoveDeletedArtifactsFromCollection(int collectionId, int userId, IDbTransaction transaction = null)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@UserId", userId);
            parameters.Add("@CollectionId", collectionId);

            if (transaction == null)
            {
                await _connectionWrapper.ExecuteAsync("RemoveDeletedArtifactsFromCollection", parameters, commandType: CommandType.StoredProcedure);
            }
            else
            {
                await transaction.Connection.ExecuteAsync("RemoveDeletedArtifactsFromCollection", parameters, transaction, commandType: CommandType.StoredProcedure);
            }
        }
    }
}
