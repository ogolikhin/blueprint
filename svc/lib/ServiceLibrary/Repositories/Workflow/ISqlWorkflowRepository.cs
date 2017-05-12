using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceLibrary.Models.Workflow;

namespace ServiceLibrary.Repositories.Workflow
{
    public interface ISqlWorkflowRepository
    {
        Task<IEnumerable<Transitions>> GetTransitions(int artifactId, int userId);
    }
}
