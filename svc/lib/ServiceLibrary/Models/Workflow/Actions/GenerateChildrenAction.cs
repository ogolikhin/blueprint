using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.PropertyType;

namespace ServiceLibrary.Models.Workflow.Actions
{
    public class GenerateChildrenAction : GenerateAction
    {
        public int? ChildCount { get; set; }

        public int ArtifactTypeId { get; set; }

        public override MessageActionType ActionType { get; } = MessageActionType.GenerateChildren;

        public override PropertySetResult ValidateAction(IExecutionParameters executionParameters)
        {
            return null;
        }
    }
}
