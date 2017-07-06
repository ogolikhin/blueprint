using NServiceBus;

namespace BluePrintSys.Messaging.Models.Action
{
    [Express]
    public class GenerateUserStoriesMessage : ActionMessage
    {
        public GenerateUserStoriesMessage(int tenantId, int workflowId) : base(MessageActionType.GenerateUserStories, tenantId, workflowId)
        {
        }
    }
}
