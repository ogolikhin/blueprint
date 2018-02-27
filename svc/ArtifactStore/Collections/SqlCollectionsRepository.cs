using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.ArtifactList.Models;
using ArtifactStore.Collections.Models;
using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Collections
{
    public class SqlCollectionsRepository : ICollectionsRepository
    {
        internal const string GetArtifactIdsInCollectionQuery =
            "SELECT * FROM [dbo].[GetArtifactIdsInCollection](@userId, @collectionId, @addDrafts)";

        private readonly ISqlConnectionWrapper _connectionWrapper;
        private readonly IArtifactRepository _artifactRepository;

        public SqlCollectionsRepository() : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        internal SqlCollectionsRepository(ISqlConnectionWrapper connectionWrapper)
            : this(
                connectionWrapper,
                new SqlArtifactRepository(connectionWrapper))
        {
        }

        public SqlCollectionsRepository(ISqlConnectionWrapper connectionWrapper, IArtifactRepository artifactRepository)
        {
            _connectionWrapper = connectionWrapper;
            _artifactRepository = artifactRepository;
        }

        public async Task<IReadOnlyList<ArtifactPropertyInfo>> GetArtifactsWithPropertyValuesAsync(int userId, IEnumerable<int> artifactIds, ProfileColumns profileColumns)
        {
            return await _artifactRepository.GetArtifactsWithPropertyValuesAsync(userId, artifactIds, profileColumns);
        }

        public async Task<IReadOnlyList<int>> GetContentArtifactIdsAsync(
            int collectionId, int userId, bool addDrafts = true)
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

        public async Task<int> AddArtifactsToCollectionAsync(
            int collectionId, IEnumerable<int> artifactIds, int userId, IDbTransaction transaction = null)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@UserId", userId);
            parameters.Add("@CollectionId", collectionId);
            parameters.Add("@ArtifactIds", SqlConnectionWrapper.ToDataTable(artifactIds));

            int result;

            if (transaction == null)
            {
                result = await _connectionWrapper.ExecuteScalarAsync<int>(
                    "AddArtifactsToCollection", parameters, commandType: CommandType.StoredProcedure);
            }
            else
            {
                result = await transaction.Connection.ExecuteScalarAsync<int>(
                    "AddArtifactsToCollection", parameters, transaction, commandType: CommandType.StoredProcedure);
            }

            return result;
        }

        public async Task<int> RemoveArtifactsFromCollectionAsync(int collectionId, IEnumerable<int> artifactIds, int userId,
            IDbTransaction transaction = null)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@UserId", userId);
            parameters.Add("@CollectionId", collectionId);
            parameters.Add("@ArtifactsIds", SqlConnectionWrapper.ToDataTable(artifactIds));

            int result;

            if (transaction == null)
            {
                result = await _connectionWrapper.ExecuteScalarAsync<int>(
                    "RemoveArtifactsFromCollection", parameters, commandType: CommandType.StoredProcedure);
            }
            else
            {
                result = await transaction.Connection.ExecuteScalarAsync<int>(
                    "RemoveArtifactsFromCollection", parameters, transaction, commandType: CommandType.StoredProcedure);
            }

            return result;
        }

        public async Task RemoveDeletedArtifactsFromCollectionAsync(
            int collectionId, int userId, IDbTransaction transaction = null)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@UserId", userId);
            parameters.Add("@CollectionId", collectionId);

            if (transaction == null)
            {
                await _connectionWrapper.ExecuteAsync(
                    "RemoveDeletedArtifactsFromCollection", parameters, commandType: CommandType.StoredProcedure);
            }
            else
            {
                await transaction.Connection.ExecuteAsync(
                    "RemoveDeletedArtifactsFromCollection", parameters, transaction,
                    commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<IReadOnlyList<PropertyTypeInfo>> GetPropertyTypeInfosForItemTypesAsync(
            IEnumerable<int> itemTypeIds, string search = null)
        {
            if (itemTypeIds.IsEmpty())
            {
                return new List<PropertyTypeInfo>();
            }

            var parameters = new DynamicParameters();

            parameters.Add("@itemTypeIds", SqlConnectionWrapper.ToDataTable(itemTypeIds));
            parameters.Add("@search", search);

            var result = await _connectionWrapper.QueryAsync<PropertyTypeInfo>(
                "GetPropertyTypeInformationForItemTypes",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result.ToList();
        }
    }
}