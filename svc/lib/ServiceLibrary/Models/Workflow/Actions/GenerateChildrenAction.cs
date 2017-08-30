using System.Threading.Tasks;
using ServiceLibrary.Models.Enums;

namespace ServiceLibrary.Models.Workflow.Actions
{
    public class GenerateChildrenAction : GenerateAction
    {
        public int? ChildCount { get; set; }

        public int ArtifactTypeId { get; set; }

        public override MessageActionType ActionType { get; } = MessageActionType.GenerateChildren;
        public override async Task<bool> Execute(IExecutionParameters executionParameters)
        {
            return await Task.FromResult(true);
        }
    }
}
