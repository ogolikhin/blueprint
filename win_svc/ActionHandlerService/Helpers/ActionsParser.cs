using System.Collections.Generic;
using ActionHandlerService.MessageHandlers.ArtifactPublished;

namespace ActionHandlerService.Helpers
{
    public interface IActionsParser
    {
        List<NotificationAction> GetNotificationActions(string actionsXmlString, int? propertyIdTest);
    }

    public class ActionsParser : IActionsParser
    {
        public List<NotificationAction> GetNotificationActions(string actionsXmlString, int? propertyIdTest)
        {
            //Should be replaced with some pattern here
            return new List<NotificationAction> {new NotificationAction {PropertyId = propertyIdTest}};
        }
    }
}
