using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using ServiceLibrary.Repositories;

namespace ActionHandlerService
{
    public class SqlWorkFlowState
    {
        public int? Result { get; set; }
        public int WorkflowId { get; set; }
        public string WorkflowStateName { get; set; }
        public int WorkflowStateId { get; set; }
    }

    public class SqlModifiedProperty
    {
        public int ArtifactId { get; set; }
        public int ItemId { get; set; }
        public int ProjectId { get; set; }
        public int Type { get; set; }
        public int? TypeId { get; set; }
        public int VersionId { get; set; }
        public int StartRevision { get; set; }
        public int EndRevision { get; set; }
        public string PropertyName { get; set; }
        public string NewPropertyValue { get; set; }
    }

    public interface IActionHandlerServiceRepository
    {
        List<SqlModifiedProperty> GetPropertyModificationsForRevisionId(int revisionId);
        List<SqlWorkFlowState> GetWorkflowStatesForArtifacts(int userId, IEnumerable<int> artifactIds, int revisionId, bool addDrafts = true);
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

        public List<SqlModifiedProperty> GetPropertyModificationsForRevisionId(int revisionId)
        {
            var param = new DynamicParameters();
            param.Add("@revisionId", revisionId);
            return _connectionWrapper.Query<SqlModifiedProperty>("GetPropertyModificationsForRevisionId", param, commandType: CommandType.StoredProcedure).ToList();
        }

        public List<SqlWorkFlowState> GetWorkflowStatesForArtifacts(int userId, IEnumerable<int> artifactIds, int revisionId, bool addDrafts = true)
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
