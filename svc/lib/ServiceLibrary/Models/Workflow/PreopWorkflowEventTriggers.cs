using System.Linq;
using System.Threading.Tasks;
using ServiceLibrary.Models.Workflow.Actions;

namespace ServiceLibrary.Models.Workflow
{
    public class PreopWorkflowEventTriggers : WorkflowEventTriggers
    {
        protected override async Task InternalBatchExecute(IExecutionParameters executionParameters)
        {
            await SavePropertyChangeActions(executionParameters);
        }

        private async Task SavePropertyChangeActions(IExecutionParameters executionParameters)
        {
            var propertyChangeActions = this.Select(t => t.Action).OfType<PropertyChangeAction>();
            if (propertyChangeActions.Any() && executionParameters != null)
            {
                var namePropertyType = executionParameters.CustomPropertyTypes.Find(
                    i => i.Predefined == ProjectMeta.PropertyTypePredefined.Name); // Property 'Name' must exist

                var nameChangeAction = propertyChangeActions.FirstOrDefault(a => a.InstancePropertyTypeId == namePropertyType.InstancePropertyTypeId);
                if (nameChangeAction != null)
                {
                    await executionParameters.SaveRepository.UpdateArtifactName(
                            executionParameters.UserId,
                            executionParameters.ArtifactInfo.Id,
                            nameChangeAction.PropertyLiteValue.TextOrChoiceValue,
                            executionParameters.Transaction);
                }

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
