using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.Workflow;

namespace BlueprintSys.RC.Services.Repositories
{
    public interface IArtifactsPublishedRepository : IActionHandlerServiceRepository
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

        /// <summary>
        /// Calls the stored procedure GetPropertyModificationsForRevisionId
        /// </summary>
        Task<List<SqlModifiedProperty>> GetPropertyModificationsForRevisionIdAsync(int revisionId);

        Task<WorkflowTriggersContainer> GetWorkflowEventTriggersForTransition(int userId, int artifactId,
            int workflowId, int fromStateId, int toStateId);

        Task<WorkflowTriggersContainer> GetWorkflowEventTriggersForNewArtifactEvent(int userId,
            IEnumerable<int> artifactIds, int revisionId);

        Task<IEnumerable<WorkflowMessageArtifactInfo>> GetWorkflowMessageArtifactInfoAsync(int userId,
            IEnumerable<int> artifactIds, int revisionId);
    }

    public class ArtifactsPublishedRepository : ActionHandlerServiceRepository, IArtifactsPublishedRepository
    {
        private readonly IWorkflowRepository _workflowRepository;

        public ArtifactsPublishedRepository(string connectionString) : this(new SqlConnectionWrapper(connectionString))
        {
        }

        public ArtifactsPublishedRepository(ISqlConnectionWrapper connectionWrapper) : this(connectionWrapper, new SqlArtifactPermissionsRepository(connectionWrapper))
        {
        }

        public ArtifactsPublishedRepository(ISqlConnectionWrapper connectionWrapper, IArtifactPermissionsRepository artifactPermissionsRepository) : 
            base(connectionWrapper, 
                artifactPermissionsRepository, 
                new SqlUsersRepository(connectionWrapper))
        {
        }

        public ArtifactsPublishedRepository(ISqlConnectionWrapper connectionWrapper, 
            IArtifactPermissionsRepository artifactPermissionsRepository,
            IUsersRepository usersRepository) :
            base(connectionWrapper, artifactPermissionsRepository, usersRepository)
        {
            _workflowRepository = new SqlWorkflowRepository(connectionWrapper, ArtifactPermissionsRepository);
        }

        public async Task<List<SqlWorkflowEvent>> GetWorkflowPropertyTransitionsForArtifactsAsync(int userId, int revisionId, int eventType, IEnumerable<int> itemIds)
        {
            var param = new DynamicParameters();
            param.Add("@userId", userId);
            param.Add("@revisionId", revisionId);
            param.Add("@eventType", eventType);
            param.Add("@itemIds", SqlConnectionWrapper.ToDataTable(itemIds));
            return (await ConnectionWrapper.QueryAsync<SqlWorkflowEvent>("GetWorkflowTriggersForArtifacts", param, commandType: CommandType.StoredProcedure)).ToList();
        }

        public async Task<List<SqlWorkFlowStateInformation>> GetWorkflowStatesForArtifactsAsync(int userId, IEnumerable<int> artifactIds, int revisionId, bool addDrafts = true)
        {
            var param = new DynamicParameters();
            param.Add("@userId", userId);
            param.Add("@artifactIds", SqlConnectionWrapper.ToDataTable(artifactIds));
            param.Add("@revisionId", revisionId);
            param.Add("@addDrafts", addDrafts);
            return (await ConnectionWrapper.QueryAsync<SqlWorkFlowStateInformation>("GetWorkflowStatesForArtifacts", param, commandType: CommandType.StoredProcedure)).ToList();
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
            return (await ConnectionWrapper.QueryAsync<SqlProject>("GetProjectNameByIds", param, commandType: CommandType.StoredProcedure)).ToList();
        }

        //TODO is this still needed?
        public async Task<List<SqlModifiedProperty>> GetPropertyModificationsForRevisionIdAsync(int revisionId)
        {
            var param = new DynamicParameters();
            param.Add("@revisionId", revisionId);
            return (await ConnectionWrapper.QueryAsync<SqlModifiedProperty>("GetPropertyModificationsForRevisionId", param, commandType: CommandType.StoredProcedure)).ToList();
        }

        public async Task<WorkflowTriggersContainer> GetWorkflowEventTriggersForTransition(int userId, int artifactId,
            int workflowId, int fromStateId, int toStateId)
        {
            return
                await
                    _workflowRepository.GetWorkflowEventTriggersForTransition(userId, artifactId, workflowId,
                        fromStateId, toStateId);
        }

        public async Task<WorkflowTriggersContainer> GetWorkflowEventTriggersForNewArtifactEvent(int userId,
            IEnumerable<int> artifactIds, int revisionId)
        {
            return await _workflowRepository.GetWorkflowEventTriggersForNewArtifactEvent(userId,
                artifactIds,
                revisionId);
        }

        public async Task<IEnumerable<WorkflowMessageArtifactInfo>> GetWorkflowMessageArtifactInfoAsync(int userId,
            IEnumerable<int> artifactIds, int revisionId)
        {
            return await _workflowRepository.GetWorkflowMessageArtifactInfoAsync(userId,
                artifactIds,
                revisionId);
        }
    }
}
