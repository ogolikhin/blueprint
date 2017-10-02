using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SearchService.Models;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;

namespace SearchService.Repositories
{
    public interface ISemanticSearchRepository
    {
        Task<List<int>> GetAccessibleProjectIds(int userId);
        // TEMPORARY ADDED artifactId
        Task<List<ArtifactSearchResult>> GetSuggestedArtifactDetails(List<int> artifactIds, int userId, int artifactId);
        Task<SemanticSearchSetting> GetSemanticSearchSetting();
        Task<string> GetSemanticSearchText(int artifactId, int userId);
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
        
//// TEMPORARRY CODE ----------------------------------------------

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
//// TEMPORARRY CODE ----------------------------------------------

        public async Task<List<ArtifactSearchResult>> GetSuggestedArtifactDetails(List<int> artifactIds, int userId, int artifactId)
        {
//// TEMPORARRY CODE ----------------------------------------------
            var ids = new List<int>();
            ids.Add(artifactId);
            var multipleResult = await GetArtifactsProjects(ids, userId, 0, false);
            var projectId = multipleResult.Item1.ToList()[0].VersionProjectId;
            var resultArtifactIds = (await GetProjectArtifactIds(userId, projectId)).ToList();
//// TEMPORARRY CODE ----------------------------------------------

            var prm = new DynamicParameters();
            prm.Add("@userId", userId);
            prm.Add("@artifactIds", SqlConnectionWrapper.ToDataTable(resultArtifactIds));

            return (await _connectionWrapper.QueryAsync<ArtifactSearchResult>("GetSuggestedArtifactDetails", prm, commandType: CommandType.StoredProcedure)).ToList();
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

        public async Task<SemanticSearchSetting> GetSemanticSearchSetting()
        {
            return (await _connectionWrapper.QueryAsync<SemanticSearchSetting>("GetSemanticSearchSetting", commandType: CommandType.StoredProcedure)).FirstOrDefault();
        }

        public async Task<string> GetSemanticSearchText(int artifactId, int userId)
        {
            var prm = new DynamicParameters();
            prm.Add("@itemId", artifactId);
            prm.Add("@userId", userId);

            return
                (await
                    _connectionWrapper.QueryAsync<string>("SELECT dbo.GetItemSemanticSearchText(@userId,@itemId)", prm,
                        commandType: CommandType.Text)).FirstOrDefault();
        }
    }
}