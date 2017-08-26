using System.Threading.Tasks;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Workflow;

namespace BluePrintSys.Messaging.CrossCutting.Models
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
