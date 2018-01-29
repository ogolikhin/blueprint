using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ServiceLibrary.Repositories;

namespace BlueprintSys.RC.Services.MessageHandlers.WorkflowsChanged
{
    public interface IWorkflowsChangedRepository : IBaseRepository
    {
        /// <summary>
        /// Calls the stored procedure [dbo].[GetSearchItemsWorkflowsChangeArtifactIds]
        /// </summary>
        Task<List<int>> GetAffectedArtifactIds(IEnumerable<int> workflowIds, int revisionId);
    }

    public class WorkflowsChangedRepository : BaseRepository, IWorkflowsChangedRepository
    {
        public WorkflowsChangedRepository(string connectionString) : base(connectionString)
        {
        }

        public async Task<List<int>> GetAffectedArtifactIds(IEnumerable<int> workflowIds, int revisionId)
        {
            var param = new DynamicParameters();
            param.Add("@workflowIds", SqlConnectionWrapper.ToDataTable(workflowIds ?? new int[0]));
            param.Add("@changeRevisionId", revisionId);
            return (await ConnectionWrapper.QueryAsync<int>("[dbo].[GetSearchItemsWorkflowsChangeArtifactIds]", param, commandType: CommandType.StoredProcedure)).ToList();
        }
    }
}
