using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.ArtifactList.Models;
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
            int userId, IEnumerable<int> artifactIds, ProfileColumns profileColumns)
        {
            var propertyTypePredefineds = (
                from c in profileColumns.Items
                where c.Predefined != PropertyTypePredefined.CustomGroup
                select (int)c.Predefined).ToList();

            var propertyTypeIds = (
                from c in profileColumns.Items
                where c.PropertyTypeId != null
                select c.PropertyTypeId.Value).ToList();

            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId, DbType.Int32);
            parameters.Add("@addDrafts", true, DbType.Boolean);
            parameters.Add("@artifactIds", SqlConnectionWrapper.ToDataTable(artifactIds));
            parameters.Add("@propertyTypePredefineds", SqlConnectionWrapper.ToDataTable(propertyTypePredefineds));
            parameters.Add("@propertyTypeIds", SqlConnectionWrapper.ToDataTable(propertyTypeIds));

            var result = await _connectionWrapper.QueryAsync<CollectionArtifact>(
                "GetPropertyValuesForArtifactsNew", parameters, commandType: CommandType.StoredProcedure);

            return result.ToList();
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