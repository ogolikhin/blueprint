using NServiceBus;

namespace BluePrintSys.ActionMessaging.Models
{
    [Express]
    public class GenerateUserStoriesMessage : ActionMessage
    {
        public GenerateUserStoriesMessage(int tenantId, int workflowId) : base(MessageActionType.GenerateUserStories, tenantId, workflowId)
        {
        }
    }
}
