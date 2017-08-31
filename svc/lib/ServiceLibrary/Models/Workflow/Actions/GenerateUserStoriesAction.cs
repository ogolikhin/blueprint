using ServiceLibrary.Models.Enums;

namespace ServiceLibrary.Models.Workflow.Actions
{
    public class GenerateUserStoriesAction : GenerateAction
    {
        public override MessageActionType ActionType { get; } = MessageActionType.GenerateUserStories;
        
        public override bool ValidateAction(IExecutionParameters executionParameters)
        {
            return true;
        }
    }
}
