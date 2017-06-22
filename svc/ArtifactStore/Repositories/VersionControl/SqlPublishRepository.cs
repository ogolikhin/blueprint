using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.Helpers;
using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Repositories.VersionControl
{
    public class SqlPublishRepository : SqlBaseArtifactRepository
    {
        protected abstract class BaseVersionData
        {
            public int DraftVersionId { get; set; }
            public int? LatestVersionId { get; set; }

            public bool DraftDeleted { get; set; }
        }
        public SqlPublishRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        public SqlPublishRepository(ISqlConnectionWrapper connectionWrapper)
            : this(connectionWrapper, new SqlArtifactPermissionsRepository(connectionWrapper))
        {
        }

        public SqlPublishRepository(ISqlConnectionWrapper connectionWrapper,
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

            return (await transaction.Connection.QueryAsync<int>("GetLiveItems", param,
                commandType: CommandType.StoredProcedure)).ToHashSet();
        }

        public async Task MarkAsLatest(HashSet<int> markAsLatestVersionIds, int revisionId, IDbTransaction transaction = null)
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
                await ConnectionWrapper.ExecuteAsync("MarkAsLatestItemVersions", param,
                commandType: CommandType.StoredProcedure);
                return;
            }

            await transaction.Connection.ExecuteAsync("MarkAsLatestItemVersions", param,
                commandType: CommandType.StoredProcedure);
        }

        public async Task DeleteVersions(HashSet<int> deleteVersionsIds, IDbTransaction transaction = null)
        {
            if (deleteVersionsIds.Count == 0)
            {
                return;
            }

            var param = new DynamicParameters();
            param.Add("@versionIds", SqlConnectionWrapper.ToDataTable(deleteVersionsIds));

            if (transaction == null)
            {
                await ConnectionWrapper.ExecuteAsync("RemoveItemVersions", param,
                commandType: CommandType.StoredProcedure);
                return;
            }

            await transaction.Connection.ExecuteAsync("RemoveItemVersions", param,
                commandType: CommandType.StoredProcedure);
        }

        public async Task CloseVersions(HashSet<int> closeVersionIds, int revisionId, IDbTransaction transaction = null)
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
                await ConnectionWrapper.ExecuteAsync("CloseItemVersions", param,
                commandType: CommandType.StoredProcedure);
                return;
            }

            await transaction.Connection.ExecuteAsync("CloseItemVersions", param,
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

            return (await transaction.Connection.QueryAsync<double?>("GetMaxChildOrderIndex", param,
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

            await transaction.Connection.ExecuteAsync("SetParentAndOrderIndex", param,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<ICollection<SqlDraftAndLatestItem>> GetDraftAndLatestItems(int userId, ISet<int> artifactIds, IDbTransaction transaction)
        {

            var param = new DynamicParameters();
            param.Add("@userId", userId);
            var artifactIdsTable = SqlConnectionWrapper.ToDataTable(artifactIds);
            param.Add("@artifactIds", artifactIdsTable);

            if (transaction == null)
            {
                return (await ConnectionWrapper.QueryAsync<SqlDraftAndLatestItem>("GetDraftAndLatestItemVersions", param,
                commandType: CommandType.StoredProcedure)).ToList();
            }

            return (await transaction.Connection.QueryAsync<SqlDraftAndLatestItem>("GetDraftAndLatestItemVersions", param,
                commandType: CommandType.StoredProcedure)).ToList();
        }

        public async Task<IList<int>> GetDeletedArtifacts(int userId, ISet<int> artifactIds, IDbTransaction transaction = null)
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

            return (await transaction.Connection.QueryAsync<int>("GetArtifactsDeletedInDraft", param,
                commandType: CommandType.StoredProcedure)).ToList();
        }
    }
}