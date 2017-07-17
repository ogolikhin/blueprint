using System.Collections.Generic;
using System.Data;
using System.Linq;
using ActionHandlerService.Models;
using Dapper;
using ServiceLibrary.Repositories;

namespace ActionHandlerService.Repositories
{
    public interface IActionHandlerServiceRepository
    {
        IList<SqlModifiedProperty> GetPropertyModificationsForRevisionId(int revisionId);
        IList<SqlWorkFlowState> GetWorkflowStatesForArtifacts(int userId, IEnumerable<int> artifactIds, int revisionId, bool addDrafts = true);
        IList<SqlArtifactTriggers> GetWorkflowTriggersForArtifacts(int userId, int revisionId, int eventType, IEnumerable<int> itemIds);
    }

    public class ActionHandlerServiceRepository : IActionHandlerServiceRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;

        public ActionHandlerServiceRepository(string connectionString) : this(new SqlConnectionWrapper(connectionString))
        {
        }

        public ActionHandlerServiceRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public IList<SqlArtifactTriggers> GetWorkflowTriggersForArtifacts(int userId, int revisionId, int eventType, IEnumerable<int> itemIds)
        {
            var param = new DynamicParameters();
            param.Add("@userId", userId);
            param.Add("@revisionId", revisionId);
            param.Add("@eventType", eventType);
            param.Add("@itemIds", SqlConnectionWrapper.ToDataTable(itemIds));
            return _connectionWrapper.Query<SqlArtifactTriggers>("GetWorkflowTriggersForArtifacts", param, commandType: CommandType.StoredProcedure).ToList();
        }

        public IList<SqlModifiedProperty> GetPropertyModificationsForRevisionId(int revisionId)
        {
            var param = new DynamicParameters();
            param.Add("@revisionId", revisionId);
            return _connectionWrapper.Query<SqlModifiedProperty>("GetPropertyModificationsForRevisionId", param, commandType: CommandType.StoredProcedure).ToList();
        }

        public IList<SqlWorkFlowState> GetWorkflowStatesForArtifacts(int userId, IEnumerable<int> artifactIds, int revisionId, bool addDrafts = true)
        {
            var param = new DynamicParameters();
            param.Add("@userId", userId);
            var artifactIdsTable = SqlConnectionWrapper.ToDataTable(artifactIds);
            param.Add("@artifactIds", artifactIdsTable);
            param.Add("@revisionId", revisionId);
            param.Add("@addDrafts", addDrafts);
            return _connectionWrapper.Query<SqlWorkFlowState>("GetWorkflowStatesForArtifacts", param, commandType: CommandType.StoredProcedure).ToList();
        }
    }
}
