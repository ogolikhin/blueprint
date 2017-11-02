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
        Task<List<int>> GetAccessibleProjectIds(int userId);
        Task<List<ArtifactSearchResult>> GetSuggestedArtifactDetails(HashSet<int> artifactIds, int userId);
        Task<SemanticSearchSetting> GetSemanticSearchSetting();
        Task<SemanticSearchText> GetSemanticSearchText(int artifactId, int userId);
        Task<HashSet<int>> GetItemSimilarItemIds(int artifactId, int userId, int limit, bool isInstanceAdmin,
            IEnumerable<int> projectIds);
        Task<string> GetSemanticSearchIndex();
    }
    public class SemanticSearchRepository : ISemanticSearchRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;
        private const string ElasticsearchSemanticSearchIndexKey = "ElasticsearchSemanticSearchIndex";

        public SemanticSearchRepository()
             : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        internal SemanticSearchRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public async Task<List<ArtifactSearchResult>> GetSuggestedArtifactDetails(HashSet<int> artifactIds, int userId)
        {
            var prm = new DynamicParameters();
            prm.Add("@userId", userId);
            prm.Add("@artifactIds", SqlConnectionWrapper.ToDataTable(artifactIds));

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
            return ToSearchSetting((await _connectionWrapper.QueryAsync<SqlSemanticSearchSetting>("GetSemanticSearchSetting", commandType: CommandType.StoredProcedure)).FirstOrDefault());
        }

        private SemanticSearchSetting ToSearchSetting(SqlSemanticSearchSetting sqlSemanticSearchSetting)
        {
            if (sqlSemanticSearchSetting == null)
            {
                return null;
            }
            return new SemanticSearchSetting()
            {
                TenantId = sqlSemanticSearchSetting.TenantId,
                TenantName = sqlSemanticSearchSetting.TenantName,
                ConnectionString = sqlSemanticSearchSetting.ElasticsearchConnectionString,
                SemanticSearchEngineType = sqlSemanticSearchSetting.SemanticSearchEngineType
            };
        }

        public async Task<SemanticSearchText> GetSemanticSearchText(int artifactId, int userId)
        {
            var prm = new DynamicParameters();
            prm.Add("@itemId", artifactId);
            prm.Add("@userId", userId);

            return
                (await
                    _connectionWrapper.QueryAsync<SemanticSearchText>("SELECT * FROM dbo.GetItemSemanticSearchNameSearchText(@userId,@itemId)", prm,
                        commandType: CommandType.Text)).FirstOrDefault();
        }

        public async Task<HashSet<int>> GetItemSimilarItemIds(int artifactId, int userId, int limit, bool isInstanceAdmin, IEnumerable<int> projectIds)
        {
            var prm = new DynamicParameters();
            prm.Add("@itemId", artifactId);
            prm.Add("@userId", userId);
            prm.Add("@limit", limit);
            prm.Add("@allProjects", isInstanceAdmin);
            prm.Add("@projectIds", SqlConnectionWrapper.ToDataTable(projectIds));

            return (await
                    _connectionWrapper.QueryAsync<int>("GetItemSimilarItemIds", prm, commandType: CommandType.StoredProcedure)).ToHashSet();
        }

        public async Task<string> GetSemanticSearchIndex()
        {
            var prm = new DynamicParameters();
            prm.Add("@returnNonRestrictedOnly", false);
            var elasticsearchSemanticSearchIndex =
                (await
                    _connectionWrapper.QueryAsync<ApplicationSetting>("GetApplicationSettings", prm,
                        commandType: CommandType.StoredProcedure)).FirstOrDefault(
                            s => s.Key == ElasticsearchSemanticSearchIndexKey);

            return elasticsearchSemanticSearchIndex == null ? string.Empty : elasticsearchSemanticSearchIndex.Value;
        }
    }
}