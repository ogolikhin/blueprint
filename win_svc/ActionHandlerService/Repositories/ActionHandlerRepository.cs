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
    public interface IActionHandlerServiceRepository : IInstanceSettingsRepository
    {
        Task<List<SqlModifiedProperty>> GetPropertyModificationsForRevisionIdAsync(int revisionId);
        Task<List<SqlWorkFlowStateInformation>> GetWorkflowStatesForArtifactsAsync(int userId, IEnumerable<int> artifactIds, int revisionId, bool addDrafts = true);
        Task<List<SqlArtifactTriggers>> GetWorkflowPropertyTransitionsForArtifactsAsync(int userId, int revisionId, int eventType, IEnumerable<int> itemIds);
        Task<string> GetTenantId();
        Task<Dictionary<int, List<int>>> GetInstancePropertyTypeIdsMap(IEnumerable<int> customPropertyTypeIds);
    }

    public class ActionHandlerServiceRepository : SqlInstanceSettingsRepository, IActionHandlerServiceRepository
    {
        public ActionHandlerServiceRepository(string connectionString) : this(new SqlConnectionWrapper(connectionString))
        {
        }

        public ActionHandlerServiceRepository(ISqlConnectionWrapper connectionWrapper) : this(connectionWrapper, new SqlArtifactPermissionsRepository(connectionWrapper))
        {
        }

        public ActionHandlerServiceRepository(ISqlConnectionWrapper connectionWrapper, IArtifactPermissionsRepository artifactPermissionsRepository) : 
            base(connectionWrapper, artifactPermissionsRepository)
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

        public async Task<List<SqlModifiedProperty>> GetPropertyModificationsForRevisionIdAsync(int revisionId)
        {
            var param = new DynamicParameters();
            param.Add("@revisionId", revisionId);
            return (await ConnectionWrapper.QueryAsync<SqlModifiedProperty>("GetPropertyModificationsForRevisionId", param, commandType: CommandType.StoredProcedure)).ToList();
        }

        public async Task<List<SqlWorkFlowStateInformation>> GetWorkflowStatesForArtifactsAsync(int userId, IEnumerable<int> artifactIds, int revisionId, bool addDrafts = true)
        {
            var param = new DynamicParameters();
            param.Add("@userId", userId);
            var artifactIdsTable = SqlConnectionWrapper.ToDataTable(artifactIds);
            param.Add("@artifactIds", artifactIdsTable);
            param.Add("@revisionId", revisionId);
            param.Add("@addDrafts", addDrafts);
            return (await ConnectionWrapper.QueryAsync<SqlWorkFlowStateInformation>("GetWorkflowStatesForArtifacts", param, commandType: CommandType.StoredProcedure)).ToList();
        }

        //TODO: remove once we get the tenant db ready
        public async Task<string> GetTenantId()
        {
            return (await ConnectionWrapper.QueryAsync<string>("SELECT TenantId FROM dbo.Instances", commandType: CommandType.Text)).FirstOrDefault();
        }

        public async Task<Dictionary<int, List<int>>> GetInstancePropertyTypeIdsMap(IEnumerable<int> customPropertyTypeIds)
        {
            var param = new DynamicParameters();
            var customPropertyTypeIdsTable = SqlConnectionWrapper.ToDataTable(customPropertyTypeIds);
            param.Add("@customPropertyTypeIds", customPropertyTypeIdsTable);
            var result =
                (await
                    ConnectionWrapper.QueryAsync<SqlCustomToInstancePropertyTypeIds>(
                        "[dbo].[GetInstancePropertyTypeIdsFromCustomIds]",
                        param, commandType: CommandType.StoredProcedure)).ToList();

            return result.ToDictionary(a => a.InstancePropertyTypeId,
                b =>
                    result.Where(c => c.InstancePropertyTypeId == b.InstancePropertyTypeId)
                        .Select(d => d.PropertyTypeId).ToList());
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
