using ActionHandlerService.Helpers;
using ActionHandlerService.Models.Enums;

namespace ActionHandlerService.Models
{
    public static class MessageTransportHostFactory
    {
        public static IMessageTransportHost GetMessageTransportHost(IConfigHelper configHelper)
        {
            if (configHelper.MessageBroker == MessageBroker.SQL)
            {
                return new SqlTransportHost();
            }
            return new RabbitMqTransportHost(configHelper);
        }
    }
}
