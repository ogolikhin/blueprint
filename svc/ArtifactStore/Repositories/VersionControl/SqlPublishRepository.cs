using ArtifactStore.Models.VersionControl;
using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ArtifactStore.Repositories.VersionControl
{
    public interface ISqlPublishRepository
    {
        Task<ISet<int>> GetLiveItemsOnly(ISet<int> itemsToVerify, IDbTransaction transaction = null);
        Task<double?> GetMaxChildOrderIndex(int parentId, IDbTransaction transaction = null);
        Task SetParentAndOrderIndex(int itemVersionId, int newParentId, double newOrderIndex, IDbTransaction transaction = null);
    }

    public abstract class SqlPublishRepository : SqlBaseArtifactRepository, ISqlPublishRepository
    {
        protected abstract class BaseVersionData
        {
            public int DraftVersionId { get; set; }
            public int? LatestVersionId { get; set; }

            public bool DraftDeleted { get; set; }
        }
        
        protected SqlPublishRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        protected SqlPublishRepository(ISqlConnectionWrapper connectionWrapper)
            : this(connectionWrapper, new SqlArtifactPermissionsRepository(connectionWrapper))
        {
        }

        protected SqlPublishRepository(ISqlConnectionWrapper connectionWrapper,
            IArtifactPermissionsRepository artifactPermissionsRepository)
            : base(connectionWrapper, artifactPermissionsRepository)
        {
        }

        public async Task<ISet<int>> GetLiveItemsOnly(ISet<int> itemsToVerify, IDbTransaction transaction = null)
        {
            if (itemsToVerify.Count == 0)
            {
                return itemsToVerify;
            }

            var parameters = new DynamicParameters();
            var itemIdsTable = SqlConnectionWrapper.ToDataTable(itemsToVerify);
            parameters.Add("@itemIds", itemIdsTable);

            if (transaction == null)
            {
                return 
                (
                    await ConnectionWrapper.QueryAsync<int>
                    (
                        "GetLiveItems", 
                        parameters,
                        commandType: CommandType.StoredProcedure)).ToHashSet();
            }

            return 
            (
                await transaction.Connection.QueryAsync<int>
                (
                    "GetLiveItems", 
                    parameters, 
                    transaction,
                    commandType: CommandType.StoredProcedure)).ToHashSet();
        }

        protected abstract string MarkAsLatestStoredProcedureName { get; }
        protected async Task MarkAsLatest(HashSet<int> markAsLatestVersionIds, int revisionId, IDbTransaction transaction = null)
        {
            if (markAsLatestVersionIds.Count == 0)
            {
                return;
            }

            var parameters = new DynamicParameters();
            parameters.Add("@revisionId", revisionId);
            parameters.Add("@versionIds", SqlConnectionWrapper.ToDataTable(markAsLatestVersionIds));

            if (transaction == null)
            {
                await ConnectionWrapper.ExecuteAsync
                (
                    MarkAsLatestStoredProcedureName, 
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return;
            }

            await transaction.Connection.ExecuteAsync
            (
                MarkAsLatestStoredProcedureName, 
                parameters, 
                transaction,
                commandType: CommandType.StoredProcedure);
        }

        protected abstract string DeleteVersionsStoredProcedureName { get; }

        protected async Task DeleteVersions(HashSet<int> deleteVersionsIds, IDbTransaction transaction = null)
        {
            if (deleteVersionsIds.Count == 0)
            {
                return;
            }

            var parameters = new DynamicParameters();
            parameters.Add("@versionIds", SqlConnectionWrapper.ToDataTable(deleteVersionsIds));

            if (transaction == null)
            {
                await ConnectionWrapper.ExecuteAsync
                (
                    DeleteVersionsStoredProcedureName, 
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return;
            }

            await transaction.Connection.ExecuteAsync
            (
                DeleteVersionsStoredProcedureName, 
                parameters, 
                transaction,
                commandType: CommandType.StoredProcedure);
        }

        protected abstract string CloseVersionsStoredProcedureName { get; }
        protected async Task CloseVersions(HashSet<int> closeVersionIds, int revisionId, IDbTransaction transaction = null)
        {
            if (closeVersionIds.Count == 0)
            {
                return;
            }

            var parameters = new DynamicParameters();
            parameters.Add("@revisionId", revisionId);
            parameters.Add("@versionIds", SqlConnectionWrapper.ToDataTable(closeVersionIds));

            if (transaction == null)
            {
                await ConnectionWrapper.ExecuteAsync
                (
                    CloseVersionsStoredProcedureName, 
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return;
            }

            await transaction.Connection.ExecuteAsync
            (
                CloseVersionsStoredProcedureName, 
                parameters, 
                transaction,
                commandType: CommandType.StoredProcedure);

            // Log.Assert(updatedRowsCount == closeVersionIds.Count, "Publish: Some item versions are not closed");
        }

        public async Task<double?> GetMaxChildOrderIndex(int parentId, IDbTransaction transaction = null)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@parentId", parentId);

            if (transaction == null)
            {
                return 
                (
                    await ConnectionWrapper.QueryAsync<double?>
                    (
                        "GetMaxChildOrderIndex", 
                        parameters,
                        commandType: CommandType.StoredProcedure)).FirstOrDefault();
            }

            return 
            (
                await transaction.Connection.QueryAsync<double?>
                (
                    "GetMaxChildOrderIndex", 
                    parameters, 
                    transaction,
                    commandType: CommandType.StoredProcedure)).FirstOrDefault();
        }

        public async Task SetParentAndOrderIndex(int itemVersionId, int newParentId, double newOrderIndex, IDbTransaction transaction = null)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@versionId", itemVersionId);
            parameters.Add("@parentId", newParentId);
            parameters.Add("@orderIndex", newOrderIndex);

            if (transaction == null)
            {
                await ConnectionWrapper.ExecuteAsync
                (
                    "SetParentAndOrderIndex", 
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return;
            }

            await transaction.Connection.ExecuteAsync
            (
                "SetParentAndOrderIndex", 
                parameters, 
                transaction,
                commandType: CommandType.StoredProcedure);
        }

        protected abstract string GetDraftAndLatestStoredProcedureName { get; }
        protected async Task<ICollection<T>> GetDraftAndLatest<T>(int userId, ISet<int> artifactIds, IDbTransaction transaction)
        {

            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            var artifactIdsTable = SqlConnectionWrapper.ToDataTable(artifactIds);
            parameters.Add("@artifactIds", artifactIdsTable);

            if (transaction == null)
            {
                return 
                (
                    await ConnectionWrapper.QueryAsync<T>
                    (
                        GetDraftAndLatestStoredProcedureName, 
                        parameters,
                        commandType: CommandType.StoredProcedure)).ToList();
            }

            return 
            (
                await transaction.Connection.QueryAsync<T>
                (
                    GetDraftAndLatestStoredProcedureName, 
                    parameters, 
                    transaction,
                    commandType: CommandType.StoredProcedure)).ToList();
        }

        protected async Task<IList<int>> GetDeletedArtifacts(int userId, ISet<int> artifactIds, IDbTransaction transaction = null)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            var artifactIdsTable = SqlConnectionWrapper.ToDataTable(artifactIds);
            parameters.Add("@artifactIds", artifactIdsTable);

            if (transaction == null)
            {
                return 
                (
                    await ConnectionWrapper.QueryAsync<int>
                    (
                        "GetArtifactsDeletedInDraft", 
                        parameters,
                        commandType: CommandType.StoredProcedure)).ToList();
            }

            return 
            (
                await transaction.Connection.QueryAsync<int>
                (
                    "GetArtifactsDeletedInDraft", 
                    parameters, 
                    transaction,
                    commandType: CommandType.StoredProcedure)).ToList();
        }

        protected async Task MarkReuseLinksOutOfSync(IEnumerable<int> artifactIds, PublishEnvironment environment, IDbTransaction transaction = null)
        {
            var enumerable = artifactIds as int[] ?? artifactIds.ToArray();
            if (!enumerable.Any())
            {
                return;
            }
            var parameters = new DynamicParameters();
            var artifactIdsTable = SqlConnectionWrapper.ToDataTable(enumerable);
            parameters.Add("@artifactIds", artifactIdsTable);
            parameters.Add("@revisionId", environment.RevisionId);

            if (transaction == null)
            {
                await ConnectionWrapper.ExecuteAsync
                (
                    "MarkReuseLinksOutOfSync", 
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return;
            }

            await transaction.Connection.ExecuteAsync
            (
                "MarkReuseLinksOutOfSync", 
                parameters, 
                transaction,
                commandType: CommandType.StoredProcedure);
        }
    }
}
