using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.Helpers;
using ArtifactStore.Models.VersionControl;
using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;

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

            var param = new DynamicParameters();
            var itemIdsTable = SqlConnectionWrapper.ToDataTable(itemsToVerify);
            param.Add("@itemIds", itemIdsTable);

            if (transaction == null)
            {
                return (await ConnectionWrapper.QueryAsync<int>("GetLiveItems", param,
                commandType: CommandType.StoredProcedure)).ToHashSet();
            }

            return (await transaction.Connection.QueryAsync<int>("GetLiveItems", param, transaction,
                commandType: CommandType.StoredProcedure)).ToHashSet();
        }

        protected abstract string MarkAsLatestStoredProcedureName { get; }
        protected async Task MarkAsLatest(HashSet<int> markAsLatestVersionIds, int revisionId, IDbTransaction transaction = null)
        {
            if (markAsLatestVersionIds.Count == 0)
            {
                return;
            }

            var param = new DynamicParameters();
            param.Add("@revisionId", revisionId);
            param.Add("@versionIds", SqlConnectionWrapper.ToDataTable(markAsLatestVersionIds));

            if (transaction == null)
            {
                await ConnectionWrapper.ExecuteAsync(MarkAsLatestStoredProcedureName, param,
                commandType: CommandType.StoredProcedure);
                return;
            }

            await transaction.Connection.ExecuteAsync(MarkAsLatestStoredProcedureName, param, transaction,
                commandType: CommandType.StoredProcedure);
        }

        protected abstract string DeleteVersionsStoredProcedureName { get; }
        protected async Task DeleteVersions(HashSet<int> deleteVersionsIds, IDbTransaction transaction = null)
        {
            if (deleteVersionsIds.Count == 0)
            {
                return;
            }

            var param = new DynamicParameters();
            param.Add("@versionIds", SqlConnectionWrapper.ToDataTable(deleteVersionsIds));

            if (transaction == null)
            {
                await ConnectionWrapper.ExecuteAsync(DeleteVersionsStoredProcedureName, param,
                commandType: CommandType.StoredProcedure);
                return;
            }

            await transaction.Connection.ExecuteAsync(DeleteVersionsStoredProcedureName, param, transaction,
                commandType: CommandType.StoredProcedure);
        }

        protected abstract string CloseVersionsStoredProcedureName { get; }
        protected async Task CloseVersions(HashSet<int> closeVersionIds, int revisionId, IDbTransaction transaction = null)
        {
            if (closeVersionIds.Count == 0)
            {
                return;
            }

            var param = new DynamicParameters();
            param.Add("@revisionId", revisionId);
            param.Add("@versionIds", SqlConnectionWrapper.ToDataTable(closeVersionIds));

            if (transaction == null)
            {
                await ConnectionWrapper.ExecuteAsync(CloseVersionsStoredProcedureName, param,
                commandType: CommandType.StoredProcedure);
                return;
            }

            await transaction.Connection.ExecuteAsync(CloseVersionsStoredProcedureName, param, transaction,
                commandType: CommandType.StoredProcedure);

            //Log.Assert(updatedRowsCount == closeVersionIds.Count, "Publish: Some item versions are not closed");
        }

        public async Task<double?> GetMaxChildOrderIndex(int parentId, IDbTransaction transaction = null)
        {
            var param = new DynamicParameters();
            param.Add("@parentId", parentId);

            if (transaction == null)
            {
                return (await ConnectionWrapper.QueryAsync<double?>("GetMaxChildOrderIndex", param,
                commandType: CommandType.StoredProcedure)).FirstOrDefault();
            }

            return (await transaction.Connection.QueryAsync<double?>("GetMaxChildOrderIndex", param, transaction,
                commandType: CommandType.StoredProcedure)).FirstOrDefault();
        }

        public async Task SetParentAndOrderIndex(int itemVersionId, int newParentId, double newOrderIndex, IDbTransaction transaction = null)
        {
            var param = new DynamicParameters();
            param.Add("@versionId", itemVersionId);
            param.Add("@parentId", newParentId);
            param.Add("@orderIndex", newOrderIndex);

            if (transaction == null)
            {
                await ConnectionWrapper.ExecuteAsync("SetParentAndOrderIndex", param,
                commandType: CommandType.StoredProcedure);
                return;
            }

            await transaction.Connection.ExecuteAsync("SetParentAndOrderIndex", param, transaction,
                commandType: CommandType.StoredProcedure);
        }

        protected abstract string GetDraftAndLatestStoredProcedureName { get; }
        protected async Task<ICollection<T>> GetDraftAndLatest<T>(int userId, ISet<int> artifactIds, IDbTransaction transaction)
        {

            var param = new DynamicParameters();
            param.Add("@userId", userId);
            var artifactIdsTable = SqlConnectionWrapper.ToDataTable(artifactIds);
            param.Add("@artifactIds", artifactIdsTable);

            if (transaction == null)
            {
                return (await ConnectionWrapper.QueryAsync<T>(GetDraftAndLatestStoredProcedureName, param,
                commandType: CommandType.StoredProcedure)).ToList();
            }

            return (await transaction.Connection.QueryAsync<T>(GetDraftAndLatestStoredProcedureName, param, transaction,
                commandType: CommandType.StoredProcedure)).ToList();
        }

        protected async Task<IList<int>> GetDeletedArtifacts(int userId, ISet<int> artifactIds, IDbTransaction transaction = null)
        {
            var param = new DynamicParameters();
            param.Add("@userId", userId);
            var artifactIdsTable = SqlConnectionWrapper.ToDataTable(artifactIds);
            param.Add("@artifactIds", artifactIdsTable);

            if (transaction == null)
            {
                return (await ConnectionWrapper.QueryAsync<int>("GetArtifactsDeletedInDraft", param,
                commandType: CommandType.StoredProcedure)).ToList();
            }

            return (await transaction.Connection.QueryAsync<int>("GetArtifactsDeletedInDraft", param, transaction,
                commandType: CommandType.StoredProcedure)).ToList();
        }

        protected async Task MarkReuseLinksOutOfSync(IEnumerable<int> artifactIds, PublishEnvironment environment, IDbTransaction transaction = null)
        {
            var enumerable = artifactIds as int[] ?? artifactIds.ToArray();
            if (!enumerable.Any())
            {
                return;
            }
            var param = new DynamicParameters();
            var artifactIdsTable = SqlConnectionWrapper.ToDataTable(enumerable);
            param.Add("@artifactIds", artifactIdsTable);
            param.Add("@revisionId", environment.RevisionId);

            if (transaction == null)
            {
                await ConnectionWrapper.ExecuteAsync("MarkReuseLinksOutOfSync", param,
                commandType: CommandType.StoredProcedure);
                return;
            }

            await transaction.Connection.ExecuteAsync("MarkReuseLinksOutOfSync", param, 
                transaction,
                commandType: CommandType.StoredProcedure);
        }
    }
}