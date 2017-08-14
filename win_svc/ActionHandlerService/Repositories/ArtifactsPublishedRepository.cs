using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ActionHandlerService.MessageHandlers.Notifications;
using ActionHandlerService.Models;
using BluePrintSys.Messaging.Models.Actions;
using Dapper;
using ServiceLibrary.Models.Email;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.InstanceSettings;

namespace ActionHandlerService.Repositories
{
    public interface IArtifactsPublishedRepository : IActionHandlerServiceRepository
    {
        /// <summary>
        /// Calls the stored procedure GetWorkflowTriggersForArtifacts
        /// </summary>
        Task<List<SqlArtifactTriggers>> GetWorkflowPropertyTransitionsForArtifactsAsync(int userId, int revisionId, int eventType, IEnumerable<int> itemIds);

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
    }

    public class ArtifactsPublishedRepository : ActionHandlerServiceRepository, IArtifactsPublishedRepository
    {
        public ArtifactsPublishedRepository(string connectionString) : base(connectionString)
        {
        }

        public ArtifactsPublishedRepository(ISqlConnectionWrapper connectionWrapper) : base(connectionWrapper)
        {
        }

        public ArtifactsPublishedRepository(ISqlConnectionWrapper connectionWrapper, IArtifactPermissionsRepository artifactPermissionsRepository) : base(connectionWrapper, artifactPermissionsRepository)
        {
        }

        public async Task<List<SqlArtifactTriggers>> GetWorkflowPropertyTransitionsForArtifactsAsync(int userId, int revisionId, int eventType, IEnumerable<int> itemIds)
        {
            var param = new DynamicParameters();
            param.Add("@userId", userId);
            param.Add("@revisionId", revisionId);
            param.Add("@eventType", eventType);
            param.Add("@itemIds", SqlConnectionWrapper.ToDataTable(itemIds));
            return (await ConnectionWrapper.QueryAsync<SqlArtifactTriggers>("GetWorkflowTriggersForArtifacts", param, commandType: CommandType.StoredProcedure)).ToList();
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
    }

    public interface INotificationActionHandlerServiceRepository : IActionHandlerServiceRepository
    {
        void SendEmail(SMTPClientConfiguration smtpClientConfiguration, Message message);
    }

    public class NotificationActionHandlerServiceRepository : ActionHandlerServiceRepository, INotificationActionHandlerServiceRepository
    {
        public NotificationActionHandlerServiceRepository(string connectionString) : base(connectionString)
        {
        }

        public NotificationActionHandlerServiceRepository(ISqlConnectionWrapper connectionWrapper) : base(connectionWrapper)
        {
        }

        public NotificationActionHandlerServiceRepository(ISqlConnectionWrapper connectionWrapper, IArtifactPermissionsRepository artifactPermissionsRepository) : base(connectionWrapper, artifactPermissionsRepository)
        {
        }

        public void SendEmail(SMTPClientConfiguration smtpClientConfiguration, Message message)
        {
            new SmtpClient(smtpClientConfiguration).SendEmail(message);
        }
    }
}
