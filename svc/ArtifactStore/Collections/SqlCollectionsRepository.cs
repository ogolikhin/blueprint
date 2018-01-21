using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.Collections.Models;
using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Collections
{
    public class SqlCollectionsRepository : ICollectionsRepository
    {
        internal const string GetArtifactIdsInCollectionQuery =
            "SELECT * FROM [dbo].[GetArtifactIdsInCollection](@userId, @collectionId, @addDrafts)";

        private readonly ISqlConnectionWrapper _connectionWrapper;

        public SqlCollectionsRepository() : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        public SqlCollectionsRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public async Task<IReadOnlyList<CollectionArtifact>> GetArtifactsWithPropertyValuesAsync(
            int userId, IEnumerable<int> artifactIds)
        {
            var propertyTypePredefineds = new List<int>
            {
                (int)PropertyTypePredefined.ArtifactType,
                (int)PropertyTypePredefined.ID
            }; // ArtifactType = 4148, ID = 4097

            // TODO: should be filled with real data after implementation of getting list of property type ids from profile settings.
            var propertyTypeIds = new List<int>();

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId, DbType.Int32);
            parameters.Add("@AddDrafts", true, DbType.Boolean);
            parameters.Add("@ArtifactIds", SqlConnectionWrapper.ToDataTable(artifactIds));
            parameters.Add("@PropertyTypePredefineds", SqlConnectionWrapper.ToDataTable(propertyTypePredefineds));
            parameters.Add("@PropertyTypeIds", SqlConnectionWrapper.ToDataTable(propertyTypeIds));

            var result = await _connectionWrapper.QueryAsync<CollectionArtifact>(
                "GetPropertyValuesForArtifacts", parameters, commandType: CommandType.StoredProcedure);

            return result.ToList();
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

        public async Task<int> AddArtifactsToCollectionAsync(int collectionId, IEnumerable<int> artifactIds, int userId,
            IDbTransaction transaction = null)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@UserId", userId);
            parameters.Add("@CollectionId", collectionId);
            parameters.Add("@ArtifactIds", SqlConnectionWrapper.ToDataTable(artifactIds, "Int32Collection", "Int32Value"));

            if (transaction == null)
            {
                return (await _connectionWrapper.QueryAsync<int>("AddArtifactsToCollection", parameters, commandType: CommandType.StoredProcedure)).FirstOrDefault();
            }
            else
            {
                return (await transaction.Connection.QueryAsync<int>("AddArtifactsToCollection", parameters, transaction, commandType: CommandType.StoredProcedure)).FirstOrDefault();
            }
        }

        public async Task RemoveDeletedArtifactsFromCollectionAsync(int collectionId, int userId, IDbTransaction transaction = null)
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