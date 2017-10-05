using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SearchService.Models;
using ServiceLibrary.Repositories;

namespace SearchService.Repositories
{
    public interface ISemanticSearchRepository
    {
        Task<List<int>> GetAccessibleProjectIds(int userId);
        Task<List<ArtifactSearchResult>> GetSuggestedArtifactDetails(List<int> artifactIds, int userId);
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

        public async Task<List<ArtifactSearchResult>> GetSuggestedArtifactDetails(List<int> artifactIds, int userId)
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