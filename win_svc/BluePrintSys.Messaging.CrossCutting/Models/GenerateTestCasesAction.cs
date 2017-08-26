using System.Threading.Tasks;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Workflow;

namespace BluePrintSys.Messaging.CrossCutting.Models
{
    public class GenerateTestCasesAction : GenerateAction
    {
        public override MessageActionType ActionType { get; } = MessageActionType.GenerateTests;

        public override async Task<bool> Execute(IExecutionParameters executionParameters)
        {
            return await Task.FromResult(true);
        }
    }
}
