using System;
using ActionHandlerService.MessageHandlers.ArtifactPublished;

namespace ActionHandlerService.Helpers
{
    public interface IActionsParser
    {
        NotificationAction GetNotificationAction(string actionsXmlString);
    }

    public class ActionsParser : IActionsParser
    {
        public NotificationAction GetNotificationAction(string actionsXmlString)
        {
            //Should be replaced with some pattern here
            return new[] { new NotificationAction
            {

                PropertyId = artifactChangedProperties.Any() ? artifactChangedProperties.First().PropertyId : 0
            } };

        }
    }
}
