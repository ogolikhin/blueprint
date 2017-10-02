using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SearchService.Models;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;

namespace SearchService.Repositories
{
    public interface ISemanticSearchRepository
    {
        Task<List<int>> GetSuggestedArtifacts(int artifactId, bool isInstanceAdmin, IEnumerable<int> projectIds);
        Task<List<int>> GetAccessibleProjectIds(int userId);
        // TEMPORARY ADDED artifactId
        Task<List<ArtifactSearchResult>> GetSuggestedArtifactDetails(List<int> artifactIds, int userId, int artifactId);
    }
    public class SemanticSearchRepository: ISemanticSearchRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;
        public SemanticSearchRepository()
             : this(new SqlConnectionWrapper(WebApiConfig.BlueprintConnectionString))
        {
        }

        internal SemanticSearchRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public async Task<List<int>> GetSuggestedArtifacts(int artifactId, bool isInstanceAdmin, IEnumerable<int> projectIds)
        {
            //var query = $"SELECT DISTINCT TOP 10 iv.VersionItemId FROM dbo.ItemVersions as iv WHERE iv.EndRevision = 2147483647 GROUP BY (iv.VersionItemId) ORDER BY iv.VersionItemId DESC";
            //return (await _connectionWrapper.QueryAsync<int>(query, commandType: CommandType.Text)).ToList();
            return await Task.FromResult(new List<int>()
            {
                // returns 10 ids from elastic search or sql
            });
        }

        // TEMPORARRY CODE ----------------------------------------------

        private async Task<Tuple<IEnumerable<ProjectsArtifactsItem>, IEnumerable<VersionProjectInfo>>> GetArtifactsProjects(IEnumerable<int> itemIds, int sessionUserId, int revisionId, bool addDrafts)
        {
            var prm = new DynamicParameters();
            prm.Add("@userId", sessionUserId);
            prm.Add("@itemIds", SqlConnectionWrapper.ToDataTable(itemIds, "Int32Collection", "Int32Value"));

            return (await _connectionWrapper.QueryMultipleAsync<ProjectsArtifactsItem, VersionProjectInfo>("GetArtifactsProjects", prm, commandType: CommandType.StoredProcedure));
        }

        private async Task<ICollection<int>> GetProjectArtifactIds(int userId, int projectId)
        {
            var prm = new DynamicParameters();
            prm.Add("@userId", userId);
            prm.Add("@projectId", projectId);
            prm.Add("@revisionId", int.MaxValue);
            prm.Add("@addDrafts", false);

            return (await _connectionWrapper.QueryAsync<int>("GetProjectArtifactIds", prm, commandType: CommandType.StoredProcedure)).ToList<int>();
        }


        public async Task<IEnumerable<ItemDetails>> GetItemsDetails(int userId, IEnumerable<int> itemIds, bool addDrafts = true, int revisionId = int.MaxValue)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            parameters.Add("@itemIds", SqlConnectionWrapper.ToDataTable(itemIds, "Int32Collection", "Int32Value"));
            parameters.Add("@addDrafts", addDrafts);
            parameters.Add("@revisionId", revisionId);

            return await _connectionWrapper.QueryAsync<ItemDetails>("GetItemsDetails", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<List<ArtifactSearchResult>> GetSuggestedArtifactDetails(List<int> artifactIds, int userId, int artifactId)
        {
            //TODO: uncomment once real artifact ids are passed in
            //var prm = new DynamicParameters();
            //prm.Add("@userId", userId);
            //prm.Add("@artifactIds", SqlConnectionWrapper.ToDataTable(artifactIds));

            //return (await _connectionWrapper.QueryAsync<ArtifactSearchResult>("GetSuggestedArtifactDetails", prm, commandType: CommandType.StoredProcedure)).ToList();
            
            // TEMPORARRY CODE ----------------------------------------------
            var ids = new List<int>();
            ids.Add(artifactId);
            var multipleResult = await GetArtifactsProjects(ids, userId, 0, false);
            var projectId = multipleResult.Item1.ToList()[0].VersionProjectId;
            var resultArtifactIds = (await GetProjectArtifactIds(userId, projectId)).ToList();
            var itemDetails = (await GetItemsDetails(userId, resultArtifactIds, true, int.MaxValue)).ToList();

            var retValue = new List<ArtifactSearchResult>();

            for (int i = 0; i < 10 && i < itemDetails.Count; i++)
            {
                retValue.Add(new ArtifactSearchResult()
                {
                    ItemId = itemDetails[i].HolderId,
                    ItemTypeId = itemDetails[i].ItemTypeId,
                    Name = itemDetails[i].Name,
                    ProjectId = projectId,
                    PredefinedType = (ItemTypePredefined)itemDetails[i].PrimitiveItemTypePredefined,
                    ProjectName = "Fake Project",
                    TypePrefix = itemDetails[i].Prefix,
                    ItemTypeIconId = itemDetails[i].ItemTypeId
                });
            }

            return await Task.FromResult(new List<ArtifactSearchResult>(retValue));

            //return await Task.FromResult(new List<ArtifactSearchResult>()
            //{
            //    new ArtifactSearchResult()
            //    {
            //        ItemId = 1,
            //        ItemTypeId = 1,
            //        Name = "Fake Artifact 1",
            //        ProjectId = 1,
            //        PredefinedType = ItemTypePredefined.Actor,
            //        ProjectName = "Fake Project",
            //        TypePrefix = "FAKE",
            //        ItemTypeIconId = 1
            //    },
            //     new ArtifactSearchResult()
            //    {
            //        ItemId = 2,
            //        ItemTypeId = 1,
            //        Name = "Fake Artifact 2",
            //        ProjectId = 2,
            //        PredefinedType = ItemTypePredefined.Actor,
            //        ProjectName = "Fake Project 2",
            //        TypePrefix = "ABC",
            //        ItemTypeIconId = null
            //    },
            //     new ArtifactSearchResult()
            //    {
            //        ItemId = 9,
            //        ItemTypeId = 5,
            //        Name = "Test Process",
            //        ProjectId = 2,
            //        PredefinedType = ItemTypePredefined.Process,
            //        ProjectName = "Fake Project 2",
            //        TypePrefix = "PROTEST",
            //        ItemTypeIconId = 5
            //    }
            //});
        }

        public async Task<List<int>> GetAccessibleProjectIds(int userId)
        {
            if (userId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(userId));
            }

            var prm = new DynamicParameters();
            prm.Add("@userId", userId);

            return (await _connectionWrapper.QueryAsync<int>("GetAccessibleProjectIds", prm, commandType: CommandType.StoredProcedure)).ToList();
        }
    }
}