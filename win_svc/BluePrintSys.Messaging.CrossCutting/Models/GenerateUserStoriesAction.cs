using System.Threading.Tasks;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Workflow;

namespace BluePrintSys.Messaging.CrossCutting.Models
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
