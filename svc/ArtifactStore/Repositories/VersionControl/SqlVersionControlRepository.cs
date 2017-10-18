using ArtifactStore.Helpers;
using ArtifactStore.Models.VersionControl;
using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.VersionControl;
using ServiceLibrary.Repositories;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ArtifactStore.Repositories.VersionControl
{
    public interface IVersionControlRepository
    {
        Task<ICollection<SqlDiscardPublishState>> GetDiscardPublishStates(int userId, IEnumerable<int> artifactIds, IDbTransaction transaction = null);

        Task<ICollection<SqlDiscardPublishState>> GetAllDiscardPublish(int userId, IDbTransaction transaction = null);

        Task<ICollection<int>> GetDiscardPublishDependentArtifacts(int userId, IEnumerable<int> artifactIds, bool forDiscard, IDbTransaction transaction = null);

        Task<SqlDiscardPublishDetailsResult> GetDiscardPublishDetails(int userId, IEnumerable<int> artifactIds, bool addProjectsNames, IDbTransaction transaction = null);

        Task<ICollection<SqlItemInfo>> GetPublishStates(int userId, ISet<int> ids, bool ignoreComments = false, IDbTransaction transaction = null);

        IDictionary<int, PublishErrors> CanPublish(ICollection<SqlItemInfo> artifactStates, IDbTransaction transaction = null);

        Task<ISet<int>> DetectAndPublishDeletedArtifacts(int userId, ISet<int> artifactIds, PublishEnvironment env, IDbTransaction transaction = null);

        Task ReleaseLock(int userId, ISet<int> affectedArtifactIds, IDbTransaction transaction = null);
    }

    public class SqlVersionControlRepository : SqlBaseArtifactRepository, IVersionControlRepository
    {
        public SqlVersionControlRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        public SqlVersionControlRepository(ISqlConnectionWrapper connectionWrapper)
            : this(connectionWrapper, new SqlArtifactPermissionsRepository(connectionWrapper))
        {
        }

        public SqlVersionControlRepository(ISqlConnectionWrapper connectionWrapper,
            IArtifactPermissionsRepository artifactPermissionsRepository)
            : base(connectionWrapper, artifactPermissionsRepository)
        {
        }

        public async Task<ICollection<SqlDiscardPublishState>> GetDiscardPublishStates(int userId, IEnumerable<int> artifactIds, IDbTransaction transaction = null)
        {
            return await GetDiscardPublishStatesInternal(userId, artifactIds, transaction);
        }

        public async Task<ICollection<SqlDiscardPublishState>> GetAllDiscardPublish(int userId, IDbTransaction transaction = null)
        {
            return await GetAllDiscardPublishInternal(userId, transaction);
        }

        public async Task<ICollection<int>> GetDiscardPublishDependentArtifacts(int userId, IEnumerable<int> artifactIds, bool forDiscard, IDbTransaction transaction = null)
        {
            return await GetDiscardPublishDependentArtifactsInternal(userId, artifactIds, forDiscard, transaction);
        }

        public async Task<SqlDiscardPublishDetailsResult> GetDiscardPublishDetails(int userId, IEnumerable<int> artifactIds, bool addProjectsNames, IDbTransaction transaction = null)
        {
            return await GetDiscardPublishDetailsInternal(userId, artifactIds, addProjectsNames, transaction);
        }

        public async Task<ICollection<SqlItemInfo>> GetPublishStates(int userId, ISet<int> ids, bool ignoreComments = false, IDbTransaction transaction = null)
        {
            return await GetPublishStatesInternal(userId, ids, ignoreComments, transaction);
        }

        public IDictionary<int, PublishErrors> CanPublish(ICollection<SqlItemInfo> artifactStates, IDbTransaction transaction = null)
        {
            return CanPublishInternal(artifactStates);
        }

        public async Task<ISet<int>> DetectAndPublishDeletedArtifacts(int userId, ISet<int> artifactIds, PublishEnvironment env, IDbTransaction transaction = null)
        {
            var deletedArtifactIds = (await GetDeletedArtifacts(userId, artifactIds, transaction)).ToHashSet();

            if (deletedArtifactIds.Any())
            {
                RemoveCollectionAssignments(userId, deletedArtifactIds, env, transaction);

                DeleteAndPublishArtifactsInSql(userId, deletedArtifactIds, env.RevisionId, transaction);
            }

            return deletedArtifactIds;
        }

        public async Task ReleaseLock(int userId, ISet<int> affectedArtifactIds, IDbTransaction transaction = null)
        {
            await ReleaseLockInternal(userId, affectedArtifactIds, transaction);
        }

        private void RemoveCollectionAssignments(int userId, IEnumerable<int> deletedArtifactIds, PublishEnvironment env, IDbTransaction transaction)
        {
            // Create stored procedure for removing collection assignments
            // bool hasChanges = false;
            // foreach (var artifactId in env.FilterByBaseType(deletedArtifactIds, ItemTypePredefined.ArtifactCollection))
            // {
            //    ACollectionAssignment.RemoveAllAsPublishMarkedForDeletion(env.RevisionId, artifactId, ctx);
            //    hasChanges = true;
            // }

            // if (hasChanges)
            // {
            //    ctx.SaveChanges();
            // }
        }

        private async Task<ICollection<SqlDiscardPublishState>> GetDiscardPublishStatesInternal(int userId,
            IEnumerable<int> artifactIds,
            IDbTransaction transaction)
        {
            var aids = artifactIds as int[] ?? artifactIds.ToArray();
            var discardPublishStates = aids.ToDictionary(artifactId => artifactId, artifactId => new SqlDiscardPublishState
            {
                ItemId = artifactId,
                NotExist = true
            });

            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            var artifactIdsTable = SqlConnectionWrapper.ToDataTable(aids);
            parameters.Add("@artifactIds", artifactIdsTable);

            IEnumerable<SqlDiscardPublishState> sqlDiscardPublishStates;

            if (transaction == null)
            {
                sqlDiscardPublishStates = await ConnectionWrapper.QueryAsync<SqlDiscardPublishState>
                (
                    "GetDiscardPublishStates",
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            else
            {
                sqlDiscardPublishStates = await transaction.Connection.QueryAsync<SqlDiscardPublishState>
                (
                    "GetDiscardPublishStates",
                    parameters,
                    transaction,
                    commandType: CommandType.StoredProcedure);
            }

            foreach (var dpState in sqlDiscardPublishStates)
            {
                SqlDiscardPublishState discardPublishState;
                if (!discardPublishStates.TryGetValue(dpState.ItemId, out discardPublishState))
                {
                    continue;
                }
                discardPublishState.NotExist = false;
                discardPublishState.NotArtifact = dpState.NotArtifact;
                if (!discardPublishState.NotArtifact)
                {
                    discardPublishState.Deleted = dpState.Deleted;
                    if (!discardPublishState.Deleted)
                    {
                        discardPublishState.HasPublishedVersion = dpState.HasPublishedVersion;
                        discardPublishState.NoChanges = ((dpState.LockedByUserId == null) || (dpState.LockedByUserId.Value != userId)) &&
                            !dpState.HasDraftRelationships.GetValueOrDefault(false);
                        if (!discardPublishState.NoChanges)
                        {
                            discardPublishState.Invalid = (dpState.LockedByUserId != null) && (dpState.LockedByUserId.Value == userId) &&
                                dpState.LastSaveInvalid.GetValueOrDefault(false);
                            discardPublishState.DiscardDependent = dpState.DiscardDependent;
                            discardPublishState.PublishDependent = dpState.PublishDependent;
                        }
                    }
                }
            }
            return discardPublishStates.Values;
        }

        private async Task<ICollection<SqlDiscardPublishState>> GetAllDiscardPublishInternal(int userId,
            IDbTransaction transaction)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);

            IList<SqlDiscardPublishState> sqlDiscardPublishStates;

            if (transaction == null)
            {
                sqlDiscardPublishStates =
                (
                    await ConnectionWrapper.QueryAsync<SqlDiscardPublishState>
                    (
                        "GetAllDiscardPublishArtifactIds",
                        parameters,
                        commandType: CommandType.StoredProcedure)).ToList();
            }
            else
            {
                sqlDiscardPublishStates =
                (
                    await transaction.Connection.QueryAsync<SqlDiscardPublishState>
                    (
                        "GetAllDiscardPublishArtifactIds",
                        parameters,
                        transaction,
                        commandType: CommandType.StoredProcedure)).ToList();
            }

            sqlDiscardPublishStates.ForEach
            (
                dps =>
                {
                    dps.NotExist = false;
                    dps.NotArtifact = false;
                    dps.Deleted = false;
                    dps.NoChanges = false;
                    dps.Invalid = dps.LockedByUserId.GetValueOrDefault(-1) == userId &&
                                    dps.LastSaveInvalid.GetValueOrDefault(false);
                    dps.DiscardDependent = false;
                    dps.PublishDependent = false;
                });

            return sqlDiscardPublishStates;
        }

        private async Task<ICollection<int>> GetDiscardPublishDependentArtifactsInternal(int userId,
            IEnumerable<int> artifactIds,
            bool forDiscard,
            IDbTransaction transaction)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            var artifactIdsTable = SqlConnectionWrapper.ToDataTable(artifactIds);
            parameters.Add("@artifactIds", artifactIdsTable);
            parameters.Add("@forDiscard", forDiscard);

            IList<SqlDiscardPublishState> sqlDiscardPublishStates;

            if (transaction == null)
            {
                sqlDiscardPublishStates =
                (
                    await ConnectionWrapper.QueryAsync<SqlDiscardPublishState>
                    (
                        "GetDiscardPublishAncestors",
                        parameters,
                        commandType: CommandType.StoredProcedure)).ToList();
            }
            else
            {
                sqlDiscardPublishStates =
                (
                    await transaction.Connection.QueryAsync<SqlDiscardPublishState>
                    (
                        "GetDiscardPublishAncestors",
                        parameters,
                        transaction,
                        commandType: CommandType.StoredProcedure)).ToList();
            }

            var discardPublishDependentArtifacts = new List<int>(0x100);
            discardPublishDependentArtifacts.AddRange(from sqlDiscardPublishState in sqlDiscardPublishStates where forDiscard ? sqlDiscardPublishState.DiscardDependent : sqlDiscardPublishState.PublishDependent select sqlDiscardPublishState.ItemId);
            return discardPublishDependentArtifacts;
        }

        private async Task<SqlDiscardPublishDetailsResult> GetDiscardPublishDetailsInternal(int userId,
            IEnumerable<int> artifactIds,
            bool addProjectsNames,
            IDbTransaction transaction)
        {
            var result = new SqlDiscardPublishDetailsResult();

            var projectIdSet = new HashSet<int>();

            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            var artifactIdsTable = SqlConnectionWrapper.ToDataTable(artifactIds);
            parameters.Add("@artifactIds", artifactIdsTable);

            result.Details.Clear();

            if (transaction == null)
            {
                result.Details.AddRange
                (
                    (
                        await ConnectionWrapper.QueryAsync<SqlDiscardPublishDetails>
                        (
                            "GetDiscardPublishDetails",
                            parameters,
                            commandType: CommandType.StoredProcedure)).ToList());
            }
            else
            {
                result.Details.AddRange
                (
                    (
                        await transaction.Connection.QueryAsync<SqlDiscardPublishDetails>
                        (
                            "GetDiscardPublishDetails",
                            parameters,
                            transaction,
                            commandType: CommandType.StoredProcedure)).ToList());
            }

            foreach (var sqlDiscardPublishDetail in result.Details)
            {
                projectIdSet.Add(sqlDiscardPublishDetail.VersionProjectId);
            }

            if (addProjectsNames)
            {
                result.ProjectInfos.Clear();
                result.ProjectInfos.AddRange(await GetProjectNames(userId, projectIdSet, transaction));
            }
            return result;
        }

        private async Task<IDictionary<int, string>> GetProjectNames(int userId,
            HashSet<int> projectIdSet,
            IDbTransaction transaction)
        {
            var result = new Dictionary<int, string>();
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            var projectIdsTable = SqlConnectionWrapper.ToDataTable(projectIdSet);
            parameters.Add("@projectIds", projectIdsTable);

            IList<SqlDiscardPublishProjectInfo> discardPublishProjectDetails;

            if (transaction == null)
            {
                discardPublishProjectDetails =
                (
                    await ConnectionWrapper.QueryAsync<SqlDiscardPublishProjectInfo>
                    (
                        "GetDiscardPublishProjectsDetails",
                        parameters,
                        commandType: CommandType.StoredProcedure)).ToList();
            }
            else
            {
                discardPublishProjectDetails =
                (
                    await transaction.Connection.QueryAsync<SqlDiscardPublishProjectInfo>
                    (
                        "GetDiscardPublishProjectsDetails",
                        parameters,
                        transaction,
                        commandType: CommandType.StoredProcedure)).ToList();
            }

            foreach (var sqlDiscardPublishProjectInfo in discardPublishProjectDetails)
            {
                if (!result.ContainsKey(sqlDiscardPublishProjectInfo.ItemId))
                {
                    result.Add(sqlDiscardPublishProjectInfo.ItemId, sqlDiscardPublishProjectInfo.Name);
                }
                else
                {
                    result[sqlDiscardPublishProjectInfo.ItemId] = sqlDiscardPublishProjectInfo.Name;
                }
            }
            return result;
        }

        private async Task<ICollection<SqlItemInfo>> GetPublishStatesInternal(int userId,
            ISet<int> ids,
            bool ignoreComments,
            IDbTransaction transaction)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            var artifactIds = SqlConnectionWrapper.ToDataTable(ids);
            parameters.Add("@artifactIds", artifactIds);
            parameters.Add("@ignoreComments", ignoreComments);
            IList<SqlItemInfo> existingItemsInfo;

            if (transaction == null)
            {
                existingItemsInfo =
                (
                    await ConnectionWrapper.QueryAsync<SqlItemInfo>
                    (
                        "GetPublishInfo",
                        parameters,
                        commandType: CommandType.StoredProcedure)).ToList();
            }
            else
            {
                existingItemsInfo =
                (
                    await transaction.Connection.QueryAsync<SqlItemInfo>
                    (
                        "GetPublishInfo",
                        parameters,
                        transaction,
                        commandType: CommandType.StoredProcedure)).ToList();
            }

            existingItemsInfo.AddRange
            (
                ids.Except
                (
                    existingItemsInfo.Select(i => i.ItemId))
                .Select
                (
                    id => new SqlItemInfo
                    {
                        ItemId = id,
                        NotFound = true
                    }));

            return existingItemsInfo;
        }

        private IDictionary<int, PublishErrors> CanPublishInternal(ICollection<SqlItemInfo> artifactStates)
        {
            var result = new Dictionary<int, PublishErrors>();

            foreach (var state in artifactStates)
            {
                if (state.NotFound)
                {
                    result.Add(state.ItemId, PublishErrors.NotFound);
                }
                else if (state.NotArtifact)
                {
                    result.Add(state.ItemId, PublishErrors.NotAnArtifact);
                }
                else if (!state.HasDraft)
                {
                    result.Add(state.ItemId, PublishErrors.NothingToPublish);
                }
            }

            return result;
        }

        private async Task<IList<int>> GetDeletedArtifacts(int userId, ISet<int> artifactIds, IDbTransaction transaction)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            var aids = SqlConnectionWrapper.ToDataTable(artifactIds);
            parameters.Add("@artifactIds", aids);

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

        private async void DeleteAndPublishArtifactsInSql(int userId, IEnumerable<int> artifactIds, int revisionId, IDbTransaction transaction)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            parameters.Add("@revisionId", revisionId);
            var aids = SqlConnectionWrapper.ToDataTable(artifactIds);
            parameters.Add("@artifactIds", aids);

            if (transaction == null)
            {
                await ConnectionWrapper.ExecuteAsync
                (
                    "DeleteAndPublishArtifacts",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return;
            }

            await transaction.Connection.ExecuteAsync
            (
                "DeleteAndPublishArtifacts",
                parameters,
                transaction,
                commandType: CommandType.StoredProcedure);
        }

        private async Task ReleaseLockInternal(int userId, ISet<int> artifactIds, IDbTransaction transaction)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            var aids = SqlConnectionWrapper.ToDataTable(artifactIds);
            parameters.Add("@artifactIds", aids);

            if (transaction == null)
            {
                await ConnectionWrapper.ExecuteAsync
                (
                    "ReleaseLock",
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            else
            {
                await transaction.Connection.ExecuteAsync
                (
                    "ReleaseLock",
                    parameters,
                    transaction,
                    commandType: CommandType.StoredProcedure);
            }
        }
    }
}
