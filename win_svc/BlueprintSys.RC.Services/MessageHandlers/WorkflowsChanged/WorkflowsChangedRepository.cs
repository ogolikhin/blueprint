using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlueprintSys.RC.Services.MessageHandlers.WorkflowsChanged
{
    public interface IWorkflowsChangedRepository : IBaseRepository
    {
        /// <summary>
        /// TODO
        /// </summary>
        Task<List<int>> GetAffectedArtifactIds();
    }

    public class WorkflowsChangedRepository : BaseRepository, IWorkflowsChangedRepository
    {
        public WorkflowsChangedRepository(string connectionString) : base(connectionString)
        {
        }

        public async Task<List<int>> GetAffectedArtifactIds()
        {
            //TODO
            return await Task.FromResult(new List<int>());
        }
    }
}
