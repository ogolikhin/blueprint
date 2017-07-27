using System.Collections.Generic;
using ActionHandlerService.MessageHandlers.ArtifactPublished;
using ActionHandlerService.Models;

namespace ActionHandlerService.Helpers
{
    public interface IActionsParser
    {
        IEnumerable<NotificationAction> GetNotificationActions(IEnumerable<SqlArtifactTriggers> sqlArtifactTriggers);
    }

    public class ActionsParser : IActionsParser
    {
        public IEnumerable<NotificationAction> GetNotificationActions(IEnumerable<SqlArtifactTriggers> sqlArtifactTriggers)
        {
            //Should be replaced with some pattern here
            foreach (var sqlArtifactTrigger in sqlArtifactTriggers)
            {
                foreach (var trigger in sqlArtifactTrigger.Triggers)
                {
                    yield return new NotificationAction
                    {
                        PropertyTypeId = sqlArtifactTrigger.EventPropertyTypeId,
                        ConditionalStateId = sqlArtifactTrigger.RequiredPreviousStateId,
                        ToEmail = "munish.saini@blueprintsys.com",
                        MessageTemplate = "Artifact has been published."
                    };
                }
            }
        }
    }
}
