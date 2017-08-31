using ServiceLibrary.Models.Enums;

namespace ServiceLibrary.Models.Workflow.Actions
{
    public class GenerateTestCasesAction : GenerateAction
    {
        public override MessageActionType ActionType { get; } = MessageActionType.GenerateTests;

        public override bool ValidateAction(IExecutionParameters executionParameters)
        {
            return true;
        }
    }
}
