using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.Webhooks;
using ServiceLibrary.Repositories.Workflow;

namespace BlueprintSys.RC.Services.MessageHandlers.ArtifactsPublished
{
    public interface IArtifactsPublishedRepository : IBaseRepository
    {
        /// <summary>
        /// Calls the stored procedure GetWorkflowTriggersForArtifacts
        /// </summary>
        Task<List<SqlWorkflowEvent>> GetWorkflowPropertyTransitionsForArtifactsAsync(int userId, int revisionId, int eventType, IEnumerable<int> itemIds);

        /// <summary>
        /// Calls the stored procedure GetWorkflowStatesForArtifacts
        /// </summary>
        Task<List<SqlWorkFlowStateInformation>> GetWorkflowStatesForArtifactsAsync(int userId, IEnumerable<int> artifactIds, int revisionId, bool addDrafts = true);

        /// <summary>
        /// Calls the stored procedure GetInstancePropertyTypeIdsFromCustomIds
        /// </summary>
        Task<Dictionary<int, List<int>>> GetInstancePropertyTypeIdsMap(IEnumerable<int> customPropertyTypeIds);

        /// <summary>
        /// Calls the stored procedure GetProjectNameByIds
        /// </summary>
        Task<List<SqlProject>> GetProjectNameByIdsAsync(IEnumerable<int> projectIds);

        IWorkflowRepository WorkflowRepository { get; }
        IWebhookRepository WebhookRepository { get; }
    }

    public class ArtifactsPublishedRepository : BaseRepository, IArtifactsPublishedRepository
    {
        public IWorkflowRepository WorkflowRepository { get; }
        public IWebhookRepository WebhookRepository { get; }

        public ArtifactsPublishedRepository(string connectionString) : this(new SqlConnectionWrapper(connectionString))
        {
        }

        public ArtifactsPublishedRepository(ISqlConnectionWrapper connectionWrapper) : this(connectionWrapper, new SqlArtifactPermissionsRepository(connectionWrapper))
        {
        }

        public ArtifactsPublishedRepository(ISqlConnectionWrapper connectionWrapper, IArtifactPermissionsRepository artifactPermissionsRepository) : this(connectionWrapper, artifactPermissionsRepository, new SqlUsersRepository(connectionWrapper))
        {
        }

        public ArtifactsPublishedRepository(ISqlConnectionWrapper connectionWrapper, IArtifactPermissionsRepository artifactPermissionsRepository, IUsersRepository usersRepository) : base(connectionWrapper, artifactPermissionsRepository, usersRepository)
        {
            WorkflowRepository = new SqlWorkflowRepository(connectionWrapper, ArtifactPermissionsRepository);
            WebhookRepository = new WebhookRepository(connectionWrapper);
        }

        public async Task<List<SqlWorkflowEvent>> GetWorkflowPropertyTransitionsForArtifactsAsync(int userId, int revisionId, int eventType, IEnumerable<int> itemIds)
        {
            var param = new DynamicParameters();
            param.Add("@userId", userId);
            param.Add("@revisionId", revisionId);
            param.Add("@eventType", eventType);
            param.Add("@itemIds", SqlConnectionWrapper.ToDataTable(itemIds));
            return (await ConnectionWrapper.QueryAsync<SqlWorkflowEvent>("[dbo].[GetWorkflowTriggersForArtifacts]", param, commandType: CommandType.StoredProcedure)).ToList();
        }

        public async Task<List<SqlWorkFlowStateInformation>> GetWorkflowStatesForArtifactsAsync(int userId, IEnumerable<int> artifactIds, int revisionId, bool addDrafts = true)
        {
            var param = new DynamicParameters();
            param.Add("@userId", userId);
            param.Add("@artifactIds", SqlConnectionWrapper.ToDataTable(artifactIds));
            param.Add("@revisionId", revisionId);
            param.Add("@addDrafts", addDrafts);
            return (await ConnectionWrapper.QueryAsync<SqlWorkFlowStateInformation>("[dbo].[GetWorkflowStatesForArtifacts]", param, commandType: CommandType.StoredProcedure)).ToList();
        }

        public async Task<Dictionary<int, List<int>>> GetInstancePropertyTypeIdsMap(IEnumerable<int> customPropertyTypeIds)
        {
            var param = new DynamicParameters();
            param.Add("@customPropertyTypeIds", SqlConnectionWrapper.ToDataTable(customPropertyTypeIds));
            var result = (await ConnectionWrapper.QueryAsync<SqlCustomToInstancePropertyTypeIds>("[dbo].[GetInstancePropertyTypeIdsFromCustomIds]", param, commandType: CommandType.StoredProcedure)).ToList();
            return result.ToDictionary(a => a.InstancePropertyTypeId, b => result.Where(c => c.InstancePropertyTypeId == b.InstancePropertyTypeId).Select(d => d.PropertyTypeId).ToList());
        }

        public async Task<List<SqlProject>> GetProjectNameByIdsAsync(IEnumerable<int> projectIds)
        {
            var param = new DynamicParameters();
            param.Add("@projectIds", SqlConnectionWrapper.ToDataTable(projectIds));
            return (await ConnectionWrapper.QueryAsync<SqlProject>("[dbo].[GetProjectNameByIds]", param, commandType: CommandType.StoredProcedure)).ToList();
        }
    }
}
