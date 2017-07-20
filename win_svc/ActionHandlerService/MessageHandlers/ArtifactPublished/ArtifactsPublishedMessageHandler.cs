using BluePrintSys.Messaging.Models.Actions;

namespace ActionHandlerService.MessageHandlers.ArtifactPublished
{
    public class ArtifactsPublishedMessageHandler : BaseMessageHandler<ArtifactsPublishedMessage>
    {
        public ArtifactsPublishedMessageHandler() : this(new ArtifactsPublishedActionHelper())
        {
        }

        public ArtifactsPublishedMessageHandler(IActionHelper actionHelper) : base(actionHelper)
        {
        }
    }
}
