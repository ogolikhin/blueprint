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
            //TODO: uncomment once real artifact ids are passed in
            //var prm = new DynamicParameters();
            //prm.Add("@userId", userId);
            //prm.Add("@artifactIds", SqlConnectionWrapper.ToDataTable(artifactIds));

            //return (await _connectionWrapper.QueryAsync<ArtifactSearchResult>("GetSuggestedArtifactDetails", prm, commandType: CommandType.StoredProcedure)).ToList();

            return await Task.FromResult(new List<ArtifactSearchResult>()
            {
                new ArtifactSearchResult()
                {
                    ItemId = 1,
                    ItemTypeId = 1,
                    Name = "Fake Artifact 1",
                    ProjectId = 1,
                    PredefinedType = ItemTypePredefined.Actor,
                    ProjectName = "Fake Project",
                    TypePrefix = "FAKE",
                    ItemTypeIconId = 1
                },
                 new ArtifactSearchResult()
                {
                    ItemId = 2,
                    ItemTypeId = 1,
                    Name = "Fake Artifact 2",
                    ProjectId = 2,
                    PredefinedType = ItemTypePredefined.Actor,
                    ProjectName = "Fake Project 2",
                    TypePrefix = "ABC",
                    ItemTypeIconId = null
                },
                 new ArtifactSearchResult()
                {
                    ItemId = 9,
                    ItemTypeId = 5,
                    Name = "Test Process",
                    ProjectId = 2,
                    PredefinedType = ItemTypePredefined.Process,
                    ProjectName = "Fake Project 2",
                    TypePrefix = "PROTEST",
                    ItemTypeIconId = 5
                }
            });
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