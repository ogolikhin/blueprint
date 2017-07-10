using NServiceBus;

namespace BluePrintSys.Messaging.Models.Actions
{
    [Express]
    public class GenerateUserStoriesMessage : ActionMessage
    {
        public GenerateUserStoriesMessage(int tenantId, int workflowId) : base(MessageActionType.GenerateUserStories, tenantId, workflowId)
        {
        }
    }
}
