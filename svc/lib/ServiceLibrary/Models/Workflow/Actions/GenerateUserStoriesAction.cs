using System.Threading.Tasks;
using ServiceLibrary.Models.Enums;

namespace ServiceLibrary.Models.Workflow.Actions
{
    public class GenerateUserStoriesAction : GenerateAction
    {
        public override MessageActionType ActionType { get; } = MessageActionType.GenerateUserStories;

        public override async Task<bool> Execute(IExecutionParameters executionParameters)
        {
            return await Task.FromResult(true);
        }
    }
}
