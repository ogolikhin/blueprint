using System.Linq;
using System.Threading.Tasks;
using BluePrintSys.Messaging.CrossCutting.Models;
using ServiceLibrary.Models.Workflow;

namespace ArtifactStore.Models.Workflow
{
    public class PreopWorkflowEventTriggers : WorkflowEventTriggers
    {
        protected override async Task InternalBatchExecute(IExecutionParameters executionParameters)
        {
            await SavePropertyChangeActions(executionParameters);
        }

        private async Task SavePropertyChangeActions(IExecutionParameters executionParameters)
        {
            var propertyChangeActions = this.Select(t => t.Action).OfType<PropertyChangeAction>().ToArray();
            if (propertyChangeActions.Any() && executionParameters != null)
            {
                await executionParameters.SaveRepository.SavePropertyChangeActions(
                    executionParameters.UserId,
                    propertyChangeActions,
                    executionParameters.CustomPropertyTypes,
                    executionParameters.ArtifactInfo,
                    executionParameters.Transaction);
            }
        }
    }
}
