using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.PropertyType;

namespace ServiceLibrary.Models.Workflow.Actions
{
    public class GenerateTestCasesAction : GenerateAction
    {
        public override MessageActionType ActionType { get; } = MessageActionType.GenerateTests;

        public override PropertySetResult ValidateAction(IExecutionParameters executionParameters)
        {
            return null;
        }
    }
}
