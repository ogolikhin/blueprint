using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.Helpers;
using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.VersionControl;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Repositories.VersionControl
{
    public interface IVersionControlRepository
    {
        Task<ICollection<SqlDiscardPublishState>> GetDiscardPublishStates(int userId, IEnumerable<int> artifactIds);

        Task<ICollection<SqlDiscardPublishState>> GetAllDiscardPublish(int userId);

        Task<ICollection<int>> GetDiscardPublishDependentArtifacts(int userId, IEnumerable<int> artifactIds, bool forDiscard);

        Task<SqlDiscardPublishDetailsResult> GetDiscardPublishDetails(int userId, IEnumerable<int> artifactIds, bool addProjectsNames);

        Task<ICollection<SqlItemInfo>> GetPublishStates(int userId, ISet<int> ids, bool ignoreComments = false);

        IDictionary<int, PublishErrors> CanPublish(ICollection<SqlItemInfo> artifactStates);

        Task<ISet<int>> DetectAndPublishDeletedArtifacts(int userId, ISet<int> artifactIds, PublishEnvironment env);

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

        public async Task<ICollection<SqlDiscardPublishState>> GetDiscardPublishStates(int userId, IEnumerable<int> artifactIds)
        {
            return await GetDiscardPublishStatesInternal(userId, artifactIds);
        }

        public async Task<ICollection<SqlDiscardPublishState>> GetAllDiscardPublish(int userId)
        {
            return await GetAllDiscardPublishInternal(userId);
        }

        public async Task<ICollection<int>> GetDiscardPublishDependentArtifacts(int userId, IEnumerable<int> artifactIds, bool forDiscard)
        {
            return await GetDiscardPublishDependentArtifactsInternal(userId, artifactIds, forDiscard);
        }

        public async Task<SqlDiscardPublishDetailsResult> GetDiscardPublishDetails(int userId, IEnumerable<int> artifactIds, bool addProjectsNames)
        {
            return await GetDiscardPublishDetailsInternal(userId, artifactIds, addProjectsNames);
        }

        public async Task<ICollection<SqlItemInfo>> GetPublishStates(int userId, ISet<int> ids, bool ignoreComments = false)
        {
            return await GetPublishStatesInternal(userId, ids, ignoreComments);
        }

        public IDictionary<int, PublishErrors> CanPublish(ICollection<SqlItemInfo> artifactStates)
        {
            return CanPublishInternal(artifactStates);
        }

        public async Task<ISet<int>> DetectAndPublishDeletedArtifacts(int userId, ISet<int> artifactIds, PublishEnvironment env)
        {
            var deletedArtifactIds = (await GetDeletedArtifacts(userId, artifactIds)).ToHashSet();

            if (deletedArtifactIds.Any())
            {
                RemoveCollectionAssignments(userId, deletedArtifactIds, env);

                DeleteAndPublishArtifactsInSql(userId, deletedArtifactIds, env.RevisionId);
            }

            return deletedArtifactIds;
        }

        private void RemoveCollectionAssignments(int userId, IEnumerable<int> deletedArtifactIds, PublishEnvironment env)
        {
            //Create stored procedure for removing collection assignments
                //bool hasChanges = false;
                //foreach (var artifactId in env.FilterByBaseType(deletedArtifactIds, ItemTypePredefined.ArtifactCollection))
                //{
                //    ACollectionAssignment.RemoveAllAsPublishMarkedForDeletion(env.RevisionId, artifactId, ctx);
                //    hasChanges = true;
                //}

                //if (hasChanges)
                //{
                //    ctx.SaveChanges();
                //}
        }

        

        private async Task<ICollection<SqlDiscardPublishState>> GetDiscardPublishStatesInternal(int userId,
            IEnumerable<int> artifactIds)
        {
            var aids = artifactIds as int[] ?? artifactIds.ToArray();
            var discardPublishStates = aids.ToDictionary(artifactId => artifactId, artifactId => new SqlDiscardPublishState
            {
                ItemId = artifactId,
                NotExist = true
            });

            var param = new DynamicParameters();
            param.Add("@userId", userId);
            var artifactIdsTable = SqlConnectionWrapper.ToDataTable(aids);
            param.Add("@artifactIds", artifactIdsTable);

            foreach (var dpState in await
                    ConnectionWrapper.QueryAsync<SqlDiscardPublishState>("GetDiscardPublishStates", param,
                        commandType: CommandType.StoredProcedure))
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

        private async Task<ICollection<SqlDiscardPublishState>> GetAllDiscardPublishInternal(int userId)
        {
            var param = new DynamicParameters();
            param.Add("@userId", userId);

            var sqlDiscardPublishStates = (await
                ConnectionWrapper.QueryAsync<SqlDiscardPublishState>("GetAllDiscardPublishArtifactIds", param,
                    commandType: CommandType.StoredProcedure)).ToList();
            sqlDiscardPublishStates.ForEach(
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

        private async Task<ICollection<int>> GetDiscardPublishDependentArtifactsInternal(int userId, IEnumerable<int> artifactIds,
            bool forDiscard)
        {
            var param = new DynamicParameters();
            param.Add("@userId", userId);
            var artifactIdsTable = SqlConnectionWrapper.ToDataTable(artifactIds);
            param.Add("@artifactIds", artifactIdsTable);
            param.Add("@forDiscard", forDiscard);

            var sqlDiscardPublishStates = (await
                ConnectionWrapper.QueryAsync<SqlDiscardPublishState>("GetDiscardPublishAncestors", param,
                    commandType: CommandType.StoredProcedure)).ToList();
            var discardPublishDependentArtifacts = new List<int>(0x100);
            discardPublishDependentArtifacts.AddRange(from sqlDiscardPublishState in sqlDiscardPublishStates where forDiscard ? sqlDiscardPublishState.DiscardDependent : sqlDiscardPublishState.PublishDependent select sqlDiscardPublishState.ItemId);
            return discardPublishDependentArtifacts;
        }

        private async Task<SqlDiscardPublishDetailsResult> GetDiscardPublishDetailsInternal(int userId, IEnumerable<int> artifactIds,
            bool addProjectsNames)
        {
            var result = new SqlDiscardPublishDetailsResult();

            var projectIdSet = new HashSet<int>();

            var param = new DynamicParameters();
            param.Add("@userId", userId);
            var artifactIdsTable = SqlConnectionWrapper.ToDataTable(artifactIds);
            param.Add("@artifactIds", artifactIdsTable);

            result.Details = (await
                ConnectionWrapper.QueryAsync<SqlDiscardPublishDetails>("GetDiscardPublishDetails", param,
                    commandType: CommandType.StoredProcedure)).ToList();
            foreach (var sqlDiscardPublishDetail in result.Details)
            {
                projectIdSet.Add(sqlDiscardPublishDetail.ProjectId);
            }

            if (addProjectsNames)
            {
                result.ProjectInfos = await GetProjectNames(userId, projectIdSet);
            }
            return result;
        }

        private async Task<IDictionary<int, string>> GetProjectNames(int userId, HashSet<int> projectIdSet)
        {
            var result = new Dictionary<int, string>();
            var param = new DynamicParameters();
            param.Add("@userId", userId);
            var projectIdsTable = SqlConnectionWrapper.ToDataTable(projectIdSet);
            param.Add("@projectIds", projectIdsTable);

            var discardPublishProjectDetails = (await
                ConnectionWrapper.QueryAsync<SqlDiscardPublishProjectInfo>("GetDiscardPublishProjectsDetails", param,
                    commandType: CommandType.StoredProcedure)).ToList();

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

        private async Task<ICollection<SqlItemInfo>> GetPublishStatesInternal(int userId, ISet<int> ids, bool ignoreComments = false)
        {
            var param = new DynamicParameters();
            param.Add("@userId", userId);
            var artifactIds = SqlConnectionWrapper.ToDataTable(ids);
            param.Add("@artifactIds", artifactIds);
            param.Add("@ignoreComments", ignoreComments);

            var existingItemsInfo = (await
                ConnectionWrapper.QueryAsync<SqlItemInfo>("GetPublishInfo", param,
                    commandType: CommandType.StoredProcedure)).ToList();

            existingItemsInfo.AddRange(
                ids.Except(existingItemsInfo.Select(i => i.ItemId))
                    .Select(id => new SqlItemInfo
                    {
                        ItemId = id,
                        NotFound = true
                    })
            );

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

        private async Task<IList<int>> GetDeletedArtifacts(int userId, ISet<int> artifactIds)
        {
            var param = new DynamicParameters();
            param.Add("@userId", userId);
            var aids = SqlConnectionWrapper.ToDataTable(artifactIds);
            param.Add("@artifactIds", aids);

            return (await
                ConnectionWrapper.QueryAsync<int>("GetArtifactsDeletedInDraft", param,
                    commandType: CommandType.StoredProcedure)).ToList();
        }

        private async void DeleteAndPublishArtifactsInSql(int userId, IEnumerable<int> artifactIds, int revisionId)
        {

            var param = new DynamicParameters();
            param.Add("@userId", userId);
            param.Add("@revisionId", revisionId);
            var aids = SqlConnectionWrapper.ToDataTable(artifactIds);
            param.Add("@artifactIds", aids);

            await
                ConnectionWrapper.ExecuteAsync("DeleteAndPublishArtifacts", param,
                    commandType: CommandType.StoredProcedure);
        }
    }
}
